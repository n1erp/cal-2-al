using System;
using CommandLine.Infrastructure;

namespace CommandLine.Parsing;

internal sealed class StringArrayEnumerator : IArgumentEnumerator
{
	private readonly int _endIndex;

	private readonly string[] _data;

	private int _index;

	public string Current
	{
		get
		{
			if (_index == -1)
			{
				throw new InvalidOperationException();
			}
			if (_index >= _endIndex)
			{
				throw new InvalidOperationException();
			}
			return _data[_index];
		}
	}

	public string Next
	{
		get
		{
			if (_index == -1)
			{
				throw new InvalidOperationException();
			}
			if (_index > _endIndex)
			{
				throw new InvalidOperationException();
			}
			if (IsLast)
			{
				return null;
			}
			return _data[_index + 1];
		}
	}

	public bool IsLast => _index == _endIndex - 1;

	public StringArrayEnumerator(string[] value)
	{
		Assumes.NotNull(value, "value");
		_data = value;
		_index = -1;
		_endIndex = value.Length;
	}

	public bool MoveNext()
	{
		if (_index < _endIndex)
		{
			_index++;
			return _index < _endIndex;
		}
		return false;
	}

	public string GetRemainingFromNext()
	{
		throw new NotSupportedException();
	}

	public bool MovePrevious()
	{
		if (_index <= 0)
		{
			throw new InvalidOperationException();
		}
		if (_index <= _endIndex)
		{
			_index--;
			return _index <= _endIndex;
		}
		return false;
	}
}
