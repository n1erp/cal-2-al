using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavModuleInfo : NavValue, IEquatable<NavModuleInfo>
{
	public static NavModuleInfo Default => new NavModuleInfo(Guid.Empty, string.Empty, string.Empty, NavVersion.Default, NavVersion.Default, new NavList<NavModuleDependencyInfo>());

	public Guid ALId { get; }

	public string ALName { get; }

	public string ALPublisher { get; }

	public NavVersion ALAppVersion { get; }

	public NavVersion ALDataVersion { get; }

	public NavList<NavModuleDependencyInfo> ALDependencies { get; }

	public override NavNclType NclType => NavNclType.NavInstalledAppInfo;

	public override int GetBytesSize => 0;

	public override bool IsZeroOrEmpty => false;

	public NavModuleInfo(Guid appId, string name, string publisher, NavVersion appVersion, NavVersion dataVersion, NavList<NavModuleDependencyInfo> dependencies)
	{
		ALId = appId;
		ALName = name;
		ALPublisher = publisher;
		ALAppVersion = appVersion;
		ALDataVersion = dataVersion;
		ALDependencies = dependencies;
	}

	public bool Equals(NavModuleInfo other)
	{
		Guid? guid = other?.ALId;
		Guid aLId = ALId;
		if (guid.HasValue && (!guid.HasValue || guid.GetValueOrDefault() == aLId) && string.Equals(other?.ALName, ALName, StringComparison.OrdinalIgnoreCase) && string.Equals(other?.ALPublisher, ALPublisher, StringComparison.OrdinalIgnoreCase) && other?.ALAppVersion == ALAppVersion)
		{
			return other?.ALDataVersion == ALDataVersion;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ALId.GetHashCode() ^ ALName.GetHashCode() ^ ALPublisher.GetHashCode() ^ ALAppVersion.GetHashCode() ^ ALDataVersion.GetHashCode();
	}
}
