using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppMediaEntry
{
	public string FileName { get; set; }

	public Guid Id { get; set; }

	public bool IsLogo { get; set; }
}
