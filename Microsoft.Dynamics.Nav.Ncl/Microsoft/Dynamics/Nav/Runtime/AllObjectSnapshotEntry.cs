using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AllObjectSnapshotEntry
{
	public string Name { get; private set; }

	public Guid? AppPackageId { get; private set; }

	public AllObjectSnapshotEntry(string name, Guid? appPackageId)
	{
		Name = name;
		AppPackageId = appPackageId;
	}
}
