using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Configuration;

public class ValidAudiences
{
	private const char Asterisk = '*';

	private const char AudienceSeparator = ';';

	private string validAudiencesValue;

	private readonly List<string> validAudiencesList = new List<string>();

	public string Value
	{
		get
		{
			return validAudiencesValue;
		}
		private set
		{
			validAudiencesValue = value;
			CalculateAudiences();
		}
	}

	public IList<string> ValidAudiencesList => validAudiencesList.AsReadOnly();

	public ValidAudiences(string settingValue)
	{
		Value = settingValue;
	}

	public static ValidAudiences Parse(string settingValue, string settingName)
	{
		if (settingValue == null)
		{
			throw new ArgumentNullException("settingValue");
		}
		try
		{
			return new ValidAudiences(settingValue);
		}
		catch (UriFormatException exception)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationFileTypeFilterInvalid, settingName), exception);
		}
	}

	private void CalculateAudiences()
	{
		if (!string.IsNullOrEmpty(Value))
		{
			(from audience in Value.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries)
				where !string.IsNullOrWhiteSpace(audience)
				select audience).ForEach((string audience) =>
			{
				string text = audience.Trim();
				ThrowIfInvalid(text);
				validAudiencesList.Add(text);
			});
		}
	}

	private static void ThrowIfInvalid(string audience)
	{
		if (Guid.TryParse(audience, out var _) || Uri.TryCreate(audience, UriKind.RelativeOrAbsolute, out var _))
		{
			return;
		}
		throw new UriFormatException(string.Format(CultureInfo.InvariantCulture, "The format of the audience URI or GUID is not valid: {0}", audience));
	}

	public override string ToString()
	{
		return Value;
	}
}
