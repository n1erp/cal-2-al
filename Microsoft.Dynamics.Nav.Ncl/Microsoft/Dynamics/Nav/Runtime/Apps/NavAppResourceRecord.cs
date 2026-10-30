using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppResourceRecord
{
	public long SqlTimestamp { get; set; }

	public PackageId PackageId { get; set; }

	public NavAppResourceType Type { get; set; }

	public string Name { get; set; }

	public Sha256Hash Hash { get; set; }

	public ReadOnlyRecordBuffer RecordBuffer { get; set; }
}
