using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTestAction : NavValue
{
	private ITestAction testAction;

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => testAction == null;

	public override NavNclType NclType => NavNclType.NavTestAction;

	public void ALInvoke()
	{
		testAction.Invoke();
	}

	public bool ALEnabled()
	{
		return testAction.Enabled;
	}

	public bool ALVisible()
	{
		return testAction.Visible;
	}

	public NavTestAction(ITestAction testAction)
	{
		this.testAction = testAction;
	}

	public override int GetHashCode()
	{
		if (testAction != null)
		{
			return testAction.GetHashCode();
		}
		return 0;
	}

	public override string ToString()
	{
		return "TestAction";
	}
}
