using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerSimpleTableField : NavDesignerTableField
{
	public bool BlankZero
	{
		get
		{
			BooleanPropertyValueSyntax propertyValue = GetPropertyValue<BooleanPropertyValueSyntax>(PropertyKind.BlankZero);
			if (propertyValue == null)
			{
				return false;
			}
			return propertyValue.Value.Kind == SyntaxKind.TrueKeyword;
		}
		set
		{
			SetProperty(PropertyKind.BlankZero, value);
		}
	}

	public string DecimalPlaces
	{
		get
		{
			return GetPropertyValue<DecimalPlacesPropertyValueSyntax>(PropertyKind.DecimalPlaces)?.Value.ToString();
		}
		set
		{
			string[] array = value.Split(':');
			switch (array.Length)
			{
			case 1:
			{
				int value4 = int.Parse(array[0], CultureInfo.InvariantCulture);
				PropertySyntax property = SyntaxFactory.Property(PropertyKind.DecimalPlaces, SyntaxFactory.DecimalPlacesPropertyValue(SyntaxFactory.DecimalPlaces(SyntaxFactory.Literal(value4))));
				SetProperty(PropertyKind.DecimalPlaces, property);
				break;
			}
			case 2:
			{
				int value2 = int.Parse(array[0], CultureInfo.InvariantCulture);
				int value3 = int.Parse(array[1], CultureInfo.InvariantCulture);
				PropertySyntax property = SyntaxFactory.Property(PropertyKind.DecimalPlaces, SyntaxFactory.DecimalPlacesPropertyValue(SyntaxFactory.DecimalPlaces(SyntaxFactory.Literal(value2), SyntaxFactory.Token(SyntaxKind.ColonToken), SyntaxFactory.Literal(value3))));
				SetProperty(PropertyKind.DecimalPlaces, property);
				break;
			}
			}
		}
	}

	public NavDesignerSimpleTableField(int fieldNumber, string fieldName, NavType fieldType)
		: base(fieldNumber, fieldName, fieldType)
	{
		SimpleNamedDataTypeSyntax type = SyntaxFactory.SimpleNamedDataType(SyntaxFactory.Identifier(fieldType.ToString()));
		FieldSyntax = SyntaxFactory.Field(SyntaxFactory.Literal(fieldNumber), SyntaxFactory.IdentifierName(fieldName), type);
	}

	public NavDesignerSimpleTableField(FieldSyntax fieldSyntax)
		: base(fieldSyntax)
	{
	}

	public override bool SetProperty(PropertyKind propertyKind, object propertyValue)
	{
		switch (propertyKind)
		{
		case PropertyKind.BlankZero:
			BlankZero = bool.Parse(propertyValue.ToString());
			return true;
		case PropertyKind.DecimalPlaces:
			DecimalPlaces = (string)propertyValue;
			return true;
		default:
			return base.SetProperty(propertyKind, propertyValue);
		}
	}
}
