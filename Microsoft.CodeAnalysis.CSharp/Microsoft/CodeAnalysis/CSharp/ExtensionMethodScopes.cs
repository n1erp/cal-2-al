namespace Microsoft.CodeAnalysis.CSharp;

internal struct ExtensionMethodScopes(Binder binder)
{
	private readonly Binder _binder = binder;

	public ExtensionMethodScopeEnumerator GetEnumerator()
	{
		return new ExtensionMethodScopeEnumerator(_binder);
	}
}
