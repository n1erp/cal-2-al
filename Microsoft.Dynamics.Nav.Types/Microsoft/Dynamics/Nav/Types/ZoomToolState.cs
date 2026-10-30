namespace Microsoft.Dynamics.Nav.Types;

public static class ZoomToolState
{
	private static bool useZoomToolForReports;

	public static bool UseZoomToolForReports
	{
		get
		{
			return useZoomToolForReports;
		}
		set
		{
			useZoomToolForReports = value;
		}
	}
}
