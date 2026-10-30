using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class ScopeAttributeEmitter : IAttributeEmitter
{
	private const string internalScope = "Internal";

	public static IAttributeEmitter Instance { get; } = new ScopeAttributeEmitter();

	public AttributeSyntax EmitAttribute(MethodCodeGenerator generator, AttributeTypeInfo attributeInfo, AttributeSymbol attribute)
	{
		if (string.Equals(attribute.Arguments[0].ValueText, "Internal", StringComparison.OrdinalIgnoreCase))
		{
			return CodeGenFacts.NavFunctionVisibilityAttribute_Internal;
		}
		return CodeGenFacts.NavFunctionVisibilityAttribute_External;
	}
}
