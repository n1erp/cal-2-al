using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class NavAppPackageMetadataV1 : NavAppPackageMetadata
{
	private const uint SizeofMetadata = 40u;

	public override uint MetadataSize => 40u;

	public override uint MetadataVersion => 1u;

	protected NavAppPackageMetadataV1()
	{
	}

	public static NavAppPackageMetadataV1 Create()
	{
		return new NavAppPackageMetadataV1
		{
			ContentLength = 0L,
			PackageId = Guid.Empty
		};
	}
}
