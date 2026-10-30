using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class DotNetControlAddInTypeSymbol : ControlAddInTypeSymbol
{
	private readonly DotNetTypeSymbol dotNetType;

	private ImmutableArray<Symbol> lazyMembers;

	public override string Name { get; }

	public override string MetadataName { get; }

	public override string PublicKeyToken { get; }

	public override ImmutableArray<Location> Locations => dotNetType.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => dotNetType.DeclaringSyntaxReferences;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetControlAddInType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetControlAddInType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetControlAddInType(this, a);
	}

	internal DotNetControlAddInTypeSymbol(ModuleSymbol containingSymbol, DotNetTypeSymbol dotNetType)
		: base(containingSymbol)
	{
		this.dotNetType = dotNetType;
		PublicKeyToken = this.dotNetType.DotNetTypeInfo.Assembly.PublicKeyToken;
		Name = this.dotNetType.Name;
		MetadataName = this.dotNetType.DotNetTypeInfo.ControlAddInName;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		LazyGetMembers();
		return lazyMembers;
	}

	private void LazyGetMembers()
	{
		if (!(lazyMembers != default(ImmutableArray<Symbol>)))
		{
			ImmutableArray<Symbol> value = dotNetType.GetMembers().WhereAsArray((Symbol m) => IsMemberVisibleInControlAddIn(m));
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, value, default);
		}
	}

	private static bool IsMemberVisibleInControlAddIn(Symbol m)
	{
		if (m.IsKind(SymbolKind.Event, SymbolKind.Method) && m is IDotNetControlAddInMemberSymbol dotNetControlAddInMemberSymbol)
		{
			return dotNetControlAddInMemberSymbol.IsVisibleInControlAddIn;
		}
		return false;
	}
}
