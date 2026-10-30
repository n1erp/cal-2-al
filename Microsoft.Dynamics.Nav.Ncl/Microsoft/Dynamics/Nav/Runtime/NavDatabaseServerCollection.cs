using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavDatabaseServerCollection : IDisposable
{
	private class DatabaseServerKey
	{
		private readonly string serverName;

		private readonly string userName;

		private readonly ProtectedDatabasePassword password;

		public string Servername => serverName;

		public string UserName => userName;

		public ProtectedDatabasePassword Password => password;

		public DatabaseServerKey(string serverName, string userName, ProtectedDatabasePassword password)
		{
			this.serverName = NavDatabaseServer.NormalizeServerName(serverName);
			this.userName = (string.IsNullOrEmpty(userName) ? null : userName);
			this.password = password;
		}

		public override bool Equals(object obj)
		{
			DatabaseServerKey databaseServerKey = obj as DatabaseServerKey;
			bool flag = string.Equals(serverName, databaseServerKey.serverName, StringComparison.OrdinalIgnoreCase) && string.Equals(userName, databaseServerKey.userName, StringComparison.Ordinal) && password == null == (databaseServerKey.password == null);
			if (flag && password != null)
			{
				flag = string.Equals(password.PlaintextPassword, databaseServerKey.password.PlaintextPassword, StringComparison.Ordinal);
			}
			return flag;
		}

		public override int GetHashCode()
		{
			return HashCodeHelper.CombineHashCodes(serverName.GetHashCode(), (userName != null) ? userName.GetHashCode() : 0);
		}
	}

	private readonly Dictionary<DatabaseServerKey, NavDatabaseServer> servers = new Dictionary<DatabaseServerKey, NavDatabaseServer>();

	public IReadOnlyList<NavDatabaseServer> AllServers
	{
		get
		{
			lock (servers)
			{
				return servers.Values.ToList().AsReadOnly();
			}
		}
	}

	public NavDatabaseServer GetOrAddDatabaseServerByName(string serverName, string databaseUserName, ProtectedDatabasePassword databasePassword)
	{
		lock (servers)
		{
			if (string.IsNullOrEmpty(serverName))
			{
				if (servers.Count == 0)
				{
					throw new InvalidOperationException("No database servers are known to the system.");
				}
				serverName = servers.First().Value.ServerName;
			}
			DatabaseServerKey databaseServerKey = new DatabaseServerKey(serverName, databaseUserName, databasePassword);
			if (!servers.TryGetValue(databaseServerKey, out var value))
			{
				return AddServer(databaseServerKey);
			}
			return value;
		}
	}

	public NavDatabaseServer GetDatabaseServerByName(string serverName, string databaseUserName, ProtectedDatabasePassword databasePassword)
	{
		lock (servers)
		{
			DatabaseServerKey key = new DatabaseServerKey(serverName, databaseUserName, databasePassword);
			servers.TryGetValue(key, out var value);
			return value;
		}
	}

	public NavDatabaseServer AddServer(string serverName, string databaseUserName = null, ProtectedDatabasePassword databasePassword = null)
	{
		return AddServer(new DatabaseServerKey(serverName, databaseUserName, databasePassword));
	}

	private NavDatabaseServer AddServer(DatabaseServerKey serverKey)
	{
		lock (servers)
		{
			if (servers.TryGetValue(serverKey, out var value))
			{
				throw new InvalidOperationException("Attempted to add a a database server with the same name. Database server name =" + serverKey.Servername);
			}
			value = new NavDatabaseServer(serverKey.Servername, serverKey.UserName, serverKey.Password);
			servers.Add(serverKey, value);
			return value;
		}
	}

	public void Dispose()
	{
		IList<NavDatabaseServer> list;
		lock (servers)
		{
			list = servers.Values.ToList();
			servers.Clear();
		}
		foreach (NavDatabaseServer item in list)
		{
			item.Dispose();
		}
	}
}
