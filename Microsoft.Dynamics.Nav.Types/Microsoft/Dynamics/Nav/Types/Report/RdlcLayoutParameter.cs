using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.Types.Report;

internal class RdlcLayoutParameter : RdlcLayoutBase, IRdlcLayoutElement
{
	private Dictionary<string, XElement> parameterCellDefinitions = new Dictionary<string, XElement>();

	private XElement numberOfColumns;

	private XElement numberOfRows;

	public RdlcLayoutParameter(RdlcDocumentContainer layout)
		: base("Parameters", (ElementType)2, layout)
	{
		Initialize();
	}

	private void Initialize()
	{
		Load();
		DocumentIsAnalyzed = false;
	}

	public new IReportChangeLogCollection Delete(string sourceName, DeleteAction actionOnDelete, string replacementText)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		RemoveParameterFromGridLayout(sourceName);
		return base.Delete(sourceName, actionOnDelete, replacementText);
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
		if (IsDefined(newName) && IsDefined(newName))
		{
			return MessageItemAllreadyDefined(oldName, newName);
		}
		XElement xElement = Definition(oldName);
		if (xElement != null)
		{
			xElement.Element(Ns + "DefaultValue")?.Element(Ns + "Values")?.Element(Ns + "Value")?.SetValue(newName);
			xElement.Element(Ns + "Prompt")?.SetValue(newName);
			xElement.Attribute("Name")?.SetValue(newName);
			UpdateDefinitionDictionary(oldName, newName, xElement);
		}
		XElement cellDefinition = GetCellDefinition(oldName);
		if (cellDefinition != null)
		{
			XElement xElement2 = cellDefinition.Element(Ns + "ParameterName");
			if (xElement2 != null && xElement2.Value != null)
			{
				xElement2.SetValue(newName);
			}
		}
		return RenameInReferences(oldName, newName);
	}

	private XElement GetCellDefinition(string name)
	{
		parameterCellDefinitions.TryGetValue(name, out var value);
		return value;
	}

	private void RemoveParameterFromGridLayout(string sourceName)
	{
		GetCellDefinition(sourceName)?.Remove();
		parameterCellDefinitions.Remove(sourceName);
		int num = 0;
		int num2 = 0;
		foreach (XElement value in parameterCellDefinitions.Values)
		{
			value?.Element(Ns + "RowIndex")?.SetValue(num++);
			value?.Element(Ns + "ColumnIndex")?.SetValue(num2);
		}
		numberOfColumns?.SetValue(num2 + 1);
		numberOfRows?.SetValue(num);
	}

	protected void Load()
	{
		XElement xElement = Document.Root.Element(Ns + "ReportParameters");
		if (xElement != null)
		{
			foreach (XElement item in xElement.Elements().ToList())
			{
				XAttribute xAttribute = item.Attribute("Name");
				if (xAttribute != null)
				{
					ItemDefinitions.Add(xAttribute.Value, item);
				}
			}
		}
		XElement xElement2 = Document.Root.Element(Ns + "ReportParametersLayout");
		if (xElement2 == null)
		{
			return;
		}
		XElement xElement3 = xElement2?.Element(Ns + "GridLayoutDefinition");
		numberOfColumns = xElement3?.Element(Ns + "NumberOfColumns");
		numberOfRows = xElement3?.Element(Ns + "NumberOfRows");
		foreach (XElement item2 in (xElement3?.Element(Ns + "CellDefinitions"))?.Elements().ToList() ?? new List<XElement>())
		{
			XElement xElement4 = item2.Element(Ns + "ParameterName");
			if (xElement4 != null && xElement4.Value != null)
			{
				parameterCellDefinitions.Add(xElement4.Value, item2);
			}
		}
	}
}
