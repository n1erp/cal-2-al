namespace Microsoft.Dynamics.Nav.Runtime;

internal class UnkownFieldMetadata : NavFieldMetadata
{
	private readonly int fieldNo;

	public int FieldNo => fieldNo;

	public UnkownFieldMetadata(NavNclType nclType, int fieldNo)
		: base(NavValueMetadata.DefaultMetadata(nclType), SqlDataType.Default)
	{
		this.fieldNo = fieldNo;
	}
}
