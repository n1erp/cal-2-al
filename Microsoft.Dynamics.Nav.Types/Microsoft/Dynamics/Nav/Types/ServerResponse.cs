using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ServerResponse : IExtensibleDataObject
{
	private const string NonRowFormat = "{0}_NonRow";

	private long versionNumber;

	[DataMember]
	private Queue<ServerCallbackRequest> requests = new Queue<ServerCallbackRequest>();

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

	public Queue<ServerCallbackRequest> Requests => requests;

	[DataMember]
	public long VersionNumber
	{
		get
		{
			return versionNumber;
		}
		set
		{
			versionNumber = value;
		}
	}

	public Guid TaskId { get; set; }

	public static string GenerateNonRowTableName(int id)
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}_NonRow", id);
	}
}
