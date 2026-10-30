using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
[JsonObject]
public class ApplicationObjectInfo
{
	[DataMember]
	private string name;

	[DataMember]
	private ApplicationObjectId objectId;

	[JsonProperty]
	public string Name
	{
		[DebuggerStepThrough]
		get
		{
			return name;
		}
	}

	[JsonProperty]
	public ApplicationObjectId ObjectId
	{
		[DebuggerStepThrough]
		get
		{
			return objectId;
		}
	}

	[JsonConstructor]
	public ApplicationObjectInfo(ApplicationObjectId objectId, string name)
	{
		this.objectId = objectId;
		this.name = name;
	}
}
