using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class PackageReader : IDisposable
{
	protected bool LeaveOpen { get; }

	protected bool Disposed { get; private set; }

	private Stream PackageStream { get; set; }

	private Package Package { get; set; }

	public PackageReader(Stream packageStream, bool leaveOpen = false)
	{
		Package = Package.Open(packageStream, FileMode.Open, FileAccess.Read);
		PackageStream = packageStream;
		LeaveOpen = leaveOpen;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public IEnumerable<string> ReadTableOfContents()
	{
		return (from part in Package.GetParts()
			select Uri.UnescapeDataString(part.Uri.ToString())).ToList();
	}

	public Stream ReadFile(string packagedPath)
	{
		return GetPart(packagedPath).GetStream(FileMode.Open, FileAccess.Read);
	}

	public string ReadFileAsString(string packagedPath)
	{
		using StreamReader streamReader = new StreamReader(ReadFile(packagedPath));
		return streamReader.ReadToEnd();
	}

	public byte[] ReadFileAsBytes(string packagedPath)
	{
		using Stream stream = ReadFile(packagedPath);
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return memoryStream.GetBuffer();
	}

	protected PackagePart GetPart(string packagedPath)
	{
		Uri partUri = PackageCommon.CreatePackagedUri(packagedPath);
		return Package.GetPart(partUri);
	}

	protected bool PartExists(string packagedPath)
	{
		Uri partUri = PackageCommon.CreatePackagedUri(packagedPath);
		return Package.PartExists(partUri);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (Disposed)
		{
			return;
		}
		if (disposing)
		{
			if (Package != null)
			{
				((IDisposable)Package).Dispose();
				Package = null;
			}
			if (!LeaveOpen && PackageStream != null)
			{
				PackageStream.Dispose();
				PackageStream = null;
			}
		}
		Disposed = true;
	}
}
