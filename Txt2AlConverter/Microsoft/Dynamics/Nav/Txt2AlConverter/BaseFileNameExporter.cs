using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public class BaseFileNameExporter : IFileNameExporter
{
	public static readonly string ObjectNameToken = "{name}";

	public static readonly string ObjectTypeToken = "{type}";

	public static readonly string ObjectIdToken = "{id}";

	public static readonly string ExtensionObjectTargetIdToken = "{targetId}";

	public static readonly string ExtensionObjectTargetNameToken = "{targetName}";

	private const string RdlcExtension = ".rdlc";

	private const string WordLayoutExtension = ".docx";

	public static readonly string AlFileExtension = ".al";

	private readonly AlExportOptions exportOptions;

	private readonly string extensionObjectFileNamePattern;

	private readonly string objectFileNamePattern;

	private readonly Dictionary<string, string> filePathToObjectMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

	private object l = new object();

	public BaseFileNameExporter()
		: this(AlExportOptions.Default, null, null)
	{
	}

	public BaseFileNameExporter(AlExportOptions exportOptions, string extensionObjectFileNamePattern, string objectFileNamePattern)
	{
		this.exportOptions = exportOptions;
		this.extensionObjectFileNamePattern = extensionObjectFileNamePattern;
		this.objectFileNamePattern = objectFileNamePattern;
	}

	public string GetAlFileName(ApplicationObject ao)
	{
		string text2;
		string objectKey;
		string text3;
		if (ao.RootElement.ElementTypeInfo.ElementType == ElementType.Modification)
		{
			string newValue = ao.Id.ObjectNumber.ToString();
			string text = ao.Id.ObjectType.ToString() + "Ext";
			int reservedExtensionObjectId = exportOptions.GetReservedExtensionObjectId(ao.RootElement);
			text2 = reservedExtensionObjectId.ToString();
			objectKey = text + text2;
			text3 = extensionObjectFileNamePattern.Replace(ObjectNameToken, RemoveInvalidCharactersFromFileName(AlUtilities.GetExtensionObjectName(ao.RootElement, reservedExtensionObjectId))).Replace(ObjectTypeToken, text).Replace(ObjectIdToken, text2)
				.Replace(ExtensionObjectTargetIdToken, newValue)
				.Replace(ExtensionObjectTargetNameToken, AlUtilities.GetModificationTargetName(ao.RootElement.GetStringProperty(PropertyType.Target)));
		}
		else
		{
			text2 = ao.Id.ObjectNumber.ToString();
			objectKey = ao.RootElement.ElementTypeInfo.ElementType.ToString() + text2;
			text3 = objectFileNamePattern.Replace(ObjectIdToken, ao.RootElement.Id.ToString()).Replace(ObjectNameToken, RemoveInvalidCharactersFromFileName(ao.RootElement.Name)).Replace(ObjectTypeToken, ao.RootElement.ElementTypeInfo.ElementType.ToString())
				.Replace(ExtensionObjectTargetIdToken, ao.RootElement.GetStringProperty(PropertyType.Id));
		}
		text3 += AlFileExtension;
		return GetFileNameWithoutClash(text2, objectKey, text3);
	}

	public string GetRdlcLayoutFileName(IElement element)
	{
		string fileName = RemoveInvalidCharactersFromFileName(element.Name) + ".rdlc";
		return GetFileNameWithoutClash(element.Id.ToString(), GetRootApplicationObjectId(element), fileName);
	}

	private string GetRootApplicationObjectId(IElement element)
	{
		return element.ElementTypeInfo.ElementType.ToString() + element.Id;
	}

	private string GetFileNameWithoutClash(string id, string objectKey, string fileName)
	{
		lock (l)
		{
			if (!filePathToObjectMap.TryGetValue(fileName, out var value))
			{
				filePathToObjectMap[fileName] = objectKey;
				return fileName;
			}
			if (objectKey == value)
			{
				return fileName;
			}
			return Path.GetFileNameWithoutExtension(fileName) + "." + id + Path.GetExtension(fileName);
		}
	}

	public string GetWordLayoutFileName(IElement element)
	{
		string fileName = RemoveInvalidCharactersFromFileName(element.Name) + ".docx";
		return GetFileNameWithoutClash(element.Id.ToString(), GetRootApplicationObjectId(element), fileName);
	}

	private static string RemoveInvalidCharactersFromFileName(string path)
	{
		HashSet<char> hashSet = new HashSet<char>();
		hashSet.AddAll(Path.GetInvalidFileNameChars());
		hashSet.AddAll(Path.GetInvalidPathChars());
		StringBuilder stringBuilder = new StringBuilder(path.Length);
		foreach (char c in path)
		{
			if (char.IsLetterOrDigit(c) && !hashSet.Contains(c))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}
}
