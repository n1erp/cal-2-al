using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class FinallyClauseSyntax : CSharpSyntaxNode
{
	private BlockSyntax block;

	public SyntaxToken FinallyKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.FinallyClauseSyntax)Green).finallyKeyword, Position, 0);

	public BlockSyntax Block => GetRed(ref block, 1);

	internal FinallyClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref block, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return block;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFinallyClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitFinallyClause(this);
	}

	public FinallyClauseSyntax Update(SyntaxToken finallyKeyword, BlockSyntax block)
	{
		if (finallyKeyword != FinallyKeyword || block != Block)
		{
			FinallyClauseSyntax finallyClauseSyntax = SyntaxFactory.FinallyClause(finallyKeyword, block);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return finallyClauseSyntax.WithAnnotations(annotations);
			}
			return finallyClauseSyntax;
		}
		return this;
	}

	public FinallyClauseSyntax WithFinallyKeyword(SyntaxToken finallyKeyword)
	{
		return Update(finallyKeyword, Block);
	}

	public FinallyClauseSyntax WithBlock(BlockSyntax block)
	{
		return Update(FinallyKeyword, block);
	}

	public FinallyClauseSyntax AddBlockStatements(params StatementSyntax[] items)
	{
		return WithBlock(Block.WithStatements(Block.Statements.AddRange(items)));
	}
}
