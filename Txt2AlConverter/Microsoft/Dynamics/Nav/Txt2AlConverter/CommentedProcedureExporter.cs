using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class CommentedProcedureExporter : ProcedureExporter
{
	public override void Export(IElement element, IAlWriter writer)
	{
		Export(element, writer, commentedOut: true);
	}
}
