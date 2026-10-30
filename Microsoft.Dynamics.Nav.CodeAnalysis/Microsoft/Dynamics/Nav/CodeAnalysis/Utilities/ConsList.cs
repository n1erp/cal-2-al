using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal class ConsList<T> : IEnumerable<T>, IEnumerable
{
	internal struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
	{
		private T current;

		private ConsList<T> tail;

		public T Current
		{
			get
			{
				DebugAssertHelper.Assert(tail != null);
				return current;
			}
		}

		object IEnumerator.Current => Current;

		internal Enumerator(ConsList<T> list)
		{
			current = default;
			tail = list;
		}

		public bool MoveNext()
		{
			ConsList<T> consList = tail;
			ConsList<T> consList2 = consList.tail;
			if (consList2 != null)
			{
				current = consList.head;
				tail = consList2;
				return true;
			}
			current = default;
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

	private readonly T head;

	private readonly ConsList<T> tail;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Head
	{
		get
		{
			DebugAssertHelper.Assert(this != Empty);
			return head;
		}
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public ConsList<T> Tail
	{
		get
		{
			DebugAssertHelper.Assert(this != Empty);
			return tail;
		}
	}

	private ConsList()
	{
		head = default;
		tail = null;
	}

	public ConsList(T head, ConsList<T> tail)
	{
		DebugAssertHelper.Assert(tail != null);
		this.head = head;
		this.tail = tail;
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
		while (consList.tail != null)
		{
			if (flag)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(consList.head);
			flag = true;
			consList = consList.tail;
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}
