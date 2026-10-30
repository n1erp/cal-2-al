using System.Collections.Immutable;

namespace Microsoft.Cci;

internal interface IImportScope
{
	IImportScope Parent { get; }

	ImmutableArray<UsedNamespaceOrType> GetUsedNamespaces();
}
