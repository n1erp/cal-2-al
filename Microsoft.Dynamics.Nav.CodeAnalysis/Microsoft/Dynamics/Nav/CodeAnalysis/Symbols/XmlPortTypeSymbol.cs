using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class XmlPortTypeSymbol : ApplicationObjectTypeSymbol, IXmlPortTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	public override SymbolKind Kind => SymbolKind.XmlPort;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind => NavTypeKind.XmlPort;

	internal override TypeSymbol BaseType => XmlportInstanceClassTypeSymbol.Instance;

	public override string MetadataName => "XMLport" + Id.ToString(CultureInfo.InvariantCulture);

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.XmlPortProperties;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitXmlPortType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitXmlPortType(this, a);
	}

	protected XmlPortTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetXmlPortTriggerInfo(name.ToUpperInvariant());
	}
}
