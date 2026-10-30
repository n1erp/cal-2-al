using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ExclusiveTenantActionScope : IDisposable
{
	private readonly bool exclusiveModeSupported = true;

	private readonly NavSession session;

	private readonly TenantConfigurationManager tenantConfigurationManager;

	private readonly ITenantConfigurationSynchronizer tenantConfigurationSynchronizer;

	private bool exclusiveStateAcquired;

	private bool isDisposed;

	private readonly bool stateOwner;

	private long tenantConfigurationInExclusiveAccessStateTimestamp;

	public bool IsDisposed => isDisposed;

	public bool IsActive
	{
		get
		{
			if (!isDisposed && exclusiveModeSupported)
			{
				return exclusiveStateAcquired;
			}
			return false;
		}
	}

	public ExclusiveTenantActionScope(NavSession session, ITenantConfigurationSynchronizer tenantConfigurationSynchronizer, TenantConfigurationManager tenantConfigurationManager)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (NavEnvironment.Instance.Tenants.IsSingleTenancyMode)
		{
			session.Tenant.Diagnostics.SendTraceTag("00000XS", Category.Tenant, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Exclusive tenant action scope is not supported if this server instance is not configured for multi tenancy. Change the Multitenant setting to true in the server configuration to enable multi tenancy on this server.");
			exclusiveModeSupported = false;
			return;
		}
		if (tenantConfigurationSynchronizer == null)
		{
			session.Tenant.Diagnostics.SendTraceTag("00000XT", Category.Tenant, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Exclusive tenant action scope is not supported without service instance running with heartbeat instance.");
			exclusiveModeSupported = false;
			return;
		}
		if (tenantConfigurationManager == null)
		{
			throw new ArgumentNullException("tenantConfigurationManager");
		}
		this.session = session;
		this.tenantConfigurationSynchronizer = tenantConfigurationSynchronizer;
		this.tenantConfigurationManager = tenantConfigurationManager;
		BeginExclusiveTenantAction();
		stateOwner = true;
	}

	public ExclusiveTenantActionScope(NavTenant tenant, string exclusiveAccessTicket, TenantConfigurationManager tenantConfigurationManager)
	{
		stateOwner = false;
		exclusiveStateAcquired = tenantConfigurationManager.ValidateExclusiveAccessTicket(tenant.Id, exclusiveAccessTicket);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void LockToCurrentTransaction()
	{
		string id = session.Tenant.Id;
		if (!tenantConfigurationManager.AcquireSharedLockForTheCurrentTransaction(id, tenantConfigurationInExclusiveAccessStateTimestamp))
		{
			throw new NavNCLExclusiveActionException(string.Format(CultureInfo.InvariantCulture, "Exclusive access operation was compromised for tenant {0}. This means that tenant configuration state was modified during before this scope was disposed.", id));
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if ((!isDisposed & disposing) && exclusiveModeSupported)
		{
			EndExclusiveTenantAction();
		}
		isDisposed = true;
	}

	private void BeginExclusiveTenantAction()
	{
		NavTenant tenant = session.Tenant;
		session.Diagnostics.SendTraceTag("00000WY", Category.Tenant, Verbosity.Normal, "BeginExclusiveTenantAction. TenantID={0}, ServerSession ID={1}", DiagnosticParameter.Oii(tenant.Id), DiagnosticParameter.SystemMetadata(session.Id));
		try
		{
			tenant.MoveTenantToExclusiveAccessMode();
			((ITenantSessionHandler)tenant).StopAllSessionsExcept(session);
			tenantConfigurationInExclusiveAccessStateTimestamp = ChangeTenantConfigurationStateToExclusiveAccessAcrossAllServers(tenant).Timestamp;
			exclusiveStateAcquired = true;
		}
		catch (Exception)
		{
			EndExclusiveTenantAction();
			throw;
		}
	}

	private void EndExclusiveTenantAction()
	{
		if (stateOwner)
		{
			NavTenant tenant = session.Tenant;
			tenant.Diagnostics.SendTraceTag("00000WZ", Category.Tenant, Verbosity.Normal, "EndExclusiveTenantAction. TenantID={0}, ServerSession ID={1}", DiagnosticParameter.Oii(tenant.Id), DiagnosticParameter.SystemMetadata(session.Id));
			if (exclusiveStateAcquired)
			{
				ChangeTenantConfigurationFromExclusiveAccessToActiveState();
				tenant.Diagnostics.SendTraceTag("00000XU", Category.Tenant, Verbosity.Normal, "Tenant {0} exited exclusive access mode on server that requested it (Server Instance ID = {1}).", DiagnosticParameter.Oii(tenant.Id), DiagnosticParameter.SystemMetadata(tenantConfigurationSynchronizer.ServiceInstanceId));
			}
			RefreshTenantStateUsingNewConnection();
		}
	}

	private void RefreshTenantStateUsingNewConnection()
	{
		NavTenant tenant = session.Tenant;
		tenant.Diagnostics.SendTraceTag("00001GK", Category.Tenant, Verbosity.Normal, "Calling NavTenant.RefreshState. TenantID={0}, ServerSession ID={1}", DiagnosticParameter.Oii(tenant.Id), DiagnosticParameter.SystemMetadata(session.Id));
		tenant.RestoreStateFromExclusiveAccessMode();
		using (NavSqlConnectionScope.CreateForceNewConnection(tenant.Database, updateOrlockingActivity: false))
		{
			tenant.RefreshState();
		}
	}

	private TenantConfiguration ChangeTenantConfigurationStateToExclusiveAccessAcrossAllServers(NavTenant tenant)
	{
		int serviceInstanceId = tenantConfigurationSynchronizer.ServiceInstanceId;
		TenantConfiguration result = tenantConfigurationManager.SetTenantConfigurationStateAndWaitWhilePropagated(tenant.Id, TenantConfigurationState.ExclusiveAccess, serviceInstanceId);
		if (tenant.IsDisposed)
		{
			throw new NavNCLExclusiveActionException("An exclusive access with the tenant cannot be established because the tenant has since been disposed.");
		}
		tenant.Diagnostics.SendTraceTag("00000XV", Category.Tenant, Verbosity.Normal, "Tenant {0} entered exclusive access mode on server that requested it (Server Instance ID = {1}).", DiagnosticParameter.Oii(tenant.Id), DiagnosticParameter.SystemMetadata(serviceInstanceId));
		return result;
	}

	private void ChangeTenantConfigurationFromExclusiveAccessToActiveState()
	{
		if (!exclusiveStateAcquired)
		{
			return;
		}
		try
		{
			tenantConfigurationManager.SetTenantConfigurationStateAndWaitWhilePropagated(session.Tenant.Id, TenantConfigurationState.Active, null, tenantConfigurationInExclusiveAccessStateTimestamp);
		}
		catch (NavTenantInvalidStateException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000YO", Category.Tenant, ex, Verbosity.Warning);
			throw new NavNCLExclusiveActionException(string.Format(CultureInfo.InvariantCulture, "Exclusive access operation was compromised for tenant {0}. This means that tenant configuration state was modified during before this scope was disposed.", session.Tenant.Id), ex);
		}
	}

	internal void Detach()
	{
		exclusiveStateAcquired = false;
	}
}
