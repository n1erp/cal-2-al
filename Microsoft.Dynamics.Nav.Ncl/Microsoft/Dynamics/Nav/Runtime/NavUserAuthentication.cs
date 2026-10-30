using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;
using Microsoft.Dynamics.Nav.LicensingService.Model.V10;
using Microsoft.Dynamics.Nav.Runtime.Authentication;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavUserAuthentication : IDisposable
{
	private enum UserType
	{
		Normal,
		Admin,
		Nas
	}

	private readonly string authenticationKey;

	private readonly Guid userGuidForTaskScheduler;

	private readonly string userName;

	private bool disposed;

	private INavSecurityToken federatedSecurityToken;

	private UserNameSecurityToken userNameSecurityToken;

	private NavExchangeIdentifierSecurityToken exchangeIdentifierSecurityToken;

	private NavUser navUser;

	private UserActionRequired userActionRequired;

	private UserType userType;

	private WindowsIdentity windowsIdentity;

	private readonly Func<WindowsIdentity> getWindowsIdentity;

	private INavLicenseServiceClient navLicenseServiceClient;

	private Guid clientActivityId = Guid.NewGuid();

	private const string OfficeInvoicingMembershipEntitlementId = "07EB0DC4-7DA7-4E7B-BB42-2D44C5E08B08";

	private static readonly NavMetric2Dimensions InvoicingTenantAuthMetric = new NavMetric2Dimensions("InvoicingTenantOfficeEntitlementsMetric", "TenantId", "AuthenticationMethod");

	public const string DelegatedAdminAuthenticationType = "NavDelegatedUserPasswordValidator";

	private string navAppId;

	internal LicenseDetails UserLicenseDetails { get; set; }

	public UserInfo AzureADUser
	{
		get
		{
			LicenseDetails userLicenseDetails = UserLicenseDetails;
			if (userLicenseDetails == null)
			{
				return null;
			}
			return userLicenseDetails.User;
		}
	}

	private string AuthenticatedUserObjectId
	{
		get
		{
			if (IsAuthenticatingAsApplication)
			{
				return string.Empty;
			}
			string text;
			if (federatedSecurityToken == null)
			{
				text = navUser?.AuthenticationObjectId;
				if (text == null)
				{
					return string.Empty;
				}
			}
			else
			{
				text = federatedSecurityToken.GetClaimValue(SecurityTokenClaimType.ObjectIdentifier);
			}
			return text;
		}
	}

	private string AuthenticatedUserAuthorizationEmail
	{
		get
		{
			string text;
			if (federatedSecurityToken == null)
			{
				text = navUser?.AuthenticationEmail;
				if (text == null)
				{
					return string.Empty;
				}
			}
			else
			{
				text = federatedSecurityToken.AuthorizationEmail;
			}
			return text;
		}
	}

	private Guid AuthenticatedUserId
	{
		get
		{
			if (!Authenticated)
			{
				return Guid.Empty;
			}
			return navUser.Id;
		}
	}

	public bool Authenticated
	{
		get
		{
			if (navUser != null && navUser.IsEnabled)
			{
				return IsEntitled;
			}
			return false;
		}
	}

	private bool IsEntitled
	{
		get
		{
			if (!UseEntitlements || string.IsNullOrEmpty(navUser?.AuthenticationObjectId))
			{
				return navUser != null;
			}
			return UserLicenseDetails != null;
		}
	}

	public bool IsAuthenticatedUserEnabled
	{
		get
		{
			if (Authenticated)
			{
				if (!PendingUserActionRequired)
				{
					if (Tenant.Database.NavUsers.TryGetNavUserFromId(User.Id, out var user))
					{
						return user.IsEnabled;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public string AuthorizedInAppId => navAppId;

	private bool UseEntitlements => userType == UserType.Normal && ServerUserSettings.Instance.EnableMembershipEntitlement.Value;

	private INavLicenseServiceClient LicenseServiceClient
	{
		get
		{
			EnsureLicenseClient();
			return navLicenseServiceClient;
		}
	}

	internal bool IsAuthenticatingAsApplication
	{
		get
		{
			if (TokenApplicationId != null)
			{
				if (string.IsNullOrEmpty(federatedSecurityToken?.GetClaimValue(SecurityTokenClaimType.Name)))
				{
					return !"app_asserted_user_v1".Equals(federatedSecurityToken?.GetClaimValue(SecurityTokenClaimType.Version), StringComparison.Ordinal);
				}
				return false;
			}
			return false;
		}
	}

	private string TokenApplicationId => federatedSecurityToken?.GetClaimValue(SecurityTokenClaimType.AppId);

	public NavClientCredentialType AuthenticationMethod { get; private set; }

	public NavTenant Tenant { get; private set; }

	public NavUser User => navUser;

	public AuthorizationCodeContext AuthorizationCodeContext { get; private set; }

	public UserActionRequired UserActionRequired => userActionRequired;

	public bool CanIdentify
	{
		get
		{
			if (IsAuthenticatedDelegatedWindowsNavAdminUser)
			{
				return true;
			}
			WindowsIdentity windowsIdentity = WindowsIdentity;
			if (windowsIdentity == null || (windowsIdentity.ImpersonationLevel != TokenImpersonationLevel.Identification && windowsIdentity.ImpersonationLevel != TokenImpersonationLevel.Impersonation && windowsIdentity.ImpersonationLevel != TokenImpersonationLevel.Delegation))
			{
				return false;
			}
			return true;
		}
	}

	private static bool IsAuthenticatedDelegatedWindowsNavAdminUser
	{
		get
		{
			IIdentity identity = ServiceSecurityContext.Current?.PrimaryIdentity;
			if (identity == null)
			{
				return false;
			}
			if (identity.AuthenticationType == "NavDelegatedUserPasswordValidator")
			{
				return identity.IsAuthenticated;
			}
			return false;
		}
	}

	private WindowsIdentity WindowsIdentity
	{
		get
		{
			if (AuthenticationMethod == NavClientCredentialType.Windows || AuthenticationMethod == NavClientCredentialType.UserName)
			{
				if (windowsIdentity != null)
				{
					return windowsIdentity;
				}
				if (ServiceSecurityContext.Current != null)
				{
					return ServiceSecurityContext.Current.WindowsIdentity;
				}
				if (getWindowsIdentity != null)
				{
					return getWindowsIdentity();
				}
				return WindowsIdentity.GetCurrent();
			}
			return null;
		}
	}

	internal bool SkipTenantLicenseStateCheck { get; private set; }

	internal bool IsNormalUserType => userType == UserType.Normal;

	public INavSecurityToken SecurityToken => federatedSecurityToken;

	public SecureString TaskSchedulerUserToken
	{
		get
		{
			if (AuthenticationMethod != NavClientCredentialType.TaskScheduler)
			{
				return null;
			}
			return SecureStringExtension.CreateFromUnsecureString(string.Format(CultureInfo.InvariantCulture, "TASKSCHEDULER:{0}", userGuidForTaskScheduler));
		}
	}

	private bool FallbackToOfficeEntitlements
	{
		get
		{
			if (!FeatureSwitches.Instance.DisableOfficeEntitlements)
			{
				return navAppId == "INV";
			}
			return false;
		}
	}

	internal bool PendingUserActionRequired => userActionRequired != UserActionRequired.None;

	internal DateTime TokenExpiresOn => federatedSecurityToken?.UtcExpiresOn ?? DateTime.UtcNow.AddMinutes(5.0);

	internal string CacheKey => NavUserAuthenticationCache.CreateCacheKey(navAppId, AuthenticatedUserId);

	public string AppId => navAppId ?? ServerUserSettings.Instance.DefaultApplicationId.Value;

	public string UserTypeDescription
	{
		get
		{
			NavUser user = User;
			if (user != null && user.IsDelegatedAdmin)
			{
				return "DELEGATED_ADMIN";
			}
			NavUser user2 = User;
			if (user2 != null && user2.IsDelegatedHelpdesk)
			{
				return "DELEGATED_HELPDESK";
			}
			if (!string.IsNullOrEmpty(SecurityToken?.GetClaimValue(SecurityTokenClaimType.Wids)))
			{
				return "INTERNAL_ADMIN";
			}
			return userType.ToString();
		}
	}

	internal Guid ClientActivityId => clientActivityId;

	public bool IsDeviceUser { get; private set; }

	private NavUserAuthentication()
	{
	}

	public NavUserAuthentication(NavTenant tenant, NavClientCredentialType credentialType, Func<WindowsIdentity> getWindowsIdentity = null)
		: this(tenant, credentialType, null, getWindowsIdentity)
	{
	}

	public NavUserAuthentication(NavTenant tenant, NavClientCredentialType credentialType, object token, Func<WindowsIdentity> getWindowsIdentity = null, string navAppId = null, INavLicenseServiceClient licenseClient = null)
		: this(tenant, credentialType, UserType.Normal, token, null, null, null, navAppId, getWindowsIdentity, licenseClient)
	{
	}

	public NavUserAuthentication(NavTenant tenant, NavClientCredentialType credentialType, object token, string userName, string authenticationKey, AuthorizationCodeContext authorizationCodeContext, string navAppId)
		: this(tenant, credentialType, UserType.Normal, token, userName, authenticationKey, authorizationCodeContext, navAppId)
	{
	}

	private NavUserAuthentication(NavTenant tenant, NavClientCredentialType authenticationType, UserType userType, object token = null, string userName = null, string authenticationKey = null, AuthorizationCodeContext authorizationCodeContext = null, string navAppId = null, Func<WindowsIdentity> getWindowsIdentity = null, INavLicenseServiceClient licenseClient = null)
	{
		Tenant = tenant;
		this.userType = userType;
		AuthenticationMethod = GetValidatedAuthenticationType(authenticationType);
		this.userName = userName;
		this.authenticationKey = authenticationKey;
		AuthorizationCodeContext = authorizationCodeContext;
		this.getWindowsIdentity = getWindowsIdentity;
		navLicenseServiceClient = licenseClient;
		if (NavCurrentThread.ClientActivityId.HasValue && NavCurrentThread.ClientActivityId.Value != Guid.Empty)
		{
			clientActivityId = NavCurrentThread.ClientActivityId.Value;
		}
		else
		{
			NavCurrentThread.ClientActivityId = clientActivityId;
		}
		if (string.IsNullOrEmpty(navAppId))
		{
			this.navAppId = ServerUserSettings.Instance.DefaultApplicationId.Value;
		}
		else if (!ApplicationIdTypes.KnownApplicationIdTypes.Contains(navAppId))
		{
			Tenant.Diagnostics.SendTraceTag("00000LO", Category.Security, Verbosity.Warning, PrivacyClassification.SystemMetadata, "The specified application ID {0} is not valid. StackTrace: {1}", navAppId, Environment.StackTrace);
			this.navAppId = ServerUserSettings.Instance.DefaultApplicationId.Value;
		}
		else
		{
			this.navAppId = navAppId.ToUpperInvariant();
		}
		switch (authenticationType)
		{
		case NavClientCredentialType.TaskScheduler:
			if (!(token is NavTaskSchedulerAuthenticationToken navTaskSchedulerAuthenticationToken))
			{
				throw new NavNCLArgumentException("taskSchedulerToken");
			}
			userGuidForTaskScheduler = navTaskSchedulerAuthenticationToken.UserId;
			this.userName = navTaskSchedulerAuthenticationToken.UserName;
			Tenant.Diagnostics.SendTraceTag("0000082", Category.Security, Verbosity.Normal, "Received task scheduler security token:" + Environment.NewLine + "{0}", DiagnosticParameter.Eupi(userGuidForTaskScheduler));
			break;
		case NavClientCredentialType.NavUserPassword:
			if (token == null)
			{
				throw new ArgumentNullException("token");
			}
			userNameSecurityToken = token as UserNameSecurityToken;
			break;
		case NavClientCredentialType.AccessControlService:
			federatedSecurityToken = token as INavSecurityToken;
			Tenant.Diagnostics.SendTraceTag("0000080", Category.Security, Verbosity.Normal, "Received security token:" + Environment.NewLine + "{0}", federatedSecurityToken.GetDetailsForLog());
			Tenant.Diagnostics.SendTraceTag("00006NH", Category.Security, Verbosity.Normal, "Received security token:" + Environment.NewLine + "{0}", federatedSecurityToken.GetTransientIdentityDetailsForLog());
			break;
		case NavClientCredentialType.ExchangeIdentity:
			exchangeIdentifierSecurityToken = token as NavExchangeIdentifierSecurityToken;
			Tenant.Diagnostics.SendTraceTag("0000083", Category.Security, Verbosity.Normal, "Received exchange identity token:" + Environment.NewLine + "{0}", exchangeIdentifierSecurityToken.GetDetailsForLog());
			Tenant.Diagnostics.SendTraceTag("00006NI", Category.Security, Verbosity.Normal, "Received exchange identity token:" + Environment.NewLine + "{0}", exchangeIdentifierSecurityToken.GetTransientIdentityDetailsForLog());
			break;
		}
		Authenticate();
		if (Authenticated)
		{
			Authorize();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public static NavUserAuthentication CreateAdminUserAuthentication(NavTenant tenant)
	{
		return new NavUserAuthentication(tenant, NavClientCredentialType.Windows, UserType.Admin);
	}

	internal static NavUserAuthentication CreateNasUserAuthentication(NavTenant tenant)
	{
		if (!tenant.TenantSettings.RunNasWithAdminRights)
		{
			return new NavUserAuthentication(tenant, NavClientCredentialType.Windows);
		}
		return new NavUserAuthentication(tenant, NavClientCredentialType.Windows, UserType.Nas);
	}

	public static NavUserAuthentication CreateSystemAuthentication(NavTenant tenant)
	{
		return new NavUserAuthentication(tenant, NavClientCredentialType.None, UserType.Admin);
	}

	public static NavUserAuthentication CreateTaskSchedulerAuthentication(NavTenant tenant, Guid userId, string userName, string navAppId)
	{
		NavTaskSchedulerAuthenticationToken token = new NavTaskSchedulerAuthenticationToken(userId, userName);
		Func<object, NavUserAuthentication> authenticationFunc = (object securityToken) => new NavUserAuthentication(tenant, NavClientCredentialType.TaskScheduler, UserType.Normal, securityToken, null, null, null, navAppId);
		using (tenant.Diagnostics.CreateTraceScope("00006XH", DiagnosticParameter.SystemMetadata("Trying to retrieve NavUserAuthentication from the cache"), MonitorType.None, Category.Security))
		{
			return tenant.AuthenticationCache.GetNavUserAuthentication(token, navAppId, tenant.Diagnostics, authenticationFunc, null, NavClientCredentialType.TaskScheduler);
		}
	}

	private static NavClientCredentialType GetValidatedAuthenticationType(NavClientCredentialType authenticationType)
	{
		if ((uint)authenticationType <= 5u)
		{
			return authenticationType;
		}
		return NavClientCredentialType.None;
	}

	private void Authenticate()
	{
		navUser = null;
		if (federatedSecurityToken != null && !IsAuthenticatingAsApplication && UseEntitlements)
		{
			AuthenticateAzureADCredential();
			return;
		}
		switch (AuthenticationMethod)
		{
		case NavClientCredentialType.Windows:
		case NavClientCredentialType.UserName:
			AuthenticateWindowsCredential();
			break;
		case NavClientCredentialType.NavUserPassword:
			AuthenticateNavUserPasswordCredential();
			break;
		case NavClientCredentialType.ExchangeIdentity:
			AuthenticateExchangeIdentityCredential();
			break;
		case NavClientCredentialType.TaskScheduler:
			AuthenticateTaskSchedulerCredential();
			break;
		case NavClientCredentialType.AccessControlService:
			AuthenticateFederatedCredential();
			break;
		case NavClientCredentialType.None:
			if (userType == UserType.Admin)
			{
				navUser = NavUser.CreateDefaultSystemUser();
				SkipTenantLicenseStateCheck = true;
			}
			break;
		}
		if (navUser != null && !NavUser.TryAuthenticateCore(navUser))
		{
			userActionRequired = UserActionRequired.None;
			navUser = null;
		}
		else if (!IsEntitled)
		{
			GetUserLicenseDetailsByObjectId(navUser?.AuthenticationObjectId);
		}
	}

	private void GetUserLicenseDetailsByObjectId(string userObjectId)
	{
		if (string.IsNullOrWhiteSpace(userObjectId))
		{
			UserLicenseDetails = null;
			return;
		}
		try
		{
			UserLicenseDetails = LicenseServiceClient.GetLicenseDetailsByObjectId(Tenant.TenantSettings.AadTenantId, userObjectId, Tenant.Diagnostics);
		}
		catch (NavLicenseServiceException exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001W0", Category.License, exception);
			UserLicenseDetails = null;
		}
	}

	private void GetUserLicenseDetailsByUpn(string upn)
	{
		if (string.IsNullOrWhiteSpace(upn))
		{
			UserLicenseDetails = null;
			return;
		}
		try
		{
			UserLicenseDetails = LicenseServiceClient.GetLicenseDetailsByUpn(SecurityToken.SerializedToken, Tenant.TenantSettings.AadTenantId, upn, Tenant.Diagnostics);
		}
		catch (NavLicenseServiceException exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001W1", Category.License, exception);
			UserLicenseDetails = null;
		}
	}

	private void AuthenticateAzureADCredential()
	{
		if (AuthenticatedUserObjectId == null && !string.IsNullOrEmpty(federatedSecurityToken.GetClaimValue(SecurityTokenClaimType.Wids)))
		{
			AuthenticateAzureADUserByNameIdentifier();
		}
		else
		{
			AuthenticateAzureADTenantUserByObjectId();
		}
	}

	private void AuthenticateAzureADUserByNameIdentifier()
	{
		Tenant.Diagnostics.SendTraceTag("0000084", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authenticating using Azure AD Partner Credential");
		string claimValue = federatedSecurityToken.GetClaimValue(SecurityTokenClaimType.NameIdentifier);
		if (string.IsNullOrEmpty(claimValue))
		{
			Tenant.Diagnostics.SendTraceTag("0000085", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Token was identified as a partner credential but the name identifier claim is empty.");
			return;
		}
		if (!Tenant.Database.NavUsers.TryGetNavUserFromNameIdentifier(claimValue, out navUser))
		{
			if (UserLicenseDetails == null)
			{
				GetUserLicenseDetailsByUpn(AuthenticatedUserAuthorizationEmail);
			}
			CreateJitNavUserObject();
			return;
		}
		if (UserLicenseDetails == null && !string.IsNullOrWhiteSpace(User.AuthenticationObjectId))
		{
			GetUserLicenseDetailsByObjectId(User.AuthenticationObjectId);
		}
		string claimValue2 = federatedSecurityToken.GetClaimValue(SecurityTokenClaimType.Wids);
		if (navUser.DirectoryRoleId != claimValue2)
		{
			navUser.ReplaceDirectoryRoleId(claimValue2);
			userActionRequired = UserActionRequired.UpdateDirectoryRoleId;
		}
	}

	private void AuthenticateAzureADTenantUserByObjectId()
	{
		Tenant.Diagnostics.SendTraceTag("000005D", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authenticating using Azure AD Tenant Credential");
		GetUserLicenseDetailsByObjectId(AuthenticatedUserObjectId);
		if (UserLicenseDetails != null && !Tenant.Database.NavUsers.TryGetNavUserFromAuthenticationObjectId(AuthenticatedUserObjectId, out navUser))
		{
			CreateJitNavUserObject();
		}
	}

	private void AuthenticateFederatedCredential()
	{
		Tenant.Diagnostics.SendTraceTag("000005E", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authenticating using Federated Credential");
		NavWSFederationUserAuthenticator navWSFederationUserAuthenticator = new NavWSFederationUserAuthenticator(federatedSecurityToken.GetClaimValue, userName, authenticationKey, Tenant?.TenantSettings?.AadTenantId, Tenant.Diagnostics);
		navWSFederationUserAuthenticator.TryAuthenticateUser(Tenant.Database.NavUsers, out navUser, out userActionRequired);
		if (navUser == null)
		{
			string text = string.Format(CultureInfo.InvariantCulture, "The received security token could be validated but does not give access to {0}.", ProductNames.ProductNameShort);
			LogWriterHelper.LogWith(NavServerEventSource.Log.WriteAuthenticationSubsystemInfoMessage, string.Format(CultureInfo.InvariantCulture, text + Environment.NewLine + "Token Details : {0}", federatedSecurityToken.GetDetailsForLog()));
			Tenant.Diagnostics.SendTraceTag("00006NJ", Category.Security, Verbosity.Normal, text + Environment.NewLine + "Token Details : {0}", federatedSecurityToken.GetTransientIdentityDetailsForLog());
		}
	}

	private void AuthenticateTaskSchedulerCredential()
	{
		Tenant.Diagnostics.SendTraceTag("000005F", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authenticating using Task Scheduler Credential");
		if (Tenant.Database.NavUsers.TryGetNavUserFromId(userGuidForTaskScheduler, out this.navUser))
		{
			if (this.navUser.Name != userName)
			{
				this.navUser = null;
			}
			NavUser navUser = this.navUser;
			if (navUser != null && navUser.IsDelegatedUser)
			{
				throw new NavCreateScheduledTasksNotAllowedException();
			}
			if (!string.IsNullOrEmpty(this.navUser?.Sid))
			{
				this.navUser?.TryUpdateUserGroupsFromTenantCache(Tenant);
			}
		}
		else if (Tenant.Database.NavUsers.IsEmpty && userGuidForTaskScheduler == NavUser.DefaultSuperGuid)
		{
			this.navUser = NavUser.CreateDefaultNamedSuperUser(userName);
		}
	}

	private void AuthenticateExchangeIdentityCredential()
	{
		Tenant.Diagnostics.SendTraceTag("000005G", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authenticating using Exchange Identity Credential");
		Tenant.Database.NavUsers.TryGetNavUserFromExchangeIdentifier(exchangeIdentifierSecurityToken.TokenId, out navUser);
	}

	private void AuthenticateNavUserPasswordCredential()
	{
		Tenant.Diagnostics.SendTraceTag("000005H", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authenticating using NavUserPassword Credential");
		if (Tenant.Database.NavUsers.TryGetNavUserFromUserName(userNameSecurityToken.UserName, out navUser) && !NavUser.TryAuthenticate(navUser, userNameSecurityToken, Tenant))
		{
			navUser = null;
		}
	}

	private void AuthenticateWindowsCredential()
	{
		Tenant.Diagnostics.SendTraceTag("00007GB", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "User type: " + (userType.ToString() ?? string.Empty) + "; Tenant type: " + (Tenant?.TenantType).ToString());
		if (IsAuthenticatedDelegatedWindowsNavAdminUser)
		{
			bool isEmpty = Tenant.Database.NavUsers.IsEmpty;
			Tenant.Diagnostics.SendTraceTag("00001ZA", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Delegated authentication service allowed the user to login as NavAdmin. SUPER rights: {0}", isEmpty.ToString(CultureInfo.InvariantCulture));
			navUser = NavUser.CreateDefaultDelegatedWindowsSuperUser(ServiceSecurityContext.Current.PrimaryIdentity.Name, isEmpty, isNavAdminUser: true);
			return;
		}
		Tenant.Diagnostics.SendTraceTag("000005I", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authenticating using Windows Credential");
		try
		{
			if (WindowsIdentity == null)
			{
				navUser = null;
				Tenant.Diagnostics.SendTraceTag("00007LK", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "WindowsIdentity object not set.");
			}
			else if (userType == UserType.Admin && Tenant.TenantType == NavTenantType.System && NavAdministration.IsNavAdministrator(WindowsIdentity))
			{
				navUser = NavUser.CreateDefaultWindowsSuperUser(WindowsIdentity, isDefaultSuper: false, isNavAdminUser: true);
			}
			else if (userType == UserType.Nas)
			{
				navUser = NavUser.CreateDefaultWindowsSuperUser(WindowsIdentity, isDefaultSuper: false, isNavAdminUser: true);
			}
			else if (Tenant.Database.NavUsers.TryGetNavUserFromSid(WindowsIdentity.User.Value, out navUser))
			{
				if (!NavUser.TryAuthenticate(navUser, WindowsIdentity))
				{
					navUser = null;
				}
			}
			else if (Tenant.Database.NavUsers.IsEmpty)
			{
				navUser = NavUser.CreateDefaultWindowsSuperUser(WindowsIdentity, isDefaultSuper: true, userType == UserType.Admin);
			}
		}
		catch (NullReferenceException exception)
		{
			Tenant.Diagnostics.SendExceptionTag("00007H5", Category.Security, exception);
			navUser = null;
		}
	}

	private void CreateJitNavUserObject()
	{
		Tenant.Diagnostics.SendTraceTag("000005J", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Creating NavUser object instance from Azure AD User");
		navUser = CreateNavUserFromLicenseServiceClientUser();
		Tenant.Diagnostics.SendTraceTag("0000029", Category.Security, Verbosity.Normal, "New user ({0} - {1}) prepared from aadUser: {2}", DiagnosticParameter.Eupi(navUser.Id), DiagnosticParameter.Euii(navUser.Name), DiagnosticParameter.Euii(navUser.ContactEmail));
		userActionRequired = UserActionRequired.InitializeAsNewUser;
	}

	private NavUser CreateNavUserFromLicenseServiceClientUser()
	{
		LicenseDetails userLicenseDetails = UserLicenseDetails;
		UserInfo val = ((userLicenseDetails != null) ? userLicenseDetails.User : null);
		UserSecurityTokenHelper tokenInfo = UserSecurityTokenHelper.Create((val != null) ? ((UserInfo)val).ObjectId : null, SecurityToken, val);
		return NavUser.CreateUserFromNavAzureADUserTokenInfo(tokenInfo);
	}

	private void Authorize()
	{
		if (UseEntitlements)
		{
			AuthorizeMembershipEntitlements();
		}
	}

	private void AuthorizeMembershipEntitlements()
	{
		Tenant.Diagnostics.SendTraceTag("000005K", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authorizing Membership Entitlements");
		EntitlementProvider entitlementProvider = NavGlobal.SystemTenant.EntitlementProvider;
		HashSet<NavMembershipEntitlement> hashSet = new HashSet<NavMembershipEntitlement>();
		if (IsAuthenticatingAsApplication)
		{
			Tenant.Diagnostics.SendTraceTag("000005L", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authorizing entitlements through Application ID membership.");
			hashSet.UnionWith(entitlementProvider.SelectMembershipEntitlementsByTypeAndId(MembershipEntitlementType.ApplicationID, TokenApplicationId, Tenant.Diagnostics));
			SkipTenantLicenseStateCheck = true;
		}
		else
		{
			string cacheKey = string.Format(CultureInfo.InvariantCulture, "{0} Entitlements for App {1}", User.Id.ToString("D"), navAppId);
			NavMembershipEntitlement[] fromCache = Tenant.GetFromCache<NavMembershipEntitlement[]>(cacheKey);
			if (fromCache != null && fromCache.Any())
			{
				Tenant.Diagnostics.SendTraceTag("00000ZK", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Reusing cached license entitlements for user.");
				hashSet.UnionWith(fromCache);
			}
			else
			{
				AuthorizeMembershipEntitlementFromAppSpecificLicense(hashSet);
				if (hashSet.Any())
				{
					Tenant.AddToCache(cacheKey, hashSet.ToArray(), DateTime.UtcNow.Add(ServerUserSettings.Instance.AzureActiveDirectoryMaxUserLicenseCacheTime.Value), ServerUserSettings.Instance.AzureActiveDirectoryIdleUserLicenseCacheTime.Value);
				}
			}
		}
		Tenant.Diagnostics.SendTraceTag("000005N", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authorizing entitlement through Nav User Id membership.");
		hashSet.UnionWith(GetNavUserEntitlements(entitlementProvider, navUser));
		if (hashSet.Count == 0 && entitlementProvider.IsAzureADUserValidDeviceUser(UserLicenseDetails, Tenant))
		{
			List<string> tenantDeviceServicePlans = entitlementProvider.GetTenantDeviceServicePlans(UserLicenseDetails);
			hashSet.UnionWith(entitlementProvider.SelectMembershipEntitlementsByTypeAndIds(MembershipEntitlementType.AzureADDevicePlan, tenantDeviceServicePlans, Tenant.Diagnostics));
			IsDeviceUser = true;
		}
		if (hashSet.Count == 0)
		{
			Tenant.Diagnostics.SendTraceTag("00000QE", Category.Security, Verbosity.Normal, "The received security token was validated but it does not have any service plans or roles that can be mapped to entitlements." + Environment.NewLine + "{0}", GetTokenLogDetails());
			Tenant.Diagnostics.SendTraceTag("00006NK", Category.Security, Verbosity.Normal, "The received security token was validated but it does not have any service plans or roles that can be mapped to entitlements." + Environment.NewLine + "{0}", GetTokenLogDetails(logPrivateInfo: true));
			navUser = null;
			userActionRequired = UserActionRequired.None;
		}
		else
		{
			Tenant.Diagnostics.SendTraceTag("000005O", Category.Security, Verbosity.Normal, "User signed in with {0};User ID:{1};Tenant ID:{2}", DiagnosticParameter.SystemMetadata(ExtractCurrentUserTypeAndLicenseTypeForStatistics(hashSet)), DiagnosticParameter.Eupi(User?.Id), DiagnosticParameter.Oii(Tenant?.Id));
			navUser.MembershipEntitlements = new UserMembershipEntitlements(hashSet);
		}
	}

	private void AuthorizeMembershipEntitlementFromAppSpecificLicense(HashSet<NavMembershipEntitlement> userEntitlements)
	{
		switch (navAppId)
		{
		case "ACC":
		case "FIN":
		case "INV":
		{
			UserSecurityTokenHelper userSecurityTokenHelper = UserSecurityTokenHelper.Create(AuthenticatedUserObjectId, SecurityToken);
			if (navUser.IsDelegatedUser)
			{
				Tenant.Diagnostics.SendTraceTag("00000LP", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authorizing entitlements through Azure AD claim role membership.");
				userEntitlements.UnionWith(NavGlobal.SystemTenant.EntitlementProvider.SelectAzureADClaimRoleMembershipEntitlements(userSecurityTokenHelper, Tenant.Diagnostics));
				if (userEntitlements.Any())
				{
					SkipTenantLicenseStateCheck = true;
				}
				break;
			}
			if (userSecurityTokenHelper == null)
			{
				Tenant.Diagnostics.SendTraceTag("00000KY", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Not authorizing entitlements through Azure AD User membership, azureADLoginUserInfo is null.");
				break;
			}
			bool isAdminUser = false;
			bool hasSubscribedSkus = false;
			bool hasEnabledSubscribedSkus = false;
			LicenseDetails userLicenseDetails = UserLicenseDetails;
			if (((userLicenseDetails != null) ? userLicenseDetails.User : null) == null)
			{
				Tenant.Diagnostics.SendTraceTag("00001DH", Category.Security, Verbosity.Normal, "Authorizing entitlements through Azure AD User and Role memberships cannot be performed as no AD user was found. UPN: {0}, Name Identifier: {1}.", DiagnosticParameter.Euii(userSecurityTokenHelper.AuthenticationEmail), DiagnosticParameter.Euii(userSecurityTokenHelper.NameIdentifier));
			}
			else
			{
				Tenant.Diagnostics.SendTraceTag("00000LQ", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Authorizing entitlements through Azure AD User and Role memberships. APPID: {0}", AppId);
				bool refreshLicenseDetails = false;
				userEntitlements.UnionWith(GetAzureADUserAndRolesEntitlements(NavGlobal.SystemTenant.EntitlementProvider, () =>
				{
					if (refreshLicenseDetails)
					{
						GetUserLicenseDetailsByObjectId(AuthenticatedUserObjectId);
					}
					else
					{
						refreshLicenseDetails = true;
					}
					return UserLicenseDetails;
				}, (!FallbackToOfficeEntitlements) ? 5 : 0, out isAdminUser, out hasSubscribedSkus, out hasEnabledSubscribedSkus));
			}
			if (isAdminUser)
			{
				SkipTenantLicenseStateCheck = true;
				Tenant.Diagnostics.SendTraceTag("0000766", Category.Security, Verbosity.Normal, "Authorized Azure AD User is an internal admin. UPN: {0}", DiagnosticParameter.Euii(userSecurityTokenHelper.AuthenticationEmail));
			}
			if (FallbackToOfficeEntitlements)
			{
				if (userEntitlements.Count > 0 && !isAdminUser)
				{
					Tenant.Diagnostics.SendTraceTag("00006TC", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Tenant: {0} uses a SubscribedSkus License for Invoicing", Tenant?.Id);
					InvoicingTenantAuthMetric.LogValue(2L, Tenant.Id, "AAD License");
				}
				else
				{
					Tenant.Diagnostics.SendTraceTag("00006TD", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Tenant: {0} does not use a SubscribedSkus License for Invoicing, switching to OfficeEntitlements License", Tenant?.Id);
					CheckOfficeEntitlements(userEntitlements);
					InvoicingTenantAuthMetric.LogValue(1L, Tenant.Id, "Office Entitlements");
				}
			}
			if (navAppId != "INV")
			{
				TenantLicenseStateHelper.CheckAndShiftState(Tenant, hasSubscribedSkus, hasEnabledSubscribedSkus);
			}
			break;
		}
		default:
			Tenant.Diagnostics.SendTraceTag("00000LM", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "Unexpected Application ID: {0}. StackTrace: {1}", navAppId, Environment.StackTrace);
			break;
		}
	}

	internal void LoadEntitlementsSimulatedForUser(NavUser user, EntitlementProvider entitlementProvider, INavLicenseServiceClient licenseServiceClient = null)
	{
		if (UseEntitlements)
		{
			if (licenseServiceClient == null)
			{
				licenseServiceClient = LicenseServiceClient;
			}
			LicenseDetails licenseDetails = licenseServiceClient.GetLicenseDetailsByObjectId(Tenant.TenantSettings.AadTenantId, user.AuthenticationObjectId, Tenant.Diagnostics);
			Tenant.Diagnostics.SendTraceTag("000028W", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Loading membership entitlements for user {0}.", DiagnosticParameter.Eupi(user.Id));
			HashSet<NavMembershipEntitlement> hashSet = new HashSet<NavMembershipEntitlement>();
			hashSet.UnionWith(GetAzureADUserAndRolesEntitlements(entitlementProvider, () => licenseDetails, 0, out var _, out var _, out var _));
			hashSet.UnionWith(GetNavUserEntitlements(entitlementProvider, user));
			user.MembershipEntitlements = new UserMembershipEntitlements(hashSet);
		}
	}

	private IEnumerable<NavMembershipEntitlement> GetAzureADUserAndRolesEntitlements(EntitlementProvider entitlementProvider, Func<LicenseDetails> licenseDetailsProvider, int numOfRetries, out bool isAdminUser, out bool hasSubscribedSkus, out bool hasEnabledSubscribedSkus)
	{
		return entitlementProvider.SelectAzureADUserAndRolesMembershipEntitlements(licenseDetailsProvider, Tenant.Diagnostics, out isAdminUser, out hasSubscribedSkus, out hasEnabledSubscribedSkus, numOfRetries);
	}

	private IEnumerable<NavMembershipEntitlement> GetNavUserEntitlements(EntitlementProvider entitlementProvider, NavUser user)
	{
		return entitlementProvider.SelectNavUserMembershipEntitlements(user, Tenant.Diagnostics);
	}

	private void CheckOfficeEntitlements(HashSet<NavMembershipEntitlement> userEntitlements)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Invalid comparison between Unknown and I4
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Invalid comparison between Unknown and I4
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		OfficeEntitlementStatus val = IsInvoiceEntitlementsEnabledForUser(federatedSecurityToken?.AuthorizationEmail ?? User?.AuthenticationEmail);
		if ((int)val == 4 || (int)val == 2 || (int)val == 3)
		{
			Tenant.Diagnostics.SendTraceTag("00006NN", Category.Security, Verbosity.Warning, "OfficeEntitlements has not been enabled for user: {0}", GetTokenLogDetails(logPrivateInfo: true));
			if (userEntitlements.Count > 0)
			{
				return;
			}
			if (ServerUserSettings.Instance.EnableInvoiceEntitlementCheck.Value)
			{
				string officeEntitlementStatusFriendlyMessage = GetOfficeEntitlementStatusFriendlyMessage(val);
				throw Tenant.Diagnostics.SendExceptionTag("000011R", Category.Security, new NavInvalidCredentialException(officeEntitlementStatusFriendlyMessage));
			}
		}
		Tenant.Diagnostics.SendTraceTag("00006NM", Category.Security, Verbosity.Normal, "OfficeEntitlements has been granted for user: {0}", GetTokenLogDetails(logPrivateInfo: true));
		userEntitlements.UnionWith(NavGlobal.SystemTenant.EntitlementProvider.SelectMembershipEntitlementsByTypeAndId(MembershipEntitlementType.NAVApplicationID, "07EB0DC4-7DA7-4E7B-BB42-2D44C5E08B08", Tenant.Diagnostics));
	}

	private static string ExtractCurrentUserTypeAndLicenseTypeForStatistics(HashSet<NavMembershipEntitlement> membershipEntitlements)
	{
		if (membershipEntitlements == null)
		{
			throw new ArgumentNullException("membershipEntitlements");
		}
		string text = string.Join(",", from t in membershipEntitlements.Where((NavMembershipEntitlement t) => t.Type != MembershipEntitlementType.AzureADPlan).DistinctBy((NavMembershipEntitlement t) => t.Name)
			select t.Name);
		string text2 = (string.IsNullOrEmpty(text) ? string.Empty : (text + ","));
		text = (string.IsNullOrEmpty(text) ? "Normal User" : text);
		text2 += string.Join(",", from t in membershipEntitlements.Where((NavMembershipEntitlement t) => t.Type == MembershipEntitlementType.AzureADPlan).DistinctBy((NavMembershipEntitlement t) => t.Name)
			select t.Name);
		return string.Format(CultureInfo.CurrentCulture, "User type: {0};License type: {1}", text, text2);
	}

	public NavUserAuthentication Clone()
	{
		NavUserAuthentication navUserAuthentication = new NavUserAuthentication
		{
			Tenant = Tenant,
			navUser = navUser,
			userType = userType,
			AuthenticationMethod = AuthenticationMethod,
			userNameSecurityToken = userNameSecurityToken,
			federatedSecurityToken = federatedSecurityToken,
			exchangeIdentifierSecurityToken = exchangeIdentifierSecurityToken,
			SkipTenantLicenseStateCheck = SkipTenantLicenseStateCheck,
			navAppId = navAppId,
			UserLicenseDetails = UserLicenseDetails,
			AuthorizationCodeContext = AuthorizationCodeContext,
			IsDeviceUser = IsDeviceUser
		};
		if (WindowsIdentity != null)
		{
			navUserAuthentication.windowsIdentity = new WindowsIdentity(WindowsIdentity.Token);
		}
		return navUserAuthentication;
	}

	public void UpdateNavUserFromSecurityToken()
	{
		if (federatedSecurityToken == null)
		{
			Tenant.Diagnostics.SendTraceTag("000004D", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Cannot update Nav User from empty security token.");
			return;
		}
		string claimValue = federatedSecurityToken.GetClaimValue(SecurityTokenClaimType.Puid);
		navUser.EnsurePuid(claimValue);
		if (UserActionRequired != UserActionRequired.None)
		{
			NavSession session = NavCurrentThread.Session;
			switch (UserActionRequired)
			{
			case UserActionRequired.UpdateNameIdentifier:
			{
				Tenant.Diagnostics.SendTraceTag("00001C6", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Update user name from security token.");
				string claimValue4 = federatedSecurityToken.GetClaimValue(SecurityTokenClaimType.NameIdentifier);
				UserManagement.SetNameIdentifier(session, session.User.Id, claimValue4);
				navUser.ReplaceNameIdentifier(claimValue4);
				break;
			}
			case UserActionRequired.UpdateAuthenticationObjectId:
			{
				Tenant.Diagnostics.SendTraceTag("00001C7", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Update user authentication object Id from security token.");
				string claimValue3 = federatedSecurityToken.GetClaimValue(SecurityTokenClaimType.ObjectIdentifier);
				session.Diagnostics.SendTraceTag("00001CC", Category.Security, Verbosity.Normal, "Replacing AAD Object ID : AadObjectIdBefore={0}, AadObjectIdAfter={1} ", DiagnosticParameter.Eupi(navUser?.AuthenticationObjectId), DiagnosticParameter.Eupi(claimValue3));
				UserManagement.DirectSetAuthenticationObjectId(session, session.User.Id, claimValue3);
				navUser.ReplaceAuthenticationObjectId(claimValue3);
				break;
			}
			case UserActionRequired.UpdateDirectoryRoleId:
			{
				Tenant.Diagnostics.SendTraceTag("00001C8", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Update user directory role Id from security token.");
				string claimValue2 = federatedSecurityToken.GetClaimValue(SecurityTokenClaimType.Wids);
				UserManagement.DirectSetDirectoryRoleId(session, session.User.Id, claimValue2);
				navUser.ReplaceDirectoryRoleId(claimValue2);
				break;
			}
			case UserActionRequired.UpdateAuthenticationEmail:
			{
				Tenant.Diagnostics.SendTraceTag("00001C9", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Update user authentication email from security token.");
				string authorizationEmail = federatedSecurityToken.AuthorizationEmail;
				UserManagement.DirectSetAuthenticationEmail(session, session.User.Id, authorizationEmail);
				navUser.ReplaceAuthenticationEmail(authorizationEmail);
				break;
			}
			case UserActionRequired.InitializeAsNewUser:
				Tenant.Diagnostics.SendTraceTag("00001CA", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Initialize new user from security token.");
				InitializeNewUser();
				break;
			}
			userActionRequired = UserActionRequired.None;
		}
	}

	public void InitializeNewUser()
	{
		if (UserActionRequired == UserActionRequired.InitializeAsNewUser)
		{
			navUser = UserManagement.SaveOrRefreshUser(Tenant, navUser, enableSuperRole: true);
		}
	}

	internal void ObtainUserAccessToken(NavSession session)
	{
		if (!AuthorizationCodeContext.IsNullOrEmpty(AuthorizationCodeContext) && session != null && Uri.TryCreate(AuthorizationCodeContext.RedirectUrl, UriKind.Absolute, out var result))
		{
			LogWriterHelper.LogEvent(EventLogEntryType.Information, "Retrieving AAD access token for user by authorization code.");
			ALAzureAdCodeGrantFlow aLAzureAdCodeGrantFlow = new ALAzureAdCodeGrantFlow(result, session);
			string value = aLAzureAdCodeGrantFlow.ALAcquireTokenByAuthorizationCode(AuthorizationCodeContext.AuthorizationCode, AzureADAuthenticationHelper.GraphApiEndpoint);
			if (!string.IsNullOrWhiteSpace(value))
			{
				AuthorizationCodeContext = null;
				Tenant.AuthenticationCache.UpdateCache(this);
			}
		}
	}

	internal bool RefreshUserAuthorization(AuthorizationCodeContext authorizationCodeContext, object token, NavClientCredentialType clientCredentialType)
	{
		bool result = false;
		if (!AuthorizationCodeContext.IsNullOrEmpty(authorizationCodeContext) && !authorizationCodeContext.Equals(AuthorizationCodeContext))
		{
			result = true;
			AuthorizationCodeContext = authorizationCodeContext;
		}
		if (AuthenticationMethod != clientCredentialType)
		{
			result = true;
			AuthenticationMethod = clientCredentialType;
		}
		switch (AuthenticationMethod)
		{
		case NavClientCredentialType.AccessControlService:
		{
			INavSecurityToken navSecurityToken = token as INavSecurityToken;
			if (navSecurityToken != federatedSecurityToken)
			{
				result = true;
				federatedSecurityToken = navSecurityToken;
			}
			break;
		}
		case NavClientCredentialType.NavUserPassword:
		{
			NavUserNameBasicAuthenticationToken navUserNameBasicAuthenticationToken = token as NavUserNameBasicAuthenticationToken;
			if (navUserNameBasicAuthenticationToken != userNameSecurityToken)
			{
				result = true;
				userNameSecurityToken = navUserNameBasicAuthenticationToken;
			}
			break;
		}
		case NavClientCredentialType.ExchangeIdentity:
		{
			NavExchangeIdentifierSecurityToken navExchangeIdentifierSecurityToken = token as NavExchangeIdentifierSecurityToken;
			if (navExchangeIdentifierSecurityToken != exchangeIdentifierSecurityToken)
			{
				result = true;
				exchangeIdentifierSecurityToken = navExchangeIdentifierSecurityToken;
			}
			break;
		}
		}
		return result;
	}

	internal void EnsureDeviceUserCanSignIn()
	{
		NavGlobal.SystemTenant.EntitlementProvider.ValidateAndUpdateConcurrentDeviceUserSignIn(UserLicenseDetails, Tenant);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && !disposed)
		{
			if (windowsIdentity != null)
			{
				windowsIdentity.Dispose();
				windowsIdentity = null;
			}
			navUser = null;
			AuthenticationMethod = NavClientCredentialType.None;
			disposed = true;
		}
	}

	private DiagnosticParameter GetTokenLogDetails(bool logPrivateInfo = false)
	{
		if (federatedSecurityToken != null)
		{
			if (!logPrivateInfo)
			{
				return federatedSecurityToken.GetDetailsForLog();
			}
			return federatedSecurityToken.GetTransientIdentityDetailsForLog();
		}
		if (exchangeIdentifierSecurityToken != null)
		{
			if (!logPrivateInfo)
			{
				return exchangeIdentifierSecurityToken.GetDetailsForLog();
			}
			return exchangeIdentifierSecurityToken.GetTransientIdentityDetailsForLog();
		}
		if (userNameSecurityToken != null)
		{
			return DiagnosticParameter.Euii(userNameSecurityToken.UserName);
		}
		return DiagnosticParameter.SystemMetadata("No token info.");
	}

	private static string GetOfficeEntitlementStatusFriendlyMessage(OfficeEntitlementStatus status)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if ((int)status != 2)
		{
			return Lang.InvoiceAppNotAccessible;
		}
		return Lang.InvoiceAppThankYouForYourInterest;
	}

	private OfficeEntitlementStatus IsInvoiceEntitlementsEnabledForUser(string userPrincipalName, bool waitOnStatus = true)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(userPrincipalName))
		{
			return (OfficeEntitlementStatus)4;
		}
		OfficeEntitlementStatus result = Tenant.OfficeEntitlementCache.GetOfficeEntitlementStatus(userPrincipalName, Tenant.Diagnostics, (string userPrincipal) =>
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			return LicenseServiceClient.GetOfficeEntitlementStatusForUser(Tenant.TenantSettings.AadTenantId, userPrincipal, Tenant.Diagnostics);
		});
		if (waitOnStatus)
		{
			result = Tenant.OfficeEntitlementCache.WaitOnStatus(userPrincipalName);
		}
		return result;
	}

	private void EnsureLicenseClient()
	{
		if (navLicenseServiceClient == null)
		{
			navLicenseServiceClient = new NavLicenseServiceClient(Tenant);
		}
	}
}
