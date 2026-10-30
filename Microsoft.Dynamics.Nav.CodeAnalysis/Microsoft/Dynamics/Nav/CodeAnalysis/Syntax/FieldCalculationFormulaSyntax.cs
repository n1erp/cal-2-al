using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class FieldCalculationFormulaSyntax : CalculationFormulaPropertyValueSyntax
{
	private MemberAccessExpressionSyntax _field;

	private WhereExpressionSyntax _whereExpression;

	public SyntaxToken Sign
	{
		get
		{
			InternalSyntaxToken internalSyntaxToken = ((InternalFieldCalculationFormulaSyntax)Node)._sign;
			if (internalSyntaxToken != null)
			{
				return new SyntaxToken(this, internalSyntaxToken, Position, 0);
			}
			return default;
		}
	}

	public SyntaxToken FormulaKeywordToken => new SyntaxToken(this, ((InternalFieldCalculationFormulaSyntax)Node)._formulaKeywordToken, GetChildPosition(1), GetChildIndex(1));

	public SyntaxToken OpenParenthesisToken => new SyntaxToken(this, ((InternalFieldCalculationFormulaSyntax)Node)._openParenthesisToken, GetChildPosition(2), GetChildIndex(2));

	public MemberAccessExpressionSyntax Field => GetRed(ref _field, 3);

	public WhereExpressionSyntax WhereExpression => GetRed(ref _whereExpression, 4);

	public SyntaxToken CloseParenthesisToken => new SyntaxToken(this, ((InternalFieldCalculationFormulaSyntax)Node)._closeParenthesisToken, GetChildPosition(5), GetChildIndex(5));

	internal FieldCalculationFormulaSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			3 => (SyntaxNode)GetRed(ref _field, 3), 
			4 => GetRed(ref _whereExpression, 4), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			3 => (SyntaxNode)_field, 
			4 => _whereExpression, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitFieldCalculationFormula(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitFieldCalculationFormula(this);
	}

	public FieldCalculationFormulaSyntax Update(SyntaxToken sign, SyntaxToken formulaKeywordToken, SyntaxToken openParenthesisToken, MemberAccessExpressionSyntax field, WhereExpressionSyntax whereExpression, SyntaxToken closeParenthesisToken)
	{
		if (sign != Sign || formulaKeywordToken != FormulaKeywordToken || openParenthesisToken != OpenParenthesisToken || field != Field || whereExpression != WhereExpression || closeParenthesisToken != CloseParenthesisToken)
		{
			FieldCalculationFormulaSyntax fieldCalculationFormulaSyntax = SyntaxFactory.FieldCalculationFormula(Kind, sign, formulaKeywordToken, openParenthesisToken, field, whereExpression, closeParenthesisToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return fieldCalculationFormulaSyntax.WithAnnotations(annotations);
			}
			return fieldCalculationFormulaSyntax;
		}
		return this;
	}

	public FieldCalculationFormulaSyntax WithSign(SyntaxToken sign)
	{
		return Update(sign, FormulaKeywordToken, OpenParenthesisToken, Field, WhereExpression, CloseParenthesisToken);
	}

	public FieldCalculationFormulaSyntax WithFormulaKeywordToken(SyntaxToken formulaKeywordToken)
	{
		return Update(Sign, formulaKeywordToken, OpenParenthesisToken, Field, WhereExpression, CloseParenthesisToken);
	}

	public FieldCalculationFormulaSyntax WithOpenParenthesisToken(SyntaxToken openParenthesisToken)
	{
		return Update(Sign, FormulaKeywordToken, openParenthesisToken, Field, WhereExpression, CloseParenthesisToken);
	}

	public FieldCalculationFormulaSyntax WithField(MemberAccessExpressionSyntax field)
	{
		return Update(Sign, FormulaKeywordToken, OpenParenthesisToken, field, WhereExpression, CloseParenthesisToken);
	}

	public FieldCalculationFormulaSyntax WithWhereExpression(WhereExpressionSyntax whereExpression)
	{
		return Update(Sign, FormulaKeywordToken, OpenParenthesisToken, Field, whereExpression, CloseParenthesisToken);
	}

	public FieldCalculationFormulaSyntax WithCloseParenthesisToken(SyntaxToken closeParenthesisToken)
	{
		return Update(Sign, FormulaKeywordToken, OpenParenthesisToken, Field, WhereExpression, closeParenthesisToken);
	}
}
