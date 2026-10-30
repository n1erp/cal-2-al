using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class EventAttributeValidator : IAttributeValidator
{
	public static IAttributeValidator Instance { get; } = new EventAttributeValidator();

	public void Validate(AttributeSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.HasErrors())
		{
			SourceMethodOrTriggerSymbol containingSymbolOfType = symbol.GetContainingSymbolOfType<SourceMethodOrTriggerSymbol>();
			CheckEventPublisherAttribute(symbol, containingSymbolOfType, diagnostics);
		}
	}

	private void CheckEventPublisherAttribute(AttributeSymbol attribute, SourceMethodOrTriggerSymbol methodSymbol, DiagnosticBag diagnostics)
	{
		AttributeValidatorHelper.CheckEventReturn(methodSymbol, diagnostics);
		ImmutableArray<AttributeArgumentSymbol> arguments = attribute.Arguments;
		if (arguments[0].ValueAsBoolean.HasValue)
		{
			bool value = arguments[0].ValueAsBoolean.Value;
			ParameterSymbol parameterSymbol = methodSymbol.Parameters.FirstOrDefault((ParameterSymbol p) => SemanticFacts.IsSameName(p.Name, "sender"));
			if (value && parameterSymbol != null)
			{
				diagnostics.Add(ErrorCode.ERR_EventPublisherIncludeSenderWithParameterSender, parameterSymbol.Locations.FirstOrDefault(), methodSymbol.Name);
			}
			if (methodSymbol.GetSyntax().Body.Statements.Count > 0)
			{
				diagnostics.Add(ErrorCode.ERR_EventPublisherCannotContainCode, methodSymbol.GetSyntax().Body.Statements.First().Location, methodSymbol.Name);
			}
			if (methodSymbol.GetSyntax().Variables != null && methodSymbol.GetSyntax().Variables.Variables.Count > 0)
			{
				diagnostics.Add(ErrorCode.ERR_EventPublisherCannotContainVariables, methodSymbol.GetSyntax().Variables.Variables.First().Location, methodSymbol.Name);
			}
		}
	}
}
