using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class SettingsParser
{
	private readonly List<string> elementChanges = new List<string> { "Table", "Field", "Key" };

	private readonly List<string> propertyChanges = new List<string> { "TableProperty", "FieldProperty", "KeyProperty" };

	private AppObjectChangesFilter filter;

	internal void LoadSettings(AppObjectChangesFilter changesFilter, string[] settingsBody)
	{
		filter = changesFilter;
		LoadSettings(settingsBody);
	}

	private void LoadSettings(string[] settingsBody)
	{
		string settingsBlock = string.Empty;
		string settingName = string.Empty;
		for (int i = 0; i < settingsBody.Length; i++)
		{
			string text = settingsBody[i].Trim();
			if (!text.StartsWith("#", StringComparison.Ordinal) && !string.IsNullOrEmpty(text))
			{
				if (elementChanges.Contains(text) || propertyChanges.Contains(text))
				{
					settingsBlock = text;
				}
				else if (text.EndsWith(":", StringComparison.Ordinal))
				{
					settingName = text.TrimEnd(':');
				}
				else
				{
					AddSettingExpression(text, settingsBlock, settingName);
				}
			}
		}
	}

	private void AddSettingExpression(string line, string settingsBlock, string settingName)
	{
		if (elementChanges.Contains(settingsBlock))
		{
			filter.AddElementSetting(settingsBlock, settingName, line);
		}
		else
		{
			filter.AddPropertySetting(settingsBlock, settingName, line);
		}
	}
}
