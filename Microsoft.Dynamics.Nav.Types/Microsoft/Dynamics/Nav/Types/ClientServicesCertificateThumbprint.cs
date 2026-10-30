using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Nav.Types;

public class ClientServicesCertificateThumbprint : RestrictedStringConfigSetting<ClientServicesCertificateThumbprint>
{
	private const int MaxLength = -1;

	private const string ValidStringRegEx = "[^A-Fa-f0-9-\\s]";

	public ClientServicesCertificateThumbprint(string value)
		: this(value, string.Empty)
	{
	}

	public ClientServicesCertificateThumbprint(string value, string settingName)
		: base(Regex.Replace(value, "\\s", ""), settingName, true, -1, "[^A-Fa-f0-9-\\s]")
	{
	}
}
