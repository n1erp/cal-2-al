using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ServicePrincipalHelper
{
	private enum DsSpnWriteOp
	{
		AddSpnOp,
		ReplaceSpnOp,
		DeleteSpnOp
	}

	private static class NativeMethods
	{
		public const uint NO_ERROR = 0u;

		public const uint DS_NAME_NO_ERROR = 0u;

		public const uint ERROR_INVALID_DOMAINNAME = 1212u;

		public const uint ERROR_NO_SUCH_DOMAIN = 1355u;

		public const uint ERROR_INVALID_PARAMETER = 87u;

		public const uint ERROR_DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING = 8474u;

		public const uint ERROR_DS_INSUFF_ACCESS_RIGHTS = 8344u;

		public const int DS_DIRECTORY_SERVICE_REQUIRED = 16;

		public const int DS_IS_FLAT_NAME = 65536;

		public const int DS_RETURN_DNS_NAME = 1073741824;

		public const int DS_NAME_NO_FLAGS = 0;

		public const int DS_FQDN_1779_NAME = 1;

		public const int DS_NT4_ACCOUNT_NAME = 2;

		[DllImport("Netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint DsGetDcName([MarshalAs(UnmanagedType.LPWStr)] string computerName, [MarshalAs(UnmanagedType.LPWStr)] string domainName, [In] IntPtr domainGuid, [MarshalAs(UnmanagedType.LPWStr)] string siteName, int flags, out IntPtr domainControlInfo);

		[DllImport("ntdsapi.dll", CharSet = CharSet.Unicode)]
		internal static extern uint DsBind([MarshalAs(UnmanagedType.LPWStr)] string DomainControllerName, [MarshalAs(UnmanagedType.LPWStr)] string DnsDomainName, out IntPtr phDS);

		[DllImport("ntdsapi.dll", CharSet = CharSet.Unicode)]
		internal static extern uint DsUnBind(ref IntPtr phDS);

		[DllImport("Netapi32.dll", SetLastError = true)]
		internal static extern int NetApiBufferFree(IntPtr Buffer);

		[DllImport("ntdsapi.dll", CharSet = CharSet.Unicode)]
		internal static extern uint DsCrackNames(IntPtr hDS, int flags, int formatOffered, int formatDesired, uint cNames, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 4)] string[] rpNames, out IntPtr ppResult);

		[DllImport("ntdsapi.dll", CharSet = CharSet.Unicode)]
		internal static extern void DsFreeNameResult(IntPtr pResult);

		[DllImport("ntdsapi.dll", CharSet = CharSet.Unicode)]
		internal static extern uint DsWriteAccountSpn([In] IntPtr hDS, [In] DsSpnWriteOp operation, [In][MarshalAs(UnmanagedType.LPWStr)] string account, [In] uint cSpn, [In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 3)] string[] rpszSpn);
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct DomainControllerInfo
	{
		[MarshalAs(UnmanagedType.LPWStr)]
		public string DomainControllerName;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string DomainControllerAddress;

		public uint DomainControllerAddressType;

		public Guid DomainGuid;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string DomainName;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string DnsForestName;

		[MarshalAs(UnmanagedType.U4)]
		public uint Flags;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string DcSiteName;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string ClientSiteName;
	}

	private struct DsNameResult
	{
		public uint Count;

		public IntPtr Items;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct DsNameResultItem
	{
		public uint Status;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Domain;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string Name;
	}

	private const string NewLine = "\n";

	private static uint lastErrorCode;

	private static string ServicePrincipalAccount
	{
		get
		{
			if (IsBuiltIn(NavEnvironment.ServiceAccount))
			{
				string machineName = DnsHelper.MachineName;
				string[] array = machineName.Split('.');
				if (array != null && array.Length >= 2)
				{
					return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}$", array[1], array[0]);
				}
			}
			return NavEnvironment.ServiceAccountName;
		}
	}

	private static bool IsBuiltIn(SecurityIdentifier sid)
	{
		if (sid.IsWellKnown(WellKnownSidType.NetworkServiceSid) || sid.IsWellKnown(WellKnownSidType.LocalServiceSid) || sid.IsWellKnown(WellKnownSidType.LocalSystemSid))
		{
			return true;
		}
		return false;
	}

	internal static uint SpnRegisterLowLevel(string serviceAccount, string fqcnSpn, string netbSpn, bool register)
	{
		if (serviceAccount == null)
		{
			throw new ArgumentNullException("serviceAccount");
		}
		DsSpnWriteOp operation = ((!register) ? DsSpnWriteOp.DeleteSpnOp : DsSpnWriteOp.AddSpnOp);
		int num = serviceAccount.IndexOf('\\');
		string domainName = ((num >= 0) ? serviceAccount.Substring(0, num) : serviceAccount);
		IntPtr domainControlInfo = IntPtr.Zero;
		uint num2 = NativeMethods.DsGetDcName(null, domainName, IntPtr.Zero, null, 1073807376, out domainControlInfo);
		if (num2 != 0)
		{
			return num2;
		}
		num2 = NativeMethods.DsBind(((DomainControllerInfo)Marshal.PtrToStructure(domainControlInfo, typeof(DomainControllerInfo))).DomainControllerName, null, out var phDS);
		NativeMethods.NetApiBufferFree(domainControlInfo);
		if (num2 != 0)
		{
			return num2;
		}
		if (NativeMethods.DsCrackNames(phDS, 0, 2, 1, 1u, new string[1] { serviceAccount }, out var ppResult) == 0)
		{
			DsNameResultItem dsNameResultItem = (DsNameResultItem)Marshal.PtrToStructure(((DsNameResult)Marshal.PtrToStructure(ppResult, typeof(DsNameResult))).Items, typeof(DsNameResultItem));
			num2 = ((dsNameResultItem.Status != 0) ? 87u : NativeMethods.DsWriteAccountSpn(phDS, operation, dsNameResultItem.Name, 2u, new string[2] { fqcnSpn, netbSpn }));
			NativeMethods.DsFreeNameResult(ppResult);
		}
		else
		{
			num2 = 87u;
		}
		NativeMethods.DsUnBind(ref phDS);
		return num2;
	}

	private static void SpnRegister(string serviceType, string servicePort, bool register)
	{
		EventLogEntryType entryType = EventLogEntryType.Warning;
		string servicePrincipalAccount = ServicePrincipalAccount;
		string text = string.Format(CultureInfo.InvariantCulture, "{0}/{1}:{2}", serviceType, DnsHelper.MachineName, servicePort);
		string text2 = string.Format(CultureInfo.InvariantCulture, "{0}/{1}:{2}", serviceType, DnsHelper.HostName, servicePort);
		string text3 = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", text2, "\n", text);
		if (lastErrorCode == 0)
		{
			lastErrorCode = SpnRegisterLowLevel(servicePrincipalAccount, text, text2, register);
		}
		string message;
		switch (lastErrorCode)
		{
		case 87u:
		case 1212u:
		case 1355u:
		case 8474u:
			message = ((!register) ? string.Format(CultureInfo.InvariantCulture, "The service could not remove service principal names because the service account could not be found in Active Directory.{2}Account: {0}{2}Service principal names:{2}{1}{2}To avoid this warning, make sure the server account is provisioned correctly (see https://go.microsoft.com/fwlink/?linkid=874634 ).", servicePrincipalAccount, text3, "\n") : string.Format(CultureInfo.InvariantCulture, "The service could not add service principal names because the service account could not be found in Active Directory.{2}Account: {0}{2}Service principal names:{2}{1}{2}To avoid this warning, make sure the server account is provisioned correctly (see https://go.microsoft.com/fwlink/?linkid=874634 ).", servicePrincipalAccount, text3, "\n"));
			break;
		case 8344u:
			message = ((!register) ? string.Format(CultureInfo.InvariantCulture, "The service account has insufficient privileges to remove service principal names from Active Directory.{2}Account: {0}{2}Service principal names:{2}{1}{2}To avoid this warning, make sure the server account is provisioned correctly (see https://go.microsoft.com/fwlink/?linkid=874634 ).", servicePrincipalAccount, text3, "\n") : string.Format(CultureInfo.InvariantCulture, "The service account has insufficient privileges to register service principal names in Active Directory.{2}Account: {0}{2}Service principal names:{2}{1}{2}To avoid this warning, make sure the server account is provisioned correctly (see https://go.microsoft.com/fwlink/?linkid=874634 ).", servicePrincipalAccount, text3, "\n"));
			break;
		case 0u:
			entryType = EventLogEntryType.Information;
			message = ((!register) ? string.Format(CultureInfo.InvariantCulture, "The service has removed service principal names from Active Directory.{2}Account: {0}{2}Service principal names:{2}{1}", servicePrincipalAccount, text3, "\n") : string.Format(CultureInfo.InvariantCulture, "The service has registered service principal names in Active Directory.{2}Account: {0}{2}Service principal names:{2}{1}", servicePrincipalAccount, text3, "\n"));
			break;
		default:
			message = ((!register) ? string.Format(CultureInfo.InvariantCulture, "The service could not remove service principal names from Active Directory.{3}Account: {0}{3}Service principal names:{3}{1}{3}Error code:{2}", servicePrincipalAccount, text3, lastErrorCode.ToString(CultureInfo.InvariantCulture), "\n") : string.Format(CultureInfo.InvariantCulture, "The service could not create service principal names in Active Directory.{3}Account: {0}{3}Service principal names:{3}{1}{3}Error code:{2}", servicePrincipalAccount, text3, lastErrorCode.ToString(CultureInfo.InvariantCulture), "\n"));
			lastErrorCode = 0u;
			break;
		}
		LogWriterHelper.LogEvent(entryType, message);
	}

	public static void Register(string serviceType, string servicePort)
	{
		if (NavEnvironment.Topology.AllowToRegisterServicePrincipalName)
		{
			SpnRegister(serviceType, servicePort, register: true);
		}
	}

	public static void Unregister(string serviceType, string servicePort)
	{
		if (NavEnvironment.Topology.AllowToRegisterServicePrincipalName)
		{
			SpnRegister(serviceType, servicePort, register: false);
		}
	}
}
