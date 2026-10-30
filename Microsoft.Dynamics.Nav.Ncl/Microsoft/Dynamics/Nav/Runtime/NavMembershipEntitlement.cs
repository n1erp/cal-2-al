namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavMembershipEntitlement
{
	public MembershipEntitlementType Type { get; set; }

	public string Id { get; set; }

	public string Name { get; set; }

	public string EntitlementSetId { get; set; }

	public bool IsEvaluation { get; set; }
}
