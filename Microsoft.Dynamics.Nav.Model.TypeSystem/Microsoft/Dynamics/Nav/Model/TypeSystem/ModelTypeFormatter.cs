using System;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class ModelTypeFormatter : IModelTypeFormatter
{
	private static ModelTypeFormatter invariantFormatter = new ModelTypeFormatter();

	public static IModelTypeFormatter InvariantFormatter => invariantFormatter;

	protected ModelTypeFormatter()
	{
	}

	public string Format(IModelType modelType)
	{
		if (modelType == null)
		{
			throw new ArgumentNullException("modelType");
		}
		if (modelType is IGenericType genericType)
		{
			return GenericTypeFormatter(genericType);
		}
		if (modelType is IEnumType enumType)
		{
			return EnumTypeFormatter(enumType);
		}
		return modelType.DataType switch
		{
			DataTypes.Integer => IntegerTypeFormatter(modelType as IntegerType), 
			DataTypes.Boolean => BooleanTypeFormatter(modelType as BooleanType), 
			_ => throw new InvalidOperationException($"Unable to format type '{modelType.GetType().Name}' having data type '{modelType.DataType.ToString()}'. Datatype is not supported"), 
		};
	}

	protected virtual string GenericTypeFormatter(IGenericType genericType)
	{
		return genericType.ValueAsString();
	}

	protected virtual string EnumTypeFormatter(IEnumType enumType)
	{
		return enumType.ValueAsString();
	}

	protected virtual string IntegerTypeFormatter(IntegerType modelType)
	{
		if (!modelType.HasValue)
		{
			return string.Empty;
		}
		return modelType.Value.ToString();
	}

	protected virtual string BooleanTypeFormatter(BooleanType modelType)
	{
		if (!modelType.HasValue)
		{
			return string.Empty;
		}
		if (!modelType.Value)
		{
			return "No";
		}
		return "Yes";
	}
}
