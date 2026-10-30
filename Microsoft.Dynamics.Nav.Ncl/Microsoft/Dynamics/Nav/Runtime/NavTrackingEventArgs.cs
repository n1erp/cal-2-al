using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavTrackingEventArgs : EventArgs
{
	private readonly IReadOnlyList<NavTrackingObject> trackingObjects;

	public IReadOnlyList<NavTrackingObject> TrackingObjects => trackingObjects;

	public NavTrackingEventArgs(IReadOnlyList<NavTrackingObject> trackingObjects)
	{
		this.trackingObjects = trackingObjects;
	}
}
