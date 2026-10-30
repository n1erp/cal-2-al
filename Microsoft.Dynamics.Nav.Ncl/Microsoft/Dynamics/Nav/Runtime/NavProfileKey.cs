using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavProfileKey
{
	internal string ProfileId { get; private set; }

	public NavGuid AppId { get; private set; }

	public ProfileScope Scope { get; private set; }

	public NavProfileKey(string profileId, NavGuid appId = null, ProfileScope scope = ProfileScope.System)
	{
		if (string.IsNullOrEmpty(profileId))
		{
			throw new ArgumentNullException("profileId");
		}
		ProfileId = profileId.ToUpper(CultureInfo.InvariantCulture).Trim();
		AppId = appId ?? new NavGuid(Guid.Empty);
		Scope = scope;
	}

	public bool ProfileExists()
	{
		NavSession session = NavCurrentThread.Session;
		using (NavCurrentThread.Session.Permissions.HasMaximizedPermissions(2000000178) ? new MaximizedPermissionScope(session, new int[3] { 2000000178, 2000000072, 2000000177 }) : null)
		{
			using NavRecord navRecord = new NavRecord(session, 2000000178);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
			return navRecord.ALGet(DataError.TrapError, NavOption.Create(fieldByNo.FieldOptionMetadata, (int)Scope), new NavGuid(AppId.Value), new NavCode(2048, ProfileId));
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is NavProfileKey navProfileKey)
		{
			if (Scope == navProfileKey.Scope && AppId.Equals(navProfileKey.AppId))
			{
				return ProfileId.Equals(navProfileKey.ProfileId);
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Scope.GetHashCode() ^ AppId.GetHashCode() ^ ProfileId.GetHashCode();
	}
}
