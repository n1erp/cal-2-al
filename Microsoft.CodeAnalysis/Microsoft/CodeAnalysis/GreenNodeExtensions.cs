using System.Collections.Generic;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal static class GreenNodeExtensions
{
	public static TNode WithAnnotationsGreen<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : GreenNode
	{
		ArrayBuilder<SyntaxAnnotation> instance = ArrayBuilder<SyntaxAnnotation>.GetInstance();
		foreach (SyntaxAnnotation annotation in annotations)
		{
			if (!instance.Contains(annotation))
			{
				instance.Add(annotation);
			}
		}
		if (instance.Count == 0)
		{
			instance.Free();
			SyntaxAnnotation[] annotations2 = node.GetAnnotations();
			if (annotations2 == null || annotations2.Length == 0)
			{
				return node;
			}
			return (TNode)node.SetAnnotations(null);
		}
		return (TNode)node.SetAnnotations(instance.ToArrayAndFree());
	}

	public static TNode WithAdditionalAnnotationsGreen<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : GreenNode
	{
		SyntaxAnnotation[] annotations2 = node.GetAnnotations();
		if (annotations == null)
		{
			return node;
		}
		ArrayBuilder<SyntaxAnnotation> instance = ArrayBuilder<SyntaxAnnotation>.GetInstance();
		instance.AddRange(annotations2);
		foreach (SyntaxAnnotation annotation in annotations)
		{
			if (!instance.Contains(annotation))
			{
				instance.Add(annotation);
			}
		}
		if (instance.Count == annotations2.Length)
		{
			instance.Free();
			return node;
		}
		return (TNode)node.SetAnnotations(instance.ToArrayAndFree());
	}

	public static TNode WithoutAnnotationsGreen<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : GreenNode
	{
		SyntaxAnnotation[] annotations2 = node.GetAnnotations();
		if (annotations == null || annotations2.Length == 0)
		{
			return node;
		}
		ArrayBuilder<SyntaxAnnotation> instance = ArrayBuilder<SyntaxAnnotation>.GetInstance();
		instance.AddRange(annotations);
		try
		{
			if (instance.Count == 0)
			{
				return node;
			}
			ArrayBuilder<SyntaxAnnotation> instance2 = ArrayBuilder<SyntaxAnnotation>.GetInstance();
			SyntaxAnnotation[] array = annotations2;
			foreach (SyntaxAnnotation item in array)
			{
				if (!instance.Contains(item))
				{
					instance2.Add(item);
				}
			}
			return (TNode)node.SetAnnotations(instance2.ToArrayAndFree());
		}
		finally
		{
			instance.Free();
		}
	}

	public static TNode WithDiagnosticsGreen<TNode>(this TNode node, DiagnosticInfo[] diagnostics) where TNode : GreenNode
	{
		return (TNode)node.SetDiagnostics(diagnostics);
	}

	public static TNode WithoutDiagnosticsGreen<TNode>(this TNode node) where TNode : GreenNode
	{
		DiagnosticInfo[] diagnostics = node.GetDiagnostics();
		if (diagnostics == null || diagnostics.Length == 0)
		{
			return node;
		}
		return (TNode)node.SetDiagnostics(null);
	}
}
