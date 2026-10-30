using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class EvaluateRelationResponse : ServerResponse
{
	[DataMember]
	private int index;

	public int RelationIndex
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}
}
