using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class SystemTask<T> : ISystemTask where T : SystemTaskParameters
{
	private string originalParametersJson = "";

	public T Parameters { get; private set; }

	public void Initialize(string parametersJson)
	{
		originalParametersJson = parametersJson;
		Parameters = JsonConvert.DeserializeObject<T>(parametersJson);
	}

	public abstract void Run(NavSession systemSession);

	public abstract bool ShouldRetry();

	public string GetParametersJson()
	{
		return originalParametersJson;
	}

	public string GetNonPersonalDataParametersForTelemetry()
	{
		return Parameters.GetNonPersonalDataParametersForTelemetry();
	}
}
