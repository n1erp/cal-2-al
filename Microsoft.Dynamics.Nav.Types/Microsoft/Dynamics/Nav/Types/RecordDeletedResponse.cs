using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class RecordDeletedResponse : ServerResponse
{
	private byte[][] deletedRecords;

	private NavRecordState state;

	[DataMember]
	public byte[][] DeletedRecords
	{
		get
		{
			return deletedRecords;
		}
		set
		{
			deletedRecords = value;
		}
	}

	[DataMember]
	public NavRecordState State
	{
		get
		{
			return state;
		}
		set
		{
			state = value;
		}
	}

	public RecordDeletedResponse(byte[][] deletedRecords, NavRecordState state)
	{
		this.deletedRecords = deletedRecords;
		this.state = state;
	}
}
