using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppDependency
{
	public Guid AppId { get; set; }

	public string Name { get; set; }

	public string Publisher { get; set; }

	public Version MinVersion { get; set; }

	public Version CompatibilityId { get; set; }

	public override string ToString()
	{
		return string.Format(CultureInfo.CurrentCulture, "{0}, {1}, {2}", Name, Publisher, MinVersion);
	}
}
