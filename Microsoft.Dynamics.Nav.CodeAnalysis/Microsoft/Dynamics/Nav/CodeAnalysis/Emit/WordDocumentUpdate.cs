using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2013.Word;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class WordDocumentUpdate
{
	internal const string NamespaceIndex = "ns0";

	private readonly HashSet<string> changelog = new HashSet<string>();

	private readonly XDocument customXmlDocument;

	private readonly string customXmlNamespaceStart;

	private readonly string customXmlRootName;

	private bool changeNamespace;

	private string xmlNamespace;

	public string XmlNamespace
	{
		get
		{
			return xmlNamespace;
		}
		set
		{
			if (!string.IsNullOrEmpty(value) && !value.StartsWith("xmlns", StringComparison.Ordinal))
			{
				xmlNamespace = string.Format(CultureInfo.InvariantCulture, "xmlns:ns0='{0}'", value);
			}
			else
			{
				xmlNamespace = value;
			}
			changeNamespace = !string.IsNullOrEmpty(value);
		}
	}

	public WordDocumentUpdate(string xmlNamespace, XDocument customXmlDocument, string customXmlRootName, string customXmlNamespaceStart)
	{
		XmlNamespace = xmlNamespace;
		this.customXmlDocument = customXmlDocument;
		this.customXmlRootName = customXmlRootName;
		this.customXmlNamespaceStart = customXmlNamespaceStart;
	}

	public void Clear()
	{
		changelog.Clear();
	}

	public MemoryStream RefactorCustomXml(MemoryStream inputStream, Dictionary<string, ReportChangeWord> changeList)
	{
		Clear();
		bool flag = false;
		using (WordprocessingDocument wordprocessingDocument = WordprocessingDocument.Open(inputStream, isEditable: true))
		{
			if ((changeList != null && changeList.Count > 0) || changeNamespace)
			{
				Dictionary<string, ReportChangeWord> dataItemChanges = GetDataItemChanges(changeList);
				Dictionary<string, ReportChangeWord> fieldChanges = GetFieldChanges(changeList);
				flag = UpdateRepeaterControls(dataItemChanges, wordprocessingDocument);
				flag |= UpdateContentControls(fieldChanges, dataItemChanges, wordprocessingDocument);
			}
			flag |= WordContentControlDecorator.DecorateContentControls(wordprocessingDocument, customXmlRootName, customXmlNamespaceStart);
			if (customXmlDocument != null)
			{
				flag |= WordCustomXml.ReplaceCustomXmlPart(customXmlDocument, wordprocessingDocument, customXmlRootName, customXmlNamespaceStart);
			}
		}
		if (!flag)
		{
			return null;
		}
		if (inputStream.CanSeek)
		{
			inputStream.Seek(0L, SeekOrigin.Begin);
		}
		return inputStream;
	}

	public bool UpdateWordDocumentLayout(Stream wordLayout, Stream outputStream, XDocument oldXmlPart)
	{
		if (wordLayout == null)
		{
			throw new ArgumentNullException("wordLayout");
		}
		if (outputStream == null)
		{
			throw new ArgumentNullException("outputStream");
		}
		if (wordLayout.Length == 0L)
		{
			wordLayout.CopyTo(outputStream);
			return false;
		}
		if (oldXmlPart != null && XNode.DeepEquals(customXmlDocument, oldXmlPart))
		{
			oldXmlPart = null;
		}
		if (wordLayout.CanSeek && wordLayout.Position > 0)
		{
			wordLayout.Seek(0L, SeekOrigin.Begin);
		}
		Stream stream = ApplyFieldWordRenames(oldXmlPart, wordLayout);
		if (stream != null)
		{
			stream.Seek(0L, SeekOrigin.Begin);
			stream.CopyTo(outputStream);
			stream.Flush();
			return true;
		}
		wordLayout.CopyTo(outputStream);
		return false;
	}

	public static XDocument StripCustomXmlPart(XDocument xmlPart)
	{
		if (xmlPart == null)
		{
			return null;
		}
		XDocument xDocument = XDocument.Load(xmlPart.CreateReader());
		xDocument.Declaration = xmlPart.Declaration;
		StripElement(xDocument.Root);
		return xDocument;
	}

	private static string CalculateKey(ReportChangeWord entry)
	{
		return entry.SourceParentPath + "/" + entry.SourceName;
	}

	private bool UpdateRepeaterControls(Dictionary<string, ReportChangeWord> dataItemChanges, WordprocessingDocument wordprocessingDocument)
	{
		List<OpenXmlElement> list = new List<OpenXmlElement>();
		if (dataItemChanges.Count == 0 && !changeNamespace)
		{
			return false;
		}
		bool result = false;
		foreach (OpenXmlElement item in wordprocessingDocument.RepeaterControls())
		{
			DocumentFormat.OpenXml.Office2013.Word.DataBinding dataBinding = item.Descendants<DocumentFormat.OpenXml.Office2013.Word.DataBinding>().FirstOrDefault();
			if (dataBinding == null)
			{
				continue;
			}
			if (changeNamespace && string.Compare(dataBinding.PrefixMappings, XmlNamespace, StringComparison.Ordinal) != 0)
			{
				dataBinding.PrefixMappings = XmlNamespace;
				result = true;
			}
			string text = string.Empty;
			if (!dataItemChanges.TryGetValue(dataBinding.XPath, out var value))
			{
				foreach (KeyValuePair<string, ReportChangeWord> dataItemChange in dataItemChanges)
				{
					if (dataBinding.XPath.ToString().StartsWith(dataItemChange.Key + "[", StringComparison.Ordinal))
					{
						value = dataItemChange.Value;
						text = dataBinding.XPath.ToString().Replace(value.XPathFrom, value.XPathTo);
						break;
					}
				}
			}
			else
			{
				text = value.XPathTo;
			}
			if (value == null)
			{
				continue;
			}
			if (value.ActionOnChange == ReportChangeOptions.ChangeAction.Delete || string.IsNullOrEmpty(value.XPathTo))
			{
				if (value.ActionOnDelete == ReportChangeOptions.DeleteAction.None)
				{
					ErrorOnInvalidDelete(value);
				}
				else
				{
					OpenXmlElement parent = dataBinding.Parent.Parent;
					if (DeleteContentControl(parent, value))
					{
						list.Add(parent);
					}
				}
			}
			else
			{
				dataBinding.XPath = text;
			}
			result = true;
		}
		RemoveListElements(list);
		return result;
	}

	private static Dictionary<string, ReportChangeWord> GetDataItemChanges(Dictionary<string, ReportChangeWord> changeList)
	{
		return changeList.Where((KeyValuePair<string, ReportChangeWord> reportChangeElementWord) => reportChangeElementWord.Value.ItemType == ReportChangeOptions.ElementType.DataItem).ToDictionary((KeyValuePair<string, ReportChangeWord> reportChangeElementWord) => reportChangeElementWord.Key, (KeyValuePair<string, ReportChangeWord> reportChangeElementWord) => reportChangeElementWord.Value);
	}

	private static Dictionary<string, ReportChangeWord> GetFieldChanges(Dictionary<string, ReportChangeWord> changeList)
	{
		return changeList.Where((KeyValuePair<string, ReportChangeWord> reportChangeElementWord) => reportChangeElementWord.Value.ItemType != ReportChangeOptions.ElementType.DataItem).ToDictionary((KeyValuePair<string, ReportChangeWord> reportChangeElementWord) => reportChangeElementWord.Key, (KeyValuePair<string, ReportChangeWord> reportChangeElementWord) => reportChangeElementWord.Value);
	}

	private static void RemoveListElements(IEnumerable<OpenXmlElement> deleteList)
	{
		foreach (OpenXmlElement item in deleteList.Where((OpenXmlElement deleteItem) => deleteItem != null && deleteItem.Parent != null))
		{
			item.Remove();
		}
	}

	private bool UpdateContentControls(Dictionary<string, ReportChangeWord> fieldChangeList, Dictionary<string, ReportChangeWord> dataItemChangeList, WordprocessingDocument wordprocessingDocument)
	{
		List<OpenXmlElement> list = new List<OpenXmlElement>();
		bool result = false;
		foreach (OpenXmlElement item in wordprocessingDocument.ContentControls())
		{
			foreach (DocumentFormat.OpenXml.Wordprocessing.DataBinding item2 in from dataBinding in item.Descendants<DocumentFormat.OpenXml.Wordprocessing.DataBinding>()
				where WordDocumentXmlHelper.IsNavNameSpace(dataBinding.PrefixMappings, customXmlNamespaceStart)
				select dataBinding)
			{
				if (changeNamespace && string.Compare(item2.PrefixMappings, XmlNamespace, StringComparison.Ordinal) != 0)
				{
					item2.PrefixMappings = XmlNamespace;
					result = true;
				}
				bool flag = false;
				if (fieldChangeList != null && fieldChangeList.TryGetValue(item2.XPath, out var value))
				{
					if (value.ActionOnChange == ReportChangeOptions.ChangeAction.Delete || string.IsNullOrEmpty(value.XPathTo))
					{
						if (value.ActionOnDelete == ReportChangeOptions.DeleteAction.None)
						{
							ErrorOnInvalidDelete(value);
						}
						else if (DeleteContentControl(item, value))
						{
							list.Add(item);
						}
					}
					else
					{
						item2.XPath = value.XPathTo;
						UpdateTextInContentControl(item, value);
						result = true;
						LogRenameAction(value);
					}
					flag = true;
				}
				if (flag || dataItemChangeList == null || dataItemChangeList.Count <= 0)
				{
					continue;
				}
				foreach (KeyValuePair<string, ReportChangeWord> dataItemChange in dataItemChangeList)
				{
					if (item2.XPath.ToString().StartsWith(dataItemChange.Key + "[", StringComparison.Ordinal))
					{
						value = dataItemChange.Value;
						if (value.ActionOnChange == ReportChangeOptions.ChangeAction.Rename)
						{
							item2.XPath = item2.XPath.ToString().Replace(value.XPathFrom, value.XPathTo);
							result = true;
							break;
						}
					}
				}
			}
		}
		RemoveListElements(list);
		return result;
	}

	private void ErrorOnInvalidDelete(ReportChangeWord entry)
	{
		string item = CalculateKey(entry);
		if (!changelog.Contains(item))
		{
			changelog.Add(item);
		}
	}

	private bool DeleteContentControl(OpenXmlElement contentControl, ReportChangeWord entry)
	{
		if (contentControl == null || entry == null)
		{
			return false;
		}
		switch (entry.ActionOnDelete)
		{
		case ReportChangeOptions.DeleteAction.DeleteContent:
		case ReportChangeOptions.DeleteAction.ReplaceWithText:
			return ReplaceControlContent(contentControl, entry, "sdtContent");
		case ReportChangeOptions.DeleteAction.DeleteNode:
		case ReportChangeOptions.DeleteAction.DeleteRepeaterAndContent:
			return DeleteControlNode(contentControl, entry, "sdtContent");
		case ReportChangeOptions.DeleteAction.DeleteRepeater:
			return DeleteRepeaterControl(contentControl, entry, "sdtContent", "sdt");
		default:
			return false;
		}
	}

	private bool DeleteControlNode(OpenXmlElement contentControl, ReportChangeWord entry, string contentTag)
	{
		OpenXmlElement openXmlElement = contentControl.FirstOrDefault((OpenXmlElement variable) => variable.LocalName.Equals(contentTag));
		if (openXmlElement == null)
		{
			return false;
		}
		if (openXmlElement is SdtContentCell)
		{
			DocumentFormat.OpenXml.Wordprocessing.Text text = new DocumentFormat.OpenXml.Wordprocessing.Text(string.Empty);
			Run run = new Run(text);
			Paragraph paragraph = new Paragraph(run);
			TableCell newElement = new TableCell(paragraph);
			contentControl.InsertBeforeSelf(newElement);
		}
		LogDeleteAction(entry);
		return true;
	}

	private bool ReplaceControlContent(OpenXmlElement contentControl, ReportChangeWord entry, string contentTag)
	{
		OpenXmlElement openXmlElement = contentControl.FirstOrDefault((OpenXmlElement variable) => variable.LocalName.Equals(contentTag));
		if (openXmlElement == null)
		{
			return false;
		}
		UpdateTextInContentControl(contentControl, entry);
		foreach (OpenXmlElement childElement in openXmlElement.ChildElements)
		{
			contentControl.InsertBeforeSelf(childElement.CloneNode(deep: true));
		}
		LogDeleteAction(entry);
		return true;
	}

	private void LogDeleteAction(ReportChangeWord entry)
	{
		string item = CalculateKey(entry);
		if (!changelog.Contains(item))
		{
			changelog.Add(item);
		}
	}

	private void LogRenameAction(ReportChangeWord entry)
	{
		string item = CalculateKey(entry);
		if (!changelog.Contains(item))
		{
			changelog.Add(item);
		}
	}

	private bool DeleteRepeaterControl(OpenXmlElement contentControl, ReportChangeWord entry, string contentTag, string sdtTag)
	{
		OpenXmlElement openXmlElement = contentControl.FirstOrDefault((OpenXmlElement variable) => variable.LocalName.Equals(contentTag));
		if (openXmlElement == null)
		{
			return false;
		}
		OpenXmlElement openXmlElement2 = openXmlElement.FirstOrDefault((OpenXmlElement variable) => variable.LocalName.Equals(sdtTag));
		openXmlElement2 = (openXmlElement2 ?? openXmlElement).FirstOrDefault((OpenXmlElement variable) => variable.LocalName.Equals(contentTag));
		if (openXmlElement2 == null)
		{
			return false;
		}
		foreach (OpenXmlElement item in openXmlElement2)
		{
			contentControl.InsertBeforeSelf(item.CloneNode(deep: true));
		}
		LogDeleteAction(entry);
		return true;
	}

	private static void UpdateTextInContentControl(OpenXmlElement contentControl, ReportChangeWord entry)
	{
		OpenXmlElement openXmlElement = contentControl.ChildElements.FirstOrDefault((OpenXmlElement child) => child is SdtContentRun || child is SdtContentCell || child is SdtContentBlock);
		if (openXmlElement == null)
		{
			return;
		}
		IEnumerable<Run> source = openXmlElement.ChildElements.OfType<Run>();
		if (!source.Any())
		{
			using IEnumerator<OpenXmlElement> enumerator = openXmlElement.GetEnumerator();
			while (enumerator.MoveNext() && !(source = enumerator.Current.Descendants<Run>()).Any())
			{
			}
		}
		foreach (DocumentFormat.OpenXml.Wordprocessing.Text item in source.SelectMany((Run run) => from text in run.ChildElements.OfType<DocumentFormat.OpenXml.Wordprocessing.Text>()
			where string.Compare(text.Text, entry.SourceName, StringComparison.CurrentCulture) == 0
			select text))
		{
			if (entry.ActionOnChange == ReportChangeOptions.ChangeAction.Delete && entry.ActionOnDelete == ReportChangeOptions.DeleteAction.ReplaceWithText)
			{
				item.Text = entry.ReplacementText;
			}
			else
			{
				item.Text = entry.TargetName;
			}
		}
	}

	private Stream ApplyFieldWordRenames(XDocument oldCustomXmlPart, Stream wordLayoutSource)
	{
		Dictionary<string, ReportChangeWord> changedCustomXmlList = OfficeCustomXmlPartDiff.GetChangedCustomXmlList(oldCustomXmlPart?.Root, customXmlDocument.Root, customXmlRootName, out var value, out var forceCustomXmlUpdate);
		if (!changedCustomXmlList.Any() && !forceCustomXmlUpdate && string.IsNullOrEmpty(value))
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		using MemoryStream memoryStream2 = new MemoryStream();
		if (wordLayoutSource.CanSeek)
		{
			wordLayoutSource.Seek(0L, SeekOrigin.Begin);
		}
		wordLayoutSource.CopyTo(memoryStream2);
		memoryStream2.Seek(0L, SeekOrigin.Begin);
		if (!string.IsNullOrEmpty(value))
		{
			XmlNamespace = value;
		}
		if (RefactorCustomXml(memoryStream2, changedCustomXmlList) == null)
		{
			return null;
		}
		memoryStream2.CopyTo(memoryStream);
		if (memoryStream.CanSeek)
		{
			memoryStream.Seek(0L, SeekOrigin.Begin);
		}
		return memoryStream;
	}

	private static void StripElement(XContainer container)
	{
		foreach (XElement item in container.Elements())
		{
			if (item.HasAttributes)
			{
				item.RemoveAttributes();
			}
			if (item.HasElements)
			{
				StripElement(item);
			}
		}
	}
}
