using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalTableCalculationFormulaSyntax : InternalCalculationFormulaPropertyValueSyntax
{
	internal readonly InternalSyntaxToken _sign;

	internal readonly InternalSyntaxToken _formulaKeywordToken;

	internal readonly InternalSyntaxToken _openParenthesisToken;

	internal readonly InternalIdentifierNameSyntax _table;

	internal readonly InternalWhereExpressionSyntax _whereExpression;

	internal readonly InternalSyntaxToken _closeParenthesisToken;

	internal override SyntaxNodeDefinition Definition => TableCalculationFormulaSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken Sign => _sign;

	public InternalSyntaxToken FormulaKeywordToken => _formulaKeywordToken;

	public InternalSyntaxToken OpenParenthesisToken => _openParenthesisToken;

	public InternalIdentifierNameSyntax Table => _table;

	public InternalWhereExpressionSyntax WhereExpression => _whereExpression;

	public InternalSyntaxToken CloseParenthesisToken => _closeParenthesisToken;

	internal InternalTableCalculationFormulaSyntax(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax table, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
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
		AdjustFlagsAndWidth(table);
		_table = table;
		if (whereExpression != null)
		{
			AdjustFlagsAndWidth(whereExpression);
			_whereExpression = whereExpression;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalTableCalculationFormulaSyntax(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax table, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken, LexerMode mode)
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
		AdjustFlagsAndWidth(table);
		_table = table;
		if (whereExpression != null)
		{
			AdjustFlagsAndWidth(whereExpression);
			_whereExpression = whereExpression;
		}
		AdjustFlagsAndWidth(closeParenthesisToken);
		_closeParenthesisToken = closeParenthesisToken;
	}

	internal InternalTableCalculationFormulaSyntax(SyntaxKind kind, InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax table, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken)
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
		AdjustFlagsAndWidth(table);
		_table = table;
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
			3 => _table, 
			4 => _whereExpression, 
			5 => _closeParenthesisToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new TableCalculationFormulaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableCalculationFormula(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitTableCalculationFormula(this);
	}

	public InternalTableCalculationFormulaSyntax Update(InternalSyntaxToken sign, InternalSyntaxToken formulaKeywordToken, InternalSyntaxToken openParenthesisToken, InternalIdentifierNameSyntax table, InternalWhereExpressionSyntax whereExpression, InternalSyntaxToken closeParenthesisToken)
	{
		if (sign != Sign || formulaKeywordToken != FormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || table != Table || whereExpression != WhereExpression || closeParenthesisToken != CloseParenthesisToken)
		{
			return InternalSyntaxFactory.TableCalculationFormula(Kind, sign, formulaKeywordToken, openParenthesisToken, table, whereExpression, closeParenthesisToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalTableCalculationFormulaSyntax(Kind, _sign, _formulaKeywordToken, _openParenthesisToken, _table, _whereExpression, _closeParenthesisToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalTableCalculationFormulaSyntax(Kind, _sign, _formulaKeywordToken, _openParenthesisToken, _table, _whereExpression, _closeParenthesisToken, GetDiagnostics(), annotations);
	}
}
