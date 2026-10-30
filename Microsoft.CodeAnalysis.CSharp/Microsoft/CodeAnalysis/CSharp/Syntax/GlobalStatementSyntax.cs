using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class GlobalStatementSyntax : MemberDeclarationSyntax
{
	private StatementSyntax statement;

	public StatementSyntax Statement => GetRedAtZero(ref statement);

	internal GlobalStatementSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref statement);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return statement;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitGlobalStatement(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitGlobalStatement(this);
	}

	public GlobalStatementSyntax Update(StatementSyntax statement)
	{
		if (statement != Statement)
		{
			GlobalStatementSyntax globalStatementSyntax = SyntaxFactory.GlobalStatement(statement);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return globalStatementSyntax.WithAnnotations(annotations);
			}
			return globalStatementSyntax;
		}
		return this;
	}

	public GlobalStatementSyntax WithStatement(StatementSyntax statement)
	{
		return Update(statement);
	}
}
