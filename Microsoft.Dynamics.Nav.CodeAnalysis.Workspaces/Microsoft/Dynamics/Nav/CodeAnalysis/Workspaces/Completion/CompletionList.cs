using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public sealed class CompletionList
{
	[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
	public static readonly CompletionList Empty = new CompletionList(default, default, null, isExclusive: false);

	public ImmutableArray<CompletionItem> Items { get; }

	public TextSpan DefaultSpan { get; }

	public CompletionItem SuggestionModeItem { get; }

	internal bool IsExclusive { get; }

	private CompletionList(TextSpan defaultSpan, ImmutableArray<CompletionItem> items, CompletionItem suggestionModeItem, bool isExclusive)
	{
		DefaultSpan = defaultSpan;
		Items = (items.IsDefault ? ImmutableArray<CompletionItem>.Empty : items);
		SuggestionModeItem = suggestionModeItem;
		IsExclusive = isExclusive;
	}

	public static CompletionList Create(TextSpan defaultSpan, ImmutableArray<CompletionItem> items, CompletionItem suggestionModeItem = null)
	{
		return Create(defaultSpan, items, suggestionModeItem, isExclusive: false);
	}

	internal static CompletionList Create(TextSpan defaultSpan, ImmutableArray<CompletionItem> items, CompletionItem suggestionModeItem, bool isExclusive)
	{
		return new CompletionList(defaultSpan, FixItemSpans(items, defaultSpan), suggestionModeItem, isExclusive);
	}

	private static ImmutableArray<CompletionItem> FixItemSpans(ImmutableArray<CompletionItem> items, TextSpan defaultSpan)
	{
		if (defaultSpan != default(TextSpan) && items.Any((CompletionItem i) => i.Span == default(TextSpan)))
		{
			items = items.Select((CompletionItem i) => (!(i.Span == default(TextSpan))) ? i : i.WithSpan(defaultSpan)).ToImmutableArray();
		}
		return items;
	}

	private CompletionList With(Optional<TextSpan> span = default(Optional<TextSpan>), Optional<ImmutableArray<CompletionItem>> items = default(Optional<ImmutableArray<CompletionItem>>), Optional<CompletionItem> suggestionModeItem = default(Optional<CompletionItem>))
	{
		TextSpan textSpan = (span.HasValue ? span.Value : DefaultSpan);
		ImmutableArray<CompletionItem> immutableArray = (items.HasValue ? items.Value : Items);
		CompletionItem completionItem = (suggestionModeItem.HasValue ? suggestionModeItem.Value : SuggestionModeItem);
		if (textSpan == DefaultSpan && immutableArray == Items && completionItem == SuggestionModeItem)
		{
			return this;
		}
		return Create(textSpan, immutableArray, completionItem);
	}

	public CompletionList WithDefaultSpan(TextSpan span)
	{
		return With(span);
	}

	public CompletionList WithItems(ImmutableArray<CompletionItem> items)
	{
		return With(default, items);
	}

	public CompletionList WithSuggestionModeItem(CompletionItem suggestionModeItem)
	{
		return With(default, default, suggestionModeItem);
	}
}
