using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ServerFault : IExtensibleDataObject
{
	[DataMember]
	private string exceptionData;

	[DataMember]
	private ServerResponse responseQueue = new ServerResponse();

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

	public string ExceptionData
	{
		get
		{
			return exceptionData;
		}
		set
		{
			exceptionData = value;
		}
	}

	public ServerResponse ResponseQueue => responseQueue;

	public ServerFault(string exceptionData)
	{
		if (exceptionData == null)
		{
			throw new ArgumentNullException("exceptionData");
		}
		this.exceptionData = exceptionData;
	}
}
