using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

public abstract class CSharpSyntaxTree : SyntaxTree
{
	private class DebuggerSyntaxTree : ParsedSyntaxTree
	{
		internal override bool SupportsLocations => true;

		public DebuggerSyntaxTree(CSharpSyntaxNode root, SourceText text)
			: base(text, text.Encoding, text.ChecksumAlgorithm, "", CSharpParseOptions.Default, root, DirectiveStack.Empty)
		{
		}
	}

	internal sealed class DummySyntaxTree : CSharpSyntaxTree
	{
		private readonly Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax _node;

		public override Encoding Encoding => Encoding.UTF8;

		public override int Length => 0;

		public override CSharpParseOptions Options => CSharpParseOptions.Default;

		public override string FilePath => string.Empty;

		public override bool HasCompilationUnitRoot => true;

		public DummySyntaxTree()
		{
			_node = CloneNodeAsRoot(SyntaxFactory.ParseCompilationUnit(string.Empty));
		}

		public override string ToString()
		{
			return string.Empty;
		}

		public override SourceText GetText(CancellationToken cancellationToken)
		{
			return SourceText.From(string.Empty, Encoding.UTF8);
		}

		public override bool TryGetText(out SourceText text)
		{
			text = SourceText.From(string.Empty, Encoding.UTF8);
			return true;
		}

		public override SyntaxReference GetReference(SyntaxNode node)
		{
			return new SimpleSyntaxReference(node);
		}

		public override CSharpSyntaxNode GetRoot(CancellationToken cancellationToken)
		{
			return _node;
		}

		public override bool TryGetRoot(out CSharpSyntaxNode root)
		{
			root = _node;
			return true;
		}

		public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
		{
			return default;
		}

		public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
		{
			return SyntaxFactory.SyntaxTree(root, options, FilePath);
		}

		public override SyntaxTree WithFilePath(string path)
		{
			return SyntaxFactory.SyntaxTree(_node, Options, path);
		}
	}

	private class ParsedSyntaxTree : CSharpSyntaxTree
	{
		private readonly CSharpParseOptions _options;

		private readonly string _path;

		private readonly CSharpSyntaxNode _root;

		private readonly bool _hasCompilationUnitRoot;

		private readonly Encoding _encodingOpt;

		private readonly SourceHashAlgorithm _checksumAlgorithm;

		private SourceText _lazyText;

		public override string FilePath => _path;

		public override Encoding Encoding => _encodingOpt;

		public override int Length => _root.FullSpan.Length;

		public override bool HasCompilationUnitRoot => _hasCompilationUnitRoot;

		public override CSharpParseOptions Options => _options;

		internal ParsedSyntaxTree(SourceText textOpt, Encoding encodingOpt, SourceHashAlgorithm checksumAlgorithm, string path, CSharpParseOptions options, CSharpSyntaxNode root, DirectiveStack directives, bool cloneRoot = true)
		{
			_lazyText = textOpt;
			_encodingOpt = encodingOpt ?? textOpt?.Encoding;
			_checksumAlgorithm = checksumAlgorithm;
			_options = options;
			_path = path ?? string.Empty;
			_root = (cloneRoot ? CloneNodeAsRoot(root) : root);
			_hasCompilationUnitRoot = root.Kind() == SyntaxKind.CompilationUnit;
			SetDirectiveStack(directives);
		}

		public override SourceText GetText(CancellationToken cancellationToken)
		{
			if (_lazyText == null)
			{
				Interlocked.CompareExchange(ref _lazyText, GetRoot(cancellationToken).GetText(_encodingOpt, _checksumAlgorithm), null);
			}
			return _lazyText;
		}

		public override bool TryGetText(out SourceText text)
		{
			text = _lazyText;
			return text != null;
		}

		public override CSharpSyntaxNode GetRoot(CancellationToken cancellationToken)
		{
			return _root;
		}

		public override bool TryGetRoot(out CSharpSyntaxNode root)
		{
			root = _root;
			return true;
		}

		public override SyntaxReference GetReference(SyntaxNode node)
		{
			return new SimpleSyntaxReference(node);
		}

		public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
		{
			if (_root == root && (object)_options == options)
			{
				return this;
			}
			return new ParsedSyntaxTree(null, _encodingOpt, _checksumAlgorithm, _path, (CSharpParseOptions)options, (CSharpSyntaxNode)root, _directives);
		}

		public override SyntaxTree WithFilePath(string path)
		{
			if (_path == path)
			{
				return this;
			}
			return new ParsedSyntaxTree(_lazyText, _encodingOpt, _checksumAlgorithm, path, _options, _root, _directives);
		}
	}

	internal static readonly SyntaxTree Dummy = new DummySyntaxTree();

	private bool _hasDirectives;

	private DirectiveStack _directives;

	private ImmutableArray<int> _preprocessorStateChangePositions;

	private ImmutableArray<DirectiveStack> _preprocessorStates;

	private CSharpLineDirectiveMap _lazyLineDirectiveMap;

	private CSharpPragmaWarningStateMap _lazyPragmaWarningStateMap;

	public new abstract CSharpParseOptions Options { get; }

	internal bool HasReferenceDirectives
	{
		get
		{
			if (Options.Kind == SourceCodeKind.Script)
			{
				return GetCompilationUnitRoot().GetReferenceDirectives().Count > 0;
			}
			return false;
		}
	}

	internal bool HasReferenceOrLoadDirectives
	{
		get
		{
			if (Options.Kind == SourceCodeKind.Script)
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax compilationUnitRoot = GetCompilationUnitRoot();
				if (compilationUnitRoot.GetReferenceDirectives().Count <= 0)
				{
					return compilationUnitRoot.GetLoadDirectives().Count > 0;
				}
				return true;
			}
			return false;
		}
	}

	protected override ParseOptions OptionsCore => Options;

	protected T CloneNodeAsRoot<T>(T node) where T : CSharpSyntaxNode
	{
		return SyntaxNode.CloneNodeAsRoot(node, this);
	}

	public new abstract CSharpSyntaxNode GetRoot(CancellationToken cancellationToken = default(CancellationToken));

	public abstract bool TryGetRoot(out CSharpSyntaxNode root);

	public new virtual Task<CSharpSyntaxNode> GetRootAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		CSharpSyntaxNode root;
		return Task.FromResult(TryGetRoot(out root) ? root : GetRoot(cancellationToken));
	}

	public Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax GetCompilationUnitRoot(CancellationToken cancellationToken = default(CancellationToken))
	{
		return (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)GetRoot(cancellationToken);
	}

	public override bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false)
	{
		return SyntaxFactory.AreEquivalent(this, tree, topLevel);
	}

	internal void SetDirectiveStack(DirectiveStack directives)
	{
		_directives = directives;
		_hasDirectives = true;
	}

	private DirectiveStack GetDirectives()
	{
		if (!_hasDirectives)
		{
			DirectiveStack directiveStack = GetRoot().CsGreen.ApplyDirectives(default);
			SetDirectiveStack(directiveStack);
		}
		return _directives;
	}

	internal bool IsAnyPreprocessorSymbolDefined(ImmutableArray<string> conditionalSymbols)
	{
		foreach (string item in conditionalSymbols)
		{
			if (IsPreprocessorSymbolDefined(item))
			{
				return true;
			}
		}
		return false;
	}

	internal bool IsPreprocessorSymbolDefined(string symbolName)
	{
		return IsPreprocessorSymbolDefined(GetDirectives(), symbolName);
	}

	private bool IsPreprocessorSymbolDefined(DirectiveStack directives, string symbolName)
	{
		return directives.IsDefined(symbolName) switch
		{
			DefineState.Defined => true, 
			DefineState.Undefined => false, 
			_ => Options.PreprocessorSymbols.Contains(symbolName), 
		};
	}

	internal bool IsPreprocessorSymbolDefined(string symbolName, int position)
	{
		if (_preprocessorStateChangePositions.IsDefault)
		{
			BuildPreprocessorStateChangeMap();
		}
		int num = _preprocessorStateChangePositions.BinarySearch(position);
		DirectiveStack directives;
		if (num < 0)
		{
			num = ~num - 1;
			directives = ((num < 0) ? DirectiveStack.Empty : _preprocessorStates[num]);
		}
		else
		{
			directives = _preprocessorStates[num];
		}
		return IsPreprocessorSymbolDefined(directives, symbolName);
	}

	private void BuildPreprocessorStateChangeMap()
	{
		DirectiveStack directiveStack = DirectiveStack.Empty;
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		ArrayBuilder<DirectiveStack> instance2 = ArrayBuilder<DirectiveStack>.GetInstance();
		foreach (Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax directive in GetRoot().GetDirectives((Microsoft.CodeAnalysis.CSharp.Syntax.DirectiveTriviaSyntax d) =>
		{
			SyntaxKind syntaxKind = d.Kind();
			return (syntaxKind - 8548 <= (SyntaxKind)3 || syntaxKind - 8554 <= SyntaxKind.List) ? true : false;
		}))
		{
			directiveStack = directive.ApplyDirectives(directiveStack);
			switch (directive.Kind())
			{
			case SyntaxKind.ElifDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.ElifDirectiveTriviaSyntax)directive).ElifKeyword.SpanStart);
				break;
			case SyntaxKind.ElseDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.ElseDirectiveTriviaSyntax)directive).ElseKeyword.SpanStart);
				break;
			case SyntaxKind.EndIfDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.EndIfDirectiveTriviaSyntax)directive).EndIfKeyword.SpanStart);
				break;
			case SyntaxKind.DefineDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.DefineDirectiveTriviaSyntax)directive).Name.SpanStart);
				break;
			case SyntaxKind.UndefDirectiveTrivia:
				instance2.Add(directiveStack);
				instance.Add(((Microsoft.CodeAnalysis.CSharp.Syntax.UndefDirectiveTriviaSyntax)directive).Name.SpanStart);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(directive.Kind());
			case SyntaxKind.IfDirectiveTrivia:
				break;
			}
		}
		ImmutableInterlocked.InterlockedInitialize(ref _preprocessorStates, instance2.ToImmutableAndFree());
		ImmutableInterlocked.InterlockedInitialize(ref _preprocessorStateChangePositions, instance.ToImmutableAndFree());
	}

	public static SyntaxTree Create(CSharpSyntaxNode root, CSharpParseOptions options = null, string path = "", Encoding encoding = null)
	{
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
		DirectiveStack directives = ((root.Kind() == SyntaxKind.CompilationUnit) ? ((Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)root).GetConditionalDirectivesStack() : DirectiveStack.Empty);
		return new ParsedSyntaxTree(null, encoding, SourceHashAlgorithm.Sha1, path, options ?? CSharpParseOptions.Default, root, directives);
	}

	internal static SyntaxTree CreateForDebugger(CSharpSyntaxNode root, SourceText text)
	{
		return new DebuggerSyntaxTree(root, text);
	}

	internal static SyntaxTree CreateWithoutClone(CSharpSyntaxNode root)
	{
		return new ParsedSyntaxTree(null, null, SourceHashAlgorithm.Sha1, "", CSharpParseOptions.Default, root, DirectiveStack.Empty, cloneRoot: false);
	}

	public static SyntaxTree ParseText(string text, CSharpParseOptions options = null, string path = "", Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ParseText(SourceText.From(text, encoding), options, path, cancellationToken);
	}

	public static SyntaxTree ParseText(SourceText text, CSharpParseOptions options = null, string path = "", CancellationToken cancellationToken = default(CancellationToken))
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		options = options ?? CSharpParseOptions.Default;
		using Lexer lexer = new Lexer(text, options);
		using LanguageParser languageParser = new LanguageParser(lexer, null, null, LexerMode.Syntax, cancellationToken);
		Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)languageParser.ParseCompilationUnit().CreateRed();
		return new ParsedSyntaxTree(text, text.Encoding, text.ChecksumAlgorithm, path, options, root, languageParser.Directives);
	}

	public override SyntaxTree WithChangedText(SourceText newText)
	{
		if (TryGetText(out var text))
		{
			IReadOnlyList<TextChangeRange> changeRanges = newText.GetChangeRanges(text);
			if (changeRanges.Count == 0 && newText == text)
			{
				return this;
			}
			return WithChanges(newText, changeRanges);
		}
		return WithChanges(newText, new TextChangeRange[1]
		{
			new TextChangeRange(new TextSpan(0, Length), newText.Length)
		});
	}

	private SyntaxTree WithChanges(SourceText newText, IReadOnlyList<TextChangeRange> changes)
	{
		if (changes == null)
		{
			throw new ArgumentNullException("changes");
		}
		CSharpSyntaxTree cSharpSyntaxTree = this;
		if (changes.Count == 1 && changes[0].Span == new TextSpan(0, Length) && changes[0].NewLength == newText.Length)
		{
			changes = null;
			cSharpSyntaxTree = null;
		}
		using Lexer lexer = new Lexer(newText, Options);
		using LanguageParser languageParser = new LanguageParser(lexer, cSharpSyntaxTree?.GetRoot(), changes);
		Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)languageParser.ParseCompilationUnit().CreateRed();
		return new ParsedSyntaxTree(newText, newText.Encoding, newText.ChecksumAlgorithm, FilePath, Options, root, languageParser.Directives);
	}

	public override IList<TextSpan> GetChangedSpans(SyntaxTree oldTree)
	{
		if (oldTree == null)
		{
			throw new ArgumentNullException("oldTree");
		}
		return SyntaxDiffer.GetPossiblyDifferentTextSpans(oldTree, this);
	}

	public override IList<TextChange> GetChanges(SyntaxTree oldTree)
	{
		if (oldTree == null)
		{
			throw new ArgumentNullException("oldTree");
		}
		return SyntaxDiffer.GetTextChanges(oldTree, this);
	}

	public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
	{
		return new FileLinePositionSpan(FilePath, GetLinePosition(span.Start), GetLinePosition(span.End));
	}

	public override FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_lazyLineDirectiveMap == null)
		{
			Interlocked.CompareExchange(ref _lazyLineDirectiveMap, new CSharpLineDirectiveMap(this), null);
		}
		return _lazyLineDirectiveMap.TranslateSpan(GetText(cancellationToken), FilePath, span);
	}

	public override LineVisibility GetLineVisibility(int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (_lazyLineDirectiveMap == null)
		{
			Interlocked.CompareExchange(ref _lazyLineDirectiveMap, new CSharpLineDirectiveMap(this), null);
		}
		return _lazyLineDirectiveMap.GetLineVisibility(GetText(cancellationToken), position);
	}

	internal override FileLinePositionSpan GetMappedLineSpanAndVisibility(TextSpan span, out bool isHiddenPosition)
	{
		if (_lazyLineDirectiveMap == null)
		{
			Interlocked.CompareExchange(ref _lazyLineDirectiveMap, new CSharpLineDirectiveMap(this), null);
		}
		return _lazyLineDirectiveMap.TranslateSpanAndVisibility(GetText(), FilePath, span, out isHiddenPosition);
	}

	public override bool HasHiddenRegions()
	{
		if (_lazyLineDirectiveMap == null)
		{
			Interlocked.CompareExchange(ref _lazyLineDirectiveMap, new CSharpLineDirectiveMap(this), null);
		}
		return _lazyLineDirectiveMap.HasAnyHiddenRegions();
	}

	internal ReportDiagnostic GetPragmaDirectiveWarningState(string id, int position)
	{
		if (_lazyPragmaWarningStateMap == null)
		{
			Interlocked.CompareExchange(ref _lazyPragmaWarningStateMap, new CSharpPragmaWarningStateMap(this), null);
		}
		return _lazyPragmaWarningStateMap.GetWarningState(id, position);
	}

	private LinePosition GetLinePosition(int position)
	{
		return GetText().Lines.GetLinePosition(position);
	}

	public override Location GetLocation(TextSpan span)
	{
		return new SourceLocation(this, span);
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		return GetDiagnostics(node.Green, node.Position);
	}

	private IEnumerable<Diagnostic> GetDiagnostics(GreenNode greenNode, int position)
	{
		if (greenNode == null)
		{
			throw new InvalidOperationException();
		}
		if (greenNode.ContainsDiagnostics)
		{
			return EnumerateDiagnostics(greenNode, position);
		}
		return SpecializedCollections.EmptyEnumerable<Diagnostic>();
	}

	private IEnumerable<Diagnostic> EnumerateDiagnostics(GreenNode node, int position)
	{
		SyntaxTreeDiagnosticEnumerator enumerator = new SyntaxTreeDiagnosticEnumerator(this, node, position);
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token)
	{
		return GetDiagnostics(token.Node, token.Position);
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia)
	{
		return GetDiagnostics(trivia.UnderlyingNode, trivia.Position);
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken)
	{
		return GetDiagnostics(nodeOrToken.UnderlyingNode, nodeOrToken.Position);
	}

	public override IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(GetRoot(cancellationToken));
	}

	protected override SyntaxNode GetRootCore(CancellationToken cancellationToken)
	{
		return GetRoot(cancellationToken);
	}

	protected override async Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken)
	{
		return await GetRootAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected override bool TryGetRootCore(out SyntaxNode root)
	{
		if (TryGetRoot(out var root2))
		{
			root = root2;
			return true;
		}
		root = null;
		return false;
	}
}
