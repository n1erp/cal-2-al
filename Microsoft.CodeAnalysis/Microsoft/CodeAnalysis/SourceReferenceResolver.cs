using System;
using System.IO;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

public abstract class SourceReferenceResolver
{
	public abstract override bool Equals(object other);

	public abstract override int GetHashCode();

	public abstract string NormalizePath(string path, string baseFilePath);

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

	public virtual SourceText ReadText(string resolvedPath)
	{
		using Stream stream = OpenRead(resolvedPath);
		return EncodedStringText.Create(stream);
	}
}
