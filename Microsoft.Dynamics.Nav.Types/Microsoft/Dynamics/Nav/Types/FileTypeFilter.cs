using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types;

public class FileTypeFilter : IEquatable<FileTypeFilter>
{
	private enum ActiveFilterType
	{
		AllowedFileTypes,
		ProhibitedFileTypes,
		None
	}

	private Regex filteredFileTypesRegEx;

	private string filterValue;

	private bool emptyFileTypeInTheFilter;

	private static readonly char[] InvalidFileTypeChars = GetInvalidFileTypeChars();

	private static readonly Dictionary<char, string> CharReplacements = new Dictionary<char, string>
	{
		{ '[', "\\[" },
		{ ']', "\\]" },
		{
			' ',
			string.Empty
		}
	};

	private const string telemetryMessage = "Registered {0} attempt to upload the file with the extension: {1}. Active server setting: {2}";

	public const string AllFileTypes = "*";

	public const string DefaultProhibitedFileTypes = "ade;adp;asp;bas;bat;chm;cmd;com;cpl;csh;dll;exe;fxp;gadget;hlp;hta;inf;ins;isp;its;js;jse;ksh;lnk;mad;maf;mag;mam;maq;mar;mas;mat;mau;mav;maw;mda;mdb;mde;mdt;mdw;mdz;msc;msi;msp;mst;ops;pcd;pif;prf;prg;pst;reg;scf;scr;sct;shb;shs;url;vb;vbe;vbs;vsmacros;vss;vst;vsw;ws;wsc;wsf;wsh";

	public const string DefaultAllowedFileTypes = "";

	public string Value
	{
		get
		{
			return filterValue;
		}
		private set
		{
			filterValue = value;
			if (!(filterValue != "*"))
			{
				return;
			}
			string[] array = filterValue.Split(';');
			StringBuilder stringBuilder = new StringBuilder(filterValue.Length);
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text.Length == 0)
				{
					emptyFileTypeInTheFilter = true;
				}
				else
				{
					stringBuilder.Append(((stringBuilder.Length == 0) ? "^" : "|^") + ReplaceAndEscape(text) + "$");
				}
			}
			if (stringBuilder.Length > 0)
			{
				filteredFileTypesRegEx = new Regex("(" + stringBuilder?.ToString() + ")+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}
			else
			{
				filteredFileTypesRegEx = new Regex("^$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			}
		}
	}

	public static bool IsFileTypeAllowedByConfiguration(string fileName)
	{
		return IsFileTypeAllowedByConfiguration(fileName, ServerUserSettings.Instance.ClientServicesAllowedFileTypes, ServerUserSettings.Instance.ClientServicesProhibitedFileTypes);
	}

	public static bool IsFileTypeAllowedByConfiguration(string fileName, FileTypeFilter allowedFilesFilter, FileTypeFilter prohibitedFilesFilter)
	{
		string extension = Path.GetExtension(fileName);
		ActiveFilterType activeFilterType;
		bool flag;
		if (allowedFilesFilter != null && !string.IsNullOrEmpty(allowedFilesFilter.Value))
		{
			activeFilterType = ActiveFilterType.AllowedFileTypes;
			flag = allowedFilesFilter.IsFileTypeAllowed(fileName);
		}
		else if (prohibitedFilesFilter == null)
		{
			activeFilterType = ActiveFilterType.None;
			flag = true;
		}
		else
		{
			activeFilterType = ActiveFilterType.ProhibitedFileTypes;
			flag = !prohibitedFilesFilter.IsFileTypeAllowed(fileName);
		}
		string message = BuildTelemetryMessage(flag, extension, activeFilterType);
		NavDiagnostics.GetMostSpecificInstance?.SendTraceTag("000085Q", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, message);
		return flag;
	}

	private static string BuildTelemetryMessage(bool isAllowed, string extension, ActiveFilterType activeFilterType)
	{
		string arg = (isAllowed ? "allowed" : "prohibited");
		_ = string.Empty;
		return string.Format(CultureInfo.InvariantCulture, "Registered {0} attempt to upload the file with the extension: {1}. Active server setting: {2}", arg, extension, activeFilterType switch
		{
			ActiveFilterType.AllowedFileTypes => (object)"ClientServicesAllowedFileTypes", 
			ActiveFilterType.ProhibitedFileTypes => "ClientServicesProhibitedFileTypes", 
			_ => "None", 
		});
	}

	private static string ReplaceAndEscape(string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < text.Length; i++)
		{
			char key = text[i];
			stringBuilder.Append(CharReplacements.TryGetValue(key, out var value) ? value : key.ToString());
		}
		return stringBuilder.ToString();
	}

	private FileTypeFilter()
	{
	}

	public FileTypeFilter(string settingValue)
	{
		Value = Parse(settingValue, string.Empty).Value;
	}

	public override string ToString()
	{
		return Value;
	}

	public static bool TryParse(string settingValue, out FileTypeFilter result)
	{
		if (settingValue == null)
		{
			result = null;
			return false;
		}
		settingValue = Trim(settingValue);
		if (IsAllOrNorthingSetting(settingValue))
		{
			result = new FileTypeFilter
			{
				Value = settingValue
			};
			return true;
		}
		if (!IsValidSetting(settingValue))
		{
			result = null;
			return false;
		}
		result = new FileTypeFilter
		{
			Value = settingValue
		};
		return true;
	}

	public static FileTypeFilter Parse(string settingValue, string settingName)
	{
		if (settingValue == null)
		{
			throw new ArgumentNullException("settingValue");
		}
		settingValue = Trim(settingValue);
		if (IsAllOrNorthingSetting(settingValue))
		{
			return new FileTypeFilter
			{
				Value = settingValue
			};
		}
		if (!IsValidSetting(settingValue))
		{
			throw new NavConfigurationException(string.Format(CultureInfo.CurrentCulture, Lang.ConfigurationFileTypeFilterInvalid, settingName));
		}
		return new FileTypeFilter
		{
			Value = settingValue
		};
	}

	private static string Trim(string settingValue)
	{
		string text = settingValue.Trim();
		if (text.Length == 0)
		{
			return text;
		}
		if (text[text.Length - 1] != ';')
		{
			return text;
		}
		return text.Substring(0, text.Length - 1);
	}

	private bool IsFileTypeAllowed(string filePath)
	{
		if (Value == "*")
		{
			return true;
		}
		if (Value.Length == 0)
		{
			return false;
		}
		string extension = Path.GetExtension(filePath);
		if (string.IsNullOrEmpty(extension))
		{
			return emptyFileTypeInTheFilter;
		}
		extension = extension.Replace(".", string.Empty);
		Match match = filteredFileTypesRegEx.Match(extension);
		if (match.Success)
		{
			return true;
		}
		return string.Compare(match.Groups[0].ToString(), extension, ignoreCase: true, CultureInfo.CurrentCulture) == 0;
	}

	private static bool IsValidSetting(string settingValue)
	{
		if (Regex.IsMatch(settingValue, "(?:([^;\\*]*);?)+([^;\\*]+)*"))
		{
			return settingValue.IndexOfAny(InvalidFileTypeChars) == -1;
		}
		return false;
	}

	private static bool IsAllOrNorthingSetting(string settingValue)
	{
		if (settingValue.Length != 0)
		{
			return settingValue == "*";
		}
		return true;
	}

	private static char[] GetInvalidFileTypeChars()
	{
		char[] array = new char[Path.GetInvalidFileNameChars().Length + 1];
		array[0] = '.';
		Path.GetInvalidFileNameChars().CopyTo(array, 1);
		return array;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		FileTypeFilter fileTypeFilter = obj as FileTypeFilter;
		if (fileTypeFilter == null)
		{
			return false;
		}
		return Value.Equals(fileTypeFilter.Value);
	}

	public bool Equals(FileTypeFilter other)
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

	public static bool operator ==(FileTypeFilter first, FileTypeFilter second)
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

	public static bool operator !=(FileTypeFilter first, FileTypeFilter second)
	{
		return !(first == second);
	}
}
