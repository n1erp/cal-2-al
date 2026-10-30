using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class EmbeddedXmlResource
{
	internal const string ResourceFolder = ".Packaging.Checking.";

	private static readonly Assembly ExecutingAssembly = typeof(EmbeddedXmlResource).GetTypeInfo().Assembly;

	private static readonly string ResourcePath = ExecutingAssembly.GetName().Name + ".Packaging.Checking.";

	private readonly List<string> loadErrors = new List<string>();

	public IEnumerable<string> LoadErrors => loadErrors;

	public XmlSchemaSet XmlSchema { get; private set; }

	[SuppressMessage("Microsoft.Security.Xml", "CA3053:UseXmlSecureResolver", Justification = "Reason: XmlResolver is null")]
	public EmbeddedXmlResource(string relativeResourcePath)
	{
		using Stream input = GetManifestResourceStream(relativeResourcePath);
		XmlSchemaSet set = new XmlSchemaSet();
		set.Add(System.Xml.Schema.XmlSchema.Read(XmlReader.Create(input, new XmlReaderSettings
		{
			XmlResolver = null
		}), (object sender, ValidationEventArgs args) =>
		{
			loadErrors.Add(args.Message);
			set = null;
		}));
		XmlSchema = set;
	}

	public static Stream GetManifestResourceStream(string relativeResourcePath)
	{
		return ExecutingAssembly.GetManifestResourceStream(ResourcePath + relativeResourcePath);
	}
}
