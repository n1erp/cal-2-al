namespace Microsoft.CodeAnalysis.CSharp;

internal enum LambdaConversionResult
{
	Success,
	BadTargetType,
	BadParameterCount,
	MissingSignatureWithOutParameter,
	MismatchedParameterType,
	RefInImplicitlyTypedLambda,
	StaticTypeInImplicitlyTypedLambda,
	ExpressionTreeMustHaveDelegateTypeArgument,
	ExpressionTreeFromAnonymousMethod,
	BindingFailed
}
