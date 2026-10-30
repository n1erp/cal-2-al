using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Emit;

public struct EditAndContinueMethodDebugInformation
{
	internal readonly int MethodOrdinal;

	internal readonly ImmutableArray<LocalSlotDebugInfo> LocalSlots;

	internal readonly ImmutableArray<LambdaDebugInfo> Lambdas;

	internal readonly ImmutableArray<ClosureDebugInfo> Closures;

	private const byte SyntaxOffsetBaseline = byte.MaxValue;

	internal EditAndContinueMethodDebugInformation(int methodOrdinal, ImmutableArray<LocalSlotDebugInfo> localSlots, ImmutableArray<ClosureDebugInfo> closures, ImmutableArray<LambdaDebugInfo> lambdas)
	{
		MethodOrdinal = methodOrdinal;
		LocalSlots = localSlots;
		Lambdas = lambdas;
		Closures = closures;
	}

	public static EditAndContinueMethodDebugInformation Create(ImmutableArray<byte> compressedSlotMap, ImmutableArray<byte> compressedLambdaMap)
	{
		UncompressLambdaMap(compressedLambdaMap, out var methodOrdinal, out var closures, out var lambdas);
		return new EditAndContinueMethodDebugInformation(methodOrdinal, UncompressSlotMap(compressedSlotMap), closures, lambdas);
	}

	private static InvalidDataException CreateInvalidDataException(ImmutableArray<byte> data, int offset)
	{
		int num = Math.Max(0, offset - 512);
		int num2 = Math.Min(data.Length, offset + 512);
		byte[] array = new byte[offset - num];
		data.CopyTo(num, array, 0, array.Length);
		byte[] array2 = new byte[num2 - offset];
		data.CopyTo(offset, array2, 0, array2.Length);
		throw new InvalidDataException(string.Format(CodeAnalysisResources.InvalidDataAtOffset, offset, (num != 0) ? "..." : "", BitConverter.ToString(array), BitConverter.ToString(array2), (num2 != data.Length) ? "..." : ""));
	}

	private unsafe static ImmutableArray<LocalSlotDebugInfo> UncompressSlotMap(ImmutableArray<byte> compressedSlotMap)
	{
		if (compressedSlotMap.IsDefaultOrEmpty)
		{
			return default;
		}
		ArrayBuilder<LocalSlotDebugInfo> instance = ArrayBuilder<LocalSlotDebugInfo>.GetInstance();
		int num = -1;
		fixed (byte* buffer = &compressedSlotMap.ToArray()[0])
		{
			BlobReader blobReader = new BlobReader(buffer, compressedSlotMap.Length);
			while (blobReader.RemainingBytes > 0)
			{
				try
				{
					byte b = blobReader.ReadByte();
					switch (b)
					{
					case byte.MaxValue:
						num = -blobReader.ReadCompressedInteger();
						continue;
					case 0:
						instance.Add(new LocalSlotDebugInfo(SynthesizedLocalKind.LoweringTemp, default));
						continue;
					}
					SynthesizedLocalKind synthesizedKind = (SynthesizedLocalKind)((b & 0x3F) - 1);
					bool flag = (b & 0x80) != 0;
					int syntaxOffset = blobReader.ReadCompressedInteger() + num;
					int ordinal = (flag ? blobReader.ReadCompressedInteger() : 0);
					instance.Add(new LocalSlotDebugInfo(synthesizedKind, new LocalDebugId(syntaxOffset, ordinal)));
				}
				catch (BadImageFormatException)
				{
					throw CreateInvalidDataException(compressedSlotMap, blobReader.Offset);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal void SerializeLocalSlots(BlobBuilder writer)
	{
		int num = -1;
		foreach (LocalSlotDebugInfo localSlot in LocalSlots)
		{
			if (localSlot.Id.SyntaxOffset < num)
			{
				num = localSlot.Id.SyntaxOffset;
			}
		}
		if (num != -1)
		{
			writer.WriteByte(byte.MaxValue);
			writer.WriteCompressedInteger(-num);
		}
		foreach (LocalSlotDebugInfo localSlot2 in LocalSlots)
		{
			SynthesizedLocalKind synthesizedKind = localSlot2.SynthesizedKind;
			if (!synthesizedKind.IsLongLived())
			{
				writer.WriteByte(0);
				continue;
			}
			byte b = (byte)(synthesizedKind + 1);
			bool flag = localSlot2.Id.Ordinal > 0;
			if (flag)
			{
				b |= 0x80;
			}
			writer.WriteByte(b);
			writer.WriteCompressedInteger(localSlot2.Id.SyntaxOffset - num);
			if (flag)
			{
				writer.WriteCompressedInteger(localSlot2.Id.Ordinal);
			}
		}
	}

	private unsafe static void UncompressLambdaMap(ImmutableArray<byte> compressedLambdaMap, out int methodOrdinal, out ImmutableArray<ClosureDebugInfo> closures, out ImmutableArray<LambdaDebugInfo> lambdas)
	{
		methodOrdinal = -1;
		closures = default;
		lambdas = default;
		if (compressedLambdaMap.IsDefaultOrEmpty)
		{
			return;
		}
		ArrayBuilder<ClosureDebugInfo> instance = ArrayBuilder<ClosureDebugInfo>.GetInstance();
		ArrayBuilder<LambdaDebugInfo> instance2 = ArrayBuilder<LambdaDebugInfo>.GetInstance();
		fixed (byte* buffer = &compressedLambdaMap.ToArray()[0])
		{
			BlobReader blobReader = new BlobReader(buffer, compressedLambdaMap.Length);
			try
			{
				methodOrdinal = blobReader.ReadCompressedInteger() - 1;
				int num = -blobReader.ReadCompressedInteger();
				int num2 = blobReader.ReadCompressedInteger();
				for (int i = 0; i < num2; i++)
				{
					int num3 = blobReader.ReadCompressedInteger();
					DebugId closureId = new DebugId(instance.Count, 0);
					instance.Add(new ClosureDebugInfo(num3 + num, closureId));
				}
				while (blobReader.RemainingBytes > 0)
				{
					int num4 = blobReader.ReadCompressedInteger();
					int num5 = blobReader.ReadCompressedInteger() + -2;
					if (num5 >= num2)
					{
						throw CreateInvalidDataException(compressedLambdaMap, blobReader.Offset);
					}
					DebugId lambdaId = new DebugId(instance2.Count, 0);
					instance2.Add(new LambdaDebugInfo(num4 + num, lambdaId, num5));
				}
			}
			catch (BadImageFormatException)
			{
				throw CreateInvalidDataException(compressedLambdaMap, blobReader.Offset);
			}
		}
		closures = instance.ToImmutableAndFree();
		lambdas = instance2.ToImmutableAndFree();
	}

	internal void SerializeLambdaMap(BlobBuilder writer)
	{
		writer.WriteCompressedInteger(MethodOrdinal + 1);
		int num = -1;
		foreach (ClosureDebugInfo closure in Closures)
		{
			if (closure.SyntaxOffset < num)
			{
				num = closure.SyntaxOffset;
			}
		}
		foreach (LambdaDebugInfo lambda in Lambdas)
		{
			if (lambda.SyntaxOffset < num)
			{
				num = lambda.SyntaxOffset;
			}
		}
		writer.WriteCompressedInteger(-num);
		writer.WriteCompressedInteger(Closures.Length);
		foreach (ClosureDebugInfo closure2 in Closures)
		{
			writer.WriteCompressedInteger(closure2.SyntaxOffset - num);
		}
		foreach (LambdaDebugInfo lambda2 in Lambdas)
		{
			writer.WriteCompressedInteger(lambda2.SyntaxOffset - num);
			writer.WriteCompressedInteger(lambda2.ClosureOrdinal - -2);
		}
	}
}
