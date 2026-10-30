using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PermissionPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PermissionPropertyValueSyntaxDefinition> Instance = new Lazy<PermissionPropertyValueSyntaxDefinition>(() => new PermissionPropertyValueSyntaxDefinition());

	public PermissionPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = PermissionSyntaxDefinition.Instance.Value,
			IsList = true,
			IsOptional = false,
			Name = "PermissionProperties",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
