using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Dynamics.Nav.Types;

public static class PrinterHelper
{
	internal static class NativeMethods
	{
		[Flags]
		public enum PrinterEnumFlags
		{
			PRINTER_ENUM_DEFAULT = 1,
			PRINTER_ENUM_LOCAL = 2,
			PRINTER_ENUM_CONNECTIONS = 4,
			PRINTER_ENUM_FAVORITE = PRINTER_ENUM_CONNECTIONS,
			PRINTER_ENUM_NAME = 8,
			PRINTER_ENUM_REMOTE = 0x10,
			PRINTER_ENUM_SHARED = 0x20,
			PRINTER_ENUM_NETWORK = 0x40,
			PRINTER_ENUM_EXPAND = 0x4000,
			PRINTER_ENUM_CONTAINER = 0x8000,
			PRINTER_ENUM_ICONMASK = 0xFF0000,
			PRINTER_ENUM_ICON1 = 0x10000,
			PRINTER_ENUM_ICON2 = 0x20000,
			PRINTER_ENUM_ICON3 = 0x40000,
			PRINTER_ENUM_ICON4 = 0x80000,
			PRINTER_ENUM_ICON5 = 0x100000,
			PRINTER_ENUM_ICON6 = 0x200000,
			PRINTER_ENUM_ICON7 = 0x400000,
			PRINTER_ENUM_ICON8 = 0x800000,
			PRINTER_ENUM_HIDE = 0x1000000
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct PRINTER_INFO_2
		{
			[MarshalAs(UnmanagedType.LPTStr)]
			public string pServerName;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pPrinterName;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pShareName;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pPortName;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pDriverName;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pComment;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pLocation;

			public IntPtr pDevMode;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pSepFile;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pPrintProcessor;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pDatatype;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pParameters;

			public IntPtr pSecurityDescriptor;

			public uint Attributes;

			public uint Priority;

			public uint DefaultPriority;

			public uint StartTime;

			public uint UntilTime;

			public uint Status;

			public uint cJobs;

			public uint AveragePPM;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct PRINTER_INFO_4
		{
			[MarshalAs(UnmanagedType.LPTStr)]
			public string pPrinterName;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string pServerName;

			public uint Attributes;
		}

		private const PrinterEnumFlags Flags = PrinterEnumFlags.PRINTER_ENUM_LOCAL | PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS;

		private const int ERROR_INSUFFICIENT_BUFFER = 122;

		[DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int pcchBuffer);

		internal static string GetDefaultPrinter()
		{
			int pcchBuffer = 0;
			if (GetDefaultPrinter(null, ref pcchBuffer))
			{
				return string.Empty;
			}
			if (Marshal.GetLastWin32Error() == 122)
			{
				StringBuilder stringBuilder = new StringBuilder(pcchBuffer);
				if (GetDefaultPrinter(stringBuilder, ref pcchBuffer))
				{
					return stringBuilder.ToString();
				}
			}
			return string.Empty;
		}

		[DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool EnumPrinters(PrinterEnumFlags Flags, [MarshalAs(UnmanagedType.LPWStr)] string Name, uint Level, IntPtr pPrinterEnum, uint cbBuf, ref uint pcbNeeded, ref uint pcReturned);

		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPWStr)] string pPrinterName, out IntPtr phPrinter, out IntPtr pDefault);

		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetPrinter(IntPtr hPrinter, int dwLevel, IntPtr pPrinter, int dwBuf, out int dwNeeded);

		[DllImport("winspool.drv", SetLastError = true)]
		public static extern int ClosePrinter(IntPtr hPrinter);

		public static Printer GetIndividualPrinter(string printerName, bool isDefaultPrinter)
		{
			Printer result = new Printer
			{
				Name = printerName
			};
			if (OpenPrinter(printerName, out var phPrinter, out var _))
			{
				GetPrinter(phPrinter, 2, IntPtr.Zero, 0, out var dwNeeded);
				try
				{
					if (dwNeeded > 0)
					{
						IntPtr intPtr = Marshal.AllocHGlobal(dwNeeded);
						try
						{
							if (GetPrinter(phPrinter, 2, intPtr, dwNeeded, out dwNeeded))
							{
								PRINTER_INFO_2 pRINTER_INFO_ = (PRINTER_INFO_2)Marshal.PtrToStructure(intPtr, typeof(PRINTER_INFO_2));
								result.Driver = pRINTER_INFO_.pDriverName;
								result.Device = pRINTER_INFO_.pPortName;
								result.DefaultPrinter = isDefaultPrinter;
								result.Comment = pRINTER_INFO_.pComment;
								result.Location = pRINTER_INFO_.pLocation;
							}
						}
						finally
						{
							Marshal.FreeHGlobal(intPtr);
						}
					}
				}
				finally
				{
					ClosePrinter(phPrinter);
				}
			}
			return result;
		}

		public static List<Printer> EnumPrintersLevel2(string defaultPrinter)
		{
			List<Printer> list = new List<Printer>();
			uint pcbNeeded = 0u;
			uint pcReturned = 0u;
			if (EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_LOCAL | PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS, null, 2u, IntPtr.Zero, 0u, ref pcbNeeded, ref pcReturned))
			{
				return list;
			}
			if (Marshal.GetLastWin32Error() == 122)
			{
				IntPtr intPtr = Marshal.AllocHGlobal((int)pcbNeeded);
				try
				{
					if (EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_LOCAL | PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS, null, 2u, intPtr, pcbNeeded, ref pcbNeeded, ref pcReturned))
					{
						long num = ((IntPtr.Size == 4) ? intPtr.ToInt32() : intPtr.ToInt64());
						Type typeFromHandle = typeof(PRINTER_INFO_2);
						int num2 = Marshal.SizeOf(typeFromHandle);
						for (int i = 0; i < pcReturned; i++)
						{
							PRINTER_INFO_2 pRINTER_INFO_ = (PRINTER_INFO_2)Marshal.PtrToStructure(new IntPtr(num), typeFromHandle);
							list.Add(new Printer
							{
								Name = pRINTER_INFO_.pPrinterName,
								Driver = pRINTER_INFO_.pDriverName,
								Device = pRINTER_INFO_.pPortName,
								Comment = pRINTER_INFO_.pComment,
								Location = pRINTER_INFO_.pLocation,
								DefaultPrinter = (pRINTER_INFO_.pPrinterName == defaultPrinter)
							});
							num += num2;
						}
					}
				}
				finally
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
			return list;
		}

		public static PRINTER_INFO_4[] EnumPrintersLevel4()
		{
			uint pcbNeeded = 0u;
			uint pcReturned = 0u;
			if (EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_LOCAL | PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS, null, 4u, IntPtr.Zero, 0u, ref pcbNeeded, ref pcReturned))
			{
				return new PRINTER_INFO_4[0];
			}
			if (Marshal.GetLastWin32Error() == 122)
			{
				IntPtr intPtr = Marshal.AllocHGlobal((int)pcbNeeded);
				try
				{
					if (EnumPrinters(PrinterEnumFlags.PRINTER_ENUM_LOCAL | PrinterEnumFlags.PRINTER_ENUM_CONNECTIONS, null, 4u, intPtr, pcbNeeded, ref pcbNeeded, ref pcReturned))
					{
						PRINTER_INFO_4[] array = new PRINTER_INFO_4[pcReturned];
						long num = ((IntPtr.Size == 4) ? intPtr.ToInt32() : intPtr.ToInt64());
						Type typeFromHandle = typeof(PRINTER_INFO_4);
						int num2 = Marshal.SizeOf(typeFromHandle);
						for (int i = 0; i < pcReturned; i++)
						{
							array[i] = (PRINTER_INFO_4)Marshal.PtrToStructure(new IntPtr(num), typeFromHandle);
							num += num2;
						}
						return array;
					}
				}
				finally
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
			return new PRINTER_INFO_4[0];
		}
	}

	public static Printer[] GetPrinters()
	{
		string defaultPrinter = NativeMethods.GetDefaultPrinter();
		NativeMethods.PRINTER_INFO_4[] array = NativeMethods.EnumPrintersLevel4();
		List<Printer> list = NativeMethods.EnumPrintersLevel2(defaultPrinter);
		NativeMethods.PRINTER_INFO_4[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			NativeMethods.PRINTER_INFO_4 printerInfo = array2[i];
			if (list.All((Printer level2Printer) => level2Printer.Name != printerInfo.pPrinterName))
			{
				list.Add(NativeMethods.GetIndividualPrinter(printerInfo.pPrinterName, printerInfo.pPrinterName == defaultPrinter));
			}
		}
		return list.ToArray();
	}
}
