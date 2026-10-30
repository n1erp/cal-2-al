using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Replication.DataContracts;
using Microsoft.Dynamics.Nav.Replication.DataContracts.Messages;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.HybridDeploy;

public class ALHybridDeployManagement : IDisposable
{
	private HybridDeployManagement deployer = new HybridDeployManagement(NavCurrentThread.Session);

	private bool disposedValue;

	public string CreateIntegrationRuntime(string sourceProduct)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		SourceProduct product;
		try
		{
			product = ConvertSourceProductToEnum(sourceProduct);
		}
		catch
		{
			throw new NavNCLArgumentException(sourceProduct, "CreateIntegrationRuntime", 1);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return deployer.CreateIntegrationRuntime(product);
		}, "CreateIntegrationRuntime");
	}

	public string DisableReplication(string sourceProduct)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		SourceProduct product;
		try
		{
			product = ConvertSourceProductToEnum(sourceProduct);
		}
		catch
		{
			throw new NavNCLArgumentException(sourceProduct, "DisableReplication", 1);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return deployer.DisableReplication(product);
		}, "DisableReplication");
	}

	[Obsolete("Use EnableReplication call with service webhook configuration parameters.")]
	public string EnableReplication(string sourceProduct, string onPremiseSqlConnection, string databaseType, string integrationRuntimeName, string notificationUrl, string clientState, string subscriptionId)
	{
		return EnableReplication(sourceProduct, onPremiseSqlConnection, databaseType, integrationRuntimeName, notificationUrl, clientState, subscriptionId, null, null, null);
	}

	public string EnableReplication(string sourceProduct, string onPremiseSqlConnection, string databaseType, string integrationRuntimeName, string notificationUrl, string clientState, string subscriptionId, string serviceNotificationUrl, string serviceClientState, string serviceSubscriptionId)
	{
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected Obj, but got Unknown
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected Obj, but got Unknown
			SourceProduct sourceProduct2;
			try
			{
				sourceProduct2 = ConvertSourceProductToEnum(sourceProduct);
			}
			catch (Exception)
			{
				throw new NavNCLArgumentException(sourceProduct, "EnableReplication", 1);
			}
			DatabaseConfiguration dbConfig;
			try
			{
				dbConfig = ConvertDatabaseTypeToEnum(databaseType);
			}
			catch
			{
				throw new NavNCLArgumentException(databaseType, "EnableReplication", 3);
			}
			WebhookConfiguration configuration = new WebhookConfiguration
			{
				NotificationUrl = notificationUrl,
				ClientState = clientState,
				SubscriptionId = subscriptionId
			};
			WebhookConfiguration serviceWebhookConfiguration = null;
			if (!string.IsNullOrWhiteSpace(serviceNotificationUrl) && !string.IsNullOrWhiteSpace(serviceClientState) && !string.IsNullOrWhiteSpace(serviceSubscriptionId))
			{
				serviceWebhookConfiguration = new WebhookConfiguration
				{
					NotificationUrl = serviceNotificationUrl,
					ClientState = serviceClientState,
					SubscriptionId = serviceSubscriptionId
				};
			}
			return deployer.EnableReplication(sourceProduct2, onPremiseSqlConnection, dbConfig, integrationRuntimeName, configuration, serviceWebhookConfiguration);
		}, "EnableReplication");
	}

	public ALGetStatusResponse GetRequestStatus(string requestTrackingId)
	{
		if (string.IsNullOrWhiteSpace(requestTrackingId))
		{
			throw new NavNCLArgumentException(requestTrackingId, "GetRequestStatus", 1);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() => deployer.GetRequestStatus(requestTrackingId), "GetRequestStatus");
	}

	public string GetIntegrationRuntimeKey(string sourceProduct)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		SourceProduct product;
		try
		{
			product = ConvertSourceProductToEnum(sourceProduct);
		}
		catch
		{
			throw new NavNCLArgumentException(sourceProduct, "GetIntegrationRuntimeKey", 1);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return deployer.GetIntegrationRuntimeKey(product);
		}, "GetIntegrationKey");
	}

	public string GetReplicationRunErrors(string sourceProduct, string runId)
	{
		return GetReplicationRunStatus(sourceProduct, runId);
	}

	public string GetReplicationRunStatus(string sourceProduct, string runId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		SourceProduct product;
		try
		{
			product = ConvertSourceProductToEnum(sourceProduct);
		}
		catch
		{
			throw new NavNCLArgumentException(sourceProduct, "GetReplicationRunErrors", 1);
		}
		if (string.IsNullOrWhiteSpace(runId))
		{
			throw new NavNCLArgumentException(runId, "GetReplicationRunErrors", 2);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return deployer.GetReplicationRunStatus(product, runId);
		}, "GetReplicationStatus");
	}

	public string GetVersionInformation(string sourceProduct)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		SourceProduct product;
		try
		{
			product = ConvertSourceProductToEnum(sourceProduct);
		}
		catch
		{
			throw new NavNCLArgumentException(sourceProduct, "GetVersionInformation", 1);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return deployer.GetVersionInformation(product);
		}, "GetVersionInformation");
	}

	public static void PrepareTablesForReplication()
	{
		HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			NavSession session = NavCurrentThread.Session;
			NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(session.Tenant.Database, TransactionType.UpdateNoLocks);
			try
			{
				NavSqlDdlCommands.EnableChangeTrackingForDatabase(session.Tenant.Database, conScope, force: true);
				IEnumerable<CloudReadyReplicationStatus> cloudReadyReplicationStatusCollection = NavSqlCloudReadyReplicationStatus.GetCloudReadyReplicationStatusCollection(conScope);
				cloudReadyReplicationStatusCollection.Where((CloudReadyReplicationStatus status) => status.CompanyName == session.CompanyName || string.IsNullOrWhiteSpace(status.CompanyName)).ForEach((CloudReadyReplicationStatus cloudReadyStatus) =>
				{
					NavSqlDdlCommands.EnableChangeTrackingForTable(cloudReadyStatus, conScope);
				});
			}
			finally
			{
				if (conScope != null)
				{
					((IDisposable)conScope).Dispose();
				}
			}
		}, "", null, "PrepareTablesForReplication");
	}

	public string RegenerateIntegrationRuntimeKey(string sourceProduct)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		SourceProduct product;
		try
		{
			product = ConvertSourceProductToEnum(sourceProduct);
		}
		catch
		{
			throw new NavNCLArgumentException(sourceProduct, "RegenerateIntegrationRuntimeKey", 1);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return deployer.RegenerateIntegrationRuntimeKey(product);
		}, "RegenerateIntegrationKey");
	}

	public static void ResetCloudData()
	{
		HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			NavSession session = NavCurrentThread.Session;
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(session.Tenant.Database, TransactionType.UpdateNoLocks);
			NavSqlCloudReadyReplicationStatus.ModifyAll(conScope, session.CompanyName, NavSqlCloudReadyReplicationStatus.FieldNo.SyncedVersion, 0);
		}, "", null, "ResetCloudData");
	}

	public string RunReplication(string sourceProduct)
	{
		return RunReplication(sourceProduct, 0);
	}

	public string RunReplication(string sourceProduct, int type)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		SourceProduct product;
		try
		{
			product = ConvertSourceProductToEnum(sourceProduct);
		}
		catch
		{
			throw new NavNCLArgumentException(sourceProduct, "RunReplication", 1);
		}
		ReplicationType replicationType = (ReplicationType)type;
		if (!Enum.IsDefined(typeof(ReplicationType), replicationType))
		{
			throw new NavNCLArgumentException(type, "RunReplication", 1);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			return deployer.RunReplication(product, replicationType);
		}, "RunReplication");
	}

	public string RunUpgrade(string sourceProduct)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		SourceProduct product;
		try
		{
			product = ConvertSourceProductToEnum(sourceProduct);
		}
		catch
		{
			throw new NavNCLArgumentException(sourceProduct, "RunUpgrade", 1);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return deployer.RunUpgrade(product);
		}, "RunUpgrade");
	}

	public string SetReplicationSchedule(string sourceProduct, string replicationFrequency, string daysToRun, DateTime timeToRun, bool activate)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		SourceProduct sourceProductType;
		try
		{
			sourceProductType = ConvertSourceProductToEnum(sourceProduct);
		}
		catch
		{
			throw new NavNCLArgumentException(sourceProduct, "SetReplicationSchedule", 1);
		}
		ReplicationFrequency repFrequencyType;
		try
		{
			repFrequencyType = ConvertReplicationFrequencyToEnum(replicationFrequency);
		}
		catch
		{
			throw new NavNCLArgumentException(replicationFrequency, "SetReplicationSchedule", 2);
		}
		DateTimeOffset scheduledTime;
		if (ServerUserSettings.Instance.ServicesDefaultTimeZone.Value == TimeZoneInfo.Utc)
		{
			scheduledTime = DateTime.SpecifyKind(timeToRun, DateTimeKind.Utc);
		}
		else
		{
			scheduledTime = DateTime.SpecifyKind(timeToRun, DateTimeKind.Local);
		}
		return HybridDeployDiagnosticSession.SendTraceTagOnFailure(() =>
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			return deployer.SetReplicationSchedule(sourceProductType, repFrequencyType, daysToRun, scheduledTime, activate);
		}, "SetReplicationSchedule");
	}

	private static DatabaseConfiguration ConvertDatabaseTypeToEnum(string databaseType)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return (DatabaseConfiguration)Enum.Parse(typeof(DatabaseConfiguration), databaseType);
	}

	private static ReplicationFrequency ConvertReplicationFrequencyToEnum(string replicationFrequency)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return (ReplicationFrequency)Enum.Parse(typeof(ReplicationFrequency), replicationFrequency);
	}

	private static SourceProduct ConvertSourceProductToEnum(string sourceProduct)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return (SourceProduct)Enum.Parse(typeof(SourceProduct), sourceProduct);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				deployer.Dispose();
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
