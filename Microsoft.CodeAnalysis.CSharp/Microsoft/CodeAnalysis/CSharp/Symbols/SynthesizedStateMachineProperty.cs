using System.Collections.Immutable;
using Microsoft.Cci;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SynthesizedStateMachineProperty : PropertySymbol, ISynthesizedMethodBodyImplementationSymbol
{
	private readonly SynthesizedStateMachineMethod _getter;

	private readonly string _name;

	public override string Name => _name;

	public override RefKind RefKind => RefKind.None;

	public override TypeSymbol Type => _getter.ReturnType;

	public override ImmutableArray<CustomModifier> TypeCustomModifiers => _getter.ReturnTypeCustomModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _getter.RefCustomModifiers;

	public override ImmutableArray<ParameterSymbol> Parameters => _getter.Parameters;

	public override bool IsIndexer => !_getter.Parameters.IsEmpty;

	internal override bool HasSpecialName => false;

	public override MethodSymbol GetMethod => _getter;

	public override MethodSymbol SetMethod => null;

	internal override CallingConvention CallingConvention => _getter.CallingConvention;

	internal override bool MustCallMethodsDirectly => false;

	private PropertySymbol ImplementedProperty => (PropertySymbol)_getter.ExplicitInterfaceImplementations[0].AssociatedSymbol;

	public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations => ImmutableArray.Create(ImplementedProperty);

	public override Symbol ContainingSymbol => _getter.ContainingSymbol;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Accessibility DeclaredAccessibility => _getter.DeclaredAccessibility;

	public override bool IsStatic => _getter.IsStatic;

	public override bool IsVirtual => _getter.IsVirtual;

	public override bool IsOverride => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => false;

	public override bool IsExtern => false;

	internal override ObsoleteAttributeData ObsoleteAttributeData => null;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => _getter.HasMethodBodyDependency;

	IMethodSymbol ISynthesizedMethodBodyImplementationSymbol.Method => ((ISynthesizedMethodBodyImplementationSymbol)ContainingSymbol).Method;

	internal SynthesizedStateMachineProperty(MethodSymbol interfacePropertyGetter, StateMachineTypeSymbol stateMachineType)
	{
		_name = ExplicitInterfaceHelpers.GetMemberName(interfacePropertyGetter.AssociatedSymbol.Name, interfacePropertyGetter.ContainingType, null);
		string memberName = ExplicitInterfaceHelpers.GetMemberName(interfacePropertyGetter.Name, interfacePropertyGetter.ContainingType, null);
		_getter = new SynthesizedStateMachineDebuggerHiddenMethod(memberName, interfacePropertyGetter, stateMachineType, this, hasMethodBodyDependency: false);
	}
}
