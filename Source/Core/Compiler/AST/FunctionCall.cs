/*

 Copyright (c) 2004-2006 Tomas Matousek, Ladislav Prosek, Vaclav Novak and Martin Maly.

 The use and distribution terms for this software are contained in the file named License.txt, 
 which can be found in the root of the Phalanger distribution. By using this software 
 in any fashion, you are agreeing to be bound by the terms of this license.
 
 You must not remove this notice from this software.

*/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections;

using PHP.Core.Emit;
using PHP.Core.Parsers;
using PHP.Core.Reflection;

#if SILVERLIGHT
using MathEx = PHP.CoreCLR.MathEx;
#else
using MathEx = System.Math;
#endif
/*
  
 NOTES:
     possible access values for all FunctionCall subclasses: Read, None, ReadRef
		 ReadRef is set even in cases when the function do NOT return ref:
		 
			function g(&$a) {}
			function f() {}
			g(f());  ... calling f has access ReadRef
			$a =& f(); ... dtto

*/

namespace PHP.Core.AST
{
	#region FunctionCall

	public abstract class FunctionCall : VarLikeConstructUse
	{
		protected CallSignature callSignature;
        /// <summary>GetUserEntryPoint calling signature</summary>
        public CallSignature CallSignature { get { return callSignature; } }

		public FunctionCall(Position position, List<ActualParam>/*!*/ parameters, List<TypeRef>/*!*/ genericParams)
			: base(position)
		{
			Debug.Assert(parameters != null);

			this.callSignature = new CallSignature(parameters, genericParams);
		}

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="Expression.Analyze"]/*'/>
		internal override Evaluation Analyze(Analyzer/*!*/ analyzer, ExInfoFromParent info)
		{
			base.Analyze(analyzer, info);
			access = info.Access;
			return new Evaluation(this);
		}

		internal void DumpArguments(AstVisitor/*!*/ visitor, TextWriter/*!*/ output)
		{
			output.Write('(');

			int i = 0;
			foreach (ActualParam param in callSignature.Parameters)
			{
				if (i++ > 0) output.Write(',');
				param.Expression.DumpTo(visitor, output);
			}

			output.Write(')');
		}
	}

	#endregion

	#region DirectFcnCall

    public sealed class DirectFcnCall : FunctionCall
	{
		internal override Operations Operation { get { return Operations.DirectCall; } }

		/// <summary>
		/// A list of inlined functions.
		/// </summary>
		private enum InlinedFunction
		{
			None,
			CreateFunction
		}

		/// <summary>
		/// Simple name for methods.
		/// </summary>
		private QualifiedName qualifiedName;
        /// <summary>Simple name for methods.</summary>
        public QualifiedName QualifiedName { get { return qualifiedName; } }

		private DRoutine routine;
		private int overloadIndex = DRoutine.InvalidOverloadIndex;

		/// <summary>
		/// An inlined function represented by the node (if any).
		/// </summary>
		private InlinedFunction inlined = InlinedFunction.None;

		public DirectFcnCall(Position position, QualifiedName qualifiedName, List<ActualParam>/*!*/ parameters,
	  List<TypeRef>/*!*/ genericParams)
			: base(position, parameters, genericParams)
		{
			this.qualifiedName = qualifiedName;
		}

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="Expression.Analyze"]/*'/>
		internal override Evaluation Analyze(Analyzer/*!*/ analyzer, ExInfoFromParent info)
		{
            base.Analyze(analyzer, info);

            if (isMemberOf == null)
            {
                // function call //

                return AnalyzeFunctionCall(analyzer, ref info);
            }
            else
            {
				// method call //

				routine = null;
				callSignature.Analyze(analyzer, UnknownSignature.Default, info, false);

                return new Evaluation(this);
			}
		}

        /// <summary>
        /// Analyze the function call (isMemberOf == null).
        /// </summary>
        /// <param name="analyzer"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <remarks>This code fragment is separated to save the stack when too long Expression chain is being compiled.</remarks>
        private Evaluation AnalyzeFunctionCall(Analyzer/*!*/ analyzer, ref ExInfoFromParent info)
        {
            Debug.Assert(isMemberOf == null);

            // resolve name:
            routine = analyzer.ResolveFunctionName(qualifiedName, position);

            if (routine.IsUnknown)
                Statistics.AST.AddUnknownFunctionCall(qualifiedName);
            // resolve overload if applicable:
            RoutineSignature signature;
            overloadIndex = routine.ResolveOverload(analyzer, callSignature, position, out signature);

            Debug.Assert(overloadIndex != DRoutine.InvalidOverloadIndex, "A function should have at least one overload");

            // warning if not supported function call is detected
            if (routine is PhpLibraryFunction && (((PhpLibraryFunction)routine).Options & FunctionImplOptions.NotSupported) != 0)
                analyzer.ErrorSink.Add(Warnings.NotSupportedFunctionCalled, analyzer.SourceUnit, Position, QualifiedName.ToString());

            // analyze parameters:
            callSignature.Analyze(analyzer, signature, info, false);

            // get properties:
            analyzer.AddCurrentRoutineProperty(routine.GetCallerRequirements());

            // replaces the node if its value can be determined at compile-time:
            object value;
            return TryEvaluate(analyzer, out value) ?
                new Evaluation(this, value) :
                new Evaluation(this);
        }

		#region Evaluation

        /// <summary>
        /// Evaluation info used to get some info from evaluated functions.
        /// </summary>
        public class TryEvaluateInfo
        {
            public bool emitDeclareLamdaFunction;
            public DRoutine newRoutine;            
        }

        /// <summary>
		/// Tries to determine a value of the node.
		/// </summary>
		/// <returns>
		/// Whether the function call can be evaluated at compile time. <B>true</B>, 
		/// if the function is a special library one and the correct number of arguments 
		/// is specified in the call and all that arguments are evaluable.
		/// </returns>
		private bool TryEvaluate(Analyzer/*!*/ analyzer, out object value)
		{
            if (callSignature.AllParamsHaveValue)
            {
                PureFunctionAttribute pureAttribute;

                // PhpLibraryFunction with PureFunctionAttribute can be evaluated
                PhpLibraryFunction lib_function;

                if ((lib_function = routine as PhpLibraryFunction) != null &&
                    (pureAttribute = PureFunctionAttribute.Reflect(lib_function.Overloads[overloadIndex].Method)) != null)
                {
                    // the method to be used for evaluation
                    MethodInfo evaluableMethod = pureAttribute.CallSpecialMethod ?
                        pureAttribute.SpecialMethod :
                        lib_function.Overloads[overloadIndex].Method;

                    Debug.Assert(evaluableMethod != null);

                    if (evaluableMethod.ContainsGenericParameters)
                        throw new ArgumentException("Evaluable method '" + evaluableMethod.Name + "' cannot contain generic parameters.");

                    var parametersInfo = evaluableMethod.GetParameters();

                    object[] invokeParameters = new object[parametersInfo.Length];

                    // convert/create proper parameters value:
                    int nextCallParamIndex = 0;

                    TryEvaluateInfo tryEvaluateInfo = null;

                    for (int i = 0; i < parametersInfo.Length; ++i)
                    {
                        ParameterInfo paramInfo = parametersInfo[i];
                        Type paramType = paramInfo.ParameterType;

                        // only In parameters are allowed
                        Debug.Assert(!paramInfo.IsOut && !paramInfo.IsRetval);

                        // perform parameter conversion:
                        Action<Converter<object, object>> PassArgument = (converter) =>
                            {
                                if (nextCallParamIndex >= callSignature.Parameters.Count)
                                    throw new ArgumentException("Not enough parameters in evaluable method.");

                                object obj = callSignature.Parameters[nextCallParamIndex++].Expression.Value;
                                invokeParameters[i] = converter(obj);
                            };

                        // special params types:
                        if (paramType == typeof(Analyzer))
                        {
                            invokeParameters[i] = analyzer;
                        }
                        else if (paramType == typeof(CallSignature))
                        {
                            invokeParameters[i] = callSignature;
                        }
                        else if (paramType == typeof(TryEvaluateInfo))
                        {
                            Debug.Assert(tryEvaluateInfo == null);
                            invokeParameters[i] = (tryEvaluateInfo = new TryEvaluateInfo());
                        }
                        else if (   // ... , params object[] // last parameter
                            paramType == typeof(object[]) &&
                            i == parametersInfo.Length - 1 &&
                            parametersInfo[i].IsDefined(typeof(ParamArrayAttribute), false))
                        {
                            // params object[]
                            var args = new object[callSignature.Parameters.Count - nextCallParamIndex];
                            for (int arg = 0; arg < args.Length; ++nextCallParamIndex, ++arg)
                                args[arg] = callSignature.Parameters[nextCallParamIndex].Expression.Value;

                            invokeParameters[i] = args;
                        }
                        // PHP value types:
                        else if (paramType == typeof(object))
                            PassArgument(obj => obj);
                        else if (paramType == typeof(PhpBytes))
                            PassArgument(Convert.ObjectToPhpBytes);
                        else if (paramType == typeof(string))
                            PassArgument(Convert.ObjectToString);
                        else if (paramType == typeof(int))
                            PassArgument(obj=>(object)Convert.ObjectToInteger(obj));
                        else if (paramType == typeof(bool))
                            PassArgument(obj => (object)Convert.ObjectToBoolean(obj));
                        else if (paramType == typeof(double))
                            PassArgument(obj => (object)Convert.ObjectToDouble(obj));
                        else if (paramType == typeof(long))
                            PassArgument(obj => (object)Convert.ObjectToLongInteger(obj));
                        else if (paramType == typeof(char))
                            PassArgument(obj => (object)Convert.ObjectToChar(obj));
                        else
                            throw new ArgumentException("Parameter type " + paramType.ToString() + " cannot be used in evaluable method.", paramInfo.Name);
                    }

                    // catch runtime errors
                    var oldErrorOverride = PhpException.ThrowCallbackOverride;
                    if (!(analyzer.ErrorSink is EvalErrorSink || analyzer.ErrorSink is WebErrorSink)) // avoid infinite recursion, PhpExceptions in such cases are passed
                        PhpException.ThrowCallbackOverride = (error, message) =>
                        {
                            analyzer.ErrorSink.AddInternal(
                                -2,
                                message, (error == PhpError.Error || error == PhpError.CoreError || error == PhpError.UserError) ? ErrorSeverity.Error : ErrorSeverity.Warning,
                                (int)WarningGroups.None,
                                analyzer.SourceUnit.GetMappedFullSourcePath(Position.FirstLine),
                                new ErrorPosition(
                                    analyzer.SourceUnit.GetMappedLine(Position.FirstLine), Position.FirstColumn,
                                    analyzer.SourceUnit.GetMappedLine(Position.LastLine), Position.LastColumn),
                                true
                                );
                        };
                    
                    // invoke the method and get the result
                    try
                    {
                        value = evaluableMethod.Invoke(null, invokeParameters);

                        // apply automatic cast to false if CastToFalse attribute is defined:
                        if (evaluableMethod.ReturnTypeCustomAttributes.IsDefined(typeof(CastToFalseAttribute), false))
                        {
                            if ((value == null) ||
                                (value is int && (int)value == -1))
                                value = false;
                        }

                        return true;
                    }
                    catch
                    {
                        // function cannot be evaluated
                        // continue with the default
                    }
                    finally
                    {
                        PhpException.ThrowCallbackOverride = oldErrorOverride;
                    }

                    // parse some results of the evaluation
                    if (tryEvaluateInfo != null)
                    {
                        if (tryEvaluateInfo.emitDeclareLamdaFunction && tryEvaluateInfo.newRoutine != null)
                        {
                            this.routine = tryEvaluateInfo.newRoutine;
                            this.inlined = InlinedFunction.CreateFunction;
                        }
                    }
                }
            }

            // function cannot be evaluated
            value = null;
            return false;

            /*

			// skips functions without "special" flag set:
			//PhpLibraryFunction lib_function = routine as PhpLibraryFunction;
			if (lib_function == null || (lib_function.Options & FunctionImplOptions.Special) == 0)
			{
				value = null;
				return false;
			}

			switch (callSignature.Parameters.Count)
			{
				case 0:
					{
						if (lib_function.Name.EqualsLowercase("phpversion"))
						{
							value = PhpVersion.Current;
							return true;
						}

						if (lib_function.Name.EqualsLowercase("pi"))
						{
							value = Math.PI;
							return true;
						}
						break;
					}

				case 1:
					{
						// tries to evaluate the parameter:
						if (!callSignature.Parameters[0].Expression.HasValue) break;

						object param = callSignature.Parameters[0].Expression.Value;

						if (lib_function.Name.EqualsLowercase("function_exists"))
						{
                            // jakub: if this returns true, it is evaluable, in case of false, we should try it during the runtime again

							// TODO:
							//Name function_name = new Name(Convert.ObjectToString(param));
							//OverloadInfo overload;

							//// only library functions can be checked; others depends on the current set of declarators:
							//ApplicationContext.Functions.Get(function_name, 0, out overload);
							//value = overload.GetUserEntryPoint != null;

							//return overload.GetUserEntryPoint != null;
							value = false;
							return false;
						}

						if (lib_function.Name.EqualsLowercase("strlen"))
						{
							value = Convert.ObjectToString(param).Length;
							return true;
						}

						if (lib_function.Name.EqualsLowercase("round"))
						{
							value = Math.Round(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("sqrt"))
						{
							value = Math.Sqrt(Convert.ObjectToDouble(param));
							return true;
						}


						if (lib_function.Name.EqualsLowercase("exp"))
						{
							value = Math.Exp(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("log"))
						{
							value = Math.Log(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("ceil"))
						{
							value = Math.Ceiling(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("floor"))
						{
							value = Math.Floor(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("deg2rad"))
						{
							value = Convert.ObjectToDouble(param) / 180 * Math.PI;
							return true;
						}

						if (lib_function.Name.EqualsLowercase("cos"))
						{
							value = Math.Cos(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("sin"))
						{
							value = Math.Sin(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("tan"))
						{
							value = Math.Tan(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("acos"))
						{
							value = Math.Acos(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("asin"))
						{
							value = Math.Asin(Convert.ObjectToDouble(param));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("atan"))
						{
							value = Math.Atan(Convert.ObjectToDouble(param));
							return true;
						}

						break;
					}

				case 2:
					{
						// tries to evaluate the parameters:
						if (!callSignature.Parameters[0].Expression.HasValue) break;
						if (!callSignature.Parameters[1].Expression.HasValue) break;

						object param1 = callSignature.Parameters[0].Expression.Value;
						object param2 = callSignature.Parameters[1].Expression.Value;

						if (lib_function.Name.EqualsLowercase("version_compare"))
						{
							value = PhpVersion.Compare(Convert.ObjectToString(param1), Convert.ObjectToString(param2));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("log"))
						{
							value = MathEx.Log(Convert.ObjectToDouble(param1), Convert.ObjectToDouble(param2));
							return true;
						}

						if (lib_function.Name.EqualsLowercase("create_function"))
						{
							// has to be a valid identifier:
							string function_name = "__" + Guid.NewGuid().ToString().Replace('-', '_');

							string prefix1, prefix2;
							DynamicCode.GetLamdaFunctionCodePrefixes(function_name, Convert.ObjectToString(param1), out prefix1, out prefix2);

							Position pos_args = callSignature.Parameters[0].Position;
							Position pos_body = callSignature.Parameters[1].Position;

							// function __XXXXXX(<args>){<fill><body>}
							string fill = GetInlinedLambdaCodeFill(pos_args, pos_body);
							string code = String.Concat(prefix2, fill, Convert.ObjectToString(param2), "}");

							// the position of the first character of the parsed code:
							// (note that escaped characters distort position a little bit, which cannot be eliminated so easily)
							Position pos = Position.Initial;
							pos.FirstOffset = pos_args.FirstOffset - prefix1.Length + 1;
							pos.FirstColumn = pos_args.FirstColumn - prefix1.Length + 1;
							pos.FirstLine = pos_args.FirstLine;

							// parses function source code:
							List<Statement> statements = analyzer.BuildAst(pos, code);

							if (statements == null)
								break;

							FunctionDecl decl_node = (FunctionDecl)statements[0];

							// modify declaration:
							this.routine = decl_node.ConvertToLambda(analyzer);

							// adds declaration to the end of the global code statement list:
							analyzer.AddLambdaFcnDeclaration(decl_node);

							this.inlined = InlinedFunction.CreateFunction;

							// we cannot replace the expression with literal (emission of lambda declaration is needed):
							value = null;
							return false;
						}

						break;
					}

				case 3:
					{
						// tries to evaluate the parameters:
						if (!callSignature.Parameters[0].Expression.HasValue) break;
						if (!callSignature.Parameters[1].Expression.HasValue) break;
						if (!callSignature.Parameters[2].Expression.HasValue) break;

						object param1 = callSignature.Parameters[0].Expression.Value;
						object param2 = callSignature.Parameters[1].Expression.Value;
						object param3 = callSignature.Parameters[2].Expression.Value;

						if (lib_function.Name.EqualsLowercase("version_compare"))
						{
							value = PhpVersion.Compare(Convert.ObjectToString(param1), Convert.ObjectToString(param2),
								Convert.ObjectToString(param3));

							return true;
						}
						break;
					}
			}

			value = null;
			return false;
             
            */
		}

		#endregion

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="IsDeeplyCopied"]/*'/>
		internal override bool IsDeeplyCopied(CopyReason reason, int nestingLevel)
		{
			// emit copy only if the call itself don't do that:
			return routine == null || !routine.ReturnValueDeepCopyEmitted;
		}

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="Emit"]/*'/>
		internal override PhpTypeCode Emit(CodeGenerator/*!*/ codeGenerator)
		{
			Debug.Assert(access == AccessType.Read || access == AccessType.ReadRef || access == AccessType.ReadUnknown
				|| access == AccessType.None, "Invalid access type in FunctionCall");
			Statistics.AST.AddNode("FunctionCall.Direct");

			PhpTypeCode result;

			if (inlined != InlinedFunction.None)
			{
				result = EmitInlinedFunctionCall(codeGenerator);
			}
			else
			{
				// this node actually represents a method call:
				if (isMemberOf != null)
				{
					result = codeGenerator.EmitRoutineOperatorCall(null, isMemberOf, qualifiedName.ToString(),
						null, callSignature);
				}
				else
				{
					// the node represents a function call:
					result = routine.EmitCall(codeGenerator, callSignature, null, false, overloadIndex, null, position, access == AccessType.None);
				}
			}

			// handles return value:
			codeGenerator.EmitReturnValueHandling(this, codeGenerator.ChainBuilder.LoadAddressOfFunctionReturnValue, ref result);

			return result;
		}

		/// <summary>
		/// Emits library function that can be inlined.
		/// </summary>
		private PhpTypeCode EmitInlinedFunctionCall(CodeGenerator/*!*/ codeGenerator)
		{
			switch (inlined)
			{
				case InlinedFunction.CreateFunction:
					{
						PhpFunction php_function = (PhpFunction)routine;

						// define builders (not defined earlier as the lambda function it is not in the tables):
						php_function.DefineBuilders();

						// LOAD PhpFunction.DeclareLamda(context,<delegate>);
						codeGenerator.EmitDeclareLamdaFunction(php_function.ArgLessInfo);

						// bake (not baked later as the lambda function it is not in the tables):
						php_function.Bake();

						return PhpTypeCode.String;
					}

				default:
					Debug.Fail("Unimplemented inlined function.");
					return PhpTypeCode.Invalid;
			}
		}


		internal override void DumpTo(AstVisitor visitor, System.IO.TextWriter output)
		{
			if (isMemberOf != null)
			{
				isMemberOf.DumpTo(visitor, output);
				output.Write("->");
			}

			output.Write(qualifiedName);
			DumpArguments(visitor, output);
			DumpAccess(output);
		}

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitDirectFcnCall(this);
        }
	}

	#endregion

	#region IndirectFcnCall

	public sealed class IndirectFcnCall : FunctionCall
	{
		internal override Operations Operation { get { return Operations.IndirectCall; } }

		internal Expression/*!*/ NameExpr { get { return nameExpr; } }
		private Expression/*!*/ nameExpr;

		public IndirectFcnCall(Position p, Expression/*!*/ nameExpr, List<ActualParam>/*!*/ parameters,
	  List<TypeRef>/*!*/ genericParams)
			: base(p, parameters, genericParams)
		{
			this.nameExpr = nameExpr;
		}

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="Expression.Analyze"]/*'/>
		internal override Evaluation Analyze(Analyzer/*!*/ analyzer, ExInfoFromParent info)
		{
			base.Analyze(analyzer, info);

			nameExpr = nameExpr.Analyze(analyzer, ExInfoFromParent.DefaultExInfo).Literalize();

			callSignature.Analyze(analyzer, UnknownSignature.Default, info, false);

			// function call:
			if (isMemberOf == null)
				analyzer.AddCurrentRoutineProperty(RoutineProperties.ContainsIndirectFcnCall);

			return new Evaluation(this);
		}

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="Emit"]/*'/>
		internal override PhpTypeCode Emit(CodeGenerator codeGenerator)
		{
			Debug.Assert(access == AccessType.Read || access == AccessType.ReadRef ||
				access == AccessType.ReadUnknown || access == AccessType.None);
			Statistics.AST.AddNode("FunctionCall.Indirect");

			PhpTypeCode result;
			result = codeGenerator.EmitRoutineOperatorCall(null, isMemberOf, null, nameExpr, callSignature);

			codeGenerator.EmitReturnValueHandling(this, codeGenerator.ChainBuilder.LoadAddressOfFunctionReturnValue, ref result);

			return result;
		}

		internal override void DumpTo(AstVisitor/*!*/ visitor, TextWriter/*!*/ output)
		{
			if (isMemberOf != null)
			{
				isMemberOf.DumpTo(visitor, output);
				output.Write("->");
			}

			output.Write('{');
			nameExpr.DumpTo(visitor, output);
			output.Write('}');
			DumpArguments(visitor, output);
			DumpAccess(output);
		}

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitIndirectFcnCall(this);
        }
	}

	#endregion

	#region StaticMtdCall

	public abstract class StaticMtdCall : FunctionCall
	{
		protected GenericQualifiedName className;
        public GenericQualifiedName ClassName { get { return className; } }
		protected DType/*!*/ type;

		public StaticMtdCall(Position position, GenericQualifiedName className, List<ActualParam>/*!*/ parameters,
	   List<TypeRef>/*!*/ genericParams)
			: base(position, parameters, genericParams)
		{
			this.className = className;
		}

		internal override Evaluation Analyze(Analyzer/*!*/ analyzer, ExInfoFromParent info)
		{
			base.Analyze(analyzer, info);

			type = analyzer.ResolveTypeName(className, analyzer.CurrentType, analyzer.CurrentRoutine, position, false);

			// analyze constructed type (new constructed type cane be used here):
			analyzer.AnalyzeConstructedType(type);

			if (type.TypeDesc.Equals(DTypeDesc.InterlockedTypeDesc))
				analyzer.ErrorSink.Add(Warnings.ClassBehaviorMayBeUnexpected, analyzer.SourceUnit, position, type.FullName);

			return new Evaluation(this);
		}
	}

	#endregion

	#region DirectStMtdCall

	public sealed class DirectStMtdCall : StaticMtdCall
	{
		internal override Operations Operation { get { return Operations.DirectStaticCall; } }

		private Name methodName;
        public Name MethodName { get { return methodName; } }
		private DRoutine method;
		private int overloadIndex = DRoutine.InvalidOverloadIndex;
		private bool runtimeVisibilityCheck;

		public DirectStMtdCall(Position position, ClassConstUse/*!*/ classConstant, List<ActualParam>/*!*/ parameters,
	  List<TypeRef>/*!*/ genericParams)
			: base(position, classConstant.ClassName, parameters, genericParams)
		{
			this.methodName = new Name(classConstant.Name.Value);
		}

		public DirectStMtdCall(Position position, GenericQualifiedName className, Name methodName, List<ActualParam>/*!*/ parameters,
		  List<TypeRef>/*!*/ genericParams)
			: base(position, className, parameters, genericParams)
		{
			this.methodName = methodName;
		}

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="Expression.Analyze"]/*'/>
		internal override Evaluation Analyze(Analyzer/*!*/ analyzer, ExInfoFromParent info)
		{
			base.Analyze(analyzer, info);

			method = analyzer.ResolveMethod(type, methodName, position, analyzer.CurrentType, analyzer.CurrentRoutine, out runtimeVisibilityCheck);

			if (!method.IsUnknown)
			{
				// we are sure about the method //

				if (method.IsAbstract)
				{
					analyzer.ErrorSink.Add(Errors.AbstractMethodCalled, analyzer.SourceUnit, position,
						method.DeclaringType.FullName, method.FullName);
				}
			}

			RoutineSignature signature;
			overloadIndex = method.ResolveOverload(analyzer, callSignature, position, out signature);

			Debug.Assert(overloadIndex != DRoutine.InvalidOverloadIndex, "Each method should have at least one overload");

			callSignature.Analyze(analyzer, signature, info, false);

			return new Evaluation(this);
		}

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="Emit"]/*'/>
		internal override PhpTypeCode Emit(CodeGenerator/*!*/ codeGenerator)
		{
			Statistics.AST.AddNode("StaticMethodCall.Direct");

			IPlace instance = null;

			// PHP allows for static invocations of instance method
			if (!method.IsUnknown && !method.IsStatic)
			{
				// if we are in an instance method and the $this for the callee is assignable from
				// current $this, then invoke the method directly with current $this
				if (codeGenerator.LocationStack.LocationType == LocationTypes.MethodDecl)
				{
					CompilerLocationStack.MethodDeclContext method_context = codeGenerator.LocationStack.PeekMethodDecl();
					if (!method_context.Method.IsStatic && method.DeclaringType.IsAssignableFrom(method_context.Type))
					{
						instance = IndexedPlace.ThisArg;
					}
				}
			}

			// class context is unknown or the class is m-decl or completely unknown at compile-time -> call the operator			
			PhpTypeCode result = method.EmitCall(codeGenerator, callSignature, instance, runtimeVisibilityCheck,
				overloadIndex, type as ConstructedType, position, access == AccessType.None);

			// handles return value:
			codeGenerator.EmitReturnValueHandling(this, codeGenerator.ChainBuilder.LoadAddressOfFunctionReturnValue, ref result);

			return result;
		}

		internal override void DumpTo(AstVisitor/*!*/ visitor, TextWriter/*!*/ output)
		{
			if (isMemberOf != null)
			{
				isMemberOf.DumpTo(visitor, output);
				output.Write("->");
			}

			output.Write(className.ToString());
			output.Write("::");
			output.Write(methodName.ToString());
			DumpArguments(visitor, output);
			DumpAccess(output);
		}

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitDirectStMtdCall(this);
        }
	}

	#endregion

	#region IndirectStMtdCall

	public class IndirectStMtdCall : StaticMtdCall
	{
		internal override Operations Operation { get { return Operations.IndirectStaticCall; } }

		private CompoundVarUse/*!*/ methodNameVar;
        /// <summary>Expression that represents name of method</summary>
        public CompoundVarUse/*!*/ MethodNameVar { get { return methodNameVar; } }

		public IndirectStMtdCall(Position position, GenericQualifiedName className, CompoundVarUse/*!*/ mtdNameVar,
	  List<ActualParam>/*!*/ parameters, List<TypeRef>/*!*/ genericParams)
			: base(position, className, parameters, genericParams)
		{
			this.methodNameVar = mtdNameVar;
		}

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="Expression.Analyze"]/*'/>
		internal override Evaluation Analyze(Analyzer/*!*/ analyzer, ExInfoFromParent info)
		{
			base.Analyze(analyzer, info);

			methodNameVar.Analyze(analyzer, ExInfoFromParent.DefaultExInfo);

			callSignature.Analyze(analyzer, UnknownSignature.Default, info, false);

			return new Evaluation(this);
		}

		/// <include file='Doc/Nodes.xml' path='doc/method[@name="Emit"]/*'/>
		internal override PhpTypeCode Emit(CodeGenerator/*!*/ codeGenerator)
		{
			Statistics.AST.AddNode("StaticMethodCall.Indirect");

			PhpTypeCode result = codeGenerator.EmitRoutineOperatorCall(type, null, null, methodNameVar, callSignature);

			// handles return value:
			codeGenerator.EmitReturnValueHandling(this, codeGenerator.ChainBuilder.LoadAddressOfFunctionReturnValue, ref result);

			return result;
		}

		internal override void DumpTo(AstVisitor/*!*/ visitor, TextWriter/*!*/ output)
		{
			if (isMemberOf != null)
			{
				isMemberOf.DumpTo(visitor, output);
				output.Write("->");
			}

			output.Write(className.ToString());
			output.Write("::");
			output.Write('{');
			methodNameVar.DumpTo(visitor, output);
			output.Write('}');
			DumpArguments(visitor, output);
			DumpAccess(output);
		}

        /// <summary>
        /// Call the right Visit* method on the given Visitor object.
        /// </summary>
        /// <param name="visitor">Visitor to be called.</param>
        public override void VisitMe(TreeVisitor visitor)
        {
            visitor.VisitIndirectStMtdCall(this);
        }
	}

	#endregion
}