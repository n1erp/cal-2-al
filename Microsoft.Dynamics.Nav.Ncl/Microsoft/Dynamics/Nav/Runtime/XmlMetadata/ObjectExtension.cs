using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

public static class ObjectExtension
{
	public static T DeepClone<T>(this T source)
	{
		if (!typeof(T).IsSerializable)
		{
			throw new ArgumentException("The type must be serializable.", "source");
		}
		if (source == null)
		{
			return default;
		}
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Binder = new NavSerializationBinder(typeof(T));
		IFormatter formatter = binaryFormatter;
		using Stream stream = new MemoryStream();
		formatter.Serialize(stream, source);
		stream.Seek(0L, SeekOrigin.Begin);
		return (T)formatter.Deserialize(stream);
	}
}
