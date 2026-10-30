using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Common.Printing;

public class NavPrinterSettings
{
	internal class ByPaperSize : IComparer<PaperSize>
	{
		public int Compare(PaperSize x, PaperSize y)
		{
			if (x.Height < y.Height)
			{
				return -1;
			}
			if (x.Height == y.Height)
			{
				if (x.Width < y.Width)
				{
					return -1;
				}
				if (x.Width == y.Width)
				{
					return 0;
				}
			}
			return 1;
		}
	}

	private Dictionary<ulong, Tuple<PaperSize, bool>> mappings = new Dictionary<ulong, Tuple<PaperSize, bool>>();

	private SortedSet<PaperSize> validPaperSizes = new SortedSet<PaperSize>(new ByPaperSize());

	public string PrinterName { get; private set; }

	public IEnumerable<PaperSize> PaperSizes => validPaperSizes;

	public PaperSize PaperSize { get; private set; }

	public bool Landscape { get; private set; }

	public short DefaultCopies { get; private set; }

	public NavPrinterSettings(string printerName)
	{
		PrinterName = printerName;
		try
		{
			PrinterSettings printerSettings = new PrinterSettings
			{
				PrinterName = printerName
			};
			if (!ReportRenderer.VerifyIfPrinterExist(printerSettings.PrinterName) || !printerSettings.IsValid)
			{
				throw new NavReportPrintingException(Lang.PrinterNotInstalled);
			}
			PageSettings defaultPageSettings = printerSettings.DefaultPageSettings;
			PrinterName = printerSettings.PrinterName;
			foreach (PaperSize paperSize in printerSettings.PaperSizes)
			{
				validPaperSizes.Add(paperSize);
			}
			PaperSize = defaultPageSettings.PaperSize;
			Landscape = defaultPageSettings.Landscape;
			DefaultCopies = printerSettings.Copies;
		}
		catch (Win32Exception innerException)
		{
			string message = string.Format(CultureInfo.CurrentCulture, Lang.PrinterSettingsError, printerName);
			EventLogWriter.Write(message, EventLogEntryType.Warning, null, 0, null, 0);
			throw new NavReportPrintingException(message, innerException);
		}
		catch (NullReferenceException innerException2)
		{
			string message2 = string.Format(CultureInfo.CurrentCulture, Lang.PrinterSettingsError, printerName);
			EventLogWriter.Write(message2, EventLogEntryType.Warning, null, 0, null, 0);
			throw new NavReportPrintingException(message2, innerException2);
		}
	}

	internal Tuple<PaperSize, bool> BestPaperSizeMatch(int width, int height, bool calculateBestPaperSize)
	{
		Tuple<PaperSize, bool> value = null;
		ulong key = (ulong)(((long)width << 32) | (uint)height);
		lock (mappings)
		{
			if (mappings.TryGetValue(key, out value))
			{
				return value;
			}
		}
		int num = int.MaxValue;
		PaperSize paperSize = (Landscape ? GetValidPaperSize(height, width) : GetValidPaperSize(width, height));
		if (paperSize != null)
		{
			num = paperSize.Height - height + paperSize.Width - width;
			if (num == 0)
			{
				return StoreAndReturn(key, Tuple.Create(paperSize, Landscape));
			}
		}
		PaperSize paperSize2 = ((!Landscape) ? GetValidPaperSize(height, width) : GetValidPaperSize(width, height));
		if (paperSize2 == null)
		{
			if ((paperSize != null) & calculateBestPaperSize)
			{
				return StoreAndReturn(key, Tuple.Create(paperSize, Landscape));
			}
			return null;
		}
		int num2 = paperSize2.Height - height + paperSize2.Width - width;
		if (num2 == 0 || paperSize == null)
		{
			return StoreAndReturn(key, Tuple.Create(paperSize2, !Landscape));
		}
		if (!calculateBestPaperSize)
		{
			return null;
		}
		return StoreAndReturn(key, (num < num2) ? Tuple.Create(paperSize, item2: false) : Tuple.Create(paperSize2, item2: true));
	}

	private Tuple<PaperSize, bool> StoreAndReturn(ulong key, Tuple<PaperSize, bool> selectedPaper)
	{
		lock (mappings)
		{
			if (!mappings.ContainsKey(key))
			{
				mappings.Add(key, selectedPaper);
			}
		}
		return selectedPaper;
	}

	private PaperSize GetValidPaperSize(int width, int height)
	{
		foreach (PaperSize validPaperSize in validPaperSizes)
		{
			if ((validPaperSize.Height >= height || Math.Abs(height - validPaperSize.Height) <= 1) && (validPaperSize.Width >= width || Math.Abs(width - validPaperSize.Width) <= 1))
			{
				return validPaperSize;
			}
		}
		return null;
	}
}
