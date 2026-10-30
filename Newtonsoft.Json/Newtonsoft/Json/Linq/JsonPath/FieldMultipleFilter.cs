using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class FieldMultipleFilter : PathFilter
{
	public List<string> Names { get; set; }

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
	{
		foreach (JToken t in current)
		{
			if (t is JObject o)
			{
				foreach (string name in Names)
				{
					JToken jToken = o[name];
					if (jToken != null)
					{
						yield return jToken;
					}
					if (errorWhenNoMatch)
					{
						throw new JsonException("Property '{0}' does not exist on JObject.".FormatWith(CultureInfo.InvariantCulture, name));
					}
				}
			}
			else if (errorWhenNoMatch)
			{
				throw new JsonException("Properties {0} not valid on {1}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", Names.Select((string n) => "'" + n + "'")), t.GetType().Name));
			}
		}
	}
}
