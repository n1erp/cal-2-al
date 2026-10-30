using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class TableCalculationFormulaSyntax : CalculationFormulaPropertyValueSyntax
{
	private IdentifierNameSyntax _table;

	private WhereExpressionSyntax _whereExpression;

	public SyntaxToken Sign
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalTableCalculationFormulaSyntax)Node)._sign;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, Position, 0);
			}
			return default;
		}
	}

	public SyntaxToken FormulaKeywordToken => new SyntaxToken(this, ((InternalTableCalculationFormulaSyntax)Node)._formulaKeywordToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalTableCalculationFormulaSyntax)Node)._openParenthesisToken, GetChildPosition(2), GetChildIndex(2));

	public IdentifierNameSyntax Table => GetRed(ref _table, 3);

	public WhereExpressionSyntax WhereExpression => GetRed(ref _whereExpression, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalTableCalculationFormulaSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal TableCalculationFormulaSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			3 => (SyntaxNode)GetRed(ref _table, 3), 
			4 => GetRed(ref _whereExpression, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			3 => (SyntaxNode)_table, 
			4 => _whereExpression, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitTableCalculationFormula(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitTableCalculationFormula(this);
	}

	public TableCalculationFormulaSyntax Update(SyntaxToken sign, SyntaxToken formulaKeywordToken, SyntaxToken openParenthesisToken, IdentifierNameSyntax table, WhereExpressionSyntax whereExpression, SyntaxToken closeParenthesisToken)
	{
		if (sign != Sign || formulaKeywordToken != FormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || table != Table || whereExpression != WhereExpression || closeParenthesisToken != CloseParenthesisToken)
		{
			TableCalculationFormulaSyntax tableCalculationFormulaSyntax = SyntaxFactory.TableCalculationFormula(Kind, sign, formulaKeywordToken, openParenthesisToken, table, whereExpression, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return tableCalculationFormulaSyntax.WithAnnotations(annotations);
			}
			return tableCalculationFormulaSyntax;
		}
		return this;
	}

	public TableCalculationFormulaSyntax WithSign(SyntaxToken sign)
	{
		return Update(sign, FormulaKeywordToken, OpenParenthesisToken, Table, WhereExpression, CloseParenthesisToken);
	}

	public TableCalculationFormulaSyntax WithFormulaKeywordToken(SyntaxToken formulaKeywordToken)
	{
		return Update(Sign, formulaKeywordToken, OpenParenthesisToken, Table, WhereExpression, CloseParenthesisToken);
	}

	public TableCalculationFormulaSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(Sign, FormulaKeywordToken, openParenthesisToken, Table, WhereExpression, CloseParenthesisToken);
	}

	public TableCalculationFormulaSyntax WithTable(IdentifierNameSyntax table)
	{
		return Update(Sign, FormulaKeywordToken, OpenParenthesisToken, table, WhereExpression, CloseParenthesisToken);
	}

	public TableCalculationFormulaSyntax WithWhereExpression(WhereExpressionSyntax whereExpression)
	{
		return Update(Sign, FormulaKeywordToken, OpenParenthesisToken, Table, whereExpression, CloseParenthesisToken);
	}

	public TableCalculationFormulaSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(Sign, FormulaKeywordToken, OpenParenthesisToken, Table, WhereExpression, closeParenthesisToken);
	}
}
