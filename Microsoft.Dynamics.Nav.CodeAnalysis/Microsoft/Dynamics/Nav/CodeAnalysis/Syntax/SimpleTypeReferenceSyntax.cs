using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class SimpleTypeReferenceSyntax : TypeReferenceBaseSyntax
{
	private ArraySyntax _array;

	private DataTypeSyntax _dataType;

	public override ArraySyntax Array => GetRedAtZero(ref _array);

	public override DataTypeSyntax DataType => GetRed(ref _dataType, 1);

	internal SimpleTypeReferenceSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)GetRedAtZero(ref _array), 
			1 => GetRed(ref _dataType, 1), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			0 => (SyntaxNode)_array, 
			1 => _dataType, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleTypeReference(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSimpleTypeReference(this);
	}

	public SimpleTypeReferenceSyntax Update(ArraySyntax array, DataTypeSyntax dataType)
	{
		if (array != Array || dataType != DataType)
		{
			SimpleTypeReferenceSyntax simpleTypeReferenceSyntax = SyntaxFactory.SimpleTypeReference(array, dataType);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return simpleTypeReferenceSyntax.WithAnnotations(annotations);
			}
			return simpleTypeReferenceSyntax;
		}
		return this;
	}

	public SimpleTypeReferenceSyntax WithArray(ArraySyntax array)
	{
		return Update(array, DataType);
	}

	public SimpleTypeReferenceSyntax WithDataType(DataTypeSyntax dataType)
	{
		return Update(Array, dataType);
	}
}
