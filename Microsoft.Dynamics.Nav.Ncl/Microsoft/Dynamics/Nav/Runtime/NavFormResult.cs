using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavFormResult : NavEnumBase<FormResult>
{
	private static NavFormResult defaultValue = new NavFormResult(FormResult.None);

	public override NavNclType NclType => NavNclType.NavFormResult;

	public static NavFormResult Default => defaultValue;

	public NavFormResult(FormResult initValue)
		: base(initValue)
	{
	}

	public NavFormResult(byte[] bytes)
		: base((FormResult)BitConverter.ToInt32(bytes, 0))
	{
	}
}
