using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class MemberSemanticModelFactory
{
	private readonly SemanticModel semanticModel;

	private readonly BinderFactory binderFactory;

	public MemberSemanticModelFactory(SemanticModel semanticModel, BinderFactory binderFactory)
	{
		this.semanticModel = semanticModel;
		this.binderFactory = binderFactory;
	}

	public MemberSemanticModel CreateMemberModel(SyntaxNode node)
	{
		Binder binder = binderFactory.GetBinder(node);
		if (semanticModel.IgnoresAccessibility)
		{
			binder = binder.WithAdditionalFlags(BinderFlags.IgnoreAccessibility);
		}
		switch (node.Kind)
		{
		case SyntaxKind.Property:
			return CreatePropertySemanticModel((PropertySyntax)node, binder);
		case SyntaxKind.Block:
			return CreateMethodBodySemanticModel(node.Parent as MethodOrTriggerDeclarationSyntax, binder);
		case SyntaxKind.MemberAttribute:
			if (node.Parent is MethodOrTriggerDeclarationSyntax memberDecl)
			{
				return CreateMethodBodySemanticModel(memberDecl, binder);
			}
			break;
		case SyntaxKind.PageField:
			return CreatePageFieldSemanticModel((PageFieldSyntax)node, binder);
		case SyntaxKind.ReportDataItem:
			return CreateReportDataItemSemanticModel((ReportDataItemSyntax)node, binder);
		case SyntaxKind.ReportColumn:
			return CreateReportColumnSemanticModel((ReportColumnSyntax)node, binder);
		case SyntaxKind.XmlPortTableElement:
		case SyntaxKind.XmlPortFieldElement:
		case SyntaxKind.XmlPortTextElement:
		case SyntaxKind.XmlPortFieldAttribute:
		case SyntaxKind.XmlPortTextAttribute:
			return CreateXmlPortNodeSemanticModel((XmlPortNodeSyntax)node, binder);
		case SyntaxKind.QueryDataItem:
			return CreateQueryDataItemSemanticModel((QueryDataItemSyntax)node, binder);
		case SyntaxKind.QueryFilter:
			return CreateQueryFilterSemanticModel((QueryFilterSyntax)node, binder);
		case SyntaxKind.QueryColumn:
			return CreateQueryColumnSemanticModel((QueryColumnSyntax)node, binder);
		}
		return null;
	}

	private MemberSemanticModel CreateMethodBodySemanticModel(MethodOrTriggerDeclarationSyntax memberDecl, Binder rootBinder)
	{
		DebugAssertHelper.Assert(memberDecl != null, "Expected a MethodOrTriggerDeclarationSyntax node");
		if (memberDecl != null)
		{
			MethodSymbol methodSymbol = (MethodSymbol)semanticModel.GetDeclaredSymbol(memberDecl);
			if ((object)methodSymbol == null)
			{
				return null;
			}
			return MethodBodySemanticModel.Create(semanticModel.Compilation, methodSymbol, rootBinder, memberDecl);
		}
		return null;
	}

	private MemberSemanticModel CreatePropertySemanticModel(PropertySyntax node, Binder rootBinder)
	{
		DebugAssertHelper.Assert(node != null, "Expected a PropertySyntax node");
		if (node != null)
		{
			PropertySymbol declaredSymbol = semanticModel.GetDeclaredSymbol(node);
			if (declaredSymbol == null)
			{
				return null;
			}
			return PropertySemanticModel.Create(semanticModel.Compilation, declaredSymbol, rootBinder, node);
		}
		return null;
	}

	private MemberSemanticModel CreatePageFieldSemanticModel(PageFieldSyntax node, Binder rootBinder)
	{
		DebugAssertHelper.Assert(node != null, "Expected a PageFieldSyntax node");
		if (node != null)
		{
			ControlSymbol declaredSymbol = semanticModel.GetDeclaredSymbol(node);
			if (declaredSymbol == null)
			{
				return null;
			}
			return ControlSemanticModel.Create(semanticModel.Compilation, declaredSymbol, rootBinder, node);
		}
		return null;
	}

	private MemberSemanticModel CreateReportDataItemSemanticModel(ReportDataItemSyntax node, Binder rootBinder)
	{
		DebugAssertHelper.Assert(node != null, "Expected a ReportDataItemSyntax node");
		if (node != null)
		{
			TypeSymbol declaredSymbol = semanticModel.GetDeclaredSymbol(node);
			if (declaredSymbol == null)
			{
				return null;
			}
			return ReportDataItemSemanticModel.Create(semanticModel.Compilation, declaredSymbol, rootBinder, node);
		}
		return null;
	}

	private MemberSemanticModel CreateReportColumnSemanticModel(ReportColumnSyntax node, Binder rootBinder)
	{
		DebugAssertHelper.Assert(node != null, "Expected a ReportColumnSyntax node");
		if (node != null)
		{
			TypeSymbol declaredSymbol = semanticModel.GetDeclaredSymbol(node);
			if (declaredSymbol == null)
			{
				return null;
			}
			return ReportColumnSemanticModel.Create(semanticModel.Compilation, declaredSymbol, rootBinder, node);
		}
		return null;
	}

	private MemberSemanticModel CreateQueryColumnSemanticModel(QueryColumnSyntax node, Binder rootBinder)
	{
		DebugAssertHelper.Assert(node != null, "Expected a QueryColumnSyntax node");
		if (node != null)
		{
			TypeSymbol declaredSymbol = semanticModel.GetDeclaredSymbol(node);
			if (declaredSymbol == null)
			{
				return null;
			}
			return QueryColumnSemanticModel.Create(semanticModel.Compilation, declaredSymbol, rootBinder, node);
		}
		return null;
	}

	private MemberSemanticModel CreateQueryFilterSemanticModel(QueryFilterSyntax node, Binder rootBinder)
	{
		DebugAssertHelper.Assert(node != null, "Expected a QueryFilterSyntax node");
		if (node != null)
		{
			TypeSymbol declaredSymbol = semanticModel.GetDeclaredSymbol(node);
			if (declaredSymbol == null)
			{
				return null;
			}
			return QueryFilterSemanticModel.Create(semanticModel.Compilation, declaredSymbol, rootBinder, node);
		}
		return null;
	}

	private MemberSemanticModel CreateQueryDataItemSemanticModel(QueryDataItemSyntax node, Binder rootBinder)
	{
		DebugAssertHelper.Assert(node != null, "Expected a QueryDataItemSyntax node");
		if (node != null)
		{
			TypeSymbol declaredSymbol = semanticModel.GetDeclaredSymbol(node);
			if (declaredSymbol == null)
			{
				return null;
			}
			return QueryDataItemSemanticModel.Create(semanticModel.Compilation, declaredSymbol, rootBinder, node);
		}
		return null;
	}

	private MemberSemanticModel CreateXmlPortNodeSemanticModel(XmlPortNodeSyntax node, Binder rootBinder)
	{
		DebugAssertHelper.Assert(node != null, "Expected a ReportColumnSyntax node");
		if (node != null)
		{
			TypeSymbol declaredSymbol = semanticModel.GetDeclaredSymbol(node);
			if (declaredSymbol == null)
			{
				return null;
			}
			return XmlPortNodeSemanticModel.Create(semanticModel.Compilation, declaredSymbol, rootBinder, node);
		}
		return null;
	}
}
