using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class HandlerFunctionsAttributeValidator : IAttributeValidator
{
	public static IAttributeValidator Instance { get; } = new HandlerFunctionsAttributeValidator();

	public void Validate(AttributeSymbol symbol, DiagnosticBag diagnostics)
	{
		if (!symbol.HasErrors())
		{
			VerifyHandlerFunctionsReferences(symbol, diagnostics);
		}
	}

	private static void VerifyHandlerFunctionsReferences(AttributeSymbol symbol, DiagnosticBag diagnostics)
	{
		if (symbol.Arguments.Length != 1)
		{
			return;
		}
		AttributeArgumentSymbol attributeArgumentSymbol = symbol.Arguments[0];
		if (attributeArgumentSymbol.ValueText.Contains(", ") || attributeArgumentSymbol.ValueText.Contains(" ,"))
		{
			diagnostics.Add(ErrorCode.ERR_HandlerFunctionsListShouldNotHaveSpaces, attributeArgumentSymbol.GetLocation());
			return;
		}
		string[] array = attributeArgumentSymbol.ValueText.Split(',');
		IEnumerable<MethodSymbol> membersOfKind = symbol.GetContainingSymbolOfKind<CodeunitTypeSymbol>(SymbolKind.Codeunit).GetMembersOfKind<MethodSymbol>(SymbolKind.Method);
		string[] array2 = array;
		foreach (string handlerMethod in array2)
		{
			if (!membersOfKind.Any((MethodSymbol procedure) => IsMatchingHandlerFunction(handlerMethod, procedure)))
			{
				diagnostics.Add(ErrorCode.ERR_MissingHandlerFunction, attributeArgumentSymbol.GetLocation(), handlerMethod);
			}
		}
	}

	private static bool IsMatchingHandlerFunction(string handlerMethodName, MethodSymbol candidateMethod)
	{
		if (SemanticFacts.IsSameName(handlerMethodName, candidateMethod.Name))
		{
			return candidateMethod.Attributes.Any((AttributeSymbol x) => x.AttributeInfo != null && x.AttributeInfo.ContainsTag(AttributeTagKind.Handler));
		}
		return false;
	}
}
