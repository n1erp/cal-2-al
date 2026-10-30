using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalRecordTypeReferenceSyntax : InternalTypeReferenceBaseSyntax
{
	internal readonly InternalArraySyntax _array;

	internal readonly InternalDataTypeSyntax _dataType;

	internal readonly InternalSyntaxToken _temporary;

	internal override SyntaxNodeDefinition Definition => RecordTypeReferenceSyntaxDefinition.Instance.Value;

	public override InternalArraySyntax Array => _array;

	public override InternalDataTypeSyntax DataType => _dataType;

	public InternalSyntaxToken Temporary => _temporary;

	internal InternalRecordTypeReferenceSyntax(SyntaxKind kind, InternalArraySyntax array, InternalDataTypeSyntax dataType, InternalSyntaxToken temporary, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		if (array != null)
		{
			AdjustFlagsAndWidth(array);
			_array = array;
		}
		AdjustFlagsAndWidth(dataType);
		_dataType = dataType;
		if (temporary != null)
		{
			AdjustFlagsAndWidth(temporary);
			_temporary = temporary;
		}
	}

	internal InternalRecordTypeReferenceSyntax(SyntaxKind kind, InternalArraySyntax array, InternalDataTypeSyntax dataType, InternalSyntaxToken temporary, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		if (array != null)
		{
			AdjustFlagsAndWidth(array);
			_array = array;
		}
		AdjustFlagsAndWidth(dataType);
		_dataType = dataType;
		if (temporary != null)
		{
			AdjustFlagsAndWidth(temporary);
			_temporary = temporary;
		}
	}

	internal InternalRecordTypeReferenceSyntax(SyntaxKind kind, InternalArraySyntax array, InternalDataTypeSyntax dataType, InternalSyntaxToken temporary)
		: base(kind)
	{
		SlotCount = 3;
		if (array != null)
		{
			AdjustFlagsAndWidth(array);
			_array = array;
		}
		AdjustFlagsAndWidth(dataType);
		_dataType = dataType;
		if (temporary != null)
		{
			AdjustFlagsAndWidth(temporary);
			_temporary = temporary;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_array, 
			1 => _dataType, 
			2 => _temporary, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new RecordTypeReferenceSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitRecordTypeReference(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitRecordTypeReference(this);
	}

	public InternalRecordTypeReferenceSyntax Update(InternalArraySyntax array, InternalDataTypeSyntax dataType, InternalSyntaxToken temporary)
	{
		if (array != Array || dataType != DataType || temporary != Temporary)
		{
			return InternalSyntaxFactory.RecordTypeReference(array, dataType, temporary).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalRecordTypeReferenceSyntax(Kind, _array, _dataType, _temporary, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalRecordTypeReferenceSyntax(Kind, _array, _dataType, _temporary, GetDiagnostics(), annotations);
	}
}
