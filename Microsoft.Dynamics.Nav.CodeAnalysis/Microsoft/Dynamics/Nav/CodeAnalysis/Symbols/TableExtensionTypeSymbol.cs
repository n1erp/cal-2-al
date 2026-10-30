using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class TableExtensionTypeSymbol : ApplicationObjectExtensionTypeSymbol, ITableExtensionTypeSymbol, IApplicationObjectExtensionTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	public override SymbolKind Kind => SymbolKind.TableExtension;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind => NavTypeKind.TableExtension;

	public override string MetadataName => "TableExtension" + Id.ToString(CultureInfo.InvariantCulture);

	internal override TypeSymbol BaseType => TableClassTypeSymbol.Instance;

	public override bool IsObsolete => Target?.IsObsolete ?? false;

	public override string ObsoleteReason => Target?.ObsoleteReason ?? string.Empty;

	public override bool IsDeprecated => Target?.IsDeprecated ?? false;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.TableProperties;

	ImmutableArray<IFieldSymbol> ITableExtensionTypeSymbol.AddedFields => StaticCast<IFieldSymbol>.From(GetAddedFields());

	ImmutableArray<IKeySymbol> ITableExtensionTypeSymbol.AddedKeys => StaticCast<IKeySymbol>.From(GetAddedKeys());

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTableExtensionType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTableExtensionType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTableExtensionType(this, a);
	}

	protected TableExtensionTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		name = name.ToUpperInvariant();
		return TriggerDefinitions.GetTableExtensionTriggerInfo(name);
	}

	internal abstract ImmutableArray<FieldSymbol> GetAddedFields();

	internal abstract ImmutableArray<KeySymbol> GetAddedKeys();
}
