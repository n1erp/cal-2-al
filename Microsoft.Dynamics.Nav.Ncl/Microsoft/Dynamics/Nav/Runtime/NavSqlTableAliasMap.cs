using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlTableAliasMap
{
	private readonly ConcurrentDictionary<NavAppOrigin, int> companionAliasIdsByApp = new ConcurrentDictionary<NavAppOrigin, int>();

	private int nextCompanionAliasId;

	public string GetDefaultMainTableAlias(int tableId)
	{
		return tableId.ToString(CultureInfo.InvariantCulture);
	}

	public string GetCompanionTableAlias(NavAppOrigin companionAppOrigin, string mainAlias)
	{
		int orAdd = companionAliasIdsByApp.GetOrAdd(companionAppOrigin, (NavAppOrigin key) => Interlocked.Increment(ref nextCompanionAliasId));
		string identifier = string.Format(CultureInfo.InvariantCulture, "{0}_e{1}", mainAlias, orAdd);
		return NavSqlStatementHelper.ConvertToSqlIdentifier(identifier);
	}

	public string ResolveFinalAlias(NCLMetaField field, string mainAlias)
	{
		string identifier = (field.IsCompanionTableField ? GetCompanionTableAlias(field.AppOrigin, mainAlias) : mainAlias);
		return NavSqlStatementHelper.ConvertToSqlIdentifier(identifier);
	}
}
