using System;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.XmlMetadata;

public class NavConfigurationProfile
{
	private string profileId;

	private string profileDescription;

	private NavProfileKey profileKey;

	internal string Id => profileId;

	internal NavProfileKey ProfileKey => profileKey;

	public bool DisablePersonalization { get; set; }

	internal string Description
	{
		get
		{
			return profileDescription;
		}
		set
		{
			profileDescription = value ?? string.Empty;
		}
	}

	internal NavConfigurationProfile(string profileId, string description)
	{
		if (string.IsNullOrEmpty(profileId))
		{
			throw new ArgumentNullException("profileId");
		}
		this.profileId = profileId;
		profileKey = new NavProfileKey(profileId);
		profileDescription = description ?? string.Empty;
	}

	public static NavConfigurationProfile Load(NavProfileKey profileKey)
	{
		if (profileKey == null)
		{
			throw new ArgumentNullException("profileKey");
		}
		NavConfigurationProfile navConfigurationProfile = null;
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[3] { 2000000178, 2000000072, 2000000177 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000178, SecurityFiltering.Ignored);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(4);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(13);
			NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo5 = navRecord.MetaTable.GetFieldByNo(1);
			navRecord.SetFilter(fieldByNo, "%1", new NavCode(2048, profileKey.ProfileId));
			navRecord.SetFilter(fieldByNo4, "%1", new NavGuid(profileKey.AppId.Value));
			navRecord.SetFilter(fieldByNo5, "%1", NavOption.Create(fieldByNo5.FieldOptionMetadata, (int)profileKey.Scope));
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				string description = navRecord.GetFieldValue(fieldByNo2).ToString();
				bool disablePersonalization = navRecord.GetFieldValue(fieldByNo3).ToBoolean();
				navConfigurationProfile = new NavConfigurationProfile(profileKey.ProfileId, description);
				navConfigurationProfile.DisablePersonalization = disablePersonalization;
			}
		}
		return navConfigurationProfile;
	}

	internal static void Save(NavConfigurationProfile profile)
	{
		if (profile == null)
		{
			throw new ArgumentNullException("profile");
		}
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000178);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(4);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(2);
		NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(1);
		navRecord.SetFilter(fieldByNo, "%1", new NavCode(2048, profile.ProfileKey.ProfileId));
		navRecord.SetFilter(fieldByNo3, "%1", new NavGuid(profile.ProfileKey.AppId.Value));
		navRecord.SetFilter(fieldByNo4, "%1", NavOption.Create(fieldByNo4.FieldOptionMetadata, (int)profile.ProfileKey.Scope));
		bool flag = navRecord.ALFindFirst(DataError.TrapError);
		session.BeginTransaction();
		navRecord.SetFieldValue(fieldByNo, new NavCode(2048, profile.Id));
		navRecord.SetFieldValue(fieldByNo2, new NavText(int.MaxValue, profile.Description));
		if (flag)
		{
			navRecord.ALModify(DataError.ThrowError);
		}
		else
		{
			navRecord.ALInsert(DataError.ThrowError);
		}
		session.EndTransaction(commit: true);
	}

	internal static void Delete(NavConfigurationProfile profile)
	{
		if (profile == null)
		{
			throw new ArgumentNullException("profile");
		}
		Delete(profile.profileKey);
	}

	internal static void Delete(NavProfileKey profileKey)
	{
		if (profileKey == null)
		{
			throw new ArgumentNullException("profileKey");
		}
		NavSession session = NavCurrentThread.Session;
		using NavRecord navRecord = new NavRecord(session, 2000000178);
		NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
		NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(1);
		navRecord.SetFilter(fieldByNo, "%1", new NavCode(2048, profileKey.ProfileId));
		navRecord.SetFilter(fieldByNo2, "%1", new NavGuid(profileKey.AppId.Value));
		navRecord.SetFilter(fieldByNo3, "%1", NavOption.Create(fieldByNo3.FieldOptionMetadata, (int)profileKey.Scope));
		if (navRecord.ALFindFirst(DataError.TrapError))
		{
			session.BeginTransaction();
			navRecord.ALDelete(DataError.ThrowError);
			session.EndTransaction(commit: true);
		}
	}

	public static NavProfileKey GetUserProfile(Guid userId)
	{
		NavProfileKey navProfileKey = null;
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000073 }))
		{
			using (new MaximizedPermissionScope(session, new int[3] { 2000000178, 2000000072, 2000000177 }))
			{
				using NavRecord navRecord = new NavRecord(session, 2000000073, SecurityFiltering.Ignored);
				NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
				NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(9);
				NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(10);
				NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(11);
				navRecord.SetFilter(fieldByNo, "%1", new NavGuid(userId));
				if (navRecord.ALFindFirst(DataError.TrapError) && !string.IsNullOrWhiteSpace(navRecord.GetFieldValue(fieldByNo2).ToString()))
				{
					navProfileKey = new NavProfileKey(navRecord.GetFieldValue(fieldByNo2).ToString(), new NavGuid(navRecord.GetFieldValue(fieldByNo3).ToGuid()), (ProfileScope)(navRecord.GetFieldValue(fieldByNo4) as NavOption).Value);
				}
				if (string.IsNullOrEmpty(navProfileKey?.ProfileId) || !navProfileKey.ProfileExists())
				{
					navProfileKey = GetDefaultConfigurationProfile();
				}
			}
		}
		return navProfileKey;
	}

	public static ProfileScope GetProfileScope(NavProfileKey profileKey)
	{
		if (profileKey == null)
		{
			throw new ArgumentNullException("profileKey");
		}
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[3] { 2000000178, 2000000072, 2000000177 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000178, SecurityFiltering.Ignored);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(1);
			navRecord.SetFilter(fieldByNo, "%1", new NavCode(2048, profileKey.ProfileId));
			navRecord.SetFilter(fieldByNo2, "%1", profileKey.AppId);
			navRecord.SetFilter(fieldByNo3, "%1", NavOption.Create(fieldByNo3.FieldOptionMetadata, (int)profileKey.Scope));
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(1);
				return (ProfileScope)(navRecord.GetFieldValue(fieldByNo4) as NavOption).Value;
			}
			throw new NavCSideRecordNotFoundException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.ProfileNotFound, profileKey.ProfileId, profileKey.Scope));
		}
	}

	internal static NavProfileKey GetDefaultConfigurationProfile()
	{
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[3] { 2000000178, 2000000072, 2000000177 }))
		{
			using NavRecordRef navRecordRef = new NavRecordRef(session, 2000000178, isTempAllocation: false, SecurityFiltering.Ignored);
			NavFieldRef navFieldRef = navRecordRef.ALField(navRecordRef, 6);
			navFieldRef.ALSetFilter("%1", NavBoolean.True);
			if (navRecordRef.ALFindFirst(DataError.TrapError))
			{
				NavFieldRef navFieldRef2 = navRecordRef.ALField(navRecordRef, 3);
				NavFieldRef navFieldRef3 = navRecordRef.ALField(navRecordRef, 2);
				NavFieldRef navFieldRef4 = navRecordRef.ALField(navRecordRef, 1);
				return new NavProfileKey(navFieldRef2.Value.ToString(), new NavGuid(navFieldRef3.Value.ToGuid()), (ProfileScope)(navFieldRef4.Value as NavOption).Value);
			}
		}
		return null;
	}

	internal static int GetProfileRoleCenterId(NavProfileKey profile)
	{
		if (profile == null)
		{
			throw new ArgumentNullException("profile");
		}
		int result = -1;
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[3] { 2000000178, 2000000072, 2000000177 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000178, SecurityFiltering.Ignored);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(1);
			navRecord.SetFilter(fieldByNo, "%1", new NavCode(2048, profile.ProfileId));
			navRecord.SetFilter(fieldByNo2, "%1", profile.AppId);
			navRecord.SetFilter(fieldByNo3, "%1", NavOption.Create(fieldByNo3.FieldOptionMetadata, (int)profile.Scope));
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(5);
				result = navRecord.GetFieldValue(fieldByNo4).ToInt32();
			}
		}
		return result;
	}

	internal static void SetUserProfile(Guid userid, NavProfileKey profileKey)
	{
		if (profileKey == null)
		{
			throw new ArgumentNullException("profileKey");
		}
		NavSession session = NavCurrentThread.Session;
		using (NavRecord navRecord = new NavRecord(session, 2000000178))
		{
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(1);
			navRecord.ALSetCurrentKey(1, 2, 3);
			navRecord.ALSetFilter(1, "%1", NavOption.Create(fieldByNo2.FieldOptionMetadata, (int)profileKey.Scope));
			navRecord.ALSetFilter(2, "%1", profileKey.AppId);
			navRecord.ALSetFilter(3, "%1", new NavCode(fieldByNo.FieldDefinedLength, profileKey.ProfileId));
			if (!navRecord.ALFindFirst(DataError.TrapError))
			{
				throw new NavProfileException(string.Format(NavCurrentThread.Session.RuntimeLanguage, Lang.ProfileNotFound, profileKey.ProfileId, ProfileScope.System));
			}
		}
		bool flag = false;
		using NavRecord navRecord2 = new NavRecord(session, 2000000073);
		NCLMetaField fieldByNo3 = navRecord2.MetaTable.GetFieldByNo(3);
		NCLMetaField fieldByNo4 = navRecord2.MetaTable.GetFieldByNo(9);
		NCLMetaField fieldByNo5 = navRecord2.MetaTable.GetFieldByNo(11);
		NCLMetaField fieldByNo6 = navRecord2.MetaTable.GetFieldByNo(10);
		navRecord2.SetFilter(fieldByNo3, "%1", new NavGuid(userid));
		bool flag2 = navRecord2.ALFindFirst(DataError.TrapError);
		try
		{
			if (!session.IsTransactionActive())
			{
				session.BeginTransaction();
				flag = true;
			}
			if (flag2)
			{
				navRecord2.SetFieldValue(fieldByNo5, NavOption.Create(fieldByNo5.FieldOptionMetadata, (int)profileKey.Scope));
				navRecord2.SetFieldValue(fieldByNo6, profileKey.AppId);
				navRecord2.SetFieldValue(fieldByNo4, new NavCode(fieldByNo4.FieldDefinedLength, profileKey.ProfileId));
				navRecord2.ALModify();
			}
			else
			{
				navRecord2.SetFieldValue(fieldByNo3, new NavGuid(userid));
				navRecord2.SetFieldValue(fieldByNo5, NavOption.Create(fieldByNo5.FieldOptionMetadata, (int)profileKey.Scope));
				navRecord2.SetFieldValue(fieldByNo6, profileKey.AppId);
				navRecord2.SetFieldValue(fieldByNo4, new NavCode(fieldByNo4.FieldDefinedLength, profileKey.ProfileId));
				navRecord2.ALInsert();
			}
			if (flag)
			{
				session.EndTransaction(commit: true);
			}
		}
		finally
		{
			if (flag && session.IsTransactionActive())
			{
				session.EndTransaction(commit: false);
			}
		}
	}

	internal static (int RoleCenterId, string Description, ProfileScope Scope, Guid AppId) GetProfileInformation(NavProfileKey profile)
	{
		if (profile == null)
		{
			throw new ArgumentNullException("profile");
		}
		int item = -1;
		string item2 = null;
		ProfileScope item3 = ProfileScope.System;
		Guid item4 = default;
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[3] { 2000000178, 2000000072, 2000000177 }))
		{
			using NavRecord navRecord = new NavRecord(session, 2000000178, SecurityFiltering.Ignored);
			NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(3);
			NCLMetaField fieldByNo2 = navRecord.MetaTable.GetFieldByNo(2);
			NCLMetaField fieldByNo3 = navRecord.MetaTable.GetFieldByNo(1);
			navRecord.SetFilter(fieldByNo, "%1", new NavCode(2048, profile.ProfileId));
			navRecord.SetFilter(fieldByNo2, "%1", profile.AppId);
			navRecord.SetFilter(fieldByNo3, "%1", NavOption.Create(fieldByNo3.FieldOptionMetadata, (int)profile.Scope));
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				NCLMetaField fieldByNo4 = navRecord.MetaTable.GetFieldByNo(5);
				item = navRecord.GetFieldValue(fieldByNo4).ToInt32();
				NCLMetaField fieldByNo5 = navRecord.MetaTable.GetFieldByNo(4);
				item2 = navRecord.GetFieldValue(fieldByNo5).ToString();
				item3 = (ProfileScope)navRecord.GetFieldValue(fieldByNo3).ToInt32();
				item4 = navRecord.GetFieldValue(fieldByNo2).ToGuid();
			}
		}
		return (RoleCenterId: item, Description: item2, Scope: item3, AppId: item4);
	}
}
