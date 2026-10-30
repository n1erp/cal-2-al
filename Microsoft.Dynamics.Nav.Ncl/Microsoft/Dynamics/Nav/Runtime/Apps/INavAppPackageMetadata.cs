using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal interface INavAppPackageMetadata
{
	PackageId PackageId { get; }

	NavAppManifest Manifest { get; }

	NavAppMediaIdListing MediaIdListing { get; }

	IEnumerable<NavAppMedia> Media { get; }

	IList<NavAppObjectMetadata> ObjectMetadata { get; }

	NavAppCombinedProfileMetadata CombinedProfileMetadata { get; }

	byte[] Symbols { get; }

	string ContentHash { get; }

	NavAppPackageType PackageType { get; }
}
