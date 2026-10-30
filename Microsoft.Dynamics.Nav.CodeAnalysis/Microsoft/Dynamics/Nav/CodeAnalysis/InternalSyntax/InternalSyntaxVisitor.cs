using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal abstract class InternalSyntaxVisitor<TResult>
{
	public virtual TResult VisitObjectCompilationUnit(InternalObjectCompilationUnitSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPropertyList(InternalPropertyListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitProfile(InternalProfileSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumType(InternalEnumTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumValue(InternalEnumValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumExtensionType(InternalEnumExtensionTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetPackage(InternalDotNetPackageSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetAssembly(InternalDotNetAssemblySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetTypeDeclaration(InternalDotNetTypeDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCodeunit(InternalCodeunitSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTable(InternalTableSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldList(InternalFieldListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldExtensionList(InternalFieldExtensionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitField(InternalFieldSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldModification(InternalFieldModificationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitKeyList(InternalKeyListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitKey(InternalKeySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldGroupList(InternalFieldGroupListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldGroupExtensionList(InternalFieldGroupExtensionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldGroupAddChange(InternalFieldGroupAddChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldGroup(InternalFieldGroupSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPage(InternalPageSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageLayout(InternalPageLayoutSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageField(InternalPageFieldSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageLabel(InternalPageLabelSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageGroup(InternalPageGroupSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPagePart(InternalPagePartSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageSystemPart(InternalPageSystemPartSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageChartPart(InternalPageChartPartSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageUserControl(InternalPageUserControlSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageArea(InternalPageAreaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageActionList(InternalPageActionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageViewList(InternalPageViewListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageView(InternalPageViewSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitGroupActionList(InternalGroupActionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageActionGroup(InternalPageActionGroupSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageActionArea(InternalPageActionAreaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageAction(InternalPageActionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageActionSeparator(InternalPageActionSeparatorSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageExtension(InternalPageExtensionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageCustomization(InternalPageCustomizationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableExtension(InternalTableExtensionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageExtensionLayout(InternalPageExtensionLayoutSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitControlAddChange(InternalControlAddChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitControlMoveChange(InternalControlMoveChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitControlModifyChange(InternalControlModifyChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageExtensionViewList(InternalPageExtensionViewListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitViewAddChange(InternalViewAddChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitViewMoveChange(InternalViewMoveChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitViewModifyChange(InternalViewModifyChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageExtensionActionList(InternalPageExtensionActionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitActionAddChange(InternalActionAddChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitActionMoveChange(InternalActionMoveChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitActionModifyChange(InternalActionModifyChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQuery(InternalQuerySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryElements(InternalQueryElementsSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryDataItem(InternalQueryDataItemSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryColumn(InternalQueryColumnSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryFilter(InternalQueryFilterSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReport(InternalReportSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportDataSetSection(InternalReportDataSetSectionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportDataItem(InternalReportDataItemSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportColumn(InternalReportColumnSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportLabelsSection(InternalReportLabelsSectionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportLabelMultilanguage(InternalReportLabelMultilanguageSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportLabel(InternalReportLabelSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRequestPage(InternalRequestPageSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPort(InternalXmlPortSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortSchema(InternalXmlPortSchemaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortTableElement(InternalXmlPortTableElementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortFieldElement(InternalXmlPortFieldElementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortFieldAttribute(InternalXmlPortFieldAttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortTextElement(InternalXmlPortTextElementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortTextAttribute(InternalXmlPortTextAttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitControlAddIn(InternalControlAddInSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberAttribute(InternalMemberAttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAttributeArgumentList(InternalAttributeArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvalidAttributeArgument(InternalInvalidAttributeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLiteralAttributeArgument(InternalLiteralAttributeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMethodReferenceAttributeArgument(InternalMethodReferenceAttributeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionAccessAttributeArgument(InternalOptionAccessAttributeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierNameOrEmpty(InternalIdentifierNameOrEmptySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLiteralExpression(InternalLiteralExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionValues(InternalOptionValuesSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInt32SignedLiteralValue(InternalInt32SignedLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInt64SignedLiteralValue(InternalInt64SignedLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDecimalSignedLiteralValue(InternalDecimalSignedLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBooleanLiteralValue(InternalBooleanLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitStringLiteralValue(InternalStringLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDateLiteralValue(InternalDateLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTimeLiteralValue(InternalTimeLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDateTimeLiteralValue(InternalDateTimeLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvalidPropertyValue(InternalInvalidPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDecimalPlacesPropertyValue(InternalDecimalPlacesPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionValuePropertyValue(InternalOptionValuePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitImagePropertyValue(InternalImagePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageFieldReferencePropertyValue(InternalPageFieldReferencePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMultilanguagePropertyValue(InternalMultilanguagePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLabelPropertyValue(InternalLabelPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberReferencePropertyValue(InternalMemberReferencePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPermissionPropertyValue(InternalPermissionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQualifiedObjectReferencePropertyValue(InternalQualifiedObjectReferencePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitObjectReferencePropertyValue(InternalObjectReferencePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPermission(InternalPermissionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumPropertyValue(InternalEnumPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierOrLiteralPropertyValue(InternalIdentifierOrLiteralPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierOrLiteralExpression(InternalIdentifierOrLiteralExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitObjectNameReference(InternalObjectNameReferenceSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitObjectNameOrId(InternalObjectNameOrIdSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitObjectId(InternalObjectIdSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDecimalPlaces(InternalDecimalPlacesSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberAccessOrIdentifierPropertyValue(InternalMemberAccessOrIdentifierPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPropertyName(InternalPropertyNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedPropertyValue(InternalCommaSeparatedPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedStringsPropertyValue(InternalCommaSeparatedStringsPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedIdentifierEqualsStringList(InternalCommaSeparatedIdentifierEqualsStringListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedIdentifierEqualsLiteralList(InternalCommaSeparatedIdentifierEqualsLiteralListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedIdentifierOrLiteralPropertyValue(InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLabel(InternalLabelSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionValuesPropertyValue(InternalOptionValuesPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBooleanPropertyValue(InternalBooleanPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInt32PropertyValue(InternalInt32PropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInt64PropertyValue(InternalInt64PropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDecimalPropertyValue(InternalDecimalPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTimePropertyValue(InternalTimePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDatePropertyValue(InternalDatePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDateTimePropertyValue(InternalDateTimePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitStringPropertyValue(InternalStringPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitStyleExpressionPropertyValue(InternalStyleExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBooleanExpressionPropertyValue(InternalBooleanExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTextExpressionPropertyValue(InternalTextExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIntegerExpressionPropertyValue(InternalIntegerExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitClientSideBooleanExpressionPropertyValue(InternalClientSideBooleanExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableFilterPropertyValue(InternalTableFilterPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrderByPropertyValue(InternalOrderByPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrderByExpression(InternalOrderByExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryDataItemLinkPropertyValue(InternalQueryDataItemLinkPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryDataItemLinkExpression(InternalQueryDataItemLinkExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportDataItemLinkPropertyValue(InternalReportDataItemLinkPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportDataItemLinkExpression(InternalReportDataItemLinkExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableViewPropertyValue(InternalTableViewPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFiltersPropertyValue(InternalFiltersPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSorting(InternalSortingSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrder(InternalOrderSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableRelationPropertyValue(InternalTableRelationPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIfTableRelation(InternalIfTableRelationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableFilterExpression(InternalTableFilterExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitElseTableRelation(InternalElseTableRelationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWhereExpression(InternalWhereExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConstExpression(InternalConstExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSimpleFieldExpression(InternalSimpleFieldExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvalidPropertyExpression(InternalInvalidPropertyExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableCalculationFormula(InternalTableCalculationFormulaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldCalculationFormula(InternalFieldCalculationFormulaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldFilterExpression(InternalFieldFilterExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldUpperLimitExpression(InternalFieldUpperLimitExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldUpperLimitFilterExpression(InternalFieldUpperLimitFilterExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFilterExpression(InternalFilterExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParenthesizedFilterExpressionValue(InternalParenthesizedFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitUnaryFilterExpressionValue(InternalUnaryFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBinaryFilterExpressionValue(InternalBinaryFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRangeFromFilterExpressionValue(InternalRangeFromFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvalidFilterExpressionValue(InternalInvalidFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEmptyProperty(InternalEmptyPropertySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitProperty(InternalPropertySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitVarSection(InternalVarSectionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitVariableDeclaration(InternalVariableDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParameter(InternalParameterSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReturnValue(InternalReturnValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSimpleTypeReference(InternalSimpleTypeReferenceSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRecordTypeReference(InternalRecordTypeReferenceSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetTypeReference(InternalDotNetTypeReferenceSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArray(InternalArraySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBracketedDimensionList(InternalBracketedDimensionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDimension(InternalDimensionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionDataType(InternalOptionDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSimpleNamedDataType(InternalSimpleNamedDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitGenericNamedDataType(InternalGenericNamedDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTextConstDataType(InternalTextConstDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLabelDataType(InternalLabelDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetDataType(InternalDotNetDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLengthDataType(InternalLengthDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSubtypedDataType(InternalSubtypedDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumDataType(InternalEnumDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParameterList(InternalParameterListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTriggerDeclaration(InternalTriggerDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEventTriggerDeclaration(InternalEventTriggerDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMethodDeclaration(InternalMethodDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEventDeclaration(InternalEventDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBlock(InternalBlockSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIfStatement(InternalIfStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrphanedElseStatement(InternalOrphanedElseStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAssignmentStatement(InternalAssignmentStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCompoundAssignmentStatement(InternalCompoundAssignmentStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitForStatement(InternalForStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWhileStatement(InternalWhileStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWithStatement(InternalWithStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitExitStatement(InternalExitStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitExpressionStatement(InternalExpressionStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRepeatStatement(InternalRepeatStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitForEachStatement(InternalForEachStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBreakStatement(InternalBreakStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAssertErrorStatement(InternalAssertErrorStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCaseStatement(InternalCaseStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCaseLine(InternalCaseLineSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCaseElse(InternalCaseElseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEmptyStatement(InternalEmptyStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParenthesizedExpression(InternalParenthesizedExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitElementAccessExpression(InternalElementAccessExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBracketedArgumentList(InternalBracketedArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvocationExpression(InternalInvocationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArgumentList(InternalArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitUnaryExpression(InternalUnaryExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBinaryExpression(InternalBinaryExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInListExpression(InternalInListExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberAccessExpression(InternalMemberAccessExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionAccessExpression(InternalOptionAccessExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierName(InternalIdentifierNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQualifiedName(InternalQualifiedNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierEqualsString(InternalIdentifierEqualsStringSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierEqualsLiteral(InternalIdentifierEqualsLiteralSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSkippedTokensTrivia(InternalSkippedTokensTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult Visit(InternalSyntaxNode node)
	{
		if (node == null)
		{
			return default;
		}
		return node.Accept(this);
	}

	public virtual TResult VisitToken(InternalSyntaxToken token)
	{
		return DefaultVisit(token);
	}

	public virtual TResult VisitTrivia(InternalSyntaxTrivia trivia)
	{
		return DefaultVisit(trivia);
	}

	protected virtual TResult DefaultVisit(InternalSyntaxNode node)
	{
		return default;
	}
}
[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal abstract class InternalSyntaxVisitor
{
	public virtual void VisitObjectCompilationUnit(InternalObjectCompilationUnitSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPropertyList(InternalPropertyListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitProfile(InternalProfileSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumType(InternalEnumTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumValue(InternalEnumValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumExtensionType(InternalEnumExtensionTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetPackage(InternalDotNetPackageSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetAssembly(InternalDotNetAssemblySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetTypeDeclaration(InternalDotNetTypeDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCodeunit(InternalCodeunitSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTable(InternalTableSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldList(InternalFieldListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldExtensionList(InternalFieldExtensionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitField(InternalFieldSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldModification(InternalFieldModificationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitKeyList(InternalKeyListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitKey(InternalKeySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldGroupList(InternalFieldGroupListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldGroupExtensionList(InternalFieldGroupExtensionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldGroupAddChange(InternalFieldGroupAddChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldGroup(InternalFieldGroupSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPage(InternalPageSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageLayout(InternalPageLayoutSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageField(InternalPageFieldSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageLabel(InternalPageLabelSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageGroup(InternalPageGroupSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPagePart(InternalPagePartSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageSystemPart(InternalPageSystemPartSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageChartPart(InternalPageChartPartSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageUserControl(InternalPageUserControlSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageArea(InternalPageAreaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageActionList(InternalPageActionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageViewList(InternalPageViewListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageView(InternalPageViewSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitGroupActionList(InternalGroupActionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageActionGroup(InternalPageActionGroupSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageActionArea(InternalPageActionAreaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageAction(InternalPageActionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageActionSeparator(InternalPageActionSeparatorSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageExtension(InternalPageExtensionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageCustomization(InternalPageCustomizationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableExtension(InternalTableExtensionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageExtensionLayout(InternalPageExtensionLayoutSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitControlAddChange(InternalControlAddChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitControlMoveChange(InternalControlMoveChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitControlModifyChange(InternalControlModifyChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageExtensionViewList(InternalPageExtensionViewListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitViewAddChange(InternalViewAddChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitViewMoveChange(InternalViewMoveChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitViewModifyChange(InternalViewModifyChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageExtensionActionList(InternalPageExtensionActionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitActionAddChange(InternalActionAddChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitActionMoveChange(InternalActionMoveChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitActionModifyChange(InternalActionModifyChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQuery(InternalQuerySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryElements(InternalQueryElementsSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryDataItem(InternalQueryDataItemSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryColumn(InternalQueryColumnSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryFilter(InternalQueryFilterSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReport(InternalReportSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportDataSetSection(InternalReportDataSetSectionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportDataItem(InternalReportDataItemSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportColumn(InternalReportColumnSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportLabelsSection(InternalReportLabelsSectionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportLabelMultilanguage(InternalReportLabelMultilanguageSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportLabel(InternalReportLabelSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRequestPage(InternalRequestPageSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPort(InternalXmlPortSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortSchema(InternalXmlPortSchemaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortTableElement(InternalXmlPortTableElementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortFieldElement(InternalXmlPortFieldElementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortFieldAttribute(InternalXmlPortFieldAttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortTextElement(InternalXmlPortTextElementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortTextAttribute(InternalXmlPortTextAttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitControlAddIn(InternalControlAddInSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberAttribute(InternalMemberAttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAttributeArgumentList(InternalAttributeArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvalidAttributeArgument(InternalInvalidAttributeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLiteralAttributeArgument(InternalLiteralAttributeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMethodReferenceAttributeArgument(InternalMethodReferenceAttributeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionAccessAttributeArgument(InternalOptionAccessAttributeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierNameOrEmpty(InternalIdentifierNameOrEmptySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLiteralExpression(InternalLiteralExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionValues(InternalOptionValuesSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInt32SignedLiteralValue(InternalInt32SignedLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInt64SignedLiteralValue(InternalInt64SignedLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDecimalSignedLiteralValue(InternalDecimalSignedLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBooleanLiteralValue(InternalBooleanLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitStringLiteralValue(InternalStringLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDateLiteralValue(InternalDateLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTimeLiteralValue(InternalTimeLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDateTimeLiteralValue(InternalDateTimeLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvalidPropertyValue(InternalInvalidPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDecimalPlacesPropertyValue(InternalDecimalPlacesPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionValuePropertyValue(InternalOptionValuePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitImagePropertyValue(InternalImagePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageFieldReferencePropertyValue(InternalPageFieldReferencePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMultilanguagePropertyValue(InternalMultilanguagePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLabelPropertyValue(InternalLabelPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberReferencePropertyValue(InternalMemberReferencePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPermissionPropertyValue(InternalPermissionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQualifiedObjectReferencePropertyValue(InternalQualifiedObjectReferencePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitObjectReferencePropertyValue(InternalObjectReferencePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPermission(InternalPermissionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumPropertyValue(InternalEnumPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierOrLiteralPropertyValue(InternalIdentifierOrLiteralPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierOrLiteralExpression(InternalIdentifierOrLiteralExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitObjectNameReference(InternalObjectNameReferenceSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitObjectNameOrId(InternalObjectNameOrIdSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitObjectId(InternalObjectIdSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDecimalPlaces(InternalDecimalPlacesSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberAccessOrIdentifierPropertyValue(InternalMemberAccessOrIdentifierPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPropertyName(InternalPropertyNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedPropertyValue(InternalCommaSeparatedPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedStringsPropertyValue(InternalCommaSeparatedStringsPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedIdentifierEqualsStringList(InternalCommaSeparatedIdentifierEqualsStringListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedIdentifierEqualsLiteralList(InternalCommaSeparatedIdentifierEqualsLiteralListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(InternalCommaSeparatedIdentifierEqualsStringListPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedIdentifierOrLiteralPropertyValue(InternalCommaSeparatedIdentifierOrLiteralPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLabel(InternalLabelSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionValuesPropertyValue(InternalOptionValuesPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBooleanPropertyValue(InternalBooleanPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInt32PropertyValue(InternalInt32PropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInt64PropertyValue(InternalInt64PropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDecimalPropertyValue(InternalDecimalPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTimePropertyValue(InternalTimePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDatePropertyValue(InternalDatePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDateTimePropertyValue(InternalDateTimePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitStringPropertyValue(InternalStringPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitStyleExpressionPropertyValue(InternalStyleExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBooleanExpressionPropertyValue(InternalBooleanExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTextExpressionPropertyValue(InternalTextExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIntegerExpressionPropertyValue(InternalIntegerExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitClientSideBooleanExpressionPropertyValue(InternalClientSideBooleanExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableFilterPropertyValue(InternalTableFilterPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrderByPropertyValue(InternalOrderByPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrderByExpression(InternalOrderByExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryDataItemLinkPropertyValue(InternalQueryDataItemLinkPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryDataItemLinkExpression(InternalQueryDataItemLinkExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportDataItemLinkPropertyValue(InternalReportDataItemLinkPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportDataItemLinkExpression(InternalReportDataItemLinkExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableViewPropertyValue(InternalTableViewPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFiltersPropertyValue(InternalFiltersPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSorting(InternalSortingSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrder(InternalOrderSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableRelationPropertyValue(InternalTableRelationPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIfTableRelation(InternalIfTableRelationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableFilterExpression(InternalTableFilterExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitElseTableRelation(InternalElseTableRelationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWhereExpression(InternalWhereExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConstExpression(InternalConstExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSimpleFieldExpression(InternalSimpleFieldExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvalidPropertyExpression(InternalInvalidPropertyExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableCalculationFormula(InternalTableCalculationFormulaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldCalculationFormula(InternalFieldCalculationFormulaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldFilterExpression(InternalFieldFilterExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldUpperLimitExpression(InternalFieldUpperLimitExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldUpperLimitFilterExpression(InternalFieldUpperLimitFilterExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFilterExpression(InternalFilterExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParenthesizedFilterExpressionValue(InternalParenthesizedFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitUnaryFilterExpressionValue(InternalUnaryFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBinaryFilterExpressionValue(InternalBinaryFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRangeFromFilterExpressionValue(InternalRangeFromFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvalidFilterExpressionValue(InternalInvalidFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEmptyProperty(InternalEmptyPropertySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitProperty(InternalPropertySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitVarSection(InternalVarSectionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitVariableDeclaration(InternalVariableDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParameter(InternalParameterSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReturnValue(InternalReturnValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSimpleTypeReference(InternalSimpleTypeReferenceSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRecordTypeReference(InternalRecordTypeReferenceSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetTypeReference(InternalDotNetTypeReferenceSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArray(InternalArraySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBracketedDimensionList(InternalBracketedDimensionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDimension(InternalDimensionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionDataType(InternalOptionDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSimpleNamedDataType(InternalSimpleNamedDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitGenericNamedDataType(InternalGenericNamedDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTextConstDataType(InternalTextConstDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLabelDataType(InternalLabelDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetDataType(InternalDotNetDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLengthDataType(InternalLengthDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSubtypedDataType(InternalSubtypedDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumDataType(InternalEnumDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParameterList(InternalParameterListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTriggerDeclaration(InternalTriggerDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEventTriggerDeclaration(InternalEventTriggerDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMethodDeclaration(InternalMethodDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEventDeclaration(InternalEventDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBlock(InternalBlockSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIfStatement(InternalIfStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrphanedElseStatement(InternalOrphanedElseStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAssignmentStatement(InternalAssignmentStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCompoundAssignmentStatement(InternalCompoundAssignmentStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitForStatement(InternalForStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWhileStatement(InternalWhileStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWithStatement(InternalWithStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitExitStatement(InternalExitStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitExpressionStatement(InternalExpressionStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRepeatStatement(InternalRepeatStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitForEachStatement(InternalForEachStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBreakStatement(InternalBreakStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAssertErrorStatement(InternalAssertErrorStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCaseStatement(InternalCaseStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCaseLine(InternalCaseLineSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCaseElse(InternalCaseElseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEmptyStatement(InternalEmptyStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParenthesizedExpression(InternalParenthesizedExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitElementAccessExpression(InternalElementAccessExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBracketedArgumentList(InternalBracketedArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvocationExpression(InternalInvocationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArgumentList(InternalArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitUnaryExpression(InternalUnaryExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBinaryExpression(InternalBinaryExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInListExpression(InternalInListExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberAccessExpression(InternalMemberAccessExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionAccessExpression(InternalOptionAccessExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierName(InternalIdentifierNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQualifiedName(InternalQualifiedNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierEqualsString(InternalIdentifierEqualsStringSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierEqualsLiteral(InternalIdentifierEqualsLiteralSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSkippedTokensTrivia(InternalSkippedTokensTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void Visit(InternalSyntaxNode node)
	{
		node?.Accept(this);
	}

	public virtual void VisitToken(InternalSyntaxToken token)
	{
		DefaultVisit(token);
	}

	public virtual void VisitTrivia(InternalSyntaxTrivia trivia)
	{
		DefaultVisit(trivia);
	}

	public virtual void DefaultVisit(InternalSyntaxNode node)
	{
	}
}
