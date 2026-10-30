using System;

namespace Microsoft.CodeAnalysis;

public abstract class Metadata : IDisposable
{
	internal readonly bool IsImageOwner;

	public MetadataId Id { get; }

	public abstract MetadataImageKind Kind { get; }

	internal Metadata(bool isImageOwner, MetadataId id)
	{
		IsImageOwner = isImageOwner;
		Id = id;
	}

	public abstract void Dispose();

	protected abstract Metadata CommonCopy();

	public Metadata Copy()
	{
		return CommonCopy();
	}
}
