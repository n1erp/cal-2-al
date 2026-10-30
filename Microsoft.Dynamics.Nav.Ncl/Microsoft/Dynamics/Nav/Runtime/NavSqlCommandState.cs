namespace Microsoft.Dynamics.Nav.Runtime;

public enum NavSqlCommandState
{
	Created,
	CreatedWithCommandText,
	Prepared,
	Executed,
	ExecutedWithPendingResults
}
