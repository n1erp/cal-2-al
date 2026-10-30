using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class SourceLabelTypeSymbol : LabelTypeSymbol
{
	public override string Name { get; }

	internal BoundLabel BoundLabel { get; }

	internal LabelDataTypeSyntax Syntax { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public override bool HasLength => BoundLabel.MaxLength != null;

	public override int Length => BoundLabel.MaxLength?.Int32Value ?? int.MaxValue;

	public static SourceLabelTypeSymbol Create(Symbol owner, LabelDataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(syntax));
		return new SourceLabelTypeSymbol(owner, syntax, locations, diagnostics);
	}

	private SourceLabelTypeSymbol(Symbol containingSymbol, LabelDataTypeSyntax syntax, ImmutableArray<Location> locations, DiagnosticBag diagnostics)
	{
		ContainingSymbol = containingSymbol;
		DeclaringSyntaxReferences = ImmutableArray.Create(syntax.GetReference());
		Syntax = syntax;
		Locations = locations;
		Name = syntax.Parent.Parent.GetNameStringValue();
		BoundLabel = Binder.BindLabel(syntax.Label, diagnostics);
	}

	internal override string GetLabelText()
	{
		return (((LabelSyntax)BoundLabel.Syntax)?.LabelText)?.Value.ValueText.UnquoteLiteral();
	}
}
