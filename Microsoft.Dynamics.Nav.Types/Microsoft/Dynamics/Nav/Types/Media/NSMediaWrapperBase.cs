using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types.Media;

public abstract class NSMediaWrapperBase
{
	private byte[] mediaData;

	private NavMediaDescriptionCollection navMediaDescriptionCollection;

	private const int SizeOfGuid = 16;

	public NavMediaDescriptionCollection MediaCollection => navMediaDescriptionCollection ?? (navMediaDescriptionCollection = DeserializeMediaReferences(mediaData));

	protected NSMediaWrapperBase(byte[] data)
	{
		mediaData = data;
		navMediaDescriptionCollection = null;
	}

	protected NSMediaWrapperBase(NavMediaDescriptionCollection data)
	{
		navMediaDescriptionCollection = data;
	}

	protected NSMediaWrapperBase(Guid data)
	{
		NavMediaDescription item = new NavMediaDescription
		{
			MediaId = data,
			Description = string.Empty
		};
		navMediaDescriptionCollection = new NavMediaDescriptionCollection { item };
	}

	protected NSMediaWrapperBase(object data)
		: this((byte[])data)
	{
	}

	public virtual string XmlString()
	{
		return XmlConvert.ToString(MediaId());
	}

	public new virtual string ToString()
	{
		return MediaId().ToString("B").ToUpperInvariant();
	}

	public virtual Guid MediaId()
	{
		return MediaCollection.FirstOrDefault()?.MediaId ?? Guid.Empty;
	}

	protected virtual byte[] MediaData()
	{
		return mediaData ?? (mediaData = SerializeCollection(navMediaDescriptionCollection));
	}

	internal static byte[] SerializeCollection(NavMediaDescriptionCollection navMediaDescriptionCollection)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		foreach (NavMediaDescription item in navMediaDescriptionCollection)
		{
			if (item != null)
			{
				binaryWriter.Write(item.MediaId.ToByteArray());
				byte[] bytes = Encoding.UTF8.GetBytes(item.Description);
				binaryWriter.Write(bytes.Length);
				binaryWriter.Write(bytes);
			}
		}
		return (memoryStream.Length == memoryStream.Capacity) ? memoryStream.GetBuffer() : memoryStream.ToArray();
	}

	internal static NavMediaDescriptionCollection DeserializeMediaReferences(byte[] value)
	{
		if (value == null || value.Length == 0)
		{
			return new NavMediaDescriptionCollection();
		}
		NavMediaDescriptionCollection navMediaDescriptionCollection = new NavMediaDescriptionCollection();
		using MemoryStream input = new MemoryStream(value);
		using BinaryReader binaryReader = new BinaryReader(input);
		while (true)
		{
			byte[] array = binaryReader.ReadBytes(16);
			if (array.Length != 16)
			{
				break;
			}
			Guid id = new Guid(array);
			int num = binaryReader.ReadInt32();
			string description;
			if (num > 0)
			{
				array = binaryReader.ReadBytes(num);
				description = Encoding.UTF8.GetString(array);
			}
			else
			{
				description = string.Empty;
			}
			navMediaDescriptionCollection.Add(new NavMediaDescription(id, description));
		}
		return navMediaDescriptionCollection;
	}

	public virtual byte[] ToArray()
	{
		return MediaData();
	}

	public bool Equals(NSMediaWrapperBase other)
	{
		if (other == null)
		{
			return false;
		}
		if (other == this)
		{
			return true;
		}
		if (GetType() != other.GetType())
		{
			return false;
		}
		byte[] array = MediaData();
		byte[] otherValue = other.MediaData();
		if (array.Length != otherValue.Length)
		{
			return false;
		}
		return !array.Where((byte t, int i) => t != otherValue[i]).Any();
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj == DBNull.Value)
		{
			return false;
		}
		if (obj is NSMediaWrapperBase other)
		{
			return Equals(other);
		}
		return ThrowUnableToCompareException(obj);
	}

	public override int GetHashCode()
	{
		return MediaCollection?.GetHashCode() ?? Guid.Empty.GetHashCode();
	}

	private bool ThrowUnableToCompareException(object unsupportedInstance)
	{
		string unsupportedTypeName = unsupportedInstance?.GetType().Name ?? "<NULL>";
		throw NavNCLInvalidComparisonException.Create(CultureInfo.InvariantCulture, GetType().Name, unsupportedTypeName);
	}
}
