using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PropertyExpressionDestinationContextRecommender : ContextAwareSymbolRecommender
{
	internal PropertyExpressionDestinationContextRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (Context.PropertyExpressionContexts == PropertyExpressionContexts.None)
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<ISymbol> result;
		if (Context.IsRightOfNameSeparator)
		{
			result = RecommendFieldsForTable(cancellationToken);
		}
		else
		{
			IEnumerable<ISymbol> enumerable = Context.SemanticModel.Compilation.GetApplicationObjectTypeSymbolsAcrossModules(SymbolKind.Table);
			result = enumerable;
		}
		return result;
	}

	private IEnumerable<ISymbol> RecommendFieldsForTable(CancellationToken cancellationToken)
	{
		SyntaxNode parent = Context.TargetToken.Parent;
		if (parent.Kind == SyntaxKind.MemberAccessExpression)
		{
			string identifierOrLiteralValue = ((MemberAccessExpressionSyntax)parent).Expression.GetIdentifierOrLiteralValue();
			ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
			try
			{
				foreach (TableTypeSymbol item in Context.SemanticModel.Compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Table, identifierOrLiteralValue).Cast<TableTypeSymbol>())
				{
					instance.AddRange(RecommendationHelper.GetFields(Context, item, cancellationToken));
				}
				return instance.ToImmutable();
			}
			finally
			{
				instance.Free();
			}
		}
		return SpecializedCollections.EmptyEnumerable<ISymbol>();
	}
}
