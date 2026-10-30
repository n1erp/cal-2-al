using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

public class NavAppMediaSetEntry
{
	public string Name { get; set; }

	public Guid SetId { get; set; }

	public IEnumerable<NavAppMediaEntry> Media { get; set; }
}
