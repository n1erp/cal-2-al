using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;

namespace Microsoft.Cci;

internal class DynamicAnalysisDataWriter
{
	private struct DocumentRow
	{
		public BlobHandle Name;

		public GuidHandle HashAlgorithm;

		public BlobHandle Hash;
	}

	private struct MethodRow
	{
		public BlobHandle Spans;
	}

	private struct Sizes(int blobHeapSize, int guidHeapSize)
	{
		public readonly int BlobHeapSize = blobHeapSize;

		public readonly int GuidHeapSize = guidHeapSize;

		public readonly int BlobIndexSize = ((blobHeapSize <= 65535) ? 2 : 4);

		public readonly int GuidIndexSize = ((guidHeapSize <= 65535) ? 2 : 4);
	}

	private readonly Dictionary<ImmutableArray<byte>, BlobHandle> _blobs;

	private int _blobHeapSize;

	private readonly Dictionary<Guid, GuidHandle> _guids;

	private readonly BlobBuilder _guidWriter;

	private readonly List<DocumentRow> _documentTable;

	private readonly List<MethodRow> _methodTable;

	private readonly Dictionary<DebugSourceDocument, int> _documentIndex;

	private static readonly char[] s_separator1 = new char[1] { '/' };

	private static readonly char[] s_separator2 = new char[1] { '\\' };

	public DynamicAnalysisDataWriter(int documentCountEstimate, int methodCountEstimate)
	{
		_blobs = new Dictionary<ImmutableArray<byte>, BlobHandle>(1 + methodCountEstimate + 4 * documentCountEstimate, ByteSequenceComparer.Instance);
		_guids = new Dictionary<Guid, GuidHandle>(documentCountEstimate);
		_guidWriter = new BlobBuilder(16 * documentCountEstimate);
		_documentTable = new List<DocumentRow>(documentCountEstimate);
		_documentIndex = new Dictionary<DebugSourceDocument, int>(documentCountEstimate);
		_methodTable = new List<MethodRow>(methodCountEstimate);
		_blobs.Add(ImmutableArray<byte>.Empty, default);
		_blobHeapSize = 1;
	}

	internal void SerializeMethodDynamicAnalysisData(IMethodBody bodyOpt)
	{
		DynamicAnalysisMethodBodyData dynamicAnalysisMethodBodyData = bodyOpt?.DynamicAnalysisData;
		if (dynamicAnalysisMethodBodyData == null)
		{
			_methodTable.Add(default);
			return;
		}
		BlobHandle spans = SerializeSpans(dynamicAnalysisMethodBodyData.Spans, _documentIndex);
		_methodTable.Add(new MethodRow
		{
			Spans = spans
		});
	}

	private BlobHandle GetOrAddBlob(BlobBuilder builder)
	{
		return GetOrAddBlob(builder.ToImmutableArray());
	}

	private BlobHandle GetOrAddBlob(ImmutableArray<byte> blob)
	{
		if (!_blobs.TryGetValue(blob, out var value))
		{
			value = MetadataTokens.BlobHandle(_blobHeapSize);
			_blobs.Add(blob, value);
			_blobHeapSize += GetCompressedIntegerLength(blob.Length) + blob.Length;
		}
		return value;
	}

	private static int GetCompressedIntegerLength(int length)
	{
		if (length > 127)
		{
			if (length > 16383)
			{
				return 4;
			}
			return 2;
		}
		return 1;
	}

	private GuidHandle GetOrAddGuid(Guid guid)
	{
		if (guid == Guid.Empty)
		{
			return default;
		}
		if (_guids.TryGetValue(guid, out var value))
		{
			return value;
		}
		value = MetadataTokens.GuidHandle((_guidWriter.Count >> 4) + 1);
		_guids.Add(guid, value);
		_guidWriter.WriteBytes(guid.ToByteArray());
		return value;
	}

	private BlobHandle SerializeSpans(ImmutableArray<SourceSpan> spans, Dictionary<DebugSourceDocument, int> documentIndex)
	{
		if (spans.Length == 0)
		{
			return default;
		}
		BlobBuilder blobBuilder = new BlobBuilder(4 + spans.Length * 4);
		int num = -1;
		int num2 = -1;
		DebugSourceDocument debugSourceDocument = spans[0].Document;
		blobBuilder.WriteCompressedInteger(GetOrAddDocument(debugSourceDocument, documentIndex));
		for (int i = 0; i < spans.Length; i++)
		{
			DebugSourceDocument document = spans[i].Document;
			if (debugSourceDocument != document)
			{
				blobBuilder.WriteInt16(0);
				blobBuilder.WriteCompressedInteger(GetOrAddDocument(document, documentIndex));
				debugSourceDocument = document;
			}
			SerializeDeltaLinesAndColumns(blobBuilder, spans[i]);
			if (num < 0)
			{
				blobBuilder.WriteCompressedInteger(spans[i].StartLine);
				blobBuilder.WriteCompressedInteger(spans[i].StartColumn);
			}
			else
			{
				blobBuilder.WriteCompressedSignedInteger(spans[i].StartLine - num);
				blobBuilder.WriteCompressedSignedInteger(spans[i].StartColumn - num2);
			}
			num = spans[i].StartLine;
			num2 = spans[i].StartColumn;
		}
		return GetOrAddBlob(blobBuilder);
	}

	private void SerializeDeltaLinesAndColumns(BlobBuilder writer, SourceSpan span)
	{
		int num = span.EndLine - span.StartLine;
		int value = span.EndColumn - span.StartColumn;
		writer.WriteCompressedInteger(num);
		if (num == 0)
		{
			writer.WriteCompressedInteger(value);
		}
		else
		{
			writer.WriteCompressedSignedInteger(value);
		}
	}

	internal int GetOrAddDocument(DebugSourceDocument document)
	{
		return GetOrAddDocument(document, _documentIndex);
	}

	private int GetOrAddDocument(DebugSourceDocument document, Dictionary<DebugSourceDocument, int> index)
	{
		if (!index.TryGetValue(document, out var value))
		{
			value = _documentTable.Count + 1;
			index.Add(document, value);
			DebugSourceInfo sourceInfo = document.GetSourceInfo();
			_documentTable.Add(new DocumentRow
			{
				Name = SerializeDocumentName(document.Location),
				HashAlgorithm = (sourceInfo.Checksum.IsDefault ? default(GuidHandle) : GetOrAddGuid(sourceInfo.ChecksumAlgorithmId)),
				Hash = (sourceInfo.Checksum.IsDefault ? default(BlobHandle) : GetOrAddBlob(sourceInfo.Checksum))
			});
		}
		return value;
	}

	private BlobHandle SerializeDocumentName(string name)
	{
		int num = Count(name, s_separator1[0]);
		int num2 = Count(name, s_separator2[0]);
		char[] array = ((num >= num2) ? s_separator1 : s_separator2);
		BlobBuilder blobBuilder = new BlobBuilder(1 + Math.Max(num, num2) * 2);
		blobBuilder.WriteByte((byte)array[0]);
		string[] array2 = name.Split(array);
		foreach (string s in array2)
		{
			BlobHandle orAddBlob = GetOrAddBlob(ImmutableArray.Create(MetadataWriter.s_utf8Encoding.GetBytes(s)));
			blobBuilder.WriteCompressedInteger(MetadataTokens.GetHeapOffset(orAddBlob));
		}
		return GetOrAddBlob(blobBuilder);
	}

	private static int Count(string str, char c)
	{
		int num = 0;
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] == c)
			{
				num++;
			}
		}
		return num;
	}

	internal void SerializeMetadataTables(BlobBuilder writer)
	{
		Sizes sizes = new Sizes(_blobHeapSize, _guidWriter.Count);
		SerializeHeader(writer, sizes);
		SerializeDocumentTable(writer, sizes);
		SerializeMethodTable(writer, sizes);
		writer.LinkSuffix(_guidWriter);
		WriteBlobHeap(writer);
	}

	private void WriteBlobHeap(BlobBuilder builder)
	{
		BlobWriter blobWriter = new BlobWriter(builder.ReserveBytes(_blobHeapSize));
		foreach (KeyValuePair<ImmutableArray<byte>, BlobHandle> blob in _blobs)
		{
			int heapOffset = MetadataTokens.GetHeapOffset(blob.Value);
			ImmutableArray<byte> key = blob.Key;
			blobWriter.Offset = heapOffset;
			blobWriter.WriteCompressedInteger(key.Length);
			blobWriter.WriteBytes(key);
		}
	}

	private void SerializeHeader(BlobBuilder writer, Sizes sizes)
	{
		writer.WriteByte(68);
		writer.WriteByte(65);
		writer.WriteByte(77);
		writer.WriteByte(68);
		writer.WriteByte(0);
		writer.WriteByte(2);
		writer.WriteInt32(_documentTable.Count);
		writer.WriteInt32(_methodTable.Count);
		writer.WriteInt32(sizes.GuidHeapSize);
		writer.WriteInt32(sizes.BlobHeapSize);
	}

	private void SerializeDocumentTable(BlobBuilder writer, Sizes sizes)
	{
		foreach (DocumentRow item in _documentTable)
		{
			writer.WriteReference(MetadataTokens.GetHeapOffset(item.Name), sizes.BlobIndexSize == 2);
			writer.WriteReference(MetadataTokens.GetHeapOffset(item.HashAlgorithm), sizes.GuidIndexSize == 2);
			writer.WriteReference(MetadataTokens.GetHeapOffset(item.Hash), sizes.BlobIndexSize == 2);
		}
	}

	private void SerializeMethodTable(BlobBuilder writer, Sizes sizes)
	{
		foreach (MethodRow item in _methodTable)
		{
			writer.WriteReference(MetadataTokens.GetHeapOffset(item.Spans), sizes.BlobIndexSize == 2);
		}
	}
}
