using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitSystemActionTriggers : NavSystemCodeunit
{
	internal enum CodeunitMethod
	{
		GetDefaultRoleCenterId = 1,
		CustomizeChart = 2,
		OpenSettings = 3,
		OpenContactMsSales = 4,
		OpenDebugger = 5,
		OnEditInExcel = 6,
		OnLastErrorPage = 8,
		GetSupportInformation = 9
	}

	public NavSystemCodeunitSystemActionTriggers(TreeObject parent)
		: base(parent, 2000000006)
	{
	}

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	public int InvokeGetDefaultRoleCenter()
	{
		int result = 0;
		Invoke(1, new object[1]
		{
			new ByRef<int>(() => result, (int value) =>
			{
				result = value;
			})
		});
		return result;
	}

	public bool InvokeCustomizeChart(NavRecord chart)
	{
		bool result = false;
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(2, new object[2]
			{
				chart,
				new ByRef<bool>(() => result, (bool value) =>
				{
					result = value;
				})
			});
		});
		return result;
	}

	public void InvokeOpenSettings()
	{
		Invoke(3, NavSystemCodeunit.EmptyObjectArray);
	}

	public void InvokeLaunchDebugger()
	{
		Invoke(5, NavSystemCodeunit.EmptyObjectArray);
	}

	public void InvokeOnEditInExcel(string serviceName, string odataFilter)
	{
		object[] args = new object[2] { serviceName, odataFilter };
		NavCurrentThread.Session.TransactionScope(() =>
		{
			Invoke(6, args);
		});
	}

	public void InvokeOpenLastErrorPage()
	{
		Invoke(8, NavSystemCodeunit.EmptyObjectArray);
	}

	public SupportInformationResponse InvokeGetSupportInformation()
	{
		NavText name = NavText.Empty;
		NavText email = NavText.Empty;
		NavText url = NavText.Empty;
		object[] arguments = new object[3]
		{
			new ByRef<NavText>(() => name, (NavText value) =>
			{
				name = value;
			}),
			new ByRef<NavText>(() => email, (NavText value) =>
			{
				email = value;
			}),
			new ByRef<NavText>(() => url, (NavText value) =>
			{
				url = value;
			})
		};
		Invoke(9, arguments);
		return new SupportInformationResponse
		{
			Name = name.Value,
			Email = email.Value,
			Url = url.Value
		};
	}
}
