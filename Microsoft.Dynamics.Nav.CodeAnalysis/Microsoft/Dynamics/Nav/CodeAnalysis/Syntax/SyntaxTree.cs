using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

public abstract class SyntaxTree
{
	private class ParsedSyntaxTree : SyntaxTree
	{
		private readonly SyntaxNode root;

		private SourceText lazyText;

		public override int Length => root.FullSpan.Length;

		public override string FilePath { get; }

		public override ParseOptions Options { get; }

		public override Encoding Encoding { get; }

		public override bool HasCompilationUnitRoot { get; }

		internal ParsedSyntaxTree(SourceText sourceText, Encoding encodingOpt, string path, SyntaxNode root, ParseOptions parseOptions, bool cloneRoot = true)
		{
			DebugAssertHelper.Assert(root != null);
			lazyText = sourceText;
			Encoding = encodingOpt;
			FilePath = path;
			this.root = (cloneRoot ? CloneNodeAsRoot(root) : root);
			HasCompilationUnitRoot = root.Kind == SyntaxKind.CompilationUnit;
			Options = parseOptions;
		}

		public override SyntaxNode GetRoot(CancellationToken cancellationToken = default(CancellationToken))
		{
			return root;
		}

		public override bool TryGetRoot(out SyntaxNode resultRoot)
		{
			resultRoot = root;
			return true;
		}

		public override bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false)
		{
			return SyntaxFactory.AreEquivalent(this, tree, topLevel);
		}

		public override bool TryGetText(out SourceText text)
		{
			text = lazyText;
			return text != null;
		}

		public override FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
		{
			return GetLineSpan(span);
		}

		public override SyntaxReference GetReference(SyntaxNode node)
		{
			return new SimpleSyntaxReference(node);
		}

		public override SourceText GetText(CancellationToken cancellationToken = default(CancellationToken))
		{
			if (lazyText == null)
			{
				Interlocked.CompareExchange(ref lazyText, GetRoot(cancellationToken).GetText(Encoding), null);
			}
			return lazyText;
		}
	}

	public abstract int Length { get; }

	public abstract string FilePath { get; }

	public abstract ParseOptions Options { get; }

	public abstract Encoding Encoding { get; }

	public abstract bool HasCompilationUnitRoot { get; }

	public abstract SyntaxNode GetRoot(CancellationToken cancellationToken = default(CancellationToken));

	public abstract bool TryGetRoot(out SyntaxNode resultRoot);

	public virtual Task<SyntaxNode> GetRootAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		SyntaxNode resultRoot;
		return Task.FromResult(TryGetRoot(out resultRoot) ? resultRoot : GetRoot(cancellationToken));
	}

	public abstract SyntaxReference GetReference(SyntaxNode node);

	public abstract bool TryGetText(out SourceText text);

	public virtual Task<SourceText> GetTextAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		SourceText text;
		return Task.FromResult(TryGetText(out text) ? text : GetText(cancellationToken));
	}

	public static SyntaxTree Create(SyntaxNode root, string path, Encoding encoding = null, SourceText text = null, ParseOptions options = null)
	{
		DebugAssertHelper.Assert(root != null);
		return new ParsedSyntaxTree(text, encoding, path, root, options);
	}

	internal static SyntaxTree CreateWithoutClone(SyntaxNode root)
	{
		DebugAssertHelper.Assert(root != null);
		return new ParsedSyntaxTree(null, null, string.Empty, root, ParseOptions.Default, cloneRoot: false);
	}

	protected T CloneNodeAsRoot<T>(T node) where T : SyntaxNode
	{
		return SyntaxNode.CloneNodeAsRoot(node, this);
	}

	public Location GetLocation(TextSpan span)
	{
		return new SourceLocation(this, span);
	}

	public abstract bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false);

	public FileLinePositionSpan GetLineSpan(TextSpan span)
	{
		return new FileLinePositionSpan(FilePath, GetLinePosition(span.Start), GetLinePosition(span.End));
	}

	private LinePosition GetLinePosition(int position)
	{
		return GetText().Lines.GetLinePosition(position);
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public abstract SourceText GetText(CancellationToken cancellationToken = default(CancellationToken));

	public abstract FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken));

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public CompilationUnitSyntax GetCompilationUnitRoot()
	{
		return (CompilationUnitSyntax)GetRoot();
	}

	public virtual LineVisibility GetLineVisibility(int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		return LineVisibility.Visible;
	}

	public SyntaxTree WithChangedText(SourceText newText)
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
		SyntaxTree syntaxTree = this;
		if (changes.Count == 1 && changes[0].Span == new TextSpan(0, Length) && changes[0].NewLength == newText.Length)
		{
			changes = null;
			syntaxTree = null;
		}
		try
		{
			using Lexer lexer = new Lexer(newText, Options);
			CompilationUnitSyntax root = (CompilationUnitSyntax)new ObjectParser(lexer, syntaxTree?.GetRoot(), changes).ParseObjects().CreateRed();
			return new ParsedSyntaxTree(newText, newText.Encoding, FilePath, root, Options);
		}
		catch (Exception e)
		{
			return InternalErrorSyntaxTree(e, Options);
		}
	}

	private static SyntaxTree InternalErrorSyntaxTree(Exception e, ParseOptions options)
	{
		SyntaxDiagnosticInfo syntaxDiagnosticInfo = new SyntaxDiagnosticInfo(ErrorCode.ERR_InternalError, e.Message, e.StackTrace);
		return SyntaxFactory.SyntaxTree(InternalSyntaxFactory.ObjectCompilationUnit(null, InternalSyntaxToken.Create(SyntaxKind.EndOfFileToken)).SetDiagnostics(new DiagnosticInfo[1] { syntaxDiagnosticInfo }).CreateRed(), string.Empty, null, options);
	}

	[Conditional("DEBUG")]
	private static void CheckForRoundTrip(CompilationUnitSyntax compilationUnit, SourceText newText)
	{
		string text = compilationUnit.ToFullString();
		if (!text.Equals(newText.ToString(), StringComparison.Ordinal))
		{
			throw new InvalidOperationException("Roundtrip parse failed. Parsed source: " + text);
		}
	}

	public IList<TextSpan> GetChangedSpans(SyntaxTree oldTree)
	{
		if (oldTree == null)
		{
			throw new ArgumentNullException("oldTree");
		}
		return SyntaxDiffer.GetPossiblyDifferentTextSpans(oldTree, this);
	}

	public IList<TextChange> GetChanges(SyntaxTree oldTree)
	{
		if (oldTree == null)
		{
			throw new ArgumentNullException("oldTree");
		}
		return SyntaxDiffer.GetTextChanges(oldTree, this);
	}

	public IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		return GetDiagnostics(node.Node, node.Position);
	}

	private IEnumerable<Diagnostic> GetDiagnostics(InternalSyntaxNode internalNode, int position)
	{
		if (internalNode == null)
		{
			throw new InvalidOperationException();
		}
		if (internalNode.ContainsDiagnostics)
		{
			return EnumerateDiagnostics(internalNode, position);
		}
		return SpecializedCollections.EmptyEnumerable<Diagnostic>();
	}

	private IEnumerable<Diagnostic> EnumerateDiagnostics(InternalSyntaxNode node, int position)
	{
		SyntaxTreeDiagnosticEnumerator enumerator = new SyntaxTreeDiagnosticEnumerator(this, node, position);
		while (enumerator.MoveNext())
		{
			yield return enumerator.Current;
		}
	}

	public IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token)
	{
		return GetDiagnostics(token.Node, token.Position);
	}

	public IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia)
	{
		return GetDiagnostics(trivia.UnderlyingNode, trivia.Position);
	}

	public IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken)
	{
		return GetDiagnostics(nodeOrToken.UnderlyingNode, nodeOrToken.Position);
	}

	public IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDiagnostics(GetRoot(cancellationToken));
	}

	public static SyntaxTree ParseObjectText(string text, string path = null, Encoding encoding = null, ParseOptions options = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ParseObjectText(SourceText.From(text, encoding), path, options, encoding, cancellationToken);
	}

	public static SyntaxTree ParseObjectText(SourceText text, string path = null, ParseOptions options = null, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		options = options ?? ParseOptions.Default;
		try
		{
			using Lexer lexer = new Lexer(text, options);
			CompilationUnitSyntax root = (CompilationUnitSyntax)new ObjectParser(lexer, null, null, cancellationToken).ParseObjects().CreateRed();
			return new ParsedSyntaxTree(text, encoding, path ?? string.Empty, root, options);
		}
		catch (Exception e)
		{
			return InternalErrorSyntaxTree(e, options);
		}
	}

	public override string ToString()
	{
		return GetText(CancellationToken.None).ToString();
	}
}
