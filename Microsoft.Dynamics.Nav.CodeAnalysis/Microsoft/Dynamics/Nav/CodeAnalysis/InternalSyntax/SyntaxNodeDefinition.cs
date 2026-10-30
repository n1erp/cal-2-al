using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal abstract class SyntaxNodeDefinition
{
	public virtual Lazy<ImmutableArray<SyntaxNodeSlotDefinition>> Slots { get; protected set; }
}
