using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

internal sealed class SourceLocation : Location, IEquatable<SourceLocation>
{
	private readonly SyntaxTree syntaxTree;

	private readonly TextSpan span;

	public override LocationKind Kind => LocationKind.SourceFile;

	public override TextSpan SourceSpan => span;

	public override SyntaxTree SourceTree => syntaxTree;

	public SourceLocation(SyntaxTree syntaxTree, TextSpan span)
	{
		this.syntaxTree = syntaxTree;
		this.span = span;
	}

	public SourceLocation(SyntaxNode node)
		: this(node.SyntaxTree, node.Span)
	{
	}

	public SourceLocation(SyntaxToken token)
		: this(token.SyntaxTree, token.Span)
	{
	}

	public SourceLocation(SyntaxNodeOrToken nodeOrToken)
		: this(nodeOrToken.SyntaxTree, nodeOrToken.Span)
	{
	}

	public SourceLocation(SyntaxTrivia trivia)
		: this(trivia.SyntaxTree, trivia.Span)
	{
	}

	public SourceLocation(SyntaxReference syntaxRef)
		: this(syntaxRef.SyntaxTree, syntaxRef.Span)
	{
	}

	public override FileLinePositionSpan GetLineSpan()
	{
		if (syntaxTree == null)
		{
			FileLinePositionSpan result = default;
			DebugAssertHelper.Assert(!result.IsValid);
			return result;
		}
		return syntaxTree.GetLineSpan(span);
	}

	public override FileLinePositionSpan GetMappedLineSpan()
	{
		if (syntaxTree == null)
		{
			FileLinePositionSpan result = default;
			DebugAssertHelper.Assert(!result.IsValid);
			return result;
		}
		return syntaxTree.GetMappedLineSpan(span);
	}

	public bool Equals(SourceLocation other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (other != null && other.syntaxTree == syntaxTree)
		{
			return other.span == span;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as SourceLocation);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(syntaxTree.GetHashCode(), span.GetHashCode());
	}

	protected override string GetDebuggerDisplay()
	{
		return base.GetDebuggerDisplay() + "\"" + syntaxTree.ToString().Substring(span.Start, span.Length) + "\"";
	}
}
