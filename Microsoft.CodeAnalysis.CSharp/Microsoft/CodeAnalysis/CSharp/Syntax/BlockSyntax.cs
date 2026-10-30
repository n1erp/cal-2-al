using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class BlockSyntax : StatementSyntax
{
	private SyntaxNode statements;

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)Green).openBraceToken, Position, 0);

	public SyntaxList<StatementSyntax> Statements => new SyntaxList<StatementSyntax>(GetRed(ref statements, 1));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.BlockSyntax)Green).closeBraceToken, GetChildPosition(2), GetChildIndex(2));

	internal BlockSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref statements, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return statements;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBlock(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitBlock(this);
	}

	public BlockSyntax Update(SyntaxToken openBraceToken, SyntaxList<StatementSyntax> statements, SyntaxToken closeBraceToken)
	{
		if (openBraceToken != OpenBraceToken || statements != Statements || closeBraceToken != CloseBraceToken)
		{
			BlockSyntax blockSyntax = SyntaxFactory.Block(openBraceToken, statements, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return blockSyntax.WithAnnotations(annotations);
			}
			return blockSyntax;
		}
		return this;
	}

	public BlockSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(openBraceToken, Statements, CloseBraceToken);
	}

	public BlockSyntax WithStatements(SyntaxList<StatementSyntax> statements)
	{
		return Update(OpenBraceToken, statements, CloseBraceToken);
	}

	public BlockSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(OpenBraceToken, Statements, closeBraceToken);
	}

	public BlockSyntax AddStatements(params StatementSyntax[] items)
	{
		return WithStatements(Statements.AddRange(items));
	}
}
