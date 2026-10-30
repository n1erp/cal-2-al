using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ObjectCompilationUnitSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ObjectCompilationUnitSyntaxDefinition> Instance = new Lazy<ObjectCompilationUnitSyntaxDefinition>(() => new ObjectCompilationUnitSyntaxDefinition());

	public ObjectCompilationUnitSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ObjectSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "Objects",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "EndOfFileToken",
			Kinds = ImmutableArray.Create(SyntaxKind.EndOfFileToken)
		}));
	}
}
