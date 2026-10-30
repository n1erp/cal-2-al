namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemErrorHandling
{
	public static string ALGetLastErrorText
	{
		get
		{
			NavSession session = NavCurrentThread.Session;
			if (!session.TestExecution.InTest)
			{
				return session.GetLastErrorText;
			}
			return NavDialog.ReplaceCRLFWithBackSlash(session.GetLastErrorText);
		}
	}

	public static string ALGetLastErrorCode => NavCurrentThread.Session.GetLastErrorCode;

	public static string ALGetLastErrorCallStack => NavCurrentThread.Session.GetLastErrorCallstack();

	public static NavDotNet ALGetLastErrorObject(ITreeObject parent)
	{
		return NavCurrentThread.Session.GetLastErrorObject(parent);
	}

	public static void ALClearLastError()
	{
		NavCurrentThread.Session.ClearLastError();
	}
}
