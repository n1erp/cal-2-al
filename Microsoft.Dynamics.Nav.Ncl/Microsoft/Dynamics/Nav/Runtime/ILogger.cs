using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface ILogger
{
	void Flush();

	void Log(Dictionary<string, string> payload);
}
