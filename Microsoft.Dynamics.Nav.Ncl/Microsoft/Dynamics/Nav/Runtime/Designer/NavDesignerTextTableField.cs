using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerTextTableField : NavDesignerTableField
{
	public bool MultiLine
	{
		get
		{
			BooleanPropertyValueSyntax propertyValue = GetPropertyValue<BooleanPropertyValueSyntax>(PropertyKind.MultiLine);
			if (propertyValue == null)
			{
				return false;
			}
			return propertyValue.Value.Kind == SyntaxKind.TrueKeyword;
		}
		set
		{
			SetProperty(PropertyKind.MultiLine, value);
		}
	}

	public bool ShowCaption
	{
		get
		{
			BooleanPropertyValueSyntax propertyValue = GetPropertyValue<BooleanPropertyValueSyntax>(PropertyKind.ShowCaption);
			if (propertyValue == null)
			{
				return false;
			}
			return propertyValue.Value.Kind == SyntaxKind.TrueKeyword;
		}
		set
		{
			SetProperty(PropertyKind.ShowCaption, value);
		}
	}

	public NavDesignerTextTableField(int fieldNumber, string fieldName, NavType fieldType, int length)
		: base(fieldNumber, fieldName, fieldType, length)
	{
		LengthDataTypeSyntax type = SyntaxFactory.LengthDataType(SyntaxFactory.Identifier(fieldType.ToString()), SyntaxFactory.Token(SyntaxKind.OpenBracketToken), SyntaxFactory.Literal(length), SyntaxFactory.Token(SyntaxKind.CloseBracketToken));
		FieldSyntax = SyntaxFactory.Field(SyntaxFactory.Literal(fieldNumber), SyntaxFactory.IdentifierName(fieldName), type);
	}

	public NavDesignerTextTableField(FieldSyntax fieldSyntax)
		: base(fieldSyntax)
	{
		Length = (int)((LengthDataTypeSyntax)fieldSyntax.Type).Length.Value;
	}

	public override bool SetProperty(PropertyKind propertyKind, object propertyValue)
	{
		switch (propertyKind)
		{
		case PropertyKind.MultiLine:
			MultiLine = bool.Parse(propertyValue.ToString());
			return true;
		case PropertyKind.ShowCaption:
			ShowCaption = bool.Parse(propertyValue.ToString());
			return true;
		default:
			return base.SetProperty(propertyKind, propertyValue);
		}
	}
}
