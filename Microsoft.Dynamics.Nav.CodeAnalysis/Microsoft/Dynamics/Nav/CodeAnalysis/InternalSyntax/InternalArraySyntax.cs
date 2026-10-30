using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalArraySyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _arrayKeyword;

	internal readonly InternalBracketedDimensionListSyntax _dimensionList;

	internal readonly InternalSyntaxToken _ofKeyword;

	internal override SyntaxNodeDefinition Definition => ArraySyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ArrayKeyword => _arrayKeyword;

	public InternalBracketedDimensionListSyntax DimensionList => _dimensionList;

	public InternalSyntaxToken OfKeyword => _ofKeyword;

	internal InternalArraySyntax(SyntaxKind kind, InternalSyntaxToken arrayKeyword, InternalBracketedDimensionListSyntax dimensionList, InternalSyntaxToken ofKeyword, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(arrayKeyword);
		_arrayKeyword = arrayKeyword;
		AdjustFlagsAndWidth(dimensionList);
		_dimensionList = dimensionList;
		AdjustFlagsAndWidth(ofKeyword);
		_ofKeyword = ofKeyword;
	}

	internal InternalArraySyntax(SyntaxKind kind, InternalSyntaxToken arrayKeyword, InternalBracketedDimensionListSyntax dimensionList, InternalSyntaxToken ofKeyword, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(arrayKeyword);
		_arrayKeyword = arrayKeyword;
		AdjustFlagsAndWidth(dimensionList);
		_dimensionList = dimensionList;
		AdjustFlagsAndWidth(ofKeyword);
		_ofKeyword = ofKeyword;
	}

	internal InternalArraySyntax(SyntaxKind kind, InternalSyntaxToken arrayKeyword, InternalBracketedDimensionListSyntax dimensionList, InternalSyntaxToken ofKeyword)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(arrayKeyword);
		_arrayKeyword = arrayKeyword;
		AdjustFlagsAndWidth(dimensionList);
		_dimensionList = dimensionList;
		AdjustFlagsAndWidth(ofKeyword);
		_ofKeyword = ofKeyword;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_arrayKeyword, 
			1 => _dimensionList, 
			2 => _ofKeyword, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ArraySyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitArray(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitArray(this);
	}

	public InternalArraySyntax Update(InternalSyntaxToken arrayKeyword, InternalBracketedDimensionListSyntax dimensionList, InternalSyntaxToken ofKeyword)
	{
		if (arrayKeyword != ArrayKeyword || dimensionList != DimensionList || ofKeyword != OfKeyword)
		{
			return InternalSyntaxFactory.Array(arrayKeyword, dimensionList, ofKeyword).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalArraySyntax(Kind, _arrayKeyword, _dimensionList, _ofKeyword, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalArraySyntax(Kind, _arrayKeyword, _dimensionList, _ofKeyword, GetDiagnostics(), annotations);
	}
}
