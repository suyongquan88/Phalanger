/*

 Copyright (c) 2005-2006 Tomas Matousek.

 The use and distribution terms for this software are contained in the file named License.txt, 
 which can be found in the root of the Phalanger distribution. By using this software 
 in any fashion, you are agreeing to be bound by the terms of this license.
 
 You must not remove this notice from this software.

*/

using System;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

using PHP.Core;

/*
namespace PHP.Library.SPL
{
/// <summary>
/// 
/// </summary>
/// <remarks>
/// <para>
/// <code>
/// class ReflectionClass implements Reflector
/// { 
///   
///   public name;
///   
///   final private __clone();
///   public static export();
///   public __construct(string name);
///   public __toString();
///   public getName();
///   public isInternal();
///   public isUserDefined();
///   public isInstantiable();
///   public getFileName();
///   public getStartLine();
///   public getEndLine();
///   public getDocComment();
///   public getConstructor();
///   public hasMethod(string name);
///   public getMethod(string name);
///   public getMethods();
///   public hasProperty(string name);
///   public getProperty(string name);
///   public getProperties();
///   public hasConstant(string name);
///   public getConstants();
///   public getConstant(string name);
///   public getInterfaces();
///   public isInterface();
///   public isAbstract();
///   public isFinal();
///   public getModifiers();
///   public isInstance(stdclass object);
///   public newInstance(mixed* args);
///   public getParentClass();
///   public isSubclassOf(ReflectionClass class);
///   public getStaticProperties();
///   public getStaticPropertyValue(string name [, mixed default]);
///   public setStaticPropertyValue(string name, mixed value);
///   public getDefaultProperties();
///   public isIterateable();
///   public implementsInterface(string name);
///   public getExtension();
///   public getExtensionName();  
/// }
/// </code>
/// </para>
/// </remarks>
[Serializable, ImplementsType]
class ReflectionClass : PhpObject, Reflector
{
 #region PHP Fields

 /// <summary>
 /// 
 /// </summary>
 public PhpReference name = new PhpSmartReference();

 #endregion
    
 #region PHP Methods

 /// <summary>
 /// 
 /// </summary>
 private object __clone()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public static object export()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object __construct()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object __toString()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getName()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object isInternal()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object isUserDefined()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object isInstantiable()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getFileName()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getStartLine()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getEndLine()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getDocComment()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getConstructor()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object hasMethod()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getMethod()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getMethods()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object hasProperty()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getProperty()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getProperties()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object hasConstant()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getConstants()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getConstant()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getInterfaces()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object isInterface()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object isAbstract()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object isFinal()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getModifiers()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object isInstance()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object newInstance()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getParentClass()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object isSubclassOf()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getStaticProperties()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getStaticPropertyValue()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object setStaticPropertyValue()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getDefaultProperties()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object isIterateable()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object implementsInterface()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getExtension()
 {
	 // TODO: write body
	 return null;
 }

 /// <summary>
 /// 
 /// </summary>
 public object getExtensionName()
 {
	 // TODO: write body
	 return null;
 }

 #endregion
    
 #region Implementation Details
    
 /// <summary>
 /// The method table.
 /// </summary>
 private static volatile PhpMethodTable methodTable;

 /// <summary>
 /// The field table.
 /// </summary>
 private static volatile PhpFieldTable fieldTable;

 /// <summary>
 /// Returns the method table.
 /// </summary>
 public override IPhpMemberTable __GetMethodTable()
 {
	 if (methodTable == null)
	 {
		 Type type = typeof(PHP.Library.SPL.ReflectionClass);
		 lock (type)
		 {
			 if (methodTable == null)
			 {
				 methodTable = new PhpMethodTable(type, base.__GetMethodTable());
				 methodTable.AddMethod(type.GetMethod("__clone", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("export", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("__construct", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("__toString", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getName", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("isInternal", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("isUserDefined", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("isInstantiable", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getFileName", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getStartLine", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getEndLine", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getDocComment", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getConstructor", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("hasMethod", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getMethod", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getMethods", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("hasProperty", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getProperty", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getProperties", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("hasConstant", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getConstants", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getConstant", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getInterfaces", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("isInterface", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("isAbstract", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("isFinal", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getModifiers", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("isInstance", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("newInstance", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getParentClass", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("isSubclassOf", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getStaticProperties", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getStaticPropertyValue", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("setStaticPropertyValue", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getDefaultProperties", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("isIterateable", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("implementsInterface", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getExtension", Emit.Types.PhpStack));
				 methodTable.AddMethod(type.GetMethod("getExtensionName", Emit.Types.PhpStack));
			 }
		 }
	 }
	 return methodTable;
 }

 /// <summary>
 /// Returns the field table.
 /// </summary>
 public override IPhpMemberTable __GetFieldTable()
 {
	 if (fieldTable == null)
	 {
		 Type type = typeof(PHP.Library.SPL.ReflectionClass);
		 lock (type)
		 {
			 if (fieldTable == null)
			 {
				 fieldTable = new PhpFieldTable(type, base.__GetFieldTable());
				 fieldTable.AddField(type.GetField("name", BindingFlags.Instance | BindingFlags.Public));
			 }
		 }
	 }      
	 return fieldTable;
 }

 /// <summary>
 /// For internal purposes only.
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public ReflectionClass(ScriptContext context, bool newInstance) : base(context, newInstance)
 {       
 }

 /// <summary>
 /// For internal purposes only.
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public ReflectionClass(ScriptContext context, RuntimeTypeHandle callingTypeHandle) : base(context, callingTypeHandle)
 { 		  
 }

 /// <summary>
 /// Deserializing constructor.
 /// </summary>
 protected ReflectionClass(SerializationInfo info, StreamingContext context) : base(info, context)
 { 
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 private object __clone(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return __clone();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public static object export(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return export();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object __construct(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return __construct();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object __toString(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return __toString();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getName(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getName();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object isInternal(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return isInternal();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object isUserDefined(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return isUserDefined();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object isInstantiable(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return isInstantiable();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getFileName(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getFileName();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getStartLine(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getStartLine();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getEndLine(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getEndLine();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getDocComment(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getDocComment();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getConstructor(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getConstructor();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object hasMethod(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return hasMethod();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getMethod(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getMethod();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getMethods(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getMethods();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object hasProperty(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return hasProperty();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getProperty(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getProperty();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getProperties(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getProperties();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object hasConstant(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return hasConstant();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getConstants(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getConstants();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getConstant(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getConstant();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getInterfaces(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getInterfaces();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object isInterface(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return isInterface();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object isAbstract(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return isAbstract();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object isFinal(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return isFinal();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getModifiers(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getModifiers();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object isInstance(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return isInstance();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object newInstance(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return newInstance();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getParentClass(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getParentClass();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object isSubclassOf(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return isSubclassOf();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getStaticProperties(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getStaticProperties();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getStaticPropertyValue(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getStaticPropertyValue();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object setStaticPropertyValue(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return setStaticPropertyValue();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getDefaultProperties(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getDefaultProperties();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object isIterateable(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return isIterateable();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object implementsInterface(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return implementsInterface();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getExtension(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getExtension();
 }

 /// <summary>
 /// 
 /// </summary>
 [EditorBrowsable(EditorBrowsableState.Never)]
 public virtual object getExtensionName(PhpStack stack)
 {
	 stack.RemoveFrame();
	 return getExtensionName();
 }

 #endregion
}
}
*/