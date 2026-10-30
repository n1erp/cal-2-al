using System;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Channels;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class NavServiceBinding : NetTcpChunkBinding
{
	private const string ManagementServiceName = "ManagementService";

	private const string UserNameServiceName = "/UserName";

	private const string ReliableSessionServiceName = "/ReliableSession";

	private const string HealthServiceName = "HealthService";

	private const string StartupServiceName = "StartupService";

	private const string OpenDynamicFileHandlerServiceName = "OpenDynamicFileHandler";

	private const string ServiceName = "Service";

	private const string PathSeparator = "/";

	private const string UriPortDelimiter = ":";

	public NavServiceBinding(ProtectionLevel protectionLevel, int chunkSize, NavClientServicesAuthenticationBehavior authenticationBehavior)
		: base(chunkSize)
	{
		MaxReceivedMessageSize = 2147483647L;
		ReaderQuotas = XmlDictionaryReaderQuotas.Max;
		Security.Transport.ProtectionLevel = protectionLevel;
		Security.Mode = authenticationBehavior.TcpSecurityMode;
		Security.Transport.ClientCredentialType = authenticationBehavior.TcpClientCredentialType;
		Security.Message.ClientCredentialType = authenticationBehavior.MessageCredentialType;
		ReliableSession.Enabled = true;
		ReliableSession.Ordered = true;
		ReceiveTimeout = TimeSpan.MaxValue;
	}

	public static Binding CopyBindingAndDisableServerIdentityCheck(Binding binding)
	{
		if (binding == null)
		{
			throw new ArgumentNullException("binding");
		}
		CustomBinding customBinding = new CustomBinding(binding);
		TransportSecurityBindingElement transportSecurityBindingElement = customBinding.Elements.Find<TransportSecurityBindingElement>();
		if (transportSecurityBindingElement != null)
		{
			transportSecurityBindingElement.LocalClientSettings.IdentityVerifier = new NoIdentityChecksIdentityVerifier();
		}
		SslStreamSecurityBindingElement sslStreamSecurityBindingElement = customBinding.Elements.Find<SslStreamSecurityBindingElement>();
		if (sslStreamSecurityBindingElement != null)
		{
			sslStreamSecurityBindingElement.IdentityVerifier = new NoIdentityChecksIdentityVerifier();
		}
		return customBinding;
	}

	public static Uri GetServiceUri(string serverMachineName, string serverInstanceName, int serverPort)
	{
		return GetServiceUri(serverMachineName, serverInstanceName, serverPort, "Service");
	}

	public static Uri GetAdminServiceUri(string serverMachineName, string serverInstanceName, int serverPort, bool useUserName = false, bool reliableSessionEnabled = false)
	{
		return GetServiceUri(serverMachineName, serverInstanceName, serverPort, "ManagementService" + (useUserName ? "/UserName" : string.Empty) + (reliableSessionEnabled ? "/ReliableSession" : string.Empty));
	}

	public static Uri GetHealthServiceUri(string serverMachineName, string serverInstanceName, int serverPort)
	{
		return GetServiceUri(serverMachineName, serverInstanceName, serverPort, "HealthService");
	}

	public static Uri GetStartupServiceUri(string serverMachineName, string serverInstanceName, int serverPort)
	{
		return GetServiceUri(serverMachineName, serverInstanceName, serverPort, "StartupService");
	}

	public static Uri GetOpenDynamicFileHandlerServiceUri(string serverMachineName, string serverInstanceName, int serverPort)
	{
		return GetServiceUri(serverMachineName, serverInstanceName, serverPort, "OpenDynamicFileHandler");
	}

	public static Uri GetServiceUri(string serverMachineName, string serverInstanceName, int serverPort, string serviceName)
	{
		if (string.IsNullOrEmpty(serverMachineName))
		{
			throw new ArgumentException(Lang.StringNullOrEmpty, "serverMachineName");
		}
		if (string.IsNullOrEmpty(serverInstanceName))
		{
			throw new ArgumentException(Lang.StringNullOrEmpty, "serverInstanceName");
		}
		return new Uri(Uri.UriSchemeNetTcp + Uri.SchemeDelimiter + serverMachineName + ":" + serverPort + "/" + serverInstanceName + "/" + serviceName);
	}
}
