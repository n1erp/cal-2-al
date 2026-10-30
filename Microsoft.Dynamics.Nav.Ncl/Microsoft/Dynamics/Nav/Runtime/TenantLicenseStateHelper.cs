using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class TenantLicenseStateHelper
{
	public static void CheckAndShiftState(NavTenant tenant, bool hasSubscribedSkus, bool hasEnabledSubscribedSkus)
	{
		((ITenantSessionHandler)tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession navSession) =>
		{
			using NavRecord tenantLicenseStateRec = new NavRecord(navSession, 2000000189, SecurityFiltering.Ignored);
			CheckAndShiftStateImpl(tenantLicenseStateRec, hasSubscribedSkus, hasEnabledSubscribedSkus);
		}), false, false, false, (LanguageSetting)null);
	}

	public static void ValidateStateTransition(TenantLicenseState currentState, TenantLicenseState newState)
	{
		if (newState == TenantLicenseState.LockedOut)
		{
			return;
		}
		if (newState == currentState && newState != TenantLicenseState.Trial)
		{
			throw new NavCSideException(Lang.CannotInsertTenantLicenseState);
		}
		switch (currentState)
		{
		case TenantLicenseState.Trial:
			if (newState == TenantLicenseState.Evaluation || newState == TenantLicenseState.Warning)
			{
				throw new NavCSideException(Lang.CannotInsertTenantLicenseState);
			}
			break;
		case TenantLicenseState.Paid:
			switch (newState)
			{
			case TenantLicenseState.Evaluation:
				throw new NavCSideException(Lang.CannotInsertTenantLicenseState);
			case TenantLicenseState.Trial:
				throw new NavCSideException(Lang.LicenseStateTrialAlreadyUsed);
			}
			break;
		case TenantLicenseState.Warning:
			if (newState != TenantLicenseState.Paid && newState != TenantLicenseState.Suspended && newState != TenantLicenseState.Deleted)
			{
				throw new NavCSideException(Lang.CannotInsertTenantLicenseState);
			}
			break;
		case TenantLicenseState.Suspended:
			if (newState != TenantLicenseState.Paid && newState != TenantLicenseState.Deleted && newState != TenantLicenseState.Trial)
			{
				throw new NavCSideException(Lang.CannotInsertTenantLicenseState);
			}
			break;
		case TenantLicenseState.Deleted:
			if (newState != TenantLicenseState.Paid)
			{
				throw new NavCSideException(Lang.CannotInsertTenantLicenseState);
			}
			break;
		case TenantLicenseState.LockedOut:
			throw new NavCSideException(Lang.CannotInsertTenantLicenseState);
		default:
			throw new NavCSideException(Lang.InvalidTenantLicenseState);
		case TenantLicenseState.Evaluation:
			break;
		}
	}

	public static TenantLicenseState GetStateFromRecord(NavRecord tenantLicenseStateRec)
	{
		int num = tenantLicenseStateRec.GetFieldValue(3).ToInt32();
		if (!Enum.IsDefined(typeof(TenantLicenseState), num))
		{
			throw new NavCSideException(Lang.InvalidTenantLicenseState);
		}
		return (TenantLicenseState)num;
	}

	private static void CheckAndShiftStateImpl(NavRecord tenantLicenseStateRec, bool hasSubscribedSkus, bool hasEnabledSubscribedSkus)
	{
		if (tenantLicenseStateRec.ALFindLast(DataError.TrapError))
		{
			TenantLicenseState stateFromRecord = GetStateFromRecord(tenantLicenseStateRec);
			TenantLicenseState tenantLicenseState = stateFromRecord;
			if (hasEnabledSubscribedSkus)
			{
				tenantLicenseState = TenantLicenseState.Paid;
			}
			else
			{
				switch (stateFromRecord)
				{
				case TenantLicenseState.Trial:
					if (hasSubscribedSkus)
					{
						tenantLicenseState = TenantLicenseState.Warning;
					}
					else if (IsStateExpired(tenantLicenseStateRec))
					{
						tenantLicenseState = TenantLicenseState.Suspended;
					}
					break;
				case TenantLicenseState.Paid:
					tenantLicenseState = TenantLicenseState.Warning;
					break;
				case TenantLicenseState.Warning:
					if (IsStateExpired(tenantLicenseStateRec))
					{
						tenantLicenseState = TenantLicenseState.Suspended;
					}
					break;
				case TenantLicenseState.Suspended:
					if (IsStateExpired(tenantLicenseStateRec))
					{
						tenantLicenseState = TenantLicenseState.Deleted;
					}
					break;
				}
			}
			if (tenantLicenseState != stateFromRecord)
			{
				InsertNewState(tenantLicenseStateRec, tenantLicenseState);
			}
		}
		else if (hasSubscribedSkus)
		{
			InsertNewState(tenantLicenseStateRec, hasEnabledSubscribedSkus ? TenantLicenseState.Paid : TenantLicenseState.Warning);
		}
	}

	private static bool IsStateExpired(NavRecord tenantLicenseStateRec)
	{
		return NavDateTime.Now > tenantLicenseStateRec.GetFieldValue(2).ToDateTime();
	}

	private static void InsertNewState(NavRecord tenantLicenseStateRec, TenantLicenseState newState)
	{
		tenantLicenseStateRec.SetFieldValue(1, NavDateTime.Now);
		tenantLicenseStateRec.SetFieldValue(3, NavInteger.Create((int)newState));
		tenantLicenseStateRec.SetFieldValue(4, new NavGuid(Guid.Empty));
		tenantLicenseStateRec.ALInsert();
	}

	private static void InsertTrialState(NavSession session)
	{
		bool commit = false;
		try
		{
			using (new MaximizedPermissionScope(session, new int[1] { 2000000189 }))
			{
				using NavRecord tenantLicenseStateRec = new NavRecord(session, 2000000189);
				session.BeginTransaction();
				session.EnsureWriteTransactionStarted();
				InsertNewState(tenantLicenseStateRec, TenantLicenseState.Trial);
				commit = true;
			}
		}
		catch (NavCSideDuplicateKeyException)
		{
		}
		finally
		{
			session.EndTransaction(commit);
		}
	}

	public static void EnsureTrialStarted(NavSession session)
	{
		if (session.TenantLicenseState == TenantLicenseState.Evaluation)
		{
			InsertTrialState(session);
		}
	}

	public static void ExtendTrialLicense(NavSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		string text = "Other user";
		bool flag = false;
		if (session.User.IsDelegatedAdmin)
		{
			text = "Partner";
			flag = true;
		}
		else if (session.User.IsNavAdminUser || session.User.IsDefaultSuper || session.Permissions.IsSuperInCurrentCompany(session.Company.CompanyNameToken))
		{
			text = "Admin user";
		}
		try
		{
			ValidateUserAllowedtoExtendTrialLicense(session);
			InsertTrialState(session);
			session.Diagnostics.SendTraceTag("000075Y", Category.License, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Tenant trial license extended by {0}. ExtendedByPartner={1}.", DiagnosticParameter.SystemMetadata(text), DiagnosticParameter.SystemMetadata(flag.ToString(CultureInfo.InvariantCulture)));
		}
		catch (NavCSideException exception)
		{
			session.Diagnostics.SendExceptionTag("000079I", Category.License, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "{0} attempted to extend Tenant trial license. ExtendedByPartner={1}.", text, flag.ToString(CultureInfo.InvariantCulture))));
			throw;
		}
	}

	public static void ValidateUserAllowedtoExtendTrialLicense(NavSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (!session.User.IsDelegatedAdmin && !session.User.IsNavAdminUser && !session.User.IsDefaultSuper && !session.Permissions.IsSuperInCurrentCompany(session.Company.CompanyNameToken))
		{
			throw new NavPermissionException(Lang.UserMustBeSuperOrAdminToPerformAction);
		}
		using NavRecord navRecord = new NavRecord(session, 2000000189, SecurityFiltering.Ignored);
		navRecord.ALSetFilter(3, "=%1", NavInteger.CreateFromObject(TenantLicenseState.Trial));
		if ((navRecord.ALCount >= 2 && !session.User.IsDelegatedAdmin) || navRecord.ALCount >= 3)
		{
			throw new NavPermissionException(Lang.TrialLicenseAlreadyExtended);
		}
	}
}
