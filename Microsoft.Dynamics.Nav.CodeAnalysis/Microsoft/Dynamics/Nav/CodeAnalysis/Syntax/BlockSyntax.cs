using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class BlockSyntax : StatementSyntax
{
	private SyntaxNode _statements;

	public SyntaxToken BeginKeywordToken => new SyntaxToken(this, ((InternalBlockSyntax)Node)._beginKeywordToken, Position, 0);

	public SyntaxList<StatementSyntax> Statements => new SyntaxList<StatementSyntax>(GetRed(ref _statements, 1));

	public SyntaxToken EndKeywordToken => new SyntaxToken(this, ((InternalBlockSyntax)Node)._endKeywordToken, GetChildPosition(2), GetChildIndex(2));

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalBlockSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(3), GetChildIndex(3));
			}
			return default;
		}
	}

	internal BlockSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _statements, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _statements;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBlock(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitBlock(this);
	}

	public BlockSyntax Update(SyntaxToken beginKeywordToken, SyntaxList<StatementSyntax> statements, SyntaxToken endKeywordToken, SyntaxToken semicolonToken)
	{
		if (beginKeywordToken != BeginKeywordToken || statements != Statements || endKeywordToken != EndKeywordToken || semicolonToken != SemicolonToken)
		{
			BlockSyntax blockSyntax = SyntaxFactory.Block(beginKeywordToken, statements, endKeywordToken, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return blockSyntax.WithAnnotations(annotations);
			}
			return blockSyntax;
		}
		return this;
	}

	public BlockSyntax WithBeginKeywordToken(SyntaxToken beginKeywordToken)
	{
		return Update(beginKeywordToken, Statements, EndKeywordToken, SemicolonToken);
	}

	public BlockSyntax WithStatements(SyntaxList<StatementSyntax> statements)
	{
		return Update(BeginKeywordToken, statements, EndKeywordToken, SemicolonToken);
	}

	public BlockSyntax WithEndKeywordToken(SyntaxToken endKeywordToken)
	{
		return Update(BeginKeywordToken, Statements, endKeywordToken, SemicolonToken);
	}

	public BlockSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(BeginKeywordToken, Statements, EndKeywordToken, semicolonToken);
	}

	public BlockSyntax AddStatements(params StatementSyntax[] items)
	{
		return WithStatements(Statements.AddRange(items));
	}
}
