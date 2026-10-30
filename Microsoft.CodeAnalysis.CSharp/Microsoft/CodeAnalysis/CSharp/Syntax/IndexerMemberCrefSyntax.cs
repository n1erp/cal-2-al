using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class IndexerMemberCrefSyntax : MemberCrefSyntax
{
	private CrefBracketedParameterListSyntax parameters;

	public SyntaxToken ThisKeyword => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.IndexerMemberCrefSyntax)Green).thisKeyword, Position, 0);

	public CrefBracketedParameterListSyntax Parameters => GetRed(ref parameters, 1);

	internal IndexerMemberCrefSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref parameters, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return parameters;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIndexerMemberCref(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIndexerMemberCref(this);
	}

	public IndexerMemberCrefSyntax Update(SyntaxToken thisKeyword, CrefBracketedParameterListSyntax parameters)
	{
		if (thisKeyword != ThisKeyword || parameters != Parameters)
		{
			IndexerMemberCrefSyntax indexerMemberCrefSyntax = SyntaxFactory.IndexerMemberCref(thisKeyword, parameters);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return indexerMemberCrefSyntax.WithAnnotations(annotations);
			}
			return indexerMemberCrefSyntax;
		}
		return this;
	}

	public IndexerMemberCrefSyntax WithThisKeyword(SyntaxToken thisKeyword)
	{
		return Update(thisKeyword, Parameters);
	}

	public IndexerMemberCrefSyntax WithParameters(CrefBracketedParameterListSyntax parameters)
	{
		return Update(ThisKeyword, parameters);
	}

	public IndexerMemberCrefSyntax AddParametersParameters(params CrefParameterSyntax[] items)
	{
		CrefBracketedParameterListSyntax crefBracketedParameterListSyntax = Parameters ?? SyntaxFactory.CrefBracketedParameterList();
		return WithParameters(crefBracketedParameterListSyntax.WithParameters(crefBracketedParameterListSyntax.Parameters.AddRange(items)));
	}
}
