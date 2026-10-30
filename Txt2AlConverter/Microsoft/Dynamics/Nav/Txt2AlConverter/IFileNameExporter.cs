using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public interface IFileNameExporter
{
	string GetAlFileName(ApplicationObject ao);

	string GetRdlcLayoutFileName(IElement element);

	string GetWordLayoutFileName(IElement element);
}
