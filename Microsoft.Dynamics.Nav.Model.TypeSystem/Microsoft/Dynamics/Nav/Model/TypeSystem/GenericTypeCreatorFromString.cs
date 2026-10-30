namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class GenericTypeCreatorFromString : ModelTypeCreator, IModelTypeCreatorFromString
{
	internal GenericTypeCreatorFromString()
	{
	}

	public virtual IModelType Create(DataTypes datatype, string value)
	{
		return GenericType.Create(datatype, value);
	}
}
