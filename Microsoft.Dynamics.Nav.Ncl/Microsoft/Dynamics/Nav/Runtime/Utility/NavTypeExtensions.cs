using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Utility;

public static class NavTypeExtensions
{
	public static TableSynchSetupForDataUpgradeResponse ToServerResponseType(this NavSqlTableAndChange input)
	{
		TableSynchSetupForDataUpgradeResponse tableSynchSetupForDataUpgradeResponse = new TableSynchSetupForDataUpgradeResponse();
		tableSynchSetupForDataUpgradeResponse.NewTableId = input.NewTable?.TableId ?? 0;
		tableSynchSetupForDataUpgradeResponse.OldTableId = input.OldTable?.TableId ?? 0;
		tableSynchSetupForDataUpgradeResponse.BaseHash = input.BaseHash?.Value;
		tableSynchSetupForDataUpgradeResponse.NewHash = input.NewHash?.Value;
		tableSynchSetupForDataUpgradeResponse.ChangesAffectSqlSchema = input.ChangesAffectSqlSchema;
		switch (input.ChangeType)
		{
		case TableChangeType.Changed:
			tableSynchSetupForDataUpgradeResponse.ChangeType = DataUpgradeTableChangeType.Changed;
			break;
		case TableChangeType.Excessive:
			tableSynchSetupForDataUpgradeResponse.ChangeType = DataUpgradeTableChangeType.Excessive;
			break;
		case TableChangeType.Missing:
			tableSynchSetupForDataUpgradeResponse.ChangeType = DataUpgradeTableChangeType.Missing;
			break;
		default:
			tableSynchSetupForDataUpgradeResponse.ChangeType = DataUpgradeTableChangeType.None;
			break;
		}
		tableSynchSetupForDataUpgradeResponse.FieldsAndChanges = new List<FieldChange>();
		if (input.FieldsAndChanges != null)
		{
			tableSynchSetupForDataUpgradeResponse.FieldsAndChanges = (from c in input.FieldsAndChanges
				where c.OldField != null
				select new FieldChange(c.OldField.ColumnIndex, c.FieldChangeTypeDescription, !c.Changes.Equals(FieldChangeTypes.None))).ToList();
		}
		tableSynchSetupForDataUpgradeResponse.IndexesAndChanges = new List<IndexChange>();
		if (input.IndexesAndChanges != null)
		{
			tableSynchSetupForDataUpgradeResponse.IndexesAndChanges = (from c in input.IndexesAndChanges
				where c.OldSqlIndex != null
				select new IndexChange(c.OldSqlIndex.SqlIndexNo, c.IndexChangeTypeDescription, !c.Changes.Equals(IndexChangeTypes.None))).ToList();
		}
		return tableSynchSetupForDataUpgradeResponse;
	}
}
