namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class ActionKindExtensions
{
	public static bool IsGroup(this ActionKind actionKind)
	{
		if ((uint)actionKind <= 1u)
		{
			return true;
		}
		return false;
	}
}
