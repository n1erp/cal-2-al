using System;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavFieldClass : NavEnumBase<FieldClass>
{
	private static NavFieldClass defaultValue = new NavFieldClass(FieldClass.Normal);

	public override NavNclType NclType => NavNclType.NavFieldClass;

	public static NavFieldClass Default => defaultValue;

	public NavFieldClass(FieldClass initValue)
		: base(initValue)
	{
	}

	public NavFieldClass(byte[] bytes)
		: base((FieldClass)BitConverter.ToInt32(bytes, 0))
	{
	}
}
