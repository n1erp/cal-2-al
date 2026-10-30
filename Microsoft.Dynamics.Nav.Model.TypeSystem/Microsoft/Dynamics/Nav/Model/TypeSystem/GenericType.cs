namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

public class GenericType : IGenericType, IModelType
{
	private readonly DataTypes dataType;

	private readonly string value;

	public DataTypes DataType => dataType;

	public bool HasValue => value != null;

	public CompiledState CompiledState => CompiledState.Uncompiled;

	public string Value => value;

	private GenericType(DataTypes dataType, string value)
	{
		this.dataType = dataType;
		this.value = value;
	}

	public static GenericType Create(DataTypes dataType, string value)
	{
		return new GenericType(dataType, value);
	}

	public string ValueAsString()
	{
		return value;
	}
}
