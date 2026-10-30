using System;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class ModelChangeOptions : ReadWriteModelToolOptions
{
	private DateTime dateTimeNow;

	public bool UseCodeCompression { get; set; }

	public bool InlineCodeConflicts { get; set; }

	public DateTimePropertyAction DateTimeProperty { get; set; }

	public ModifiedPropertyAction ModifiedProperty { get; set; }

	public VersionListPropertyAction VersionListProperty { get; set; }

	public DocumentationConflictAction DocumentationConflict { get; set; }

	public string TimeNowString { get; set; }

	public string DateNowString { get; set; }

	public bool Strict { get; set; }

	public bool DisableCommentOut { get; set; }

	public DateTime DateTimeNow
	{
		get
		{
			return dateTimeNow;
		}
		set
		{
			dateTimeNow = value;
			UpdateDateTimeStrings();
		}
	}

	public bool SkipOriginalValueCheck { get; set; }

	public bool AutoAlignControlIndent { get; set; }

	public bool IgnoreDocumentation { get; set; }

	public bool ExportToNewSyntax { get; set; }

	public ModelChangeOptions()
	{
		RunInParallel = true;
		IncludeInvalidObjects = true;
		DateTimeNow = DateTime.Now;
	}

	internal ComputeChangesOptions GetComputeChangesOptions()
	{
		return new ComputeChangesOptions
		{
			DevBaseCompatibility = DevBaseCompatibility,
			UseCodeCompression = (UseCodeCompression && !DevBaseCompatibility),
			IgnoreDocumentationChanges = IgnoreDocumentation,
			ExportToNewSyntax = ExportToNewSyntax
		};
	}

	internal ApplyChangesOptions GetApplyChangesOptions()
	{
		return new ApplyChangesOptions
		{
			DevBaseCompatibility = DevBaseCompatibility,
			InlineCodeConflicts = InlineCodeConflicts,
			DocumentationConflict = DocumentationConflict,
			ConflictSeverityLevel = ((!Strict) ? ConflictSeverityType.Medium : ConflictSeverityType.Low),
			SkipCodeValidation = DisableCommentOut,
			SkipOriginalValueCheck = SkipOriginalValueCheck,
			AutoAlignControlIndent = AutoAlignControlIndent
		};
	}

	internal static string GetTimeTextFormatString()
	{
		DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
		char c = (string.IsNullOrEmpty(dateTimeFormat.TimeSeparator) ? ':' : dateTimeFormat.TimeSeparator[0]);
		if (string.IsNullOrEmpty(dateTimeFormat.ShortTimePattern))
		{
			return "HH" + c + "mm" + c + "ss";
		}
		bool flag = dateTimeFormat.ShortTimePattern.IndexOf("H", StringComparison.Ordinal) != -1;
		bool flag2 = dateTimeFormat.ShortTimePattern.IndexOf("HH", StringComparison.OrdinalIgnoreCase) != -1;
		if (flag)
		{
			return (flag2 ? "HH" : "H") + c + "mm" + c + "ss";
		}
		return (flag2 ? "hh" : "h") + c + "mm" + c + "ss tt";
	}

	internal static string GetDateTextFormatString()
	{
		DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
		string text = dateTimeFormat.ShortDatePattern;
		char value = (string.IsNullOrEmpty(dateTimeFormat.DateSeparator) ? ' ' : dateTimeFormat.DateSeparator[0]);
		if (string.IsNullOrEmpty(text))
		{
			return "dd" + value + "MM" + value + "yy";
		}
		StringBuilder stringBuilder = new StringBuilder(4);
		int length = text.Length;
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = true;
		while (flag4)
		{
			while (num < length)
			{
				string value2 = string.Empty;
				switch (text[num++])
				{
				case 'D':
				case 'd':
					if (!flag)
					{
						flag = true;
						value2 = "dd";
					}
					break;
				case 'M':
				case 'm':
					if (!flag2)
					{
						flag2 = true;
						value2 = "MM";
					}
					break;
				case 'Y':
				case 'y':
					if (!flag3)
					{
						flag3 = true;
						value2 = "yy";
					}
					break;
				}
				if (!string.IsNullOrEmpty(value2))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(value);
					}
					stringBuilder.Append(value2);
				}
			}
			if (flag && flag2 && flag3)
			{
				break;
			}
			flag4 = false;
			text = "DMY";
		}
		return stringBuilder.ToString();
	}

	private void UpdateDateTimeStrings()
	{
		TimeNowString = DateTimeNow.ToString(GetTimeTextFormatString(), CultureInfo.CurrentCulture);
		DateNowString = DateTimeNow.ToString(GetDateTextFormatString(), CultureInfo.CurrentCulture);
	}
}
