using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.RuntimeMembers;

internal struct MemberDescriptor
{
	public readonly MemberFlags Flags;

	public readonly short DeclaringTypeId;

	public readonly ushort Arity;

	public readonly string Name;

	public readonly ImmutableArray<byte> Signature;

	public string DeclaringTypeMetadataName
	{
		get
		{
			if (DeclaringTypeId > 43)
			{
				return ((WellKnownType)DeclaringTypeId).GetMetadataName();
			}
			return ((SpecialType)DeclaringTypeId).GetMetadataName();
		}
	}

	public int ParametersCount
	{
		get
		{
			MemberFlags memberFlags = Flags & MemberFlags.KindMask;
			switch (memberFlags)
			{
			case MemberFlags.Method:
			case MemberFlags.Constructor:
			case MemberFlags.PropertyGet:
			case MemberFlags.Property:
				return Signature[0];
			default:
				throw ExceptionUtilities.UnexpectedValue(memberFlags);
			}
		}
	}

	public MemberDescriptor(MemberFlags Flags, short DeclaringTypeId, string Name, ImmutableArray<byte> Signature, ushort Arity = 0)
	{
		this.Flags = Flags;
		this.DeclaringTypeId = DeclaringTypeId;
		this.Name = Name;
		this.Arity = Arity;
		this.Signature = Signature;
	}

	internal static ImmutableArray<MemberDescriptor> InitializeFromStream(Stream stream, string[] nameTable)
	{
		int num = nameTable.Length;
		ImmutableArray<MemberDescriptor>.Builder builder = ImmutableArray.CreateBuilder<MemberDescriptor>(num);
		ImmutableArray<byte>.Builder builder2 = ImmutableArray.CreateBuilder<byte>();
		for (int i = 0; i < num; i++)
		{
			MemberFlags memberFlags = (MemberFlags)stream.ReadByte();
			short declaringTypeId = ReadTypeId(stream);
			ushort arity = (ushort)stream.ReadByte();
			if ((memberFlags & MemberFlags.Field) != 0)
			{
				ParseType(builder2, stream);
			}
			else
			{
				ParseMethodOrPropertySignature(builder2, stream);
			}
			builder.Add(new MemberDescriptor(memberFlags, declaringTypeId, nameTable[i], builder2.ToImmutable(), arity));
			builder2.Clear();
		}
		return builder.ToImmutable();
	}

	private static short ReadTypeId(Stream stream)
	{
		byte b = (byte)stream.ReadByte();
		if (b == byte.MaxValue)
		{
			return (short)(stream.ReadByte() + 255);
		}
		return b;
	}

	private static void ParseMethodOrPropertySignature(ImmutableArray<byte>.Builder builder, Stream stream)
	{
		int num = stream.ReadByte();
		builder.Add((byte)num);
		ParseType(builder, stream, allowByRef: true);
		for (int i = 0; i < num; i++)
		{
			ParseType(builder, stream, allowByRef: true);
		}
	}

	private static void ParseType(ImmutableArray<byte>.Builder builder, Stream stream, bool allowByRef = false)
	{
		while (true)
		{
			SignatureTypeCode signatureTypeCode = (SignatureTypeCode)stream.ReadByte();
			builder.Add((byte)signatureTypeCode);
			switch (signatureTypeCode)
			{
			default:
				throw ExceptionUtilities.UnexpectedValue(signatureTypeCode);
			case SignatureTypeCode.TypeHandle:
				ParseTypeHandle(builder, stream);
				return;
			case SignatureTypeCode.GenericTypeParameter:
			case SignatureTypeCode.GenericMethodParameter:
				builder.Add((byte)stream.ReadByte());
				return;
			case SignatureTypeCode.ByReference:
				if (allowByRef)
				{
					break;
				}
				goto default;
			case SignatureTypeCode.GenericTypeInstance:
				ParseGenericTypeInstance(builder, stream);
				return;
			case SignatureTypeCode.Pointer:
			case SignatureTypeCode.SZArray:
				break;
			}
			allowByRef = false;
		}
	}

	private static void ParseTypeHandle(ImmutableArray<byte>.Builder builder, Stream stream)
	{
		byte b = (byte)stream.ReadByte();
		builder.Add(b);
		if (b == byte.MaxValue)
		{
			byte item = (byte)stream.ReadByte();
			builder.Add(item);
		}
	}

	private static void ParseGenericTypeInstance(ImmutableArray<byte>.Builder builder, Stream stream)
	{
		ParseType(builder, stream);
		int num = stream.ReadByte();
		builder.Add((byte)num);
		for (int i = 0; i < num; i++)
		{
			ParseType(builder, stream);
		}
	}
}
