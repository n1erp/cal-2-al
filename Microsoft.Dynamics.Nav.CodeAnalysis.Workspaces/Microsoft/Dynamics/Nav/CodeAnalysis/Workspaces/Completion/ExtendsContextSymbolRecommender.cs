using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ExtendsContextSymbolRecommender : ContextAwareSymbolRecommender
{
	internal ExtendsContextSymbolRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.General.HasFlag(GeneralContexts.Extends))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		SymbolKind symbolKind = LookupSymbol();
		if (symbolKind == SymbolKind.Undefined)
		{
			return Context.SemanticModel.LookupSymbols(Context.LeftToken.SpanStart, LookupOptions.Default, null, null, cancellationToken);
		}
		return Context.SemanticModel.Compilation.GetApplicationObjectTypeSymbolsAcrossModules(symbolKind);
	}

	private SymbolKind LookupSymbol()
	{
		SyntaxNode parent = Context.TargetToken.Parent;
		if (parent != null)
		{
			switch (parent.Kind.ToSymbolKind())
			{
			case SymbolKind.PageExtension:
			case SymbolKind.PageCustomization:
				return SymbolKind.Page;
			case SymbolKind.TableExtension:
				return SymbolKind.Table;
			case SymbolKind.EnumExtension:
				return SymbolKind.Enum;
			}
		}
		return SymbolKind.Undefined;
	}
}
