using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Xml.XmlNodeAdapters;

internal class NavXNodeAdapter<TNode> : NavXObjectAdapter<TNode> where TNode : XNode
{
	protected virtual XmlWriterSettings DefaultXmlWriterSettings { get; } = new XmlWriterSettings
	{
		OmitXmlDeclaration = true,
		ConformanceLevel = ConformanceLevel.Auto
	};

	public NavXNodeAdapter(TNode node)
		: base(node)
	{
	}

	public override bool AddAfterSelf(DataError errorLevel, params object[] content)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object[] content2 = XmlNodeMapper.UnwrapXmlNodes(content);
			Value.AddAfterSelf(content2);
		});
	}

	public override bool AddBeforeSelf(DataError errorLevel, params object[] content)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object[] content2 = XmlNodeMapper.UnwrapXmlNodes(content);
			Value.AddBeforeSelf(content2);
		});
	}

	public override bool ReplaceWith(DataError errorLevel, params object[] content)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object[] content2 = XmlNodeMapper.UnwrapXmlNodes(content);
			Value.ReplaceWith(content2);
		});
	}

	public override bool Remove(DataError errorLevel)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			Value.Remove();
		});
	}

	public override bool SelectSingleNode(DataError errorLevel, string xpath, ByRef<NavXmlNode> result)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object xpathResults = Value.XPathEvaluate(xpath);
			NavXmlNode navXmlNode = XmlNodeMapper.SelectFirstOrDefaultXmlNode(xpathResults);
			if (navXmlNode == null)
			{
				throw new NavNCLInvalidXPathSelectionException(string.Format(CultureInfo.CurrentCulture, Lang.NavXmlXPathQueryDidNotProduceAnyResults, xpath));
			}
			result.Value = navXmlNode;
		});
	}

	public override bool SelectSingleNode(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNode> result)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object xpathResults = Value.XPathEvaluate(xpath, namespaceManager.Value);
			NavXmlNode navXmlNode = XmlNodeMapper.SelectFirstOrDefaultXmlNode(xpathResults);
			if (navXmlNode == null)
			{
				throw new NavNCLInvalidXPathSelectionException(string.Format(CultureInfo.CurrentCulture, Lang.NavXmlXPathQueryDidNotProduceAnyResults, xpath));
			}
			result.Value = navXmlNode;
		});
	}

	public override bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, NavOutStream data)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			XmlWriterSettings xmlWriterSettings = GetXmlWriterSettings(writeOptions);
			using XmlWriter writer = XmlWriter.Create(data.InternalStream, xmlWriterSettings);
			Value.WriteTo(writer);
		});
	}

	public override bool WriteTo(DataError errorLevel, NavXmlWriteOptions writeOptions, ByRef<NavText> data)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			XmlWriterSettings xmlWriterSettings = GetXmlWriterSettings(writeOptions);
			StringBuilder stringBuilder = new StringBuilder();
			using (XmlWriter writer = XmlWriter.Create(stringBuilder, xmlWriterSettings))
			{
				Value.WriteTo(writer);
			}
			data.Value = new NavText(data.Value.MaxLength, stringBuilder.ToString());
		});
	}

	public override bool SelectNodes(DataError errorLevel, string xpath, ByRef<NavXmlNodeList> result)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object obj = Value.XPathEvaluate(xpath);
			result.Value = XmlNodeMapper.SelectXmlNodes(obj);
		});
	}

	public override bool SelectNodes(DataError errorLevel, string xpath, NavXmlNamespaceManager namespaceManager, ByRef<NavXmlNodeList> result)
	{
		return TrappableXmlOperationExecutor.ExecuteXmlOperation(errorLevel, () =>
		{
			object obj = Value.XPathEvaluate(xpath, namespaceManager.Value);
			result.Value = XmlNodeMapper.SelectXmlNodes(obj);
		});
	}

	protected XmlWriterSettings GetXmlWriterSettings(NavXmlWriteOptions writeOptions)
	{
		XmlWriterSettings xmlWriterSettings = DefaultXmlWriterSettings.Clone();
		xmlWriterSettings.Indent = !writeOptions.ALPreserveWhitespace;
		return xmlWriterSettings;
	}
}
