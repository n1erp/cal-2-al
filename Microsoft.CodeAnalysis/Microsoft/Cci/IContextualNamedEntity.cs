namespace Microsoft.Cci;

internal interface IContextualNamedEntity : INamedEntity
{
	void AssociateWithMetadataWriter(MetadataWriter metadataWriter);
}
