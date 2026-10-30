using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis.Collections;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class CryptoBlobParser
{
	private enum AlgorithmClass
	{
		Signature = 1,
		Hash = 4
	}

	private enum AlgorithmSubId
	{
		Sha1Hash = 4,
		MacHash,
		RipeMdHash,
		RipeMd160Hash,
		Ssl3ShaMD5Hash,
		HmacHash,
		Tls1PrfHash,
		HashReplacOwfHash,
		Sha256Hash,
		Sha384Hash,
		Sha512Hash
	}

	private struct AlgorithmId
	{
		private const int AlgorithmClassOffset = 13;

		private const int AlgorithmClassMask = 7;

		private const int AlgorithmSubIdOffset = 0;

		private const int AlgorithmSubIdMask = 511;

		private readonly uint _flags;

		public const int RsaSign = 9216;

		public const int Sha = 32772;

		public bool IsSet => _flags != 0;

		public AlgorithmClass Class => (AlgorithmClass)((_flags >> 13) & 7);

		public AlgorithmSubId SubId => (AlgorithmSubId)(_flags & 0x1FF);

		public AlgorithmId(uint flags)
		{
			_flags = flags;
		}
	}

	private static readonly ImmutableArray<byte> s_ecmaKey = ImmutableArray.Create(new byte[16]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 4, 0,
		0, 0, 0, 0, 0, 0
	});

	private const int SnPublicKeyBlobSize = 13;

	private const byte PublicKeyBlobId = 6;

	private const byte PrivateKeyBlobId = 7;

	internal const int s_publicKeyHeaderSize = 12;

	private const int BlobHeaderSize = 8;

	private const int RsaPubKeySize = 12;

	private const uint RSA1 = 826364754u;

	private const uint RSA2 = 843141970u;

	private const int s_offsetToKeyData = 20;

	internal static bool IsValidPublicKey(ImmutableArray<byte> blob)
	{
		if (blob.IsDefault || blob.Length < 13)
		{
			return false;
		}
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(blob.DangerousGetUnderlyingArray()));
		uint flags = binaryReader.ReadUInt32();
		uint flags2 = binaryReader.ReadUInt32();
		uint num = binaryReader.ReadUInt32();
		byte b = binaryReader.ReadByte();
		if (blob.Length != 12 + num)
		{
			return false;
		}
		if (ByteSequenceComparer.Equals(blob, s_ecmaKey))
		{
			return true;
		}
		if (b != 6)
		{
			return false;
		}
		AlgorithmId algorithmId = new AlgorithmId(flags);
		if (algorithmId.IsSet && algorithmId.Class != AlgorithmClass.Signature)
		{
			return false;
		}
		AlgorithmId algorithmId2 = new AlgorithmId(flags2);
		if (algorithmId2.IsSet && (algorithmId2.Class != AlgorithmClass.Hash || algorithmId2.SubId < AlgorithmSubId.Sha1Hash))
		{
			return false;
		}
		return true;
	}

	private static ImmutableArray<byte> CreateSnPublicKeyBlob(byte type, byte version, uint algId, uint magic, uint bitLen, uint pubExp, byte[] pubKeyData)
	{
		BlobWriter blobWriter = new BlobWriter(32 + pubKeyData.Length);
		blobWriter.WriteUInt32(9216u);
		blobWriter.WriteUInt32(32772u);
		blobWriter.WriteUInt32((uint)(20 + pubKeyData.Length));
		blobWriter.WriteByte(type);
		blobWriter.WriteByte(version);
		blobWriter.WriteUInt16(0);
		blobWriter.WriteUInt32(algId);
		blobWriter.WriteUInt32(magic);
		blobWriter.WriteUInt32(bitLen);
		blobWriter.WriteUInt32(pubExp);
		blobWriter.WriteBytes(pubKeyData);
		return blobWriter.ToImmutableArray();
	}

	public static bool TryParseKey(ImmutableArray<byte> blob, out ImmutableArray<byte> snKey, out RSAParameters? privateKey)
	{
		privateKey = null;
		snKey = default;
		byte[] array = blob.DangerousGetUnderlyingArray();
		if (IsValidPublicKey(blob))
		{
			snKey = blob;
			return true;
		}
		if (blob.Length < 20)
		{
			return false;
		}
		try
		{
			using (new MemoryStream(array))
			{
				using BinaryReader binaryReader = new BinaryReader(new MemoryStream(array));
				byte b = binaryReader.ReadByte();
				byte version = binaryReader.ReadByte();
				binaryReader.ReadUInt16();
				uint algId = binaryReader.ReadUInt32();
				uint num = binaryReader.ReadUInt32();
				uint num2 = binaryReader.ReadUInt32();
				uint pubExp = binaryReader.ReadUInt32();
				int num3 = (int)(num2 / 8);
				if (blob.Length - 20 < num3)
				{
					return false;
				}
				byte[] pubKeyData = binaryReader.ReadBytes(num3);
				if ((b != 7 || num != 843141970) && (b != 6 || num != 826364754))
				{
					return false;
				}
				if (b == 7)
				{
					privateKey = array.ToRSAParameters(includePrivateParameters: true);
					algId = 9216u;
				}
				snKey = CreateSnPublicKeyBlob(6, version, algId, 826364754u, num2, pubExp, pubKeyData);
				return true;
			}
		}
		catch (Exception)
		{
			return false;
		}
	}

	internal static RSAParameters ToRSAParameters(this byte[] cspBlob, bool includePrivateParameters)
	{
		BinaryReader binaryReader = new BinaryReader(new MemoryStream(cspBlob));
		binaryReader.ReadByte();
		binaryReader.ReadByte();
		binaryReader.ReadUInt16();
		binaryReader.ReadInt32();
		binaryReader.ReadInt32();
		int num = binaryReader.ReadInt32() / 8;
		int count = (num + 1) / 2;
		uint exponent = binaryReader.ReadUInt32();
		RSAParameters result = new RSAParameters
		{
			Exponent = ExponentAsBytes(exponent),
			Modulus = binaryReader.ReadReversed(num)
		};
		if (includePrivateParameters)
		{
			result.P = binaryReader.ReadReversed(count);
			result.Q = binaryReader.ReadReversed(count);
			result.DP = binaryReader.ReadReversed(count);
			result.DQ = binaryReader.ReadReversed(count);
			result.InverseQ = binaryReader.ReadReversed(count);
			result.D = binaryReader.ReadReversed(num);
		}
		return result;
	}

	private static byte[] ExponentAsBytes(uint exponent)
	{
		if (exponent > 255)
		{
			if (exponent > 65535)
			{
				if (exponent > 16777215)
				{
					return new byte[4]
					{
						(byte)(exponent >> 24),
						(byte)(exponent >> 16),
						(byte)(exponent >> 8),
						(byte)exponent
					};
				}
				return new byte[3]
				{
					(byte)(exponent >> 16),
					(byte)(exponent >> 8),
					(byte)exponent
				};
			}
			return new byte[2]
			{
				(byte)(exponent >> 8),
				(byte)exponent
			};
		}
		return new byte[1] { (byte)exponent };
	}

	private static byte[] ReadReversed(this BinaryReader br, int count)
	{
		byte[] array = br.ReadBytes(count);
		Array.Reverse((Array)array);
		return array;
	}
}
