using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class TableTypeSymbol : ApplicationObjectTypeSymbol, ITableTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol, ISymbolWithTriggerEvents
{
	private ImmutableArray<BuiltInTriggerEventSymbol> triggerEvents;

	private ObsoleteStateKind? obsoleteState;

	private RecordTypeSymbol temporaryRecordSymbol;

	private RecordTypeSymbol recordSymbol;

	public override SymbolKind Kind => SymbolKind.Table;

	public abstract TableTypeKind TableType { get; }

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind => NavTypeKind.Record;

	public override string MetadataName => "Record" + Id.ToString(CultureInfo.InvariantCulture);

	internal override TypeSymbol BaseType => TableClassTypeSymbol.Instance;

	public ImmutableArray<BuiltInTriggerEventSymbol> TriggerEvents
	{
		get
		{
			LazyGetTriggerEvents();
			return triggerEvents;
		}
	}

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.TableProperties;

	public override bool IsObsolete => GetIsObsoleteState() == ObsoleteStateKind.Removed;

	public override bool IsDeprecated => GetIsObsoleteState() == ObsoleteStateKind.Pending;

	public override string ObsoleteReason => GetProperty(PropertyKind.ObsoleteReason)?.Value as string;

	ImmutableArray<IFieldSymbol> ITableTypeSymbol.Fields => StaticCast<IFieldSymbol>.From(GetFields());

	ImmutableArray<IKeySymbol> ITableTypeSymbol.Keys => StaticCast<IKeySymbol>.From(GetKeys());

	ImmutableArray<IFieldGroupSymbol> ITableTypeSymbol.FieldGroups => StaticCast<IFieldGroupSymbol>.From(GetFieldGroups());

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTableType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTableType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTableType(this, a);
	}

	protected TableTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	internal abstract ImmutableArray<FieldSymbol> GetFields();

	internal KeySymbol GetPrimaryKey()
	{
		KeySymbol keySymbol = GetKeys().FirstOrDefault();
		if (keySymbol != null)
		{
			return keySymbol;
		}
		FieldSymbol fieldSymbol = (from f in GetFields()
			where f.Type.IsValidKeyType() && f.HasValidPropertiesForKey()
			orderby f.Id
			select f).FirstOrDefault();
		if (fieldSymbol == null)
		{
			return null;
		}
		return new SynthesizedKeySymbol(this, fieldSymbol);
	}

	internal abstract ImmutableArray<KeySymbol> GetKeys();

	internal abstract ImmutableArray<FieldGroupSymbol> GetFieldGroups();

	internal RecordTypeSymbol GetRecord(bool isTemporary)
	{
		if (isTemporary)
		{
			if (temporaryRecordSymbol == null)
			{
				Interlocked.CompareExchange(ref temporaryRecordSymbol, new RecordTypeSymbol(this, isTemporary), null);
			}
			return temporaryRecordSymbol;
		}
		if (recordSymbol == null)
		{
			Interlocked.CompareExchange(ref recordSymbol, new RecordTypeSymbol(this, isTemporary), null);
		}
		return recordSymbol;
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		name = name.ToUpperInvariant();
		return TriggerDefinitions.GetTableTriggerInfo(name);
	}

	private void LazyGetTriggerEvents()
	{
		if (!(triggerEvents != null))
		{
			ImmutableArray<BuiltInTriggerEventSymbol> value = TriggerEventDefinitions.TableTriggerEvents.Values.Select((TriggerEventTypeInfo x) => new BuiltInTriggerEventSymbol(this, x)).ToImmutableArray();
			ImmutableInterlocked.InterlockedCompareExchange(ref triggerEvents, value, default);
		}
	}

	private ObsoleteStateKind GetIsObsoleteState()
	{
		if (!obsoleteState.HasValue)
		{
			obsoleteState = this.GetEnumPropertyValueOrDefault<ObsoleteStateKind>(PropertyKind.ObsoleteState);
		}
		return obsoleteState.Value;
	}

	internal override bool Equals(TypeSymbol t2)
	{
		if (t2?.NavTypeKind != NavTypeKind)
		{
			return false;
		}
		TableTypeSymbol tableTypeSymbol = t2 as TableTypeSymbol;
		if (tableTypeSymbol == null)
		{
			return false;
		}
		return tableTypeSymbol.Id == Id;
	}
}
