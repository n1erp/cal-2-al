using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavOutStream : NavStream, INavStreamWriter, INavStreamMode, INavStreamTextEncoding
{
	public class Factory : ElementFactory<NavOutStream>
	{
		public Factory(ITreeObject parentForArray)
			: base(parentForArray)
		{
		}

		public override NavOutStream CreateElement()
		{
			return Default(ParentForElements);
		}

		public override void AssignElement(NavOutStream destinationElement, NavOutStream sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavOutStream> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone);
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

	public override NavNclType NclType => NavNclType.NavOutStream;

	NavStreamMode INavStreamMode.Mode => NavStreamMode.Stream;

	TextEncoding INavStreamTextEncoding.TextEncoding => Target.TextEncoding;

	public void ALAssign(NavOutStream other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		Assign(other);
	}

	public NavOutStream ALByValue(ITreeObject parentOfResult)
	{
		NavOutStream navOutStream = new NavOutStream(parentOfResult, null);
		navOutStream.ALAssign(this);
		return navOutStream;
	}

	public NavOutStream(ITreeObject parent, INavStreamProvider value, TextEncoding textEncoding)
		: base(parent, value)
	{
		TextEncoding = textEncoding;
	}

	public NavOutStream(ITreeObject parent, INavStreamProvider value)
		: this(parent, value, TextEncoding.MSDos)
	{
	}

	public static NavOutStream Default(ITreeObject parent)
	{
		return new NavOutStream(parent, null);
	}

	void INavStreamWriter.WriteByte(byte value)
	{
		Stream internalStreamCheckedForWrite = InternalStreamCheckedForWrite;
		if (internalStreamCheckedForWrite.CanSeek)
		{
			internalStreamCheckedForWrite.Seek(Position, SeekOrigin.Begin);
		}
		internalStreamCheckedForWrite.WriteByte(value);
		if (internalStreamCheckedForWrite.CanSeek)
		{
			Position = (int)internalStreamCheckedForWrite.Position;
		}
		else
		{
			Position++;
		}
	}

	void INavStreamWriter.WriteBytes(byte[] bytes, int length)
	{
		Stream internalStreamCheckedForWrite = InternalStreamCheckedForWrite;
		if (internalStreamCheckedForWrite.CanSeek)
		{
			internalStreamCheckedForWrite.Seek(Position, SeekOrigin.Begin);
		}
		internalStreamCheckedForWrite.Write(bytes, 0, length);
		if (internalStreamCheckedForWrite.CanSeek)
		{
			Position = (int)internalStreamCheckedForWrite.Position;
		}
		else
		{
			Position += length;
		}
	}
}
