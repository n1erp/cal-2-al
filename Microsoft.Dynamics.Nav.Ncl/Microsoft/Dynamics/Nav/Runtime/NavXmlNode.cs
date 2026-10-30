using System.Xml;
using Microsoft.Dynamics.Nav.Runtime.Xml.XmlNodeAdapters;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlNode : NavValue
{
	private readonly INavXmlNode adaptedNode;

	public static NavXmlNode Default => new NavXmlNode(null);

	public string ALOuterXml => adaptedNode.OuterXml;

	public bool ALIsXmlAttribute => IsXmlAttribute;

	public bool ALIsXmlCData => IsXmlCData;

	public bool ALIsXmlComment => IsXmlComment;

	public bool ALIsXmlDeclaration => IsXmlDeclaration;

	public bool ALIsXmlDocument => IsXmlDocument;

	public bool ALIsXmlDocumentType => IsXmlDocumentType;

	public bool ALIsXmlElement => IsXmlElement;

	public bool ALIsXmlProcessingInstruction => IsXmlProcessingInstruction;

	public bool ALIsXmlText => IsXmlText;

	private bool IsXmlAttribute => NclType == NavNclType.NavXmlAttribute;

	private bool IsXmlCData => NclType == NavNclType.NavXmlCData;

	private bool IsXmlComment => NclType == NavNclType.NavXmlComment;

	private bool IsXmlDeclaration => NclType == NavNclType.NavXmlDeclaration;

	private bool IsXmlDocument => NclType == NavNclType.NavXmlDocument;

	private bool IsXmlDocumentType => NclType == NavNclType.NavXmlDocumentType;

	private bool IsXmlElement => NclType == NavNclType.NavXmlElement;

	private bool IsXmlProcessingInstruction => NclType == NavNclType.NavXmlProcessingInstruction;

	private bool IsXmlText => NclType == NavNclType.NavXmlText;

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty
	{
		get
		{
			if (adaptedNode != null)
			{
				return adaptedNode.NodeType == XmlNodeType.None;
			}
			return true;
		}
	}

	public override bool IsInitialized => !IsZeroOrEmpty;

	public override NavNclType NclType => NavNclType.NavXmlNode;

	public virtual string DebuggerStringValue => string.Empty;

	public bool ALGetParent(DataError errorLevel, ByRef<NavXmlElement> result)
	{
		return adaptedNode.GetParent(errorLevel, result);
	}

	public bool ALGetDocument(DataError errorLevel, ByRef<NavXmlDocument> result)
	{
		return adaptedNode.GetDocument(errorLevel, result);
	}

	public bool ALAddAfterSelf(DataError errorLevel, params object[] content)
	{
		return adaptedNode.AddAfterSelf(errorLevel, content);
	}

	public bool ALAddBeforeSelf(DataError errorLevel, params object[] content)
	{
		return adaptedNode.AddBeforeSelf(errorLevel, content);
	}

	public bool ALReplaceWith(DataError errorLevel, params object[] content)
	{
		return adaptedNode.ReplaceWith(errorLevel, content);
	}

	public bool ALRemove(DataError errorLevel)
	{
		return adaptedNode.Remove(errorLevel);
	}

	public bool ALSelectSingleNode(DataError errorLevel, string xpath, ByRef<NavXmlNode> result)
	{
		return adaptedNode.SelectSingleNode(errorLevel, xpath, result);
	}

	public bool ALSelectSingleNode(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNode> result)
	{
		return adaptedNode.SelectSingleNode(errorLevel, xpath, namespaceManager, result);
	}

	public bool ALSelectNodes(DataError errorLevel, string xpath, ByRef<NavXmlNodeList> result)
	{
		return adaptedNode.SelectNodes(errorLevel, xpath, result);
	}

	public bool ALSelectNodes(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNodeList> result)
	{
		return adaptedNode.SelectNodes(errorLevel, xpath, namespaceManager, result);
	}

	public bool ALWriteTo(DataError errorLevel, ByRef<NavText> result)
	{
		return adaptedNode.WriteTo(errorLevel, result);
	}

	public bool ALWriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, ByRef<NavText> result)
	{
		return adaptedNode.WriteTo(errorLevel, writeOptions, result);
	}

	public bool ALWriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, NavOutStream result)
	{
		return adaptedNode.WriteTo(errorLevel, writeOptions, result);
	}

	public bool ALWriteTo(DataError errorLevel, NavOutStream result)
	{
		return adaptedNode.WriteTo(errorLevel, result);
	}

	public NavXmlNode ALAsXmlNode()
	{
		return this;
	}

	public NavXmlAttribute ALAsXmlAttribute()
	{
		return AsXmlAttribute();
	}

	public NavXmlCData ALAsXmlCData()
	{
		return AsXmlCData();
	}

	public NavXmlComment ALAsXmlComment()
	{
		return AsXmlComment();
	}

	public NavXmlDeclaration ALAsXmlDeclaration()
	{
		return AsXmlDeclaration();
	}

	public NavXmlDocument ALAsXmlDocument()
	{
		return AsXmlDocument();
	}

	public NavXmlDocumentType ALAsXmlDocumentType()
	{
		return AsXmlDocumentType();
	}

	public NavXmlElement ALAsXmlElement()
	{
		return AsXmlElement();
	}

	public NavXmlProcessingInstruction ALAsXmlProcessingInstruction()
	{
		return AsXmlProcessingInstruction();
	}

	public NavXmlText ALAsXmlText()
	{
		return AsXmlText();
	}

	internal NavXmlNode(object value)
	{
		adaptedNode = NavXmlNodeAdapterFactory.CreateAdapter(value);
	}

	public static NavXmlNode CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default;
		}
		if (value is NavXmlNode result)
		{
			return result;
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavXmlNode), value);
	}

	private NavXmlAttribute AsXmlAttribute()
	{
		if (!IsXmlAttribute)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavXmlAttribute));
		}
		return (NavXmlAttribute)this;
	}

	private NavXmlCData AsXmlCData()
	{
		if (!IsXmlCData)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavXmlCData));
		}
		return (NavXmlCData)this;
	}

	private NavXmlComment AsXmlComment()
	{
		if (!IsXmlComment)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavXmlComment));
		}
		return (NavXmlComment)this;
	}

	private NavXmlDeclaration AsXmlDeclaration()
	{
		if (!IsXmlDeclaration)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavXmlDeclaration));
		}
		return (NavXmlDeclaration)this;
	}

	private NavXmlDocument AsXmlDocument()
	{
		if (!IsXmlDocument)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavXmlDocument));
		}
		return (NavXmlDocument)this;
	}

	private NavXmlDocumentType AsXmlDocumentType()
	{
		if (!IsXmlDocumentType)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavXmlDocumentType));
		}
		return (NavXmlDocumentType)this;
	}

	private NavXmlElement AsXmlElement()
	{
		if (!IsXmlElement)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavXmlElement));
		}
		return (NavXmlElement)this;
	}

	private NavXmlProcessingInstruction AsXmlProcessingInstruction()
	{
		if (!IsXmlProcessingInstruction)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavXmlProcessingInstruction));
		}
		return (NavXmlProcessingInstruction)this;
	}

	private NavXmlText AsXmlText()
	{
		if (!IsXmlText)
		{
			throw new NavNCLConversionException(GetType(), typeof(NavXmlText));
		}
		return (NavXmlText)this;
	}

	public override int GetHashCode()
	{
		return adaptedNode?.GetHashCode() ?? 0;
	}
}
