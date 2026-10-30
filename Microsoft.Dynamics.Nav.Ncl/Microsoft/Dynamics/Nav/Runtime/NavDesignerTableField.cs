using System;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.Runtime.Designer;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDesignerTableField : NavDesignerTableFieldBase
{
	public int Number { get; }

	public string Name { get; }

	public NavType FieldType { get; }

	public int Length { get; set; }

	public string Description
	{
		get
		{
			return GetPropertyValue<StringPropertyValueSyntax>(PropertyKind.Description)?.Value.Value.Value as string;
		}
		set
		{
			SetProperty(PropertyKind.Description, value);
		}
	}

	public bool Editable
	{
		get
		{
			BooleanPropertyValueSyntax propertyValue = GetPropertyValue<BooleanPropertyValueSyntax>(PropertyKind.Editable);
			if (propertyValue == null)
			{
				return false;
			}
			return propertyValue.Value.Kind == SyntaxKind.TrueKeyword;
		}
		set
		{
			SetProperty(PropertyKind.Editable, value);
		}
	}

	public bool Enabled
	{
		get
		{
			BooleanPropertyValueSyntax propertyValue = GetPropertyValue<BooleanPropertyValueSyntax>(PropertyKind.Enabled);
			if (propertyValue == null)
			{
				return false;
			}
			return propertyValue.Value.Kind == SyntaxKind.TrueKeyword;
		}
		set
		{
			SetProperty(PropertyKind.Enabled, value);
		}
	}

	public override FieldBaseSyntax Syntax => FieldSyntax;

	internal FieldSyntax FieldSyntax { get; set; }

	public NavDesignerTableField(int fieldNumber, string fieldName, NavType fieldType, int length = 0)
	{
		Number = fieldNumber;
		Name = fieldName;
		FieldType = fieldType;
		Length = length;
	}

	public NavDesignerTableField(FieldSyntax fieldSyntax)
	{
		Number = (int)fieldSyntax.No.Value;
		Name = fieldSyntax.Name.Identifier.Text.UnquoteIdentifier();
		FieldType = (Enum.TryParse<NavTypeKind>(fieldSyntax.Type.TypeName.Value.ToString(), out var result) ? GetNavTypeFromTypeKind(result) : NavType.None);
		FieldSyntax = fieldSyntax;
	}

	public virtual bool SetProperty(PropertyKind propertyKind, object propertyValue)
	{
		switch (propertyKind)
		{
		case PropertyKind.Description:
			Description = (string)propertyValue;
			return true;
		case PropertyKind.Enabled:
			Enabled = bool.Parse(propertyValue.ToString());
			return true;
		case PropertyKind.Editable:
			Editable = bool.Parse(propertyValue.ToString());
			return true;
		case PropertyKind.Caption:
			SetCaption((string)propertyValue);
			return true;
		default:
			return false;
		}
	}

	public void SetCaption(string caption)
	{
		SetProperty(PropertyKind.Caption, SyntaxFactory.Property(PropertyKind.Caption, SyntaxFactory.LabelPropertyValue(SyntaxFactory.Label(SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(caption))))));
	}

	public string GetCaption()
	{
		return GetPropertyValue<LabelPropertyValueSyntax>(PropertyKind.Caption).Value.LabelText.ToString();
	}

	protected void SetProperty(PropertyKind propertyKind, string propertyValue)
	{
		SetProperty(propertyKind, SyntaxFactory.PropertyLiteral(propertyKind, propertyValue));
	}

	protected void SetProperty(PropertyKind propertyKind, bool propertyValue)
	{
		SetProperty(propertyKind, SyntaxFactory.PropertyLiteral(propertyKind, propertyValue));
	}

	protected void SetProperty(PropertyKind propertyKind, int propertyValue)
	{
		SetProperty(propertyKind, SyntaxFactory.PropertyLiteral(propertyKind, propertyValue));
	}

	protected void SetProperty(PropertyKind propertyKind, PropertySyntax property)
	{
		PropertySyntax property2 = GetProperty(propertyKind);
		PropertyListSyntax propertyList = ((property2 == null) ? Syntax.PropertyList.AddProperties(property) : Syntax.PropertyList.Update(Syntax.PropertyList.Properties.Replace(property2, property)));
		FieldSyntax = FieldSyntax.WithPropertyList(propertyList);
	}

	protected PropertySyntax GetProperty(PropertyKind propertyKind)
	{
		return Syntax.GetProperty(propertyKind.ToString());
	}

	protected TPropType GetPropertyValue<TPropType>(PropertyKind propertyKind) where TPropType : PropertyValueSyntax
	{
		return GetProperty(propertyKind)?.Value as TPropType;
	}

	protected void RemoveProperty(PropertyKind propertyKind)
	{
		PropertySyntax property = GetProperty(propertyKind);
		if (property != null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = Syntax.PropertyList.Properties.Remove(property);
			FieldSyntax = FieldSyntax.WithPropertyList(SyntaxFactory.PropertyList(properties));
		}
	}

	private static NavType GetNavTypeFromTypeKind(NavTypeKind typeKind)
	{
		return typeKind switch
		{
			NavTypeKind.Integer => NavType.Integer, 
			NavTypeKind.BigInteger => NavType.BigInteger, 
			NavTypeKind.Decimal => NavType.Decimal, 
			NavTypeKind.Text => NavType.Text, 
			NavTypeKind.Code => NavType.Code, 
			NavTypeKind.Option => NavType.Option, 
			NavTypeKind.Boolean => NavType.Boolean, 
			NavTypeKind.Date => NavType.Date, 
			NavTypeKind.Guid => NavType.GUID, 
			NavTypeKind.DateTime => NavType.DateTime, 
			NavTypeKind.Time => NavType.Time, 
			_ => NavType.None, 
		};
	}
}
