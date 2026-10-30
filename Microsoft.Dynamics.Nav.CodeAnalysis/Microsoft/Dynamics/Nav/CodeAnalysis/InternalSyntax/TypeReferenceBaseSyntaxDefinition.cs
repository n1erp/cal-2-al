using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class TypeReferenceBaseSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<TypeReferenceBaseSyntaxDefinition> Instance = new Lazy<TypeReferenceBaseSyntaxDefinition>(() => new TypeReferenceBaseSyntaxDefinition());

	public TypeReferenceBaseSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = SimpleTypeReferenceSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = RecordTypeReferenceSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}, new SyntaxNodeSlotDefinition
		{
			Node = DotNetTypeReferenceSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Kinds = ImmutableArray<SyntaxKind>.Empty
		}));
	}
}
