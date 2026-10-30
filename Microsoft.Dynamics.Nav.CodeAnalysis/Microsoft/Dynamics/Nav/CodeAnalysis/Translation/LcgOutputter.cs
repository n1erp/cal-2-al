using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

internal class LcgOutputter : LanguageFileOutputter
{
	private HashSet<string> Ids { get; }

	public LcgOutputter()
	{
		Ids = new HashSet<string>();
	}

	public override void WriteLabel(XmlWriter writer, TranslationItem item)
	{
		if (Ids.Contains(item.Id))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, CodeAnalysisResources.LclFileGenerationHashCollisionError, item.Id, item.Id));
		}
		Ids.Add(item.Id);
		WriteItem(writer, item);
	}

	private static void WriteItem(XmlWriter writer, TranslationItem item)
	{
		writer.WriteStartEnd("Item", () =>
		{
			writer.WriteAttributeString("ItemId", ";" + item.Id);
			writer.WriteAttributeString("ItemType", ";" + item.ItemType);
			writer.WriteAttributeString("PsrId", "63");
			writer.WriteAttributeString("InstFlg", "true");
			writer.WriteAttributeString("Leaf", "true");
			writer.WriteStartEnd("Str", () =>
			{
				writer.WriteAttributeString("Cat", "Text");
				writer.WriteAttributeString("CP", "DOS");
				writer.WriteStartEnd("Val", () =>
				{
					writer.WriteCData(LanguageFileUtilities.EscapeCdata(item.SourceString));
				});
			});
			writer.WriteStartEnd("Disp", () =>
			{
				writer.WriteAttributeString("Icon", "Str");
				writer.WriteAttributeString("Expand", "true");
			});
			writer.WriteStartEnd("Cmts", () =>
			{
				writer.WriteStartEnd("Cmt", () =>
				{
					writer.WriteAttributeString("Name", "Dev");
					string text = (string.IsNullOrEmpty(item.Comment) ? string.Empty : (LanguageFileUtilities.EscapeCdata(item.Comment) + "]D;]A;"));
					string text2 = (item.Locked ? "{Locked}]D;]A;" : string.Empty);
					string text3 = string.Empty;
					if (item.MaxSize.HasValue)
					{
						text3 = "{MaxLength=" + item.MaxSize.Value.ToString(CultureInfo.InvariantCulture) + "}]D;]A;";
					}
					string text4 = string.Empty;
					if (item.LegacyId != null)
					{
						text4 = "(LegacyId=" + item.LegacyId + ")";
					}
					string text5 = "(LookupHint=" + item.NamedId + ")";
					writer.WriteCData(text + text2 + text3 + text4 + text5);
				});
			});
		});
	}

	public override void WriteDefaultNodesAndContinue(XmlWriter writer, string appName, string targetLanguage, Action p)
	{
		targetLanguage = targetLanguage ?? "en-US";
		writer.WriteStartDocument();
		writer.WriteStartEnd("LCX", () =>
		{
			writer.WriteAttributeString("SchemaVersion", "6.0");
			writer.WriteAttributeString("Name", appName + ".g");
			writer.WriteAttributeString("PsrId", "63");
			writer.WriteAttributeString("SrcCul", "en-US");
			writer.WriteAttributeString("TgtCul", targetLanguage);
			writer.WriteStartEnd("OwnedComments", () =>
			{
				writer.WriteStartEnd("Cmt", () =>
				{
					writer.WriteAttributeString("Name", "Dev");
				});
				writer.WriteStartEnd("Cmt", () =>
				{
					writer.WriteAttributeString("Name", "LcxAdmin");
				});
				writer.WriteStartEnd("Cmt", () =>
				{
					writer.WriteAttributeString("Name", "Rccx");
				});
			});
			p();
		}, "http://schemas.microsoft.com/locstudio/2006/6/lcx");
		writer.WriteEndDocument();
		writer.Flush();
		writer.Close();
	}
}
