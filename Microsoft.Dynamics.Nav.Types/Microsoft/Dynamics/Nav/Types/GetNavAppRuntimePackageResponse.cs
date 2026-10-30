using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

[MessageContract(WrapperNamespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class GetNavAppRuntimePackageResponse : IExtensibleDataObject
{
	[MessageHeader]
	private string error;

	[MessageBodyMember]
	private Stream packageStream;

	private ExtensionDataObject unknownDataBag;

	public ExtensionDataObject ExtensionData
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
	public string Error
	{
		get
		{
			return error;
		}
		set
		{
			error = value;
		}
	}

	[DataMember]
	public bool DidErrorOccur => !string.IsNullOrWhiteSpace(Error);

	[DataMember]
	public Stream Package
	{
		get
		{
			return packageStream;
		}
		set
		{
			packageStream = value;
		}
	}

	private GetNavAppRuntimePackageResponse()
	{
	}

	public static GetNavAppRuntimePackageResponse CreateForError(string error)
	{
		return new GetNavAppRuntimePackageResponse
		{
			Error = error,
			Package = Stream.Null
		};
	}

	public static GetNavAppRuntimePackageResponse CreateForSuccess(Stream packageStream)
	{
		return new GetNavAppRuntimePackageResponse
		{
			packageStream = packageStream
		};
	}
}
