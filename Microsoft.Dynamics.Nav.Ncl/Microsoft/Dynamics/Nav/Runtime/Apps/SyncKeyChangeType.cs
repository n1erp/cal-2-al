namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal enum SyncKeyChangeType
{
	AddSqlIndex,
	AddSiftIndex,
	RemoveSqlIndex,
	RemoveSiftIndex,
	IndexFields,
	SumFields,
	PrimaryKey,
	Clustered,
	Unique
}
