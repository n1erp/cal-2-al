using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class EntitlementPermissionSet : PermissionSetBase
{
	public static Func<NavSession, NavUser, IEnumerable<NavCode>> UserEntitlementSetIdRetriever = (NavSession session, NavUser user) => user?.MembershipEntitlements?.GetEntitlementSetIds(session);

	private static EntitlementCache entitlements = new EntitlementCache();

	public EntitlementPermissionSet(ApplicationObjectId objectId, NavApplicationObjectBase callingObject, PermissionManager permissionManager)
		: base(objectId, callingObject, permissionManager)
	{
	}

	public static PermissionMask GetEntitlementPermissions(NavUser user, ApplicationObjectId objectId)
	{
		if (user == null)
		{
			return PermissionMask.None;
		}
		if (objectId.ObjectNumber <= 0)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000A0", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Invalid application object number: {0}", objectId.ObjectNumber);
			return PermissionMask.None;
		}
		if (FeatureSwitches.Instance.EntitlementByAadPresenceOnly)
		{
			return PermissionMask.Max;
		}
		EntitlementCache cachedEntitlements = entitlements;
		return UserEntitlementSetIdRetriever(NavCurrentThread.Session, user)?.Select((NavCode setId) => cachedEntitlements.GetEntitlement(setId, objectId)).Aggregate(PermissionMask.None, (PermissionMask p, PermissionMask q) => p | q) ?? PermissionMask.None;
	}

	public static void ClearCache()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000A1", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Clear the entitlement cache.");
		entitlements = new EntitlementCache();
	}

	protected override PermissionMask FetchPermissions()
	{
		return GetEntitlementPermissions(NavCurrentThread.Session.User, ObjectId);
	}
}
