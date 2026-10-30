using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavComStreamWrapper : IStream
{
	private NavStream stream;

	internal NavComStreamWrapper(NavStream stream)
	{
		this.stream = stream;
	}

	void IStream.Clone(out IStream ppstm)
	{
		throw new NotImplementedException();
	}

	void IStream.Commit(int grfCommitFlags)
	{
	}

	void IStream.CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
	{
		throw new NotImplementedException();
	}

	void IStream.LockRegion(long libOffset, long cb, int lockType)
	{
	}

	unsafe void IStream.Read(byte[] pv, int cb, IntPtr pcbRead)
	{
		if (!(stream is INavStreamReader navStreamReader))
		{
			throw new NavNCLStreamAccessException();
		}
		byte[] array = navStreamReader.ReadBytes(cb);
		if (array == null)
		{
			*(int*)(void*)pcbRead = 0;
			return;
		}
		*(int*)(void*)pcbRead = array.Length;
		Array.Copy(array, pv, array.Length);
	}

	void IStream.Revert()
	{
		throw new NotImplementedException();
	}

	unsafe void IStream.Seek(long dlibMove, int origin, IntPtr plibNewPosition)
	{
		switch ((SeekOrigin)origin)
		{
		case SeekOrigin.Begin:
			stream.Position = (int)dlibMove;
			break;
		case SeekOrigin.Current:
			stream.Position = (int)(stream.Position + dlibMove);
			break;
		case SeekOrigin.End:
			stream.Position = (int)(stream.Length + dlibMove);
			break;
		}
		*(long*)(void*)plibNewPosition = stream.Position;
	}

	void IStream.SetSize(long libNewSize)
	{
		throw new NotImplementedException();
	}

	void IStream.Stat(out STATSTG pstatstg, int grfStatFlag)
	{
		throw new NotImplementedException();
	}

	void IStream.UnlockRegion(long libOffset, long cb, int lockType)
	{
	}

	unsafe void IStream.Write(byte[] pv, int cb, IntPtr pcbWritten)
	{
		if (!(stream is INavStreamWriter navStreamWriter))
		{
			throw new NavNCLStreamAccessException();
		}
		long position = stream.Position;
		navStreamWriter.WriteBytes(pv, cb);
		if (pcbWritten.ToPointer() != null)
		{
			*(int*)(void*)pcbWritten = (int)(stream.Position - position);
		}
	}
}
