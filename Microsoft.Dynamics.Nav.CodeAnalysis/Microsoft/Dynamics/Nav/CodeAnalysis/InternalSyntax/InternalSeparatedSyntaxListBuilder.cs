namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal struct InternalSeparatedSyntaxListBuilder<TNode> where TNode : InternalSyntaxNode
{
	private readonly InternalSyntaxListBuilder builder;

	public bool IsNull => builder == null;

	public int Count => builder.Count;

	public InternalSyntaxNode this[int index]
	{
		get
		{
			return builder[index];
		}
		set
		{
			builder[index] = value;
		}
	}

	internal InternalSyntaxListBuilder UnderlyingBuilder => builder;

	public InternalSeparatedSyntaxListBuilder(int size)
		: this(new InternalSyntaxListBuilder(size))
	{
	}

	public static InternalSeparatedSyntaxListBuilder<TNode> Create()
	{
		return new InternalSeparatedSyntaxListBuilder<TNode>(8);
	}

	internal InternalSeparatedSyntaxListBuilder(InternalSyntaxListBuilder builderParameter)
	{
		builder = builderParameter;
	}

	public void Clear()
	{
		builder.Clear();
	}

	public InternalSeparatedSyntaxListBuilder<TNode> Add(TNode node)
	{
		builder.Add(node);
		return this;
	}

	public void AddSeparator(InternalSyntaxToken separatorToken)
	{
		builder.Add(separatorToken);
	}

	public void AddRange(TNode[] items, int offset, int length)
	{
		builder.AddRange(items, offset, length);
	}

	public void AddRange(InternalSeparatedSyntaxList<TNode> nodes)
	{
		builder.AddRange(nodes.GetWithSeparators());
	}

	public bool Any(SyntaxKind kind)
	{
		return builder.Any(kind);
	}

	public InternalSeparatedSyntaxList<TNode> ToList()
	{
		if (builder != null)
		{
			return new InternalSeparatedSyntaxList<TNode>(new InternalSyntaxList<InternalSyntaxNode>(builder.ToListNode()));
		}
		return default;
	}

	public static implicit operator InternalSeparatedSyntaxList<TNode>(InternalSeparatedSyntaxListBuilder<TNode> builder)
	{
		return builder.ToList();
	}
}
