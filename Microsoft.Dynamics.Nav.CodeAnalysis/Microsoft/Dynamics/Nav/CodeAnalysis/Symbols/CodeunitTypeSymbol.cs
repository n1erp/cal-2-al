using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class CodeunitTypeSymbol : ApplicationObjectTypeSymbol, ICodeunitTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	private CodeunitSubtypeKind? subType;

	public override SymbolKind Kind => SymbolKind.Codeunit;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind => NavTypeKind.Codeunit;

	public CodeunitSubtypeKind Subtype
	{
		get
		{
			if (!subType.HasValue)
			{
				subType = this.GetEnumPropertyValueOrDefault<CodeunitSubtypeKind>(PropertyKind.Subtype);
			}
			return subType.Value;
		}
	}

	public override string MetadataName => "Codeunit" + Id.ToString(CultureInfo.InvariantCulture);

	internal override TypeSymbol BaseType => null;

	internal virtual TableTypeSymbol RelatedTable
	{
		get
		{
			PropertySymbol property = GetProperty(PropertyKind.TableNo);
			if (!(property != null) || property.Value == null)
			{
				return null;
			}
			return (TableTypeSymbol)property.Value;
		}
	}

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.CodeunitProperties;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitCodeunitType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitCodeunitType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitCodeunitType(this, a);
	}

	protected CodeunitTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetCodeunitTriggerInfo(name);
	}
}
