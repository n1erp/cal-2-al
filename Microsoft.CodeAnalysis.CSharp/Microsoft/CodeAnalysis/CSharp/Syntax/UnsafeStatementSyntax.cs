using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class UnsafeStatementSyntax : StatementSyntax
{
	private BlockSyntax block;

	public SyntaxToken UnsafeKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.UnsafeStatementSyntax)Green).unsafeKeyword, Position, 0);

	public BlockSyntax Block => GetRed(ref block, 1);

	internal UnsafeStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitUnsafeStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitUnsafeStatement(this);
	}

	public UnsafeStatementSyntax Update(SyntaxToken unsafeKeyword, BlockSyntax block)
	{
		if (unsafeKeyword != UnsafeKeyword || block != Block)
		{
			UnsafeStatementSyntax unsafeStatementSyntax = SyntaxFactory.UnsafeStatement(unsafeKeyword, block);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return unsafeStatementSyntax.WithAnnotations(annotations);
			}
			return unsafeStatementSyntax;
		}
		return this;
	}

	public UnsafeStatementSyntax WithUnsafeKeyword(SyntaxToken unsafeKeyword)
	{
		return Update(unsafeKeyword, Block);
	}

	public UnsafeStatementSyntax WithBlock(BlockSyntax block)
	{
		return Update(UnsafeKeyword, block);
	}

	public UnsafeStatementSyntax AddBlockStatements(params StatementSyntax[] items)
	{
		return WithBlock(Block.WithStatements(Block.Statements.AddRange(items)));
	}
}
