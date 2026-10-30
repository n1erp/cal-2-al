using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IServiceControl
{
	void Terminate(Exception exception);

	void WriteWatsonLog(Exception exception);
}
