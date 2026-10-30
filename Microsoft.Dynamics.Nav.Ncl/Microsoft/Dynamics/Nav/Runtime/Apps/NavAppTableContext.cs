namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppTableContext
{
	public static NavAppTableContext Main { get; } = new NavAppTableContext(NavAppTableContextType.Main, NavAppOrigin.Base);

	public NavAppOrigin CompanionTableAppOrigin { get; }

	public NavAppTableContextType TableType { get; }

	private NavAppTableContext(NavAppTableContextType tableType, NavAppOrigin companionTableAppOrigin)
	{
		TableType = tableType;
		CompanionTableAppOrigin = companionTableAppOrigin;
	}

	public static NavAppTableContext Companion(NavAppOrigin appOrigin)
	{
		return new NavAppTableContext(NavAppTableContextType.Companion, appOrigin);
	}

	protected bool Equals(NavAppTableContext other)
	{
		if (object.Equals(CompanionTableAppOrigin, other.CompanionTableAppOrigin))
		{
			return TableType == other.TableType;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((NavAppTableContext)obj);
	}

	public override int GetHashCode()
	{
		return ((CompanionTableAppOrigin?.GetHashCode() ?? 0) * 397) ^ (int)TableType;
	}
}
