using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class ReferenceFieldGroupSymbol : FieldGroupSymbol
{
	private readonly Lazy<ImmutableArray<FieldSymbol>> lazyFields;

	public override string Name { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<FieldSymbol> Fields => lazyFields.Value;

	public ReferenceFieldGroupSymbol(ContainerSymbol containingSymbol, FieldGroupDefinition fieldGroup)
	{
		ReferenceFieldGroupSymbol referenceFieldGroupSymbol = this;
		DebugAssertHelper.Assert(fieldGroup != null);
		ContainingSymbol = containingSymbol;
		Name = fieldGroup.Name;
		lazyFields = ReferenceSymbolHelper.CreateLazy(() => referenceFieldGroupSymbol.GetLazyFields(fieldGroup));
	}

	private ImmutableArray<FieldSymbol> GetLazyFields(FieldGroupDefinition fieldGroupDefinition)
	{
		ImmutableArray<FieldSymbol> fields = ((TableTypeSymbol)ContainingSymbol).GetFields();
		ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance(fieldGroupDefinition.FieldNames.Length);
		string[] fieldNames = fieldGroupDefinition.FieldNames;
		foreach (string fieldName in fieldNames)
		{
			FieldSymbol fieldSymbol = fields.FirstOrDefault((FieldSymbol f) => SemanticFacts.IsSameName(f.Name, fieldName));
			if (fieldSymbol != null)
			{
				instance.Add(fieldSymbol);
			}
		}
		return instance.ToImmutableAndFree();
	}
}
