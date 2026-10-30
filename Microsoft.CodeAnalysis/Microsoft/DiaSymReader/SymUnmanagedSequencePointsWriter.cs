using System;

namespace Microsoft.DiaSymReader;

internal sealed class SymUnmanagedSequencePointsWriter
{
	private readonly SymUnmanagedWriter _writer;

	private int _currentDocumentIndex;

	private int _count;

	private int[] _offsets;

	private int[] _startLines;

	private int[] _startColumns;

	private int[] _endLines;

	private int[] _endColumns;

	public SymUnmanagedSequencePointsWriter(SymUnmanagedWriter writer, int capacity = 64)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		_writer = writer ?? throw new ArgumentNullException("writer");
		_currentDocumentIndex = -1;
		_offsets = new int[capacity];
		_startLines = new int[capacity];
		_startColumns = new int[capacity];
		_endLines = new int[capacity];
		_endColumns = new int[capacity];
	}

	private void EnsureCapacity(int length)
	{
		if (length > _offsets.Length)
		{
			int newSize = Math.Max(length, (_offsets.Length + 1) * 2);
			Array.Resize(ref _offsets, newSize);
			Array.Resize(ref _startLines, newSize);
			Array.Resize(ref _startColumns, newSize);
			Array.Resize(ref _endLines, newSize);
			Array.Resize(ref _endColumns, newSize);
		}
	}

	private void Clear()
	{
		_currentDocumentIndex = -1;
		_count = 0;
	}

	public void Add(int documentIndex, int offset, int startLine, int startColumn, int endLine, int endColumn)
	{
		if (documentIndex < 0)
		{
			throw new ArgumentOutOfRangeException("documentIndex");
		}
		if (_currentDocumentIndex != documentIndex)
		{
			if (_currentDocumentIndex != -1)
			{
				Flush();
			}
			_currentDocumentIndex = documentIndex;
		}
		int num = _count++;
		EnsureCapacity(_count);
		_offsets[num] = offset;
		_startLines[num] = startLine;
		_startColumns[num] = startColumn;
		_endLines[num] = endLine;
		_endColumns[num] = endColumn;
	}

	public void Flush()
	{
		if (_count > 0)
		{
			_writer.DefineSequencePoints(_currentDocumentIndex, _count, _offsets, _startLines, _startColumns, _endLines, _endColumns);
		}
		Clear();
	}
}
