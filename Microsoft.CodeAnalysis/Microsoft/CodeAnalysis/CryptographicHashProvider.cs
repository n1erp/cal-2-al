using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class CryptographicHashProvider
{
	private ImmutableArray<byte> _lazySHA1Hash;

	private ImmutableArray<byte> _lazySHA256Hash;

	private ImmutableArray<byte> _lazySHA384Hash;

	private ImmutableArray<byte> _lazySHA512Hash;

	private ImmutableArray<byte> _lazyMD5Hash;

	internal const int Sha1HashSize = 20;

	internal abstract ImmutableArray<byte> ComputeHash(HashAlgorithm algorithm);

	internal ImmutableArray<byte> GetHash(AssemblyHashAlgorithm algorithmId)
	{
		using HashAlgorithm hashAlgorithm = TryGetAlgorithm(algorithmId);
		if (hashAlgorithm == null)
		{
			return ImmutableArray.Create<byte>();
		}
		switch (algorithmId)
		{
		case AssemblyHashAlgorithm.None:
		case AssemblyHashAlgorithm.Sha1:
			return GetHash(ref _lazySHA1Hash, hashAlgorithm);
		case AssemblyHashAlgorithm.Sha256:
			return GetHash(ref _lazySHA256Hash, hashAlgorithm);
		case AssemblyHashAlgorithm.Sha384:
			return GetHash(ref _lazySHA384Hash, hashAlgorithm);
		case AssemblyHashAlgorithm.Sha512:
			return GetHash(ref _lazySHA512Hash, hashAlgorithm);
		case AssemblyHashAlgorithm.MD5:
			return GetHash(ref _lazyMD5Hash, hashAlgorithm);
		default:
			throw ExceptionUtilities.UnexpectedValue(algorithmId);
		}
	}

	internal static int GetHashSize(SourceHashAlgorithm algorithmId)
	{
		return algorithmId switch
		{
			SourceHashAlgorithm.Sha1 => 20, 
			SourceHashAlgorithm.Sha256 => 32, 
			_ => throw ExceptionUtilities.UnexpectedValue(algorithmId), 
		};
	}

	internal static HashAlgorithm TryGetAlgorithm(SourceHashAlgorithm algorithmId)
	{
		return algorithmId switch
		{
			SourceHashAlgorithm.Sha1 => (HashAlgorithm)SHA1.Create(), 
			SourceHashAlgorithm.Sha256 => SHA256.Create(), 
			_ => null, 
		};
	}

	internal static HashAlgorithmName GetAlgorithmName(SourceHashAlgorithm algorithmId)
	{
		return algorithmId switch
		{
			SourceHashAlgorithm.Sha1 => HashAlgorithmName.SHA1, 
			SourceHashAlgorithm.Sha256 => HashAlgorithmName.SHA256, 
			_ => throw ExceptionUtilities.UnexpectedValue(algorithmId), 
		};
	}

	internal static HashAlgorithm TryGetAlgorithm(AssemblyHashAlgorithm algorithmId)
	{
		switch (algorithmId)
		{
		case AssemblyHashAlgorithm.None:
		case AssemblyHashAlgorithm.Sha1:
			return SHA1.Create();
		case AssemblyHashAlgorithm.Sha256:
			return SHA256.Create();
		case AssemblyHashAlgorithm.Sha384:
			return SHA384.Create();
		case AssemblyHashAlgorithm.Sha512:
			return SHA512.Create();
		case AssemblyHashAlgorithm.MD5:
			return MD5.Create();
		default:
			return null;
		}
	}

	internal static bool IsSupportedAlgorithm(AssemblyHashAlgorithm algorithmId)
	{
		if (algorithmId == AssemblyHashAlgorithm.None || (uint)(algorithmId - 32771) <= 1u || (uint)(algorithmId - 32780) <= 2u)
		{
			return true;
		}
		return false;
	}

	private ImmutableArray<byte> GetHash(ref ImmutableArray<byte> lazyHash, HashAlgorithm algorithm)
	{
		if (lazyHash.IsDefault)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyHash, ComputeHash(algorithm), default);
		}
		return lazyHash;
	}

	internal static ImmutableArray<byte> ComputeSha1(Stream stream)
	{
		if (stream != null)
		{
			stream.Seek(0L, SeekOrigin.Begin);
			using SHA1 sHA = SHA1.Create();
			return ImmutableArray.Create(sHA.ComputeHash(stream));
		}
		return ImmutableArray<byte>.Empty;
	}

	internal static ImmutableArray<byte> ComputeSha1(ImmutableArray<byte> bytes)
	{
		return ComputeSha1(bytes.ToArray());
	}

	internal static ImmutableArray<byte> ComputeSha1(byte[] bytes)
	{
		using SHA1 sHA = SHA1.Create();
		return ImmutableArray.Create(sHA.ComputeHash(bytes));
	}

	internal static ImmutableArray<byte> ComputeHash(HashAlgorithmName algorithmName, IEnumerable<Blob> bytes)
	{
		using IncrementalHash incrementalHash = IncrementalHash.CreateHash(algorithmName);
		incrementalHash.AppendData(bytes);
		return ImmutableArray.Create(incrementalHash.GetHashAndReset());
	}

	internal static ImmutableArray<byte> ComputeHash(HashAlgorithmName algorithmName, IEnumerable<ArraySegment<byte>> bytes)
	{
		using IncrementalHash incrementalHash = IncrementalHash.CreateHash(algorithmName);
		incrementalHash.AppendData(bytes);
		return ImmutableArray.Create(incrementalHash.GetHashAndReset());
	}
}
