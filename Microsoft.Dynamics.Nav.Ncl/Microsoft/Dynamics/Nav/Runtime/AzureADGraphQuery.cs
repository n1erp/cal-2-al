using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;
using Microsoft.Dynamics.Nav.Runtime.Authentication;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class AzureADGraphQuery : IAzureADGraphQuery
{
	private const string TenantDetailCacheKey = "TENANTDETAILKEY";

	private readonly INavLicenseServiceClient licenseServiceClient;

	private readonly NavSession currentSession;

	public AzureADGraphQuery(NavSession session = null)
	{
		currentSession = session ?? NavCurrentThread.Session;
		if (currentSession == null)
		{
			throw new NavNCLArgumentException("currentSession");
		}
		licenseServiceClient = new NavLicenseServiceClient(currentSession?.Tenant);
	}

	public IEnumerable<SkuInfo> GetDirectorySubscribedSkus()
	{
		using (DiagnosticsResolver.CreateTimingScope("00000Z8", Category.Security, DiagnosticParameter.SystemMetadata("AzureADGraphQuery.GetDirectorySubscribedSkus")))
		{
			return ExecuteActionRethrowAggregateException(() =>
			{
				IEnumerable<SkuInfo> directorySubscribedSkus = licenseServiceClient.GetDirectorySubscribedSkus(GetCallerIdentityToken(), currentSession?.Tenant.TenantSettings.AadTenantId, DiagnosticsResolver.GetMostSpecificInstance());
				if (directorySubscribedSkus == null)
				{
					throw new NavLicenseServiceException(Lang.GraphSubscriptionNotFound);
				}
				return directorySubscribedSkus;
			}, "GetDirectorySubscribedSkus");
		}
	}

	public IEnumerable<UserInfo> GetUsers(int count)
	{
		using (DiagnosticsResolver.CreateTimingScope("00000Z9", Category.Security, DiagnosticParameter.SystemMetadata("AzureADGraphQuery.GetUsers")))
		{
			return ExecuteActionRethrowAggregateException(() =>
			{
				IEnumerable<UserInfo> users = licenseServiceClient.GetUsers(GetCallerIdentityToken(), currentSession?.Tenant.TenantSettings.AadTenantId, count, DiagnosticsResolver.GetMostSpecificInstance());
				if (users == null)
				{
					throw new NavLicenseServiceException(Lang.GraphUsersNotFound);
				}
				return users;
			}, "GetUsers");
		}
	}

	public UserInfoCollectionPage GetUsersPage(int count)
	{
		using (DiagnosticsResolver.CreateTimingScope("000093I", Category.Security, DiagnosticParameter.SystemMetadata("AzureADGraphQuery.GetUsersPage")))
		{
			return ExecuteActionRethrowAggregateException(() =>
			{
				UserInfoCollectionPage usersPage = licenseServiceClient.GetUsersPage(GetCallerIdentityToken(), currentSession?.Tenant.TenantSettings.AadTenantId, count, DiagnosticsResolver.GetMostSpecificInstance());
				return usersPage ?? throw new NavLicenseServiceException(Lang.GraphUsersNotFound);
			}, "GetUsersPage");
		}
	}

	public UserInfoCollectionPage GetNextUsersPage(IList<QueryOption> options)
	{
		using (DiagnosticsResolver.CreateTimingScope("000093J", Category.Security, DiagnosticParameter.SystemMetadata("AzureADGraphQuery.GetNextUsersPage")))
		{
			return ExecuteActionRethrowAggregateException(() =>
			{
				UserInfoCollectionPage usersPage = licenseServiceClient.GetUsersPage(GetCallerIdentityToken(), currentSession?.Tenant.TenantSettings.AadTenantId, options, DiagnosticsResolver.GetMostSpecificInstance());
				return usersPage ?? throw new NavLicenseServiceException(Lang.GraphUsersNotFound);
			}, "GetNextUsersPage");
		}
	}

	public UserInfo GetUser(string userPrincipalName)
	{
		using (DiagnosticsResolver.CreateTimingScope("00000ZA", Category.Security, DiagnosticParameter.SystemMetadata("AzureADGraphQuery.GetUser")))
		{
			return ExecuteActionRethrowAggregateException(() => GetUserInfoByUpn(userPrincipalName), "GetUser");
		}
	}

	public bool TryGetUserByObjectId(string objectId, out UserInfo user)
	{
		user = null;
		try
		{
			if (string.IsNullOrWhiteSpace(objectId))
			{
				return false;
			}
			UserInfo userByObjectId = GetUserByObjectId(objectId);
			if (userByObjectId != null)
			{
				user = userByObjectId;
				return true;
			}
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000202", Category.Runtime, exception);
		}
		return false;
	}

	public UserInfo GetUserByObjectId(string objectId)
	{
		using (DiagnosticsResolver.CreateTimingScope("00000ZB", Category.Security, DiagnosticParameter.SystemMetadata("AzureADGraphQuery.GetUserbyObjectId")))
		{
			return ExecuteActionRethrowAggregateException(() =>
			{
				LicenseDetails licenseDetailsByObjectId = licenseServiceClient.GetLicenseDetailsByObjectId(currentSession?.Tenant.TenantSettings.AadTenantId, objectId, DiagnosticsResolver.GetMostSpecificInstance());
				if (((licenseDetailsByObjectId != null) ? licenseDetailsByObjectId.User : null) == null)
				{
					throw new NavLicenseServiceException(string.Format(CultureInfo.CurrentCulture, Lang.GraphUserNotFound, objectId));
				}
				return licenseDetailsByObjectId.User;
			}, "GetUserByObjectId");
		}
	}

	public UserInfo GetUserByAuthorizationEmail(string authorizationEmail)
	{
		using (DiagnosticsResolver.CreateTimingScope("00001OP", Category.Security, DiagnosticParameter.SystemMetadata("AzureADGraphQuery.GetUserbyAuthorizationEmail")))
		{
			return ExecuteActionRethrowAggregateException(() => GetUserInfoByUpn(authorizationEmail), "GetUserByAuthorizationEmail");
		}
	}

	public IEnumerable<RoleInfo> GetDirectoryRoles()
	{
		using (DiagnosticsResolver.CreateTimingScope("00000ZC", Category.Security, DiagnosticParameter.SystemMetadata("AzureADGraphQuery.GetDirectoryRoles")))
		{
			return ExecuteActionRethrowAggregateException(() =>
			{
				IEnumerable<RoleInfo> directoryRoles = licenseServiceClient.GetDirectoryRoles(GetCallerIdentityToken(), currentSession?.Tenant.TenantSettings.AadTenantId, DiagnosticsResolver.GetMostSpecificInstance());
				if (directoryRoles == null)
				{
					throw new NavLicenseServiceException(Lang.GraphRoleNotFound);
				}
				return directoryRoles;
			}, "GetDirectoryRoles");
		}
	}

	public TenantInfo GetTenantDetail()
	{
		using (DiagnosticsResolver.CreateTimingScope("00000ZF", Category.Security, DiagnosticParameter.SystemMetadata("AzureADGraphQuery.GetTenantDetail")))
		{
			return ExecuteActionRethrowAggregateException(() =>
			{
				TenantInfo val = currentSession?.Tenant.GetFromCache<TenantInfo>("TENANTDETAILKEY");
				string text = currentSession?.Tenant.TenantSettings.AadTenantId;
				if (val == null)
				{
					val = licenseServiceClient.GetTenantDetail(GetCallerIdentityToken(), text, DiagnosticsResolver.GetMostSpecificInstance());
					if (val != null)
					{
						currentSession?.Tenant.AddToCache("TENANTDETAILKEY", val, DateTime.UtcNow.AddHours(0.5), TimeSpan.FromMinutes(2.0));
					}
				}
				if (val == null)
				{
					throw new NavLicenseServiceException(string.Format(CultureInfo.CurrentCulture, Lang.GraphTenantDetailsNotFound, text));
				}
				return val;
			}, "GetTenantDetail");
		}
	}

	public static T ExecuteActionRethrowAggregateException<T>(Func<T> action, string functionName)
	{
		try
		{
			return action();
		}
		catch (AggregateException ex)
		{
			Exception ex2 = ex.Flatten().InnerExceptions.FirstOrDefault();
			ExceptionDispatchInfo exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex2);
			if (exceptionDispatchInfo.SourceException is NavLicenseServiceException)
			{
				exceptionDispatchInfo.Throw();
			}
			NavLicenseServiceException exception = new NavLicenseServiceException(string.Format(CultureInfo.InvariantCulture, "Error reading data from Azure Active Directory in function '{0}'. Exception Message: {1}.", functionName, ex2.Message), ex.Flatten());
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001VB", Category.Security, exception);
		}
	}

	private UserInfo GetUserInfoByUpn(string userPrincipalName)
	{
		LicenseDetails licenseDetailsByUpn = licenseServiceClient.GetLicenseDetailsByUpn(GetCallerIdentityToken(), currentSession?.Tenant.TenantSettings.AadTenantId, userPrincipalName, DiagnosticsResolver.GetMostSpecificInstance());
		if (licenseDetailsByUpn.User == null)
		{
			throw new NavLicenseServiceException(string.Format(CultureInfo.CurrentCulture, Lang.GraphUserNotFound, userPrincipalName));
		}
		return licenseDetailsByUpn.User;
	}

	private SecureString GetCallerIdentityToken()
	{
		SecureString secureString = currentSession?.Authenticator?.SecurityToken?.SerializedToken;
		if (secureString == null)
		{
			NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
			if (currentSession == null)
			{
				mostSpecificInstance.SendTraceTag("000079E", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Unexpected empty session while getting caller identity token");
			}
			else if (currentSession.Authenticator == null)
			{
				mostSpecificInstance.SendTraceTag("000079E", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Unexpected empty authenticator while getting caller identity token");
			}
			else if (currentSession.Authenticator.SecurityToken == null)
			{
				SecureString secureString2 = currentSession?.Authenticator?.TaskSchedulerUserToken;
				if (secureString2 != null)
				{
					mostSpecificInstance.SendTraceTag("000094W", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Using special task scheduler user token as caller identity token");
					return secureString2;
				}
				mostSpecificInstance.SendTraceTag("000079E", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Unexpected empty security token while getting caller identity token");
			}
			return null;
		}
		return secureString;
	}
}
