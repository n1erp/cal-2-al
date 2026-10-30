using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ReducedExtensionMethodSymbol : MethodSymbol
{
	private sealed class ReducedExtensionMethodParameterSymbol : WrappedParameterSymbol
	{
		private readonly ReducedExtensionMethodSymbol _containingMethod;

		public override Symbol ContainingSymbol => _containingMethod;

		public override int Ordinal => _underlyingParameter.Ordinal - 1;

		public override TypeSymbol Type => _containingMethod._typeMap.SubstituteType(_underlyingParameter.Type).Type;

		public override ImmutableArray<CustomModifier> CustomModifiers => _containingMethod._typeMap.SubstituteCustomModifiers(_underlyingParameter.Type, _underlyingParameter.CustomModifiers);

		public override ImmutableArray<CustomModifier> RefCustomModifiers => _containingMethod._typeMap.SubstituteCustomModifiers(_underlyingParameter.RefCustomModifiers);

		public ReducedExtensionMethodParameterSymbol(ReducedExtensionMethodSymbol containingMethod, ParameterSymbol underlyingParameter)
			: base(underlyingParameter)
		{
			_containingMethod = containingMethod;
		}

		public sealed override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj is ReducedExtensionMethodParameterSymbol reducedExtensionMethodParameterSymbol && Ordinal == reducedExtensionMethodParameterSymbol.Ordinal)
			{
				return ContainingSymbol.Equals(reducedExtensionMethodParameterSymbol.ContainingSymbol);
			}
			return false;
		}

		public sealed override int GetHashCode()
		{
			return Hash.Combine(ContainingSymbol, _underlyingParameter.Ordinal);
		}
	}

	private readonly MethodSymbol _reducedFrom;

	private readonly TypeMap _typeMap;

	private readonly ImmutableArray<TypeParameterSymbol> _typeParameters;

	private readonly ImmutableArray<TypeSymbol> _typeArguments;

	private ImmutableArray<ParameterSymbol> _lazyParameters;

	internal override MethodSymbol CallsiteReducedFromMethod => _reducedFrom.ConstructIfGeneric(_typeArguments);

	public override TypeSymbol ReceiverType => _reducedFrom.Parameters[0].Type;

	public override MethodSymbol ReducedFrom => _reducedFrom;

	public override MethodSymbol ConstructedFrom => this;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => _typeParameters;

	public override ImmutableArray<TypeSymbol> TypeArguments => _typeArguments;

	internal override CallingConvention CallingConvention => _reducedFrom.CallingConvention;

	public override int Arity => _reducedFrom.Arity;

	public override string Name => _reducedFrom.Name;

	internal override bool HasSpecialName => _reducedFrom.HasSpecialName;

	internal override MethodImplAttributes ImplementationAttributes => _reducedFrom.ImplementationAttributes;

	internal override bool RequiresSecurityObject => _reducedFrom.RequiresSecurityObject;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => _reducedFrom.ReturnValueMarshallingInformation;

	internal override bool HasDeclarativeSecurity => _reducedFrom.HasDeclarativeSecurity;

	public override AssemblySymbol ContainingAssembly => _reducedFrom.ContainingAssembly;

	public override ImmutableArray<Location> Locations => _reducedFrom.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _reducedFrom.DeclaringSyntaxReferences;

	public override MethodSymbol OriginalDefinition => this;

	public override bool IsExtern => _reducedFrom.IsExtern;

	public override bool IsSealed => _reducedFrom.IsSealed;

	public override bool IsVirtual => _reducedFrom.IsVirtual;

	public override bool IsAbstract => _reducedFrom.IsAbstract;

	public override bool IsOverride => _reducedFrom.IsOverride;

	public override bool IsStatic => false;

	public override bool IsAsync => _reducedFrom.IsAsync;

	public override bool IsExtensionMethod => true;

	internal override bool IsMetadataFinal => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => _reducedFrom.ObsoleteAttributeData;

	public override Accessibility DeclaredAccessibility => _reducedFrom.DeclaredAccessibility;

	public override Symbol ContainingSymbol => _reducedFrom.ContainingSymbol;

	public override Symbol AssociatedSymbol => null;

	public override MethodKind MethodKind => MethodKind.ReducedExtension;

	public override bool ReturnsVoid => _reducedFrom.ReturnsVoid;

	public override bool IsGenericMethod => _reducedFrom.IsGenericMethod;

	public override bool IsVararg => _reducedFrom.IsVararg;

	public override RefKind RefKind => _reducedFrom.RefKind;

	public override TypeSymbol ReturnType => _typeMap.SubstituteType(_reducedFrom.ReturnType).Type;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => _typeMap.SubstituteCustomModifiers(_reducedFrom.ReturnType, _reducedFrom.ReturnTypeCustomModifiers);

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _typeMap.SubstituteCustomModifiers(_reducedFrom.RefCustomModifiers);

	internal override int ParameterCount => _reducedFrom.ParameterCount - 1;

	internal override bool GenerateDebugInfo => _reducedFrom.GenerateDebugInfo;

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			if (_lazyParameters.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _lazyParameters, MakeParameters(), default);
			}
			return _lazyParameters;
		}
	}

	internal override bool IsExplicitInterfaceImplementation => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	public override bool HidesBaseMethodsByName => false;

	public static MethodSymbol Create(MethodSymbol method, TypeSymbol receiverType, Compilation compilation)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		method = method.InferExtensionMethodTypeArguments(receiverType, compilation, ref useSiteDiagnostics);
		if ((object)method == null)
		{
			return null;
		}
		if (!new TypeConversions(method.ContainingAssembly.CorLibrary).ConvertExtensionMethodThisArg(method.Parameters[0].Type, receiverType, ref useSiteDiagnostics).Exists)
		{
			return null;
		}
		if (useSiteDiagnostics != null)
		{
			foreach (DiagnosticInfo item in useSiteDiagnostics)
			{
				if (item.Severity == DiagnosticSeverity.Error)
				{
					return null;
				}
			}
		}
		return Create(method);
	}

	public static MethodSymbol Create(MethodSymbol method)
	{
		MethodSymbol constructedFrom = method.ConstructedFrom;
		ReducedExtensionMethodSymbol reducedExtensionMethodSymbol = new ReducedExtensionMethodSymbol(constructedFrom);
		if (constructedFrom == method)
		{
			return reducedExtensionMethodSymbol;
		}
		return reducedExtensionMethodSymbol.Construct(method.TypeArguments);
	}

	private ReducedExtensionMethodSymbol(MethodSymbol reducedFrom)
	{
		_reducedFrom = reducedFrom;
		_typeMap = TypeMap.Empty.WithAlphaRename(reducedFrom, this, out _typeParameters);
		_typeArguments = _typeMap.SubstituteTypesWithoutModifiers(reducedFrom.TypeArguments);
	}

	public override TypeSymbol GetTypeInferredDuringReduction(TypeParameterSymbol reducedFromTypeParameter)
	{
		if ((object)reducedFromTypeParameter == null)
		{
			throw new ArgumentNullException();
		}
		if (reducedFromTypeParameter.ContainingSymbol != _reducedFrom)
		{
			throw new ArgumentException();
		}
		return null;
	}

	public override DllImportData GetDllImportData()
	{
		return _reducedFrom.GetDllImportData();
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		return _reducedFrom.GetSecurityInformation();
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return _reducedFrom.GetAppliedConditionalSymbols();
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _reducedFrom.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal sealed override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return _reducedFrom.GetAttributes();
	}

	internal override bool CallsAreOmitted(SyntaxTree syntaxTree)
	{
		return _reducedFrom.CallsAreOmitted(syntaxTree);
	}

	private ImmutableArray<ParameterSymbol> MakeParameters()
	{
		ImmutableArray<ParameterSymbol> parameters = _reducedFrom.Parameters;
		int length = parameters.Length;
		if (length <= 1)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		ParameterSymbol[] array = new ParameterSymbol[length - 1];
		for (int i = 0; i < length - 1; i++)
		{
			array[i] = new ReducedExtensionMethodParameterSymbol(this, parameters[i + 1]);
		}
		return array.AsImmutableOrNull();
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is ReducedExtensionMethodSymbol reducedExtensionMethodSymbol)
		{
			return _reducedFrom.Equals(reducedExtensionMethodSymbol._reducedFrom);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _reducedFrom.GetHashCode();
	}
}
