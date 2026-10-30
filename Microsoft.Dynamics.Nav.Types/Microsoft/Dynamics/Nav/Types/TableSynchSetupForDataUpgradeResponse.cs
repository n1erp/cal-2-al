using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

public class TableSynchSetupForDataUpgradeResponse : ServerResponse
{
	[DataMember]
	public int OldTableId { get; set; }

	[DataMember]
	public int NewTableId { get; set; }

	[DataMember]
	public DataUpgradeTableChangeType ChangeType { get; set; }

	[DataMember]
	public string BaseHash { get; set; }

	[DataMember]
	public string NewHash { get; set; }

	[DataMember]
	public bool ChangesAffectSqlSchema { get; set; }

	[DataMember]
	public IEnumerable<FieldChange> FieldsAndChanges { get; set; }

	[DataMember]
	public IEnumerable<IndexChange> IndexesAndChanges { get; set; }
}
