using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

public abstract class TextLineCollection : IReadOnlyList<TextLine>, IReadOnlyCollection<TextLine>, IEnumerable<TextLine>, IEnumerable
{
	[SuppressMessage("Performance", "RS0008", Justification = "Equality not actually implemented")]
	public struct Enumerator : IEnumerator<TextLine>, IDisposable, IEnumerator
	{
		private readonly TextLineCollection lines;

		private int index;

		public TextLine Current
		{
			get
			{
				int num = index;
				if (num >= 0 && num < lines.Count)
				{
					return lines[num];
				}
				return default;
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(TextLineCollection lines, int index = -1)
		{
			this.lines = lines;
			this.index = index;
		}

		public bool MoveNext()
		{
			if (index < lines.Count - 1)
			{
				index++;
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

		public void Dispose()
		{
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
