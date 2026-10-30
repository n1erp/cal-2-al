using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal static class InternalSyntaxNodeExtensions
{
	public static TNode WithAnnotations<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : InternalSyntaxNode
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

	public static TNode WithAdditionalAnnotationsGreen<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : InternalSyntaxNode
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

	public static TNode WithoutAnnotationsGreen<TNode>(this TNode node, IEnumerable<SyntaxAnnotation> annotations) where TNode : InternalSyntaxNode
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

	public static TNode WithInternalDiagnostics<TNode>(this TNode node, DiagnosticInfo[] diagnostics) where TNode : InternalSyntaxNode
	{
		return (TNode)node.SetDiagnostics(diagnostics);
	}

	public static TNode WithoutInternalDiagnostics<TNode>(this TNode node) where TNode : InternalSyntaxNode
	{
		DiagnosticInfo[] diagnostics = node.GetDiagnostics();
		if (diagnostics == null || diagnostics.Length == 0)
		{
			return node;
		}
		return (TNode)node.SetDiagnostics(null);
	}

	public static InternalSyntaxList<InternalSyntaxToken> ToTokenList(this InternalSyntaxListBuilder builder)
	{
		if (builder == null)
		{
			return default;
		}
		return new InternalSyntaxList<InternalSyntaxToken>(builder.ToListNode());
	}

	public static InternalSyntaxList<InternalSyntaxNode> ToList(this InternalSyntaxListBuilder builder)
	{
		return builder.ToList<InternalSyntaxNode>();
	}

	public static InternalSyntaxList<TNode> ToList<TNode>(this InternalSyntaxListBuilder builder) where TNode : InternalSyntaxNode
	{
		if (builder == null)
		{
			return default(InternalSyntaxList<InternalSyntaxNode>);
		}
		return new InternalSyntaxList<TNode>(builder.ToListNode());
	}

	public static InternalSeparatedSyntaxList<TNode> ToSeparatedList<TNode>(this InternalSyntaxListBuilder builder) where TNode : InternalSyntaxNode
	{
		return builder?.ToList().AsSeparatedList<TNode>() ?? default(InternalSeparatedSyntaxList<TNode>);
	}

	internal static ErrorCode FirstErrorCode<TNode>(this TNode node) where TNode : InternalSyntaxNode
	{
		DiagnosticInfo[] diagnostics = node.GetDiagnostics();
		if (diagnostics == null || diagnostics.Length == 0)
		{
			return ErrorCode.None;
		}
		return (ErrorCode)diagnostics[0].Code;
	}

	internal static TNode WithDiagnosticsAndAnnotations<TNode>(this TNode node) where TNode : InternalSyntaxNode
	{
		DiagnosticInfo[] diagnostics = node.GetDiagnostics();
		if (diagnostics != null && diagnostics.Length != 0)
		{
			node = node.WithInternalDiagnostics(diagnostics);
		}
		SyntaxAnnotation[] annotations = node.GetAnnotations();
		if (annotations != null && annotations.Length != 0)
		{
			node = node.WithAnnotations(annotations);
		}
		return node;
	}

	internal static SyntaxNodeDefinition GetPropertySyntaxDefinition(this PropertySyntax property)
	{
		string name = property?.Name.Identifier.ValueText;
		if (SyntaxFacts.GetPropertyKind(name) == PropertyKind.DataItemLink && property.Parent.ParentIsKind(SyntaxKind.QueryDataItem))
		{
			return null;
		}
		return PropertyNameToSyntaxDefinition.GetPropertyValueDefinition(name);
	}
}
