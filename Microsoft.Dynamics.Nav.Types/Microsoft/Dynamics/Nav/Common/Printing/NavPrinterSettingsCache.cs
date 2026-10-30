using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Common.Printing;

public static class NavPrinterSettingsCache
{
	private static readonly Dictionary<string, Lazy<NavPrinterSettings>> printerSettingsCache = new Dictionary<string, Lazy<NavPrinterSettings>>();

	public static bool TryGetValue(string printerName, out NavPrinterSettings printerSettings)
	{
		printerSettings = null;
		if (string.IsNullOrWhiteSpace(printerName))
		{
			printerName = string.Empty;
		}
		Lazy<NavPrinterSettings> value = null;
		lock (printerSettingsCache)
		{
			if (!printerSettingsCache.TryGetValue(printerName, out value))
			{
				string realPrinterName = printerName;
				if (string.IsNullOrWhiteSpace(printerName))
				{
					realPrinterName = PrinterHelper.NativeMethods.GetDefaultPrinter();
				}
				value = new Lazy<NavPrinterSettings>(() => new NavPrinterSettings(realPrinterName));
				printerSettingsCache.Add(printerName, value);
				if (string.IsNullOrWhiteSpace(printerName) && !string.IsNullOrWhiteSpace(realPrinterName) && !printerSettingsCache.ContainsKey(realPrinterName))
				{
					printerSettingsCache.Add(realPrinterName, value);
				}
			}
		}
		try
		{
			printerSettings = value.Value;
		}
		catch (NavReportPrintingException)
		{
			return false;
		}
		return true;
	}
}
