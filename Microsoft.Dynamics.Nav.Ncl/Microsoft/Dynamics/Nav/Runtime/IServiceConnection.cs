using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IServiceConnection : IDisposable
{
	IClientCallback ClientCallback { get; }

	void BeginNonServiceCallExecution();

	void EndNonServiceCallExecution();

	INavService CreateNavTestService();

	void SetServerFormRequestData(NavForm form, NavFormRuntimeParameters parameters, FormRunData data);

	FormCloseRequest MakeFormCloseRequest(NavForm form);

	void OnSessionTerminating(bool notifyClient, bool abortChannel);
}
