using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.CodeAnalysis.Operations;

internal interface IBoundNodeWithIOperationChildren
{
	ImmutableArray<BoundNode> Children { get; }
}
