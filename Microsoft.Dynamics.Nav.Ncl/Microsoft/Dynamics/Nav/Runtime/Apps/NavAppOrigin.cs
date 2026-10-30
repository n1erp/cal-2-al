using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppOrigin
{
	private static readonly NavAppOrigin baseTag = new NavAppOrigin(null);

	public static NavAppOrigin Base => baseTag;

	public Guid? AppId { get; }

	private NavAppOrigin(Guid? appId)
	{
		AppId = appId;
	}

	public static NavAppOrigin CreateForApp(Guid? appId)
	{
		return new NavAppOrigin(appId);
	}

	protected bool Equals(NavAppOrigin other)
	{
		return AppId.Equals(other.AppId);
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
		return Equals((NavAppOrigin)obj);
	}

	public override int GetHashCode()
	{
		return AppId.GetHashCode();
	}
}
