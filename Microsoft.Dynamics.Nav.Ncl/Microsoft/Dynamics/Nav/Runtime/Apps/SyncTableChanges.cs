using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncTableChanges
{
	public SyncTableChangeType ChangeType { get; }

	public IEnumerable<SyncFieldChange> FieldChanges { get; }

	public IEnumerable<SyncKeyChange> KeyChanges { get; }

	public SyncTableChanges(SyncTableChangeType changeType, IEnumerable<SyncFieldChange> fieldChanges, IEnumerable<SyncKeyChange> keyChanges)
	{
		ChangeType = changeType;
		FieldChanges = fieldChanges;
		KeyChanges = keyChanges;
	}
}
