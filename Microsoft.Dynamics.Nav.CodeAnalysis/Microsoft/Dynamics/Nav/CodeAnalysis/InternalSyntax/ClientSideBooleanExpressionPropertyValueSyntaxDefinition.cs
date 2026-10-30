using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class ClientSideBooleanExpressionPropertyValueSyntaxDefinition : SyntaxNodeDefinition
{
	public static Lazy<ClientSideBooleanExpressionPropertyValueSyntaxDefinition> Instance = new Lazy<ClientSideBooleanExpressionPropertyValueSyntaxDefinition>(() => new ClientSideBooleanExpressionPropertyValueSyntaxDefinition());

	public ClientSideBooleanExpressionPropertyValueSyntaxDefinition()
	{
		Slots = new Lazy<ImmutableArray<SyntaxNodeSlotDefinition>>(() => ImmutableArray.Create(new SyntaxNodeSlotDefinition
		{
			Node = CodeExpressionSyntaxDefinition.Instance.Value,
			IsList = false,
			IsOptional = false,
			Name = "Expression",
			Kinds = ImmutableArray.Create<SyntaxKind>()
		}));
	}
}
