using System;

namespace Microsoft.CodeAnalysis.Syntax;

internal struct SeparatedSyntaxListBuilder<TNode> where TNode : SyntaxNode
{
	private readonly SyntaxListBuilder _builder;

	private bool _expectedSeparator;

	public bool IsNull => _builder == null;

	public int Count => _builder.Count;

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
		_builder = builder;
		_expectedSeparator = false;
	}

	public void Clear()
	{
		_builder.Clear();
	}

	private void CheckExpectedElement()
	{
		if (_expectedSeparator)
		{
			throw new InvalidOperationException(CodeAnalysisResources.SeparatorIsExpected);
		}
	}

	private void CheckExpectedSeparator()
	{
		if (!_expectedSeparator)
		{
			throw new InvalidOperationException(CodeAnalysisResources.ElementIsExpected);
		}
	}

	public SeparatedSyntaxListBuilder<TNode> Add(TNode node)
	{
		CheckExpectedElement();
		_expectedSeparator = true;
		_builder.Add(node);
		return this;
	}

	public SeparatedSyntaxListBuilder<TNode> AddSeparator(in SyntaxToken separatorToken)
	{
		CheckExpectedSeparator();
		_expectedSeparator = false;
		_builder.AddInternal(separatorToken.Node);
		return this;
	}

	public SeparatedSyntaxListBuilder<TNode> AddRange(in SeparatedSyntaxList<TNode> nodes)
	{
		CheckExpectedElement();
		SyntaxNodeOrTokenList withSeparators = nodes.GetWithSeparators();
		_builder.AddRange(withSeparators);
		_expectedSeparator = (_builder.Count & 1) != 0;
		return this;
	}

	public SeparatedSyntaxListBuilder<TNode> AddRange(in SeparatedSyntaxList<TNode> nodes, int count)
	{
		CheckExpectedElement();
		SyntaxNodeOrTokenList withSeparators = nodes.GetWithSeparators();
		_builder.AddRange(withSeparators, Count, Math.Min(count << 1, withSeparators.Count));
		_expectedSeparator = (_builder.Count & 1) != 0;
		return this;
	}

	public SeparatedSyntaxList<TNode> ToList()
	{
		if (_builder == null)
		{
			return default;
		}
		return _builder.ToSeparatedList<TNode>();
	}

	public SeparatedSyntaxList<TDerived> ToList<TDerived>() where TDerived : TNode
	{
		if (_builder == null)
		{
			return default;
		}
		return _builder.ToSeparatedList<TDerived>();
	}

	public static implicit operator SyntaxListBuilder(in SeparatedSyntaxListBuilder<TNode> builder)
	{
		return builder._builder;
	}

	public static implicit operator SeparatedSyntaxList<TNode>(in SeparatedSyntaxListBuilder<TNode> builder)
	{
		if (builder._builder != null)
		{
			return builder.ToList();
		}
		return default;
	}
}
