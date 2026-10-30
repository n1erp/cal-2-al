using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal struct InternalSyntaxDiagnosticInfoList
{
	public struct Enumerator
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

		private NodeIteration[] stack = null;

		private int count = 0;

		private DiagnosticInfo current = null;

		public DiagnosticInfo Current => current;

		internal Enumerator(InternalSyntaxNode node)
		{
			if (node != null && node.ContainsDiagnostics)
			{
				stack = new NodeIteration[8];
				PushNodeOrToken(node);
			}
		}

		public bool MoveNext()
		{
			while (count > 0)
			{
				int diagnosticIndex = stack[count - 1].DiagnosticIndex;
				InternalSyntaxNode node = stack[count - 1].Node;
				DiagnosticInfo[] diagnostics = node.GetDiagnostics();
				if (diagnosticIndex < diagnostics.Length - 1)
				{
					diagnosticIndex++;
					current = diagnostics[diagnosticIndex];
					stack[count - 1].DiagnosticIndex = diagnosticIndex;
					return true;
				}
				int num = stack[count - 1].SlotIndex;
				while (true)
				{
					if (num < node.SlotCount - 1)
					{
						num++;
						InternalSyntaxNode slot = node.GetSlot(num);
						if (slot != null && slot.ContainsDiagnostics)
						{
							stack[count - 1].SlotIndex = num;
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

		private void PushNodeOrToken(InternalSyntaxNode node)
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

		private void Pop()
		{
			count--;
		}
	}

	private readonly InternalSyntaxNode node;

	internal InternalSyntaxDiagnosticInfoList(InternalSyntaxNode nodeParameter)
	{
		node = nodeParameter;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(node);
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
