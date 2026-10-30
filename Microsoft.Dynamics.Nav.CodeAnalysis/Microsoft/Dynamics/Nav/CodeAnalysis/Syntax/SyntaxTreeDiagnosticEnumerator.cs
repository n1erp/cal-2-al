using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal struct SyntaxTreeDiagnosticEnumerator
{
	private struct NodeIteration
	{
		internal readonly InternalSyntaxNode Node;

		internal int DiagnosticIndex;

		internal int SlotIndex;

		internal NodeIteration(InternalSyntaxNode node)
		{
			Node = node;
			SlotIndex = -1;
			DiagnosticIndex = -1;
		}
	}

	private struct NodeIterationStack
	{
		private NodeIteration[] stack;

		private int count;

		internal NodeIteration Top => this[count - 1];

		internal NodeIteration this[int index]
		{
			get
			{
				DebugAssertHelper.Assert(stack != null);
				DebugAssertHelper.Assert(index >= 0 && index < count);
				return stack[index];
			}
		}

		internal NodeIterationStack(int capacity)
		{
			DebugAssertHelper.Assert(capacity > 0);
			stack = new NodeIteration[capacity];
			count = 0;
		}

		internal void PushNodeOrToken(InternalSyntaxNode node)
		{
			if (node is InternalSyntaxToken token)
			{
				PushToken(token);
			}
			else
			{
				Push(node);
			}
		}

		private void PushToken(InternalSyntaxToken token)
		{
			InternalSyntaxNode trailingTrivia = token.GetTrailingTrivia();
			if (trailingTrivia != null)
			{
				Push(trailingTrivia);
			}
			Push(token);
			InternalSyntaxNode leadingTrivia = token.GetLeadingTrivia();
			if (leadingTrivia != null)
			{
				Push(leadingTrivia);
			}
		}

		private void Push(InternalSyntaxNode node)
		{
			if (count >= stack.Length)
			{
				NodeIteration[] destinationArray = new NodeIteration[stack.Length * 2];
				Array.Copy(stack, destinationArray, stack.Length);
				stack = destinationArray;
			}
			stack[count] = new NodeIteration(node);
			count++;
		}

		internal void Pop()
		{
			count--;
		}

		internal bool Any()
		{
			return count > 0;
		}

		internal void UpdateSlotIndexForStackTop(int slotIndex)
		{
			DebugAssertHelper.Assert(stack != null);
			DebugAssertHelper.Assert(count > 0);
			stack[count - 1].SlotIndex = slotIndex;
		}

		internal void UpdateDiagnosticIndexForStackTop(int diagnosticIndex)
		{
			DebugAssertHelper.Assert(stack != null);
			DebugAssertHelper.Assert(count > 0);
			stack[count - 1].DiagnosticIndex = diagnosticIndex;
		}
	}

	private readonly SyntaxTree syntaxTree;

	private NodeIterationStack stack;

	private Diagnostic current;

	private int position;

	private const int DefaultStackCapacity = 8;

	public Diagnostic Current => current;

	internal SyntaxTreeDiagnosticEnumerator(SyntaxTree syntaxTree, InternalSyntaxNode node, int position)
	{
		this.syntaxTree = null;
		current = null;
		this.position = position;
		if (node != null && node.ContainsDiagnostics)
		{
			this.syntaxTree = syntaxTree;
			stack = new NodeIterationStack(8);
			stack.PushNodeOrToken(node);
		}
		else
		{
			stack = default;
		}
	}

	public bool MoveNext()
	{
		while (stack.Any())
		{
			int diagnosticIndex = stack.Top.DiagnosticIndex;
			InternalSyntaxNode node = stack.Top.Node;
			DiagnosticInfo[] diagnostics = node.GetDiagnostics();
			if (diagnosticIndex < diagnostics.Length - 1)
			{
				diagnosticIndex++;
				SyntaxDiagnosticInfo syntaxDiagnosticInfo = (SyntaxDiagnosticInfo)diagnostics[diagnosticIndex];
				int num = (node.IsToken ? node.GetLeadingTriviaWidth() : 0);
				int length = syntaxTree.GetRoot().FullSpan.Length;
				int num2 = Math.Min(position - num + syntaxDiagnosticInfo.Offset, length);
				int length2 = Math.Min(num2 + syntaxDiagnosticInfo.Width, length) - num2;
				current = new NavDiagnostic(syntaxDiagnosticInfo, new SourceLocation(syntaxTree, new TextSpan(num2, length2)));
				stack.UpdateDiagnosticIndexForStackTop(diagnosticIndex);
				return true;
			}
			int num3 = stack.Top.SlotIndex;
			while (true)
			{
				if (num3 < node.SlotCount - 1)
				{
					num3++;
					InternalSyntaxNode slot = node.GetSlot(num3);
					if (slot != null)
					{
						if (slot.ContainsDiagnostics)
						{
							stack.UpdateSlotIndexForStackTop(num3);
							stack.PushNodeOrToken(slot);
							break;
						}
						position += slot.FullWidth;
					}
					continue;
				}
				if (node.SlotCount == 0)
				{
					position += node.Width;
				}
				stack.Pop();
				break;
			}
		}
		return false;
	}
}
