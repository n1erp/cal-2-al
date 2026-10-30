using System.Collections.Generic;

namespace Newtonsoft.Json.Linq.JsonPath;

internal class ScanMultipleFilter : PathFilter
{
	public List<string> Names { get; set; }

	public override IEnumerable<JToken> ExecuteFilter(JToken root, IEnumerable<JToken> current, bool errorWhenNoMatch)
	{
		foreach (JToken c in current)
		{
			JToken value = c;
			JToken jToken = c;
			while (true)
			{
				if (jToken != null && jToken.HasValues)
				{
					value = jToken.First;
				}
				else
				{
					while (value != null && value != c && value == value.Parent.Last)
					{
						value = value.Parent;
					}
					if (value == null || value == c)
					{
						break;
					}
					value = value.Next;
				}
				if (value is JProperty e)
				{
					foreach (string name in Names)
					{
						if (e.Name == name)
						{
							yield return e.Value;
						}
					}
				}
				jToken = value as JContainer;
			}
		}
	}
}
