using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class RequestPageParameterHelper
{
	internal static Dictionary<string, NavValue> ConvertXmlToVariables(string parameters, NavForm requestPage)
	{
		Dictionary<string, NavValue> dictionary = new Dictionary<string, NavValue>();
		if (string.IsNullOrWhiteSpace(parameters))
		{
			return dictionary;
		}
		XmlDocument xmlDocument = CreateParameterXmlDocument(parameters);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//Options/Field");
		if (xmlNodeList == null)
		{
			return dictionary;
		}
		foreach (XmlNode item in xmlNodeList)
		{
			string text = string.Empty;
			foreach (XmlNode attribute in item.Attributes)
			{
				if (attribute.Name == "name")
				{
					text = attribute.Value;
					break;
				}
			}
			string innerText = item.InnerText;
			foreach (NavFormSourceExpression value2 in requestPage.SourceExpressions.Values)
			{
				if (!(value2.Name == text))
				{
					continue;
				}
				NavType navTypeFromType = NavNclTypeHelper.GetNavTypeFromType(value2.Type);
				INavValueMetadata navValueMetadata = value2.Get();
				if (navValueMetadata == null)
				{
					break;
				}
				if ((navTypeFromType == NavType.OemText || navTypeFromType == NavType.OemCode || navTypeFromType == NavType.Option || navTypeFromType == NavType.Text || navTypeFromType == NavType.Code) && !string.IsNullOrEmpty(innerText))
				{
					NavStringValue navStringValue = navValueMetadata as NavStringValue;
					if (navStringValue != null && innerText.Length > navStringValue.MaxLength)
					{
						dictionary.Add(text, NavValue.GetDefaultNavValue(navValueMetadata));
						continue;
					}
					NavValue value = NavValue.CreateNavValueFromObject(navValueMetadata, innerText);
					dictionary.Add(text, value);
					break;
				}
				NavValue navValue = ((!string.IsNullOrEmpty(innerText)) ? ALSystemVariable.EvaluateIntoNavValue(DataError.TrapError, navValueMetadata, innerText, 9) : NavValue.GetDefaultNavValue(navValueMetadata));
				if (navValue != null)
				{
					dictionary.Add(text, navValue);
					break;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001I8", Category.Runtime, Verbosity.Error, PrivacyClassification.SystemMetadata, "Unsupported data type in request page xml: {0}", navValueMetadata.NavType);
				throw new NavNCLArgumentException("NavType");
			}
		}
		return dictionary;
	}

	public static string GetReportParameterXml(string objectName, int objectId, Dictionary<string, NavValue> requestPageVariables, IEnumerable<(string name, string view)> dataItems)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringWriter stringWriter = new StringWriter(stringBuilder, CultureInfo.InvariantCulture);
		XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
		string text = "version=\"1.0\" standalone=\"yes\"";
		xmlTextWriter.WriteProcessingInstruction("xml", text);
		xmlTextWriter.WriteStartElement("ReportParameters");
		xmlTextWriter.WriteAttributeString("name", objectName);
		xmlTextWriter.WriteAttributeString("id", objectId.ToString(CultureInfo.InvariantCulture));
		if (requestPageVariables != null && requestPageVariables.Any())
		{
			xmlTextWriter.WriteStartElement("Options");
			foreach (KeyValuePair<string, NavValue> requestPageVariable in requestPageVariables)
			{
				xmlTextWriter.WriteStartElement("Field");
				xmlTextWriter.WriteAttributeString("name", requestPageVariable.Key);
				string text2 = NavFormatEvaluateHelper.Format(requestPageVariable.Value, 0, 9);
				xmlTextWriter.WriteString(text2);
				xmlTextWriter.WriteEndElement();
			}
			xmlTextWriter.WriteEndElement();
		}
		if (dataItems.Any())
		{
			xmlTextWriter.WriteStartElement("DataItems");
			foreach (var (value, text3) in dataItems)
			{
				xmlTextWriter.WriteStartElement("DataItem");
				xmlTextWriter.WriteAttributeString("name", value);
				xmlTextWriter.WriteString(text3);
				xmlTextWriter.WriteEndElement();
			}
			xmlTextWriter.WriteEndElement();
		}
		xmlTextWriter.WriteEndElement();
		xmlTextWriter.Close();
		stringWriter.Close();
		return stringBuilder.ToString();
	}

	public static void ApplyXmlViews(string parameters, Action<(string name, string view)> applyView)
	{
		if (string.IsNullOrWhiteSpace(parameters))
		{
			return;
		}
		XmlDocument xmlDocument = CreateParameterXmlDocument(parameters);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//DataItems/DataItem");
		if (xmlNodeList == null)
		{
			return;
		}
		foreach (XmlNode item in xmlNodeList)
		{
			string value = item.Attributes["name"].Value;
			string innerText = item.InnerText;
			if (!string.IsNullOrEmpty(value))
			{
				applyView((value, innerText));
			}
		}
	}

	private static XmlDocument CreateParameterXmlDocument(string parameters)
	{
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		try
		{
			string xmlDocumentData = parameters.Replace("&#x0;", string.Empty);
			XmlHelper.SafeXmlLoader(xmlDocument, xmlDocumentData);
			return xmlDocument;
		}
		catch (XmlException ex)
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidRequestPageParameter, ex.Message));
		}
	}
}
