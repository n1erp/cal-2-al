using System.Collections.Generic;
using System.Security;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;

namespace Microsoft.Dynamics.Nav.Runtime.Authentication;

public interface INavLicenseServiceClient
{
	IEnumerable<RoleInfo> GetDirectoryRoles(SecureString callerIdentityToken, string aadTenantId, NavDiagnostics diagnostics);

	IEnumerable<SkuInfo> GetDirectorySubscribedSkus(SecureString callerIdentityToken, string aadTenantId, NavDiagnostics diagnostics);

	LicenseDetails GetLicenseDetailsByObjectId(string aadTenantId, string aadUserObjectId, NavDiagnostics diagnostics);

	LicenseDetails GetLicenseDetailsByUpn(SecureString callerIdentityToken, string aadTenantId, string addUserUpn, NavDiagnostics diagnostics);

	OfficeEntitlementStatus GetOfficeEntitlementStatusForUser(string aadTenantId, string userPrincipalName, NavDiagnostics diagnostics);

	TenantInfo GetTenantDetail(SecureString callerIdentityToken, string aadTenantId, NavDiagnostics diagnostics);

	IEnumerable<UserInfo> GetUsers(SecureString callerIdentityToken, string aadTenantId, int count, NavDiagnostics diagnostics);

	UserInfoCollectionPage GetUsersPage(SecureString callerIdentityToken, string aadTenantId, int count, NavDiagnostics diagnostics);

	UserInfoCollectionPage GetUsersPage(SecureString callerIdentityToken, string aadTenantId, IList<QueryOption> options, NavDiagnostics diagnostics);
}
