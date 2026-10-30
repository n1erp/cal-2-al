using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal interface IElementExporter
{
	void Export(IElement element, IAlWriter writer);
}
