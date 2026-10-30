using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

[DataType(DataTypes.MLText)]
public class MLTextType : IModelType
{
	private KeyValuePair<CultureInfo, string>[] value;

	private static MLTextType empty = new MLTextType(null);

	public DataTypes DataType => DataTypes.MLText;

	public bool HasValue
	{
		get
		{
			if (value != null)
			{
				return value.Length != 0;
			}
			return false;
		}
	}

	public IEnumerable<KeyValuePair<CultureInfo, string>> Value => value;

	public CompiledState CompiledState => CompiledState.Compiled;

	public static MLTextType Empty => empty;

	private MLTextType(string value)
	{
		this.value = ModelTextHelper.MLTextSplitter(value).ToArray();
	}

	public override string ToString()
	{
		return ModelTextHelper.MLTextFormatter(value, int.MaxValue).Single();
	}

	public static MLTextType Create(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return Empty;
		}
		return new MLTextType(value);
	}
}
