using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public struct NavFormRuntimeParameters
{
	private ReportRequestOptions reportRequestOptions;

	public bool LookupMode { get; set; }

	public string FieldName { get; set; }

	public ReportRequestOptions ReportRequestOptions => reportRequestOptions ?? (reportRequestOptions = new ReportRequestOptions());

	public override bool Equals(object obj)
	{
		if (!(obj is NavFormRuntimeParameters navFormRuntimeParameters))
		{
			return false;
		}
		if (FieldName == navFormRuntimeParameters.FieldName && LookupMode == navFormRuntimeParameters.LookupMode)
		{
			return ReportRequestOptions.ParametersOnly == navFormRuntimeParameters.ReportRequestOptions.ParametersOnly;
		}
		return false;
	}

	public static bool operator ==(NavFormRuntimeParameters param1, NavFormRuntimeParameters param2)
	{
		return param1.Equals(param2);
	}

	public static bool operator !=(NavFormRuntimeParameters param1, NavFormRuntimeParameters param2)
	{
		return !(param1 == param2);
	}

	public override int GetHashCode()
	{
		return FieldName.GetHashCode() ^ LookupMode.GetHashCode() ^ ReportRequestOptions.ParametersOnly.GetHashCode();
	}
}
