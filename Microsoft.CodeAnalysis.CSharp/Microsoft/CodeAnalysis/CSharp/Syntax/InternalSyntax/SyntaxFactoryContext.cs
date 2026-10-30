namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class SyntaxFactoryContext
{
	internal bool IsInAsync;

	internal int QueryDepth;

	internal bool IsInQuery => QueryDepth > 0;
}
