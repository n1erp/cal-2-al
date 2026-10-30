using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class FileActionResponse : IExtensibleDataObject
{
	private FileActionRequest fileActionRequest;

	private bool isFileAccessed;

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

	[DataMember]
	public FileActionRequest FileActionRequest
	{
		get
		{
			return fileActionRequest;
		}
		set
		{
			fileActionRequest = value;
		}
	}

	[DataMember]
	public bool IsFileAccessed
	{
		get
		{
			return isFileAccessed;
		}
		set
		{
			isFileAccessed = value;
		}
	}
}
