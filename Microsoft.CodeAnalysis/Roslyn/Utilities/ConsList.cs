using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Roslyn.Utilities;

internal class ConsList<T> : IEnumerable<T>, IEnumerable
{
	internal struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private T _current;

		private ConsList<T> _tail;

		public T Current => _current;

		object IEnumerator.Current => Current;

		internal Enumerator(ConsList<T> list)
		{
			_current = default;
			_tail = list;
		}

		public bool MoveNext()
		{
			ConsList<T> tail = _tail;
			ConsList<T> tail2 = tail._tail;
			if (tail2 != null)
			{
				_current = tail._head;
				_tail = tail2;
				return true;
			}
			_current = default;
			return false;
		}

		public void Dispose()
		{
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	public static readonly ConsList<T> Empty = new ConsList<T>();

	private readonly T _head;

	private readonly ConsList<T> _tail;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Head => _head;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public ConsList<T> Tail => _tail;

	private ConsList()
	{
		_head = default;
		_tail = null;
	}

	public ConsList(T head, ConsList<T> tail)
	{
		_head = head;
		_tail = tail;
	}

	public bool Any()
	{
		return this != Empty;
	}

	public ConsList<T> Push(T value)
	{
		return new ConsList<T>(value, this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("ConsList[");
		bool flag = false;
		ConsList<T> consList = this;
		while (consList._tail != null)
		{
			if (flag)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(consList._head);
			flag = true;
			consList = consList._tail;
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}
