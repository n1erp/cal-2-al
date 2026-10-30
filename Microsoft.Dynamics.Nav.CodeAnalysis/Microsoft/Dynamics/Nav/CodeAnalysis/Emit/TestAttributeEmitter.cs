using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class TestAttributeEmitter : IAttributeEmitter
{
	private static readonly NameSyntax NavTestAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavTest");

	private static readonly IdentifierNameSyntax TestMethodNoNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("TestMethodNo");

	private static readonly IdentifierNameSyntax TransactionModelNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("TransactionModel");

	private static readonly IdentifierNameSyntax TestPermissionsNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("TestPermissions");

	private static readonly IdentifierNameSyntax HandlersNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Handlers");

	public static IAttributeEmitter Instance { get; } = new TestAttributeEmitter();

	public AttributeSyntax EmitAttribute(MethodCodeGenerator generator, AttributeTypeInfo attributeInfo, AttributeSymbol attribute)
	{
		AttributeSyntax attributeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(NavTestAttributeNameSyntax).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.LiteralExpression(generator.Method.Name)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NameEquals(TestMethodNoNameSyntax), null, CodeGenHelper.LiteralExpression(generator.Method.GetFirstSyntax().SpanStart)));
		attributeSyntax = AddAttributeAsNamedArgument(generator, AttributeCategory.HandlerFunctions, HandlersNameSyntax, attributeSyntax);
		attributeSyntax = AddAttributeAsNamedArgument(generator, AttributeCategory.TransactionModel, TransactionModelNameSyntax, attributeSyntax);
		return AddTestPermissions(generator, attributeSyntax, attribute);
	}

	private static AttributeSyntax AddAttributeAsNamedArgument(MethodCodeGenerator generator, AttributeCategory kind, IdentifierNameSyntax namedParameterSyntax, AttributeSyntax attributeSyntax)
	{
		AttributeSymbol attributeSymbol = generator.Method.Attributes.SingleOrDefault((AttributeSymbol at) => at.AttributeInfo.Category == kind);
		if (attributeSymbol != null)
		{
			BoundExpression boundExpression = attributeSymbol.Attribute.Arguments.SingleOrDefault();
			if (boundExpression != null)
			{
				attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NameEquals(namedParameterSyntax), null, generator.EmitExpression(boundExpression)));
			}
		}
		return attributeSyntax;
	}

	private static AttributeSyntax AddTestPermissions(MethodCodeGenerator generator, AttributeSyntax attributeSyntax, AttributeSymbol attributeSymbol)
	{
		AttributeSymbol attributeSymbol2 = generator.Method.Attributes.SingleOrDefault((AttributeSymbol at) => at.AttributeInfo.Category == AttributeCategory.TestPermissions);
		attributeSyntax = ((!(attributeSymbol2 != null)) ? attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NameEquals(TestPermissionsNameSyntax), null, CodeunitTestPermissionsProperty(attributeSymbol))) : ((attributeSymbol2.Attribute.Arguments.SingleOrDefault() is BoundOptionAccess boundOptionAccess && boundOptionAccess.OptionSymbol.Ordinal != 0) ? attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NameEquals(TestPermissionsNameSyntax), null, generator.EmitExpression(boundOptionAccess))) : attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NameEquals(TestPermissionsNameSyntax), null, CodeunitTestPermissionsProperty(attributeSymbol)))));
		return attributeSyntax;
	}

	private static ExpressionSyntax CodeunitTestPermissionsProperty(AttributeSymbol attributeSymbol)
	{
		TestPermissionsKind enumPropertyValueOrDefault = attributeSymbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>().GetEnumPropertyValueOrDefault<TestPermissionsKind>(PropertyKind.TestPermissions);
		if (enumPropertyValueOrDefault != TestPermissionsKind.InheritFromTestCodeunit)
		{
			return CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavTestPermissions"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(enumPropertyValueOrDefault.ToString()));
		}
		PropertyTypeInfo propertyTypeInfo = ObjectParser.LookupCodeunitProperty("TestPermissions");
		return CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavTestPermissions"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(propertyTypeInfo.DefaultValue));
	}
}
