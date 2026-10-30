using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class FieldModifySymbolRecommmender : ContextAwareSymbolRecommender
{
	internal FieldModifySymbolRecommmender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.Table.HasFlag(TableContexts.ModifyContext))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (DeclaringObject == null || DeclaringObject.Kind != SymbolKind.TableExtension)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		TableExtensionTypeSymbol tableExtension = (TableExtensionTypeSymbol)DeclaringObject;
		TableTypeSymbol tableTypeSymbol = (TableTypeSymbol)tableExtension.Target;
		if (tableTypeSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		SyntaxToken previousToken = Context.LeftToken.GetPreviousToken();
		int position = (tableTypeSymbol.IsSourceSymbol() ? tableTypeSymbol.GetLocation().SourceSpan.Start : 0);
		if (previousToken.Parent.GetFirstParent(SyntaxKind.FieldGroupExtensionList) != null)
		{
			return from s in Context.SemanticModel.LookupSymbols(position, LookupOptions.MustBeFieldGroup, tableTypeSymbol, null, cancellationToken)
				where s.IsKind(SymbolKind.FieldGroup) && tableExtension != s.ContainingSymbol
				select s;
		}
		return from s in Context.SemanticModel.LookupSymbols(position, LookupOptions.Default, tableTypeSymbol, null, cancellationToken)
			where s.IsKind(SymbolKind.Field) && tableExtension != s.ContainingSymbol
			select s;
	}
}
