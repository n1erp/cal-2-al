namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public static class ControlKindExtensions
{
	public static bool IsGroup(this ControlKind controlKind)
	{
		if ((uint)controlKind <= 5u)
		{
			return true;
		}
		return false;
	}

	public static bool IsAnyPartKind(this ControlKind controlKind)
	{
		if ((uint)(controlKind - 6) <= 1u || controlKind == ControlKind.ChartPart)
		{
			return true;
		}
		return false;
	}
}
