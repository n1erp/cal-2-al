using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class TenantConfigurationManager : IDisposable
{
	private readonly TenantConfigurationStorage tenantConfigurationStorage;

	private readonly TenantDatabaseConfigurationStorage tenantDatabaseConfigurationStorage;

	private ConcurrentDictionary<string, TenantConfiguration> tenantConfigurations;

	private readonly NavDatabase database;

	private NonReEntrantTimerWrapper synchronizationTimer;

	private readonly int synchronizationPeriod;

	private readonly Func<NavTenantMountingParameters, NavTenant> addTenant;

	private readonly bool isStandalone;

	private readonly Action<NavTenantMountingParameters> replaceTenant;

	private readonly Action<string> removeTenant;

	private readonly Action<NavTenantSettings> addFailedTenant;

	private readonly Func<int> serverInstanceId;

	internal int manualPollCounter;

	private readonly ManualResetGate configurationCheckGate = new ManualResetGate();

	private readonly NavDiagnostics diagnostics;

	private readonly ITenantConfigurationSynchronizer tenantConfigurationSynchronizer;

	private long lastKnownTenantConfigurationTimestamp;

	private long lastKnownByOtherServersTenantConfigurationTimestamp;

	private readonly EventHandler<MinimumKnownConfigurationChangedEventArgs> tenantConfigurationTimestampAcknowledgedByOtherServersHandler;

	public TenantConfigurationManager(NavDatabase database, int synchronizationPeriod, ITenantConfigurationSynchronizer tenantConfigurationSynchronizer, Func<NavTenantMountingParameters, NavTenant> addTenant = null, Action<string> removeTenant = null, Action<NavTenantSettings> addFailedTenant = null, Action<NavTenantMountingParameters> replaceTenant = null, bool isStandalone = false, Func<int> serverInstanceId = null)
	{
		this.database = database ?? throw new ArgumentNullException("database");
		diagnostics = NavDiagnostics.GlobalInstance;
		manualPollCounter = 0;
		this.serverInstanceId = serverInstanceId ?? ((Func<int>)(() => 0));
		tenantConfigurationStorage = new TenantConfigurationStorage(database);
		tenantDatabaseConfigurationStorage = new TenantDatabaseConfigurationStorage(database);
		this.synchronizationPeriod = synchronizationPeriod;
		this.addTenant = addTenant;
		this.removeTenant = removeTenant;
		this.addFailedTenant = addFailedTenant;
		this.replaceTenant = replaceTenant;
		this.tenantConfigurationSynchronizer = tenantConfigurationSynchronizer;
		this.isStandalone = isStandalone;
		tenantConfigurations = new ConcurrentDictionary<string, TenantConfiguration>(StringComparer.OrdinalIgnoreCase);
		if (tenantConfigurationSynchronizer != null)
		{
			tenantConfigurationTimestampAcknowledgedByOtherServersHandler = (object sender, MinimumKnownConfigurationChangedEventArgs e) =>
			{
				lastKnownByOtherServersTenantConfigurationTimestamp = e.ConfigurationVersion;
			};
			tenantConfigurationSynchronizer.MinimumKnownConfigurationChanged += tenantConfigurationTimestampAcknowledgedByOtherServersHandler;
		}
	}

	private void HandleTenantConfigurationChange(TenantConfiguration oldConfiguration, TenantConfiguration newConfiguration)
	{
		if (newConfiguration == null)
		{
			throw new ArgumentNullException("newConfiguration");
		}
		if (newConfiguration.ExclusiveAccessOwnerServerId.HasValue && newConfiguration.ExclusiveAccessOwnerServerId == serverInstanceId())
		{
			return;
		}
		if (oldConfiguration == null)
		{
			diagnostics.SendTraceTag("00000WN", Category.Tenant, Verbosity.Normal, "Handling new tenant configuration. New Configuration State={0}, TenantId={1}", DiagnosticParameter.SystemMetadata(Enum.GetName(typeof(TenantConfigurationState), newConfiguration.State)), DiagnosticParameter.Oii(newConfiguration.Settings.Id));
			if (newConfiguration.CanTenantBeMounted && tenantConfigurations.TryAdd(newConfiguration.Settings.Id, newConfiguration))
			{
				tenantDatabaseConfigurationStorage.FillDatabaseInformation(newConfiguration.Settings);
				AddTenantFromConfiguration(newConfiguration, allowAsync: false);
			}
		}
		else
		{
			if (oldConfiguration.Timestamp >= newConfiguration.Timestamp)
			{
				return;
			}
			if (oldConfiguration.State == TenantConfigurationState.ExclusiveAccess && newConfiguration.State != TenantConfigurationState.ExclusiveAccess)
			{
				diagnostics.SendTraceTag("00000XR", Category.Runtime, Verbosity.Warning, "Tenant {0} exited exclusive access mode on server (Server Instance ID = {1}). This server was not a requestor of this mode.", DiagnosticParameter.Oii(newConfiguration.Settings.Id), DiagnosticParameter.SystemMetadata(tenantConfigurationSynchronizer.ServiceInstanceId));
			}
			diagnostics.SendTraceTag("00000WO", Category.Tenant, Verbosity.Normal, "Handling tenant configuration change. Old Configuration State={0}. New Configuration State={1}, TenantId={2}", DiagnosticParameter.SystemMetadata(Enum.GetName(typeof(TenantConfigurationState), oldConfiguration.State)), DiagnosticParameter.SystemMetadata(Enum.GetName(typeof(TenantConfigurationState), newConfiguration.State)), DiagnosticParameter.Oii(newConfiguration.Settings.Id));
			if (newConfiguration.State == TenantConfigurationState.Removed)
			{
				if (RemoveConfigurationFromDictionary(newConfiguration.Settings.Id))
				{
					removeTenant(newConfiguration.Settings.Id);
				}
				return;
			}
			if (oldConfiguration.CanTenantBeMounted)
			{
				removeTenant(oldConfiguration.Settings.Id);
			}
			tenantDatabaseConfigurationStorage.FillDatabaseInformation(newConfiguration.Settings);
			AddTenantFromConfiguration(newConfiguration, allowAsync: false);
			RemoveConfigurationFromDictionary(oldConfiguration.Settings.Id);
			tenantConfigurations.TryAdd(newConfiguration.Settings.Id, newConfiguration);
		}
	}

	public void RemountFromLocalConfiguration(string tenantId)
	{
		if (tenantConfigurations.TryGetValue(tenantId, out var value))
		{
			tenantDatabaseConfigurationStorage.FillDatabaseInformation(value.Settings);
			replaceTenant(new NavTenantMountingParameters
			{
				TenantSettings = value.Settings,
				VerifyServerInstanceKey = true,
				EnsureDatabaseExclusiveStateWhileOverwritingTenantId = true,
				AllowAsync = false,
				ExclusiveAccessMode = (value.State == TenantConfigurationState.ExclusiveAccess)
			});
		}
	}

	private void AddTenantFromConfiguration(TenantConfiguration tenantConfiguration, bool allowAsync = true)
	{
		addTenant(new NavTenantMountingParameters
		{
			TenantSettings = tenantConfiguration.Settings,
			VerifyServerInstanceKey = true,
			EnsureDatabaseExclusiveStateWhileOverwritingTenantId = true,
			AllowAsync = allowAsync,
			ExclusiveAccessMode = (tenantConfiguration.State == TenantConfigurationState.ExclusiveAccess)
		});
	}

	private void SynchronizeConfigurationCallback(object state)
	{
		NavSession session = NavCurrentThread.Session;
		NavCurrentThread.Session = null;
		try
		{
			Synchronize();
		}
		catch (NavCSideException ex)
		{
			int errorCode = ex.ErrorCode;
			if (errorCode != 22924076 && errorCode != 22926090)
			{
				LogWriterHelper.LogExceptionEvent(ex, EventLogEntryType.Error);
				diagnostics.SendExceptionTag("000021S", Category.Runtime, ex);
			}
		}
		catch (NavBaseException exception)
		{
			LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
			diagnostics.SendExceptionTag("000021T", Category.Runtime, exception);
		}
		catch (Exception exception2)
		{
			LogWriterHelper.LogExceptionEvent(exception2, EventLogEntryType.Error);
			diagnostics.SendExceptionTag("000021U", Category.Runtime, exception2);
			throw;
		}
		finally
		{
			NavCurrentThread.Session = session;
		}
	}

	internal void CheckForConfigurationChanges()
	{
		try
		{
			if (!configurationCheckGate.TryEnter())
			{
				return;
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000021V", Category.Tenant, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Polling application database for tenant configuration changes");
			long num = lastKnownTenantConfigurationTimestamp;
			ConcurrentDictionary<string, TenantConfiguration> source;
			long maxTenantConfigurationTimestamp;
			using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true))
			{
				source = tenantConfigurationStorage.LoadUnknownOrChangedConfigurations(conScope, lastKnownTenantConfigurationTimestamp);
				maxTenantConfigurationTimestamp = tenantConfigurationStorage.GetMaxTenantConfigurationTimestamp(conScope);
			}
			List<Tuple<TenantConfiguration, TenantConfiguration>> source2 = source.Select((KeyValuePair<string, TenantConfiguration> kvp) =>
			{
				TenantConfiguration value = kvp.Value;
				tenantConfigurations.TryGetValue(value.Settings.Id, out var value2);
				return Tuple.Create(value2, value);
			}).ToList();
			foreach (Tuple<TenantConfiguration, TenantConfiguration> item3 in source2.OrderBy((Tuple<TenantConfiguration, TenantConfiguration> c) => c.Item2.Timestamp))
			{
				TenantConfiguration item = item3.Item1;
				TenantConfiguration item2 = item3.Item2;
				if (!NavEnvironment.Instance.InServerShutdown)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001JN", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, FormattableString.Invariant($"Applying tenant configuration change. Current version: {lastKnownTenantConfigurationTimestamp}. Change version: {item2.Timestamp}."));
					HandleTenantConfigurationChange(item, item2);
					lastKnownTenantConfigurationTimestamp = item2.Timestamp;
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001C4", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, FormattableString.Invariant($"Tenant configuration change applied. Current version: {lastKnownTenantConfigurationTimestamp}."));
					continue;
				}
				break;
			}
			if (maxTenantConfigurationTimestamp < lastKnownTenantConfigurationTimestamp)
			{
				lastKnownTenantConfigurationTimestamp = maxTenantConfigurationTimestamp;
			}
			if (num != lastKnownTenantConfigurationTimestamp)
			{
				tenantConfigurationSynchronizer?.AcknowledgeConfigurationChange(lastKnownTenantConfigurationTimestamp);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001JO", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, FormattableString.Invariant($"Acknowledged tenant configuration change of version: {lastKnownTenantConfigurationTimestamp}."));
			}
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00009M6", Category.Runtime, exception);
			throw;
		}
		finally
		{
			configurationCheckGate.Reset();
		}
	}

	public void Synchronize()
	{
		Interlocked.Exchange(ref manualPollCounter, 0);
		CheckForConfigurationChanges();
	}

	public void PerformInitialMounting(bool verifyServerInstanceKey = true)
	{
		if (synchronizationTimer != null)
		{
			throw new InvalidOperationException("Should only be called once");
		}
		ITenantSessionHandler tenant = NavGlobal.AppDatabase.Tenant;
		ConcurrentDictionary<string, TenantConfiguration> initialConfigurations;
		tenant.RunTenantActionInSystemSession((NavTenant navTenant, NavSession session) =>
		{
			RecoverTenantsFromExclusiveAccessState();
			using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true))
			{
				tenantConfigurations = (initialConfigurations = tenantConfigurationStorage.LoadUnknownOrChangedConfigurations(conScope, 0L));
			}
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true);
			string message = string.Format(CultureInfo.InvariantCulture, "Tenant config loaded from database {0} on server {1} containing {2} tenants.", database.DatabaseName, database.ServerName, initialConfigurations.Count);
			NavServerEventSource.Log.WriteTenantConfigLoadedEvent(NavEnvironment.ServiceInstanceName, navTenant?.Id, message);
			foreach (TenantConfiguration item in from kvp in initialConfigurations
				select kvp.Value into tenanConfiguration
				where tenanConfiguration.CanTenantBeMounted
				select tenanConfiguration)
			{
				if (NavEnvironment.Instance.InServerShutdown)
				{
					break;
				}
				diagnostics.SendTraceTag("00000WQ", Category.Tenant, Verbosity.Verbose, "Performing initial mounting for tenant '{0}'.", DiagnosticParameter.Oii(item.Settings.Id));
				AddTenant(item, verifyServerInstanceKey);
				lastKnownTenantConfigurationTimestamp = item.Timestamp;
			}
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
		});
		if (synchronizationPeriod != 0)
		{
			synchronizationTimer = new NonReEntrantTimerWrapper("TenantConfigurationManager", SynchronizeConfigurationCallback, null, synchronizationPeriod);
		}
	}

	private void AddTenant(TenantConfiguration tenantConfiguration, bool verifyServerInstanceKey)
	{
		NavTenantSettings settings = tenantConfiguration.Settings;
		try
		{
			tenantDatabaseConfigurationStorage.FillDatabaseInformation(settings);
		}
		catch (NavBaseException ex)
		{
			string text = string.Format(CultureInfo.InvariantCulture, "Tenant '{0}' could not be mounted due to the following error: {1}", settings.Id, ex.Message);
			NavServerEventSource.Log.WriteTenantMountFailedEvent(NavEnvironment.ServiceInstanceName, settings.Id, string.Join(Environment.NewLine, text, ExceptionHandler.FormatException(ex)));
			addFailedTenant(settings);
			return;
		}
		addTenant(new NavTenantMountingParameters
		{
			TenantSettings = settings,
			VerifyServerInstanceKey = verifyServerInstanceKey,
			EnsureDatabaseExclusiveStateWhileOverwritingTenantId = true,
			AllowAsync = true,
			ExclusiveAccessMode = (tenantConfiguration.State == TenantConfigurationState.ExclusiveAccess)
		});
	}

	public void AddTenantConfiguration(NavTenantSettings tenantSettings)
	{
		if (tenantConfigurations.TryGetValue(tenantSettings.Id, out var value) && value.CanTenantBeMounted)
		{
			throw new InvalidOperationException("Active tenantconfiguration with the same id already exists");
		}
		using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true))
		{
			TenantConfiguration value2 = null;
			bool flag = false;
			do
			{
				try
				{
					value2 = tenantConfigurationStorage.AddTenantConfiguration(conScope, tenantSettings);
					flag = true;
				}
				catch (NavCSideDuplicateKeyException)
				{
					TenantConfiguration tenantConfiguration = tenantConfigurationStorage.GetTenantConfiguration(conScope, tenantSettings.Id, withLock: true);
					if (tenantConfiguration.State != TenantConfigurationState.Removed)
					{
						throw new NavTenantAlreadyExistsException(string.Format(CultureInfo.CurrentCulture, Lang.TenantAlreadyExists, tenantSettings.Id));
					}
					tenantConfigurationStorage.RemoveTenantConfiguration(conScope, tenantSettings.Id, tenantConfiguration.Timestamp);
				}
			}
			while (!flag);
			if (value != null)
			{
				tenantConfigurations.TryRemove(tenantSettings.Id, out var _);
			}
			tenantConfigurations.TryAdd(tenantSettings.Id, value2);
		}
		if (!isStandalone)
		{
			SetTenantConfigurationStateAndWaitWhilePropagated(tenantSettings.Id, TenantConfigurationState.Active, null, null, null, ignoreStateSyncErrors: true);
		}
	}

	public TenantConfiguration SetTenantConfigurationStateToRemoved(string tenantId)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true);
		TenantConfiguration tenantConfiguration = tenantConfigurationStorage.GetTenantConfiguration(conScope, tenantId, withLock: true);
		return SetTenantConfigurationState(tenantConfiguration, TenantConfigurationState.Removed);
	}

	public bool ValidateExclusiveAccessTicket(string tenantId, string exclusiveAccessTicket)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		TenantConfiguration tenantConfiguration = tenantConfigurationStorage.GetTenantConfiguration(conScope, tenantId);
		return exclusiveAccessTicket == tenantConfiguration.CalculateExclusiveAccessTicket();
	}

	public void RemoveTenantConfiguration(string tenantId)
	{
		diagnostics.SendTraceTag("00000Y4", Category.Tenant, Verbosity.Verbose, "Tenant with the ID {0} is being is dismounted on all alive server instances.", DiagnosticParameter.Oii(tenantId));
		TenantConfiguration tenantConfiguration = SetTenantConfigurationStateAndWaitWhilePropagated(tenantId, TenantConfigurationState.Removed, null, null, null, ignoreStateSyncErrors: true);
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true);
		TenantConfiguration tenantConfiguration2 = tenantConfigurationStorage.GetTenantConfiguration(conScope, tenantId, withLock: true);
		if (tenantConfiguration2 == null)
		{
			RemoveConfigurationFromDictionary(tenantId);
			return;
		}
		if (tenantConfiguration2.Timestamp != tenantConfiguration.Timestamp)
		{
			throw new NavTenantInvalidStateException(string.Format(CultureInfo.InvariantCulture, "Tenant with the ID {0} cannot be dismounted because tenant configuration was changed during dismount process.", tenantId));
		}
		RemoveConfigurationFromDictionary(tenantId);
		tenantConfigurationStorage.RemoveTenantConfiguration(conScope, tenantId, tenantConfiguration2.Timestamp);
	}

	public TenantConfiguration SetTenantConfigurationState(TenantConfiguration currentTenantConfiguration, TenantConfigurationState desiredState, int? exclusiveAccessOwnerServerInstanceId = null)
	{
		string id = currentTenantConfiguration.Settings.Id;
		ValidateStateTransition(currentTenantConfiguration, desiredState);
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		TenantConfiguration tenantConfiguration = tenantConfigurationStorage.UpdateTenantConfiguration(conScope, currentTenantConfiguration, desiredState, exclusiveAccessOwnerServerInstanceId);
		if (tenantConfiguration == null)
		{
			throw new NavTenantInvalidStateException(string.Format(CultureInfo.InvariantCulture, "Cannot change state of tenant '{0}' to {1}, because tenant state was changed by another transaction.", id, desiredState));
		}
		RemoveConfigurationFromDictionary(id);
		if (!tenantConfigurations.TryAdd(id, tenantConfiguration))
		{
			throw new NavTenantInvalidStateException(string.Format(CultureInfo.InvariantCulture, "Cannot change state of tenant '{0}' from {1} to {2}.", id, tenantConfiguration.State, desiredState));
		}
		return tenantConfiguration;
	}

	public TenantConfiguration UpdateTenantConfigurationSettings(string tenantId, NavTenantSettings tenantSettings)
	{
		NavSqlConnectionScope current = NavSqlConnectionScope.Current;
		tenantConfigurations.TryGetValue(tenantId, out var value);
		if (value == null)
		{
			throw NavTenantNotFoundException.CreateForGeneralNotFound(tenantId);
		}
		tenantConfigurationStorage.RemoveTenantConfiguration(current, value.Settings.Id, value.Timestamp);
		TenantConfiguration tenantConfiguration = tenantConfigurationStorage.AddTenantConfiguration(current, tenantSettings);
		RemoveConfigurationFromDictionary(tenantId);
		tenantConfigurations.TryAdd(tenantId, tenantConfiguration);
		return tenantConfiguration;
	}

	public bool AcquireSharedLockForTheCurrentTransaction(string tenantId, long expectedTenantConfigurationTimestamp)
	{
		NavSqlConnectionScope current = NavSqlConnectionScope.Current;
		return tenantConfigurationStorage.ConvertOptimisticToPessimisticSharedLock(current, tenantId, expectedTenantConfigurationTimestamp);
	}

	private static void ValidateStateTransition(TenantConfiguration currentTenantConfiguration, TenantConfigurationState desiredState)
	{
		string id = currentTenantConfiguration.Settings.Id;
		bool flag = desiredState == TenantConfigurationState.ExclusiveAccess && currentTenantConfiguration.State != TenantConfigurationState.Active;
		bool flag2 = desiredState == TenantConfigurationState.Removed && currentTenantConfiguration.State != TenantConfigurationState.Active;
		bool flag3 = currentTenantConfiguration.State == TenantConfigurationState.ExclusiveAccess && ExclusiveTenantActionElevatedScope.HasElevatedAccess;
		if (flag || (flag2 && !flag3))
		{
			throw new NavTenantInvalidStateException(string.Format(CultureInfo.InvariantCulture, "Cannot change state of tenant '{0}' from {1} to {2}.", id, currentTenantConfiguration.State, desiredState));
		}
	}

	public TenantConfiguration SetTenantConfigurationStateAndWaitWhilePropagated(string tenantId, TenantConfigurationState desiredState, int? exclusiveAccessOwnerServerInstanceId = null, long? expectedTenantConfigurationTimestamp = null, string exclusiveAccessTicket = null, bool ignoreStateSyncErrors = false)
	{
		if (tenantConfigurationSynchronizer == null)
		{
			throw new InvalidOperationException("\"Setting tenant configuration state with wait is not supported without valid tenant configuration synchronizer.\"");
		}
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		TenantConfiguration tenantConfiguration2;
		using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true))
		{
			TenantConfiguration tenantConfiguration = tenantConfigurationStorage.GetTenantConfiguration(conScope, tenantId, withLock: true);
			if (tenantConfiguration == null)
			{
				throw NavTenantNotFoundException.CreateForGeneralNotFound(tenantId);
			}
			if (expectedTenantConfigurationTimestamp.HasValue && tenantConfiguration.Timestamp != expectedTenantConfigurationTimestamp.Value)
			{
				throw new NavTenantInvalidStateException(string.Format(CultureInfo.InvariantCulture, "Cannot change state of tenant '{0}' to {1}, because tenant state was changed by another transaction.", tenantId, desiredState));
			}
			if (exclusiveAccessTicket != null && exclusiveAccessTicket != tenantConfiguration.CalculateExclusiveAccessTicket())
			{
				throw new NavNCLExclusiveActionException(string.Format(CultureInfo.InvariantCulture, "Exclusive access ticket is invalid for the tenant {0}", tenantId));
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000010J", Category.Tenant, Verbosity.Normal, "Changing tenant configuration state of tenant {0} to {1} and waiting while it is propagated across all server instances.", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(desiredState));
			tenantConfiguration2 = SetTenantConfigurationState(tenantConfiguration, desiredState, exclusiveAccessOwnerServerInstanceId);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001C5", Category.Tenant, Verbosity.Normal, "Waiting for tenant configuration state propagation for tenant {0}. Required configuration version {1}.", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(tenantConfiguration2.Timestamp));
		try
		{
			tenantConfigurationSynchronizer.WaitForConfigurationSynchronization(tenantConfiguration2.Timestamp, ServerUserSettings.Instance.StateSynchronizationTimeout.TotalSeconds * 1000);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000XW", Category.Tenant, Verbosity.Normal, "Tenant configuration state propagation of tenant {0} to state {1} completed in {2}.", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(desiredState), DiagnosticParameter.SystemMetadata(stopwatch.Elapsed));
		}
		catch (NavTenantStateSynchronizationException exception)
		{
			diagnostics.SendExceptionTag("00001ZJ", Category.Tenant, exception, ignoreStateSyncErrors ? Verbosity.Normal : Verbosity.Warning);
			if (!ignoreStateSyncErrors)
			{
				throw;
			}
		}
		return tenantConfiguration2;
	}

	internal void RecoverTenantsFromExclusiveAccessState()
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true);
		tenantConfigurationStorage.RecoverTenantsFromExclusiveAccessState(conScope);
	}

	internal void DeleteRemovedTenantConfigurationsIfAcknowledged()
	{
		if (lastKnownByOtherServersTenantConfigurationTimestamp > 0)
		{
			long num = Math.Min(lastKnownByOtherServersTenantConfigurationTimestamp, lastKnownTenantConfigurationTimestamp);
			diagnostics.SendTraceTag("00000Y6", Category.Tenant, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Deleting tenant configuration records with status Removed. Tenant configuration timestamp acknowledged by all server instances is {0}.", num);
			int num2;
			using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true))
			{
				num2 = tenantConfigurationStorage.DeleteRemovedTenantConfigurationsIfAcknowledged(conScope, num);
			}
			if (num2 > 0)
			{
				diagnostics.SendTraceTag("00000Y7", Category.Tenant, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Tenant configuration records with status Removed has been deleted. Number of deleted records {0}.", num2);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool RemoveConfigurationFromDictionary(string tenantId)
	{
		TenantConfiguration value;
		return tenantConfigurations.TryRemove(tenantId, out value);
	}

	internal bool TenantConfigurationExists(string tenantId)
	{
		return tenantConfigurations.ContainsKey(tenantId);
	}

	public void Dispose()
	{
		InterlockedAction.Dispose(ref synchronizationTimer);
		if (tenantConfigurationSynchronizer != null && tenantConfigurationTimestampAcknowledgedByOtherServersHandler != null)
		{
			tenantConfigurationSynchronizer.MinimumKnownConfigurationChanged -= tenantConfigurationTimestampAcknowledgedByOtherServersHandler;
		}
	}
}
