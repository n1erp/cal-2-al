using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal class EmptyEnumerator<TNode> : IEnumerator<TNode>, IDisposable, IEnumerator
{
	private static EmptyEnumerator<TNode> empty;

	public TNode Current => default;

	object IEnumerator.Current => null;

	private EmptyEnumerator()
	{
	}

	public bool MoveNext()
	{
		return false;
	}

	void IDisposable.Dispose()
	{
	}

	void IEnumerator.Reset()
	{
	}

	internal static EmptyEnumerator<TNode> GetInstance()
	{
		if (empty == null)
		{
			empty = new EmptyEnumerator<TNode>();
		}
		return empty;
	}
}
