using System;

namespace Microsoft.Dynamics.Nav.Types.Cache;

internal interface IExpirableItem
{
	DateTime ScavengeTime { get; }
}
