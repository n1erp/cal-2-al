using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct DeclarationInfo
{
	public SyntaxNode DeclaredNode { get; }

	public ImmutableArray<SyntaxNode> ExecutableCodeBlocks { get; }

	public ISymbol DeclaredSymbol { get; }

	internal DeclarationInfo(SyntaxNode declaredNode, ImmutableArray<SyntaxNode> executableCodeBlocks, ISymbol declaredSymbol)
	{
		DebugAssertHelper.Assert(declaredNode != null);
		DebugAssertHelper.Assert(!executableCodeBlocks.IsDefault);
		DeclaredNode = declaredNode;
		ExecutableCodeBlocks = executableCodeBlocks;
		DeclaredSymbol = declaredSymbol;
	}
}
