using System;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavStream : NavComplexValue, ITreeObjectReference, ITreeObject
{
	public Stream InternalStream
	{
		get
		{
			if (!HasTarget || Target.SharedStream == null)
			{
				return null;
			}
			Stream stream = Target.SharedStream.GetStream();
			if (stream != null && stream.CanSeek && stream.Position != Target.Position)
			{
				stream.Position = Target.Position;
			}
			return stream;
		}
	}

	public Stream InternalStreamChecked
	{
		get
		{
			Stream internalStream = InternalStream;
			if (internalStream == null)
			{
				throw new NavNCLNotInitializedException(GetType());
			}
			return internalStream;
		}
	}

	public Stream InternalStreamCheckedForRead
	{
		get
		{
			Stream internalStreamChecked = InternalStreamChecked;
			if (!internalStreamChecked.CanRead)
			{
				throw new NavNCLStreamAccessException();
			}
			return internalStreamChecked;
		}
	}

	public Stream InternalStreamCheckedForWrite
	{
		get
		{
			Stream internalStreamChecked = InternalStreamChecked;
			if (!internalStreamChecked.CanWrite)
			{
				throw new NavNCLStreamAccessException();
			}
			return internalStreamChecked;
		}
	}

	public override bool IsInitialized => InternalStream != null;

	public long Length
	{
		get
		{
			EnsureReadToEos();
			return InternalStreamChecked.Length;
		}
	}

	public long Position
	{
		get
		{
			return Target.Position;
		}
		set
		{
			Target.Position = value;
		}
	}

	internal bool HasTarget => Tree.GetReferenceTarget() != null;

	internal SharedNavStream Target
	{
		get
		{
			SharedNavStream sharedNavStream = (SharedNavStream)Tree.GetReferenceTarget();
			if (sharedNavStream == null)
			{
				sharedNavStream = new SharedNavStream();
				Tree.SetReferenceTarget(sharedNavStream);
			}
			return sharedNavStream;
		}
		set
		{
			Tree.SetReferenceTarget(value);
		}
	}

	public override int GetBytesSize
	{
		get
		{
			EnsureReadToEos();
			return (int)InternalStreamChecked.Length;
		}
	}

	public override NavNclType NclType => NavNclType.NavStream;

	internal NavStream(ITreeObject parent, INavStreamProvider navStreamProvider)
		: base(parent)
	{
		Target.SharedStream = navStreamProvider;
		Target.TextEncoding = TextEncoding.MSDos;
	}

	public override void Clear()
	{
		Target.SharedStream = null;
		Position = 0L;
	}

	protected void Assign(NavStream other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (Target != other.Target)
		{
			Target = other.Target;
		}
	}

	public byte[] GetBytes(bool ignoreCurrentPosition)
	{
		Stream internalStreamChecked = InternalStreamChecked;
		MemoryStream memoryStream = internalStreamChecked as MemoryStream;
		if ((memoryStream != null) & ignoreCurrentPosition)
		{
			return memoryStream.ToArray();
		}
		FileBufferedStream fileBufferedStream = internalStreamChecked as FileBufferedStream;
		if ((fileBufferedStream != null) & ignoreCurrentPosition)
		{
			fileBufferedStream.ReadToEos();
		}
		HybridMemoryDecompressStream hybridMemoryDecompressStream = internalStreamChecked as HybridMemoryDecompressStream;
		if ((hybridMemoryDecompressStream != null) & ignoreCurrentPosition)
		{
			return hybridMemoryDecompressStream.ToArray();
		}
		if (internalStreamChecked.CanSeek)
		{
			long position = internalStreamChecked.Position;
			try
			{
				if (ignoreCurrentPosition)
				{
					internalStreamChecked.Position = 0L;
				}
				int num = 0;
				byte[] array = new byte[internalStreamChecked.Length];
				while (internalStreamChecked.Position < internalStreamChecked.Length)
				{
					long num2 = internalStreamChecked.Length - internalStreamChecked.Position;
					num = ((num2 < int.MaxValue) ? (num + internalStreamChecked.Read(array, num, Convert.ToInt32(num2))) : (num + internalStreamChecked.Read(array, num, int.MaxValue)));
				}
				return array;
			}
			finally
			{
				internalStreamChecked.Position = position;
			}
		}
		if (Position == 0L)
		{
			byte[] array2 = new byte[4096];
			int num3 = 0;
			while (true)
			{
				int num4 = internalStreamChecked.Read(array2, num3, array2.Length - num3);
				if (num4 == 0 || num3 >= 16777216)
				{
					break;
				}
				num3 += num4;
				byte[] array3 = new byte[(array2.Length < 16777216) ? (array2.Length * 2) : array2.Length];
				array2.CopyTo(array3, 0);
				array2 = array3;
			}
			return array2;
		}
		throw new NavNCLStreamIOException();
	}

	public override byte[] GetBytes()
	{
		return GetBytes(ignoreCurrentPosition: true);
	}

	public override int GetHashCode()
	{
		return InternalStream?.GetHashCode() ?? (-1);
	}

	private void EnsureReadToEos()
	{
		if (InternalStreamChecked is FileBufferedStream fileBufferedStream)
		{
			fileBufferedStream.ReadToEos();
		}
	}
}
