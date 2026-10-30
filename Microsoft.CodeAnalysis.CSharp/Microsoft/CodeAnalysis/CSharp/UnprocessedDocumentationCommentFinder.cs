using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal class UnprocessedDocumentationCommentFinder : CSharpSyntaxWalker
{
	private readonly DiagnosticBag _diagnostics;

	private readonly CancellationToken _cancellationToken;

	private readonly TextSpan? _filterSpanWithinTree;

	private bool _isValidLocation;

	private UnprocessedDocumentationCommentFinder(DiagnosticBag diagnostics, TextSpan? filterSpanWithinTree, CancellationToken cancellationToken)
		: base(SyntaxWalkerDepth.Trivia)
	{
		_diagnostics = diagnostics;
		_filterSpanWithinTree = filterSpanWithinTree;
		_cancellationToken = cancellationToken;
	}

	public static void ReportUnprocessed(SyntaxTree tree, TextSpan? filterSpanWithinTree, DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		if (tree.ReportDocumentationCommentDiagnostics())
		{
			new UnprocessedDocumentationCommentFinder(diagnostics, filterSpanWithinTree, cancellationToken).Visit(tree.GetRoot(cancellationToken));
		}
	}

	private bool IsSyntacticallyFilteredOut(TextSpan fullSpan)
	{
		if (_filterSpanWithinTree.HasValue)
		{
			return !_filterSpanWithinTree.Value.Contains(fullSpan);
		}
		return false;
	}

	public override void DefaultVisit(SyntaxNode node)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (IsSyntacticallyFilteredOut(node.FullSpan))
		{
			return;
		}
		if (!node.HasStructuredTrivia)
		{
			if (node.Span.Length > 0)
			{
				_isValidLocation = false;
			}
			return;
		}
		if (node is BaseTypeDeclarationSyntax || node is DelegateDeclarationSyntax || node is EnumMemberDeclarationSyntax || node is BaseMethodDeclarationSyntax || node is BasePropertyDeclarationSyntax || node is BaseFieldDeclarationSyntax)
		{
			_isValidLocation = true;
		}
		base.DefaultVisit(node);
	}

	public override void VisitLeadingTrivia(SyntaxToken token)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (!IsSyntacticallyFilteredOut(token.FullSpan))
		{
			base.VisitLeadingTrivia(token);
			_isValidLocation = false;
		}
	}

	public override void VisitTrivia(SyntaxTrivia trivia)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (!IsSyntacticallyFilteredOut(trivia.FullSpan))
		{
			if (!_isValidLocation && SyntaxFacts.IsDocumentationCommentTrivia(trivia.Kind()))
			{
				int position = trivia.Position;
				_diagnostics.Add(ErrorCode.WRN_UnprocessedXMLComment, new SourceLocation(trivia.SyntaxTree, new TextSpan(position, 1)));
			}
			base.VisitTrivia(trivia);
		}
	}
}
