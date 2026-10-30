using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class TextRecommender
{
	protected internal virtual Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		return Task.FromResult(SpecializedCollections.EmptyEnumerable<CompletionItem>());
	}

	protected internal static IEnumerable<CompletionItem> ToCompletionItemArray(IEnumerable<string> result, Glyph glyph, TextSpan span = default(TextSpan))
	{
		ArrayBuilder<CompletionItem> instance = ArrayBuilder<CompletionItem>.GetInstance();
		foreach (string item in result)
		{
			instance.Add(CreateItem(item, glyph, span));
		}
		return instance.ToArrayAndFree();
	}

	protected static CompletionItem CreateItem(string displayText, Glyph glyph, TextSpan span = default(TextSpan), string descriptionText = null)
	{
		Glyph? glyph2 = glyph;
		return CommonCompletionItem.Create(displayText, span, glyph2, descriptionText, null, null, null, displayText);
	}

	protected static IEnumerable<CompletionItem> CreateTrueAndFalseCompletionItems()
	{
		return new List<CompletionItem>
		{
			CommonCompletionItem.Create("true", default, Glyph.Constant, null, null, "0000000000000001"),
			CommonCompletionItem.Create("false", default, Glyph.Constant, null, null, "0000000000000002")
		};
	}

	protected static Glyph CategorizeKeyword(string displayText)
	{
		string text = displayText.ToUpperInvariant();
		if (text == "TRUE" || text == "FALSE")
		{
			return Glyph.Constant;
		}
		return Glyph.Keyword;
	}
}
