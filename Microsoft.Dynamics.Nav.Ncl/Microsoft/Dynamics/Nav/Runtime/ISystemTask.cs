namespace Microsoft.Dynamics.Nav.Runtime;

internal interface ISystemTask
{
	void Initialize(string parametersJson);

	void Run(NavSession systemSession);

	bool ShouldRetry();

	string GetParametersJson();

	string GetNonPersonalDataParametersForTelemetry();
}
