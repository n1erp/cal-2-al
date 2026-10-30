using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class CodeGenHelper
{
	internal static FieldDeclarationSyntax FieldDeclaration(SyntaxToken name, TypeSyntax type, params Microsoft.CodeAnalysis.CSharp.SyntaxKind[] modifiers)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(type).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(name))).AddModifiers(modifiers.Select(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token).ToArray());
	}

	internal static FieldDeclarationSyntax FieldDeclaration(SyntaxToken name, TypeSyntax type, ExpressionSyntax initialValue, params Microsoft.CodeAnalysis.CSharp.SyntaxKind[] modifiers)
	{
		VariableDeclaratorSyntax variableDeclaratorSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(name);
		if (initialValue != null)
		{
			variableDeclaratorSyntax = variableDeclaratorSyntax.WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(initialValue));
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(type).AddVariables(variableDeclaratorSyntax)).AddModifiers(modifiers.Select(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token).ToArray());
	}

	internal static ArrayTypeSyntax ArrayType(TypeSyntax elementType, int rank)
	{
		List<ArrayRankSpecifierSyntax> nodes = new List<ArrayRankSpecifierSyntax> { Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayRankSpecifier(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(Enumerable.Repeat((ExpressionSyntax)Microsoft.CodeAnalysis.CSharp.SyntaxFactory.OmittedArraySizeExpression(), rank))) };
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayType(elementType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.List(nodes));
	}

	internal static PropertyDeclarationSyntax PropertyDeclaration(string name, TypeSyntax type, ExpressionSyntax expression, params Microsoft.CodeAnalysis.CSharp.SyntaxKind[] modifiers)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(type, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(name)).AddModifiers(modifiers.Select(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token).ToArray()).AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration).WithBody(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(expression))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SetAccessorDeclaration).WithBody(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, expression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("value"))))));
	}

	internal static MemberAccessExpressionSyntax MemberAccessExpression(string expression, string name)
	{
		return MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(expression)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(name)));
	}

	internal static MemberAccessExpressionSyntax MemberAccessExpression(SyntaxToken expression, SyntaxToken name)
	{
		return MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(expression), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(name));
	}

	internal static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, IdentifierNameSyntax name)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, expression, name);
	}

	internal static MemberAccessExpressionSyntax MemberAccessExpression(ExpressionSyntax expression, SimpleNameSyntax name)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, expression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken), name);
	}

	internal static TypeSyntax GetApplicationObjectTypeWithSuffixTypeSyntax(this ApplicationObjectTypeSymbol applicationObjectType, EmitOptions options)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName(applicationObjectType.NavTypeKind.ToString() + applicationObjectType.Id.ToString(CultureInfo.InvariantCulture) + options.RuntimeMetadataSuffix);
	}

	internal static TypeSyntax GetObjectTypeWithSuffixTypeSyntax(this ObjectTypeSymbol objectType, EmitOptions options)
	{
		ApplicationObjectTypeSymbol applicationObjectTypeSymbol = objectType as ApplicationObjectTypeSymbol;
		if (applicationObjectTypeSymbol != null)
		{
			return applicationObjectTypeSymbol.GetApplicationObjectTypeWithSuffixTypeSyntax(options);
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName(objectType.NavTypeKind.ToString());
	}

	internal static AttributeListSyntax NavObjectIdAttribute(this ApplicationObjectTypeSymbol applicationObjectTypeSymbol)
	{
		return NavObjectIdAttribute(applicationObjectTypeSymbol.Id);
	}

	internal static AttributeListSyntax NavObjectIdAttribute(int id)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(CodeGenFacts.NavObjectIdAttributeNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NameEquals("ObjectId"), null, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(id))))));
	}

	internal static AttributeListSyntax NavCodeunitOptionsAttribute(bool singleInstance, bool eventManualBinding, int tableNo, CodeunitSubtypeKind codeunitSubtype)
	{
		ExpressionSyntax expression;
		if (singleInstance & eventManualBinding)
		{
			expression = CodeGenFacts.NavCodeunitOptionsSingleInstanceEventManualBinding;
		}
		else if (singleInstance)
		{
			expression = CodeGenFacts.NavCodeunitOptionsSingleInstance;
		}
		else
		{
			expression = ((!eventManualBinding) ? CodeGenFacts.NavCodeunitOptionsDefault : CodeGenFacts.NavCodeunitOptionsEventManualBinding);
		}
		LiteralExpressionSyntax expression2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(tableNo));
		codeunitSubtype = ((codeunitSubtype != CodeunitSubtypeKind.Install) ? codeunitSubtype : CodeunitSubtypeKind.Normal);
		MemberAccessExpressionSyntax expression3 = MemberAccessExpression("CodeunitSubType", codeunitSubtype.ToString());
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(CodeGenFacts.NavCodeunitOptionsAttributeNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(expression), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(expression2), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(expression3))));
	}

	internal static AttributeListSyntax NavNameAttribute(string name)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(CodeGenFacts.NavNameAttributeNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(LiteralExpression(name)))));
	}

	internal static AttributeListSyntax SourceSpansAttribute(IEnumerable<DebuggerSourceSpan> spans)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(CodeGenFacts.SourceSpansAttributeNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgumentList().AddArguments(spans.Select((DebuggerSourceSpan s) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(LiteralExpression((long)s))).ToArray())));
	}

	internal static AttributeListSyntax ReturnValueAttribute(string name = null)
	{
		AttributeSyntax attributeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(CodeGenFacts.ReturnValueAttributeNameSyntax);
		if (!string.IsNullOrEmpty(name))
		{
			attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(LiteralExpression(name)));
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(attributeSyntax);
	}

	internal static AttributeListSyntax FieldTriggerHandlerAttribute(string triggerType, int fieldNo)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(CodeGenFacts.FieldTriggerHandlerAttributeNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(MemberAccessExpression("FieldTriggerType", triggerType)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(LiteralExpression(fieldNo)))));
	}

	internal static LiteralExpressionSyntax LiteralExpression(char value)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.CharacterLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(value));
	}

	internal static LiteralExpressionSyntax LiteralExpression(string value)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(value));
	}

	internal static LiteralExpressionSyntax LiteralExpression(int value)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(value));
	}

	internal static LiteralExpressionSyntax LiteralExpression(uint value)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(value));
	}

	internal static LiteralExpressionSyntax LiteralExpression(long value)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(value));
	}

	internal static ExpressionSyntax LiteralExpression(decimal value)
	{
		ExpressionSyntax expressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(CodeGenFacts.RuntimeDecimalType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(Math.Abs(value))));
		if (value < 0m)
		{
			expressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PrefixUnaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.UnaryMinusExpression, expressionSyntax);
		}
		return expressionSyntax;
	}

	internal static ExpressionSyntax LiteralExpression(bool value)
	{
		if (!value)
		{
			return CodeGenFacts.FalseLiteralExpression;
		}
		return CodeGenFacts.TrueLiteralExpression;
	}

	internal static ObjectCreationExpressionSyntax NavDotNetObjectCreationExpression(ExpressionSyntax parent, string assemblyName, string typeName, bool runOnClient, bool isInterface, bool suppressDispose, ExpressionSyntax eventSink = null)
	{
		DebugAssertHelper.Assert(!suppressDispose || eventSink == null);
		ArgumentListSyntax argumentListSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(parent), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(assemblyName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(typeName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(runOnClient)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(isInterface)));
		if (suppressDispose)
		{
			argumentListSyntax = argumentListSyntax.AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(suppressDispose)));
		}
		if (eventSink != null)
		{
			argumentListSyntax = argumentListSyntax.AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(eventSink));
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavDotNetType, argumentListSyntax, null);
	}

	internal static ObjectCreationExpressionSyntax NavDotNetObjectCreationExpression(ExpressionSyntax parent, ExpressionSyntax value, bool runOnClient, bool isInterface)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavDotNetType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(parent), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(value), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(runOnClient)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(isInterface))), null);
	}

	internal static IdentifierNameSyntax NavEventHandlerClassName(VariableSymbol publisher, EmitOptions emitOptions)
	{
		DebugAssertHelper.Assert(publisher.Kind == SymbolKind.GlobalVariable);
		TypeSyntax objectTypeWithSuffixTypeSyntax = ((ObjectTypeSymbol)publisher.ContainingType).GetObjectTypeWithSuffixTypeSyntax(emitOptions);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(publisher.MetadataName + "_Eventhandler_" + objectTypeWithSuffixTypeSyntax);
	}

	internal static ExpressionSyntax CreateNavStringValue(BoundTextIndexAccess boundExpression, ExpressionSyntax target, ExpressionSyntax stringValue)
	{
		Symbol symbol = boundExpression.Expression?.ExpressionSymbol;
		TypeSymbol typeSymbol = symbol.GetTypeSymbol();
		DebugAssertHelper.Assert(typeSymbol != null && (typeSymbol.NavTypeKind == NavTypeKind.Text || typeSymbol.NavTypeKind == NavTypeKind.Code));
		ObjectCreationExpressionSyntax objectCreationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(symbol.GetRuntimeTypeSyntax());
		if (typeSymbol != null && typeSymbol.HasLength)
		{
			if (boundExpression.Expression.Kind == BoundKind.FieldAccess)
			{
				MemberAccessExpressionSyntax expression = MemberAccessExpression(target, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavDefinedLengthMetadata"));
				objectCreationExpressionSyntax = objectCreationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
			}
			else
			{
				objectCreationExpressionSyntax = objectCreationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(typeSymbol.Length)));
			}
		}
		return objectCreationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(stringValue));
	}

	internal static InvocationExpressionSyntax AddExtensionArgument(this InvocationExpressionSyntax invocation, Symbol symbol)
	{
		ApplicationObjectExtensionTypeSymbol parentExtensionTypeSymbol = symbol.GetParentExtensionTypeSymbol();
		if (parentExtensionTypeSymbol != null)
		{
			return invocation.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(parentExtensionTypeSymbol.Id)));
		}
		return invocation;
	}

	internal static InvocationExpressionSyntax AddExtensionAndFieldArguments(this InvocationExpressionSyntax invocation, BoundFieldAccess fieldAccess)
	{
		ApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbol = fieldAccess?.FieldSymbol.GetParentExtensionTypeSymbol();
		if (applicationObjectExtensionTypeSymbol != null)
		{
			invocation = invocation.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(applicationObjectExtensionTypeSymbol.Id)));
		}
		return invocation.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(LiteralExpression(fieldAccess.FieldSymbol.Id)));
	}

	internal static string ToSyntaxTreeAndString(this CompilationUnitSyntax unit)
	{
		if (unit != null)
		{
			return unit.ToSyntaxTree().ToString();
		}
		return string.Empty;
	}

	internal static SyntaxTree ToSyntaxTree(this CompilationUnitSyntax unit)
	{
		if (unit == null)
		{
			return null;
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SyntaxTree(unit);
	}
}
