using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class AttributeDocumentationResources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Microsoft.Dynamics.Nav.CodeAnalysis.AttributeDocumentationResources", typeof(AttributeDocumentationResources).GetTypeInfo().Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string BusinessEvent => ResourceManager.GetString("BusinessEvent", resourceCulture);

	internal static string BusinessEvent_IncludeSender => ResourceManager.GetString("BusinessEvent_IncludeSender", resourceCulture);

	internal static string ConfirmHandler => ResourceManager.GetString("ConfirmHandler", resourceCulture);

	internal static string ConfirmHandler_Question => ResourceManager.GetString("ConfirmHandler_Question", resourceCulture);

	internal static string ConfirmHandler_Reply => ResourceManager.GetString("ConfirmHandler_Reply", resourceCulture);

	internal static string EventSubscriber => ResourceManager.GetString("EventSubscriber", resourceCulture);

	internal static string EventSubscriber_ElementName => ResourceManager.GetString("EventSubscriber_ElementName", resourceCulture);

	internal static string EventSubscriber_EventName => ResourceManager.GetString("EventSubscriber_EventName", resourceCulture);

	internal static string EventSubscriber_ObjectId => ResourceManager.GetString("EventSubscriber_ObjectId", resourceCulture);

	internal static string EventSubscriber_ObjectType => ResourceManager.GetString("EventSubscriber_ObjectType", resourceCulture);

	internal static string EventSubscriber_SkipOnMissingLicense => ResourceManager.GetString("EventSubscriber_SkipOnMissingLicense", resourceCulture);

	internal static string EventSubscriber_SkipOnMissingPermission => ResourceManager.GetString("EventSubscriber_SkipOnMissingPermission", resourceCulture);

	internal static string FilterPageHandler => ResourceManager.GetString("FilterPageHandler", resourceCulture);

	internal static string FilterPageHandler_Record1 => ResourceManager.GetString("FilterPageHandler_Record1", resourceCulture);

	internal static string FilterPageHandler_Record10 => ResourceManager.GetString("FilterPageHandler_Record10", resourceCulture);

	internal static string FilterPageHandler_Record2 => ResourceManager.GetString("FilterPageHandler_Record2", resourceCulture);

	internal static string FilterPageHandler_Record3 => ResourceManager.GetString("FilterPageHandler_Record3", resourceCulture);

	internal static string FilterPageHandler_Record4 => ResourceManager.GetString("FilterPageHandler_Record4", resourceCulture);

	internal static string FilterPageHandler_Record5 => ResourceManager.GetString("FilterPageHandler_Record5", resourceCulture);

	internal static string FilterPageHandler_Record6 => ResourceManager.GetString("FilterPageHandler_Record6", resourceCulture);

	internal static string FilterPageHandler_Record7 => ResourceManager.GetString("FilterPageHandler_Record7", resourceCulture);

	internal static string FilterPageHandler_Record8 => ResourceManager.GetString("FilterPageHandler_Record8", resourceCulture);

	internal static string FilterPageHandler_Record9 => ResourceManager.GetString("FilterPageHandler_Record9", resourceCulture);

	internal static string HandlerFunctions => ResourceManager.GetString("HandlerFunctions", resourceCulture);

	internal static string HandlerFunctions_HandlerNames => ResourceManager.GetString("HandlerFunctions_HandlerNames", resourceCulture);

	internal static string HyperlinkHandler => ResourceManager.GetString("HyperlinkHandler", resourceCulture);

	internal static string HyperlinkHandler_Message => ResourceManager.GetString("HyperlinkHandler_Message", resourceCulture);

	internal static string InDataSet => ResourceManager.GetString("InDataSet", resourceCulture);

	internal static string IntegrationEvent => ResourceManager.GetString("IntegrationEvent", resourceCulture);

	internal static string IntegrationEvent_GlobalVarAccess => ResourceManager.GetString("IntegrationEvent_GlobalVarAccess", resourceCulture);

	internal static string IntegrationEvent_IncludeSender => ResourceManager.GetString("IntegrationEvent_IncludeSender", resourceCulture);

	internal static string MessageHandler => ResourceManager.GetString("MessageHandler", resourceCulture);

	internal static string MessageHandler_Message => ResourceManager.GetString("MessageHandler_Message", resourceCulture);

	internal static string ModalPageHandler => ResourceManager.GetString("ModalPageHandler", resourceCulture);

	internal static string ModalPageHandler_Page => ResourceManager.GetString("ModalPageHandler_Page", resourceCulture);

	internal static string ModalPageHandler_Response => ResourceManager.GetString("ModalPageHandler_Response", resourceCulture);

	internal static string NonDebuggable => ResourceManager.GetString("NonDebuggable", resourceCulture);

	internal static string None => ResourceManager.GetString("None", resourceCulture);

	internal static string Normal => ResourceManager.GetString("Normal", resourceCulture);

	internal static string PageHandler => ResourceManager.GetString("PageHandler", resourceCulture);

	internal static string PageHandler_Page => ResourceManager.GetString("PageHandler_Page", resourceCulture);

	internal static string RecallNotificationHandler => ResourceManager.GetString("RecallNotificationHandler", resourceCulture);

	internal static string RecallNotificationHandler_HandlerIsOptional => ResourceManager.GetString("RecallNotificationHandler_HandlerIsOptional", resourceCulture);

	internal static string RecallNotificationHandler_TheNotification => ResourceManager.GetString("RecallNotificationHandler_TheNotification", resourceCulture);

	internal static string ReportHandler => ResourceManager.GetString("ReportHandler", resourceCulture);

	internal static string ReportHandler_Report => ResourceManager.GetString("ReportHandler_Report", resourceCulture);

	internal static string RequestPageHandler => ResourceManager.GetString("RequestPageHandler", resourceCulture);

	internal static string RequestPageHandler_RequestPage => ResourceManager.GetString("RequestPageHandler_RequestPage", resourceCulture);

	internal static string RunOnClient => ResourceManager.GetString("RunOnClient", resourceCulture);

	internal static string Scope => ResourceManager.GetString("Scope", resourceCulture);

	internal static string Scope_Kind => ResourceManager.GetString("Scope_Kind", resourceCulture);

	internal static string SecurityFiltering => ResourceManager.GetString("SecurityFiltering", resourceCulture);

	internal static string SecurityFiltering_Kind => ResourceManager.GetString("SecurityFiltering_Kind", resourceCulture);

	internal static string SendNotificationHandler => ResourceManager.GetString("SendNotificationHandler", resourceCulture);

	internal static string SendNotificationHandler_HandlerIsOptional => ResourceManager.GetString("SendNotificationHandler_HandlerIsOptional", resourceCulture);

	internal static string SendNotificationHandler_TheNotification => ResourceManager.GetString("SendNotificationHandler_TheNotification", resourceCulture);

	internal static string ServiceEnabled => ResourceManager.GetString("ServiceEnabled", resourceCulture);

	internal static string SessionSettingsHandler => ResourceManager.GetString("SessionSettingsHandler", resourceCulture);

	internal static string SessionSettingsHandler_SessionSettings => ResourceManager.GetString("SessionSettingsHandler_SessionSettings", resourceCulture);

	internal static string StrMenuHandler => ResourceManager.GetString("StrMenuHandler", resourceCulture);

	internal static string StrMenuHandler_Choice => ResourceManager.GetString("StrMenuHandler_Choice", resourceCulture);

	internal static string StrMenuHandler_Instruction => ResourceManager.GetString("StrMenuHandler_Instruction", resourceCulture);

	internal static string StrMenuHandler_Options => ResourceManager.GetString("StrMenuHandler_Options", resourceCulture);

	internal static string SuppressDispose => ResourceManager.GetString("SuppressDispose", resourceCulture);

	internal static string Test => ResourceManager.GetString("Test", resourceCulture);

	internal static string TestPermissions => ResourceManager.GetString("TestPermissions", resourceCulture);

	internal static string TestPermissions_TestPermissions => ResourceManager.GetString("TestPermissions_TestPermissions", resourceCulture);

	internal static string TransactionModel => ResourceManager.GetString("TransactionModel", resourceCulture);

	internal static string TransactionModel_TransactionModel => ResourceManager.GetString("TransactionModel_TransactionModel", resourceCulture);

	internal static string TryFunction => ResourceManager.GetString("TryFunction", resourceCulture);

	internal static string WithEvents => ResourceManager.GetString("WithEvents", resourceCulture);

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	internal AttributeDocumentationResources()
	{
	}
}
