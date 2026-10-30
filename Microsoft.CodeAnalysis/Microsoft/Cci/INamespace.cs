namespace Microsoft.Cci;

internal interface INamespace : INamedEntity
{
	INamespace ContainingNamespace { get; }
}
