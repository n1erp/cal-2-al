using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ServicesUrlResponse : ServerResponse
{
	private Uri url;

	[DataMember]
	public Uri Url
	{
		get
		{
			return url;
		}
		set
		{
			url = value;
		}
	}

	public ServicesUrlResponse(Uri url)
	{
		this.url = url;
	}

	public ServicesUrlResponse(string url)
		: this(string.IsNullOrEmpty(url) ? null : new Uri(url))
	{
	}
}
