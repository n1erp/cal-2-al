using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class SystemTaskParameters
{
	internal string GetSerializedAsJson()
	{
		return JsonConvert.SerializeObject(this);
	}

	internal abstract string GetNonPersonalDataParametersForTelemetry();
}
