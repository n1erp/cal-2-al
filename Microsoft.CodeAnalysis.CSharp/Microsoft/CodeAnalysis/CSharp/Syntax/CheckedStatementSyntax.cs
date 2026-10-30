using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class CheckedStatementSyntax : StatementSyntax
{
	private BlockSyntax block;

	public SyntaxToken Keyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CheckedStatementSyntax)Green).keyword, Position, 0);

	public BlockSyntax Block => GetRed(ref block, 1);

	internal CheckedStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitCheckedStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCheckedStatement(this);
	}

	public CheckedStatementSyntax Update(SyntaxToken keyword, BlockSyntax block)
	{
		if (keyword != Keyword || block != Block)
		{
			CheckedStatementSyntax checkedStatementSyntax = SyntaxFactory.CheckedStatement(Kind(), keyword, block);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return checkedStatementSyntax.WithAnnotations(annotations);
			}
			return checkedStatementSyntax;
		}
		return this;
	}

	public CheckedStatementSyntax WithKeyword(SyntaxToken keyword)
	{
		return Update(keyword, Block);
	}

	public CheckedStatementSyntax WithBlock(BlockSyntax block)
	{
		return Update(Keyword, block);
	}

	public CheckedStatementSyntax AddBlockStatements(params StatementSyntax[] items)
	{
		return WithBlock(Block.WithStatements(Block.Statements.AddRange(items)));
	}
}
