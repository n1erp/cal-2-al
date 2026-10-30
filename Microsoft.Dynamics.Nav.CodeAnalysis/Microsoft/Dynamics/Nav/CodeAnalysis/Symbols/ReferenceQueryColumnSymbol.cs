using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceQueryColumnSymbol : QueryColumnSymbol
{
	private readonly Lazy<TypeSymbol> lazyTypeSymbol;

	private readonly Lazy<FieldSymbol> lazyRelatedField;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private DiagnosticInfo useSiteDiagnostics;

	public override int Id { get; }

	public override string Name { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override FieldSymbol RelatedField => lazyRelatedField.Value;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override TypeSymbol Type => lazyTypeSymbol.Value;

	public ReferenceQueryColumnSymbol(Symbol containingSymbol, QueryColumnDefinition column)
	{
		ReferenceQueryColumnSymbol referenceQueryColumnSymbol = this;
		ContainingSymbol = containingSymbol;
		Id = column.Id ?? (-1);
		Name = column.Name();
		lazyRelatedField = ReferenceSymbolHelper.CreateLazy(() => GetSourceFieldSymbol(column, (QueryDataItemSymbol)containingSymbol, ref referenceQueryColumnSymbol.useSiteDiagnostics));
		lazyTypeSymbol = ReferenceSymbolHelper.CreateLazy(() => referenceQueryColumnSymbol.GetColumnType());
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, column);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return Properties.CastArray<Symbol>();
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (!(RelatedField == null))
		{
			return null;
		}
		return useSiteDiagnostics;
	}

	internal static FieldSymbol GetSourceFieldSymbol(QueryElementDefinition element, QueryDataItemSymbol containingQueryDataItem, ref DiagnosticInfo useSiteDiagnostics)
	{
		if (string.IsNullOrEmpty(element.SourceColumn))
		{
			return null;
		}
		TableTypeSymbol relatedTable = containingQueryDataItem.RelatedTable;
		if (relatedTable == null)
		{
			useSiteDiagnostics = new NavDiagnosticInfo(ErrorCode.ERR_MissingPackageDependency);
			return null;
		}
		FieldSymbol fieldSymbol = relatedTable.GetFields().FirstOrDefault((FieldSymbol f) => SemanticFacts.IsSameName(f.Name, element.SourceColumn));
		if (fieldSymbol != null)
		{
			return fieldSymbol;
		}
		ArrayBuilder<FieldSymbol> instance = ArrayBuilder<FieldSymbol>.GetInstance();
		try
		{
			foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in containingQueryDataItem.ContainingModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(relatedTable))
			{
				foreach (ISymbol member in applicationObjectExtensionTypeSymbolsAcrossModule.GetMembers())
				{
					if (member.Kind == SymbolKind.Field && SemanticFacts.IsSameName(member.Name, element.SourceColumn))
					{
						instance.Add((FieldSymbol)member);
					}
				}
			}
			if (instance.Count == 0)
			{
				useSiteDiagnostics = new NavDiagnosticInfo(ErrorCode.ERR_MissingPackageDependency);
				return null;
			}
			if (instance.Count > 1)
			{
				useSiteDiagnostics = new NavDiagnosticInfo(ErrorCode.ERR_AmbigReference, element.SourceColumn);
				return null;
			}
			return instance[0];
		}
		finally
		{
			instance.Free();
		}
	}
}
