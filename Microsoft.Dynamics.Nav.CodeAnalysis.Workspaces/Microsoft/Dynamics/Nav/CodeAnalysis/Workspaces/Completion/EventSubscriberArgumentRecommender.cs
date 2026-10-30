using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class EventSubscriberArgumentRecommender : TextRecommender
{
	private const string OptionAccesser = "::";

	private const string EmptyQuotes = "''";

	private const string DatabaseKeyword = "Database";

	protected internal override async Task<IEnumerable<CompletionItem>> RecommendTextAsync(CompletionContext context, MemberSyntaxContext syntaxContext, CancellationToken cancellationToken)
	{
		if (!syntaxContext.General.HasFlag(GeneralContexts.AttributeArgumentList))
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken);
		}
		AttributeArgumentListSyntax attributeArgumentListSyntax = MemberSyntaxContext.FindAttributeArgumentListContext(syntaxContext.TargetToken);
		AttributeSymbol attributeSymbol = (AttributeSymbol)syntaxContext.SemanticModel.GetDeclaredSymbol(attributeArgumentListSyntax.Parent);
		if ((object)attributeSymbol == null || attributeSymbol.AttributeInfo?.Kind != AttributeKind.EventSubscriber)
		{
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken);
		}
		int num = attributeArgumentListSyntax.CalculateAttributeArgumentPosition(syntaxContext.Position);
		switch (num)
		{
		case 0:
			return GetObjectTypeSuggestions(attributeSymbol);
		case 1:
			return GetObjectIdSuggestions(attributeSymbol);
		default:
			return await base.RecommendTextAsync(context, syntaxContext, cancellationToken);
		case 2:
		case 3:
		{
			if (attributeSymbol.Arguments.Length < 2)
			{
				return await base.RecommendTextAsync(context, syntaxContext, cancellationToken);
			}
			AttributeArgumentSymbol attributeArgumentSymbol = attributeSymbol.Arguments[0];
			SourceAttributeArgumentSymbol sourceAttributeArgumentSymbol = (SourceAttributeArgumentSymbol)attributeSymbol.Arguments[1];
			if (!Enum.TryParse<SymbolKind>(attributeArgumentSymbol.ValueText, out var result))
			{
				return await base.RecommendTextAsync(context, syntaxContext, cancellationToken);
			}
			int id;
			if (sourceAttributeArgumentSymbol.ValueAsInteger.HasValue)
			{
				id = sourceAttributeArgumentSymbol.ValueAsInteger.Value;
			}
			else
			{
				if (!(sourceAttributeArgumentSymbol.ValueAsSymbol != null))
				{
					return await base.RecommendTextAsync(context, syntaxContext, cancellationToken);
				}
				id = sourceAttributeArgumentSymbol.ValueAsSymbol.Id;
			}
			IEnumerable<CompletionItem> result2 = new List<CompletionItem>();
			ImmutableArray<ISymbolWithId> objectSymbolsByIdAcrossModules = syntaxContext.SemanticModel.Compilation.GetObjectSymbolsByIdAcrossModules(result, id);
			if (objectSymbolsByIdAcrossModules.Length == 1)
			{
				ApplicationObjectTypeSymbol applicationObjectTypeSymbol = (ApplicationObjectTypeSymbol)objectSymbolsByIdAcrossModules[0];
				TableTypeSymbol table = applicationObjectTypeSymbol as TableTypeSymbol;
				PageTypeSymbol pageTypeSymbol = applicationObjectTypeSymbol as PageTypeSymbol;
				if (pageTypeSymbol != null)
				{
					table = pageTypeSymbol.RelatedTable;
				}
				bool fieldFilter = ShouldFilterByFields(syntaxContext, table, attributeSymbol);
				bool controlFilter = ShouldFilterByControls(pageTypeSymbol, attributeSymbol);
				bool actionFilter = ShouldFilterByActions(syntaxContext, pageTypeSymbol, attributeSymbol);
				switch (num)
				{
				case 2:
					result2 = GetEventNameSuggestions(syntaxContext, applicationObjectTypeSymbol, fieldFilter, controlFilter, actionFilter);
					break;
				case 3:
				{
					if (CanCompleteElementName(syntaxContext, applicationObjectTypeSymbol, attributeSymbol, out var eventInfo))
					{
						result2 = GetElementNameSuggestions(syntaxContext, table, pageTypeSymbol, eventInfo);
						break;
					}
					CompletionItem item = CreateItem("''", Glyph.Field);
					return new List<CompletionItem> { item };
				}
				}
			}
			return result2;
		}
		}
	}

	private static IEnumerable<CompletionItem> GetObjectTypeSuggestions(AttributeSymbol eventSubscriber)
	{
		if (eventSubscriber.Arguments.Length >= 1 && eventSubscriber.Arguments[0].Type.NavTypeKind == NavTypeKind.ObjectType)
		{
			return new List<CompletionItem>();
		}
		string name = NavCorLib.ObjectTypeType.Name;
		return new List<CompletionItem> { CreateItem(name, Glyph.Option, name + "::") };
	}

	private static IEnumerable<CompletionItem> GetObjectIdSuggestions(AttributeSymbol eventSubscriber)
	{
		List<CompletionItem> result = new List<CompletionItem>
		{
			CreateObjectIdItem(NavTypeKind.Codeunit),
			CreateObjectIdItem("Database"),
			CreateObjectIdItem(NavTypeKind.Page),
			CreateObjectIdItem(NavTypeKind.Query),
			CreateObjectIdItem(NavTypeKind.Report),
			CreateObjectIdItem(NavTypeKind.XmlPort)
		};
		if (eventSubscriber.Arguments.Length == 1)
		{
			return result;
		}
		if (eventSubscriber.Arguments.Length >= 2)
		{
			NavTypeKind navTypeKind = eventSubscriber.Arguments[1].Type.NavTypeKind;
			if (navTypeKind == NavTypeKind.String || navTypeKind == NavTypeKind.Boolean)
			{
				return result;
			}
		}
		return new List<CompletionItem>();
	}

	private IEnumerable<CompletionItem> GetEventNameSuggestions(MemberSyntaxContext syntaxContext, ApplicationObjectTypeSymbol applicationObject, bool fieldFilter, bool controlFilter, bool actionFilter)
	{
		ImmutableArray<BuiltInTriggerEventSymbol>? immutableArray = (applicationObject as ISymbolWithTriggerEvents)?.TriggerEvents;
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		try
		{
			if (fieldFilter)
			{
				instance.AddRange(immutableArray.Value.Where((BuiltInTriggerEventSymbol x) => x.Info.ElementType == SymbolKind.Field));
			}
			else if (controlFilter)
			{
				instance.AddRange(immutableArray.Value.Where((BuiltInTriggerEventSymbol x) => x.Info.ElementType == SymbolKind.Control));
			}
			else if (actionFilter)
			{
				instance.AddRange(immutableArray.Value.Where((BuiltInTriggerEventSymbol x) => x.Info.ElementType == SymbolKind.Action));
			}
			else
			{
				if (immutableArray != null)
				{
					instance.AddRange(immutableArray);
				}
				instance.AddRange(applicationObject.QueryAllEvents(syntaxContext.SemanticModel.Compilation));
			}
			return from x in instance.ToArray()
				select CreateItem(x.Name.QuoteLiteral(), Glyph.Method);
		}
		finally
		{
			instance.Free();
		}
	}

	private IEnumerable<CompletionItem> GetElementNameSuggestions(MemberSyntaxContext syntaxContext, TableTypeSymbol table, PageTypeSymbol page, TriggerEventTypeInfo eventInfo)
	{
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		try
		{
			if (eventInfo == null)
			{
				if (table != null)
				{
					instance.AddRange(from x in table.QueryAllFields(syntaxContext.SemanticModel.Compilation)
						where x.IsKind(SymbolKind.Field)
						select x);
				}
				if (page != null)
				{
					instance.AddRange(from x in page.QueryAllActions(syntaxContext.SemanticModel.Compilation)
						where x.ActionKind == ActionKind.Action
						select x);
				}
			}
			else if (eventInfo.ElementType == SymbolKind.Field && table != null)
			{
				instance.AddRange(from x in table.QueryAllFields(syntaxContext.SemanticModel.Compilation)
					where x.IsKind(SymbolKind.Field)
					select x);
			}
			else if (eventInfo.ElementType == SymbolKind.Control && page != null)
			{
				instance.AddRange(from x in page.QueryAllControls(syntaxContext.SemanticModel.Compilation)
					where x.ControlKind == ControlKind.Field
					select x);
			}
			else if (eventInfo.ElementType == SymbolKind.Action && page != null)
			{
				instance.AddRange(from x in page.QueryAllActions(syntaxContext.SemanticModel.Compilation)
					where x.ActionKind == ActionKind.Action
					select x);
			}
			return from x in instance.ToArray()
				select CreateItem(x.Name.QuoteLiteral(), Glyph.Field);
		}
		finally
		{
			instance.Free();
		}
	}

	private bool ShouldFilterByFields(MemberSyntaxContext syntaxContext, TableTypeSymbol table, AttributeSymbol eventSubscriber)
	{
		if (table != null && eventSubscriber.Arguments.Length > 3)
		{
			AttributeArgumentSymbol argument = eventSubscriber.Arguments[3];
			argument = AdjustElementNamePosition(argument, eventSubscriber);
			if (string.IsNullOrEmpty(argument.ValueText))
			{
				return false;
			}
			return (from x in table.QueryAllFields(argument.ValueText, syntaxContext.SemanticModel.Compilation)
				where x.IsKind(SymbolKind.Field)
				select x).Count() == 1;
		}
		return false;
	}

	private static bool ShouldFilterByControls(PageTypeSymbol page, AttributeSymbol eventSubscriber)
	{
		if (page != null && eventSubscriber.Arguments.Length > 3)
		{
			AttributeArgumentSymbol fieldArgument = eventSubscriber.Arguments[3];
			fieldArgument = AdjustElementNamePosition(fieldArgument, eventSubscriber);
			if (string.IsNullOrEmpty(fieldArgument.ValueText))
			{
				return false;
			}
			return page.FlattenedControls.Where((ControlSymbol x) => SemanticFacts.IsSameName(x.Name, fieldArgument.ValueText) && x.ControlKind == ControlKind.Field).Count() == 1;
		}
		return false;
	}

	private bool ShouldFilterByActions(MemberSyntaxContext syntaxContext, PageTypeSymbol page, AttributeSymbol eventSubscriber)
	{
		if (page != null && eventSubscriber.Arguments.Length > 3)
		{
			AttributeArgumentSymbol argument = eventSubscriber.Arguments[3];
			argument = AdjustElementNamePosition(argument, eventSubscriber);
			if (string.IsNullOrEmpty(argument.ValueText))
			{
				return false;
			}
			return (from x in page.QueryAllActions(argument.ValueText, syntaxContext.SemanticModel.Compilation)
				where x.ActionKind == ActionKind.Action
				select x).Count() == 1;
		}
		return false;
	}

	private static AttributeArgumentSymbol AdjustElementNamePosition(AttributeArgumentSymbol argument, AttributeSymbol eventSubscriber)
	{
		if (argument.Type.NavTypeKind != NavTypeKind.String)
		{
			argument = eventSubscriber.Arguments[2];
		}
		return argument;
	}

	private bool CanCompleteElementName(MemberSyntaxContext syntaxContext, ApplicationObjectTypeSymbol applicationObject, AttributeSymbol eventSubscriber, out TriggerEventTypeInfo eventInfo)
	{
		eventInfo = null;
		if (eventSubscriber.Arguments.Length > 2)
		{
			AttributeArgumentSymbol eventArgument = eventSubscriber.Arguments[2];
			if (string.IsNullOrEmpty(eventArgument.ValueText))
			{
				return true;
			}
			ref TriggerEventTypeInfo reference = ref eventInfo;
			object obj;
			if (applicationObject.IsKind(SymbolKind.Page))
			{
				obj = TriggerEventDefinitions.GetPageTriggerEventInfo(eventArgument.ValueText);
			}
			else
			{
				obj = (applicationObject.IsKind(SymbolKind.Table) ? TriggerEventDefinitions.GetTableTriggerEventInfo(eventArgument.ValueText) : null);
			}
			reference = (TriggerEventTypeInfo)obj;
			TriggerEventTypeInfo obj2 = eventInfo;
			if (obj2 != null && obj2.ElementType == SymbolKind.Undefined)
			{
				return false;
			}
			TriggerEventTypeInfo obj3 = eventInfo;
			if (obj3 != null && obj3.ElementType == SymbolKind.Control)
			{
				PageTypeSymbol obj4 = applicationObject as PageTypeSymbol;
				if ((object)obj4 == null)
				{
					return true;
				}
				return Enumerable.Count(obj4.FlattenedControls, (ControlSymbol c) => c.ControlKind == ControlKind.Field && SemanticFacts.IsSameName(eventArgument.ValueText, c.Name)) != 1;
			}
			return applicationObject.QueryAllEvents(eventArgument.ValueText, syntaxContext.SemanticModel.Compilation).Count() != 1;
		}
		return false;
	}

	private static CompletionItem CreateObjectIdItem(object kind)
	{
		return CreateItem(kind, Glyph.Option, string.Concat(kind, "::"));
	}

	private static CompletionItem CreateItem(object displayText, Glyph glyph, string insertionText = null)
	{
		return CommonCompletionItem.Create(displayText.ToString(), default, insertionText: insertionText ?? displayText.ToString(), glyph: glyph);
	}
}
