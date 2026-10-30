using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types;

public class ChunkSize : IEquatable<ChunkSize>
{
	private const string NumberFormatSpecifier = "N";

	private int chunkSise;

	private string settingName;

	private const int MaxChunkSizeKB = 80;

	private const int MinChunkSizeKB = 4;

	private const int BytesMultiplier = 1024;

	public const int ChunkingDisabled = 0;

	public const int Default = 28;

	public int Value
	{
		get
		{
			return chunkSise;
		}
		set
		{
			if (value != 0)
			{
				ConfigSettings.ValidateChunkSize(value, 4, 80, 0, settingName);
			}
			chunkSise = value;
		}
	}

	public int ValueInBytes => Value * 1024;

	private ChunkSize()
	{
	}

	public ChunkSize(string settingValue)
		: this(settingValue, string.Empty)
	{
		chunkSise = Parse(settingValue, string.Empty).Value;
	}

	public ChunkSize(string settingValue, string settingName)
	{
		chunkSise = Parse(settingValue, string.Empty).Value;
		this.settingName = settingName;
	}

	public static ChunkSize Parse(string settingValue, string settingName)
	{
		int value;
		try
		{
			value = int.Parse(settingValue, CultureInfo.CurrentCulture);
		}
		catch (OverflowException exception)
		{
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
			numberFormatInfo.NumberDecimalDigits = 0;
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationSettingOutOfRange, settingName, 4.ToString("N", numberFormatInfo), 80.ToString("N", numberFormatInfo)), exception);
		}
		catch (FormatException exception2)
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToParseConfigSetting, settingName, typeof(int).Name), exception2);
		}
		return new ChunkSize
		{
			settingName = settingName,
			Value = value
		};
	}

	public static bool TryParse(string settingValue, out ChunkSize result)
	{
		result = null;
		if (settingValue == null)
		{
			return false;
		}
		try
		{
			result = Parse(settingValue, string.Empty);
		}
		catch (NavConfigurationException)
		{
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		return chunkSise.ToString(CultureInfo.CurrentCulture);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		ChunkSize chunkSize = obj as ChunkSize;
		if (chunkSize == null)
		{
			return false;
		}
		return Value.Equals(chunkSize.Value);
	}

	public bool Equals(ChunkSize other)
	{
		if (other == null)
		{
			return false;
		}
		return Value.Equals(other.Value);
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(ChunkSize first, ChunkSize second)
	{
		if ((object)first == second)
		{
			return true;
		}
		if ((object)first == null || (object)second == null)
		{
			return false;
		}
		return first.Value.Equals(second.Value);
	}

	public static bool operator !=(ChunkSize first, ChunkSize second)
	{
		return !(first == second);
	}
}
