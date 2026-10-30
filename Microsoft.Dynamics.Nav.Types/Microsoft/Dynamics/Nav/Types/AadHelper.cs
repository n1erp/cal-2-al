using System;
using System.Web;

namespace Microsoft.Dynamics.Nav.Types;

public static class AadHelper
{
	public const string TenantPlaceholder = "{AADTENANTID}";

	internal const string AppIdUriPlaceholder = "{APPIDURI}";

	internal const string HostnamePlaceholder = "{HOSTNAME}";

	public static string FindTenantId(Uri federationLoginEndpoint)
	{
		if (federationLoginEndpoint != null && federationLoginEndpoint.IsAbsoluteUri && federationLoginEndpoint.Segments.Length >= 2)
		{
			return HttpUtility.UrlDecode(federationLoginEndpoint.Segments[1].Trim('/'));
		}
		return string.Empty;
	}

	public static Uri CalculateWSFederationLoginEndpoint(Uri federationLoginEndpoint, string tenantId, Uri appIdUri)
	{
		string text = FindTenantId(federationLoginEndpoint);
		bool flag = text.Equals("{AADTENANTID}", StringComparison.OrdinalIgnoreCase);
		string text2 = FindRealm(federationLoginEndpoint);
		bool flag2 = text2.Equals("{APPIDURI}", StringComparison.OrdinalIgnoreCase);
		string text3 = federationLoginEndpoint.ToString();
		if (flag)
		{
			text3 = text3.Replace(text, tenantId);
		}
		if (flag2 && appIdUri != null)
		{
			text3 = text3.Replace(text2, appIdUri.ToString());
		}
		return DecodedUriWithPlaceholderSetting.Parse(HttpUtility.UrlDecode(text3), "WSFederationLoginEndpoint").Value;
	}

	public static Uri CalculateWSFederationMetadataLocation(Uri federationMetadataLocation, string tenantId)
	{
		string text = FindTenantId(federationMetadataLocation);
		if (text.Equals("{AADTENANTID}", StringComparison.OrdinalIgnoreCase))
		{
			return SimpleConfigSettingNoMaxValue<Uri>.Parse(federationMetadataLocation.ToString().Replace(text, tenantId), "ClientServicesFederationMetadataLocation").Value;
		}
		return federationMetadataLocation;
	}

	public static string FindRealm(Uri federationLoginEndpoint)
	{
		return GetQueryStringParameterValue(federationLoginEndpoint, "wtrealm");
	}

	public static string FindReply(Uri federationLoginEndpoint)
	{
		return GetQueryStringParameterValue(federationLoginEndpoint, "wreply");
	}

	public static string FindAcsServiceNamespace(Uri federationLoginEndpoint)
	{
		if (federationLoginEndpoint != null)
		{
			return federationLoginEndpoint.Host.Split('.')[0];
		}
		return string.Empty;
	}

	private static string GetQueryStringParameterValue(Uri uri, string parameterName)
	{
		if (uri != null && uri.IsAbsoluteUri && !string.IsNullOrEmpty(uri.Query))
		{
			string text = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(uri.Query))[parameterName];
			if (!string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		return string.Empty;
	}
}
