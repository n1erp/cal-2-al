namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class NavAppTraceHelper
{
	public static string CreateActivityIdString(string activityId)
	{
		string text = (string.IsNullOrWhiteSpace(activityId) ? "NONE" : activityId);
		return "ExtensionActivityId:" + text;
	}
}
