using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class XmlPortReferenceTypeSymbol : NavTypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol
{
	private readonly XmlPortTypeSymbol xmlPortTypeSymbol;

	internal bool IsCurrXmlPort { get; }

	public override TypeSymbol OriginalDefinition => xmlPortTypeSymbol;

	internal override TypeSymbol BaseType => XmlportInstanceClassTypeSymbol.Instance;

	protected override Symbol OriginalSymbolDefinition => xmlPortTypeSymbol;

	public override TypeCategoryKind TypeCategory => xmlPortTypeSymbol.TypeCategory;

	public override SymbolKind Kind => SymbolKind.XmlPortReference;

	public override string Name => OriginalSymbolDefinition.Name;

	public int Id => xmlPortTypeSymbol.Id;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitXmlPortReferenceType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortReferenceType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitXmlPortReferenceType(this, a);
	}

	private XmlPortReferenceTypeSymbol(XmlPortTypeSymbol xmlPortTypeSymbol, bool isCurrXmlPort)
		: base(isCurrXmlPort ? NavTypeKind.CurrXmlPort : NavTypeKind.XmlPort)
	{
		this.xmlPortTypeSymbol = xmlPortTypeSymbol;
		IsCurrXmlPort = isCurrXmlPort;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return xmlPortTypeSymbol.GetMembers();
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return xmlPortTypeSymbol.GetMembers(name);
	}

	internal static XmlPortReferenceTypeSymbol Create(XmlPortTypeSymbol xmlPortTypeSymbol, bool isCurrXmlPort = false)
	{
		return new XmlPortReferenceTypeSymbol(xmlPortTypeSymbol, isCurrXmlPort);
	}

	internal override bool Equals(TypeSymbol t2)
	{
		if (t2?.NavTypeKind != NavTypeKind)
		{
			return false;
		}
		return ((XmlPortReferenceTypeSymbol)t2).xmlPortTypeSymbol.Equals(xmlPortTypeSymbol);
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return xmlPortTypeSymbol.GetCodeMembers();
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return xmlPortTypeSymbol.GetCodeMembers(name);
	}
}
