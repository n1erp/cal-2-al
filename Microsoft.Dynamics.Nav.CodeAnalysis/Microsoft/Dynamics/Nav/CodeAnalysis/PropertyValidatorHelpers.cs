using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class PropertyValidatorHelpers
{
	internal static ErrorCode ValidateRatio(this NavTypeKind declaringFieldType)
	{
		if (declaringFieldType != NavTypeKind.Integer && declaringFieldType != NavTypeKind.BigInteger && declaringFieldType != NavTypeKind.Decimal)
		{
			return ErrorCode.ERR_InvalidExtendedDataTypeRatio;
		}
		return ErrorCode.None;
	}

	internal static ErrorCode ValidateForText(this NavTypeKind declaringFieldType)
	{
		if (declaringFieldType != NavTypeKind.Text && declaringFieldType != NavTypeKind.Code && declaringFieldType != NavTypeKind.TextConst && declaringFieldType != NavTypeKind.Label)
		{
			return ErrorCode.ERR_RequiresTextualExtendedDataType;
		}
		return ErrorCode.None;
	}

	internal static ErrorCode ValidateForMedia(this NavTypeKind declaringFieldType)
	{
		if (declaringFieldType != NavTypeKind.Media && declaringFieldType != NavTypeKind.MediaSet)
		{
			return ErrorCode.ERR_RequiresMediaExtendedDataType;
		}
		return ErrorCode.None;
	}

	internal static bool IsPositive(string propertyName, SyntaxNode node, string propertyValue, DiagnosticBag diagnostics)
	{
		if (int.TryParse(propertyValue, out var result) && result >= 0)
		{
			return true;
		}
		Binder.Error(diagnostics, ErrorCode.ERR_PropertyValueMustBePositive, node, propertyName);
		return false;
	}

	internal static bool IsWithinRange(int value, int low, int high, DiagnosticBag diagnostics, SyntaxToken integerToken)
	{
		if (value < low || value > high)
		{
			diagnostics.Add(ErrorCode.ERR_ValueOutsideValidRange, integerToken.GetLocation(), value, low, high);
			return false;
		}
		return true;
	}
}
