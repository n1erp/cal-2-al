using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class ApplicationObjectEmitter
{
	private ObjectTypeSymbol Symbol { get; }

	private ModuleBuilder ModuleBuilder { get; }

	private ICodeEmitter<MemberDeclarationSyntax> CodeEmitter { get; }

	public ApplicationObjectEmitter(ObjectTypeSymbol symbol, ModuleBuilder moduleBuilder, ICodeEmitter<MemberDeclarationSyntax> codeEmitter)
	{
		DebugAssertHelper.Assert(symbol != null);
		DebugAssertHelper.Assert(moduleBuilder != null);
		DebugAssertHelper.Assert(codeEmitter != null);
		Symbol = symbol;
		ModuleBuilder = moduleBuilder;
		CodeEmitter = codeEmitter;
	}

	public void EmitObjectCode()
	{
		MemberDeclarationSyntax memberDeclarationSyntax = CodeEmitter.Emit();
		CompilationUnitSyntax syntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CompilationUnit().AddMembers(memberDeclarationSyntax);
		string alSourceCode = Symbol.DeclaringSyntaxReferences.Single().GetSyntax().ToFullString();
		ModuleBuilder.SetApplicationObjectCode(Symbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>(), syntax, alSourceCode);
	}
}
