using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavBLOB : NavValue, INavStreamProvider, IDisposable
{
	internal class InternalStream : Stream
	{
		private readonly NavBLOB host;

		private int position;

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => true;

		public override long Length
		{
			get
			{
				if (!host.IsInMemory)
				{
					return 0L;
				}
				return host.GetLength();
			}
		}

		public override long Position
		{
			get
			{
				return position;
			}
			set
			{
				position = (int)value;
			}
		}

		internal InternalStream(NavBLOB host)
		{
			this.host = host;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			position = (int)NCLStreamHelper.CalcPositionFromSeek(offset, origin, Position, Length);
			return position;
		}

		public override void SetLength(long value)
		{
			host.BlobTruncate((int)value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (!host.IsZeroOrEmpty)
			{
				int num = host.BlobRead(position, buffer, offset, count);
				position += num;
				return num;
			}
			return 0;
		}

		public override int ReadByte()
		{
			int num = host.BlobReadByte(position);
			if (num >= 0)
			{
				position++;
			}
			return num;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			int num = host.BlobWrite(position, buffer, offset, count);
			position += num;
		}

		public override void WriteByte(byte value)
		{
			host.BlobWriteByte(position, value);
			position++;
		}
	}

	private Stream exposedStream;

	private MemoryStream contents;

	private int sizeWhenNoContents;

	private bool isDirty;

	internal const int BlobMagicSize = 4;

	internal const int ZeroFlag = 0;

	public const int NotZeroFlag = 4;

	internal static byte[] BlobMagic = new byte[4] { 2, 69, 125, 91 };

	public bool ALHasValue => !IsZeroOrEmpty;

	public int ALLength => GetLength();

	public override NavNclType NclType => NavNclType.NavBlob;

	public override int GetBytesSize
	{
		get
		{
			if (!IsInMemory)
			{
				return 8;
			}
			return GetLength();
		}
	}

	public override bool IsMutable => true;

	public override object ValueAsObject => GetBytes();

	public override object ClientObject => GetBytes();

	public bool IsInMemory => contents != null;

	public bool IsDirty
	{
		get
		{
			return isDirty;
		}
		internal set
		{
			isDirty = value;
		}
	}

	public override bool IsZeroOrEmpty => GetLength() == 0;

	public string ALImport(string fileName)
	{
		using NavFile navFile = new NavFile(NavCurrentThread.Session);
		navFile.ALTextMode = false;
		navFile.ALOpen(fileName);
		if (navFile.ALLen > int.MaxValue)
		{
			throw new NavNCLFileLoadException(Lang.FileIsTooBig);
		}
		AssignFromStream(navFile.InternalStream);
		return navFile.ALName;
	}

	public string ALExport(string fileName)
	{
		using NavFile navFile = new NavFile(NavCurrentThread.Session);
		navFile.ALTextMode = false;
		navFile.ALWriteMode = true;
		navFile.ALCreate(fileName);
		NCLStreamHelper.CopyStream(new InternalStream(this), navFile.InternalStream);
		return navFile.ALName;
	}

	public NavInStream ALCreateInStream(ITreeObject parentForResult)
	{
		return new NavInStream(parentForResult, this, TextEncoding.MSDos);
	}

	public NavInStream ALCreateInStream(ITreeObject parentForResult, TextEncoding textEncoding)
	{
		return new NavInStream(parentForResult, this, textEncoding);
	}

	public void ALCreateInStream(ITreeObject parentForResult, NavInStream inStream, TextEncoding textEncoding = TextEncoding.MSDos)
	{
		inStream.ALAssign(new NavInStream(parentForResult, this, textEncoding));
	}

	public NavOutStream ALCreateOutStream(ITreeObject parentForResult)
	{
		return new NavOutStream(parentForResult, this, TextEncoding.MSDos);
	}

	public NavOutStream ALCreateOutStream(ITreeObject parentForResult, TextEncoding textEncoding)
	{
		return new NavOutStream(parentForResult, this, textEncoding);
	}

	public void ALCreateOutStream(ITreeObject parentForResult, NavOutStream outStream, TextEncoding textEncoding = TextEncoding.MSDos)
	{
		outStream.ALAssign(new NavOutStream(parentForResult, this, textEncoding));
	}

	public void ALAssign(NavBLOB other)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public NavBLOB ALByValue(ITreeObject parentOfResult)
	{
		if (ALHasValue)
		{
			if (IsInMemory)
			{
				NavBLOB navBLOB = new NavBLOB();
				navBLOB.AssignFromStream(new InternalStream(this));
				return navBLOB;
			}
			return new NavBLOB(GetLength());
		}
		return new NavBLOB();
	}

	public NavBLOB()
	{
	}

	public NavBLOB(int sizeWhenNoContents)
	{
		this.sizeWhenNoContents = sizeWhenNoContents;
	}

	public NavBLOB(byte[] contentsByteArray, bool useContentInstance = false)
	{
		if (contentsByteArray == null)
		{
			throw new ArgumentNullException("contentsByteArray");
		}
		if (useContentInstance)
		{
			contents = new MemoryStream(contentsByteArray);
			return;
		}
		contents = new MemoryStream(contentsByteArray.Length);
		contents.Write(contentsByteArray, 0, contentsByteArray.Length);
		contents.Position = 0L;
	}

	public NavBLOB(Encoding encoding, string contents)
	{
		if (encoding == null)
		{
			throw new ArgumentNullException("encoding");
		}
		if (contents == null)
		{
			throw new ArgumentNullException("contents");
		}
		this.contents = new MemoryStream(encoding.GetBytes(contents));
	}

	public static NavBLOB Default()
	{
		return new NavBLOB();
	}

	public static bool BlobMagicOk(byte[] checkMagic)
	{
		for (int i = 0; i < 4; i++)
		{
			if (checkMagic[i] != BlobMagic[i])
			{
				return false;
			}
		}
		return true;
	}

	public void AssignFromBytes(byte[] bytes)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		Clear();
		if (bytes.Length != 0)
		{
			BlobWrite(0, bytes, 0, bytes.Length);
		}
	}

	public void AssignFromStream(Stream stream)
	{
		Clear();
		if (stream != null && stream.Length != 0L)
		{
			NCLStreamHelper.CopyStream(stream, new InternalStream(this), entireSource: true);
		}
	}

	public static int CompareBlobs(NavBLOB left, NavBLOB right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		if (left == right)
		{
			return 1;
		}
		if (left.IsZeroOrEmpty != right.IsZeroOrEmpty)
		{
			return 0;
		}
		if (left.IsZeroOrEmpty)
		{
			return 1;
		}
		if (left.IsInMemory)
		{
			if (right.IsInMemory)
			{
				if (!NCLStreamHelper.CompareStreams(new InternalStream(left), new InternalStream(right)))
				{
					return 0;
				}
				return 1;
			}
			if (left.IsDirty)
			{
				return 0;
			}
		}
		else if (right.IsInMemory && right.IsDirty)
		{
			return 0;
		}
		return -1;
	}

	internal Stream GetNewStream()
	{
		return new InternalStream(this);
	}

	public override byte[] GetBytes()
	{
		if (!IsInMemory)
		{
			if (sizeWhenNoContents <= 0)
			{
				return new byte[0];
			}
			return BitConverter.GetBytes(4L);
		}
		int length = GetLength();
		byte[] array = new byte[length];
		BlobRead(0, array, 0, length);
		return array;
	}

	public override int GetHashCode()
	{
		return -1;
	}

	public override bool Equals(NavValue other)
	{
		if (other is NavBLOB right)
		{
			return CompareBlobs(this, right) != 0;
		}
		return ThrowUnableToCompareException(other);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as NavValue);
	}

	public override object GetSqlWhereClauseValue(SqlDataType sqlDataType)
	{
		return (long)(IsZeroOrEmpty ? 0 : 1);
	}

	public override object GetSqlWritableValue(SqlDataType sqlDataType, int maxLength, bool compressed = false)
	{
		if (!IsInMemory || GetLength() == 0)
		{
			return DBNull.Value;
		}
		using Stream stream = GetNewStream();
		if (compressed)
		{
			using (MemoryStream memoryStream = MemoryStreamManager.GetStream())
			{
				memoryStream.Write(BlobMagic, 0, 4);
				using (DeflateStream destination = new DeflateStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
				{
					stream.CopyTo(destination);
				}
				return memoryStream.ToArray();
			}
		}
		byte[] array = new byte[stream.Length];
		stream.Read(array, 0, array.Length);
		return array;
	}

	public Stream GetStream()
	{
		if (exposedStream == null)
		{
			exposedStream = new InternalStream(this);
		}
		return exposedStream;
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (contents != null)
			{
				contents.Dispose();
				contents = null;
			}
			if (exposedStream != null)
			{
				exposedStream.Dispose();
				exposedStream = null;
			}
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public static NavBLOB CreateFromObject(object value)
	{
		if (value == null)
		{
			return Default();
		}
		if (value is byte[] contentsByteArray)
		{
			return new NavBLOB(contentsByteArray);
		}
		throw NavValue.CreateUnableToCreateFromObjectException(typeof(NavBLOB), value);
	}

	public static bool IsEqualWithBlobInMemoryWithNoContent(byte[] value)
	{
		if (value != null && value.Length == 8)
		{
			return BitConverter.ToInt64(value, 0) == 4;
		}
		return false;
	}

	public void Clear()
	{
		if (GetLength() != 0)
		{
			isDirty = true;
		}
		if (contents != null)
		{
			contents.Dispose();
		}
		contents = MemoryStreamManager.GetStream();
	}

	internal int GetLength()
	{
		if (contents != null)
		{
			return (int)contents.Length;
		}
		return sizeWhenNoContents;
	}

	private int BlobRead(int blobPosition, byte[] destination, int offset, int length)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (blobPosition < 0)
		{
			throw new ArgumentException(Lang.BlobPositionNegative);
		}
		if (offset < 0 || offset > destination.Length)
		{
			throw new ArgumentException(Lang.OffsetMustBeWithinDestination);
		}
		if (length < 0 || offset + length > destination.Length)
		{
			throw new ArgumentException(Lang.LengthMustBeWithinSource);
		}
		if (length > 0 && contents != null)
		{
			contents.Position = blobPosition;
			return contents.Read(destination, offset, length);
		}
		return 0;
	}

	private int BlobReadByte(int blobPosition)
	{
		if (contents != null)
		{
			contents.Position = blobPosition;
			return contents.ReadByte();
		}
		return -1;
	}

	private int BlobWrite(int blobPosition, byte[] source, int offset, int length)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (blobPosition < 0)
		{
			throw new ArgumentException(Lang.BlobPositionNegative);
		}
		if (offset < 0 || offset > source.Length)
		{
			throw new ArgumentException(Lang.OffsetMustBeWithinDestination);
		}
		if (length < 0 || offset + length > source.Length)
		{
			throw new ArgumentException(Lang.LengthMustBeWithinSource);
		}
		if (contents == null)
		{
			contents = MemoryStreamManager.GetStream(blobPosition + length);
		}
		contents.Position = blobPosition;
		contents.Write(source, offset, length);
		isDirty = true;
		return length;
	}

	private void BlobWriteByte(int blobPosition, byte value)
	{
		if (contents == null)
		{
			contents = MemoryStreamManager.GetStream(blobPosition + 16);
		}
		contents.Position = blobPosition;
		contents.WriteByte(value);
		isDirty = true;
	}

	private void BlobTruncate(int newLength)
	{
		if (newLength <= 0)
		{
			Clear();
		}
		else if (contents != null)
		{
			contents.SetLength(newLength);
			isDirty = true;
		}
	}

	internal NavBLOB DeepCopy()
	{
		NavBLOB navBLOB = new NavBLOB();
		if (IsInMemory)
		{
			navBLOB.AssignFromStream(GetNewStream());
		}
		navBLOB.sizeWhenNoContents = sizeWhenNoContents;
		navBLOB.isDirty = isDirty;
		return navBLOB;
	}
}
