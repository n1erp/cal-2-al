using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class ApplicationObjectCodeEmitter : ICodeEmitter<NamespaceDeclarationSyntax>
{
	private readonly CodeBuilder codeBuilder;

	private readonly ObjectCodeEmitter objectEmitter;

	public ApplicationObjectCodeEmitter(CodeBuilder codeBuilder)
	{
		objectEmitter = new ObjectCodeEmitter(codeBuilder);
		this.codeBuilder = codeBuilder;
	}

	public NamespaceDeclarationSyntax Emit()
	{
		ClassDeclarationSyntax classDeclarationSyntax = objectEmitter.Emit();
		NamespaceDeclarationSyntax namespaceDeclarationSyntax = codeBuilder.CreateNamespaceDeclaration().AddMembers(classDeclarationSyntax);
		if (codeBuilder.ModuleBuilder.Options.EmitCodeAsAst)
		{
			return namespaceDeclarationSyntax;
		}
		return namespaceDeclarationSyntax.NormalizeWhitespace();
	}
}
