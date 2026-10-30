using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SerializableObjectContract
{
	[DataMember]
	private string assemblyName;

	[DataMember]
	private string typeName;

	[DataMember]
	private byte[] objectData;

	public string AssemblyName => assemblyName;

	public string TypeName => typeName;

	public byte[] ObjectData => objectData;

	public SerializableObjectContract(object value)
	{
		if (value != null)
		{
			Type type = value.GetType();
			assemblyName = type.Assembly.FullName;
			typeName = type.FullName;
			MemoryStream memoryStream = new MemoryStream();
			new BinaryFormatter().Serialize(memoryStream, value);
			objectData = memoryStream.GetBuffer();
		}
	}
}
