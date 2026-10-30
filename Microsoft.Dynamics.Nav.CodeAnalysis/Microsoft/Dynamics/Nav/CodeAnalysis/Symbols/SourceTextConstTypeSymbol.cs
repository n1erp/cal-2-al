using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class SourceTextConstTypeSymbol : TypeSymbol
{
	internal ImmutableArray<BoundMultilanguageExpression> LanguageTexts;

	public override SymbolKind Kind => SymbolKind.NamedType;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public override NavTypeKind NavTypeKind => NavTypeKind.TextConst;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override string Name { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public static SourceTextConstTypeSymbol Create(Symbol owner, TextConstDataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax));
		return new SourceTextConstTypeSymbol(owner, syntax, locations, diagnostics);
	}

	private SourceTextConstTypeSymbol(Symbol containingSymbol, TextConstDataTypeSyntax syntax, ImmutableArray<Location> locations, DiagnosticBag diagnostics)
	{
		ContainingSymbol = containingSymbol;
		DeclaringSyntaxReferences = ImmutableArray.Create(syntax.GetReference());
		Locations = locations;
		Name = NavTypeKind.TextConst.ToString();
		LanguageTexts = Binder.BindMultiLanguageValue(syntax.Multilanguage, DeclaringCompilation.Options.CompilerFeatures, diagnostics);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}
