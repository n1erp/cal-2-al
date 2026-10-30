using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

public class TableCaptions
{
	[DataMember]
	public ResolvedCaption[] Captions { get; set; }
}
