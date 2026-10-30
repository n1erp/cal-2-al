using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Linq;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types;

public static class AudienceHelper
{
	private static readonly NavDiagnostics diagnostics = NavDiagnostics.CreateSectionDiagnostics(NavDiagnostics.GlobalInstance, MonitorType.None, singleThreadMonitorAccess: true);

	private static IEnumerable<string> allowedAudiences = AllowedAudiences;

	private static AudienceRestriction audienceRestrictions = AudienceRestrictions;

	public static AudienceRestriction DefaultAudienceRestrictions
	{
		get
		{
			AudienceRestriction audienceRestriction = audienceRestrictions;
			if (audienceRestriction != null && audienceRestriction.AllowedAudienceUris?.Count == 0)
			{
				EventLogWriter.Write(Lang.ConfigurationInvalidUri, EventLogEntryType.Error, null, 0, null, 0);
				diagnostics?.SendTraceTag("00000MD", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, Lang.ConfigurationInvalidUri);
			}
			return audienceRestrictions;
		}
	}

	public static IEnumerable<string> DefaultAllowedAudiences
	{
		get
		{
			IEnumerable<string> enumerable = allowedAudiences;
			if (enumerable != null && enumerable.Count() == 0)
			{
				EventLogWriter.Write(Lang.ConfigurationInvalidUri, EventLogEntryType.Error, null, 0, null, 0);
				diagnostics?.SendTraceTag("00000ME", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, Lang.ConfigurationInvalidUri);
			}
			return allowedAudiences;
		}
	}

	private static IEnumerable<string> AllowedAudiences
	{
		get
		{
			List<string> list = new List<string>();
			IList<string> list2 = ServerUserSettings.Instance.ValidAudiences?.ValidAudiencesList;
			if (list2 != null)
			{
				list.AddRange(list2);
			}
			foreach (string knownApplicationIdType in ApplicationIdTypes.KnownApplicationIdTypes)
			{
				try
				{
					Uri uri = ServerUserSettings.Instance.GetAppSpecificSettings(knownApplicationIdType)?.AppIdUri.Value ?? ServerUserSettings.Instance.AppIdUri.Value;
					string text = AadHelper.FindRealm(AadHelper.CalculateWSFederationLoginEndpoint(ServerUserSettings.Instance.WSFederationLoginEndpoint.Value, "notused", uri));
					if (string.IsNullOrWhiteSpace(text))
					{
						text = uri?.ToString();
					}
					if (!string.IsNullOrWhiteSpace(text))
					{
						text = text.TrimEnd('/');
						list.Add(text);
						list.Add(text + "/");
					}
				}
				catch (NavConfigurationException)
				{
				}
			}
			return list.Distinct();
		}
	}

	private static AudienceRestriction AudienceRestrictions
	{
		get
		{
			AudienceRestriction audienceRestriction = new AudienceRestriction(AudienceUriMode.Always);
			foreach (string allowedAudience in allowedAudiences)
			{
				if (!Uri.IsWellFormedUriString(allowedAudience, UriKind.Absolute))
				{
					string message = string.Format(CultureInfo.CurrentCulture, Lang.SkipNotWellFormedUriAudience, allowedAudience);
					diagnostics?.SendTraceTag("00000M7", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, message);
				}
				else
				{
					audienceRestriction.AllowedAudienceUris.Add(new Uri(allowedAudience));
				}
			}
			return audienceRestriction;
		}
	}
}
