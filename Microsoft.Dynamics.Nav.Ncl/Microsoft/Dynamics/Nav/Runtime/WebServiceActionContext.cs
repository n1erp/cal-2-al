using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class WebServiceActionContext
{
	public enum StatusCode
	{
		None = 0,
		Get = 200,
		Created = 201,
		Updated = Get,
		Deleted = 204
	}

	private Dictionary<int, object> entityKeys;

	public Dictionary<int, object> EntityKeys => entityKeys ?? (entityKeys = new Dictionary<int, object>());

	public int NavObjectId { get; set; }

	public NavObjectType NavObjectType { get; private set; }

	public StatusCode ResultCode { get; set; }

	public void AddEntityKeys(Dictionary<int, object> keys)
	{
		foreach (KeyValuePair<int, object> key in keys)
		{
			EntityKeys.Add(key.Key, key.Value);
		}
	}

	public void SetNavObjectType(object value)
	{
		NavObjectType = (NavObjectType)value;
	}
}
