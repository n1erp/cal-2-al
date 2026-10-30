using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis;

internal class IdentifierCollection
{
	private abstract class CollectionBase : ICollection<string>, IEnumerable<string>, IEnumerable
	{
		protected readonly IdentifierCollection IdentifierCollection;

		private int _count = -1;

		public int Count
		{
			get
			{
				if (_count == -1)
				{
					_count = IdentifierCollection._map.Values.Sum((object o) => (o is string) ? 1 : ((ISet<string>)o).Count);
				}
				return _count;
			}
		}

		public bool IsReadOnly => true;

		protected CollectionBase(IdentifierCollection identifierCollection)
		{
			IdentifierCollection = identifierCollection;
		}

		public abstract bool Contains(string item);

		public void CopyTo(string[] array, int arrayIndex)
		{
			using IEnumerator<string> enumerator = GetEnumerator();
			while (arrayIndex < array.Length && enumerator.MoveNext())
			{
				array[arrayIndex] = enumerator.Current;
				arrayIndex++;
			}
		}

		public IEnumerator<string> GetEnumerator()
		{
			foreach (object value in IdentifierCollection._map.Values)
			{
				if (value is HashSet<string> hashSet)
				{
					foreach (string item in hashSet)
					{
						yield return item;
					}
				}
				else
				{
					yield return (string)value;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(string item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Remove(string item)
		{
			throw new NotSupportedException();
		}
	}

	private sealed class CaseSensitiveCollection : CollectionBase
	{
		public CaseSensitiveCollection(IdentifierCollection identifierCollection)
			: base(identifierCollection)
		{
		}

		public override bool Contains(string item)
		{
			return IdentifierCollection.CaseSensitiveContains(item);
		}
	}

	private sealed class CaseInsensitiveCollection : CollectionBase
	{
		public CaseInsensitiveCollection(IdentifierCollection identifierCollection)
			: base(identifierCollection)
		{
		}

		public override bool Contains(string item)
		{
			return IdentifierCollection.CaseInsensitiveContains(item);
		}
	}

	private readonly Dictionary<string, object> _map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

	public IdentifierCollection()
	{
	}

	public IdentifierCollection(IEnumerable<string> identifiers)
	{
		AddIdentifiers(identifiers);
	}

	public void AddIdentifiers(IEnumerable<string> identifiers)
	{
		foreach (string identifier in identifiers)
		{
			AddIdentifier(identifier);
		}
	}

	public void AddIdentifier(string identifier)
	{
		if (!_map.TryGetValue(identifier, out var value))
		{
			AddInitialSpelling(identifier);
		}
		else
		{
			AddAdditionalSpelling(identifier, value);
		}
	}

	private void AddAdditionalSpelling(string identifier, object value)
	{
		if (value is string text)
		{
			if (!string.Equals(identifier, text, StringComparison.Ordinal))
			{
				_map[identifier] = new HashSet<string> { identifier, text };
			}
		}
		else
		{
			((HashSet<string>)value).Add(identifier);
		}
	}

	private void AddInitialSpelling(string identifier)
	{
		_map.Add(identifier, identifier);
	}

	public bool ContainsIdentifier(string identifier, bool caseSensitive)
	{
		if (caseSensitive)
		{
			return CaseSensitiveContains(identifier);
		}
		return CaseInsensitiveContains(identifier);
	}

	private bool CaseInsensitiveContains(string identifier)
	{
		return _map.ContainsKey(identifier);
	}

	private bool CaseSensitiveContains(string identifier)
	{
		if (_map.TryGetValue(identifier, out var value))
		{
			if (value is string b)
			{
				return string.Equals(identifier, b, StringComparison.Ordinal);
			}
			return ((HashSet<string>)value).Contains(identifier);
		}
		return false;
	}

	public ICollection<string> AsCaseSensitiveCollection()
	{
		return new CaseSensitiveCollection(this);
	}

	public ICollection<string> AsCaseInsensitiveCollection()
	{
		return new CaseInsensitiveCollection(this);
	}
}
