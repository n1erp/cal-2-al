using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class EventSubscriberAttributeEmitter : IAttributeEmitter
{
	private static readonly NameSyntax NavEventSubscriberAttributeNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("NavEventSubscriber");

	private static readonly NameSyntax ObjectTypeRuntimeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("ObjectType");

	public static IAttributeEmitter Instance { get; } = new EventSubscriberAttributeEmitter();

	public AttributeSyntax EmitAttribute(MethodCodeGenerator generator, AttributeTypeInfo attributeInfo, AttributeSymbol attribute)
	{
		AttributeSyntax attributeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(NavEventSubscriberAttributeNameSyntax);
		attributeSyntax = EmitObjectType(attribute, attributeSyntax);
		attributeSyntax = EmitObjectId(generator, attribute, attributeSyntax);
		attributeSyntax = EmitMethodName(generator, attribute, attributeSyntax);
		attributeSyntax = EmitMethodId(generator, attributeSyntax);
		attributeSyntax = EmitFieldName(generator, attribute, attributeSyntax);
		return EmitLicensePermissionFlags(attribute, attributeSyntax);
	}

	private static AttributeSyntax EmitObjectType(AttributeSymbol attribute, AttributeSyntax attributeSyntax)
	{
		BoundOptionAccess optionAccess = (BoundOptionAccess)attribute.Attribute.Arguments[0];
		return attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(EmitObjectTypeEnum(optionAccess)));
	}

	private static AttributeSyntax EmitObjectId(MethodCodeGenerator generator, AttributeSymbol attribute, AttributeSyntax attributeSyntax)
	{
		BoundExpression expr = attribute.Attribute.Arguments[1];
		attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(generator.EmitExpression(expr)));
		return attributeSyntax;
	}

	private static ExpressionSyntax EmitObjectTypeEnum(BoundOptionAccess optionAccess)
	{
		string name = (optionAccess.ExpressionSymbol.MetadataName.Equals("Codeunit", StringComparison.OrdinalIgnoreCase) ? "CodeUnit" : optionAccess.ExpressionSymbol.MetadataName);
		return CodeGenHelper.MemberAccessExpression(ObjectTypeRuntimeType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(name));
	}

	private static AttributeSyntax EmitMethodName(MethodCodeGenerator generator, AttributeSymbol attribute, AttributeSyntax attributeSyntax)
	{
		MethodSymbol methodSymbol = (attribute as SourceAttributeSymbol)?.GetEventMethod();
		attributeSyntax = ((!(methodSymbol != null)) ? attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(generator.EmitExpression(attribute.Attribute.Arguments[2]))) : attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.LiteralExpression(methodSymbol.Name))));
		return attributeSyntax;
	}

	private static AttributeSyntax EmitMethodId(MethodCodeGenerator generator, AttributeSyntax attributeSyntax)
	{
		attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.LiteralExpression(generator.Method.Id)));
		return attributeSyntax;
	}

	private static AttributeSyntax EmitFieldName(MethodCodeGenerator generator, AttributeSymbol attribute, AttributeSyntax attributeSyntax)
	{
		IApplicationObjectTypeSymbol applicationObjectTypeSymbol = (attribute as SourceAttributeSymbol)?.GetEventApplicationObject();
		string methodOrEventName = attribute.Attribute.Arguments[2].ConstantValue.StringValue;
		string fieldOrActionName = GetFieldOrActionName(attribute);
		bool flag = false;
		bool flag2 = false;
		if (applicationObjectTypeSymbol is ISymbolWithTriggerEvents symbolWithTriggerEvents)
		{
			BuiltInTriggerEventSymbol builtInTriggerEventSymbol = symbolWithTriggerEvents.TriggerEvents.SingleOrDefault((BuiltInTriggerEventSymbol x) => SemanticFacts.IsSameName(x.Name, methodOrEventName));
			if (builtInTriggerEventSymbol != null)
			{
				flag = builtInTriggerEventSymbol.Info.ElementType == SymbolKind.Action;
				flag2 = builtInTriggerEventSymbol.Info.ElementType == SymbolKind.Control;
			}
		}
		if (flag)
		{
			ActionSymbol actionSymbol = ((PageTypeSymbol)applicationObjectTypeSymbol).QueryAllActions(fieldOrActionName, attribute.DeclaringCompilation).Single((ActionSymbol x) => x.ActionKind == ActionKind.Action);
			attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.LiteralExpression(actionSymbol.Id)));
		}
		else if (flag2)
		{
			ControlSymbol controlSymbol = ((PageTypeSymbol)applicationObjectTypeSymbol).QueryAllControls(fieldOrActionName, attribute.DeclaringCompilation).Single((ControlSymbol x) => x.ControlKind == ControlKind.Field);
			attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.LiteralExpression(controlSymbol.Id)));
		}
		else
		{
			attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(generator.EmitExpression(attribute.Attribute.Arguments[3])));
		}
		return attributeSyntax;
	}

	private static string GetFieldOrActionName(AttributeSymbol attribute)
	{
		ConstantValue constantValue = attribute.Attribute.Arguments[3].ConstantValue;
		if (!constantValue.IsString)
		{
			return constantValue.CharValue.ToString();
		}
		return constantValue.StringValue;
	}

	private static AttributeSyntax EmitLicensePermissionFlags(AttributeSymbol attribute, AttributeSyntax attributeSyntax)
	{
		bool booleanValue = attribute.Attribute.Arguments[4].ConstantValue.BooleanValue;
		bool booleanValue2 = attribute.Attribute.Arguments[5].ConstantValue.BooleanValue;
		attributeSyntax = attributeSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression(BuildEnumFlagsExpression(booleanValue, booleanValue2))));
		return attributeSyntax;
	}

	private static string BuildEnumFlagsExpression(bool skipMissingLicense, bool skipMissingPermission)
	{
		if (!skipMissingLicense && !skipMissingPermission)
		{
			return "0";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (skipMissingLicense)
		{
			stringBuilder.Append("EventSubscriberCallOptions.SkipOnMissingLicense");
			if (skipMissingPermission)
			{
				stringBuilder.Append(" | ");
			}
		}
		if (skipMissingPermission)
		{
			stringBuilder.Append("EventSubscriberCallOptions.SkipOnMissingPermission");
		}
		return stringBuilder.ToString();
	}
}
