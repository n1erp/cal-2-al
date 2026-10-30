using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal struct DeclarationInfo
{
	private readonly SyntaxNode _declaredNode;

	private readonly ImmutableArray<SyntaxNode> _executableCodeBlocks;

	private readonly ISymbol _declaredSymbol;

	public SyntaxNode DeclaredNode => _declaredNode;

	public ImmutableArray<SyntaxNode> ExecutableCodeBlocks => _executableCodeBlocks;

	public ISymbol DeclaredSymbol => _declaredSymbol;

	internal DeclarationInfo(SyntaxNode declaredNode, ImmutableArray<SyntaxNode> executableCodeBlocks, ISymbol declaredSymbol)
	{
		_declaredNode = declaredNode;
		_executableCodeBlocks = executableCodeBlocks;
		_declaredSymbol = declaredSymbol;
	}
}
