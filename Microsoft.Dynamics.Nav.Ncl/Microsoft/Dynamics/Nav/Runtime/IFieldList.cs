namespace Microsoft.Dynamics.Nav.Runtime;

public interface IFieldList
{
	ulong UInt64Hash { get; }

	bool HasFieldWithColumnIndex(int columnIndex);
}
