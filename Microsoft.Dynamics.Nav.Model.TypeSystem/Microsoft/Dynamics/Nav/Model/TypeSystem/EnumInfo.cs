using System;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class EnumInfo
{
	public readonly Type TypeOfEnum;

	public readonly Type TypeOfEnumType;

	public readonly DataTypes DataType;

	public readonly Func<string, IEnumType> CreateEnumTypeFromString;

	internal EnumInfo(Type typeOfEnum, Type typeOfEnumType, DataTypes datatype, Func<string, IEnumType> createEnumTypeFromString)
	{
		TypeOfEnum = typeOfEnum;
		TypeOfEnumType = typeOfEnumType;
		DataType = datatype;
		CreateEnumTypeFromString = createEnumTypeFromString;
	}
}
