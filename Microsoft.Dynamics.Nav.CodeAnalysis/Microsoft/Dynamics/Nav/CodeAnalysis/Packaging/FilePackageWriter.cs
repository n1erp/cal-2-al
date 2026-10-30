using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public sealed class FilePackageWriter : IDisposable, IPackageWriter
{
	private string zipFileName;

	private bool isDisposed;

	internal DirectoryInfo DirectoryInfo { get; private set; }

	internal string ZipFileName
	{
		get
		{
			if (!string.IsNullOrEmpty(zipFileName))
			{
				return zipFileName;
			}
			zipFileName = Path.Combine(DirectoryInfo.Parent.FullName, DirectoryInfo.Parent.Name) + ".zip";
			return zipFileName;
		}
	}

	public FilePackageWriter(string outputDirectoryName = "/bin/")
	{
		if (!Directory.Exists(outputDirectoryName))
		{
			DirectoryInfo = Directory.CreateDirectory(outputDirectoryName);
		}
		else
		{
			DirectoryInfo = new DirectoryInfo(outputDirectoryName);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void WriteString(string contents, string path)
	{
		WriteBytes(Encoding.UTF8.GetBytes(contents), path);
	}

	public void WriteBytes(byte[] content, string path)
	{
		using FileStream fileStream = CreateFileStream(path);
		fileStream.Write(content, 0, content.Length);
	}

	public void WriteStream(Stream stream, string path)
	{
		using (stream)
		{
			using FileStream destination = CreateFileStream(path);
			stream.CopyTo(destination);
		}
	}

	public void WithStream(string path, Action<Stream> action)
	{
		using FileStream obj = CreateFileStream(path);
		action(obj);
	}

	public void Flush()
	{
		if (DirectoryInfo == null)
		{
			return;
		}
		string text = ZipFileName;
		try
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
		catch (IOException)
		{
		}
		ZipFile.CreateFromDirectory(DirectoryInfo.FullName, text, CompressionLevel.Fastest, includeBaseDirectory: false);
		try
		{
			DirectoryInfo.Delete(recursive: true);
		}
		catch (IOException)
		{
		}
		finally
		{
			DirectoryInfo = null;
		}
	}

	internal FileStream CreateFileStream(string packagedRelativePath)
	{
		return new FileStream(PathUtilities.CombinePath(DirectoryInfo.FullName, packagedRelativePath), FileMode.Create, FileAccess.ReadWrite);
	}

	private void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			if (disposing)
			{
				Flush();
			}
			isDisposed = true;
		}
	}
}
