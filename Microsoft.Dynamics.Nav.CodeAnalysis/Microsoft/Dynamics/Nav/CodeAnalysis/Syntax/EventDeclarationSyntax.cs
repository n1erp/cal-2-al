using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class EventDeclarationSyntax : MemberSyntax
{
	private SyntaxNode _attributes;

	private IdentifierNameSyntax _name;

	private ParameterListSyntax _parameterList;

	public SyntaxList<MemberAttributeSyntax> Attributes => new SyntaxList<MemberAttributeSyntax>(GetRed(ref _attributes, 0));

	public SyntaxToken EventKeyword => new SyntaxToken(this, ((InternalEventDeclarationSyntax)Node)._eventKeyword, GetChildPosition(1), GetChildIndex(1));

	public IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public ParameterListSyntax ParameterList => GetRed(ref _parameterList, 3);

	public SyntaxToken SemicolonToken
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalEventDeclarationSyntax)Node)._semicolonToken;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(4), GetChildIndex(4));
			}
			return default;
		}
	}

	internal EventDeclarationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => GetRedAtZero(ref _attributes), 
			2 => GetRed(ref _name, 2), 
			3 => GetRed(ref _parameterList, 3), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => _attributes, 
			2 => _name, 
			3 => _parameterList, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEventDeclaration(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitEventDeclaration(this);
	}

	public EventDeclarationSyntax Update(SyntaxList<MemberAttributeSyntax> attributes, SyntaxToken eventKeyword, IdentifierNameSyntax name, ParameterListSyntax parameterList, SyntaxToken semicolonToken)
	{
		if (attributes != Attributes || eventKeyword != EventKeyword || name != Name || parameterList != ParameterList || semicolonToken != SemicolonToken)
		{
			EventDeclarationSyntax eventDeclarationSyntax = SyntaxFactory.EventDeclaration(attributes, eventKeyword, name, parameterList, semicolonToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return eventDeclarationSyntax.WithAnnotations(annotations);
			}
			return eventDeclarationSyntax;
		}
		return this;
	}

	public EventDeclarationSyntax WithAttributes(SyntaxList<MemberAttributeSyntax> attributes)
	{
		return Update(attributes, EventKeyword, Name, ParameterList, SemicolonToken);
	}

	public EventDeclarationSyntax WithEventKeyword(SyntaxToken eventKeyword)
	{
		return Update(Attributes, eventKeyword, Name, ParameterList, SemicolonToken);
	}

	public EventDeclarationSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(Attributes, EventKeyword, name, ParameterList, SemicolonToken);
	}

	public EventDeclarationSyntax WithParameterList(ParameterListSyntax parameterList)
	{
		return Update(Attributes, EventKeyword, Name, parameterList, SemicolonToken);
	}

	public EventDeclarationSyntax WithSemicolonToken(SyntaxToken semicolonToken)
	{
		return Update(Attributes, EventKeyword, Name, ParameterList, semicolonToken);
	}

	public EventDeclarationSyntax AddAttributes(params MemberAttributeSyntax[] items)
	{
		return WithAttributes(Attributes.AddRange(items));
	}

	public EventDeclarationSyntax AddParameterListParameters(params ParameterSyntax[] items)
	{
		return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
	}
}
