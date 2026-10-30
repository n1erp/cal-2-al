using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public static class PrivacyHelper
{
	public const string RedactedText = "<redacted>";

	public static bool IsPersonalOrRestrictedData(DiagnosticParameter diagnosticParameter)
	{
		return IsPersonalOrRestrictedData(diagnosticParameter.Classification);
	}

	public static bool IsPersonalOrRestrictedData(PrivacyClassification classification)
	{
		if (classification >= PrivacyClassification.OrganizationIdentifiableInformation)
		{
			return classification == PrivacyClassification.Invalid;
		}
		return true;
	}

	public static bool ShouldBeScrubbed(PrivacyClassification classification)
	{
		if (classification != PrivacyClassification.CustomerContent && classification != PrivacyClassification.ObjectMetadata)
		{
			return classification == PrivacyClassification.Invalid;
		}
		return true;
	}

	public static string MarkIfDataShouldBeScrubbed(string content, PrivacyClassification classification)
	{
		if (!ShouldBeScrubbed(classification))
		{
			return content;
		}
		return MarkForScrubbing(content);
	}

	public static string MarkForScrubbing(string value)
	{
		if (value == null)
		{
			return null;
		}
		return "<pi>" + value + "</pi>";
	}

	public static PrivacyClassification GetMoreRestrictiveClassification(PrivacyClassification classification1, PrivacyClassification classification2)
	{
		if (classification1 > classification2)
		{
			return classification2;
		}
		return classification1;
	}

	public static PrivacyClassification ConvertDataClassificationToPrivacyClassification(this DataClassification classification)
	{
		switch (classification)
		{
		case DataClassification.CustomerContent:
		case DataClassification.ToBeClassified:
			return PrivacyClassification.CustomerContent;
		case DataClassification.EndUserIdentifiableInformation:
			return PrivacyClassification.EndUserIdentifiableInformation;
		case DataClassification.AccountData:
			return PrivacyClassification.AccountData;
		case DataClassification.EndUserPseudonymousIdentifiers:
			return PrivacyClassification.EndUserPseudonymousIdentifiers;
		case DataClassification.OrganizationIdentifiableInformation:
			return PrivacyClassification.OrganizationIdentifiableInformation;
		case DataClassification.SystemMetadata:
			return PrivacyClassification.SystemMetadata;
		default:
			throw new ArgumentOutOfRangeException("classification", classification, null);
		}
	}
}
