using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class CodeBuilder
{
	protected internal ModuleBuilder ModuleBuilder { get; }

	protected CodeBuilder(ModuleBuilder moduleBuilder)
	{
		ModuleBuilder = moduleBuilder;
	}

	public abstract NamespaceDeclarationSyntax CreateNamespaceDeclaration();

	public abstract ClassDeclarationSyntax CreateClassDeclaration();

	public abstract ClassDeclarationSyntax AddGlobalVariables(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddInitializeComponentMethod(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddEvaluateCaptionClass(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddFrameworkMethods(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddConstructMethod(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddPermissionList(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddMethods(ClassDeclarationSyntax classDeclaration);

	public abstract ClassDeclarationSyntax AddNonUserCode(ClassDeclarationSyntax classDeclaration);
}
