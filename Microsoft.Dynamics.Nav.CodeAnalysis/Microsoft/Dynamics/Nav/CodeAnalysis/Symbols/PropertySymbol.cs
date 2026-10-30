using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class PropertySymbol : Symbol, IPropertySymbol, ISymbol, IEquatable<ISymbol>
{
	private SymbolCompilationScope? compilationScope;

	public override SymbolKind Kind => SymbolKind.Property;

	public abstract PropertyKind PropertyKind { get; }

	internal abstract BoundProperty Property { get; }

	public override string MetadataName => GetPropertyTypeInfo()?.MetadataName;

	public override SymbolCompilationScope CompilationScope
	{
		get
		{
			if (compilationScope.HasValue)
			{
				return compilationScope.Value;
			}
			SymbolCompilationScope symbolCompilationScope = SymbolCompilationScope.Personalization;
			PropertyTypeInfo propertyTypeInfo = GetPropertyTypeInfo();
			if (propertyTypeInfo != null)
			{
				symbolCompilationScope = ((Property.PropertyValue.Kind != BoundKind.EnumPropertyValue) ? propertyTypeInfo.Scope : ((EnumPropertyTypeInfo)propertyTypeInfo).GetScope(ValueText));
			}
			compilationScope = symbolCompilationScope;
			return symbolCompilationScope;
		}
	}

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	internal abstract object Value { get; }

	public abstract string ValueText { get; }

	public override VersionCompatibility Compatibility => GetPropertyTypeInfo()?.Compatibility;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitProperty(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitProperty(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitProperty(this, a);
	}

	internal PropertyTypeInfo GetPropertyTypeInfo()
	{
		return Property?.PropertyValue?.GetPropertyTypeInfo();
	}

	public override string GetDocumentation()
	{
		return DocumentationFacts.GetDocumentation(this);
	}
}
