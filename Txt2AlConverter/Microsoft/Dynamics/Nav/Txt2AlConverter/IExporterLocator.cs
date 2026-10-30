using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal interface IExporterLocator
{
	T GetService<T>(string serviceKey);

	string GetElementKey(IElement element);
}
