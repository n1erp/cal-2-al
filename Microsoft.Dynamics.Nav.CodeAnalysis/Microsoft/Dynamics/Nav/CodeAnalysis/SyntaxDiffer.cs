using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class SyntaxDiffer
{
	private enum DiffOp
	{
		None,
		SkipBoth,
		ReduceOld,
		ReduceNew,
		ReduceBoth,
		InsertNew,
		DeleteOld,
		ReplaceOldWithNew
	}

	private struct DiffAction
	{
		public readonly DiffOp Operation;

		public readonly int Count;

		public DiffAction(DiffOp operation, int count)
		{
			DebugAssertHelper.Assert(count >= 0);
			Operation = operation;
			Count = count;
		}
	}

	private struct ChangeRecord
	{
		public readonly TextChangeRange Range;

		public readonly Stack<SyntaxNodeOrToken> OldNodes;

		public readonly Stack<SyntaxNodeOrToken> NewNodes;

		internal ChangeRecord(TextChangeRange range, Stack<SyntaxNodeOrToken> oldNodes, Stack<SyntaxNodeOrToken> newNodes)
		{
			Range = range;
			OldNodes = oldNodes;
			NewNodes = newNodes;
		}
	}

	private struct ChangeRangeWithText(TextChangeRange range, string newText)
	{
		public readonly TextChangeRange Range = range;

		public readonly string NewText = newText;
	}

	private const int InitialStackSize = 8;

	private const int MaxSearchLength = 8;

	private readonly Stack<SyntaxNodeOrToken> oldNodes = new Stack<SyntaxNodeOrToken>(8);

	private readonly Stack<SyntaxNodeOrToken> newNodes = new Stack<SyntaxNodeOrToken>(8);

	private readonly List<ChangeRecord> changes = new List<ChangeRecord>();

	private readonly TextSpan oldSpan;

	private readonly bool computeNewText;

	private readonly HashSet<InternalSyntaxNode> nodeSimilaritySet = new HashSet<InternalSyntaxNode>();

	private readonly HashSet<string> tokenTextSimilaritySet = new HashSet<string>();

	private SyntaxDiffer(SyntaxNode oldNode, SyntaxNode newNode, bool computeNewText)
	{
		oldNodes.Push(oldNode);
		newNodes.Push(newNode);
		oldSpan = oldNode.FullSpan;
		this.computeNewText = computeNewText;
	}

	internal static IList<TextChange> GetTextChanges(SyntaxTree before, SyntaxTree after)
	{
		if (before == after)
		{
			return SpecializedCollections.EmptyList<TextChange>();
		}
		if (before == null)
		{
			return new TextChange[1]
			{
				new TextChange(new TextSpan(0, 0), after.GetText().ToString())
			};
		}
		if (after == null)
		{
			throw new ArgumentNullException("after");
		}
		return GetTextChanges(before.GetRoot(), after.GetRoot());
	}

	internal static IList<TextChange> GetTextChanges(SyntaxNode oldNode, SyntaxNode newNode)
	{
		return new SyntaxDiffer(oldNode, newNode, computeNewText: true).ComputeTextChangesFromOld();
	}

	private IList<TextChange> ComputeTextChangesFromOld()
	{
		ComputeChangeRecords();
		return (from c in ReduceChanges(changes)
			select new TextChange(c.Range.Span, c.NewText)).ToList();
	}

	internal static IList<TextSpan> GetPossiblyDifferentTextSpans(SyntaxTree before, SyntaxTree after)
	{
		if (before == after)
		{
			return SpecializedCollections.EmptyList<TextSpan>();
		}
		if (before == null)
		{
			return new TextSpan[1]
			{
				new TextSpan(0, after.GetText().Length)
			};
		}
		if (after == null)
		{
			throw new ArgumentNullException("after");
		}
		return GetPossiblyDifferentTextSpans(before.GetRoot(), after.GetRoot());
	}

	internal static IList<TextSpan> GetPossiblyDifferentTextSpans(SyntaxNode oldNode, SyntaxNode newNode)
	{
		return new SyntaxDiffer(oldNode, newNode, computeNewText: false).ComputeSpansInNew();
	}

	private IList<TextSpan> ComputeSpansInNew()
	{
		ComputeChangeRecords();
		List<ChangeRangeWithText> list = ReduceChanges(changes);
		List<TextSpan> list2 = new List<TextSpan>();
		int num = 0;
		foreach (ChangeRangeWithText item in list)
		{
			if (item.Range.NewLength > 0)
			{
				int start = item.Range.Span.Start + num;
				list2.Add(new TextSpan(start, item.Range.NewLength));
			}
			num += item.Range.NewLength - item.Range.Span.Length;
		}
		return list2;
	}

	private void ComputeChangeRecords()
	{
		while (true)
		{
			if (newNodes.Count == 0)
			{
				if (oldNodes.Count > 0)
				{
					RecordDeleteOld(oldNodes.Count);
				}
				break;
			}
			if (oldNodes.Count == 0)
			{
				if (newNodes.Count > 0)
				{
					RecordInsertNew(newNodes.Count);
				}
				break;
			}
			DiffAction nextAction = GetNextAction();
			switch (nextAction.Operation)
			{
			case DiffOp.SkipBoth:
				RemoveFirst(oldNodes, nextAction.Count);
				RemoveFirst(newNodes, nextAction.Count);
				break;
			case DiffOp.ReduceOld:
				ReplaceFirstWithChildren(oldNodes);
				break;
			case DiffOp.ReduceNew:
				ReplaceFirstWithChildren(newNodes);
				break;
			case DiffOp.ReduceBoth:
				ReplaceFirstWithChildren(oldNodes);
				ReplaceFirstWithChildren(newNodes);
				break;
			case DiffOp.InsertNew:
				RecordInsertNew(nextAction.Count);
				break;
			case DiffOp.DeleteOld:
				RecordDeleteOld(nextAction.Count);
				break;
			case DiffOp.ReplaceOldWithNew:
				RecordReplaceOldWithNew(nextAction.Count, nextAction.Count);
				break;
			}
		}
	}

	private DiffAction GetNextAction()
	{
		bool isToken = oldNodes.Peek().IsToken;
		bool isToken2 = newNodes.Peek().IsToken;
		FindBestMatch(newNodes, oldNodes.Peek(), out var index, out var similarity);
		FindBestMatch(oldNodes, newNodes.Peek(), out var index2, out var similarity2);
		if (index == 0 && index2 == 0)
		{
			if (AreIdentical(oldNodes.Peek(), newNodes.Peek()))
			{
				return new DiffAction(DiffOp.SkipBoth, 1);
			}
			if (!isToken && !isToken2)
			{
				return new DiffAction(DiffOp.ReduceBoth, 1);
			}
			return new DiffAction(DiffOp.ReplaceOldWithNew, 1);
		}
		if (index >= 0 || index2 >= 0)
		{
			if (index2 < 0 || similarity >= similarity2)
			{
				if (index > 0)
				{
					FindBestMatch(oldNodes, oldNodes.Peek(), out var index3, out var similarity3, 1);
					if (index3 < 1 || similarity3 < similarity)
					{
						return new DiffAction(DiffOp.InsertNew, index);
					}
				}
				if (!isToken2)
				{
					if (AreSimilar(oldNodes.Peek(), newNodes.Peek()))
					{
						return new DiffAction(DiffOp.ReduceBoth, 1);
					}
					return new DiffAction(DiffOp.ReduceNew, 1);
				}
				return new DiffAction(DiffOp.ReplaceOldWithNew, 1);
			}
			if (index2 > 0)
			{
				return new DiffAction(DiffOp.DeleteOld, index2);
			}
			if (!isToken)
			{
				if (AreSimilar(oldNodes.Peek(), newNodes.Peek()))
				{
					return new DiffAction(DiffOp.ReduceBoth, 1);
				}
				return new DiffAction(DiffOp.ReduceOld, 1);
			}
			return new DiffAction(DiffOp.ReplaceOldWithNew, 1);
		}
		if (!isToken && !isToken2 && GetSimilarity(oldNodes.Peek(), newNodes.Peek()) >= Math.Max(oldNodes.Peek().FullSpan.Length, newNodes.Peek().FullSpan.Length))
		{
			return new DiffAction(DiffOp.ReduceBoth, 1);
		}
		return new DiffAction(DiffOp.ReplaceOldWithNew, 1);
	}

	private static void ReplaceFirstWithChildren(Stack<SyntaxNodeOrToken> stack)
	{
		SyntaxNodeOrToken syntaxNodeOrToken = stack.Pop();
		int num = 0;
		SyntaxNodeOrToken[] array = new SyntaxNodeOrToken[syntaxNodeOrToken.ChildNodesAndTokens().Count];
		foreach (SyntaxNodeOrToken item in syntaxNodeOrToken.ChildNodesAndTokens())
		{
			if (item.FullSpan.Length > 0)
			{
				array[num] = item;
				num++;
			}
		}
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			stack.Push(array[num2]);
		}
	}

	private void FindBestMatch(Stack<SyntaxNodeOrToken> stack, SyntaxNodeOrToken node, out int index, out int similarity, int startIndex = 0)
	{
		index = -1;
		similarity = -1;
		int num = 0;
		foreach (SyntaxNodeOrToken item in stack)
		{
			if (num >= 8)
			{
				break;
			}
			if (num >= startIndex)
			{
				if (AreIdentical(item, node))
				{
					int length = node.FullSpan.Length;
					if (length > similarity)
					{
						index = num;
						similarity = length;
						break;
					}
				}
				else if (AreSimilar(item, node))
				{
					int similarity2 = GetSimilarity(item, node);
					if (similarity2 == node.FullSpan.Length && node.IsToken && item.ToFullString() == node.ToFullString())
					{
						index = num;
						similarity = similarity2;
						break;
					}
					if (similarity2 > similarity)
					{
						index = num;
						similarity = similarity2;
					}
				}
				else
				{
					int num2 = 0;
					foreach (SyntaxNodeOrToken item2 in item.ChildNodesAndTokens())
					{
						if (num2 >= 8)
						{
							break;
						}
						num2++;
						if (AreIdentical(item2, node))
						{
							index = num;
							similarity = node.FullSpan.Length;
							return;
						}
						if (AreSimilar(item2, node))
						{
							int similarity3 = GetSimilarity(item2, node);
							if (similarity3 > similarity)
							{
								index = num;
								similarity = similarity3;
							}
						}
					}
				}
			}
			num++;
		}
	}

	private int GetSimilarity(SyntaxNodeOrToken node1, SyntaxNodeOrToken node2)
	{
		int num = 0;
		nodeSimilaritySet.Clear();
		tokenTextSimilaritySet.Clear();
		if (node1.IsToken && node2.IsToken)
		{
			string text = node1.ToString();
			string text2 = node2.ToString();
			if (text == text2)
			{
				num += text.Length;
			}
			foreach (SyntaxTrivia leadingTrivium in node1.GetLeadingTrivia())
			{
				nodeSimilaritySet.Add(leadingTrivium.UnderlyingNode);
			}
			foreach (SyntaxTrivia trailingTrivium in node1.GetTrailingTrivia())
			{
				nodeSimilaritySet.Add(trailingTrivium.UnderlyingNode);
			}
			foreach (SyntaxTrivia leadingTrivium2 in node2.GetLeadingTrivia())
			{
				if (nodeSimilaritySet.Contains(leadingTrivium2.UnderlyingNode))
				{
					num += leadingTrivium2.FullSpan.Length;
				}
			}
			foreach (SyntaxTrivia trailingTrivium2 in node2.GetTrailingTrivia())
			{
				if (nodeSimilaritySet.Contains(trailingTrivium2.UnderlyingNode))
				{
					num += trailingTrivium2.FullSpan.Length;
				}
			}
		}
		else
		{
			foreach (SyntaxNodeOrToken item in node1.ChildNodesAndTokens())
			{
				nodeSimilaritySet.Add(item.UnderlyingNode);
				if (item.IsToken)
				{
					tokenTextSimilaritySet.Add(item.ToString());
				}
			}
			foreach (SyntaxNodeOrToken item2 in node2.ChildNodesAndTokens())
			{
				if (nodeSimilaritySet.Contains(item2.UnderlyingNode))
				{
					num += item2.FullSpan.Length;
				}
				else if (item2.IsToken)
				{
					string text3 = item2.ToString();
					if (tokenTextSimilaritySet.Contains(text3))
					{
						num += text3.Length;
					}
				}
			}
		}
		return num;
	}

	private static bool AreIdentical(SyntaxNodeOrToken node1, SyntaxNodeOrToken node2)
	{
		return node1.UnderlyingNode == node2.UnderlyingNode;
	}

	private static bool AreSimilar(SyntaxNodeOrToken node1, SyntaxNodeOrToken node2)
	{
		return node1.Kind == node2.Kind;
	}

	private void RecordDeleteOld(int oldNodeCount)
	{
		TextSpan span = GetSpan(oldNodes, 0, oldNodeCount);
		Stack<SyntaxNodeOrToken> stack = CopyFirst(oldNodes, oldNodeCount);
		RemoveFirst(oldNodes, oldNodeCount);
		RecordChange(new ChangeRecord(new TextChangeRange(span, 0), stack, null));
	}

	private void RecordReplaceOldWithNew(int oldNodeCount, int newNodeCount)
	{
		TextSpan span = GetSpan(oldNodes, 0, oldNodeCount);
		Stack<SyntaxNodeOrToken> stack = CopyFirst(oldNodes, oldNodeCount);
		RemoveFirst(oldNodes, oldNodeCount);
		TextSpan span2 = GetSpan(newNodes, 0, newNodeCount);
		Stack<SyntaxNodeOrToken> stack2 = CopyFirst(newNodes, newNodeCount);
		RemoveFirst(newNodes, newNodeCount);
		RecordChange(new ChangeRecord(new TextChangeRange(span, span2.Length), stack, stack2));
	}

	private void RecordInsertNew(int newNodeCount)
	{
		TextSpan span = GetSpan(newNodes, 0, newNodeCount);
		Stack<SyntaxNodeOrToken> stack = CopyFirst(newNodes, newNodeCount);
		RemoveFirst(newNodes, newNodeCount);
		int start = ((oldNodes.Count > 0) ? oldNodes.Peek().Position : oldSpan.End);
		RecordChange(new ChangeRecord(new TextChangeRange(new TextSpan(start, 0), span.Length), null, stack));
	}

	private void RecordChange(ChangeRecord change)
	{
		if (changes.Count > 0)
		{
			ChangeRecord changeRecord = changes[changes.Count - 1];
			if (changeRecord.Range.Span.End == change.Range.Span.Start)
			{
				changes[changes.Count - 1] = new ChangeRecord(new TextChangeRange(new TextSpan(changeRecord.Range.Span.Start, changeRecord.Range.Span.Length + change.Range.Span.Length), changeRecord.Range.NewLength + change.Range.NewLength), Combine(changeRecord.OldNodes, change.OldNodes), Combine(changeRecord.NewNodes, change.NewNodes));
				return;
			}
			DebugAssertHelper.Assert(change.Range.Span.Start >= changeRecord.Range.Span.End);
		}
		changes.Add(change);
	}

	private static TextSpan GetSpan(Stack<SyntaxNodeOrToken> stack, int first, int length)
	{
		int num = -1;
		int num2 = -1;
		int num3 = 0;
		foreach (SyntaxNodeOrToken item in stack)
		{
			if (num3 == first)
			{
				num = item.Position;
			}
			if (num3 == first + length - 1)
			{
				num2 = item.EndPosition;
				break;
			}
			num3++;
		}
		DebugAssertHelper.Assert(num >= 0);
		DebugAssertHelper.Assert(num2 >= 0);
		return TextSpan.FromBounds(num, num2);
	}

	private static Stack<SyntaxNodeOrToken> Combine(Stack<SyntaxNodeOrToken> first, Stack<SyntaxNodeOrToken> next)
	{
		if (first == null)
		{
			return next;
		}
		if (next == null)
		{
			return first;
		}
		SyntaxNodeOrToken[] array = ToArray(first, first.Count);
		for (int i = 0; i < array.Length; i++)
		{
			next.Push(array[i]);
		}
		return next;
	}

	private static Stack<SyntaxNodeOrToken> CopyFirst(Stack<SyntaxNodeOrToken> stack, int n)
	{
		if (n == 0)
		{
			return null;
		}
		return new Stack<SyntaxNodeOrToken>(ToArray(stack, n));
	}

	private static SyntaxNodeOrToken[] ToArray(Stack<SyntaxNodeOrToken> stack, int n)
	{
		SyntaxNodeOrToken[] array = new SyntaxNodeOrToken[n];
		int num = n - 1;
		foreach (SyntaxNodeOrToken item in stack)
		{
			array[num] = item;
			num--;
			if (num < 0)
			{
				break;
			}
		}
		return array;
	}

	private static void RemoveFirst(Stack<SyntaxNodeOrToken> stack, int count)
	{
		for (int i = 0; i < count; i++)
		{
			stack.Pop();
		}
	}

	private List<ChangeRangeWithText> ReduceChanges(List<ChangeRecord> changeRecords)
	{
		List<ChangeRangeWithText> list = new List<ChangeRangeWithText>(changeRecords.Count);
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (ChangeRecord changeRecord in changeRecords)
		{
			if (changeRecord.Range.Span.Length > 0 && changeRecord.Range.NewLength > 0)
			{
				TextChangeRange range = changeRecord.Range;
				CopyText(changeRecord.OldNodes, stringBuilder);
				CopyText(changeRecord.NewNodes, stringBuilder2);
				GetCommonEdgeLengths(stringBuilder, stringBuilder2, out var commonLeadingCount, out var commonTrailingCount);
				if (commonLeadingCount > 0 || commonTrailingCount > 0)
				{
					range = new TextChangeRange(new TextSpan(range.Span.Start + commonLeadingCount, range.Span.Length - (commonLeadingCount + commonTrailingCount)), range.NewLength - (commonLeadingCount + commonTrailingCount));
					if (commonTrailingCount > 0)
					{
						stringBuilder2.Remove(stringBuilder2.Length - commonTrailingCount, commonTrailingCount);
					}
					if (commonLeadingCount > 0)
					{
						stringBuilder2.Remove(0, commonLeadingCount);
					}
				}
				if (range.Span.Length > 0 || range.NewLength > 0)
				{
					list.Add(new ChangeRangeWithText(range, computeNewText ? stringBuilder2.ToString() : null));
				}
			}
			else
			{
				list.Add(new ChangeRangeWithText(changeRecord.Range, computeNewText ? GetText(changeRecord.NewNodes) : null));
			}
		}
		return list;
	}

	private static void GetCommonEdgeLengths(StringBuilder oldText, StringBuilder newText, out int commonLeadingCount, out int commonTrailingCount)
	{
		int num = Math.Min(oldText.Length, newText.Length);
		commonLeadingCount = 0;
		while (commonLeadingCount < num && oldText[commonLeadingCount] == newText[commonLeadingCount])
		{
			commonLeadingCount++;
		}
		num -= commonLeadingCount;
		commonTrailingCount = 0;
		while (commonTrailingCount < num && oldText[oldText.Length - commonTrailingCount - 1] == newText[newText.Length - commonTrailingCount - 1])
		{
			commonTrailingCount++;
		}
	}

	private static string GetText(Stack<SyntaxNodeOrToken> stack)
	{
		if (stack == null || stack.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder(GetSpan(stack, 0, stack.Count).Length);
		CopyText(stack, stringBuilder);
		return stringBuilder.ToString();
	}

	[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.IO.StringWriter.#ctor(System.Text.StringBuilder)")]
	private static void CopyText(Stack<SyntaxNodeOrToken> stack, StringBuilder builder)
	{
		builder.Length = 0;
		if (stack == null || stack.Count <= 0)
		{
			return;
		}
		StringWriter stringWriter = new StringWriter(builder, CultureInfo.InvariantCulture);
		foreach (SyntaxNodeOrToken item in stack)
		{
			item.WriteTo(stringWriter);
		}
		stringWriter.Flush();
	}
}
