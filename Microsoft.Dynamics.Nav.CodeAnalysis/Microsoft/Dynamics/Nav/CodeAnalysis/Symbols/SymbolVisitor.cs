using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class SymbolVisitor
{
	public virtual void VisitAction(ActionSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitArrayType(ArrayTypeSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitAttributeArgument(AttributeArgumentSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitAttribute(AttributeSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitLabelType(LabelTypeSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitChangeAdd(ChangeAddSymbol node)
	{
		VisitChange(node);
	}

	public virtual void VisitChangeModify(ChangeModifySymbol node)
	{
		VisitChange(node);
	}

	public virtual void VisitChangeMove(ChangeMoveSymbol node)
	{
		VisitChange(node);
	}

	public virtual void VisitCodeunitType(CodeunitTypeSymbol node)
	{
		VisitApplicationObjectType(node);
	}

	public virtual void VisitControlAddInType(ControlAddInTypeSymbol node)
	{
		VisitObjectType(node);
	}

	public virtual void VisitDotNetControlAddInType(DotNetControlAddInTypeSymbol node)
	{
		VisitControlAddInType(node);
	}

	public virtual void VisitControl(ControlSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitDotNetAssembly(DotNetAssemblySymbol node)
	{
		VisitContainer(node);
	}

	public virtual void VisitDotNetEvent(DotNetEventSymbol node)
	{
		VisitEvent(node);
	}

	public virtual void VisitDotNetPackageType(DotNetPackageTypeSymbol node)
	{
		VisitObjectType(node);
	}

	public virtual void VisitDotNetTypeDeclaration(DotNetTypeDeclarationSymbol node)
	{
		VisitObjectType(node);
	}

	public virtual void VisitDotNetType(DotNetTypeSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitEnumType(EnumTypeSymbol node)
	{
		VisitApplicationObjectType(node);
	}

	public virtual void VisitEnumValue(EnumValueSymbol node)
	{
		VisitOption(node);
	}

	public virtual void VisitErrorMethod(ErrorMethodSymbol node)
	{
		VisitMethod(node);
	}

	public virtual void VisitErrorType(ErrorTypeSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitEvent(EventSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitExtendedErrorType(ExtendedErrorTypeSymbol node)
	{
		VisitErrorType(node);
	}

	public virtual void VisitFieldGroup(FieldGroupSymbol node)
	{
		VisitFieldNameList(node);
	}

	public virtual void VisitFieldNameList(FieldNameListSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitField(FieldSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitKey(KeySymbol node)
	{
		VisitFieldNameList(node);
	}

	public virtual void VisitLabel(LabelSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitMethod(MethodSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitModule(ModuleSymbol node)
	{
		VisitContainer(node);
	}

	public virtual void VisitOption(OptionSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitOptionType(OptionTypeSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitPageCustomizationType(PageCustomizationTypeSymbol node)
	{
		VisitPageExtensionBaseType(node);
	}

	public virtual void VisitPageExtensionType(PageExtensionTypeSymbol node)
	{
		VisitPageExtensionBaseType(node);
	}

	public virtual void VisitPageType(PageTypeSymbol node)
	{
		VisitApplicationObjectType(node);
	}

	public virtual void VisitParameter(ParameterSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitProfileType(ProfileTypeSymbol node)
	{
		VisitObjectType(node);
	}

	public virtual void VisitProperty(PropertySymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitQueryColumn(QueryColumnSymbol node)
	{
		VisitQueryElement(node);
	}

	public virtual void VisitQueryDataItem(QueryDataItemSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitQueryFilter(QueryFilterSymbol node)
	{
		VisitQueryElement(node);
	}

	public virtual void VisitQueryType(QueryTypeSymbol node)
	{
		VisitApplicationObjectType(node);
	}

	public virtual void VisitReportDataItem(ReportDataItemSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitReportColumn(ReportColumnSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitReportLabel(ReportLabelSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitReportType(ReportTypeSymbol node)
	{
		VisitApplicationObjectType(node);
	}

	public virtual void VisitRequestPageType(RequestPageTypeSymbol node)
	{
		VisitObjectType(node);
	}

	public virtual void VisitReturnValue(ReturnValueSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitTableExtensionType(TableExtensionTypeSymbol node)
	{
		VisitApplicationObjectExtensionType(node);
	}

	public virtual void VisitTableType(TableTypeSymbol node)
	{
		VisitApplicationObjectType(node);
	}

	public virtual void VisitVariable(VariableSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitView(ViewSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitXmlPortNode(XmlPortNodeSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitXmlPortType(XmlPortTypeSymbol node)
	{
		VisitApplicationObjectType(node);
	}

	public virtual void VisitContainer(ContainerSymbol node)
	{
		DefaultVisit(node);
	}

	public virtual void VisitType(TypeSymbol node)
	{
		VisitContainer(node);
	}

	public virtual void VisitObjectType(ObjectTypeSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitApplicationObjectType(ApplicationObjectTypeSymbol node)
	{
		VisitObjectType(node);
	}

	public virtual void VisitApplicationObjectExtensionType(ApplicationObjectExtensionTypeSymbol node)
	{
		VisitApplicationObjectType(node);
	}

	public virtual void VisitChange(ChangeSymbol node)
	{
		VisitContainer(node);
	}

	public virtual void VisitPageExtensionBaseType(PageExtensionBaseTypeSymbol node)
	{
		VisitApplicationObjectExtensionType(node);
	}

	public virtual void VisitQueryElement(QueryElementSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitBuiltInAttributeArgument(BuiltInAttributeArgumentSymbol node)
	{
		VisitAttributeArgument(node);
	}

	public virtual void VisitBuiltInMethodType(BuiltInMethodTypeSymbol node)
	{
		VisitMethod(node);
	}

	public virtual void VisitBuiltInParameter(BuiltInParameterSymbol node)
	{
		VisitParameter(node);
	}

	public virtual void VisitBuiltInReturnValue(BuiltInReturnValueSymbol node)
	{
		VisitReturnValue(node);
	}

	public virtual void VisitBuiltInTriggerEvent(BuiltInTriggerEventSymbol node)
	{
		VisitMethod(node);
	}

	public virtual void VisitCodeType(CodeTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitDotNetConstructorType(DotNetConstructorTypeSymbol node)
	{
		VisitDotNetMethodBaseType(node);
	}

	public virtual void VisitDotNetEnumMember(DotNetEnumMemberSymbol node)
	{
		VisitOption(node);
	}

	public virtual void VisitDotNetMethodBaseType(DotNetMethodBaseTypeSymbol node)
	{
		VisitBuiltInMethodType(node);
	}

	public virtual void VisitDotNetMethodType(DotNetMethodTypeSymbol node)
	{
		VisitDotNetMethodBaseType(node);
	}

	public virtual void VisitDotNetParameter(DotNetParameterSymbol node)
	{
		VisitBuiltInParameter(node);
	}

	public virtual void VisitDotNetPropertyType(DotNetPropertyTypeSymbol node)
	{
		VisitDotNetMethodBaseType(node);
	}

	public virtual void VisitGenericLanguageClassType(GenericLanguageClassTypeSymbol node)
	{
		VisitLanguageClassType(node);
	}

	public virtual void VisitGenericNavType(GenericNavTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitLanguageClassType(LanguageClassTypeSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitNavType(NavTypeSymbol node)
	{
		VisitType(node);
	}

	public virtual void VisitPageReferenceType(PageReferenceTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitQueryReferenceType(QueryReferenceTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitRecordType(RecordTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitReportReferenceType(ReportReferenceTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitSimpleNavType(SimpleNavTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitSystemOption(SystemOptionSymbol node)
	{
		VisitOption(node);
	}

	public virtual void VisitTestAction(TestActionSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitTestField(TestFieldSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitTestFilterField(TestFilterFieldSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitTestFilter(TestFilterSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitTestPageType(TestPageTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitTestPart(TestPartSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitTestRequestPageType(TestRequestPageTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitTextType(TextTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void VisitXmlPortReferenceType(XmlPortReferenceTypeSymbol node)
	{
		VisitNavType(node);
	}

	public virtual void Visit(Symbol symbol)
	{
		symbol?.Accept(this);
	}

	public virtual void DefaultVisit(Symbol symbol)
	{
	}
}
[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class SymbolVisitor<TArgument, TResult>
{
	public virtual TResult VisitAction(ActionSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitArrayType(ArrayTypeSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitAttributeArgument(AttributeArgumentSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitAttribute(AttributeSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitLabelType(LabelTypeSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitChangeAdd(ChangeAddSymbol node, TArgument argument)
	{
		return VisitChange(node, argument);
	}

	public virtual TResult VisitChangeModify(ChangeModifySymbol node, TArgument argument)
	{
		return VisitChange(node, argument);
	}

	public virtual TResult VisitChangeMove(ChangeMoveSymbol node, TArgument argument)
	{
		return VisitChange(node, argument);
	}

	public virtual TResult VisitCodeunitType(CodeunitTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectType(node, argument);
	}

	public virtual TResult VisitControlAddInType(ControlAddInTypeSymbol node, TArgument argument)
	{
		return VisitObjectType(node, argument);
	}

	public virtual TResult VisitDotNetControlAddInType(DotNetControlAddInTypeSymbol node, TArgument argument)
	{
		return VisitControlAddInType(node, argument);
	}

	public virtual TResult VisitControl(ControlSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitDotNetAssembly(DotNetAssemblySymbol node, TArgument argument)
	{
		return VisitContainer(node, argument);
	}

	public virtual TResult VisitDotNetEvent(DotNetEventSymbol node, TArgument argument)
	{
		return VisitEvent(node, argument);
	}

	public virtual TResult VisitDotNetPackageType(DotNetPackageTypeSymbol node, TArgument argument)
	{
		return VisitObjectType(node, argument);
	}

	public virtual TResult VisitDotNetTypeDeclaration(DotNetTypeDeclarationSymbol node, TArgument argument)
	{
		return VisitObjectType(node, argument);
	}

	public virtual TResult VisitDotNetType(DotNetTypeSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitEnumType(EnumTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectType(node, argument);
	}

	public virtual TResult VisitEnumValue(EnumValueSymbol node, TArgument argument)
	{
		return VisitOption(node, argument);
	}

	public virtual TResult VisitErrorMethod(ErrorMethodSymbol node, TArgument argument)
	{
		return VisitMethod(node, argument);
	}

	public virtual TResult VisitErrorType(ErrorTypeSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitEvent(EventSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitExtendedErrorType(ExtendedErrorTypeSymbol node, TArgument argument)
	{
		return VisitErrorType(node, argument);
	}

	public virtual TResult VisitFieldGroup(FieldGroupSymbol node, TArgument argument)
	{
		return VisitFieldNameList(node, argument);
	}

	public virtual TResult VisitFieldNameList(FieldNameListSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitField(FieldSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitKey(KeySymbol node, TArgument argument)
	{
		return VisitFieldNameList(node, argument);
	}

	public virtual TResult VisitLabel(LabelSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitMethod(MethodSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitModule(ModuleSymbol node, TArgument argument)
	{
		return VisitContainer(node, argument);
	}

	public virtual TResult VisitOption(OptionSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitOptionType(OptionTypeSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitPageCustomizationType(PageCustomizationTypeSymbol node, TArgument argument)
	{
		return VisitPageExtensionBaseType(node, argument);
	}

	public virtual TResult VisitPageExtensionType(PageExtensionTypeSymbol node, TArgument argument)
	{
		return VisitPageExtensionBaseType(node, argument);
	}

	public virtual TResult VisitPageType(PageTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectType(node, argument);
	}

	public virtual TResult VisitParameter(ParameterSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitProfileType(ProfileTypeSymbol node, TArgument argument)
	{
		return VisitObjectType(node, argument);
	}

	public virtual TResult VisitProperty(PropertySymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitQueryColumn(QueryColumnSymbol node, TArgument argument)
	{
		return VisitQueryElement(node, argument);
	}

	public virtual TResult VisitQueryDataItem(QueryDataItemSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitQueryFilter(QueryFilterSymbol node, TArgument argument)
	{
		return VisitQueryElement(node, argument);
	}

	public virtual TResult VisitQueryType(QueryTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectType(node, argument);
	}

	public virtual TResult VisitReportDataItem(ReportDataItemSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitReportColumn(ReportColumnSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitReportLabel(ReportLabelSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitReportType(ReportTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectType(node, argument);
	}

	public virtual TResult VisitRequestPageType(RequestPageTypeSymbol node, TArgument argument)
	{
		return VisitObjectType(node, argument);
	}

	public virtual TResult VisitReturnValue(ReturnValueSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitTableExtensionType(TableExtensionTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectExtensionType(node, argument);
	}

	public virtual TResult VisitTableType(TableTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectType(node, argument);
	}

	public virtual TResult VisitVariable(VariableSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitView(ViewSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitXmlPortNode(XmlPortNodeSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitXmlPortType(XmlPortTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectType(node, argument);
	}

	public virtual TResult VisitContainer(ContainerSymbol node, TArgument argument)
	{
		return DefaultVisit(node, argument);
	}

	public virtual TResult VisitType(TypeSymbol node, TArgument argument)
	{
		return VisitContainer(node, argument);
	}

	public virtual TResult VisitObjectType(ObjectTypeSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitApplicationObjectType(ApplicationObjectTypeSymbol node, TArgument argument)
	{
		return VisitObjectType(node, argument);
	}

	public virtual TResult VisitApplicationObjectExtensionType(ApplicationObjectExtensionTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectType(node, argument);
	}

	public virtual TResult VisitChange(ChangeSymbol node, TArgument argument)
	{
		return VisitContainer(node, argument);
	}

	public virtual TResult VisitPageExtensionBaseType(PageExtensionBaseTypeSymbol node, TArgument argument)
	{
		return VisitApplicationObjectExtensionType(node, argument);
	}

	public virtual TResult VisitQueryElement(QueryElementSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitBuiltInAttributeArgument(BuiltInAttributeArgumentSymbol node, TArgument argument)
	{
		return VisitAttributeArgument(node, argument);
	}

	public virtual TResult VisitBuiltInMethodType(BuiltInMethodTypeSymbol node, TArgument argument)
	{
		return VisitMethod(node, argument);
	}

	public virtual TResult VisitBuiltInParameter(BuiltInParameterSymbol node, TArgument argument)
	{
		return VisitParameter(node, argument);
	}

	public virtual TResult VisitBuiltInReturnValue(BuiltInReturnValueSymbol node, TArgument argument)
	{
		return VisitReturnValue(node, argument);
	}

	public virtual TResult VisitBuiltInTriggerEvent(BuiltInTriggerEventSymbol node, TArgument argument)
	{
		return VisitMethod(node, argument);
	}

	public virtual TResult VisitCodeType(CodeTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitDotNetConstructorType(DotNetConstructorTypeSymbol node, TArgument argument)
	{
		return VisitDotNetMethodBaseType(node, argument);
	}

	public virtual TResult VisitDotNetEnumMember(DotNetEnumMemberSymbol node, TArgument argument)
	{
		return VisitOption(node, argument);
	}

	public virtual TResult VisitDotNetMethodBaseType(DotNetMethodBaseTypeSymbol node, TArgument argument)
	{
		return VisitBuiltInMethodType(node, argument);
	}

	public virtual TResult VisitDotNetMethodType(DotNetMethodTypeSymbol node, TArgument argument)
	{
		return VisitDotNetMethodBaseType(node, argument);
	}

	public virtual TResult VisitDotNetParameter(DotNetParameterSymbol node, TArgument argument)
	{
		return VisitBuiltInParameter(node, argument);
	}

	public virtual TResult VisitDotNetPropertyType(DotNetPropertyTypeSymbol node, TArgument argument)
	{
		return VisitDotNetMethodBaseType(node, argument);
	}

	public virtual TResult VisitGenericLanguageClassType(GenericLanguageClassTypeSymbol node, TArgument argument)
	{
		return VisitLanguageClassType(node, argument);
	}

	public virtual TResult VisitGenericNavType(GenericNavTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitLanguageClassType(LanguageClassTypeSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitNavType(NavTypeSymbol node, TArgument argument)
	{
		return VisitType(node, argument);
	}

	public virtual TResult VisitPageReferenceType(PageReferenceTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitQueryReferenceType(QueryReferenceTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitRecordType(RecordTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitReportReferenceType(ReportReferenceTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitSimpleNavType(SimpleNavTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitSystemOption(SystemOptionSymbol node, TArgument argument)
	{
		return VisitOption(node, argument);
	}

	public virtual TResult VisitTestAction(TestActionSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitTestField(TestFieldSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitTestFilterField(TestFilterFieldSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitTestFilter(TestFilterSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitTestPageType(TestPageTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitTestPart(TestPartSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitTestRequestPageType(TestRequestPageTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitTextType(TextTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult VisitXmlPortReferenceType(XmlPortReferenceTypeSymbol node, TArgument argument)
	{
		return VisitNavType(node, argument);
	}

	public virtual TResult Visit(Symbol symbol, TArgument argument = default(TArgument))
	{
		if ((object)symbol == null)
		{
			return default;
		}
		return symbol.Accept(this, argument);
	}

	public virtual TResult DefaultVisit(Symbol symbol, TArgument argument)
	{
		return default;
	}
}
[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class SymbolVisitor<TResult>
{
	public virtual TResult VisitAction(ActionSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitArrayType(ArrayTypeSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitAttributeArgument(AttributeArgumentSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitAttribute(AttributeSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitLabelType(LabelTypeSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitChangeAdd(ChangeAddSymbol node)
	{
		return VisitChange(node);
	}

	public virtual TResult VisitChangeModify(ChangeModifySymbol node)
	{
		return VisitChange(node);
	}

	public virtual TResult VisitChangeMove(ChangeMoveSymbol node)
	{
		return VisitChange(node);
	}

	public virtual TResult VisitCodeunitType(CodeunitTypeSymbol node)
	{
		return VisitApplicationObjectType(node);
	}

	public virtual TResult VisitControlAddInType(ControlAddInTypeSymbol node)
	{
		return VisitObjectType(node);
	}

	public virtual TResult VisitDotNetControlAddInType(DotNetControlAddInTypeSymbol node)
	{
		return VisitControlAddInType(node);
	}

	public virtual TResult VisitControl(ControlSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitDotNetAssembly(DotNetAssemblySymbol node)
	{
		return VisitContainer(node);
	}

	public virtual TResult VisitDotNetEvent(DotNetEventSymbol node)
	{
		return VisitEvent(node);
	}

	public virtual TResult VisitDotNetPackageType(DotNetPackageTypeSymbol node)
	{
		return VisitObjectType(node);
	}

	public virtual TResult VisitDotNetTypeDeclaration(DotNetTypeDeclarationSymbol node)
	{
		return VisitObjectType(node);
	}

	public virtual TResult VisitDotNetType(DotNetTypeSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitEnumType(EnumTypeSymbol node)
	{
		return VisitApplicationObjectType(node);
	}

	public virtual TResult VisitEnumValue(EnumValueSymbol node)
	{
		return VisitOption(node);
	}

	public virtual TResult VisitErrorMethod(ErrorMethodSymbol node)
	{
		return VisitMethod(node);
	}

	public virtual TResult VisitErrorType(ErrorTypeSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitEvent(EventSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitExtendedErrorType(ExtendedErrorTypeSymbol node)
	{
		return VisitErrorType(node);
	}

	public virtual TResult VisitFieldGroup(FieldGroupSymbol node)
	{
		return VisitFieldNameList(node);
	}

	public virtual TResult VisitFieldNameList(FieldNameListSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitField(FieldSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitKey(KeySymbol node)
	{
		return VisitFieldNameList(node);
	}

	public virtual TResult VisitLabel(LabelSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitMethod(MethodSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitModule(ModuleSymbol node)
	{
		return VisitContainer(node);
	}

	public virtual TResult VisitOption(OptionSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitOptionType(OptionTypeSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitPageCustomizationType(PageCustomizationTypeSymbol node)
	{
		return VisitPageExtensionBaseType(node);
	}

	public virtual TResult VisitPageExtensionType(PageExtensionTypeSymbol node)
	{
		return VisitPageExtensionBaseType(node);
	}

	public virtual TResult VisitPageType(PageTypeSymbol node)
	{
		return VisitApplicationObjectType(node);
	}

	public virtual TResult VisitParameter(ParameterSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitProfileType(ProfileTypeSymbol node)
	{
		return VisitObjectType(node);
	}

	public virtual TResult VisitProperty(PropertySymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitQueryColumn(QueryColumnSymbol node)
	{
		return VisitQueryElement(node);
	}

	public virtual TResult VisitQueryDataItem(QueryDataItemSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitQueryFilter(QueryFilterSymbol node)
	{
		return VisitQueryElement(node);
	}

	public virtual TResult VisitQueryType(QueryTypeSymbol node)
	{
		return VisitApplicationObjectType(node);
	}

	public virtual TResult VisitReportDataItem(ReportDataItemSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitReportColumn(ReportColumnSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitReportLabel(ReportLabelSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitReportType(ReportTypeSymbol node)
	{
		return VisitApplicationObjectType(node);
	}

	public virtual TResult VisitRequestPageType(RequestPageTypeSymbol node)
	{
		return VisitObjectType(node);
	}

	public virtual TResult VisitReturnValue(ReturnValueSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitTableExtensionType(TableExtensionTypeSymbol node)
	{
		return VisitApplicationObjectExtensionType(node);
	}

	public virtual TResult VisitTableType(TableTypeSymbol node)
	{
		return VisitApplicationObjectType(node);
	}

	public virtual TResult VisitVariable(VariableSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitView(ViewSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitXmlPortNode(XmlPortNodeSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitXmlPortType(XmlPortTypeSymbol node)
	{
		return VisitApplicationObjectType(node);
	}

	public virtual TResult VisitContainer(ContainerSymbol node)
	{
		return DefaultVisit(node);
	}

	public virtual TResult VisitType(TypeSymbol node)
	{
		return VisitContainer(node);
	}

	public virtual TResult VisitObjectType(ObjectTypeSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitApplicationObjectType(ApplicationObjectTypeSymbol node)
	{
		return VisitObjectType(node);
	}

	public virtual TResult VisitApplicationObjectExtensionType(ApplicationObjectExtensionTypeSymbol node)
	{
		return VisitApplicationObjectType(node);
	}

	public virtual TResult VisitChange(ChangeSymbol node)
	{
		return VisitContainer(node);
	}

	public virtual TResult VisitPageExtensionBaseType(PageExtensionBaseTypeSymbol node)
	{
		return VisitApplicationObjectExtensionType(node);
	}

	public virtual TResult VisitQueryElement(QueryElementSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitBuiltInAttributeArgument(BuiltInAttributeArgumentSymbol node)
	{
		return VisitAttributeArgument(node);
	}

	public virtual TResult VisitBuiltInMethodType(BuiltInMethodTypeSymbol node)
	{
		return VisitMethod(node);
	}

	public virtual TResult VisitBuiltInParameter(BuiltInParameterSymbol node)
	{
		return VisitParameter(node);
	}

	public virtual TResult VisitBuiltInReturnValue(BuiltInReturnValueSymbol node)
	{
		return VisitReturnValue(node);
	}

	public virtual TResult VisitBuiltInTriggerEvent(BuiltInTriggerEventSymbol node)
	{
		return VisitMethod(node);
	}

	public virtual TResult VisitCodeType(CodeTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitDotNetConstructorType(DotNetConstructorTypeSymbol node)
	{
		return VisitDotNetMethodBaseType(node);
	}

	public virtual TResult VisitDotNetEnumMember(DotNetEnumMemberSymbol node)
	{
		return VisitOption(node);
	}

	public virtual TResult VisitDotNetMethodBaseType(DotNetMethodBaseTypeSymbol node)
	{
		return VisitBuiltInMethodType(node);
	}

	public virtual TResult VisitDotNetMethodType(DotNetMethodTypeSymbol node)
	{
		return VisitDotNetMethodBaseType(node);
	}

	public virtual TResult VisitDotNetParameter(DotNetParameterSymbol node)
	{
		return VisitBuiltInParameter(node);
	}

	public virtual TResult VisitDotNetPropertyType(DotNetPropertyTypeSymbol node)
	{
		return VisitDotNetMethodBaseType(node);
	}

	public virtual TResult VisitGenericLanguageClassType(GenericLanguageClassTypeSymbol node)
	{
		return VisitLanguageClassType(node);
	}

	public virtual TResult VisitGenericNavType(GenericNavTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitLanguageClassType(LanguageClassTypeSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitNavType(NavTypeSymbol node)
	{
		return VisitType(node);
	}

	public virtual TResult VisitPageReferenceType(PageReferenceTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitQueryReferenceType(QueryReferenceTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitRecordType(RecordTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitReportReferenceType(ReportReferenceTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitSimpleNavType(SimpleNavTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitSystemOption(SystemOptionSymbol node)
	{
		return VisitOption(node);
	}

	public virtual TResult VisitTestAction(TestActionSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitTestField(TestFieldSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitTestFilterField(TestFilterFieldSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitTestFilter(TestFilterSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitTestPageType(TestPageTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitTestPart(TestPartSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitTestRequestPageType(TestRequestPageTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitTextType(TextTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult VisitXmlPortReferenceType(XmlPortReferenceTypeSymbol node)
	{
		return VisitNavType(node);
	}

	public virtual TResult Visit(Symbol symbol)
	{
		if ((object)symbol != null)
		{
			return symbol.Accept(this);
		}
		return default;
	}

	public virtual TResult DefaultVisit(Symbol symbol)
	{
		return default;
	}
}
