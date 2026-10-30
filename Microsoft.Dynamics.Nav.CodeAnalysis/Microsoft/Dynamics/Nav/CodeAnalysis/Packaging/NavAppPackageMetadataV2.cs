using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class NavAppPackageMetadataV2 : NavAppPackageMetadataV1
{
	public override uint MetadataVersion => 2u;

	public new static NavAppPackageMetadataV2 Create()
	{
		return new NavAppPackageMetadataV2
		{
			ContentLength = 0L,
			PackageId = Guid.Empty
		};
	}
}
