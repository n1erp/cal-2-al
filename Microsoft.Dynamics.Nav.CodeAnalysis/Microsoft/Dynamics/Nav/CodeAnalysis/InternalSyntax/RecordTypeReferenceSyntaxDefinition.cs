using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class RecordTypeReferenceSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<RecordTypeReferenceSyntaxDefinition> Instance = new Lazy<RecordTypeReferenceSyntaxDefinition>(() => new RecordTypeReferenceSyntaxDefinition());

	public RecordTypeReferenceSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = ArraySyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = true,
			Name = "Array",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = DataTypeSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "DataType",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}, new SyntaxNodeSlotDefinition
		{
			Node = null,
			IsList = false,
			IsOptional = true,
			Name = "Temporary",
			Kinds = ImmutableArray.Create(SyntaxKind.TemporaryKeyword)
		}));
	}
}
