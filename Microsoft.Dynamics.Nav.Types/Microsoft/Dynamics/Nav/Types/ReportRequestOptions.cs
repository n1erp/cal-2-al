using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ReportRequestOptions
{
	[DataMember]
	public bool ParametersOnly { get; set; }

	[DataMember]
	public CustomLayout ReportLayoutFormat { get; set; }

	[DataMember]
	public bool ShowScheduleButton { get; set; }

	public ReportRequestOptions Copy()
	{
		return new ReportRequestOptions
		{
			ParametersOnly = ParametersOnly,
			ReportLayoutFormat = ReportLayoutFormat,
			ShowScheduleButton = ShowScheduleButton
		};
	}

	public void CopyTo(ReportRequestOptions reportRequestOptions)
	{
		reportRequestOptions.ParametersOnly = ParametersOnly;
		reportRequestOptions.ReportLayoutFormat = ReportLayoutFormat;
		reportRequestOptions.ShowScheduleButton = ShowScheduleButton;
	}
}
