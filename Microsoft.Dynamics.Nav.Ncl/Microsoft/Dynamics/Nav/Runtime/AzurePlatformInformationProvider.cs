using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Runtime;

public class AzurePlatformInformationProvider : IAzurePlatformInformationProvider
{
	private const int MaxNumberOfRetries = 3;

	private static readonly TimeSpan HttpTimeOut = TimeSpan.FromSeconds(120.0);

	private static readonly TimeSpan InitialWaitTimeBetweenRetries = TimeSpan.FromSeconds(2.0);

	private const string VirtualMachineBusServiceName = "vmbus";

	private const string ComputeUriString = "http://169.254.169.254/metadata/instance?api-version=2017-04-02";

	private readonly Uri computeUri;

	private readonly NavDiagnostics diagnostics;

	public AzurePlatformInformationProvider(NavDiagnostics diagnostics)
		: this(diagnostics, new Uri("http://169.254.169.254/metadata/instance?api-version=2017-04-02"))
	{
	}

	internal AzurePlatformInformationProvider(NavDiagnostics diagnostics, Uri computeUri)
	{
		this.diagnostics = diagnostics ?? throw new ArgumentNullException("diagnostics");
		this.computeUri = computeUri ?? throw new ArgumentNullException("computeUri");
	}

	public bool TryReadAzurePlatformInfo(out AzurePlatformInformation azurePlatformInformation)
	{
		azurePlatformInformation = GetAzurePlatformInfoFromMetadata() ?? GetAzurePlatformInfoFromNetwork();
		return azurePlatformInformation != null;
	}

	internal AzurePlatformInformation GetAzurePlatformInfoFromMetadata()
	{
		int num = 1;
		int num2 = (int)InitialWaitTimeBetweenRetries.TotalSeconds;
		while (true)
		{
			try
			{
				HttpWebRequest httpWebRequest = WebRequest.CreateHttp(computeUri);
				httpWebRequest.Timeout = (int)HttpTimeOut.TotalMilliseconds;
				httpWebRequest.Headers.Add("Metadata", "True");
				using WebResponse webResponse = httpWebRequest.GetResponse();
				return ParseWebResponse(webResponse.GetResponseStream());
			}
			catch (WebException exception)
			{
				diagnostics.SendExceptionTag("00001L3", Category.Telemetry, exception, Verbosity.Normal, DiagnosticParameter.SystemMetadata("Azure information endpoint couldn't be contacted. This is perfectly normal if not running on Azure or on an old VM."));
			}
			catch (JsonException exception2)
			{
				diagnostics.SendExceptionTag("00001L4", Category.Telemetry, exception2, Verbosity.Error, DiagnosticParameter.SystemMetadata("Azure information endpoint returned a request that couldn't be parsed."));
			}
			catch (Exception exception3)
			{
				diagnostics.SendExceptionTag("00001L5", Category.Telemetry, exception3, Verbosity.Error, DiagnosticParameter.SystemMetadata("An unexpected error occured while trying to detect Azure using Web Request. Ignoring Azure detection using web request."));
			}
			num++;
			if (num > 3)
			{
				break;
			}
			Thread.Sleep(num2 * 1000);
			num2 = (int)Math.Pow(num2, num);
		}
		return null;
	}

	internal AzurePlatformInformation GetAzurePlatformInfoFromNetwork()
	{
		try
		{
			ServiceController serviceController = ServiceController.GetDevices().FirstOrDefault((ServiceController sc) => sc.ServiceName == "vmbus");
			if (serviceController != null && serviceController.Status == ServiceControllerStatus.Running)
			{
				using DhcpClient dhcpClient = new DhcpClient();
				foreach (NetworkInterface dhcpInterface in dhcpClient.GetDhcpInterfaces())
				{
					try
					{
						if (IsAzureNetworkInterface(dhcpInterface, dhcpClient))
						{
							return new AzurePlatformInformation
							{
								Name = serviceController.MachineName
							};
						}
					}
					catch (Win32Exception exception)
					{
						diagnostics.SendExceptionTag("00001L6", Category.Telemetry, exception, Verbosity.Normal, DiagnosticParameter.SystemMetadata("Couldn't retrieve DHCP information for interface " + dhcpInterface.Id));
					}
				}
			}
		}
		catch (Win32Exception exception2)
		{
			diagnostics.SendExceptionTag("00001L7", Category.Telemetry, exception2, Verbosity.Warning, DiagnosticParameter.SystemMetadata("Couldn't initialize Win32 DHCP interface. Ignoring Azure detection using DHCP."));
		}
		catch (Exception exception3)
		{
			diagnostics.SendExceptionTag("00001L8", Category.Telemetry, exception3, Verbosity.Warning, DiagnosticParameter.SystemMetadata("An unexpected error occured while trying to detect Azure using DHCP. Ignoring Azure detection."));
		}
		return null;
	}

	internal static AzurePlatformInformation ParseWebResponse(Stream stream)
	{
		JsonSerializer jsonSerializer = new JsonSerializer();
		using StreamReader reader = new StreamReader(stream);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		return jsonSerializer.Deserialize<AzureMetadataInformation>(reader2).PlatformInformation;
	}

	private static bool IsAzureNetworkInterface(NetworkInterface networkInterface, DhcpClient dhcpClient)
	{
		byte[] array = dhcpClient.DhcpRequestParams(networkInterface.Id, 245);
		if (array != null)
		{
			return array.Length == 4;
		}
		return false;
	}
}
