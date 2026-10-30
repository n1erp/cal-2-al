using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ConvertUserPersonalizationResponse : ServerResponse
{
	[DataMember]
	private readonly ConvertUserPersonalizationResult result;

	[DataMember]
	private readonly string error;

	public ConvertUserPersonalizationResult Result => result;

	public string Error => error;

	public ConvertUserPersonalizationResponse(ConvertUserPersonalizationResult result)
		: this(result, string.Empty)
	{
	}

	public ConvertUserPersonalizationResponse(ConvertUserPersonalizationResult result, string error)
	{
		this.result = result;
		this.error = error;
	}
}
