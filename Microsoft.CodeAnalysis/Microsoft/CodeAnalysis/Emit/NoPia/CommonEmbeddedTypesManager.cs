using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.Emit.NoPia;

internal abstract class CommonEmbeddedTypesManager
{
	public abstract bool IsFrozen { get; }

	public abstract ImmutableArray<INamespaceTypeDefinition> GetTypes(DiagnosticBag diagnostics, HashSet<string> namesOfTopLevelTypes);
}
