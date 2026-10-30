namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

[DataType(DataTypes.ExtendedDataType)]
public enum ExtendedDatatypes
{
	None,
	[Name("Phone No.")]
	PhoneNo,
	URL,
	[Name("E-Mail")]
	Email,
	Ratio,
	Masked,
	Person,
	Resource
}
