using System;

namespace Microsoft.Dynamics.Nav.Types;

public class RecordBookmark
{
	private byte[] value;

	public const int TableIdBytesLength = 4;

	public int TableId => BitConverter.ToInt32(value, 0);

	public RecordBookmark(byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		this.value = value;
	}

	public byte[] GetBytes()
	{
		return value;
	}

	public byte[] GetPrimaryKey()
	{
		byte[] array = new byte[value.Length - 4];
		Array.Copy(value, 4, array, 0, array.Length);
		return array;
	}
}
