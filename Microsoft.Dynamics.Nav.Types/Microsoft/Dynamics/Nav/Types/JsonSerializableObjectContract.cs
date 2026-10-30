using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class JsonSerializableObjectContract
{
	[DataMember]
	private string jsonData;

	public string JsonData => jsonData;

	private JsonSerializableObjectContract()
	{
	}

	public JsonSerializableObjectContract(object value)
	{
		if (value != null)
		{
			jsonData = JsonConvert.SerializeObject(value);
		}
	}

	public static JsonSerializableObjectContract FromJson(string jsonData)
	{
		return new JsonSerializableObjectContract
		{
			jsonData = jsonData
		};
	}

	public object Deserialize(Type type)
	{
		JsonSerializerSettings settings = new JsonSerializerSettings
		{
			DateParseHandling = DateParseHandling.None
		};
		return JsonConvert.DeserializeObject(jsonData, type, settings);
	}
}
