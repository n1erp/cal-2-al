using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class AttributeArgumentSymbolRecommender : ContextAwareSymbolRecommender
{
	private const int ObjectTypePosition = 0;

	private const int EventNamePosition = 2;

	private const int ElementNamePosition = 3;

	public AttributeArgumentSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.General.HasFlag(GeneralContexts.AttributeArgumentList) || !Context.TargetToken.IsKind(SyntaxKind.OpenParenToken, SyntaxKind.CommaToken))
		{
			return base.RecommendSymbolsAsync(cancellationToken);
		}
		AttributeArgumentListSyntax attributeArgumentListSyntax = MemberSyntaxContext.FindAttributeArgumentListContext(Context.TargetToken);
		AttributeTypeInfo attributeTypeInfo = ((AttributeSymbol)Context.SemanticModel.GetDeclaredSymbol(attributeArgumentListSyntax.Parent))?.AttributeInfo;
		if (attributeTypeInfo == null || attributeTypeInfo.Kind == AttributeKind.EventSubscriber)
		{
			return base.RecommendSymbolsAsync(cancellationToken);
		}
		int num = attributeArgumentListSyntax.CalculateAttributeArgumentPosition(Context.Position);
		if (num < attributeTypeInfo.Parameters.Length)
		{
			AttributeParameter attributeParameter = attributeTypeInfo.Parameters[num];
			if (attributeParameter.Type.Kind == SymbolKind.OptionType)
			{
				return Task.FromResult(SpecializedCollections.SingletonEnumerable(attributeParameter.Type).Cast<ISymbol>());
			}
		}
		return base.RecommendSymbolsAsync(cancellationToken);
	}
}
