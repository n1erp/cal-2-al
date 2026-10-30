using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis;

public static class AnnotationExtensions
{
	public static TNode WithAdditionalAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : SyntaxNode
	{
		return (TNode)node.WithAdditionalAnnotationsInternal(annotations);
	}

	public static TNode WithAdditionalAnnotations<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : SyntaxNode
	{
		return (TNode)node.WithAdditionalAnnotationsInternal(annotations);
	}

	public static TNode WithoutAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : SyntaxNode
	{
		return (TNode)node.GetNodeWithoutAnnotations(annotations);
	}

	public static TNode WithoutAnnotations<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : SyntaxNode
	{
		return (TNode)node.GetNodeWithoutAnnotations(annotations);
	}

	public static TNode WithoutAnnotations<TNode>(this TNode node, string annotationKind) where TNode : SyntaxNode
	{
		if (node.HasAnnotations(annotationKind))
		{
			return node.WithoutAnnotations(node.GetAnnotations(annotationKind).ToArray());
		}
		return node;
	}
}
