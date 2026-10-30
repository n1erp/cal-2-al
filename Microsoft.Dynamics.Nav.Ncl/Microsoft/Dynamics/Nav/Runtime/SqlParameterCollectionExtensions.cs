using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class SqlParameterCollectionExtensions
{
	public static void Add(this SqlParameterCollection paramCollection, int parameter, SqlDbType type, object value)
	{
		string parameterName = string.Format(CultureInfo.InvariantCulture, "@{0}", parameter.ToString(CultureInfo.InvariantCulture));
		paramCollection.Add(parameterName, type);
		paramCollection[parameterName].Value = value;
	}

	public static void Add(this SqlParameterCollection paramCollection, int parameter, SqlDbType type, int size, object value)
	{
		string parameterName = string.Format(CultureInfo.InvariantCulture, "@{0}", parameter.ToString(CultureInfo.InvariantCulture));
		paramCollection.Add(parameterName, type, size);
		paramCollection[parameterName].Value = value;
	}
}
