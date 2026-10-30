using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Common.Printing;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MetaReport
{
	public delegate MasterPage CreateRequestForm(PageDefinition page, KeyValuePair<int, string>[] captionML, int id);

	public delegate MasterPage RemoveItemsOnPageBasedOnLicenseAndApplicationArea(MasterPage masterPage);

	public const string DataSetName = "DataSet";

	public const string DataTableName = "Result";

	private readonly KeyValuePair<int, string>[] captionML;

	private static readonly XmlSerializer PageDefinitionSerializer = new XmlSerializer(typeof(PageDefinition));

	private readonly List<MetaDataItem> dataItems = new List<MetaDataItem>();

	private readonly List<MetaDataItem> rootdataItems = new List<MetaDataItem>();

	private readonly Lazy<MasterPage> lazyMasterPage;

	private readonly Lazy<byte[]> lazyRDLCData;

	private readonly KeyValuePair<int, string>[] additionalSearchTermsML;

	private PageDefinition requestPageDefinition;

	private long versionNumber;

	private int metadataAppGroupId;

	private int languageAppGroupId;

	private RemoveItemsOnPageBasedOnLicenseAndApplicationArea removeItemsOnPageBasedOnLicenseAndApplicationArea;

	public bool ShowPrintStatus { get; private set; }

	public bool UseRequestForm { get; private set; }

	public bool UseSystemPrinter { get; private set; }

	public bool ProcessingOnly { get; private set; }

	public bool EnableExternalImages { get; private set; }

	public bool EnableHyperlinks { get; private set; }

	public bool EnableExternalAssemblies { get; private set; }

	public int? DefaultPaperSourceKindRaw { get; private set; }

	public int? FirstPagePaperSourceKindRaw { get; private set; }

	public int? LastPagePaperSourceKindRaw { get; private set; }

	public UsageCategory UsageCategory { get; private set; }

	public string ApplicationArea { get; private set; }

	public IReadOnlyList<MetaReportLabel> Labels { get; private set; }

	public MasterPage RequestFormMetadata
	{
		get
		{
			EnsureMasterPageLoaded();
			if (removeItemsOnPageBasedOnLicenseAndApplicationArea != null)
			{
				return removeItemsOnPageBasedOnLicenseAndApplicationArea(lazyMasterPage.Value.Clone() as MasterPage);
			}
			return lazyMasterPage.Value;
		}
	}

	public string RDLCData { get; private set; }

	public byte[] RDLCRuntimeData
	{
		get
		{
			if (!string.IsNullOrEmpty(RDLCData))
			{
				return lazyRDLCData.Value;
			}
			return null;
		}
	}

	public byte[] WordLayout { get; private set; }

	public PageDefinition RequestPageDefinition
	{
		get
		{
			EnsureMasterPageLoaded();
			return requestPageDefinition;
		}
	}

	public long VersionNumber
	{
		get
		{
			EnsureMasterPageLoaded();
			return versionNumber;
		}
	}

	[XmlIgnore]
	public IReadOnlyCollection<KeyValuePair<int, string>> CaptionML => captionML;

	[XmlIgnore]
	public IReadOnlyCollection<KeyValuePair<int, string>> AdditionalSearchTermsML => additionalSearchTermsML;

	public int Id { get; private set; }

	public string Name { get; private set; }

	public string ExternalizedName { get; private set; }

	public TransactionType TransactionType { get; private set; }

	public PreviewMode PreviewMode { get; private set; }

	public DefaultLayout DefaultLayout { get; private set; }

	public bool PdfFontEmbedding { get; private set; }

	public string WordMergeDataItem { get; private set; }

	public IReadOnlyList<MetaDataItem> DataItems => dataItems;

	public IReadOnlyList<MetaDataItem> RootDataItems => rootdataItems;

	public int MetadataAppGroupId => metadataAppGroupId;

	public int LanguageAppGroupId => languageAppGroupId;

	internal static PageDefinition DeserializePageDefinition(string requestPageXml, ObjectType objType, int id, int languageAppGroupId)
	{
		PageDefinition pageDefinition = (PageDefinition)PageDefinitionSerializer.Deserialize(XmlHelper.SafeXmlReader(new StringReader(requestPageXml)));
		if (LanguageProvider.Provider.HasDataFor(objType, id, languageAppGroupId))
		{
			pageDefinition.MergeMultiLanguage(objType, id, languageAppGroupId);
		}
		return pageDefinition;
	}

	private void EnsureMasterPageLoaded()
	{
		_ = lazyMasterPage.Value;
	}

	public MetaReport(XmlElement root, CreateRequestForm createRequestForm, int metadataAppGroupId, int languageAppGroupId, RemoveItemsOnPageBasedOnLicenseAndApplicationArea removeItemsOnPageBasedOnLicenseAndApplicationArea = null)
	{
		MetaReport metaReport = this;
		this.removeItemsOnPageBasedOnLicenseAndApplicationArea = removeItemsOnPageBasedOnLicenseAndApplicationArea;
		string requestPageXml = null;
		UseRequestForm = true;
		ShowPrintStatus = true;
		PdfFontEmbedding = ServerUserSettings.Instance.ReportPdfFontEmbedding.Value;
		this.metadataAppGroupId = metadataAppGroupId;
		this.languageAppGroupId = languageAppGroupId;
		List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
		List<KeyValuePair<int, string>> list2 = new List<KeyValuePair<int, string>>();
		List<MetaReportLabel> list3 = new List<MetaReportLabel>();
		Dictionary<string, string> uniqueDataItemNames = new Dictionary<string, string>();
		foreach (XmlNode childNode in root.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "ID":
				Id = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "NAME":
				Name = childNode.InnerText;
				ExternalizedName = MetaBase.ExternalizeName(Name);
				break;
			case "WORDMERGEDATAITEM":
				WordMergeDataItem = MetaBase.ExternalizeName(childNode.InnerText);
				break;
			case "REPORTVERSIONOBSOLETE":
				throw new NavReportVersionObsoleteException(Id);
			case "LABELS":
				AssignLabels(childNode, list3);
				break;
			case "CAPTIONML":
				AssignCaptionML(childNode, list);
				break;
			case "SHOWPRINTSTATUS":
				ShowPrintStatus = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "USEREQFORM":
			case "USEREQUESTPAGE":
				UseRequestForm = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "USESYSTEMPRINTER":
				UseSystemPrinter = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "PROCESSINGONLY":
				ProcessingOnly = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "ENABLEEXTERNALIMAGES":
				EnableExternalImages = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "ENABLEHYPERLINKS":
				EnableHyperlinks = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "ENABLEEXTERNALASSEMBLIES":
				EnableExternalAssemblies = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "PAPERSOURCEDEFAULTPAGE":
				DefaultPaperSourceKindRaw = ParsePaperSourceKindRaw(childNode.InnerText);
				break;
			case "PAPERSOURCEFIRSTPAGE":
				FirstPagePaperSourceKindRaw = ParsePaperSourceKindRaw(childNode.InnerText);
				break;
			case "PAPERSOURCELASTPAGE":
				LastPagePaperSourceKindRaw = ParsePaperSourceKindRaw(childNode.InnerText);
				break;
			case "TRANSACTIONTYPE":
				AssignTransactionType(childNode);
				break;
			case "DATAITEM":
			{
				MetaDataItem item = new MetaDataItem(childNode, Id, uniqueDataItemNames, languageAppGroupId);
				dataItems.Add(item);
				break;
			}
			case "REQUESTPAGE":
				requestPageXml = AssignRequestPage(childNode, createRequestForm, requestPageXml);
				break;
			case "PREVIEWMODE":
				AssignPreviewMode(childNode);
				break;
			case "DEFAULTLAYOUT":
				AssignDefaultLayout(childNode);
				break;
			case "PDFFONTEMBEDDING":
				AssignPdfFontEmbedding(childNode);
				break;
			case "RDL":
				RDLCData = (ProcessingOnly ? null : childNode?.InnerText);
				break;
			case "WORDLAYOUT":
				AssignWordLayout(childNode);
				break;
			case "APPLICATIONAREA":
				ApplicationArea = childNode.InnerText;
				break;
			case "USAGECATEGORY":
				UsageCategory = ParseUsageCategory(childNode.InnerText);
				break;
			case "ADDITIONALSEARCHTERMML":
				AssignCaptionML(childNode, list2);
				break;
			}
		}
		list = MultiLanguageDictionaryMerge(list, 0, ObjectType.Report, Id, -1, languageAppGroupId);
		list2 = MultiLanguageDictionaryMerge(list2, 7, ObjectType.Report, Id, -1, languageAppGroupId);
		captionML = list.ToArray();
		additionalSearchTermsML = list2.ToArray();
		Labels = list3;
		BuildDataItemTree();
		lazyMasterPage = new Lazy<MasterPage>(() =>
		{
			MasterPage masterPage = null;
			if (!string.IsNullOrEmpty(requestPageXml) && createRequestForm != null)
			{
				metaReport.requestPageDefinition = DeserializePageDefinition(requestPageXml, ObjectType.Report, metaReport.Id, languageAppGroupId);
				masterPage = createRequestForm(metaReport.requestPageDefinition, metaReport.captionML, metaReport.Id);
			}
			metaReport.versionNumber = masterPage?.VersionNumber ?? 0;
			return masterPage;
		});
		lazyRDLCData = new Lazy<byte[]>(() => (!string.IsNullOrEmpty(metaReport.RDLCData)) ? CheckAndPrepareRdlc(metaReport.Id, metaReport.RDLCData) : null);
	}

	private static UsageCategory ParseUsageCategory(string innerText)
	{
		try
		{
			return (UsageCategory)Enum.Parse(typeof(UsageCategory), innerText);
		}
		catch (ArgumentException)
		{
			return UsageCategory.None;
		}
	}

	private static string AssignRequestPage(XmlNode node, CreateRequestForm createRequestForm, string requestPageXml)
	{
		if (createRequestForm != null)
		{
			requestPageXml = node.FirstChild.OuterXml;
		}
		return requestPageXml;
	}

	private static void AssignCaptionML(XmlNode node, List<KeyValuePair<int, string>> captionMLList)
	{
		if (!node.HasChildNodes)
		{
			return;
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Attributes != null && childNode.Attributes.Count > 0)
			{
				int key = int.Parse(childNode.Attributes["Id"].InnerText, CultureInfo.InvariantCulture);
				string innerText = childNode.InnerText;
				captionMLList.Add(new KeyValuePair<int, string>(key, innerText));
			}
		}
	}

	private void AssignLabels(XmlNode node, List<MetaReportLabel> metaReportLabels)
	{
		if (!node.HasChildNodes)
		{
			return;
		}
		if (Labels == null)
		{
			Labels = new List<MetaReportLabel>();
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			MetaReportLabel item = new MetaReportLabel(childNode, this);
			metaReportLabels.Add(item);
		}
	}

	private void AssignTransactionType(XmlNode node)
	{
		switch (node.InnerText.ToUpper(CultureInfo.InvariantCulture))
		{
		case "UPDATENOLOCKS":
			TransactionType = TransactionType.UpdateNoLocks;
			break;
		case "UPDATE":
			TransactionType = TransactionType.Update;
			break;
		case "SNAPSHOT":
			TransactionType = TransactionType.Snapshot;
			break;
		case "BROWSE":
			TransactionType = TransactionType.Browse;
			break;
		case "REPORT":
			TransactionType = TransactionType.Report;
			break;
		default:
			throw new ArgumentException("TransactionType");
		}
	}

	private void BuildDataItemTree()
	{
		foreach (MetaDataItem dataItem in dataItems)
		{
			dataItem.BuildDataItemChildren(dataItems);
			if (dataItem.DataItemIndent == 0)
			{
				rootdataItems.Add(dataItem);
			}
		}
	}

	public static byte[] CheckAndPrepareRdlc(int reportId, string rdl)
	{
		if (string.IsNullOrEmpty(rdl))
		{
			throw new NavNCLReportDefinitionInvalidException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidReportMetadata, reportId));
		}
		if (!rdl.Contains("http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition") && !rdl.Contains("http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition"))
		{
			throw new NavReportVersionObsoleteException(reportId);
		}
		rdl = ReportRdlPatchingHelper.PatchRdlVariablesWithParameters(rdl);
		return Encoding.UTF8.GetBytes(rdl);
	}

	private void AssignWordLayout(XmlNode node)
	{
		if (!string.IsNullOrEmpty(node?.InnerText))
		{
			string innerText = node.InnerText;
			if (!string.IsNullOrEmpty(innerText))
			{
				try
				{
					WordLayout = Convert.FromBase64String(innerText);
					return;
				}
				catch (FormatException innerException)
				{
					throw new NavNCLReportDefinitionInvalidException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidReportMetadata, Id), innerException);
				}
			}
			return;
		}
		throw new NavNCLReportDefinitionInvalidException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidReportMetadata, Id));
	}

	private void AssignPreviewMode(XmlNode node)
	{
		switch (node.InnerText.ToUpper(CultureInfo.InvariantCulture))
		{
		case "NORMAL":
			PreviewMode = PreviewMode.Normal;
			break;
		case "PRINTLAYOUT":
			PreviewMode = PreviewMode.PrintLayout;
			break;
		default:
			throw new ArgumentException("PreviewMode");
		}
	}

	private void AssignPdfFontEmbedding(XmlNode node)
	{
		switch (node.InnerText.ToUpper(CultureInfo.InvariantCulture))
		{
		case "YES":
			PdfFontEmbedding = true;
			break;
		case "NO":
			PdfFontEmbedding = false;
			break;
		}
	}

	private void AssignDefaultLayout(XmlNode node)
	{
		switch (node.InnerText.ToUpper(CultureInfo.InvariantCulture))
		{
		case "RDLC":
			DefaultLayout = DefaultLayout.RDLC;
			break;
		case "WORD":
			DefaultLayout = DefaultLayout.Word;
			break;
		default:
			throw new ArgumentException("DefaultLayout");
		}
	}

	internal static List<KeyValuePair<int, string>> MultiLanguageDictionaryMerge(List<KeyValuePair<int, string>> attributeValue, int attributeId, ObjectType objType, int objId, int fieldId, int languageAppGroupId)
	{
		if (LanguageProvider.Provider.HasDataFor(objType, objId, languageAppGroupId))
		{
			LanguageElements globalLanguageElements = LanguageProvider.Provider.GetGlobalLanguageElements(objType, objId, ControlType.Column, fieldId, attributeId, requestPage: false);
			LanguageElements navAppLanguageElements = LanguageProvider.Provider.GetNavAppLanguageElements(objType, objId, ControlType.Column, fieldId, attributeId, requestPage: false, languageAppGroupId);
			DoMergeForAppElements(attributeValue, navAppLanguageElements);
			DoMergeForGlobalElements(attributeValue, globalLanguageElements);
		}
		return attributeValue;
	}

	private static void DoMergeForAppElements(List<KeyValuePair<int, string>> attributeValue, LanguageElements appElements)
	{
		if (appElements == null)
		{
			return;
		}
		for (int i = 0; i < appElements.LanguageIds.Count; i++)
		{
			int num = appElements.LanguageIds[i];
			string text = appElements.Messages[i];
			if (string.IsNullOrWhiteSpace(text))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < attributeValue.Count; j++)
			{
				if (flag)
				{
					break;
				}
				if (attributeValue[j].Key == num)
				{
					flag = true;
					if (attributeValue[j].Value != text)
					{
						attributeValue[j] = new KeyValuePair<int, string>(num, text);
					}
				}
			}
			if (!flag)
			{
				attributeValue.Add(new KeyValuePair<int, string>(num, text));
			}
		}
	}

	private static void DoMergeForGlobalElements(List<KeyValuePair<int, string>> attributeValue, LanguageElements globalElements)
	{
		if (globalElements == null)
		{
			return;
		}
		for (int i = 0; i < globalElements.LanguageIds.Count; i++)
		{
			string value = (string.IsNullOrEmpty(globalElements.Messages[i]) ? "\"\"" : globalElements.Messages[i]);
			KeyValuePair<int, string> item = new KeyValuePair<int, string>(globalElements.LanguageIds[i], value);
			if (!attributeValue.Contains(item))
			{
				attributeValue.Add(item);
			}
		}
	}

	internal static int? ParsePaperSourceKindRaw(string inputString)
	{
		if (!string.IsNullOrEmpty(inputString))
		{
			if (inputString.StartsWith("Custom ", StringComparison.Ordinal))
			{
				int result = 0;
				if (int.TryParse(inputString.Substring("Custom ".Length), out result))
				{
					return result + 255;
				}
				return null;
			}
			try
			{
				return (int)(PaperSourceKind)Enum.Parse(typeof(PaperSourceKind), inputString);
			}
			catch (ArgumentException)
			{
				return null;
			}
		}
		return null;
	}
}
