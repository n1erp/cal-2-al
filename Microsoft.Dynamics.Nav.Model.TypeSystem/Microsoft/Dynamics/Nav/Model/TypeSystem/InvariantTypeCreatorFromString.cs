namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class InvariantTypeCreatorFromString : GenericTypeCreatorFromString, IModelTypeCreatorFromString
{
	internal InvariantTypeCreatorFromString()
	{
	}

	public override IModelType Create(DataTypes dataType, string value)
	{
		EnumInfo enumInfo = ModelTypeInfo.GetEnumInfo(dataType);
		if (enumInfo != null)
		{
			return enumInfo.CreateEnumTypeFromString(value);
		}
		if (dataType == DataTypes.Integer)
		{
			return IntegerType.Create(value);
		}
		if (ModelTypeInfo.DataTypeIsInteger == null)
		{
			ModelTypeInfo.DataTypeIsInteger = new int[100];
		}
		if (ModelTypeInfo.DataTypeIsInteger[(int)dataType] < 2)
		{
			if (!int.TryParse(value, out var _))
			{
				ModelTypeInfo.DataTypeIsInteger[(int)dataType] = 2;
			}
			else
			{
				ModelTypeInfo.DataTypeIsInteger[(int)dataType] = 1;
			}
		}
		return GenericType.Create(dataType, value);
	}
}
