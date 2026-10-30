using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class ReportWordUtilities
{
	internal static void GenerateWordLayout(SourceReportTypeSymbol report, IFileSystem fileSystem, string docName)
	{
		string officeCustomXmlPart = ReportRdlcUtilities.GetOfficeCustomXmlPart(report, writeMergeInformation: false);
		string officeCustomXmlPart2 = ReportRdlcUtilities.GetOfficeCustomXmlPart(report, writeMergeInformation: true);
		if (fileSystem.Exists(docName))
		{
			UpdateWordFile(fileSystem, docName, officeCustomXmlPart, officeCustomXmlPart2);
		}
		else
		{
			CreateNewWordFile(fileSystem, docName, officeCustomXmlPart);
		}
	}

	private static void CreateNewWordFile(IFileSystem fileSystem, string docName, string officeCustomXmlPart)
	{
		using MemoryStream memoryStream = new MemoryStream();
		WordReportManager.NewWordDocumentLayout(memoryStream, officeCustomXmlPart);
		memoryStream.Position = 0L;
		fileSystem.CreateDirectoryForFile(docName);
		fileSystem.WriteBytes(docName, memoryStream.ToArray());
	}

	private static void UpdateWordFile(IFileSystem fileSystem, string docName, string officeCustomXmlPart, string officeCustomXmlPart_Extended)
	{
		using MemoryStream memoryStream = new MemoryStream(fileSystem.ReadBytes(docName));
		using MemoryStream memoryStream2 = new MemoryStream();
		using MemoryStream memoryStream3 = new MemoryStream();
		string oldXmlPart = FillXmlPartDataWithExtendedInfo(GetXmlPartStreamFromWordFile(memoryStream));
		WordReportManager wordReportManager = new WordReportManager();
		wordReportManager.UpdateWordDocumentLayout(memoryStream, memoryStream2, oldXmlPart, officeCustomXmlPart_Extended);
		wordReportManager.UpdateWordDocumentLayout(memoryStream2, memoryStream3, officeCustomXmlPart, officeCustomXmlPart);
		memoryStream.Position = 0L;
		memoryStream3.Position = 0L;
		memoryStream2.Position = 0L;
		if (memoryStream3.Length != 0L)
		{
			fileSystem.WriteBytes(docName, memoryStream3.ToArray());
		}
		else if (memoryStream2.Length != 0L)
		{
			fileSystem.WriteBytes(docName, memoryStream2.ToArray());
		}
		else
		{
			fileSystem.WriteBytes(docName, memoryStream.ToArray());
		}
	}

	private static string GetXmlPartStreamFromWordFile(MemoryStream fileStream)
	{
		fileStream.Position = 0L;
		using ZipArchive zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, leaveOpen: true);
		using Stream stream = zipArchive.Entries.FirstOrDefault((ZipArchiveEntry e) => e.FullName.StartsWith("customXML/item", StringComparison.OrdinalIgnoreCase) && !e.FullName.Contains("Props")).Open();
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}

	private static string FillXmlPartDataWithExtendedInfo(string oldXmlPart)
	{
		return string.Join("\n", oldXmlPart.Split('\n').Select((string line) =>
		{
			if (!line.StartsWith("  ", StringComparison.Ordinal) || line.TrimStart().StartsWith("</", StringComparison.Ordinal))
			{
				return line;
			}
			int num = line.IndexOf("<", StringComparison.OrdinalIgnoreCase) + 1;
			int num2 = line.IndexOf(">", StringComparison.OrdinalIgnoreCase);
			int memberId = IdSpace.GetMemberId(line.Substring(num, num2 - num));
			if (line[num2 - 1] == '/')
			{
				num2--;
			}
			string value;
			if (line.Contains("</") && !line.Contains(" </"))
			{
				value = " ElementType=\"Column\" ElementId =\"" + memberId + "\" DataType =\"None\"";
				return line.Insert(num2, value);
			}
			value = " ElementType=\"DataItem\" ElementId =\"" + memberId + "\"";
			return line.Insert(num2, value);
		}));
	}
}
