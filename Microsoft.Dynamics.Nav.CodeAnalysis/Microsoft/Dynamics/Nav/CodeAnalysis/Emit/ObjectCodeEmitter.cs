using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class ObjectCodeEmitter : ICodeEmitter<ClassDeclarationSyntax>
{
	private readonly CodeBuilder codeBuilder;

	public ObjectCodeEmitter(CodeBuilder codeBuilder)
	{
		this.codeBuilder = codeBuilder;
	}

	public ClassDeclarationSyntax Emit()
	{
		ClassDeclarationSyntax classDeclaration = codeBuilder.CreateClassDeclaration();
		classDeclaration = codeBuilder.AddGlobalVariables(classDeclaration);
		classDeclaration = codeBuilder.AddConstructor(classDeclaration);
		classDeclaration = codeBuilder.AddInitializeComponentMethod(classDeclaration);
		classDeclaration = codeBuilder.AddProperties(classDeclaration);
		classDeclaration = codeBuilder.AddOnInvoke(classDeclaration);
		classDeclaration = codeBuilder.AddEvaluateCaptionClass(classDeclaration);
		classDeclaration = codeBuilder.AddFrameworkMethods(classDeclaration);
		classDeclaration = codeBuilder.AddConstructMethod(classDeclaration);
		classDeclaration = codeBuilder.AddPermissionList(classDeclaration);
		classDeclaration = codeBuilder.AddMethods(classDeclaration);
		return codeBuilder.AddNonUserCode(classDeclaration);
	}
}
