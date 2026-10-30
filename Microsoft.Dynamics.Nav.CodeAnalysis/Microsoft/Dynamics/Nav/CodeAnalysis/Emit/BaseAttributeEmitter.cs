using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class BaseAttributeEmitter : IAttributeEmitter
{
	public static IAttributeEmitter Instance { get; } = new BaseAttributeEmitter();

	public AttributeSyntax EmitAttribute(MethodCodeGenerator generator, AttributeTypeInfo attributeInfo, AttributeSymbol attribute)
	{
		AttributeSyntax attributeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName(attributeInfo.MetadataName));
		if (!string.IsNullOrEmpty(attributeInfo.EmitConstantExpressionArgument))
		{
			attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression(attributeInfo.EmitConstantExpressionArgument)));
		}
		foreach (BoundExpression argument in attribute.Attribute.Arguments)
		{
			attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(generator.EmitExpression(argument)));
		}
		return attributeSyntax;
	}
}
