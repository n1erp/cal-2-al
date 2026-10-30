using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class CompilationUnitSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<CompilationUnitSyntaxDefinition> Instance = new Lazy<CompilationUnitSyntaxDefinition>(() => new CompilationUnitSyntaxDefinition());

	public CompilationUnitSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ObjectCompilationUnitSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
