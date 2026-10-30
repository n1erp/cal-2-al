using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalInvalidPropertyExpressionSyntax : InternalPropertyExpressionSyntax
{
	internal readonly InternalIdentifierNameSyntax _leftHandSide;

	internal readonly InternalSyntaxToken _equalsToken;

	internal readonly InternalSyntaxToken _constOrFieldOrFilterKeywordToken;

	internal readonly InternalSyntaxToken _invalidExpression;

	internal override SyntaxNodeDefinition Definition => InvalidPropertyExpressionSyntaxDefinition.Instance.Value;

	public InternalIdentifierNameSyntax LeftHandSide => _leftHandSide;

	public InternalSyntaxToken EqualsToken => _equalsToken;

	public InternalSyntaxToken ConstOrFieldOrFilterKeywordToken => _constOrFieldOrFilterKeywordToken;

	public InternalSyntaxToken InvalidExpression => _invalidExpression;

	internal InternalInvalidPropertyExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constOrFieldOrFilterKeywordToken, InternalSyntaxToken invalidExpression, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		if (constOrFieldOrFilterKeywordToken != null)
		{
			AdjustFlagsAndWidth(constOrFieldOrFilterKeywordToken);
			_constOrFieldOrFilterKeywordToken = constOrFieldOrFilterKeywordToken;
		}
		if (invalidExpression != null)
		{
			AdjustFlagsAndWidth(invalidExpression);
			_invalidExpression = invalidExpression;
		}
	}

	internal InternalInvalidPropertyExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constOrFieldOrFilterKeywordToken, InternalSyntaxToken invalidExpression, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		if (constOrFieldOrFilterKeywordToken != null)
		{
			AdjustFlagsAndWidth(constOrFieldOrFilterKeywordToken);
			_constOrFieldOrFilterKeywordToken = constOrFieldOrFilterKeywordToken;
		}
		if (invalidExpression != null)
		{
			AdjustFlagsAndWidth(invalidExpression);
			_invalidExpression = invalidExpression;
		}
	}

	internal InternalInvalidPropertyExpressionSyntax(SyntaxKind kind, InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constOrFieldOrFilterKeywordToken, InternalSyntaxToken invalidExpression)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(leftHandSide);
		_leftHandSide = leftHandSide;
		AdjustFlagsAndWidth(equalsToken);
		_equalsToken = equalsToken;
		if (constOrFieldOrFilterKeywordToken != null)
		{
			AdjustFlagsAndWidth(constOrFieldOrFilterKeywordToken);
			_constOrFieldOrFilterKeywordToken = constOrFieldOrFilterKeywordToken;
		}
		if (invalidExpression != null)
		{
			AdjustFlagsAndWidth(invalidExpression);
			_invalidExpression = invalidExpression;
		}
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_leftHandSide, 
			1 => _equalsToken, 
			2 => _constOrFieldOrFilterKeywordToken, 
			3 => _invalidExpression, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new InvalidPropertyExpressionSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInvalidPropertyExpression(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitInvalidPropertyExpression(this);
	}

	public InternalInvalidPropertyExpressionSyntax Update(InternalIdentifierNameSyntax leftHandSide, InternalSyntaxToken equalsToken, InternalSyntaxToken constOrFieldOrFilterKeywordToken, InternalSyntaxToken invalidExpression)
	{
		if (leftHandSide != LeftHandSide || equalsToken != EqualsToken || constOrFieldOrFilterKeywordToken != ConstOrFieldOrFilterKeywordToken || invalidExpression != InvalidExpression)
		{
			return InternalSyntaxFactory.InvalidPropertyExpression(leftHandSide, equalsToken, constOrFieldOrFilterKeywordToken, invalidExpression).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalInvalidPropertyExpressionSyntax(Kind, _leftHandSide, _equalsToken, _constOrFieldOrFilterKeywordToken, _invalidExpression, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalInvalidPropertyExpressionSyntax(Kind, _leftHandSide, _equalsToken, _constOrFieldOrFilterKeywordToken, _invalidExpression, GetDiagnostics(), annotations);
	}
}
