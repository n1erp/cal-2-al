using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DhcpClient : IDisposable
{
	private bool disposedValue;

	public DhcpClient()
	{
		uint num = NativeMethods.DhcpCApiInitialize(out var _);
		if (num != 0)
		{
			throw new Win32Exception((int)num);
		}
	}

	public IEnumerable<NetworkInterface> GetDhcpInterfaces()
	{
		NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
		foreach (NetworkInterface networkInterface in allNetworkInterfaces)
		{
			if (!networkInterface.Supports(NetworkInterfaceComponent.IPv4))
			{
				continue;
			}
			IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
			if (iPProperties != null)
			{
				IPv4InterfaceProperties iPv4Properties = iPProperties.GetIPv4Properties();
				if (iPv4Properties != null && iPv4Properties.IsDhcpEnabled)
				{
					yield return networkInterface;
				}
			}
		}
	}

	public byte[] DhcpRequestParams(string adapterName, int optionId)
	{
		if (disposedValue)
		{
			throw new ObjectDisposedException("DhcpClient");
		}
		uint pSize = 1024u;
		while (true)
		{
			IntPtr intPtr = Marshal.AllocHGlobal((int)pSize);
			try
			{
				NativeMethods.DHCPCAPI_PARAMS_ARRAY sendParams = new NativeMethods.DHCPCAPI_PARAMS_ARRAY
				{
					nParams = 0u,
					Params = IntPtr.Zero
				};
				NativeMethods.DHCPCAPI_PARAMS structure = new NativeMethods.DHCPCAPI_PARAMS
				{
					Flags = 0u,
					OptionId = (uint)optionId,
					IsVendor = false,
					Data = IntPtr.Zero,
					nBytesData = 0u
				};
				IntPtr intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
				try
				{
					Marshal.StructureToPtr(structure, intPtr2, fDeleteOld: false);
					NativeMethods.DHCPCAPI_PARAMS_ARRAY recdParams = new NativeMethods.DHCPCAPI_PARAMS_ARRAY
					{
						nParams = 1u,
						Params = intPtr2
					};
					NativeMethods.DHCPCAPI_REQUEST flags = NativeMethods.DHCPCAPI_REQUEST.DHCPCAPI_REQUEST_SYNCHRONOUS;
					uint num = NativeMethods.DhcpRequestParams(flags, IntPtr.Zero, adapterName, IntPtr.Zero, sendParams, recdParams, intPtr, ref pSize, null);
					switch (num)
					{
					case 234u:
						pSize *= 2;
						break;
					default:
						throw new Win32Exception((int)num);
					case 0u:
					{
						structure = (NativeMethods.DHCPCAPI_PARAMS)Marshal.PtrToStructure(intPtr2, typeof(NativeMethods.DHCPCAPI_PARAMS));
						if (structure.Data == IntPtr.Zero)
						{
							return null;
						}
						byte[] array = new byte[structure.nBytesData];
						Marshal.Copy(structure.Data, array, 0, (int)structure.nBytesData);
						return array;
					}
					}
				}
				finally
				{
					Marshal.FreeHGlobal(intPtr2);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
			}
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			NativeMethods.DhcpCApiCleanup();
			disposedValue = true;
		}
	}

	~DhcpClient()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
