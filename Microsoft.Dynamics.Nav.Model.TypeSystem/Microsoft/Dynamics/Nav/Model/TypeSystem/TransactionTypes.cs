namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

[DataType(DataTypes.TransactionType)]
public enum TransactionTypes
{
	UpdateNoLocks,
	Update,
	Snapshot,
	Browse,
	Report
}
