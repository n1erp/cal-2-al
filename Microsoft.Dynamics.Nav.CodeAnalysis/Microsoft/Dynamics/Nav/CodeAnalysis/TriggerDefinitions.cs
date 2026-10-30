using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class TriggerDefinitions
{
	internal static ImmutableDictionary<string, TriggerTypeInfo> CodeunitTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> TableTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> TableExtensionTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> FieldTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> FieldExtensionTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> PageTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> RequestPageTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> PageExtensionTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> ControlTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> ControlExtensionTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> ActionTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> ActionExtensionTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> ReportTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> ReportDataItemTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> XmlPortTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> XmlPortTableElementTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> XmlPortFieldElementTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> XmlPortTextElementTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> XmlPortFieldAttributeTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> XmlPortTextAttributeTriggers { get; }

	internal static ImmutableDictionary<string, TriggerTypeInfo> QueryTriggers { get; }

	static TriggerDefinitions()
	{
		CodeunitTriggers = GetCodeunitTriggers();
		TableTriggers = GetTableTriggers();
		TableExtensionTriggers = GetTableExtensionTriggers();
		FieldTriggers = GetFieldTriggers();
		FieldExtensionTriggers = GetFieldExtensionTriggers();
		PageTriggers = GetPageTriggers();
		RequestPageTriggers = GetRequestPageTriggers();
		PageExtensionTriggers = GetPageExtensionTriggers();
		ControlTriggers = GetControlTriggers();
		ControlExtensionTriggers = GetControlExtensionTriggers();
		ActionTriggers = GetActionTriggers();
		ActionExtensionTriggers = GetActionExtensionTriggers();
		ReportTriggers = GetReportTriggers();
		ReportDataItemTriggers = GetReportDataItemTriggers();
		XmlPortTriggers = GetXmlPortTriggers();
		XmlPortTableElementTriggers = GetXmlPortTableElementTriggers();
		XmlPortFieldElementTriggers = GetXmlPortFieldElementTriggers();
		XmlPortTextElementTriggers = GetXmlPortTextElementTriggers();
		XmlPortFieldAttributeTriggers = GetXmlPortFieldAttributeTriggers();
		XmlPortTextAttributeTriggers = GetXmlPortTextAttributeTriggers();
		QueryTriggers = GetQueryTriggers();
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetCodeunitTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONRUN", new TriggerTypeInfo("Codeunit", "OnRun", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONCHECKPRECONDITIONSPERDATABASE", new TriggerTypeInfo("Codeunit", "OnCheckPreconditionsPerDatabase", null, NavCorLib.NoneType, null, ImmutableArray.Create(new DependentProperty("Subtype(Upgrade)")), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONCHECKPRECONDITIONSPERCOMPANY", new TriggerTypeInfo("Codeunit", "OnCheckPreconditionsPerCompany", null, NavCorLib.NoneType, null, ImmutableArray.Create(new DependentProperty("Subtype(Upgrade)")), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONUPGRADEPERDATABASE", new TriggerTypeInfo("Codeunit", "OnUpgradePerDatabase", null, NavCorLib.NoneType, null, ImmutableArray.Create(new DependentProperty("Subtype(Upgrade)")), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONUPGRADEPERCOMPANY", new TriggerTypeInfo("Codeunit", "OnUpgradePerCompany", null, NavCorLib.NoneType, null, ImmutableArray.Create(new DependentProperty("Subtype(Upgrade)")), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONVALIDATEUPGRADEPERDATABASE", new TriggerTypeInfo("Codeunit", "OnValidateUpgradePerDatabase", null, NavCorLib.NoneType, null, ImmutableArray.Create(new DependentProperty("Subtype(Upgrade)")), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONVALIDATEUPGRADEPERCOMPANY", new TriggerTypeInfo("Codeunit", "OnValidateUpgradePerCompany", null, NavCorLib.NoneType, null, ImmutableArray.Create(new DependentProperty("Subtype(Upgrade)")), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONINSTALLAPPPERDATABASE", new TriggerTypeInfo("Codeunit", "OnInstallAppPerDatabase", null, NavCorLib.NoneType, null, ImmutableArray.Create(new DependentProperty("Subtype(Install)")), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONINSTALLAPPPERCOMPANY", new TriggerTypeInfo("Codeunit", "OnInstallAppPerCompany", null, NavCorLib.NoneType, null, ImmutableArray.Create(new DependentProperty("Subtype(Install)")), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFORETESTRUN", new TriggerTypeInfo("Codeunit", "OnBeforeTestRun", null, NavCorLib.BooleanType, "true", ImmutableArray.Create(new DependentProperty("Subtype(TestRunner)")), ImmutableArray.Create(new TriggerParameter("CodeunitId", NavCorLib.IntegerType, isVar: false), new TriggerParameter("CodeunitName", NavCorLib.TextType, isVar: false), new TriggerParameter("FunctionName", NavCorLib.TextType, isVar: false), new TriggerParameter("Permissions", NavCorLib.TestPermissionsType, isVar: false))));
			instance.Add("ONAFTERTESTRUN", new TriggerTypeInfo("Codeunit", "OnAfterTestRun", null, NavCorLib.NoneType, null, ImmutableArray.Create(new DependentProperty("Subtype(TestRunner)")), ImmutableArray.Create<TriggerParameter>(new TriggerParameter("CodeunitId", NavCorLib.IntegerType, isVar: false), new TriggerParameter("CodeunitName", NavCorLib.TextType, isVar: false), new TriggerParameter("FunctionName", NavCorLib.TextType, isVar: false), new TriggerParameter("Permissions", NavCorLib.TestPermissionsType, isVar: false), new TriggerParameter("Success", NavCorLib.BooleanType, isVar: false))));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidCodeunitTrigger(string name)
	{
		return CodeunitTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetCodeunitTriggerInfo(string name)
	{
		CodeunitTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetTableTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONINSERT", new TriggerTypeInfo("Table", "OnInsert", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONMODIFY", new TriggerTypeInfo("Table", "OnModify", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONDELETE", new TriggerTypeInfo("Table", "OnDelete", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONRENAME", new TriggerTypeInfo("Table", "OnRename", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidTableTrigger(string name)
	{
		return TableTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetTableTriggerInfo(string name)
	{
		TableTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetTableExtensionTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONBEFOREINSERT", new TriggerTypeInfo("TableExtension", "OnBeforeInsert", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONINSERT", new TriggerTypeInfo("TableExtension", "OnInsert", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERINSERT", new TriggerTypeInfo("TableExtension", "OnAfterInsert", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFOREMODIFY", new TriggerTypeInfo("TableExtension", "OnBeforeModify", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONMODIFY", new TriggerTypeInfo("TableExtension", "OnModify", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERMODIFY", new TriggerTypeInfo("TableExtension", "OnAfterModify", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFOREDELETE", new TriggerTypeInfo("TableExtension", "OnBeforeDelete", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONDELETE", new TriggerTypeInfo("TableExtension", "OnDelete", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERDELETE", new TriggerTypeInfo("TableExtension", "OnAfterDelete", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFORERENAME", new TriggerTypeInfo("TableExtension", "OnBeforeRename", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONRENAME", new TriggerTypeInfo("TableExtension", "OnRename", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERRENAME", new TriggerTypeInfo("TableExtension", "OnAfterRename", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidTableExtensionTrigger(string name)
	{
		return TableExtensionTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetTableExtensionTriggerInfo(string name)
	{
		TableExtensionTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetFieldTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONVALIDATE", new TriggerTypeInfo("Field", "OnValidate", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONLOOKUP", new TriggerTypeInfo("Field", "OnLookup", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidFieldTrigger(string name)
	{
		return FieldTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetFieldTriggerInfo(string name)
	{
		FieldTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetFieldExtensionTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONBEFOREVALIDATE", new TriggerTypeInfo("FieldExtension", "OnBeforeValidate", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERVALIDATE", new TriggerTypeInfo("FieldExtension", "OnAfterValidate", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidFieldExtensionTrigger(string name)
	{
		return FieldExtensionTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetFieldExtensionTriggerInfo(string name)
	{
		FieldExtensionTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetPageTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONINIT", new TriggerTypeInfo("Page", "OnInit", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONOPENPAGE", new TriggerTypeInfo("Page", "OnOpenPage", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONCLOSEPAGE", new TriggerTypeInfo("Page", "OnClosePage", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONFINDRECORD", new TriggerTypeInfo("Page", "OnFindRecord", null, NavCorLib.BooleanType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("Which", NavCorLib.TextType, isVar: false))));
			instance.Add("ONNEXTRECORD", new TriggerTypeInfo("Page", "OnNextRecord", null, NavCorLib.IntegerType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("Steps", NavCorLib.IntegerType, isVar: false))));
			instance.Add("ONAFTERGETRECORD", new TriggerTypeInfo("Page", "OnAfterGetRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONNEWRECORD", new TriggerTypeInfo("Page", "OnNewRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("BelowxRec", NavCorLib.BooleanType, isVar: false))));
			instance.Add("ONINSERTRECORD", new TriggerTypeInfo("Page", "OnInsertRecord", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("BelowxRec", NavCorLib.BooleanType, isVar: false))));
			instance.Add("ONMODIFYRECORD", new TriggerTypeInfo("Page", "OnModifyRecord", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONDELETERECORD", new TriggerTypeInfo("Page", "OnDeleteRecord", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONQUERYCLOSEPAGE", new TriggerTypeInfo("Page", "OnQueryClosePage", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("CloseAction", NavCorLib.ActionType, isVar: false))));
			instance.Add("ONAFTERGETCURRRECORD", new TriggerTypeInfo("Page", "OnAfterGetCurrRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidPageTrigger(string name)
	{
		return PageTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetPageTriggerInfo(string name)
	{
		PageTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetRequestPageTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONINIT", new TriggerTypeInfo("RequestPage", "OnInit", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONOPENPAGE", new TriggerTypeInfo("RequestPage", "OnOpenPage", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONCLOSEPAGE", new TriggerTypeInfo("RequestPage", "OnClosePage", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONFINDRECORD", new TriggerTypeInfo("RequestPage", "OnFindRecord", null, NavCorLib.BooleanType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("Which", NavCorLib.TextType, isVar: false))));
			instance.Add("ONNEXTRECORD", new TriggerTypeInfo("RequestPage", "OnNextRecord", null, NavCorLib.IntegerType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("Steps", NavCorLib.IntegerType, isVar: false))));
			instance.Add("ONAFTERGETRECORD", new TriggerTypeInfo("RequestPage", "OnAfterGetRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONNEWRECORD", new TriggerTypeInfo("RequestPage", "OnNewRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("BelowxRec", NavCorLib.BooleanType, isVar: false))));
			instance.Add("ONINSERTRECORD", new TriggerTypeInfo("RequestPage", "OnInsertRecord", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("BelowxRec", NavCorLib.BooleanType, isVar: false))));
			instance.Add("ONMODIFYRECORD", new TriggerTypeInfo("RequestPage", "OnModifyRecord", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONDELETERECORD", new TriggerTypeInfo("RequestPage", "OnDeleteRecord", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONQUERYCLOSEPAGE", new TriggerTypeInfo("RequestPage", "OnQueryClosePage", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("CloseAction", NavCorLib.ActionType, isVar: false))));
			instance.Add("ONAFTERGETCURRRECORD", new TriggerTypeInfo("RequestPage", "OnAfterGetCurrRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidRequestPageTrigger(string name)
	{
		return RequestPageTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetRequestPageTriggerInfo(string name)
	{
		RequestPageTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetPageExtensionTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONOPENPAGE", new TriggerTypeInfo("PageExtension", "OnOpenPage", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONCLOSEPAGE", new TriggerTypeInfo("PageExtension", "OnClosePage", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERGETRECORD", new TriggerTypeInfo("PageExtension", "OnAfterGetRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONNEWRECORD", new TriggerTypeInfo("PageExtension", "OnNewRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("BelowxRec", NavCorLib.BooleanType, isVar: false))));
			instance.Add("ONINSERTRECORD", new TriggerTypeInfo("PageExtension", "OnInsertRecord", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("BelowxRec", NavCorLib.BooleanType, isVar: false))));
			instance.Add("ONMODIFYRECORD", new TriggerTypeInfo("PageExtension", "OnModifyRecord", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONDELETERECORD", new TriggerTypeInfo("PageExtension", "OnDeleteRecord", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONQUERYCLOSEPAGE", new TriggerTypeInfo("PageExtension", "OnQueryClosePage", null, NavCorLib.BooleanType, "true", ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("CloseAction", NavCorLib.ActionType, isVar: false))));
			instance.Add("ONAFTERGETCURRRECORD", new TriggerTypeInfo("PageExtension", "OnAfterGetCurrRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidPageExtensionTrigger(string name)
	{
		return PageExtensionTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetPageExtensionTriggerInfo(string name)
	{
		PageExtensionTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetControlTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONVALIDATE", new TriggerTypeInfo("Control", "OnValidate", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONLOOKUP", new TriggerTypeInfo("Control", "OnLookup", null, NavCorLib.BooleanType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("Text", NavCorLib.TextType, isVar: true))));
			instance.Add("ONDRILLDOWN", new TriggerTypeInfo("Control", "OnDrillDown", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONASSISTEDIT", new TriggerTypeInfo("Control", "OnAssistEdit", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONCONTROLADDIN", new TriggerTypeInfo("Control", "OnControlAddIn", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create(new TriggerParameter("Index", NavCorLib.IntegerType, isVar: false), new TriggerParameter("Data", NavCorLib.TextType, isVar: false))));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidControlTrigger(string name)
	{
		return ControlTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetControlTriggerInfo(string name)
	{
		ControlTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetControlExtensionTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONBEFOREVALIDATE", new TriggerTypeInfo("ControlExtension", "OnBeforeValidate", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERVALIDATE", new TriggerTypeInfo("ControlExtension", "OnAfterValidate", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONLOOKUP", new TriggerTypeInfo("ControlExtension", "OnLookup", null, NavCorLib.BooleanType, null, ImmutableArray.Create<DependentProperty>(), compatibility: VersionCompatibility.Parse("3.0"), parameters: ImmutableArray.Create(new TriggerParameter("Text", NavCorLib.TextType, isVar: true))));
			instance.Add("ONDRILLDOWN", new TriggerTypeInfo("ControlExtension", "OnDrillDown", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), compatibility: VersionCompatibility.Parse("3.0"), parameters: ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONASSISTEDIT", new TriggerTypeInfo("ControlExtension", "OnAssistEdit", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), compatibility: VersionCompatibility.Parse("3.0"), parameters: ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidControlExtensionTrigger(string name)
	{
		return ControlExtensionTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetControlExtensionTriggerInfo(string name)
	{
		ControlExtensionTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetActionTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONACTION", new TriggerTypeInfo("Action", "OnAction", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidActionTrigger(string name)
	{
		return ActionTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetActionTriggerInfo(string name)
	{
		ActionTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetActionExtensionTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONBEFOREACTION", new TriggerTypeInfo("ActionExtension", "OnBeforeAction", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERACTION", new TriggerTypeInfo("ActionExtension", "OnAfterAction", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidActionExtensionTrigger(string name)
	{
		return ActionExtensionTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetActionExtensionTriggerInfo(string name)
	{
		ActionExtensionTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetReportTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONINITREPORT", new TriggerTypeInfo("Report", "OnInitReport", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONPREREPORT", new TriggerTypeInfo("Report", "OnPreReport", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONPOSTREPORT", new TriggerTypeInfo("Report", "OnPostReport", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidReportTrigger(string name)
	{
		return ReportTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetReportTriggerInfo(string name)
	{
		ReportTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetReportDataItemTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONPREDATAITEM", new TriggerTypeInfo("ReportDataItem", "OnPreDataItem", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERGETRECORD", new TriggerTypeInfo("ReportDataItem", "OnAfterGetRecord", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONPOSTDATAITEM", new TriggerTypeInfo("ReportDataItem", "OnPostDataItem", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidReportDataItemTrigger(string name)
	{
		return ReportDataItemTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetReportDataItemTriggerInfo(string name)
	{
		ReportDataItemTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetXmlPortTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONINITXMLPORT", new TriggerTypeInfo("XmlPort", "OnInitXmlPort", "OnInitXMLport", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONPREXMLPORT", new TriggerTypeInfo("XmlPort", "OnPreXmlPort", "OnPreXMLport", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONPOSTXMLPORT", new TriggerTypeInfo("XmlPort", "OnPostXmlPort", "OnPostXMLport", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidXmlPortTrigger(string name)
	{
		return XmlPortTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetXmlPortTriggerInfo(string name)
	{
		XmlPortTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetXmlPortTableElementTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONAFTERINITRECORD", new TriggerTypeInfo("XmlPortTableElement", "OnAfterInitRecord", "Import::OnAfterInitRecord", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFOREINSERTRECORD", new TriggerTypeInfo("XmlPortTableElement", "OnBeforeInsertRecord", "Import::OnBeforeInsertRecord", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERINSERTRECORD", new TriggerTypeInfo("XmlPortTableElement", "OnAfterInsertRecord", "Import::OnAfterInsertRecord", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFOREMODIFYRECORD", new TriggerTypeInfo("XmlPortTableElement", "OnBeforeModifyRecord", "Import::OnBeforeModifyRecord", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERMODIFYRECORD", new TriggerTypeInfo("XmlPortTableElement", "OnAfterModifyRecord", "Import::OnAfterModifyRecord", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONPREXMLITEM", new TriggerTypeInfo("XmlPortTableElement", "OnPreXmlItem", "Export::OnPreXMLItem", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONAFTERGETRECORD", new TriggerTypeInfo("XmlPortTableElement", "OnAfterGetRecord", "Export::OnAfterGetRecord", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidXmlPortTableElementTrigger(string name)
	{
		return XmlPortTableElementTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetXmlPortTableElementTriggerInfo(string name)
	{
		XmlPortTableElementTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetXmlPortFieldElementTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONAFTERASSIGNFIELD", new TriggerTypeInfo("XmlPortFieldElement", "OnAfterAssignField", "Import::OnAfterAssignField", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFOREPASSFIELD", new TriggerTypeInfo("XmlPortFieldElement", "OnBeforePassField", "Export::OnBeforePassField", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidXmlPortFieldElementTrigger(string name)
	{
		return XmlPortFieldElementTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetXmlPortFieldElementTriggerInfo(string name)
	{
		XmlPortFieldElementTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetXmlPortTextElementTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONAFTERASSIGNVARIABLE", new TriggerTypeInfo("XmlPortTextElement", "OnAfterAssignVariable", "Import::OnAfterAssignVariable", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFOREPASSVARIABLE", new TriggerTypeInfo("XmlPortTextElement", "OnBeforePassVariable", "Export::OnBeforePassVariable", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidXmlPortTextElementTrigger(string name)
	{
		return XmlPortTextElementTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetXmlPortTextElementTriggerInfo(string name)
	{
		XmlPortTextElementTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetXmlPortFieldAttributeTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONAFTERASSIGNFIELD", new TriggerTypeInfo("XmlPortFieldAttribute", "OnAfterAssignField", "Import::OnAfterAssignField", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFOREPASSFIELD", new TriggerTypeInfo("XmlPortFieldAttribute", "OnBeforePassField", "Export::OnBeforePassField", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidXmlPortFieldAttributeTrigger(string name)
	{
		return XmlPortFieldAttributeTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetXmlPortFieldAttributeTriggerInfo(string name)
	{
		XmlPortFieldAttributeTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetXmlPortTextAttributeTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONAFTERASSIGNVARIABLE", new TriggerTypeInfo("XmlPortTextAttribute", "OnAfterAssignVariable", "Import::OnAfterAssignVariable", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			instance.Add("ONBEFOREPASSVARIABLE", new TriggerTypeInfo("XmlPortTextAttribute", "OnBeforePassVariable", "Export::OnBeforePassVariable", NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidXmlPortTextAttributeTrigger(string name)
	{
		return XmlPortTextAttributeTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetXmlPortTextAttributeTriggerInfo(string name)
	{
		XmlPortTextAttributeTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, TriggerTypeInfo> GetQueryTriggers()
	{
		PooledDictionary<string, TriggerTypeInfo> instance = PooledDictionary<string, TriggerTypeInfo>.GetInstance();
		try
		{
			instance.Add("ONBEFOREOPEN", new TriggerTypeInfo("Query", "OnBeforeOpen", null, NavCorLib.NoneType, null, ImmutableArray.Create<DependentProperty>(), ImmutableArray.Create<TriggerParameter>()));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool IsValidQueryTrigger(string name)
	{
		return QueryTriggers.ContainsKey(name.ToUpperInvariant());
	}

	internal static TriggerTypeInfo GetQueryTriggerInfo(string name)
	{
		QueryTriggers.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	internal static bool IsValidTrigger(string name)
	{
		string key = name.ToUpperInvariant();
		if (!CodeunitTriggers.ContainsKey(key) && !TableTriggers.ContainsKey(key) && !TableExtensionTriggers.ContainsKey(key) && !FieldTriggers.ContainsKey(key) && !FieldExtensionTriggers.ContainsKey(key) && !PageTriggers.ContainsKey(key) && !RequestPageTriggers.ContainsKey(key) && !PageExtensionTriggers.ContainsKey(key) && !ControlTriggers.ContainsKey(key) && !ControlExtensionTriggers.ContainsKey(key) && !ActionTriggers.ContainsKey(key) && !ActionExtensionTriggers.ContainsKey(key) && !ReportTriggers.ContainsKey(key) && !ReportDataItemTriggers.ContainsKey(key) && !XmlPortTriggers.ContainsKey(key) && !XmlPortTableElementTriggers.ContainsKey(key) && !XmlPortFieldElementTriggers.ContainsKey(key) && !XmlPortTextElementTriggers.ContainsKey(key) && !XmlPortFieldAttributeTriggers.ContainsKey(key) && !XmlPortTextAttributeTriggers.ContainsKey(key))
		{
			return QueryTriggers.ContainsKey(key);
		}
		return true;
	}
}
