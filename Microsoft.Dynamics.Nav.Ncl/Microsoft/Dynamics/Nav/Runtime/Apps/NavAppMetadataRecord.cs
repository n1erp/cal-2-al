using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppMetadataRecord
{
	public long SqlTimestamp { get; set; }

	public PackageId PackageId { get; set; }

	public AppId AppId { get; set; }

	public string Name { get; set; }

	public string Publisher { get; set; }

	public int VersionMajor { get; set; }

	public int VersionMinor { get; set; }

	public int VersionBuild { get; set; }

	public int VersionRevision { get; set; }

	public int CompatibilityMajor { get; set; }

	public int CompatibilityMinor { get; set; }

	public int CompatibilityBuild { get; set; }

	public int CompatibilityRevision { get; set; }

	public NavAppDependencyRecord[] Dependencies { get; set; }

	public NavAppObjectMetadataRecord[] ObjectMetadata { get; set; }

	public NavAppResourceRecord[] Resources { get; set; }

	public ReadOnlyRecordBuffer RecordBuffer { get; set; }

	public NavAppPackageType PackageType { get; set; }

	public string ContentHash { get; set; }

	public bool ShowMyCode { get; set; }

	public NavAppScope Scope { get; set; }

	public string TenantId { get; set; }

	public Sha256Hash PackageHash { get; set; }
}
