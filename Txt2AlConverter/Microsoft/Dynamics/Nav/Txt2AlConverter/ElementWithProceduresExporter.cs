using System.Linq;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class ElementWithProceduresExporter : IElementExporter
{
	private readonly IElementExporter nextExporter;

	private readonly AlExportOptions options;

	private readonly ProcedureExporter procedureExporter;

	public ElementWithProceduresExporter(IElementExporter nextExporter, AlExportOptions options)
	{
		procedureExporter = new ProcedureExporter();
		this.nextExporter = nextExporter;
		this.options = options;
	}

	public void Export(IElement element, IAlWriter writer)
	{
		ExportProcedures(element, writer);
		nextExporter.Export(element, writer);
	}

	private void ExportProcedures(IElement element, IAlWriter writer)
	{
		foreach (IElement item in element.GetSubElements().Where(IsProcedure))
		{
			procedureExporter.Export(item, writer);
		}
	}

	private bool IsProcedure(IElement p)
	{
		if (options.TableDataOnly)
		{
			return false;
		}
		if (p.ElementTypeInfo.ElementType != ElementType.Procedure)
		{
			if (p.ElementTypeInfo.ElementType == ElementType.Event)
			{
				return p.IsDotNetEventTrigger();
			}
			return false;
		}
		return true;
	}
}
