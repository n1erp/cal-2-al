using System;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class DataTypeAttribute : Attribute
{
	public readonly DataTypes DataType;

	public DataTypeAttribute(DataTypes dataType)
	{
		DataType = dataType;
	}
}
