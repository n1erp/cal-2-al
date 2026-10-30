using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavModuleDependencyInfo : NavValue, IEquatable<NavModuleDependencyInfo>
{
	public static NavModuleDependencyInfo Default => new NavModuleDependencyInfo(Guid.Empty, string.Empty, string.Empty);

	public Guid ALId { get; }

	public string ALName { get; }

	public string ALPublisher { get; }

	public override NavNclType NclType => NavNclType.NavAppDependencyInfo;

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => false;

	public NavModuleDependencyInfo(Guid appId, string name, string publisher)
	{
		ALId = appId;
		ALName = name;
		ALPublisher = publisher;
	}

	public bool Equals(NavModuleDependencyInfo other)
	{
		Guid? guid = other?.ALId;
		Guid aLId = ALId;
		if (guid.HasValue && (!guid.HasValue || guid.GetValueOrDefault() == aLId) && string.Equals(other?.ALName, ALName, StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(other?.ALPublisher, ALPublisher, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ALId.GetHashCode() ^ ALName.GetHashCode() ^ ALPublisher.GetHashCode();
	}
}
