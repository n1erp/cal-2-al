using System;

namespace Microsoft.Dynamics.Nav.Types;

public static class WindowsLanguageFields
{
	public const int LanguageId = 1;

	public const int PrimaryLanguageId = 2;

	public const int Name = 10;

	public const int AbbreviatedName = 11;

	public const int Enabled = 20;

	public const int GloballyEnabled = 21;

	public const int FormEnabled = 22;

	public const int ReportEnabled = 23;

	public const int DataportEnabled = 24;

	public const int XMLportEnabled = 25;

	public const int PageEnabled = 26;

	public const int PrimaryCodePage = 30;

	[Obsolete("This value has been deprecated.  Please use LocalizationExist.")]
	public const int STXFileExist = 31;

	[Obsolete("This value has been deprecated.  Please use LocalizationExist.")]
	public const int ETXFileExist = 32;

	public const int HelpFileExist = 33;

	public const int LocalizationExist = 34;
}
