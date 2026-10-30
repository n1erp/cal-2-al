using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal sealed class SystemPartNameRecommender : TextRecommender
{
	private readonly Lazy<ImmutableArray<string>> systemPartNames = new Lazy<ImmutableArray<string>>(() => GetEnumNames(typeof(PageSystemPartKind)));

	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.Page.HasFlag(PageContexts.SystemPartPage))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return TextRecommender.ToCompletionItemArray(systemPartNames.Value, Glyph.Keyword);
	}

	private static ImmutableArray<string> GetEnumNames(Type enumType)
	{
		return (from n in enumType.GetTypeInfo().GetEnumNames()
			where n != "None"
			select n).ToImmutableArray();
	}
}
