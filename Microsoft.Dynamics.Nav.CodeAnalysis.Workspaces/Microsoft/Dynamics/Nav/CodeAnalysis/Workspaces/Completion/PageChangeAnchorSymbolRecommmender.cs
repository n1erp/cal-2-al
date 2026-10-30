using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PageChangeAnchorSymbolRecommmender : ContextAwareSymbolRecommender
{
	internal PageChangeAnchorSymbolRecommmender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<ISymbol>> RecommendSymbolsAsync(CancellationToken cancellationToken)
	{
		if (!Context.Page.HasFlag(PageContexts.PageChangeAnchor))
		{
			return await base.RecommendSymbolsAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (DeclaringObject == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (!DeclaringObject.IsKind(SymbolKind.PageExtension, SymbolKind.PageCustomization) && (!Context.Page.HasFlag(PageContexts.View) || !DeclaringObject.IsKind(SymbolKind.Page)))
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		SyntaxToken previousToken = Context.LeftToken.GetPreviousToken();
		if (!previousToken.Kind.IsPageChangeKeyword())
		{
			previousToken = previousToken.GetPreviousToken();
			if (!previousToken.Kind.IsPageChangeKeyword())
			{
				return SpecializedCollections.EmptyEnumerable<ISymbol>();
			}
		}
		GetMembersAndGroupsForPageLayoutChange(previousToken, cancellationToken, out var members, out var groupMembers, out var areaMembers);
		ChangeKind changeKind = previousToken.Kind.ToChangeKind();
		if (ChangeSymbolExtensions.IsPositionInsideAnchor(changeKind))
		{
			return groupMembers.Concat(areaMembers);
		}
		if (ChangeSymbolExtensions.IsPositionRelativeToAnchor(changeKind))
		{
			return Enumerable.Except(members, areaMembers);
		}
		if (changeKind == ChangeKind.Modify)
		{
			return Enumerable.Except(members, areaMembers);
		}
		return members;
	}
}
