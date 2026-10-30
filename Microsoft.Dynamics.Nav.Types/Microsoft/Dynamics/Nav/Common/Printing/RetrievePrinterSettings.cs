using System.Drawing.Printing;

namespace Microsoft.Dynamics.Nav.Common.Printing;

public delegate bool RetrievePrinterSettings(PrintDocument document, string printerName);
