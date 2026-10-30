using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class UserMembershipEntitlements
{
	private readonly HashSet<NavCode> evalEntitlementSetIds;

	private readonly HashSet<NavCode> prodEntitlementSetIds;

	public bool HasProductionMembershipEntitlements => prodEntitlementSetIds.Any();

	public UserMembershipEntitlements(HashSet<NavMembershipEntitlement> membershipEntitlements)
	{
		evalEntitlementSetIds = new HashSet<NavCode>(membershipEntitlements.Select((NavMembershipEntitlement e) => new NavCode(2048, e.EntitlementSetId)));
		prodEntitlementSetIds = new HashSet<NavCode>(from e in membershipEntitlements
			where !e.IsEvaluation
			select new NavCode(2048, e.EntitlementSetId));
	}

	public IEnumerable<NavCode> GetEntitlementSetIds(NavSession session)
	{
		if (session.AppId == "INV")
		{
			return prodEntitlementSetIds;
		}
		if (session.Company.IsEvaluationCompany || session.TenantLicenseState == TenantLicenseState.Evaluation || session.TenantLicenseState == TenantLicenseState.Trial)
		{
			return evalEntitlementSetIds;
		}
		return prodEntitlementSetIds;
	}
}
