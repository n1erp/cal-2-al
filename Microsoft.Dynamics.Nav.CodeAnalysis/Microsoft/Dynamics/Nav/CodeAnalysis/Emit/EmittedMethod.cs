using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class EmittedMethod
{
	internal MethodSymbol Method { get; private set; }

	internal ImmutableArray<FieldDeclarationSyntax> Fields { get; private set; }

	internal MethodDeclarationSyntax MethodDeclaration { get; private set; }

	internal ClassDeclarationSyntax MethodScope { get; private set; }

	public EmittedMethod(MethodSymbol method, MethodDeclarationSyntax methodDeclaration, ClassDeclarationSyntax methodScopeDeclaration, ImmutableArray<FieldDeclarationSyntax> fields)
	{
		Method = method;
		Fields = fields;
		MethodDeclaration = methodDeclaration;
		MethodScope = methodScopeDeclaration;
	}

	internal EmittedMethod WithDeclaration(MethodDeclarationSyntax methodDeclaration)
	{
		return new EmittedMethod(Method, methodDeclaration, MethodScope, Fields);
	}
}
