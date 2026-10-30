using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IExecutionListener
{
	void ProcessExit(NavMethodScope methodScope);

	void ProcessStatementHit(NavMethodScope methodScope);

	void ProcessException(NavMethodScope methodScope, Exception exception);

	bool CanCoexistWithOtherListener(IExecutionListener otherListener);
}
