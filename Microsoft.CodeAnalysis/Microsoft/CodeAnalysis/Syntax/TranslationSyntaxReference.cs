using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Syntax;

internal abstract class TranslationSyntaxReference : SyntaxReference
{
	private readonly SyntaxReference _reference;

	public sealed override TextSpan Span => _reference.Span;

	public sealed override SyntaxTree SyntaxTree => _reference.SyntaxTree;

	protected TranslationSyntaxReference(SyntaxReference reference)
	{
		_reference = reference;
	}

	public sealed override SyntaxNode GetSyntax(CancellationToken cancellationToken = default(CancellationToken))
	{
		return Translate(_reference, cancellationToken);
	}

	protected abstract SyntaxNode Translate(SyntaxReference reference, CancellationToken cancellationToken);
}
