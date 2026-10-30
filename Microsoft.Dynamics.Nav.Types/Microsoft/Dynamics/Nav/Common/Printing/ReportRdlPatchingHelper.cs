using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Common.Printing;

public static class ReportRdlPatchingHelper
{
	private class RdlParameter
	{
		public string BuiltInToReplace { get; }

		public string ParameterName { get; }

		public string DefaultValue { get; }

		public string Type { get; }

		public Func<bool> ReplacementEnabled { get; }

		public RdlParameter(string builtInToReplace, string parameterName, string defaultValue, string type, Func<bool> replacementEnabled)
		{
			BuiltInToReplace = builtInToReplace;
			ParameterName = parameterName;
			DefaultValue = defaultValue;
			Type = type;
			ReplacementEnabled = replacementEnabled;
		}

		public RdlParameter(string builtInToReplace, string parameterName, string defaultValue, string type)
			: this(builtInToReplace, parameterName, defaultValue, type, () => true)
		{
		}
	}

	public const string ReportUserIdParameterName = "NavUserIdAE6AEB42076449B78A3E78A5EE366751";

	public const string ReportExecutionTimeParameterName = "ExecutionTime7587CE19257C4965B7E6066BDFF21EE3";

	public const string RdlUserUserId = "User!UserID";

	public const string RdlExecutionTime = "Globals!ExecutionTime";

	private const string ReportParametersNodeSelector = "//a:ReportParameters";

	private const string ReportSectionsNodeSelector = "//a:ReportSections";

	private const string RdlReportParameters = "ReportParameters";

	private const string RdlReportParameter = "ReportParameter";

	private const string RdlName = "Name";

	private const string RdlDataType = "DataType";

	private const string RdlDefaultValue = "DefaultValue";

	private const string RdlPrompt = "Prompt";

	private const string RdlValue = "Value";

	private const string RdlValues = "Values";

	private static RdlParameter[] RdlParameters => new RdlParameter[2]
	{
		new RdlParameter("User!UserID", "NavUserIdAE6AEB42076449B78A3E78A5EE366751", "NavUserIdAE6AEB42076449B78A3E78A5EE366751", "String"),
		new RdlParameter("Globals!ExecutionTime", "ExecutionTime7587CE19257C4965B7E6066BDFF21EE3", "=Globals!ExecutionTime", "DateTime", () => ServerUserSettings.Instance.ReplaceReportExecutionTimeWithClientTime.Value)
	};

	public static Dictionary<string, string> CreateDefaultParameters(string userId, string executionTime)
	{
		return new Dictionary<string, string>
		{
			{ "NavUserIdAE6AEB42076449B78A3E78A5EE366751", userId },
			{ "ExecutionTime7587CE19257C4965B7E6066BDFF21EE3", executionTime }
		};
	}

	public static string PatchRdlVariablesWithParameters(string rdl)
	{
		List<RdlParameter> list = new List<RdlParameter>();
		RdlParameter[] rdlParameters = RdlParameters;
		foreach (RdlParameter rdlParameter in rdlParameters)
		{
			if (rdlParameter.ReplacementEnabled() && rdl.Contains(rdlParameter.BuiltInToReplace))
			{
				rdl = rdl.Replace(rdlParameter.BuiltInToReplace, string.Format(CultureInfo.InvariantCulture, "Parameters!{0}.Value", rdlParameter.ParameterName));
				list.Add(rdlParameter);
			}
		}
		if (list.Count > 0)
		{
			rdl = SaveXmlDocument(AddReportParameters(XmlHelper.SafeXmlLoader(null, rdl), list));
		}
		return rdl;
	}

	private static XmlDocument AddReportParameters(XmlDocument document, IList<RdlParameter> parameters)
	{
		if (parameters.Count == 0)
		{
			return document;
		}
		string rDLCSchemaName = GetRDLCSchemaName(document);
		XmlNode xmlNode = FindNodeInDocument(document, "//a:ReportParameters");
		if (xmlNode == null)
		{
			xmlNode = document.CreateElement(string.Empty, "ReportParameters", rDLCSchemaName);
			XmlNode xmlNode2 = FindNodeInDocument(document, "//a:ReportSections");
			if (xmlNode2 != null)
			{
				document.DocumentElement.InsertAfter(xmlNode, xmlNode2);
			}
			else
			{
				document.DocumentElement.AppendChild(xmlNode);
			}
		}
		foreach (RdlParameter parameter in parameters)
		{
			XmlElement xmlElement = document.CreateElement(string.Empty, "ReportParameter", rDLCSchemaName);
			xmlElement.SetAttribute("Name", parameter.ParameterName);
			XmlElement xmlElement2 = document.CreateElement(string.Empty, "DataType", rDLCSchemaName);
			xmlElement2.AppendChild(document.CreateTextNode(parameter.Type));
			xmlElement.AppendChild(xmlElement2);
			XmlElement xmlElement3 = document.CreateElement(string.Empty, "DefaultValue", rDLCSchemaName);
			xmlElement.AppendChild(xmlElement3);
			XmlElement xmlElement4 = document.CreateElement(string.Empty, "Values", rDLCSchemaName);
			xmlElement3.AppendChild(xmlElement4);
			XmlElement xmlElement5 = document.CreateElement(string.Empty, "Value", rDLCSchemaName);
			xmlElement5.AppendChild(document.CreateTextNode(parameter.DefaultValue));
			xmlElement4.AppendChild(xmlElement5);
			XmlElement xmlElement6 = document.CreateElement(string.Empty, "Prompt", rDLCSchemaName);
			xmlElement6.AppendChild(document.CreateTextNode(parameter.ParameterName));
			xmlElement.AppendChild(xmlElement6);
			xmlNode.AppendChild(xmlElement);
		}
		ReportXmlHelper.AlignReportParametersLayout(document);
		return document;
	}

	private static string GetRDLCSchemaName(IXPathNavigable docNavigator)
	{
		XPathNavigator xPathNavigator = docNavigator.CreateNavigator();
		xPathNavigator.MoveToFirstChild();
		return xPathNavigator.NamespaceURI;
	}

	private static XmlNode FindNodeInDocument(XmlDocument document, string searchString)
	{
		string rDLCSchemaName = GetRDLCSchemaName(document);
		XmlNamespaceManager namespaceManager = GetNamespaceManager(document, rDLCSchemaName);
		return document.SelectSingleNode(searchString, namespaceManager);
	}

	private static XmlNamespaceManager GetNamespaceManager(XmlDocument document, string schemaName)
	{
		XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(document.NameTable);
		xmlNamespaceManager.AddNamespace("a", schemaName);
		return xmlNamespaceManager;
	}

	public static string SaveXmlDocument(IXPathNavigable document)
	{
		if (!(document is XmlDocument xmlDocument))
		{
			return string.Empty;
		}
		using MemoryStream memoryStream = new MemoryStream();
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Encoding = Encoding.UTF8,
			Indent = true,
			CloseOutput = false
		};
		using (XmlWriter w = XmlWriter.Create(memoryStream, settings))
		{
			xmlDocument.WriteContentTo(w);
		}
		memoryStream.Seek(0L, SeekOrigin.Begin);
		using StreamReader streamReader = new StreamReader(memoryStream);
		return streamReader.ReadToEnd();
	}
}
