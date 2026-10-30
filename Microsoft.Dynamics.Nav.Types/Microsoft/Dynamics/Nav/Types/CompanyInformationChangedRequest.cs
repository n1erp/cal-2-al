using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
internal sealed class CompanyInformationChangedRequest : ServerCallbackRequest
{
	[DataMember]
	public CompanyInformationChanges CompanyInformationChanges { get; private set; }

	public CompanyInformationChangedRequest(CompanyInformationChanges companyInformationChanges)
	{
		CompanyInformationChanges = companyInformationChanges;
	}

	public override void Process(IClientCallbackHandler callbackHandler)
	{
		if (callbackHandler == null)
		{
			throw new ArgumentNullException("callbackHandler");
		}
		callbackHandler.CompanyInformationChanged(CompanyInformationChanges);
	}
}
