namespace Microsoft.Cci;

internal interface IMetadataExpression
{
	ITypeReference Type { get; }

	void Dispatch(MetadataVisitor visitor);
}
