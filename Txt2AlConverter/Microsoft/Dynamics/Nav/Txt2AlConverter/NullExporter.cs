using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal sealed class NullExporter : IElementExporter
{
	public void Export(IElement element, IAlWriter writer)
	{
	}
}
