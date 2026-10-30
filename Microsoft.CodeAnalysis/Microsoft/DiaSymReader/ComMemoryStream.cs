using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.DiaSymReader;

internal sealed class ComMemoryStream : IUnsafeComStream
{
	internal const int STREAM_SEEK_SET = 0;

	internal const int STREAM_SEEK_CUR = 1;

	internal const int STREAM_SEEK_END = 2;

	private readonly int _chunkSize;

	private readonly List<byte[]> _chunks = new List<byte[]>();

	private int _position;

	private int _length;

	public ComMemoryStream(int chunkSize = 32768)
	{
		_chunkSize = chunkSize;
	}

	public void CopyTo(Stream stream)
	{
		if (stream.CanSeek)
		{
			stream.SetLength(stream.Position + _length);
		}
		int num = 0;
		int num2 = _length;
		while (num2 > 0)
		{
			int num3;
			if (num < _chunks.Count)
			{
				byte[] array = _chunks[num];
				num3 = Math.Min(array.Length, num2);
				stream.Write(array, 0, num3);
				num++;
			}
			else
			{
				num3 = num2;
				for (int i = 0; i < num3; i++)
				{
					stream.WriteByte(0);
				}
			}
			num2 -= num3;
		}
	}

	public IEnumerable<ArraySegment<byte>> GetChunks()
	{
		int chunkIndex = 0;
		int remainingBytes = _length;
		while (remainingBytes > 0)
		{
			byte[] array;
			int bytesToCopy;
			if (chunkIndex < _chunks.Count)
			{
				array = _chunks[chunkIndex];
				bytesToCopy = Math.Min(array.Length, remainingBytes);
				chunkIndex++;
			}
			else
			{
				array = new byte[remainingBytes];
				bytesToCopy = remainingBytes;
			}
			yield return new ArraySegment<byte>(array, 0, bytesToCopy);
			remainingBytes -= bytesToCopy;
		}
	}

	private unsafe static void ZeroMemory(byte* dest, int count)
	{
		byte* ptr = dest;
		while (count-- > 0)
		{
			*(ptr++) = 0;
		}
	}

	unsafe void IUnsafeComStream.Read(byte* pv, int cb, int* pcbRead)
	{
		int num = _position / _chunkSize;
		int num2 = _position % _chunkSize;
		int num3 = 0;
		int num4 = 0;
		while (true)
		{
			int num5 = Math.Min(_length - _position, Math.Min(cb, _chunkSize - num2));
			if (num5 == 0)
			{
				break;
			}
			if (num < _chunks.Count)
			{
				Marshal.Copy(_chunks[num], num2, (IntPtr)(pv + num3), num5);
			}
			else
			{
				ZeroMemory(pv + num3, num5);
			}
			num4 += num5;
			_position += num5;
			cb -= num5;
			num3 += num5;
			num++;
			num2 = 0;
		}
		if (pcbRead != null)
		{
			*pcbRead = num4;
		}
	}

	private int SetPosition(int newPos)
	{
		if (newPos < 0)
		{
			newPos = 0;
		}
		_position = newPos;
		if (newPos > _length)
		{
			_length = newPos;
		}
		return newPos;
	}

	unsafe void IUnsafeComStream.Seek(long dlibMove, int origin, long* plibNewPosition)
	{
		int num = origin switch
		{
			0 => SetPosition((int)dlibMove), 
			1 => SetPosition(_position + (int)dlibMove), 
			2 => SetPosition(_length + (int)dlibMove), 
			_ => throw new ArgumentException(string.Format("{0} ({1}) is invalid.", "origin", origin), "origin"), 
		};
		if (plibNewPosition != null)
		{
			*plibNewPosition = num;
		}
	}

	void IUnsafeComStream.SetSize(long libNewSize)
	{
		_length = (int)libNewSize;
	}

	void IUnsafeComStream.Stat(out STATSTG pstatstg, int grfStatFlag)
	{
		pstatstg = new STATSTG
		{
			cbSize = _length
		};
	}

	unsafe void IUnsafeComStream.Write(byte* pv, int cb, int* pcbWritten)
	{
		int num = _position / _chunkSize;
		int num2 = _position % _chunkSize;
		int num3 = 0;
		while (true)
		{
			int num4 = Math.Min(cb, _chunkSize - num2);
			if (num4 == 0)
			{
				break;
			}
			while (num >= _chunks.Count)
			{
				_chunks.Add(new byte[_chunkSize]);
			}
			Marshal.Copy((IntPtr)(pv + num3), _chunks[num], num2, num4);
			num3 += num4;
			cb -= num4;
			num++;
			num2 = 0;
		}
		SetPosition(_position + num3);
		if (pcbWritten != null)
		{
			*pcbWritten = num3;
		}
	}

	void IUnsafeComStream.Commit(int grfCommitFlags)
	{
	}

	void IUnsafeComStream.Clone(out IStream ppstm)
	{
		throw new NotSupportedException();
	}

	unsafe void IUnsafeComStream.CopyTo(IStream pstm, long cb, int* pcbRead, int* pcbWritten)
	{
		throw new NotSupportedException();
	}

	void IUnsafeComStream.LockRegion(long libOffset, long cb, int lockType)
	{
		throw new NotSupportedException();
	}

	void IUnsafeComStream.Revert()
	{
		throw new NotSupportedException();
	}

	void IUnsafeComStream.UnlockRegion(long libOffset, long cb, int lockType)
	{
		throw new NotSupportedException();
	}
}
