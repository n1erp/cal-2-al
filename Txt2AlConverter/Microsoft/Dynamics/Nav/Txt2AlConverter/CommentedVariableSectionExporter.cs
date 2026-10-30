using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class CommentedVariableSectionExporter : VariableSectionExporter
{
	private const bool commentedOut = true;

	public new void Export(IElement element, IAlWriter writer)
	{
		VariableSectionExporter.ExportVariableSectionOfElement(writer, element, commentedOut: true);
	}
}
