namespace Microsoft.Dynamics.Nav.Types;

public class SystemPages
{
	public const int RunQueryPage = 9597;

	public const int RunFilterPage = 9598;

	public const int RunTablePage = 9599;

	public static bool IsServerSystemPageId(int page)
	{
		if (page != 9597 && page != 9598)
		{
			return page == 9599;
		}
		return true;
	}
}
