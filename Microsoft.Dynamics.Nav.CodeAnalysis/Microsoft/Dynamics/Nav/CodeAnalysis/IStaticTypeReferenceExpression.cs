namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IStaticTypeReferenceExpression : IOperation
{
	ITypeSymbol StaticType { get; }
}
