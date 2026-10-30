using System;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Security.Permissions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class PermissionSetBase : IPermissionSet
{
	private readonly PermissionMask indirectPermissionMaskFromCallingObject;

	private PermissionMask actualDirectPermissions;

	private bool actualDirectPermissionsEvaluated;

	private ApplicationObjectId callingObjectId;

	protected internal PermissionManager PermissionManager { get; private set; }

	protected internal ApplicationObjectId ObjectId { get; private set; }

	protected internal virtual bool HasMaximizedPermissions => PermissionManager.HasMaximizedPermissions(ObjectId.ObjectNumber);

	internal PermissionMask ActualDirectPermissions
	{
		get
		{
			if (actualDirectPermissionsEvaluated)
			{
				return actualDirectPermissions;
			}
			actualDirectPermissions = FetchPermissions();
			actualDirectPermissionsEvaluated = !NavTestExecution.IsInTestMode();
			return actualDirectPermissions;
		}
	}

	protected PermissionSetBase(ApplicationObjectId objectId, NavApplicationObjectBase callingObject, PermissionManager permissionManager)
	{
		ObjectId = objectId;
		PermissionManager = permissionManager;
		callingObjectId = callingObject?.ObjectId ?? default(ApplicationObjectId);
		indirectPermissionMaskFromCallingObject = ((objectId.ObjectType == ObjectType.TableData && callingObject != null) ? ((PermissionMask)callingObject.GetIndirectPermissionMask(objectId.ObjectNumber)) : PermissionMask.None);
	}

	protected abstract PermissionMask FetchPermissions();

	public bool HasPermissions(NavApplicationObjectBase securableObject, PermissionMask permissionMask, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty = null, PermissionMask additionalIndirectPermissions = PermissionMask.None, bool countAsLimitedUserAllowedTable = true, PermissionValidationCategory validationCategory = PermissionValidationCategory.Normal)
	{
		return InternalHasPermisisons(NavCurrentThread.Session, securableObject, permissionMask, limitedUserPermissionChecker, checkTableEmpty, additionalIndirectPermissions, countAsLimitedUserAllowedTable, validationCategory);
	}

	public void VerifyPermissions(NavApplicationObjectBase securableObject, PermissionMask permissionMask, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty = null, PermissionMask additionalIndirectPermissions = PermissionMask.None, bool countAsLimitedUserAllowedTable = true)
	{
		NavSession session = NavCurrentThread.Session;
		if (InternalHasPermisisons(session, securableObject, permissionMask, limitedUserPermissionChecker, checkTableEmpty, additionalIndirectPermissions, countAsLimitedUserAllowedTable))
		{
			return;
		}
		if (NavTestExecution.IsInTestMode())
		{
			session.Diagnostics.SendTraceTag("000004E", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Permissions were denied ({0} {1}), InTestExecution {2} Effective sets {1}", ObjectId.ObjectType, ObjectId.ObjectNumber, NavTestExecution.IsInTestMode(), (!NavTestExecution.IsInTestMode()) ? "No effective sets" : (session.TestExecution.EffectivePermissionSets?.ToArray().Aggregate("", (string acc, string n) => acc = acc + n + ",") ?? "No test effective permissions set"));
		}
		if (FeatureSwitches.Instance.EnableSecurityTracing)
		{
			session.Diagnostics.SendTraceTag("00000XN", Category.Security, Verbosity.Warning, PrivacyClassification.EndUserIdentifiableInformation, "Permissions were denied ({0} {1}): {2}", ObjectId.ObjectType, ObjectId.ObjectNumber, session.User.GetDetailsForLog(session));
		}
		throw session.Diagnostics.SendExceptionTag("00000XO", Category.Security, GeneratePermissionException(permissionMask));
	}

	internal bool InternalHasPermissionsDoNotCheckIncludedExcludedPermissions(NavApplicationObjectBase securableObject, PermissionMask permissions, Func<int, bool> checkTableEmpty, PermissionMask additionalIndirectPermissions, bool checkMaximizedPermissions = true)
	{
		if (checkMaximizedPermissions && (ObjectId.ObjectType == ObjectType.Table || ObjectId.ObjectType == ObjectType.TableData) && HasMaximizedPermissions)
		{
			return true;
		}
		if (ObjectId.ObjectType == ObjectType.Page && SystemPages.IsServerSystemPageId(ObjectId.ObjectNumber))
		{
			return true;
		}
		PermissionMask permissionMask = ActualDirectPermissions;
		permissions = ~permissionMask & permissions;
		if (permissions == PermissionMask.None)
		{
			return true;
		}
		if (ObjectId.ObjectType != ObjectType.TableData)
		{
			return false;
		}
		NavApplicationObjectBase navApplicationObjectBase = ((securableObject != null) ? NavApplicationObjectBase.FindApplicationObject(NavCurrentThread.Session.CurrentMethodScope) : null);
		PermissionMask permissionMask2 = ((navApplicationObjectBase == null) ? indirectPermissionMaskFromCallingObject : ((PermissionMask)((int)indirectPermissionMaskFromCallingObject | (int)navApplicationObjectBase.GetIndirectPermissionMask(ObjectId.ObjectNumber))));
		permissionMask2 |= additionalIndirectPermissions;
		permissionMask2 &= permissionMask;
		if (((uint)permissions & ((uint)permissionMask2 >> 5)) == (uint)permissions)
		{
			return true;
		}
		return checkTableEmpty?.Invoke(ObjectId.ObjectNumber) ?? false;
	}

	internal static void LogPermissionCheck(bool hasPermissions, ApplicationObjectId securableObject, NavSession session, PermissionMask permissions, NavApplicationObjectBase callingObject, PermissionMask indirectPermissionFromCaller = PermissionMask.None, PermissionValidationCategory validationCategory = PermissionValidationCategory.Normal)
	{
		LogPermissionCheck(hasPermissions, securableObject, session, permissions, callingObject?.ObjectId ?? default(ApplicationObjectId), indirectPermissionFromCaller, 0, validationCategory);
	}

	internal static void LogPermissionCheck(bool hasPermissions, ApplicationObjectId securableObject, NavSession session, PermissionMask permissions, ApplicationObjectId callingObjectId, PermissionMask indirectPermissionFromCaller = PermissionMask.None, int permissionManagerId = 0, PermissionValidationCategory validationCategory = PermissionValidationCategory.Normal)
	{
		if (hasPermissions)
		{
			NavServerEventSource.Log.WritePermissionCheckSuccessfulEvent(NavEnvironment.ServerInstanceName, session?.Tenant?.Id, session?.Id ?? (-1), (int)securableObject.ObjectType, securableObject.ObjectNumber, (int)permissions, (int)((callingObjectId != default(ApplicationObjectId)) ? callingObjectId.ObjectType : ((ObjectType)(-1))), (callingObjectId != default(ApplicationObjectId)) ? callingObjectId.ObjectNumber : (-1), (int)indirectPermissionFromCaller, (int)validationCategory);
		}
		else
		{
			NavServerEventSource.Log.WritePermissionCheckUnsuccessfulEvent(NavEnvironment.ServerInstanceName, session?.Tenant?.Id, session?.Id ?? (-1), (int)securableObject.ObjectType, securableObject.ObjectNumber, (int)permissions, (int)((callingObjectId != default(ApplicationObjectId)) ? callingObjectId.ObjectType : ((ObjectType)(-1))), (callingObjectId != default(ApplicationObjectId)) ? callingObjectId.ObjectNumber : (-1), (int)indirectPermissionFromCaller, (int)validationCategory);
		}
		if (FeatureSwitches.Instance.EnableSecurityTracing)
		{
			session?.Diagnostics.SendTraceTag("00000YD", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Session Id {0}\n PermissionManager Id {8}\n HasPermissions {7}\n Object Type {1}\n Object Id {2}\n PermissionMask {3}\n Calling Object Type {4}\n Calling Object Id {5}\n Indirect Permissions {6}\n Query category {7}", session?.Id, securableObject.ObjectType, securableObject.ObjectNumber, permissions, (int)((callingObjectId != default(ApplicationObjectId)) ? callingObjectId.ObjectType : ((ObjectType)(-1))), (callingObjectId != default(ApplicationObjectId)) ? callingObjectId.ObjectNumber : (-1), indirectPermissionFromCaller, hasPermissions, permissionManagerId, EnumHelper<PermissionValidationCategory>.EnumToString(validationCategory));
		}
	}

	internal bool InternalHasPermisisons(NavSession session, NavApplicationObjectBase securableObject, PermissionMask permissions, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty, PermissionMask additionalIndirectPermissions, bool countAsLimitedUserAllowedTable, PermissionValidationCategory validationCategory = PermissionValidationCategory.Normal)
	{
		NavServerEventSource.Log.WritePermissionCheckStartEvent(NavEnvironment.ServerInstanceName, session.Tenant?.Id, session.Id, (int)ObjectId.ObjectType, ObjectId.ObjectNumber, (int)permissions, (int)((callingObjectId != default(ApplicationObjectId)) ? callingObjectId.ObjectType : ((ObjectType)(-1))), (callingObjectId != default(ApplicationObjectId)) ? callingObjectId.ObjectNumber : (-1), (int)validationCategory);
		bool flag = ObjectId.ObjectType.IsExtensionType() || (ObjectId.ObjectType == ObjectType.Page && SystemPages.IsServerSystemPageId(ObjectId.ObjectNumber)) || ((ObjectId.ObjectType == ObjectType.TableData && limitedUserPermissionChecker != null) ? limitedUserPermissionChecker.CheckPermissionsForTableData(this, securableObject, permissions, checkTableEmpty, additionalIndirectPermissions, countAsLimitedUserAllowedTable) : InternalHasPermissionsDoNotCheckIncludedExcludedPermissions(securableObject, permissions, checkTableEmpty, additionalIndirectPermissions));
		PermissionMask indirectPermissionFromCaller = (PermissionMask)((int)indirectPermissionMaskFromCallingObject & ((int)permissions << 5));
		LogPermissionCheck(flag, ObjectId, session, permissions, callingObjectId, indirectPermissionFromCaller, PermissionManager.Id, validationCategory);
		return flag;
	}

	private NavPermissionException GeneratePermissionException(PermissionMask permissionMask)
	{
		int errorCode = 0;
		if ((permissionMask & PermissionMask.Read) == PermissionMask.Read)
		{
			errorCode = 18023807;
		}
		else if ((permissionMask & PermissionMask.Insert) == PermissionMask.Insert)
		{
			errorCode = 18023808;
		}
		else if ((permissionMask & PermissionMask.Delete) == PermissionMask.Delete)
		{
			errorCode = 18023809;
		}
		else if ((permissionMask & PermissionMask.Modify) == PermissionMask.Modify)
		{
			errorCode = 18023810;
		}
		else if ((permissionMask & PermissionMask.Execute) == PermissionMask.Execute)
		{
			errorCode = ((ObjectId.ObjectType == ObjectType.TableData) ? 18023698 : 18023703);
		}
		if (ObjectId.ObjectType == ObjectType.System && (permissionMask & PermissionMask.Execute) == PermissionMask.Execute && SystemObjects.TryGetSystemObjectCaption(ObjectId.ObjectNumber, out var caption))
		{
			return new NavPermissionException(errorCode, string.Format(CultureInfo.CurrentCulture, Lang.MissingSystemPermissions, caption));
		}
		return new NavPermissionException(errorCode, string.Format(CultureInfo.CurrentCulture, Lang.MissingPermissions, ObjectId.ObjectType, PermissionManager.GetObjectName(ObjectId), permissionMask, NavCurrentThread.SessionId));
	}
}
