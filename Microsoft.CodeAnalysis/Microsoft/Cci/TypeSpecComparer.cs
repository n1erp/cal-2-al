using System.Collections.Generic;

namespace Microsoft.Cci;

internal sealed class TypeSpecComparer : IEqualityComparer<ITypeReference>
{
	private readonly MetadataWriter _metadataWriter;

	internal TypeSpecComparer(MetadataWriter metadataWriter)
	{
		_metadataWriter = metadataWriter;
	}

	public bool Equals(ITypeReference x, ITypeReference y)
	{
		if (x != y)
		{
			return _metadataWriter.GetTypeSpecSignatureIndex(x).Equals(_metadataWriter.GetTypeSpecSignatureIndex(y));
		}
		return true;
	}

	public int GetHashCode(ITypeReference typeReference)
	{
		return _metadataWriter.GetTypeSpecSignatureIndex(typeReference).GetHashCode();
	}
}
