namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

[DataType(DataTypes.BlobType)]
public enum BlobTypes
{
	Unknown,
	Bitmap,
	Memo,
	[Name("User-Defined")]
	UserDefined,
	Json
}
