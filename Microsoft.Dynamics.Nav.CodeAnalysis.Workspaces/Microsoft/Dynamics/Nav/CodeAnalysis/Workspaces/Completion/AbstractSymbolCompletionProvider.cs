using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Options;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class AbstractSymbolCompletionProvider : CompletionProvider
{
	private static readonly ConditionalWeakTable<Document, Task<AbstractSyntaxContext>> cachedDocuments = new ConditionalWeakTable<Document, Task<AbstractSyntaxContext>>();

	private static int cachedPosition;

	private static readonly object cacheGate = new object();

	protected abstract (string, string, bool) GetDisplayAndInsertionText(ISymbol symbol, AbstractSyntaxContext context);

	protected abstract CompletionItemRules GetCompletionItemRules(IReadOnlyList<ISymbol> symbols, AbstractSyntaxContext context);

	protected IEnumerable<CompletionItem> CreateItems(int position, IEnumerable<ISymbol> symbols, TextSpan span, AbstractSyntaxContext context, Dictionary<ISymbol, List<ProjectId>> invalidProjectMap, List<ProjectId> totalProjects, bool preselect, bool shouldSerialize)
	{
		return (from symbol in symbols
			let texts = GetDisplayAndInsertionText(symbol, context)
			group symbol by texts into g
			select CreateItem(g.Key.Item1, g.Key.Item2, position, g.ToList(), context, span, invalidProjectMap, totalProjects, preselect, g.Key.Item3, shouldSerialize)).ToList();
	}

	protected IEnumerable<CompletionItem> CreateItems(int position, IEnumerable<ISymbol> symbols, TextSpan span, Dictionary<ISymbol, AbstractSyntaxContext> originatingContextMap, Dictionary<ISymbol, List<ProjectId>> invalidProjectMap, List<ProjectId> totalProjects, bool preselect, bool shouldSerializeItem)
	{
		return (from symbol in symbols
			let texts = GetDisplayAndInsertionText(symbol, originatingContextMap[symbol])
			group symbol by texts into g
			select CreateItem(g.Key.Item1, g.Key.Item2, position, g.ToList(), originatingContextMap[g.First()], span, invalidProjectMap, totalProjects, preselect, g.Key.Item3, shouldSerializeItem)).ToList();
	}

	private CompletionItem CreateItem(string displayText, string insertionText, int position, List<ISymbol> symbols, AbstractSyntaxContext context, TextSpan span, Dictionary<ISymbol, List<ProjectId>> invalidProjectMap, List<ProjectId> totalProjects, bool preselect, bool isSnippet, bool shouldSerializeItem)
	{
		Contract.ThrowIfNull(symbols);
		SupportedPlatformData supportedPlatformData = null;
		if (invalidProjectMap != null)
		{
			List<ProjectId> value = null;
			foreach (ISymbol symbol in symbols)
			{
				if (invalidProjectMap.TryGetValue(symbol, out value))
				{
					break;
				}
			}
			if (value != null)
			{
				supportedPlatformData = new SupportedPlatformData(value, totalProjects, context.Workspace);
			}
		}
		return CreateItem(displayText, insertionText, position, symbols, context, span, preselect, supportedPlatformData, isSnippet, shouldSerializeItem);
	}

	protected virtual CompletionItem CreateItem(string displayText, string insertionText, int position, List<ISymbol> symbols, AbstractSyntaxContext context, TextSpan span, bool preselect, SupportedPlatformData supportedPlatformData, bool isSnippet, bool shouldSerializeItem)
	{
		string filterText = GetFilterText(symbols[0], displayText, context);
		string name = symbols[0].Name;
		int position2 = context.Position;
		return SymbolCompletionItem.Create(displayText, span, symbols, position2, position, name, insertionText, null, filterText, preselect, supportedPlatformData, isArgumentName: false, null, default, GetCompletionItemRules(symbols, context), isSnippet, shouldSerializeItem);
	}

	protected virtual string GetFilterText(ISymbol symbol, string displayText, AbstractSyntaxContext context)
	{
		return ((displayText == symbol.Name || (displayText.Length > 0 && displayText[0] == '@')) ? displayText : symbol.Name).QuoteIdentifier();
	}

	protected abstract Task<IEnumerable<ISymbol>> GetSymbolsWorker(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken);

	protected virtual Task<IEnumerable<ISymbol>> GetPreselectedSymbolsWorker(AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken)
	{
		return SpecializedTasks.EmptyEnumerable<ISymbol>();
	}

	public override async Task ProvideCompletionsAsync(CompletionContext context, AbstractSyntaxContext memberSyntaxContext)
	{
		Document document = context.Document;
		int position = context.Position;
		TextSpan span = context.DefaultItemSpan;
		OptionSet options = context.Options;
		CancellationToken cancellationToken = context.CancellationToken;
		if (context.Trigger.Kind != CompletionTriggerKind.Insertion || await IsSemanticTriggerCharacterAsync(document, position - 1, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			context.IsExclusive = IsExclusive();
			using (Logger.LogBlock(FunctionId.Completion_SymbolCompletionProvider_GetItemsWorker, cancellationToken))
			{
				context.AddItems(await GetItemsWorkerAsync(document, memberSyntaxContext, span, options, preselect: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
			}
		}
	}

	private async Task<IEnumerable<CompletionItem>> GetItemsWorkerAsync(Document document, AbstractSyntaxContext context, TextSpan span, OptionSet options, bool preselect, CancellationToken cancellationToken)
	{
		ImmutableArray<DocumentId> linkedDocumentIds = document.GetLinkedDocumentIds();
		options = GetUpdatedRecommendationOptions(options, document.Project.Language);
		bool shouldSerialize = options?.GetOption(CompletionOptions.SerializeCompletionResult) ?? CompletionOptions.SerializeCompletionResult.DefaultValue;
		if (!linkedDocumentIds.Any())
		{
			IEnumerable<ISymbol> symbols = (await GetSymbolsWorker(preselect, context, options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) ?? SpecializedCollections.EmptyEnumerable<ISymbol>();
			return CreateItems(context.Position, symbols, span, context, null, null, preselect, shouldSerialize);
		}
		List<Tuple<DocumentId, AbstractSyntaxContext, IEnumerable<ISymbol>>> list = await GetPerContextSymbols(document, context.Position, options, new DocumentId[1] { document.Id }.Concat(linkedDocumentIds), preselect, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		Dictionary<ISymbol, AbstractSyntaxContext> originDictionary = null;
		HashSet<ISymbol> hashSet = UnionSymbols(list, out originDictionary);
		Dictionary<ISymbol, List<ProjectId>> invalidProjectMap = FindSymbolsMissingInLinkedContexts(hashSet, list);
		List<ProjectId> totalProjects = list.Select((Tuple<DocumentId, AbstractSyntaxContext, IEnumerable<ISymbol>> t) => t.Item1.ProjectId).ToList();
		return CreateItems(context.Position, hashSet, span, originDictionary, invalidProjectMap, totalProjects, preselect, shouldSerialize);
	}

	protected virtual bool IsExclusive()
	{
		return false;
	}

	protected virtual Task<bool> IsSemanticTriggerCharacterAsync(Document document, int characterPosition, CancellationToken cancellationToken)
	{
		return SpecializedTasks.True;
	}

	private Task<IEnumerable<ISymbol>> GetSymbolsWorker(bool preselect, AbstractSyntaxContext context, OptionSet options, CancellationToken cancellationToken)
	{
		if (!preselect)
		{
			return GetSymbolsWorker(context, options, cancellationToken);
		}
		return GetPreselectedSymbolsWorker(context, options, cancellationToken);
	}

	private static HashSet<ISymbol> UnionSymbols(List<Tuple<DocumentId, AbstractSyntaxContext, IEnumerable<ISymbol>>> linkedContextSymbolLists, out Dictionary<ISymbol, AbstractSyntaxContext> originDictionary)
	{
		originDictionary = new Dictionary<ISymbol, AbstractSyntaxContext>(LinkedFilesSymbolEquivalenceComparer.Instance);
		HashSet<ISymbol> hashSet = new HashSet<ISymbol>(LinkedFilesSymbolEquivalenceComparer.Instance);
		foreach (Tuple<DocumentId, AbstractSyntaxContext, IEnumerable<ISymbol>> linkedContextSymbolList in linkedContextSymbolLists)
		{
			foreach (ISymbol item in from s in linkedContextSymbolList.Item3
				group s by new { s.Name, s.Kind } into g
				select g.First())
			{
				if (hashSet.Add(item))
				{
					originDictionary.Add(item, linkedContextSymbolList.Item2);
				}
			}
		}
		return hashSet;
	}

	protected async Task<List<Tuple<DocumentId, AbstractSyntaxContext, IEnumerable<ISymbol>>>> GetPerContextSymbols(Document document, int position, OptionSet options, IEnumerable<DocumentId> relatedDocuments, bool preselect, CancellationToken cancellationToken)
	{
		List<Tuple<DocumentId, AbstractSyntaxContext, IEnumerable<ISymbol>>> perContextSymbols = new List<Tuple<DocumentId, AbstractSyntaxContext, IEnumerable<ISymbol>>>();
		foreach (DocumentId relatedDocument2 in relatedDocuments)
		{
			Document relatedDocument = document.Project.Solution.GetDocument(relatedDocument2);
			AbstractSyntaxContext context = await GetOrCreateContext(relatedDocument, position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (IsCandidateProject(context, cancellationToken))
			{
				IEnumerable<ISymbol> enumerable = await GetSymbolsWorker(preselect, context, options, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				perContextSymbols.Add(Tuple.Create(relatedDocument.Id, context, enumerable ?? SpecializedCollections.EmptyEnumerable<ISymbol>()));
			}
		}
		return perContextSymbols;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cancellationToken")]
	private static bool IsCandidateProject(AbstractSyntaxContext context, CancellationToken cancellationToken)
	{
		return true;
	}

	protected static OptionSet GetUpdatedRecommendationOptions(OptionSet options, string language)
	{
		bool option = options.GetOption(CompletionOptions.FilterOutOfScopeLocals);
		bool option2 = options.GetOption(CompletionOptions.HideAdvancedMembers, language);
		return options.WithChangedOption(RecommendationOptions.FilterOutOfScopeLocals, language, option).WithChangedOption(RecommendationOptions.HideAdvancedMembers, language, option2);
	}

	private static Task<AbstractSyntaxContext> GetOrCreateContext(Document document, int position, CancellationToken cancellationToken)
	{
		List<Document> list = document.GetLinkedDocumentIds().Concat(document.Id).Select(document.Project.Solution.GetDocument)
			.ToList();
		lock (cacheGate)
		{
			if (cachedPosition != position || !list.All((Document d) => cachedDocuments.TryGetValue(d, out var _)))
			{
				cachedPosition = position;
				for (int num = 0; num < list.Count; num++)
				{
					cachedDocuments.Remove(document);
				}
			}
			return cachedDocuments.GetValue(document, (Document d) => CompletionProvider.CreateMemberContextAsync(d, position, cancellationToken));
		}
	}

	protected internal override async Task<AbstractSyntaxContext> CreateContextAsync(Document document, int position, CancellationToken cancellationToken)
	{
		return await GetOrCreateContext(document, position, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected static Dictionary<ISymbol, List<ProjectId>> FindSymbolsMissingInLinkedContexts(HashSet<ISymbol> expectedSymbols, IEnumerable<Tuple<DocumentId, AbstractSyntaxContext, IEnumerable<ISymbol>>> linkedContextSymbolLists)
	{
		Dictionary<ISymbol, List<ProjectId>> dictionary = new Dictionary<ISymbol, List<ProjectId>>(LinkedFilesSymbolEquivalenceComparer.Instance);
		foreach (Tuple<DocumentId, AbstractSyntaxContext, IEnumerable<ISymbol>> linkedContextSymbolList in linkedContextSymbolLists)
		{
			foreach (ISymbol item in expectedSymbols.Except(linkedContextSymbolList.Item3, LinkedFilesSymbolEquivalenceComparer.Instance))
			{
				dictionary.GetOrAdd(item, (ISymbol m) => new List<ProjectId>()).Add(linkedContextSymbolList.Item1.ProjectId);
			}
		}
		return dictionary;
	}

	public override async Task<TextChange?> GetTextChangeAsync(Document document, CompletionItem selectedItem, char? ch, CancellationToken cancellationToken)
	{
		string newText;
		if (!ch.HasValue)
		{
			newText = SymbolCompletionItem.GetInsertionText(selectedItem) ?? selectedItem.DisplayText;
		}
		else
		{
			int contextPosition = SymbolCompletionItem.GetContextPosition(selectedItem);
			AbstractSyntaxContext context = await CreateContextAsync(document, contextPosition, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			ImmutableArray<ISymbol> immutableArray = await SymbolCompletionItem.GetSymbolsAsync(selectedItem, document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			newText = ((immutableArray.Length <= 0) ? selectedItem.DisplayText : GetDisplayText(immutableArray[0], context, ch.Value));
		}
		return new TextChange(selectedItem.Span, newText);
	}

	protected abstract string GetDisplayText(ISymbol symbol, AbstractSyntaxContext context, char ch);
}
