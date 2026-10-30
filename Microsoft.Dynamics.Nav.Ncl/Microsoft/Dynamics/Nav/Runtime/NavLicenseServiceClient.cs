using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService;
using Microsoft.Dynamics.Nav.LicensingService.Client;
using Microsoft.Dynamics.Nav.LicensingService.Model;
using Microsoft.Dynamics.Nav.LicensingService.Model.Exceptions;
using Microsoft.Dynamics.Nav.Runtime.Authentication;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavLicenseServiceClient : INavLicenseServiceClient
{
	private ILicenseService serviceClient;

	private NavTenant tenant;

	private static readonly NavMultidimensionMetric licenseServiceClientDurationMetric = new NavMultidimensionMetric("LicenseServiceDurationMetric", "Method", "ClientType");

	public NavLicenseServiceClient(NavTenant tenant)
	{
		this.tenant = tenant;
		serviceClient = CreateClient(this.tenant, DiagnosticsResolver.GetMostSpecificInstance());
	}

	private NavLicenseServiceClient()
	{
	}

	public LicenseDetails GetLicenseDetailsByObjectId(string aadTenantId, string aadUserObjectId, NavDiagnostics diagnostics)
	{
		return ExecuteLicenseServiceClientActionWithRetry(() => serviceClient.LicenseDetailsByObjectId(aadTenantId, aadUserObjectId), "GetLicenseDetailsByObjectId", diagnostics);
	}

	public LicenseDetails GetLicenseDetailsByUpn(SecureString callerIdentityToken, string aadTenantId, string addUserUpn, NavDiagnostics diagnostics)
	{
		return ExecuteLicenseServiceClientActionWithRetry(() => serviceClient.LicenseDetailsByUpn(callerIdentityToken.ConvertToUnsecureString(), aadTenantId, addUserUpn), "GetLicenseDetailsByUpn", diagnostics);
	}

	public OfficeEntitlementStatus GetOfficeEntitlementStatusForUser(string aadTenantId, string userPrincipalName, NavDiagnostics diagnostics)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		return ExecuteLicenseServiceClientActionWithRetry(() => serviceClient.OfficeEntitlementStatusForUser(aadTenantId, userPrincipalName), "GetOfficeEntitlementStatusForUser", diagnostics);
	}

	public IEnumerable<SkuInfo> GetDirectorySubscribedSkus(SecureString callerIdentityToken, string aadTenantId, NavDiagnostics diagnostics)
	{
		return ExecuteLicenseServiceClientActionWithRetry(() => serviceClient.GetDirectorySubscribedSkus(callerIdentityToken.ConvertToUnsecureString(), aadTenantId), "GetDirectorySubscribedSkus", diagnostics);
	}

	public IEnumerable<UserInfo> GetUsers(SecureString callerIdentityToken, string aadTenantId, int count, NavDiagnostics diagnostics)
	{
		return ExecuteLicenseServiceClientActionWithRetry(() => serviceClient.GetUsers(callerIdentityToken.ConvertToUnsecureString(), aadTenantId, count), "GetUsers", diagnostics);
	}

	public UserInfoCollectionPage GetUsersPage(SecureString callerIdentityToken, string aadTenantId, int count, NavDiagnostics diagnostics)
	{
		return ExecuteLicenseServiceClientActionWithRetry(() => serviceClient.GetUserPage(callerIdentityToken.ConvertToUnsecureString(), aadTenantId, count), "GetUsersPage", diagnostics);
	}

	public UserInfoCollectionPage GetUsersPage(SecureString callerIdentityToken, string aadTenantId, IList<QueryOption> options, NavDiagnostics diagnostics)
	{
		return ExecuteLicenseServiceClientActionWithRetry(() => serviceClient.GetUserPage(callerIdentityToken.ConvertToUnsecureString(), aadTenantId, options), "GetUsersPage", diagnostics);
	}

	public IEnumerable<RoleInfo> GetDirectoryRoles(SecureString callerIdentityToken, string aadTenantId, NavDiagnostics diagnostics)
	{
		return ExecuteLicenseServiceClientActionWithRetry(() => serviceClient.GetDirectoryRoles(callerIdentityToken.ConvertToUnsecureString(), aadTenantId), "GetDirectoryRoles", diagnostics);
	}

	public TenantInfo GetTenantDetail(SecureString callerIdentityToken, string aadTenantId, NavDiagnostics diagnostics)
	{
		return ExecuteLicenseServiceClientActionWithRetry(() => serviceClient.GetTenantDetail(callerIdentityToken.ConvertToUnsecureString(), aadTenantId), "GetTenantDetail", diagnostics);
	}

	internal static TResult ExecuteLicenseServiceClientActionWithRetry<TResult>(Func<Task<TResult>> Operation, string method, NavDiagnostics diagnostics)
	{
		using (new IfxTimingScope(licenseServiceClientDurationMetric, method, ServerUserSettings.Instance.LicensingServiceClientType))
		{
			try
			{
				return ExecuteOperationWithRetry(Operation, method, diagnostics);
			}
			catch (Exception exception)
			{
				Exception inner = diagnostics.SendExceptionTag("00001VC", Category.License, exception);
				throw new NavLicenseServiceException("An error occurred while querying for external license information. For error details check the inner exception.", inner);
			}
		}
	}

	private static TResult ExecuteOperationWithRetry<TResult>(Func<Task<TResult>> Operation, string method, NavDiagnostics diagnostics)
	{
		//IL_0017: Expected Obj, but got Unknown
		try
		{
			return Operation().GetAwaiter().GetResult();
		}
		catch (LicenseServiceTransientException ex)
		{
			LicenseServiceTransientException ex2 = ex;
			diagnostics.SendTraceTag("000022Z", Category.License, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Retrying execution of method : " + method + "due to transient exception :" + ((Exception)(object)ex2).Message + "Inner Exception : " + ((Exception)(object)ex2).InnerException?.Message);
			return Operation().GetAwaiter().GetResult();
		}
	}

	private static ILicenseService CreateClient(NavTenant tenant, NavDiagnostics diagnosticsInstance)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected Obj, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected Obj, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected Obj, but got Unknown
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected Obj, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected Obj, but got Unknown
		//IL_015c: Expected Obj, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Expected Obj, but got Unknown
		if (diagnosticsInstance == null)
		{
			throw new ArgumentNullException("diagnosticsInstance");
		}
		if (ServerUserSettings.Instance.EnableLicensingService.Value)
		{
			string text = ServerUserSettings.Instance.LicensingServiceUri.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				diagnosticsInstance?.SendTraceTag("00001SG", Category.License, Verbosity.Normal, PrivacyClassification.SystemMetadata, "License service client connected using uri: {0}", text);
				return ProxyClientFactory.CreateLicenseServiceProxy(new Uri(text), ServerUserSettings.Instance.ReverseProxyCertificateThumbprint.Value);
			}
			diagnosticsInstance?.SendTraceTag("000023A", Category.License, Verbosity.Normal, PrivacyClassification.SystemMetadata, "License service client connected using default uri");
			return ProxyClientFactory.CreateLicenseServiceProxy(ServerUserSettings.Instance.ReverseProxyCertificateThumbprint.Value);
		}
		AzureADAuthenticationHelper azureAdHelper = new AzureADAuthenticationHelper(tenant);
		LicenseServiceClientSetup val = new LicenseServiceClientSetup
		{
			AuthenticationTokenProviderAsync = (string aadTenantId, string resouceAppIdUri) => azureAdHelper.AcquireTokenAsyncForApplication(resouceAppIdUri),
			Settings = new ClientSettings
			{
				GraphApiEndpoint = AzureADAuthenticationHelper.GraphApiEndpoint,
				InvoiceAccessControlEndpoint = ServerUserSettings.Instance.InvoiceAccessControlEndpoint.Value,
				OfficePortalEndpoint = AzureADAuthenticationHelper.OfficePortalEndpoint
			},
			EuiiTraceProviderDelegate = (string tag, string message, object[] parameters) =>
			{
				diagnosticsInstance?.SendTraceTag(tag, Category.License, Verbosity.Normal, PrivacyClassification.EndUserIdentifiableInformation, message, parameters);
			},
			TraceProvider = (string tag, string message, object[] parameters) =>
			{
				diagnosticsInstance?.SendTraceTag(tag, Category.License, Verbosity.Normal, PrivacyClassification.SystemMetadata, message, parameters);
			},
			ProcessTraceProvider = (string tag, string name) => diagnosticsInstance.CreateTraceScope(tag, DiagnosticParameter.SystemMetadata(name), MonitorType.None, Category.License)
		};
		diagnosticsInstance?.SendTraceTag("00001SH", Category.License, Verbosity.Normal, PrivacyClassification.SystemMetadata, "License service client connected using on-prem license client.");
		return (ILicenseService)new LicenseServiceClient(val);
	}
}
