using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal interface IMethodCallEmitter
{
	ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call);
}
