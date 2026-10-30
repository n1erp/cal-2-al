using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.CodeAnalysis.CSharp;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class CSharpResources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Microsoft.CodeAnalysis.CSharp.CSharpResources", typeof(CSharpResources).GetTypeInfo().Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string CantReferenceCompilationOf => ResourceManager.GetString("CantReferenceCompilationOf", resourceCulture);

	internal static string ChainingSpeculativeModelIsNotSupported => ResourceManager.GetString("ChainingSpeculativeModelIsNotSupported", resourceCulture);

	internal static string CompilationC => ResourceManager.GetString("CompilationC", resourceCulture);

	internal static string CouldNotFindFile => ResourceManager.GetString("CouldNotFindFile", resourceCulture);

	internal static string ElementsCannotBeNull => ResourceManager.GetString("ElementsCannotBeNull", resourceCulture);

	internal static string ERR_AbstractAndExtern => ResourceManager.GetString("ERR_AbstractAndExtern", resourceCulture);

	internal static string ERR_AbstractAndSealed => ResourceManager.GetString("ERR_AbstractAndSealed", resourceCulture);

	internal static string ERR_AbstractAttributeClass => ResourceManager.GetString("ERR_AbstractAttributeClass", resourceCulture);

	internal static string ERR_AbstractBaseCall => ResourceManager.GetString("ERR_AbstractBaseCall", resourceCulture);

	internal static string ERR_AbstractEventInitializer => ResourceManager.GetString("ERR_AbstractEventInitializer", resourceCulture);

	internal static string ERR_AbstractField => ResourceManager.GetString("ERR_AbstractField", resourceCulture);

	internal static string ERR_AbstractHasBody => ResourceManager.GetString("ERR_AbstractHasBody", resourceCulture);

	internal static string ERR_AbstractInConcreteClass => ResourceManager.GetString("ERR_AbstractInConcreteClass", resourceCulture);

	internal static string ERR_AbstractNotVirtual => ResourceManager.GetString("ERR_AbstractNotVirtual", resourceCulture);

	internal static string ERR_AbstractSealedStatic => ResourceManager.GetString("ERR_AbstractSealedStatic", resourceCulture);

	internal static string ERR_AccessModMissingAccessor => ResourceManager.GetString("ERR_AccessModMissingAccessor", resourceCulture);

	internal static string ERR_AccessorImplementingMethod => ResourceManager.GetString("ERR_AccessorImplementingMethod", resourceCulture);

	internal static string ERR_AddModuleAssembly => ResourceManager.GetString("ERR_AddModuleAssembly", resourceCulture);

	internal static string ERR_AddOrRemoveExpected => ResourceManager.GetString("ERR_AddOrRemoveExpected", resourceCulture);

	internal static string ERR_AddRemoveMustHaveBody => ResourceManager.GetString("ERR_AddRemoveMustHaveBody", resourceCulture);

	internal static string ERR_AgnosticToMachineModule => ResourceManager.GetString("ERR_AgnosticToMachineModule", resourceCulture);

	internal static string ERR_AliasMissingFile => ResourceManager.GetString("ERR_AliasMissingFile", resourceCulture);

	internal static string ERR_AliasNotFound => ResourceManager.GetString("ERR_AliasNotFound", resourceCulture);

	internal static string ERR_AliasQualAsExpression => ResourceManager.GetString("ERR_AliasQualAsExpression", resourceCulture);

	internal static string ERR_AliasQualifiedNameNotAnExpression => ResourceManager.GetString("ERR_AliasQualifiedNameNotAnExpression", resourceCulture);

	internal static string ERR_AmbigBinaryOps => ResourceManager.GetString("ERR_AmbigBinaryOps", resourceCulture);

	internal static string ERR_AmbigBinaryOpsOnDefault => ResourceManager.GetString("ERR_AmbigBinaryOpsOnDefault", resourceCulture);

	internal static string ERR_AmbigCall => ResourceManager.GetString("ERR_AmbigCall", resourceCulture);

	internal static string ERR_AmbigContext => ResourceManager.GetString("ERR_AmbigContext", resourceCulture);

	internal static string ERR_AmbigMember => ResourceManager.GetString("ERR_AmbigMember", resourceCulture);

	internal static string ERR_AmbigOverride => ResourceManager.GetString("ERR_AmbigOverride", resourceCulture);

	internal static string ERR_AmbigQM => ResourceManager.GetString("ERR_AmbigQM", resourceCulture);

	internal static string ERR_AmbigUDConv => ResourceManager.GetString("ERR_AmbigUDConv", resourceCulture);

	internal static string ERR_AmbigUnaryOp => ResourceManager.GetString("ERR_AmbigUnaryOp", resourceCulture);

	internal static string ERR_AmbiguousAttribute => ResourceManager.GetString("ERR_AmbiguousAttribute", resourceCulture);

	internal static string ERR_AnonDelegateCantUse => ResourceManager.GetString("ERR_AnonDelegateCantUse", resourceCulture);

	internal static string ERR_AnonDelegateCantUseLocal => ResourceManager.GetString("ERR_AnonDelegateCantUseLocal", resourceCulture);

	internal static string ERR_AnonMethGrpInForEach => ResourceManager.GetString("ERR_AnonMethGrpInForEach", resourceCulture);

	internal static string ERR_AnonMethToNonDel => ResourceManager.GetString("ERR_AnonMethToNonDel", resourceCulture);

	internal static string ERR_AnonymousMethodToExpressionTree => ResourceManager.GetString("ERR_AnonymousMethodToExpressionTree", resourceCulture);

	internal static string ERR_AnonymousReturnExpected => ResourceManager.GetString("ERR_AnonymousReturnExpected", resourceCulture);

	internal static string ERR_AnonymousTypeDuplicatePropertyName => ResourceManager.GetString("ERR_AnonymousTypeDuplicatePropertyName", resourceCulture);

	internal static string ERR_AnonymousTypeNotAvailable => ResourceManager.GetString("ERR_AnonymousTypeNotAvailable", resourceCulture);

	internal static string ERR_AnonymousTypePropertyAssignedBadValue => ResourceManager.GetString("ERR_AnonymousTypePropertyAssignedBadValue", resourceCulture);

	internal static string ERR_ArgsInvalid => ResourceManager.GetString("ERR_ArgsInvalid", resourceCulture);

	internal static string ERR_ArrayElementCantBeRefAny => ResourceManager.GetString("ERR_ArrayElementCantBeRefAny", resourceCulture);

	internal static string ERR_ArrayInitializerExpected => ResourceManager.GetString("ERR_ArrayInitializerExpected", resourceCulture);

	internal static string ERR_ArrayInitializerIncorrectLength => ResourceManager.GetString("ERR_ArrayInitializerIncorrectLength", resourceCulture);

	internal static string ERR_ArrayInitInBadPlace => ResourceManager.GetString("ERR_ArrayInitInBadPlace", resourceCulture);

	internal static string ERR_ArrayInitToNonArrayType => ResourceManager.GetString("ERR_ArrayInitToNonArrayType", resourceCulture);

	internal static string ERR_ArrayOfStaticClass => ResourceManager.GetString("ERR_ArrayOfStaticClass", resourceCulture);

	internal static string ERR_ArraySizeInDeclaration => ResourceManager.GetString("ERR_ArraySizeInDeclaration", resourceCulture);

	internal static string ERR_AsMustHaveReferenceType => ResourceManager.GetString("ERR_AsMustHaveReferenceType", resourceCulture);

	internal static string ERR_AssemblyMatchBadVersion => ResourceManager.GetString("ERR_AssemblyMatchBadVersion", resourceCulture);

	internal static string ERR_AssemblyNameOnNonModule => ResourceManager.GetString("ERR_AssemblyNameOnNonModule", resourceCulture);

	internal static string ERR_AssemblySpecifiedForLinkAndRef => ResourceManager.GetString("ERR_AssemblySpecifiedForLinkAndRef", resourceCulture);

	internal static string ERR_AssgLvalueExpected => ResourceManager.GetString("ERR_AssgLvalueExpected", resourceCulture);

	internal static string ERR_AssgReadonly => ResourceManager.GetString("ERR_AssgReadonly", resourceCulture);

	internal static string ERR_AssgReadonly2 => ResourceManager.GetString("ERR_AssgReadonly2", resourceCulture);

	internal static string ERR_AssgReadonlyLocal => ResourceManager.GetString("ERR_AssgReadonlyLocal", resourceCulture);

	internal static string ERR_AssgReadonlyLocal2Cause => ResourceManager.GetString("ERR_AssgReadonlyLocal2Cause", resourceCulture);

	internal static string ERR_AssgReadonlyLocalCause => ResourceManager.GetString("ERR_AssgReadonlyLocalCause", resourceCulture);

	internal static string ERR_AssgReadonlyProp => ResourceManager.GetString("ERR_AssgReadonlyProp", resourceCulture);

	internal static string ERR_AssgReadonlyStatic => ResourceManager.GetString("ERR_AssgReadonlyStatic", resourceCulture);

	internal static string ERR_AssgReadonlyStatic2 => ResourceManager.GetString("ERR_AssgReadonlyStatic2", resourceCulture);

	internal static string ERR_AssignReadonlyNotField => ResourceManager.GetString("ERR_AssignReadonlyNotField", resourceCulture);

	internal static string ERR_AssignReadonlyNotField2 => ResourceManager.GetString("ERR_AssignReadonlyNotField2", resourceCulture);

	internal static string ERR_AsWithTypeVar => ResourceManager.GetString("ERR_AsWithTypeVar", resourceCulture);

	internal static string ERR_AttrArgWithTypeVars => ResourceManager.GetString("ERR_AttrArgWithTypeVars", resourceCulture);

	internal static string ERR_AttributeCantBeGeneric => ResourceManager.GetString("ERR_AttributeCantBeGeneric", resourceCulture);

	internal static string ERR_AttributeCtorInParameter => ResourceManager.GetString("ERR_AttributeCtorInParameter", resourceCulture);

	internal static string ERR_AttributeNotOnAccessor => ResourceManager.GetString("ERR_AttributeNotOnAccessor", resourceCulture);

	internal static string ERR_AttributeOnBadSymbolType => ResourceManager.GetString("ERR_AttributeOnBadSymbolType", resourceCulture);

	internal static string ERR_AttributeParameterRequired1 => ResourceManager.GetString("ERR_AttributeParameterRequired1", resourceCulture);

	internal static string ERR_AttributeParameterRequired2 => ResourceManager.GetString("ERR_AttributeParameterRequired2", resourceCulture);

	internal static string ERR_AttributesInLocalFuncDecl => ResourceManager.GetString("ERR_AttributesInLocalFuncDecl", resourceCulture);

	internal static string ERR_AttributesNotAllowed => ResourceManager.GetString("ERR_AttributesNotAllowed", resourceCulture);

	internal static string ERR_AttributeUsageOnNonAttributeClass => ResourceManager.GetString("ERR_AttributeUsageOnNonAttributeClass", resourceCulture);

	internal static string ERR_AutoPropertyCannotBeRefReturning => ResourceManager.GetString("ERR_AutoPropertyCannotBeRefReturning", resourceCulture);

	internal static string ERR_AutoPropertyInitializerInInterface => ResourceManager.GetString("ERR_AutoPropertyInitializerInInterface", resourceCulture);

	internal static string ERR_AutoPropertyMustHaveGetAccessor => ResourceManager.GetString("ERR_AutoPropertyMustHaveGetAccessor", resourceCulture);

	internal static string ERR_AutoPropertyMustOverrideSet => ResourceManager.GetString("ERR_AutoPropertyMustOverrideSet", resourceCulture);

	internal static string ERR_AutoPropsInRoStruct => ResourceManager.GetString("ERR_AutoPropsInRoStruct", resourceCulture);

	internal static string ERR_AwaitInUnsafeContext => ResourceManager.GetString("ERR_AwaitInUnsafeContext", resourceCulture);

	internal static string ERR_BadAccess => ResourceManager.GetString("ERR_BadAccess", resourceCulture);

	internal static string ERR_BadAppConfigPath => ResourceManager.GetString("ERR_BadAppConfigPath", resourceCulture);

	internal static string ERR_BadArgCount => ResourceManager.GetString("ERR_BadArgCount", resourceCulture);

	internal static string ERR_BadArgExtraRef => ResourceManager.GetString("ERR_BadArgExtraRef", resourceCulture);

	internal static string ERR_BadArgRef => ResourceManager.GetString("ERR_BadArgRef", resourceCulture);

	internal static string ERR_BadArgType => ResourceManager.GetString("ERR_BadArgType", resourceCulture);

	internal static string ERR_BadArgTypeDynamicExtension => ResourceManager.GetString("ERR_BadArgTypeDynamicExtension", resourceCulture);

	internal static string ERR_BadArgTypesForCollectionAdd => ResourceManager.GetString("ERR_BadArgTypesForCollectionAdd", resourceCulture);

	internal static string ERR_BadArgumentToAttribute => ResourceManager.GetString("ERR_BadArgumentToAttribute", resourceCulture);

	internal static string ERR_BadArity => ResourceManager.GetString("ERR_BadArity", resourceCulture);

	internal static string ERR_BadArraySyntax => ResourceManager.GetString("ERR_BadArraySyntax", resourceCulture);

	internal static string ERR_BadAssemblyName => ResourceManager.GetString("ERR_BadAssemblyName", resourceCulture);

	internal static string ERR_BadAsyncArgType => ResourceManager.GetString("ERR_BadAsyncArgType", resourceCulture);

	internal static string ERR_BadAsyncExpressionTree => ResourceManager.GetString("ERR_BadAsyncExpressionTree", resourceCulture);

	internal static string ERR_BadAsyncLacksBody => ResourceManager.GetString("ERR_BadAsyncLacksBody", resourceCulture);

	internal static string ERR_BadAsyncLocalType => ResourceManager.GetString("ERR_BadAsyncLocalType", resourceCulture);

	internal static string ERR_BadAsyncMethodBuilderTaskProperty => ResourceManager.GetString("ERR_BadAsyncMethodBuilderTaskProperty", resourceCulture);

	internal static string ERR_BadAsyncReturn => ResourceManager.GetString("ERR_BadAsyncReturn", resourceCulture);

	internal static string ERR_BadAsyncReturnExpression => ResourceManager.GetString("ERR_BadAsyncReturnExpression", resourceCulture);

	internal static string ERR_BadAttributeArgument => ResourceManager.GetString("ERR_BadAttributeArgument", resourceCulture);

	internal static string ERR_BadAttributeParamDefaultArgument => ResourceManager.GetString("ERR_BadAttributeParamDefaultArgument", resourceCulture);

	internal static string ERR_BadAttributeParamType => ResourceManager.GetString("ERR_BadAttributeParamType", resourceCulture);

	internal static string ERR_BadAwaitArg => ResourceManager.GetString("ERR_BadAwaitArg", resourceCulture);

	internal static string ERR_BadAwaitArg_NeedSystem => ResourceManager.GetString("ERR_BadAwaitArg_NeedSystem", resourceCulture);

	internal static string ERR_BadAwaitArgIntrinsic => ResourceManager.GetString("ERR_BadAwaitArgIntrinsic", resourceCulture);

	internal static string ERR_BadAwaitArgVoidCall => ResourceManager.GetString("ERR_BadAwaitArgVoidCall", resourceCulture);

	internal static string ERR_BadAwaitAsIdentifier => ResourceManager.GetString("ERR_BadAwaitAsIdentifier", resourceCulture);

	internal static string ERR_BadAwaiterPattern => ResourceManager.GetString("ERR_BadAwaiterPattern", resourceCulture);

	internal static string ERR_BadAwaitInCatch => ResourceManager.GetString("ERR_BadAwaitInCatch", resourceCulture);

	internal static string ERR_BadAwaitInCatchFilter => ResourceManager.GetString("ERR_BadAwaitInCatchFilter", resourceCulture);

	internal static string ERR_BadAwaitInFinally => ResourceManager.GetString("ERR_BadAwaitInFinally", resourceCulture);

	internal static string ERR_BadAwaitInLock => ResourceManager.GetString("ERR_BadAwaitInLock", resourceCulture);

	internal static string ERR_BadAwaitInQuery => ResourceManager.GetString("ERR_BadAwaitInQuery", resourceCulture);

	internal static string ERR_BadAwaitInStaticVariableInitializer => ResourceManager.GetString("ERR_BadAwaitInStaticVariableInitializer", resourceCulture);

	internal static string ERR_BadAwaitWithoutAsync => ResourceManager.GetString("ERR_BadAwaitWithoutAsync", resourceCulture);

	internal static string ERR_BadAwaitWithoutAsyncLambda => ResourceManager.GetString("ERR_BadAwaitWithoutAsyncLambda", resourceCulture);

	internal static string ERR_BadAwaitWithoutAsyncMethod => ResourceManager.GetString("ERR_BadAwaitWithoutAsyncMethod", resourceCulture);

	internal static string ERR_BadAwaitWithoutVoidAsyncMethod => ResourceManager.GetString("ERR_BadAwaitWithoutVoidAsyncMethod", resourceCulture);

	internal static string ERR_BadBaseNumber => ResourceManager.GetString("ERR_BadBaseNumber", resourceCulture);

	internal static string ERR_BadBaseType => ResourceManager.GetString("ERR_BadBaseType", resourceCulture);

	internal static string ERR_BadBinaryOperatorSignature => ResourceManager.GetString("ERR_BadBinaryOperatorSignature", resourceCulture);

	internal static string ERR_BadBinaryOps => ResourceManager.GetString("ERR_BadBinaryOps", resourceCulture);

	internal static string ERR_BadBinOpArgs => ResourceManager.GetString("ERR_BadBinOpArgs", resourceCulture);

	internal static string ERR_BadBoolOp => ResourceManager.GetString("ERR_BadBoolOp", resourceCulture);

	internal static string ERR_BadBoundType => ResourceManager.GetString("ERR_BadBoundType", resourceCulture);

	internal static string ERR_BadCallerFilePathParamWithoutDefaultValue => ResourceManager.GetString("ERR_BadCallerFilePathParamWithoutDefaultValue", resourceCulture);

	internal static string ERR_BadCallerLineNumberParamWithoutDefaultValue => ResourceManager.GetString("ERR_BadCallerLineNumberParamWithoutDefaultValue", resourceCulture);

	internal static string ERR_BadCallerMemberNameParamWithoutDefaultValue => ResourceManager.GetString("ERR_BadCallerMemberNameParamWithoutDefaultValue", resourceCulture);

	internal static string ERR_BadCastInFixed => ResourceManager.GetString("ERR_BadCastInFixed", resourceCulture);

	internal static string ERR_BadCoClassSig => ResourceManager.GetString("ERR_BadCoClassSig", resourceCulture);

	internal static string ERR_BadCompatMode => ResourceManager.GetString("ERR_BadCompatMode", resourceCulture);

	internal static string ERR_BadCompilationOptionValue => ResourceManager.GetString("ERR_BadCompilationOptionValue", resourceCulture);

	internal static string ERR_BadConstraintType => ResourceManager.GetString("ERR_BadConstraintType", resourceCulture);

	internal static string ERR_BadConstType => ResourceManager.GetString("ERR_BadConstType", resourceCulture);

	internal static string ERR_BadCtorArgCount => ResourceManager.GetString("ERR_BadCtorArgCount", resourceCulture);

	internal static string ERR_BadDebugType => ResourceManager.GetString("ERR_BadDebugType", resourceCulture);

	internal static string ERR_BadDelArgCount => ResourceManager.GetString("ERR_BadDelArgCount", resourceCulture);

	internal static string ERR_BadDelegateConstructor => ResourceManager.GetString("ERR_BadDelegateConstructor", resourceCulture);

	internal static string ERR_BadDelegateLeave => ResourceManager.GetString("ERR_BadDelegateLeave", resourceCulture);

	internal static string ERR_BadDestructorName => ResourceManager.GetString("ERR_BadDestructorName", resourceCulture);

	internal static string ERR_BadDirectivePlacement => ResourceManager.GetString("ERR_BadDirectivePlacement", resourceCulture);

	internal static string ERR_BadDocumentationMode => ResourceManager.GetString("ERR_BadDocumentationMode", resourceCulture);

	internal static string ERR_BadDynamicConversion => ResourceManager.GetString("ERR_BadDynamicConversion", resourceCulture);

	internal static string ERR_BadDynamicMethodArg => ResourceManager.GetString("ERR_BadDynamicMethodArg", resourceCulture);

	internal static string ERR_BadDynamicMethodArgDefaultLiteral => ResourceManager.GetString("ERR_BadDynamicMethodArgDefaultLiteral", resourceCulture);

	internal static string ERR_BadDynamicMethodArgLambda => ResourceManager.GetString("ERR_BadDynamicMethodArgLambda", resourceCulture);

	internal static string ERR_BadDynamicMethodArgMemgrp => ResourceManager.GetString("ERR_BadDynamicMethodArgMemgrp", resourceCulture);

	internal static string ERR_BadDynamicQuery => ResourceManager.GetString("ERR_BadDynamicQuery", resourceCulture);

	internal static string ERR_BadDynamicTypeof => ResourceManager.GetString("ERR_BadDynamicTypeof", resourceCulture);

	internal static string ERR_BadEmbeddedStmt => ResourceManager.GetString("ERR_BadEmbeddedStmt", resourceCulture);

	internal static string ERR_BadEmptyThrow => ResourceManager.GetString("ERR_BadEmptyThrow", resourceCulture);

	internal static string ERR_BadEmptyThrowInFinally => ResourceManager.GetString("ERR_BadEmptyThrowInFinally", resourceCulture);

	internal static string ERR_BadEventUsage => ResourceManager.GetString("ERR_BadEventUsage", resourceCulture);

	internal static string ERR_BadEventUsageNoField => ResourceManager.GetString("ERR_BadEventUsageNoField", resourceCulture);

	internal static string ERR_BadExceptionType => ResourceManager.GetString("ERR_BadExceptionType", resourceCulture);

	internal static string ERR_BadExtensionAgg => ResourceManager.GetString("ERR_BadExtensionAgg", resourceCulture);

	internal static string ERR_BadExtensionMeth => ResourceManager.GetString("ERR_BadExtensionMeth", resourceCulture);

	internal static string ERR_BadExternAlias => ResourceManager.GetString("ERR_BadExternAlias", resourceCulture);

	internal static string ERR_BadExternIdentifier => ResourceManager.GetString("ERR_BadExternIdentifier", resourceCulture);

	internal static string ERR_BadFinallyLeave => ResourceManager.GetString("ERR_BadFinallyLeave", resourceCulture);

	internal static string ERR_BadFixedInitType => ResourceManager.GetString("ERR_BadFixedInitType", resourceCulture);

	internal static string ERR_BadForeachDecl => ResourceManager.GetString("ERR_BadForeachDecl", resourceCulture);

	internal static string ERR_BadGetEnumerator => ResourceManager.GetString("ERR_BadGetEnumerator", resourceCulture);

	internal static string ERR_BadIncDecRetType => ResourceManager.GetString("ERR_BadIncDecRetType", resourceCulture);

	internal static string ERR_BadIncDecSignature => ResourceManager.GetString("ERR_BadIncDecSignature", resourceCulture);

	internal static string ERR_BadIndexCount => ResourceManager.GetString("ERR_BadIndexCount", resourceCulture);

	internal static string ERR_BadIndexerNameAttr => ResourceManager.GetString("ERR_BadIndexerNameAttr", resourceCulture);

	internal static string ERR_BadIndexLHS => ResourceManager.GetString("ERR_BadIndexLHS", resourceCulture);

	internal static string ERR_BadInstanceArgType => ResourceManager.GetString("ERR_BadInstanceArgType", resourceCulture);

	internal static string ERR_BadIsPatternExpression => ResourceManager.GetString("ERR_BadIsPatternExpression", resourceCulture);

	internal static string ERR_BadIteratorArgType => ResourceManager.GetString("ERR_BadIteratorArgType", resourceCulture);

	internal static string ERR_BadIteratorLocalType => ResourceManager.GetString("ERR_BadIteratorLocalType", resourceCulture);

	internal static string ERR_BadIteratorReturn => ResourceManager.GetString("ERR_BadIteratorReturn", resourceCulture);

	internal static string ERR_BadIteratorReturnRef => ResourceManager.GetString("ERR_BadIteratorReturnRef", resourceCulture);

	internal static string ERR_BadLanguageVersion => ResourceManager.GetString("ERR_BadLanguageVersion", resourceCulture);

	internal static string ERR_BadMemberFlag => ResourceManager.GetString("ERR_BadMemberFlag", resourceCulture);

	internal static string ERR_BadMemberProtection => ResourceManager.GetString("ERR_BadMemberProtection", resourceCulture);

	internal static string ERR_BadModifierLocation => ResourceManager.GetString("ERR_BadModifierLocation", resourceCulture);

	internal static string ERR_BadModifiersOnNamespace => ResourceManager.GetString("ERR_BadModifiersOnNamespace", resourceCulture);

	internal static string ERR_BadModuleName => ResourceManager.GetString("ERR_BadModuleName", resourceCulture);

	internal static string ERR_BadNamedArgument => ResourceManager.GetString("ERR_BadNamedArgument", resourceCulture);

	internal static string ERR_BadNamedArgumentForDelegateInvoke => ResourceManager.GetString("ERR_BadNamedArgumentForDelegateInvoke", resourceCulture);

	internal static string ERR_BadNamedAttributeArgument => ResourceManager.GetString("ERR_BadNamedAttributeArgument", resourceCulture);

	internal static string ERR_BadNamedAttributeArgumentType => ResourceManager.GetString("ERR_BadNamedAttributeArgumentType", resourceCulture);

	internal static string ERR_BadNewExpr => ResourceManager.GetString("ERR_BadNewExpr", resourceCulture);

	internal static string ERR_BadNonTrailingNamedArgument => ResourceManager.GetString("ERR_BadNonTrailingNamedArgument", resourceCulture);

	internal static string ERR_BadOperatorSyntax => ResourceManager.GetString("ERR_BadOperatorSyntax", resourceCulture);

	internal static string ERR_BadOpOnNullOrDefault => ResourceManager.GetString("ERR_BadOpOnNullOrDefault", resourceCulture);

	internal static string ERR_BadParameterModifiers => ResourceManager.GetString("ERR_BadParameterModifiers", resourceCulture);

	internal static string ERR_BadParamExtraRef => ResourceManager.GetString("ERR_BadParamExtraRef", resourceCulture);

	internal static string ERR_BadParamModThis => ResourceManager.GetString("ERR_BadParamModThis", resourceCulture);

	internal static string ERR_BadParamRef => ResourceManager.GetString("ERR_BadParamRef", resourceCulture);

	internal static string ERR_BadParamType => ResourceManager.GetString("ERR_BadParamType", resourceCulture);

	internal static string ERR_BadPdbData => ResourceManager.GetString("ERR_BadPdbData", resourceCulture);

	internal static string ERR_BadPlatformType => ResourceManager.GetString("ERR_BadPlatformType", resourceCulture);

	internal static string ERR_BadPrefer32OnLib => ResourceManager.GetString("ERR_BadPrefer32OnLib", resourceCulture);

	internal static string ERR_BadProtectedAccess => ResourceManager.GetString("ERR_BadProtectedAccess", resourceCulture);

	internal static string ERR_BadRefReturnExpressionTree => ResourceManager.GetString("ERR_BadRefReturnExpressionTree", resourceCulture);

	internal static string ERR_BadResourceVis => ResourceManager.GetString("ERR_BadResourceVis", resourceCulture);

	internal static string ERR_BadRetType => ResourceManager.GetString("ERR_BadRetType", resourceCulture);

	internal static string ERR_BadShiftOperatorSignature => ResourceManager.GetString("ERR_BadShiftOperatorSignature", resourceCulture);

	internal static string ERR_BadSKknown => ResourceManager.GetString("ERR_BadSKknown", resourceCulture);

	internal static string ERR_BadSKunknown => ResourceManager.GetString("ERR_BadSKunknown", resourceCulture);

	internal static string ERR_BadSourceCodeKind => ResourceManager.GetString("ERR_BadSourceCodeKind", resourceCulture);

	internal static string ERR_BadSpecialByRefIterator => ResourceManager.GetString("ERR_BadSpecialByRefIterator", resourceCulture);

	internal static string ERR_BadSpecialByRefLocal => ResourceManager.GetString("ERR_BadSpecialByRefLocal", resourceCulture);

	internal static string ERR_BadStackAllocExpr => ResourceManager.GetString("ERR_BadStackAllocExpr", resourceCulture);

	internal static string ERR_BadSwitch => ResourceManager.GetString("ERR_BadSwitch", resourceCulture);

	internal static string ERR_BadThisParam => ResourceManager.GetString("ERR_BadThisParam", resourceCulture);

	internal static string ERR_BadTypeArgument => ResourceManager.GetString("ERR_BadTypeArgument", resourceCulture);

	internal static string ERR_BadTypeforThis => ResourceManager.GetString("ERR_BadTypeforThis", resourceCulture);

	internal static string ERR_BadTypeReference => ResourceManager.GetString("ERR_BadTypeReference", resourceCulture);

	internal static string ERR_BadUnaryOp => ResourceManager.GetString("ERR_BadUnaryOp", resourceCulture);

	internal static string ERR_BadUnaryOperatorSignature => ResourceManager.GetString("ERR_BadUnaryOperatorSignature", resourceCulture);

	internal static string ERR_BadUnOpArgs => ResourceManager.GetString("ERR_BadUnOpArgs", resourceCulture);

	internal static string ERR_BadUsingNamespace => ResourceManager.GetString("ERR_BadUsingNamespace", resourceCulture);

	internal static string ERR_BadUsingType => ResourceManager.GetString("ERR_BadUsingType", resourceCulture);

	internal static string ERR_BadVarargs => ResourceManager.GetString("ERR_BadVarargs", resourceCulture);

	internal static string ERR_BadVarDecl => ResourceManager.GetString("ERR_BadVarDecl", resourceCulture);

	internal static string ERR_BadVisBaseClass => ResourceManager.GetString("ERR_BadVisBaseClass", resourceCulture);

	internal static string ERR_BadVisBaseInterface => ResourceManager.GetString("ERR_BadVisBaseInterface", resourceCulture);

	internal static string ERR_BadVisBound => ResourceManager.GetString("ERR_BadVisBound", resourceCulture);

	internal static string ERR_BadVisDelegateParam => ResourceManager.GetString("ERR_BadVisDelegateParam", resourceCulture);

	internal static string ERR_BadVisDelegateReturn => ResourceManager.GetString("ERR_BadVisDelegateReturn", resourceCulture);

	internal static string ERR_BadVisEventType => ResourceManager.GetString("ERR_BadVisEventType", resourceCulture);

	internal static string ERR_BadVisFieldType => ResourceManager.GetString("ERR_BadVisFieldType", resourceCulture);

	internal static string ERR_BadVisIndexerParam => ResourceManager.GetString("ERR_BadVisIndexerParam", resourceCulture);

	internal static string ERR_BadVisIndexerReturn => ResourceManager.GetString("ERR_BadVisIndexerReturn", resourceCulture);

	internal static string ERR_BadVisOpParam => ResourceManager.GetString("ERR_BadVisOpParam", resourceCulture);

	internal static string ERR_BadVisOpReturn => ResourceManager.GetString("ERR_BadVisOpReturn", resourceCulture);

	internal static string ERR_BadVisParamType => ResourceManager.GetString("ERR_BadVisParamType", resourceCulture);

	internal static string ERR_BadVisPropertyType => ResourceManager.GetString("ERR_BadVisPropertyType", resourceCulture);

	internal static string ERR_BadVisReturnType => ResourceManager.GetString("ERR_BadVisReturnType", resourceCulture);

	internal static string ERR_BadWarningLevel => ResourceManager.GetString("ERR_BadWarningLevel", resourceCulture);

	internal static string ERR_BadWin32Res => ResourceManager.GetString("ERR_BadWin32Res", resourceCulture);

	internal static string ERR_BadYieldInCatch => ResourceManager.GetString("ERR_BadYieldInCatch", resourceCulture);

	internal static string ERR_BadYieldInFinally => ResourceManager.GetString("ERR_BadYieldInFinally", resourceCulture);

	internal static string ERR_BadYieldInTryOfCatch => ResourceManager.GetString("ERR_BadYieldInTryOfCatch", resourceCulture);

	internal static string ERR_BaseClassMustBeFirst => ResourceManager.GetString("ERR_BaseClassMustBeFirst", resourceCulture);

	internal static string ERR_BaseConstraintConflict => ResourceManager.GetString("ERR_BaseConstraintConflict", resourceCulture);

	internal static string ERR_BaseIllegal => ResourceManager.GetString("ERR_BaseIllegal", resourceCulture);

	internal static string ERR_BaseInBadContext => ResourceManager.GetString("ERR_BaseInBadContext", resourceCulture);

	internal static string ERR_BaseInStaticMeth => ResourceManager.GetString("ERR_BaseInStaticMeth", resourceCulture);

	internal static string ERR_BinaryFile => ResourceManager.GetString("ERR_BinaryFile", resourceCulture);

	internal static string ERR_BindToBogus => ResourceManager.GetString("ERR_BindToBogus", resourceCulture);

	internal static string ERR_BindToBogusProp1 => ResourceManager.GetString("ERR_BindToBogusProp1", resourceCulture);

	internal static string ERR_BindToBogusProp2 => ResourceManager.GetString("ERR_BindToBogusProp2", resourceCulture);

	internal static string ERR_BlockBodyAndExpressionBody => ResourceManager.GetString("ERR_BlockBodyAndExpressionBody", resourceCulture);

	internal static string ERR_BogusExplicitImpl => ResourceManager.GetString("ERR_BogusExplicitImpl", resourceCulture);

	internal static string ERR_BogusType => ResourceManager.GetString("ERR_BogusType", resourceCulture);

	internal static string ERR_ByReferenceVariableMustBeInitialized => ResourceManager.GetString("ERR_ByReferenceVariableMustBeInitialized", resourceCulture);

	internal static string ERR_ByRefParameterInExpressionTree => ResourceManager.GetString("ERR_ByRefParameterInExpressionTree", resourceCulture);

	internal static string ERR_ByRefReturnUnsupported => ResourceManager.GetString("ERR_ByRefReturnUnsupported", resourceCulture);

	internal static string ERR_ByRefTypeAndAwait => ResourceManager.GetString("ERR_ByRefTypeAndAwait", resourceCulture);

	internal static string ERR_CallArgMixing => ResourceManager.GetString("ERR_CallArgMixing", resourceCulture);

	internal static string ERR_CallingBaseFinalizeDeprecated => ResourceManager.GetString("ERR_CallingBaseFinalizeDeprecated", resourceCulture);

	internal static string ERR_CallingFinalizeDeprecated => ResourceManager.GetString("ERR_CallingFinalizeDeprecated", resourceCulture);

	internal static string ERR_CannotDeconstructDynamic => ResourceManager.GetString("ERR_CannotDeconstructDynamic", resourceCulture);

	internal static string ERR_CannotEmbedWithoutPdb => ResourceManager.GetString("ERR_CannotEmbedWithoutPdb", resourceCulture);

	internal static string ERR_CannotPassNullForFriendAssembly => ResourceManager.GetString("ERR_CannotPassNullForFriendAssembly", resourceCulture);

	internal static string ERR_CantCallSpecialMethod => ResourceManager.GetString("ERR_CantCallSpecialMethod", resourceCulture);

	internal static string ERR_CantChangeAccessOnOverride => ResourceManager.GetString("ERR_CantChangeAccessOnOverride", resourceCulture);

	internal static string ERR_CantChangeRefReturnOnOverride => ResourceManager.GetString("ERR_CantChangeRefReturnOnOverride", resourceCulture);

	internal static string ERR_CantChangeReturnTypeOnOverride => ResourceManager.GetString("ERR_CantChangeReturnTypeOnOverride", resourceCulture);

	internal static string ERR_CantChangeTupleNamesOnOverride => ResourceManager.GetString("ERR_CantChangeTupleNamesOnOverride", resourceCulture);

	internal static string ERR_CantChangeTypeOnOverride => ResourceManager.GetString("ERR_CantChangeTypeOnOverride", resourceCulture);

	internal static string ERR_CantConvAnonMethNoParams => ResourceManager.GetString("ERR_CantConvAnonMethNoParams", resourceCulture);

	internal static string ERR_CantConvAnonMethParams => ResourceManager.GetString("ERR_CantConvAnonMethParams", resourceCulture);

	internal static string ERR_CantConvAnonMethReturns => ResourceManager.GetString("ERR_CantConvAnonMethReturns", resourceCulture);

	internal static string ERR_CantConvAsyncAnonFuncReturns => ResourceManager.GetString("ERR_CantConvAsyncAnonFuncReturns", resourceCulture);

	internal static string ERR_CantDeriveFromSealedType => ResourceManager.GetString("ERR_CantDeriveFromSealedType", resourceCulture);

	internal static string ERR_CantHaveWin32ResAndIcon => ResourceManager.GetString("ERR_CantHaveWin32ResAndIcon", resourceCulture);

	internal static string ERR_CantHaveWin32ResAndManifest => ResourceManager.GetString("ERR_CantHaveWin32ResAndManifest", resourceCulture);

	internal static string ERR_CantInferMethTypeArgs => ResourceManager.GetString("ERR_CantInferMethTypeArgs", resourceCulture);

	internal static string ERR_CantMakeTempFile => ResourceManager.GetString("ERR_CantMakeTempFile", resourceCulture);

	internal static string ERR_CantOpenFileWrite => ResourceManager.GetString("ERR_CantOpenFileWrite", resourceCulture);

	internal static string ERR_CantOpenIcon => ResourceManager.GetString("ERR_CantOpenIcon", resourceCulture);

	internal static string ERR_CantOpenWin32Manifest => ResourceManager.GetString("ERR_CantOpenWin32Manifest", resourceCulture);

	internal static string ERR_CantOpenWin32Res => ResourceManager.GetString("ERR_CantOpenWin32Res", resourceCulture);

	internal static string ERR_CantOverrideBogusMethod => ResourceManager.GetString("ERR_CantOverrideBogusMethod", resourceCulture);

	internal static string ERR_CantOverrideNonEvent => ResourceManager.GetString("ERR_CantOverrideNonEvent", resourceCulture);

	internal static string ERR_CantOverrideNonFunction => ResourceManager.GetString("ERR_CantOverrideNonFunction", resourceCulture);

	internal static string ERR_CantOverrideNonProperty => ResourceManager.GetString("ERR_CantOverrideNonProperty", resourceCulture);

	internal static string ERR_CantOverrideNonVirtual => ResourceManager.GetString("ERR_CantOverrideNonVirtual", resourceCulture);

	internal static string ERR_CantOverrideSealed => ResourceManager.GetString("ERR_CantOverrideSealed", resourceCulture);

	internal static string ERR_CantReadConfigFile => ResourceManager.GetString("ERR_CantReadConfigFile", resourceCulture);

	internal static string ERR_CantReadResource => ResourceManager.GetString("ERR_CantReadResource", resourceCulture);

	internal static string ERR_CantReadRulesetFile => ResourceManager.GetString("ERR_CantReadRulesetFile", resourceCulture);

	internal static string ERR_CantRefResource => ResourceManager.GetString("ERR_CantRefResource", resourceCulture);

	internal static string ERR_CantReturnVoid => ResourceManager.GetString("ERR_CantReturnVoid", resourceCulture);

	internal static string ERR_CantSetWin32Manifest => ResourceManager.GetString("ERR_CantSetWin32Manifest", resourceCulture);

	internal static string ERR_CantUseInOrOutInArglist => ResourceManager.GetString("ERR_CantUseInOrOutInArglist", resourceCulture);

	internal static string ERR_CantUseRequiredAttribute => ResourceManager.GetString("ERR_CantUseRequiredAttribute", resourceCulture);

	internal static string ERR_CantUseVoidInArglist => ResourceManager.GetString("ERR_CantUseVoidInArglist", resourceCulture);

	internal static string ERR_CheckedOverflow => ResourceManager.GetString("ERR_CheckedOverflow", resourceCulture);

	internal static string ERR_CircConstValue => ResourceManager.GetString("ERR_CircConstValue", resourceCulture);

	internal static string ERR_CircularBase => ResourceManager.GetString("ERR_CircularBase", resourceCulture);

	internal static string ERR_CircularConstraint => ResourceManager.GetString("ERR_CircularConstraint", resourceCulture);

	internal static string ERR_ClassBoundNotFirst => ResourceManager.GetString("ERR_ClassBoundNotFirst", resourceCulture);

	internal static string ERR_ClassDoesntImplementInterface => ResourceManager.GetString("ERR_ClassDoesntImplementInterface", resourceCulture);

	internal static string ERR_ClassTypeExpected => ResourceManager.GetString("ERR_ClassTypeExpected", resourceCulture);

	internal static string ERR_CloseParenExpected => ResourceManager.GetString("ERR_CloseParenExpected", resourceCulture);

	internal static string ERR_CloseUnimplementedInterfaceMemberNotPublic => ResourceManager.GetString("ERR_CloseUnimplementedInterfaceMemberNotPublic", resourceCulture);

	internal static string ERR_CloseUnimplementedInterfaceMemberStatic => ResourceManager.GetString("ERR_CloseUnimplementedInterfaceMemberStatic", resourceCulture);

	internal static string ERR_CloseUnimplementedInterfaceMemberWrongRefReturn => ResourceManager.GetString("ERR_CloseUnimplementedInterfaceMemberWrongRefReturn", resourceCulture);

	internal static string ERR_CloseUnimplementedInterfaceMemberWrongReturnType => ResourceManager.GetString("ERR_CloseUnimplementedInterfaceMemberWrongReturnType", resourceCulture);

	internal static string ERR_CmdOptionConflictsSource => ResourceManager.GetString("ERR_CmdOptionConflictsSource", resourceCulture);

	internal static string ERR_ColColWithTypeAlias => ResourceManager.GetString("ERR_ColColWithTypeAlias", resourceCulture);

	internal static string ERR_CollectionInitRequiresIEnumerable => ResourceManager.GetString("ERR_CollectionInitRequiresIEnumerable", resourceCulture);

	internal static string ERR_ComImportWithBase => ResourceManager.GetString("ERR_ComImportWithBase", resourceCulture);

	internal static string ERR_ComImportWithImpl => ResourceManager.GetString("ERR_ComImportWithImpl", resourceCulture);

	internal static string ERR_ComImportWithInitializers => ResourceManager.GetString("ERR_ComImportWithInitializers", resourceCulture);

	internal static string ERR_ComImportWithoutUuidAttribute => ResourceManager.GetString("ERR_ComImportWithoutUuidAttribute", resourceCulture);

	internal static string ERR_ComImportWithUserCtor => ResourceManager.GetString("ERR_ComImportWithUserCtor", resourceCulture);

	internal static string ERR_CompileCancelled => ResourceManager.GetString("ERR_CompileCancelled", resourceCulture);

	internal static string ERR_CompilerAndLanguageVersion => ResourceManager.GetString("ERR_CompilerAndLanguageVersion", resourceCulture);

	internal static string ERR_ComRefCallInExpressionTree => ResourceManager.GetString("ERR_ComRefCallInExpressionTree", resourceCulture);

	internal static string ERR_ConcreteMissingBody => ResourceManager.GetString("ERR_ConcreteMissingBody", resourceCulture);

	internal static string ERR_ConditionalInInterpolation => ResourceManager.GetString("ERR_ConditionalInInterpolation", resourceCulture);

	internal static string ERR_ConditionalMustReturnVoid => ResourceManager.GetString("ERR_ConditionalMustReturnVoid", resourceCulture);

	internal static string ERR_ConditionalOnInterfaceMethod => ResourceManager.GetString("ERR_ConditionalOnInterfaceMethod", resourceCulture);

	internal static string ERR_ConditionalOnNonAttributeClass => ResourceManager.GetString("ERR_ConditionalOnNonAttributeClass", resourceCulture);

	internal static string ERR_ConditionalOnOverride => ResourceManager.GetString("ERR_ConditionalOnOverride", resourceCulture);

	internal static string ERR_ConditionalOnSpecialMethod => ResourceManager.GetString("ERR_ConditionalOnSpecialMethod", resourceCulture);

	internal static string ERR_ConditionalWithOutParam => ResourceManager.GetString("ERR_ConditionalWithOutParam", resourceCulture);

	internal static string ERR_ConflictAliasAndMember => ResourceManager.GetString("ERR_ConflictAliasAndMember", resourceCulture);

	internal static string ERR_ConflictingAliasAndDefinition => ResourceManager.GetString("ERR_ConflictingAliasAndDefinition", resourceCulture);

	internal static string ERR_ConflictingMachineModule => ResourceManager.GetString("ERR_ConflictingMachineModule", resourceCulture);

	internal static string ERR_ConstantExpected => ResourceManager.GetString("ERR_ConstantExpected", resourceCulture);

	internal static string ERR_ConstantStringTooLong => ResourceManager.GetString("ERR_ConstantStringTooLong", resourceCulture);

	internal static string ERR_ConstOutOfRange => ResourceManager.GetString("ERR_ConstOutOfRange", resourceCulture);

	internal static string ERR_ConstOutOfRangeChecked => ResourceManager.GetString("ERR_ConstOutOfRangeChecked", resourceCulture);

	internal static string ERR_ConstraintIsStaticClass => ResourceManager.GetString("ERR_ConstraintIsStaticClass", resourceCulture);

	internal static string ERR_ConstraintOnlyAllowedOnGenericDecl => ResourceManager.GetString("ERR_ConstraintOnlyAllowedOnGenericDecl", resourceCulture);

	internal static string ERR_ConstructedDynamicTypeAsBound => ResourceManager.GetString("ERR_ConstructedDynamicTypeAsBound", resourceCulture);

	internal static string ERR_ConstructorInStaticClass => ResourceManager.GetString("ERR_ConstructorInStaticClass", resourceCulture);

	internal static string ERR_ConstValueRequired => ResourceManager.GetString("ERR_ConstValueRequired", resourceCulture);

	internal static string ERR_ConversionNotInvolvingContainedType => ResourceManager.GetString("ERR_ConversionNotInvolvingContainedType", resourceCulture);

	internal static string ERR_ConversionNotTupleCompatible => ResourceManager.GetString("ERR_ConversionNotTupleCompatible", resourceCulture);

	internal static string ERR_ConversionWithBase => ResourceManager.GetString("ERR_ConversionWithBase", resourceCulture);

	internal static string ERR_ConversionWithDerived => ResourceManager.GetString("ERR_ConversionWithDerived", resourceCulture);

	internal static string ERR_ConversionWithInterface => ResourceManager.GetString("ERR_ConversionWithInterface", resourceCulture);

	internal static string ERR_ConvertToStaticClass => ResourceManager.GetString("ERR_ConvertToStaticClass", resourceCulture);

	internal static string ERR_ConWithUnmanagedCon => ResourceManager.GetString("ERR_ConWithUnmanagedCon", resourceCulture);

	internal static string ERR_ConWithValCon => ResourceManager.GetString("ERR_ConWithValCon", resourceCulture);

	internal static string ERR_CryptoHashFailed => ResourceManager.GetString("ERR_CryptoHashFailed", resourceCulture);

	internal static string ERR_CStyleArray => ResourceManager.GetString("ERR_CStyleArray", resourceCulture);

	internal static string ERR_CycleInInterfaceInheritance => ResourceManager.GetString("ERR_CycleInInterfaceInheritance", resourceCulture);

	internal static string ERR_CycleInTypeForwarder => ResourceManager.GetString("ERR_CycleInTypeForwarder", resourceCulture);

	internal static string ERR_DebugEntryPointNotSourceMethodDefinition => ResourceManager.GetString("ERR_DebugEntryPointNotSourceMethodDefinition", resourceCulture);

	internal static string ERR_DecConstError => ResourceManager.GetString("ERR_DecConstError", resourceCulture);

	internal static string ERR_DeclarationExpressionNotPermitted => ResourceManager.GetString("ERR_DeclarationExpressionNotPermitted", resourceCulture);

	internal static string ERR_DeconstructionVarFormDisallowsSpecificType => ResourceManager.GetString("ERR_DeconstructionVarFormDisallowsSpecificType", resourceCulture);

	internal static string ERR_DeconstructRequiresExpression => ResourceManager.GetString("ERR_DeconstructRequiresExpression", resourceCulture);

	internal static string ERR_DeconstructTooFewElements => ResourceManager.GetString("ERR_DeconstructTooFewElements", resourceCulture);

	internal static string ERR_DeconstructWrongCardinality => ResourceManager.GetString("ERR_DeconstructWrongCardinality", resourceCulture);

	internal static string ERR_DefaultInPattern => ResourceManager.GetString("ERR_DefaultInPattern", resourceCulture);

	internal static string ERR_DefaultInSwitch => ResourceManager.GetString("ERR_DefaultInSwitch", resourceCulture);

	internal static string ERR_DefaultLiteralNotValid => ResourceManager.GetString("ERR_DefaultLiteralNotValid", resourceCulture);

	internal static string ERR_DefaultMemberOnIndexedType => ResourceManager.GetString("ERR_DefaultMemberOnIndexedType", resourceCulture);

	internal static string ERR_DefaultValueBadValueType => ResourceManager.GetString("ERR_DefaultValueBadValueType", resourceCulture);

	internal static string ERR_DefaultValueBeforeRequiredValue => ResourceManager.GetString("ERR_DefaultValueBeforeRequiredValue", resourceCulture);

	internal static string ERR_DefaultValueForExtensionParameter => ResourceManager.GetString("ERR_DefaultValueForExtensionParameter", resourceCulture);

	internal static string ERR_DefaultValueForParamsParameter => ResourceManager.GetString("ERR_DefaultValueForParamsParameter", resourceCulture);

	internal static string ERR_DefaultValueMustBeConstant => ResourceManager.GetString("ERR_DefaultValueMustBeConstant", resourceCulture);

	internal static string ERR_DefaultValueNotAllowed => ResourceManager.GetString("ERR_DefaultValueNotAllowed", resourceCulture);

	internal static string ERR_DefaultValueTypeMustMatch => ResourceManager.GetString("ERR_DefaultValueTypeMustMatch", resourceCulture);

	internal static string ERR_DefaultValueUsedWithAttributes => ResourceManager.GetString("ERR_DefaultValueUsedWithAttributes", resourceCulture);

	internal static string ERR_DelegateOnConditional => ResourceManager.GetString("ERR_DelegateOnConditional", resourceCulture);

	internal static string ERR_DelegateOnNullable => ResourceManager.GetString("ERR_DelegateOnNullable", resourceCulture);

	internal static string ERR_DelegateRefMismatch => ResourceManager.GetString("ERR_DelegateRefMismatch", resourceCulture);

	internal static string ERR_DeprecatedCollectionInitAddStr => ResourceManager.GetString("ERR_DeprecatedCollectionInitAddStr", resourceCulture);

	internal static string ERR_DeprecatedSymbolStr => ResourceManager.GetString("ERR_DeprecatedSymbolStr", resourceCulture);

	internal static string ERR_DeriveFromConstructedDynamic => ResourceManager.GetString("ERR_DeriveFromConstructedDynamic", resourceCulture);

	internal static string ERR_DeriveFromDynamic => ResourceManager.GetString("ERR_DeriveFromDynamic", resourceCulture);

	internal static string ERR_DeriveFromEnumOrValueType => ResourceManager.GetString("ERR_DeriveFromEnumOrValueType", resourceCulture);

	internal static string ERR_DerivingFromATyVar => ResourceManager.GetString("ERR_DerivingFromATyVar", resourceCulture);

	internal static string ERR_DestructorInStaticClass => ResourceManager.GetString("ERR_DestructorInStaticClass", resourceCulture);

	internal static string ERR_DictionaryInitializerInExpressionTree => ResourceManager.GetString("ERR_DictionaryInitializerInExpressionTree", resourceCulture);

	internal static string ERR_DiscardTypeInferenceFailed => ResourceManager.GetString("ERR_DiscardTypeInferenceFailed", resourceCulture);

	internal static string ERR_DllImportOnGenericMethod => ResourceManager.GetString("ERR_DllImportOnGenericMethod", resourceCulture);

	internal static string ERR_DllImportOnInvalidMethod => ResourceManager.GetString("ERR_DllImportOnInvalidMethod", resourceCulture);

	internal static string ERR_DocFileGen => ResourceManager.GetString("ERR_DocFileGen", resourceCulture);

	internal static string ERR_DoesntImplementAwaitInterface => ResourceManager.GetString("ERR_DoesntImplementAwaitInterface", resourceCulture);

	internal static string ERR_DoNotUseFixedBufferAttr => ResourceManager.GetString("ERR_DoNotUseFixedBufferAttr", resourceCulture);

	internal static string ERR_DoNotUseFixedBufferAttrOnProperty => ResourceManager.GetString("ERR_DoNotUseFixedBufferAttrOnProperty", resourceCulture);

	internal static string ERR_DottedTypeNameNotFoundInAgg => ResourceManager.GetString("ERR_DottedTypeNameNotFoundInAgg", resourceCulture);

	internal static string ERR_DottedTypeNameNotFoundInNS => ResourceManager.GetString("ERR_DottedTypeNameNotFoundInNS", resourceCulture);

	internal static string ERR_DottedTypeNameNotFoundInNSFwd => ResourceManager.GetString("ERR_DottedTypeNameNotFoundInNSFwd", resourceCulture);

	internal static string ERR_DuplicateAccessor => ResourceManager.GetString("ERR_DuplicateAccessor", resourceCulture);

	internal static string ERR_DuplicateAlias => ResourceManager.GetString("ERR_DuplicateAlias", resourceCulture);

	internal static string ERR_DuplicateAttribute => ResourceManager.GetString("ERR_DuplicateAttribute", resourceCulture);

	internal static string ERR_DuplicateAttributeInNetModule => ResourceManager.GetString("ERR_DuplicateAttributeInNetModule", resourceCulture);

	internal static string ERR_DuplicateBound => ResourceManager.GetString("ERR_DuplicateBound", resourceCulture);

	internal static string ERR_DuplicateCaseLabel => ResourceManager.GetString("ERR_DuplicateCaseLabel", resourceCulture);

	internal static string ERR_DuplicateConstraintClause => ResourceManager.GetString("ERR_DuplicateConstraintClause", resourceCulture);

	internal static string ERR_DuplicateConversionInClass => ResourceManager.GetString("ERR_DuplicateConversionInClass", resourceCulture);

	internal static string ERR_DuplicateGeneratedName => ResourceManager.GetString("ERR_DuplicateGeneratedName", resourceCulture);

	internal static string ERR_DuplicateImport => ResourceManager.GetString("ERR_DuplicateImport", resourceCulture);

	internal static string ERR_DuplicateImportSimple => ResourceManager.GetString("ERR_DuplicateImportSimple", resourceCulture);

	internal static string ERR_DuplicateInterfaceInBaseList => ResourceManager.GetString("ERR_DuplicateInterfaceInBaseList", resourceCulture);

	internal static string ERR_DuplicateInterfaceWithTupleNamesInBaseList => ResourceManager.GetString("ERR_DuplicateInterfaceWithTupleNamesInBaseList", resourceCulture);

	internal static string ERR_DuplicateLabel => ResourceManager.GetString("ERR_DuplicateLabel", resourceCulture);

	internal static string ERR_DuplicateModifier => ResourceManager.GetString("ERR_DuplicateModifier", resourceCulture);

	internal static string ERR_DuplicateNamedArgument => ResourceManager.GetString("ERR_DuplicateNamedArgument", resourceCulture);

	internal static string ERR_DuplicateNamedAttributeArgument => ResourceManager.GetString("ERR_DuplicateNamedAttributeArgument", resourceCulture);

	internal static string ERR_DuplicateNameInClass => ResourceManager.GetString("ERR_DuplicateNameInClass", resourceCulture);

	internal static string ERR_DuplicateNameInNS => ResourceManager.GetString("ERR_DuplicateNameInNS", resourceCulture);

	internal static string ERR_DuplicateParamName => ResourceManager.GetString("ERR_DuplicateParamName", resourceCulture);

	internal static string ERR_DuplicatePropertyAccessMods => ResourceManager.GetString("ERR_DuplicatePropertyAccessMods", resourceCulture);

	internal static string ERR_DuplicateTypeForwarder => ResourceManager.GetString("ERR_DuplicateTypeForwarder", resourceCulture);

	internal static string ERR_DuplicateTypeParameter => ResourceManager.GetString("ERR_DuplicateTypeParameter", resourceCulture);

	internal static string ERR_DupParamMod => ResourceManager.GetString("ERR_DupParamMod", resourceCulture);

	internal static string ERR_DynamicAttributeMissing => ResourceManager.GetString("ERR_DynamicAttributeMissing", resourceCulture);

	internal static string ERR_DynamicLocalFunctionParamsParameter => ResourceManager.GetString("ERR_DynamicLocalFunctionParamsParameter", resourceCulture);

	internal static string ERR_DynamicLocalFunctionTypeParameter => ResourceManager.GetString("ERR_DynamicLocalFunctionTypeParameter", resourceCulture);

	internal static string ERR_DynamicRequiredTypesMissing => ResourceManager.GetString("ERR_DynamicRequiredTypesMissing", resourceCulture);

	internal static string ERR_DynamicTypeAsBound => ResourceManager.GetString("ERR_DynamicTypeAsBound", resourceCulture);

	internal static string ERR_EmptyCharConst => ResourceManager.GetString("ERR_EmptyCharConst", resourceCulture);

	internal static string ERR_EmptyElementInitializer => ResourceManager.GetString("ERR_EmptyElementInitializer", resourceCulture);

	internal static string ERR_EmptyFormatSpecifier => ResourceManager.GetString("ERR_EmptyFormatSpecifier", resourceCulture);

	internal static string ERR_EmptyYield => ResourceManager.GetString("ERR_EmptyYield", resourceCulture);

	internal static string ERR_EncNoPIAReference => ResourceManager.GetString("ERR_EncNoPIAReference", resourceCulture);

	internal static string ERR_EncodinglessSyntaxTree => ResourceManager.GetString("ERR_EncodinglessSyntaxTree", resourceCulture);

	internal static string ERR_EncReferenceToAddedMember => ResourceManager.GetString("ERR_EncReferenceToAddedMember", resourceCulture);

	internal static string ERR_EncUpdateFailedMissingAttribute => ResourceManager.GetString("ERR_EncUpdateFailedMissingAttribute", resourceCulture);

	internal static string ERR_EndifDirectiveExpected => ResourceManager.GetString("ERR_EndifDirectiveExpected", resourceCulture);

	internal static string ERR_EndOfPPLineExpected => ResourceManager.GetString("ERR_EndOfPPLineExpected", resourceCulture);

	internal static string ERR_EndRegionDirectiveExpected => ResourceManager.GetString("ERR_EndRegionDirectiveExpected", resourceCulture);

	internal static string ERR_EnumeratorOverflow => ResourceManager.GetString("ERR_EnumeratorOverflow", resourceCulture);

	internal static string ERR_EnumsCantContainDefaultConstructor => ResourceManager.GetString("ERR_EnumsCantContainDefaultConstructor", resourceCulture);

	internal static string ERR_EOFExpected => ResourceManager.GetString("ERR_EOFExpected", resourceCulture);

	internal static string ERR_ErrorBuildingWin32Resources => ResourceManager.GetString("ERR_ErrorBuildingWin32Resources", resourceCulture);

	internal static string ERR_ErrorDirective => ResourceManager.GetString("ERR_ErrorDirective", resourceCulture);

	internal static string ERR_ErrorInReferencedAssembly => ResourceManager.GetString("ERR_ErrorInReferencedAssembly", resourceCulture);

	internal static string ERR_EscapeCall => ResourceManager.GetString("ERR_EscapeCall", resourceCulture);

	internal static string ERR_EscapeCall2 => ResourceManager.GetString("ERR_EscapeCall2", resourceCulture);

	internal static string ERR_EscapedCurly => ResourceManager.GetString("ERR_EscapedCurly", resourceCulture);

	internal static string ERR_EscapeLocal => ResourceManager.GetString("ERR_EscapeLocal", resourceCulture);

	internal static string ERR_EscapeOther => ResourceManager.GetString("ERR_EscapeOther", resourceCulture);

	internal static string ERR_EscapeStackAlloc => ResourceManager.GetString("ERR_EscapeStackAlloc", resourceCulture);

	internal static string ERR_EventNeedsBothAccessors => ResourceManager.GetString("ERR_EventNeedsBothAccessors", resourceCulture);

	internal static string ERR_EventNotDelegate => ResourceManager.GetString("ERR_EventNotDelegate", resourceCulture);

	internal static string ERR_EventPropertyInInterface => ResourceManager.GetString("ERR_EventPropertyInInterface", resourceCulture);

	internal static string ERR_ExpectedContextualKeywordBy => ResourceManager.GetString("ERR_ExpectedContextualKeywordBy", resourceCulture);

	internal static string ERR_ExpectedContextualKeywordEquals => ResourceManager.GetString("ERR_ExpectedContextualKeywordEquals", resourceCulture);

	internal static string ERR_ExpectedContextualKeywordOn => ResourceManager.GetString("ERR_ExpectedContextualKeywordOn", resourceCulture);

	internal static string ERR_ExpectedEndTry => ResourceManager.GetString("ERR_ExpectedEndTry", resourceCulture);

	internal static string ERR_ExpectedPPFile => ResourceManager.GetString("ERR_ExpectedPPFile", resourceCulture);

	internal static string ERR_ExpectedSelectOrGroup => ResourceManager.GetString("ERR_ExpectedSelectOrGroup", resourceCulture);

	internal static string ERR_ExpectedSingleScript => ResourceManager.GetString("ERR_ExpectedSingleScript", resourceCulture);

	internal static string ERR_ExpectedVerbatimLiteral => ResourceManager.GetString("ERR_ExpectedVerbatimLiteral", resourceCulture);

	internal static string ERR_ExplicitDynamicAttr => ResourceManager.GetString("ERR_ExplicitDynamicAttr", resourceCulture);

	internal static string ERR_ExplicitEventFieldImpl => ResourceManager.GetString("ERR_ExplicitEventFieldImpl", resourceCulture);

	internal static string ERR_ExplicitExtension => ResourceManager.GetString("ERR_ExplicitExtension", resourceCulture);

	internal static string ERR_ExplicitImplCollisionOnRefOut => ResourceManager.GetString("ERR_ExplicitImplCollisionOnRefOut", resourceCulture);

	internal static string ERR_ExplicitImplParams => ResourceManager.GetString("ERR_ExplicitImplParams", resourceCulture);

	internal static string ERR_ExplicitInterfaceImplementationInNonClassOrStruct => ResourceManager.GetString("ERR_ExplicitInterfaceImplementationInNonClassOrStruct", resourceCulture);

	internal static string ERR_ExplicitInterfaceImplementationNotInterface => ResourceManager.GetString("ERR_ExplicitInterfaceImplementationNotInterface", resourceCulture);

	internal static string ERR_ExplicitMethodImplAccessor => ResourceManager.GetString("ERR_ExplicitMethodImplAccessor", resourceCulture);

	internal static string ERR_ExplicitParamArray => ResourceManager.GetString("ERR_ExplicitParamArray", resourceCulture);

	internal static string ERR_ExplicitPropertyAddingAccessor => ResourceManager.GetString("ERR_ExplicitPropertyAddingAccessor", resourceCulture);

	internal static string ERR_ExplicitPropertyMissingAccessor => ResourceManager.GetString("ERR_ExplicitPropertyMissingAccessor", resourceCulture);

	internal static string ERR_ExplicitReservedAttr => ResourceManager.GetString("ERR_ExplicitReservedAttr", resourceCulture);

	internal static string ERR_ExplicitTupleElementNamesAttribute => ResourceManager.GetString("ERR_ExplicitTupleElementNamesAttribute", resourceCulture);

	internal static string ERR_ExportedTypeConflictsWithDeclaration => ResourceManager.GetString("ERR_ExportedTypeConflictsWithDeclaration", resourceCulture);

	internal static string ERR_ExportedTypesConflict => ResourceManager.GetString("ERR_ExportedTypesConflict", resourceCulture);

	internal static string ERR_ExprCannotBeFixed => ResourceManager.GetString("ERR_ExprCannotBeFixed", resourceCulture);

	internal static string ERR_ExpressionExpected => ResourceManager.GetString("ERR_ExpressionExpected", resourceCulture);

	internal static string ERR_ExpressionHasNoName => ResourceManager.GetString("ERR_ExpressionHasNoName", resourceCulture);

	internal static string ERR_ExpressionOrDeclarationExpected => ResourceManager.GetString("ERR_ExpressionOrDeclarationExpected", resourceCulture);

	internal static string ERR_ExpressionTreeContainsAnonymousMethod => ResourceManager.GetString("ERR_ExpressionTreeContainsAnonymousMethod", resourceCulture);

	internal static string ERR_ExpressionTreeContainsAssignment => ResourceManager.GetString("ERR_ExpressionTreeContainsAssignment", resourceCulture);

	internal static string ERR_ExpressionTreeContainsBadCoalesce => ResourceManager.GetString("ERR_ExpressionTreeContainsBadCoalesce", resourceCulture);

	internal static string ERR_ExpressionTreeContainsBaseAccess => ResourceManager.GetString("ERR_ExpressionTreeContainsBaseAccess", resourceCulture);

	internal static string ERR_ExpressionTreeContainsDiscard => ResourceManager.GetString("ERR_ExpressionTreeContainsDiscard", resourceCulture);

	internal static string ERR_ExpressionTreeContainsDynamicOperation => ResourceManager.GetString("ERR_ExpressionTreeContainsDynamicOperation", resourceCulture);

	internal static string ERR_ExpressionTreeContainsIndexedProperty => ResourceManager.GetString("ERR_ExpressionTreeContainsIndexedProperty", resourceCulture);

	internal static string ERR_ExpressionTreeContainsIsMatch => ResourceManager.GetString("ERR_ExpressionTreeContainsIsMatch", resourceCulture);

	internal static string ERR_ExpressionTreeContainsLocalFunction => ResourceManager.GetString("ERR_ExpressionTreeContainsLocalFunction", resourceCulture);

	internal static string ERR_ExpressionTreeContainsMultiDimensionalArrayInitializer => ResourceManager.GetString("ERR_ExpressionTreeContainsMultiDimensionalArrayInitializer", resourceCulture);

	internal static string ERR_ExpressionTreeContainsNamedArgument => ResourceManager.GetString("ERR_ExpressionTreeContainsNamedArgument", resourceCulture);

	internal static string ERR_ExpressionTreeContainsOptionalArgument => ResourceManager.GetString("ERR_ExpressionTreeContainsOptionalArgument", resourceCulture);

	internal static string ERR_ExpressionTreeContainsOutVariable => ResourceManager.GetString("ERR_ExpressionTreeContainsOutVariable", resourceCulture);

	internal static string ERR_ExpressionTreeContainsPointerOp => ResourceManager.GetString("ERR_ExpressionTreeContainsPointerOp", resourceCulture);

	internal static string ERR_ExpressionTreeContainsThrowExpression => ResourceManager.GetString("ERR_ExpressionTreeContainsThrowExpression", resourceCulture);

	internal static string ERR_ExpressionTreeContainsTupleBinOp => ResourceManager.GetString("ERR_ExpressionTreeContainsTupleBinOp", resourceCulture);

	internal static string ERR_ExpressionTreeContainsTupleConversion => ResourceManager.GetString("ERR_ExpressionTreeContainsTupleConversion", resourceCulture);

	internal static string ERR_ExpressionTreeContainsTupleLiteral => ResourceManager.GetString("ERR_ExpressionTreeContainsTupleLiteral", resourceCulture);

	internal static string ERR_ExpressionTreeMustHaveDelegate => ResourceManager.GetString("ERR_ExpressionTreeMustHaveDelegate", resourceCulture);

	internal static string ERR_ExtensionAttrNotFound => ResourceManager.GetString("ERR_ExtensionAttrNotFound", resourceCulture);

	internal static string ERR_ExtensionCollectionElementInitializerInExpressionTree => ResourceManager.GetString("ERR_ExtensionCollectionElementInitializerInExpressionTree", resourceCulture);

	internal static string ERR_ExtensionMethodsDecl => ResourceManager.GetString("ERR_ExtensionMethodsDecl", resourceCulture);

	internal static string ERR_ExternAfterElements => ResourceManager.GetString("ERR_ExternAfterElements", resourceCulture);

	internal static string ERR_ExternAliasNotAllowed => ResourceManager.GetString("ERR_ExternAliasNotAllowed", resourceCulture);

	internal static string ERR_ExternHasBody => ResourceManager.GetString("ERR_ExternHasBody", resourceCulture);

	internal static string ERR_ExternHasConstructorInitializer => ResourceManager.GetString("ERR_ExternHasConstructorInitializer", resourceCulture);

	internal static string ERR_FeatureIsUnimplemented => ResourceManager.GetString("ERR_FeatureIsUnimplemented", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion1 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion1", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion2 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion2", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion3 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion3", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion4 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion4", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion5 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion5", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion6 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion6", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion7 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion7", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion7_1 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion7_1", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion7_2 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion7_2", resourceCulture);

	internal static string ERR_FeatureNotAvailableInVersion7_3 => ResourceManager.GetString("ERR_FeatureNotAvailableInVersion7_3", resourceCulture);

	internal static string ERR_FeatureNotValidInExpressionTree => ResourceManager.GetString("ERR_FeatureNotValidInExpressionTree", resourceCulture);

	internal static string ERR_FieldAutoPropCantBeByRefLike => ResourceManager.GetString("ERR_FieldAutoPropCantBeByRefLike", resourceCulture);

	internal static string ERR_FieldCantBeRefAny => ResourceManager.GetString("ERR_FieldCantBeRefAny", resourceCulture);

	internal static string ERR_FieldCantHaveVoidType => ResourceManager.GetString("ERR_FieldCantHaveVoidType", resourceCulture);

	internal static string ERR_FieldHasMultipleDistinctConstantValues => ResourceManager.GetString("ERR_FieldHasMultipleDistinctConstantValues", resourceCulture);

	internal static string ERR_FieldInitializerInStruct => ResourceManager.GetString("ERR_FieldInitializerInStruct", resourceCulture);

	internal static string ERR_FieldInitRefNonstatic => ResourceManager.GetString("ERR_FieldInitRefNonstatic", resourceCulture);

	internal static string ERR_FieldlikeEventsInRoStruct => ResourceManager.GetString("ERR_FieldlikeEventsInRoStruct", resourceCulture);

	internal static string ERR_FieldsInRoStruct => ResourceManager.GetString("ERR_FieldsInRoStruct", resourceCulture);

	internal static string ERR_FileNotFound => ResourceManager.GetString("ERR_FileNotFound", resourceCulture);

	internal static string ERR_FixedBufferNotFixed => ResourceManager.GetString("ERR_FixedBufferNotFixed", resourceCulture);

	internal static string ERR_FixedBufferTooManyDimensions => ResourceManager.GetString("ERR_FixedBufferTooManyDimensions", resourceCulture);

	internal static string ERR_FixedDimsRequired => ResourceManager.GetString("ERR_FixedDimsRequired", resourceCulture);

	internal static string ERR_FixedLocalInLambda => ResourceManager.GetString("ERR_FixedLocalInLambda", resourceCulture);

	internal static string ERR_FixedMustInit => ResourceManager.GetString("ERR_FixedMustInit", resourceCulture);

	internal static string ERR_FixedNeeded => ResourceManager.GetString("ERR_FixedNeeded", resourceCulture);

	internal static string ERR_FixedNeedsLvalue => ResourceManager.GetString("ERR_FixedNeedsLvalue", resourceCulture);

	internal static string ERR_FixedNotInStruct => ResourceManager.GetString("ERR_FixedNotInStruct", resourceCulture);

	internal static string ERR_FixedNotNeeded => ResourceManager.GetString("ERR_FixedNotNeeded", resourceCulture);

	internal static string ERR_FixedOverflow => ResourceManager.GetString("ERR_FixedOverflow", resourceCulture);

	internal static string ERR_FloatOverflow => ResourceManager.GetString("ERR_FloatOverflow", resourceCulture);

	internal static string ERR_ForEachMissingMember => ResourceManager.GetString("ERR_ForEachMissingMember", resourceCulture);

	internal static string ERR_ForwardedTypeConflictsWithDeclaration => ResourceManager.GetString("ERR_ForwardedTypeConflictsWithDeclaration", resourceCulture);

	internal static string ERR_ForwardedTypeConflictsWithExportedType => ResourceManager.GetString("ERR_ForwardedTypeConflictsWithExportedType", resourceCulture);

	internal static string ERR_ForwardedTypeInThisAssembly => ResourceManager.GetString("ERR_ForwardedTypeInThisAssembly", resourceCulture);

	internal static string ERR_ForwardedTypeIsNested => ResourceManager.GetString("ERR_ForwardedTypeIsNested", resourceCulture);

	internal static string ERR_ForwardedTypesConflict => ResourceManager.GetString("ERR_ForwardedTypesConflict", resourceCulture);

	internal static string ERR_FriendAssemblyBadArgs => ResourceManager.GetString("ERR_FriendAssemblyBadArgs", resourceCulture);

	internal static string ERR_FriendAssemblySNReq => ResourceManager.GetString("ERR_FriendAssemblySNReq", resourceCulture);

	internal static string ERR_FriendRefNotEqualToThis => ResourceManager.GetString("ERR_FriendRefNotEqualToThis", resourceCulture);

	internal static string ERR_FriendRefSigningMismatch => ResourceManager.GetString("ERR_FriendRefSigningMismatch", resourceCulture);

	internal static string ERR_GenericArgIsStaticClass => ResourceManager.GetString("ERR_GenericArgIsStaticClass", resourceCulture);

	internal static string ERR_GenericConstraintNotSatisfiedNullableEnum => ResourceManager.GetString("ERR_GenericConstraintNotSatisfiedNullableEnum", resourceCulture);

	internal static string ERR_GenericConstraintNotSatisfiedNullableInterface => ResourceManager.GetString("ERR_GenericConstraintNotSatisfiedNullableInterface", resourceCulture);

	internal static string ERR_GenericConstraintNotSatisfiedRefType => ResourceManager.GetString("ERR_GenericConstraintNotSatisfiedRefType", resourceCulture);

	internal static string ERR_GenericConstraintNotSatisfiedTyVar => ResourceManager.GetString("ERR_GenericConstraintNotSatisfiedTyVar", resourceCulture);

	internal static string ERR_GenericConstraintNotSatisfiedValType => ResourceManager.GetString("ERR_GenericConstraintNotSatisfiedValType", resourceCulture);

	internal static string ERR_GenericDerivingFromAttribute => ResourceManager.GetString("ERR_GenericDerivingFromAttribute", resourceCulture);

	internal static string ERR_GenericsUsedAcrossAssemblies => ResourceManager.GetString("ERR_GenericsUsedAcrossAssemblies", resourceCulture);

	internal static string ERR_GenericsUsedInNoPIAType => ResourceManager.GetString("ERR_GenericsUsedInNoPIAType", resourceCulture);

	internal static string ERR_GetOrSetExpected => ResourceManager.GetString("ERR_GetOrSetExpected", resourceCulture);

	internal static string ERR_GlobalAttributesNotAllowed => ResourceManager.GetString("ERR_GlobalAttributesNotAllowed", resourceCulture);

	internal static string ERR_GlobalAttributesNotFirst => ResourceManager.GetString("ERR_GlobalAttributesNotFirst", resourceCulture);

	internal static string ERR_GlobalDefinitionOrStatementExpected => ResourceManager.GetString("ERR_GlobalDefinitionOrStatementExpected", resourceCulture);

	internal static string ERR_GlobalExternAlias => ResourceManager.GetString("ERR_GlobalExternAlias", resourceCulture);

	internal static string ERR_GlobalSingleTypeNameNotFound => ResourceManager.GetString("ERR_GlobalSingleTypeNameNotFound", resourceCulture);

	internal static string ERR_GlobalSingleTypeNameNotFoundFwd => ResourceManager.GetString("ERR_GlobalSingleTypeNameNotFoundFwd", resourceCulture);

	internal static string ERR_GlobalStatement => ResourceManager.GetString("ERR_GlobalStatement", resourceCulture);

	internal static string ERR_HasNoTypeVars => ResourceManager.GetString("ERR_HasNoTypeVars", resourceCulture);

	internal static string ERR_HidingAbstractMethod => ResourceManager.GetString("ERR_HidingAbstractMethod", resourceCulture);

	internal static string ERR_IdentifierExpected => ResourceManager.GetString("ERR_IdentifierExpected", resourceCulture);

	internal static string ERR_IdentifierExpectedKW => ResourceManager.GetString("ERR_IdentifierExpectedKW", resourceCulture);

	internal static string ERR_IdentityConversion => ResourceManager.GetString("ERR_IdentityConversion", resourceCulture);

	internal static string ERR_IllegalArglist => ResourceManager.GetString("ERR_IllegalArglist", resourceCulture);

	internal static string ERR_IllegalEscape => ResourceManager.GetString("ERR_IllegalEscape", resourceCulture);

	internal static string ERR_IllegalFixedType => ResourceManager.GetString("ERR_IllegalFixedType", resourceCulture);

	internal static string ERR_IllegalInnerUnsafe => ResourceManager.GetString("ERR_IllegalInnerUnsafe", resourceCulture);

	internal static string ERR_IllegalParams => ResourceManager.GetString("ERR_IllegalParams", resourceCulture);

	internal static string ERR_IllegalRefParam => ResourceManager.GetString("ERR_IllegalRefParam", resourceCulture);

	internal static string ERR_IllegalStatement => ResourceManager.GetString("ERR_IllegalStatement", resourceCulture);

	internal static string ERR_IllegalUnsafe => ResourceManager.GetString("ERR_IllegalUnsafe", resourceCulture);

	internal static string ERR_IllegalVarArgs => ResourceManager.GetString("ERR_IllegalVarArgs", resourceCulture);

	internal static string ERR_IllegalVarianceSyntax => ResourceManager.GetString("ERR_IllegalVarianceSyntax", resourceCulture);

	internal static string ERR_ImplBadConstraints => ResourceManager.GetString("ERR_ImplBadConstraints", resourceCulture);

	internal static string ERR_ImplBadTupleNames => ResourceManager.GetString("ERR_ImplBadTupleNames", resourceCulture);

	internal static string ERR_ImplicitlyTypedArrayNoBestType => ResourceManager.GetString("ERR_ImplicitlyTypedArrayNoBestType", resourceCulture);

	internal static string ERR_ImplicitlyTypedLocalCannotBeFixed => ResourceManager.GetString("ERR_ImplicitlyTypedLocalCannotBeFixed", resourceCulture);

	internal static string ERR_ImplicitlyTypedOutVariableUsedInTheSameArgumentList => ResourceManager.GetString("ERR_ImplicitlyTypedOutVariableUsedInTheSameArgumentList", resourceCulture);

	internal static string ERR_ImplicitlyTypedVariableAssignedArrayInitializer => ResourceManager.GetString("ERR_ImplicitlyTypedVariableAssignedArrayInitializer", resourceCulture);

	internal static string ERR_ImplicitlyTypedVariableAssignedBadValue => ResourceManager.GetString("ERR_ImplicitlyTypedVariableAssignedBadValue", resourceCulture);

	internal static string ERR_ImplicitlyTypedVariableCannotBeConst => ResourceManager.GetString("ERR_ImplicitlyTypedVariableCannotBeConst", resourceCulture);

	internal static string ERR_ImplicitlyTypedVariableMultipleDeclarator => ResourceManager.GetString("ERR_ImplicitlyTypedVariableMultipleDeclarator", resourceCulture);

	internal static string ERR_ImplicitlyTypedVariableWithNoInitializer => ResourceManager.GetString("ERR_ImplicitlyTypedVariableWithNoInitializer", resourceCulture);

	internal static string ERR_ImportedCircularBase => ResourceManager.GetString("ERR_ImportedCircularBase", resourceCulture);

	internal static string ERR_ImportNonAssembly => ResourceManager.GetString("ERR_ImportNonAssembly", resourceCulture);

	internal static string ERR_InaccessibleGetter => ResourceManager.GetString("ERR_InaccessibleGetter", resourceCulture);

	internal static string ERR_InaccessibleSetter => ResourceManager.GetString("ERR_InaccessibleSetter", resourceCulture);

	internal static string ERR_InAttrOnOutParam => ResourceManager.GetString("ERR_InAttrOnOutParam", resourceCulture);

	internal static string ERR_InconsistentIndexerNames => ResourceManager.GetString("ERR_InconsistentIndexerNames", resourceCulture);

	internal static string ERR_InconsistentLambdaParameterUsage => ResourceManager.GetString("ERR_InconsistentLambdaParameterUsage", resourceCulture);

	internal static string ERR_IncrementLvalueExpected => ResourceManager.GetString("ERR_IncrementLvalueExpected", resourceCulture);

	internal static string ERR_IndexedPropertyMustHaveAllOptionalParams => ResourceManager.GetString("ERR_IndexedPropertyMustHaveAllOptionalParams", resourceCulture);

	internal static string ERR_IndexedPropertyRequiresParams => ResourceManager.GetString("ERR_IndexedPropertyRequiresParams", resourceCulture);

	internal static string ERR_IndexerCantHaveVoidType => ResourceManager.GetString("ERR_IndexerCantHaveVoidType", resourceCulture);

	internal static string ERR_IndexerInStaticClass => ResourceManager.GetString("ERR_IndexerInStaticClass", resourceCulture);

	internal static string ERR_IndexerNeedsParam => ResourceManager.GetString("ERR_IndexerNeedsParam", resourceCulture);

	internal static string ERR_IndirectRecursiveConstructorCall => ResourceManager.GetString("ERR_IndirectRecursiveConstructorCall", resourceCulture);

	internal static string ERR_InDynamicMethodArg => ResourceManager.GetString("ERR_InDynamicMethodArg", resourceCulture);

	internal static string ERR_InExpected => ResourceManager.GetString("ERR_InExpected", resourceCulture);

	internal static string ERR_InExtensionMustBeValueType => ResourceManager.GetString("ERR_InExtensionMustBeValueType", resourceCulture);

	internal static string ERR_InitializeByReferenceVariableWithValue => ResourceManager.GetString("ERR_InitializeByReferenceVariableWithValue", resourceCulture);

	internal static string ERR_InitializeByValueVariableWithReference => ResourceManager.GetString("ERR_InitializeByValueVariableWithReference", resourceCulture);

	internal static string ERR_InitializerAddHasParamModifiers => ResourceManager.GetString("ERR_InitializerAddHasParamModifiers", resourceCulture);

	internal static string ERR_InitializerAddHasWrongSignature => ResourceManager.GetString("ERR_InitializerAddHasWrongSignature", resourceCulture);

	internal static string ERR_InitializerInStructWithoutExplicitConstructor => ResourceManager.GetString("ERR_InitializerInStructWithoutExplicitConstructor", resourceCulture);

	internal static string ERR_InitializerOnNonAutoProperty => ResourceManager.GetString("ERR_InitializerOnNonAutoProperty", resourceCulture);

	internal static string ERR_InstanceMemberInStaticClass => ResourceManager.GetString("ERR_InstanceMemberInStaticClass", resourceCulture);

	internal static string ERR_InstantiatingStaticClass => ResourceManager.GetString("ERR_InstantiatingStaticClass", resourceCulture);

	internal static string ERR_InsufficientStack => ResourceManager.GetString("ERR_InsufficientStack", resourceCulture);

	internal static string ERR_IntDivByZero => ResourceManager.GetString("ERR_IntDivByZero", resourceCulture);

	internal static string ERR_IntegralTypeExpected => ResourceManager.GetString("ERR_IntegralTypeExpected", resourceCulture);

	internal static string ERR_IntegralTypeValueExpected => ResourceManager.GetString("ERR_IntegralTypeValueExpected", resourceCulture);

	internal static string ERR_InterfaceEventInitializer => ResourceManager.GetString("ERR_InterfaceEventInitializer", resourceCulture);

	internal static string ERR_InterfaceImplementedByConditional => ResourceManager.GetString("ERR_InterfaceImplementedByConditional", resourceCulture);

	internal static string ERR_InterfaceImplementedImplicitlyByVariadic => ResourceManager.GetString("ERR_InterfaceImplementedImplicitlyByVariadic", resourceCulture);

	internal static string ERR_InterfaceMemberHasBody => ResourceManager.GetString("ERR_InterfaceMemberHasBody", resourceCulture);

	internal static string ERR_InterfaceMemberNotFound => ResourceManager.GetString("ERR_InterfaceMemberNotFound", resourceCulture);

	internal static string ERR_InterfacesCannotContainTypes => ResourceManager.GetString("ERR_InterfacesCannotContainTypes", resourceCulture);

	internal static string ERR_InterfacesCantContainConstructors => ResourceManager.GetString("ERR_InterfacesCantContainConstructors", resourceCulture);

	internal static string ERR_InterfacesCantContainFields => ResourceManager.GetString("ERR_InterfacesCantContainFields", resourceCulture);

	internal static string ERR_InterfacesCantContainOperators => ResourceManager.GetString("ERR_InterfacesCantContainOperators", resourceCulture);

	internal static string ERR_InteropMethodWithBody => ResourceManager.GetString("ERR_InteropMethodWithBody", resourceCulture);

	internal static string ERR_InteropStructContainsMethods => ResourceManager.GetString("ERR_InteropStructContainsMethods", resourceCulture);

	internal static string ERR_InteropTypeMissingAttribute => ResourceManager.GetString("ERR_InteropTypeMissingAttribute", resourceCulture);

	internal static string ERR_InteropTypesWithSameNameAndGuid => ResourceManager.GetString("ERR_InteropTypesWithSameNameAndGuid", resourceCulture);

	internal static string ERR_IntOverflow => ResourceManager.GetString("ERR_IntOverflow", resourceCulture);

	internal static string ERR_InvalidAddrOp => ResourceManager.GetString("ERR_InvalidAddrOp", resourceCulture);

	internal static string ERR_InvalidAnonymousTypeMemberDeclarator => ResourceManager.GetString("ERR_InvalidAnonymousTypeMemberDeclarator", resourceCulture);

	internal static string ERR_InvalidArray => ResourceManager.GetString("ERR_InvalidArray", resourceCulture);

	internal static string ERR_InvalidAssemblyCulture => ResourceManager.GetString("ERR_InvalidAssemblyCulture", resourceCulture);

	internal static string ERR_InvalidAssemblyCultureForExe => ResourceManager.GetString("ERR_InvalidAssemblyCultureForExe", resourceCulture);

	internal static string ERR_InvalidAssemblyName => ResourceManager.GetString("ERR_InvalidAssemblyName", resourceCulture);

	internal static string ERR_InvalidAttributeArgument => ResourceManager.GetString("ERR_InvalidAttributeArgument", resourceCulture);

	internal static string ERR_InvalidConstantDeclarationType => ResourceManager.GetString("ERR_InvalidConstantDeclarationType", resourceCulture);

	internal static string ERR_InvalidDebugInfo => ResourceManager.GetString("ERR_InvalidDebugInfo", resourceCulture);

	internal static string ERR_InvalidDebugInformationFormat => ResourceManager.GetString("ERR_InvalidDebugInformationFormat", resourceCulture);

	internal static string ERR_InvalidDelegateType => ResourceManager.GetString("ERR_InvalidDelegateType", resourceCulture);

	internal static string ERR_InvalidDynamicCondition => ResourceManager.GetString("ERR_InvalidDynamicCondition", resourceCulture);

	internal static string ERR_InvalidExprTerm => ResourceManager.GetString("ERR_InvalidExprTerm", resourceCulture);

	internal static string ERR_InvalidFileAlignment => ResourceManager.GetString("ERR_InvalidFileAlignment", resourceCulture);

	internal static string ERR_InvalidFixedArraySize => ResourceManager.GetString("ERR_InvalidFixedArraySize", resourceCulture);

	internal static string ERR_InvalidFormatForGuidForOption => ResourceManager.GetString("ERR_InvalidFormatForGuidForOption", resourceCulture);

	internal static string ERR_InvalidFwdType => ResourceManager.GetString("ERR_InvalidFwdType", resourceCulture);

	internal static string ERR_InvalidGotoCase => ResourceManager.GetString("ERR_InvalidGotoCase", resourceCulture);

	internal static string ERR_InvalidHashAlgorithmName => ResourceManager.GetString("ERR_InvalidHashAlgorithmName", resourceCulture);

	internal static string ERR_InvalidInitializerElementInitializer => ResourceManager.GetString("ERR_InvalidInitializerElementInitializer", resourceCulture);

	internal static string ERR_InvalidInstrumentationKind => ResourceManager.GetString("ERR_InvalidInstrumentationKind", resourceCulture);

	internal static string ERR_InvalidLineNumber => ResourceManager.GetString("ERR_InvalidLineNumber", resourceCulture);

	internal static string ERR_InvalidMemberDecl => ResourceManager.GetString("ERR_InvalidMemberDecl", resourceCulture);

	internal static string ERR_InvalidNamedArgument => ResourceManager.GetString("ERR_InvalidNamedArgument", resourceCulture);

	internal static string ERR_InvalidNumber => ResourceManager.GetString("ERR_InvalidNumber", resourceCulture);

	internal static string ERR_InvalidObjectCreation => ResourceManager.GetString("ERR_InvalidObjectCreation", resourceCulture);

	internal static string ERR_InvalidOutputName => ResourceManager.GetString("ERR_InvalidOutputName", resourceCulture);

	internal static string ERR_InvalidPathMap => ResourceManager.GetString("ERR_InvalidPathMap", resourceCulture);

	internal static string ERR_InvalidPreprocessingSymbol => ResourceManager.GetString("ERR_InvalidPreprocessingSymbol", resourceCulture);

	internal static string ERR_InvalidPreprocExpr => ResourceManager.GetString("ERR_InvalidPreprocExpr", resourceCulture);

	internal static string ERR_InvalidPropertyAccessMod => ResourceManager.GetString("ERR_InvalidPropertyAccessMod", resourceCulture);

	internal static string ERR_InvalidQM => ResourceManager.GetString("ERR_InvalidQM", resourceCulture);

	internal static string ERR_InvalidReal => ResourceManager.GetString("ERR_InvalidReal", resourceCulture);

	internal static string ERR_InvalidSignaturePublicKey => ResourceManager.GetString("ERR_InvalidSignaturePublicKey", resourceCulture);

	internal static string ERR_InvalidSpecifier => ResourceManager.GetString("ERR_InvalidSpecifier", resourceCulture);

	internal static string ERR_InvalidStackAllocArray => ResourceManager.GetString("ERR_InvalidStackAllocArray", resourceCulture);

	internal static string ERR_InvalidSubsystemVersion => ResourceManager.GetString("ERR_InvalidSubsystemVersion", resourceCulture);

	internal static string ERR_InvalidVersionFormat => ResourceManager.GetString("ERR_InvalidVersionFormat", resourceCulture);

	internal static string ERR_InvalidVersionFormat2 => ResourceManager.GetString("ERR_InvalidVersionFormat2", resourceCulture);

	internal static string ERR_InvalidVersionFormatDeterministic => ResourceManager.GetString("ERR_InvalidVersionFormatDeterministic", resourceCulture);

	internal static string ERR_IteratorInInteractive => ResourceManager.GetString("ERR_IteratorInInteractive", resourceCulture);

	internal static string ERR_LabelNotFound => ResourceManager.GetString("ERR_LabelNotFound", resourceCulture);

	internal static string ERR_LabelShadow => ResourceManager.GetString("ERR_LabelShadow", resourceCulture);

	internal static string ERR_LambdaInIsAs => ResourceManager.GetString("ERR_LambdaInIsAs", resourceCulture);

	internal static string ERR_LanguageVersionCannotHaveLeadingZeroes => ResourceManager.GetString("ERR_LanguageVersionCannotHaveLeadingZeroes", resourceCulture);

	internal static string ERR_LbraceExpected => ResourceManager.GetString("ERR_LbraceExpected", resourceCulture);

	internal static string ERR_LegacyObjectIdSyntax => ResourceManager.GetString("ERR_LegacyObjectIdSyntax", resourceCulture);

	internal static string ERR_LinkedNetmoduleMetadataMustProvideFullPEImage => ResourceManager.GetString("ERR_LinkedNetmoduleMetadataMustProvideFullPEImage", resourceCulture);

	internal static string ERR_LiteralDoubleCast => ResourceManager.GetString("ERR_LiteralDoubleCast", resourceCulture);

	internal static string ERR_LoadDirectiveOnlyAllowedInScripts => ResourceManager.GetString("ERR_LoadDirectiveOnlyAllowedInScripts", resourceCulture);

	internal static string ERR_LocalCantBeFixedAndHoisted => ResourceManager.GetString("ERR_LocalCantBeFixedAndHoisted", resourceCulture);

	internal static string ERR_LocalDuplicate => ResourceManager.GetString("ERR_LocalDuplicate", resourceCulture);

	internal static string ERR_LocalFunctionMissingBody => ResourceManager.GetString("ERR_LocalFunctionMissingBody", resourceCulture);

	internal static string ERR_LocalIllegallyOverrides => ResourceManager.GetString("ERR_LocalIllegallyOverrides", resourceCulture);

	internal static string ERR_LocalSameNameAsTypeParam => ResourceManager.GetString("ERR_LocalSameNameAsTypeParam", resourceCulture);

	internal static string ERR_LocalTypeNameClash => ResourceManager.GetString("ERR_LocalTypeNameClash", resourceCulture);

	internal static string ERR_LockNeedsReference => ResourceManager.GetString("ERR_LockNeedsReference", resourceCulture);

	internal static string ERR_LookupInTypeVariable => ResourceManager.GetString("ERR_LookupInTypeVariable", resourceCulture);

	internal static string ERR_MainClassIsImport => ResourceManager.GetString("ERR_MainClassIsImport", resourceCulture);

	internal static string ERR_MainClassNotClass => ResourceManager.GetString("ERR_MainClassNotClass", resourceCulture);

	internal static string ERR_MainClassNotFound => ResourceManager.GetString("ERR_MainClassNotFound", resourceCulture);

	internal static string ERR_ManagedAddr => ResourceManager.GetString("ERR_ManagedAddr", resourceCulture);

	internal static string ERR_MarshalUnmanagedTypeNotValidForFields => ResourceManager.GetString("ERR_MarshalUnmanagedTypeNotValidForFields", resourceCulture);

	internal static string ERR_MarshalUnmanagedTypeOnlyValidForFields => ResourceManager.GetString("ERR_MarshalUnmanagedTypeOnlyValidForFields", resourceCulture);

	internal static string ERR_MemberAlreadyExists => ResourceManager.GetString("ERR_MemberAlreadyExists", resourceCulture);

	internal static string ERR_MemberAlreadyInitialized => ResourceManager.GetString("ERR_MemberAlreadyInitialized", resourceCulture);

	internal static string ERR_MemberCannotBeInitialized => ResourceManager.GetString("ERR_MemberCannotBeInitialized", resourceCulture);

	internal static string ERR_MemberNameSameAsType => ResourceManager.GetString("ERR_MemberNameSameAsType", resourceCulture);

	internal static string ERR_MemberNeedsType => ResourceManager.GetString("ERR_MemberNeedsType", resourceCulture);

	internal static string ERR_MemberReserved => ResourceManager.GetString("ERR_MemberReserved", resourceCulture);

	internal static string ERR_MemGroupInExpressionTree => ResourceManager.GetString("ERR_MemGroupInExpressionTree", resourceCulture);

	internal static string ERR_Merge_conflict_marker_encountered => ResourceManager.GetString("ERR_Merge_conflict_marker_encountered", resourceCulture);

	internal static string ERR_MetadataNameTooLong => ResourceManager.GetString("ERR_MetadataNameTooLong", resourceCulture);

	internal static string ERR_MetadataReferencesNotSupported => ResourceManager.GetString("ERR_MetadataReferencesNotSupported", resourceCulture);

	internal static string ERR_MethDelegateMismatch => ResourceManager.GetString("ERR_MethDelegateMismatch", resourceCulture);

	internal static string ERR_MethGrpToNonDel => ResourceManager.GetString("ERR_MethGrpToNonDel", resourceCulture);

	internal static string ERR_MethodArgCantBeRefAny => ResourceManager.GetString("ERR_MethodArgCantBeRefAny", resourceCulture);

	internal static string ERR_MethodImplementingAccessor => ResourceManager.GetString("ERR_MethodImplementingAccessor", resourceCulture);

	internal static string ERR_MethodNameExpected => ResourceManager.GetString("ERR_MethodNameExpected", resourceCulture);

	internal static string ERR_MethodReturnCantBeRefAny => ResourceManager.GetString("ERR_MethodReturnCantBeRefAny", resourceCulture);

	internal static string ERR_MismatchedRefEscapeInTernary => ResourceManager.GetString("ERR_MismatchedRefEscapeInTernary", resourceCulture);

	internal static string ERR_MissingArgument => ResourceManager.GetString("ERR_MissingArgument", resourceCulture);

	internal static string ERR_MissingArraySize => ResourceManager.GetString("ERR_MissingArraySize", resourceCulture);

	internal static string ERR_MissingCoClass => ResourceManager.GetString("ERR_MissingCoClass", resourceCulture);

	internal static string ERR_MissingDebugSwitch => ResourceManager.GetString("ERR_MissingDebugSwitch", resourceCulture);

	internal static string ERR_MissingDeconstruct => ResourceManager.GetString("ERR_MissingDeconstruct", resourceCulture);

	internal static string ERR_MissingGuidForOption => ResourceManager.GetString("ERR_MissingGuidForOption", resourceCulture);

	internal static string ERR_MissingMethodOnSourceInterface => ResourceManager.GetString("ERR_MissingMethodOnSourceInterface", resourceCulture);

	internal static string ERR_MissingNetModuleReference => ResourceManager.GetString("ERR_MissingNetModuleReference", resourceCulture);

	internal static string ERR_MissingPartial => ResourceManager.GetString("ERR_MissingPartial", resourceCulture);

	internal static string ERR_MissingPPFile => ResourceManager.GetString("ERR_MissingPPFile", resourceCulture);

	internal static string ERR_MissingPredefinedMember => ResourceManager.GetString("ERR_MissingPredefinedMember", resourceCulture);

	internal static string ERR_MissingSourceInterface => ResourceManager.GetString("ERR_MissingSourceInterface", resourceCulture);

	internal static string ERR_MissingStructOffset => ResourceManager.GetString("ERR_MissingStructOffset", resourceCulture);

	internal static string ERR_MissingTypeInAssembly => ResourceManager.GetString("ERR_MissingTypeInAssembly", resourceCulture);

	internal static string ERR_MissingTypeInSource => ResourceManager.GetString("ERR_MissingTypeInSource", resourceCulture);

	internal static string ERR_MixedDeconstructionUnsupported => ResourceManager.GetString("ERR_MixedDeconstructionUnsupported", resourceCulture);

	internal static string ERR_MixingWinRTEventWithRegular => ResourceManager.GetString("ERR_MixingWinRTEventWithRegular", resourceCulture);

	internal static string ERR_ModuleEmitFailure => ResourceManager.GetString("ERR_ModuleEmitFailure", resourceCulture);

	internal static string ERR_MultipleEntryPoints => ResourceManager.GetString("ERR_MultipleEntryPoints", resourceCulture);

	internal static string ERR_MultipleIEnumOfT => ResourceManager.GetString("ERR_MultipleIEnumOfT", resourceCulture);

	internal static string ERR_MultiTypeInDeclaration => ResourceManager.GetString("ERR_MultiTypeInDeclaration", resourceCulture);

	internal static string ERR_MustDeclareForeachIteration => ResourceManager.GetString("ERR_MustDeclareForeachIteration", resourceCulture);

	internal static string ERR_MustHaveOpTF => ResourceManager.GetString("ERR_MustHaveOpTF", resourceCulture);

	internal static string ERR_MustHaveRefReturn => ResourceManager.GetString("ERR_MustHaveRefReturn", resourceCulture);

	internal static string ERR_MustNotHaveRefReturn => ResourceManager.GetString("ERR_MustNotHaveRefReturn", resourceCulture);

	internal static string ERR_MutuallyExclusiveOptions => ResourceManager.GetString("ERR_MutuallyExclusiveOptions", resourceCulture);

	internal static string ERR_NamedArgumentExpected => ResourceManager.GetString("ERR_NamedArgumentExpected", resourceCulture);

	internal static string ERR_NamedArgumentForArray => ResourceManager.GetString("ERR_NamedArgumentForArray", resourceCulture);

	internal static string ERR_NamedArgumentSpecificationBeforeFixedArgument => ResourceManager.GetString("ERR_NamedArgumentSpecificationBeforeFixedArgument", resourceCulture);

	internal static string ERR_NamedArgumentSpecificationBeforeFixedArgumentInDynamicInvocation => ResourceManager.GetString("ERR_NamedArgumentSpecificationBeforeFixedArgumentInDynamicInvocation", resourceCulture);

	internal static string ERR_NamedArgumentUsedInPositional => ResourceManager.GetString("ERR_NamedArgumentUsedInPositional", resourceCulture);

	internal static string ERR_NameNotInContext => ResourceManager.GetString("ERR_NameNotInContext", resourceCulture);

	internal static string ERR_NameNotInContextPossibleMissingReference => ResourceManager.GetString("ERR_NameNotInContextPossibleMissingReference", resourceCulture);

	internal static string ERR_NameofExtensionMethod => ResourceManager.GetString("ERR_NameofExtensionMethod", resourceCulture);

	internal static string ERR_NameofMethodGroupWithTypeParameters => ResourceManager.GetString("ERR_NameofMethodGroupWithTypeParameters", resourceCulture);

	internal static string ERR_NamespaceNotAllowedInScript => ResourceManager.GetString("ERR_NamespaceNotAllowedInScript", resourceCulture);

	internal static string ERR_NamespaceUnexpected => ResourceManager.GetString("ERR_NamespaceUnexpected", resourceCulture);

	internal static string ERR_NegativeArraySize => ResourceManager.GetString("ERR_NegativeArraySize", resourceCulture);

	internal static string ERR_NegativeStackAllocSize => ResourceManager.GetString("ERR_NegativeStackAllocSize", resourceCulture);

	internal static string ERR_NetModuleNameMismatch => ResourceManager.GetString("ERR_NetModuleNameMismatch", resourceCulture);

	internal static string ERR_NetModuleNameMustBeUnique => ResourceManager.GetString("ERR_NetModuleNameMustBeUnique", resourceCulture);

	internal static string ERR_NewBoundMustBeLast => ResourceManager.GetString("ERR_NewBoundMustBeLast", resourceCulture);

	internal static string ERR_NewBoundWithUnmanaged => ResourceManager.GetString("ERR_NewBoundWithUnmanaged", resourceCulture);

	internal static string ERR_NewBoundWithVal => ResourceManager.GetString("ERR_NewBoundWithVal", resourceCulture);

	internal static string ERR_NewCoClassOnLink => ResourceManager.GetString("ERR_NewCoClassOnLink", resourceCulture);

	internal static string ERR_NewConstraintNotSatisfied => ResourceManager.GetString("ERR_NewConstraintNotSatisfied", resourceCulture);

	internal static string ERR_NewlineInConst => ResourceManager.GetString("ERR_NewlineInConst", resourceCulture);

	internal static string ERR_NewTyvarWithArgs => ResourceManager.GetString("ERR_NewTyvarWithArgs", resourceCulture);

	internal static string ERR_NewVirtualInSealed => ResourceManager.GetString("ERR_NewVirtualInSealed", resourceCulture);

	internal static string ERR_NewWithTupleTypeSyntax => ResourceManager.GetString("ERR_NewWithTupleTypeSyntax", resourceCulture);

	internal static string ERR_NoAliasHere => ResourceManager.GetString("ERR_NoAliasHere", resourceCulture);

	internal static string ERR_NoBaseClass => ResourceManager.GetString("ERR_NoBaseClass", resourceCulture);

	internal static string ERR_NoBreakOrCont => ResourceManager.GetString("ERR_NoBreakOrCont", resourceCulture);

	internal static string ERR_NoCanonicalView => ResourceManager.GetString("ERR_NoCanonicalView", resourceCulture);

	internal static string ERR_NoConstructors => ResourceManager.GetString("ERR_NoConstructors", resourceCulture);

	internal static string ERR_NoConversionForCallerFilePathParam => ResourceManager.GetString("ERR_NoConversionForCallerFilePathParam", resourceCulture);

	internal static string ERR_NoConversionForCallerLineNumberParam => ResourceManager.GetString("ERR_NoConversionForCallerLineNumberParam", resourceCulture);

	internal static string ERR_NoConversionForCallerMemberNameParam => ResourceManager.GetString("ERR_NoConversionForCallerMemberNameParam", resourceCulture);

	internal static string ERR_NoConversionForDefaultParam => ResourceManager.GetString("ERR_NoConversionForDefaultParam", resourceCulture);

	internal static string ERR_NoConversionForNubDefaultParam => ResourceManager.GetString("ERR_NoConversionForNubDefaultParam", resourceCulture);

	internal static string ERR_NoConvToIDisp => ResourceManager.GetString("ERR_NoConvToIDisp", resourceCulture);

	internal static string ERR_NoCorrespondingArgument => ResourceManager.GetString("ERR_NoCorrespondingArgument", resourceCulture);

	internal static string ERR_NoDynamicPhantomOnBase => ResourceManager.GetString("ERR_NoDynamicPhantomOnBase", resourceCulture);

	internal static string ERR_NoDynamicPhantomOnBaseCtor => ResourceManager.GetString("ERR_NoDynamicPhantomOnBaseCtor", resourceCulture);

	internal static string ERR_NoDynamicPhantomOnBaseIndexer => ResourceManager.GetString("ERR_NoDynamicPhantomOnBaseIndexer", resourceCulture);

	internal static string ERR_NoEntryPoint => ResourceManager.GetString("ERR_NoEntryPoint", resourceCulture);

	internal static string ERR_NoExplicitBuiltinConv => ResourceManager.GetString("ERR_NoExplicitBuiltinConv", resourceCulture);

	internal static string ERR_NoExplicitConv => ResourceManager.GetString("ERR_NoExplicitConv", resourceCulture);

	internal static string ERR_NoFileSpec => ResourceManager.GetString("ERR_NoFileSpec", resourceCulture);

	internal static string ERR_NoGetToOverride => ResourceManager.GetString("ERR_NoGetToOverride", resourceCulture);

	internal static string ERR_NoImplicitConv => ResourceManager.GetString("ERR_NoImplicitConv", resourceCulture);

	internal static string ERR_NoImplicitConvCast => ResourceManager.GetString("ERR_NoImplicitConvCast", resourceCulture);

	internal static string ERR_NoMainInClass => ResourceManager.GetString("ERR_NoMainInClass", resourceCulture);

	internal static string ERR_NoMainOnDLL => ResourceManager.GetString("ERR_NoMainOnDLL", resourceCulture);

	internal static string ERR_NoMetadataFile => ResourceManager.GetString("ERR_NoMetadataFile", resourceCulture);

	internal static string ERR_NoModifiersOnAccessor => ResourceManager.GetString("ERR_NoModifiersOnAccessor", resourceCulture);

	internal static string ERR_NoMultipleInheritance => ResourceManager.GetString("ERR_NoMultipleInheritance", resourceCulture);

	internal static string ERR_NoNamespacePrivate => ResourceManager.GetString("ERR_NoNamespacePrivate", resourceCulture);

	internal static string ERR_NoNetModuleOutputWhenRefOutOrRefOnly => ResourceManager.GetString("ERR_NoNetModuleOutputWhenRefOutOrRefOnly", resourceCulture);

	internal static string ERR_NoNewAbstract => ResourceManager.GetString("ERR_NoNewAbstract", resourceCulture);

	internal static string ERR_NoNewTyvar => ResourceManager.GetString("ERR_NoNewTyvar", resourceCulture);

	internal static string ERR_NonInterfaceInInterfaceList => ResourceManager.GetString("ERR_NonInterfaceInInterfaceList", resourceCulture);

	internal static string ERR_NonInvocableMemberCalled => ResourceManager.GetString("ERR_NonInvocableMemberCalled", resourceCulture);

	internal static string ERR_NonTaskMainCantBeAsync => ResourceManager.GetString("ERR_NonTaskMainCantBeAsync", resourceCulture);

	internal static string ERR_NoPIAAssemblyMissingAttribute => ResourceManager.GetString("ERR_NoPIAAssemblyMissingAttribute", resourceCulture);

	internal static string ERR_NoPIAAssemblyMissingAttributes => ResourceManager.GetString("ERR_NoPIAAssemblyMissingAttributes", resourceCulture);

	internal static string ERR_NoPIANestedType => ResourceManager.GetString("ERR_NoPIANestedType", resourceCulture);

	internal static string ERR_NoRefOutWhenRefOnly => ResourceManager.GetString("ERR_NoRefOutWhenRefOnly", resourceCulture);

	internal static string ERR_NoSetToOverride => ResourceManager.GetString("ERR_NoSetToOverride", resourceCulture);

	internal static string ERR_NoSourceFile => ResourceManager.GetString("ERR_NoSourceFile", resourceCulture);

	internal static string ERR_NoSuchMember => ResourceManager.GetString("ERR_NoSuchMember", resourceCulture);

	internal static string ERR_NoSuchMemberOrExtension => ResourceManager.GetString("ERR_NoSuchMemberOrExtension", resourceCulture);

	internal static string ERR_NoSuchMemberOrExtensionNeedUsing => ResourceManager.GetString("ERR_NoSuchMemberOrExtensionNeedUsing", resourceCulture);

	internal static string ERR_NotAnAttributeClass => ResourceManager.GetString("ERR_NotAnAttributeClass", resourceCulture);

	internal static string ERR_NotConstantExpression => ResourceManager.GetString("ERR_NotConstantExpression", resourceCulture);

	internal static string ERR_NotNullConstRefField => ResourceManager.GetString("ERR_NotNullConstRefField", resourceCulture);

	internal static string ERR_NotNullRefDefaultParameter => ResourceManager.GetString("ERR_NotNullRefDefaultParameter", resourceCulture);

	internal static string ERR_NotYetImplementedInRoslyn => ResourceManager.GetString("ERR_NotYetImplementedInRoslyn", resourceCulture);

	internal static string ERR_NoTypeDef => ResourceManager.GetString("ERR_NoTypeDef", resourceCulture);

	internal static string ERR_NoTypeDefFromModule => ResourceManager.GetString("ERR_NoTypeDefFromModule", resourceCulture);

	internal static string ERR_NoVoidHere => ResourceManager.GetString("ERR_NoVoidHere", resourceCulture);

	internal static string ERR_NoVoidParameter => ResourceManager.GetString("ERR_NoVoidParameter", resourceCulture);

	internal static string ERR_NullNotValid => ResourceManager.GetString("ERR_NullNotValid", resourceCulture);

	internal static string ERR_NullPropagatingOpInExpressionTree => ResourceManager.GetString("ERR_NullPropagatingOpInExpressionTree", resourceCulture);

	internal static string ERR_ObjectCallingBaseConstructor => ResourceManager.GetString("ERR_ObjectCallingBaseConstructor", resourceCulture);

	internal static string ERR_ObjectCantHaveBases => ResourceManager.GetString("ERR_ObjectCantHaveBases", resourceCulture);

	internal static string ERR_ObjectOrCollectionInitializerWithDelegateCreation => ResourceManager.GetString("ERR_ObjectOrCollectionInitializerWithDelegateCreation", resourceCulture);

	internal static string ERR_ObjectProhibited => ResourceManager.GetString("ERR_ObjectProhibited", resourceCulture);

	internal static string ERR_ObjectRequired => ResourceManager.GetString("ERR_ObjectRequired", resourceCulture);

	internal static string ERR_OneAliasPerReference => ResourceManager.GetString("ERR_OneAliasPerReference", resourceCulture);

	internal static string ERR_OnlyClassesCanContainDestructors => ResourceManager.GetString("ERR_OnlyClassesCanContainDestructors", resourceCulture);

	internal static string ERR_OpenEndedComment => ResourceManager.GetString("ERR_OpenEndedComment", resourceCulture);

	internal static string ERR_OpenResponseFile => ResourceManager.GetString("ERR_OpenResponseFile", resourceCulture);

	internal static string ERR_OperatorCantReturnVoid => ResourceManager.GetString("ERR_OperatorCantReturnVoid", resourceCulture);

	internal static string ERR_OperatorInStaticClass => ResourceManager.GetString("ERR_OperatorInStaticClass", resourceCulture);

	internal static string ERR_OperatorNeedsMatch => ResourceManager.GetString("ERR_OperatorNeedsMatch", resourceCulture);

	internal static string ERR_OperatorsMustBeStatic => ResourceManager.GetString("ERR_OperatorsMustBeStatic", resourceCulture);

	internal static string ERR_OpTFRetType => ResourceManager.GetString("ERR_OpTFRetType", resourceCulture);

	internal static string ERR_OptionMustBeAbsolutePath => ResourceManager.GetString("ERR_OptionMustBeAbsolutePath", resourceCulture);

	internal static string ERR_OutAttrOnInParam => ResourceManager.GetString("ERR_OutAttrOnInParam", resourceCulture);

	internal static string ERR_OutAttrOnRefParam => ResourceManager.GetString("ERR_OutAttrOnRefParam", resourceCulture);

	internal static string ERR_OutputNeedsName => ResourceManager.GetString("ERR_OutputNeedsName", resourceCulture);

	internal static string ERR_OutputWriteFailed => ResourceManager.GetString("ERR_OutputWriteFailed", resourceCulture);

	internal static string ERR_OutVariableCannotBeByRef => ResourceManager.GetString("ERR_OutVariableCannotBeByRef", resourceCulture);

	internal static string ERR_OverloadRefKind => ResourceManager.GetString("ERR_OverloadRefKind", resourceCulture);

	internal static string ERR_OverrideFinalizeDeprecated => ResourceManager.GetString("ERR_OverrideFinalizeDeprecated", resourceCulture);

	internal static string ERR_OverrideNotExpected => ResourceManager.GetString("ERR_OverrideNotExpected", resourceCulture);

	internal static string ERR_OverrideNotNew => ResourceManager.GetString("ERR_OverrideNotNew", resourceCulture);

	internal static string ERR_OverrideWithConstraints => ResourceManager.GetString("ERR_OverrideWithConstraints", resourceCulture);

	internal static string ERR_OvlBinaryOperatorExpected => ResourceManager.GetString("ERR_OvlBinaryOperatorExpected", resourceCulture);

	internal static string ERR_OvlOperatorExpected => ResourceManager.GetString("ERR_OvlOperatorExpected", resourceCulture);

	internal static string ERR_OvlUnaryOperatorExpected => ResourceManager.GetString("ERR_OvlUnaryOperatorExpected", resourceCulture);

	internal static string ERR_ParamDefaultValueDiffersFromAttribute => ResourceManager.GetString("ERR_ParamDefaultValueDiffersFromAttribute", resourceCulture);

	internal static string ERR_ParameterIsStaticClass => ResourceManager.GetString("ERR_ParameterIsStaticClass", resourceCulture);

	internal static string ERR_ParameterNotValidForType => ResourceManager.GetString("ERR_ParameterNotValidForType", resourceCulture);

	internal static string ERR_ParamsCantBeWithModifier => ResourceManager.GetString("ERR_ParamsCantBeWithModifier", resourceCulture);

	internal static string ERR_ParamsLast => ResourceManager.GetString("ERR_ParamsLast", resourceCulture);

	internal static string ERR_ParamsMustBeArray => ResourceManager.GetString("ERR_ParamsMustBeArray", resourceCulture);

	internal static string ERR_ParamUnassigned => ResourceManager.GetString("ERR_ParamUnassigned", resourceCulture);

	internal static string ERR_PartialMethodCannotHaveOutParameters => ResourceManager.GetString("ERR_PartialMethodCannotHaveOutParameters", resourceCulture);

	internal static string ERR_PartialMethodExtensionDifference => ResourceManager.GetString("ERR_PartialMethodExtensionDifference", resourceCulture);

	internal static string ERR_PartialMethodInconsistentConstraints => ResourceManager.GetString("ERR_PartialMethodInconsistentConstraints", resourceCulture);

	internal static string ERR_PartialMethodInconsistentTupleNames => ResourceManager.GetString("ERR_PartialMethodInconsistentTupleNames", resourceCulture);

	internal static string ERR_PartialMethodInExpressionTree => ResourceManager.GetString("ERR_PartialMethodInExpressionTree", resourceCulture);

	internal static string ERR_PartialMethodInvalidModifier => ResourceManager.GetString("ERR_PartialMethodInvalidModifier", resourceCulture);

	internal static string ERR_PartialMethodMustHaveLatent => ResourceManager.GetString("ERR_PartialMethodMustHaveLatent", resourceCulture);

	internal static string ERR_PartialMethodMustReturnVoid => ResourceManager.GetString("ERR_PartialMethodMustReturnVoid", resourceCulture);

	internal static string ERR_PartialMethodNotExplicit => ResourceManager.GetString("ERR_PartialMethodNotExplicit", resourceCulture);

	internal static string ERR_PartialMethodOnlyInPartialClass => ResourceManager.GetString("ERR_PartialMethodOnlyInPartialClass", resourceCulture);

	internal static string ERR_PartialMethodOnlyOneActual => ResourceManager.GetString("ERR_PartialMethodOnlyOneActual", resourceCulture);

	internal static string ERR_PartialMethodOnlyOneLatent => ResourceManager.GetString("ERR_PartialMethodOnlyOneLatent", resourceCulture);

	internal static string ERR_PartialMethodParamsDifference => ResourceManager.GetString("ERR_PartialMethodParamsDifference", resourceCulture);

	internal static string ERR_PartialMethodStaticDifference => ResourceManager.GetString("ERR_PartialMethodStaticDifference", resourceCulture);

	internal static string ERR_PartialMethodToDelegate => ResourceManager.GetString("ERR_PartialMethodToDelegate", resourceCulture);

	internal static string ERR_PartialMethodUnsafeDifference => ResourceManager.GetString("ERR_PartialMethodUnsafeDifference", resourceCulture);

	internal static string ERR_PartialMisplaced => ResourceManager.GetString("ERR_PartialMisplaced", resourceCulture);

	internal static string ERR_PartialModifierConflict => ResourceManager.GetString("ERR_PartialModifierConflict", resourceCulture);

	internal static string ERR_PartialMultipleBases => ResourceManager.GetString("ERR_PartialMultipleBases", resourceCulture);

	internal static string ERR_PartialTypeKindConflict => ResourceManager.GetString("ERR_PartialTypeKindConflict", resourceCulture);

	internal static string ERR_PartialWrongConstraints => ResourceManager.GetString("ERR_PartialWrongConstraints", resourceCulture);

	internal static string ERR_PartialWrongTypeParams => ResourceManager.GetString("ERR_PartialWrongTypeParams", resourceCulture);

	internal static string ERR_PartialWrongTypeParamsVariance => ResourceManager.GetString("ERR_PartialWrongTypeParamsVariance", resourceCulture);

	internal static string ERR_PatternDynamicType => ResourceManager.GetString("ERR_PatternDynamicType", resourceCulture);

	internal static string ERR_PatternIsSubsumed => ResourceManager.GetString("ERR_PatternIsSubsumed", resourceCulture);

	internal static string ERR_PatternNullableType => ResourceManager.GetString("ERR_PatternNullableType", resourceCulture);

	internal static string ERR_PatternWrongGenericTypeInVersion => ResourceManager.GetString("ERR_PatternWrongGenericTypeInVersion", resourceCulture);

	internal static string ERR_PatternWrongType => ResourceManager.GetString("ERR_PatternWrongType", resourceCulture);

	internal static string ERR_PermissionSetAttributeFileReadError => ResourceManager.GetString("ERR_PermissionSetAttributeFileReadError", resourceCulture);

	internal static string ERR_PermissionSetAttributeInvalidFile => ResourceManager.GetString("ERR_PermissionSetAttributeInvalidFile", resourceCulture);

	internal static string ERR_PeWritingFailure => ResourceManager.GetString("ERR_PeWritingFailure", resourceCulture);

	internal static string ERR_PointerInAsOrIs => ResourceManager.GetString("ERR_PointerInAsOrIs", resourceCulture);

	internal static string ERR_PossibleBadNegCast => ResourceManager.GetString("ERR_PossibleBadNegCast", resourceCulture);

	internal static string ERR_PPDefFollowsToken => ResourceManager.GetString("ERR_PPDefFollowsToken", resourceCulture);

	internal static string ERR_PPDirectiveExpected => ResourceManager.GetString("ERR_PPDirectiveExpected", resourceCulture);

	internal static string ERR_PPLoadFollowsToken => ResourceManager.GetString("ERR_PPLoadFollowsToken", resourceCulture);

	internal static string ERR_PPReferenceFollowsToken => ResourceManager.GetString("ERR_PPReferenceFollowsToken", resourceCulture);

	internal static string ERR_PredefinedTypeMemberNotFoundInAssembly => ResourceManager.GetString("ERR_PredefinedTypeMemberNotFoundInAssembly", resourceCulture);

	internal static string ERR_PredefinedTypeNotFound => ResourceManager.GetString("ERR_PredefinedTypeNotFound", resourceCulture);

	internal static string ERR_PredefinedValueTupleTypeAmbiguous3 => ResourceManager.GetString("ERR_PredefinedValueTupleTypeAmbiguous3", resourceCulture);

	internal static string ERR_PredefinedValueTupleTypeMustBeStruct => ResourceManager.GetString("ERR_PredefinedValueTupleTypeMustBeStruct", resourceCulture);

	internal static string ERR_PredefinedValueTupleTypeNotFound => ResourceManager.GetString("ERR_PredefinedValueTupleTypeNotFound", resourceCulture);

	internal static string ERR_PrincipalPermissionInvalidAction => ResourceManager.GetString("ERR_PrincipalPermissionInvalidAction", resourceCulture);

	internal static string ERR_PrivateAbstractAccessor => ResourceManager.GetString("ERR_PrivateAbstractAccessor", resourceCulture);

	internal static string ERR_PropertyAccessModInInterface => ResourceManager.GetString("ERR_PropertyAccessModInInterface", resourceCulture);

	internal static string ERR_PropertyCantHaveVoidType => ResourceManager.GetString("ERR_PropertyCantHaveVoidType", resourceCulture);

	internal static string ERR_PropertyLacksGet => ResourceManager.GetString("ERR_PropertyLacksGet", resourceCulture);

	internal static string ERR_PropertyWithNoAccessors => ResourceManager.GetString("ERR_PropertyWithNoAccessors", resourceCulture);

	internal static string ERR_ProtectedInStatic => ResourceManager.GetString("ERR_ProtectedInStatic", resourceCulture);

	internal static string ERR_ProtectedInStruct => ResourceManager.GetString("ERR_ProtectedInStruct", resourceCulture);

	internal static string ERR_PtrExpected => ResourceManager.GetString("ERR_PtrExpected", resourceCulture);

	internal static string ERR_PtrIndexSingle => ResourceManager.GetString("ERR_PtrIndexSingle", resourceCulture);

	internal static string ERR_PublicKeyContainerFailure => ResourceManager.GetString("ERR_PublicKeyContainerFailure", resourceCulture);

	internal static string ERR_PublicKeyFileFailure => ResourceManager.GetString("ERR_PublicKeyFileFailure", resourceCulture);

	internal static string ERR_PublicSignButNoKey => ResourceManager.GetString("ERR_PublicSignButNoKey", resourceCulture);

	internal static string ERR_PublicSignNetModule => ResourceManager.GetString("ERR_PublicSignNetModule", resourceCulture);

	internal static string ERR_QueryDuplicateRangeVariable => ResourceManager.GetString("ERR_QueryDuplicateRangeVariable", resourceCulture);

	internal static string ERR_QueryInnerKey => ResourceManager.GetString("ERR_QueryInnerKey", resourceCulture);

	internal static string ERR_QueryMultipleProviders => ResourceManager.GetString("ERR_QueryMultipleProviders", resourceCulture);

	internal static string ERR_QueryNoProvider => ResourceManager.GetString("ERR_QueryNoProvider", resourceCulture);

	internal static string ERR_QueryNoProviderCastable => ResourceManager.GetString("ERR_QueryNoProviderCastable", resourceCulture);

	internal static string ERR_QueryNoProviderStandard => ResourceManager.GetString("ERR_QueryNoProviderStandard", resourceCulture);

	internal static string ERR_QueryOuterKey => ResourceManager.GetString("ERR_QueryOuterKey", resourceCulture);

	internal static string ERR_QueryOutRefRangeVariable => ResourceManager.GetString("ERR_QueryOutRefRangeVariable", resourceCulture);

	internal static string ERR_QueryRangeVariableAssignedBadValue => ResourceManager.GetString("ERR_QueryRangeVariableAssignedBadValue", resourceCulture);

	internal static string ERR_QueryRangeVariableOverrides => ResourceManager.GetString("ERR_QueryRangeVariableOverrides", resourceCulture);

	internal static string ERR_QueryRangeVariableReadOnly => ResourceManager.GetString("ERR_QueryRangeVariableReadOnly", resourceCulture);

	internal static string ERR_QueryRangeVariableSameAsTypeParam => ResourceManager.GetString("ERR_QueryRangeVariableSameAsTypeParam", resourceCulture);

	internal static string ERR_QueryTypeInferenceFailed => ResourceManager.GetString("ERR_QueryTypeInferenceFailed", resourceCulture);

	internal static string ERR_QueryTypeInferenceFailedMulti => ResourceManager.GetString("ERR_QueryTypeInferenceFailedMulti", resourceCulture);

	internal static string ERR_QueryTypeInferenceFailedSelectMany => ResourceManager.GetString("ERR_QueryTypeInferenceFailedSelectMany", resourceCulture);

	internal static string ERR_RbraceExpected => ResourceManager.GetString("ERR_RbraceExpected", resourceCulture);

	internal static string ERR_ReadonlyValueTypeInObjectInitializer => ResourceManager.GetString("ERR_ReadonlyValueTypeInObjectInitializer", resourceCulture);

	internal static string ERR_RecursiveConstructorCall => ResourceManager.GetString("ERR_RecursiveConstructorCall", resourceCulture);

	internal static string ERR_RecursivelyTypedVariable => ResourceManager.GetString("ERR_RecursivelyTypedVariable", resourceCulture);

	internal static string ERR_RefAssignmentMustHaveIdentityConversion => ResourceManager.GetString("ERR_RefAssignmentMustHaveIdentityConversion", resourceCulture);

	internal static string ERR_RefAssignNarrower => ResourceManager.GetString("ERR_RefAssignNarrower", resourceCulture);

	internal static string ERR_RefConditionalAndAwait => ResourceManager.GetString("ERR_RefConditionalAndAwait", resourceCulture);

	internal static string ERR_RefConditionalDifferentTypes => ResourceManager.GetString("ERR_RefConditionalDifferentTypes", resourceCulture);

	internal static string ERR_RefConditionalNeedsTwoRefs => ResourceManager.GetString("ERR_RefConditionalNeedsTwoRefs", resourceCulture);

	internal static string ERR_RefConstraintNotSatisfied => ResourceManager.GetString("ERR_RefConstraintNotSatisfied", resourceCulture);

	internal static string ERR_ReferenceDirectiveOnlyAllowedInScripts => ResourceManager.GetString("ERR_ReferenceDirectiveOnlyAllowedInScripts", resourceCulture);

	internal static string ERR_RefExtensionMustBeValueTypeOrConstrainedToOne => ResourceManager.GetString("ERR_RefExtensionMustBeValueTypeOrConstrainedToOne", resourceCulture);

	internal static string ERR_RefLocalOrParamExpected => ResourceManager.GetString("ERR_RefLocalOrParamExpected", resourceCulture);

	internal static string ERR_RefLvalueExpected => ResourceManager.GetString("ERR_RefLvalueExpected", resourceCulture);

	internal static string ERR_RefOutDefaultValue => ResourceManager.GetString("ERR_RefOutDefaultValue", resourceCulture);

	internal static string ERR_RefProperty => ResourceManager.GetString("ERR_RefProperty", resourceCulture);

	internal static string ERR_RefPropertyCannotHaveSetAccessor => ResourceManager.GetString("ERR_RefPropertyCannotHaveSetAccessor", resourceCulture);

	internal static string ERR_RefPropertyMustHaveGetAccessor => ResourceManager.GetString("ERR_RefPropertyMustHaveGetAccessor", resourceCulture);

	internal static string ERR_RefReadonly => ResourceManager.GetString("ERR_RefReadonly", resourceCulture);

	internal static string ERR_RefReadonly2 => ResourceManager.GetString("ERR_RefReadonly2", resourceCulture);

	internal static string ERR_RefReadonlyLocal => ResourceManager.GetString("ERR_RefReadonlyLocal", resourceCulture);

	internal static string ERR_RefReadonlyLocal2Cause => ResourceManager.GetString("ERR_RefReadonlyLocal2Cause", resourceCulture);

	internal static string ERR_RefReadonlyLocalCause => ResourceManager.GetString("ERR_RefReadonlyLocalCause", resourceCulture);

	internal static string ERR_RefReadonlyNotField => ResourceManager.GetString("ERR_RefReadonlyNotField", resourceCulture);

	internal static string ERR_RefReadonlyNotField2 => ResourceManager.GetString("ERR_RefReadonlyNotField2", resourceCulture);

	internal static string ERR_RefReadonlyStatic => ResourceManager.GetString("ERR_RefReadonlyStatic", resourceCulture);

	internal static string ERR_RefReadonlyStatic2 => ResourceManager.GetString("ERR_RefReadonlyStatic2", resourceCulture);

	internal static string ERR_RefReturningCallAndAwait => ResourceManager.GetString("ERR_RefReturningCallAndAwait", resourceCulture);

	internal static string ERR_RefReturningCallInExpressionTree => ResourceManager.GetString("ERR_RefReturningCallInExpressionTree", resourceCulture);

	internal static string ERR_RefReturnLocal => ResourceManager.GetString("ERR_RefReturnLocal", resourceCulture);

	internal static string ERR_RefReturnLocal2 => ResourceManager.GetString("ERR_RefReturnLocal2", resourceCulture);

	internal static string ERR_RefReturnLvalueExpected => ResourceManager.GetString("ERR_RefReturnLvalueExpected", resourceCulture);

	internal static string ERR_RefReturnMustHaveIdentityConversion => ResourceManager.GetString("ERR_RefReturnMustHaveIdentityConversion", resourceCulture);

	internal static string ERR_RefReturnNonreturnableLocal => ResourceManager.GetString("ERR_RefReturnNonreturnableLocal", resourceCulture);

	internal static string ERR_RefReturnNonreturnableLocal2 => ResourceManager.GetString("ERR_RefReturnNonreturnableLocal2", resourceCulture);

	internal static string ERR_RefReturnParameter => ResourceManager.GetString("ERR_RefReturnParameter", resourceCulture);

	internal static string ERR_RefReturnParameter2 => ResourceManager.GetString("ERR_RefReturnParameter2", resourceCulture);

	internal static string ERR_RefReturnRangeVariable => ResourceManager.GetString("ERR_RefReturnRangeVariable", resourceCulture);

	internal static string ERR_RefReturnReadonly => ResourceManager.GetString("ERR_RefReturnReadonly", resourceCulture);

	internal static string ERR_RefReturnReadonly2 => ResourceManager.GetString("ERR_RefReturnReadonly2", resourceCulture);

	internal static string ERR_RefReturnReadonlyLocal => ResourceManager.GetString("ERR_RefReturnReadonlyLocal", resourceCulture);

	internal static string ERR_RefReturnReadonlyLocal2Cause => ResourceManager.GetString("ERR_RefReturnReadonlyLocal2Cause", resourceCulture);

	internal static string ERR_RefReturnReadonlyLocalCause => ResourceManager.GetString("ERR_RefReturnReadonlyLocalCause", resourceCulture);

	internal static string ERR_RefReturnReadonlyNotField => ResourceManager.GetString("ERR_RefReturnReadonlyNotField", resourceCulture);

	internal static string ERR_RefReturnReadonlyNotField2 => ResourceManager.GetString("ERR_RefReturnReadonlyNotField2", resourceCulture);

	internal static string ERR_RefReturnReadonlyStatic => ResourceManager.GetString("ERR_RefReturnReadonlyStatic", resourceCulture);

	internal static string ERR_RefReturnReadonlyStatic2 => ResourceManager.GetString("ERR_RefReturnReadonlyStatic2", resourceCulture);

	internal static string ERR_RefReturnStructThis => ResourceManager.GetString("ERR_RefReturnStructThis", resourceCulture);

	internal static string ERR_RefReturnThis => ResourceManager.GetString("ERR_RefReturnThis", resourceCulture);

	internal static string ERR_RefStructInterfaceImpl => ResourceManager.GetString("ERR_RefStructInterfaceImpl", resourceCulture);

	internal static string ERR_RefValBoundMustBeFirst => ResourceManager.GetString("ERR_RefValBoundMustBeFirst", resourceCulture);

	internal static string ERR_RefValBoundWithClass => ResourceManager.GetString("ERR_RefValBoundWithClass", resourceCulture);

	internal static string ERR_ReservedAssemblyName => ResourceManager.GetString("ERR_ReservedAssemblyName", resourceCulture);

	internal static string ERR_ReservedEnumerator => ResourceManager.GetString("ERR_ReservedEnumerator", resourceCulture);

	internal static string ERR_ResourceFileNameNotUnique => ResourceManager.GetString("ERR_ResourceFileNameNotUnique", resourceCulture);

	internal static string ERR_ResourceNotUnique => ResourceManager.GetString("ERR_ResourceNotUnique", resourceCulture);

	internal static string ERR_RetNoObjectRequired => ResourceManager.GetString("ERR_RetNoObjectRequired", resourceCulture);

	internal static string ERR_RetNoObjectRequiredLambda => ResourceManager.GetString("ERR_RetNoObjectRequiredLambda", resourceCulture);

	internal static string ERR_RetObjectRequired => ResourceManager.GetString("ERR_RetObjectRequired", resourceCulture);

	internal static string ERR_ReturnExpected => ResourceManager.GetString("ERR_ReturnExpected", resourceCulture);

	internal static string ERR_ReturnInIterator => ResourceManager.GetString("ERR_ReturnInIterator", resourceCulture);

	internal static string ERR_ReturnNotLValue => ResourceManager.GetString("ERR_ReturnNotLValue", resourceCulture);

	internal static string ERR_ReturnTypeIsStaticClass => ResourceManager.GetString("ERR_ReturnTypeIsStaticClass", resourceCulture);

	internal static string ERR_SameFullNameAggAgg => ResourceManager.GetString("ERR_SameFullNameAggAgg", resourceCulture);

	internal static string ERR_SameFullNameNsAgg => ResourceManager.GetString("ERR_SameFullNameNsAgg", resourceCulture);

	internal static string ERR_SameFullNameThisAggThisNs => ResourceManager.GetString("ERR_SameFullNameThisAggThisNs", resourceCulture);

	internal static string ERR_SealedNonOverride => ResourceManager.GetString("ERR_SealedNonOverride", resourceCulture);

	internal static string ERR_SealedStaticClass => ResourceManager.GetString("ERR_SealedStaticClass", resourceCulture);

	internal static string ERR_SecurityAttributeInvalidAction => ResourceManager.GetString("ERR_SecurityAttributeInvalidAction", resourceCulture);

	internal static string ERR_SecurityAttributeInvalidActionAssembly => ResourceManager.GetString("ERR_SecurityAttributeInvalidActionAssembly", resourceCulture);

	internal static string ERR_SecurityAttributeInvalidActionTypeOrMethod => ResourceManager.GetString("ERR_SecurityAttributeInvalidActionTypeOrMethod", resourceCulture);

	internal static string ERR_SecurityAttributeInvalidTarget => ResourceManager.GetString("ERR_SecurityAttributeInvalidTarget", resourceCulture);

	internal static string ERR_SecurityAttributeMissingAction => ResourceManager.GetString("ERR_SecurityAttributeMissingAction", resourceCulture);

	internal static string ERR_SecurityCriticalOrSecuritySafeCriticalOnAsync => ResourceManager.GetString("ERR_SecurityCriticalOrSecuritySafeCriticalOnAsync", resourceCulture);

	internal static string ERR_SecurityCriticalOrSecuritySafeCriticalOnAsyncInClassOrStruct => ResourceManager.GetString("ERR_SecurityCriticalOrSecuritySafeCriticalOnAsyncInClassOrStruct", resourceCulture);

	internal static string ERR_SemicolonExpected => ResourceManager.GetString("ERR_SemicolonExpected", resourceCulture);

	internal static string ERR_SemiOrLBraceExpected => ResourceManager.GetString("ERR_SemiOrLBraceExpected", resourceCulture);

	internal static string ERR_SemiOrLBraceOrArrowExpected => ResourceManager.GetString("ERR_SemiOrLBraceOrArrowExpected", resourceCulture);

	internal static string ERR_SignButNoPrivateKey => ResourceManager.GetString("ERR_SignButNoPrivateKey", resourceCulture);

	internal static string ERR_SingleLineCommentInExpressionHole => ResourceManager.GetString("ERR_SingleLineCommentInExpressionHole", resourceCulture);

	internal static string ERR_SingleTypeNameNotFound => ResourceManager.GetString("ERR_SingleTypeNameNotFound", resourceCulture);

	internal static string ERR_SingleTypeNameNotFoundFwd => ResourceManager.GetString("ERR_SingleTypeNameNotFoundFwd", resourceCulture);

	internal static string ERR_SizeofUnsafe => ResourceManager.GetString("ERR_SizeofUnsafe", resourceCulture);

	internal static string ERR_SourceFileReferencesNotSupported => ResourceManager.GetString("ERR_SourceFileReferencesNotSupported", resourceCulture);

	internal static string ERR_SourceLinkRequiresPdb => ResourceManager.GetString("ERR_SourceLinkRequiresPdb", resourceCulture);

	internal static string ERR_SpecialByRefInLambda => ResourceManager.GetString("ERR_SpecialByRefInLambda", resourceCulture);

	internal static string ERR_SpecialTypeAsBound => ResourceManager.GetString("ERR_SpecialTypeAsBound", resourceCulture);

	internal static string ERR_StackAllocConversionNotPossible => ResourceManager.GetString("ERR_StackAllocConversionNotPossible", resourceCulture);

	internal static string ERR_StackallocInCatchFinally => ResourceManager.GetString("ERR_StackallocInCatchFinally", resourceCulture);

	internal static string ERR_StatementLambdaToExpressionTree => ResourceManager.GetString("ERR_StatementLambdaToExpressionTree", resourceCulture);

	internal static string ERR_StaticBaseClass => ResourceManager.GetString("ERR_StaticBaseClass", resourceCulture);

	internal static string ERR_StaticClassInterfaceImpl => ResourceManager.GetString("ERR_StaticClassInterfaceImpl", resourceCulture);

	internal static string ERR_StaticConstant => ResourceManager.GetString("ERR_StaticConstant", resourceCulture);

	internal static string ERR_StaticConstParam => ResourceManager.GetString("ERR_StaticConstParam", resourceCulture);

	internal static string ERR_StaticConstructorWithAccessModifiers => ResourceManager.GetString("ERR_StaticConstructorWithAccessModifiers", resourceCulture);

	internal static string ERR_StaticConstructorWithExplicitConstructorCall => ResourceManager.GetString("ERR_StaticConstructorWithExplicitConstructorCall", resourceCulture);

	internal static string ERR_StaticDerivedFromNonObject => ResourceManager.GetString("ERR_StaticDerivedFromNonObject", resourceCulture);

	internal static string ERR_StaticInAsOrIs => ResourceManager.GetString("ERR_StaticInAsOrIs", resourceCulture);

	internal static string ERR_StaticMemberInObjectInitializer => ResourceManager.GetString("ERR_StaticMemberInObjectInitializer", resourceCulture);

	internal static string ERR_StaticNotVirtual => ResourceManager.GetString("ERR_StaticNotVirtual", resourceCulture);

	internal static string ERR_StructLayoutCycle => ResourceManager.GetString("ERR_StructLayoutCycle", resourceCulture);

	internal static string ERR_StructOffsetOnBadField => ResourceManager.GetString("ERR_StructOffsetOnBadField", resourceCulture);

	internal static string ERR_StructOffsetOnBadStruct => ResourceManager.GetString("ERR_StructOffsetOnBadStruct", resourceCulture);

	internal static string ERR_StructsCantContainDefaultConstructor => ResourceManager.GetString("ERR_StructsCantContainDefaultConstructor", resourceCulture);

	internal static string ERR_StructWithBaseConstructorCall => ResourceManager.GetString("ERR_StructWithBaseConstructorCall", resourceCulture);

	internal static string ERR_SubexpressionNotInNameof => ResourceManager.GetString("ERR_SubexpressionNotInNameof", resourceCulture);

	internal static string ERR_SwitchExpressionValueExpected => ResourceManager.GetString("ERR_SwitchExpressionValueExpected", resourceCulture);

	internal static string ERR_SwitchFallOut => ResourceManager.GetString("ERR_SwitchFallOut", resourceCulture);

	internal static string ERR_SwitchFallThrough => ResourceManager.GetString("ERR_SwitchFallThrough", resourceCulture);

	internal static string ERR_SwitchNeedsNumber => ResourceManager.GetString("ERR_SwitchNeedsNumber", resourceCulture);

	internal static string ERR_SwitchNeedsString => ResourceManager.GetString("ERR_SwitchNeedsString", resourceCulture);

	internal static string ERR_SynchronizedAsyncMethod => ResourceManager.GetString("ERR_SynchronizedAsyncMethod", resourceCulture);

	internal static string ERR_SyntaxError => ResourceManager.GetString("ERR_SyntaxError", resourceCulture);

	internal static string ERR_SystemVoid => ResourceManager.GetString("ERR_SystemVoid", resourceCulture);

	internal static string ERR_TaskRetNoObjectRequired => ResourceManager.GetString("ERR_TaskRetNoObjectRequired", resourceCulture);

	internal static string ERR_TaskRetNoObjectRequiredLambda => ResourceManager.GetString("ERR_TaskRetNoObjectRequiredLambda", resourceCulture);

	internal static string ERR_ThisInBadContext => ResourceManager.GetString("ERR_ThisInBadContext", resourceCulture);

	internal static string ERR_ThisInStaticMeth => ResourceManager.GetString("ERR_ThisInStaticMeth", resourceCulture);

	internal static string ERR_ThisOrBaseExpected => ResourceManager.GetString("ERR_ThisOrBaseExpected", resourceCulture);

	internal static string ERR_ThisStructNotInAnonMeth => ResourceManager.GetString("ERR_ThisStructNotInAnonMeth", resourceCulture);

	internal static string ERR_ThrowMisplaced => ResourceManager.GetString("ERR_ThrowMisplaced", resourceCulture);

	internal static string ERR_TooManyCatches => ResourceManager.GetString("ERR_TooManyCatches", resourceCulture);

	internal static string ERR_TooManyCharsInConst => ResourceManager.GetString("ERR_TooManyCharsInConst", resourceCulture);

	internal static string ERR_TooManyLocals => ResourceManager.GetString("ERR_TooManyLocals", resourceCulture);

	internal static string ERR_TooManyUserStrings => ResourceManager.GetString("ERR_TooManyUserStrings", resourceCulture);

	internal static string ERR_TrailingWhitespaceInFormatSpecifier => ResourceManager.GetString("ERR_TrailingWhitespaceInFormatSpecifier", resourceCulture);

	internal static string ERR_TupleDuplicateElementName => ResourceManager.GetString("ERR_TupleDuplicateElementName", resourceCulture);

	internal static string ERR_TupleElementNamesAttributeMissing => ResourceManager.GetString("ERR_TupleElementNamesAttributeMissing", resourceCulture);

	internal static string ERR_TupleElementNamesInDeconstruction => ResourceManager.GetString("ERR_TupleElementNamesInDeconstruction", resourceCulture);

	internal static string ERR_TupleInferredNamesNotAvailable => ResourceManager.GetString("ERR_TupleInferredNamesNotAvailable", resourceCulture);

	internal static string ERR_TupleReservedElementName => ResourceManager.GetString("ERR_TupleReservedElementName", resourceCulture);

	internal static string ERR_TupleReservedElementNameAnyPosition => ResourceManager.GetString("ERR_TupleReservedElementNameAnyPosition", resourceCulture);

	internal static string ERR_TupleSizesMismatchForBinOps => ResourceManager.GetString("ERR_TupleSizesMismatchForBinOps", resourceCulture);

	internal static string ERR_TupleTooFewElements => ResourceManager.GetString("ERR_TupleTooFewElements", resourceCulture);

	internal static string ERR_TypeArgsNotAllowed => ResourceManager.GetString("ERR_TypeArgsNotAllowed", resourceCulture);

	internal static string ERR_TypeExpected => ResourceManager.GetString("ERR_TypeExpected", resourceCulture);

	internal static string ERR_TypeForwardedToMultipleAssemblies => ResourceManager.GetString("ERR_TypeForwardedToMultipleAssemblies", resourceCulture);

	internal static string ERR_TypeInferenceFailedForImplicitlyTypedDeconstructionVariable => ResourceManager.GetString("ERR_TypeInferenceFailedForImplicitlyTypedDeconstructionVariable", resourceCulture);

	internal static string ERR_TypeInferenceFailedForImplicitlyTypedOutVariable => ResourceManager.GetString("ERR_TypeInferenceFailedForImplicitlyTypedOutVariable", resourceCulture);

	internal static string ERR_TypelessTupleInAs => ResourceManager.GetString("ERR_TypelessTupleInAs", resourceCulture);

	internal static string ERR_TypeParamMustBeIdentifier => ResourceManager.GetString("ERR_TypeParamMustBeIdentifier", resourceCulture);

	internal static string ERR_TypeReserved => ResourceManager.GetString("ERR_TypeReserved", resourceCulture);

	internal static string ERR_TypeVarCantBeNull => ResourceManager.GetString("ERR_TypeVarCantBeNull", resourceCulture);

	internal static string ERR_TypeVariableSameAsParent => ResourceManager.GetString("ERR_TypeVariableSameAsParent", resourceCulture);

	internal static string ERR_TypeVarNotFound => ResourceManager.GetString("ERR_TypeVarNotFound", resourceCulture);

	internal static string ERR_TypeVarNotFoundRangeVariable => ResourceManager.GetString("ERR_TypeVarNotFoundRangeVariable", resourceCulture);

	internal static string ERR_TyVarNotFoundInConstraint => ResourceManager.GetString("ERR_TyVarNotFoundInConstraint", resourceCulture);

	internal static string ERR_UnassignedThis => ResourceManager.GetString("ERR_UnassignedThis", resourceCulture);

	internal static string ERR_UnassignedThisAutoProperty => ResourceManager.GetString("ERR_UnassignedThisAutoProperty", resourceCulture);

	internal static string ERR_UnboxNotLValue => ResourceManager.GetString("ERR_UnboxNotLValue", resourceCulture);

	internal static string ERR_UnclosedExpressionHole => ResourceManager.GetString("ERR_UnclosedExpressionHole", resourceCulture);

	internal static string ERR_UnescapedCurly => ResourceManager.GetString("ERR_UnescapedCurly", resourceCulture);

	internal static string ERR_UnexpectedAliasedName => ResourceManager.GetString("ERR_UnexpectedAliasedName", resourceCulture);

	internal static string ERR_UnexpectedBoundGenericName => ResourceManager.GetString("ERR_UnexpectedBoundGenericName", resourceCulture);

	internal static string ERR_UnexpectedCharacter => ResourceManager.GetString("ERR_UnexpectedCharacter", resourceCulture);

	internal static string ERR_UnexpectedDirective => ResourceManager.GetString("ERR_UnexpectedDirective", resourceCulture);

	internal static string ERR_UnexpectedGenericName => ResourceManager.GetString("ERR_UnexpectedGenericName", resourceCulture);

	internal static string ERR_UnexpectedSemicolon => ResourceManager.GetString("ERR_UnexpectedSemicolon", resourceCulture);

	internal static string ERR_UnexpectedToken => ResourceManager.GetString("ERR_UnexpectedToken", resourceCulture);

	internal static string ERR_UnexpectedUnboundGenericName => ResourceManager.GetString("ERR_UnexpectedUnboundGenericName", resourceCulture);

	internal static string ERR_UnexpectedVariance => ResourceManager.GetString("ERR_UnexpectedVariance", resourceCulture);

	internal static string ERR_UnifyingInterfaceInstantiations => ResourceManager.GetString("ERR_UnifyingInterfaceInstantiations", resourceCulture);

	internal static string ERR_UnimplementedAbstractMethod => ResourceManager.GetString("ERR_UnimplementedAbstractMethod", resourceCulture);

	internal static string ERR_UnimplementedInterfaceAccessor => ResourceManager.GetString("ERR_UnimplementedInterfaceAccessor", resourceCulture);

	internal static string ERR_UnimplementedInterfaceMember => ResourceManager.GetString("ERR_UnimplementedInterfaceMember", resourceCulture);

	internal static string ERR_UnmanagedBoundWithClass => ResourceManager.GetString("ERR_UnmanagedBoundWithClass", resourceCulture);

	internal static string ERR_UnmanagedConstraintMustBeFirst => ResourceManager.GetString("ERR_UnmanagedConstraintMustBeFirst", resourceCulture);

	internal static string ERR_UnmanagedConstraintNotSatisfied => ResourceManager.GetString("ERR_UnmanagedConstraintNotSatisfied", resourceCulture);

	internal static string ERR_UnreachableCatch => ResourceManager.GetString("ERR_UnreachableCatch", resourceCulture);

	internal static string ERR_UnsafeAsyncArgType => ResourceManager.GetString("ERR_UnsafeAsyncArgType", resourceCulture);

	internal static string ERR_UnsafeIteratorArgType => ResourceManager.GetString("ERR_UnsafeIteratorArgType", resourceCulture);

	internal static string ERR_UnsafeNeeded => ResourceManager.GetString("ERR_UnsafeNeeded", resourceCulture);

	internal static string ERR_UnsafeTypeInObjectCreation => ResourceManager.GetString("ERR_UnsafeTypeInObjectCreation", resourceCulture);

	internal static string ERR_UnsupportedTransparentIdentifierAccess => ResourceManager.GetString("ERR_UnsupportedTransparentIdentifierAccess", resourceCulture);

	internal static string ERR_UnterminatedStringLit => ResourceManager.GetString("ERR_UnterminatedStringLit", resourceCulture);

	internal static string ERR_UseDefViolation => ResourceManager.GetString("ERR_UseDefViolation", resourceCulture);

	internal static string ERR_UseDefViolationField => ResourceManager.GetString("ERR_UseDefViolationField", resourceCulture);

	internal static string ERR_UseDefViolationOut => ResourceManager.GetString("ERR_UseDefViolationOut", resourceCulture);

	internal static string ERR_UseDefViolationProperty => ResourceManager.GetString("ERR_UseDefViolationProperty", resourceCulture);

	internal static string ERR_UseDefViolationThis => ResourceManager.GetString("ERR_UseDefViolationThis", resourceCulture);

	internal static string ERR_UsingAfterElements => ResourceManager.GetString("ERR_UsingAfterElements", resourceCulture);

	internal static string ERR_V6SwitchGoverningTypeValueExpected => ResourceManager.GetString("ERR_V6SwitchGoverningTypeValueExpected", resourceCulture);

	internal static string ERR_ValConstraintNotSatisfied => ResourceManager.GetString("ERR_ValConstraintNotSatisfied", resourceCulture);

	internal static string ERR_ValueCantBeNull => ResourceManager.GetString("ERR_ValueCantBeNull", resourceCulture);

	internal static string ERR_ValueExpected => ResourceManager.GetString("ERR_ValueExpected", resourceCulture);

	internal static string ERR_ValueTypeExtDelegate => ResourceManager.GetString("ERR_ValueTypeExtDelegate", resourceCulture);

	internal static string ERR_ValueTypePropertyInObjectInitializer => ResourceManager.GetString("ERR_ValueTypePropertyInObjectInitializer", resourceCulture);

	internal static string ERR_VarargsAsync => ResourceManager.GetString("ERR_VarargsAsync", resourceCulture);

	internal static string ERR_VarArgsInExpressionTree => ResourceManager.GetString("ERR_VarArgsInExpressionTree", resourceCulture);

	internal static string ERR_VarargsIterator => ResourceManager.GetString("ERR_VarargsIterator", resourceCulture);

	internal static string ERR_VarargsLast => ResourceManager.GetString("ERR_VarargsLast", resourceCulture);

	internal static string ERR_VarDeclIsStaticClass => ResourceManager.GetString("ERR_VarDeclIsStaticClass", resourceCulture);

	internal static string ERR_VariableUsedBeforeDeclaration => ResourceManager.GetString("ERR_VariableUsedBeforeDeclaration", resourceCulture);

	internal static string ERR_VariableUsedBeforeDeclarationAndHidesField => ResourceManager.GetString("ERR_VariableUsedBeforeDeclarationAndHidesField", resourceCulture);

	internal static string ERR_VarInvocationLvalueReserved => ResourceManager.GetString("ERR_VarInvocationLvalueReserved", resourceCulture);

	internal static string ERR_VirtualPrivate => ResourceManager.GetString("ERR_VirtualPrivate", resourceCulture);

	internal static string ERR_VoidAssignment => ResourceManager.GetString("ERR_VoidAssignment", resourceCulture);

	internal static string ERR_VoidError => ResourceManager.GetString("ERR_VoidError", resourceCulture);

	internal static string ERR_VoidInTuple => ResourceManager.GetString("ERR_VoidInTuple", resourceCulture);

	internal static string ERR_VolatileAndReadonly => ResourceManager.GetString("ERR_VolatileAndReadonly", resourceCulture);

	internal static string ERR_VolatileStruct => ResourceManager.GetString("ERR_VolatileStruct", resourceCulture);

	internal static string ERR_WinRtEventPassedByRef => ResourceManager.GetString("ERR_WinRtEventPassedByRef", resourceCulture);

	internal static string ERR_YieldInAnonMeth => ResourceManager.GetString("ERR_YieldInAnonMeth", resourceCulture);

	internal static string ERR_YieldNotAllowedInScript => ResourceManager.GetString("ERR_YieldNotAllowedInScript", resourceCulture);

	internal static string FTL_BadChecksumAlgorithm => ResourceManager.GetString("FTL_BadChecksumAlgorithm", resourceCulture);

	internal static string FTL_BadCodepage => ResourceManager.GetString("FTL_BadCodepage", resourceCulture);

	internal static string FTL_DebugEmitFailure => ResourceManager.GetString("FTL_DebugEmitFailure", resourceCulture);

	internal static string FTL_InvalidInputFileName => ResourceManager.GetString("FTL_InvalidInputFileName", resourceCulture);

	internal static string FTL_InvalidTarget => ResourceManager.GetString("FTL_InvalidTarget", resourceCulture);

	internal static string FTL_MetadataCantOpenFile => ResourceManager.GetString("FTL_MetadataCantOpenFile", resourceCulture);

	internal static string FTL_OutputFileExists => ResourceManager.GetString("FTL_OutputFileExists", resourceCulture);

	internal static string GenericParameterDefinition => ResourceManager.GetString("GenericParameterDefinition", resourceCulture);

	internal static string HDN_UnusedExternAlias => ResourceManager.GetString("HDN_UnusedExternAlias", resourceCulture);

	internal static string HDN_UnusedExternAlias_Title => ResourceManager.GetString("HDN_UnusedExternAlias_Title", resourceCulture);

	internal static string HDN_UnusedUsingDirective => ResourceManager.GetString("HDN_UnusedUsingDirective", resourceCulture);

	internal static string HDN_UnusedUsingDirective_Title => ResourceManager.GetString("HDN_UnusedUsingDirective_Title", resourceCulture);

	internal static string ICompoundAssignmentOperationIsNotCSharpCompoundAssignment => ResourceManager.GetString("ICompoundAssignmentOperationIsNotCSharpCompoundAssignment", resourceCulture);

	internal static string IConversionExpressionIsNotCSharpConversion => ResourceManager.GetString("IConversionExpressionIsNotCSharpConversion", resourceCulture);

	internal static string IDS_AnonMethod => ResourceManager.GetString("IDS_AnonMethod", resourceCulture);

	internal static string IDS_AwaitInCatchAndFinally => ResourceManager.GetString("IDS_AwaitInCatchAndFinally", resourceCulture);

	internal static string IDS_Collection => ResourceManager.GetString("IDS_Collection", resourceCulture);

	internal static string IDS_Contravariant => ResourceManager.GetString("IDS_Contravariant", resourceCulture);

	internal static string IDS_Contravariantly => ResourceManager.GetString("IDS_Contravariantly", resourceCulture);

	internal static string IDS_Covariant => ResourceManager.GetString("IDS_Covariant", resourceCulture);

	internal static string IDS_Covariantly => ResourceManager.GetString("IDS_Covariantly", resourceCulture);

	internal static string IDS_CSCHelp => ResourceManager.GetString("IDS_CSCHelp", resourceCulture);

	internal static string IDS_DirectoryDoesNotExist => ResourceManager.GetString("IDS_DirectoryDoesNotExist", resourceCulture);

	internal static string IDS_DirectoryHasInvalidPath => ResourceManager.GetString("IDS_DirectoryHasInvalidPath", resourceCulture);

	internal static string IDS_FeatureAnonDelegates => ResourceManager.GetString("IDS_FeatureAnonDelegates", resourceCulture);

	internal static string IDS_FeatureAnonymousTypes => ResourceManager.GetString("IDS_FeatureAnonymousTypes", resourceCulture);

	internal static string IDS_FeatureAsync => ResourceManager.GetString("IDS_FeatureAsync", resourceCulture);

	internal static string IDS_FeatureAsyncMain => ResourceManager.GetString("IDS_FeatureAsyncMain", resourceCulture);

	internal static string IDS_FeatureAutoImplementedProperties => ResourceManager.GetString("IDS_FeatureAutoImplementedProperties", resourceCulture);

	internal static string IDS_FeatureAutoPropertyInitializer => ResourceManager.GetString("IDS_FeatureAutoPropertyInitializer", resourceCulture);

	internal static string IDS_FeatureBinaryLiteral => ResourceManager.GetString("IDS_FeatureBinaryLiteral", resourceCulture);

	internal static string IDS_FeatureCollectionInitializer => ResourceManager.GetString("IDS_FeatureCollectionInitializer", resourceCulture);

	internal static string IDS_FeatureDefault => ResourceManager.GetString("IDS_FeatureDefault", resourceCulture);

	internal static string IDS_FeatureDefaultLiteral => ResourceManager.GetString("IDS_FeatureDefaultLiteral", resourceCulture);

	internal static string IDS_FeatureDelegateGenericTypeConstraint => ResourceManager.GetString("IDS_FeatureDelegateGenericTypeConstraint", resourceCulture);

	internal static string IDS_FeatureDictionaryInitializer => ResourceManager.GetString("IDS_FeatureDictionaryInitializer", resourceCulture);

	internal static string IDS_FeatureDigitSeparator => ResourceManager.GetString("IDS_FeatureDigitSeparator", resourceCulture);

	internal static string IDS_FeatureDynamic => ResourceManager.GetString("IDS_FeatureDynamic", resourceCulture);

	internal static string IDS_FeatureEnumGenericTypeConstraint => ResourceManager.GetString("IDS_FeatureEnumGenericTypeConstraint", resourceCulture);

	internal static string IDS_FeatureExceptionFilter => ResourceManager.GetString("IDS_FeatureExceptionFilter", resourceCulture);

	internal static string IDS_FeatureExpressionBodiedAccessor => ResourceManager.GetString("IDS_FeatureExpressionBodiedAccessor", resourceCulture);

	internal static string IDS_FeatureExpressionBodiedDeOrConstructor => ResourceManager.GetString("IDS_FeatureExpressionBodiedDeOrConstructor", resourceCulture);

	internal static string IDS_FeatureExpressionBodiedIndexer => ResourceManager.GetString("IDS_FeatureExpressionBodiedIndexer", resourceCulture);

	internal static string IDS_FeatureExpressionBodiedMethod => ResourceManager.GetString("IDS_FeatureExpressionBodiedMethod", resourceCulture);

	internal static string IDS_FeatureExpressionBodiedProperty => ResourceManager.GetString("IDS_FeatureExpressionBodiedProperty", resourceCulture);

	internal static string IDS_FeatureExpressionVariablesInQueriesAndInitializers => ResourceManager.GetString("IDS_FeatureExpressionVariablesInQueriesAndInitializers", resourceCulture);

	internal static string IDS_FeatureExtensibleFixedStatement => ResourceManager.GetString("IDS_FeatureExtensibleFixedStatement", resourceCulture);

	internal static string IDS_FeatureExtensionMethod => ResourceManager.GetString("IDS_FeatureExtensionMethod", resourceCulture);

	internal static string IDS_FeatureExternAlias => ResourceManager.GetString("IDS_FeatureExternAlias", resourceCulture);

	internal static string IDS_FeatureFixedBuffer => ResourceManager.GetString("IDS_FeatureFixedBuffer", resourceCulture);

	internal static string IDS_FeatureGenerics => ResourceManager.GetString("IDS_FeatureGenerics", resourceCulture);

	internal static string IDS_FeatureGlobalNamespace => ResourceManager.GetString("IDS_FeatureGlobalNamespace", resourceCulture);

	internal static string IDS_FeatureImplicitArray => ResourceManager.GetString("IDS_FeatureImplicitArray", resourceCulture);

	internal static string IDS_FeatureImplicitLocal => ResourceManager.GetString("IDS_FeatureImplicitLocal", resourceCulture);

	internal static string IDS_FeatureIndexingMovableFixedBuffers => ResourceManager.GetString("IDS_FeatureIndexingMovableFixedBuffers", resourceCulture);

	internal static string IDS_FeatureInterpolatedStrings => ResourceManager.GetString("IDS_FeatureInterpolatedStrings", resourceCulture);

	internal static string IDS_FeatureIterators => ResourceManager.GetString("IDS_FeatureIterators", resourceCulture);

	internal static string IDS_FeatureLambda => ResourceManager.GetString("IDS_FeatureLambda", resourceCulture);

	internal static string IDS_FeatureLeadingDigitSeparator => ResourceManager.GetString("IDS_FeatureLeadingDigitSeparator", resourceCulture);

	internal static string IDS_FeatureLocalFunctions => ResourceManager.GetString("IDS_FeatureLocalFunctions", resourceCulture);

	internal static string IDS_FeatureModuleAttrLoc => ResourceManager.GetString("IDS_FeatureModuleAttrLoc", resourceCulture);

	internal static string IDS_FeatureNamedArgument => ResourceManager.GetString("IDS_FeatureNamedArgument", resourceCulture);

	internal static string IDS_FeatureNameof => ResourceManager.GetString("IDS_FeatureNameof", resourceCulture);

	internal static string IDS_FeatureNullable => ResourceManager.GetString("IDS_FeatureNullable", resourceCulture);

	internal static string IDS_FeatureNullPropagatingOperator => ResourceManager.GetString("IDS_FeatureNullPropagatingOperator", resourceCulture);

	internal static string IDS_FeatureObjectInitializer => ResourceManager.GetString("IDS_FeatureObjectInitializer", resourceCulture);

	internal static string IDS_FeatureOptionalParameter => ResourceManager.GetString("IDS_FeatureOptionalParameter", resourceCulture);

	internal static string IDS_FeatureOutVar => ResourceManager.GetString("IDS_FeatureOutVar", resourceCulture);

	internal static string IDS_FeaturePartialMethod => ResourceManager.GetString("IDS_FeaturePartialMethod", resourceCulture);

	internal static string IDS_FeaturePartialTypes => ResourceManager.GetString("IDS_FeaturePartialTypes", resourceCulture);

	internal static string IDS_FeaturePatternMatching => ResourceManager.GetString("IDS_FeaturePatternMatching", resourceCulture);

	internal static string IDS_FeaturePragma => ResourceManager.GetString("IDS_FeaturePragma", resourceCulture);

	internal static string IDS_FeaturePrivateProtected => ResourceManager.GetString("IDS_FeaturePrivateProtected", resourceCulture);

	internal static string IDS_FeaturePropertyAccessorMods => ResourceManager.GetString("IDS_FeaturePropertyAccessorMods", resourceCulture);

	internal static string IDS_FeatureQueryExpression => ResourceManager.GetString("IDS_FeatureQueryExpression", resourceCulture);

	internal static string IDS_FeatureReadonlyAutoImplementedProperties => ResourceManager.GetString("IDS_FeatureReadonlyAutoImplementedProperties", resourceCulture);

	internal static string IDS_FeatureReadOnlyReferences => ResourceManager.GetString("IDS_FeatureReadOnlyReferences", resourceCulture);

	internal static string IDS_FeatureReadOnlyStructs => ResourceManager.GetString("IDS_FeatureReadOnlyStructs", resourceCulture);

	internal static string IDS_FeatureRefConditional => ResourceManager.GetString("IDS_FeatureRefConditional", resourceCulture);

	internal static string IDS_FeatureRefExtensionMethods => ResourceManager.GetString("IDS_FeatureRefExtensionMethods", resourceCulture);

	internal static string IDS_FeatureRefFor => ResourceManager.GetString("IDS_FeatureRefFor", resourceCulture);

	internal static string IDS_FeatureRefForEach => ResourceManager.GetString("IDS_FeatureRefForEach", resourceCulture);

	internal static string IDS_FeatureRefLocalsReturns => ResourceManager.GetString("IDS_FeatureRefLocalsReturns", resourceCulture);

	internal static string IDS_FeatureRefReassignment => ResourceManager.GetString("IDS_FeatureRefReassignment", resourceCulture);

	internal static string IDS_FeatureRefStructs => ResourceManager.GetString("IDS_FeatureRefStructs", resourceCulture);

	internal static string IDS_FeatureStackAllocInitializer => ResourceManager.GetString("IDS_FeatureStackAllocInitializer", resourceCulture);

	internal static string IDS_FeatureStaticClasses => ResourceManager.GetString("IDS_FeatureStaticClasses", resourceCulture);

	internal static string IDS_FeatureSwitchOnBool => ResourceManager.GetString("IDS_FeatureSwitchOnBool", resourceCulture);

	internal static string IDS_FeatureThrowExpression => ResourceManager.GetString("IDS_FeatureThrowExpression", resourceCulture);

	internal static string IDS_FeatureTupleEquality => ResourceManager.GetString("IDS_FeatureTupleEquality", resourceCulture);

	internal static string IDS_FeatureTuples => ResourceManager.GetString("IDS_FeatureTuples", resourceCulture);

	internal static string IDS_FeatureTypeVariance => ResourceManager.GetString("IDS_FeatureTypeVariance", resourceCulture);

	internal static string IDS_FeatureUnmanagedGenericTypeConstraint => ResourceManager.GetString("IDS_FeatureUnmanagedGenericTypeConstraint", resourceCulture);

	internal static string IDS_FeatureUsingStatic => ResourceManager.GetString("IDS_FeatureUsingStatic", resourceCulture);

	internal static string IDS_FIXEDLOCAL => ResourceManager.GetString("IDS_FIXEDLOCAL", resourceCulture);

	internal static string IDS_FOREACHLOCAL => ResourceManager.GetString("IDS_FOREACHLOCAL", resourceCulture);

	internal static string IDS_GlobalNamespace => ResourceManager.GetString("IDS_GlobalNamespace", resourceCulture);

	internal static string IDS_Invariantly => ResourceManager.GetString("IDS_Invariantly", resourceCulture);

	internal static string IDS_Lambda => ResourceManager.GetString("IDS_Lambda", resourceCulture);

	internal static string IDS_LangVersions => ResourceManager.GetString("IDS_LangVersions", resourceCulture);

	internal static string IDS_LIB_ENV => ResourceManager.GetString("IDS_LIB_ENV", resourceCulture);

	internal static string IDS_LIB_OPTION => ResourceManager.GetString("IDS_LIB_OPTION", resourceCulture);

	internal static string IDS_LogoLine1 => ResourceManager.GetString("IDS_LogoLine1", resourceCulture);

	internal static string IDS_LogoLine2 => ResourceManager.GetString("IDS_LogoLine2", resourceCulture);

	internal static string IDS_MethodGroup => ResourceManager.GetString("IDS_MethodGroup", resourceCulture);

	internal static string IDS_Namespace1 => ResourceManager.GetString("IDS_Namespace1", resourceCulture);

	internal static string IDS_NULL => ResourceManager.GetString("IDS_NULL", resourceCulture);

	internal static string IDS_OperationCausedStackOverflow => ResourceManager.GetString("IDS_OperationCausedStackOverflow", resourceCulture);

	internal static string IDS_PathList => ResourceManager.GetString("IDS_PathList", resourceCulture);

	internal static string IDS_REFERENCEPATH_OPTION => ResourceManager.GetString("IDS_REFERENCEPATH_OPTION", resourceCulture);

	internal static string IDS_RELATEDERROR => ResourceManager.GetString("IDS_RELATEDERROR", resourceCulture);

	internal static string IDS_RELATEDWARNING => ResourceManager.GetString("IDS_RELATEDWARNING", resourceCulture);

	internal static string IDS_SK_ALIAS => ResourceManager.GetString("IDS_SK_ALIAS", resourceCulture);

	internal static string IDS_SK_CONSTRUCTOR => ResourceManager.GetString("IDS_SK_CONSTRUCTOR", resourceCulture);

	internal static string IDS_SK_EVENT => ResourceManager.GetString("IDS_SK_EVENT", resourceCulture);

	internal static string IDS_SK_EXTERNALIAS => ResourceManager.GetString("IDS_SK_EXTERNALIAS", resourceCulture);

	internal static string IDS_SK_FIELD => ResourceManager.GetString("IDS_SK_FIELD", resourceCulture);

	internal static string IDS_SK_LABEL => ResourceManager.GetString("IDS_SK_LABEL", resourceCulture);

	internal static string IDS_SK_METHOD => ResourceManager.GetString("IDS_SK_METHOD", resourceCulture);

	internal static string IDS_SK_NAMESPACE => ResourceManager.GetString("IDS_SK_NAMESPACE", resourceCulture);

	internal static string IDS_SK_PROPERTY => ResourceManager.GetString("IDS_SK_PROPERTY", resourceCulture);

	internal static string IDS_SK_TYPE => ResourceManager.GetString("IDS_SK_TYPE", resourceCulture);

	internal static string IDS_SK_TYVAR => ResourceManager.GetString("IDS_SK_TYVAR", resourceCulture);

	internal static string IDS_SK_UNKNOWN => ResourceManager.GetString("IDS_SK_UNKNOWN", resourceCulture);

	internal static string IDS_SK_VARIABLE => ResourceManager.GetString("IDS_SK_VARIABLE", resourceCulture);

	internal static string IDS_Text => ResourceManager.GetString("IDS_Text", resourceCulture);

	internal static string IDS_ThrowExpression => ResourceManager.GetString("IDS_ThrowExpression", resourceCulture);

	internal static string IDS_ToolName => ResourceManager.GetString("IDS_ToolName", resourceCulture);

	internal static string IDS_USINGLOCAL => ResourceManager.GetString("IDS_USINGLOCAL", resourceCulture);

	internal static string IDS_VersionExperimental => ResourceManager.GetString("IDS_VersionExperimental", resourceCulture);

	internal static string IDS_XMLBADINCLUDE => ResourceManager.GetString("IDS_XMLBADINCLUDE", resourceCulture);

	internal static string IDS_XMLFAILEDINCLUDE => ResourceManager.GetString("IDS_XMLFAILEDINCLUDE", resourceCulture);

	internal static string IDS_XMLIGNORED => ResourceManager.GetString("IDS_XMLIGNORED", resourceCulture);

	internal static string IDS_XMLIGNORED2 => ResourceManager.GetString("IDS_XMLIGNORED2", resourceCulture);

	internal static string IDS_XMLMISSINGINCLUDEFILE => ResourceManager.GetString("IDS_XMLMISSINGINCLUDEFILE", resourceCulture);

	internal static string IDS_XMLMISSINGINCLUDEPATH => ResourceManager.GetString("IDS_XMLMISSINGINCLUDEPATH", resourceCulture);

	internal static string IDS_XMLNOINCLUDE => ResourceManager.GetString("IDS_XMLNOINCLUDE", resourceCulture);

	internal static string INF_UnableToLoadSomeTypesInAnalyzer => ResourceManager.GetString("INF_UnableToLoadSomeTypesInAnalyzer", resourceCulture);

	internal static string INF_UnableToLoadSomeTypesInAnalyzer_Title => ResourceManager.GetString("INF_UnableToLoadSomeTypesInAnalyzer_Title", resourceCulture);

	internal static string InvalidGetDeclarationNameMultipleDeclarators => ResourceManager.GetString("InvalidGetDeclarationNameMultipleDeclarators", resourceCulture);

	internal static string ItemsMustBeNonEmpty => ResourceManager.GetString("ItemsMustBeNonEmpty", resourceCulture);

	internal static string LocationMustBeProvided => ResourceManager.GetString("LocationMustBeProvided", resourceCulture);

	internal static string LookupOptionsHasInvalidCombo => ResourceManager.GetString("LookupOptionsHasInvalidCombo", resourceCulture);

	internal static string NameConflictForName => ResourceManager.GetString("NameConflictForName", resourceCulture);

	internal static string NoNoneSearchCriteria => ResourceManager.GetString("NoNoneSearchCriteria", resourceCulture);

	internal static string NotACSharpSymbol => ResourceManager.GetString("NotACSharpSymbol", resourceCulture);

	internal static string PositionIsNotWithinSyntax => ResourceManager.GetString("PositionIsNotWithinSyntax", resourceCulture);

	internal static string PositionNotWithinTree => ResourceManager.GetString("PositionNotWithinTree", resourceCulture);

	internal static string SpeculatedSyntaxNodeCannotBelongToCurrentCompilation => ResourceManager.GetString("SpeculatedSyntaxNodeCannotBelongToCurrentCompilation", resourceCulture);

	internal static string SubmissionCanHaveAtMostOne => ResourceManager.GetString("SubmissionCanHaveAtMostOne", resourceCulture);

	internal static string SubmissionCanOnlyInclude => ResourceManager.GetString("SubmissionCanOnlyInclude", resourceCulture);

	internal static string SyntaxNodeIsNotWithinSynt => ResourceManager.GetString("SyntaxNodeIsNotWithinSynt", resourceCulture);

	internal static string SyntaxTreeAlreadyPresent => ResourceManager.GetString("SyntaxTreeAlreadyPresent", resourceCulture);

	internal static string SyntaxTreeFromLoadNoRemoveReplace => ResourceManager.GetString("SyntaxTreeFromLoadNoRemoveReplace", resourceCulture);

	internal static string SyntaxTreeIsNotASubmission => ResourceManager.GetString("SyntaxTreeIsNotASubmission", resourceCulture);

	internal static string SyntaxTreeNotFound => ResourceManager.GetString("SyntaxTreeNotFound", resourceCulture);

	internal static string SyntaxTreeNotFoundToRemove => ResourceManager.GetString("SyntaxTreeNotFoundToRemove", resourceCulture);

	internal static string SyntaxTreeSemanticModelMust => ResourceManager.GetString("SyntaxTreeSemanticModelMust", resourceCulture);

	internal static string ThisMethodCanOnlyBeUsedToCreateTokens => ResourceManager.GetString("ThisMethodCanOnlyBeUsedToCreateTokens", resourceCulture);

	internal static string TreeMustHaveARootNodeWith => ResourceManager.GetString("TreeMustHaveARootNodeWith", resourceCulture);

	internal static string TreeNotPartOfCompilation => ResourceManager.GetString("TreeNotPartOfCompilation", resourceCulture);

	internal static string TypeArgumentCannotBeNull => ResourceManager.GetString("TypeArgumentCannotBeNull", resourceCulture);

	internal static string TypeMustBeVar => ResourceManager.GetString("TypeMustBeVar", resourceCulture);

	internal static string UseLiteralForNumeric => ResourceManager.GetString("UseLiteralForNumeric", resourceCulture);

	internal static string UseLiteralForTokens => ResourceManager.GetString("UseLiteralForTokens", resourceCulture);

	internal static string UseVerbatimIdentifier => ResourceManager.GetString("UseVerbatimIdentifier", resourceCulture);

	internal static string WRN_AlignmentMagnitude => ResourceManager.GetString("WRN_AlignmentMagnitude", resourceCulture);

	internal static string WRN_AlignmentMagnitude_Title => ResourceManager.GetString("WRN_AlignmentMagnitude_Title", resourceCulture);

	internal static string WRN_AlwaysNull => ResourceManager.GetString("WRN_AlwaysNull", resourceCulture);

	internal static string WRN_AlwaysNull_Title => ResourceManager.GetString("WRN_AlwaysNull_Title", resourceCulture);

	internal static string WRN_AmbiguousXMLReference => ResourceManager.GetString("WRN_AmbiguousXMLReference", resourceCulture);

	internal static string WRN_AmbiguousXMLReference_Title => ResourceManager.GetString("WRN_AmbiguousXMLReference_Title", resourceCulture);

	internal static string WRN_AnalyzerCannotBeCreated => ResourceManager.GetString("WRN_AnalyzerCannotBeCreated", resourceCulture);

	internal static string WRN_AnalyzerCannotBeCreated_Title => ResourceManager.GetString("WRN_AnalyzerCannotBeCreated_Title", resourceCulture);

	internal static string WRN_AssemblyAttributeFromModuleIsOverridden => ResourceManager.GetString("WRN_AssemblyAttributeFromModuleIsOverridden", resourceCulture);

	internal static string WRN_AssemblyAttributeFromModuleIsOverridden_Title => ResourceManager.GetString("WRN_AssemblyAttributeFromModuleIsOverridden_Title", resourceCulture);

	internal static string WRN_AssignmentToLockOrDispose => ResourceManager.GetString("WRN_AssignmentToLockOrDispose", resourceCulture);

	internal static string WRN_AssignmentToLockOrDispose_Title => ResourceManager.GetString("WRN_AssignmentToLockOrDispose_Title", resourceCulture);

	internal static string WRN_AssignmentToSelf => ResourceManager.GetString("WRN_AssignmentToSelf", resourceCulture);

	internal static string WRN_AssignmentToSelf_Title => ResourceManager.GetString("WRN_AssignmentToSelf_Title", resourceCulture);

	internal static string WRN_AsyncLacksAwaits => ResourceManager.GetString("WRN_AsyncLacksAwaits", resourceCulture);

	internal static string WRN_AsyncLacksAwaits_Title => ResourceManager.GetString("WRN_AsyncLacksAwaits_Title", resourceCulture);

	internal static string WRN_AttributeIgnoredWhenPublicSigning => ResourceManager.GetString("WRN_AttributeIgnoredWhenPublicSigning", resourceCulture);

	internal static string WRN_AttributeIgnoredWhenPublicSigning_Title => ResourceManager.GetString("WRN_AttributeIgnoredWhenPublicSigning_Title", resourceCulture);

	internal static string WRN_AttributeLocationOnBadDeclaration => ResourceManager.GetString("WRN_AttributeLocationOnBadDeclaration", resourceCulture);

	internal static string WRN_AttributeLocationOnBadDeclaration_Title => ResourceManager.GetString("WRN_AttributeLocationOnBadDeclaration_Title", resourceCulture);

	internal static string WRN_AttributesOnBackingFieldsNotAvailable => ResourceManager.GetString("WRN_AttributesOnBackingFieldsNotAvailable", resourceCulture);

	internal static string WRN_AttributesOnBackingFieldsNotAvailable_Title => ResourceManager.GetString("WRN_AttributesOnBackingFieldsNotAvailable_Title", resourceCulture);

	internal static string WRN_BadRefCompareLeft => ResourceManager.GetString("WRN_BadRefCompareLeft", resourceCulture);

	internal static string WRN_BadRefCompareLeft_Title => ResourceManager.GetString("WRN_BadRefCompareLeft_Title", resourceCulture);

	internal static string WRN_BadRefCompareRight => ResourceManager.GetString("WRN_BadRefCompareRight", resourceCulture);

	internal static string WRN_BadRefCompareRight_Title => ResourceManager.GetString("WRN_BadRefCompareRight_Title", resourceCulture);

	internal static string WRN_BadRestoreNumber => ResourceManager.GetString("WRN_BadRestoreNumber", resourceCulture);

	internal static string WRN_BadRestoreNumber_Title => ResourceManager.GetString("WRN_BadRestoreNumber_Title", resourceCulture);

	internal static string WRN_BadUILang => ResourceManager.GetString("WRN_BadUILang", resourceCulture);

	internal static string WRN_BadUILang_Title => ResourceManager.GetString("WRN_BadUILang_Title", resourceCulture);

	internal static string WRN_BadWarningNumber => ResourceManager.GetString("WRN_BadWarningNumber", resourceCulture);

	internal static string WRN_BadWarningNumber_Description => ResourceManager.GetString("WRN_BadWarningNumber_Description", resourceCulture);

	internal static string WRN_BadWarningNumber_Title => ResourceManager.GetString("WRN_BadWarningNumber_Title", resourceCulture);

	internal static string WRN_BadXMLRef => ResourceManager.GetString("WRN_BadXMLRef", resourceCulture);

	internal static string WRN_BadXMLRef_Title => ResourceManager.GetString("WRN_BadXMLRef_Title", resourceCulture);

	internal static string WRN_BadXMLRefParamType => ResourceManager.GetString("WRN_BadXMLRefParamType", resourceCulture);

	internal static string WRN_BadXMLRefParamType_Title => ResourceManager.GetString("WRN_BadXMLRefParamType_Title", resourceCulture);

	internal static string WRN_BadXMLRefReturnType => ResourceManager.GetString("WRN_BadXMLRefReturnType", resourceCulture);

	internal static string WRN_BadXMLRefReturnType_Title => ResourceManager.GetString("WRN_BadXMLRefReturnType_Title", resourceCulture);

	internal static string WRN_BadXMLRefSyntax => ResourceManager.GetString("WRN_BadXMLRefSyntax", resourceCulture);

	internal static string WRN_BadXMLRefSyntax_Title => ResourceManager.GetString("WRN_BadXMLRefSyntax_Title", resourceCulture);

	internal static string WRN_BadXMLRefTypeVar => ResourceManager.GetString("WRN_BadXMLRefTypeVar", resourceCulture);

	internal static string WRN_BadXMLRefTypeVar_Title => ResourceManager.GetString("WRN_BadXMLRefTypeVar_Title", resourceCulture);

	internal static string WRN_BitwiseOrSignExtend => ResourceManager.GetString("WRN_BitwiseOrSignExtend", resourceCulture);

	internal static string WRN_BitwiseOrSignExtend_Description => ResourceManager.GetString("WRN_BitwiseOrSignExtend_Description", resourceCulture);

	internal static string WRN_BitwiseOrSignExtend_Title => ResourceManager.GetString("WRN_BitwiseOrSignExtend_Title", resourceCulture);

	internal static string WRN_ByRefNonAgileField => ResourceManager.GetString("WRN_ByRefNonAgileField", resourceCulture);

	internal static string WRN_ByRefNonAgileField_Title => ResourceManager.GetString("WRN_ByRefNonAgileField_Title", resourceCulture);

	internal static string WRN_CA2000_DisposeObjectsBeforeLosingScope1 => ResourceManager.GetString("WRN_CA2000_DisposeObjectsBeforeLosingScope1", resourceCulture);

	internal static string WRN_CA2000_DisposeObjectsBeforeLosingScope1_Title => ResourceManager.GetString("WRN_CA2000_DisposeObjectsBeforeLosingScope1_Title", resourceCulture);

	internal static string WRN_CA2000_DisposeObjectsBeforeLosingScope2 => ResourceManager.GetString("WRN_CA2000_DisposeObjectsBeforeLosingScope2", resourceCulture);

	internal static string WRN_CA2000_DisposeObjectsBeforeLosingScope2_Title => ResourceManager.GetString("WRN_CA2000_DisposeObjectsBeforeLosingScope2_Title", resourceCulture);

	internal static string WRN_CA2202_DoNotDisposeObjectsMultipleTimes => ResourceManager.GetString("WRN_CA2202_DoNotDisposeObjectsMultipleTimes", resourceCulture);

	internal static string WRN_CA2202_DoNotDisposeObjectsMultipleTimes_Title => ResourceManager.GetString("WRN_CA2202_DoNotDisposeObjectsMultipleTimes_Title", resourceCulture);

	internal static string WRN_CallerFilePathParamForUnconsumedLocation => ResourceManager.GetString("WRN_CallerFilePathParamForUnconsumedLocation", resourceCulture);

	internal static string WRN_CallerFilePathParamForUnconsumedLocation_Title => ResourceManager.GetString("WRN_CallerFilePathParamForUnconsumedLocation_Title", resourceCulture);

	internal static string WRN_CallerFilePathPreferredOverCallerMemberName => ResourceManager.GetString("WRN_CallerFilePathPreferredOverCallerMemberName", resourceCulture);

	internal static string WRN_CallerFilePathPreferredOverCallerMemberName_Title => ResourceManager.GetString("WRN_CallerFilePathPreferredOverCallerMemberName_Title", resourceCulture);

	internal static string WRN_CallerLineNumberParamForUnconsumedLocation => ResourceManager.GetString("WRN_CallerLineNumberParamForUnconsumedLocation", resourceCulture);

	internal static string WRN_CallerLineNumberParamForUnconsumedLocation_Title => ResourceManager.GetString("WRN_CallerLineNumberParamForUnconsumedLocation_Title", resourceCulture);

	internal static string WRN_CallerLineNumberPreferredOverCallerFilePath => ResourceManager.GetString("WRN_CallerLineNumberPreferredOverCallerFilePath", resourceCulture);

	internal static string WRN_CallerLineNumberPreferredOverCallerFilePath_Title => ResourceManager.GetString("WRN_CallerLineNumberPreferredOverCallerFilePath_Title", resourceCulture);

	internal static string WRN_CallerLineNumberPreferredOverCallerMemberName => ResourceManager.GetString("WRN_CallerLineNumberPreferredOverCallerMemberName", resourceCulture);

	internal static string WRN_CallerLineNumberPreferredOverCallerMemberName_Title => ResourceManager.GetString("WRN_CallerLineNumberPreferredOverCallerMemberName_Title", resourceCulture);

	internal static string WRN_CallerMemberNameParamForUnconsumedLocation => ResourceManager.GetString("WRN_CallerMemberNameParamForUnconsumedLocation", resourceCulture);

	internal static string WRN_CallerMemberNameParamForUnconsumedLocation_Title => ResourceManager.GetString("WRN_CallerMemberNameParamForUnconsumedLocation_Title", resourceCulture);

	internal static string WRN_CallOnNonAgileField => ResourceManager.GetString("WRN_CallOnNonAgileField", resourceCulture);

	internal static string WRN_CallOnNonAgileField_Description => ResourceManager.GetString("WRN_CallOnNonAgileField_Description", resourceCulture);

	internal static string WRN_CallOnNonAgileField_Title => ResourceManager.GetString("WRN_CallOnNonAgileField_Title", resourceCulture);

	internal static string WRN_CantHaveManifestForModule => ResourceManager.GetString("WRN_CantHaveManifestForModule", resourceCulture);

	internal static string WRN_CantHaveManifestForModule_Title => ResourceManager.GetString("WRN_CantHaveManifestForModule_Title", resourceCulture);

	internal static string WRN_CLS_ArrayArgumentToAttribute => ResourceManager.GetString("WRN_CLS_ArrayArgumentToAttribute", resourceCulture);

	internal static string WRN_CLS_ArrayArgumentToAttribute_Title => ResourceManager.GetString("WRN_CLS_ArrayArgumentToAttribute_Title", resourceCulture);

	internal static string WRN_CLS_AssemblyNotCLS => ResourceManager.GetString("WRN_CLS_AssemblyNotCLS", resourceCulture);

	internal static string WRN_CLS_AssemblyNotCLS_Title => ResourceManager.GetString("WRN_CLS_AssemblyNotCLS_Title", resourceCulture);

	internal static string WRN_CLS_AssemblyNotCLS2 => ResourceManager.GetString("WRN_CLS_AssemblyNotCLS2", resourceCulture);

	internal static string WRN_CLS_AssemblyNotCLS2_Title => ResourceManager.GetString("WRN_CLS_AssemblyNotCLS2_Title", resourceCulture);

	internal static string WRN_CLS_BadArgType => ResourceManager.GetString("WRN_CLS_BadArgType", resourceCulture);

	internal static string WRN_CLS_BadArgType_Title => ResourceManager.GetString("WRN_CLS_BadArgType_Title", resourceCulture);

	internal static string WRN_CLS_BadAttributeType => ResourceManager.GetString("WRN_CLS_BadAttributeType", resourceCulture);

	internal static string WRN_CLS_BadAttributeType_Title => ResourceManager.GetString("WRN_CLS_BadAttributeType_Title", resourceCulture);

	internal static string WRN_CLS_BadBase => ResourceManager.GetString("WRN_CLS_BadBase", resourceCulture);

	internal static string WRN_CLS_BadBase_Description => ResourceManager.GetString("WRN_CLS_BadBase_Description", resourceCulture);

	internal static string WRN_CLS_BadBase_Title => ResourceManager.GetString("WRN_CLS_BadBase_Title", resourceCulture);

	internal static string WRN_CLS_BadFieldPropType => ResourceManager.GetString("WRN_CLS_BadFieldPropType", resourceCulture);

	internal static string WRN_CLS_BadFieldPropType_Description => ResourceManager.GetString("WRN_CLS_BadFieldPropType_Description", resourceCulture);

	internal static string WRN_CLS_BadFieldPropType_Title => ResourceManager.GetString("WRN_CLS_BadFieldPropType_Title", resourceCulture);

	internal static string WRN_CLS_BadIdentifier => ResourceManager.GetString("WRN_CLS_BadIdentifier", resourceCulture);

	internal static string WRN_CLS_BadIdentifier_Title => ResourceManager.GetString("WRN_CLS_BadIdentifier_Title", resourceCulture);

	internal static string WRN_CLS_BadIdentifierCase => ResourceManager.GetString("WRN_CLS_BadIdentifierCase", resourceCulture);

	internal static string WRN_CLS_BadIdentifierCase_Title => ResourceManager.GetString("WRN_CLS_BadIdentifierCase_Title", resourceCulture);

	internal static string WRN_CLS_BadInterface => ResourceManager.GetString("WRN_CLS_BadInterface", resourceCulture);

	internal static string WRN_CLS_BadInterface_Title => ResourceManager.GetString("WRN_CLS_BadInterface_Title", resourceCulture);

	internal static string WRN_CLS_BadInterfaceMember => ResourceManager.GetString("WRN_CLS_BadInterfaceMember", resourceCulture);

	internal static string WRN_CLS_BadInterfaceMember_Title => ResourceManager.GetString("WRN_CLS_BadInterfaceMember_Title", resourceCulture);

	internal static string WRN_CLS_BadReturnType => ResourceManager.GetString("WRN_CLS_BadReturnType", resourceCulture);

	internal static string WRN_CLS_BadReturnType_Title => ResourceManager.GetString("WRN_CLS_BadReturnType_Title", resourceCulture);

	internal static string WRN_CLS_BadTypeVar => ResourceManager.GetString("WRN_CLS_BadTypeVar", resourceCulture);

	internal static string WRN_CLS_BadTypeVar_Title => ResourceManager.GetString("WRN_CLS_BadTypeVar_Title", resourceCulture);

	internal static string WRN_CLS_IllegalTrueInFalse => ResourceManager.GetString("WRN_CLS_IllegalTrueInFalse", resourceCulture);

	internal static string WRN_CLS_IllegalTrueInFalse_Title => ResourceManager.GetString("WRN_CLS_IllegalTrueInFalse_Title", resourceCulture);

	internal static string WRN_CLS_MeaninglessOnParam => ResourceManager.GetString("WRN_CLS_MeaninglessOnParam", resourceCulture);

	internal static string WRN_CLS_MeaninglessOnParam_Title => ResourceManager.GetString("WRN_CLS_MeaninglessOnParam_Title", resourceCulture);

	internal static string WRN_CLS_MeaninglessOnPrivateType => ResourceManager.GetString("WRN_CLS_MeaninglessOnPrivateType", resourceCulture);

	internal static string WRN_CLS_MeaninglessOnPrivateType_Title => ResourceManager.GetString("WRN_CLS_MeaninglessOnPrivateType_Title", resourceCulture);

	internal static string WRN_CLS_MeaninglessOnReturn => ResourceManager.GetString("WRN_CLS_MeaninglessOnReturn", resourceCulture);

	internal static string WRN_CLS_MeaninglessOnReturn_Title => ResourceManager.GetString("WRN_CLS_MeaninglessOnReturn_Title", resourceCulture);

	internal static string WRN_CLS_ModuleMissingCLS => ResourceManager.GetString("WRN_CLS_ModuleMissingCLS", resourceCulture);

	internal static string WRN_CLS_ModuleMissingCLS_Title => ResourceManager.GetString("WRN_CLS_ModuleMissingCLS_Title", resourceCulture);

	internal static string WRN_CLS_NoAbstractMembers => ResourceManager.GetString("WRN_CLS_NoAbstractMembers", resourceCulture);

	internal static string WRN_CLS_NoAbstractMembers_Title => ResourceManager.GetString("WRN_CLS_NoAbstractMembers_Title", resourceCulture);

	internal static string WRN_CLS_NotOnModules => ResourceManager.GetString("WRN_CLS_NotOnModules", resourceCulture);

	internal static string WRN_CLS_NotOnModules_Title => ResourceManager.GetString("WRN_CLS_NotOnModules_Title", resourceCulture);

	internal static string WRN_CLS_NotOnModules2 => ResourceManager.GetString("WRN_CLS_NotOnModules2", resourceCulture);

	internal static string WRN_CLS_NotOnModules2_Title => ResourceManager.GetString("WRN_CLS_NotOnModules2_Title", resourceCulture);

	internal static string WRN_CLS_NoVarArgs => ResourceManager.GetString("WRN_CLS_NoVarArgs", resourceCulture);

	internal static string WRN_CLS_NoVarArgs_Title => ResourceManager.GetString("WRN_CLS_NoVarArgs_Title", resourceCulture);

	internal static string WRN_CLS_OverloadRefOut => ResourceManager.GetString("WRN_CLS_OverloadRefOut", resourceCulture);

	internal static string WRN_CLS_OverloadRefOut_Title => ResourceManager.GetString("WRN_CLS_OverloadRefOut_Title", resourceCulture);

	internal static string WRN_CLS_OverloadUnnamed => ResourceManager.GetString("WRN_CLS_OverloadUnnamed", resourceCulture);

	internal static string WRN_CLS_OverloadUnnamed_Description => ResourceManager.GetString("WRN_CLS_OverloadUnnamed_Description", resourceCulture);

	internal static string WRN_CLS_OverloadUnnamed_Title => ResourceManager.GetString("WRN_CLS_OverloadUnnamed_Title", resourceCulture);

	internal static string WRN_CLS_VolatileField => ResourceManager.GetString("WRN_CLS_VolatileField", resourceCulture);

	internal static string WRN_CLS_VolatileField_Title => ResourceManager.GetString("WRN_CLS_VolatileField_Title", resourceCulture);

	internal static string WRN_CmdOptionConflictsSource => ResourceManager.GetString("WRN_CmdOptionConflictsSource", resourceCulture);

	internal static string WRN_CmdOptionConflictsSource_Description => ResourceManager.GetString("WRN_CmdOptionConflictsSource_Description", resourceCulture);

	internal static string WRN_CmdOptionConflictsSource_Title => ResourceManager.GetString("WRN_CmdOptionConflictsSource_Title", resourceCulture);

	internal static string WRN_CmpAlwaysFalse => ResourceManager.GetString("WRN_CmpAlwaysFalse", resourceCulture);

	internal static string WRN_CmpAlwaysFalse_Title => ResourceManager.GetString("WRN_CmpAlwaysFalse_Title", resourceCulture);

	internal static string WRN_CoClassWithoutComImport => ResourceManager.GetString("WRN_CoClassWithoutComImport", resourceCulture);

	internal static string WRN_CoClassWithoutComImport_Title => ResourceManager.GetString("WRN_CoClassWithoutComImport_Title", resourceCulture);

	internal static string WRN_ComparisonToSelf => ResourceManager.GetString("WRN_ComparisonToSelf", resourceCulture);

	internal static string WRN_ComparisonToSelf_Title => ResourceManager.GetString("WRN_ComparisonToSelf_Title", resourceCulture);

	internal static string WRN_ConflictingChecksum => ResourceManager.GetString("WRN_ConflictingChecksum", resourceCulture);

	internal static string WRN_ConflictingChecksum_Title => ResourceManager.GetString("WRN_ConflictingChecksum_Title", resourceCulture);

	internal static string WRN_ConflictingMachineAssembly => ResourceManager.GetString("WRN_ConflictingMachineAssembly", resourceCulture);

	internal static string WRN_ConflictingMachineAssembly_Title => ResourceManager.GetString("WRN_ConflictingMachineAssembly_Title", resourceCulture);

	internal static string WRN_DebugFullNameTooLong => ResourceManager.GetString("WRN_DebugFullNameTooLong", resourceCulture);

	internal static string WRN_DebugFullNameTooLong_Title => ResourceManager.GetString("WRN_DebugFullNameTooLong_Title", resourceCulture);

	internal static string WRN_DefaultValueForUnconsumedLocation => ResourceManager.GetString("WRN_DefaultValueForUnconsumedLocation", resourceCulture);

	internal static string WRN_DefaultValueForUnconsumedLocation_Title => ResourceManager.GetString("WRN_DefaultValueForUnconsumedLocation_Title", resourceCulture);

	internal static string WRN_DefineIdentifierRequired => ResourceManager.GetString("WRN_DefineIdentifierRequired", resourceCulture);

	internal static string WRN_DefineIdentifierRequired_Title => ResourceManager.GetString("WRN_DefineIdentifierRequired_Title", resourceCulture);

	internal static string WRN_DelaySignButNoKey => ResourceManager.GetString("WRN_DelaySignButNoKey", resourceCulture);

	internal static string WRN_DelaySignButNoKey_Title => ResourceManager.GetString("WRN_DelaySignButNoKey_Title", resourceCulture);

	internal static string WRN_DeprecatedCollectionInitAdd => ResourceManager.GetString("WRN_DeprecatedCollectionInitAdd", resourceCulture);

	internal static string WRN_DeprecatedCollectionInitAdd_Title => ResourceManager.GetString("WRN_DeprecatedCollectionInitAdd_Title", resourceCulture);

	internal static string WRN_DeprecatedCollectionInitAddStr => ResourceManager.GetString("WRN_DeprecatedCollectionInitAddStr", resourceCulture);

	internal static string WRN_DeprecatedCollectionInitAddStr_Title => ResourceManager.GetString("WRN_DeprecatedCollectionInitAddStr_Title", resourceCulture);

	internal static string WRN_DeprecatedSymbol => ResourceManager.GetString("WRN_DeprecatedSymbol", resourceCulture);

	internal static string WRN_DeprecatedSymbol_Title => ResourceManager.GetString("WRN_DeprecatedSymbol_Title", resourceCulture);

	internal static string WRN_DeprecatedSymbolStr => ResourceManager.GetString("WRN_DeprecatedSymbolStr", resourceCulture);

	internal static string WRN_DeprecatedSymbolStr_Title => ResourceManager.GetString("WRN_DeprecatedSymbolStr_Title", resourceCulture);

	internal static string WRN_DotOnDefault => ResourceManager.GetString("WRN_DotOnDefault", resourceCulture);

	internal static string WRN_DotOnDefault_Title => ResourceManager.GetString("WRN_DotOnDefault_Title", resourceCulture);

	internal static string WRN_DuplicateParamTag => ResourceManager.GetString("WRN_DuplicateParamTag", resourceCulture);

	internal static string WRN_DuplicateParamTag_Title => ResourceManager.GetString("WRN_DuplicateParamTag_Title", resourceCulture);

	internal static string WRN_DuplicateTypeParamTag => ResourceManager.GetString("WRN_DuplicateTypeParamTag", resourceCulture);

	internal static string WRN_DuplicateTypeParamTag_Title => ResourceManager.GetString("WRN_DuplicateTypeParamTag_Title", resourceCulture);

	internal static string WRN_DuplicateUsing => ResourceManager.GetString("WRN_DuplicateUsing", resourceCulture);

	internal static string WRN_DuplicateUsing_Title => ResourceManager.GetString("WRN_DuplicateUsing_Title", resourceCulture);

	internal static string WRN_DynamicDispatchToConditionalMethod => ResourceManager.GetString("WRN_DynamicDispatchToConditionalMethod", resourceCulture);

	internal static string WRN_DynamicDispatchToConditionalMethod_Title => ResourceManager.GetString("WRN_DynamicDispatchToConditionalMethod_Title", resourceCulture);

	internal static string WRN_EmptySwitch => ResourceManager.GetString("WRN_EmptySwitch", resourceCulture);

	internal static string WRN_EmptySwitch_Title => ResourceManager.GetString("WRN_EmptySwitch_Title", resourceCulture);

	internal static string WRN_EndOfPPLineExpected => ResourceManager.GetString("WRN_EndOfPPLineExpected", resourceCulture);

	internal static string WRN_EndOfPPLineExpected_Title => ResourceManager.GetString("WRN_EndOfPPLineExpected_Title", resourceCulture);

	internal static string WRN_EqualityOpWithoutEquals => ResourceManager.GetString("WRN_EqualityOpWithoutEquals", resourceCulture);

	internal static string WRN_EqualityOpWithoutEquals_Title => ResourceManager.GetString("WRN_EqualityOpWithoutEquals_Title", resourceCulture);

	internal static string WRN_EqualityOpWithoutGetHashCode => ResourceManager.GetString("WRN_EqualityOpWithoutGetHashCode", resourceCulture);

	internal static string WRN_EqualityOpWithoutGetHashCode_Title => ResourceManager.GetString("WRN_EqualityOpWithoutGetHashCode_Title", resourceCulture);

	internal static string WRN_EqualsWithoutGetHashCode => ResourceManager.GetString("WRN_EqualsWithoutGetHashCode", resourceCulture);

	internal static string WRN_EqualsWithoutGetHashCode_Title => ResourceManager.GetString("WRN_EqualsWithoutGetHashCode_Title", resourceCulture);

	internal static string WRN_ErrorOverride => ResourceManager.GetString("WRN_ErrorOverride", resourceCulture);

	internal static string WRN_ErrorOverride_Description => ResourceManager.GetString("WRN_ErrorOverride_Description", resourceCulture);

	internal static string WRN_ErrorOverride_Title => ResourceManager.GetString("WRN_ErrorOverride_Title", resourceCulture);

	internal static string WRN_Experimental => ResourceManager.GetString("WRN_Experimental", resourceCulture);

	internal static string WRN_Experimental_Title => ResourceManager.GetString("WRN_Experimental_Title", resourceCulture);

	internal static string WRN_ExplicitImplCollision => ResourceManager.GetString("WRN_ExplicitImplCollision", resourceCulture);

	internal static string WRN_ExplicitImplCollision_Title => ResourceManager.GetString("WRN_ExplicitImplCollision_Title", resourceCulture);

	internal static string WRN_ExternCtorNoImplementation => ResourceManager.GetString("WRN_ExternCtorNoImplementation", resourceCulture);

	internal static string WRN_ExternCtorNoImplementation_Title => ResourceManager.GetString("WRN_ExternCtorNoImplementation_Title", resourceCulture);

	internal static string WRN_ExternMethodNoImplementation => ResourceManager.GetString("WRN_ExternMethodNoImplementation", resourceCulture);

	internal static string WRN_ExternMethodNoImplementation_Title => ResourceManager.GetString("WRN_ExternMethodNoImplementation_Title", resourceCulture);

	internal static string WRN_FailedInclude => ResourceManager.GetString("WRN_FailedInclude", resourceCulture);

	internal static string WRN_FailedInclude_Title => ResourceManager.GetString("WRN_FailedInclude_Title", resourceCulture);

	internal static string WRN_FileAlreadyIncluded => ResourceManager.GetString("WRN_FileAlreadyIncluded", resourceCulture);

	internal static string WRN_FileAlreadyIncluded_Title => ResourceManager.GetString("WRN_FileAlreadyIncluded_Title", resourceCulture);

	internal static string WRN_FileNameTooLong => ResourceManager.GetString("WRN_FileNameTooLong", resourceCulture);

	internal static string WRN_FileNameTooLong_Title => ResourceManager.GetString("WRN_FileNameTooLong_Title", resourceCulture);

	internal static string WRN_FilterIsConstantFalse => ResourceManager.GetString("WRN_FilterIsConstantFalse", resourceCulture);

	internal static string WRN_FilterIsConstantFalse_Title => ResourceManager.GetString("WRN_FilterIsConstantFalse_Title", resourceCulture);

	internal static string WRN_FilterIsConstantFalseRedundantTryCatch => ResourceManager.GetString("WRN_FilterIsConstantFalseRedundantTryCatch", resourceCulture);

	internal static string WRN_FilterIsConstantFalseRedundantTryCatch_Title => ResourceManager.GetString("WRN_FilterIsConstantFalseRedundantTryCatch_Title", resourceCulture);

	internal static string WRN_FilterIsConstantTrue => ResourceManager.GetString("WRN_FilterIsConstantTrue", resourceCulture);

	internal static string WRN_FilterIsConstantTrue_Title => ResourceManager.GetString("WRN_FilterIsConstantTrue_Title", resourceCulture);

	internal static string WRN_FinalizeMethod => ResourceManager.GetString("WRN_FinalizeMethod", resourceCulture);

	internal static string WRN_FinalizeMethod_Description => ResourceManager.GetString("WRN_FinalizeMethod_Description", resourceCulture);

	internal static string WRN_FinalizeMethod_Title => ResourceManager.GetString("WRN_FinalizeMethod_Title", resourceCulture);

	internal static string WRN_GlobalAliasDefn => ResourceManager.GetString("WRN_GlobalAliasDefn", resourceCulture);

	internal static string WRN_GlobalAliasDefn_Title => ResourceManager.GetString("WRN_GlobalAliasDefn_Title", resourceCulture);

	internal static string WRN_GotoCaseShouldConvert => ResourceManager.GetString("WRN_GotoCaseShouldConvert", resourceCulture);

	internal static string WRN_GotoCaseShouldConvert_Title => ResourceManager.GetString("WRN_GotoCaseShouldConvert_Title", resourceCulture);

	internal static string WRN_IdentifierOrNumericLiteralExpected => ResourceManager.GetString("WRN_IdentifierOrNumericLiteralExpected", resourceCulture);

	internal static string WRN_IdentifierOrNumericLiteralExpected_Title => ResourceManager.GetString("WRN_IdentifierOrNumericLiteralExpected_Title", resourceCulture);

	internal static string WRN_IllegalPPChecksum => ResourceManager.GetString("WRN_IllegalPPChecksum", resourceCulture);

	internal static string WRN_IllegalPPChecksum_Title => ResourceManager.GetString("WRN_IllegalPPChecksum_Title", resourceCulture);

	internal static string WRN_IllegalPPWarning => ResourceManager.GetString("WRN_IllegalPPWarning", resourceCulture);

	internal static string WRN_IllegalPPWarning_Title => ResourceManager.GetString("WRN_IllegalPPWarning_Title", resourceCulture);

	internal static string WRN_IllegalPragma => ResourceManager.GetString("WRN_IllegalPragma", resourceCulture);

	internal static string WRN_IllegalPragma_Title => ResourceManager.GetString("WRN_IllegalPragma_Title", resourceCulture);

	internal static string WRN_IncorrectBooleanAssg => ResourceManager.GetString("WRN_IncorrectBooleanAssg", resourceCulture);

	internal static string WRN_IncorrectBooleanAssg_Title => ResourceManager.GetString("WRN_IncorrectBooleanAssg_Title", resourceCulture);

	internal static string WRN_InvalidAssemblyName => ResourceManager.GetString("WRN_InvalidAssemblyName", resourceCulture);

	internal static string WRN_InvalidAssemblyName_Description => ResourceManager.GetString("WRN_InvalidAssemblyName_Description", resourceCulture);

	internal static string WRN_InvalidAssemblyName_Title => ResourceManager.GetString("WRN_InvalidAssemblyName_Title", resourceCulture);

	internal static string WRN_InvalidAttributeLocation => ResourceManager.GetString("WRN_InvalidAttributeLocation", resourceCulture);

	internal static string WRN_InvalidAttributeLocation_Title => ResourceManager.GetString("WRN_InvalidAttributeLocation_Title", resourceCulture);

	internal static string WRN_InvalidInclude => ResourceManager.GetString("WRN_InvalidInclude", resourceCulture);

	internal static string WRN_InvalidInclude_Title => ResourceManager.GetString("WRN_InvalidInclude_Title", resourceCulture);

	internal static string WRN_InvalidMainSig => ResourceManager.GetString("WRN_InvalidMainSig", resourceCulture);

	internal static string WRN_InvalidMainSig_Title => ResourceManager.GetString("WRN_InvalidMainSig_Title", resourceCulture);

	internal static string WRN_InvalidNumber => ResourceManager.GetString("WRN_InvalidNumber", resourceCulture);

	internal static string WRN_InvalidNumber_Title => ResourceManager.GetString("WRN_InvalidNumber_Title", resourceCulture);

	internal static string WRN_InvalidSearchPathDir => ResourceManager.GetString("WRN_InvalidSearchPathDir", resourceCulture);

	internal static string WRN_InvalidSearchPathDir_Title => ResourceManager.GetString("WRN_InvalidSearchPathDir_Title", resourceCulture);

	internal static string WRN_InvalidVersionFormat => ResourceManager.GetString("WRN_InvalidVersionFormat", resourceCulture);

	internal static string WRN_InvalidVersionFormat_Title => ResourceManager.GetString("WRN_InvalidVersionFormat_Title", resourceCulture);

	internal static string WRN_IsAlwaysFalse => ResourceManager.GetString("WRN_IsAlwaysFalse", resourceCulture);

	internal static string WRN_IsAlwaysFalse_Title => ResourceManager.GetString("WRN_IsAlwaysFalse_Title", resourceCulture);

	internal static string WRN_IsAlwaysTrue => ResourceManager.GetString("WRN_IsAlwaysTrue", resourceCulture);

	internal static string WRN_IsAlwaysTrue_Title => ResourceManager.GetString("WRN_IsAlwaysTrue_Title", resourceCulture);

	internal static string WRN_IsDynamicIsConfusing => ResourceManager.GetString("WRN_IsDynamicIsConfusing", resourceCulture);

	internal static string WRN_IsDynamicIsConfusing_Title => ResourceManager.GetString("WRN_IsDynamicIsConfusing_Title", resourceCulture);

	internal static string WRN_LowercaseEllSuffix => ResourceManager.GetString("WRN_LowercaseEllSuffix", resourceCulture);

	internal static string WRN_LowercaseEllSuffix_Title => ResourceManager.GetString("WRN_LowercaseEllSuffix_Title", resourceCulture);

	internal static string WRN_MainCantBeGeneric => ResourceManager.GetString("WRN_MainCantBeGeneric", resourceCulture);

	internal static string WRN_MainCantBeGeneric_Title => ResourceManager.GetString("WRN_MainCantBeGeneric_Title", resourceCulture);

	internal static string WRN_MainIgnored => ResourceManager.GetString("WRN_MainIgnored", resourceCulture);

	internal static string WRN_MainIgnored_Title => ResourceManager.GetString("WRN_MainIgnored_Title", resourceCulture);

	internal static string WRN_MissingParamTag => ResourceManager.GetString("WRN_MissingParamTag", resourceCulture);

	internal static string WRN_MissingParamTag_Title => ResourceManager.GetString("WRN_MissingParamTag_Title", resourceCulture);

	internal static string WRN_MissingTypeParamTag => ResourceManager.GetString("WRN_MissingTypeParamTag", resourceCulture);

	internal static string WRN_MissingTypeParamTag_Title => ResourceManager.GetString("WRN_MissingTypeParamTag_Title", resourceCulture);

	internal static string WRN_MissingXMLComment => ResourceManager.GetString("WRN_MissingXMLComment", resourceCulture);

	internal static string WRN_MissingXMLComment_Description => ResourceManager.GetString("WRN_MissingXMLComment_Description", resourceCulture);

	internal static string WRN_MissingXMLComment_Title => ResourceManager.GetString("WRN_MissingXMLComment_Title", resourceCulture);

	internal static string WRN_MultiplePredefTypes => ResourceManager.GetString("WRN_MultiplePredefTypes", resourceCulture);

	internal static string WRN_MultiplePredefTypes_Description => ResourceManager.GetString("WRN_MultiplePredefTypes_Description", resourceCulture);

	internal static string WRN_MultiplePredefTypes_Title => ResourceManager.GetString("WRN_MultiplePredefTypes_Title", resourceCulture);

	internal static string WRN_MultipleRuntimeImplementationMatches => ResourceManager.GetString("WRN_MultipleRuntimeImplementationMatches", resourceCulture);

	internal static string WRN_MultipleRuntimeImplementationMatches_Description => ResourceManager.GetString("WRN_MultipleRuntimeImplementationMatches_Description", resourceCulture);

	internal static string WRN_MultipleRuntimeImplementationMatches_Title => ResourceManager.GetString("WRN_MultipleRuntimeImplementationMatches_Title", resourceCulture);

	internal static string WRN_MultipleRuntimeOverrideMatches => ResourceManager.GetString("WRN_MultipleRuntimeOverrideMatches", resourceCulture);

	internal static string WRN_MultipleRuntimeOverrideMatches_Title => ResourceManager.GetString("WRN_MultipleRuntimeOverrideMatches_Title", resourceCulture);

	internal static string WRN_NegativeArrayIndex => ResourceManager.GetString("WRN_NegativeArrayIndex", resourceCulture);

	internal static string WRN_NegativeArrayIndex_Title => ResourceManager.GetString("WRN_NegativeArrayIndex_Title", resourceCulture);

	internal static string WRN_NewNotRequired => ResourceManager.GetString("WRN_NewNotRequired", resourceCulture);

	internal static string WRN_NewNotRequired_Title => ResourceManager.GetString("WRN_NewNotRequired_Title", resourceCulture);

	internal static string WRN_NewOrOverrideExpected => ResourceManager.GetString("WRN_NewOrOverrideExpected", resourceCulture);

	internal static string WRN_NewOrOverrideExpected_Title => ResourceManager.GetString("WRN_NewOrOverrideExpected_Title", resourceCulture);

	internal static string WRN_NewRequired => ResourceManager.GetString("WRN_NewRequired", resourceCulture);

	internal static string WRN_NewRequired_Description => ResourceManager.GetString("WRN_NewRequired_Description", resourceCulture);

	internal static string WRN_NewRequired_Title => ResourceManager.GetString("WRN_NewRequired_Title", resourceCulture);

	internal static string WRN_NoAnalyzerInAssembly => ResourceManager.GetString("WRN_NoAnalyzerInAssembly", resourceCulture);

	internal static string WRN_NoAnalyzerInAssembly_Title => ResourceManager.GetString("WRN_NoAnalyzerInAssembly_Title", resourceCulture);

	internal static string WRN_NoConfigNotOnCommandLine => ResourceManager.GetString("WRN_NoConfigNotOnCommandLine", resourceCulture);

	internal static string WRN_NoConfigNotOnCommandLine_Title => ResourceManager.GetString("WRN_NoConfigNotOnCommandLine_Title", resourceCulture);

	internal static string WRN_NonECMAFeature => ResourceManager.GetString("WRN_NonECMAFeature", resourceCulture);

	internal static string WRN_NonECMAFeature_Title => ResourceManager.GetString("WRN_NonECMAFeature_Title", resourceCulture);

	internal static string WRN_NonObsoleteOverridingObsolete => ResourceManager.GetString("WRN_NonObsoleteOverridingObsolete", resourceCulture);

	internal static string WRN_NonObsoleteOverridingObsolete_Title => ResourceManager.GetString("WRN_NonObsoleteOverridingObsolete_Title", resourceCulture);

	internal static string WRN_NoRuntimeMetadataVersion => ResourceManager.GetString("WRN_NoRuntimeMetadataVersion", resourceCulture);

	internal static string WRN_NoRuntimeMetadataVersion_Title => ResourceManager.GetString("WRN_NoRuntimeMetadataVersion_Title", resourceCulture);

	internal static string WRN_NoSources => ResourceManager.GetString("WRN_NoSources", resourceCulture);

	internal static string WRN_NoSources_Title => ResourceManager.GetString("WRN_NoSources_Title", resourceCulture);

	internal static string WRN_NubExprIsConstBool => ResourceManager.GetString("WRN_NubExprIsConstBool", resourceCulture);

	internal static string WRN_NubExprIsConstBool_Title => ResourceManager.GetString("WRN_NubExprIsConstBool_Title", resourceCulture);

	internal static string WRN_NubExprIsConstBool2 => ResourceManager.GetString("WRN_NubExprIsConstBool2", resourceCulture);

	internal static string WRN_NubExprIsConstBool2_Title => ResourceManager.GetString("WRN_NubExprIsConstBool2_Title", resourceCulture);

	internal static string WRN_ObsoleteOverridingNonObsolete => ResourceManager.GetString("WRN_ObsoleteOverridingNonObsolete", resourceCulture);

	internal static string WRN_ObsoleteOverridingNonObsolete_Title => ResourceManager.GetString("WRN_ObsoleteOverridingNonObsolete_Title", resourceCulture);

	internal static string WRN_PatternBadSignature => ResourceManager.GetString("WRN_PatternBadSignature", resourceCulture);

	internal static string WRN_PatternBadSignature_Title => ResourceManager.GetString("WRN_PatternBadSignature_Title", resourceCulture);

	internal static string WRN_PatternIsAmbiguous => ResourceManager.GetString("WRN_PatternIsAmbiguous", resourceCulture);

	internal static string WRN_PatternIsAmbiguous_Title => ResourceManager.GetString("WRN_PatternIsAmbiguous_Title", resourceCulture);

	internal static string WRN_PatternStaticOrInaccessible => ResourceManager.GetString("WRN_PatternStaticOrInaccessible", resourceCulture);

	internal static string WRN_PatternStaticOrInaccessible_Title => ResourceManager.GetString("WRN_PatternStaticOrInaccessible_Title", resourceCulture);

	internal static string WRN_PdbLocalNameTooLong => ResourceManager.GetString("WRN_PdbLocalNameTooLong", resourceCulture);

	internal static string WRN_PdbLocalNameTooLong_Title => ResourceManager.GetString("WRN_PdbLocalNameTooLong_Title", resourceCulture);

	internal static string WRN_PossibleMistakenNullStatement => ResourceManager.GetString("WRN_PossibleMistakenNullStatement", resourceCulture);

	internal static string WRN_PossibleMistakenNullStatement_Title => ResourceManager.GetString("WRN_PossibleMistakenNullStatement_Title", resourceCulture);

	internal static string WRN_ProtectedInSealed => ResourceManager.GetString("WRN_ProtectedInSealed", resourceCulture);

	internal static string WRN_ProtectedInSealed_Title => ResourceManager.GetString("WRN_ProtectedInSealed_Title", resourceCulture);

	internal static string WRN_RefCultureMismatch => ResourceManager.GetString("WRN_RefCultureMismatch", resourceCulture);

	internal static string WRN_RefCultureMismatch_Title => ResourceManager.GetString("WRN_RefCultureMismatch_Title", resourceCulture);

	internal static string WRN_ReferencedAssemblyDoesNotHaveStrongName => ResourceManager.GetString("WRN_ReferencedAssemblyDoesNotHaveStrongName", resourceCulture);

	internal static string WRN_ReferencedAssemblyDoesNotHaveStrongName_Title => ResourceManager.GetString("WRN_ReferencedAssemblyDoesNotHaveStrongName_Title", resourceCulture);

	internal static string WRN_ReferencedAssemblyReferencesLinkedPIA => ResourceManager.GetString("WRN_ReferencedAssemblyReferencesLinkedPIA", resourceCulture);

	internal static string WRN_ReferencedAssemblyReferencesLinkedPIA_Description => ResourceManager.GetString("WRN_ReferencedAssemblyReferencesLinkedPIA_Description", resourceCulture);

	internal static string WRN_ReferencedAssemblyReferencesLinkedPIA_Title => ResourceManager.GetString("WRN_ReferencedAssemblyReferencesLinkedPIA_Title", resourceCulture);

	internal static string WRN_SameFullNameThisAggAgg => ResourceManager.GetString("WRN_SameFullNameThisAggAgg", resourceCulture);

	internal static string WRN_SameFullNameThisAggAgg_Title => ResourceManager.GetString("WRN_SameFullNameThisAggAgg_Title", resourceCulture);

	internal static string WRN_SameFullNameThisAggNs => ResourceManager.GetString("WRN_SameFullNameThisAggNs", resourceCulture);

	internal static string WRN_SameFullNameThisAggNs_Title => ResourceManager.GetString("WRN_SameFullNameThisAggNs_Title", resourceCulture);

	internal static string WRN_SameFullNameThisNsAgg => ResourceManager.GetString("WRN_SameFullNameThisNsAgg", resourceCulture);

	internal static string WRN_SameFullNameThisNsAgg_Title => ResourceManager.GetString("WRN_SameFullNameThisNsAgg_Title", resourceCulture);

	internal static string WRN_SequentialOnPartialClass => ResourceManager.GetString("WRN_SequentialOnPartialClass", resourceCulture);

	internal static string WRN_SequentialOnPartialClass_Title => ResourceManager.GetString("WRN_SequentialOnPartialClass_Title", resourceCulture);

	internal static string WRN_TooManyLinesForDebugger => ResourceManager.GetString("WRN_TooManyLinesForDebugger", resourceCulture);

	internal static string WRN_TooManyLinesForDebugger_Title => ResourceManager.GetString("WRN_TooManyLinesForDebugger_Title", resourceCulture);

	internal static string WRN_TupleBinopLiteralNameMismatch => ResourceManager.GetString("WRN_TupleBinopLiteralNameMismatch", resourceCulture);

	internal static string WRN_TupleBinopLiteralNameMismatch_Title => ResourceManager.GetString("WRN_TupleBinopLiteralNameMismatch_Title", resourceCulture);

	internal static string WRN_TupleLiteralNameMismatch => ResourceManager.GetString("WRN_TupleLiteralNameMismatch", resourceCulture);

	internal static string WRN_TupleLiteralNameMismatch_Title => ResourceManager.GetString("WRN_TupleLiteralNameMismatch_Title", resourceCulture);

	internal static string WRN_TypeParameterSameAsOuterMethodTypeParameter => ResourceManager.GetString("WRN_TypeParameterSameAsOuterMethodTypeParameter", resourceCulture);

	internal static string WRN_TypeParameterSameAsOuterMethodTypeParameter_Title => ResourceManager.GetString("WRN_TypeParameterSameAsOuterMethodTypeParameter_Title", resourceCulture);

	internal static string WRN_TypeParameterSameAsOuterTypeParameter => ResourceManager.GetString("WRN_TypeParameterSameAsOuterTypeParameter", resourceCulture);

	internal static string WRN_TypeParameterSameAsOuterTypeParameter_Title => ResourceManager.GetString("WRN_TypeParameterSameAsOuterTypeParameter_Title", resourceCulture);

	internal static string WRN_UnableToLoadAnalyzer => ResourceManager.GetString("WRN_UnableToLoadAnalyzer", resourceCulture);

	internal static string WRN_UnableToLoadAnalyzer_Title => ResourceManager.GetString("WRN_UnableToLoadAnalyzer_Title", resourceCulture);

	internal static string WRN_UnassignedInternalField => ResourceManager.GetString("WRN_UnassignedInternalField", resourceCulture);

	internal static string WRN_UnassignedInternalField_Title => ResourceManager.GetString("WRN_UnassignedInternalField_Title", resourceCulture);

	internal static string WRN_UnifyReferenceBldRev => ResourceManager.GetString("WRN_UnifyReferenceBldRev", resourceCulture);

	internal static string WRN_UnifyReferenceBldRev_Description => ResourceManager.GetString("WRN_UnifyReferenceBldRev_Description", resourceCulture);

	internal static string WRN_UnifyReferenceBldRev_Title => ResourceManager.GetString("WRN_UnifyReferenceBldRev_Title", resourceCulture);

	internal static string WRN_UnifyReferenceMajMin => ResourceManager.GetString("WRN_UnifyReferenceMajMin", resourceCulture);

	internal static string WRN_UnifyReferenceMajMin_Description => ResourceManager.GetString("WRN_UnifyReferenceMajMin_Description", resourceCulture);

	internal static string WRN_UnifyReferenceMajMin_Title => ResourceManager.GetString("WRN_UnifyReferenceMajMin_Title", resourceCulture);

	internal static string WRN_UnimplementedCommandLineSwitch => ResourceManager.GetString("WRN_UnimplementedCommandLineSwitch", resourceCulture);

	internal static string WRN_UnimplementedCommandLineSwitch_Title => ResourceManager.GetString("WRN_UnimplementedCommandLineSwitch_Title", resourceCulture);

	internal static string WRN_UnmatchedParamRefTag => ResourceManager.GetString("WRN_UnmatchedParamRefTag", resourceCulture);

	internal static string WRN_UnmatchedParamRefTag_Title => ResourceManager.GetString("WRN_UnmatchedParamRefTag_Title", resourceCulture);

	internal static string WRN_UnmatchedParamTag => ResourceManager.GetString("WRN_UnmatchedParamTag", resourceCulture);

	internal static string WRN_UnmatchedParamTag_Title => ResourceManager.GetString("WRN_UnmatchedParamTag_Title", resourceCulture);

	internal static string WRN_UnmatchedTypeParamRefTag => ResourceManager.GetString("WRN_UnmatchedTypeParamRefTag", resourceCulture);

	internal static string WRN_UnmatchedTypeParamRefTag_Title => ResourceManager.GetString("WRN_UnmatchedTypeParamRefTag_Title", resourceCulture);

	internal static string WRN_UnmatchedTypeParamTag => ResourceManager.GetString("WRN_UnmatchedTypeParamTag", resourceCulture);

	internal static string WRN_UnmatchedTypeParamTag_Title => ResourceManager.GetString("WRN_UnmatchedTypeParamTag_Title", resourceCulture);

	internal static string WRN_UnobservedAwaitableExpression => ResourceManager.GetString("WRN_UnobservedAwaitableExpression", resourceCulture);

	internal static string WRN_UnobservedAwaitableExpression_Description => ResourceManager.GetString("WRN_UnobservedAwaitableExpression_Description", resourceCulture);

	internal static string WRN_UnobservedAwaitableExpression_Title => ResourceManager.GetString("WRN_UnobservedAwaitableExpression_Title", resourceCulture);

	internal static string WRN_UnprocessedXMLComment => ResourceManager.GetString("WRN_UnprocessedXMLComment", resourceCulture);

	internal static string WRN_UnprocessedXMLComment_Title => ResourceManager.GetString("WRN_UnprocessedXMLComment_Title", resourceCulture);

	internal static string WRN_UnqualifiedNestedTypeInCref => ResourceManager.GetString("WRN_UnqualifiedNestedTypeInCref", resourceCulture);

	internal static string WRN_UnqualifiedNestedTypeInCref_Title => ResourceManager.GetString("WRN_UnqualifiedNestedTypeInCref_Title", resourceCulture);

	internal static string WRN_UnreachableCode => ResourceManager.GetString("WRN_UnreachableCode", resourceCulture);

	internal static string WRN_UnreachableCode_Title => ResourceManager.GetString("WRN_UnreachableCode_Title", resourceCulture);

	internal static string WRN_UnreachableGeneralCatch => ResourceManager.GetString("WRN_UnreachableGeneralCatch", resourceCulture);

	internal static string WRN_UnreachableGeneralCatch_Description => ResourceManager.GetString("WRN_UnreachableGeneralCatch_Description", resourceCulture);

	internal static string WRN_UnreachableGeneralCatch_Title => ResourceManager.GetString("WRN_UnreachableGeneralCatch_Title", resourceCulture);

	internal static string WRN_UnreferencedEvent => ResourceManager.GetString("WRN_UnreferencedEvent", resourceCulture);

	internal static string WRN_UnreferencedEvent_Title => ResourceManager.GetString("WRN_UnreferencedEvent_Title", resourceCulture);

	internal static string WRN_UnreferencedField => ResourceManager.GetString("WRN_UnreferencedField", resourceCulture);

	internal static string WRN_UnreferencedField_Title => ResourceManager.GetString("WRN_UnreferencedField_Title", resourceCulture);

	internal static string WRN_UnreferencedFieldAssg => ResourceManager.GetString("WRN_UnreferencedFieldAssg", resourceCulture);

	internal static string WRN_UnreferencedFieldAssg_Title => ResourceManager.GetString("WRN_UnreferencedFieldAssg_Title", resourceCulture);

	internal static string WRN_UnreferencedLabel => ResourceManager.GetString("WRN_UnreferencedLabel", resourceCulture);

	internal static string WRN_UnreferencedLabel_Title => ResourceManager.GetString("WRN_UnreferencedLabel_Title", resourceCulture);

	internal static string WRN_UnreferencedLocalFunction => ResourceManager.GetString("WRN_UnreferencedLocalFunction", resourceCulture);

	internal static string WRN_UnreferencedLocalFunction_Title => ResourceManager.GetString("WRN_UnreferencedLocalFunction_Title", resourceCulture);

	internal static string WRN_UnreferencedVar => ResourceManager.GetString("WRN_UnreferencedVar", resourceCulture);

	internal static string WRN_UnreferencedVar_Title => ResourceManager.GetString("WRN_UnreferencedVar_Title", resourceCulture);

	internal static string WRN_UnreferencedVarAssg => ResourceManager.GetString("WRN_UnreferencedVarAssg", resourceCulture);

	internal static string WRN_UnreferencedVarAssg_Title => ResourceManager.GetString("WRN_UnreferencedVarAssg_Title", resourceCulture);

	internal static string WRN_VacuousIntegralComp => ResourceManager.GetString("WRN_VacuousIntegralComp", resourceCulture);

	internal static string WRN_VacuousIntegralComp_Title => ResourceManager.GetString("WRN_VacuousIntegralComp_Title", resourceCulture);

	internal static string WRN_VolatileByRef => ResourceManager.GetString("WRN_VolatileByRef", resourceCulture);

	internal static string WRN_VolatileByRef_Description => ResourceManager.GetString("WRN_VolatileByRef_Description", resourceCulture);

	internal static string WRN_VolatileByRef_Title => ResourceManager.GetString("WRN_VolatileByRef_Title", resourceCulture);

	internal static string WRN_WarningDirective => ResourceManager.GetString("WRN_WarningDirective", resourceCulture);

	internal static string WRN_WarningDirective_Title => ResourceManager.GetString("WRN_WarningDirective_Title", resourceCulture);

	internal static string WRN_XMLParseError => ResourceManager.GetString("WRN_XMLParseError", resourceCulture);

	internal static string WRN_XMLParseError_Title => ResourceManager.GetString("WRN_XMLParseError_Title", resourceCulture);

	internal static string WRN_XMLParseIncludeError => ResourceManager.GetString("WRN_XMLParseIncludeError", resourceCulture);

	internal static string WRN_XMLParseIncludeError_Title => ResourceManager.GetString("WRN_XMLParseIncludeError_Title", resourceCulture);

	internal static string WrongNumberOfTypeArguments => ResourceManager.GetString("WrongNumberOfTypeArguments", resourceCulture);

	internal static string WrongSemanticModelType => ResourceManager.GetString("WrongSemanticModelType", resourceCulture);

	internal static string XML_CDataEndTagNotAllowed => ResourceManager.GetString("XML_CDataEndTagNotAllowed", resourceCulture);

	internal static string XML_DuplicateAttribute => ResourceManager.GetString("XML_DuplicateAttribute", resourceCulture);

	internal static string XML_ElementTypeMatch => ResourceManager.GetString("XML_ElementTypeMatch", resourceCulture);

	internal static string XML_EndTagExpected => ResourceManager.GetString("XML_EndTagExpected", resourceCulture);

	internal static string XML_EndTagNotExpected => ResourceManager.GetString("XML_EndTagNotExpected", resourceCulture);

	internal static string XML_ExpectedEndOfTag => ResourceManager.GetString("XML_ExpectedEndOfTag", resourceCulture);

	internal static string XML_ExpectedEndOfXml => ResourceManager.GetString("XML_ExpectedEndOfXml", resourceCulture);

	internal static string XML_ExpectedIdentifier => ResourceManager.GetString("XML_ExpectedIdentifier", resourceCulture);

	internal static string XML_IncorrectComment => ResourceManager.GetString("XML_IncorrectComment", resourceCulture);

	internal static string XML_InvalidCharEntity => ResourceManager.GetString("XML_InvalidCharEntity", resourceCulture);

	internal static string XML_InvalidToken => ResourceManager.GetString("XML_InvalidToken", resourceCulture);

	internal static string XML_InvalidUnicodeChar => ResourceManager.GetString("XML_InvalidUnicodeChar", resourceCulture);

	internal static string XML_InvalidWhitespace => ResourceManager.GetString("XML_InvalidWhitespace", resourceCulture);

	internal static string XML_LessThanInAttributeValue => ResourceManager.GetString("XML_LessThanInAttributeValue", resourceCulture);

	internal static string XML_MissingEqualsAttribute => ResourceManager.GetString("XML_MissingEqualsAttribute", resourceCulture);

	internal static string XML_RefUndefinedEntity_1 => ResourceManager.GetString("XML_RefUndefinedEntity_1", resourceCulture);

	internal static string XML_StringLiteralNoEndQuote => ResourceManager.GetString("XML_StringLiteralNoEndQuote", resourceCulture);

	internal static string XML_StringLiteralNonAsciiQuote => ResourceManager.GetString("XML_StringLiteralNonAsciiQuote", resourceCulture);

	internal static string XML_StringLiteralNoStartQuote => ResourceManager.GetString("XML_StringLiteralNoStartQuote", resourceCulture);

	internal static string XML_WhitespaceMissing => ResourceManager.GetString("XML_WhitespaceMissing", resourceCulture);

	internal CSharpResources()
	{
	}
}
