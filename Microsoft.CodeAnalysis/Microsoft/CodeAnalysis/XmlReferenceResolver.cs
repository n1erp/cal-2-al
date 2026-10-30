using System;
using System.IO;

namespace Microsoft.CodeAnalysis;

public abstract class XmlReferenceResolver
{
	public abstract override bool Equals(object other);

	public abstract override int GetHashCode();

	public abstract string ResolveReference(string path, string baseFilePath);

	public abstract Stream OpenRead(string resolvedPath);

	internal Stream OpenReadChecked(string fullPath)
	{
		Stream stream = OpenRead(fullPath);
		if (stream == null || !stream.CanRead)
		{
			throw new InvalidOperationException(CodeAnalysisResources.ReferenceResolverShouldReturnReadableNonNullStream);
		}
		return stream;
	}
}
