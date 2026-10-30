using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ObjectReferencePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ObjectReferencePropertyValueSyntaxDefinition> Instance = new Lazy<ObjectReferencePropertyValueSyntaxDefinition>(() => new ObjectReferencePropertyValueSyntaxDefinition());

	public ObjectReferencePropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ObjectNameOrIdSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ObjectNameOrId",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
