using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class BoundPropertyExpressionFactory<T> where T : PropertyExpressionSyntax
{
	private readonly T propertyExpression;

	private readonly bool isSourceTableBound;

	private readonly ExpressionContext expressionContext;

	internal BoundPropertyExpressionFactory(T propertyExpression, bool isSourceTableBound, ExpressionContext tableContext)
	{
		this.propertyExpression = propertyExpression;
		this.isSourceTableBound = isSourceTableBound;
		expressionContext = tableContext;
	}

	protected internal virtual BoundEqualsPropertyExpression CreateExpression(IdentifierNameSyntax leftHandside, SyntaxNode rightHandside, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = CreateLhs(leftHandside, diagnostics);
		if (boundExpression == null || boundExpression.Kind == BoundKind.BadPropertyValue)
		{
			return null;
		}
		TypeSymbol navType = Binder.GetNavType(NavTypeKind.Text, diagnostics, propertyExpression);
		BoundPropertyRhsExpression boundPropertyRhsExpression = CreateRhs(rightHandside, leftHandside.Identifier.ValueText.UnquoteIdentifier(), diagnostics);
		if (boundPropertyRhsExpression == null && propertyExpression.Kind == SyntaxKind.ConstExpression)
		{
			boundPropertyRhsExpression = new BoundPropertyRhsExpression(propertyExpression, propertyExpression.Kind, new BoundNullPropertyValue(propertyExpression, null), navType);
		}
		if (boundPropertyRhsExpression == null || boundPropertyRhsExpression.Kind == BoundKind.BadPropertyValue)
		{
			return null;
		}
		if (!ValidateTypeIntegrity(boundExpression, boundPropertyRhsExpression, diagnostics))
		{
			return null;
		}
		return new BoundEqualsPropertyExpression(propertyExpression, boundExpression, boundPropertyRhsExpression, navType);
	}

	private BoundPropertyRhsExpression CreateRhs(SyntaxNode rightHandSide, string leftHandSideName, DiagnosticBag diagnostics)
	{
		BoundPropertyValueExpression boundPropertyValueExpression = CreateBoundPropertyValueExpressionFromRhs(rightHandSide, diagnostics, leftHandSideName);
		if (boundPropertyValueExpression != null)
		{
			return new BoundPropertyRhsExpression(rightHandSide, propertyExpression.Kind, boundPropertyValueExpression, boundPropertyValueExpression.Type);
		}
		return null;
	}

	private BoundExpression CreateLhs(IdentifierNameSyntax leftHandSide, DiagnosticBag diagnostics)
	{
		if (leftHandSide != null)
		{
			_ = leftHandSide.Identifier;
			if (0 == 0 && !leftHandSide.Identifier.IsMissing)
			{
				if (isSourceTableBound)
				{
					return CreateLhsForCondition(leftHandSide, diagnostics);
				}
				return CreateLhsForWhereExpression(leftHandSide, diagnostics);
			}
		}
		Binder.Error(diagnostics, ErrorCode.ERR_InvalidPropertyExpression, propertyExpression, propertyExpression);
		return null;
	}

	private BoundExpression CreateLhsForWhereExpression(IdentifierNameSyntax leftHandSide, DiagnosticBag diagnostics)
	{
		return BindLhsExpression(leftHandSide, diagnostics, bindOnCurrent: false);
	}

	private BoundExpression CreateLhsForCondition(IdentifierNameSyntax leftHandSide, DiagnosticBag diagnostics)
	{
		string unquotedSymbolName = leftHandSide.Identifier.ValueText.UnquoteIdentifier();
		int symbolId = 0;
		if (!expressionContext.TryGetSymbolByNameOrId(bindOnSource: true, symbolId, unquotedSymbolName, out var _))
		{
			Binder.Error(diagnostics, ErrorCode.ERR_MissingTypeReference, propertyExpression, leftHandSide.Identifier.ToString(), expressionContext.SourceSymbol?.Name ?? string.Empty);
			return null;
		}
		return BindLhsExpression(leftHandSide, diagnostics, bindOnCurrent: true);
	}

	private BoundExpression BindLhsExpression(IdentifierNameSyntax node, DiagnosticBag diagnostics, bool bindOnCurrent)
	{
		string symbolName = node.Identifier.ValueText.UnquoteIdentifier();
		if (bindOnCurrent)
		{
			return expressionContext.BindExpression(node, diagnostics);
		}
		expressionContext.TryBindSymbolAccess(bindOnCurrent, node, symbolName, diagnostics, out var boundExpression);
		return boundExpression;
	}

	private BoundExpression BindExpression(IdentifierOrLiteralExpressionSyntax node, DiagnosticBag diagnostics, string lhsIdentifierName)
	{
		bool flag = node.Identifier.IsKind(SyntaxKind.LiteralExpression);
		bool flag2 = false;
		LiteralExpressionSyntax literalExpressionSyntax = null;
		if (flag)
		{
			literalExpressionSyntax = (LiteralExpressionSyntax)node.Identifier;
			flag2 = literalExpressionSyntax.Literal.IsKind(SyntaxKind.Int32SignedLiteralValue);
		}
		bool flag3 = false;
		bool flag4 = false;
		if (lhsIdentifierName != null)
		{
			expressionContext.TryGetSymbolByNameOrId(isSourceTableBound, 0, lhsIdentifierName.UnquoteIdentifier(), out var symbol);
			if ((symbol != null) & flag2)
			{
				flag3 = symbol.Type.NavTypeKind == NavTypeKind.Integer;
				flag4 = symbol.Type.NavTypeKind == NavTypeKind.Option;
			}
		}
		if (!flag4 && (flag3 | flag))
		{
			if (literalExpressionSyntax == null)
			{
				return Binder.CreateBoundStringLiteral(node.GetIdentifierOrLiteralValue().UnquoteIdentifier(), node, diagnostics);
			}
			return Binder.CreateBoundLiteral(literalExpressionSyntax, diagnostics);
		}
		expressionContext.TryBindOptionSymbolAccess(isSourceTableBound, lhsIdentifierName, node.Identifier, diagnostics, out var boundExpression);
		return boundExpression;
	}

	[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
	private BoundPropertyValueExpression CreateBoundPropertyValueExpressionFromRhs(SyntaxNode rhs, DiagnosticBag diagnostics, string lhsIdentifierName)
	{
		if (propertyExpression.Kind == SyntaxKind.ConstExpression)
		{
			IdentifierOrLiteralExpressionSyntax node = (IdentifierOrLiteralExpressionSyntax)rhs;
			if (rhs == null)
			{
				return null;
			}
			BoundExpression boundExpression = BindExpression(node, diagnostics, lhsIdentifierName);
			return new BoundConstOrFieldAccessValueExpression(rhs, boundExpression, boundExpression.Type);
		}
		if (propertyExpression.Kind != SyntaxKind.FilterExpression)
		{
			DebugAssertHelper.Assert(rhs.Kind == SyntaxKind.IdentifierName);
			IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)rhs;
			if (identifierNameSyntax.IsMissing)
			{
				DebugAssertHelper.Assert(diagnostics.HasAnyErrors());
				return null;
			}
			BoundExpression boundExpression2 = (isSourceTableBound ? CreateLhsForWhereExpression(identifierNameSyntax, diagnostics) : CreateLhsForCondition(identifierNameSyntax, diagnostics));
			if (boundExpression2 == null)
			{
				DebugAssertHelper.Assert(diagnostics.HasAnyErrors());
				return null;
			}
			return new BoundConstOrFieldAccessValueExpression(rhs, boundExpression2, boundExpression2.Type);
		}
		return CreateBoundFilterExpression(rhs, diagnostics, lhsIdentifierName);
	}

	private BoundPropertyValueExpression CreateBoundFilterExpression(SyntaxNode rhs, DiagnosticBag diagnostics, string lhsIdentifierName)
	{
		TypeSymbol type = Binder.CreateTypeSymbol(rhs, diagnostics);
		if (rhs.Kind == SyntaxKind.ParenthesizedFilterExpressionValue)
		{
			ParenthesizedFilterExpressionValueSyntax parenthesizedFilterExpressionValueSyntax = (ParenthesizedFilterExpressionValueSyntax)rhs;
			BoundPropertyValueExpression expression = CreateBoundFilterExpression(parenthesizedFilterExpressionValueSyntax.Expression, diagnostics, lhsIdentifierName);
			return new BoundParenthesizedFilterExpression(rhs, expression, type);
		}
		UnaryOperatorKind unaryOperatorKind = OperatorKindExtensions.MapUnaryFilterExpression(rhs);
		if (unaryOperatorKind == UnaryOperatorKind.FilterRangeFrom)
		{
			RangeFromFilterExpressionValueSyntax rangeFromFilterExpressionValueSyntax = (RangeFromFilterExpressionValueSyntax)rhs;
			BoundExpression operand = BindExpression(rangeFromFilterExpressionValueSyntax.Left, diagnostics, lhsIdentifierName);
			return new BoundUnaryPropertyFilterExpression(rhs, unaryOperatorKind, operand, type);
		}
		if (SyntaxFacts.IsUnaryFilterExpressionValueToken(rhs.Kind) && unaryOperatorKind != UnaryOperatorKind.Error)
		{
			UnaryFilterExpressionValueSyntax unaryFilterExpressionValueSyntax = (UnaryFilterExpressionValueSyntax)rhs;
			BoundExpression operand2 = BindExpression(unaryFilterExpressionValueSyntax.Identifier, diagnostics, lhsIdentifierName);
			return new BoundUnaryPropertyFilterExpression(rhs, unaryOperatorKind, operand2, type);
		}
		if (SyntaxFacts.IsBinaryFilterExpressionValueToken(rhs.Kind))
		{
			BinaryOperatorKind operatorKind = OperatorKindExtensions.MapBinaryFilterExpression(rhs);
			BinaryFilterExpressionValueSyntax binaryFilterExpressionValueSyntax = (BinaryFilterExpressionValueSyntax)rhs;
			BoundExpression left = CreateBoundFilterExpression(binaryFilterExpressionValueSyntax.Left, diagnostics, lhsIdentifierName);
			BoundExpression right = CreateBoundFilterExpression(binaryFilterExpressionValueSyntax.Right, diagnostics, lhsIdentifierName);
			return new BoundBinaryPropertyFilterExpression(rhs, operatorKind, left, right, type);
		}
		if (rhs.Kind == SyntaxKind.IdentifierOrLiteralExpression)
		{
			IdentifierOrLiteralExpressionSyntax node = (IdentifierOrLiteralExpressionSyntax)rhs;
			BoundExpression boundExpression = BindExpression(node, diagnostics, lhsIdentifierName);
			return new BoundUnaryPropertyFilterExpression(rhs, UnaryOperatorKind.FilterEquals, boundExpression, boundExpression.Type);
		}
		return null;
	}

	private bool ValidateTypeIntegrity(BoundExpression boundLhs, BoundPropertyRhsExpression boundRhs, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(boundLhs != null && boundRhs != null);
		if ((propertyExpression.Kind != SyntaxKind.SimpleFieldExpression && propertyExpression.Kind != SyntaxKind.ConstExpression) || (boundLhs.Kind != BoundKind.FieldAccess && boundLhs.Kind != BoundKind.QueryElementAccess))
		{
			return true;
		}
		DebugAssertHelper.Assert(boundRhs.Value != null && (boundRhs.Value.Kind == BoundKind.ConstOrFieldAccessValueExpression || boundRhs.Value.Kind == BoundKind.NullPropertyValue));
		if (boundRhs.Value.Kind == BoundKind.NullPropertyValue && propertyExpression.Kind == SyntaxKind.ConstExpression)
		{
			return ValidateConstNullTypeIntegrity(boundLhs, diagnostics);
		}
		TypeSymbol type = ((BoundConstOrFieldAccessValueExpression)boundRhs.Value).Value.Type;
		if (boundLhs.Type.NavTypeKind == type.NavTypeKind)
		{
			return true;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversion(type, boundLhs.Type, ref useSiteDiagnostics);
		if (conversion != Conversion.NoConversion && conversion.Kind != ConversionKind.ImplicitNumericToSmaller)
		{
			return true;
		}
		Binder.Error(diagnostics, ErrorCode.ERR_FieldTypeMismatch, boundLhs.Syntax, boundLhs.Type.Name, type.Name);
		return false;
	}

	private bool ValidateConstNullTypeIntegrity(BoundExpression boundLhs, DiagnosticBag diagnostics)
	{
		if (boundLhs.Kind != BoundKind.FieldAccess && boundLhs.Kind != BoundKind.QueryElementAccess)
		{
			return true;
		}
		Symbol expressionSymbol = boundLhs.ExpressionSymbol;
		DebugAssertHelper.Assert(boundLhs.Type != null);
		if (boundLhs.Type.NavTypeKind == NavTypeKind.Option)
		{
			if (expressionSymbol.GetTypeSymbol().GetMembers(" ").FirstOrDefault() == null)
			{
				string text = (isSourceTableBound ? expressionContext.SourceSymbol.Name : expressionContext.RelatedSymbol.Name);
				Binder.Error(diagnostics, ErrorCode.ERR_MissingTypeReference, expressionSymbol.GetLocation(), " ", text);
				return false;
			}
			Binder.Error(diagnostics, ErrorCode.WRN_EmptyConstIsObsolete, boundLhs.Syntax, expressionSymbol.Name);
			return true;
		}
		switch (boundLhs.Type.NavTypeKind)
		{
		case NavTypeKind.TableFilter:
		case NavTypeKind.DateFormula:
		case NavTypeKind.RecordId:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return true;
		default:
			Binder.Error(diagnostics, ErrorCode.ERR_EmptyConstIsNotSupported, boundLhs.Syntax, expressionSymbol.Name, boundLhs.Type.Name);
			return true;
		}
	}
}
