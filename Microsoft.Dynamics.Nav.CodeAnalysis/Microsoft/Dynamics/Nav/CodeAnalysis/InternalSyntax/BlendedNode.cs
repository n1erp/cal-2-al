using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerDisplay("{Node}, {Token}")]
internal struct BlendedNode
{
	internal readonly SyntaxNode Node;

	internal readonly InternalSyntaxToken Token;

	internal readonly Blender Blender;

	internal BlendedNode(SyntaxNode node, InternalSyntaxToken token, Blender blender)
	{
		Node = node;
		Token = token;
		Blender = blender;
	}
}
