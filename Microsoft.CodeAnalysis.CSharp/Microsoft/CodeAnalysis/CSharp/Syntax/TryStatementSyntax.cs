using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class TryStatementSyntax : StatementSyntax
{
	private BlockSyntax block;

	private SyntaxNode catches;

	private FinallyClauseSyntax @finally;

	public SyntaxToken TryKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.TryStatementSyntax)Green).tryKeyword, Position, 0);

	public BlockSyntax Block => GetRed(ref block, 1);

	public SyntaxList<CatchClauseSyntax> Catches => new SyntaxList<CatchClauseSyntax>(GetRed(ref catches, 2));

	public FinallyClauseSyntax Finally => GetRed(ref @finally, 3);

	internal TryStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref block, 1), 
			2 => GetRed(ref catches, 2), 
			3 => GetRed(ref @finally, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => block, 
			2 => catches, 
			3 => @finally, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTryStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitTryStatement(this);
	}

	public TryStatementSyntax Update(SyntaxToken tryKeyword, BlockSyntax block, SyntaxList<CatchClauseSyntax> catches, FinallyClauseSyntax @finally)
	{
		if (tryKeyword != TryKeyword || block != Block || catches != Catches || @finally != Finally)
		{
			TryStatementSyntax tryStatementSyntax = SyntaxFactory.TryStatement(tryKeyword, block, catches, @finally);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tryStatementSyntax.WithAnnotations(annotations);
			}
			return tryStatementSyntax;
		}
		return this;
	}

	public TryStatementSyntax WithTryKeyword(SyntaxToken tryKeyword)
	{
		return Update(tryKeyword, Block, Catches, Finally);
	}

	public TryStatementSyntax WithBlock(BlockSyntax block)
	{
		return Update(TryKeyword, block, Catches, Finally);
	}

	public TryStatementSyntax WithCatches(SyntaxList<CatchClauseSyntax> catches)
	{
		return Update(TryKeyword, Block, catches, Finally);
	}

	public TryStatementSyntax WithFinally(FinallyClauseSyntax @finally)
	{
		return Update(TryKeyword, Block, Catches, @finally);
	}

	public TryStatementSyntax AddBlockStatements(params StatementSyntax[] items)
	{
		return WithBlock(Block.WithStatements(Block.Statements.AddRange(items)));
	}

	public TryStatementSyntax AddCatches(params CatchClauseSyntax[] items)
	{
		return WithCatches(Catches.AddRange(items));
	}
}
