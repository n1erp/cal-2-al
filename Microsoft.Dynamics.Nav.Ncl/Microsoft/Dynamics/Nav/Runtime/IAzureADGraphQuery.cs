using System.Collections.Generic;
using Microsoft.Dynamics.Nav.LicensingService.Model;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IAzureADGraphQuery
{
	IEnumerable<SkuInfo> GetDirectorySubscribedSkus();

	IEnumerable<UserInfo> GetUsers(int count);

	UserInfoCollectionPage GetNextUsersPage(IList<QueryOption> options);

	UserInfo GetUser(string userPrincipalName);

	bool TryGetUserByObjectId(string objectId, out UserInfo user);

	UserInfo GetUserByObjectId(string objectId);

	UserInfo GetUserByAuthorizationEmail(string authorizationEmail);

	IEnumerable<RoleInfo> GetDirectoryRoles();

	TenantInfo GetTenantDetail();
}
