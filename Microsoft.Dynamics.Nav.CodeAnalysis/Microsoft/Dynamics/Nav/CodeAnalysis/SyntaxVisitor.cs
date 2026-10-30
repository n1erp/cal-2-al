using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
public abstract class SyntaxVisitor<TResult>
{
	public virtual TResult VisitObjectCompilationUnit(ObjectCompilationUnitSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPropertyList(PropertyListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitProfile(ProfileSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumType(EnumTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumValue(EnumValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumExtensionType(EnumExtensionTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetPackage(DotNetPackageSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetAssembly(DotNetAssemblySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetTypeDeclaration(DotNetTypeDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCodeunit(CodeunitSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTable(TableSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldList(FieldListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldExtensionList(FieldExtensionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitField(FieldSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldModification(FieldModificationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitKeyList(KeyListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitKey(KeySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldGroupList(FieldGroupListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldGroupExtensionList(FieldGroupExtensionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldGroupAddChange(FieldGroupAddChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldGroup(FieldGroupSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPage(PageSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageLayout(PageLayoutSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageField(PageFieldSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageLabel(PageLabelSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageGroup(PageGroupSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPagePart(PagePartSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageSystemPart(PageSystemPartSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageChartPart(PageChartPartSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageUserControl(PageUserControlSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageArea(PageAreaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageActionList(PageActionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageViewList(PageViewListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageView(PageViewSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitGroupActionList(GroupActionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageActionGroup(PageActionGroupSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageActionArea(PageActionAreaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageAction(PageActionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageActionSeparator(PageActionSeparatorSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageExtension(PageExtensionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageCustomization(PageCustomizationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableExtension(TableExtensionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageExtensionLayout(PageExtensionLayoutSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitControlAddChange(ControlAddChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitControlMoveChange(ControlMoveChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitControlModifyChange(ControlModifyChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageExtensionViewList(PageExtensionViewListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitViewAddChange(ViewAddChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitViewMoveChange(ViewMoveChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitViewModifyChange(ViewModifyChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageExtensionActionList(PageExtensionActionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitActionAddChange(ActionAddChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitActionMoveChange(ActionMoveChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitActionModifyChange(ActionModifyChangeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQuery(QuerySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryElements(QueryElementsSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryDataItem(QueryDataItemSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryColumn(QueryColumnSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryFilter(QueryFilterSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReport(ReportSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportDataSetSection(ReportDataSetSectionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportDataItem(ReportDataItemSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportColumn(ReportColumnSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportLabelsSection(ReportLabelsSectionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportLabelMultilanguage(ReportLabelMultilanguageSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportLabel(ReportLabelSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRequestPage(RequestPageSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPort(XmlPortSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortSchema(XmlPortSchemaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortTableElement(XmlPortTableElementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortFieldElement(XmlPortFieldElementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortFieldAttribute(XmlPortFieldAttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortTextElement(XmlPortTextElementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitXmlPortTextAttribute(XmlPortTextAttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitControlAddIn(ControlAddInSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberAttribute(MemberAttributeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAttributeArgumentList(AttributeArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvalidAttributeArgument(InvalidAttributeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLiteralAttributeArgument(LiteralAttributeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMethodReferenceAttributeArgument(MethodReferenceAttributeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionAccessAttributeArgument(OptionAccessAttributeArgumentSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierNameOrEmpty(IdentifierNameOrEmptySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLiteralExpression(LiteralExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionValues(OptionValuesSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInt32SignedLiteralValue(Int32SignedLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInt64SignedLiteralValue(Int64SignedLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDecimalSignedLiteralValue(DecimalSignedLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBooleanLiteralValue(BooleanLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitStringLiteralValue(StringLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDateLiteralValue(DateLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTimeLiteralValue(TimeLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDateTimeLiteralValue(DateTimeLiteralValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvalidPropertyValue(InvalidPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDecimalPlacesPropertyValue(DecimalPlacesPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionValuePropertyValue(OptionValuePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitImagePropertyValue(ImagePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPageFieldReferencePropertyValue(PageFieldReferencePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMultilanguagePropertyValue(MultilanguagePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLabelPropertyValue(LabelPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberReferencePropertyValue(MemberReferencePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPermissionPropertyValue(PermissionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQualifiedObjectReferencePropertyValue(QualifiedObjectReferencePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitObjectReferencePropertyValue(ObjectReferencePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPermission(PermissionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumPropertyValue(EnumPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierOrLiteralPropertyValue(IdentifierOrLiteralPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierOrLiteralExpression(IdentifierOrLiteralExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitObjectNameReference(ObjectNameReferenceSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitObjectNameOrId(ObjectNameOrIdSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitObjectId(ObjectIdSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDecimalPlaces(DecimalPlacesSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberAccessOrIdentifierPropertyValue(MemberAccessOrIdentifierPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitPropertyName(PropertyNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedPropertyValue(CommaSeparatedPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedStringsPropertyValue(CommaSeparatedStringsPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedIdentifierEqualsStringList(CommaSeparatedIdentifierEqualsStringListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedIdentifierEqualsLiteralList(CommaSeparatedIdentifierEqualsLiteralListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCommaSeparatedIdentifierOrLiteralPropertyValue(CommaSeparatedIdentifierOrLiteralPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLabel(LabelSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionValuesPropertyValue(OptionValuesPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBooleanPropertyValue(BooleanPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInt32PropertyValue(Int32PropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInt64PropertyValue(Int64PropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDecimalPropertyValue(DecimalPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTimePropertyValue(TimePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDatePropertyValue(DatePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDateTimePropertyValue(DateTimePropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitStringPropertyValue(StringPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitStyleExpressionPropertyValue(StyleExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBooleanExpressionPropertyValue(BooleanExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTextExpressionPropertyValue(TextExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIntegerExpressionPropertyValue(IntegerExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitClientSideBooleanExpressionPropertyValue(ClientSideBooleanExpressionPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableFilterPropertyValue(TableFilterPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrderByPropertyValue(OrderByPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrderByExpression(OrderByExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryDataItemLinkPropertyValue(QueryDataItemLinkPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryDataItemLinkExpression(QueryDataItemLinkExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportDataItemLinkPropertyValue(ReportDataItemLinkPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReportDataItemLinkExpression(ReportDataItemLinkExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableViewPropertyValue(TableViewPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFiltersPropertyValue(FiltersPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSorting(SortingSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrder(OrderSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableRelationPropertyValue(TableRelationPropertyValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIfTableRelation(IfTableRelationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableFilterExpression(TableFilterExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitElseTableRelation(ElseTableRelationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWhereExpression(WhereExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitConstExpression(ConstExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSimpleFieldExpression(SimpleFieldExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvalidPropertyExpression(InvalidPropertyExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableCalculationFormula(TableCalculationFormulaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldCalculationFormula(FieldCalculationFormulaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldFilterExpression(FieldFilterExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldUpperLimitExpression(FieldUpperLimitExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFieldUpperLimitFilterExpression(FieldUpperLimitFilterExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitFilterExpression(FilterExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParenthesizedFilterExpressionValue(ParenthesizedFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitUnaryFilterExpressionValue(UnaryFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBinaryFilterExpressionValue(BinaryFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRangeFromFilterExpressionValue(RangeFromFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvalidFilterExpressionValue(InvalidFilterExpressionValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEmptyProperty(EmptyPropertySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitProperty(PropertySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitVarSection(VarSectionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitVariableDeclaration(VariableDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParameter(ParameterSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitReturnValue(ReturnValueSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSimpleTypeReference(SimpleTypeReferenceSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRecordTypeReference(RecordTypeReferenceSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetTypeReference(DotNetTypeReferenceSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArray(ArraySyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBracketedDimensionList(BracketedDimensionListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDimension(DimensionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionDataType(OptionDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSimpleNamedDataType(SimpleNamedDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitGenericNamedDataType(GenericNamedDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTextConstDataType(TextConstDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLabelDataType(LabelDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitDotNetDataType(DotNetDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLengthDataType(LengthDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSubtypedDataType(SubtypedDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEnumDataType(EnumDataTypeSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParameterList(ParameterListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTriggerDeclaration(TriggerDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEventTriggerDeclaration(EventTriggerDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEventDeclaration(EventDeclarationSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBlock(BlockSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIfStatement(IfStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOrphanedElseStatement(OrphanedElseStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAssignmentStatement(AssignmentStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitForStatement(ForStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWhileStatement(WhileStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitWithStatement(WithStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitExitStatement(ExitStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitExpressionStatement(ExpressionStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitRepeatStatement(RepeatStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitForEachStatement(ForEachStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBreakStatement(BreakStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAssertErrorStatement(AssertErrorStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCaseStatement(CaseStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCaseLine(CaseLineSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitCaseElse(CaseElseSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitEmptyStatement(EmptyStatementSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitElementAccessExpression(ElementAccessExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBracketedArgumentList(BracketedArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInvocationExpression(InvocationExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitArgumentList(ArgumentListSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitUnaryExpression(UnaryExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitBinaryExpression(BinaryExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitInListExpression(InListExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionAccessExpression(OptionAccessExpressionSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierName(IdentifierNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQualifiedName(QualifiedNameSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierEqualsString(IdentifierEqualsStringSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitIdentifierEqualsLiteral(IdentifierEqualsLiteralSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult Visit(SyntaxNode node)
	{
		if (node != null)
		{
			return node.Accept(this);
		}
		return default;
	}

	public virtual TResult DefaultVisit(SyntaxNode node)
	{
		return default;
	}
}
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
public abstract class SyntaxVisitor
{
	public virtual void VisitObjectCompilationUnit(ObjectCompilationUnitSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPropertyList(PropertyListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitProfile(ProfileSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumType(EnumTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumValue(EnumValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumExtensionType(EnumExtensionTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetPackage(DotNetPackageSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetAssembly(DotNetAssemblySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetTypeDeclaration(DotNetTypeDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCodeunit(CodeunitSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTable(TableSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldList(FieldListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldExtensionList(FieldExtensionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitField(FieldSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldModification(FieldModificationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitKeyList(KeyListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitKey(KeySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldGroupList(FieldGroupListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldGroupExtensionList(FieldGroupExtensionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldGroupAddChange(FieldGroupAddChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldGroup(FieldGroupSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPage(PageSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageLayout(PageLayoutSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageField(PageFieldSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageLabel(PageLabelSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageGroup(PageGroupSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPagePart(PagePartSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageSystemPart(PageSystemPartSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageChartPart(PageChartPartSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageUserControl(PageUserControlSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageArea(PageAreaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageActionList(PageActionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageViewList(PageViewListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageView(PageViewSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitGroupActionList(GroupActionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageActionGroup(PageActionGroupSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageActionArea(PageActionAreaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageAction(PageActionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageActionSeparator(PageActionSeparatorSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageExtension(PageExtensionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageCustomization(PageCustomizationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableExtension(TableExtensionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageExtensionLayout(PageExtensionLayoutSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitControlAddChange(ControlAddChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitControlMoveChange(ControlMoveChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitControlModifyChange(ControlModifyChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageExtensionViewList(PageExtensionViewListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitViewAddChange(ViewAddChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitViewMoveChange(ViewMoveChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitViewModifyChange(ViewModifyChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageExtensionActionList(PageExtensionActionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitActionAddChange(ActionAddChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitActionMoveChange(ActionMoveChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitActionModifyChange(ActionModifyChangeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQuery(QuerySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryElements(QueryElementsSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryDataItem(QueryDataItemSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryColumn(QueryColumnSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryFilter(QueryFilterSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReport(ReportSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportDataSetSection(ReportDataSetSectionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportDataItem(ReportDataItemSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportColumn(ReportColumnSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportLabelsSection(ReportLabelsSectionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportLabelMultilanguage(ReportLabelMultilanguageSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportLabel(ReportLabelSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRequestPage(RequestPageSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPort(XmlPortSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortSchema(XmlPortSchemaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortTableElement(XmlPortTableElementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortFieldElement(XmlPortFieldElementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortFieldAttribute(XmlPortFieldAttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortTextElement(XmlPortTextElementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitXmlPortTextAttribute(XmlPortTextAttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitControlAddIn(ControlAddInSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberAttribute(MemberAttributeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAttributeArgumentList(AttributeArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvalidAttributeArgument(InvalidAttributeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLiteralAttributeArgument(LiteralAttributeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMethodReferenceAttributeArgument(MethodReferenceAttributeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionAccessAttributeArgument(OptionAccessAttributeArgumentSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierNameOrEmpty(IdentifierNameOrEmptySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLiteralExpression(LiteralExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionValues(OptionValuesSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInt32SignedLiteralValue(Int32SignedLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInt64SignedLiteralValue(Int64SignedLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDecimalSignedLiteralValue(DecimalSignedLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBooleanLiteralValue(BooleanLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitStringLiteralValue(StringLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDateLiteralValue(DateLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTimeLiteralValue(TimeLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDateTimeLiteralValue(DateTimeLiteralValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvalidPropertyValue(InvalidPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDecimalPlacesPropertyValue(DecimalPlacesPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionValuePropertyValue(OptionValuePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitImagePropertyValue(ImagePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPageFieldReferencePropertyValue(PageFieldReferencePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMultilanguagePropertyValue(MultilanguagePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLabelPropertyValue(LabelPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberReferencePropertyValue(MemberReferencePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPermissionPropertyValue(PermissionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQualifiedObjectReferencePropertyValue(QualifiedObjectReferencePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitObjectReferencePropertyValue(ObjectReferencePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPermission(PermissionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumPropertyValue(EnumPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierOrLiteralPropertyValue(IdentifierOrLiteralPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierOrLiteralExpression(IdentifierOrLiteralExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitObjectNameReference(ObjectNameReferenceSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitObjectNameOrId(ObjectNameOrIdSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitObjectId(ObjectIdSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDecimalPlaces(DecimalPlacesSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberAccessOrIdentifierPropertyValue(MemberAccessOrIdentifierPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitPropertyName(PropertyNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedPropertyValue(CommaSeparatedPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedStringsPropertyValue(CommaSeparatedStringsPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedIdentifierEqualsStringList(CommaSeparatedIdentifierEqualsStringListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedIdentifierEqualsLiteralList(CommaSeparatedIdentifierEqualsLiteralListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedIdentifierEqualsStringListPropertyValue(CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCommaSeparatedIdentifierOrLiteralPropertyValue(CommaSeparatedIdentifierOrLiteralPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLabel(LabelSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionValuesPropertyValue(OptionValuesPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBooleanPropertyValue(BooleanPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInt32PropertyValue(Int32PropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInt64PropertyValue(Int64PropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDecimalPropertyValue(DecimalPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTimePropertyValue(TimePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDatePropertyValue(DatePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDateTimePropertyValue(DateTimePropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitStringPropertyValue(StringPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitStyleExpressionPropertyValue(StyleExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBooleanExpressionPropertyValue(BooleanExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTextExpressionPropertyValue(TextExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIntegerExpressionPropertyValue(IntegerExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitClientSideBooleanExpressionPropertyValue(ClientSideBooleanExpressionPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableFilterPropertyValue(TableFilterPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrderByPropertyValue(OrderByPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrderByExpression(OrderByExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryDataItemLinkPropertyValue(QueryDataItemLinkPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryDataItemLinkExpression(QueryDataItemLinkExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportDataItemLinkPropertyValue(ReportDataItemLinkPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReportDataItemLinkExpression(ReportDataItemLinkExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableViewPropertyValue(TableViewPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFiltersPropertyValue(FiltersPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSorting(SortingSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrder(OrderSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableRelationPropertyValue(TableRelationPropertyValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIfTableRelation(IfTableRelationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableFilterExpression(TableFilterExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitElseTableRelation(ElseTableRelationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWhereExpression(WhereExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitConstExpression(ConstExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSimpleFieldExpression(SimpleFieldExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvalidPropertyExpression(InvalidPropertyExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableCalculationFormula(TableCalculationFormulaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldCalculationFormula(FieldCalculationFormulaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldFilterExpression(FieldFilterExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldUpperLimitExpression(FieldUpperLimitExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFieldUpperLimitFilterExpression(FieldUpperLimitFilterExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitFilterExpression(FilterExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParenthesizedFilterExpressionValue(ParenthesizedFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitUnaryFilterExpressionValue(UnaryFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBinaryFilterExpressionValue(BinaryFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRangeFromFilterExpressionValue(RangeFromFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvalidFilterExpressionValue(InvalidFilterExpressionValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEmptyProperty(EmptyPropertySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitProperty(PropertySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitVarSection(VarSectionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitVariableDeclaration(VariableDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParameter(ParameterSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitReturnValue(ReturnValueSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSimpleTypeReference(SimpleTypeReferenceSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRecordTypeReference(RecordTypeReferenceSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetTypeReference(DotNetTypeReferenceSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArray(ArraySyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBracketedDimensionList(BracketedDimensionListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDimension(DimensionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionDataType(OptionDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSimpleNamedDataType(SimpleNamedDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitGenericNamedDataType(GenericNamedDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTextConstDataType(TextConstDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLabelDataType(LabelDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitDotNetDataType(DotNetDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLengthDataType(LengthDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSubtypedDataType(SubtypedDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEnumDataType(EnumDataTypeSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParameterList(ParameterListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTriggerDeclaration(TriggerDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEventTriggerDeclaration(EventTriggerDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEventDeclaration(EventDeclarationSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBlock(BlockSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIfStatement(IfStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOrphanedElseStatement(OrphanedElseStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAssignmentStatement(AssignmentStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitForStatement(ForStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWhileStatement(WhileStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitWithStatement(WithStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitExitStatement(ExitStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitExpressionStatement(ExpressionStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitRepeatStatement(RepeatStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitForEachStatement(ForEachStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBreakStatement(BreakStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAssertErrorStatement(AssertErrorStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCaseStatement(CaseStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCaseLine(CaseLineSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitCaseElse(CaseElseSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitEmptyStatement(EmptyStatementSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBracketedArgumentList(BracketedArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInvocationExpression(InvocationExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitArgumentList(ArgumentListSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitUnaryExpression(UnaryExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitBinaryExpression(BinaryExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitInListExpression(InListExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionAccessExpression(OptionAccessExpressionSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierName(IdentifierNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQualifiedName(QualifiedNameSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierEqualsString(IdentifierEqualsStringSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitIdentifierEqualsLiteral(IdentifierEqualsLiteralSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax node)
	{
		DefaultVisit(node);
	}

	public virtual void Visit(SyntaxNode node)
	{
		node?.Accept(this);
	}

	public virtual void DefaultVisit(SyntaxNode node)
	{
	}
}
