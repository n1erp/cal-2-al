using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SourceFieldGroupSymbol : FieldGroupSymbol
{
	private readonly FieldNameListMembers members;

	public FieldGroupSyntax Syntax { get; }

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override ImmutableArray<PropertySymbol> Properties => members.Properties;

	public override ImmutableArray<FieldSymbol> Fields => members.Fields;

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	internal override bool RequiresCompletion => true;

	public static SourceFieldGroupSymbol Create(Symbol owner, FieldGroupSyntax fieldNameList)
	{
		return new SourceFieldGroupSymbol(owner, fieldNameList);
	}

	private SourceFieldGroupSymbol(Symbol containingSymbol, FieldGroupSyntax fieldNameList)
	{
		members = new FieldNameListMembers(this, fieldNameList);
		ContainingSymbol = containingSymbol;
		Name = fieldNameList.Name.Unquoted();
		Syntax = fieldNameList;
		Locations = ImmutableArray.Create((Location)new SourceLocation(fieldNameList));
		DeclaringSyntaxReferences = ImmutableArray.Create(fieldNameList.GetReference());
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return members.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		members.ForceComplete(locationOpt, cancellationToken);
	}
}
