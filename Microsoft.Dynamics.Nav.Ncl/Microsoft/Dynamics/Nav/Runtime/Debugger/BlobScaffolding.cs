using System;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class BlobScaffolding : NavValueScaffolding
{
	public BlobScaffolding(string name, Type type, ValueBinder valueBinder)
		: base(name, type, NavNclType.NavBlob, valueBinder)
	{
	}

	protected override string GetStringValue(NavValue instance)
	{
		return "Length = " + ((NavBLOB)instance).GetLength();
	}
}
