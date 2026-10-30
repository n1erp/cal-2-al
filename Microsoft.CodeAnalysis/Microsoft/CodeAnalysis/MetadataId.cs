namespace Microsoft.CodeAnalysis;

public sealed class MetadataId
{
	private MetadataId()
	{
	}

	internal static MetadataId CreateNewId()
	{
		return new MetadataId();
	}
}
