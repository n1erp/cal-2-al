using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime.Media;

internal abstract class NavMediaBase : IDisposable
{
	private bool disposed;

	public abstract byte[] BuiltinThumbnailData { get; }

	public abstract int BuiltinThumbnailWidth { get; }

	public abstract int BuiltinThumbnailHeight { get; }

	public abstract string BuiltinThumbnailMimeType { get; }

	public abstract int Height { get; }

	public abstract int Width { get; }

	public virtual string MimeType => InnerMimeType;

	protected string InnerMimeType { private get; set; }

	public abstract string ThumbnailMimeType { get; }

	public virtual bool AllowCustomThumbnails => true;

	public abstract NavMediaThumbnail Scale(int width);

	public abstract NavMediaThumbnail Scale(int width, int height);

	public abstract byte[] Bytes();

	public abstract void ValidateContentHeader(Stream contentStream);

	protected virtual void Dispose(bool disposing)
	{
	}

	public void Dispose()
	{
		if (!disposed)
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
			disposed = true;
		}
	}
}
