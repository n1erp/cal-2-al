using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class ObsolescenceGuard
{
	private ObsolescenceGuard()
	{
	}

	public static void ThrowIfObsoleted(NCLMetaKey key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (ObsolescenceIsEffective(key.ObsoleteState))
		{
			throw new NavMetadataObsoleteException(string.Format(CultureInfo.InvariantCulture, "Key {0} ({1}) for table {2} ({3}) is obsoleted, reason: {4}", key.GetKeyName(), key.KeyIndex, key.Parent.TableName, key.Parent.TableId, key.ObsoleteReason));
		}
	}

	public static void ThrowIfObsoleted(NCLMetaTable table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		if (ObsolescenceIsEffective(table.ObsoleteState))
		{
			throw new NavMetadataObsoleteException(string.Format(CultureInfo.InvariantCulture, "Table {0} ({1}) is obsoleted, reason: {2}", table.TableName, table.TableId, table.ObsoleteReason));
		}
	}

	public static void ThrowIfObsoleted(NCLMetaField field)
	{
		if (field == null)
		{
			throw new ArgumentNullException("field");
		}
		if (ObsolescenceIsEffective(field.ObsoleteState))
		{
			throw new NavMetadataObsoleteException(string.Format(CultureInfo.InvariantCulture, "Field {0} ({1}) of table {2} ({3}) is obsoleted, reason: {4}", field.FieldName, field.FieldNo, field.Parent.TableName, field.Parent.TableId, field.ObsoleteReason));
		}
	}

	public static bool ObsolescenceIsEffective(ObsoleteState state)
	{
		if (state == ObsoleteState.Removed && NavCurrentThread.Session != null && NavCurrentThread.Session.RemovedMetadataAccessContext == null && NavCurrentThread.Session.RunTableContext == null && NavCurrentThread.Session.AppUpgradeContext == null)
		{
			if (NavCurrentThread.Session.AppInstallationContext != null)
			{
				return NavCurrentThread.Session.AppInstallationContext.HasData;
			}
			return true;
		}
		return false;
	}
}
