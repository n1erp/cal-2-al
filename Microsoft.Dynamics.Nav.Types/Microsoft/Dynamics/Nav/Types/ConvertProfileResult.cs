namespace Microsoft.Dynamics.Nav.Types;

public enum ConvertProfileResult
{
	ConversionSuccess,
	ProfileConfigurationsNotFoundWarning,
	ProfileNotFoundWarning,
	ConversionWithMixedResults,
	ConversionError,
	UnknownConversionError
}
