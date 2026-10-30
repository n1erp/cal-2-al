using System;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class SourceLocation : Location, IEquatable<SourceLocation>
{
	private readonly SyntaxTree _syntaxTree;

	private readonly TextSpan _span;

	public override LocationKind Kind => LocationKind.SourceFile;

	public override TextSpan SourceSpan => _span;

	public override SyntaxTree SourceTree => _syntaxTree;

	public SourceLocation(SyntaxTree syntaxTree, TextSpan span)
	{
		_syntaxTree = syntaxTree;
		_span = span;
	}

	public SourceLocation(SyntaxNode node)
		: this(node.SyntaxTree, node.Span)
	{
	}

	public SourceLocation(in SyntaxToken token)
		: this(token.SyntaxTree, token.Span)
	{
	}

	public SourceLocation(in SyntaxNodeOrToken nodeOrToken)
		: this(nodeOrToken.SyntaxTree, nodeOrToken.Span)
	{
	}

	public SourceLocation(in SyntaxTrivia trivia)
		: this(trivia.SyntaxTree, trivia.Span)
	{
	}

	public SourceLocation(SyntaxReference syntaxRef)
		: this(syntaxRef.SyntaxTree, syntaxRef.Span)
	{
	}

	public override FileLinePositionSpan GetLineSpan()
	{
		if (_syntaxTree == null)
		{
			return default;
		}
		return _syntaxTree.GetLineSpan(_span);
	}

	public override FileLinePositionSpan GetMappedLineSpan()
	{
		if (_syntaxTree == null)
		{
			return default;
		}
		return _syntaxTree.GetMappedLineSpan(_span);
	}

	public bool Equals(SourceLocation other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (other != null && other._syntaxTree == _syntaxTree)
		{
			return other._span == _span;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SourceLocation);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_syntaxTree, _span.GetHashCode());
	}

	protected override string GetDebuggerDisplay()
	{
		return base.GetDebuggerDisplay() + "\"" + _syntaxTree.ToString().Substring(_span.Start, _span.Length) + "\"";
	}
}
