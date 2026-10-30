using System;
using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

internal class XliffOutputter : LanguageFileOutputter
{
	public override void WriteLabel(XmlWriter writer, TranslationItem item)
	{
		WriteTransUnit(writer, item);
	}

	private static void WriteTransUnit(XmlWriter writer, TranslationItem item)
	{
		writer.WriteStartEnd("trans-unit", () =>
		{
			writer.WriteAttributeString("id", item.Id);
			if (item.MaxSize.HasValue)
			{
				writer.WriteAttributeString("maxwidth", item.MaxSize.Value.ToString(CultureInfo.InvariantCulture));
			}
			writer.WriteAttributeString("size-unit", "char");
			writer.WriteAttributeString("translate", item.Locked ? "no" : "yes");
			writer.WriteAttributeString("xml", "space", null, "preserve");
			writer.WriteStartEnd("source", () =>
			{
				writer.WriteString(item.SourceString);
			});
			if (!string.IsNullOrEmpty(item.TranslatedString))
			{
				writer.WriteStartEnd("target", () =>
				{
					writer.WriteString(item.TranslatedString);
				});
			}
			writer.WriteStartEnd("note", () =>
			{
				writer.WriteAttributeString("from", "Developer");
				writer.WriteAttributeString("annotates", "general");
				writer.WriteAttributeString("priority", "2");
				writer.WriteValue(item.Comment);
			});
			writer.WriteStartEnd("note", () =>
			{
				writer.WriteAttributeString("from", "Xliff Generator");
				writer.WriteAttributeString("annotates", "general");
				writer.WriteAttributeString("priority", "3");
				writer.WriteValue(item.NamedId);
			});
			if (!string.IsNullOrEmpty(item.LegacyId))
			{
				writer.WriteStartEnd("note", () =>
				{
					writer.WriteAttributeString("from", "Transitional");
					writer.WriteAttributeString("annotates", "general");
					writer.WriteAttributeString("priority", "4");
					writer.WriteValue(item.LegacyId);
				});
			}
		});
	}

	public override void WriteDefaultNodesAndContinue(XmlWriter writer, string appName, string targetLanguage, Action groupContentWriter)
	{
		targetLanguage = targetLanguage ?? "en-US";
		writer.WriteStartDocument();
		writer.WriteStartEnd("xliff", () =>
		{
			writer.WriteAttributeString("version", "1.2");
			writer.WriteAttributeString("xmlns", "urn:oasis:names:tc:xliff:document:1.2");
			writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			writer.WriteAttributeString("xsi", "schemaLocation", null, "urn:oasis:names:tc:xliff:document:1.2 xliff-core-1.2-transitional.xsd");
			writer.WriteStartEnd("file", () =>
			{
				writer.WriteAttributeString("datatype", "xml");
				writer.WriteAttributeString("source-language", "en-US");
				writer.WriteAttributeString("target-language", targetLanguage);
				writer.WriteAttributeString("original", appName);
				writer.WriteStartEnd("body", () =>
				{
					writer.WriteStartEnd("group", () =>
					{
						writer.WriteAttributeString("id", "body");
						groupContentWriter();
					});
				});
			});
		}, "urn:oasis:names:tc:xliff:document:1.2");
		writer.WriteEndDocument();
		writer.Flush();
		writer.Close();
	}
}
