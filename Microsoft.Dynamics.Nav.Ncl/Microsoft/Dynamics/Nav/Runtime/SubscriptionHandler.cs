using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class SubscriptionHandler
{
	public static void HandleNotification(NavTenant tenant, string changeType, string typeName, string subscriptionId, string resourceId, int sequenceNumber, JObject notification)
	{
		Guid subscriptionRunNotificationAsUserAndComany = GetSubscriptionRunNotificationAsUserAndComany(tenant, subscriptionId, out var userName, out var companyName, out var appId);
		if (subscriptionRunNotificationAsUserAndComany == Guid.Empty)
		{
			tenant.Diagnostics.SendTraceTag("00000ET", Category.Webhooks, Verbosity.Error, "Could not resolve a user to handle notification from subscription: {0}", DiagnosticParameter.Eupi(subscriptionId));
			return;
		}
		tenant.Diagnostics.SendTraceTag("00000XJ", Category.Webhooks, Verbosity.Normal, "Attempting to insert Subscription Notification: {0}", DiagnosticParameter.Eupi(subscriptionId));
		using (NavSession navSession = ((ITenantSessionHandler)tenant).CreateTaskSchedulerSession(subscriptionRunNotificationAsUserAndComany, userName, ServerUserSettings.Instance.ServicesDefaultTimeZone.Value, appId))
		{
			NavCurrentThread.Session = navSession;
			using (navSession.Diagnostics.CreateTraceScope("00000EU", DiagnosticParameter.SystemMetadata("Subscription Notification"), MonitorType.None, Category.Webhooks))
			{
				using (NavThread.AddCurrentThreadToScheduler(NavEnvironment.Instance.ThreadScheduler))
				{
					navSession.ClientCallbackOverride = new HeadlessClientCallback(ProductNames.ServerServiceDisplayName, navSession);
					navSession.ClientTimeZone = ServerUserSettings.Instance.ServicesDefaultTimeZone.Value;
					navSession.SessionComment = string.Format(CultureInfo.CurrentCulture, Lang.StartSubscriptionNotificationSessionMessage, subscriptionId);
					try
					{
						navSession.AccessLock.BeginExclusiveExecution();
						navSession.Diagnostics.SendTraceTag("00000EV", Category.Webhooks, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Opening session");
						navSession.Open(useUserPersonalization: false);
						navSession.Diagnostics.SendTraceTag("00000EW", Category.Webhooks, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Opening company");
						navSession.BeginTransaction();
						if (!string.IsNullOrEmpty(companyName))
						{
							navSession.OpenCompany(companyName);
						}
						if (!navSession.IsCompanyOpen)
						{
							navSession.OpenCompany(navSession.GetServicesDefaultCompanyName());
						}
						using (new MaximizedPermissionScope(navSession, new int[1] { 2000000194 }))
						{
							using NavRecord navRecord = NavGlobal.NCLMetadata.GetMetaTableById(2000000194).CreateObjectInstance(navSession);
							navSession.Diagnostics.SendTraceTag("0000105", Category.Webhooks, Verbosity.Verbose, "Preparing to insert Webhook Notification for subscription {0} in table.", DiagnosticParameter.Eupi(subscriptionId));
							navRecord.ALInit();
							navRecord.SetFieldValue(3, NavText.Create(subscriptionId));
							navRecord.SetFieldValue(5, NavText.Create(changeType));
							navRecord.SetFieldValue(6, NavText.Create(typeName));
							navRecord.SetFieldValue(4, NavText.Create(resourceId));
							navRecord.SetFieldValue(1, new NavGuid(Guid.NewGuid()));
							navRecord.SetFieldValue(2, NavInteger.Create(sequenceNumber));
							NavBLOB navBLOB = new NavBLOB();
							navBLOB.AssignFromBytes(Encoding.UTF8.GetBytes(notification.ToString(Formatting.None)));
							navRecord.SetFieldValue(7, navBLOB);
							navRecord.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
						}
						navSession.EndTransaction(commit: true);
					}
					catch (NavSessionTerminatedException exception)
					{
						navSession.EndTransactionIfTransactionActive(commit: false);
						navSession.Diagnostics.SendExceptionTag("00000EX", Category.Webhooks, exception);
						throw;
					}
					catch (Exception ex)
					{
						navSession.EndTransactionIfTransactionActive(commit: false);
						if (!(ex is NavBaseException))
						{
							navSession.Diagnostics.SendExceptionTag("00000EY", Category.Webhooks, ex);
							LogWriterHelper.LogExceptionEvent(ex);
						}
						navSession.Diagnostics.SendExceptionTag("00000EZ", Category.Webhooks, ex, Verbosity.Normal);
						throw;
					}
					finally
					{
						NavCurrentThread.Session = null;
					}
				}
			}
		}
		tenant.Diagnostics.SendTraceTag("00000XK", Category.Webhooks, Verbosity.Verbose, "Done inserting Subscription Notification: {0}", DiagnosticParameter.Eupi(subscriptionId));
	}

	public static bool ValidateSubscriptionCreated(NavTenant tenant, string clientState)
	{
		bool result = false;
		try
		{
			((ITenantSessionHandler)tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession navSession) =>
			{
				using NavRecord navRecord = new NavRecord(navSession, 2000000199);
				navRecord.ALSetFilter(3, "%1", new NavText(clientState));
				result = navRecord.ALFind(DataError.TrapError, "-");
			}), true, false, false, (LanguageSetting)null);
		}
		catch (Exception exception) when (tenant.Diagnostics.SendExceptionTagExceptionFilter("00000F0", Category.Webhooks, exception))
		{
			throw;
		}
		return result;
	}

	public static bool ValidateSubscription(NavTenant tenant, string subscriptionId, string clientState)
	{
		bool result = false;
		try
		{
			((ITenantSessionHandler)tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession navSession) =>
			{
				ALDatabase.ALSelectLatestVersion();
				using NavRecord navRecord = new NavRecord(navSession, 2000000199);
				NavText navText = new NavText(subscriptionId);
				NavText navText2 = new NavText(clientState);
				tenant.Diagnostics.SendTraceTag("0000106", Category.Webhooks, Verbosity.Verbose, "Trying to find subscription {0} with client state {1}.", DiagnosticParameter.Eupi(navText.Value), DiagnosticParameter.SystemMetadata(navText2.Value));
				navRecord.ALSetFilter(1, "%1", navText);
				navRecord.ALSetFilter(3, "%1", navText2);
				result = navRecord.ALFind(DataError.TrapError, "-");
				tenant.Diagnostics.SendTraceTag("0000107", Category.Webhooks, Verbosity.Verbose, "Search for subscription {0} with client state {1} returned {2}.", DiagnosticParameter.Eupi(navText.Value), DiagnosticParameter.SystemMetadata(navText2.Value), DiagnosticParameter.SystemMetadata(result));
			}), true, false, false, (LanguageSetting)null);
		}
		catch (Exception exception) when (tenant.Diagnostics.SendExceptionTagExceptionFilter("00000F1", Category.Webhooks, exception))
		{
			throw;
		}
		return result;
	}

	private static Guid GetSubscriptionRunNotificationAsUserAndComany(NavTenant tenant, string subscriptionId, out string userName, out string companyName, out string appId)
	{
		Guid foundUserId = Guid.Empty;
		string foundUserName = string.Empty;
		string foundCompanyName = string.Empty;
		string foundAppId = ServerUserSettings.Instance.DefaultApplicationId.Value;
		((ITenantSessionHandler)tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession navSession) =>
		{
			navSession.Diagnostics.SendTraceTag("00000F2", Category.Webhooks, Verbosity.Verbose, "Searching for Subscription RunNotificationAs UserID for subscription {0}", DiagnosticParameter.Eupi(subscriptionId));
			NavUser user = null;
			using (NavRecord navRecord = new NavRecord(navSession, 2000000199))
			{
				navRecord.ALSetFilter(1, "%1", new NavText(subscriptionId));
				if (navRecord.ALFind(DataError.TrapError, "-"))
				{
					foundUserId = (navRecord.GetFieldValue(5) as NavGuid)?.Value ?? Guid.Empty;
					foundCompanyName = (navRecord.GetFieldValue(6) as NavText)?.Value ?? string.Empty;
					foundAppId = (navRecord.GetFieldValue(7) as NavText)?.Value ?? ServerUserSettings.Instance.DefaultApplicationId.Value;
					navSession.Diagnostics.SendTraceTag("00000F3", Category.Webhooks, Verbosity.Verbose, "Subscription {0} RunNotificationAs UserID {1}", DiagnosticParameter.Eupi(subscriptionId), DiagnosticParameter.Euii(foundUserId.ToString()));
					if (!tenant.Database.NavUsers.TryGetNavUserFromId(foundUserId, out user))
					{
						foundUserId = Guid.Empty;
					}
				}
			}
			if (foundUserId == Guid.Empty && !tenant.Database.NavUsers.TryGetNavUserFromApplicationId(ServerUserSettings.Instance.AzureActiveDirectoryClientId.Value, out user))
			{
				foundUserId = Guid.Empty;
			}
			if (foundUserId != Guid.Empty && user != null)
			{
				foundUserName = user.Name;
			}
		}), true, false, false, (LanguageSetting)null);
		userName = foundUserName;
		companyName = foundCompanyName;
		appId = foundAppId;
		return foundUserId;
	}
}
