using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SendDataToDocumentResponse : ServerResponse
{
	private string path;

	[DataMember]
	public string Path
	{
		get
		{
			return path;
		}
		set
		{
			path = value;
		}
	}

	public SendDataToDocumentResponse(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		this.path = path;
	}
}
