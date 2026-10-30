using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class SubtypedDataTypeSyntax : DataTypeSyntax
{
	private ObjectNameOrIdSyntax _subtype;

	public override SyntaxToken TypeName => new SyntaxToken(this, ((InternalSubtypedDataTypeSyntax)Node)._typeName, Position, 0);

	public ObjectNameOrIdSyntax Subtype => GetRed(ref _subtype, 1);

	internal SubtypedDataTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _subtype, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _subtype;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSubtypedDataType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSubtypedDataType(this);
	}

	public SubtypedDataTypeSyntax Update(SyntaxToken typeName, ObjectNameOrIdSyntax subtype)
	{
		if (typeName != TypeName || subtype != Subtype)
		{
			SubtypedDataTypeSyntax subtypedDataTypeSyntax = SyntaxFactory.SubtypedDataType(typeName, subtype);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return subtypedDataTypeSyntax.WithAnnotations(annotations);
			}
			return subtypedDataTypeSyntax;
		}
		return this;
	}

	public SubtypedDataTypeSyntax WithTypeName(SyntaxToken typeName)
	{
		return Update(typeName, Subtype);
	}

	public SubtypedDataTypeSyntax WithSubtype(ObjectNameOrIdSyntax subtype)
	{
		return Update(TypeName, subtype);
	}
}
