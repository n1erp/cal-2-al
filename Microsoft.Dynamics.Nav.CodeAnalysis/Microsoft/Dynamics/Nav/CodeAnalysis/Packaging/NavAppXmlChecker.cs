using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal abstract class NavAppXmlChecker : NavAppChecker
{
	private readonly List<(NavAppSourceFile sourceFile, XDocument xmlDocument)> filteredDocuments = new List<(NavAppSourceFile, XDocument)>();

	protected EmbeddedXmlResource SchemaResource { get; }

	protected string Classification { get; }

	internal NavAppXmlChecker(EmbeddedXmlResource schemaResource, string classification, IFileSystem fileSystem)
		: base(new string[1] { ".xml" }, fileSystem)
	{
		SchemaResource = schemaResource;
		Classification = classification;
	}

	private static XDocument ResolveDocument(EmbeddedXmlResource schema, XDocument doc)
	{
		bool valid = true;
		doc.Validate(schema.XmlSchema, (object sender, ValidationEventArgs args) =>
		{
			valid = false;
		});
		if (!valid)
		{
			return null;
		}
		return doc;
	}

	protected override IEnumerable<NavAppSourceFile> FilterSourceFiles(IEnumerable<NavAppSourceFile> sources, ICollection<NavAppError> errors)
	{
		List<NavAppSourceFile> filteredSources = new List<NavAppSourceFile>();
		NavAppResult navAppResult = ReadSchemas();
		if (navAppResult.Success)
		{
			sources.Do((NavAppSourceFile source) =>
			{
				NavAppResult<XDocument> navAppResult2 = ReadDocument(source.FileLocation);
				if (navAppResult2.Success)
				{
					XDocument value = navAppResult2.Value;
					if (ResolveDocument(SchemaResource, value) != null)
					{
						filteredDocuments.Add((source, value));
						filteredSources.Add(source);
					}
				}
				else
				{
					errors.Add(navAppResult2.NavAppError);
				}
			});
		}
		else
		{
			errors.Add(navAppResult.NavAppError);
		}
		return filteredSources;
	}

	protected sealed override NavAppCheckerResult InternalVerify(NavAppCheckerResult result)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		return ValidateDocuments(result, filteredDocuments);
	}

	protected virtual NavAppCheckerResult ValidateDocuments(NavAppCheckerResult result, IEnumerable<(NavAppSourceFile sourceFile, XDocument xmlDocument)> documentMap)
	{
		return result;
	}

	private NavAppResult ReadSchemas()
	{
		List<NavAppError> list = new List<NavAppError>();
		list.AddRange(SchemaResource.LoadErrors.Select((string e) => NavAppError.FromMessage(e, NavAppErrorType.InvalidOperation)));
		if (!list.Any())
		{
			return NavAppResult.Ok();
		}
		return NavAppResult.Fail<XmlSchemaSet>(NavAppError.FromMultipleErrors("Schema Load Errors", list));
	}

	private NavAppResult<XDocument> ReadDocument(string file)
	{
		try
		{
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.XmlResolver = null;
			XDocument value;
			using (MemoryStream input = new MemoryStream(FileSystem.ReadBytes(file)))
			{
				using XmlReader reader = XmlReader.Create(input, xmlReaderSettings);
				value = XDocument.Load(reader);
			}
			return NavAppResult.Ok(value);
		}
		catch (XmlException ex)
		{
			return NavAppResult.Fail<XDocument>(NavAppError.FromMessage(string.Format(CultureInfo.CurrentCulture, NavAppCheckerStrings.XmlLoadError, Classification, file, ex.Message), NavAppErrorType.InvalidOperation));
		}
	}
}
