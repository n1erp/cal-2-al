using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class AsyncStateMachine : StateMachineTypeSymbol
{
	private readonly TypeKind _typeKind;

	private readonly MethodSymbol _constructor;

	private readonly ImmutableArray<NamedTypeSymbol> _interfaces;

	public override TypeKind TypeKind => _typeKind;

	internal override MethodSymbol Constructor => _constructor;

	public AsyncStateMachine(VariableSlotAllocator variableAllocatorOpt, TypeCompilationState compilationState, MethodSymbol asyncMethod, int asyncMethodOrdinal, TypeKind typeKind)
		: base(variableAllocatorOpt, compilationState, asyncMethod, asyncMethodOrdinal)
	{
		_typeKind = typeKind;
		_interfaces = ImmutableArray.Create(asyncMethod.DeclaringCompilation.GetWellKnownType(WellKnownType.System_Runtime_CompilerServices_IAsyncStateMachine));
		_constructor = new AsyncConstructor(this);
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved)
	{
		return _interfaces;
	}
}
