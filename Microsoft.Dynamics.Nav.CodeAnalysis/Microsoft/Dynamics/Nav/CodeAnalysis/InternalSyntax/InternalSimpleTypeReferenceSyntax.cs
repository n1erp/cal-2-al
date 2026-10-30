using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalSimpleTypeReferenceSyntax : InternalTypeReferenceBaseSyntax
{
	internal readonly InternalArraySyntax _array;

	internal readonly InternalDataTypeSyntax _dataType;

	internal override SyntaxNodeDefinition Definition => SimpleTypeReferenceSyntaxDefinition.Instance.Value;

	public override InternalArraySyntax Array => _array;

	public override InternalDataTypeSyntax DataType => _dataType;

	internal InternalSimpleTypeReferenceSyntax(SyntaxKind kind, InternalArraySyntax array, InternalDataTypeSyntax dataType, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		if (array != null)
		{
			AdjustFlagsAndWidth(array);
			_array = array;
		}
		AdjustFlagsAndWidth(dataType);
		_dataType = dataType;
	}

	internal InternalSimpleTypeReferenceSyntax(SyntaxKind kind, InternalArraySyntax array, InternalDataTypeSyntax dataType, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 2;
		if (array != null)
		{
			AdjustFlagsAndWidth(array);
			_array = array;
		}
		AdjustFlagsAndWidth(dataType);
		_dataType = dataType;
	}

	internal InternalSimpleTypeReferenceSyntax(SyntaxKind kind, InternalArraySyntax array, InternalDataTypeSyntax dataType)
		: base(kind)
	{
		SlotCount = 2;
		if (array != null)
		{
			AdjustFlagsAndWidth(array);
			_array = array;
		}
		AdjustFlagsAndWidth(dataType);
		_dataType = dataType;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_array, 
			1 => _dataType, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new SimpleTypeReferenceSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSimpleTypeReference(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitSimpleTypeReference(this);
	}

	public InternalSimpleTypeReferenceSyntax Update(InternalArraySyntax array, InternalDataTypeSyntax dataType)
	{
		if (array != Array || dataType != DataType)
		{
			return InternalSyntaxFactory.SimpleTypeReference(array, dataType).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalSimpleTypeReferenceSyntax(Kind, _array, _dataType, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalSimpleTypeReferenceSyntax(Kind, _array, _dataType, GetDiagnostics(), annotations);
	}
}
