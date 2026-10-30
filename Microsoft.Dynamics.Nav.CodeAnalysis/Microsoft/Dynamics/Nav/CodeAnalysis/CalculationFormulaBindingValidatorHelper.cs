using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class CalculationFormulaBindingValidatorHelper
{
	internal static bool ValidateCalculationFormulaDeclaringFieldType(SyntaxKind operatorKind, FieldSyntax fieldSyntax, DiagnosticBag diagnostics)
	{
		if (fieldSyntax?.Type == null)
		{
			return false;
		}
		DataTypeSyntax type = fieldSyntax.Type;
		string valueText = fieldSyntax.Name.Identifier.ValueText;
		NavTypeKind navTypeKind = NavTypeExtensions.GetNavTypeKind(type.TypeName.ValueText, throwIfUnknown: false);
		Location location = type.Location;
		return ValidateCalculationFormulaSourceFieldType(operatorKind, navTypeKind, valueText, location, diagnostics);
	}

	internal static bool ValidateCalculationFormulaSourceFieldType(SyntaxKind operatorKind, BoundFieldAccess fieldAccess, DiagnosticBag diagnostics)
	{
		if (fieldAccess?.FieldSymbol == null)
		{
			return false;
		}
		NavTypeKind navTypeKind = fieldAccess.FieldSymbol.Type.NavTypeKind;
		string name = fieldAccess.FieldSymbol.Name;
		Location location = fieldAccess.Syntax.Location;
		return ValidateCalculationFormulaSourceFieldType(operatorKind, navTypeKind, name, location, diagnostics);
	}

	private static bool ValidateCalculationFormulaSourceFieldType(SyntaxKind operatorKind, NavTypeKind sourceFieldType, string fieldName, Location location, DiagnosticBag diagnostics)
	{
		switch (operatorKind)
		{
		case SyntaxKind.ExistCalculationFormulaStatement:
			if (sourceFieldType != NavTypeKind.Boolean)
			{
				Binder.Error(diagnostics, ErrorCode.ERR_FlowFieldMustBeBoolean, location, fieldName);
				return false;
			}
			break;
		case SyntaxKind.SumCalculationFormulaStatement:
		case SyntaxKind.AverageCalculationFormulaStatement:
			if (sourceFieldType != NavTypeKind.Decimal && sourceFieldType != NavTypeKind.Duration && sourceFieldType != NavTypeKind.Integer && sourceFieldType != NavTypeKind.BigInteger)
			{
				Binder.Error(diagnostics, ErrorCode.ERR_SumAverageOnlySupportedForNumericFields, location, fieldName);
				return false;
			}
			break;
		case SyntaxKind.CountCalculationFormulaStatement:
			if (sourceFieldType != NavTypeKind.Integer && sourceFieldType != NavTypeKind.BigInteger)
			{
				Binder.Error(diagnostics, ErrorCode.ERR_FlowFieldMustBeInteger, location, fieldName);
				return false;
			}
			break;
		default:
			return true;
		}
		return true;
	}

	internal static bool ValidateCalculationFormulaSourceFieldTypeForMinMaxLookupFormulas(SyntaxKind operatorKind, FieldSyntax declaringFieldSyntax, BoundFieldAccess boundRhs, DiagnosticBag diagnostics)
	{
		if (operatorKind != SyntaxKind.MinCalculationFormulaStatement && operatorKind != SyntaxKind.MaxCalculationFormulaStatement && operatorKind != SyntaxKind.LookupCalculationFormulaStatement)
		{
			return true;
		}
		if (declaringFieldSyntax?.Type == null || boundRhs?.Type == null)
		{
			return false;
		}
		DataTypeSyntax type = declaringFieldSyntax.Type;
		string valueText = declaringFieldSyntax.Name.Identifier.ValueText;
		NavTypeKind navTypeKind = NavTypeExtensions.GetNavTypeKind(type.TypeName.ValueText, throwIfUnknown: false);
		Location location = type.Location;
		SqlDataTypeKind? sqldataType = null;
		if (declaringFieldSyntax.GetPropertyValue("SqlDataType") is EnumPropertyValueSyntax node)
		{
			string identifierOrLiteralValue = node.GetIdentifierOrLiteralValue();
			if (!string.IsNullOrEmpty(identifierOrLiteralValue) && Enum.TryParse<SqlDataTypeKind>(identifierOrLiteralValue, ignoreCase: true, out var result))
			{
				sqldataType = result;
			}
		}
		SqlDataTypeKind? simplePropertyValue = boundRhs.FieldSymbol.GetSimplePropertyValue<SqlDataTypeKind>(PropertyKind.SqlDataType);
		bool flag = IsRuntimeStringType(navTypeKind, sqldataType);
		bool flag2 = IsRuntimeStringType(boundRhs.Type.NavTypeKind, simplePropertyValue);
		if (navTypeKind == boundRhs.Type.NavTypeKind)
		{
			if ((!flag | flag2) && (flag || !flag2))
			{
				return true;
			}
		}
		else if (flag & flag2)
		{
			return true;
		}
		Binder.Error(diagnostics, ErrorCode.ERR_CalcFormulaConversionError, location, valueText, boundRhs.Type.ToString());
		return false;
	}

	private static bool IsRuntimeStringType(NavTypeKind type, SqlDataTypeKind? sqldataType)
	{
		switch (type)
		{
		case NavTypeKind.BigText:
		case NavTypeKind.DateFormula:
		case NavTypeKind.Text:
			return true;
		case NavTypeKind.Code:
			if (sqldataType.HasValue && sqldataType != SqlDataTypeKind.Varchar)
			{
				return sqldataType == SqlDataTypeKind.Variant;
			}
			return true;
		default:
			return false;
		}
	}
}
