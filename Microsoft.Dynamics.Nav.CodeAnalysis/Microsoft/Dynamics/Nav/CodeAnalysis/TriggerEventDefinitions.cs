using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class TriggerEventDefinitions
{
	private static ImmutableDictionary<string, TriggerEventTypeInfo> lazyTableTriggerEvents;

	private static ImmutableDictionary<string, TriggerEventTypeInfo> lazyPageTriggerEvents;

	internal static ImmutableDictionary<string, TriggerEventTypeInfo> TableTriggerEvents
	{
		get
		{
			if (lazyTableTriggerEvents == null)
			{
				PooledDictionary<string, TriggerEventTypeInfo> instance = PooledDictionary<string, TriggerEventTypeInfo>.GetInstance();
				instance.Add("ONAFTERDELETEEVENT", new TriggerEventTypeInfo("Table", "OnAfterDeleteEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("RunTrigger", NavCorLib.BooleanType, isVar: false)));
				instance.Add("ONAFTERINSERTEVENT", new TriggerEventTypeInfo("Table", "OnAfterInsertEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("RunTrigger", NavCorLib.BooleanType, isVar: false)));
				instance.Add("ONAFTERMODIFYEVENT", new TriggerEventTypeInfo("Table", "OnAfterModifyEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true), new TriggerParameter("RunTrigger", NavCorLib.BooleanType, isVar: false)));
				instance.Add("ONAFTERRENAMEEVENT", new TriggerEventTypeInfo("Table", "OnAfterRenameEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true), new TriggerParameter("RunTrigger", NavCorLib.BooleanType, isVar: false)));
				instance.Add("ONAFTERVALIDATEEVENT", new TriggerEventTypeInfo("Table", "OnAfterValidateEvent", SymbolKind.Field, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true), new TriggerParameter("CurrFieldNo", NavCorLib.IntegerType, isVar: false)));
				instance.Add("ONBEFOREDELETEEVENT", new TriggerEventTypeInfo("Table", "OnBeforeDeleteEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("RunTrigger", NavCorLib.BooleanType, isVar: false)));
				instance.Add("ONBEFOREINSERTEVENT", new TriggerEventTypeInfo("Table", "OnBeforeInsertEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("RunTrigger", NavCorLib.BooleanType, isVar: false)));
				instance.Add("ONBEFOREMODIFYEVENT", new TriggerEventTypeInfo("Table", "OnBeforeModifyEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true), new TriggerParameter("RunTrigger", NavCorLib.BooleanType, isVar: false)));
				instance.Add("ONBEFORERENAMEEVENT", new TriggerEventTypeInfo("Table", "OnBeforeRenameEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true), new TriggerParameter("RunTrigger", NavCorLib.BooleanType, isVar: false)));
				instance.Add("ONBEFOREVALIDATEEVENT", new TriggerEventTypeInfo("Table", "OnBeforeValidateEvent", SymbolKind.Field, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true), new TriggerParameter("CurrFieldNo", NavCorLib.IntegerType, isVar: false)));
				lazyTableTriggerEvents = instance.ToImmutableDictionaryAndFree();
			}
			return lazyTableTriggerEvents;
		}
	}

	internal static ImmutableDictionary<string, TriggerEventTypeInfo> PageTriggerEvents
	{
		get
		{
			if (lazyPageTriggerEvents == null)
			{
				PooledDictionary<string, TriggerEventTypeInfo> instance = PooledDictionary<string, TriggerEventTypeInfo>.GetInstance();
				instance.Add("ONAFTERACTIONEVENT", new TriggerEventTypeInfo("Page", "OnAfterActionEvent", SymbolKind.Action, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true)));
				instance.Add("ONAFTERGETCURRRECORDEVENT", new TriggerEventTypeInfo("Page", "OnAfterGetCurrRecordEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true)));
				instance.Add("ONAFTERGETRECORDEVENT", new TriggerEventTypeInfo("Page", "OnAfterGetRecordEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true)));
				instance.Add("ONAFTERVALIDATEEVENT", new TriggerEventTypeInfo("Page", "OnAfterValidateEvent", SymbolKind.Control, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true)));
				instance.Add("ONBEFOREACTIONEVENT", new TriggerEventTypeInfo("Page", "OnBeforeActionEvent", SymbolKind.Action, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true)));
				instance.Add("ONBEFOREVALIDATEEVENT", new TriggerEventTypeInfo("Page", "OnBeforeValidateEvent", SymbolKind.Control, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true)));
				instance.Add("ONCLOSEPAGEEVENT", new TriggerEventTypeInfo("Page", "OnClosePageEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true)));
				instance.Add("ONDELETERECORDEVENT", new TriggerEventTypeInfo("Page", "OnDeleteRecordEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("AllowDelete", NavCorLib.BooleanType, isVar: true)));
				instance.Add("ONINSERTRECORDEVENT", new TriggerEventTypeInfo("Page", "OnInsertRecordEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("BelowxRec", NavCorLib.BooleanType, isVar: false), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true), new TriggerParameter("AllowInsert", NavCorLib.BooleanType, isVar: true)));
				instance.Add("ONMODIFYRECORDEVENT", new TriggerEventTypeInfo("Page", "OnModifyRecordEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true), new TriggerParameter("AllowModify", NavCorLib.BooleanType, isVar: true)));
				instance.Add("ONNEWRECORDEVENT", new TriggerEventTypeInfo("Page", "OnNewRecordEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("BelowxRec", NavCorLib.BooleanType, isVar: false), new TriggerParameter("xRec", NavCorLib.RecordType, isVar: true)));
				instance.Add("ONOPENPAGEEVENT", new TriggerEventTypeInfo("Page", "OnOpenPageEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true)));
				instance.Add("ONQUERYCLOSEPAGEEVENT", new TriggerEventTypeInfo("Page", "OnQueryClosePageEvent", SymbolKind.Undefined, new TriggerParameter("Rec", NavCorLib.RecordType, isVar: true), new TriggerParameter("AllowClose", NavCorLib.BooleanType, isVar: true)));
				lazyPageTriggerEvents = instance.ToImmutableDictionaryAndFree();
			}
			return lazyPageTriggerEvents;
		}
	}

	internal static bool IsValidTableTriggerEvent(string name)
	{
		return TableTriggerEvents.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerEventTypeInfo GetTableTriggerEventInfo(string name)
	{
		TableTriggerEvents.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	internal static bool IsValidPageTriggerEvent(string name)
	{
		return PageTriggerEvents.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerEventTypeInfo GetPageTriggerEventInfo(string name)
	{
		PageTriggerEvents.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	internal static bool IsValidTriggerEvent(string name)
	{
		string key = name.ToUpperInvariant();
		if (!TableTriggerEvents.ContainsKey(key))
		{
			return PageTriggerEvents.ContainsKey(key);
		}
		return true;
	}
}
