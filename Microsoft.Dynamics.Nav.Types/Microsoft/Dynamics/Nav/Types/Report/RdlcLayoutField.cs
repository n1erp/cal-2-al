using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Types.Report;

internal class RdlcLayoutField : RdlcLayoutBase, IRdlcLayoutElement
{
	public RdlcLayoutField(RdlcDocumentContainer layout)
		: base("Fields", (ElementType)0, layout)
	{
		Initialize();
	}

	private void Initialize()
	{
		Load();
		DocumentIsAnalyzed = false;
	}

	protected void Load()
	{
		if (Document == null || Document.Root == null)
		{
			return;
		}
		XElement xElement = Document.Root.Element(Ns + "DataSets");
		if (xElement == null)
		{
			return;
		}
		XElement xElement2 = xElement.Element(Ns + "DataSet");
		if (xElement2 == null)
		{
			return;
		}
		XElement xElement3 = xElement2.Element(Ns + "Fields");
		if (xElement3 == null)
		{
			return;
		}
		foreach (XElement item in xElement3.Elements().ToList())
		{
			XElement xElement4 = item.Element(Ns + "DataField");
			XAttribute xAttribute = item.Attribute("Name");
			if (xElement4 != null && xAttribute != null)
			{
				ItemDefinitions.Add(xAttribute.Value, item);
			}
		}
	}

	public new void ResolveReference(bool throwOnErrors, XElement value)
	{
		base.ResolveReference(throwOnErrors, value);
	}

	public IReportChangeLogCollection Rename(string oldName, string newName)
	{
		if (!IsDefined(oldName) && !IsDefined(newName))
		{
			return MessageItemNotDefined(oldName);
		}
		if (IsDefined(newName) && IsDefined(oldName))
		{
			return MessageItemAllreadyDefined(oldName, newName);
		}
		XElement xElement = Definition(oldName);
		if (xElement != null)
		{
			xElement.Element(Ns + "DataField")?.SetValue(newName);
			xElement.Attribute("Name")?.SetValue(newName);
			UpdateDefinitionDictionary(oldName, newName, xElement);
		}
		return RenameInReferences(oldName, newName);
	}

	public new IReportChangeLogCollection Delete(string sourceName, DeleteAction actionOnDelete, string replacementText)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return base.Delete(sourceName, actionOnDelete, replacementText);
	}
}
