namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

public static class SyntaxKindExtensions
{
	internal static bool IsCompilationUnitSyntax(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.CompilationUnit)
		{
			return true;
		}
		return false;
	}

	internal static bool IsObjectSyntax(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.RequestPage:
		case SyntaxKind.EnumType:
		case SyntaxKind.EnumExtensionType:
		case SyntaxKind.CodeunitObject:
		case SyntaxKind.TableObject:
		case SyntaxKind.TableExtensionObject:
		case SyntaxKind.PageObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.ReportObject:
		case SyntaxKind.XmlPortObject:
		case SyntaxKind.QueryObject:
		case SyntaxKind.ControlAddInObject:
		case SyntaxKind.ProfileObject:
		case SyntaxKind.PageCustomizationObject:
		case SyntaxKind.DotNetPackage:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsApplicationObjectSyntax(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.EnumType:
		case SyntaxKind.EnumExtensionType:
		case SyntaxKind.CodeunitObject:
		case SyntaxKind.TableObject:
		case SyntaxKind.TableExtensionObject:
		case SyntaxKind.PageObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.ReportObject:
		case SyntaxKind.XmlPortObject:
		case SyntaxKind.QueryObject:
		case SyntaxKind.PageCustomizationObject:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsFieldBaseSyntax(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.Field || kind == SyntaxKind.FieldModification)
		{
			return true;
		}
		return false;
	}

	internal static bool IsKeyFieldGroupBaseSyntax(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.Key || kind == SyntaxKind.FieldGroup)
		{
			return true;
		}
		return false;
	}

	internal static bool IsFieldGroupChangeBaseSyntax(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.FieldGroupAddChange)
		{
			return true;
		}
		return false;
	}

	internal static bool IsControlBaseSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 272) <= 7u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsControlGroupBaseSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 272) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsActionBaseSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 280) <= 3u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsActionGroupBaseSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 281) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsApplicationObjectExtensionSyntax(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.EnumExtensionType:
		case SyntaxKind.TableExtensionObject:
		case SyntaxKind.PageExtensionObject:
		case SyntaxKind.PageCustomizationObject:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsAbstractPageExtensionSyntax(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.PageExtensionObject || kind == SyntaxKind.PageCustomizationObject)
		{
			return true;
		}
		return false;
	}

	internal static bool IsControlChangeBaseSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 289) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsViewChangeBaseSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 293) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsActionChangeBaseSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 285) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsQueryDataItemElementSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 310) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsQueryElementSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 311) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsReportDataItemElementSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 298) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsReportLabelBaseSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 300) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsXmlPortNodeSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 303) <= 4u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsXmlPortFieldNodeSyntax(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.XmlPortFieldElement || kind == SyntaxKind.XmlPortFieldAttribute)
		{
			return true;
		}
		return false;
	}

	internal static bool IsXmlPortTextNodeSyntax(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.XmlPortTextElement || kind == SyntaxKind.XmlPortTextAttribute)
		{
			return true;
		}
		return false;
	}

	internal static bool IsAttributeArgumentSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 424) <= 3u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsLiteralValueSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 169) <= 7u || kind == SyntaxKind.OptionValues)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPropertyValueSyntax(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.InvalidPropertyValue:
		case SyntaxKind.Int32PropertyValue:
		case SyntaxKind.Int64PropertyValue:
		case SyntaxKind.DecimalPropertyValue:
		case SyntaxKind.StringPropertyValue:
		case SyntaxKind.TimePropertyValue:
		case SyntaxKind.DatePropertyValue:
		case SyntaxKind.DateTimePropertyValue:
		case SyntaxKind.MultilanguagePropertyValue:
		case SyntaxKind.LabelPropertyValue:
		case SyntaxKind.EnumPropertyValue:
		case SyntaxKind.BooleanPropertyValue:
		case SyntaxKind.ImagePropertyValue:
		case SyntaxKind.OptionValuePropertyValue:
		case SyntaxKind.IdentifierOrLiteralPropertyValue:
		case SyntaxKind.MemberAccessOrIdentifierPropertyValue:
		case SyntaxKind.PageFieldReferencePropertyValue:
		case SyntaxKind.TableRelationStatement:
		case SyntaxKind.ExistCalculationFormulaStatement:
		case SyntaxKind.AverageCalculationFormulaStatement:
		case SyntaxKind.OptionValuesPropertyValue:
		case SyntaxKind.PermissionPropertyValue:
		case SyntaxKind.CommaSeparatedPropertyValue:
		case SyntaxKind.CommaSeparatedStringsPropertyValue:
		case SyntaxKind.CommaSeparatedIdentifierEqualsStringListPropertyValue:
		case SyntaxKind.CommaSeparatedIdentifierOrLiteralPropertyValue:
		case SyntaxKind.QualifiedObjectReferencePropertyValue:
		case SyntaxKind.ObjectReferencePropertyValue:
		case SyntaxKind.BooleanExpressionPropertyValue:
		case SyntaxKind.ClientSideBooleanExpressionPropertyValue:
		case SyntaxKind.IntegerExpressionPropertyValue:
		case SyntaxKind.TextExpressionPropertyValue:
		case SyntaxKind.DecimalPlacesPropertyValue:
		case SyntaxKind.TableFilterPropertyValue:
		case SyntaxKind.TableViewPropertyValue:
		case SyntaxKind.FiltersPropertyValue:
		case SyntaxKind.StyleExpressionPropertyValue:
		case SyntaxKind.MemberReferencePropertyValue:
		case SyntaxKind.OrderByPropertyValue:
		case SyntaxKind.QueryDataItemLinkPropertyValue:
		case SyntaxKind.ReportDataItemLinkPropertyValue:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsIdentifierNamePropertyValueSyntax(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.EnumPropertyValue:
		case SyntaxKind.ImagePropertyValue:
		case SyntaxKind.OptionValuePropertyValue:
		case SyntaxKind.PageFieldReferencePropertyValue:
		case SyntaxKind.MemberReferencePropertyValue:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsSeparatedListPropertyValueSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 369) <= 1u || (uint)(kind - 373) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsLiteralPropertyValueSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 325) <= 6u || kind == SyntaxKind.BooleanPropertyValue || kind == SyntaxKind.OptionValuesPropertyValue)
		{
			return true;
		}
		return false;
	}

	internal static bool IsExpressionPropertyValueSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 380) <= 3u || kind == SyntaxKind.StyleExpressionPropertyValue)
		{
			return true;
		}
		return false;
	}

	internal static bool IsPropertyExpressionSyntax(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.TableFilterExpression:
		case SyntaxKind.ConstExpression:
		case SyntaxKind.FieldFilterExpression:
		case SyntaxKind.SimpleFieldExpression:
		case SyntaxKind.FilterExpression:
		case SyntaxKind.FieldUpperLimitExpression:
		case SyntaxKind.FieldUpperLimitFilterExpression:
		case SyntaxKind.InvalidFilterExpressionValue:
		case SyntaxKind.InvalidPropertyExpression:
		case SyntaxKind.IfTableRelationExpression:
		case SyntaxKind.ElseTableRelationExpression:
		case SyntaxKind.WhereExpression:
		case SyntaxKind.SortingExpression:
		case SyntaxKind.OrderExpression:
		case SyntaxKind.ReportDataItemLinkExpression:
		case SyntaxKind.ParenthesizedFilterExpressionValue:
		case SyntaxKind.UnaryNotEqualsFilterExpression:
		case SyntaxKind.AndFilterExpression:
		case SyntaxKind.RangeFromFilterExpression:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsCalculationFormulaPropertyValueSyntax(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.ExistCalculationFormulaStatement || kind == SyntaxKind.AverageCalculationFormulaStatement)
		{
			return true;
		}
		return false;
	}

	internal static bool IsFilterExpressionValueSyntax(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.InvalidFilterExpressionValue:
		case SyntaxKind.ParenthesizedFilterExpressionValue:
		case SyntaxKind.UnaryNotEqualsFilterExpression:
		case SyntaxKind.AndFilterExpression:
		case SyntaxKind.RangeFromFilterExpression:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsPropertySyntaxOrEmpty(this SyntaxKind kind)
	{
		if ((uint)(kind - 321) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsMemberSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 235) <= 4u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsTypeReferenceBaseSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 243) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsDataTypeSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 246) <= 8u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsMethodOrTriggerDeclarationSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 236) <= 2u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsStatementSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 177) <= 15u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsExpressionSyntax(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.ParenthesizedExpression:
		case SyntaxKind.AddExpression:
		case SyntaxKind.UnaryPlusExpression:
		case SyntaxKind.ArrayIndexExpression:
		case SyntaxKind.InvocationExpression:
		case SyntaxKind.MemberAccessExpression:
		case SyntaxKind.OptionAccessExpression:
		case SyntaxKind.InListExpression:
		case SyntaxKind.LiteralExpression:
		case SyntaxKind.IdentifierName:
		case SyntaxKind.QualifiedName:
		case SyntaxKind.ObjectId:
		case SyntaxKind.TableFilterExpression:
		case SyntaxKind.ConstExpression:
		case SyntaxKind.FieldFilterExpression:
		case SyntaxKind.SimpleFieldExpression:
		case SyntaxKind.FilterExpression:
		case SyntaxKind.FieldUpperLimitExpression:
		case SyntaxKind.FieldUpperLimitFilterExpression:
		case SyntaxKind.InvalidFilterExpressionValue:
		case SyntaxKind.InvalidPropertyExpression:
		case SyntaxKind.IfTableRelationExpression:
		case SyntaxKind.ElseTableRelationExpression:
		case SyntaxKind.WhereExpression:
		case SyntaxKind.IdentifierEqualsString:
		case SyntaxKind.IdentifierEqualsLiteral:
		case SyntaxKind.SortingExpression:
		case SyntaxKind.OrderExpression:
		case SyntaxKind.OrderByExpression:
		case SyntaxKind.QueryDataItemLinkExpression:
		case SyntaxKind.ReportDataItemLinkExpression:
		case SyntaxKind.ParenthesizedFilterExpressionValue:
		case SyntaxKind.UnaryNotEqualsFilterExpression:
		case SyntaxKind.AndFilterExpression:
		case SyntaxKind.RangeFromFilterExpression:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsCodeExpressionSyntax(this SyntaxKind kind)
	{
		switch (kind)
		{
		case SyntaxKind.ParenthesizedExpression:
		case SyntaxKind.AddExpression:
		case SyntaxKind.UnaryPlusExpression:
		case SyntaxKind.ArrayIndexExpression:
		case SyntaxKind.InvocationExpression:
		case SyntaxKind.MemberAccessExpression:
		case SyntaxKind.OptionAccessExpression:
		case SyntaxKind.InListExpression:
		case SyntaxKind.LiteralExpression:
		case SyntaxKind.IdentifierName:
		case SyntaxKind.QualifiedName:
		case SyntaxKind.ObjectId:
		case SyntaxKind.IdentifierEqualsString:
		case SyntaxKind.IdentifierEqualsLiteral:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsNameSyntax(this SyntaxKind kind)
	{
		if ((uint)(kind - 220) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsSimpleNameSyntax(this SyntaxKind kind)
	{
		if (kind == SyntaxKind.IdentifierName)
		{
			return true;
		}
		return false;
	}
}
