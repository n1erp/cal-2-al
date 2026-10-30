namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class MergedNamespaceOrTypeDeclaration : Declaration
{
	protected MergedNamespaceOrTypeDeclaration(string name)
		: base(name)
	{
	}
}
