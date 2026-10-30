using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ClientConfiguration
{
	[DataMember]
	public Uri AcsUri { get; set; }

	[DataMember]
	public string ExcelAuthenticationClientId { get; set; }

	[DataMember]
	public string ExcelAuthenticationRealm { get; set; }

	[DataMember]
	public string AadTenantId { get; set; }

	[DataMember]
	public string ExcelAuthenticationResourceId { get; set; }

	[DataMember]
	public string ExcelAlternateODataHost { get; set; }

	[DataMember]
	public string[] CompanyIds { get; set; }

	[DataMember]
	public string DefaultCompanyId { get; set; }

	[DataMember]
	public Guid AzureActiveDirectoryClientId { get; set; }

	[DataMember]
	public Uri TokenAuthorityEndpoint { get; set; }

	[DataMember]
	public bool IsPreviewRelease { get; set; }

	[DataMember]
	public Guid AzureActiveDirectoryAuthCodeClientId { get; set; }
}
