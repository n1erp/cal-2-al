using System;
using System.IO;
using System.IO.Packaging;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class PackageWriter : IDisposable, IPackageWriter
{
	protected Package Package { get; private set; }

	protected bool Disposed { get; private set; }

	public PackageWriter(Stream stream)
	{
		Package = Package.Open(stream, FileMode.Create, FileAccess.ReadWrite);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void WriteString(string contents, string path)
	{
		WriteBytes(Encoding.UTF8.GetBytes(contents), path);
	}

	public void WriteBytes(byte[] content, string path)
	{
		using Stream stream = CreatePackagePart(path).GetStream(FileMode.Create, FileAccess.ReadWrite);
		stream.Write(content, 0, content.Length);
	}

	public void WriteStream(Stream stream, string path)
	{
		using (stream)
		{
			using Stream destination = CreatePackagePart(path).GetStream(FileMode.Create, FileAccess.ReadWrite);
			stream.CopyTo(destination);
		}
	}

	public void WithStream(string path, Action<Stream> action)
	{
		using Stream obj = CreatePackagePart(path).GetStream(FileMode.Create, FileAccess.ReadWrite);
		action(obj);
	}

	public Stream GetPartStream(string path)
	{
		return GetOrCreatePackagePart(path).GetStream(FileMode.Create, FileAccess.ReadWrite);
	}

	private PackagePart CreatePackagePart(string packagedPath)
	{
		Uri partUri = PackageCommon.CreatePackagedUri(packagedPath);
		return Package.CreatePart(partUri, string.Empty, CompressionOption.Normal);
	}

	private PackagePart GetOrCreatePackagePart(string packagedPath)
	{
		Uri partUri = PackageCommon.CreatePackagedUri(packagedPath);
		if (Package.PartExists(partUri))
		{
			return Package.GetPart(partUri);
		}
		return CreatePackagePart(packagedPath);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			if (disposing && Package != null)
			{
				((IDisposable)Package).Dispose();
				Package = null;
			}
			Disposed = true;
		}
	}
}
