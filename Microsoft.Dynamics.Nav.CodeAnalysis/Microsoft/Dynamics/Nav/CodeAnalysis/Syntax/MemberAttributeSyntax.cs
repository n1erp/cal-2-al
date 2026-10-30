using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class MemberAttributeSyntax : SyntaxNode
{
	private IdentifierNameSyntax _name;

	private AttributeArgumentListSyntax _argumentList;

	public SyntaxToken OpenBracketToken => new SyntaxToken(this, ((InternalMemberAttributeSyntax)Node)._openBracketToken, Position, 0);

	public IdentifierNameSyntax Name => GetRed(ref _name, 1);

	public AttributeArgumentListSyntax ArgumentList => GetRed(ref _argumentList, 2);

	public SyntaxToken CloseBracketToken => new SyntaxToken(this, ((InternalMemberAttributeSyntax)Node)._closeBracketToken, GetChildPosition(3), GetChildIndex(3));

	internal MemberAttributeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)GetRed(ref _name, 1), 
			2 => GetRed(ref _argumentList, 2), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => (SyntaxNode)_name, 
			2 => _argumentList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitMemberAttribute(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitMemberAttribute(this);
	}

	public MemberAttributeSyntax Update(SyntaxToken openBracketToken, IdentifierNameSyntax name, AttributeArgumentListSyntax argumentList, SyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || name != Name || argumentList != ArgumentList || closeBracketToken != CloseBracketToken)
		{
			MemberAttributeSyntax memberAttributeSyntax = SyntaxFactory.MemberAttribute(openBracketToken, name, argumentList, closeBracketToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return memberAttributeSyntax.WithAnnotations(annotations);
			}
			return memberAttributeSyntax;
		}
		return this;
	}

	public MemberAttributeSyntax WithOpenBracketToken(SyntaxToken openBracketToken)
	{
		return Update(openBracketToken, Name, ArgumentList, CloseBracketToken);
	}

	public MemberAttributeSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(OpenBracketToken, name, ArgumentList, CloseBracketToken);
	}

	public MemberAttributeSyntax WithArgumentList(AttributeArgumentListSyntax argumentList)
	{
		return Update(OpenBracketToken, Name, argumentList, CloseBracketToken);
	}

	public MemberAttributeSyntax WithCloseBracketToken(SyntaxToken closeBracketToken)
	{
		return Update(OpenBracketToken, Name, ArgumentList, closeBracketToken);
	}

	public MemberAttributeSyntax AddArgumentListArguments(params AttributeArgumentSyntax[] items)
	{
		AttributeArgumentListSyntax attributeArgumentListSyntax = ArgumentList ?? SyntaxFactory.AttributeArgumentList();
		return WithArgumentList(attributeArgumentListSyntax.WithArguments(attributeArgumentListSyntax.Arguments.AddRange(items)));
	}
}
