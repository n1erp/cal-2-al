using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class HttpClientRequestFilterService : IHttpClientRequestFilterService
{
	internal enum AllowHttpClientRequestsOption
	{
		Canceled,
		AllowAlways,
		AllowOnce,
		BlockAlways,
		BlockOnce
	}

	public bool IsRequestFilteringEnabled => NavCurrentThread.Session.Tenant.TenantSettings.EnvironmentType == NavTenantEnvironmentType.Sandbox;

	internal Func<NavMethodScope, NavAppRuntimeMetadata> CallingApplicationProvider { get; set; } = GetCallingApplication;

	public bool ApplyRequestFiltering(NavMethodScope callingContext)
	{
		if (!IsRequestFilteringEnabled)
		{
			return false;
		}
		NavAppRuntimeMetadata val = CallingApplicationProvider(callingContext);
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000201 }))
		{
			using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000201);
			navRecord.ALSetFilter(1, "%1", new NavGuid(AppId.FromAppId(val.AppId)));
			if (navRecord.ALFindSet(DataError.TrapError, forUpdate: false, updateKey: false))
			{
				return IsHttpRequestBlockedForApplication(navRecord);
			}
		}
		return IsUserBlockingHttpRequest(val);
	}

	private static NavAppRuntimeMetadata GetCallingApplication(NavMethodScope callingContext)
	{
		NCLMetaApplicationObject metaApplicationObject = callingContext.ApplicationObject.MetaApplicationObject;
		return metaApplicationObject.OwningApp;
	}

	private static (bool allowHttpClientRequests, bool saveOption) RequestUserPermissionForBlockingOrAllowingRequestFromApplication(NavAppRuntimeMetadata owningApp)
	{
		if (!NavCurrentThread.Session.CallbackAllowed)
		{
			return (allowHttpClientRequests: false, saveOption: false);
		}
		string instruction = string.Format(CultureInfo.CurrentCulture, Lang.UnblockHttpClientRequestDialogMenuInstruction, owningApp.FriendlyDisplayName);
		int num = NavDialog.ALStrMenu(Lang.UnblockHttpClientRequestDialogMenuOptions, 1, instruction, Guid.NewGuid());
		switch ((AllowHttpClientRequestsOption)num)
		{
		case AllowHttpClientRequestsOption.Canceled:
			return (allowHttpClientRequests: false, saveOption: false);
		case AllowHttpClientRequestsOption.AllowAlways:
			return (allowHttpClientRequests: true, saveOption: true);
		case AllowHttpClientRequestsOption.AllowOnce:
			return (allowHttpClientRequests: true, saveOption: false);
		case AllowHttpClientRequestsOption.BlockAlways:
			return (allowHttpClientRequests: false, saveOption: true);
		case AllowHttpClientRequestsOption.BlockOnce:
			return (allowHttpClientRequests: false, saveOption: false);
		default:
			DiagnosticsResolver.GetMostSpecificInstance()?.SendTraceTag("000027V", Category.Runtime, Verbosity.Error, PrivacyClassification.SystemMetadata, "The menu used to acquire user input returned an invalid value: {0}", num);
			return (allowHttpClientRequests: false, saveOption: false);
		}
	}

	private static bool IsUserBlockingHttpRequest(NavAppRuntimeMetadata owningApp)
	{
		(bool allowHttpClientRequests, bool saveOption) tuple = RequestUserPermissionForBlockingOrAllowingRequestFromApplication(owningApp);
		var (flag, _) = tuple;
		if (tuple.saveOption)
		{
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000201 }))
			{
				NavRecord record = new NavRecord(NavCurrentThread.Session, 2000000201);
				try
				{
					record.SetFieldValue(1, new NavGuid(AppId.FromAppId(owningApp.AppId)));
					record.SetFieldValue(2, NavBoolean.CreateFromObject(flag));
					NavCurrentThread.Session.TransactionScope(() =>
					{
						record.ALInsert();
					});
				}
				finally
				{
					if (record != null)
					{
						((IDisposable)record).Dispose();
					}
				}
			}
		}
		return !flag;
	}

	private static bool IsHttpRequestBlockedForApplication(NavRecord serviceRedirectRecord)
	{
		return !serviceRedirectRecord.GetFieldValueSafe(2, NavType.Boolean).ToBoolean();
	}
}
