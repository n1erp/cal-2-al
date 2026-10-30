using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Xml.XmlNodeAdapters;

internal class NavXDeclarationAdapter : INavXmlNode
{
	private readonly XDeclaration value;

	public XmlNodeType NodeType => XmlNodeType.XmlDeclaration;

	public string OuterXml => value.ToString();

	public NavXDeclarationAdapter(XDeclaration value)
	{
		this.value = value;
	}

	public bool AddAfterSelf(DataError errorLevel, params object[] content)
	{
		return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
	}

	public bool AddBeforeSelf(DataError errorLevel, params object[] content)
	{
		return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
	}

	public bool GetDocument(DataError errorLevel, ByRef<NavXmlDocument> result)
	{
		return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
	}

	public bool GetParent(DataError errorLevel, ByRef<NavXmlElement> result)
	{
		return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
	}

	public bool Remove(DataError errorLevel)
	{
		return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
	}

	public bool ReplaceWith(DataError errorLevel, params object[] content)
	{
		return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
	}

	public bool SelectNodes(DataError errorLevel, string xpath, ByRef<NavXmlNodeList> result)
	{
		return TrappableXmlOperationExecutor.RaiseNotSupportedOperation(errorLevel);
	}

	public bool SelectNodes(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNodeList> result)
	{
		return TrappableXmlOperationExecutor.RaiseNotSupportedOperation(errorLevel);
	}

	public bool SelectSingleNode(DataError errorLevel, string xpath, ByRef<NavXmlNode> result)
	{
		return TrappableXmlOperationExecutor.RaiseNotSupportedOperation(errorLevel);
	}

	public bool SelectSingleNode(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNode> result)
	{
		return TrappableXmlOperationExecutor.RaiseNotSupportedOperation(errorLevel);
	}

	public bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, NavOutStream data)
	{
		return WriteTo(errorLevel, data);
	}

	public bool WriteTo(DataError errorLevel, NavOutStream data)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			ALStream.ALWriteString(data, errorLevel, OuterXml);
		});
	}

	public bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, ByRef<NavText> data)
	{
		return WriteTo(errorLevel, data);
	}

	public bool WriteTo(DataError errorLevel, ByRef<NavText> data)
	{
		data.Value = new NavText(data.Value.MaxLength, OuterXml);
		return true;
	}
}
