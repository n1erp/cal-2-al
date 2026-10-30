namespace Microsoft.Dynamics.Nav.Diagnostic;

public enum MonitorType
{
	None = 0,
	SqlReadCount = 65536,
	SqlWriteCount = 131073,
	ALStatementsExecuted = 262146,
	ExceptionCount = 524291,
	CrmServiceCall = 1048580
}
