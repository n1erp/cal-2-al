using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal interface ICodeEmitter<out T> where T : MemberDeclarationSyntax
{
	T Emit();
}
