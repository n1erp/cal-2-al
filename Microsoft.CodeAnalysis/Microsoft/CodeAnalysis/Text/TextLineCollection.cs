using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Text;

public abstract class TextLineCollection : IReadOnlyList<TextLine>, IEnumerable<TextLine>, IEnumerable, IReadOnlyCollection<TextLine>
{
	public struct Enumerator : IEnumerator<TextLine>, IEnumerator, IDisposable
	{
		private readonly TextLineCollection _lines;

		private int _index;

		public TextLine Current
		{
			get
			{
				int index = _index;
				if (index >= 0 && index < _lines.Count)
				{
					return _lines[index];
				}
				return default;
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(TextLineCollection lines, int index = -1)
		{
			_lines = lines;
			_index = index;
		}

		public bool MoveNext()
		{
			if (_index < _lines.Count - 1)
			{
				_index++;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			return MoveNext();
		}

		void IEnumerator.Reset()
		{
		}

		void IDisposable.Dispose()
		{
		}

		public override bool Equals(object obj)
		{
			throw new NotSupportedException();
		}

		public override int GetHashCode()
		{
			throw new NotSupportedException();
		}
	}

	public abstract int Count { get; }

	public abstract TextLine this[int index] { get; }

	public abstract int IndexOf(int position);

	public virtual TextLine GetLineFromPosition(int position)
	{
		return this[IndexOf(position)];
	}

	public virtual LinePosition GetLinePosition(int position)
	{
		TextLine lineFromPosition = GetLineFromPosition(position);
		return new LinePosition(lineFromPosition.LineNumber, position - lineFromPosition.Start);
	}

	public LinePositionSpan GetLinePositionSpan(TextSpan span)
	{
		return new LinePositionSpan(GetLinePosition(span.Start), GetLinePosition(span.End));
	}

	public int GetPosition(LinePosition position)
	{
		return this[position.Line].Start + position.Character;
	}

	public TextSpan GetTextSpan(LinePositionSpan span)
	{
		return TextSpan.FromBounds(GetPosition(span.Start), GetPosition(span.End));
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<TextLine> IEnumerable<TextLine>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
