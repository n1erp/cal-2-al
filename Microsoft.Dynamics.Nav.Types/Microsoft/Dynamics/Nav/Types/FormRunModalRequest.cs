using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FormRunModalRequest : ServerResponse
{
	private FormResult result;

	private FormRunData formRunData = new FormRunData();

	[DataMember]
	public FormResult Result
	{
		get
		{
			return result;
		}
		set
		{
			result = value;
		}
	}

	[DataMember]
	public FormRunData Data
	{
		get
		{
			return formRunData;
		}
		set
		{
			formRunData = value;
		}
	}
}
