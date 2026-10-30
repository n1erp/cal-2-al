using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class LabelDataTypeSyntax : DataTypeSyntax
{
	private LabelSyntax _label;

	public override SyntaxToken TypeName => new SyntaxToken(this, ((InternalLabelDataTypeSyntax)Node)._typeName, Position, 0);

	public LabelSyntax Label => GetRed(ref _label, 1);

	internal LabelDataTypeSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref _label, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return _label;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitLabelDataType(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitLabelDataType(this);
	}

	public LabelDataTypeSyntax Update(SyntaxToken typeName, LabelSyntax label)
	{
		if (typeName != TypeName || label != Label)
		{
			LabelDataTypeSyntax labelDataTypeSyntax = SyntaxFactory.LabelDataType(typeName, label);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return labelDataTypeSyntax.WithAnnotations(annotations);
			}
			return labelDataTypeSyntax;
		}
		return this;
	}

	public LabelDataTypeSyntax WithTypeName(SyntaxToken typeName)
	{
		return Update(typeName, Label);
	}

	public LabelDataTypeSyntax WithLabel(LabelSyntax label)
	{
		return Update(TypeName, label);
	}
}
