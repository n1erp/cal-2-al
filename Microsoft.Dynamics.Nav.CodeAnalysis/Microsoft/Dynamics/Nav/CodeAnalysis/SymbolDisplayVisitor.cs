using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class SymbolDisplayVisitor : SymbolVisitor
{
	private readonly ArrayBuilder<SymbolDisplayPart> builder;

	private readonly SymbolDisplayFormat format;

	private readonly bool isFirstSymbolVisited;

	private readonly SemanticModel semanticModelOpt;

	private readonly int positionOpt;

	private SymbolDisplayVisitor lazyNotFirstVisitor;

	private static readonly HashSet<PropertyKind> PropertyTypesDependingOnParentFieldType = new HashSet<PropertyKind>
	{
		PropertyKind.MinValue,
		PropertyKind.MaxValue,
		PropertyKind.InitValue,
		PropertyKind.ValuesAllowed
	};

	protected SymbolDisplayVisitor NotFirstVisitor
	{
		get
		{
			if (lazyNotFirstVisitor == null)
			{
				lazyNotFirstVisitor = MakeNotFirstVisitor();
			}
			return lazyNotFirstVisitor;
		}
	}

	internal SymbolDisplayVisitor(ArrayBuilder<SymbolDisplayPart> builder, SymbolDisplayFormat format, SemanticModel semanticModelOpt, int positionOpt, bool isFirstSymbolVisited = true)
	{
		this.builder = builder;
		this.format = format;
		this.isFirstSymbolVisited = isFirstSymbolVisited;
		this.semanticModelOpt = semanticModelOpt;
		this.positionOpt = positionOpt;
		if (!this.isFirstSymbolVisited)
		{
			lazyNotFirstVisitor = this;
		}
	}

	private SymbolDisplayVisitor MakeNotFirstVisitor()
	{
		return new SymbolDisplayVisitor(builder, format, semanticModelOpt, positionOpt, isFirstSymbolVisited: false);
	}

	internal static SymbolDisplayPart CreatePart(SymbolDisplayPartKind kind, ISymbol symbol, string text)
	{
		text = ((text == null) ? "?" : text);
		return new SymbolDisplayPart(kind, symbol, text);
	}

	protected void AddSpace()
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.Space, null, " "));
	}

	private void AddPunctuation(SyntaxKind punctuationKind)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.Punctuation, null, punctuationKind.GetText()));
	}

	private void AddKeyword(SyntaxKind keywordKind)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, null, keywordKind.GetText()));
	}

	private void AddModifier(string modifier)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.Modifier, null, modifier));
	}

	private void AddLiteralValue(int value)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.NumericLiteral, null, ObjectDisplay.FormatLiteral(value)));
	}

	private void AddLiteralValue(string value)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.StringLiteral, null, value));
	}

	public override void VisitField(FieldSymbol symbol)
	{
		VisitTypedSymbol(symbol);
	}

	public override void VisitModule(ModuleSymbol node)
	{
		VisitModuleSymbol(node);
	}

	public override void VisitVariable(VariableSymbol symbol)
	{
		bool flag = symbol.Kind == SymbolKind.GlobalVariable;
		if (isFirstSymbolVisited && format.VariableOptions.IncludesOption(SymbolDisplayVariableOptions.IncludeSymbolKind))
		{
			AddLiteralValue(flag ? "(global)" : "(local)");
			AddSpace();
		}
		bool flag2 = format.VariableOptions.IncludesOption(SymbolDisplayVariableOptions.IncludeName);
		bool flag3 = format.VariableOptions.IncludesOption(SymbolDisplayVariableOptions.IncludeType);
		if (flag2)
		{
			AddIdentifierName(flag ? SymbolDisplayPartKind.GlobalName : SymbolDisplayPartKind.LocalName, symbol, symbol.Name);
		}
		if (flag2 & flag3)
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
		}
		if (flag3)
		{
			symbol.Type.Accept(NotFirstVisitor);
		}
	}

	public override void VisitParameter(ParameterSymbol symbol)
	{
		if (isFirstSymbolVisited && format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeSymbolKind))
		{
			AddLiteralValue("(parameter)");
			AddSpace();
		}
		if (format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeIsVar) && symbol.IsVar && symbol.Type.Kind != SymbolKind.DotNet)
		{
			AddKeyword(SyntaxKind.VarKeyword);
			AddSpace();
		}
		bool num = format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeName) && !string.IsNullOrEmpty(symbol.Name);
		bool flag = format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeType);
		bool flag2 = format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeOptionalBrackets) && symbol.IsOptional;
		if (flag2)
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
		}
		if (num)
		{
			AddIdentifierName(SymbolDisplayPartKind.ParameterName, symbol, symbol.Name);
		}
		if (num & flag)
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
		}
		if (flag)
		{
			symbol.Type.Accept(NotFirstVisitor);
		}
		if (format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeParams) && symbol.IsParams)
		{
			AddPunctuation(SyntaxKind.CommaToken);
			AddSpace();
			AddPunctuation(SyntaxKind.DotToken);
			AddPunctuation(SyntaxKind.DotToken);
			AddPunctuation(SyntaxKind.DotToken);
		}
		if (flag2)
		{
			AddPunctuation(SyntaxKind.CloseBracketToken);
		}
	}

	public override void VisitType(TypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.Name));
		if (symbol.HasLength)
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
			AddLiteralValue(symbol.Length);
			AddPunctuation(SyntaxKind.CloseBracketToken);
		}
	}

	public override void VisitDotNetType(DotNetTypeSymbol symbol)
	{
		AddLiteralValue("DotNet");
		AddSpace();
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.Name.QuoteIdentifierIfNeeded()));
	}

	public override void VisitTestPageType(TestPageTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.NavTypeKind.ToString()));
		AddSpace();
		builder.Add(CreatePart(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name));
	}

	public override void VisitTestRequestPageType(TestRequestPageTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.NavTypeKind.ToString()));
		AddSpace();
		builder.Add(CreatePart(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name));
	}

	public override void VisitGenericNavType(GenericNavTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.Name));
		AddSpace();
		AddKeyword(SyntaxKind.OfKeyword);
		AddSpace();
		AddPunctuation(SyntaxKind.OpenBracketToken);
		if (symbol.TypeArguments.Length > 0)
		{
			symbol.TypeArguments[0].Accept(NotFirstVisitor);
			for (int i = 1; i < symbol.TypeArguments.Length; i++)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
				symbol.TypeArguments[i].Accept(NotFirstVisitor);
			}
		}
		AddPunctuation(SyntaxKind.CloseBracketToken);
	}

	public override void VisitRecordType(RecordTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, NavTypeKind.Record.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name);
	}

	public override void VisitPageReferenceType(PageReferenceTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.IsCurrPage ? NavTypeKind.CurrPage.ToString() : NavTypeKind.Page.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name);
	}

	public override void VisitReportReferenceType(ReportReferenceTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.IsCurrReport ? NavTypeKind.CurrReport.ToString() : NavTypeKind.Report.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name);
	}

	public override void VisitQueryReferenceType(QueryReferenceTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.IsCurrQuery ? NavTypeKind.CurrQuery.ToString() : NavTypeKind.Query.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name);
	}

	public override void VisitApplicationObjectType(ApplicationObjectTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.Kind.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name);
	}

	public override void VisitPageExtensionType(PageExtensionTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, NavTypeKind.PageExtension.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name);
	}

	public override void VisitPageCustomizationType(PageCustomizationTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, NavTypeKind.PageCustomization.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name);
	}

	public override void VisitLabelType(LabelTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.NavTypeKind.ToString()));
	}

	public override void VisitObjectType(ObjectTypeSymbol symbol)
	{
		AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
	}

	public override void VisitProfileType(ProfileTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.Kind.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
	}

	public override void VisitQueryType(QueryTypeSymbol symbol)
	{
		VisitApplicationObjectType(symbol);
	}

	public override void VisitControlAddInType(ControlAddInTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.Kind.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name);
	}

	public override void VisitDotNetControlAddInType(DotNetControlAddInTypeSymbol symbol)
	{
		AddLiteralValue(".NET Add-In");
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ApplicationObjectName, symbol, symbol.Name);
	}

	private void AddIdentifierName(SymbolDisplayPartKind kind, Symbol symbol, string name)
	{
		builder.Add(CreatePart(kind, symbol, name.QuoteIdentifierIfNeeded()));
	}

	public override void VisitMethod(MethodSymbol symbol)
	{
		switch (symbol.MethodKind)
		{
		case MethodKind.Property:
			VisitMethodProperty(symbol);
			break;
		case MethodKind.EventTrigger:
			VisitEventTrigger((SourceDotNetEventTriggerSymbol)symbol);
			break;
		default:
			VisitMethodDefault(symbol);
			break;
		}
	}

	private void VisitMethodDefault(MethodSymbol symbol)
	{
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeModifiers) && symbol.IsTryFunction)
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
			AddModifier(AttributeKind.TryFunction.ToString());
			AddPunctuation(SyntaxKind.CloseBracketToken);
			AddSpace();
		}
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeAccessibility) && symbol.IsLocal)
		{
			AddKeyword(SyntaxKind.LocalKeyword);
			AddSpace();
		}
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeKeyword))
		{
			AddKeyword(symbol.IsTrigger() ? SyntaxKind.TriggerKeyword : SyntaxKind.ProcedureKeyword);
			AddSpace();
		}
		AddIdentifierName(SymbolDisplayPartKind.MethodName, symbol, symbol.Name);
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeParameters))
		{
			AddPunctuation(SyntaxKind.OpenParenToken);
			AddParametersIfRequired(symbol.Parameters);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
		AddReturnValueIfRequired(symbol);
	}

	private void VisitEventTrigger(SourceDotNetEventTriggerSymbol symbol)
	{
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeModifiers) && symbol.IsTryFunction)
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
			AddModifier(AttributeKind.TryFunction.ToString());
			AddPunctuation(SyntaxKind.CloseBracketToken);
			AddSpace();
		}
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeKeyword))
		{
			AddKeyword(SyntaxKind.TriggerKeyword);
			AddSpace();
		}
		AddIdentifierName(SymbolDisplayPartKind.MethodName, symbol, symbol.PublisherName);
		AddKeyword(SyntaxKind.ColonColonToken);
		AddIdentifierName(SymbolDisplayPartKind.MethodName, symbol, symbol.EventName);
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeParameters))
		{
			AddPunctuation(SyntaxKind.OpenParenToken);
			AddParametersIfRequired(symbol.Parameters);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
		AddReturnValueIfRequired(symbol);
	}

	private void VisitMethodProperty(MethodSymbol symbol)
	{
		AddLiteralValue("(property)");
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.PropertyName, symbol, symbol.Name);
		string value = symbol.ReturnType.ToString();
		if (!string.IsNullOrEmpty(value))
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
			AddLiteralValue(value);
		}
	}

	public override void VisitEvent(EventSymbol symbol)
	{
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeKeyword))
		{
			AddKeyword(SyntaxKind.EventKeyword);
			AddSpace();
		}
		AddIdentifierName(SymbolDisplayPartKind.MethodName, symbol, symbol.Name);
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeParameters))
		{
			AddPunctuation(SyntaxKind.OpenParenToken);
			AddParametersIfRequired(symbol.Parameters);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
	}

	private void AddParametersIfRequired(ImmutableArray<ParameterSymbol> parameters)
	{
		if (format.ParameterOptions == SymbolDisplayParameterOptions.None)
		{
			return;
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			ParameterSymbol parameterSymbol = parameters[i];
			if (i > 0)
			{
				SyntaxKind punctuationKind = (format.ParameterOptions.HasFlag(SymbolDisplayParameterOptions.UseDeclarationSeparator) ? SyntaxKind.SemicolonToken : SyntaxKind.CommaToken);
				AddPunctuation(punctuationKind);
				AddSpace();
			}
			parameterSymbol.Accept(NotFirstVisitor);
		}
	}

	private void AddReturnValueIfRequired(MethodSymbol symbol)
	{
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeType) && symbol.ReturnValue != null && symbol.HasReturnValue())
		{
			if (!symbol.ReturnValue.IsNamed)
			{
				AddPunctuation(SyntaxKind.ColonToken);
			}
			AddSpace();
			symbol.ReturnValue.Accept(NotFirstVisitor);
		}
	}

	public override void VisitFieldNameList(FieldNameListSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeName, symbol, symbol.Kind.ToString()));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
		AddPunctuation(SyntaxKind.ColonToken);
		AddSpace();
		for (int i = 0; i < symbol.Fields.Length; i++)
		{
			FieldSymbol fieldSymbol = symbol.Fields[i];
			AddIdentifierName(SymbolDisplayPartKind.ControlName, fieldSymbol, fieldSymbol.Name);
			if (i < symbol.Fields.Length - 1)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
		}
	}

	public override void VisitReturnValue(ReturnValueSymbol symbol)
	{
		if (isFirstSymbolVisited && format.ReturnValueOptions.IncludesOption(SymbolDisplayReturnValueOptions.IncludeSymbolKind))
		{
			AddLiteralValue("(return value)");
			AddSpace();
		}
		bool num = format.VariableOptions.IncludesOption(SymbolDisplayVariableOptions.IncludeName) && symbol.IsNamed;
		bool flag = format.VariableOptions.IncludesOption(SymbolDisplayVariableOptions.IncludeType);
		if (num)
		{
			AddIdentifierName(SymbolDisplayPartKind.ReturnValueName, symbol, symbol.Name);
		}
		if (num & flag)
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
		}
		if (flag)
		{
			symbol.Type.Accept(NotFirstVisitor);
		}
	}

	public override void VisitArrayType(ArrayTypeSymbol symbol)
	{
		AddKeyword(SyntaxKind.ArrayKeyword);
		AddPunctuation(SyntaxKind.OpenBracketToken);
		for (int i = 0; i < symbol.Dimensions.Length; i++)
		{
			if (i > 0)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
			AddLiteralValue(symbol.Dimensions[i]);
		}
		AddPunctuation(SyntaxKind.CloseBracketToken);
		AddSpace();
		AddKeyword(SyntaxKind.OfKeyword);
		AddSpace();
		symbol.ElementType.Accept(NotFirstVisitor);
	}

	public override void VisitTestField(TestFieldSymbol symbol)
	{
		if (symbol.ControlSymbol != null)
		{
			VisitControl(symbol.ControlSymbol);
		}
		else
		{
			base.VisitTestField(symbol);
		}
	}

	public override void VisitTestAction(TestActionSymbol symbol)
	{
		if (symbol.ActionSymbol != null)
		{
			VisitAction(symbol.ActionSymbol);
		}
		else
		{
			base.VisitTestAction(symbol);
		}
	}

	public override void VisitControl(ControlSymbol symbol)
	{
		switch (symbol.ControlKind)
		{
		case ControlKind.Field:
			VisitPageField(symbol);
			return;
		case ControlKind.Part:
			VisitControlWithReference(symbol, symbol.RelatedPartSymbol);
			return;
		case ControlKind.SystemPart:
			VisitPageSystemPart(symbol);
			return;
		case ControlKind.ChartPart:
			VisitPageChartPart(symbol);
			return;
		case ControlKind.UserControl:
			VisitControlWithReference(symbol, symbol.RelatedControlAddInSymbol);
			return;
		}
		AddLiteralValue(symbol.ControlKind.ToString());
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
	}

	private void VisitPageField(ControlSymbol symbol)
	{
		AddLiteralValue(symbol.ControlKind.ToString());
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
		TypeSymbol type = symbol.Type;
		if (type.NavTypeKind != NavTypeKind.None)
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
			type?.Accept(NotFirstVisitor);
		}
	}

	private void VisitControlWithReference(ControlSymbol symbol, Symbol relatedSymbol)
	{
		AddLiteralValue(symbol.ControlKind.ToString());
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
		if (relatedSymbol != null)
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
			AddIdentifierName(SymbolDisplayPartKind.ControlName, relatedSymbol, relatedSymbol.Name);
		}
	}

	private void VisitPageSystemPart(ControlSymbol symbol)
	{
		AddLiteralValue(symbol.ControlKind.ToString());
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
		string text = ((PageSystemPartSyntax)symbol.GetFirstSyntax())?.SystemPartType?.Identifier.Text;
		if (text != null)
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
			AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol.RelatedPartSymbol, text);
		}
	}

	private void VisitPageChartPart(ControlSymbol symbol)
	{
		AddLiteralValue(symbol.ControlKind.ToString());
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
		string text = ((PageChartPartSyntax)symbol.GetFirstSyntax())?.ChartPartType?.Identifier.Text;
		if (text != null)
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
			AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol.RelatedPartSymbol, text);
		}
	}

	public override void VisitReportDataItem(ReportDataItemSymbol symbol)
	{
		VisitTypedSymbol(symbol);
	}

	public override void VisitReportColumn(ReportColumnSymbol symbol)
	{
		VisitTypedSymbol(symbol);
	}

	public override void VisitQueryColumn(QueryColumnSymbol symbol)
	{
		VisitTypedSymbol(symbol);
	}

	public override void VisitQueryFilter(QueryFilterSymbol symbol)
	{
		VisitTypedSymbol(symbol, () => symbol.RelatedField?.Type);
	}

	public override void VisitQueryDataItem(QueryDataItemSymbol symbol)
	{
		VisitTypedSymbol(symbol);
	}

	public override void VisitXmlPortNode(XmlPortNodeSymbol symbol)
	{
		AddLiteralValue(XmlPortNodeType(symbol));
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
	}

	private static string XmlPortNodeType(XmlPortNodeSymbol symbol)
	{
		return symbol.SourceTypeKind.ToString() + symbol.NodeXmlKind;
	}

	public override void VisitAction(ActionSymbol symbol)
	{
		AddLiteralValue(symbol.ActionKind.ToString());
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ActionName, symbol, symbol.Name);
	}

	public override void VisitView(ViewSymbol symbol)
	{
		AddLiteralValue(SymbolKind.View.ToString());
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.ViewName, symbol, symbol.Name);
	}

	public override void VisitProperty(PropertySymbol symbol)
	{
		AddLiteralValue("(property)");
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.PropertyName, symbol, symbol.Name);
		if (PropertyTypesDependingOnParentFieldType.Contains(symbol.PropertyKind))
		{
			VisitPropertySymbolTypeDependingOnParentField(symbol);
			return;
		}
		string value = symbol.GetPropertyTypeInfo()?.GetTypeDescription();
		if (string.IsNullOrEmpty(value))
		{
			value = (symbol.GetFirstSyntax() as PropertySyntax)?.GetPropertyTypeInfo()?.GetTypeDescription();
		}
		if (!string.IsNullOrEmpty(value))
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
			AddLiteralValue(value);
		}
	}

	private void VisitPropertySymbolTypeDependingOnParentField(PropertySymbol symbol)
	{
		AddPunctuation(SyntaxKind.ColonToken);
		AddSpace();
		FieldSymbol containingSymbolOfType = symbol.GetContainingSymbolOfType<FieldSymbol>();
		if (containingSymbolOfType != null)
		{
			Visit(containingSymbolOfType.Type);
			return;
		}
		ControlSymbol containingSymbolOfType2 = symbol.GetContainingSymbolOfType<ControlSymbol>();
		if (containingSymbolOfType2 != null)
		{
			Visit(containingSymbolOfType2.Type);
		}
	}

	public override void VisitAttribute(AttributeSymbol symbol)
	{
		AddIdentifierName(SymbolDisplayPartKind.MethodName, symbol, symbol.Name);
		if (format.MethodOptions.IncludesOption(SymbolDisplayMethodOptions.IncludeParameters))
		{
			AddPunctuation(SyntaxKind.OpenParenToken);
			AddAttributeArgumentsIfRequired(symbol);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
	}

	public override void VisitOption(OptionSymbol symbol)
	{
		AddIdentifierName(SymbolDisplayPartKind.OptionName, symbol, symbol.Name);
	}

	public override void VisitEnumValue(EnumValueSymbol symbol)
	{
		AddLiteralValue("Enum Value");
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.EnumValueName, symbol, symbol.Name);
	}

	public override void VisitDotNetAssembly(DotNetAssemblySymbol symbol)
	{
		AddLiteralValue(".NET Assembly");
		AddSpace();
		if (symbol.AssemblyInfo == null)
		{
			AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.Name);
		}
		else
		{
			AddIdentifierName(SymbolDisplayPartKind.ControlName, symbol, symbol.AssemblyInfo.FullName);
		}
	}

	public override void VisitDotNetTypeDeclaration(DotNetTypeDeclarationSymbol symbol)
	{
		AddLiteralValue(".NET Type");
		AddSpace();
		AddIdentifierName(SymbolDisplayPartKind.TypeName, symbol, symbol.TypeName);
		if (!string.Equals(symbol.TypeName, symbol.Name, StringComparison.OrdinalIgnoreCase))
		{
			AddSpace();
			AddLiteralValue("as");
			AddSpace();
			AddIdentifierName(SymbolDisplayPartKind.TypeName, symbol, symbol.Name);
		}
	}

	public override void VisitAttributeArgument(AttributeArgumentSymbol symbol)
	{
		bool flag = format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeName);
		bool flag2 = format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeType);
		if (flag)
		{
			AddLiteralValue(symbol.Name);
		}
		if (flag & flag2)
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
		}
		if (flag2 && symbol.Type != null)
		{
			symbol.Type.Accept(NotFirstVisitor);
		}
	}

	private void AddAttributeArgumentsIfRequired(AttributeSymbol symbol)
	{
		if (format.ParameterOptions == SymbolDisplayParameterOptions.None)
		{
			return;
		}
		ImmutableArray<AttributeArgumentSymbol> signatureArguments = symbol.SignatureArguments;
		for (int i = 0; i < signatureArguments.Length; i++)
		{
			AttributeArgumentSymbol attributeArgumentSymbol = signatureArguments[i];
			if (i > 0)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
			attributeArgumentSymbol.Accept(NotFirstVisitor);
		}
	}

	private void VisitTypedSymbol(ISymbolWithType symbol, Func<TypeSymbol> getTypeOverride = null)
	{
		bool flag = format.TypedSymbolOptions.IncludesOption(SymbolDisplayTypedSymbolOptions.IncludeName);
		bool flag2 = format.TypedSymbolOptions.IncludesOption(SymbolDisplayTypedSymbolOptions.IncludeType);
		if (isFirstSymbolVisited && format.TypedSymbolOptions.IncludesOption(SymbolDisplayTypedSymbolOptions.IncludeSymbolKind))
		{
			AddLiteralValue(FormattableString.Invariant($"({GetSymbolKindDisplay(symbol)})"));
			AddSpace();
		}
		if (flag)
		{
			AddIdentifierName(SymbolDisplayPartKind.FieldName, (Symbol)symbol, symbol.Name);
		}
		if (flag & flag2)
		{
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
		}
		if (flag2)
		{
			TypeSymbol typeSymbol = symbol.Type;
			if (getTypeOverride != null)
			{
				typeSymbol = getTypeOverride();
			}
			typeSymbol?.Accept(NotFirstVisitor);
		}
	}

	private void VisitModuleSymbol(ModuleSymbol module)
	{
		AddLiteralValue(module.Name);
		AddPunctuation(SyntaxKind.CommaToken);
		AddSpace();
		AddLiteralValue(module.PublisherName);
		AddPunctuation(SyntaxKind.CommaToken);
		AddSpace();
		AddLiteralValue(module.Version.ToString());
	}

	[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
	private static string GetSymbolKindDisplay(ISymbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.QueryColumn:
		case SymbolKind.ReportColumn:
			return "column";
		case SymbolKind.QueryFilter:
			return "filter";
		case SymbolKind.QueryDataItem:
		case SymbolKind.ReportDataItem:
			return "dataitem";
		default:
			return symbol.Kind.ToString().ToLowerInvariant();
		}
	}
}
