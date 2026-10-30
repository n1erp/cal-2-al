using System.IO;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ConvertProfileResponse : IExtensibleDataObject
{
	[DataMember]
	private readonly ConvertProfileResult result;

	[DataMember]
	private readonly string error;

	[DataMember]
	private readonly byte[] conversionResults;

	public ConvertProfileResult Result => result;

	public string Error => error;

	public ExtensionDataObject ExtensionData { get; set; }

	public ConvertProfileResponse(ConvertProfileResult result)
		: this(result, string.Empty)
	{
	}

	public ConvertProfileResponse(ConvertProfileResult result, string error)
	{
		this.result = result;
		this.error = error;
	}

	public ConvertProfileResponse(ConvertProfileResult result, byte[] conversionResults)
	{
		this.result = result;
		this.conversionResults = conversionResults;
	}

	public ConvertProfileResponse(ConvertProfileResult result, string error, byte[] conversionResults)
	{
		this.result = result;
		this.error = error;
		this.conversionResults = conversionResults;
	}

	public byte[] GetConversionResults()
	{
		return (byte[])conversionResults.Clone();
	}

	public void StoreResults(string exportPath)
	{
		File.WriteAllBytes(exportPath, conversionResults);
	}
}
