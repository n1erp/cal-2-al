using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Replication.Client;
using Microsoft.Dynamics.Nav.Replication.Client.Core;
using Microsoft.Dynamics.Nav.Replication.DataContracts;
using Microsoft.Dynamics.Nav.Replication.DataContracts.Headers;
using Microsoft.Dynamics.Nav.Replication.DataContracts.Messages;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.HybridDeploy;

public class HybridDeployManagement : IDisposable
{
	private readonly ReplicationServiceClient client;

	private readonly NavSession session;

	private readonly ServerUserSettings settings;

	private bool disposedValue;

	public HybridDeployManagement(NavSession session)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected Obj, but got Unknown
		this.session = session ?? throw new ArgumentNullException("session");
		settings = ServerUserSettings.Instance;
		client = new ReplicationServiceClient(settings.HybridDeploymentProxyEndpoint.Value, (ReplicationServiceVersion)0, settings.LocalServiceApiKey?.Value);
		((MicroserviceClient)client).ServiceSslThumbprint = settings.ReverseProxyCertificateThumbprint.Value;
	}

	public string CreateIntegrationRuntime(SourceProduct sourceProduct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		ServiceRequestMessage replicationRequest = new ServiceRequestMessage
		{
			SourceProduct = sourceProduct
		};
		CheckStatusResponse val = ProcessReplicationRequest<ServiceRequestMessage, CheckStatusResponse>(replicationRequest, (Func<ServiceRequestMessage, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.CreateIntegrationRuntimeAsync);
		return val.Id;
	}

	public string DisableReplication(SourceProduct sourceProduct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		DisableReplicationRequest replicationRequest = new DisableReplicationRequest
		{
			SourceProduct = sourceProduct
		};
		CheckStatusResponse val = ProcessReplicationRequest<DisableReplicationRequest, CheckStatusResponse>(replicationRequest, (Func<DisableReplicationRequest, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.DisableReplicationAsync);
		return val.Id;
	}

	public string EnableReplication(SourceProduct sourceProduct, string onPremiseSqlConnection, DatabaseConfiguration dbConfig, string integrationRuntimeName, WebhookConfiguration configuration, WebhookConfiguration serviceWebhookConfiguration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected Obj, but got Unknown
		EnableReplicationRequest replicationRequest = new EnableReplicationRequest
		{
			DatabaseType = dbConfig,
			IntegrationRuntimeName = integrationRuntimeName,
			OnPremConnectionString = onPremiseSqlConnection,
			SourceProduct = sourceProduct,
			WebhookConfiguration = configuration,
			ServiceWebhookConfiguration = serviceWebhookConfiguration
		};
		CheckStatusResponse val = ProcessReplicationRequest<EnableReplicationRequest, CheckStatusResponse>(replicationRequest, (Func<EnableReplicationRequest, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.EnableReplicationAsync);
		return val.Id;
	}

	public ALGetStatusResponse GetRequestStatus(string requestTrackingId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		GetStatusRequest replicationRequest = new GetStatusRequest
		{
			InstanceId = requestTrackingId
		};
		GetStatusResponse val = ProcessReplicationRequest<GetStatusRequest, GetStatusResponse>(replicationRequest, (Func<GetStatusRequest, Task<(ReplicationServiceResponseHeaders, GetStatusResponse)>>)client.GetStatusAsync);
		ALGetStatusResponse aLGetStatusResponse = new ALGetStatusResponse();
		if (val.Output.HasValues)
		{
			aLGetStatusResponse.ResponseJson = val.Output.ToString();
		}
		aLGetStatusResponse.Status = ((object)val.Status/*cast due to constrained. prefix*/).ToString();
		aLGetStatusResponse.InstanceId = val.InstanceId;
		return aLGetStatusResponse;
	}

	public string GetIntegrationRuntimeKey(SourceProduct sourceProduct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		GetIntegrationKeyRequest replicationRequest = new GetIntegrationKeyRequest
		{
			SourceProduct = sourceProduct
		};
		CheckStatusResponse val = ProcessReplicationRequest<GetIntegrationKeyRequest, CheckStatusResponse>(replicationRequest, (Func<GetIntegrationKeyRequest, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.GetIntegrationKeyAsync);
		return val.Id;
	}

	public string GetReplicationRunStatus(SourceProduct sourceProduct, string runId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		GetReplicationRunRequest replicationRequest = new GetReplicationRunRequest
		{
			SourceProduct = sourceProduct,
			ReplicationRunId = runId
		};
		CheckStatusResponse val = ProcessReplicationRequest<GetReplicationRunRequest, CheckStatusResponse>(replicationRequest, (Func<GetReplicationRunRequest, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.GetReplicationStatusAsync);
		return val.Id;
	}

	public string GetVersionInformation(SourceProduct sourceProduct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		GetVersionInformationRequest replicationRequest = new GetVersionInformationRequest
		{
			SourceProduct = sourceProduct
		};
		CheckStatusResponse val = ProcessReplicationRequest<GetVersionInformationRequest, CheckStatusResponse>(replicationRequest, (Func<GetVersionInformationRequest, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.GetVersionInformationAsync);
		return val.Id;
	}

	public string RegenerateIntegrationRuntimeKey(SourceProduct sourceProduct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		ServiceRequestMessage replicationRequest = new ServiceRequestMessage
		{
			SourceProduct = sourceProduct
		};
		CheckStatusResponse val = ProcessReplicationRequest<ServiceRequestMessage, CheckStatusResponse>(replicationRequest, (Func<ServiceRequestMessage, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.RegenerateIntegrationKeyAsync);
		return val.Id;
	}

	public string RunReplication(SourceProduct sourceProduct, ReplicationType replicationType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		RunReplicationRequest replicationRequest = new RunReplicationRequest
		{
			SourceProduct = sourceProduct,
			ReplicationType = replicationType
		};
		CheckStatusResponse val = ProcessReplicationRequest<RunReplicationRequest, CheckStatusResponse>(replicationRequest, (Func<RunReplicationRequest, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.RunReplicationAsync);
		return val.Id;
	}

	public string RunUpgrade(SourceProduct sourceProduct)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected Obj, but got Unknown
		RunUpgradeRequest replicationRequest = new RunUpgradeRequest
		{
			SourceProduct = sourceProduct
		};
		CheckStatusResponse val = ProcessReplicationRequest<RunUpgradeRequest, CheckStatusResponse>(replicationRequest, (Func<RunUpgradeRequest, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.RunUpgradeAsync);
		return val.Id;
	}

	public string SetReplicationSchedule(SourceProduct sourceProduct, ReplicationFrequency replicationFrequency, string daysToRunText, DateTimeOffset scheduledTime, bool activate)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected Obj, but got Unknown
		SetReplicationScheduleRequest replicationRequest = new SetReplicationScheduleRequest
		{
			SourceProduct = sourceProduct,
			Frequency = replicationFrequency,
			DaysToRun = ConstructDaysToRun(daysToRunText),
			TimeToRun = scheduledTime,
			Activate = activate
		};
		CheckStatusResponse val = ProcessReplicationRequest<SetReplicationScheduleRequest, CheckStatusResponse>(replicationRequest, (Func<SetReplicationScheduleRequest, Task<(ReplicationServiceResponseHeaders, CheckStatusResponse)>>)client.SetScheduleAsync);
		return val.Id;
	}

	private T2 ProcessReplicationRequest<T1, T2>(T1 replicationRequest, Func<T1, Task<(ReplicationServiceResponseHeaders, T2)>> replicationFunc) where T1 : ServiceRequestMessage where T2 : ServiceResponseMessage
	{
		Guid guid = Guid.NewGuid();
		session.Diagnostics.SendTraceTag("000024C", Category.Security, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "The ClientRequestId for the request to Hybrid Proxy service: {0}", guid);
		client.ClientRequestId = guid;
		client.RequestTimeout = settings.HybridDeploymentProxyRequestTimeout.Value.Milliseconds;
		((ServiceRequestMessage)replicationRequest).TenantId = session.Tenant.Id;
		(ReplicationServiceResponseHeaders, T2) result = replicationFunc(replicationRequest).GetAwaiter().GetResult();
		session.Diagnostics.SendTraceTag("000024D", Category.Security, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "The ClientRequestId for the response from Hybrid Proxy service: {0}", result.Item1.ClientRequestId);
		return result.Item2;
	}

	private static IList<DayOfWeek> ConstructDaysToRun(string daysToRunText)
	{
		if (!string.IsNullOrEmpty(daysToRunText))
		{
			List<DayOfWeek> list = new List<DayOfWeek>();
			List<string> list2 = daysToRunText.TrimEnd(',').Split(',').ToList();
			{
				foreach (string item in list2)
				{
					try
					{
						list.Add((DayOfWeek)Enum.Parse(typeof(DayOfWeek), item));
					}
					catch (Exception ex)
					{
						throw (NavNCLException)ex;
					}
				}
				return list;
			}
		}
		return null;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				((MicroserviceClient)client).Dispose();
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
