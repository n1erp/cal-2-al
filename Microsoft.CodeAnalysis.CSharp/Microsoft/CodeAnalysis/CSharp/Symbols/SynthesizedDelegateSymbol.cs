using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedDelegateSymbol : SynthesizedContainer
{
	private sealed class DelegateConstructor : SynthesizedInstanceConstructor
	{
		private readonly ImmutableArray<ParameterSymbol> _parameters;

		public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

		public DelegateConstructor(NamedTypeSymbol containingType, TypeSymbol objectType, TypeSymbol intPtrType)
			: base(containingType)
		{
			_parameters = ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, objectType, 0, RefKind.None, "object"), SynthesizedParameterSymbol.Create(this, intPtrType, 1, RefKind.None, "method"));
		}
	}

	private sealed class InvokeMethod : SynthesizedInstanceMethodSymbol
	{
		private readonly ImmutableArray<ParameterSymbol> _parameters;

		private readonly TypeSymbol _containingType;

		private readonly TypeSymbol _returnType;

		public override string Name => "Invoke";

		internal override bool IsMetadataFinal => false;

		public override MethodKind MethodKind => MethodKind.DelegateInvoke;

		public override int Arity => 0;

		public override bool IsExtensionMethod => false;

		internal override bool HasSpecialName => false;

		internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.CodeTypeMask;

		internal override bool HasDeclarativeSecurity => false;

		internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

		internal override bool RequiresSecurityObject => false;

		public override bool HidesBaseMethodsByName => false;

		public override bool IsVararg => false;

		public override bool ReturnsVoid => _returnType.SpecialType == SpecialType.System_Void;

		public override bool IsAsync => false;

		public override RefKind RefKind => RefKind.None;

		public override TypeSymbol ReturnType => _returnType;

		public override ImmutableArray<TypeSymbol> TypeArguments => ImmutableArray<TypeSymbol>.Empty;

		public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

		public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

		public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

		public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

		public override Symbol AssociatedSymbol => null;

		internal override CallingConvention CallingConvention => CallingConvention.HasThis;

		internal override bool GenerateDebugInfo => false;

		public override Symbol ContainingSymbol => _containingType;

		public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

		public override Accessibility DeclaredAccessibility => Accessibility.Public;

		public override bool IsStatic => false;

		public override bool IsVirtual => true;

		public override bool IsOverride => false;

		public override bool IsAbstract => false;

		public override bool IsSealed => false;

		public override bool IsExtern => false;

		internal InvokeMethod(SynthesizedDelegateSymbol containingType, BitVector byRefParameters, TypeSymbol voidReturnTypeOpt)
		{
			ImmutableArray<TypeParameterSymbol> typeParameters = containingType.TypeParameters;
			_containingType = containingType;
			_returnType = voidReturnTypeOpt ?? typeParameters.Last();
			ParameterSymbol[] array = new ParameterSymbol[typeParameters.Length - (((object)voidReturnTypeOpt == null) ? 1 : 0)];
			for (int i = 0; i < array.Length; i++)
			{
				RefKind refKind = ((!byRefParameters.IsNull && byRefParameters[i]) ? RefKind.Ref : RefKind.None);
				array[i] = SynthesizedParameterSymbol.Create(this, typeParameters[i], i, refKind);
			}
			_parameters = array.AsImmutableOrNull();
		}

		internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
		{
			return true;
		}

		internal override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
		{
			return true;
		}

		public override DllImportData GetDllImportData()
		{
			return null;
		}

		internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
		{
			throw ExceptionUtilities.Unreachable;
		}

		internal override ImmutableArray<string> GetAppliedConditionalSymbols()
		{
			return ImmutableArray<string>.Empty;
		}
	}

	private readonly NamespaceOrTypeSymbol _containingSymbol;

	private readonly MethodSymbol _constructor;

	private readonly MethodSymbol _invoke;

	public override Symbol ContainingSymbol => _containingSymbol;

	public override TypeKind TypeKind => TypeKind.Delegate;

	internal override MethodSymbol Constructor => _constructor;

	public override IEnumerable<string> MemberNames => new string[2] { _constructor.Name, _invoke.Name };

	public override Accessibility DeclaredAccessibility => Accessibility.Internal;

	public override bool IsSealed => true;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => ContainingAssembly.GetSpecialType(SpecialType.System_MulticastDelegate);

	public SynthesizedDelegateSymbol(NamespaceOrTypeSymbol containingSymbol, string name, TypeSymbol objectType, TypeSymbol intPtrType, TypeSymbol voidReturnTypeOpt, int parameterCount, BitVector byRefParameters)
		: base(name, parameterCount, (object)voidReturnTypeOpt != null)
	{
		_containingSymbol = containingSymbol;
		_constructor = new DelegateConstructor(this, objectType, intPtrType);
		_invoke = new InvokeMethod(this, byRefParameters, voidReturnTypeOpt);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray.Create((Symbol)_constructor, (Symbol)_invoke);
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		if (!(name == _constructor.Name))
		{
			if (!(name == _invoke.Name))
			{
				return ImmutableArray<Symbol>.Empty;
			}
			return ImmutableArray.Create((Symbol)_invoke);
		}
		return ImmutableArray.Create((Symbol)_constructor);
	}
}
