using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class OperatorMemberCrefSyntax : MemberCrefSyntax
{
	private CrefParameterListSyntax parameters;

	public SyntaxToken OperatorKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorMemberCrefSyntax)Green).operatorKeyword, Position, 0);

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorMemberCrefSyntax)Green).operatorToken, GetChildPosition(1), GetChildIndex(1));

	public CrefParameterListSyntax Parameters => GetRed(ref parameters, 2);

	internal OperatorMemberCrefSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 2)
		{
			return GetRed(ref parameters, 2);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 2)
		{
			return parameters;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitOperatorMemberCref(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOperatorMemberCref(this);
	}

	public OperatorMemberCrefSyntax Update(SyntaxToken operatorKeyword, SyntaxToken operatorToken, CrefParameterListSyntax parameters)
	{
		if (operatorKeyword != OperatorKeyword || operatorToken != OperatorToken || parameters != Parameters)
		{
			OperatorMemberCrefSyntax operatorMemberCrefSyntax = SyntaxFactory.OperatorMemberCref(operatorKeyword, operatorToken, parameters);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return operatorMemberCrefSyntax.WithAnnotations(annotations);
			}
			return operatorMemberCrefSyntax;
		}
		return this;
	}

	public OperatorMemberCrefSyntax WithOperatorKeyword(SyntaxToken operatorKeyword)
	{
		return Update(operatorKeyword, OperatorToken, Parameters);
	}

	public OperatorMemberCrefSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(OperatorKeyword, operatorToken, Parameters);
	}

	public OperatorMemberCrefSyntax WithParameters(CrefParameterListSyntax parameters)
	{
		return Update(OperatorKeyword, OperatorToken, parameters);
	}

	public OperatorMemberCrefSyntax AddParametersParameters(params CrefParameterSyntax[] items)
	{
		CrefParameterListSyntax crefParameterListSyntax = Parameters ?? SyntaxFactory.CrefParameterList();
		return WithParameters(crefParameterListSyntax.WithParameters(crefParameterListSyntax.Parameters.AddRange(items)));
	}
}
