using Microsoft.Dynamics.Nav.MetaMetaModel;

namespace Microsoft.Dynamics.Nav.MetaModel;

public interface IElementKeySegment
{
	string Id { get; }

	string Name { get; }

	ElementType ElementType { get; }
}
