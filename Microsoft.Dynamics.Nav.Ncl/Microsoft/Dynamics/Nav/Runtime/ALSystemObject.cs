using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSystemObject
{
	public static void ALExportObjects(string fileName, NavRecord objectTableRecord, int format)
	{
		throw new NavMethodNotSupportedException("ExportObjects");
	}

	public static void ALExportObjects(string fileName, NavRecord objectTableRecord)
	{
		throw new NavMethodNotSupportedException("ExportObjects");
	}

	public static void ALImportObjects(string fileName, int format)
	{
		throw new NavMethodNotSupportedException("ImportObjects");
	}

	public static void ALImportObjects(string fileName)
	{
		throw new NavMethodNotSupportedException("ImportObjects");
	}

	public static string ALCaptionClassTranslate(string captionClassText)
	{
		if (string.IsNullOrEmpty(captionClassText))
		{
			return string.Empty;
		}
		return NavCurrentThread.Session.SystemCodeunitFactory.UIHelperTriggers.InvokeCaptionClassTranslate(NavCurrentThread.Session.LocalLanguage, new NavText(captionClassText)).Value;
	}
}
