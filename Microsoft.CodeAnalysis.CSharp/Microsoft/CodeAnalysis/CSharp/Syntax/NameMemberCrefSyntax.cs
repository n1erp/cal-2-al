using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class NameMemberCrefSyntax : MemberCrefSyntax
{
	private TypeSyntax name;

	private CrefParameterListSyntax parameters;

	public TypeSyntax Name => GetRedAtZero(ref name);

	public CrefParameterListSyntax Parameters => GetRed(ref parameters, 1);

	internal NameMemberCrefSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref name), 
			1 => GetRed(ref parameters, 1), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)name, 
			1 => parameters, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitNameMemberCref(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitNameMemberCref(this);
	}

	public NameMemberCrefSyntax Update(TypeSyntax name, CrefParameterListSyntax parameters)
	{
		if (name != Name || parameters != Parameters)
		{
			NameMemberCrefSyntax nameMemberCrefSyntax = SyntaxFactory.NameMemberCref(name, parameters);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return nameMemberCrefSyntax.WithAnnotations(annotations);
			}
			return nameMemberCrefSyntax;
		}
		return this;
	}

	public NameMemberCrefSyntax WithName(TypeSyntax name)
	{
		return Update(name, Parameters);
	}

	public NameMemberCrefSyntax WithParameters(CrefParameterListSyntax parameters)
	{
		return Update(Name, parameters);
	}

	public NameMemberCrefSyntax AddParametersParameters(params CrefParameterSyntax[] items)
	{
		CrefParameterListSyntax crefParameterListSyntax = Parameters ?? SyntaxFactory.CrefParameterList();
		return WithParameters(crefParameterListSyntax.WithParameters(crefParameterListSyntax.Parameters.AddRange(items)));
	}
}
