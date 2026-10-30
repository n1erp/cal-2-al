using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitFilterHelperTriggers : NavSystemCodeunit
{
	internal enum CodeunitMethod
	{
		MakeFilterOffset = 200
	}

	public NavSystemCodeunitFilterHelperTriggers(TreeObject parent)
		: base(parent, 2000000007)
	{
	}

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	internal static int Datatype2Cu1TriggerNo(NavType datatype)
	{
		return datatype switch
		{
			NavType.Boolean => -1, 
			NavType.Option => -1, 
			NavType.Integer => 3, 
			NavType.Decimal => -1, 
			NavType.Date => 5, 
			NavType.Time => 6, 
			NavType.Text => 7, 
			NavType.Char => -1, 
			NavType.Code => 9, 
			NavType.NotSupported_Binary => -1, 
			NavType.BLOB => -1, 
			NavType.BigInteger => -1, 
			NavType.Duration => -1, 
			NavType.DateTime => 14, 
			NavType.GUID => -1, 
			_ => 0, 
		};
	}

	public string InvokeMakeNavTypeFilter(string toEvaluate, NavType navType)
	{
		int num = Datatype2Cu1TriggerNo(navType);
		if (num == -1 || num == 0)
		{
			return toEvaluate;
		}
		NavText text = new NavText(toEvaluate);
		object[] arguments = new object[1]
		{
			new ByRef<NavText>(() => text, (NavText value) =>
			{
				text = value;
			})
		};
		Invoke(200 + num, arguments);
		return text;
	}
}
