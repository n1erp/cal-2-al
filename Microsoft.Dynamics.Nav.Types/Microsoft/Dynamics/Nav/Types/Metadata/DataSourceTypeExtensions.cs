using System.ComponentModel;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public static class DataSourceTypeExtensions
{
	public static ObjectType ToObjectType(this DataSourceType dataSourceType)
	{
		return dataSourceType switch
		{
			DataSourceType.Table => ObjectType.Table, 
			DataSourceType.Query => ObjectType.Query, 
			_ => throw new InvalidEnumArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.UnsupportDataSourceType, dataSourceType)), 
		};
	}
}
