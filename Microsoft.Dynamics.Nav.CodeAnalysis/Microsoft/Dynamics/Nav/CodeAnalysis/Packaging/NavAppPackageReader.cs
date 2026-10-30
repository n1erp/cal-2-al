using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppPackageReader : PackageReader
{
	private readonly Lazy<NavAppMediaIdListing> mediaListing;

	private NavAppPackage NavAppPackage { get; set; }

	private Stream NavAppPackageStream { get; set; }

	public Guid PackageId => NavAppPackage.PackageId;

	public uint MetadataVersion => NavAppPackage.MetadataVersion;

	public NavAppMediaIdListing MediaIdListing => mediaListing.Value;

	public NavAppPackageReader(Stream packageStream, NavAppPackage appPackage, bool leaveOpen)
		: base(appPackage.ContentStream, leaveOpen)
	{
		NavAppPackage = appPackage;
		NavAppPackageStream = packageStream;
		mediaListing = new Lazy<NavAppMediaIdListing>(ReadNavAppMediaIdListing);
	}

	public static NavAppPackageReader Create(Stream stream, bool leaveOpen = false)
	{
		return new NavAppPackageReader(stream, NavAppPackage.Open(stream, allowWrite: false), leaveOpen);
	}

	public NavAppManifest ReadNavAppManifest()
	{
		using Stream stream = ReadFile("/NavxManifest.xml");
		using StreamReader streamReader = new StreamReader(stream);
		return NavAppManifest.FromXml(streamReader.ReadToEnd());
	}

	private NavAppMediaIdListing ReadNavAppMediaIdListing()
	{
		if (PartExists("/MediaIdListing.xml"))
		{
			using (Stream stream = ReadFile("/MediaIdListing.xml"))
			{
				using StreamReader streamReader = new StreamReader(stream);
				return NavAppMediaIdListing.FromXml(streamReader.ReadToEnd());
			}
		}
		return NavAppMediaIdListing.Empty;
	}

	public IEnumerable<NavAppMediaEntry> ReadMedia()
	{
		if (!string.IsNullOrEmpty(MediaIdListing.Logo?.FileName))
		{
			yield return MediaIdListing.Logo;
		}
		if (MediaIdListing.MediaSets == null)
		{
			yield break;
		}
		foreach (NavAppMediaSetEntry mediaSet in MediaIdListing.MediaSets)
		{
			foreach (NavAppMediaEntry medium in mediaSet.Media)
			{
				yield return medium;
			}
		}
	}

	public Stream ReadMediaFile(NavAppMediaEntry entry)
	{
		if (entry.IsLogo)
		{
			return ReadFile("/logo/" + MediaIdListing?.Logo?.FileName);
		}
		return ReadFile("/media/" + entry.FileName);
	}

	public IEnumerable<string> ReadTranslationFilePaths()
	{
		return ReadFilePaths("/trans/").ToList();
	}

	public IEnumerable<string> ReadTableDataPaths()
	{
		return (from path in ReadSourceFilePaths()
			where path.EndsWith(".navxdata", StringComparison.OrdinalIgnoreCase)
			select path).ToList();
	}

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Addin", Justification = "Intended spelling")]
	public IEnumerable<string> ReadControlAddinPaths()
	{
		return ReadDirectoryFilePaths("addin");
	}

	public IEnumerable<string> ReadPermissionSetPaths()
	{
		return ReadDirectoryFilePaths("perm");
	}

	public IEnumerable<string> ReadWebServicePaths()
	{
		return ReadDirectoryFilePaths("serv");
	}

	public IEnumerable<string> ReadCustomReportLayoutPaths()
	{
		return ReadDirectoryFilePaths("replay");
	}

	public IEnumerable<string> ReadProfilePaths()
	{
		return ReadDirectoryFilePaths("profile");
	}

	public Stream ReadNavigationFile()
	{
		if (!PartExists("/navigation.xml"))
		{
			return null;
		}
		return ReadFile("/navigation.xml");
	}

	public IEnumerable<string> ReadSourceFilePaths()
	{
		return ReadFilePaths("/src/").Concat(ReadFilePaths("/perm/")).Concat(ReadFilePaths("/tabledata/")).Concat(ReadFilePaths("/addin/"))
			.Concat(ReadFilePaths("/serv/"))
			.Concat(ReadFilePaths("/replay/"))
			.Concat(ReadFilePaths("/media/"))
			.Concat(ReadFilePaths("/logo/"))
			.Concat(ReadFilePaths("/trans/"))
			.Concat(ReadFilePaths("/profile/").Concat(ReadFilePaths("/layout/")).Concat(ReadFilePaths("/Translations/")));
	}

	public IEnumerable<string> ReadSourceCodeFilePaths()
	{
		return ReadFilePaths("/src/");
	}

	public IEnumerable<string> ReadEmittedFilePaths()
	{
		return ReadFilePaths("/bin/");
	}

	public IEnumerable<string> ReadSourceContents()
	{
		foreach (string item in ReadFilePaths("/src/"))
		{
			using Stream stream = ReadFile(item);
			using StreamReader reader = new StreamReader(stream);
			yield return reader.ReadToEnd();
		}
	}

	public Stream ReadSymbolReferenceFile()
	{
		return ReadFile("/SymbolReference.json");
	}

	public Stream ReadRadFile()
	{
		if (!PartExists("/rad.json"))
		{
			return null;
		}
		return ReadFile("/rad.json");
	}

	public ObjectChangeModelDefinition GetRadChanges()
	{
		using Stream stream = ReadRadFile();
		if (stream == null)
		{
			return null;
		}
		StreamReader reader = new StreamReader(stream);
		return JsonSerializer.Create(SymbolReferenceJsonWriter.SerializerSettings).Deserialize(reader, typeof(ObjectChangeModelDefinition)) as ObjectChangeModelDefinition;
	}

	private IEnumerable<string> ReadFilePaths(string path)
	{
		return (from x in ReadTableOfContents()
			where x.StartsWith(path, StringComparison.OrdinalIgnoreCase)
			select x).ToList();
	}

	private List<string> ReadDirectoryFilePaths(string directory)
	{
		return (from file in ReadSourceFilePaths()
			where string.Equals(Path.GetDirectoryName(file)?.Trim('\\'), directory, StringComparison.OrdinalIgnoreCase)
			select file).ToList();
	}

	private void CopyFolderContent(IDictionary<string, byte[]> builder, params string[] folders)
	{
		foreach (string text in folders)
		{
			foreach (string item in ReadFilePaths(text))
			{
				string key = item.Remove(0, text.Length);
				using MemoryStream memoryStream = new MemoryStream();
				using Stream stream = ReadFile(item);
				stream.CopyTo(memoryStream);
				builder[key] = memoryStream.ToArray();
			}
		}
	}

	public ImmutableDictionary<string, byte[]> MakeDictionaryForPackageRecompilation()
	{
		PooledDictionary<string, byte[]> instance = PooledDictionary<string, byte[]>.GetInstance();
		try
		{
			IEnumerable<string> enumerable = ReadFilePaths("/layout/");
			string[] source = new string[3] { ".rdl", ".rdlc", ".docx" };
			foreach (string item in enumerable)
			{
				string filepath = item.Remove(0, "/layout/".Length);
				if (source.Any((string ext) => string.Equals(Path.GetExtension(filepath), ext, StringComparison.OrdinalIgnoreCase)))
				{
					MemoryStream memoryStream = new MemoryStream();
					ReadFile(item).CopyTo(memoryStream);
					instance[filepath] = memoryStream.ToArray();
				}
			}
			foreach (string item2 in ReadFilePaths("/Translations/"))
			{
				if (item2.EndsWith(".lcl", StringComparison.OrdinalIgnoreCase) || item2.EndsWith(".xlf", StringComparison.OrdinalIgnoreCase) || item2.EndsWith(".xliff", StringComparison.OrdinalIgnoreCase))
				{
					MemoryStream memoryStream2 = new MemoryStream();
					ReadFile(item2).CopyTo(memoryStream2);
					instance[item2] = memoryStream2.ToArray();
				}
			}
			foreach (string item3 in ReadTranslationFilePaths())
			{
				using Stream stream = ReadFile(item3);
				byte[] array = new byte[stream.Length];
				stream.Read(array, 0, array.Length);
				instance[item3] = array;
			}
			CopyFolderContent(instance, "/addin/src/", "/perm/", "/serv/", "/tabledata/", "/replay/");
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	protected override void Dispose(bool disposing)
	{
		bool disposed = Disposed;
		base.Dispose(disposing);
		if (!disposed & disposing)
		{
			if (NavAppPackage != null)
			{
				NavAppPackage.Dispose();
				NavAppPackage = null;
			}
			if (!LeaveOpen && NavAppPackageStream != null)
			{
				NavAppPackageStream.Dispose();
				NavAppPackageStream = null;
			}
		}
	}
}
