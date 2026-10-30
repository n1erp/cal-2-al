using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldModificationSyntax : FieldBaseSyntax
{
	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private SyntaxNode _triggers;

	public override SyntaxToken FieldKeyword => new SyntaxToken(this, ((InternalFieldModificationSyntax)Node)._fieldKeyword, Position, 0);

	public override SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalFieldModificationSyntax)Node)._openParenthesisToken, GetChildPosition(1), GetChildIndex(1));

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public override SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalFieldModificationSyntax)Node)._closeParenthesisToken, GetChildPosition(3), GetChildIndex(3));

	public override SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalFieldModificationSyntax)Node)._openBraceToken, GetChildPosition(4), GetChildIndex(4));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 5);

	public override SyntaxList<TriggerDeclarationSyntax> Triggers => new SyntaxList<TriggerDeclarationSyntax>(GetRed(ref _triggers, 6));

	public override SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalFieldModificationSyntax)Node)._closeBraceToken, GetChildPosition(7), GetChildIndex(7));

	internal FieldModificationSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			2 => GetRed(ref _name, 2), 
			5 => GetRed(ref _propertyList, 5), 
			6 => GetRed(ref _triggers, 6), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			2 => _name, 
			5 => _propertyList, 
			6 => _triggers, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldModification(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldModification(this);
	}

	public FieldModificationSyntax Update(SyntaxToken fieldKeyword, SyntaxToken openParenthesisToken, IdentifierNameSyntax name, SyntaxToken closeParenthesisToken, SyntaxToken openBraceToken, PropertyListSyntax propertyList, SyntaxList<TriggerDeclarationSyntax> triggers, SyntaxToken closeBraceToken)
	{
		if (fieldKeyword != FieldKeyword || openParenthesisToken != OpenParenthesisToken || name != Name || closeParenthesisToken != CloseParenthesisToken || openBraceToken != OpenBraceToken || propertyList != PropertyList || triggers != Triggers || closeBraceToken != CloseBraceToken)
		{
			FieldModificationSyntax fieldModificationSyntax = SyntaxFactory.FieldModification(fieldKeyword, openParenthesisToken, name, closeParenthesisToken, openBraceToken, propertyList, triggers, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldModificationSyntax.WithAnnotations(annotations);
			}
			return fieldModificationSyntax;
		}
		return this;
	}

	public FieldModificationSyntax WithFieldKeyword(SyntaxToken fieldKeyword)
	{
		return Update(fieldKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldModificationSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(FieldKeyword, openParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldModificationSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(FieldKeyword, OpenParenthesisToken, name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldModificationSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(FieldKeyword, OpenParenthesisToken, Name, closeParenthesisToken, OpenBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldModificationSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(FieldKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, openBraceToken, PropertyList, Triggers, CloseBraceToken);
	}

	public FieldModificationSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(FieldKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, propertyList, Triggers, CloseBraceToken);
	}

	public FieldModificationSyntax WithTriggers(SyntaxList<TriggerDeclarationSyntax> triggers)
	{
		return Update(FieldKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, triggers, CloseBraceToken);
	}

	public FieldModificationSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(FieldKeyword, OpenParenthesisToken, Name, CloseParenthesisToken, OpenBraceToken, PropertyList, Triggers, closeBraceToken);
	}

	public FieldModificationSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public FieldModificationSyntax AddTriggers(params TriggerDeclarationSyntax[] items)
	{
		return WithTriggers(Triggers.AddRange(items));
	}
}
