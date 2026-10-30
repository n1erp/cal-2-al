using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public sealed class NavAppPackage : IDisposable
{
	private readonly Stream stream;

	private readonly NavAppPackageMetadata metadata;

	public Stream ContentStream { get; private set; }

	public Guid PackageId => metadata.PackageId;

	public uint MetadataVersion => metadata.MetadataVersion;

	private bool Disposed { get; set; }

	private NavAppPackage(Stream stream, NavAppPackageMetadata metadata)
	{
		this.stream = stream ?? throw new ArgumentNullException("stream");
		this.metadata = metadata ?? throw new ArgumentNullException("metadata");
	}

	public static NavAppPackage Create(Stream stream)
	{
		return Create(stream, Guid.NewGuid(), neaContent: false);
	}

	public static NavAppPackage CreateNeaPackage(Stream stream)
	{
		return Create(stream, Guid.NewGuid(), neaContent: true);
	}

	public static NavAppPackage Create(Stream stream, Guid guid, bool neaContent)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		stream.SetLength(0L);
		NavAppPackageMetadataV2 navAppPackageMetadataV = NavAppPackageMetadataV2.Create();
		navAppPackageMetadataV.PackageId = guid;
		navAppPackageMetadataV.ContentLength = 0L;
		navAppPackageMetadataV.Write(stream);
		return new NavAppPackage(stream, navAppPackageMetadataV)
		{
			ContentStream = NavAppPackageContentStream.Create(stream, navAppPackageMetadataV.MetadataSize, -1L, neaContent)
		};
	}

	public static NavAppPackage Open(Stream stream, bool allowWrite)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (allowWrite && !stream.CanWrite)
		{
			throw new ArgumentException(CodeAnalysisResources.StreamNotWriteable, "allowWrite");
		}
		if (!NavAppPackageMetadata.TryParse(stream, out var navAppPackageMetadata))
		{
			throw new ArgumentException(CodeAnalysisResources.DataNotApp, "stream");
		}
		navAppPackageMetadata.Read(stream);
		NavAppPackage navAppPackage = new NavAppPackage(stream, navAppPackageMetadata);
		if (allowWrite)
		{
			navAppPackageMetadata.PackageId = Guid.NewGuid();
			navAppPackageMetadata.Write(stream);
			stream.SetLength(navAppPackageMetadata.MetadataSize + navAppPackageMetadata.ContentLength);
		}
		navAppPackage.ContentStream = NavAppPackageContentStream.Create(stream, navAppPackageMetadata.MetadataSize, allowWrite ? (-1) : navAppPackageMetadata.ContentLength, writeAsNeaStream: false);
		return navAppPackage;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (Disposed)
		{
			return;
		}
		if (disposing && ContentStream != null)
		{
			if (ContentStream.CanWrite)
			{
				ContentStream.Flush();
				if (metadata.ContentLength != ContentStream.Length)
				{
					metadata.ContentLength = ContentStream.Length;
					metadata.Write(stream);
				}
			}
			ContentStream.Dispose();
			ContentStream = null;
		}
		Disposed = true;
	}
}
