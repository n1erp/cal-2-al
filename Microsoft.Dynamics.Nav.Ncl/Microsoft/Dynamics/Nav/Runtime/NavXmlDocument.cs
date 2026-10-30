using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlDocument : NavXmlContainerGeneric<object>
{
	public new static NavXmlDocument Default => ALCreate();

	public NavXmlNameTable ALNameTable => NameTable;

	internal new XDocument Value => (XDocument)base.Value;

	public override NavNclType NclType => NavNclType.NavXmlDocument;

	private NavXmlNameTable NameTable
	{
		get
		{
			using XmlReader xmlReader = Value.CreateReader();
			if (Value.Root != null)
			{
				XDocument.Load(xmlReader);
			}
			return new NavXmlNameTable(xmlReader.NameTable);
		}
	}

	public static NavXmlDocument ALCreate()
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() => new NavXmlDocument(new XDocument()));
	}

	public static NavXmlDocument ALCreate(params object[] content)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(() =>
		{
			object[] array = XmlNodeMapper.UnwrapXmlNodes(content);
			if (array.Length != 0 && array[0] is XDeclaration)
			{
				XDeclaration declaration = (XDeclaration)array[0];
				object[] content2 = array.Skip(1).ToArray();
				return new NavXmlDocument(new XDocument(declaration, content2));
			}
			return new NavXmlDocument(new XDocument(array));
		});
	}

	public static bool ALReadFrom(DataError errorLevel, string data, ByRef<NavXmlDocument> result)
	{
		return ReadFrom(errorLevel, data, result, prohibitDtdProcessing: false);
	}

	public static bool ALReadFrom(DataError errorLevel, NavInStream data, ByRef<NavXmlDocument> result)
	{
		return ReadFrom(errorLevel, data, result, prohibitDtdProcessing: false);
	}

	public static bool ALReadFrom(DataError errorLevel, string data, NavXmlReadOptions readOptions, ByRef<NavXmlDocument> result)
	{
		return ReadFrom(errorLevel, data, readOptions, result, prohibitDtdProcessing: false);
	}

	public static bool ALReadFrom(DataError errorLevel, NavInStream data, NavXmlReadOptions readOptions, ByRef<NavXmlDocument> result)
	{
		return ReadFrom(errorLevel, data, readOptions, result, prohibitDtdProcessing: false);
	}

	public bool ALGetRoot(DataError errorLevel, ByRef<NavXmlElement> result)
	{
		return GetRoot(errorLevel, result);
	}

	public bool ALGetDeclaration(DataError errorLevel, ByRef<NavXmlDeclaration> result)
	{
		return GetDeclaration(errorLevel, result);
	}

	public bool ALSetDeclaration(DataError errorLevel, NavXmlDeclaration declaration)
	{
		return SetDeclaration(errorLevel, declaration);
	}

	public bool ALGetDocumentType(DataError errorLevel, ByRef<NavXmlDocumentType> result)
	{
		return GetDocumentType(errorLevel, result);
	}

	internal NavXmlDocument(XDocument document)
		: base((object)document)
	{
	}

	private static bool ReadFrom(DataError errorLevel, string data, ByRef<NavXmlDocument> result, bool prohibitDtdProcessing)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			using StringReader textReader = new StringReader(data);
			using XmlReader reader = XmlHelper.SafeXmlReader(textReader, prohibitDtdProcessing);
			XDocument document = XDocument.Load(reader);
			result.Value = new NavXmlDocument(document);
		});
	}

	private static bool ReadFrom(DataError errorLevel, string data, NavXmlReadOptions readOptions, ByRef<NavXmlDocument> result, bool prohibitDtdProcessing)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			using StringReader textReader = new StringReader(data);
			using XmlReader reader = XmlHelper.SafeXmlReader(textReader, prohibitDtdProcessing);
			XDocument document = XDocument.Load(reader, readOptions.LoadOptions);
			result.Value = new NavXmlDocument(document);
		});
	}

	private static bool ReadFrom(DataError errorLevel, NavInStream data, ByRef<NavXmlDocument> result, bool prohibitDtdProcessing)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			using XmlReader reader = XmlHelper.SafeXmlReader(data.InternalStreamCheckedForRead, prohibitDtdProcessing);
			XDocument document = XDocument.Load(reader);
			result.Value = new NavXmlDocument(document);
		});
	}

	private static bool ReadFrom(DataError errorLevel, NavInStream data, NavXmlReadOptions readOptions, ByRef<NavXmlDocument> result, bool prohibitDtdProcessing)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			using XmlReader reader = XmlHelper.SafeXmlReader(data.InternalStreamCheckedForRead, prohibitDtdProcessing);
			XDocument document = XDocument.Load(reader, readOptions.LoadOptions);
			result.Value = new NavXmlDocument(document);
		});
	}

	private bool GetRoot(DataError errorLevel, ByRef<NavXmlElement> result)
	{
		if (Value.Root == null)
		{
			return TrappableXmlOperationExecutor.RaiseXmlNodeNotFound(errorLevel);
		}
		result.Value = new NavXmlElement(Value.Root);
		return true;
	}

	private bool GetDeclaration(DataError errorLevel, ByRef<NavXmlDeclaration> result)
	{
		if (Value.Declaration == null)
		{
			return TrappableXmlOperationExecutor.RaiseXmlNodeNotFound(errorLevel);
		}
		result.Value = new NavXmlDeclaration(Value.Declaration);
		return true;
	}

	private bool SetDeclaration(DataError errorLevel, NavXmlDeclaration declaration)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			Value.Declaration = declaration.Value;
		});
	}

	private bool GetDocumentType(DataError errorLevel, ByRef<NavXmlDocumentType> result)
	{
		if (Value.DocumentType == null)
		{
			return TrappableXmlOperationExecutor.RaiseXmlNodeNotFound(errorLevel);
		}
		result.Value = new NavXmlDocumentType(Value.DocumentType);
		return true;
	}
}
