using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Log;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class DocumentState : TextDocumentState
{
	private class DocumentBranch
	{
		internal readonly SourceText Text;

		internal readonly DocumentState State;

		internal DocumentBranch(SourceText text, DocumentState state)
		{
			Text = text;
			State = state;
		}
	}

	private class TreeTextSource : ValueSource<TextAndVersion>, ITextVersionable
	{
		private readonly ValueSource<SourceText> lazyText;

		private readonly VersionStamp version;

		private readonly string filePath;

		public TreeTextSource(ValueSource<SourceText> text, VersionStamp version, string filePath)
		{
			lazyText = text;
			this.version = version;
			this.filePath = filePath;
		}

		public override async Task<TextAndVersion> GetValueAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return TextAndVersion.Create(await lazyText.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), version, filePath);
		}

		public override TextAndVersion GetValue(CancellationToken cancellationToken = default(CancellationToken))
		{
			return TextAndVersion.Create(lazyText.GetValue(cancellationToken), version, filePath);
		}

		public override bool TryGetValue(out TextAndVersion value)
		{
			if (lazyText.TryGetValue(out var value2))
			{
				value = TextAndVersion.Create(value2, version, filePath);
				return true;
			}
			value = null;
			return false;
		}

		public bool TryGetTextVersion(out VersionStamp resultVersion)
		{
			resultVersion = version;
			return resultVersion != default(VersionStamp);
		}
	}

	private static readonly Func<string, PreservationMode, string> s_fullParseLog = (string path, PreservationMode mode) => string.Format(CultureInfo.InvariantCulture, "{0} : {1}", path, mode);

	private readonly ValueSource<TreeAndVersion> treeSource;

	private readonly ParseOptions parseOptions;

	private const int MaxTextChangeRangeLength = 4096;

	private DocumentBranch _firstBranch;

	private static readonly ReaderWriterLockSlim s_syntaxTreeToIdMapLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private static readonly ConditionalWeakTable<SyntaxTree, DocumentId> s_syntaxTreeToIdMap = new ConditionalWeakTable<SyntaxTree, DocumentId>();

	internal bool SupportsSyntaxTree => LanguageServices.SyntaxTreeFactory != null;

	public AbstractHostLanguageServices LanguageServices { get; }

	public bool IsGenerated => Info.IsGenerated;

	private DocumentState(AbstractHostLanguageServices languageServices, SolutionServices solutionServices, DocumentInfo info, ParseOptions parseOptions, ValueSource<TextAndVersion> textSource, ValueSource<TreeAndVersion> treeSource)
		: base(solutionServices, info, textSource)
	{
		LanguageServices = languageServices;
		this.treeSource = (SupportsSyntaxTree ? treeSource : ValueSource<TreeAndVersion>.Empty);
		this.parseOptions = parseOptions;
	}

	public static DocumentState Create(DocumentInfo info, ParseOptions parseOptions, AbstractHostLanguageServices language, SolutionServices services)
	{
		ValueSource<TextAndVersion> valueSource = ((info.TextLoader != null) ? TextDocumentState.CreateRecoverableText(info.TextLoader, info.Id, services, reportInvalidDataException: true) : TextDocumentState.CreateStrongText(TextAndVersion.Create(SourceText.From(string.Empty, Encoding.UTF8), VersionStamp.Default, info.FilePath)));
		ValueSource<TreeAndVersion> valueSource2 = CreateLazyFullyParsedTree(valueSource, info.Id.ProjectId, GetSyntaxTreeFilePath(info), parseOptions, language, services);
		info = info.WithTextLoader(null);
		return new DocumentState(language, services, info, parseOptions, valueSource, valueSource2);
	}

	private static string GetSyntaxTreeFilePath(DocumentInfo info)
	{
		return info.FilePath ?? info.Name;
	}

	private static ValueSource<TreeAndVersion> CreateLazyFullyParsedTree(ValueSource<TextAndVersion> newTextSource, ProjectId cacheKey, string filePath, ParseOptions parseOptions, AbstractHostLanguageServices languageServices, SolutionServices solutionServices, PreservationMode mode = PreservationMode.PreserveValue)
	{
		return new AsyncLazy<TreeAndVersion>((CancellationToken c) => FullyParseTreeAsync(newTextSource, cacheKey, filePath, parseOptions, languageServices, solutionServices, mode, c), cacheResult: true);
	}

	private static async Task<TreeAndVersion> FullyParseTreeAsync(ValueSource<TextAndVersion> newTextSource, ProjectId cacheKey, string filePath, ParseOptions parseOptions, AbstractHostLanguageServices languageServices, SolutionServices solutionServices, PreservationMode mode, CancellationToken cancellationToken)
	{
		using (Logger.LogBlock(FunctionId.Workspace_Document_State_FullyParseSyntaxTree, s_fullParseLog, filePath, mode, cancellationToken))
		{
			TextAndVersion textAndVersion = await newTextSource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			SourceText text = textAndVersion.Text;
			ISyntaxTreeFactoryService service = languageServices.GetService<ISyntaxTreeFactoryService>();
			SyntaxTree syntaxTree = service.ParseSyntaxTree(filePath, text, cancellationToken, parseOptions);
			SyntaxNode root = syntaxTree.GetRoot(cancellationToken);
			if (mode == PreservationMode.PreserveValue && service.CanCreateRecoverableTree(root))
			{
				syntaxTree = service.CreateRecoverableTree(cacheKey, syntaxTree.FilePath, newTextSource, text.Encoding, root);
			}
			Contract.ThrowIfNull(syntaxTree);
			return TreeAndVersion.Create(syntaxTree, textAndVersion.Version);
		}
	}

	private static ValueSource<TreeAndVersion> CreateLazyIncrementallyParsedTree(ValueSource<TreeAndVersion> oldTreeSource, ValueSource<TextAndVersion> newTextSource)
	{
		return new AsyncLazy<TreeAndVersion>((CancellationToken c) => IncrementallyParseTreeAsync(oldTreeSource, newTextSource, c), cacheResult: true);
	}

	private static async Task<TreeAndVersion> IncrementallyParseTreeAsync(ValueSource<TreeAndVersion> oldTreeSource, ValueSource<TextAndVersion> newTextSource, CancellationToken cancellationToken)
	{
		try
		{
			using (Logger.LogBlock(FunctionId.Workspace_Document_State_IncrementallyParseSyntaxTree, cancellationToken))
			{
				TextAndVersion newTextAndVersion = await newTextSource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				SourceText newText = newTextAndVersion.Text;
				TreeAndVersion oldTreeAndVersion = await oldTreeSource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				SyntaxTree oldTree = oldTreeAndVersion.Tree;
				SourceText oldText = await oldTree.GetTextAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				SyntaxTree syntaxTree = oldTree.WithChangedText(newText);
				Contract.ThrowIfNull(syntaxTree);
				return MakeNewTreeAndVersion(oldTree, oldText, oldTreeAndVersion.Version, syntaxTree, newText, newTextAndVersion.Version);
			}
		}
		catch (Exception exception) when (FatalError.ReportUnlessCanceled(exception))
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private static TreeAndVersion MakeNewTreeAndVersion(SyntaxTree oldTree, SourceText oldText, VersionStamp oldVersion, SyntaxTree newTree, SourceText newText, VersionStamp newVersion)
	{
		VersionStamp version = (TopLevelChanged(oldTree, oldText, newTree, newText) ? newVersion : oldVersion);
		return TreeAndVersion.Create(newTree, version);
	}

	private static bool TopLevelChanged(SyntaxTree oldTree, SourceText oldText, SyntaxTree newTree, SourceText newText)
	{
		TextChangeRange encompassingTextChangeRange = newText.GetEncompassingTextChangeRange(oldText);
		if (encompassingTextChangeRange == default(TextChangeRange))
		{
			return false;
		}
		if (oldText.Length < 4096 && newText.Length < 4096)
		{
			return !newTree.IsEquivalentTo(oldTree, topLevel: true);
		}
		if (encompassingTextChangeRange.NewLength == newText.Length)
		{
			return true;
		}
		if (encompassingTextChangeRange.Span.Length < 4096 && encompassingTextChangeRange.NewLength < 4096)
		{
			return !newTree.IsEquivalentTo(oldTree, topLevel: true);
		}
		return true;
	}

	public DocumentState UpdateFolders(IList<string> folders)
	{
		return new DocumentState(LanguageServices, SolutionServices, Info.WithFolders(folders), parseOptions, TextSource, treeSource);
	}

	public new DocumentState UpdateText(SourceText newText, PreservationMode mode)
	{
		if (newText == null)
		{
			throw new ArgumentNullException("newText");
		}
		if (mode == PreservationMode.PreserveIdentity)
		{
			DocumentBranch firstBranch = _firstBranch;
			if (firstBranch != null && firstBranch.Text == newText)
			{
				return firstBranch.State;
			}
		}
		VersionStamp newerVersion = GetNewerVersion();
		TextAndVersion newTextAndVersion = TextAndVersion.Create(newText, newerVersion, FilePath);
		DocumentState documentState = UpdateText(newTextAndVersion, mode);
		if (mode == PreservationMode.PreserveIdentity && _firstBranch == null)
		{
			Interlocked.CompareExchange(ref _firstBranch, new DocumentBranch(newText, documentState), null);
		}
		return documentState;
	}

	public DocumentState ReParse(ParseOptions newParseOptions)
	{
		ValueSource<TreeAndVersion> valueSource = CreateLazyFullyParsedTree(TextSource, Id.ProjectId, GetSyntaxTreeFilePath(Info), newParseOptions, LanguageServices, SolutionServices);
		return new DocumentState(LanguageServices, SolutionServices, Info, newParseOptions, TextSource, valueSource);
	}

	public new DocumentState UpdateText(TextAndVersion newTextAndVersion, PreservationMode mode)
	{
		if (newTextAndVersion == null)
		{
			throw new ArgumentNullException("newTextAndVersion");
		}
		ValueSource<TextAndVersion> valueSource = ((mode == PreservationMode.PreserveIdentity) ? TextDocumentState.CreateStrongText(newTextAndVersion) : TextDocumentState.CreateRecoverableText(newTextAndVersion, SolutionServices));
		ValueSource<TreeAndVersion> valueSource2 = ((!SupportsSyntaxTree) ? ValueSource<TreeAndVersion>.Empty : CreateLazyIncrementallyParsedTree(treeSource, valueSource));
		return new DocumentState(LanguageServices, SolutionServices, Info, parseOptions, valueSource, valueSource2);
	}

	public new DocumentState UpdateText(TextLoader loader, PreservationMode mode)
	{
		if (loader == null)
		{
			throw new ArgumentNullException("loader");
		}
		ValueSource<TextAndVersion> valueSource = ((mode == PreservationMode.PreserveIdentity) ? TextDocumentState.CreateStrongText(loader, Id, SolutionServices, reportInvalidDataException: true) : TextDocumentState.CreateRecoverableText(loader, Id, SolutionServices, reportInvalidDataException: true));
		ValueSource<TreeAndVersion> valueSource2 = ((!SupportsSyntaxTree) ? ValueSource<TreeAndVersion>.Empty : CreateLazyFullyParsedTree(valueSource, Id.ProjectId, GetSyntaxTreeFilePath(Info), parseOptions, LanguageServices, SolutionServices, mode));
		return new DocumentState(LanguageServices, SolutionServices, Info, parseOptions, valueSource, valueSource2);
	}

	internal DocumentState UpdateTree(SyntaxNode newRoot, PreservationMode mode)
	{
		if (newRoot == null)
		{
			throw new ArgumentNullException("newRoot");
		}
		VersionStamp newerVersion = GetNewerVersion();
		VersionStamp newTreeVersionForUpdatedTree = GetNewTreeVersionForUpdatedTree(newRoot, newerVersion, mode);
		Encoding encoding;
		if (TryGetSyntaxTree(out var syntaxTree))
		{
			encoding = syntaxTree.Encoding;
		}
		else
		{
			encoding = ((!TryGetText(out var text)) ? null : text.Encoding);
		}
		ISyntaxTreeFactoryService service = LanguageServices.GetService<ISyntaxTreeFactoryService>();
		Tuple<ValueSource<TextAndVersion>, TreeAndVersion> tuple = CreateRecoverableTextAndTree(newRoot, newerVersion, newTreeVersionForUpdatedTree, encoding, Info, parseOptions, service, mode);
		return new DocumentState(LanguageServices, SolutionServices, Info, parseOptions, tuple.Item1, new ConstantValueSource<TreeAndVersion>(tuple.Item2));
	}

	private VersionStamp GetNewTreeVersionForUpdatedTree(SyntaxNode newRoot, VersionStamp newTextVersion, PreservationMode mode)
	{
		if (mode != PreservationMode.PreserveIdentity)
		{
			return newTextVersion;
		}
		if (!treeSource.TryGetValue(out var value) || !value.Tree.TryGetRoot(out var resultRoot))
		{
			return newTextVersion;
		}
		if (!resultRoot.IsEquivalentTo(newRoot, topLevel: true))
		{
			return newTextVersion;
		}
		return value.Version;
	}

	private static Tuple<ValueSource<TextAndVersion>, TreeAndVersion> CreateRecoverableTextAndTree(SyntaxNode newRoot, VersionStamp textVersion, VersionStamp treeVersion, Encoding encoding, DocumentInfo info, ParseOptions parseOptions1, ISyntaxTreeFactoryService factory, PreservationMode mode)
	{
		string filePath = info.FilePath;
		SyntaxTree tree = null;
		ValueSource<TextAndVersion> valueSource;
		if (mode == PreservationMode.PreserveIdentity || !factory.CanCreateRecoverableTree(newRoot))
		{
			valueSource = new TreeTextSource(new AsyncLazy<SourceText>((CancellationToken c) => tree.GetTextAsync(c), (CancellationToken c) => tree.GetText(c), cacheResult: true), textVersion, filePath);
			tree = factory.CreateSyntaxTree(GetSyntaxTreeFilePath(info), encoding, newRoot, parseOptions1);
		}
		else
		{
			valueSource = new TreeTextSource(new CachedWeakValueSource<SourceText>(new AsyncLazy<SourceText>((CancellationToken c) => BuildRecoverableTreeTextAsync(tree, encoding, c), (CancellationToken c) => BuildRecoverableTreeText(tree, encoding, c), cacheResult: false)), textVersion, filePath);
			tree = factory.CreateRecoverableTree(info.Id.ProjectId, GetSyntaxTreeFilePath(info), valueSource, encoding, newRoot);
		}
		return Tuple.Create(valueSource, TreeAndVersion.Create(tree, treeVersion));
	}

	private static SourceText BuildRecoverableTreeText(SyntaxTree tree, Encoding encoding, CancellationToken cancellationToken)
	{
		return tree.GetRoot(cancellationToken).GetText(encoding);
	}

	private static async Task<SourceText> BuildRecoverableTreeTextAsync(SyntaxTree tree, Encoding encoding, CancellationToken cancellationToken)
	{
		return (await tree.GetRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).GetText(encoding);
	}

	private VersionStamp GetNewerVersion()
	{
		if (TextSource.TryGetValue(out var value))
		{
			return value.Version.GetNewerVersion();
		}
		if (treeSource.TryGetValue(out var value2) && value2 != null)
		{
			return value2.Version.GetNewerVersion();
		}
		return VersionStamp.Create();
	}

	public bool TryGetSyntaxTree(out SyntaxTree syntaxTree)
	{
		syntaxTree = null;
		if (treeSource.TryGetValue(out var value) && value != null)
		{
			syntaxTree = value.Tree;
			BindSyntaxTreeToId(syntaxTree, Id);
			return true;
		}
		return false;
	}

	public async Task<SyntaxTree> GetSyntaxTreeAsync(CancellationToken cancellationToken)
	{
		TreeAndVersion treeAndVersion = await treeSource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		BindSyntaxTreeToId(treeAndVersion.Tree, Id);
		return treeAndVersion.Tree;
	}

	public bool TryGetTopLevelChangeTextVersion(out VersionStamp version)
	{
		if (treeSource.TryGetValue(out var value) && value != null)
		{
			version = value.Version;
			return true;
		}
		version = default;
		return false;
	}

	public override async Task<VersionStamp> GetTopLevelChangeTextVersionAsync(CancellationToken cancellationToken)
	{
		if (!SupportsSyntaxTree)
		{
			return await GetTextVersionAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (treeSource.TryGetValue(out var value) && value != null)
		{
			return value.Version;
		}
		return (await treeSource.GetValueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)).Version;
	}

	private static void BindSyntaxTreeToId(SyntaxTree tree, DocumentId id)
	{
		using (s_syntaxTreeToIdMapLock.DisposableWrite())
		{
			if (s_syntaxTreeToIdMap.TryGetValue(tree, out var value))
			{
				Contract.ThrowIfFalse(value == id);
			}
			else
			{
				s_syntaxTreeToIdMap.Add(tree, id);
			}
		}
	}

	public static DocumentId GetDocumentIdForTree(SyntaxTree tree)
	{
		using (s_syntaxTreeToIdMapLock.DisposableRead())
		{
			s_syntaxTreeToIdMap.TryGetValue(tree, out var value);
			return value;
		}
	}
}
