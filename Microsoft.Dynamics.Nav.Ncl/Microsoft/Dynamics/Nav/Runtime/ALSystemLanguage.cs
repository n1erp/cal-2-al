namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemLanguage
{
	public static int ALGlobalLanguage
	{
		get
		{
			return NavCurrentThread.Session.GlobalLanguage;
		}
		set
		{
			NavCurrentThread.Session.GlobalLanguage = value;
		}
	}

	public static int ALWindowsLanguage => NavCurrentThread.Session.WindowsLanguage;
}
