using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract]
[JsonObject]
public class LocalNodeContext
{
	[DataMember]
	private readonly int index;

	[DataMember]
	private readonly string path;

	[DataMember]
	private readonly LocalNode node;

	[JsonProperty]
	public int Index => index;

	[JsonProperty]
	public string Path => path;

	[JsonProperty]
	public LocalNode Node => node;

	[JsonConstructor]
	public LocalNodeContext(int index, string path, LocalNode node)
	{
		this.index = index;
		this.path = path;
		this.node = node;
	}
}
