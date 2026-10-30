using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Constants;

public static class HttpDomainHelper
{
	internal static string PreferredDomainFragment(string appId)
	{
		appId = appId?.ToUpperInvariant();
		return appId switch
		{
			"FIN" => "financials.dynamics".ToUpperInvariant(), 
			"INV" => "invoicing.office".ToUpperInvariant(), 
			"ACC" => "accountants.dynamics".ToUpperInvariant(), 
			_ => string.Empty, 
		};
	}

	internal static string EvaluateApplicationIdTypeFromUrlHost(Uri requestUrl, NavDiagnostics diagnostics)
	{
		string text = EvaluateApplicationIdTypeFromHostName(requestUrl.Host);
		diagnostics?.SendTraceTag("0000103", Category.Security, Verbosity.Verbose, PrivacyClassification.OrganizationIdentifiableInformation, "EvaluateApplicationIdTypeFromUrlHost requestUrl={0}", requestUrl);
		if (text != null)
		{
			return text;
		}
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(requestUrl.Query);
		string text2 = nameValueCollection.Get("aid");
		if (!string.IsNullOrWhiteSpace(text2))
		{
			text = ValidateApplicationId(text2);
		}
		if (text != null)
		{
			return text;
		}
		string text3 = nameValueCollection.Get("tenant");
		if (!string.IsNullOrWhiteSpace(text3))
		{
			text = EvaluateApplicationIdTypeFromHostName(text3);
		}
		return text ?? ServerUserSettings.Instance.DefaultApplicationId.Value;
	}

	private static string EvaluateApplicationIdTypeFromHostName(string hostName)
	{
		if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(hostName, "invoicing.office", CompareOptions.IgnoreCase) >= 0)
		{
			return "INV";
		}
		if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(hostName, "financials.dynamics", CompareOptions.IgnoreCase) >= 0)
		{
			return "FIN";
		}
		if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(hostName, "accountants.dynamics", CompareOptions.IgnoreCase) >= 0)
		{
			return "ACC";
		}
		return null;
	}

	private static string ValidateApplicationId(string aid)
	{
		if (aid.Equals("INV") || aid.Equals("FIN") || aid.Equals("ACC"))
		{
			return aid;
		}
		return null;
	}
}
