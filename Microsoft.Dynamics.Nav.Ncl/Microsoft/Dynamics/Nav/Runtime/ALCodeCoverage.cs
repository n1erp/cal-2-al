namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALCodeCoverage
{
	public static bool ALCodeCoverageLog()
	{
		return CodeCoverageManager.IsCodeCoverageEnabledForConnection(NavCurrentThread.Session);
	}

	public static bool ALCodeCoverageLog(bool newIsActive)
	{
		return ALCodeCoverageLog(newIsActive, multisession: false);
	}

	public static bool ALCodeCoverageLog(bool newIsActive, bool multisession)
	{
		NavSession session = NavCurrentThread.Session;
		ALCodeEnvironment codeEnvironment = session.NCLMetadata.CodeEnvironment;
		if (newIsActive)
		{
			if (multisession)
			{
				CodeCoverageManager.StartMultiSessionCodeCoverage(session, codeEnvironment);
			}
			else
			{
				CodeCoverageManager.StartSingleSessionCodeCoverage(session, codeEnvironment);
			}
		}
		else
		{
			CodeCoverageManager.StopCodeCoverageRecording(session);
		}
		return newIsActive;
	}

	public static void ALCodeCoverageInclude(NavRecord objectTableRecord)
	{
		CodeCoverageManager.EnsureApplicationObjectsAreIncludedInCoverageTable(objectTableRecord);
	}

	public static void ALCodeCoverageRefreshTable()
	{
		CodeCoverageManager.RefreshTable(NavCurrentThread.Session);
	}

	public static void ALCodeCoverageLoadFromTable()
	{
		CodeCoverageManager.LoadTableDataIntoCounters(NavCurrentThread.Session);
	}
}
