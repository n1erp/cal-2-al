namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitCompanyTriggers : NavSystemCodeunit
{
	internal enum CodeunitMethod
	{
		OnCompanyOpen = 1,
		OnCompanyClose,
		OnAfterCompanyOpen
	}

	public NavSystemCodeunitCompanyTriggers(TreeObject parent)
		: base(parent, 2000000003)
	{
	}

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	public void InvokeOnCompanyOpen()
	{
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(1, NavSystemCodeunit.EmptyObjectArray);
		});
	}

	public void InvokeOnCompanyClose()
	{
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(2, NavSystemCodeunit.EmptyObjectArray);
		});
	}

	public void InvokeOnAfterCompanyOpen()
	{
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(3, NavSystemCodeunit.EmptyObjectArray);
		});
	}
}
