using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NotificationInfoAction
{
	[DataMember]
	public int Index { get; set; }

	[DataMember]
	public string Caption { get; set; }

	[DataMember]
	public int CodeunitId { get; set; }

	[DataMember]
	public string FunctionName { get; set; }

	public NotificationInfoAction()
		: this(0, string.Empty, 0, string.Empty)
	{
	}

	public NotificationInfoAction(int index, string caption, int codeunitId, string functionName)
	{
		Index = index;
		Caption = caption;
		CodeunitId = codeunitId;
		FunctionName = functionName;
	}

	public NotificationInfoAction Clone()
	{
		return new NotificationInfoAction(Index, Caption, CodeunitId, FunctionName);
	}
}
