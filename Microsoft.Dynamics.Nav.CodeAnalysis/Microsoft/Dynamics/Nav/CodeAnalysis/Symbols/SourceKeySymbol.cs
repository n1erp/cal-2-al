using System.Collections.Immutable;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SourceKeySymbol : KeySymbol
{
	private readonly FieldNameListMembers members;

	public KeySyntax Syntax { get; }

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override ImmutableArray<PropertySymbol> Properties => members.Properties;

	public override ImmutableArray<FieldSymbol> Fields => members.Fields;

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	internal override bool RequiresCompletion => true;

	public static SourceKeySymbol Create(Symbol owner, KeySyntax fieldNameList)
	{
		return new SourceKeySymbol(owner, fieldNameList);
	}

	private SourceKeySymbol(Symbol containingSymbol, KeySyntax fieldNameList)
	{
		ContainingSymbol = containingSymbol;
		Name = fieldNameList.Name.Unquoted();
		Syntax = fieldNameList;
		Locations = ImmutableArray.Create((Location)new SourceLocation(fieldNameList));
		DeclaringSyntaxReferences = ImmutableArray.Create(fieldNameList.GetReference());
		members = new FieldNameListMembers(this, fieldNameList, ValidateFieldMember, ValidateKey);
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return members.HasComplete(part);
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		members.ForceComplete(locationOpt, cancellationToken);
	}

	private static void ValidateFieldMember(FieldSymbol field, IdentifierNameSyntax fieldName, ApplicationObjectTypeSymbol applicationObject, DiagnosticBag diagnostics)
	{
		if (!field.HasValidFieldClassForKey())
		{
			diagnostics.Add(ErrorCode.ERR_FlowFieldCannotBePartOfKeys, fieldName.Location, fieldName, applicationObject.Name);
		}
		if (!field.HasValidSqlTimestampPropertyForKey())
		{
			diagnostics.Add(ErrorCode.ERR_SqlTimestampFieldCannotBePartOfKeys, fieldName.Location, fieldName, applicationObject.Name);
		}
	}

	private void ValidateKey(KeyFieldGroupBaseSyntax syntax, DiagnosticBag diagnostics)
	{
		if (Name.Length > 28)
		{
			diagnostics.Add(ErrorCode.WRN_ERR_KeyNameIdentifierTooLong, syntax.Name.Location, 28);
		}
	}
}
