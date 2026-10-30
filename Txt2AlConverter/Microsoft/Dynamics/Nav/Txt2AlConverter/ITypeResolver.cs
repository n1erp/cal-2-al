using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal interface ITypeResolver
{
	string ResolveType(IElement element);
}
