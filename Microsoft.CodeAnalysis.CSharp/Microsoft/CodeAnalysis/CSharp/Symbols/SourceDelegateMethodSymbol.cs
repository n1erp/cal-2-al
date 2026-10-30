using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceDelegateMethodSymbol : SourceMemberMethodSymbol
{
	private sealed class Constructor : SourceDelegateMethodSymbol
	{
		public override string Name => ".ctor";

		public override RefKind RefKind => RefKind.None;

		internal Constructor(SourceMemberContainerTypeSymbol delegateType, TypeSymbol voidType, TypeSymbol objectType, TypeSymbol intPtrType, DelegateDeclarationSyntax syntax)
			: base(delegateType, voidType, syntax, MethodKind.Constructor, DeclarationModifiers.Public)
		{
			InitializeParameters(ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, objectType, 0, RefKind.None, "object"), SynthesizedParameterSymbol.Create(this, intPtrType, 1, RefKind.None, "method")));
		}

		internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetReturnTypeAttributeDeclarations()
		{
			return OneOrMany.Create(default(SyntaxList<AttributeListSyntax>));
		}

		internal override LexicalSortKey GetLexicalSortKey()
		{
			return new LexicalSortKey(syntaxReferenceOpt.GetLocation(), DeclaringCompilation);
		}
	}

	private sealed class InvokeMethod : SourceDelegateMethodSymbol
	{
		private readonly RefKind _refKind;

		private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

		public override string Name => "Invoke";

		public override RefKind RefKind => _refKind;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

		internal InvokeMethod(SourceMemberContainerTypeSymbol delegateType, RefKind refKind, TypeSymbol returnType, DelegateDeclarationSyntax syntax, Binder binder, DiagnosticBag diagnostics)
			: base(delegateType, returnType, syntax, MethodKind.DelegateInvoke, DeclarationModifiers.Public | DeclarationModifiers.Virtual)
		{
			_refKind = refKind;
			ImmutableArray<ParameterSymbol> parameters = ParameterHelpers.MakeParameters(binder, this, syntax.ParameterList, out var arglistToken, diagnostics, allowRefOrOut: true, allowThis: false, addRefReadOnlyModifier: true);
			if (arglistToken.Kind() == SyntaxKind.ArgListKeyword)
			{
				diagnostics.Add(ErrorCode.ERR_IllegalVarArgs, new SourceLocation(in arglistToken));
			}
			if (_refKind == RefKind.In)
			{
				NamedTypeSymbol wellKnownType = binder.GetWellKnownType(WellKnownType.System_Runtime_InteropServices_InAttribute, diagnostics, syntax.ReturnType);
				_refCustomModifiers = ImmutableArray.Create(CSharpCustomModifier.CreateRequired(wellKnownType));
			}
			else
			{
				_refCustomModifiers = ImmutableArray<CustomModifier>.Empty;
			}
			InitializeParameters(parameters);
		}

		internal override LexicalSortKey GetLexicalSortKey()
		{
			return new LexicalSortKey(syntaxReferenceOpt.GetLocation(), DeclaringCompilation);
		}

		internal override void AfterAddingTypeMembersChecks(ConversionsBase conversions, DiagnosticBag diagnostics)
		{
			base.AfterAddingTypeMembersChecks(conversions, diagnostics);
			if (_refKind == RefKind.In)
			{
				DelegateDeclarationSyntax delegateDeclarationSyntax = (DelegateDeclarationSyntax)SyntaxRef.GetSyntax();
				DeclaringCompilation.EnsureIsReadOnlyAttributeExists(diagnostics, delegateDeclarationSyntax.ReturnType.GetLocation(), modifyCompilationForRefReadOnly: true);
			}
			ParameterHelpers.EnsureIsReadOnlyAttributeExists(Parameters, diagnostics, modifyCompilationForRefReadOnly: true);
		}
	}

	private sealed class BeginInvokeMethod : SourceDelegateMethodSymbol
	{
		public override string Name => "BeginInvoke";

		public override RefKind RefKind => RefKind.None;

		internal BeginInvokeMethod(InvokeMethod invoke, TypeSymbol iAsyncResultType, TypeSymbol objectType, TypeSymbol asyncCallbackType, DelegateDeclarationSyntax syntax)
			: base((SourceNamedTypeSymbol)invoke.ContainingType, iAsyncResultType, syntax, MethodKind.Ordinary, DeclarationModifiers.Public | DeclarationModifiers.Virtual)
		{
			ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
			foreach (SourceParameterSymbol parameter in invoke.Parameters)
			{
				SourceClonedParameterSymbol item = new SourceClonedParameterSymbol(parameter, this, parameter.Ordinal, suppressOptional: true);
				instance.Add(item);
			}
			int parameterCount = invoke.ParameterCount;
			instance.Add(SynthesizedParameterSymbol.Create(this, asyncCallbackType, parameterCount, RefKind.None, GetUniqueParameterName(instance, "callback")));
			instance.Add(SynthesizedParameterSymbol.Create(this, objectType, parameterCount + 1, RefKind.None, GetUniqueParameterName(instance, "object")));
			InitializeParameters(instance.ToImmutableAndFree());
		}

		internal override OneOrMany<SyntaxList<AttributeListSyntax>> GetReturnTypeAttributeDeclarations()
		{
			return OneOrMany.Create(default(SyntaxList<AttributeListSyntax>));
		}
	}

	private sealed class EndInvokeMethod : SourceDelegateMethodSymbol
	{
		private readonly InvokeMethod _invoke;

		protected override SourceMemberMethodSymbol BoundAttributesSource => _invoke;

		public override string Name => "EndInvoke";

		public override RefKind RefKind => _invoke.RefKind;

		public override ImmutableArray<CustomModifier> RefCustomModifiers => _invoke.RefCustomModifiers;

		internal EndInvokeMethod(InvokeMethod invoke, TypeSymbol iAsyncResultType, DelegateDeclarationSyntax syntax)
			: base((SourceNamedTypeSymbol)invoke.ContainingType, invoke.ReturnType, syntax, MethodKind.Ordinary, DeclarationModifiers.Public | DeclarationModifiers.Virtual)
		{
			_invoke = invoke;
			ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
			int ordinal = 0;
			foreach (SourceParameterSymbol parameter in invoke.Parameters)
			{
				if (parameter.RefKind != RefKind.None)
				{
					SourceClonedParameterSymbol item = new SourceClonedParameterSymbol(parameter, this, ordinal++, suppressOptional: true);
					instance.Add(item);
				}
			}
			instance.Add(SynthesizedParameterSymbol.Create(this, iAsyncResultType, ordinal, RefKind.None, GetUniqueParameterName(instance, "result")));
			InitializeParameters(instance.ToImmutableAndFree());
		}
	}

	private ImmutableArray<ParameterSymbol> _parameters;

	private readonly TypeSymbol _returnType;

	public sealed override bool IsVararg => false;

	public sealed override ImmutableArray<ParameterSymbol> Parameters => _parameters;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override ImmutableArray<TypeParameterConstraintClause> TypeParameterConstraintClauses => ImmutableArray<TypeParameterConstraintClause>.Empty;

	public sealed override TypeSymbol ReturnType => _returnType;

	public sealed override bool IsImplicitlyDeclared => true;

	internal override bool IsExpressionBodied => false;

	internal override bool GenerateDebugInfo => false;

	protected sealed override IAttributeTargetSymbol AttributeOwner => (SourceNamedTypeSymbol)ContainingSymbol;

	internal sealed override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.CodeTypeMask;

	protected SourceDelegateMethodSymbol(SourceMemberContainerTypeSymbol delegateType, TypeSymbol returnType, DelegateDeclarationSyntax syntax, MethodKind methodKind, DeclarationModifiers declarationModifiers)
		: base(delegateType, syntax.GetReference(), syntax.Identifier.GetLocation())
	{
		_returnType = returnType;
		MakeFlags(methodKind, declarationModifiers, _returnType.SpecialType == SpecialType.System_Void, isExtensionMethod: false);
	}

	protected void InitializeParameters(ImmutableArray<ParameterSymbol> parameters)
	{
		_parameters = parameters;
	}

	internal static void AddDelegateMembers(SourceMemberContainerTypeSymbol delegateType, ArrayBuilder<Symbol> symbols, DelegateDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		Binder binder = delegateType.GetBinder(syntax.ParameterList);
		TypeSyntax typeSyntax = syntax.ReturnType.SkipRef(out var refKind);
		TypeSymbol typeSymbol = binder.BindType(typeSyntax, diagnostics);
		NamedTypeSymbol specialType = binder.GetSpecialType(SpecialType.System_Void, diagnostics, syntax);
		NamedTypeSymbol specialType2 = binder.GetSpecialType(SpecialType.System_Object, diagnostics, syntax);
		NamedTypeSymbol specialType3 = binder.GetSpecialType(SpecialType.System_IntPtr, diagnostics, syntax);
		if (typeSymbol.IsRestrictedType(ignoreSpanLikeTypes: true))
		{
			diagnostics.Add(ErrorCode.ERR_MethodReturnCantBeRefAny, typeSyntax.Location, typeSymbol);
		}
		InvokeMethod invokeMethod = new InvokeMethod(delegateType, refKind, typeSymbol, syntax, binder, diagnostics);
		invokeMethod.CheckDelegateVarianceSafety(diagnostics);
		symbols.Add(invokeMethod);
		symbols.Add(new Constructor(delegateType, specialType, specialType2, specialType3, syntax));
		if (binder.Compilation.GetSpecialType(SpecialType.System_IAsyncResult).TypeKind != TypeKind.Error && binder.Compilation.GetSpecialType(SpecialType.System_AsyncCallback).TypeKind != TypeKind.Error && !delegateType.IsCompilationOutputWinMdObj())
		{
			NamedTypeSymbol specialType4 = binder.GetSpecialType(SpecialType.System_IAsyncResult, diagnostics, syntax);
			NamedTypeSymbol specialType5 = binder.GetSpecialType(SpecialType.System_AsyncCallback, diagnostics, syntax);
			symbols.Add(new BeginInvokeMethod(invokeMethod, specialType4, specialType2, specialType5, syntax));
			symbols.Add(new EndInvokeMethod(invokeMethod, specialType4, syntax));
		}
		if (delegateType.DeclaredAccessibility <= Accessibility.Private)
		{
			return;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (!delegateType.IsNoMoreVisibleThan(invokeMethod.ReturnType, ref useSiteDiagnostics))
		{
			diagnostics.Add(ErrorCode.ERR_BadVisDelegateReturn, delegateType.Locations[0], delegateType, invokeMethod.ReturnType);
		}
		foreach (ParameterSymbol parameter in invokeMethod.Parameters)
		{
			if (!parameter.Type.IsAtLeastAsVisibleAs(delegateType, ref useSiteDiagnostics))
			{
				diagnostics.Add(ErrorCode.ERR_BadVisDelegateParam, delegateType.Locations[0], delegateType, parameter.Type);
			}
		}
		diagnostics.Add(delegateType.Locations[0], useSiteDiagnostics);
	}

	protected override void MethodChecks(DiagnosticBag diagnostics)
	{
	}

	internal sealed override OneOrMany<SyntaxList<AttributeListSyntax>> GetAttributeDeclarations()
	{
		return OneOrMany.Create(((SourceNamedTypeSymbol)ContainingSymbol).GetAttributeDeclarations());
	}

	internal sealed override AttributeTargets GetAttributeTarget()
	{
		return AttributeTargets.Delegate;
	}

	private static string GetUniqueParameterName(ArrayBuilder<ParameterSymbol> currentParameters, string name)
	{
		while (!IsUnique(currentParameters, name))
		{
			name = "__" + name;
		}
		return name;
	}

	private static bool IsUnique(ArrayBuilder<ParameterSymbol> currentParameters, string name)
	{
		foreach (ParameterSymbol currentParameter in currentParameters)
		{
			if (string.CompareOrdinal(currentParameter.Name, name) == 0)
			{
				return false;
			}
		}
		return true;
	}
}
