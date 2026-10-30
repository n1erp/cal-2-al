using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public struct ClientSettings
{
	[DataMember]
	public bool UseLocaleForCurrentSessionOnly { get; set; }

	[DataMember]
	public string ShortTimeFormat { get; set; }

	[DataMember]
	public string LongTimePattern { get; set; }

	[DataMember]
	public string ShortTimeZero { get; set; }

	[DataMember]
	public string ShortDatePattern { get; set; }

	[DataMember]
	public string TimeAMDesignator { get; set; }

	[DataMember]
	public string TimePMDesignator { get; set; }

	[DataMember]
	public string TimeSeparator { get; set; }

	[DataMember]
	public string DateSeparator { get; set; }

	[DataMember]
	public string DecimalSeparator { get; set; }

	[DataMember]
	public string GroupSeparator { get; set; }

	[DataMember]
	public string CurrencyDecimalSeparator { get; set; }

	[DataMember]
	public string CurrencyGroupSeparator { get; set; }

	[DataMember]
	public int LCID { get; set; }

	[DataMember]
	public int WindowsLCID { get; set; }

	public void Refresh(CultureInfo formatCulture, CultureInfo userCulture)
	{
		if (formatCulture == null)
		{
			throw new ArgumentNullException("formatCulture");
		}
		if (userCulture == null)
		{
			throw new ArgumentNullException("userCulture");
		}
		DateTimeFormatInfo dateTimeFormat = formatCulture.DateTimeFormat;
		NumberFormatInfo numberFormat = formatCulture.NumberFormat;
		ShortTimeFormat = dateTimeFormat.ShortTimePattern;
		ShortTimeZero = dateTimeFormat.ShortTimePattern;
		LongTimePattern = dateTimeFormat.LongTimePattern;
		ShortDatePattern = dateTimeFormat.ShortDatePattern;
		TimeAMDesignator = dateTimeFormat.AMDesignator;
		TimePMDesignator = dateTimeFormat.PMDesignator;
		TimeSeparator = dateTimeFormat.TimeSeparator;
		DateSeparator = dateTimeFormat.DateSeparator;
		DecimalSeparator = numberFormat.NumberDecimalSeparator;
		GroupSeparator = numberFormat.NumberGroupSeparator;
		CurrencyDecimalSeparator = numberFormat.CurrencyDecimalSeparator;
		CurrencyGroupSeparator = numberFormat.CurrencyGroupSeparator;
		LCID = userCulture.LCID;
		WindowsLCID = formatCulture.LCID;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ClientSettings))
		{
			return false;
		}
		return Equals((ClientSettings)obj);
	}

	public bool Equals(ClientSettings other)
	{
		if (ShortTimeFormat.Equals(other.ShortTimeFormat) && ShortTimeZero.Equals(other.ShortTimeZero) && LongTimePattern.Equals(other.LongTimePattern) && ShortDatePattern.Equals(other.ShortDatePattern) && TimeAMDesignator.Equals(other.TimeAMDesignator) && TimePMDesignator.Equals(other.TimePMDesignator) && TimeSeparator.Equals(other.TimeSeparator) && DateSeparator.Equals(other.DateSeparator) && DecimalSeparator.Equals(other.DecimalSeparator) && GroupSeparator.Equals(other.GroupSeparator) && CurrencyDecimalSeparator.Equals(other.CurrencyDecimalSeparator) && CurrencyGroupSeparator.Equals(other.CurrencyGroupSeparator) && LCID.Equals(other.LCID))
		{
			return WindowsLCID.Equals(other.WindowsLCID);
		}
		return false;
	}

	public static bool operator ==(ClientSettings settings1, ClientSettings settings2)
	{
		return settings1.Equals(settings2);
	}

	public static bool operator !=(ClientSettings settings1, ClientSettings settings2)
	{
		return !(settings1 == settings2);
	}

	public override int GetHashCode()
	{
		return ShortTimeFormat.GetHashCode() ^ ShortTimeZero.GetHashCode() ^ LongTimePattern.GetHashCode() ^ ShortDatePattern.GetHashCode() ^ TimeAMDesignator.GetHashCode() ^ TimePMDesignator.GetHashCode() ^ TimeSeparator.GetHashCode() ^ DateSeparator.GetHashCode() ^ DecimalSeparator.GetHashCode() ^ GroupSeparator.GetHashCode() ^ CurrencyDecimalSeparator.GetHashCode() ^ CurrencyGroupSeparator.GetHashCode() ^ LCID.GetHashCode() ^ WindowsLCID.GetHashCode();
	}
}
