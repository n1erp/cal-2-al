namespace Microsoft.Dynamics.Nav.Runtime;

public class DatabaseLockDiagnosticEntry
{
	public string TableName { get; set; }

	public string ResourceType { get; set; }

	public string RequestMode { get; set; }

	public string RequestStatus { get; set; }

	internal long SqlTransactionId { get; set; }

	public int RequesterSqlServerSessionId { get; set; }

	public ALObjectScopeDiagnosticEntry RequesterALObjectScope { get; set; }

	public int RequesterUserSessionId { get; set; }

	internal string UserName { get; set; }
}
