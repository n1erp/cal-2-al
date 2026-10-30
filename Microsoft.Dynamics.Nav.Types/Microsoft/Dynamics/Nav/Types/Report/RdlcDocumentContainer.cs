using System.IO;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Types.Report;

internal class RdlcDocumentContainer
{
	public XDocument Document { get; private set; }

	public XElement Root => Document.Root;

	public XNamespace Ns { get; private set; }

	public RdlcDocumentContainer(XDocument document)
	{
		Document = document;
		if (document.Root != null)
		{
			Ns = document.Root.Name.NamespaceName;
		}
	}

	public void Save(Stream stream)
	{
		Document.Save(stream, SaveOptions.DisableFormatting);
	}
}
