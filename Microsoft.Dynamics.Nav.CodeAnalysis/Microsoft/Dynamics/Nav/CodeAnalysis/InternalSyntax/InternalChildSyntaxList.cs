namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal struct InternalChildSyntaxList
{
	internal struct Enumerator
	{
		private readonly InternalSyntaxNode node;

		private int childIndex;

		private InternalSyntaxNode list;

		private int listIndex;

		private InternalSyntaxNode currentChild;

		public InternalSyntaxNode Current => currentChild;

		internal Enumerator(InternalSyntaxNode node)
		{
			this.node = node;
			childIndex = -1;
			listIndex = -1;
			list = null;
			currentChild = null;
		}

		public bool MoveNext()
		{
			if (node != null)
			{
				if (list != null)
				{
					listIndex++;
					if (listIndex < list.SlotCount)
					{
						currentChild = list.GetSlot(listIndex);
						return true;
					}
					list = null;
					listIndex = -1;
				}
				while (true)
				{
					childIndex++;
					if (childIndex == node.SlotCount)
					{
						break;
					}
					InternalSyntaxNode slot = node.GetSlot(childIndex);
					if (slot != null)
					{
						if (slot.Kind != SyntaxKind.List)
						{
							currentChild = slot;
							return true;
						}
						list = slot;
						listIndex++;
						if (listIndex < list.SlotCount)
						{
							currentChild = list.GetSlot(listIndex);
							return true;
						}
						list = null;
						listIndex = -1;
					}
				}
			}
			currentChild = null;
			return false;
		}
	}

	private readonly InternalSyntaxNode node;

	private int count;

	public int Count
	{
		get
		{
			if (count == -1)
			{
				count = CountNodes();
			}
			return count;
		}
	}

	internal InternalChildSyntaxList(InternalSyntaxNode node)
	{
		this.node = node;
		count = -1;
	}

	private int CountNodes()
	{
		int num = 0;
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			num++;
		}
		return num;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(node);
	}
}
