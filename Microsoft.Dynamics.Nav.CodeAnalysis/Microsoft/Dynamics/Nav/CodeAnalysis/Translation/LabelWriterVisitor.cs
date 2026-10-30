using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

internal class LabelWriterVisitor : SymbolVisitor
{
	private XmlWriter Writer { get; }

	private ILanguageFileOutputter Outputter { get; }

	public LabelWriterVisitor(XmlWriter writer, ILanguageFileOutputter outputter)
	{
		Writer = writer;
		Outputter = outputter;
	}

	public override void DefaultVisit(Symbol symbol)
	{
		if (ShouldSymbolBeVisited(symbol))
		{
			VisitSymbolWithProperties(symbol);
			VisitMembersOfSymbol(symbol);
		}
	}

	private static bool ShouldSymbolBeVisited(Symbol symbol)
	{
		if (!symbol.IsSourceSymbol())
		{
			if (symbol.Kind == SymbolKind.Module)
			{
				return symbol is SourceModuleSymbol;
			}
			return false;
		}
		return true;
	}

	public override void VisitReportLabel(ReportLabelSymbol symbol)
	{
		if (symbol.Kind == SymbolKind.ReportLabel && !symbol.IsMultilanguageReportLabel)
		{
			SourceReportLabelSymbol sourceReportLabelSymbol = (SourceReportLabelSymbol)symbol;
			TranslationItem translationItem = CreateUntranslatedTranslationItem(sourceReportLabelSymbol, sourceReportLabelSymbol, sourceReportLabelSymbol?.BoundLabel);
			Outputter.WriteLabel(Writer, translationItem);
		}
	}

	public override void VisitVariable(VariableSymbol symbol)
	{
		if (symbol.Type.NavTypeKind == NavTypeKind.Label && IsNotOnCodeunitTest(symbol))
		{
			SourceLabelTypeSymbol sourceLabelTypeSymbol = (SourceLabelTypeSymbol)symbol.Type;
			TranslationItem translationItem = CreateUntranslatedTranslationItem(sourceLabelTypeSymbol, symbol, sourceLabelTypeSymbol?.BoundLabel);
			Outputter.WriteLabel(Writer, translationItem);
		}
	}

	internal static bool IsNotOnCodeunitTest(VariableSymbol symbol)
	{
		if (symbol.ContainingType.Kind == SymbolKind.Codeunit)
		{
			return ((ICodeunitTypeSymbol)symbol.ContainingType).Subtype != CodeunitSubtypeKind.Test;
		}
		return true;
	}

	public override void VisitMethod(MethodSymbol node)
	{
		foreach (VariableSymbol localVariable in node.LocalVariables)
		{
			VisitVariable(localVariable);
		}
	}

	private void VisitSymbolWithProperties(Symbol symbolWithProperties)
	{
		bool flag = false;
		foreach (PropertySymbol property in symbolWithProperties.Properties)
		{
			if (property.PropertyKind.IsTranslatableProperty())
			{
				flag = VisitTranslatableProperty(property, flag);
			}
		}
		if (!flag && symbolWithProperties.Kind == SymbolKind.Field)
		{
			TranslationItem translationItem = CreateUntranslatedTranslationItem(symbolWithProperties, symbolWithProperties, null);
			Outputter.WriteLabel(Writer, translationItem);
		}
	}

	private bool VisitTranslatableProperty(PropertySymbol p, bool hasCaption)
	{
		if (p.PropertyKind == PropertyKind.Caption || p.PropertyKind == PropertyKind.CaptionML)
		{
			hasCaption = true;
		}
		if (p.Property.PropertyValue is BoundLabelPropertyValue boundLabelPropertyValue)
		{
			TranslationItem translationItem = CreateUntranslatedTranslationItem(p, p.ContainingSymbol, boundLabelPropertyValue?.BoundLabel);
			Outputter.WriteLabel(Writer, translationItem);
		}
		return hasCaption;
	}

	private static TranslationItem CreateUntranslatedTranslationItem(Symbol symbol, Symbol namedIdSymbol, BoundLabel label)
	{
		string languageSymbolId = LanguageFileUtilities.GetLanguageSymbolId(symbol, shouldHashName: true, label == null);
		string languageSymbolId2 = LanguageFileUtilities.GetLanguageSymbolId(symbol, shouldHashName: false, label == null);
		string itemTypeFromSymbol = LanguageFileUtilities.GetItemTypeFromSymbol(namedIdSymbol);
		string sourceString = label?.Text?.StringValue ?? symbol.Name;
		return TranslationItem.CreateUntranslated(languageSymbolId, languageSymbolId2, sourceString, symbol.GetLocation(), label?.MaxLength?.Int32Value, label?.Locked?.BooleanValue, label?.Comment?.StringValue, itemTypeFromSymbol);
	}

	private void VisitMembersOfSymbol(Symbol symbol)
	{
		ContainerSymbol containerSymbol = symbol as ContainerSymbol;
		if (containerSymbol != null)
		{
			foreach (Symbol member in containerSymbol.GetMembers())
			{
				member.Accept(this);
			}
		}
	}
}
