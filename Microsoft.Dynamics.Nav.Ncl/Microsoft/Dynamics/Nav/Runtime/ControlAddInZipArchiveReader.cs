using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class ControlAddInZipArchiveReader : IDisposable
{
	private readonly byte[] data;

	private ZipArchive zipArchive;

	public const string ManifestFileName = "manifest.xml";

	public DateTimeOffset WriteTime
	{
		get
		{
			DateTimeOffset dateTimeOffset = DateTimeOffset.MinValue;
			try
			{
				OpenZipArchive();
				foreach (ZipArchiveEntry entry in zipArchive.Entries)
				{
					if (entry.LastWriteTime > dateTimeOffset)
					{
						dateTimeOffset = entry.LastWriteTime;
					}
				}
			}
			catch (NotSupportedException)
			{
			}
			return dateTimeOffset;
		}
	}

	public ControlAddInZipArchiveReader(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		this.data = data;
	}

	public ControlAddInZipArchiveReader(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		zipArchive = new ZipArchive(stream);
	}

	public void Dispose()
	{
		if (zipArchive != null)
		{
			zipArchive.Dispose();
			zipArchive = null;
		}
	}

	public void ExtractToDirectory(string destinationDirectoryName)
	{
		zipArchive.ExtractToDirectory(destinationDirectoryName);
	}

	public byte[] ReadManifest()
	{
		try
		{
			return ReadZipEntry("manifest.xml");
		}
		catch (NotSupportedException)
		{
		}
		return null;
	}

	public byte[] ReadResource(ControlAddInResourceType resourceType, string resourceName)
	{
		try
		{
			if (resourceName.Contains('/'))
			{
				return ReadZipEntry(resourceName);
			}
			return ReadZipEntry(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", resourceType, resourceName));
		}
		catch (NotSupportedException)
		{
		}
		return null;
	}

	public byte[] ReadResource(string resourceName)
	{
		try
		{
			return ReadZipEntry(resourceName);
		}
		catch (NotSupportedException)
		{
		}
		return null;
	}

	private void OpenZipArchive()
	{
		if (zipArchive == null)
		{
			zipArchive = new ZipArchive(new MemoryStream(data), ZipArchiveMode.Read);
		}
	}

	private byte[] ReadZipEntry(string fullName)
	{
		OpenZipArchive();
		byte[] array = null;
		ZipArchiveEntry zipArchiveEntry = zipArchive.Entries.FirstOrDefault((ZipArchiveEntry e) => string.Compare(e.FullName.Replace('\\', '/'), fullName, StringComparison.OrdinalIgnoreCase) == 0);
		if (zipArchiveEntry != null)
		{
			array = new byte[zipArchiveEntry.Length];
			using Stream stream = zipArchiveEntry.Open();
			stream.Read(array, 0, array.Length);
		}
		return array;
	}
}
