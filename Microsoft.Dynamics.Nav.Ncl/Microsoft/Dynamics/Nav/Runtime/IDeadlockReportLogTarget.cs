namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IDeadlockReportLogTarget
{
	void LogReport(NavDatabase database, DeadlockXmlReport report, NavSqlTransactionHistory history);

	void WriteTransactionHistoryToFile(string fileName, NavSqlTransactionHistory history);
}
