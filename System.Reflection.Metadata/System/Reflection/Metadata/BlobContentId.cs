using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Internal;

namespace System.Reflection.Metadata;

public readonly struct BlobContentId : IEquatable<BlobContentId>
{
	private const int Size = 20;

	public Guid Guid { get; }

	public uint Stamp { get; }

	public bool IsDefault
	{
		get
		{
			if (Guid == default(Guid))
			{
				return Stamp == 0;
			}
			return false;
		}
	}

	public BlobContentId(Guid guid, uint stamp)
	{
		Guid = guid;
		Stamp = stamp;
	}

	public BlobContentId(ImmutableArray<byte> id)
		: this(ImmutableByteArrayInterop.DangerousGetUnderlyingArray(id))
	{
	}

	public unsafe BlobContentId(byte[] id)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (id.Length != 20)
		{
			throw new ArgumentException(SR.Format(SR.UnexpectedArrayLength, 20), "id");
		}
		fixed (byte* buffer = &id[0])
		{
			BlobReader blobReader = new BlobReader(buffer, id.Length);
			Guid = blobReader.ReadGuid();
			Stamp = blobReader.ReadUInt32();
		}
	}

	public static BlobContentId FromHash(ImmutableArray<byte> hashCode)
	{
		return FromHash(ImmutableByteArrayInterop.DangerousGetUnderlyingArray(hashCode));
	}

	public unsafe static BlobContentId FromHash(byte[] hashCode)
	{
		if (hashCode == null)
		{
			throw new ArgumentNullException("hashCode");
		}
		if (hashCode.Length < 20)
		{
			throw new ArgumentException(SR.Format(SR.HashTooShort, 20), "hashCode");
		}
		Guid guid = default;
		byte* ptr = (byte*)(&guid);
		for (int i = 0; i < 16; i++)
		{
			ptr[i] = hashCode[i];
		}
		ptr[7] = (byte)((ptr[7] & 0xF) | 0x40);
		ptr[8] = (byte)((ptr[8] & 0x3F) | 0x80);
		uint stamp = (uint)(int.MinValue | ((hashCode[19] << 24) | (hashCode[18] << 16) | (hashCode[17] << 8) | hashCode[16]));
		return new BlobContentId(guid, stamp);
	}

	public static Func<IEnumerable<Blob>, BlobContentId> GetTimeBasedProvider()
	{
		uint timestamp = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		return (IEnumerable<Blob> content) => new BlobContentId(Guid.NewGuid(), timestamp);
	}

	public bool Equals(BlobContentId other)
	{
		if (Guid == other.Guid)
		{
			return Stamp == other.Stamp;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BlobContentId)
		{
			return Equals((BlobContentId)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Stamp, Guid.GetHashCode());
	}

	public static bool operator ==(BlobContentId left, BlobContentId right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BlobContentId left, BlobContentId right)
	{
		return !left.Equals(right);
	}
}
