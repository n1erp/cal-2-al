using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLOptionMetadata
{
	private string optionString;

	private string[] options;

	private static Dictionary<string, NCLOptionMetadata> metadataCache = new Dictionary<string, NCLOptionMetadata>();

	private static NCLOptionMetadata defaultObject;

	private NavOption[] navOptionCache;

	internal virtual string[] Options => options;

	internal virtual int[] OrdinalValues => null;

	public int Count => Options.Length;

	public virtual bool IsEnum => false;

	public virtual string OptionString => optionString;

	public virtual string OptionCaption => optionString;

	public virtual string OptionCaptionML => null;

	public virtual bool HasCaption => false;

	public static NCLOptionMetadata Default
	{
		get
		{
			if (defaultObject == null)
			{
				defaultObject = Create(string.Empty);
			}
			return defaultObject;
		}
	}

	protected NCLOptionMetadata(string option)
	{
		if (option == null)
		{
			throw new ArgumentNullException("option");
		}
		optionString = option;
		options = option.Split(NavOption.OptionDelimiter);
		for (int i = 0; i < options.Length - 1; i++)
		{
			if (string.IsNullOrEmpty(options[i]))
			{
				continue;
			}
			for (int j = i + 1; j < options.Length; j++)
			{
				if (options[i] == options[j])
				{
					throw NavNCLInvalidOptionMetadataException.Create(CultureInfo.CurrentCulture, option, options[i]);
				}
			}
		}
	}

	public virtual string GetOptionFromIndex(int index)
	{
		if (index < 0 || index >= options.Length)
		{
			return index.ToString(CultureInfo.InvariantCulture);
		}
		return options[index];
	}

	public virtual bool IsValidOrdinal(int ordinal)
	{
		if (ordinal >= 0)
		{
			return ordinal < options.Length;
		}
		return false;
	}

	public virtual int GetIndexFromOption(string option)
	{
		int result = StringHelper.FindStringInStringArrayUsingCurrentCulture(options, option);
		if (result != -1)
		{
			return result;
		}
		if (int.TryParse(option, out result))
		{
			return result;
		}
		return -1;
	}

	public virtual string GetCaptionFromIndex(int index)
	{
		return GetOptionFromIndex(index);
	}

	public virtual int GetIndexFromCaption(string caption)
	{
		return GetIndexFromOption(caption);
	}

	protected static bool TryFindCurrentCultureIndex(NCLLookupArray<int, string[]> lcidCaptions, out int index)
	{
		if (lcidCaptions != null)
		{
			if (NavCurrentThread.Session != null)
			{
				int localLanguage = NavCurrentThread.Session.LocalLanguage;
				if (lcidCaptions.TryGetIndexFromKey(localLanguage, out index))
				{
					return true;
				}
				if (lcidCaptions.TryGetIndexFromKey(LanguageHelper.GetPrimaryLanguageId(localLanguage), out index))
				{
					return true;
				}
				if (lcidCaptions.TryGetIndexFromKey(NavCurrentThread.Session.GlobalLanguage, out index))
				{
					return true;
				}
				if (lcidCaptions.TryGetIndexFromKey(LanguageHelper.GetPrimaryLanguageId(NavCurrentThread.Session.GlobalLanguage), out index))
				{
					return true;
				}
				if (localLanguage != NavEnvironment.DefaultLanguage && lcidCaptions.TryGetIndexFromKey(NavEnvironment.DefaultLanguage, out index))
				{
					return true;
				}
			}
			else if (lcidCaptions.TryGetIndexFromKey(NavEnvironment.DefaultLanguage, out index))
			{
				return true;
			}
		}
		index = 0;
		return false;
	}

	public static NCLOptionMetadata Create(string option)
	{
		if (option.Length == 0 && defaultObject != null)
		{
			return defaultObject;
		}
		NCLOptionMetadata value;
		lock (metadataCache)
		{
			if (!metadataCache.TryGetValue(option, out value))
			{
				value = new NCLOptionMetadata(option);
				metadataCache.Add(option, value);
			}
		}
		return value;
	}

	public NavOption CreateNavOption(int value)
	{
		if (navOptionCache == null)
		{
			navOptionCache = new NavOption[Count + 7];
		}
		int num;
		NavOption navOption;
		if (value >= 0 && value < Count)
		{
			navOption = navOptionCache[value];
			if (navOption != null)
			{
				return navOption;
			}
			num = value;
		}
		else
		{
			num = Count + (value & 0x7FFFFFFF) % 7;
			navOption = navOptionCache[num];
			if (navOption != null && navOption.Value == value)
			{
				return navOption;
			}
		}
		navOption = NavOption.CreateBypassCache(this, value);
		navOptionCache[num] = navOption;
		return navOption;
	}
}
