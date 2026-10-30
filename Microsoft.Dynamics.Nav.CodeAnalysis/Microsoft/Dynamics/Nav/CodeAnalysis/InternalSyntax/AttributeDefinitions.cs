using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal static class AttributeDefinitions
{
	internal static ImmutableDictionary<string, AttributeTypeInfo> Attributes { get; }

	static AttributeDefinitions()
	{
		Attributes = GetAttributes();
	}

	internal static bool IsValidAttribute(string name)
	{
		return Attributes.ContainsKey(name.ToUpperInvariant());
	}

	internal static AttributeTypeInfo GetAttributeInfo(string name)
	{
		Attributes.TryGetValue(name.ToUpperInvariant(), out var value);
		return value;
	}

	private static ImmutableDictionary<string, AttributeTypeInfo> GetAttributes()
	{
		PooledDictionary<string, AttributeTypeInfo> instance = PooledDictionary<string, AttributeTypeInfo>.GetInstance();
		try
		{
			SymbolKind? allowedIn = SymbolKind.Codeunit;
			IAttributeEmitter instance2 = EventSubscriberAttributeEmitter.Instance;
			IAttributeValidator instance3 = EventSubscriberAttributeValidator.Instance;
			instance.Add("EVENTSUBSCRIBER", new AttributeTypeInfo("EventSubscriber", "NavEventSubscriber", AttributeCategory.EventSubscriber, AttributeKind.EventSubscriber, null, AttributeAllowedOn.Method, allowedIn, null, "", noEmit: false, isObsolete: false, null, instance2, instance3, new AttributeParameter[6]
			{
				new AttributeParameter("ObjectType", NavCorLib.ObjectTypeType, isVar: false, isOptional: false),
				new AttributeParameter("ObjectId", NavCorLib.IntegerType, isVar: false, isOptional: false),
				new AttributeParameter("EventName", NavCorLib.TextType, isVar: false, isOptional: false),
				new AttributeParameter("ElementName", NavCorLib.TextType, isVar: false, isOptional: false),
				new AttributeParameter("SkipOnMissingLicense", NavCorLib.BooleanType, isVar: false, isOptional: false),
				new AttributeParameter("SkipOnMissingPermission", NavCorLib.BooleanType, isVar: false, isOptional: false)
			}, null, null));
			AttributeTagKind[] tags = new AttributeTagKind[1];
			instance3 = EventAttributeValidator.Instance;
			instance.Add("INTEGRATIONEVENT", new AttributeTypeInfo("IntegrationEvent", "NavEvent", AttributeCategory.Integration, AttributeKind.IntegrationEvent, tags, AttributeAllowedOn.Method, null, null, "NavEventType.Integration", noEmit: false, isObsolete: false, null, null, instance3, new AttributeParameter[2]
			{
				new AttributeParameter("IncludeSender", NavCorLib.BooleanType, isVar: false, isOptional: false),
				new AttributeParameter("GlobalVarAccess", NavCorLib.BooleanType, isVar: false, isOptional: false)
			}, null, null));
			AttributeTagKind[] tags2 = new AttributeTagKind[1];
			instance3 = EventAttributeValidator.Instance;
			instance.Add("BUSINESSEVENT", new AttributeTypeInfo("BusinessEvent", "NavEvent", AttributeCategory.Business, AttributeKind.BusinessEvent, tags2, AttributeAllowedOn.Method, null, null, "NavEventType.Business", noEmit: false, isObsolete: false, null, null, instance3, new AttributeParameter[1]
			{
				new AttributeParameter("IncludeSender", NavCorLib.BooleanType, isVar: false, isOptional: false)
			}, null, null));
			instance.Add("NORMAL", new AttributeTypeInfo("Normal", "NavNormal", AttributeCategory.FunctionType, AttributeKind.Normal, null, AttributeAllowedOn.Method, SymbolKind.Codeunit, null, "", noEmit: true, isObsolete: false, null, null, null, null, null, null));
			SymbolKind? allowedIn2 = SymbolKind.Codeunit;
			CodeunitSubtypeKind? requiredCodeunitSubtype = CodeunitSubtypeKind.Test;
			instance2 = TestAttributeEmitter.Instance;
			instance.Add("TEST", new AttributeTypeInfo("Test", "NavTest", AttributeCategory.FunctionType, AttributeKind.Test, null, AttributeAllowedOn.Method, allowedIn2, requiredCodeunitSubtype, "", noEmit: false, isObsolete: false, null, instance2, null, null, new AttributeRequiredMethodSignature[1]
			{
				new AttributeRequiredMethodSignature("Test", FormattableString.Invariant($"()"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), null)
			}, null));
			SymbolKind? allowedIn3 = SymbolKind.Codeunit;
			CodeunitSubtypeKind? requiredCodeunitSubtype2 = CodeunitSubtypeKind.Test;
			instance3 = HandlerFunctionsAttributeValidator.Instance;
			instance.Add("HANDLERFUNCTIONS", new AttributeTypeInfo("HandlerFunctions", "NavHandlerFunctions", AttributeCategory.HandlerFunctions, AttributeKind.HandlerFunctions, null, AttributeAllowedOn.Method, allowedIn3, requiredCodeunitSubtype2, "", noEmit: true, isObsolete: false, null, null, instance3, new AttributeParameter[1]
			{
				new AttributeParameter("HandlerNames", NavCorLib.StringType, isVar: false, isOptional: false)
			}, null, null));
			instance.Add("TRANSACTIONMODEL", new AttributeTypeInfo("TransactionModel", "NavTransactionModel", AttributeCategory.TransactionModel, AttributeKind.TransactionModel, null, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "", noEmit: true, isObsolete: false, null, null, null, new AttributeParameter[1]
			{
				new AttributeParameter("TransactionModel", NavCorLib.TransactionModelType, isVar: false, isOptional: false)
			}, null, null));
			instance.Add("TESTPERMISSIONS", new AttributeTypeInfo("TestPermissions", "NavTestPermissions", AttributeCategory.TestPermissions, AttributeKind.TestPermissions, null, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "", noEmit: true, isObsolete: false, null, null, null, new AttributeParameter[1]
			{
				new AttributeParameter("TestPermissions", NavCorLib.TestPermissionsType, isVar: false, isOptional: false)
			}, null, null));
			instance.Add("MESSAGEHANDLER", new AttributeTypeInfo("MessageHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.MessageHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.Message", noEmit: false, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[2]
			{
				new AttributeRequiredMethodSignature("MessageHandler", FormattableString.Invariant($"(Message: {NavCorLib.Text1024Type})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), new AttributeParameter[1]
				{
					new AttributeParameter("Message", NavCorLib.Text1024Type, isVar: false, isOptional: false)
				}),
				new AttributeRequiredMethodSignature("MessageHandler", FormattableString.Invariant($"(Message: {NavCorLib.TextType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), compatibility: VersionCompatibility.Parse("2.1"), requiredParameters: new AttributeParameter[1]
				{
					new AttributeParameter("Message", NavCorLib.TextType, isVar: false, isOptional: false)
				})
			}, null));
			instance.Add("CONFIRMHANDLER", new AttributeTypeInfo("ConfirmHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.ConfirmHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.Confirm", noEmit: false, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[2]
			{
				new AttributeRequiredMethodSignature("ConfirmHandler", FormattableString.Invariant($"(Question: {NavCorLib.Text1024Type}, var Reply: {NavCorLib.BooleanType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), new AttributeParameter[2]
				{
					new AttributeParameter("Question", NavCorLib.Text1024Type, isVar: false, isOptional: false),
					new AttributeParameter("Reply", NavCorLib.BooleanType, isVar: true, isOptional: false)
				}),
				new AttributeRequiredMethodSignature("ConfirmHandler", FormattableString.Invariant($"(Question: {NavCorLib.TextType}, var Reply: {NavCorLib.BooleanType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), compatibility: VersionCompatibility.Parse("2.1"), requiredParameters: new AttributeParameter[2]
				{
					new AttributeParameter("Question", NavCorLib.TextType, isVar: false, isOptional: false),
					new AttributeParameter("Reply", NavCorLib.BooleanType, isVar: true, isOptional: false)
				})
			}, null));
			instance.Add("STRMENUHANDLER", new AttributeTypeInfo("StrMenuHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.StrMenuHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.StrMenu", noEmit: false, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[2]
			{
				new AttributeRequiredMethodSignature("StrMenuHandler", FormattableString.Invariant($"(Options: {NavCorLib.Text1024Type}, var Choice: {NavCorLib.IntegerType}, Instruction: {NavCorLib.Text1024Type})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), new AttributeParameter[3]
				{
					new AttributeParameter("Options", NavCorLib.Text1024Type, isVar: false, isOptional: false),
					new AttributeParameter("Choice", NavCorLib.IntegerType, isVar: true, isOptional: false),
					new AttributeParameter("Instruction", NavCorLib.Text1024Type, isVar: false, isOptional: false)
				}),
				new AttributeRequiredMethodSignature("StrMenuHandler", FormattableString.Invariant($"(Options: {NavCorLib.TextType}, var Choice: {NavCorLib.IntegerType}, Instruction: {NavCorLib.TextType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), compatibility: VersionCompatibility.Parse("2.1"), requiredParameters: new AttributeParameter[3]
				{
					new AttributeParameter("Options", NavCorLib.TextType, isVar: false, isOptional: false),
					new AttributeParameter("Choice", NavCorLib.IntegerType, isVar: true, isOptional: false),
					new AttributeParameter("Instruction", NavCorLib.TextType, isVar: false, isOptional: false)
				})
			}, null));
			instance.Add("PAGEHANDLER", new AttributeTypeInfo("PageHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.PageHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.Page", noEmit: false, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[2]
			{
				new AttributeRequiredMethodSignature("PageHandler", FormattableString.Invariant($"(var Page: {NavCorLib.TestPageType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), new AttributeParameter[1]
				{
					new AttributeParameter("Page", NavCorLib.TestPageType, isVar: true, isOptional: false)
				}),
				new AttributeRequiredMethodSignature("PageHandler", FormattableString.Invariant($"(var Page: {NavCorLib.PageType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), compatibility: VersionCompatibility.Parse("2.1"), requiredParameters: new AttributeParameter[1]
				{
					new AttributeParameter("Page", NavCorLib.PageType, isVar: true, isOptional: false)
				})
			}, null));
			instance.Add("MODALPAGEHANDLER", new AttributeTypeInfo("ModalPageHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.ModalPageHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.ModalPage", noEmit: false, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[2]
			{
				new AttributeRequiredMethodSignature("ModalPageHandler", FormattableString.Invariant($"(var Page: {NavCorLib.TestPageType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), new AttributeParameter[1]
				{
					new AttributeParameter("Page", NavCorLib.TestPageType, isVar: true, isOptional: false)
				}),
				new AttributeRequiredMethodSignature("ModalPageHandler", FormattableString.Invariant($"(var Page: {NavCorLib.PageType}, var Response: {NavCorLib.ActionType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), compatibility: VersionCompatibility.Parse("2.1"), requiredParameters: new AttributeParameter[2]
				{
					new AttributeParameter("Page", NavCorLib.PageType, isVar: true, isOptional: false),
					new AttributeParameter("Response", NavCorLib.ActionType, isVar: true, isOptional: false)
				})
			}, null));
			instance.Add("REPORTHANDLER", new AttributeTypeInfo("ReportHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.ReportHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.Report", noEmit: false, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[1]
			{
				new AttributeRequiredMethodSignature("ReportHandler", FormattableString.Invariant($"(var Report: {NavCorLib.ReportType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), new AttributeParameter[1]
				{
					new AttributeParameter("Report", NavCorLib.ReportType, isVar: true, isOptional: false)
				})
			}, null));
			instance.Add("REQUESTPAGEHANDLER", new AttributeTypeInfo("RequestPageHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.RequestPageHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.RequestPage", noEmit: false, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[1]
			{
				new AttributeRequiredMethodSignature("RequestPageHandler", FormattableString.Invariant($"(var RequestPage: {NavCorLib.TestRequestPageType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), new AttributeParameter[1]
				{
					new AttributeParameter("RequestPage", NavCorLib.TestRequestPageType, isVar: true, isOptional: false)
				})
			}, null));
			instance.Add("FILTERPAGEHANDLER", new AttributeTypeInfo("FilterPageHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.FilterPageHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.FilterPage", noEmit: false, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[1]
			{
				new AttributeRequiredMethodSignature("FilterPageHandler", FormattableString.Invariant($"(var Record1: {NavCorLib.RecordRefType}, [var Record2: {NavCorLib.RecordRefType}], [var Record3: {NavCorLib.RecordRefType}], [var Record4: {NavCorLib.RecordRefType}], [var Record5: {NavCorLib.RecordRefType}], [var Record6: {NavCorLib.RecordRefType}], [var Record7: {NavCorLib.RecordRefType}], [var Record8: {NavCorLib.RecordRefType}], [var Record9: {NavCorLib.RecordRefType}], [var Record10: {NavCorLib.RecordRefType}]): Boolean"), true, new AttributeParameter("", NavCorLib.BooleanType, isVar: false, isOptional: false), new AttributeParameter[10]
				{
					new AttributeParameter("Record1", NavCorLib.RecordRefType, isVar: true, isOptional: false),
					new AttributeParameter("Record2", NavCorLib.RecordRefType, isVar: true, isOptional: true),
					new AttributeParameter("Record3", NavCorLib.RecordRefType, isVar: true, isOptional: true),
					new AttributeParameter("Record4", NavCorLib.RecordRefType, isVar: true, isOptional: true),
					new AttributeParameter("Record5", NavCorLib.RecordRefType, isVar: true, isOptional: true),
					new AttributeParameter("Record6", NavCorLib.RecordRefType, isVar: true, isOptional: true),
					new AttributeParameter("Record7", NavCorLib.RecordRefType, isVar: true, isOptional: true),
					new AttributeParameter("Record8", NavCorLib.RecordRefType, isVar: true, isOptional: true),
					new AttributeParameter("Record9", NavCorLib.RecordRefType, isVar: true, isOptional: true),
					new AttributeParameter("Record10", NavCorLib.RecordRefType, isVar: true, isOptional: true)
				})
			}, null));
			instance.Add("HYPERLINKHANDLER", new AttributeTypeInfo("HyperlinkHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.HyperlinkHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.Hyperlink", noEmit: false, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[2]
			{
				new AttributeRequiredMethodSignature("HyperlinkHandler", FormattableString.Invariant($"(Message: {NavCorLib.Text1024Type})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), new AttributeParameter[1]
				{
					new AttributeParameter("Message", NavCorLib.Text1024Type, isVar: false, isOptional: false)
				}),
				new AttributeRequiredMethodSignature("HyperlinkHandler", FormattableString.Invariant($"(Message: {NavCorLib.TextType})"), true, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), compatibility: VersionCompatibility.Parse("2.1"), requiredParameters: new AttributeParameter[1]
				{
					new AttributeParameter("Message", NavCorLib.TextType, isVar: false, isOptional: false)
				})
			}, null));
			instance.Add("SESSIONSETTINGSHANDLER", new AttributeTypeInfo("SessionSettingsHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.SessionSettingsHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.SessionSettings", noEmit: false, isObsolete: false, null, null, null, null, compatibility: VersionCompatibility.Parse("3.0"), requiredSignatures: new AttributeRequiredMethodSignature[1]
			{
				new AttributeRequiredMethodSignature("SessionSettingsHandler", FormattableString.Invariant($"(var SessionSettings: {NavCorLib.SessionSettingsType}): Boolean"), true, new AttributeParameter("", NavCorLib.BooleanType, isVar: false, isOptional: false), compatibility: VersionCompatibility.Parse("3.0"), requiredParameters: new AttributeParameter[1]
				{
					new AttributeParameter("SessionSettings", NavCorLib.SessionSettingsType, isVar: true, isOptional: false)
				})
			}));
			instance.Add("SENDNOTIFICATIONHANDLER", new AttributeTypeInfo("SendNotificationHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.SendNotificationHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.SendNotification", noEmit: false, isObsolete: false, null, null, null, new AttributeParameter[1]
			{
				new AttributeParameter("HandlerIsOptional", NavCorLib.BooleanType, isVar: false, isOptional: true)
			}, new AttributeRequiredMethodSignature[1]
			{
				new AttributeRequiredMethodSignature("SendNotificationHandler", FormattableString.Invariant($"(var TheNotification: {NavCorLib.NotificationType}): Boolean"), true, new AttributeParameter("", NavCorLib.BooleanType, isVar: false, isOptional: false), new AttributeParameter[1]
				{
					new AttributeParameter("TheNotification", NavCorLib.NotificationType, isVar: true, isOptional: false)
				})
			}, null));
			instance.Add("RECALLNOTIFICATIONHANDLER", new AttributeTypeInfo("RecallNotificationHandler", "NavHandler", AttributeCategory.FunctionType, AttributeKind.RecallNotificationHandler, new AttributeTagKind[1] { AttributeTagKind.Handler }, AttributeAllowedOn.Method, SymbolKind.Codeunit, CodeunitSubtypeKind.Test, "NavHandlerType.RecallNotification", noEmit: false, isObsolete: false, null, null, null, new AttributeParameter[1]
			{
				new AttributeParameter("HandlerIsOptional", NavCorLib.BooleanType, isVar: false, isOptional: true)
			}, new AttributeRequiredMethodSignature[1]
			{
				new AttributeRequiredMethodSignature("RecallNotificationHandler", FormattableString.Invariant($"(var TheNotification: {NavCorLib.NotificationType}): Boolean"), true, new AttributeParameter("", NavCorLib.BooleanType, isVar: false, isOptional: false), new AttributeParameter[1]
				{
					new AttributeParameter("TheNotification", NavCorLib.NotificationType, isVar: true, isOptional: false)
				})
			}, null));
			instance.Add("NONE", new AttributeTypeInfo("None", "NavNone", AttributeCategory.None, AttributeKind.None, null, AttributeAllowedOn.Method, null, null, "", noEmit: true, isObsolete: false, null, null, null, null, null, null));
			instance.Add("SERVICEENABLED", new AttributeTypeInfo("ServiceEnabled", "NavServiceEnabled", AttributeCategory.ServiceEnabled, AttributeKind.ServiceEnabled, null, AttributeAllowedOn.Method, null, null, "", noEmit: false, isObsolete: false, null, null, null, null, null, VersionCompatibility.Parse("2.0")));
			instance.Add("TRYFUNCTION", new AttributeTypeInfo("TryFunction", "NavTryFunction", AttributeCategory.TryFunction, AttributeKind.TryFunction, null, AttributeAllowedOn.Method, null, null, "", noEmit: true, isObsolete: false, null, null, null, null, new AttributeRequiredMethodSignature[1]
			{
				new AttributeRequiredMethodSignature("TryFunction", FormattableString.Invariant($"()"), false, new AttributeParameter("", NavCorLib.NoneType, isVar: false, isOptional: false), null, VersionCompatibility.Parse("2.0"))
			}, null));
			instance.Add("RUNONCLIENT", new AttributeTypeInfo("RunOnClient", "NavRunOnClient", AttributeCategory.RunOnClient, AttributeKind.RunOnClient, null, AttributeAllowedOn.Variable, null, null, "", noEmit: true, isObsolete: false, validator: RunOnClientAttributeValidator.Instance, compilationScope: SymbolCompilationScope.Internal, emitter: null, parameters: null, requiredSignatures: null, compatibility: null));
			instance.Add("WITHEVENTS", new AttributeTypeInfo("WithEvents", "NavWithEvents", AttributeCategory.WithEvents, AttributeKind.WithEvents, null, AttributeAllowedOn.Variable, null, null, "", noEmit: true, isObsolete: false, validator: WithEventsAttributeValidator.Instance, compilationScope: SymbolCompilationScope.Internal, emitter: null, parameters: null, requiredSignatures: null, compatibility: null));
			instance.Add("SUPPRESSDISPOSE", new AttributeTypeInfo("SuppressDispose", "NavSuppressDispose", AttributeCategory.SuppressDispose, AttributeKind.SuppressDispose, null, AttributeAllowedOn.Variable, null, null, "", noEmit: true, isObsolete: false, validator: SuppressDisposeAttributeValidator.Instance, compilationScope: SymbolCompilationScope.Internal, emitter: null, parameters: null, requiredSignatures: null, compatibility: null));
			instance.Add("INDATASET", new AttributeTypeInfo("InDataSet", "NavInDataSet", AttributeCategory.InDataSet, AttributeKind.InDataSet, null, AttributeAllowedOn.Variable, null, null, "", noEmit: true, isObsolete: false, null, null, null, null, null, null));
			instance3 = SecurityFilteringAttributeValidator.Instance;
			instance.Add("SECURITYFILTERING", new AttributeTypeInfo("SecurityFiltering", "NavSecurityFiltering", AttributeCategory.SecurityFiltering, AttributeKind.SecurityFiltering, null, AttributeAllowedOn.Variable, null, null, "", noEmit: true, isObsolete: false, null, null, instance3, new AttributeParameter[1]
			{
				new AttributeParameter("Kind", NavCorLib.SecurityFilterType, isVar: false, isOptional: false)
			}, null, null));
			instance2 = ScopeAttributeEmitter.Instance;
			instance.Add("SCOPE", new AttributeTypeInfo("Scope", "NavScope", AttributeCategory.Scope, AttributeKind.Scope, null, AttributeAllowedOn.Method, null, null, "", noEmit: false, isObsolete: false, null, instance2, null, new AttributeParameter[1]
			{
				new AttributeParameter("Kind", NavCorLib.TextType, isVar: false, isOptional: false)
			}, null, null));
			instance.Add("NONDEBUGGABLE", new AttributeTypeInfo("NonDebuggable", "NavNonDebuggable", AttributeCategory.Debugger, AttributeKind.NonDebuggable, null, AttributeAllowedOn.Method | AttributeAllowedOn.Variable, null, null, "", noEmit: true, isObsolete: false, null, null, null, null, null, VersionCompatibility.Parse("2.0")));
			return instance.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
		}
	}
}
