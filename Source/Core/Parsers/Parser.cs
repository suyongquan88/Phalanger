using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using PHP.Core.AST;
using PHP.Core.Reflection;
using Pair = System.Tuple<object,object>;

namespace PHP.Core.Parsers
{
	#region Helpers

	public interface IReductionsSink
	{
		void InclusionReduced(Parser/*!*/ parser, IncludingEx/*!*/ decl);
		void FunctionDeclarationReduced(Parser/*!*/ parser, FunctionDecl/*!*/ decl);
		void TypeDeclarationReduced(Parser/*!*/ parser, TypeDecl/*!*/ decl);
		void GlobalConstantDeclarationReduced(Parser/*!*/ parser, GlobalConstantDecl/*!*/ decl);
	}

	// Due to a MCS bug, it has to be in the other partial class in generated (Generated/Parser.cs)
    // .. uncomment the following once it is fixed!

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
	public partial struct SemanticValueType
	{
		public override string ToString()
		{
			if (Object != null) return Object.ToString();
			if (Offset != 0) return String.Format("[{0}-{1}]", Offset, Integer);
			if (Double != 0.0) return Double.ToString();
			return Integer.ToString();
		}
	}

	/// <summary>
	/// Position in a source file.
	/// </summary>
	public partial struct Position
	{
		public static Position Invalid = new Position(-1, -1, -1, -1, -1, -1);
		public static Position Initial = new Position(+1, 0, 0, +1, 0, 0);

		public override string ToString()
		{
			return string.Format("({0},{1})-({2},{3})", FirstLine, FirstColumn, LastLine, LastColumn);
		}

		public ShortPosition Short
		{
			get { return new ShortPosition(FirstLine, FirstColumn); }
		}

		public static implicit operator ShortPosition(Position pos)
		{
			return new ShortPosition(pos.FirstLine, pos.FirstColumn);
		}

		public static implicit operator ErrorPosition(Position pos)
		{
			return new ErrorPosition(pos.FirstLine, pos.FirstColumn, pos.LastLine, pos.LastColumn);
		}
		
		public bool IsValid
		{
			get
			{
				return FirstOffset != -1;
			}
		}
	}

	#endregion

	public partial class Parser
	{
		#region Reductions Sinks

		public static readonly IReductionsSink/*!*/ NullReductionSink = new _NullReductionsSink();

		private sealed class _NullReductionsSink : IReductionsSink
		{
			void IReductionsSink.InclusionReduced(Parser/*!*/ parser, IncludingEx/*!*/ incl)
			{
			}

			void IReductionsSink.FunctionDeclarationReduced(Parser/*!*/ parser, FunctionDecl/*!*/ decl)
			{
			}

			void IReductionsSink.TypeDeclarationReduced(Parser/*!*/ parser, TypeDecl/*!*/ decl)
			{
			}

			void IReductionsSink.GlobalConstantDeclarationReduced(Parser/*!*/ parser, GlobalConstantDecl/*!*/ decl)
			{
			}
		}

		public sealed class ReductionsCounter : IReductionsSink
		{
			public int InclusionCount { get { return inclusionCount; } }
			private int inclusionCount;

			public int FunctionCount { get { return functionCount; } }
			private int functionCount;

			public int TypeCount { get { return typeCount; } }
			private int typeCount;

			public int ConstantCount { get { return constantCount; } }
			private int constantCount;

			public ReductionsCounter()
			{
				this.inclusionCount = 0;
				this.functionCount = 0;
				this.typeCount = 0;
				this.constantCount = 0;
			}

			void IReductionsSink.InclusionReduced(Parser/*!*/ parser, IncludingEx/*!*/ incl)
			{
				this.inclusionCount++;
			}

			void IReductionsSink.FunctionDeclarationReduced(Parser/*!*/ parser, FunctionDecl/*!*/ decl)
			{
				this.functionCount++;
			}

			void IReductionsSink.TypeDeclarationReduced(Parser/*!*/ parser, TypeDecl/*!*/ decl)
			{
				this.typeCount++;
			}

			void IReductionsSink.GlobalConstantDeclarationReduced(Parser/*!*/ parser, GlobalConstantDecl/*!*/ decl)
			{
				this.constantCount++;
			}
		}

		#endregion

		protected sealed override int EofToken
		{
			get { return (int)Tokens.EOF; }
		}

		protected sealed override int ErrorToken
		{
			get { return (int)Tokens.ERROR; }
		}

		protected sealed override Position InvalidPosition
		{
			get { return Position.Invalid; }
		}

		private Scanner scanner;
		private LanguageFeatures features;

		public ErrorSink ErrorSink { get { return errors; } }
		private ErrorSink errors;

		public SourceUnit SourceUnit { get { return sourceUnit; } }
		private SourceUnit sourceUnit;

		private IReductionsSink reductionsSink;
		private bool unicodeSemantics;
		private TextReader reader;
		private Scope currentScope;

		public bool AllowGlobalCode { get { return allowGlobalCode; } set { allowGlobalCode = value; } }
		private bool allowGlobalCode;

		/// <summary>
		/// The root of AST.
		/// </summary>
		private GlobalCode astRoot;

		private const int strBufSize = 100;

		private NamespaceDecl currentNamespace;

		// stack of string buffers; used when processing encaps strings
        private readonly Stack<PhpStringBuilder> strBufStack = new Stack<PhpStringBuilder>(100);

		public Parser()
		{
		}

		private new bool Parse()
		{
			return false;
		}

		public GlobalCode Parse(SourceUnit/*!*/ sourceUnit, TextReader/*!*/ reader, ErrorSink/*!*/ errors,
			IReductionsSink/*!*/ reductionsSink, Parsers.Position initialPosition, Lexer.LexicalStates initialLexicalState,
			LanguageFeatures features)
		{
			Debug.Assert(reader != null && errors != null);

			// initialization:
			InitializeFields();

			this.scanner = new Scanner(initialPosition, reader, sourceUnit, errors, features);
			this.scanner.CurrentLexicalState = initialLexicalState;
			this.reductionsSink = reductionsSink;
			this.errors = errors;
			this.features = features;
			this.sourceUnit = sourceUnit;
			this.reader = reader;
			this.currentScope = new Scope(1); // starts assigning scopes from 2 (1 is reserved for prepended inclusion)

			this.unicodeSemantics = (features & LanguageFeatures.UnicodeSemantics) != 0;


			base.Scanner = this.scanner;
			base.Parse();

			GlobalCode result = astRoot;

			// clean and let GC collect unused AST and other stuff:
			ClearFields();

			return result;
		}

		private void InitializeFields()
		{
			strBufStack.Clear();
			docCommentStack.Clear();
			condLevel = 0;
			currentNamespace = null;
		}

		private void ClearFields()
		{
			scanner = null;
			features = 0;
			errors = null;
			sourceUnit = null;
			reductionsSink = null;
			astRoot = null;
			reader = null;
		}

		#region DocComments

		private readonly Stack<string> docCommentStack = new Stack<string>(10);

		private String PopDocComment()
		{
			return (docCommentStack.Count > 0) ? docCommentStack.Pop() : null;
		}

		private void PushDocComment(object comment)
		{
			docCommentStack.Push((string)comment);
		}

		#endregion

		#region Conditional Code, Scope

		private int condLevel;

		private void EnterConditionalCode()
		{
			condLevel++;
		}

		private void LeaveConditionalCode()
		{
			Debug.Assert(condLevel > 0);
			condLevel--;
		}

		public bool IsCurrentCodeConditional
		{
			get
			{
				return condLevel > 0;
			}
		}

		public bool IsCurrentCodeOneLevelConditional
		{
			get
			{
				return condLevel > 1;
			}
		}

		internal Scope GetScope()
		{
			currentScope.Increment();
			return currentScope;
		}

		#endregion Conditional Code

		#region Complex Productions

		private Expression CreateConcatExOrStringLiteral(Position p, List<Expression> exprs, bool trimEoln)
		{
			PhpStringBuilder encapsed_str = strBufStack.Pop();
			
			if (trimEoln)
                encapsed_str.TrimEoln();

			if (exprs.Count > 0)
			{
                if (encapsed_str.Length > 0)
                    exprs.Add(encapsed_str.CreateLiteral(p));

				return new ConcatEx(p, exprs);
			}
            else
            {
                return encapsed_str.CreateLiteral(p);
            }
		}

		private VariableUse CreateStaticFieldUse(Position position, GenericQualifiedName/*!*/ className, CompoundVarUse/*!*/ field)
		{
			DirectVarUse dvu;
			IndirectVarUse ivu;

			if ((dvu = field as DirectVarUse) != null)
			{
				return new DirectStFldUse(position, className, dvu.VarName);
			}
			else if ((ivu = field as IndirectVarUse) != null)
			{
				return new IndirectStFldUse(position, className, ivu.VarNameEx);
			}
			else
			{
				ItemUse iu = (ItemUse)field;
				iu.Array = CreateStaticFieldUse(iu.Array.Position, className, (CompoundVarUse)iu.Array);
				return iu;
			}
		}

		private ForeachStmt/*!*/ CreateForeachStmt(Position pos, Expression/*!*/ enumeree, ForeachVar/*!*/ var1,
		  Position pos1, ForeachVar var2, Statement/*!*/ body)
		{
			ForeachVar key, value;
			if (var2 != null)
			{
				key = var1;
				value = var2;

				if (key.Alias)
				{
					errors.Add(Errors.KeyAlias, SourceUnit, pos1);
					key.Alias = false;
				}
			}
			else
			{
				key = null;
				value = var1;
			}

			return new ForeachStmt(pos, enumeree, key, value, body);
		}

		private TryStmt/*!*/ CreateTryStmt(Position pos, List<Statement>/*!*/ tryStmts, Position classNamePos,
			GenericQualifiedName/*!*/ className, Position variablePos, string/*!*/ variable,
			List<Statement>/*!*/ catchStmts, List<CatchItem> catches)
		{
			CatchItem catch_item = new CatchItem(classNamePos, className, new DirectVarUse(variablePos,
				new VariableName(variable)), catchStmts);

			if (catches != null)
			{
				catches.Insert(0, catch_item);
			}
			else
			{
				catches = new List<CatchItem>();
				catches.Add(catch_item);
			}

			return new TryStmt(pos, tryStmts, catches);
		}

		private void CheckVariableUse(Position position, object item)
		{
			if (item as VariableUse == null)
			{
				errors.Add(FatalErrors.CheckVarUseFault, SourceUnit, position);
				throw new CompilerException();
			}
		}

		private VarLikeConstructUse/*!*/ CreateVariableUse(Position pos, VarLikeConstructUse/*!*/ variable, VarLikeConstructUse/*!*/ property,
	  Pair parameters, VarLikeConstructUse chain)
		{
			if (parameters != null)
			{
				if (property is ItemUse)
				{
					property.IsMemberOf = variable;
                    property = new IndirectFcnCall(pos, property, (List<ActualParam>)parameters.Item2, (List<TypeRef>)parameters.Item1);
				}
				else
				{
					DirectVarUse direct_use;
					if ((direct_use = property as DirectVarUse) != null)
					{
						QualifiedName method_name = new QualifiedName(new Name(direct_use.VarName.Value), Name.EmptyNames);
                        property = new DirectFcnCall(pos, method_name, (List<ActualParam>)parameters.Item2, (List<TypeRef>)parameters.Item1);
					}
					else
					{
						IndirectVarUse indirect_use = (IndirectVarUse)property;
                        property = new IndirectFcnCall(pos, indirect_use.VarNameEx, (List<ActualParam>)parameters.Item2, (List<TypeRef>)parameters.Item1);
					}
					property.IsMemberOf = variable;
				}
			}
			else
			{
				property.IsMemberOf = variable;
			}

			if (chain != null)
			{
				// finds the first variable use in the chain and connects it to the property

				VarLikeConstructUse first_in_chain = chain;
				while (first_in_chain.IsMemberOf != null)
					first_in_chain = first_in_chain.IsMemberOf;

				first_in_chain.IsMemberOf = property;
				return chain;
			}
			else
			{
				return property;
			}
		}

		private VarLikeConstructUse/*!*/ CreatePropertyVariable(Position pos, CompoundVarUse/*!*/ property,
			Pair parameters)
		{
			if (parameters != null)
			{
				DirectVarUse direct_use;
				IndirectVarUse indirect_use;

				if ((direct_use = property as DirectVarUse) != null)
				{
					QualifiedName method_name = new QualifiedName(new Name(direct_use.VarName.Value), Name.EmptyNames);
                    return new DirectFcnCall(pos, method_name, (List<ActualParam>)parameters.Item2, (List<TypeRef>)parameters.Item1);
				}

				if ((indirect_use = property as IndirectVarUse) != null)
                    return new IndirectFcnCall(pos, indirect_use.VarNameEx, (List<ActualParam>)parameters.Item2, (List<TypeRef>)parameters.Item1);

                return new IndirectFcnCall(pos, (ItemUse)property, (List<ActualParam>)parameters.Item2, (List<TypeRef>)parameters.Item1);
			}
			else
			{
				return property;
			}
		}

		private VarLikeConstructUse/*!*/ CreatePropertyVariables(VarLikeConstructUse chain, VarLikeConstructUse/*!*/ member)
		{
			if (chain != null)
			{
				IndirectFcnCall ifc = member as IndirectFcnCall;

				if (ifc != null && ifc.NameExpr as ItemUse != null)
				{
					// we know that FcNAme is VLCU and not Expression, because chain is being parsed:
					((VarLikeConstructUse)ifc.NameExpr).IsMemberOf = chain;
				}
				else
				{
					member.IsMemberOf = chain;
				}
			}
			else
			{
				member.IsMemberOf = null;
			}

			return member;
		}

		private Expression/*!*/ CheckInitializer(Position pos, Expression/*!*/ initializer)
		{
			if (initializer is ArrayEx)
			{
				errors.Add(Errors.ArrayInClassConstant, SourceUnit, pos);
				return new NullLiteral(pos);
			}

			return initializer;
		}

		private PhpMemberAttributes CheckPrivateType(Position pos)
		{
			if (currentNamespace != null)
			{
				errors.Add(Errors.PrivateClassInGlobalNamespace, SourceUnit, pos);
				return PhpMemberAttributes.None;
			}

			return PhpMemberAttributes.Private;
		}

		private int CheckPartialType(Position pos)
		{
			if (IsCurrentCodeConditional)
			{
				errors.Add(Errors.PartialConditionalDeclaration, SourceUnit, pos);
				return 0;
			}

			if (sourceUnit.CompilationUnit.IsTransient)
			{
				errors.Add(Errors.PartialTransientDeclaration, SourceUnit, pos);
				return 0;
			}

			if (!sourceUnit.CompilationUnit.IsPure)
			{
				errors.Add(Errors.PartialImpureDeclaration, SourceUnit, pos);
				return 0;
			}

			return 1;
		}

		private Statement/*!*/ CheckGlobalStatement(Statement/*!*/ statement)
		{
			if (sourceUnit.CompilationUnit.IsPure && !allowGlobalCode)
			{
				if (!statement.SkipInPureGlobalCode())
					errors.Add(Errors.GlobalCodeInPureUnit, SourceUnit, statement.Position);

				return EmptyStmt.Skipped;
			}

			return statement;
		}

		/// <summary>
		/// Checks whether a reserved class name is used in generic qualified name.
		/// </summary>
		private void CheckReservedNamesAbsence(GenericQualifiedName? genericName, Position position)
		{
			if (genericName.HasValue)
			{
				if (genericName.Value.QualifiedName.IsReservedClassName)
				{
					errors.Add(Errors.CannotUseReservedName, SourceUnit, position, genericName.Value.QualifiedName.Name);
				}

				if (genericName.Value.GenericParams != null)
					CheckReservedNamesAbsence(genericName.Value.GenericParams, position);
			}
		}

		private void CheckReservedNamesAbsence(object[] staticTypeRefs, Position position)
		{
			foreach (object static_type_ref in staticTypeRefs)
			{
				if (static_type_ref is GenericQualifiedName)
					CheckReservedNamesAbsence((GenericQualifiedName)static_type_ref, position);
			}
		}

		private void CheckReservedNamesAbsence(List<GenericQualifiedName> genericNames, Position position)
		{
			foreach (GenericQualifiedName genericName in genericNames)
			{
				CheckReservedNamesAbsence(genericName, position);
			}
		}

		private void CheckReservedNameAbsence(Name typeName, Position position)
		{
			if (Name.ParentClassName.Equals(typeName) || Name.SelfClassName.Equals(typeName))
				errors.Add(Errors.CannotUseReservedName, SourceUnit, position, typeName);
		}

		private void CheckTypeParameterNames(List<FormalTypeParam> typeParams, string declarerName)
		{
			if (typeParams == null) return;

			for (int i = 0; i < typeParams.Count; i++)
			{
				if (typeParams[i].Name.Equals(declarerName))
				{
					ErrorSink.Add(Errors.GenericParameterCollidesWithDeclarer, SourceUnit, typeParams[i].Position, declarerName);
				}
				else
				{
					for (int j = 0; j < i; j++)
					{
						if (typeParams[j].Name.Equals(typeParams[i].Name))
							errors.Add(Errors.DuplicateGenericParameter, SourceUnit, typeParams[i].Position);
					}
				}
			}
		}

		private CustomAttribute.TargetSelectors IdentifierToTargetSelector(Position position, string/*!*/ identifier)
		{
			if (String.Compare(identifier, "assembly", StringComparison.OrdinalIgnoreCase) == 0)
				return CustomAttribute.TargetSelectors.Assembly;

			if (String.Compare(identifier, "module", StringComparison.OrdinalIgnoreCase) == 0)
				return CustomAttribute.TargetSelectors.Module;

			errors.Add(Errors.InvalidAttributeTargetSelector, SourceUnit, position, identifier);
			return CustomAttribute.TargetSelectors.Default;
		}

		private List<CustomAttribute>/*!*/ AddCustomAttributes(List<CustomAttribute> result,
		  List<CustomAttribute>/*!*/ attrs, CustomAttribute.TargetSelectors targetSelector)
		{
			if (result == null)
				result = attrs;
			else
				result.AddRange(attrs);

			for (int i = 0; i < attrs.Count; i++)
				attrs[i].TargetSelector = targetSelector;

			return result;
		}

		#endregion

		#region Imports

		/// <summary>
		/// Import of a particular type or function.
		/// </summary>
		public void AddImport(Position position, DeclarationKind kind, List<string>/*!*/ names, string aliasName)
		{
			QualifiedName qn = new QualifiedName(names, true);
			Name alias = (aliasName != null) ? new Name(aliasName) : qn.Name;

			switch (kind)
			{
				case DeclarationKind.Type:
					if (!sourceUnit.AddTypeAlias(qn, alias))
						errors.Add(Errors.ConflictingTypeAliases, SourceUnit, position);
					break;

				case DeclarationKind.Function:
					if (!sourceUnit.AddFunctionAlias(qn, alias))
						errors.Add(Errors.ConflictingFunctionAliases, SourceUnit, position);
					break;

				case DeclarationKind.Constant:
					if (!sourceUnit.AddConstantAlias(qn, alias))
						errors.Add(Errors.ConflictingConstantAliases, SourceUnit, position);
					break;
			}
		}

		/// <summary>
		/// Import of a namespace with a qualified name.
		/// </summary>
		public void AddImport(List<string>/*!*/ namespaceNames)
		{
			sourceUnit.AddImportedNamespace(new QualifiedName(namespaceNames, false));
		}

		/// <summary>
		/// Import of a namespace with a simple name.
		/// </summary>
		public void AddImport(string namespaceName)
		{
			sourceUnit.AddImportedNamespace(new QualifiedName(Name.EmptyBaseName, new Name[] { new Name(namespaceName) }));
		}

		#endregion

		#region Helpers

		private static readonly List<Statement> emptyStatementList = new List<Statement>(1);
		private static readonly List<GenericQualifiedName> emptyGenericQualifiedNameList = new List<GenericQualifiedName>(1);
		private static readonly List<FormalParam> emptyFormalParamListIndex = new List<FormalParam>(1);
		private static readonly List<ActualParam> emptyActualParamListIndex = new List<ActualParam>(1);
		private static readonly List<Expression> emptyExpressionListIndex = new List<Expression>(1);
		private static readonly List<Item> emptyItemListIndex = new List<Item>(1);
		private static readonly List<NamedActualParam> emptyNamedActualParamListIndex = new List<NamedActualParam>(1);
		private static readonly List<FormalTypeParam> emptyFormalTypeParamList = new List<FormalTypeParam>(1);
		private static readonly List<TypeRef> emptyTypeRefList = new List<TypeRef>(1);


		private static void ListAdd<T>(object list, object item)
		{
            ((List<T>)list).Add((T)item);
		}

		private static List<T> NewList<T>(T item)
		{
			List<T> list = new List<T>();
			list.Add(item);

			return list;
		}

		private static List<T> NewList<T>(object item)
		{
			List<T> list = new List<T>();
			list.Add((T)item);

			return list;
		}

        private ShortPosition GetHeadingEnd(Position lastNonBodySymbolPosition)
        {
            return new ShortPosition(lastNonBodySymbolPosition.LastLine, lastNonBodySymbolPosition.LastColumn + 1);
        }

        private ShortPosition GetBodyStart(Position bodyPosition)
        {
            return new ShortPosition(bodyPosition.FirstLine, bodyPosition.FirstColumn);
        }


		#endregion
	}
}