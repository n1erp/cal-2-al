namespace Microsoft.Dynamics.Nav.Runtime;

internal interface IRecordBuffer
{
	int FieldCount { get; }

	NavValue this[int index] { get; }

	NCLMetaApplicationObject MetaApplicationObject { get; }

	NavValue GetChangedFieldValue(int index);

	NavValue GetOriginalValue(int index);

	NavRecordId GetRecordId();

	NavValue[] ToArray();
}
