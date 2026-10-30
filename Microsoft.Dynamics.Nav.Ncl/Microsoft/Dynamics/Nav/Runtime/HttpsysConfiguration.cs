using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class HttpsysConfiguration
{
	private static readonly string netshPath = Path.Combine(Environment.SystemDirectory, "netsh.exe");

	private const string SslcertAppId = "{00112233-4455-6677-8899-aabbccddeeff}";

	private readonly string endpointName;

	public HttpsysConfiguration(string endpointName)
	{
		if (string.IsNullOrWhiteSpace(endpointName))
		{
			throw new ArgumentException("Http endpoint is invalid");
		}
		this.endpointName = endpointName;
	}

	public static IEnumerator GetAllReservedUrls(string portNumber, bool isSsl)
	{
		string allUrlAclsStr = string.Empty;
		ProcessHelper.RunProcess(ShowUrlAcl(), netshPath, throwOnError: false, (string str) =>
		{
			allUrlAclsStr = str;
			return true;
		});
		string arg = (isSsl ? "https" : "http");
		string pattern = string.Format(CultureInfo.InvariantCulture, "{0}://.*:{1}/.*", arg, portNumber);
		return Regex.Matches(allUrlAclsStr, pattern).GetEnumerator();
	}

	public bool FindUrlAccessControlRule(string portNumber, bool isSsl)
	{
		return _FindUrlAclRule(endpointName, portNumber, isSsl);
	}

	private static bool _FindUrlAclRule(string endpointName, string portNumber, bool isSsl)
	{
		bool urlAclIsFound = false;
		ProcessHelper.RunProcess(FindUrlAcl(endpointName, portNumber, isSsl), netshPath, throwOnError: false, (string str) => urlAclIsFound = str.Contains(endpointName));
		return urlAclIsFound;
	}

	public void AddUrlAccessControlRule(string portNumber, string userName, bool isSsl)
	{
		_AddUrlAclRule(endpointName, portNumber, userName, isSsl);
	}

	private static void _AddUrlAclRule(string endpointName, string portNumber, string userName, bool isSsl)
	{
		if (!_FindUrlAclRule(endpointName, portNumber, isSsl))
		{
			ProcessHelper.RunProcess(AddUrlAcl(endpointName, portNumber, isSsl, userName), netshPath);
		}
	}

	public bool RemoveUrlAccessControlRule(string portNumber, bool isSsl)
	{
		return _RemoveUrlAclRule(endpointName, portNumber, isSsl);
	}

	private static bool _RemoveUrlAclRule(string endpointName, string portNumber, bool isSsl)
	{
		if (_FindUrlAclRule(endpointName, portNumber, isSsl))
		{
			ProcessHelper.RunProcess(DeleteUrlAcl(endpointName, portNumber, isSsl), netshPath);
			return true;
		}
		return false;
	}

	public static void AddSslCertBinding(string portNumber, string certificateThumbprint)
	{
		bool sslCertIsFound = false;
		ProcessHelper.RunProcess(FindCert(portNumber), netshPath, throwOnError: false, (string str) => sslCertIsFound = str.Contains("{00112233-4455-6677-8899-aabbccddeeff}"));
		if (!sslCertIsFound)
		{
			certificateThumbprint = Regex.Replace(certificateThumbprint, "\\s", string.Empty);
			ProcessHelper.RunProcess(AddCert(portNumber, certificateThumbprint), netshPath);
		}
	}

	public static void DeleteSslCertBinding(string portNumber)
	{
		bool sslCertIsFound = false;
		ProcessHelper.RunProcess(FindCert(portNumber), netshPath, throwOnError: false, (string str) => sslCertIsFound = str.Contains("{00112233-4455-6677-8899-aabbccddeeff}"));
		if (sslCertIsFound)
		{
			ProcessHelper.RunProcess(DeleteCert(portNumber), netshPath);
		}
	}

	private static string FindUrlAcl(string endpointName, string portNumber, bool isSsl)
	{
		return string.Format(CultureInfo.InvariantCulture, "http show urlacl url=\"{2}://+:{0}/{1}/\"", portNumber, endpointName, isSsl ? "https" : "http");
	}

	private static string ShowUrlAcl()
	{
		return "http show urlacl";
	}

	private static string AddUrlAcl(string endpointName, string portNumber, bool isSsl, string userName)
	{
		return string.Format(CultureInfo.InvariantCulture, "http add urlacl url={3}://+:{0}/{1} user={2}", portNumber, endpointName, userName, isSsl ? "https" : "http");
	}

	private static string DeleteUrlAcl(string endpointName, string portNumber, bool isSsl)
	{
		return string.Format(CultureInfo.InvariantCulture, "http delete urlacl url=\"{2}://+:{0}/{1}/\"", portNumber, endpointName, isSsl ? "https" : "http");
	}

	private static string FindCert(string portNumber)
	{
		return string.Format(CultureInfo.InvariantCulture, "http show sslcert ipport=0.0.0.0:{0}", portNumber);
	}

	private static string AddCert(string portNumber, string certificateThumbprint)
	{
		return string.Format(CultureInfo.InvariantCulture, "http add sslcert ipport=0.0.0.0:{0} certhash={1} appid=\"{2}\"", portNumber, certificateThumbprint, "{00112233-4455-6677-8899-aabbccddeeff}");
	}

	private static string DeleteCert(string portNumber)
	{
		return string.Format(CultureInfo.InvariantCulture, "http delete sslcert ipport=0.0.0.0:{0}", portNumber);
	}
}
