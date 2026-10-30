using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal interface IAppOrigin
{
	NavAppOrigin AppOrigin { get; }

	Guid? SourceAppId { get; }

	NavExtensionType SourceExtensionType { get; }
}
