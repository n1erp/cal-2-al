using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class TableConnectionManagement
{
	public static IList<TableConnectionSettingsBase> GetTableConnection(TableConnectionType tableType, string connectionId)
	{
		IEnumerable<TableConnectionSettingsBase> source;
		if (string.IsNullOrEmpty(connectionId))
		{
			source = NavGlobal.AppDatabase.TableConnectionSettingsStorage.GetAll();
		}
		else
		{
			TableConnectionSettingsBase tableConnectionSettingsBase = NavGlobal.AppDatabase.TableConnectionSettingsStorage.Get(tableType, connectionId.ToUpperInvariant());
			source = ((tableConnectionSettingsBase == null) ? new TableConnectionSettingsBase[0] : new TableConnectionSettingsBase[1] { tableConnectionSettingsBase });
		}
		return source.ToList();
	}

	public static void AddTableConnection(TableConnectionSettingsBase connectionSettings)
	{
		NavGlobal.AppDatabase.TableConnectionSettingsStorage.Add(connectionSettings);
	}

	public static void RemoveTableConnection(TableConnectionType tableType, string connectionId)
	{
		NavGlobal.AppDatabase.TableConnectionSettingsStorage.Remove(tableType, connectionId);
	}
}
