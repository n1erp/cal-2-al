using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class RecordTypeReferenceSyntax : TypeReferenceBaseSyntax
{
	private ArraySyntax _array;

	private DataTypeSyntax _dataType;

	public override ArraySyntax Array => GetRedAtZero(ref _array);

	public override DataTypeSyntax DataType => GetRed(ref _dataType, 1);

	public SyntaxToken Temporary
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalRecordTypeReferenceSyntax)Node)._temporary;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, GetChildPosition(2), GetChildIndex(2));
			}
			return default;
		}
	}

	internal RecordTypeReferenceSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitRecordTypeReference(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitRecordTypeReference(this);
	}

	public RecordTypeReferenceSyntax Update(ArraySyntax array, DataTypeSyntax dataType, SyntaxToken temporary)
	{
		if (array != Array || dataType != DataType || temporary != Temporary)
		{
			RecordTypeReferenceSyntax recordTypeReferenceSyntax = SyntaxFactory.RecordTypeReference(array, dataType, temporary);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return recordTypeReferenceSyntax.WithAnnotations(annotations);
			}
			return recordTypeReferenceSyntax;
		}
		return this;
	}

	public RecordTypeReferenceSyntax WithArray(ArraySyntax array)
	{
		return Update(array, DataType, Temporary);
	}

	public RecordTypeReferenceSyntax WithDataType(DataTypeSyntax dataType)
	{
		return Update(Array, dataType, Temporary);
	}

	public RecordTypeReferenceSyntax WithTemporary(SyntaxToken temporary)
	{
		return Update(Array, DataType, temporary);
	}
}
