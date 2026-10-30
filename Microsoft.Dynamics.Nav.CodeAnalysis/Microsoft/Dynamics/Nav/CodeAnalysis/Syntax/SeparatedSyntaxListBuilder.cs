using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal struct SeparatedSyntaxListBuilder<TNode> where TNode : SyntaxNode
{
	private readonly SyntaxListBuilder builder;

	private bool expectedSeparator;

	public bool IsNull => builder == null;

	public int Count => builder.Count;

	public SeparatedSyntaxListBuilder(int size)
		: this(new SyntaxListBuilder(size))
	{
	}

	public static SeparatedSyntaxListBuilder<TNode> Create()
	{
		return new SeparatedSyntaxListBuilder<TNode>(8);
	}

	internal SeparatedSyntaxListBuilder(SyntaxListBuilder builder)
	{
		this.builder = builder;
		expectedSeparator = false;
	}

	public void Clear()
	{
		builder.Clear();
	}

	private void CheckExpectedElement()
	{
		if (expectedSeparator)
		{
			throw new InvalidOperationException(CodeAnalysisResources.SeparatorIsExpected);
		}
	}

	private void CheckExpectedSeparator()
	{
		if (!expectedSeparator)
		{
			throw new InvalidOperationException(CodeAnalysisResources.ElementIsExpected);
		}
	}

	public SeparatedSyntaxListBuilder<TNode> Add(TNode node)
	{
		CheckExpectedElement();
		expectedSeparator = true;
		builder.Add(node);
		return this;
	}

	public SeparatedSyntaxListBuilder<TNode> AddSeparator(SyntaxToken separatorToken)
	{
		CheckExpectedSeparator();
		expectedSeparator = false;
		builder.AddInternal(separatorToken.Node);
		return this;
	}

	public SeparatedSyntaxListBuilder<TNode> AddRange(SeparatedSyntaxList<TNode> nodes)
	{
		CheckExpectedElement();
		SyntaxNodeOrTokenList withSeparators = nodes.GetWithSeparators();
		builder.AddRange(withSeparators);
		expectedSeparator = (builder.Count & 1) != 0;
		return this;
	}

	public SeparatedSyntaxListBuilder<TNode> AddRange(SeparatedSyntaxList<TNode> nodes, int count)
	{
		CheckExpectedElement();
		SyntaxNodeOrTokenList withSeparators = nodes.GetWithSeparators();
		builder.AddRange(withSeparators, Count, Math.Min(count << 1, withSeparators.Count));
		expectedSeparator = (builder.Count & 1) != 0;
		return this;
	}

	public SeparatedSyntaxList<TNode> ToList()
	{
		if (builder == null)
		{
			return default;
		}
		return builder.ToSeparatedList<TNode>();
	}

	public SeparatedSyntaxList<TDerived> ToList<TDerived>() where TDerived : TNode
	{
		if (builder == null)
		{
			return default;
		}
		return builder.ToSeparatedList<TDerived>();
	}

	public static implicit operator SyntaxListBuilder(SeparatedSyntaxListBuilder<TNode> builder)
	{
		return builder.builder;
	}

	public static implicit operator SeparatedSyntaxList<TNode>(SeparatedSyntaxListBuilder<TNode> builder)
	{
		if (builder.builder != null)
		{
			return builder.ToList();
		}
		return default;
	}
}
