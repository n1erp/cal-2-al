using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.AL.Common;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class ReportRdlcUtilities
{
	private enum ElementType
	{
		Parameter,
		DataItem,
		Column
	}

	public const string NavCustomXmlRootName = "NavWordReportXmlPart";

	public const string NavCustomXmlNamespaceStart = "urn:microsoft-dynamics-nav/reports/";

	private static readonly CultureInfo MetadataCulture = CultureInfo.InvariantCulture;

	private static readonly XName ElementIdXName = "ElementId";

	private static readonly XName ElementTypeXName = "ElementType";

	private static readonly XName DataTypeXName = "DataType";

	public static string FormatXmlNamespace(string externalizedName, int reportId)
	{
		return string.Format(MetadataCulture, "{0}{1}/{2}/", "urn:microsoft-dynamics-nav/reports/", externalizedName, reportId);
	}

	internal static string GetOfficeCustomXmlPart(SourceReportTypeSymbol report, bool writeMergeInformation)
	{
		string text = FormatXmlNamespace(report.Name, report.Id);
		Dictionary<string, int> dictionary = null;
		StringBuilder stringBuilder = new StringBuilder();
		using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings
		{
			Encoding = Encoding.Unicode,
			Indent = true
		}))
		{
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("NavWordReportXmlPart", text);
			IEnumerable<ReportLabelSymbol> membersOfKind = report.GetMembersOfKind<ReportLabelSymbol>(SymbolKind.ReportLabel);
			IEnumerable<ReportDataItemSymbol> membersOfKind2 = report.GetMembersOfKind<ReportDataItemSymbol>(SymbolKind.ReportDataItem);
			if (membersOfKind.Any())
			{
				xmlWriter.WriteStartElement("Labels");
				foreach (ReportLabelSymbol item in membersOfKind.OrderBy((ReportLabelSymbol label) => label.Name))
				{
					int num = item.Id;
					if (num == 0)
					{
						if (dictionary == null)
						{
							dictionary = PopulateFieldDictionary(membersOfKind2);
						}
						num = GetLabelIdFromField(dictionary, item.Name);
					}
					WriteColumnElement(xmlWriter, text, item.Name, ElementType.Parameter, num.ToString(MetadataCulture), NavTypeKind.String, writeMergeInformation);
				}
				xmlWriter.WriteEndElement();
			}
			foreach (ReportDataItemSymbol item2 in membersOfKind2)
			{
				WriteDataItem(xmlWriter, text, item2, writeMergeInformation);
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndDocument();
		}
		return stringBuilder.ToString();
	}

	internal static void GenerateRdlcLayout(ReportTypeSymbol report, IFileSystem fileSystem, string rdlFileName)
	{
		string dataSets = GetDataSets(report);
		Encoding uTF = Encoding.UTF8;
		IEnumerable<string> reportParameters = GetReportParameters(report);
		string reportLayout = ((!fileSystem.Exists(rdlFileName)) ? GetDefaultRdl() : GetCurrentRdl(fileSystem, rdlFileName, uTF).OuterXml);
		XmlDocument xmlDocument = ReportXmlHelper.ApplyReportParameters(ReportXmlHelper.ApplyReportDataSet(reportLayout, dataSets), reportParameters);
		XmlWriterSettings settings = new XmlWriterSettings
		{
			Indent = true,
			IndentChars = "  ",
			NewLineChars = "\r\n",
			NewLineHandling = NewLineHandling.Replace,
			Encoding = uTF
		};
		byte[] content = uTF.GetBytes(xmlDocument.OuterXml);
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings);
			xmlDocument.Save(xmlWriter);
			xmlWriter.Flush();
			memoryStream.Position = 0L;
			content = memoryStream.ToArray();
		}
		fileSystem.CreateDirectoryForFile(rdlFileName);
		fileSystem.WriteBytes(rdlFileName, content);
	}

	private static IEnumerable<string> GetReportParameters(ReportTypeSymbol report)
	{
		IEnumerable<string> first = from s in report.GetMembersOfKind<ReportLabelSymbol>(SymbolKind.ReportLabel)
			select s.Name;
		IEnumerable<string> second = from c in report.FlattenedDataItems.SelectMany((ReportDataItemSymbol r) => r.GetMembersOfKind<SourceReportColumnSymbol>(SymbolKind.ReportColumn))
			where c.GetBoundPropertyValue<BoundLiteralPropertyValue>(PropertyKind.IncludeCaption)?.ValueObject as bool? == true
			select c.Name + "Caption";
		return first.Concat(second);
	}

	private static Dictionary<string, int> PopulateFieldDictionary(IEnumerable<ReportDataItemSymbol> rootDataItems)
	{
		Dictionary<string, int> seed = new Dictionary<string, int>();
		return rootDataItems.Aggregate(seed, PopulateFieldDictionary);
	}

	private static Dictionary<string, int> PopulateFieldDictionary(Dictionary<string, int> labelMap, ReportDataItemSymbol dataItem)
	{
		foreach (SourceReportColumnSymbol item in dataItem.GetMembersOfKind<SourceReportColumnSymbol>(SymbolKind.ReportColumn))
		{
			labelMap.Add(item.Name + "Caption", item.Id);
		}
		return dataItem.GetMembersOfKind<SourceReportDataItemSymbol>(SymbolKind.ReportDataItem).Aggregate(labelMap, PopulateFieldDictionary);
	}

	private static int GetLabelIdFromField(Dictionary<string, int> labelMap, string label)
	{
		if (labelMap.TryGetValue(label, out var value))
		{
			return value;
		}
		return -1;
	}

	private static void WriteColumnElement(XmlWriter xmlWriter, string xmlNamespace, string name, ElementType elementType, string identifier, NavTypeKind navType, bool writeMergeInformation)
	{
		xmlWriter.WriteStartElement(name, xmlNamespace);
		if (writeMergeInformation)
		{
			xmlWriter.WriteAttributeString(ElementTypeXName.LocalName, elementType.ToString());
			xmlWriter.WriteAttributeString(ElementIdXName.LocalName, identifier);
			xmlWriter.WriteAttributeString(DataTypeXName.LocalName, navType.ToString());
		}
		if (navType != NavTypeKind.Blob)
		{
			xmlWriter.WriteValue(name);
		}
		xmlWriter.WriteEndElement();
	}

	private static void WriteDataItem(XmlWriter xmlWriter, string xmlNamespace, ReportDataItemSymbol dataItem, bool writeMergeInformation)
	{
		string text = NameTransformations.MangleIntoValidXmlIdentifier(dataItem.Name);
		xmlWriter.WriteStartElement(text, xmlNamespace);
		if (writeMergeInformation)
		{
			int memberId = IdSpace.GetMemberId(text);
			xmlWriter.WriteAttributeString(ElementTypeXName.LocalName, ElementType.DataItem.ToString());
			xmlWriter.WriteAttributeString(ElementIdXName.LocalName, memberId.ToString(MetadataCulture));
		}
		foreach (SourceReportColumnSymbol item in from column in dataItem.GetMembersOfKind<SourceReportColumnSymbol>(SymbolKind.ReportColumn)
			orderby column.Name
			select column)
		{
			WriteColumnElement(xmlWriter, xmlNamespace, item.Name, ElementType.Column, item.Id.ToString(MetadataCulture), item.NavTypeKind, writeMergeInformation);
		}
		foreach (SourceReportDataItemSymbol item2 in dataItem.GetMembersOfKind<SourceReportDataItemSymbol>(SymbolKind.ReportDataItem))
		{
			WriteDataItem(xmlWriter, xmlNamespace, item2, writeMergeInformation);
		}
		xmlWriter.WriteEndElement();
	}

	private static XmlDocument GetCurrentRdl(IFileSystem fileSystem, string rdlFileName, Encoding fileEncoding)
	{
		string text = CleanOfPreambleAndToString(fileSystem.ReadBytes(rdlFileName), fileEncoding);
		XmlDocument xmlDocument = new XmlDocument();
		if (!string.IsNullOrEmpty(text))
		{
			xmlDocument.LoadXml(text);
			XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("DataSets");
			if (elementsByTagName.Count > 0)
			{
				elementsByTagName[0].ParentNode.RemoveChild(elementsByTagName[0]);
			}
		}
		return xmlDocument;
	}

	private static string CleanOfPreambleAndToString(byte[] fileBytes, Encoding fileEncoding)
	{
		byte[] preamble = fileEncoding.GetPreamble();
		bool flag = preamble.Length <= fileBytes.Length;
		for (int i = 0; i < preamble.Length && i < fileBytes.Length; i++)
		{
			flag = flag && fileBytes[i] == preamble[i];
		}
		string text = fileEncoding.GetString(fileBytes);
		if (flag)
		{
			return text.Substring(1);
		}
		return text;
	}

	private static string GetDataSets(ReportTypeSymbol report)
	{
		StringBuilder stringBuilder = new StringBuilder();
		XmlWriter writer = XmlWriter.Create(stringBuilder);
		try
		{
			IEnumerable<SourceReportDataItemSymbol> codeMembersOfKind = report.GetCodeMembersOfKind<SourceReportDataItemSymbol>(SymbolKind.ReportDataItem);
			IEnumerable<SourceReportColumnSymbol> columns = codeMembersOfKind.SelectMany((SourceReportDataItemSymbol item) => item.GetMembersOfKind<SourceReportColumnSymbol>(SymbolKind.ReportColumn));
			ObjectMetadataEmitHelper.WriteStartEndElement(writer, "DataSets", () =>
			{
				ObjectMetadataEmitHelper.WriteStartEndElement(writer, "DataSet", () =>
				{
					writer.WriteAttributeString("Name", "DataSet_Result");
					ObjectMetadataEmitHelper.WriteStartEndElement(writer, "Fields", () =>
					{
						foreach (SourceReportColumnSymbol column in columns)
						{
							ObjectMetadataEmitHelper.WriteStartEndElement(writer, "Field", () =>
							{
								writer.WriteAttributeString("Name", column.Name);
								ObjectMetadataEmitHelper.WriteStartEndElement(writer, "DataField", () =>
								{
									writer.WriteString(column.Name);
								});
							});
							if (IsDecimalFormatter(column))
							{
								ObjectMetadataEmitHelper.WriteStartEndElement(writer, "Field", () =>
								{
									writer.WriteAttributeString("Name", column.Name + "Format");
									ObjectMetadataEmitHelper.WriteStartEndElement(writer, "DataField", () =>
									{
										writer.WriteString(column.Name + "Format");
									});
								});
							}
						}
					});
					ObjectMetadataEmitHelper.WriteStartEndElement(writer, "Query", () =>
					{
						ObjectMetadataEmitHelper.WriteStartEndElement(writer, "DataSourceName", () =>
						{
							writer.WriteString("DataSource");
						});
						ObjectMetadataEmitHelper.WriteStartEndElement(writer, "CommandText", () =>
						{
						});
					});
				});
			});
			writer.Flush();
			return stringBuilder.ToString();
		}
		finally
		{
			if (writer != null)
			{
				((IDisposable)writer).Dispose();
			}
		}
	}

	private static bool IsDecimalFormatter(SourceReportColumnSymbol column)
	{
		return column.SourceExpression.Type.NavTypeKind == NavTypeKind.Decimal;
	}

	internal static string GetDefaultRdl()
	{
		using StreamReader streamReader = new StreamReader(typeof(ReportMetadataEmitter).GetTypeInfo().Assembly.GetManifestResourceStream("Microsoft.Dynamics.Nav.CodeAnalysis.Report.rdlc"));
		return streamReader.ReadToEnd();
	}
}
