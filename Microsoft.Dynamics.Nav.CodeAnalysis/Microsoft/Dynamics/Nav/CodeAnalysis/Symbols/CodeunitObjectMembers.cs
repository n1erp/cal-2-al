using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class CodeunitObjectMembers : ApplicationObjectMembers<CodeunitTypeSymbol>
{
	internal new CodeunitSyntax Declaration => (CodeunitSyntax)base.Declaration;

	internal CodeunitObjectMembers(CodeunitTypeSymbol containingSymbol, CodeunitSyntax declaration)
		: base(containingSymbol, (ApplicationObjectSyntax)declaration)
	{
	}

	protected override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetCodeunitTriggerInfo(name);
	}

	protected override void BuildDeclaredMembersCore(ArrayBuilder<Symbol> members, DiagnosticBag diagnostics)
	{
		AddBuiltinMethods(members, ContainingSymbol);
	}

	internal static void AddBuiltinMethods(ArrayBuilder<Symbol> members, CodeunitTypeSymbol codeunit)
	{
		TableTypeSymbol relatedTable = codeunit.RelatedTable;
		if (relatedTable != null)
		{
			members.Add(new BuiltInMethodTypeSymbol(codeunit, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Run", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
			{
				new BuiltInParameterSymbol(method, codeunit.ContainingModule.GetRecord(relatedTable, isTemporary: false), "Record", 0, isVar: true)
			}));
		}
		else
		{
			members.Add(new BuiltInMethodTypeSymbol(codeunit, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Run", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true));
		}
	}

	protected override bool IsNameRestrictedForTriggers(string memberName)
	{
		return TriggerDefinitions.IsValidCodeunitTrigger(memberName);
	}

	protected override void CheckMembers(DiagnosticBag diagnostics)
	{
		base.CheckMembers(diagnostics);
		CodeunitSubtypeKind subtype = ContainingSymbol.Subtype;
		ValidateHandlerFunctions(subtype, diagnostics);
	}

	private void ValidateHandlerFunctions(CodeunitSubtypeKind codeunitType, DiagnosticBag diagnostics)
	{
		if (codeunitType == CodeunitSubtypeKind.Test)
		{
			ObjectMembers<CodeunitTypeSymbol>.CheckMembersForDuplicateNames(GetMembers().WhereAsArray((Symbol x) => x.Kind == SymbolKind.Method && ((MethodSymbol)x).HasAttribute(AttributeTagKind.Handler)), diagnostics, (Symbol _) => true, ErrorCode.ERR_DuplicateHandlerMethodName);
		}
	}
}
