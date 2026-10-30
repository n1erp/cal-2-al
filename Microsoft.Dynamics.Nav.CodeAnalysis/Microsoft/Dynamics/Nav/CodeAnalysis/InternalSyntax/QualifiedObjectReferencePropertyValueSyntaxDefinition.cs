using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class QualifiedObjectReferencePropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<QualifiedObjectReferencePropertyValueSyntaxDefinition> Instance = new Lazy<QualifiedObjectReferencePropertyValueSyntaxDefinition>(() => new QualifiedObjectReferencePropertyValueSyntaxDefinition());

	public QualifiedObjectReferencePropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = false,
			Name = "ObjectType",
			Kinds = ImmutableArray.Create<SyntaxKind>(SyntaxKind.TableKeyword, SyntaxKind.PageKeyword, SyntaxKind.CodeunitKeyword, SyntaxKind.ReportKeyword, SyntaxKind.XmlPortKeyword, SyntaxKind.QueryKeyword)
		}, new SyntaxNodeSlotDefinition
		{
			Node = ObjectNameOrIdSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "ObjectNameOrId",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
