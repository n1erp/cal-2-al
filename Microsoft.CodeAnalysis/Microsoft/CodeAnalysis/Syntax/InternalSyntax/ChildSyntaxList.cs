namespace Microsoft.CodeAnalysis.Syntax.InternalSyntax;

internal struct ChildSyntaxList
{
	internal struct Enumerator
	{
		private readonly GreenNode _node;

		private int _childIndex;

		private GreenNode _list;

		private int _listIndex;

		private GreenNode _currentChild;

		public GreenNode Current => _currentChild;

		internal Enumerator(GreenNode node)
		{
			_node = node;
			_childIndex = -1;
			_listIndex = -1;
			_list = null;
			_currentChild = null;
		}

		public bool MoveNext()
		{
			if (_node != null)
			{
				if (_list != null)
				{
					_listIndex++;
					if (_listIndex < _list.SlotCount)
					{
						_currentChild = _list.GetSlot(_listIndex);
						return true;
					}
					_list = null;
					_listIndex = -1;
				}
				while (true)
				{
					_childIndex++;
					if (_childIndex == _node.SlotCount)
					{
						break;
					}
					GreenNode slot = _node.GetSlot(_childIndex);
					if (slot != null)
					{
						if (slot.RawKind != 1)
						{
							_currentChild = slot;
							return true;
						}
						_list = slot;
						_listIndex++;
						if (_listIndex < _list.SlotCount)
						{
							_currentChild = _list.GetSlot(_listIndex);
							return true;
						}
						_list = null;
						_listIndex = -1;
					}
				}
			}
			_currentChild = null;
			return false;
		}
	}

	internal struct Reversed
	{
		internal struct Enumerator
		{
			private readonly GreenNode _node;

			private int _childIndex;

			private GreenNode _list;

			private int _listIndex;

			private GreenNode _currentChild;

			public GreenNode Current => _currentChild;

			internal Enumerator(GreenNode node)
			{
				if (node != null)
				{
					_node = node;
					_childIndex = node.SlotCount;
					_listIndex = -1;
				}
				else
				{
					_node = null;
					_childIndex = 0;
					_listIndex = -1;
				}
				_list = null;
				_currentChild = null;
			}

			public bool MoveNext()
			{
				if (_node != null)
				{
					if (_list != null)
					{
						if (--_listIndex >= 0)
						{
							_currentChild = _list.GetSlot(_listIndex);
							return true;
						}
						_list = null;
						_listIndex = -1;
					}
					while (--_childIndex >= 0)
					{
						GreenNode slot = _node.GetSlot(_childIndex);
						if (slot != null)
						{
							if (!slot.IsList)
							{
								_currentChild = slot;
								return true;
							}
							_list = slot;
							_listIndex = _list.SlotCount;
							if (--_listIndex >= 0)
							{
								_currentChild = _list.GetSlot(_listIndex);
								return true;
							}
							_list = null;
							_listIndex = -1;
						}
					}
				}
				_currentChild = null;
				return false;
			}
		}

		private readonly GreenNode _node;

		internal Reversed(GreenNode node)
		{
			_node = node;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(_node);
		}
	}

	private readonly GreenNode _node;

	private int _count;

	public int Count
	{
		get
		{
			if (_count == -1)
			{
				_count = CountNodes();
			}
			return _count;
		}
	}

	private GreenNode[] Nodes
	{
		get
		{
			GreenNode[] array = new GreenNode[Count];
			int num = 0;
			Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				GreenNode current = enumerator.Current;
				array[num++] = current;
			}
			return array;
		}
	}

	internal ChildSyntaxList(GreenNode node)
	{
		_node = node;
		_count = -1;
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
		return new Enumerator(_node);
	}

	public Reversed Reverse()
	{
		return new Reversed(_node);
	}
}
