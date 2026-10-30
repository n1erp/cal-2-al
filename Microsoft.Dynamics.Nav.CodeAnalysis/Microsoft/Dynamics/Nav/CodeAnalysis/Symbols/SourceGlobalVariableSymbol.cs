using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceGlobalVariableSymbol : SourceVariableSymbol
{
	public override SymbolKind Kind => SymbolKind.GlobalVariable;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public static SourceGlobalVariableSymbol Create(Symbol owner, VariableDeclarationSyntax syntax)
	{
		string valueText = syntax.Name.Identifier.ValueText;
		ImmutableArray<Location> immutableArray = ImmutableArray.Create((Location)new SourceLocation(syntax.Name.Identifier));
		return new SourceGlobalVariableSymbol(owner, syntax, syntax.Name.Identifier, valueText, immutableArray);
	}

	private SourceGlobalVariableSymbol(Symbol containingSymbol, VariableDeclarationSyntax syntax, SyntaxToken identifier, string name, ImmutableArray<Location> locations)
		: base(containingSymbol, syntax, identifier, name, locations)
	{
	}

	protected override void VariableCheck()
	{
		base.VariableCheck();
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			if (Attributes.Any((AttributeSymbol a) =>
			{
				AttributeTypeInfo attributeInfo = a.AttributeInfo;
				return attributeInfo != null && attributeInfo.Kind == AttributeKind.InDataSet;
			}) && !Type.NavTypeKind.IsValidInDataSetType())
			{
				instance.Add(ErrorCode.ERR_InvalidInDatasetDatatype, this.GetLocation(), Name);
			}
			AddDeclarationDiagnostics(instance);
		}
		finally
		{
			instance.Free();
		}
	}
}
