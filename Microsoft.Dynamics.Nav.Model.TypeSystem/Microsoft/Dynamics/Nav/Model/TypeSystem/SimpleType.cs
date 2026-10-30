using System;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public abstract class SimpleType
{
	public abstract DataTypes DataType { get; }

	public virtual bool IsValidated => true;

	public abstract bool HasValue { get; }

	public abstract string ValueAsString { get; }

	public static IModelType Create(DataTypes dataType, string value)
	{
		Func<string, IModelType> typeCreator = ModelTypeInfo.GetTypeCreator(dataType);
		if (typeCreator == null)
		{
			return GenericType.Create(dataType, value);
		}
		return typeCreator(value);
	}

	public virtual bool Validate(IModelReferenceResolver resolver)
	{
		return true;
	}

	internal void AssertValidation()
	{
		if (!IsValidated)
		{
			throw new InvalidOperationException("Type has not been validated");
		}
	}
}
