namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public interface IModelTypeCreatorFromString
{
	IModelType Create(DataTypes dataType, string value);
}
