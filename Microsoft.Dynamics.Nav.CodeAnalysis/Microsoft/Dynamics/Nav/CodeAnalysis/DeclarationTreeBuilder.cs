using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class DeclarationTreeBuilder : SyntaxVisitor<SingleDeclaration>
{
	private readonly SyntaxTree syntaxTree;

	private DeclarationTreeBuilder(SyntaxTree syntaxTree)
	{
		this.syntaxTree = syntaxTree;
	}

	public static RootObjectCollectionDeclaration ForTree(SyntaxTree syntaxTree)
	{
		return (RootObjectCollectionDeclaration)new DeclarationTreeBuilder(syntaxTree).Visit(syntaxTree.GetRoot());
	}

	public override SingleDeclaration VisitObjectCompilationUnit(ObjectCompilationUnitSyntax node)
	{
		SyntaxList<ObjectSyntax> objects = node.Objects;
		ArrayBuilder<SingleObjectDeclaration> instance = ArrayBuilder<SingleObjectDeclaration>.GetInstance();
		ImmutableArray<SingleObjectDeclaration> children;
		try
		{
			foreach (ObjectSyntax item in objects)
			{
				instance.Add(CreateSingleObjectDeclaration(item));
			}
		}
		finally
		{
			children = instance.ToImmutableAndFree();
		}
		return new RootObjectCollectionDeclaration(syntaxTree.GetReference(node), new SourceLocation(node), children);
	}

	public SingleObjectDeclaration CreateSingleObjectDeclaration(ObjectSyntax node)
	{
		return node.Kind switch
		{
			SyntaxKind.CodeunitObject => CreateObjectDeclaration(DeclarationKind.Codeunit, (CodeunitSyntax)node), 
			SyntaxKind.TableObject => CreateObjectDeclaration(DeclarationKind.Table, (TableSyntax)node), 
			SyntaxKind.TableExtensionObject => CreateObjectDeclaration(DeclarationKind.TableExtension, (TableExtensionSyntax)node), 
			SyntaxKind.PageObject => CreateObjectDeclaration(DeclarationKind.Page, (PageSyntax)node), 
			SyntaxKind.PageExtensionObject => CreateObjectDeclaration(DeclarationKind.PageExtension, (PageExtensionSyntax)node), 
			SyntaxKind.XmlPortObject => CreateObjectDeclaration(DeclarationKind.XmlPort, (XmlPortSyntax)node), 
			SyntaxKind.ReportObject => CreateObjectDeclaration(DeclarationKind.Report, (ReportSyntax)node), 
			SyntaxKind.ProfileObject => CreateObjectDeclaration(DeclarationKind.Profile, (ProfileSyntax)node), 
			SyntaxKind.QueryObject => CreateObjectDeclaration(DeclarationKind.Query, (QuerySyntax)node), 
			SyntaxKind.PageCustomizationObject => CreateObjectDeclaration(DeclarationKind.PageCustomization, (PageCustomizationSyntax)node), 
			SyntaxKind.ControlAddInObject => CreateObjectDeclaration(DeclarationKind.ControlAddIn, (ControlAddInSyntax)node), 
			SyntaxKind.DotNetPackage => CreateObjectDeclaration(DeclarationKind.DotNetPackage, (DotNetPackageSyntax)node), 
			SyntaxKind.EnumType => CreateObjectDeclaration(DeclarationKind.EnumType, (EnumTypeSyntax)node), 
			SyntaxKind.EnumExtensionType => CreateObjectDeclaration(DeclarationKind.EnumExtensionType, (EnumExtensionTypeSyntax)node), 
			_ => throw ExceptionUtilities.UnexpectedValue(node.Kind), 
		};
	}

	public SingleObjectDeclaration CreateObjectDeclaration(DeclarationKind kind, ObjectSyntax node)
	{
		int objectId = 0;
		if (node.Kind.IsApplicationObject())
		{
			objectId = ((ApplicationObjectSyntax)node).ObjectId.GetObjectIdOrDefault();
		}
		string name = node.Name?.Identifier.ValueText ?? string.Empty;
		return new SingleObjectDeclaration(kind, objectId, name, syntaxTree.GetReference(node), new SourceLocation(node));
	}
}
