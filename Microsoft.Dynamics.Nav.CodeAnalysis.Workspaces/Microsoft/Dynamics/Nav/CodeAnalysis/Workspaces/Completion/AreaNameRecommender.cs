using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class AreaNameRecommender : TextRecommender
{
	private readonly Lazy<IEnumerable<ActionAreaKind>> actionAreaNames = new Lazy<IEnumerable<ActionAreaKind>>(() => GetEnumValues<ActionAreaKind>());

	private readonly Lazy<IEnumerable<AreaKind>> areaNames = new Lazy<IEnumerable<AreaKind>>(() => GetEnumValues<AreaKind>());

	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.Page.HasFlag(PageContexts.Area))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<string> suggestions;
		switch (syntaxContext.TargetToken.Parent.Kind)
		{
		case SyntaxKind.PageArea:
			suggestions = GetSuggestions(syntaxContext, GetPageAreas);
			break;
		case SyntaxKind.PageActionArea:
			suggestions = GetSuggestions(syntaxContext, GetPageActionAreas);
			break;
		default:
			return ImmutableArray<CompletionItem>.Empty;
		}
		return TextRecommender.ToCompletionItemArray(suggestions.ToImmutableArray(), Glyph.Keyword);
	}

	private IEnumerable<string> GetSuggestions<T>(MemberSyntaxContext syntaxContext, Func<PageTypeKind, IEnumerable<T>> f)
	{
		IObjectTypeSymbol declaredSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.DeclaringObject);
		PageTypeSymbol pageTypeSymbol = declaredSymbol as PageTypeSymbol;
		if (pageTypeSymbol != null)
		{
			return from x in f(pageTypeSymbol.PageType)
				select x.ToString();
		}
		RequestPageTypeSymbol requestPageTypeSymbol = declaredSymbol as RequestPageTypeSymbol;
		if (requestPageTypeSymbol != null)
		{
			return from x in f(requestPageTypeSymbol.PageType)
				select x.ToString();
		}
		return ImmutableArray<string>.Empty;
	}

	private IEnumerable<AreaKind> GetPageAreas(PageTypeKind pageTypeKind)
	{
		IEnumerable<AreaKind> enumerable = areaNames.Value.Where((AreaKind x) => x != AreaKind.None);
		if (pageTypeKind == PageTypeKind.RoleCenter)
		{
			return enumerable;
		}
		if (pageTypeKind.IsPartPageType())
		{
			return enumerable.Where((AreaKind x) => x != AreaKind.FactBoxes && x != AreaKind.RoleCenter);
		}
		return enumerable.Where((AreaKind x) => x != AreaKind.RoleCenter);
	}

	private IEnumerable<ActionAreaKind> GetPageActionAreas(PageTypeKind pageTypeKind)
	{
		IEnumerable<ActionAreaKind> enumerable = actionAreaNames.Value.Where((ActionAreaKind x) => x != ActionAreaKind.None);
		if (pageTypeKind == PageTypeKind.RoleCenter)
		{
			return enumerable;
		}
		if (pageTypeKind.IsPartPageType())
		{
			return new ActionAreaKind[1] { ActionAreaKind.Processing };
		}
		return enumerable.Where((ActionAreaKind x) => !x.IsActionAreaDedicatedToRoleCenter());
	}

	private static IEnumerable<T> GetEnumValues<T>()
	{
		return new List<T>((T[])Enum.GetValues(typeof(T)));
	}
}
