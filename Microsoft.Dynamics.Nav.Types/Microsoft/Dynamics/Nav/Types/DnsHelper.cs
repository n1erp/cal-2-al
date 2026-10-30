using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Dynamics.Nav.Types;

public static class DnsHelper
{
	private static class NativeMethods
	{
		internal enum ComputerNameFormat
		{
			NetBIOS,
			DnsHostName,
			Dns,
			DnsFullyQualified,
			PhysicalNetBIOS,
			PhysicalDnsHostName,
			PhysicalDnsDomain,
			PhysicalDnsFullyQualified
		}

		[DllImport("kernel32.dll", EntryPoint = "GetComputerNameExW", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetComputerNameEx(ComputerNameFormat format, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer, ref int size);
	}

	private static readonly object syncObject = new object();

	private static readonly string machineName = GetComputerName(NativeMethods.ComputerNameFormat.DnsFullyQualified);

	private static readonly string hostName = GetComputerName(NativeMethods.ComputerNameFormat.DnsHostName);

	private static readonly string dnsDomainName = GetComputerName(NativeMethods.ComputerNameFormat.Dns);

	private static string dnsResolvedHostName;

	private const int ErrorMoreData = 234;

	public static string MachineName => machineName;

	public static string HostName => hostName;

	public static string DnsDomainName => dnsDomainName;

	private static string DnsResolvedHostName
	{
		get
		{
			if (dnsResolvedHostName == null)
			{
				lock (syncObject)
				{
					if (dnsResolvedHostName == null)
					{
						dnsResolvedHostName = Dns.GetHostEntry(HostName).HostName;
					}
				}
			}
			return dnsResolvedHostName;
		}
	}

	public static bool IsLocalhost(string computerName)
	{
		bool flag = false;
		if (!string.IsNullOrEmpty(computerName))
		{
			flag = "localhost".Equals(computerName, StringComparison.OrdinalIgnoreCase) || MachineName.Equals(computerName, StringComparison.OrdinalIgnoreCase);
			if (!flag)
			{
				flag = HostName.Equals(computerName, StringComparison.OrdinalIgnoreCase);
				if (!flag)
				{
					try
					{
						string value = Dns.GetHostEntry(computerName).HostName;
						flag = DnsResolvedHostName.Equals(value, StringComparison.OrdinalIgnoreCase);
					}
					catch (SocketException)
					{
					}
					catch (ArgumentException)
					{
					}
				}
			}
		}
		return flag;
	}

	private static string GetComputerName(NativeMethods.ComputerNameFormat type)
	{
		int size = 0;
		if (!NativeMethods.GetComputerNameEx(type, null, ref size))
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 234)
			{
				throw new Win32Exception(lastWin32Error);
			}
		}
		if (size < 0)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "GetComputerName returned an invalid length: {0}.", size));
		}
		StringBuilder stringBuilder = new StringBuilder(size);
		if (!NativeMethods.GetComputerNameEx(type, stringBuilder, ref size))
		{
			throw new Win32Exception(Marshal.GetLastWin32Error());
		}
		return stringBuilder.ToString();
	}
}
