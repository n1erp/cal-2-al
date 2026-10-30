using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal class ReferenceModuleSymbol : ModuleSymbol
{
	private readonly ImmutableDictionary<SymbolKind, ImmutableDictionary<int, ISymbolWithId>> objectsById;

	private readonly ImmutableDictionary<SymbolKind, ImmutableDictionary<string, ISymbol>> objectsByName;

	private readonly ImmutableArray<ISymbol> tableTypeSymbols;

	private readonly ImmutableArray<ISymbol> codeunitTypeSymbols;

	private readonly ImmutableArray<ISymbol> pageTypeSymbols;

	private readonly ImmutableArray<ISymbol> pageExtensionTypeSymbols;

	private readonly ImmutableArray<ISymbol> pageCustomizationTypeSymbols;

	private readonly ImmutableArray<ISymbol> tableExtensionTypeSymbols;

	private readonly ImmutableArray<ISymbol> reportTypeSymbols;

	private readonly ImmutableArray<ISymbol> xmlportTypeSymbols;

	private readonly ImmutableArray<ISymbol> queryTypeSymbols;

	private readonly ImmutableArray<ISymbol> controlAddInTypeSymbols;

	private readonly ImmutableArray<ISymbol> enumTypeSymbols;

	private readonly ImmutableArray<ISymbol> enumExtensionTypeSymbols;

	private readonly ImmutableArray<ISymbol> dotNetPackageSymbols;

	private readonly ImmutableArray<Symbol> members;

	private readonly ImmutableArray<ISymbol> dotNetTypeDeclarations;

	internal override IReferenceResolver ReferenceResolver { get; }

	internal override IDotNetResolver DotNetResolver { get; }

	public override string Name { get; }

	public override string PublisherName { get; }

	public override Version Version { get; }

	public override Guid AppId { get; }

	internal override Compilation DeclaringCompilation => null;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override SymbolKind Kind => SymbolKind.Module;

	internal ReferenceModuleSymbol(IReferenceResolver resolver, IDotNetResolver dotNetResolver, ModuleDefinition module, ImmutableArray<IModuleSymbol> referenceModules)
		: base(referenceModules)
	{
		ReferenceResolver = resolver;
		DotNetResolver = dotNetResolver;
		Name = module.Name;
		AppId = module.AppId;
		PublisherName = module.Publisher ?? string.Empty;
		Version = Version.Parse(module.Version ?? "1.0.0.0");
		PooledDictionary<SymbolKind, ImmutableDictionary<string, ISymbol>> instance = PooledDictionary<SymbolKind, ImmutableDictionary<string, ISymbol>>.GetInstance();
		PooledDictionary<SymbolKind, ImmutableDictionary<int, ISymbolWithId>> instance2 = PooledDictionary<SymbolKind, ImmutableDictionary<int, ISymbolWithId>>.GetInstance();
		try
		{
			tableTypeSymbols = ConstructObjectTypeSymbol(module.Tables, SymbolKind.Table, instance, instance2);
			codeunitTypeSymbols = ConstructObjectTypeSymbol(module.Codeunits, SymbolKind.Codeunit, instance, instance2);
			pageTypeSymbols = ConstructObjectTypeSymbol(module.Pages, SymbolKind.Page, instance, instance2);
			pageExtensionTypeSymbols = ConstructObjectTypeSymbol(module.PageExtensions, SymbolKind.PageExtension, instance, instance2);
			pageCustomizationTypeSymbols = ConstructObjectTypeSymbol(module.PageCustomizations, SymbolKind.PageCustomization, instance, instance2);
			tableExtensionTypeSymbols = ConstructObjectTypeSymbol(module.TableExtensions, SymbolKind.TableExtension, instance, instance2);
			reportTypeSymbols = ConstructObjectTypeSymbol(module.Reports, SymbolKind.Report, instance, instance2);
			xmlportTypeSymbols = ConstructObjectTypeSymbol(module.XmlPorts, SymbolKind.XmlPort, instance, instance2);
			queryTypeSymbols = ConstructObjectTypeSymbol(module.Queries, SymbolKind.Query, instance, instance2);
			controlAddInTypeSymbols = ConstructObjectTypeSymbol(module.ControlAddIns, SymbolKind.ControlAddIn, instance, instance2);
			enumTypeSymbols = ConstructObjectTypeSymbol(module.EnumTypes, SymbolKind.Enum, instance, instance2);
			enumExtensionTypeSymbols = ConstructObjectTypeSymbol(module.EnumExtensionTypes, SymbolKind.EnumExtension, instance, instance2);
			dotNetPackageSymbols = ConstructObjectTypeSymbol(module.DotNetPackages, SymbolKind.DotNetPackage);
			dotNetTypeDeclarations = ConstructDotNetTypeDeclarations(instance);
			objectsByName = instance.ToImmutableDictionary();
			objectsById = instance2.ToImmutableDictionary();
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
		members = CombineMembers();
	}

	private ImmutableArray<Symbol> CombineMembers()
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(objectsById.Count);
		try
		{
			for (int i = 0; i < tableTypeSymbols.Length; i++)
			{
				instance.Add((Symbol)tableTypeSymbols[i]);
			}
			for (int j = 0; j < tableExtensionTypeSymbols.Length; j++)
			{
				instance.Add((Symbol)tableExtensionTypeSymbols[j]);
			}
			for (int k = 0; k < codeunitTypeSymbols.Length; k++)
			{
				instance.Add((Symbol)codeunitTypeSymbols[k]);
			}
			for (int l = 0; l < pageTypeSymbols.Length; l++)
			{
				instance.Add((Symbol)pageTypeSymbols[l]);
			}
			for (int m = 0; m < pageExtensionTypeSymbols.Length; m++)
			{
				instance.Add((Symbol)pageExtensionTypeSymbols[m]);
			}
			for (int n = 0; n < pageCustomizationTypeSymbols.Length; n++)
			{
				instance.Add((Symbol)pageCustomizationTypeSymbols[n]);
			}
			for (int num = 0; num < reportTypeSymbols.Length; num++)
			{
				instance.Add((Symbol)reportTypeSymbols[num]);
			}
			for (int num2 = 0; num2 < xmlportTypeSymbols.Length; num2++)
			{
				instance.Add((Symbol)xmlportTypeSymbols[num2]);
			}
			for (int num3 = 0; num3 < queryTypeSymbols.Length; num3++)
			{
				instance.Add((Symbol)queryTypeSymbols[num3]);
			}
			for (int num4 = 0; num4 < controlAddInTypeSymbols.Length; num4++)
			{
				instance.Add((Symbol)controlAddInTypeSymbols[num4]);
			}
			for (int num5 = 0; num5 < dotNetPackageSymbols.Length; num5++)
			{
				instance.Add((Symbol)dotNetPackageSymbols[num5]);
			}
			for (int num6 = 0; num6 < enumTypeSymbols.Length; num6++)
			{
				instance.Add((Symbol)enumTypeSymbols[num6]);
			}
			for (int num7 = 0; num7 < enumExtensionTypeSymbols.Length; num7++)
			{
				instance.Add((Symbol)enumExtensionTypeSymbols[num7]);
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	private ImmutableArray<ISymbol> ConstructDotNetTypeDeclarations(PooledDictionary<SymbolKind, ImmutableDictionary<string, ISymbol>> objectsByNameParam)
	{
		if (dotNetPackageSymbols.IsDefaultOrEmpty)
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		ArrayBuilder<ISymbol> instance = ArrayBuilder<ISymbol>.GetInstance();
		try
		{
			for (int i = 0; i < dotNetPackageSymbols.Length; i++)
			{
				instance.AddRange(((DotNetPackageTypeSymbol)dotNetPackageSymbols[i]).FlattenedTypeDeclarations);
			}
			objectsByNameParam.Add(SymbolKind.DotNetTypeDeclaration, instance.ToImmutableDictionary((ISymbol o) => o.Name, SemanticFacts.NameEqualityComparer));
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}

	public override ImmutableArray<ISymbol> GetDeclaredObjectSymbols()
	{
		return ImmutableArray<ISymbol>.Empty;
	}

	public override ImmutableArray<ISymbol> GetObjectSymbols(SymbolKind kind)
	{
		return kind switch
		{
			SymbolKind.Codeunit => codeunitTypeSymbols, 
			SymbolKind.Table => tableTypeSymbols, 
			SymbolKind.TableExtension => tableExtensionTypeSymbols, 
			SymbolKind.Page => pageTypeSymbols, 
			SymbolKind.PageExtension => pageExtensionTypeSymbols, 
			SymbolKind.PageCustomization => pageCustomizationTypeSymbols, 
			SymbolKind.Report => reportTypeSymbols, 
			SymbolKind.XmlPort => xmlportTypeSymbols, 
			SymbolKind.Query => queryTypeSymbols, 
			SymbolKind.ControlAddIn => controlAddInTypeSymbols, 
			SymbolKind.DotNetPackage => dotNetPackageSymbols, 
			SymbolKind.DotNetTypeDeclaration => dotNetTypeDeclarations, 
			SymbolKind.Enum => enumTypeSymbols, 
			SymbolKind.EnumExtension => enumExtensionTypeSymbols, 
			_ => ImmutableArray<ISymbol>.Empty, 
		};
	}

	public override ISymbolWithId GetObjectSymbolById(SymbolKind kind, int id)
	{
		if (!objectsById.TryGetValue(kind, out var value))
		{
			return null;
		}
		if (!value.TryGetValue(id, out var value2))
		{
			return null;
		}
		return value2;
	}

	public override ISymbol GetObjectSymbolByName(SymbolKind kind, string name)
	{
		return ModuleSymbol.GetObjectSymbolByName(objectsByName, kind, name);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return members;
	}

	private ImmutableArray<ISymbol> ConstructObjectTypeSymbol<TDefinitionType>(IEnumerable<TDefinitionType> definitions, SymbolKind kind)
	{
		if (definitions == null)
		{
			return ImmutableArray<ISymbol>.Empty;
		}
		Func<TDefinitionType, ISymbol> referenceTypeSymbolFactory = GetReferenceTypeSymbolFactory<TDefinitionType>(kind);
		return definitions.Select((TDefinitionType t) => referenceTypeSymbolFactory(t)).ToImmutableArray();
	}

	private ImmutableArray<ISymbol> ConstructObjectTypeSymbol<TDefinitionType>(IEnumerable<TDefinitionType> definitions, SymbolKind kind, IDictionary<SymbolKind, ImmutableDictionary<string, ISymbol>> objectsByNameParam, IDictionary<SymbolKind, ImmutableDictionary<int, ISymbolWithId>> objectsByIdParam)
	{
		ImmutableArray<ISymbol> immutableArray = ConstructObjectTypeSymbol(definitions, kind);
		if (!immutableArray.IsDefaultOrEmpty)
		{
			objectsByNameParam.Add(kind, immutableArray.ToImmutableDictionary((ISymbol o) => o.Name, SemanticFacts.NameEqualityComparer));
			objectsByIdParam.Add(kind, immutableArray.Where((ISymbol x) => IsSymbolWithId(x)).Cast<ISymbolWithId>().ToImmutableDictionary((ISymbolWithId o) => o.Id));
		}
		return immutableArray;
	}

	private static bool IsSymbolWithId(ISymbol symbol)
	{
		if (symbol is ISymbolWithId)
		{
			return symbol.Kind != SymbolKind.PageCustomization;
		}
		return false;
	}

	private Func<TDefinitionType, ISymbol> GetReferenceTypeSymbolFactory<TDefinitionType>(SymbolKind kind)
	{
		return kind switch
		{
			SymbolKind.Codeunit => (Func<TDefinitionType, ISymbol>)((TDefinitionType t) => new ReferenceCodeunitTypeSymbol(this, t as CodeunitDefinition)), 
			SymbolKind.Table => (TDefinitionType t) => new ReferenceTableTypeSymbol(this, t as TableDefinition), 
			SymbolKind.TableExtension => (TDefinitionType t) => new ReferenceTableExtensionTypeSymbol(this, t as TableExtensionDefinition), 
			SymbolKind.Page => (TDefinitionType t) => new ReferencePageTypeSymbol(this, t as PageDefinition), 
			SymbolKind.PageExtension => (TDefinitionType t) => new ReferencePageExtensionTypeSymbol(this, t as PageExtensionDefinition), 
			SymbolKind.PageCustomization => (TDefinitionType t) => new ReferencePageCustomizationTypeSymbol(this, t as PageCustomizationDefinition), 
			SymbolKind.XmlPort => (TDefinitionType t) => new ReferenceXmlPortTypeSymbol(this, t as XmlPortDefinition), 
			SymbolKind.Report => (TDefinitionType t) => new ReferenceReportTypeSymbol(this, t as ReportDefinition), 
			SymbolKind.Query => (TDefinitionType t) => new ReferenceQueryTypeSymbol(this, t as QueryDefinition), 
			SymbolKind.ControlAddIn => (TDefinitionType t) => new ReferenceControlAddInTypeSymbol(this, t as ControlAddInDefinition), 
			SymbolKind.Enum => (TDefinitionType t) => ReferenceEnumTypeSymbol.Create(this, t as EnumTypeDefinition), 
			SymbolKind.EnumExtension => (TDefinitionType t) => ReferenceEnumExtensionTypeSymbol.Create(this, t as EnumExtensionTypeDefinition), 
			SymbolKind.DotNetPackage => (TDefinitionType t) => new ReferenceDotNetPackageTypeSymbol(this, t as DotNetPackageDefinition), 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}
}
