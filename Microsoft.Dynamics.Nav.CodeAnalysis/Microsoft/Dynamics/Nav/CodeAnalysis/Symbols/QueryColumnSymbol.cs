using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class QueryColumnSymbol : QueryElementSymbol, IQueryColumnSymbol, IQueryElementSymbol, ISymbol, IEquatable<ISymbol>
{
	public override SymbolKind Kind => SymbolKind.QueryColumn;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.QueryColumnProperties;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitQueryColumn(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitQueryColumn(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitQueryColumn(this, a);
	}

	protected TypeSymbol GetColumnType()
	{
		QueryColumnMethodKind? enumPropertyValue = this.GetEnumPropertyValue<QueryColumnMethodKind>(PropertyKind.Method);
		if (enumPropertyValue.HasValue)
		{
			if (enumPropertyValue.Value.IsDateMethod() || enumPropertyValue.Value.IsCountMethod())
			{
				return NavCorLib.IntegerType;
			}
			if (enumPropertyValue.Value.IsAggregationMethod())
			{
				return GetColumnTypeFromRelatedField();
			}
			throw ExceptionUtilities.Unreachable;
		}
		return GetColumnTypeFromRelatedField();
	}

	private TypeSymbol GetColumnTypeFromRelatedField()
	{
		if (RelatedField != null)
		{
			return RelatedField.Type;
		}
		return NavCorLib.None;
	}
}
