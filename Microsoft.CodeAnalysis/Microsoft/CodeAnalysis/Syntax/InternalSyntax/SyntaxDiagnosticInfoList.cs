using System;

namespace Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal struct SyntaxDiagnosticInfoList
{
	public struct Enumerator
	{
		private struct NodeIteration
		{
			internal readonly GreenNode Node;

			internal int DiagnosticIndex;

			internal int SlotIndex;

			internal NodeIteration(GreenNode node)
			{
				Node = node;
				SlotIndex = -1;
				DiagnosticIndex = -1;
			}
		}

		private NodeIteration[] _stack = null;

		private int _count = 0;

		public DiagnosticInfo Current { get; private set; } = null;

		internal Enumerator(GreenNode node)
		{
			if (node != null && node.ContainsDiagnostics)
			{
				_stack = new NodeIteration[8];
				PushNodeOrToken(node);
			}
		}

		public bool MoveNext()
		{
			while (_count > 0)
			{
				int diagnosticIndex = _stack[_count - 1].DiagnosticIndex;
				GreenNode node = _stack[_count - 1].Node;
				DiagnosticInfo[] diagnostics = node.GetDiagnostics();
				if (diagnosticIndex < diagnostics.Length - 1)
				{
					diagnosticIndex++;
					Current = diagnostics[diagnosticIndex];
					_stack[_count - 1].DiagnosticIndex = diagnosticIndex;
					return true;
				}
				int num = _stack[_count - 1].SlotIndex;
				while (true)
				{
					if (num < node.SlotCount - 1)
					{
						num++;
						GreenNode slot = node.GetSlot(num);
						if (slot != null && slot.ContainsDiagnostics)
						{
							_stack[_count - 1].SlotIndex = num;
							PushNodeOrToken(slot);
							break;
						}
						continue;
					}
					Pop();
					break;
				}
			}
			return false;
		}

		private void PushNodeOrToken(GreenNode node)
		{
			if (node.IsToken)
			{
				PushToken(node);
			}
			else
			{
				Push(node);
			}
		}

		private void PushToken(GreenNode token)
		{
			GreenNode trailingTriviaCore = token.GetTrailingTriviaCore();
			if (trailingTriviaCore != null)
			{
				Push(trailingTriviaCore);
			}
			Push(token);
			GreenNode leadingTriviaCore = token.GetLeadingTriviaCore();
			if (leadingTriviaCore != null)
			{
				Push(leadingTriviaCore);
			}
		}

		private void Push(GreenNode node)
		{
			if (_count >= _stack.Length)
			{
				NodeIteration[] array = new NodeIteration[_stack.Length * 2];
				Array.Copy(_stack, array, _stack.Length);
				_stack = array;
			}
			_stack[_count] = new NodeIteration(node);
			_count++;
		}

		private void Pop()
		{
			_count--;
		}
	}

	private readonly GreenNode _node;

	internal SyntaxDiagnosticInfoList(GreenNode node)
	{
		_node = node;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_node);
	}

	internal bool Any(Func<DiagnosticInfo, bool> predicate)
	{
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (predicate(enumerator.Current))
			{
				return true;
			}
		}
		return false;
	}
}
