using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public static class CertificateValidator
{
	public const char LeftToRightMark = '\u200e';

	private static Regex regex = new Regex("^([0-9a-fA-F]\\s*){40}$");

	public static void ValidateCertificateThumbprint(string thumbprint)
	{
		if (string.IsNullOrEmpty(thumbprint))
		{
			throw new NavConfigurationException(Lang.CertificateThumbprintNotSpecified);
		}
		if (thumbprint[0].Equals('\u200e'))
		{
			throw new NavConfigurationException(Lang.CertificateThumbprintInvalidExtraCharacter);
		}
		if (!regex.Match(thumbprint).Success)
		{
			throw new NavConfigurationException(Lang.CertificateThumbprintInvalid);
		}
	}
}
