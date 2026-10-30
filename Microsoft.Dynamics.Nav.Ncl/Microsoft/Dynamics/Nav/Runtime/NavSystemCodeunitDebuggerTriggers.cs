namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitDebuggerTriggers : NavSystemCodeunit
{
	internal enum CodeunitMethod
	{
		OnDebuggerBreak = 1
	}

	public NavSystemCodeunitDebuggerTriggers(TreeObject parent)
		: base(parent, 2000000009)
	{
	}

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	public void InvokeOnDebuggerBreak(NavText errorMessage)
	{
		Invoke(1, new object[1] { errorMessage });
	}
}
