using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NSSystemInfoData : IExtensibleDataObject
{
	private string licenseNumber;

	private string licensedTo;

	private string applicationVersion;

	private ExtensionDataObject unknownDataBag;

	public virtual ExtensionDataObject ExtensionData
	{
		get
		{
			return unknownDataBag;
		}
		set
		{
			unknownDataBag = value;
		}
	}

	[DataMember(Name = "LicNum")]
	public string LicenseNumber
	{
		get
		{
			return licenseNumber;
		}
		set
		{
			licenseNumber = value;
		}
	}

	[DataMember(Name = "LicTo")]
	public string LicensedTo
	{
		get
		{
			return licensedTo;
		}
		set
		{
			licensedTo = value;
		}
	}

	[DataMember(Name = "AppVer")]
	public string ApplicationVersion
	{
		get
		{
			return applicationVersion;
		}
		set
		{
			applicationVersion = value;
		}
	}

	[DataMember(Name = "LicState")]
	public TenantLicenseState LicenseState { get; set; }
}
