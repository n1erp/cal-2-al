using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal struct LexicalSortKey
{
	private int treeOrdinal;

	private int position;

	public static readonly LexicalSortKey NotInSource = new LexicalSortKey
	{
		treeOrdinal = -1,
		position = 0
	};

	public static readonly LexicalSortKey NotInitialized = new LexicalSortKey
	{
		treeOrdinal = -1,
		position = -1
	};

	public static readonly LexicalSortKey SynthesizedCtor = new LexicalSortKey
	{
		treeOrdinal = int.MaxValue,
		position = 2147483646
	};

	public static readonly LexicalSortKey SynthesizedCCtor = new LexicalSortKey
	{
		treeOrdinal = int.MaxValue,
		position = int.MaxValue
	};

	public int TreeOrdinal => treeOrdinal;

	public int Position => position;

	public bool IsInitialized => Volatile.Read(ref position) >= 0;

	private LexicalSortKey(int treeOrdinal, int position)
	{
		DebugAssertHelper.Assert(position >= 0);
		DebugAssertHelper.Assert(treeOrdinal >= 0);
		this.treeOrdinal = treeOrdinal;
		this.position = position;
	}

	private LexicalSortKey(SyntaxTree tree, int position, Compilation compilation)
		: this((tree == null) ? (-1) : compilation.GetSyntaxTreeOrdinal(tree), position)
	{
	}

	public LexicalSortKey(SyntaxReference syntaxRef, Compilation compilation)
		: this(syntaxRef.SyntaxTree, syntaxRef.Span.Start, compilation)
	{
	}

	public LexicalSortKey(Location location, Compilation compilation)
		: this(location.SourceTree, location.SourceSpan.Start, compilation)
	{
	}

	public LexicalSortKey(SyntaxNode node, Compilation compilation)
		: this(node.SyntaxTree, node.SpanStart, compilation)
	{
	}

	public LexicalSortKey(SyntaxToken token, Compilation compilation)
		: this(token.SyntaxTree, token.SpanStart, compilation)
	{
	}

	public static int Compare(LexicalSortKey leftSortKey, LexicalSortKey rightSortKey)
	{
		if (leftSortKey.TreeOrdinal != rightSortKey.TreeOrdinal)
		{
			if (leftSortKey.TreeOrdinal < 0)
			{
				return 1;
			}
			if (rightSortKey.TreeOrdinal < 0)
			{
				return -1;
			}
			int num = leftSortKey.TreeOrdinal - rightSortKey.TreeOrdinal;
			DebugAssertHelper.Assert(num != 0);
			return num;
		}
		return leftSortKey.Position - rightSortKey.Position;
	}

	public static LexicalSortKey First(LexicalSortKey leftSortKey, LexicalSortKey rightSortKey)
	{
		if (Compare(leftSortKey, rightSortKey) <= 0)
		{
			return leftSortKey;
		}
		return rightSortKey;
	}

	public void SetFrom(LexicalSortKey other)
	{
		DebugAssertHelper.Assert(other.IsInitialized);
		treeOrdinal = other.treeOrdinal;
		Volatile.Write(ref position, other.position);
	}
}
