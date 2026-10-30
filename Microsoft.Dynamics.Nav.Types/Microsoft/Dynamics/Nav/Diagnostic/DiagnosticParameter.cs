using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Diagnostic;

public class DiagnosticParameter : IFormattable
{
	public object Content { get; set; }

	public PrivacyClassification Classification { get; set; }

	internal DiagnosticParameter(object content, PrivacyClassification classification)
	{
		Content = content;
		Classification = classification;
	}

	public static DiagnosticParameter AccessControlData(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.AccessControlData);
	}

	public static DiagnosticParameter CustomerContent(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.CustomerContent);
	}

	public static DiagnosticParameter Euii(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.EndUserIdentifiableInformation);
	}

	public static DiagnosticParameter ObjectMetadata(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.ObjectMetadata);
	}

	public static DiagnosticParameter CustomerSupportData(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.CustomerSupportData);
	}

	public static DiagnosticParameter AccountData(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.AccountData);
	}

	public static DiagnosticParameter PublicPersonalData(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.PublicPersonalData);
	}

	public static DiagnosticParameter Eupi(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.EndUserPseudonymousIdentifiers);
	}

	public static DiagnosticParameter Oii(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.OrganizationIdentifiableInformation);
	}

	public static DiagnosticParameter SystemMetadata(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.SystemMetadata);
	}

	public static DiagnosticParameter PublicNonPersonalData(object content)
	{
		return new DiagnosticParameter(content, PrivacyClassification.PublicNonPersonalData);
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		if (Content is IFormattable formattable)
		{
			return PrivacyHelper.MarkIfDataShouldBeScrubbed(formattable.ToString(format, provider), Classification);
		}
		if (Content != null)
		{
			return PrivacyHelper.MarkIfDataShouldBeScrubbed(Content.ToString(), Classification);
		}
		return PrivacyHelper.MarkIfDataShouldBeScrubbed(null, Classification);
	}

	[Conditional("DEBUG")]
	private void AssertContentHasUsefulStringRepresentation(string contentAsString)
	{
		_ = contentAsString == Content.GetType().FullName;
	}
}
