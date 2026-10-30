namespace Microsoft.CodeAnalysis.CSharp;

internal struct ExtensionMethodScope(Binder binder, bool searchUsingsNotNamespace)
{
	public readonly Binder Binder = binder;

	public readonly bool SearchUsingsNotNamespace = searchUsingsNotNamespace;
}
