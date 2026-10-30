using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalFieldCalculationFormulaSyntax : InternalCalculationFormulaPropertyValueSyntax
{
	internal readonly InternalSyntaxToken _sign;

	internal readonly InternalSyntaxToken _formulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalMemberAccessExpressionSyntax _field;

	internal readonly InternalWhereExpressionSyntax _whereExpression;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => FieldCalculationFormulaSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken Sign => _sign;

	public InternalSyntaxToken FormulaKeywordToken => _formulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalMemberAccessExpressionSyntax Field => _field;

	public InternalWhereExpressionSyntax WhereExpression => _whereExpression;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalFieldCalculationFormulaSyntax(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalMemberAccessExpressionSyntax field, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 6;
		if (sign != null)
		{
			AdjustFlagsAndWidth(sign);
			_sign = sign;
		}
		AdjustFlagsAndWidth(formulaKeywordToken);
		_formulaKeywordToken = formulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(field);
		_field = field;
		if (whereExpression != null)
		{
			AdjustFlagsAndWidth(whereExpression);
			_whereExpression = whereExpression;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalFieldCalculationFormulaSyntax(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalMemberAccessExpressionSyntax field, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 6;
		if (sign != null)
		{
			AdjustFlagsAndWidth(sign);
			_sign = sign;
		}
		AdjustFlagsAndWidth(formulaKeywordToken);
		_formulaKeywordToken = formulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(field);
		_field = field;
		if (whereExpression != null)
		{
			AdjustFlagsAndWidth(whereExpression);
			_whereExpression = whereExpression;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalFieldCalculationFormulaSyntax(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalMemberAccessExpressionSyntax field, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken)
		: base(kind)
	{
		SlotCount = 6;
		if (sign != null)
		{
			AdjustFlagsAndWidth(sign);
			_sign = sign;
		}
		AdjustFlagsAndWidth(formulaKeywordToken);
		_formulaKeywordToken = formulaKeywordToken;
		AdjustFlagsAndWidth(openParenthesisToken);
		_openParenthesisToken = openParenthesisToken;
		AdjustFlagsAndWidth(field);
		_field = field;
		if (whereExpression != null)
		{
			AdjustFlagsAndWidth(whereExpression);
			_whereExpression = whereExpression;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => (InternalSyntaxNode)_sign, 
			1 => _formulaKeywordToken, 
			2 => _openParenthesisToken, 
			3 => _field, 
			4 => _whereExpression, 
			5 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new FieldCalculationFormulaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldCalculationFormula(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitFieldCalculationFormula(this);
	}

	public InternalFieldCalculationFormulaSyntax Update(InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalMemberAccessExpressionSyntax field, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken)
	{
		if (sign != Sign || formulaKeywordToken != FormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || field != Field || whereExpression != WhereExpression || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.FieldCalculationFormula(Kind, sign, formulaKeywordToken, openParenthesisToken, field, whereExpression, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalFieldCalculationFormulaSyntax(Kind, _sign, _formulaKeywordToken, _openParenthesisToken, _field, _whereExpression, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalFieldCalculationFormulaSyntax(Kind, _sign, _formulaKeywordToken, _openParenthesisToken, _field, _whereExpression, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
