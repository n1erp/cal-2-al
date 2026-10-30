using System;

namespace Microsoft.Dynamics.Nav.Types.Media;

public class NavUrlAccessibleMedia
{
	public Guid MediaId { get; set; }

	public byte[] Content { get; set; }

	public string MimeType { get; set; }

	public string FileName { get; set; }

	public string SecurityToken { get; set; }

	public DateTime Expiration { get; set; }
}
