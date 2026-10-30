using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

public abstract class CompletionServiceWithProviders : CompletionService, IEqualityComparer<ImmutableHashSet<string>>
{
	private static readonly Func<string, List<CompletionItem>> createList = (string _) => new List<CompletionItem>();

	private readonly object gate = new object();

	private readonly Dictionary<string, CompletionProvider> nameToProvider = new Dictionary<string, CompletionProvider>();

	private readonly Dictionary<ImmutableHashSet<string>, ImmutableArray<CompletionProvider>> rolesToProviders;

	private readonly Func<ImmutableHashSet<string>, ImmutableArray<CompletionProvider>> createRoleProviders;

	private ImmutableArray<CompletionProvider> testProviders = ImmutableArray<CompletionProvider>.Empty;

	private readonly Workspace workspace;

	internal readonly ImmutableArray<CompletionProvider>? ExclusiveProviders;

	protected CompletionServiceWithProviders(Workspace workspace)
		: this(workspace, null)
	{
	}

	internal CompletionServiceWithProviders(Workspace workspace, ImmutableArray<CompletionProvider>? exclusiveProviders = null)
	{
		this.workspace = workspace;
		ExclusiveProviders = exclusiveProviders;
		rolesToProviders = new Dictionary<ImmutableHashSet<string>, ImmutableArray<CompletionProvider>>(this);
		createRoleProviders = CreateRoleProviders;
	}

	protected virtual ImmutableArray<CompletionProvider> GetBuiltInProviders()
	{
		return ImmutableArray<CompletionProvider>.Empty;
	}

	internal void SetTestProviders(IEnumerable<CompletionProvider> testProvidersParam)
	{
		lock (gate)
		{
			testProviders = testProvidersParam?.ToImmutableArray() ?? ImmutableArray<CompletionProvider>.Empty;
			rolesToProviders.Clear();
			nameToProvider.Clear();
		}
	}

	private ImmutableArray<CompletionProvider> CreateRoleProviders(ImmutableHashSet<string> roles)
	{
		ImmutableArray<CompletionProvider> allProviders = GetAllProviders(roles);
		foreach (CompletionProvider item in allProviders)
		{
			nameToProvider[item.Name] = item;
		}
		return allProviders;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "roles")]
	private ImmutableArray<CompletionProvider> GetAllProviders(ImmutableHashSet<string> roles)
	{
		if (ExclusiveProviders.HasValue)
		{
			return ExclusiveProviders.Value;
		}
		return Enumerable.Concat(GetBuiltInProviders(), testProviders).ToImmutableArray();
	}

	protected ImmutableArray<CompletionProvider> GetProviders(ImmutableHashSet<string> roles)
	{
		roles = roles ?? ImmutableHashSet<string>.Empty;
		lock (gate)
		{
			return rolesToProviders.GetOrAdd(roles, createRoleProviders);
		}
	}

	protected virtual ImmutableArray<CompletionProvider> GetProviders(ImmutableHashSet<string> roles, CompletionTrigger trigger)
	{
		if (trigger.Kind == CompletionTriggerKind.Snippets)
		{
			return (from p in GetProviders(roles)
				where p.IsSnippetProvider
				select p).ToImmutableArray();
		}
		return GetProviders(roles);
	}

	protected internal CompletionProvider GetProvider(CompletionItem item)
	{
		CompletionProvider value = null;
		if (item.Properties.TryGetValue("Provider", out var value2))
		{
			lock (gate)
			{
				nameToProvider.TryGetValue(value2, out value);
			}
		}
		return value;
	}

	public override async Task<CompletionList> GetCompletionsAsync(Document document, int caretPosition, CompletionTrigger trigger, ImmutableHashSet<string> roles, OptionSet options, CancellationToken cancellationToken)
	{
		SourceText text = await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		TextSpan defaultItemSpan = await GetDefaultItemSpanAsync(document, text, caretPosition, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		options = options ?? document.Project.Solution.Workspace.Options;
		ImmutableArray<CompletionProvider> providers = GetProviders(roles, trigger);
		Dictionary<CompletionProvider, int> completionProviderToIndex = GetCompletionProviderToIndex(providers);
		ImmutableArray<CompletionProvider> immutableArray = ImmutableArray<CompletionProvider>.Empty;
		CompletionTriggerKind kind = trigger.Kind;
		if ((uint)(kind - 1) <= 1u)
		{
			if (ShouldTriggerCompletion(text, caretPosition, trigger, roles, options))
			{
				immutableArray = providers.Where((CompletionProvider p) => p.ShouldTriggerCompletion(text, caretPosition, trigger, options)).ToImmutableArrayOrEmpty();
				if (immutableArray.Length == 0)
				{
					immutableArray = providers;
				}
			}
		}
		else
		{
			immutableArray = providers;
		}
		List<CompletionContext> completionLists = new List<CompletionContext>();
		foreach (CompletionProvider item in immutableArray)
		{
			CompletionContext completionContext = await GetContextAsync(item, document, caretPosition, trigger, options, defaultItemSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (completionContext != null && !completionContext.Items.IsEmpty())
			{
				completionLists.Add(completionContext);
			}
		}
		CompletionContext completionContext2 = completionLists.FirstOrDefault((CompletionContext t) => t.IsExclusive && t.Items.Any());
		if (completionContext2 != null)
		{
			return MergeAndPruneCompletionLists(SpecializedCollections.SingletonEnumerable(completionContext2), defaultItemSpan, isExclusive: true);
		}
		IEnumerable<CompletionContext> nonExclusiveLists = completionLists.Where((CompletionContext t) => !t.IsExclusive);
		if (!nonExclusiveLists.Any((CompletionContext g) => g.Items.Any()))
		{
			return null;
		}
		IEnumerable<CompletionProvider> second = nonExclusiveLists.Select((CompletionContext g) => g.Provider);
		IEnumerable<CompletionProvider> enumerable = Enumerable.Except(providers, second);
		List<CompletionContext> nonUsedNonExclusiveLists = new List<CompletionContext>();
		foreach (CompletionProvider item2 in enumerable)
		{
			CompletionContext completionContext3 = await GetContextAsync(item2, document, caretPosition, trigger, options, defaultItemSpan, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (completionContext3 != null && !completionContext3.IsExclusive)
			{
				nonUsedNonExclusiveLists.Add(completionContext3);
			}
		}
		List<CompletionContext> list = nonExclusiveLists.Concat(nonUsedNonExclusiveLists).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		list.Sort((CompletionContext p1, CompletionContext p2) => completionProviderToIndex[p1.Provider] - completionProviderToIndex[p2.Provider]);
		return MergeAndPruneCompletionLists(list, defaultItemSpan, isExclusive: false);
	}

	private CompletionList MergeAndPruneCompletionLists(IEnumerable<CompletionContext> completionLists, TextSpan contextSpan, bool isExclusive)
	{
		Dictionary<string, List<CompletionItem>> dictionary = new Dictionary<string, List<CompletionItem>>();
		CompletionItem completionItem = null;
		foreach (CompletionContext completionList in completionLists)
		{
			foreach (CompletionItem item in completionList.Items)
			{
				AddToDisplayMap(item, dictionary);
			}
			completionItem = completionItem ?? completionList.SuggestionModeItem;
		}
		if (dictionary.Count == 0)
		{
			return CompletionList.Empty;
		}
		List<CompletionItem> list = dictionary.Values.Flatten().ToList();
		list.Sort();
		return CompletionList.Create(contextSpan, list.ToImmutableArray(), completionItem, isExclusive);
	}

	private void AddToDisplayMap(CompletionItem item, Dictionary<string, List<CompletionItem>> displayNameToItemsMap)
	{
		List<CompletionItem> orAdd = displayNameToItemsMap.GetOrAdd(item.DisplayText, createList);
		for (int i = 0; i < orAdd.Count; i++)
		{
			CompletionItem existingItem = orAdd[i];
			if (ItemsMatch(item, existingItem))
			{
				orAdd[i] = GetBetterItem(item, existingItem);
				return;
			}
		}
		orAdd.Add(item);
	}

	protected virtual bool ItemsMatch(CompletionItem item, CompletionItem existingItem)
	{
		if (item.Span == existingItem.Span)
		{
			return item.SortText == existingItem.SortText;
		}
		return false;
	}

	protected virtual CompletionItem GetBetterItem(CompletionItem item, CompletionItem existingItem)
	{
		return item;
	}

	private static Dictionary<CompletionProvider, int> GetCompletionProviderToIndex(IEnumerable<CompletionProvider> completionProviders)
	{
		Dictionary<CompletionProvider, int> dictionary = new Dictionary<CompletionProvider, int>();
		int num = 0;
		foreach (CompletionProvider completionProvider in completionProviders)
		{
			dictionary[completionProvider] = num;
			num++;
		}
		return dictionary;
	}

	internal async Task<CompletionContext> GetContextAsync(CompletionProvider provider, Document document, int position, CompletionTrigger triggerInfo, OptionSet options, CancellationToken cancellationToken)
	{
		return await GetContextAsync(provider, document, position, triggerInfo, options, null, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<CompletionContext> GetContextAsync(CompletionProvider provider, Document document, int position, CompletionTrigger triggerInfo, OptionSet options, TextSpan? defaultSpan, CancellationToken cancellationToken)
	{
		options = options ?? document.Project.Solution.Workspace.Options;
		if (!defaultSpan.HasValue)
		{
			defaultSpan = await GetDefaultItemSpanAsync(document, await document.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		CompletionContext context = new CompletionContext(provider, document, position, defaultSpan.Value, triggerInfo, options, cancellationToken);
		AbstractSyntaxContext abstractSyntaxContext = await provider.CreateContextAsync(document, position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (abstractSyntaxContext.IsCommentContext(position, cancellationToken))
		{
			return null;
		}
		await provider.ProvideCompletionsAsync(context, abstractSyntaxContext).ConfigureAwait(continueOnCapturedContext: false);
		return context;
	}

	public override bool ShouldTriggerCompletion(SourceText text, int caretPosition, CompletionTrigger trigger, ImmutableHashSet<string> roles = null, OptionSet options = null)
	{
		options = options ?? workspace.Options;
		if (!options.GetOption(CompletionOptions.TriggerOnTyping, "AL"))
		{
			return false;
		}
		return GetProviders(roles, CompletionTrigger.Default).Any((CompletionProvider p) => p.ShouldTriggerCompletion(text, caretPosition, trigger, options));
	}

	public override async Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item, char? commitKey, CancellationToken cancellationToken)
	{
		CompletionProvider provider = GetProvider(item);
		return (provider == null) ? CompletionChange.Create(new TextChange(item.Span, item.DisplayText)) : (await provider.GetChangeAsync(document, item, commitKey, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	bool IEqualityComparer<ImmutableHashSet<string>>.Equals(ImmutableHashSet<string> x, ImmutableHashSet<string> y)
	{
		if (x == y)
		{
			return true;
		}
		if (x.Count != y.Count)
		{
			return false;
		}
		foreach (string item in x)
		{
			if (!y.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	int IEqualityComparer<ImmutableHashSet<string>>.GetHashCode(ImmutableHashSet<string> obj)
	{
		int num = 0;
		foreach (string item in obj)
		{
			num += item.GetHashCode();
		}
		return num;
	}
}
