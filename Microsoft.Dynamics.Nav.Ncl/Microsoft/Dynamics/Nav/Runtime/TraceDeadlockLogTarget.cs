using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TraceDeadlockLogTarget : IDeadlockReportLogTarget
{
	public void LogReport(NavDatabase database, DeadlockXmlReport report, NavSqlTransactionHistory history)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Processes involved in the deadlock:");
		foreach (DeadlockXmlReport.Process process in report.ProcessList)
		{
			stringBuilder.AppendLine("Process id: " + process.Id);
			stringBuilder.AppendLine("Is victim: " + report.VictimList.Contains(process.Id));
			NavSqlTransactionHistory.HistoryEntry? historyEntry = history.Find(delegate(ref NavSqlTransactionHistory.HistoryEntry e)
			{
				return e.TransactionId == process.OwnerId;
			});
			if (historyEntry.HasValue)
			{
				stringBuilder.AppendLine("User: " + historyEntry.Value.UserName);
				stringBuilder.AppendLine("SessionId: " + historyEntry.Value.SessionId);
				stringBuilder.AppendLine("SQL TransactionId: " + historyEntry.Value.TransactionId);
				stringBuilder.AppendLine("AL ObjectType: " + historyEntry.Value.ObjectType);
				stringBuilder.AppendLine("AL ObjectNumber: " + historyEntry.Value.ObjectNumber);
				stringBuilder.AppendLine("AL ScopeName: " + historyEntry.Value.ScopeName);
			}
			else
			{
				stringBuilder.AppendLine("Could not get additional details for this particular process, possibly because it was executed on a different Dynamics 365 Business Central Server instance.");
			}
			stringBuilder.AppendLine();
		}
		stringBuilder.AppendLine("SQL Server deadlock xml report received from the SQL server extended event (for more information, see https://aka.ms/vp7f5k):");
		stringBuilder.Append(report);
		database.Tenant.Diagnostics.SendTraceTag("00000DI", Category.Sql, Verbosity.Warning, PrivacyClassification.EndUserIdentifiableInformation, stringBuilder.ToString());
	}

	public void WriteTransactionHistoryToFile(string fileName, NavSqlTransactionHistory history)
	{
		using FileStream stream = File.Create(fileName);
		using StreamWriter textWriter = new StreamWriter(stream, Encoding.UTF8);
		WriteTransactionHistoryToStream(textWriter, history);
	}

	public static void WriteTransactionHistoryToStream(TextWriter textWriter, NavSqlTransactionHistory history)
	{
		textWriter.WriteLine(string.Join(";", "SessionId", "TransactionId", "UserName", "ObjectType", "ObjectNumber", "ScopeName"));
		history.Foreach((NavSqlTransactionHistory.HistoryEntry entry) =>
		{
			if (entry.TransactionId != 0L)
			{
				textWriter.WriteLine(string.Join(";", entry.SessionId, entry.TransactionId, entry.UserName, entry.ObjectType, entry.ObjectNumber, entry.ScopeName));
			}
		});
	}
}
