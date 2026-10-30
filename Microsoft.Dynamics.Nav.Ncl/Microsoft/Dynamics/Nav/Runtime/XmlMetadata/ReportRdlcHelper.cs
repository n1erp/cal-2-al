using System;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.Types.Report;

namespace Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

public static class ReportRdlcHelper
{
	public static string PatchRdlcWithNewDataSet(int reportId, string rdlc)
	{
		MetaReport reportMetadata = NavGlobal.MetadataProvider.GetReportMetadata(reportId);
		XmlDocument xmlDocument = ReportXmlHelper.ApplyReportDataSet(rdlc, CreateXmlDataSet(reportMetadata));
		ReportXmlHelper.ApplyReportParameters(xmlDocument, reportMetadata.Labels.Select((MetaReportLabel label) => label.Name));
		return ReportXmlHelper.SaveXmlDocument(xmlDocument);
	}

	public static void ValidateReport(int reportId, string rdlc)
	{
		try
		{
			MetaReport reportMetadata = NavGlobal.MetadataProvider.GetReportMetadata(reportId);
			if (!RdlcCompiler.CompileReport(reportMetadata.EnableExternalImages, reportMetadata.EnableHyperlinks, reportMetadata.EnableExternalAssemblies, rdlc, CreateXmlDataSet(reportMetadata), reportMetadata.Labels.Select((MetaReportLabel label) => label.Name)))
			{
				throw new NavReportLayoutValidationException();
			}
		}
		catch (Exception ex)
		{
			throw new NavReportValidationException(ex.Message, ex);
		}
	}

	private static string CreateXmlDataSet(MetaReport report)
	{
		StringBuilder stringBuilder = new StringBuilder();
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.OmitXmlDeclaration = true;
		xmlWriterSettings.Indent = true;
		XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings);
		xmlWriter.WriteStartElement("DataSets");
		xmlWriter.WriteStartElement("DataSet");
		xmlWriter.WriteAttributeString("Name", "DataSet_Result");
		xmlWriter.WriteStartElement("Fields");
		foreach (MetaDataItem dataItem in report.DataItems)
		{
			foreach (MetaDataItemColumn dataItemColumn in dataItem.DataItemColumns)
			{
				xmlWriter.WriteStartElement("Field");
				xmlWriter.WriteAttributeString("Name", dataItemColumn.Name);
				xmlWriter.WriteElementString("DataField", dataItemColumn.Name);
				xmlWriter.WriteEndElement();
			}
		}
		xmlWriter.WriteEndElement();
		xmlWriter.WriteStartElement("Query");
		xmlWriter.WriteElementString("DataSourceName", "DataSource");
		xmlWriter.WriteElementString("CommandText", string.Empty);
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.WriteEndElement();
		xmlWriter.Close();
		xmlWriter.Flush();
		return stringBuilder.ToString();
	}
}
