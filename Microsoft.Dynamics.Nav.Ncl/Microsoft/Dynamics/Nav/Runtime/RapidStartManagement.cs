using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class RapidStartManagement
{
	public static void ImportConfigurationPackageFile(ITreeObject session, byte[] packageData, string type, int? processingOrderOverride, bool force)
	{
		if (packageData == null)
		{
			throw new ArgumentNullException("packageData");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		XmlDocument packageXml = GetPackageXml(packageData);
		ReadPackageHeader(packageXml, out var name, out var code, out var languageId, out var processingOrder);
		if (processingOrderOverride.HasValue)
		{
			processingOrder = processingOrderOverride.Value;
		}
		InsertOrUpdatePackage(session, packageData, type, code, name, languageId, processingOrder, force);
	}

	public static void RemoveConfigurationPackageFile(ITreeObject session, string code)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000170);
		navRecord.ALSetRange(1, new NavCode(navRecord.GetFieldValue(1).NavDefinedLengthMetadata, code));
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			navRecord.Delete(DataError.ThrowError, runApplicationTrigger: false, isCalledFromUI: false, isBulkDelete: false);
		}
	}

	private static XmlDocument GetPackageXml(byte[] packageData)
	{
		if (!IsGzip(packageData))
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationPackageFileWrongFileFormat));
		}
		byte[] buffer = DecompressGzip(packageData);
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.XmlResolver = null;
		try
		{
			MemoryStream input = new MemoryStream(buffer);
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
			xmlReaderSettings.XmlResolver = null;
			XmlReader reader = XmlReader.Create(input, xmlReaderSettings);
			xmlDocument.Load(reader);
			return xmlDocument;
		}
		catch (XmlException)
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationPackageFileHeaderInvalidXml));
		}
	}

	private static bool IsGzip(IReadOnlyList<byte> fileContents)
	{
		if (fileContents.Count >= 2 && fileContents[0] == 31)
		{
			return fileContents[1] == 139;
		}
		return false;
	}

	private static byte[] DecompressGzip(byte[] compressedData)
	{
		using MemoryStream stream = new MemoryStream(compressedData, writable: false);
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		gZipStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	private static void ReadPackageHeader(XmlDocument packageXml, out string name, out string code, out int languageId, out int processingOrder)
	{
		XmlElement documentElement = packageXml.DocumentElement;
		if (documentElement == null)
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationPackageFileHeaderInvalidXml));
		}
		name = documentElement.GetAttribute("PackageName");
		code = documentElement.GetAttribute("Code");
		string attribute = documentElement.GetAttribute("LanguageID");
		if (string.IsNullOrEmpty(attribute) || !int.TryParse(attribute, out languageId))
		{
			languageId = 0;
		}
		string attribute2 = documentElement.GetAttribute("ProcessingOrder");
		if (string.IsNullOrEmpty(attribute2) || !int.TryParse(attribute2, out processingOrder))
		{
			processingOrder = 0;
		}
	}

	private static void InsertOrUpdatePackage(ITreeObject session, byte[] packageData, string type, string code, string name, int languageId, int processingOrder, bool force)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000170);
		if (string.IsNullOrWhiteSpace(code))
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationPackageFileDataNotDefined, navRecord.MetaTable.GetFieldByNo(1).FieldCaption));
		}
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationPackageFileDataNotDefined, navRecord.MetaTable.GetFieldByNo(2).FieldCaption));
		}
		NCLOptionMetadata navOptionMetadata = navRecord.GetFieldValue(4).NavOptionMetadata;
		int indexFromOption = navOptionMetadata.GetIndexFromOption(type);
		if (indexFromOption < 0 || indexFromOption > navOptionMetadata.Count)
		{
			throw NavNCLInvalidOptionStringException.Create(CultureInfo.CurrentCulture, type, navOptionMetadata.OptionString);
		}
		NavCode navCode = new NavCode(navRecord.GetFieldValue(1).NavDefinedLengthMetadata, code);
		navRecord.SetRange(navRecord.MetaTable.GetFieldByNo(1), navCode, null);
		bool flag = false;
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			if (!force)
			{
				throw NavConfigurationPackageFileAlreadyExistsException.Create(code);
			}
			flag = true;
		}
		else
		{
			navRecord.ALInit();
		}
		NavText newValue = new NavText(navRecord.GetFieldValue(2).NavDefinedLengthMetadata, name);
		NavOption newValue2 = NavOption.Create(navOptionMetadata, indexFromOption);
		NavInteger newValue3 = NavInteger.Create(languageId);
		NavInteger newValue4 = NavInteger.Create(processingOrder);
		NavBLOB newValue5 = new NavBLOB(packageData);
		navRecord.SetFieldValue(1, navCode);
		navRecord.SetFieldValue(2, newValue);
		navRecord.SetFieldValue(3, newValue3);
		navRecord.SetFieldValue(5, newValue4);
		navRecord.SetFieldValue(6, newValue5);
		navRecord.SetFieldValue(4, newValue2);
		if (flag)
		{
			navRecord.Modify(DataError.ThrowError, runApplicationTrigger: false, runGlobalTrigger: false, isBulkModify: false);
		}
		else
		{
			navRecord.Insert(DataError.ThrowError, runApplicationTrigger: false, runGlobalTrigger: false);
		}
	}
}
