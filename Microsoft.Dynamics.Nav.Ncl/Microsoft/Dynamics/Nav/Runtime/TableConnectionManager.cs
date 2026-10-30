using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TableConnectionManager : TreeObject
{
	private readonly Dictionary<TableConnectionType, Dictionary<string, TableConnection>> tableConnectionStrings = new Dictionary<TableConnectionType, Dictionary<string, TableConnection>>();

	private readonly Dictionary<TableConnectionType, string> defaultTableConnection = new Dictionary<TableConnectionType, string>();

	internal TableConnectionManager(ITreeObject parent)
		: base(parent)
	{
	}

	private TableConnection CreateTableConnection(TableConnectionType connectionType, string name, string connectionString)
	{
		return connectionType switch
		{
			TableConnectionType.CRM => (TableConnection)CrmTableConnection.Create(this, Tree.Session, name, connectionString), 
			TableConnectionType.ExternalSQL => ExternalSqlTableConnection.Create(this, Tree.Session, name, connectionString), 
			TableConnectionType.Exchange => ExchangeTableConnection.Create(this, Tree.Session, name, connectionString), 
			TableConnectionType.MicrosoftGraph => GraphTableConnection.Create(this, Tree.Session, name, connectionString), 
			TableConnectionType.Query => QueryTableConnection.Create(this, Tree.Session, name), 
			_ => throw new NavNotSupportedException(connectionType.ToString()), 
		};
	}

	private TableConnection CreateTableConnection(TableConnectionType connectionType, string name, TableConnectionSettingsBase connectionSettings)
	{
		if (connectionType == TableConnectionType.ExternalSQL)
		{
			return ExternalSqlTableConnection.Create(this, Tree.Session, name, (ExternalSqlTableConnectionSettings)connectionSettings);
		}
		throw new NavNotSupportedException(connectionType.ToString());
	}

	internal void RegisterTableConnection(TableConnectionType connectionType, string name, string connectionString)
	{
		Dictionary<string, TableConnection> tableConnectionForConnectionType = GetTableConnectionForConnectionType(connectionType);
		string text = NormalizeName(name);
		if (tableConnectionForConnectionType.TryGetValue(text, out var _))
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.TableConnectionAlreadyRegistered, name, connectionType));
		}
		if (NavGlobal.AppDatabase.TableConnectionSettingsStorage.Get(connectionType, text) != null)
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.TableConnectionAlreadyAdded, name, connectionType));
		}
		TableConnection value2 = CreateTableConnection(connectionType, name, connectionString);
		tableConnectionForConnectionType.Add(text, value2);
	}

	internal void UnregisterTableConnection(TableConnectionType connectionType, string name)
	{
		name = NormalizeName(name);
		TableConnection tableConnection = FindTableConnection<TableConnection>(connectionType, name);
		if (tableConnection != null)
		{
			if (tableConnection.SupportsTransactions)
			{
				NavCurrentThread.Session.Rollback();
			}
			GetTableConnectionForConnectionType(connectionType).Remove(name);
			tableConnection.Dispose();
		}
		if (GetDefaultTableConnection(connectionType) == name)
		{
			SetDefaultTableConnection(connectionType, null, scoped: false);
		}
	}

	internal void SetDefaultTableConnection(TableConnectionType connectionType, string name, bool scoped)
	{
		if (scoped)
		{
			NavMethodScope currentMethodScope = Tree.FindParentType<NavSession>().CurrentMethodScope;
			currentMethodScope.SetDefaultTableConnection(connectionType, NormalizeName(name));
			return;
		}
		defaultTableConnection.Remove(connectionType);
		if (name != null)
		{
			defaultTableConnection.Add(connectionType, NormalizeName(name));
		}
	}

	internal string GetDefaultTableConnection(TableConnectionType connectionType)
	{
		string value = Tree.FindParentType<NavSession>().CurrentMethodScope.GetDefaultTableConnectionInScope(connectionType);
		if (value == null && !defaultTableConnection.TryGetValue(connectionType, out value))
		{
			return TableConnection.DefaultTableConnectionName;
		}
		TableConnection tableConnection = FindTableConnection<TableConnection>(connectionType, value);
		if (tableConnection == null)
		{
			return value;
		}
		return tableConnection.Name;
	}

	internal string GetCurrentTableConnectionName(TableConnectionType connectionType)
	{
		string value = Tree.FindParentType<NavSession>().CurrentMethodScope.GetDefaultTableConnectionInScope(connectionType);
		if (value == null && !defaultTableConnection.TryGetValue(connectionType, out value))
		{
			return TableConnection.DefaultTableConnectionName;
		}
		return value;
	}

	internal T GetCurrentTableConnection<T>(TableConnectionType connectionType) where T : TableConnection
	{
		string currentTableConnectionName = GetCurrentTableConnectionName(connectionType);
		Dictionary<string, TableConnection> tableConnectionForConnectionType = GetTableConnectionForConnectionType(connectionType);
		if (connectionType == TableConnectionType.Query && tableConnectionForConnectionType.Count == 0)
		{
			NavCurrentThread.Session.TableConnectionManager.RegisterTableConnection(connectionType, "@Query@", string.Empty);
			NavCurrentThread.Session.TableConnectionManager.SetDefaultTableConnection(connectionType, "@Query@", scoped: false);
			currentTableConnectionName = GetCurrentTableConnectionName(connectionType);
			tableConnectionForConnectionType = GetTableConnectionForConnectionType(connectionType);
		}
		if (!tableConnectionForConnectionType.TryGetValue(currentTableConnectionName, out var value))
		{
			TableConnectionSettingsBase tableConnectionSettingsBase = NavGlobal.AppDatabase.TableConnectionSettingsStorage.Get(connectionType, NormalizeName(currentTableConnectionName));
			if (tableConnectionSettingsBase == null)
			{
				throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.TableConnectionNotRegistered, currentTableConnectionName, connectionType));
			}
			value = CreateTableConnection(connectionType, currentTableConnectionName, tableConnectionSettingsBase);
			tableConnectionForConnectionType.Add(currentTableConnectionName, value);
		}
		return value as T;
	}

	internal T FindTableConnection<T>(TableConnectionType connectionType, string name) where T : TableConnection
	{
		if (GetTableConnectionForConnectionType(connectionType).TryGetValue(NormalizeName(name), out var value))
		{
			return value as T;
		}
		return null;
	}

	private Dictionary<string, TableConnection> GetTableConnectionForConnectionType(TableConnectionType connectionType)
	{
		if (!tableConnectionStrings.TryGetValue(connectionType, out var value))
		{
			value = new Dictionary<string, TableConnection>();
			tableConnectionStrings.Add(connectionType, value);
		}
		return value;
	}

	private static string NormalizeName(string name)
	{
		return name.ToUpperInvariant();
	}
}
