#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public static class FilesystemLanguageStoreFactory
{
	public const string TranslationFolder = "Translations";

	public const string TranslationMasterFile = "TranslationMaster.lang";

	private const string FileExtensionLang = ".lang";

	private const string FileExtensionText = ".txt";

	public static LanguageStore CreateStoreFromAllFilesInFolders(string serverSourceFolderPath, string translationMasterFile)
	{
		LanguageStore result = new LanguageStore(new List<string>());
		if (serverSourceFolderPath == null || translationMasterFile == null)
		{
			return result;
		}
		try
		{
			if (!Directory.Exists(serverSourceFolderPath))
			{
				return result;
			}
			DirectoryInfo directoryInfo = new DirectoryInfo(serverSourceFolderPath);
			List<FileInfo> list = directoryInfo.EnumerateFiles("*.lang", SearchOption.TopDirectoryOnly).ToList();
			List<FileInfo> list2 = directoryInfo.EnumerateFiles("*.txt", SearchOption.TopDirectoryOnly).ToList();
			if (list.Count == 0 && list2.Count == 0)
			{
				return result;
			}
			return new LanguageStore(MergeMultipleLanguageFiles(translationMasterFile, list, list2));
		}
		catch (Exception ex)
		{
			Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "Creating the store failed with an exception, data not loaded. : Ex.Message={0}", ex.Message));
			return result;
		}
	}

	private static IList<string> MergeMultipleLanguageFiles(string translationMasterFile, List<FileInfo> langFiles, List<FileInfo> txtFiles)
	{
		FileInfo fileInfo = new FileInfo(translationMasterFile.Contains(".lang") ? translationMasterFile : Path.Combine(translationMasterFile, "TranslationMaster.lang"));
		if (fileInfo.Exists)
		{
			DateTime masterFileTimeStamp = fileInfo.LastWriteTimeUtc;
			if (!langFiles.Any((FileInfo fileInfo2) => fileInfo2.LastWriteTimeUtc > masterFileTimeStamp) && !txtFiles.Any((FileInfo fileInfo2) => fileInfo2.LastWriteTimeUtc > masterFileTimeStamp))
			{
				return ReadFileLines(fileInfo);
			}
			string text = Path.ChangeExtension(fileInfo.FullName, "bak");
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			File.Move(fileInfo.FullName, text);
		}
		LanguageFormatConverter languageFormatConverter = new LanguageFormatConverter();
		List<string> list = new List<string>();
		foreach (FileInfo item in langFiles.Union(txtFiles))
		{
			string[] array = File.ReadAllLines(item.FullName);
			if (LanguageFormatConverter.IsTextFormat(array))
			{
				IList<string> collection = languageFormatConverter.ConvertToLangFormat(array);
				list.AddRange(collection);
			}
			else
			{
				list.AddRange(array);
			}
		}
		list.Sort();
		File.WriteAllLines(fileInfo.FullName, list);
		return list;
	}

	private static string[] ReadFileLines(FileSystemInfo dataFile)
	{
		dataFile.Refresh();
		if (!dataFile.Exists)
		{
			return new string[0];
		}
		return File.ReadAllLines(dataFile.FullName);
	}
}
