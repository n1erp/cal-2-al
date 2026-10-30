using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class CSharpDeclarationComputer : DeclarationComputer
{
	public static void ComputeDeclarationsInSpan(SemanticModel model, TextSpan span, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken)
	{
		ComputeDeclarations(model, model.SyntaxTree.GetRoot(cancellationToken), (SyntaxNode node, int? level) => !node.Span.OverlapsWith(span) || InvalidLevel(level), getSymbol, builder, null, cancellationToken);
	}

	public static void ComputeDeclarationsInNode(SemanticModel model, SyntaxNode node, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null)
	{
		ComputeDeclarations(model, node, (SyntaxNode n, int? level) => InvalidLevel(level), getSymbol, builder, levelsToCompute, cancellationToken);
	}

	private static bool InvalidLevel(int? level)
	{
		if (level.HasValue)
		{
			return level.Value <= 0;
		}
		return false;
	}

	private static int? DecrementLevel(int? level)
	{
		if (!level.HasValue)
		{
			return level;
		}
		return level - 1;
	}

	private static void ComputeDeclarations(SemanticModel model, SyntaxNode node, Func<SyntaxNode, int?, bool> shouldSkip, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, int? levelsToCompute, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (shouldSkip(node, levelsToCompute))
		{
			return;
		}
		int? levelsToCompute2 = DecrementLevel(levelsToCompute);
		switch (node.Kind())
		{
		case SyntaxKind.NamespaceDeclaration:
		{
			NamespaceDeclarationSyntax namespaceDeclarationSyntax = (NamespaceDeclarationSyntax)node;
			foreach (MemberDeclarationSyntax member in namespaceDeclarationSyntax.Members)
			{
				ComputeDeclarations(model, member, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
			}
			DeclarationInfo declarationInfo = DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, cancellationToken);
			builder.Add(declarationInfo);
			NameSyntax nameSyntax = namespaceDeclarationSyntax.Name;
			INamespaceSymbol namespaceSymbol = declarationInfo.DeclaredSymbol as INamespaceSymbol;
			while (nameSyntax.Kind() == SyntaxKind.QualifiedName)
			{
				nameSyntax = ((QualifiedNameSyntax)nameSyntax).Left;
				INamespaceSymbol namespaceSymbol2 = (getSymbol ? namespaceSymbol?.ContainingNamespace : null);
				builder.Add(new DeclarationInfo(nameSyntax, ImmutableArray<SyntaxNode>.Empty, namespaceSymbol2));
				namespaceSymbol = namespaceSymbol2;
			}
			break;
		}
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
		case SyntaxKind.InterfaceDeclaration:
		{
			TypeDeclarationSyntax typeDeclarationSyntax = (TypeDeclarationSyntax)node;
			foreach (MemberDeclarationSyntax member2 in typeDeclarationSyntax.Members)
			{
				ComputeDeclarations(model, member2, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
			}
			IEnumerable<SyntaxNode> executableCodeBlocks5 = GetAttributes(typeDeclarationSyntax.AttributeLists).Concat(GetTypeParameterListAttributes(typeDeclarationSyntax.TypeParameterList));
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, executableCodeBlocks5, cancellationToken));
			break;
		}
		case SyntaxKind.EnumDeclaration:
		{
			EnumDeclarationSyntax enumDeclarationSyntax = (EnumDeclarationSyntax)node;
			foreach (EnumMemberDeclarationSyntax member3 in enumDeclarationSyntax.Members)
			{
				ComputeDeclarations(model, member3, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
			}
			IEnumerable<SyntaxNode> attributes5 = GetAttributes(enumDeclarationSyntax.AttributeLists);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, attributes5, cancellationToken));
			break;
		}
		case SyntaxKind.EnumMemberDeclaration:
		{
			EnumMemberDeclarationSyntax enumMemberDeclarationSyntax = (EnumMemberDeclarationSyntax)node;
			IEnumerable<SyntaxNode> executableCodeBlocks4 = Enumerable.Concat(second: GetAttributes(enumMemberDeclarationSyntax.AttributeLists), first: SpecializedCollections.SingletonEnumerable(enumMemberDeclarationSyntax.EqualsValue));
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, executableCodeBlocks4, cancellationToken));
			break;
		}
		case SyntaxKind.DelegateDeclaration:
		{
			DelegateDeclarationSyntax delegateDeclarationSyntax = (DelegateDeclarationSyntax)node;
			IEnumerable<SyntaxNode> executableCodeBlocks = GetAttributes(delegateDeclarationSyntax.AttributeLists).Concat(GetParameterListInitializersAndAttributes(delegateDeclarationSyntax.ParameterList)).Concat(GetTypeParameterListAttributes(delegateDeclarationSyntax.TypeParameterList));
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, executableCodeBlocks, cancellationToken));
			break;
		}
		case SyntaxKind.EventDeclaration:
		{
			EventDeclarationSyntax eventDeclarationSyntax = (EventDeclarationSyntax)node;
			foreach (AccessorDeclarationSyntax accessor in eventDeclarationSyntax.AccessorList.Accessors)
			{
				ComputeDeclarations(model, accessor, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
			}
			IEnumerable<SyntaxNode> attributes6 = GetAttributes(eventDeclarationSyntax.AttributeLists);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, attributes6, cancellationToken));
			break;
		}
		case SyntaxKind.FieldDeclaration:
		case SyntaxKind.EventFieldDeclaration:
		{
			BaseFieldDeclarationSyntax baseFieldDeclarationSyntax = (BaseFieldDeclarationSyntax)node;
			IEnumerable<SyntaxNode> attributes3 = GetAttributes(baseFieldDeclarationSyntax.AttributeLists);
			foreach (VariableDeclaratorSyntax variable in baseFieldDeclarationSyntax.Declaration.Variables)
			{
				IEnumerable<SyntaxNode> executableCodeBlocks2 = SpecializedCollections.SingletonEnumerable(variable.Initializer).Concat(attributes3);
				builder.Add(DeclarationComputer.GetDeclarationInfo(model, variable, getSymbol, executableCodeBlocks2, cancellationToken));
			}
			break;
		}
		case SyntaxKind.ArrowExpressionClause:
			if (node.Parent is BasePropertyDeclarationSyntax declarationWithExpressionBody)
			{
				builder.Add(GetExpressionBodyDeclarationInfo(declarationWithExpressionBody, (ArrowExpressionClauseSyntax)node, model, getSymbol, cancellationToken));
			}
			break;
		case SyntaxKind.PropertyDeclaration:
		{
			PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)node;
			if (propertyDeclarationSyntax.AccessorList != null)
			{
				foreach (AccessorDeclarationSyntax accessor2 in propertyDeclarationSyntax.AccessorList.Accessors)
				{
					ComputeDeclarations(model, accessor2, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
				}
			}
			if (propertyDeclarationSyntax.ExpressionBody != null)
			{
				ComputeDeclarations(model, propertyDeclarationSyntax.ExpressionBody, shouldSkip, getSymbol, builder, levelsToCompute, cancellationToken);
			}
			IEnumerable<SyntaxNode> attributes4 = GetAttributes(propertyDeclarationSyntax.AttributeLists);
			IEnumerable<SyntaxNode> executableCodeBlocks3 = SpecializedCollections.SingletonEnumerable(propertyDeclarationSyntax.Initializer).Concat(attributes4);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, executableCodeBlocks3, cancellationToken));
			break;
		}
		case SyntaxKind.IndexerDeclaration:
		{
			IndexerDeclarationSyntax indexerDeclarationSyntax = (IndexerDeclarationSyntax)node;
			if (indexerDeclarationSyntax.AccessorList != null)
			{
				foreach (AccessorDeclarationSyntax accessor3 in indexerDeclarationSyntax.AccessorList.Accessors)
				{
					ComputeDeclarations(model, accessor3, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
				}
			}
			if (indexerDeclarationSyntax.ExpressionBody != null)
			{
				ComputeDeclarations(model, indexerDeclarationSyntax.ExpressionBody, shouldSkip, getSymbol, builder, levelsToCompute, cancellationToken);
			}
			IEnumerable<SyntaxNode> parameterListInitializersAndAttributes = GetParameterListInitializersAndAttributes(indexerDeclarationSyntax.ParameterList);
			IEnumerable<SyntaxNode> attributes2 = GetAttributes(indexerDeclarationSyntax.AttributeLists);
			parameterListInitializersAndAttributes = parameterListInitializersAndAttributes.Concat(attributes2);
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, parameterListInitializersAndAttributes, cancellationToken));
			break;
		}
		case SyntaxKind.GetAccessorDeclaration:
		case SyntaxKind.SetAccessorDeclaration:
		case SyntaxKind.AddAccessorDeclaration:
		case SyntaxKind.RemoveAccessorDeclaration:
		{
			AccessorDeclarationSyntax accessorDeclarationSyntax = (AccessorDeclarationSyntax)node;
			ArrayBuilder<SyntaxNode> instance = ArrayBuilder<SyntaxNode>.GetInstance();
			instance.AddIfNotNull(accessorDeclarationSyntax.Body);
			instance.AddIfNotNull(accessorDeclarationSyntax.ExpressionBody);
			instance.AddRange(GetAttributes(accessorDeclarationSyntax.AttributeLists));
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, instance, cancellationToken));
			instance.Free();
			break;
		}
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
		{
			BaseMethodDeclarationSyntax baseMethodDeclarationSyntax = (BaseMethodDeclarationSyntax)node;
			IEnumerable<SyntaxNode> parameterListInitializersAndAttributes2 = GetParameterListInitializersAndAttributes(baseMethodDeclarationSyntax.ParameterList);
			parameterListInitializersAndAttributes2 = parameterListInitializersAndAttributes2.Concat(baseMethodDeclarationSyntax.Body);
			if (baseMethodDeclarationSyntax is ConstructorDeclarationSyntax { Initializer: not null } constructorDeclarationSyntax)
			{
				parameterListInitializersAndAttributes2 = parameterListInitializersAndAttributes2.Concat(constructorDeclarationSyntax.Initializer);
			}
			ArrowExpressionClauseSyntax expressionBodySyntax = GetExpressionBodySyntax(baseMethodDeclarationSyntax);
			if (expressionBodySyntax != null)
			{
				parameterListInitializersAndAttributes2 = parameterListInitializersAndAttributes2.Concat(expressionBodySyntax);
			}
			parameterListInitializersAndAttributes2 = parameterListInitializersAndAttributes2.Concat(GetAttributes(baseMethodDeclarationSyntax.AttributeLists));
			if (node is MethodDeclarationSyntax { TypeParameterList: not null } methodDeclarationSyntax)
			{
				parameterListInitializersAndAttributes2 = parameterListInitializersAndAttributes2.Concat(GetTypeParameterListAttributes(methodDeclarationSyntax.TypeParameterList));
			}
			builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, parameterListInitializersAndAttributes2, cancellationToken));
			break;
		}
		case SyntaxKind.CompilationUnit:
		{
			CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)node;
			foreach (MemberDeclarationSyntax member4 in compilationUnitSyntax.Members)
			{
				ComputeDeclarations(model, member4, shouldSkip, getSymbol, builder, levelsToCompute2, cancellationToken);
			}
			if (compilationUnitSyntax.AttributeLists.Any())
			{
				IEnumerable<SyntaxNode> attributes = GetAttributes(compilationUnitSyntax.AttributeLists);
				builder.Add(DeclarationComputer.GetDeclarationInfo(model, node, getSymbol, attributes, cancellationToken));
			}
			break;
		}
		}
	}

	private static IEnumerable<SyntaxNode> GetAttributes(SyntaxList<AttributeListSyntax> attributeLists)
	{
		foreach (AttributeListSyntax item in attributeLists)
		{
			foreach (AttributeSyntax attribute in item.Attributes)
			{
				yield return attribute;
			}
		}
	}

	private static IEnumerable<SyntaxNode> GetParameterListInitializersAndAttributes(BaseParameterListSyntax parameterList)
	{
		if (parameterList == null)
		{
			return SpecializedCollections.EmptyEnumerable<SyntaxNode>();
		}
		return parameterList.Parameters.SelectMany((ParameterSyntax p) => GetParameterInitializersAndAttributes(p));
	}

	private static IEnumerable<SyntaxNode> GetParameterInitializersAndAttributes(ParameterSyntax parameter)
	{
		return SpecializedCollections.SingletonEnumerable(parameter.Default).Concat(GetAttributes(parameter.AttributeLists));
	}

	private static IEnumerable<SyntaxNode> GetTypeParameterListAttributes(TypeParameterListSyntax typeParameterList)
	{
		if (typeParameterList == null)
		{
			return SpecializedCollections.EmptyEnumerable<SyntaxNode>();
		}
		return typeParameterList.Parameters.SelectMany((TypeParameterSyntax p) => GetAttributes(p.AttributeLists));
	}

	private static DeclarationInfo GetExpressionBodyDeclarationInfo(BasePropertyDeclarationSyntax declarationWithExpressionBody, ArrowExpressionClauseSyntax expressionBody, SemanticModel model, bool getSymbol, CancellationToken cancellationToken)
	{
		IMethodSymbol declaredSymbol = ((!getSymbol) ? null : (model.GetDeclaredSymbol(declarationWithExpressionBody, cancellationToken) as IPropertySymbol)?.GetMethod);
		return new DeclarationInfo(expressionBody, ImmutableArray.Create((SyntaxNode)expressionBody), declaredSymbol);
	}

	internal static ArrowExpressionClauseSyntax GetExpressionBodySyntax(CSharpSyntaxNode node)
	{
		ArrowExpressionClauseSyntax result = null;
		switch (node.Kind())
		{
		case SyntaxKind.ArrowExpressionClause:
			result = (ArrowExpressionClauseSyntax)node;
			break;
		case SyntaxKind.MethodDeclaration:
			result = ((MethodDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.OperatorDeclaration:
			result = ((OperatorDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.ConversionOperatorDeclaration:
			result = ((ConversionOperatorDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.PropertyDeclaration:
			result = ((PropertyDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.IndexerDeclaration:
			result = ((IndexerDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.ConstructorDeclaration:
			result = ((ConstructorDeclarationSyntax)node).ExpressionBody;
			break;
		case SyntaxKind.DestructorDeclaration:
			result = ((DestructorDeclarationSyntax)node).ExpressionBody;
			break;
		default:
			ExceptionUtilities.UnexpectedValue(node.Kind());
			break;
		}
		return result;
	}
}
