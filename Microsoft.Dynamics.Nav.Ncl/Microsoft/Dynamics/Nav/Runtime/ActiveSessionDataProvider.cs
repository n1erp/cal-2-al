using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ActiveSessionDataProvider : EagerVirtualDataProvider
{
	private class SessionEventData
	{
		public Guid UserSID { get; private set; }

		public int SessionId { get; private set; }

		public ClientType ClientType { get; private set; }

		public NavDatabase Database { get; private set; }

		public string ClientComputerName { get; private set; }

		public string UserID { get; private set; }

		public CompanyId? CompanyId { get; private set; }

		public Guid SessionUniqueId { get; private set; }

		public DateTime LoginDatetime { get; private set; }

		public string TenantId { get; set; }

		public SessionEventData(Guid userSid, int sessionId, ClientType clientType, NavDatabase database, string clientComputerName, string userId, Guid sessionUniqueId, CompanyId? companyId)
		{
			UserSID = userSid;
			SessionId = sessionId;
			ClientType = clientType;
			Database = database;
			ClientComputerName = clientComputerName;
			UserID = userId;
			CompanyId = companyId;
			SessionUniqueId = sessionUniqueId;
			LoginDatetime = DateTime.UtcNow;
			TenantId = NavCurrentThread.Session?.Tenant?.Id;
		}
	}

	public static readonly ActiveSessionDataProvider Instance = new ActiveSessionDataProvider();

	private readonly ConcurrentDictionary<Guid, SessionEventData> sessions = new ConcurrentDictionary<Guid, SessionEventData>();

	public override int TableId => 2000000110;

	internal ActiveSessionDataProvider()
	{
	}

	public void CreateSession(Guid userSid, int sessionId, ClientType clientType, NavDatabase database, string clientComputerName, string userId, Guid sessionUniqueId, CompanyId? companyId)
	{
		sessions.TryAdd(sessionUniqueId, new SessionEventData(userSid, sessionId, clientType, database, clientComputerName, userId, sessionUniqueId, companyId));
	}

	public bool RemoveSession(Guid uniqueSessionId)
	{
		SessionEventData value;
		return sessions.TryRemove(uniqueSessionId, out value);
	}

	public bool IsSessionActive(Guid uniqueSessionId)
	{
		return sessions.ContainsKey(uniqueSessionId);
	}

	public bool TryFindSessionUniqueIdById(int sessionId, out Guid uniqueSessionId)
	{
		string text = NavCurrentThread.Session?.Tenant?.Id;
		foreach (KeyValuePair<Guid, SessionEventData> session in sessions)
		{
			if (session.Value.SessionId == sessionId && session.Value.TenantId == text)
			{
				uniqueSessionId = session.Key;
				return true;
			}
		}
		uniqueSessionId = Guid.Empty;
		return false;
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = true;
		NavSession session = NavCurrentThread.Session;
		List<ReadOnlyRecordBuffer> list = new List<ReadOnlyRecordBuffer>();
		NCLOptionMetadata fieldOptionMetadata = MetaTable.GetFieldByIndex(7).FieldOptionMetadata;
		string tenantId = NavCurrentThread.Session?.Tenant?.Id;
		if (!session.Database.IsSharedTenantSchema)
		{
			foreach (KeyValuePair<Guid, SessionEventData> item in sessions.Where((KeyValuePair<Guid, SessionEventData> s) => s.Value.TenantId == tenantId))
			{
				SessionEventData value = item.Value;
				list.Add(new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(NavEnvironment.Instance.GetServiceInstanceId()), NavInteger.Create(value.SessionId), NavGuid.CreateFromObject(value.UserSID), NavText.Create(NavEnvironment.ServerInstanceName), NavText.Create(NavEnvironment.ServerComputerName), NavText.Create(value.UserID), NavOption.Create(fieldOptionMetadata, (int)value.ClientType), NavText.Create(value.ClientComputerName), NavDateTime.Create(value.LoginDatetime), NavText.Create(value.Database.DatabaseName), NavGuid.CreateFromObject(value.SessionUniqueId)));
			}
		}
		else
		{
			foreach (KeyValuePair<Guid, SessionEventData> item2 in sessions.Where((KeyValuePair<Guid, SessionEventData> s) => s.Value.TenantId == tenantId))
			{
				SessionEventData value2 = item2.Value;
				list.Add(new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(NavEnvironment.Instance.GetServiceInstanceId()), NavInteger.Create(value2.SessionId), NavGuid.CreateFromObject(value2.UserSID), NavText.Create(NavEnvironment.ServerInstanceName), NavText.Create(NavEnvironment.ServerComputerName), NavText.Create(value2.UserID), NavOption.Create(fieldOptionMetadata, (int)value2.ClientType), NavText.Create(value2.ClientComputerName), NavDateTime.Create(value2.LoginDatetime), NavText.Create(value2.Database.DatabaseName), NavGuid.CreateFromObject(value2.SessionUniqueId), NavInteger.Create(value2.CompanyId.Value.Value)));
			}
		}
		return list;
	}
}
