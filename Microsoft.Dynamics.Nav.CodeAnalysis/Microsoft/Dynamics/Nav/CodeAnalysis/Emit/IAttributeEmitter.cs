using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal interface IAttributeEmitter
{
	AttributeSyntax EmitAttribute(MethodCodeGenerator generator, AttributeTypeInfo attributeInfo, AttributeSymbol attribute);
}
