using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class InconsistentRecords
{
	private Dictionary<NavRecord, NCLMetaTable> inconsistentRecords;

	public void SetRecordConsistent(NavRecord record)
	{
		if (inconsistentRecords != null)
		{
			inconsistentRecords.Remove(record);
		}
	}

	public void SetRecordInconsistent(NavRecord record)
	{
		if (inconsistentRecords == null)
		{
			inconsistentRecords = new Dictionary<NavRecord, NCLMetaTable>();
		}
		inconsistentRecords[record] = record.MetaTable;
	}

	public void VerifyAllRecordsConsistent(bool commit)
	{
		if (inconsistentRecords == null)
		{
			return;
		}
		try
		{
			if (commit && inconsistentRecords.Count > 0)
			{
				NCLMetaTable nCLMetaTable = inconsistentRecords.Values.First();
				throw new NavCSideException(18023508, string.Format(CultureInfo.CurrentCulture, Lang.TransactionCannotCommitInconsistentTable, nCLMetaTable.TableCaptions.GetValueOrDefault()));
			}
		}
		finally
		{
			inconsistentRecords.Clear();
		}
	}
}
