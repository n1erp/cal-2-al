using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalBracketedDimensionListSyntax : InternalSyntaxNode
{
	internal readonly InternalSyntaxToken _openBracketToken;

	internal readonly InternalSyntaxNode _dimensions;

	internal readonly InternalSyntaxToken _closeBracketToken;

	internal override SyntaxNodeDefinition Definition => BracketedDimensionListSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken OpenBracketToken => _openBracketToken;

	public InternalSeparatedSyntaxList<InternalDimensionSyntax> Dimensions => new InternalSeparatedSyntaxList<InternalDimensionSyntax>(new InternalSyntaxList<InternalSyntaxNode>(_dimensions));

	public InternalSyntaxToken CloseBracketToken => _closeBracketToken;

	internal InternalBracketedDimensionListSyntax(SyntaxKind kind, InternalSyntaxToken openBracketToken, InternalSyntaxNode dimensions, InternalSyntaxToken closeBracketToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		_openBracketToken = openBracketToken;
		if (dimensions != null)
		{
			AdjustFlagsAndWidth(dimensions);
			_dimensions = dimensions;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		_closeBracketToken = closeBracketToken;
	}

	internal InternalBracketedDimensionListSyntax(SyntaxKind kind, InternalSyntaxToken openBracketToken, InternalSyntaxNode dimensions, InternalSyntaxToken closeBracketToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		_openBracketToken = openBracketToken;
		if (dimensions != null)
		{
			AdjustFlagsAndWidth(dimensions);
			_dimensions = dimensions;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		_closeBracketToken = closeBracketToken;
	}

	internal InternalBracketedDimensionListSyntax(SyntaxKind kind, InternalSyntaxToken openBracketToken, InternalSyntaxNode dimensions, InternalSyntaxToken closeBracketToken)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(openBracketToken);
		_openBracketToken = openBracketToken;
		if (dimensions != null)
		{
			AdjustFlagsAndWidth(dimensions);
			_dimensions = dimensions;
		}
		AdjustFlagsAndWidth(closeBracketToken);
		_closeBracketToken = closeBracketToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _openBracketToken, 
			1 => _dimensions, 
			2 => _closeBracketToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new BracketedDimensionListSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitBracketedDimensionList(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitBracketedDimensionList(this);
	}

	public InternalBracketedDimensionListSyntax Update(InternalSyntaxToken openBracketToken, InternalSeparatedSyntaxList<InternalDimensionSyntax> dimensions, InternalSyntaxToken closeBracketToken)
	{
		if (openBracketToken != OpenBracketToken || dimensions != Dimensions || closeBracketToken != CloseBracketToken)
		{
			return InternalSyntaxFactory.BracketedDimensionList(openBracketToken, dimensions, closeBracketToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalBracketedDimensionListSyntax(Kind, _openBracketToken, _dimensions, _closeBracketToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalBracketedDimensionListSyntax(Kind, _openBracketToken, _dimensions, _closeBracketToken, GetDiagnostics(), annotations);
	}
}
