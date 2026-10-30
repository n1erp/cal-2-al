using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

public class EndpointConnectionString
{
	private readonly Uri url;

	private string spnIdentity;

	public const string SpnServiceType = "DynamicsNAV";

	public string SpnIdentityName
	{
		get
		{
			if (string.IsNullOrEmpty(spnIdentity))
			{
				spnIdentity = string.Format(CultureInfo.InvariantCulture, "{0}/{1}:{2}", "DynamicsNAV", url.Host, url.Port);
			}
			return spnIdentity;
		}
	}

	public Uri Url => url;

	public EndpointConnectionString(Uri url)
	{
		this.url = url;
	}
}
