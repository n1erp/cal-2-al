using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavBLOBEvaluator : NavValueEvaluator<NavBLOB>
{
	private static readonly NavBLOBEvaluator instance = new NavBLOBEvaluator();

	internal static NavBLOBEvaluator Instance => instance;

	private NavBLOBEvaluator()
	{
	}

	protected override bool InternalEvaluate(DataError dataError, out NavBLOB value, INavValueMetadata metadata, string source, int number)
	{
		string text = source.Trim();
		if (text != null)
		{
			switch (text)
			{
			default:
				if (text.Length != 0)
				{
					goto case null;
				}
				goto IL_0049;
			case null:
				if (!(text == "0"))
				{
					break;
				}
				goto IL_0049;
			case "1":
			case "*":
				{
					value = new NavBLOB(1);
					return true;
				}
				IL_0049:
				value = NavBLOB.Default();
				return true;
			}
		}
		value = NavBLOB.Default();
		return false;
	}

	protected override NavBLOB GetDefaultValue(INavValueMetadata metadata)
	{
		return NavBLOB.Default();
	}
}
