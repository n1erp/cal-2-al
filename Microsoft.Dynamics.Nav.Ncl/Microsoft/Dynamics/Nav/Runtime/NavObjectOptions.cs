using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavObjectOptions
{
	private enum ReportSettingsScope
	{
		NoScope,
		MustBeAssignedToCurrentUser,
		PreferAssignedToCurrentUser
	}

	private bool? publicVisible;

	private bool? temporary;

	public int Id { get; }

	public ObjectType ObjectType { get; }

	public virtual string ParameterName { get; }

	public NavCode UserName { get; private set; }

	public virtual bool PublicVisible
	{
		get
		{
			if (publicVisible.HasValue)
			{
				return publicVisible.Value;
			}
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000196 }))
			{
				using NavRecordHandle optionHandle = new NavRecordHandle(NavCurrentThread.Session, 2000000196, temporary: false, SecurityFiltering.Validated);
				return FindRecord(optionHandle) && OptionIsPublic(optionHandle);
			}
		}
		set
		{
			publicVisible = value;
		}
	}

	public virtual bool Temporary
	{
		get
		{
			if (temporary.HasValue)
			{
				return temporary.Value;
			}
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000196 }))
			{
				using NavRecordHandle optionHandle = new NavRecordHandle(NavCurrentThread.Session, 2000000196, temporary: false, SecurityFiltering.Validated);
				return FindRecord(optionHandle) && OptionIsTemporary(optionHandle);
			}
		}
		set
		{
			temporary = value;
		}
	}

	public NavObjectOptions(ObjectType objectType, int id, string parameterName)
	{
		if (!IsValidObjectType(objectType))
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.UnsupportedObjectType, objectType));
		}
		if (string.IsNullOrEmpty(parameterName))
		{
			throw new NavNCLArgumentException(Lang.ParameterNameMustNotBeEmpty);
		}
		if (id < 1)
		{
			throw new NavNCLArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, Lang._00095_50170, id));
		}
		ObjectType = objectType;
		Id = id;
		ParameterName = parameterName;
	}

	public virtual string Load()
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000196 }))
		{
			using NavRecordHandle optionHandle = new NavRecordHandle(NavCurrentThread.Session, 2000000196, temporary: false, SecurityFiltering.Validated);
			return FindRecord(optionHandle, ReportSettingsScope.PreferAssignedToCurrentUser) ? GetParameters(optionHandle) : string.Empty;
		}
	}

	public bool Exists()
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000196 }))
		{
			using NavRecordHandle optionHandle = new NavRecordHandle(NavCurrentThread.Session, 2000000196, temporary: false, SecurityFiltering.Validated);
			return FindRecord(optionHandle);
		}
	}

	public static bool Exists(int objectId, ObjectType objectType)
	{
		if (!IsValidObjectType(objectType))
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.UnsupportedObjectType, objectType));
		}
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000196 }))
		{
			using NavRecordHandle navRecordHandle = new NavRecordHandle(NavCurrentThread.Session, 2000000196, temporary: false, SecurityFiltering.Validated);
			NavText navText = new NavText(30, NavCurrentThread.Session.CompanyName);
			navRecordHandle.Target.ALSetRange(4, navText, navText);
			int navDefinedLengthMetadata = navRecordHandle.Target.GetFieldValue(5).NavDefinedLengthMetadata;
			navRecordHandle.Target.ALSetFilter(5, "%1|%2", new NavCode(navDefinedLengthMetadata, NavCurrentThread.Session.User.Name), new NavCode(navDefinedLengthMetadata, string.Empty));
			navRecordHandle.Target.ALSetFilter(2, objectId.ToString(CultureInfo.CurrentCulture));
			navRecordHandle.Target.ALSetFilter(3, NavOption.Create(NCLOptionMetadata.Default, (int)objectType).ToString());
			return navRecordHandle.Target.ALFind(DataError.TrapError, "-");
		}
	}

	public virtual void Save(string parameters)
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000196 }))
		{
			using NavRecordHandle navRecordHandle = new NavRecordHandle(NavCurrentThread.Session, 2000000196, temporary: false, SecurityFiltering.Validated);
			if (FindRecord(navRecordHandle, ReportSettingsScope.MustBeAssignedToCurrentUser))
			{
				if (!ValidateOwnerShip(navRecordHandle))
				{
					throw new NavPermissionException(Lang.ObjectOptionNoPermission);
				}
				navRecordHandle.Target.SetFieldValue(6, new NavBLOB(Encoding.UTF8.GetBytes(parameters)));
				if (publicVisible.HasValue)
				{
					navRecordHandle.Target.SetFieldValue(7, NavBoolean.Create(publicVisible.Value));
				}
				if (temporary.HasValue)
				{
					navRecordHandle.Target.SetFieldValue(8, NavBoolean.Create(temporary.Value));
				}
				navRecordHandle.Target.ALModify(DataError.ThrowError);
			}
			else
			{
				navRecordHandle.Target.ALInit();
				navRecordHandle.Target.SetFieldValue(1, new NavText(navRecordHandle.Target.GetFieldValue(1).NavDefinedLengthMetadata, ParameterName));
				navRecordHandle.Target.SetFieldValue(2, NavInteger.Create(Id));
				navRecordHandle.Target.SetFieldValue(3, NavOption.Create(navRecordHandle.Target.GetFieldValue(3).NavOptionMetadata, (int)ObjectType));
				navRecordHandle.Target.SetFieldValue(4, new NavText(navRecordHandle.Target.GetFieldValue(4).NavDefinedLengthMetadata, NavCurrentThread.Session.CompanyName));
				navRecordHandle.Target.SetFieldValue(5, new NavCode(navRecordHandle.Target.GetFieldValue(5).NavDefinedLengthMetadata, NavCurrentThread.Session.User.Name));
				navRecordHandle.Target.SetFieldValue(9, new NavCode(navRecordHandle.Target.GetFieldValue(9).NavDefinedLengthMetadata, NavCurrentThread.Session.User.Name));
				navRecordHandle.Target.SetFieldValue(7, NavBoolean.Create(publicVisible == true));
				navRecordHandle.Target.SetFieldValue(8, NavBoolean.Create(temporary == true));
				navRecordHandle.Target.SetFieldValue(6, new NavBLOB(Encoding.UTF8.GetBytes(parameters)));
				navRecordHandle.Target.ALInsert(DataError.ThrowError);
			}
		}
	}

	public void Delete()
	{
		using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000196 }))
		{
			using NavRecordHandle navRecordHandle = new NavRecordHandle(NavCurrentThread.Session, 2000000196, temporary: false, SecurityFiltering.Validated);
			if (FindRecord(navRecordHandle))
			{
				navRecordHandle.Target.ALDelete();
			}
		}
	}

	private static bool IsValidObjectType(ObjectType objectType)
	{
		if (objectType != ObjectType.Report)
		{
			return objectType == ObjectType.XmlPort;
		}
		return true;
	}

	private bool FindRecord(NavRecordHandle optionHandle, ReportSettingsScope scope = ReportSettingsScope.NoScope)
	{
		string companyName = NavCurrentThread.Session.CompanyName;
		optionHandle.Target.ALSetRange(4, new NavText(companyName));
		optionHandle.Target.ALSetFilter(2, Id.ToString(CultureInfo.CurrentCulture));
		optionHandle.Target.ALSetFilter(1, ParameterName);
		int navDefinedLengthMetadata = optionHandle.Target.GetFieldValue(5).NavDefinedLengthMetadata;
		switch (scope)
		{
		case ReportSettingsScope.MustBeAssignedToCurrentUser:
			optionHandle.Target.ALSetFilter(5, "%1", new NavCode(navDefinedLengthMetadata, NavCurrentThread.Session.User.Name));
			break;
		case ReportSettingsScope.PreferAssignedToCurrentUser:
			optionHandle.Target.ALSetFilter(5, "%1", new NavCode(navDefinedLengthMetadata, NavCurrentThread.Session.User.Name));
			if (optionHandle.Target.ALFind(DataError.TrapError, "-"))
			{
				return true;
			}
			optionHandle.Target.ALSetFilter(5, string.Empty);
			optionHandle.Target.ALSetFilter(7, NavBoolean.True.ToString());
			break;
		}
		return optionHandle.Target.ALFind(DataError.TrapError, "-");
	}

	private string GetParameters(NavRecordHandle optionHandle)
	{
		if (!ValidateOwnerShip(optionHandle))
		{
			return string.Empty;
		}
		optionHandle.Target.ALCalcFields(DataError.ThrowError, 6);
		NavBLOB navBLOB = (NavBLOB)optionHandle.Target.GetFieldValue(6);
		return Encoding.UTF8.GetString(navBLOB.GetBytes());
	}

	private bool ValidateOwnerShip(NavRecordHandle optionHandle)
	{
		if (OptionIsPublic(optionHandle) || NavCurrentThread.Session.User.IsNavAdminUser)
		{
			return true;
		}
		UserName = (NavCode)optionHandle.Target.GetFieldValue(5);
		if (UserName.IsZeroOrEmpty)
		{
			return !UserName.Equals(NavCurrentThread.Session.User.Name);
		}
		return true;
	}

	private static bool OptionIsPublic(NavRecordHandle optionHandle)
	{
		NavBoolean navBoolean = (NavBoolean)optionHandle.Target.GetFieldValue(7);
		return navBoolean;
	}

	private static bool OptionIsTemporary(NavRecordHandle optionHandle)
	{
		NavBoolean navBoolean = (NavBoolean)optionHandle.Target.GetFieldValue(8);
		return navBoolean;
	}
}
