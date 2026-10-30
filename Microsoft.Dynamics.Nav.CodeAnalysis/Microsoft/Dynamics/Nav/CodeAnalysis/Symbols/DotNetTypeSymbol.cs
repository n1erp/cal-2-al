using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class DotNetTypeSymbol : TypeSymbol, IDotNetTypeSymbol, IRootTypeSymbol, ISymbol, IEquatable<ISymbol>, IEquatable<DotNetTypeSymbol>
{
	private int memberIdSeed;

	private DotNetTypeSymbol lazyElementType;

	private ImmutableArray<Symbol> lazyMembers;

	private ImmutableArray<EventSymbol> lazyEvents;

	private readonly Lazy<ControlAddInTypeSymbol> lazyControlAddInSymbol;

	public override string Name => DotNetTypeInfo.DisplayName;

	public override string MetadataName => DotNetTypeInfo.TypeName;

	public string AssemblyName => DotNetTypeInfo.Assembly.FullName;

	public override Symbol ContainingSymbol { get; }

	public DotNetTypeInfo DotNetTypeInfo { get; }

	public ControlAddInTypeSymbol ControlAddIn => lazyControlAddInSymbol.Value;

	public bool IsControlAddInType => DeclaringSymbol?.IsControlAddIn ?? false;

	public override Accessibility DeclaredAccessibility { get; } = Accessibility.Public;

	public override SymbolKind Kind => SymbolKind.DotNet;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public override NavTypeKind NavTypeKind => NavTypeKind.DotNet;

	public override SymbolCompilationScope CompilationScope { get; } = SymbolCompilationScope.Internal;

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	internal override ModuleSymbol ContainingModule { get; }

	internal DotNetTypeDeclarationSymbol DeclaringSymbol { get; }

	public bool IsEnumerable => DotNetTypeInfo.IsEnumerable;

	public DotNetTypeSymbol ElementType
	{
		get
		{
			if (DotNetTypeInfo.EnumeratorElementType == null)
			{
				return null;
			}
			if (lazyElementType != null)
			{
				return lazyElementType;
			}
			lazyElementType = (DotNetTypeSymbol)ContainingModule.DotNetResolver.MapReferencedType(ContainingModule, DotNetTypeInfo.EnumeratorElementType);
			return lazyElementType;
		}
	}

	public ImmutableArray<EventSymbol> Events
	{
		get
		{
			if (lazyEvents != null)
			{
				return lazyEvents;
			}
			ImmutableArray<EventSymbol> value = GetMembers().WhereAsCastArray<EventSymbol, Symbol>((Symbol s) => s.Kind == SymbolKind.Event);
			ImmutableInterlocked.InterlockedCompareExchange(ref lazyEvents, value, default);
			return lazyEvents;
		}
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetType(this, a);
	}

	public DotNetTypeSymbol(ModuleSymbol containingModule, DotNetTypeInfo typeInfo, DotNetTypeDeclarationSymbol declaringSymbol)
	{
		DebugAssertHelper.Assert(typeInfo != null);
		ContainingModule = containingModule;
		ContainingSymbol = containingModule;
		DeclaringSymbol = declaringSymbol;
		DotNetTypeInfo = typeInfo;
		memberIdSeed = 0;
		lazyControlAddInSymbol = new Lazy<ControlAddInTypeSymbol>(() => (!IsControlAddInType) ? null : new DotNetControlAddInTypeSymbol(ContainingModule, this));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableArray<Symbol> value = ContainingModule.DotNetResolver.MapTypeMembers(this).CastArray<Symbol>();
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, value, default);
		return lazyMembers;
	}

	internal int GetNextMemberId()
	{
		return Interlocked.Increment(ref memberIdSeed);
	}

	internal override bool Equals(TypeSymbol t2)
	{
		if (t2?.NavTypeKind != NavTypeKind)
		{
			return false;
		}
		return Equals(t2 as DotNetTypeSymbol);
	}

	public bool Equals(DotNetTypeSymbol other)
	{
		return DotNetTypeInfo.Equals(other?.DotNetTypeInfo);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as DotNetTypeSymbol);
	}

	public override int GetHashCode()
	{
		return DotNetTypeInfo.GetHashCode();
	}
}
