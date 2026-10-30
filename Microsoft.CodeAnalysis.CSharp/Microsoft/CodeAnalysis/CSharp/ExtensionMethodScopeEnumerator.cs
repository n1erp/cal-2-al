namespace Microsoft.CodeAnalysis.CSharp;

internal struct ExtensionMethodScopeEnumerator(Binder binder)
{
	private readonly Binder _binder = binder;

	private ExtensionMethodScope _current = default;

	public ExtensionMethodScope Current => _current;

	public bool MoveNext()
	{
		if (_current.Binder == null)
		{
			_current = GetNextScope(_binder);
		}
		else
		{
			Binder binder = _current.Binder;
			if (!_current.SearchUsingsNotNamespace)
			{
				_current = new ExtensionMethodScope(binder, searchUsingsNotNamespace: true);
			}
			else
			{
				_current = GetNextScope(binder.Next);
			}
		}
		return _current.Binder != null;
	}

	private static ExtensionMethodScope GetNextScope(Binder binder)
	{
		for (Binder binder2 = binder; binder2 != null; binder2 = binder2.Next)
		{
			if (binder2.SupportsExtensionMethods)
			{
				return new ExtensionMethodScope(binder2, searchUsingsNotNamespace: false);
			}
		}
		return default;
	}
}
