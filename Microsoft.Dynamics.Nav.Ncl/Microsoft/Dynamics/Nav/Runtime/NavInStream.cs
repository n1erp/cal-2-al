using System;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavInStream : NavStream, INavStreamReader, INavStreamMode, INavStreamTextEncoding
{
	public class Factory : ElementFactory<NavInStream>
	{
		public Factory(ITreeObject parentForArray)
			: base(parentForArray)
		{
		}

		public override NavInStream CreateElement()
		{
			return Default(ParentForElements);
		}

		public override void AssignElement(NavInStream destinationElement, NavInStream sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavInStream> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone);
		}
	}

	private BinaryReader binaryReader;

	private Decoder decoder;

	public bool ALIsEOS
	{
		get
		{
			if (InternalStreamChecked.CanSeek)
			{
				return Position >= Length;
			}
			throw new NavNCLNotSupportedOperationException();
		}
	}

	public TextEncoding TextEncoding
	{
		get
		{
			return Target.TextEncoding;
		}
		set
		{
			Target.TextEncoding = value;
		}
	}

	public override NavNclType NclType => NavNclType.NavInStream;

	private BinaryReader BinaryReader
	{
		get
		{
			if (binaryReader == null)
			{
				binaryReader = new BinaryReader(InternalStreamCheckedForRead, (Target.TextEncoding == TextEncoding.UTF16) ? Encoding.Unicode : Encoding.UTF8, leaveOpen: true);
			}
			return binaryReader;
		}
	}

	private Decoder TextDecoder
	{
		get
		{
			if (decoder == null)
			{
				decoder = ((Target.TextEncoding == TextEncoding.UTF16) ? Encoding.Unicode : Encoding.UTF8).GetDecoder();
				decoder.Fallback = new DecoderExceptionFallback();
			}
			return decoder;
		}
	}

	NavStreamMode INavStreamMode.Mode => NavStreamMode.Stream;

	TextEncoding INavStreamTextEncoding.TextEncoding => Target.TextEncoding;

	public void ALAssign(NavInStream other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		binaryReader = null;
		Assign(other);
	}

	public NavInStream ALByValue(ITreeObject parentOfResult)
	{
		NavInStream navInStream = new NavInStream(parentOfResult, null);
		navInStream.ALAssign(this);
		return navInStream;
	}

	public NavInStream(ITreeObject parent, INavStreamProvider navStreamProvider, TextEncoding textEncoding)
		: base(parent, navStreamProvider)
	{
		TextEncoding = textEncoding;
	}

	public NavInStream(ITreeObject parent, INavStreamProvider navStreamProvider)
		: this(parent, navStreamProvider, TextEncoding.MSDos)
	{
	}

	public static NavInStream Default(ITreeObject parent)
	{
		return new NavInStream(parent, null);
	}

	public override void Clear()
	{
		base.Clear();
		if (binaryReader != null)
		{
			binaryReader.Dispose();
			binaryReader = null;
		}
	}

	string INavStreamReader.ReadChar()
	{
		Stream internalStreamCheckedForRead = InternalStreamCheckedForRead;
		if (Target.TextEncoding != TextEncoding.UTF16 && Target.TextEncoding != TextEncoding.UTF8)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		if (internalStreamCheckedForRead.CanSeek)
		{
			internalStreamCheckedForRead.Seek(Position, SeekOrigin.Begin);
		}
		int num = 0;
		try
		{
			int num2 = 0;
			byte[] array = new byte[7];
			while (num < array.Length)
			{
				array[num++] = BinaryReader.ReadByte();
				int charCount = TextDecoder.GetCharCount(array, num2, num - num2);
				if (charCount > 0)
				{
					char[] array2 = new char[charCount];
					TextDecoder.GetChars(array, num2, num - num2, array2, 0, flush: true);
					if (Position + num2 != 0L || array2[0] != '\ufeff')
					{
						return new string(array2);
					}
					num2 += ((Target.TextEncoding == TextEncoding.UTF8) ? 3 : 2);
				}
			}
			throw new NavNCLInvalidStreamDataException();
		}
		catch (ArgumentException)
		{
			throw new NavNCLInvalidStreamDataException();
		}
		finally
		{
			Position = (internalStreamCheckedForRead.CanSeek ? internalStreamCheckedForRead.Position : (Position + num));
		}
	}

	int INavStreamReader.ReadByte()
	{
		Stream internalStreamCheckedForRead = InternalStreamCheckedForRead;
		if (internalStreamCheckedForRead.CanSeek)
		{
			internalStreamCheckedForRead.Seek(Position, SeekOrigin.Begin);
		}
		int num = 1;
		int num2;
		if (Target.TextEncoding == TextEncoding.UTF16 || Target.TextEncoding == TextEncoding.UTF8)
		{
			if (Position == 0L)
			{
				num2 = BinaryReader.ReadByte();
				num++;
				if (num2 == 254 || num2 == 255 || num2 == 239)
				{
					num2 = BinaryReader.ReadByte();
					num++;
					if (num2 == 187)
					{
						BinaryReader.ReadByte();
						num++;
					}
					num2 = BinaryReader.ReadByte();
					num++;
				}
			}
			else
			{
				num2 = BinaryReader.ReadByte();
			}
		}
		else
		{
			num2 = internalStreamCheckedForRead.ReadByte();
		}
		if (internalStreamCheckedForRead.CanSeek)
		{
			Position = internalStreamCheckedForRead.Position;
		}
		else if (num2 >= 0)
		{
			Position += num;
		}
		return num2;
	}

	int INavStreamReader.PeekByte()
	{
		Stream internalStreamCheckedForRead = InternalStreamCheckedForRead;
		if (internalStreamCheckedForRead.CanSeek)
		{
			internalStreamCheckedForRead.Seek(Position, SeekOrigin.Begin);
			int num = internalStreamCheckedForRead.ReadByte();
			if (num >= 0)
			{
				internalStreamCheckedForRead.Seek(-1L, SeekOrigin.Current);
			}
			return num;
		}
		throw new NavNCLNotSupportedOperationException();
	}

	byte[] INavStreamReader.ReadBytes(int length)
	{
		Stream internalStreamCheckedForRead = InternalStreamCheckedForRead;
		if (internalStreamCheckedForRead.CanSeek)
		{
			internalStreamCheckedForRead.Seek(Position, SeekOrigin.Begin);
		}
		byte[] array = ((Target.TextEncoding != TextEncoding.UTF16 && Target.TextEncoding != TextEncoding.UTF8) ? NCLStreamHelper.ReadToByteArray(internalStreamCheckedForRead, length) : BinaryReader.ReadBytes(length));
		if (internalStreamCheckedForRead.CanSeek)
		{
			Position = internalStreamCheckedForRead.Position;
		}
		else
		{
			Position += array.Length;
		}
		return array;
	}

	byte[] INavStreamReader.PeekBytes(int length)
	{
		Stream internalStreamCheckedForRead = InternalStreamCheckedForRead;
		if (internalStreamCheckedForRead.CanSeek)
		{
			internalStreamCheckedForRead.Seek(Position, SeekOrigin.Begin);
			byte[] array = NCLStreamHelper.ReadToByteArray(internalStreamCheckedForRead, length);
			if (array.Length >= 0)
			{
				internalStreamCheckedForRead.Seek(-array.Length, SeekOrigin.Current);
			}
			return array;
		}
		throw new NavNCLNotSupportedOperationException();
	}
}
