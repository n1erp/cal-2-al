namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public interface IModelType
{
	DataTypes DataType { get; }

	CompiledState CompiledState { get; }

	bool HasValue { get; }
}
