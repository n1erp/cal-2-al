using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class PermissionSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<PermissionSyntaxDefinition> Instance = new Lazy<PermissionSyntaxDefinition>(() => new PermissionSyntaxDefinition());

	public PermissionSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ObjectType",
			Kinds = ImmutableArray.Create<SyntaxKind>(SyntaxKind.TableDataKeyword, SyntaxKind.TableKeyword, SyntaxKind.CodeunitKeyword, SyntaxKind.PageKeyword, SyntaxKind.ReportKeyword, SyntaxKind.QueryKeyword, SyntaxKind.XmlPortKeyword, SyntaxKind.SystemKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = ObjectNameOrIdSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ObjectReference",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "EqualsToken",
			Kinds = ImmutableArray.Create(SyntaxKind.EqualsToken)
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "Permissions",
			Kinds = ImmutableArray.Create(SyntaxKind.IdentifierToken)
		}));
	}
}
