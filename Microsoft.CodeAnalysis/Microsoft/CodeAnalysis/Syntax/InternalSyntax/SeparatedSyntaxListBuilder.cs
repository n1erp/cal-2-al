using System;

namespace Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal struct SeparatedSyntaxListBuilder<TNode> where TNode : GreenNode
{
	private readonly SyntaxListBuilder _builder;

	public bool IsNull => _builder == null;

	public int Count => _builder.Count;

	public GreenNode this[int index]
	{
		get
		{
			return _builder[index];
		}
		set
		{
			_builder[index] = value;
		}
	}

	internal SyntaxListBuilder UnderlyingBuilder => _builder;

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
	}

	public void Clear()
	{
		_builder.Clear();
	}

	public void RemoveLast()
	{
		_builder.RemoveLast();
	}

	public SeparatedSyntaxListBuilder<TNode> Add(TNode node)
	{
		_builder.Add(node);
		return this;
	}

	public void AddSeparator(GreenNode separatorToken)
	{
		_builder.Add(separatorToken);
	}

	public void AddRange(TNode[] items, int offset, int length)
	{
		_builder.AddRange(items, offset, length);
	}

	public void AddRange(in SeparatedSyntaxList<TNode> nodes)
	{
		_builder.AddRange(nodes.GetWithSeparators());
	}

	public void AddRange(in SeparatedSyntaxList<TNode> nodes, int count)
	{
		SyntaxList<GreenNode> withSeparators = nodes.GetWithSeparators();
		_builder.AddRange(withSeparators, Count, Math.Min(count * 2, withSeparators.Count));
	}

	public bool Any(int kind)
	{
		return _builder.Any(kind);
	}

	public SeparatedSyntaxList<TNode> ToList()
	{
		if (_builder != null)
		{
			return new SeparatedSyntaxList<TNode>(new SyntaxList<GreenNode>(_builder.ToListNode()));
		}
		return default;
	}

	public static implicit operator SeparatedSyntaxList<TNode>(in SeparatedSyntaxListBuilder<TNode> builder)
	{
		return builder.ToList();
	}

	public static implicit operator SyntaxListBuilder(in SeparatedSyntaxListBuilder<TNode> builder)
	{
		return builder._builder;
	}
}
