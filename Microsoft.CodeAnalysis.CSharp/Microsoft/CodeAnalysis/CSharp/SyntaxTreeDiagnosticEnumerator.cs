using System;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct SyntaxTreeDiagnosticEnumerator
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

	private struct NodeIterationStack
	{
		private NodeIteration[] _stack;

		private int _count;

		internal NodeIteration Top => this[_count - 1];

		internal NodeIteration this[int index] => _stack[index];

		internal NodeIterationStack(int capacity)
		{
			_stack = new NodeIteration[capacity];
			_count = 0;
		}

		internal void PushNodeOrToken(GreenNode node)
		{
			if (node is Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken token)
			{
				PushToken(token);
			}
			else
			{
				Push(node);
			}
		}

		private void PushToken(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.SyntaxToken token)
		{
			GreenNode trailingTrivia = token.GetTrailingTrivia();
			if (trailingTrivia != null)
			{
				Push(trailingTrivia);
			}
			Push(token);
			GreenNode leadingTrivia = token.GetLeadingTrivia();
			if (leadingTrivia != null)
			{
				Push(leadingTrivia);
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

		internal void Pop()
		{
			_count--;
		}

		internal bool Any()
		{
			return _count > 0;
		}

		internal void UpdateSlotIndexForStackTop(int slotIndex)
		{
			_stack[_count - 1].SlotIndex = slotIndex;
		}

		internal void UpdateDiagnosticIndexForStackTop(int diagnosticIndex)
		{
			_stack[_count - 1].DiagnosticIndex = diagnosticIndex;
		}
	}

	private readonly SyntaxTree _syntaxTree;

	private NodeIterationStack _stack;

	private Diagnostic _current;

	private int _position;

	private const int DefaultStackCapacity = 8;

	public Diagnostic Current => _current;

	internal SyntaxTreeDiagnosticEnumerator(SyntaxTree syntaxTree, GreenNode node, int position)
	{
		_syntaxTree = null;
		_current = null;
		_position = position;
		if (node != null && node.ContainsDiagnostics)
		{
			_syntaxTree = syntaxTree;
			_stack = new NodeIterationStack(8);
			_stack.PushNodeOrToken(node);
		}
		else
		{
			_stack = default;
		}
	}

	public bool MoveNext()
	{
		while (_stack.Any())
		{
			int diagnosticIndex = _stack.Top.DiagnosticIndex;
			GreenNode node = _stack.Top.Node;
			DiagnosticInfo[] diagnostics = node.GetDiagnostics();
			if (diagnosticIndex < diagnostics.Length - 1)
			{
				diagnosticIndex++;
				SyntaxDiagnosticInfo syntaxDiagnosticInfo = (SyntaxDiagnosticInfo)diagnostics[diagnosticIndex];
				int num = (node.IsToken ? node.GetLeadingTriviaWidth() : 0);
				int length = _syntaxTree.GetRoot().FullSpan.Length;
				int num2 = Math.Min(_position - num + syntaxDiagnosticInfo.Offset, length);
				int length2 = Math.Min(num2 + syntaxDiagnosticInfo.Width, length) - num2;
				_current = new CSDiagnostic(syntaxDiagnosticInfo, new SourceLocation(_syntaxTree, new TextSpan(num2, length2)));
				_stack.UpdateDiagnosticIndexForStackTop(diagnosticIndex);
				return true;
			}
			int num3 = _stack.Top.SlotIndex;
			while (true)
			{
				if (num3 < node.SlotCount - 1)
				{
					num3++;
					GreenNode slot = node.GetSlot(num3);
					if (slot != null)
					{
						if (slot.ContainsDiagnostics)
						{
							_stack.UpdateSlotIndexForStackTop(num3);
							_stack.PushNodeOrToken(slot);
							break;
						}
						_position += slot.FullWidth;
					}
					continue;
				}
				if (node.SlotCount == 0)
				{
					_position += node.Width;
				}
				_stack.Pop();
				break;
			}
		}
		return false;
	}
}
