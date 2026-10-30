using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerOptionTableField : NavDesignerTableField
{
	public string OptionString
	{
		get
		{
			return GetPropertyValue<OptionValuesPropertyValueSyntax>(PropertyKind.OptionMembers)?.Value.Options.ToString();
		}
		set
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				OptionValuesPropertyValueSyntax propertyValue = SyntaxFactory.OptionValuesPropertyValue(SyntaxFactory.OptionValues(default(SeparatedSyntaxList<IdentifierNameOrEmptySyntax>).AddRange(from o in value.Split(',')
					select SyntaxFactory.IdentifierNameOrEmpty(SyntaxFactory.IdentifierName(o)))));
				PropertySyntax property = SyntaxFactory.Property(PropertyKind.OptionMembers, propertyValue);
				SetProperty(PropertyKind.OptionMembers, property);
			}
			else
			{
				RemoveProperty(PropertyKind.OptionMembers);
			}
		}
	}

	public string InitValue
	{
		get
		{
			return GetPropertyValue<OptionValuePropertyValueSyntax>(PropertyKind.InitValue)?.Value.ToString();
		}
		set
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				OptionValuePropertyValueSyntax propertyValue = SyntaxFactory.OptionValuePropertyValue(SyntaxFactory.IdentifierName(value));
				PropertySyntax property = SyntaxFactory.Property(PropertyKind.InitValue, propertyValue);
				SetProperty(PropertyKind.InitValue, property);
			}
			else
			{
				RemoveProperty(PropertyKind.InitValue);
			}
		}
	}

	public NavDesignerOptionTableField(int fieldNumber, string fieldName)
		: base(fieldNumber, fieldName, NavType.Option)
	{
		OptionDataTypeSyntax type = SyntaxFactory.OptionDataType(SyntaxFactory.Identifier(NavType.Option.ToString()));
		FieldSyntax = SyntaxFactory.Field(SyntaxFactory.Literal(fieldNumber), SyntaxFactory.IdentifierName(fieldName), type);
	}

	public NavDesignerOptionTableField(FieldSyntax fieldSyntax)
		: base(fieldSyntax)
	{
	}

	public override bool SetProperty(PropertyKind propertyKind, object propertyValue)
	{
		switch (propertyKind)
		{
		case PropertyKind.OptionMembers:
			OptionString = (string)propertyValue;
			return true;
		case PropertyKind.InitValue:
			InitValue = (string)propertyValue;
			return true;
		default:
			return base.SetProperty(propertyKind, propertyValue);
		}
	}
}
