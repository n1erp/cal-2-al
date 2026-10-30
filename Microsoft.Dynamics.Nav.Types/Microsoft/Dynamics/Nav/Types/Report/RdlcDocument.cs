using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Dynamics.Nav.Types.Report.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Report;

internal class RdlcDocument
{
	private readonly RdlcDocumentContainer container;

	private readonly RdlcLayoutField fields;

	private readonly RdlcLayoutParameter parameters;

	public RdlcLayoutField Fields => fields;

	public RdlcLayoutParameter Parameters => parameters;

	public RdlcDocument(string document, bool throwOnErrors)
		: this(XDocument.Parse(document), throwOnErrors)
	{
	}

	public RdlcDocument(Stream document, bool throwOnErrors)
		: this(XDocument.Load(document, LoadOptions.PreserveWhitespace), throwOnErrors)
	{
	}

	private RdlcDocument(XDocument document, bool throwOnErrors)
	{
		container = new RdlcDocumentContainer(document);
		fields = new RdlcLayoutField(container);
		parameters = new RdlcLayoutParameter(container);
		new RdlcDocumentResolver(container, new List<IRdlcLayoutElement> { fields, parameters }).Resolve(throwOnErrors);
	}

	public bool IsDefined(ElementType type, string name)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Select(type)?.IsDefined(name) ?? false;
	}

	public bool IsReferenceed(ElementType type, string name)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Select(type)?.IsReferenced(name) ?? false;
	}

	public IReportChangeLogCollection Rename(ElementType type, string oldName, string newName)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return Select(type)?.Rename(oldName, newName);
	}

	public IReportChangeLogCollection Delete(ElementType type, string name, DeleteAction actionOnDelete, string replacementText)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return Select(type)?.Delete(name, actionOnDelete, replacementText);
	}

	public void Save(Stream reportLayoutUpdate)
	{
		container.Save(reportLayoutUpdate);
	}

	private IRdlcLayoutElement Select(ElementType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected I4, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		return (int)type switch
		{
			0 => (IRdlcLayoutElement)fields, 
			2 => parameters, 
			1 => null, 
			_ => throw new InvalidMetadataElementTypeException(((object)type/*cast due to constrained. prefix*/).ToString()), 
		};
	}
}
