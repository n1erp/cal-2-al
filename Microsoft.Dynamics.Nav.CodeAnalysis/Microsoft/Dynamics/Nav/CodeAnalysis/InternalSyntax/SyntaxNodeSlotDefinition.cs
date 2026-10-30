using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class SyntaxNodeSlotDefinition
{
	public SyntaxNodeDefinition Node { get; set; }

	public string Name { get; set; }

	public bool IsList { get; set; }

	public bool IsOptional { get; set; }

	public ImmutableArray<SyntaxKind> Kinds { get; set; }
}
