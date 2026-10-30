using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal static class SymbolCompletionItem
{
	private const char SymbolSplitter = '|';

	private static readonly char[] SymbolSplitters = new char[1] { '|' };

	private static readonly char[] ProjectSeperators = new char[1] { ';' };

	public static CompletionItem Create(string displayText, TextSpan span, IReadOnlyList<ISymbol> symbols, int contextPosition = -1, int descriptionPosition = -1, string sortText = null, string insertionText = null, Glyph? glyph = null, string filterText = null, bool preselect = false, SupportedPlatformData supportedPlatforms = null, bool isArgumentName = false, ImmutableDictionary<string, string> properties = null, ImmutableArray<string> tags = default(ImmutableArray<string>), CompletionItemRules rules = null, bool isSnippet = false, bool shouldSerializeItem = false)
	{
		ImmutableDictionary<string, string> immutableDictionary = properties ?? ImmutableDictionary<string, string>.Empty;
		if (shouldSerializeItem)
		{
			immutableDictionary = immutableDictionary.Add("Symbols", EncodeSymbols(symbols));
		}
		if (contextPosition >= 0)
		{
			immutableDictionary = immutableDictionary.Add("ContextPosition", contextPosition.ToString(CultureInfo.InvariantCulture));
		}
		if (descriptionPosition >= 0)
		{
			immutableDictionary = immutableDictionary.Add("DescriptionPosition", descriptionPosition.ToString(CultureInfo.InvariantCulture));
		}
		string text = symbols[0].ToDisplayParts(SymbolDisplayFormat.HoverFormat).ToDisplayString();
		string documentation = DocumentationFacts.GetDocumentation(symbols.FirstOrDefault());
		string filterText2 = filterText ?? ((displayText.Length > 0 && displayText[0] == '@') ? displayText : symbols[0].Name);
		string sortText2 = sortText ?? symbols[0].Name;
		Glyph? glyph2 = glyph ?? symbols[0].GetGlyph();
		string detailText = text;
		bool preselect2 = preselect;
		bool flag = isArgumentName;
		bool showsWarningIcon = supportedPlatforms != null;
		bool isArgumentName2 = flag;
		ImmutableDictionary<string, string> properties2 = immutableDictionary;
		return WithSupportedPlatforms(CommonCompletionItem.Create(displayText, span, glyph2, documentation, detailText, sortText2, filterText2, insertionText, preselect2, showsWarningIcon, shouldFormatOnCommit: false, isArgumentName2, isSnippet, isMarkdownDocs: false, properties2, tags, rules), supportedPlatforms);
	}

	public static string EncodeSymbols(IReadOnlyList<ISymbol> symbols)
	{
		if (symbols.Count > 1)
		{
			return string.Join('|'.ToString(), symbols.Select(EncodeSymbol));
		}
		if (symbols.Count == 1)
		{
			return EncodeSymbol(symbols[0]);
		}
		return string.Empty;
	}

	public static string EncodeSymbol(ISymbol symbol)
	{
		return SymbolId.CreateId(symbol);
	}

	public static bool HasSymbols(CompletionItem item)
	{
		return item.Properties.ContainsKey("Symbols");
	}

	public static async Task<ImmutableArray<ISymbol>> GetSymbolsAsync(CompletionItem item, Document document, CancellationToken cancellationToken)
	{
		if (item.Properties.TryGetValue("Symbols", out var value))
		{
			List<string> idList = value.Split(SymbolSplitters, StringSplitOptions.RemoveEmptyEntries).ToList();
			List<ISymbol> symbols = new List<ISymbol>();
			DecodeSymbols(idList, await document.Project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), symbols);
			if (idList.Count > 0)
			{
				ImmutableArray<DocumentId> linkedDocumentIds = document.GetLinkedDocumentIds();
				if (linkedDocumentIds.Length > 0)
				{
					foreach (DocumentId item2 in linkedDocumentIds)
					{
						DecodeSymbols(idList, await document.Project.Solution.GetDocument(item2).Project.GetCompilationAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), symbols);
					}
				}
			}
			return symbols.ToImmutableArray();
		}
		return ImmutableArray<ISymbol>.Empty;
	}

	private static void DecodeSymbols(List<string> ids, Compilation compilation, List<ISymbol> symbols)
	{
		int num = 0;
		while (num < ids.Count)
		{
			ISymbol symbol = DecodeSymbol(ids[num], compilation);
			if (symbol != null)
			{
				ids.RemoveAt(num);
				symbols.Add(symbol);
			}
			else
			{
				num++;
			}
		}
	}

	private static ISymbol DecodeSymbol(string id, Compilation compilation)
	{
		return SymbolId.GetFirstSymbolForId(id, compilation);
	}

	public static async Task<CompletionDescription> GetDescriptionAsync(CompletionItem item, Document document, CancellationToken cancellationToken)
	{
		Workspace workspace = document.Project.Solution.Workspace;
		int position = GetDescriptionPosition(item);
		if (position == -1)
		{
			position = item.Span.Start;
		}
		SupportedPlatformData supportedPlatforms = GetSupportedPlatforms(item, workspace);
		Document document2 = document;
		if (supportedPlatforms != null && supportedPlatforms.InvalidProjects.Contains(document.Id.ProjectId))
		{
			DocumentId documentId = document.GetLinkedDocumentIds().FirstOrDefault((DocumentId id) => !supportedPlatforms.InvalidProjects.Contains(id.ProjectId));
			if (documentId != null)
			{
				document2 = document.Project.Solution.GetDocument(documentId);
			}
		}
		SemanticModel semanticModel = await document2.GetSemanticModelAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		ImmutableArray<ISymbol> immutableArray = await GetSymbolsAsync(item, document, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (immutableArray.Length > 0)
		{
			return await CommonCompletionUtilities.CreateDescriptionAsync(workspace, semanticModel, position, immutableArray, supportedPlatforms, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return CompletionDescription.Empty;
	}

	private static CompletionItem WithSupportedPlatforms(CompletionItem completionItem, SupportedPlatformData supportedPlatforms)
	{
		if (supportedPlatforms != null)
		{
			return completionItem.AddProperty("InvalidProjects", string.Join(";", supportedPlatforms.InvalidProjects.Select((ProjectId id) => id.Id))).AddProperty("CandidateProjects", string.Join(";", supportedPlatforms.CandidateProjects.Select((ProjectId id) => id.Id)));
		}
		return completionItem;
	}

	public static SupportedPlatformData GetSupportedPlatforms(CompletionItem item, Workspace workspace)
	{
		if (item.Properties.TryGetValue("InvalidProjects", out var value) && item.Properties.TryGetValue("CandidateProjects", out var value2))
		{
			return new SupportedPlatformData((from s in value.Split(ProjectSeperators)
				select ProjectId.CreateFromSerialized(Guid.Parse(s))).ToList(), (from s in value2.Split(ProjectSeperators)
				select ProjectId.CreateFromSerialized(Guid.Parse(s))).ToList(), workspace);
		}
		return null;
	}

	public static int GetContextPosition(CompletionItem item)
	{
		if (item.Properties.TryGetValue("ContextPosition", out var value) && int.TryParse(value, out var result))
		{
			return result;
		}
		return -1;
	}

	public static int GetDescriptionPosition(CompletionItem item)
	{
		if (item.Properties.TryGetValue("DescriptionPosition", out var value) && int.TryParse(value, out var result))
		{
			return result;
		}
		return -1;
	}

	public static string GetInsertionText(CompletionItem item)
	{
		item.Properties.TryGetValue("InsertionText", out var value);
		return value;
	}
}
