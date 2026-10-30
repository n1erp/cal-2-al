using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceChangeModifyFieldSymbol : SourceChangeModifySymbol
{
	public override SymbolKind ChangeTargetKind => SymbolKind.Field;

	private new FieldModificationSyntax Syntax => (FieldModificationSyntax)base.Syntax;

	protected override SyntaxNode TargetSyntax => Syntax.Name;

	private SourceChangeModifyFieldSymbol(Symbol owner, FieldModificationSyntax syntax)
		: base(owner, syntax, syntax.Name, syntax.PropertyList, syntax.Triggers)
	{
	}

	public static SourceChangeModifyFieldSymbol Create(Symbol owner, FieldModificationSyntax syntax)
	{
		return new SourceChangeModifyFieldSymbol(owner, syntax);
	}

	protected override ISymbol LazyGetTarget(DiagnosticBag diagnostics)
	{
		TableTypeSymbol tableTypeSymbol = ((ApplicationObjectExtensionTypeSymbol)ContainingSymbol).Target as TableTypeSymbol;
		if (tableTypeSymbol == null)
		{
			return null;
		}
		IdentifierNameSyntax name = Syntax.Name;
		List<Symbol> foundSymbols = this.GetTableFields(tableTypeSymbol, Syntax.Name).ToList();
		return ChangeSymbolExtensions.GetTargetSymbolFromList(this, foundSymbols, name, ErrorCode.ERR_FieldNotFound, ErrorCode.ERR_FieldNotFound, diagnostics);
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetFieldExtensionTriggerInfo(name);
	}
}
