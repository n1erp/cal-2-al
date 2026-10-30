using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class WordDocumentNewHandler
{
	public static void EmptyDocument(Stream newXmlPart, Stream outputStream)
	{
		if (newXmlPart == null)
		{
			throw new ArgumentNullException("newXmlPart");
		}
		if (outputStream == null)
		{
			throw new ArgumentNullException("outputStream");
		}
		using MemoryStream memoryStream = new MemoryStream();
		using (WordprocessingDocument wordprocessingDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
		{
			wordprocessingDocument.AddMainDocumentPart();
			List<OpenXmlElement> list = new List<OpenXmlElement>();
			Document document = new Document
			{
				MCAttributes = new MarkupCompatibilityAttributes
				{
					Ignorable = "w14 w15 wp14"
				}
			};
			document.AddNamespaceDeclaration("wpc", "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas");
			document.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
			document.AddNamespaceDeclaration("o", "urn:schemas-microsoft-com:office:office");
			document.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
			document.AddNamespaceDeclaration("m", "http://schemas.openxmlformats.org/officeDocument/2006/math");
			document.AddNamespaceDeclaration("v", "urn:schemas-microsoft-com:vml");
			document.AddNamespaceDeclaration("wp14", "http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing");
			document.AddNamespaceDeclaration("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing");
			document.AddNamespaceDeclaration("w10", "urn:schemas-microsoft-com:office:word");
			document.AddNamespaceDeclaration("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
			document.AddNamespaceDeclaration("w14", "http://schemas.microsoft.com/office/word/2010/wordml");
			document.AddNamespaceDeclaration("w15", "http://schemas.microsoft.com/office/word/2012/wordml");
			document.AddNamespaceDeclaration("wpg", "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup");
			document.AddNamespaceDeclaration("wpi", "http://schemas.microsoft.com/office/word/2010/wordprocessingInk");
			document.AddNamespaceDeclaration("wne", "http://schemas.microsoft.com/office/word/2006/wordml");
			document.AddNamespaceDeclaration("wps", "http://schemas.microsoft.com/office/word/2010/wordprocessingShape");
			Paragraph paragraph = new Paragraph
			{
				RsidParagraphAddition = "0055490F",
				RsidRunAdditionDefault = "0055490F"
			};
			list.Add(new BookmarkStart
			{
				Name = "_GoBack",
				Id = "0"
			});
			list.Add(new BookmarkEnd
			{
				Id = "0"
			});
			paragraph.Append(list);
			list.Clear();
			SectionProperties sectionProperties = new SectionProperties
			{
				RsidR = "0055490F"
			};
			list.Add(new PageSize
			{
				Width = 11906u,
				Height = 16838u
			});
			list.Add(new PageMargin
			{
				Top = 1701,
				Right = 1134u,
				Bottom = 1701,
				Left = 1134u,
				Header = 708u,
				Footer = 708u,
				Gutter = 0u
			});
			list.Add(new Columns
			{
				Space = "708"
			});
			list.Add(new DocGrid
			{
				LinePitch = 360
			});
			sectionProperties.Append(list);
			list.Clear();
			Body body = new Body();
			body.Append(paragraph);
			body.Append(sectionProperties);
			document.Append(body);
			wordprocessingDocument.MainDocumentPart.Document = document;
			wordprocessingDocument.AddNewPart<ExtendedFilePropertiesPart>().Properties = new Properties();
			CreateDocumentSettings(wordprocessingDocument);
			CustomXmlPart customXmlPart = wordprocessingDocument.MainDocumentPart.AddCustomXmlPart(CustomXmlPartType.CustomXml);
			newXmlPart.Seek(0L, SeekOrigin.Begin);
			customXmlPart.FeedData(newXmlPart);
		}
		memoryStream.Seek(0L, SeekOrigin.Begin);
		memoryStream.CopyTo(outputStream);
	}

	private static void CreateDocumentSettings(WordprocessingDocument wordprocessingDocument)
	{
		DocumentSettingsPart documentSettingsPart = wordprocessingDocument.MainDocumentPart.AddNewPart<DocumentSettingsPart>();
		Settings settings = new Settings
		{
			MCAttributes = new MarkupCompatibilityAttributes
			{
				Ignorable = "w14 w15"
			}
		};
		settings.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
		settings.AddNamespaceDeclaration("o", "urn:schemas-microsoft-com:office:office");
		settings.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		settings.AddNamespaceDeclaration("m", "http://schemas.openxmlformats.org/officeDocument/2006/math");
		settings.AddNamespaceDeclaration("v", "urn:schemas-microsoft-com:vml");
		settings.AddNamespaceDeclaration("w10", "urn:schemas-microsoft-com:office:word");
		settings.AddNamespaceDeclaration("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
		settings.AddNamespaceDeclaration("w14", "http://schemas.microsoft.com/office/word/2010/wordml");
		settings.AddNamespaceDeclaration("w15", "http://schemas.microsoft.com/office/word/2012/wordml");
		settings.AddNamespaceDeclaration("sl", "http://schemas.openxmlformats.org/schemaLibrary/2006/main");
		Compatibility compatibility = new Compatibility();
		List<OpenXmlElement> newChildren = new List<OpenXmlElement>
		{
			new UseFarEastLayout(),
			new CompatibilitySetting
			{
				Name = CompatSettingNameValues.CompatibilityMode,
				Uri = "http://schemas.microsoft.com/office/word",
				Val = "15"
			},
			new CompatibilitySetting
			{
				Name = CompatSettingNameValues.OverrideTableStyleFontSizeAndJustification,
				Uri = "http://schemas.microsoft.com/office/word",
				Val = "1"
			},
			new CompatibilitySetting
			{
				Name = CompatSettingNameValues.EnableOpenTypeFeatures,
				Uri = "http://schemas.microsoft.com/office/word",
				Val = "1"
			},
			new CompatibilitySetting
			{
				Name = CompatSettingNameValues.DoNotFlipMirrorIndents,
				Uri = "http://schemas.microsoft.com/office/word",
				Val = "1"
			},
			new CompatibilitySetting
			{
				Name = CompatSettingNameValues.DifferentiateMultirowTableHeaders,
				Uri = "http://schemas.microsoft.com/office/word",
				Val = "1"
			}
		};
		compatibility.Append(newChildren);
		settings.Append(compatibility);
		documentSettingsPart.Settings = settings;
	}
}
