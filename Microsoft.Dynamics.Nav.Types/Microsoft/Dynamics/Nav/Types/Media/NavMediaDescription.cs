using System;

namespace Microsoft.Dynamics.Nav.Types.Media;

[Serializable]
public class NavMediaDescription
{
	public Guid MediaId { get; set; }

	public string Description { get; set; }

	public NavMediaDescription()
	{
	}

	public NavMediaDescription(Guid id, string description)
	{
		MediaId = id;
		Description = description;
	}
}
