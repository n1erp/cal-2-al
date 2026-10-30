using System;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public sealed class EmbeddedText
{
	private sealed class CountingDeflateStream : DeflateStream
	{
		public int BytesWritten { get; private set; }

		public CountingDeflateStream(Stream stream, CompressionLevel compressionLevel, bool leaveOpen)
			: base(stream, compressionLevel, leaveOpen)
		{
		}

		public override void Write(byte[] array, int offset, int count)
		{
			base.Write(array, offset, count);
			checked
			{
				BytesWritten += count;
			}
		}

		public override void WriteByte(byte value)
		{
			((Stream)this).WriteByte(value);
			checked
			{
				BytesWritten++;
			}
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal const int CompressionThreshold = 200;

	public string FilePath { get; }

	public SourceHashAlgorithm ChecksumAlgorithm { get; }

	public ImmutableArray<byte> Checksum { get; }

	internal ImmutableArray<byte> Blob { get; }

	private EmbeddedText(string filePath, ImmutableArray<byte> checksum, SourceHashAlgorithm checksumAlgorithm, ImmutableArray<byte> blob)
	{
		FilePath = filePath;
		Checksum = checksum;
		ChecksumAlgorithm = checksumAlgorithm;
		Blob = blob;
	}

	public static EmbeddedText FromSource(string filePath, SourceText text)
	{
		ValidateFilePath(filePath);
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (!text.CanBeEmbedded)
		{
			throw new ArgumentException(CodeAnalysisResources.SourceTextCannotBeEmbedded, "text");
		}
		if (!text.PrecomputedEmbeddedTextBlob.IsDefault)
		{
			return new EmbeddedText(filePath, text.GetChecksum(), text.ChecksumAlgorithm, text.PrecomputedEmbeddedTextBlob);
		}
		return new EmbeddedText(filePath, text.GetChecksum(), text.ChecksumAlgorithm, CreateBlob(text));
	}

	public static EmbeddedText FromStream(string filePath, Stream stream, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
	{
		ValidateFilePath(filePath);
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanRead || !stream.CanSeek)
		{
			throw new ArgumentException(CodeAnalysisResources.StreamMustSupportReadAndSeek, "stream");
		}
		SourceText.ValidateChecksumAlgorithm(checksumAlgorithm);
		return new EmbeddedText(filePath, SourceText.CalculateChecksum(stream, checksumAlgorithm), checksumAlgorithm, CreateBlob(stream));
	}

	public static EmbeddedText FromBytes(string filePath, ArraySegment<byte> bytes, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
	{
		ValidateFilePath(filePath);
		if (bytes.Array == null)
		{
			throw new ArgumentNullException("bytes");
		}
		SourceText.ValidateChecksumAlgorithm(checksumAlgorithm);
		return new EmbeddedText(filePath, SourceText.CalculateChecksum(bytes.Array, bytes.Offset, bytes.Count, checksumAlgorithm), checksumAlgorithm, CreateBlob(bytes));
	}

	private static void ValidateFilePath(string filePath)
	{
		if (filePath == null)
		{
			throw new ArgumentNullException("filePath");
		}
		if (filePath.Length == 0)
		{
			throw new ArgumentException(CodeAnalysisResources.ArgumentCannotBeEmpty, "filePath");
		}
	}

	internal static ImmutableArray<byte> CreateBlob(Stream stream)
	{
		long length = stream.Length;
		if (length > int.MaxValue)
		{
			throw new IOException(CodeAnalysisResources.StreamIsTooLong);
		}
		stream.Seek(0L, SeekOrigin.Begin);
		int num = (int)length;
		if (num < 200)
		{
			using (PooledBlobBuilder pooledBlobBuilder = PooledBlobBuilder.GetInstance())
			{
				pooledBlobBuilder.WriteInt32(0);
				int num2 = pooledBlobBuilder.TryWriteBytes(stream, num);
				if (num != num2)
				{
					throw new EndOfStreamException();
				}
				return pooledBlobBuilder.ToImmutableArray();
			}
		}
		using BlobBuildingStream blobBuildingStream = BlobBuildingStream.GetInstance();
		blobBuildingStream.WriteInt32(num);
		using (CountingDeflateStream countingDeflateStream = new CountingDeflateStream(blobBuildingStream, CompressionLevel.Optimal, leaveOpen: true))
		{
			stream.CopyTo(countingDeflateStream);
			if (num != countingDeflateStream.BytesWritten)
			{
				throw new EndOfStreamException();
			}
		}
		return blobBuildingStream.ToImmutableArray();
	}

	internal static ImmutableArray<byte> CreateBlob(ArraySegment<byte> bytes)
	{
		if (bytes.Count < 200)
		{
			using (PooledBlobBuilder pooledBlobBuilder = PooledBlobBuilder.GetInstance())
			{
				pooledBlobBuilder.WriteInt32(0);
				pooledBlobBuilder.WriteBytes(bytes.Array, bytes.Offset, bytes.Count);
				return pooledBlobBuilder.ToImmutableArray();
			}
		}
		using BlobBuildingStream blobBuildingStream = BlobBuildingStream.GetInstance();
		blobBuildingStream.WriteInt32(bytes.Count);
		using (CountingDeflateStream countingDeflateStream = new CountingDeflateStream(blobBuildingStream, CompressionLevel.Optimal, leaveOpen: true))
		{
			countingDeflateStream.Write(bytes.Array, bytes.Offset, bytes.Count);
		}
		return blobBuildingStream.ToImmutableArray();
	}

	private static ImmutableArray<byte> CreateBlob(SourceText text)
	{
		int num;
		try
		{
			num = text.Encoding.GetMaxByteCount(text.Length);
		}
		catch (ArgumentOutOfRangeException)
		{
			num = int.MaxValue;
		}
		using BlobBuildingStream blobBuildingStream = BlobBuildingStream.GetInstance();
		if (num < 200)
		{
			blobBuildingStream.WriteInt32(0);
			using StreamWriter textWriter = new StreamWriter(blobBuildingStream, text.Encoding, Math.Max(1, text.Length), leaveOpen: true);
			text.Write(textWriter);
		}
		else
		{
			Blob blob = blobBuildingStream.ReserveBytes(4);
			using CountingDeflateStream countingDeflateStream = new CountingDeflateStream(blobBuildingStream, CompressionLevel.Optimal, leaveOpen: true);
			using (StreamWriter textWriter2 = new StreamWriter(countingDeflateStream, text.Encoding, 1024, leaveOpen: true))
			{
				text.Write(textWriter2);
			}
			new BlobWriter(blob).WriteInt32(countingDeflateStream.BytesWritten);
		}
		return blobBuildingStream.ToImmutableArray();
	}

	internal DebugSourceInfo GetDebugSourceInfo()
	{
		return new DebugSourceInfo(Checksum, ChecksumAlgorithm, Blob);
	}
}
