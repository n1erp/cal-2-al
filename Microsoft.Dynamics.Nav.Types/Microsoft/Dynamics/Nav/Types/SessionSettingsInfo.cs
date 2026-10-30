using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SessionSettingsInfo
{
	[DataMember]
	public string SessionCompany { get; set; } = string.Empty;

	[DataMember]
	public string ProfileId { get; set; } = string.Empty;

	[DataMember]
	public Guid ProfileAppId { get; set; } = Guid.Empty;

	[DataMember]
	public bool ProfileSystemScope { get; set; }

	[DataMember]
	public string TimeZone { get; set; } = string.Empty;

	[DataMember]
	public int LocaleId { get; set; }

	[DataMember]
	public int LanguageId { get; set; }

	[DataMember]
	public bool SettingsSaved { get; set; }

	public bool IsZeroOrEmpty
	{
		get
		{
			if (string.IsNullOrEmpty(SessionCompany) && LanguageId == 0 && LocaleId == 0 && string.IsNullOrEmpty(TimeZone) && string.IsNullOrEmpty(ProfileId) && ProfileAppId.Equals(Guid.Empty))
			{
				return !ProfileSystemScope;
			}
			return false;
		}
	}

	public SessionSettingsInfo Clone()
	{
		return new SessionSettingsInfo(this);
	}

	public SessionSettingsInfo()
	{
	}

	public SessionSettingsInfo(SessionSettingsInfo other)
	{
		SessionCompany = other.SessionCompany;
		LanguageId = other.LanguageId;
		LocaleId = other.LocaleId;
		TimeZone = other.TimeZone;
		ProfileId = other.ProfileId;
		SettingsSaved = other.SettingsSaved;
		ProfileAppId = other.ProfileAppId;
		ProfileSystemScope = other.ProfileSystemScope;
	}

	public SessionSettingsInfo(string formattedValue)
	{
		char[] separator = new char[1] { ',' };
		string[] array = formattedValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			int num = text.IndexOf("=", StringComparison.OrdinalIgnoreCase);
			if (num > 0)
			{
				string text2 = text.Substring(0, num).Trim();
				string text3 = text.Substring(num + 1).Trim();
				switch (text2.ToUpperInvariant())
				{
				case "COMPANY":
					SessionCompany = text3;
					break;
				case "PROFILEID":
					ProfileId = text3;
					break;
				case "LANGUAGEID":
					LanguageId = int.Parse(text3, CultureInfo.InvariantCulture);
					break;
				case "LOCALEID":
					LocaleId = int.Parse(text3, CultureInfo.InvariantCulture);
					break;
				case "TIMEZONE":
					TimeZone = text3;
					break;
				case "PROFILEAPPID":
					ProfileAppId = new Guid(text3);
					break;
				case "PROFILESYSTEMSCOPE":
					ProfileSystemScope = bool.Parse(text3);
					break;
				}
			}
		}
	}

	protected bool Equals(SessionSettingsInfo other)
	{
		if (string.Equals(SessionCompany, other.SessionCompany) && string.Equals(ProfileId, other.ProfileId) && string.Equals(TimeZone, other.TimeZone) && LocaleId == other.LocaleId && LanguageId == other.LanguageId && SettingsSaved == other.SettingsSaved && ProfileAppId == other.ProfileAppId)
		{
			return ProfileSystemScope == other.ProfileSystemScope;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((SessionSettingsInfo)obj);
	}

	public override int GetHashCode()
	{
		return (((((((((((((((SessionCompany != null) ? SessionCompany.GetHashCode() : 0) * 397) ^ ((ProfileId != null) ? ProfileId.GetHashCode() : 0)) * 397) ^ ((TimeZone != null) ? TimeZone.GetHashCode() : 0)) * 397) ^ LocaleId) * 397) ^ LanguageId) * 397) ^ SettingsSaved.GetHashCode()) * 397) ^ ProfileAppId.GetHashCode()) * 397) ^ ProfileSystemScope.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "Company={0}, ProfileId={1}, LanguageId={2}, LocaleId={3}, TimeZone={4}, ProfileAppId={5}, ProfileSystemScope={6}", SessionCompany, ProfileId, LanguageId, LocaleId, TimeZone, ProfileAppId.ToString("B").ToUpperInvariant(), ProfileSystemScope);
	}
}
