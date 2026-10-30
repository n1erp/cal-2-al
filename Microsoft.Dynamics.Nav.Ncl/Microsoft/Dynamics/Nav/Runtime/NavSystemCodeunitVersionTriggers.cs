namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitVersionTriggers : NavSystemCodeunit
{
	internal enum CodeunitMethod
	{
		GetApplicationVersion = 1,
		GetReleaseVersion,
		GetApplicationBuild
	}

	public NavSystemCodeunitVersionTriggers(TreeObject parent)
		: base(parent, 2000000001)
	{
	}

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	public NavText InvokeApplicationVersion()
	{
		NavText result = NavText.Default(0);
		Invoke(1, new object[1]
		{
			new ByRef<NavText>(() => result, (NavText value) =>
			{
				result = value;
			})
		});
		return result;
	}

	public NavText InvokeApplicationBuild()
	{
		NavText result = NavText.Default(0);
		Invoke(3, new object[1]
		{
			new ByRef<NavText>(() => result, (NavText value) =>
			{
				result = value;
			})
		});
		return result;
	}

	public NavText InvokeReleaseVersion()
	{
		NavText result = NavText.Default(0);
		Invoke(2, new object[1]
		{
			new ByRef<NavText>(() => result, (NavText value) =>
			{
				result = value;
			})
		});
		return result;
	}
}
