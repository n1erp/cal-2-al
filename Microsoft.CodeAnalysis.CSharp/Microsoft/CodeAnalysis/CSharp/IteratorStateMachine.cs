using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class IteratorStateMachine : StateMachineTypeSymbol
{
	private readonly MethodSymbol _constructor;

	private readonly ImmutableArray<NamedTypeSymbol> _interfaces;

	internal readonly TypeSymbol ElementType;

	public override TypeKind TypeKind => TypeKind.Class;

	internal override MethodSymbol Constructor => _constructor;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => ContainingAssembly.GetSpecialType(SpecialType.System_Object);

	public IteratorStateMachine(VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, MethodSymbol iteratorMethod, int iteratorMethodOrdinal, bool isEnumerable, TypeSymbol elementType)
		: base(slotAllocatorOpt, compilationState, iteratorMethod, iteratorMethodOrdinal)
	{
		ElementType = TypeMap.SubstituteType(elementType).Type;
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		if (isEnumerable)
		{
			instance.Add(ContainingAssembly.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T).Construct(ElementType));
			instance.Add(ContainingAssembly.GetSpecialType(SpecialType.System_Collections_IEnumerable));
		}
		instance.Add(ContainingAssembly.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerator_T).Construct(ElementType));
		instance.Add(ContainingAssembly.GetSpecialType(SpecialType.System_IDisposable));
		instance.Add(ContainingAssembly.GetSpecialType(SpecialType.System_Collections_IEnumerator));
		_interfaces = instance.ToImmutableAndFree();
		_constructor = new IteratorConstructor(this);
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved)
	{
		return _interfaces;
	}
}
