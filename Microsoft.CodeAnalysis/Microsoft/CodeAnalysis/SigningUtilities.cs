using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class SigningUtilities
{
	internal static byte[] CalculateRsaSignature(IEnumerable<Blob> content, RSAParameters privateKey)
	{
		byte[] hash = CalculateSha1(content);
		using RSA rSA = RSA.Create();
		rSA.ImportParameters(privateKey);
		byte[] array = rSA.SignHash(hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
		Array.Reverse((Array)array);
		return array;
	}

	internal static byte[] CalculateSha1(IEnumerable<Blob> content)
	{
		using IncrementalHash incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
		incrementalHash.AppendData(content);
		return incrementalHash.GetHashAndReset();
	}

	internal static int CalculateStrongNameSignatureSize(CommonPEModuleBuilder module, RSAParameters? privateKey)
	{
		ISourceAssemblySymbolInternal sourceAssemblyOpt = module.SourceAssemblyOpt;
		if (sourceAssemblyOpt == null && !privateKey.HasValue)
		{
			return 0;
		}
		int num = 0;
		if (num == 0 && sourceAssemblyOpt != null)
		{
			num = ((sourceAssemblyOpt.SignatureKey != null) ? (sourceAssemblyOpt.SignatureKey.Length / 2) : 0);
		}
		if (num == 0 && sourceAssemblyOpt != null)
		{
			num = sourceAssemblyOpt.Identity.PublicKey.Length;
		}
		if (num == 0 && privateKey.HasValue)
		{
			num = privateKey.Value.Modulus.Length;
		}
		if (num == 0)
		{
			return 0;
		}
		if (num >= 160)
		{
			return num - 32;
		}
		return 128;
	}
}
