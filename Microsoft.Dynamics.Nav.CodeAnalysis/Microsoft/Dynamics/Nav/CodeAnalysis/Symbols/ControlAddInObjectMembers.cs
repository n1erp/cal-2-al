using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ControlAddInObjectMembers : ObjectMembers<SourceControlAddInTypeSymbol>
{
	internal static readonly char[] InvalidControlAddInNameCharacters = new char[10] { '(', ')', '#', '/', '\\', '?', ':', '<', '>', '|' };

	internal static string InvalidControlAddInNameCharacters_Joined = string.Join(", ", InvalidControlAddInNameCharacters.Select((char ch) => "'" + ch + "'"));

	protected override bool MethodsMustHaveBody => false;

	internal ControlAddInObjectMembers(SourceControlAddInTypeSymbol containingSymbol, ControlAddInSyntax declaration)
		: base(containingSymbol, (ObjectSyntax)declaration)
	{
	}

	protected override void CheckObject(DiagnosticBag diagnostics)
	{
		base.CheckObject(diagnostics);
		if (Name.Any((char c) => InvalidControlAddInNameCharacters.Any((char c2) => c == c2)))
		{
			diagnostics.Add(ErrorCode.ERR_InvalidControlAddInNameCharacter, ContainingSymbol.GetLocation(), InvalidControlAddInNameCharacters_Joined);
		}
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		foreach (Symbol member in GetMembers())
		{
			if (member.Kind == SymbolKind.Method)
			{
				MethodSymbol methodSymbol = (MethodSymbol)member;
				if (methodSymbol.MethodKind == MethodKind.DeclareMethod)
				{
					if (methodSymbol.IsLocal)
					{
						diagnostics.Add(ErrorCode.ERR_AccessibilityModifierNotAllowed, member.GetLocation(), member.Name);
					}
					if (methodSymbol.ReturnValue != null && methodSymbol.ReturnValue.ReturnType.NavTypeKind != NavTypeKind.None)
					{
						diagnostics.Add(ErrorCode.ERR_ReturnValueNotAllowed, methodSymbol.Locations.FirstOrDefault(), methodSymbol.Name);
					}
				}
				CheckParameters(methodSymbol.Parameters, diagnostics);
			}
			else if (member.Kind == SymbolKind.Event)
			{
				CheckParameters(((EventSymbol)member).Parameters, diagnostics);
			}
		}
	}

	private static void CheckParameters(IEnumerable<ParameterSymbol> parameters, DiagnosticBag diagnostics)
	{
		foreach (ParameterSymbol parameter in parameters)
		{
			if (parameter.IsVar)
			{
				diagnostics.Add(ErrorCode.ERR_VarParameterNotAllowed, parameter.Locations.FirstOrDefault(), parameter.Name);
			}
			if (!parameter.Type.IsSerializable())
			{
				diagnostics.Add(ErrorCode.ERR_ParameterTypeNotSerializable, parameter.Locations.FirstOrDefault(), parameter.Name);
			}
		}
	}
}
