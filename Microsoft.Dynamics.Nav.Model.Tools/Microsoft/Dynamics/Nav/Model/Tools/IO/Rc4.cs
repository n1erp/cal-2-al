using System;

namespace Microsoft.Dynamics.Nav.Model.Tools.IO;

internal sealed class Rc4
{
	private byte[] key;

	private uint x;

	private uint y;

	public Rc4(byte[] key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		byte[] array = new byte[256];
		for (int i = 0; i < 256; i++)
		{
			array[i] = (byte)i;
		}
		int j = 0;
		int num = 0;
		for (; j < 256; j++)
		{
			num = (num + key[j % key.Length] + array[j]) & 0xFF;
			byte b = array[j];
			array[j] = array[num];
			array[num] = b;
		}
		this.key = array;
	}

	public void Decode(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (buffer.Length < offset + count)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		for (int i = 0; i < count; i++)
		{
			buffer[offset + i] = (byte)(buffer[offset + i] ^ GetMask());
		}
	}

	public byte[] Encode(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (buffer.Length < offset + count)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		byte[] array = new byte[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = (byte)(buffer[offset + i] ^ GetMask());
		}
		return array;
	}

	private byte GetMask()
	{
		x = (x + 1) & 0xFF;
		y = (y + key[x]) & 0xFF;
		byte b = key[x];
		key[x] = key[y];
		key[y] = b;
		return key[(key[x] + key[y]) & 0xFF];
	}
}
