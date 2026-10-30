using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Xml.XmlNodeAdapters;

internal class NavXAttributeAdapter : NavXObjectAdapter<XAttribute>
{
	public NavXAttributeAdapter(XAttribute attribute)
		: base(attribute)
	{
	}

	public override bool AddAfterSelf(DataError errorLevel, params object[] content)
	{
		if (Value.Parent == null)
		{
			return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
		}
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			InsertAttribute(content, 1);
		});
	}

	public override bool AddBeforeSelf(DataError errorLevel, params object[] content)
	{
		if (Value.Parent == null)
		{
			return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
		}
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			InsertAttribute(content, 0);
		});
	}

	public override bool Remove(DataError errorLevel)
	{
		if (Value.Parent == null)
		{
			return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
		}
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			Value.Parent.SetAttributeValue(Value.Name, null);
		});
	}

	public override bool ReplaceWith(DataError errorLevel, params object[] content)
	{
		if (Value.Parent == null)
		{
			return TrappableXmlOperationExecutor.RaiseInvalidOperation(errorLevel);
		}
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			InsertAttribute(content, 0);
			Value.Parent.SetAttributeValue(Value.Name, null);
		});
	}

	public override bool SelectNodes(DataError errorLevel, string xpath, ByRef<NavXmlNodeList> result)
	{
		return TrappableXmlOperationExecutor.RaiseNotSupportedOperation(errorLevel);
	}

	public override bool SelectNodes(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNodeList> result)
	{
		return TrappableXmlOperationExecutor.RaiseNotSupportedOperation(errorLevel);
	}

	public override bool SelectSingleNode(DataError errorLevel, string xpath, ByRef<NavXmlNode> result)
	{
		return TrappableXmlOperationExecutor.RaiseNotSupportedOperation(errorLevel);
	}

	public override bool SelectSingleNode(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNode> result)
	{
		return TrappableXmlOperationExecutor.RaiseNotSupportedOperation(errorLevel);
	}

	public override bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, NavOutStream data)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			ALStream.ALWriteStringBuffer(data, errorLevel, Value.ToString());
		});
	}

	public override bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, ByRef<NavText> data)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			data.Value = new NavText(data.Value.MaxLength, Value.ToString());
		});
	}

	private void InsertAttribute(object[] content, int offset)
	{
		IEnumerable<XAttribute> collection = XmlNodeMapper.UnwrapXmlNodes(content).Cast<XAttribute>();
		List<XAttribute> list = Value.Parent.Attributes().ToList();
		int index = list.IndexOf(Value) + offset;
		list.InsertRange(index, collection);
		XElement parent = Value.Parent;
		parent.Attributes().Remove();
		parent.Add(list);
	}
}
