using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MethodTypeInferrer
{
	private enum InferenceResult
	{
		InferenceFailed,
		MadeProgress,
		NoProgress,
		Success
	}

	private enum Dependency
	{
		Unknown = 0,
		NotDependent = 1,
		DependsMask = 16,
		Direct = 17,
		Indirect = 18
	}

	private sealed class EqualsIgnoringDynamicAndTupleNamesComparer : EqualityComparer<TypeSymbol>
	{
		public static EqualsIgnoringDynamicAndTupleNamesComparer Instance { get; } = new EqualsIgnoringDynamicAndTupleNamesComparer();

		public override int GetHashCode(TypeSymbol obj)
		{
			return obj?.GetHashCode() ?? 0;
		}

		public override bool Equals(TypeSymbol x, TypeSymbol y)
		{
			if (x.IsDynamic() ^ y.IsDynamic())
			{
				return false;
			}
			return x?.Equals(y, TypeCompareKind.IgnoreDynamicAndTupleNames) ?? ((object)y == null);
		}
	}

	private readonly ConversionsBase _conversions;

	private readonly ImmutableArray<TypeParameterSymbol> _methodTypeParameters;

	private readonly NamedTypeSymbol _constructedContainingTypeOfMethod;

	private readonly ImmutableArray<TypeSymbol> _formalParameterTypes;

	private readonly ImmutableArray<RefKind> _formalParameterRefKinds;

	private readonly ImmutableArray<BoundExpression> _arguments;

	private readonly TypeSymbol[] _fixedResults;

	private readonly HashSet<TypeSymbol>[] _exactBounds;

	private readonly HashSet<TypeSymbol>[] _upperBounds;

	private readonly HashSet<TypeSymbol>[] _lowerBounds;

	private Dependency[,] _dependencies;

	private bool _dependenciesDirty;

	private int NumberArgumentsToProcess => Math.Min(_arguments.Length, _formalParameterTypes.Length);

	public static MethodTypeInferenceResult Infer(Binder binder, ImmutableArray<TypeParameterSymbol> methodTypeParameters, NamedTypeSymbol constructedContainingTypeOfMethod, ImmutableArray<TypeSymbol> formalParameterTypes, ImmutableArray<RefKind> formalParameterRefKinds, ImmutableArray<BoundExpression> arguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (formalParameterTypes.Length == 0)
		{
			return new MethodTypeInferenceResult(success: false, default);
		}
		return new MethodTypeInferrer(binder.Conversions, methodTypeParameters, constructedContainingTypeOfMethod, formalParameterTypes, formalParameterRefKinds, arguments).InferTypeArgs(binder, ref useSiteDiagnostics);
	}

	private MethodTypeInferrer(ConversionsBase conversions, ImmutableArray<TypeParameterSymbol> methodTypeParameters, NamedTypeSymbol constructedContainingTypeOfMethod, ImmutableArray<TypeSymbol> formalParameterTypes, ImmutableArray<RefKind> formalParameterRefKinds, ImmutableArray<BoundExpression> arguments)
	{
		_conversions = conversions;
		_methodTypeParameters = methodTypeParameters;
		_constructedContainingTypeOfMethod = constructedContainingTypeOfMethod;
		_formalParameterTypes = formalParameterTypes;
		_formalParameterRefKinds = formalParameterRefKinds;
		_arguments = arguments;
		_fixedResults = new TypeSymbol[methodTypeParameters.Length];
		_exactBounds = new HashSet<TypeSymbol>[methodTypeParameters.Length];
		_upperBounds = new HashSet<TypeSymbol>[methodTypeParameters.Length];
		_lowerBounds = new HashSet<TypeSymbol>[methodTypeParameters.Length];
		_dependencies = null;
		_dependenciesDirty = false;
	}

	private RefKind GetRefKind(int index)
	{
		if (!_formalParameterRefKinds.IsDefault)
		{
			return _formalParameterRefKinds[index];
		}
		return RefKind.None;
	}

	private ImmutableArray<TypeSymbol> GetResults()
	{
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			if ((object)_fixedResults[i] == null || (_fixedResults[i].IsErrorType() && _fixedResults[i].Name == null))
			{
				_fixedResults[i] = new ExtendedErrorTypeSymbol(_constructedContainingTypeOfMethod, _methodTypeParameters[i].Name, 0, null);
			}
		}
		return ImmutableArray.Create(_fixedResults);
	}

	private bool ValidIndex(int index)
	{
		if (0 <= index)
		{
			return index < _methodTypeParameters.Length;
		}
		return false;
	}

	private bool IsUnfixed(int methodTypeParameterIndex)
	{
		return (object)_fixedResults[methodTypeParameterIndex] == null;
	}

	private bool IsUnfixedTypeParameter(TypeSymbol type)
	{
		if (type.TypeKind != TypeKind.TypeParameter)
		{
			return false;
		}
		TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)type;
		int ordinal = typeParameterSymbol.Ordinal;
		if (ValidIndex(ordinal) && typeParameterSymbol == _methodTypeParameters[ordinal])
		{
			return IsUnfixed(ordinal);
		}
		return false;
	}

	private bool AllFixed()
	{
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			if (IsUnfixed(i))
			{
				return false;
			}
		}
		return true;
	}

	private void AddBound(TypeSymbol addedBound, HashSet<TypeSymbol>[] collectedBounds, TypeParameterSymbol methodTypeParameter)
	{
		int ordinal = methodTypeParameter.Ordinal;
		if (collectedBounds[ordinal] == null)
		{
			collectedBounds[ordinal] = new HashSet<TypeSymbol>();
		}
		collectedBounds[ordinal].Add(addedBound);
	}

	private void AddLowerBound(TypeParameterSymbol methodTypeParameter, TypeSymbol lowerBound)
	{
		AddBound(lowerBound, _lowerBounds, methodTypeParameter);
	}

	private void AddUpperBound(TypeParameterSymbol methodTypeParameter, TypeSymbol upperBound)
	{
		AddBound(upperBound, _upperBounds, methodTypeParameter);
	}

	private void AddExactBound(TypeParameterSymbol methodTypeParameter, TypeSymbol exactBound)
	{
		AddBound(exactBound, _exactBounds, methodTypeParameter);
	}

	private bool HasBound(int methodTypeParameterIndex)
	{
		if (_lowerBounds[methodTypeParameterIndex] == null && _upperBounds[methodTypeParameterIndex] == null)
		{
			return _exactBounds[methodTypeParameterIndex] != null;
		}
		return true;
	}

	private NamedTypeSymbol GetFixedDelegate(NamedTypeSymbol delegateType)
	{
		ArrayBuilder<TypeWithModifiers> instance = ArrayBuilder<TypeWithModifiers>.GetInstance(_methodTypeParameters.Length);
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			instance.Add(new TypeWithModifiers(IsUnfixed(i) ? _methodTypeParameters[i] : _fixedResults[i]));
		}
		return new TypeMap(_constructedContainingTypeOfMethod, _methodTypeParameters, instance.ToImmutableAndFree()).SubstituteNamedType(delegateType);
	}

	private MethodTypeInferenceResult InferTypeArgs(Binder binder, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		InferTypeArgsFirstPhase(binder, ref useSiteDiagnostics);
		return new MethodTypeInferenceResult(InferTypeArgsSecondPhase(binder, ref useSiteDiagnostics), GetResults());
	}

	private void InferTypeArgsFirstPhase(Binder binder, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		int i = 0;
		for (int numberArgumentsToProcess = NumberArgumentsToProcess; i < numberArgumentsToProcess; i++)
		{
			BoundExpression argument = _arguments[i];
			TypeSymbol typeSymbol = _formalParameterTypes[i];
			bool isExactInference = GetRefKind(i).IsManagedReference() || typeSymbol.IsPointerType();
			MakeExplicitParameterTypeInferences(binder, argument, typeSymbol, isExactInference, ref useSiteDiagnostics);
		}
	}

	private void MakeExplicitParameterTypeInferences(Binder binder, BoundExpression argument, TypeSymbol target, bool isExactInference, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol type = argument.Type;
		if (argument.Kind == BoundKind.UnboundLambda)
		{
			ExplicitParameterTypeInference(argument, target, ref useSiteDiagnostics);
		}
		else if ((argument.Kind != BoundKind.TupleLiteral || !MakeExplicitParameterTypeInferences(binder, (BoundTupleLiteral)argument, target, isExactInference, ref useSiteDiagnostics)) && IsReallyAType(type))
		{
			if (isExactInference)
			{
				ExactInference(type, target, ref useSiteDiagnostics);
			}
			else
			{
				LowerBoundInference(type, target, ref useSiteDiagnostics);
			}
		}
	}

	private bool MakeExplicitParameterTypeInferences(Binder binder, BoundTupleLiteral argument, TypeSymbol target, bool isExactInference, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (target.Kind != SymbolKind.NamedType)
		{
			return false;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)target;
		ImmutableArray<BoundExpression> arguments = argument.Arguments;
		if (!namedTypeSymbol.IsTupleOrCompatibleWithTupleOfCardinality(arguments.Length))
		{
			return false;
		}
		ImmutableArray<TypeSymbol> elementTypesOfTupleOrCompatible = namedTypeSymbol.GetElementTypesOfTupleOrCompatible();
		for (int i = 0; i < arguments.Length; i++)
		{
			BoundExpression argument2 = arguments[i];
			TypeSymbol target2 = elementTypesOfTupleOrCompatible[i];
			MakeExplicitParameterTypeInferences(binder, argument2, target2, isExactInference, ref useSiteDiagnostics);
		}
		return true;
	}

	private bool InferTypeArgsSecondPhase(Binder binder, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		InitializeDependencies();
		while (true)
		{
			switch (DoSecondPhase(binder, ref useSiteDiagnostics))
			{
			case InferenceResult.InferenceFailed:
				return false;
			case InferenceResult.Success:
				return true;
			}
		}
	}

	private InferenceResult DoSecondPhase(Binder binder, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (AllFixed())
		{
			return InferenceResult.Success;
		}
		MakeOutputTypeInferences(binder, ref useSiteDiagnostics);
		InferenceResult inferenceResult = FixNondependentParameters(ref useSiteDiagnostics);
		if (inferenceResult != InferenceResult.NoProgress)
		{
			return inferenceResult;
		}
		inferenceResult = FixDependentParameters(ref useSiteDiagnostics);
		if (inferenceResult != InferenceResult.NoProgress)
		{
			return inferenceResult;
		}
		return InferenceResult.InferenceFailed;
	}

	private void MakeOutputTypeInferences(Binder binder, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		int i = 0;
		for (int numberArgumentsToProcess = NumberArgumentsToProcess; i < numberArgumentsToProcess; i++)
		{
			TypeSymbol formalType = _formalParameterTypes[i];
			BoundExpression argument = _arguments[i];
			MakeOutputTypeInferences(binder, argument, formalType, ref useSiteDiagnostics);
		}
	}

	private void MakeOutputTypeInferences(Binder binder, BoundExpression argument, TypeSymbol formalType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (argument.Kind == BoundKind.TupleLiteral && (object)argument.Type == null)
		{
			MakeOutputTypeInferences(binder, (BoundTupleLiteral)argument, formalType, ref useSiteDiagnostics);
		}
		else if (HasUnfixedParamInOutputType(argument, formalType) && !HasUnfixedParamInInputType(argument, formalType))
		{
			OutputTypeInference(binder, argument, formalType, ref useSiteDiagnostics);
		}
	}

	private void MakeOutputTypeInferences(Binder binder, BoundTupleLiteral argument, TypeSymbol formalType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (formalType.Kind != SymbolKind.NamedType)
		{
			return;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)formalType;
		ImmutableArray<BoundExpression> arguments = argument.Arguments;
		if (namedTypeSymbol.IsTupleOrCompatibleWithTupleOfCardinality(arguments.Length))
		{
			ImmutableArray<TypeSymbol> elementTypesOfTupleOrCompatible = namedTypeSymbol.GetElementTypesOfTupleOrCompatible();
			for (int i = 0; i < arguments.Length; i++)
			{
				BoundExpression argument2 = arguments[i];
				TypeSymbol formalType2 = elementTypesOfTupleOrCompatible[i];
				MakeOutputTypeInferences(binder, argument2, formalType2, ref useSiteDiagnostics);
			}
		}
	}

	private InferenceResult FixNondependentParameters(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool[] array = new bool[_methodTypeParameters.Length];
		InferenceResult result = InferenceResult.NoProgress;
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			if (IsUnfixed(i) && HasBound(i) && !DependsOnAny(i))
			{
				array[i] = true;
				result = InferenceResult.MadeProgress;
			}
		}
		for (int j = 0; j < _methodTypeParameters.Length; j++)
		{
			if (array[j] && !Fix(j, ref useSiteDiagnostics))
			{
				result = InferenceResult.InferenceFailed;
			}
		}
		return result;
	}

	private InferenceResult FixDependentParameters(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool[] array = new bool[_methodTypeParameters.Length];
		InferenceResult result = InferenceResult.NoProgress;
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			if (IsUnfixed(i) && HasBound(i) && AnyDependsOn(i))
			{
				array[i] = true;
				result = InferenceResult.MadeProgress;
			}
		}
		for (int j = 0; j < _methodTypeParameters.Length; j++)
		{
			if (array[j] && !Fix(j, ref useSiteDiagnostics))
			{
				result = InferenceResult.InferenceFailed;
			}
		}
		return result;
	}

	private static bool DoesInputTypeContain(BoundExpression argument, TypeSymbol formalParameterType, TypeParameterSymbol typeParameter)
	{
		NamedTypeSymbol delegateType = formalParameterType.GetDelegateType();
		if ((object)delegateType == null)
		{
			return false;
		}
		if (argument.Kind != BoundKind.UnboundLambda && argument.Kind != BoundKind.MethodGroup)
		{
			return false;
		}
		ImmutableArray<ParameterSymbol> immutableArray = delegateType.DelegateParameters();
		if (immutableArray.IsDefaultOrEmpty)
		{
			return false;
		}
		foreach (ParameterSymbol item in immutableArray)
		{
			if (item.Type.ContainsTypeParameter(typeParameter))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasUnfixedParamInInputType(BoundExpression pSource, TypeSymbol pDest)
	{
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			if (IsUnfixed(i) && DoesInputTypeContain(pSource, pDest, _methodTypeParameters[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool DoesOutputTypeContain(BoundExpression argument, TypeSymbol formalParameterType, TypeParameterSymbol typeParameter)
	{
		NamedTypeSymbol delegateType = formalParameterType.GetDelegateType();
		if ((object)delegateType == null)
		{
			return false;
		}
		if (argument.Kind != BoundKind.UnboundLambda && argument.Kind != BoundKind.MethodGroup)
		{
			return false;
		}
		MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
		if ((object)delegateInvokeMethod == null || delegateInvokeMethod.HasUseSiteError)
		{
			return false;
		}
		return delegateInvokeMethod.ReturnType?.ContainsTypeParameter(typeParameter) ?? false;
	}

	private bool HasUnfixedParamInOutputType(BoundExpression argument, TypeSymbol formalParameterType)
	{
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			if (IsUnfixed(i) && DoesOutputTypeContain(argument, formalParameterType, _methodTypeParameters[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool DependsDirectlyOn(int iParam, int jParam)
	{
		int i = 0;
		for (int numberArgumentsToProcess = NumberArgumentsToProcess; i < numberArgumentsToProcess; i++)
		{
			TypeSymbol formalParameterType = _formalParameterTypes[i];
			BoundExpression argument = _arguments[i];
			if (DoesInputTypeContain(argument, formalParameterType, _methodTypeParameters[jParam]) && DoesOutputTypeContain(argument, formalParameterType, _methodTypeParameters[iParam]))
			{
				return true;
			}
		}
		return false;
	}

	private void InitializeDependencies()
	{
		_dependencies = new Dependency[_methodTypeParameters.Length, _methodTypeParameters.Length];
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			for (int j = 0; j < _methodTypeParameters.Length; j++)
			{
				if (DependsDirectlyOn(i, j))
				{
					_dependencies[i, j] = Dependency.Direct;
				}
			}
		}
		DeduceAllDependencies();
	}

	private bool DependsOn(int iParam, int jParam)
	{
		if (_dependenciesDirty)
		{
			SetIndirectsToUnknown();
			DeduceAllDependencies();
		}
		return (_dependencies[iParam, jParam] & Dependency.DependsMask) != 0;
	}

	private bool DependsTransitivelyOn(int iParam, int jParam)
	{
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			if ((_dependencies[iParam, i] & Dependency.DependsMask) != 0 && (_dependencies[i, jParam] & Dependency.DependsMask) != 0)
			{
				return true;
			}
		}
		return false;
	}

	private void DeduceAllDependencies()
	{
		while (DeduceDependencies())
		{
		}
		SetUnknownsToNotDependent();
		_dependenciesDirty = false;
	}

	private bool DeduceDependencies()
	{
		bool result = false;
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			for (int j = 0; j < _methodTypeParameters.Length; j++)
			{
				if (_dependencies[i, j] == Dependency.Unknown && DependsTransitivelyOn(i, j))
				{
					_dependencies[i, j] = Dependency.Indirect;
					result = true;
				}
			}
		}
		return result;
	}

	private void SetUnknownsToNotDependent()
	{
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			for (int j = 0; j < _methodTypeParameters.Length; j++)
			{
				if (_dependencies[i, j] == Dependency.Unknown)
				{
					_dependencies[i, j] = Dependency.NotDependent;
				}
			}
		}
	}

	private void SetIndirectsToUnknown()
	{
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			for (int j = 0; j < _methodTypeParameters.Length; j++)
			{
				if (_dependencies[i, j] == Dependency.Indirect)
				{
					_dependencies[i, j] = Dependency.Unknown;
				}
			}
		}
	}

	private void UpdateDependenciesAfterFix(int iParam)
	{
		if (_dependencies != null)
		{
			for (int i = 0; i < _methodTypeParameters.Length; i++)
			{
				_dependencies[iParam, i] = Dependency.NotDependent;
				_dependencies[i, iParam] = Dependency.NotDependent;
			}
			_dependenciesDirty = true;
		}
	}

	private bool DependsOnAny(int iParam)
	{
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			if (DependsOn(iParam, i))
			{
				return true;
			}
		}
		return false;
	}

	private bool AnyDependsOn(int iParam)
	{
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			if (DependsOn(i, iParam))
			{
				return true;
			}
		}
		return false;
	}

	private void OutputTypeInference(Binder binder, BoundExpression expression, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!InferredReturnTypeInference(expression, target, ref useSiteDiagnostics) && !MethodGroupReturnTypeInference(binder, expression, target, ref useSiteDiagnostics))
		{
			TypeSymbol type = expression.Type;
			if ((object)type != null)
			{
				LowerBoundInference(type, target, ref useSiteDiagnostics);
			}
		}
	}

	private bool InferredReturnTypeInference(BoundExpression source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		NamedTypeSymbol delegateType = target.GetDelegateType();
		if ((object)delegateType == null)
		{
			return false;
		}
		TypeSymbol returnType = delegateType.DelegateInvokeMethod.ReturnType;
		if ((object)returnType == null || returnType.SpecialType == SpecialType.System_Void)
		{
			return false;
		}
		TypeSymbol typeSymbol = InferReturnType(source, delegateType, ref useSiteDiagnostics);
		if ((object)typeSymbol == null)
		{
			return false;
		}
		LowerBoundInference(typeSymbol, returnType, ref useSiteDiagnostics);
		return true;
	}

	private bool MethodGroupReturnTypeInference(Binder binder, BoundExpression source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.Kind != BoundKind.MethodGroup)
		{
			return false;
		}
		NamedTypeSymbol delegateType = target.GetDelegateType();
		if ((object)delegateType == null)
		{
			return false;
		}
		MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
		TypeSymbol returnType = delegateInvokeMethod.ReturnType;
		if ((object)returnType == null || returnType.SpecialType == SpecialType.System_Void)
		{
			return false;
		}
		ImmutableArray<ParameterSymbol> delegateParameters = GetFixedDelegate(delegateType).DelegateParameters();
		if (delegateParameters.IsDefault)
		{
			return false;
		}
		TypeSymbol typeSymbol = MethodGroupReturnType(binder, (BoundMethodGroup)source, delegateParameters, delegateInvokeMethod.RefKind, ref useSiteDiagnostics);
		if ((object)typeSymbol == null || typeSymbol.SpecialType == SpecialType.System_Void)
		{
			return false;
		}
		LowerBoundInference(typeSymbol, returnType, ref useSiteDiagnostics);
		return true;
	}

	private static TypeSymbol MethodGroupReturnType(Binder binder, BoundMethodGroup source, ImmutableArray<ParameterSymbol> delegateParameters, RefKind delegateRefKind, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		Conversions.GetDelegateArguments(source.Syntax, instance, delegateParameters, binder.Compilation);
		MethodGroupResolution methodGroupResolution = binder.ResolveMethodGroup(source, instance, isMethodGroupConversion: true, ref useSiteDiagnostics, inferWithDynamic: false, delegateRefKind);
		TypeSymbol result = null;
		if (!methodGroupResolution.IsEmpty)
		{
			OverloadResolutionResult<MethodSymbol> overloadResolutionResult = methodGroupResolution.OverloadResolutionResult;
			if (overloadResolutionResult.Succeeded)
			{
				result = overloadResolutionResult.BestResult.Member.ReturnType;
			}
		}
		instance.Free();
		methodGroupResolution.Free();
		return result;
	}

	private void ExplicitParameterTypeInference(BoundExpression source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.Kind != BoundKind.UnboundLambda)
		{
			return;
		}
		UnboundLambda unboundLambda = (UnboundLambda)source;
		if (!unboundLambda.HasExplicitlyTypedParameterList)
		{
			return;
		}
		NamedTypeSymbol delegateType = target.GetDelegateType();
		if ((object)delegateType == null)
		{
			return;
		}
		ImmutableArray<ParameterSymbol> immutableArray = delegateType.DelegateParameters();
		if (!immutableArray.IsDefault)
		{
			int num = immutableArray.Length;
			if (unboundLambda.ParameterCount < num)
			{
				num = unboundLambda.ParameterCount;
			}
			for (int i = 0; i < num; i++)
			{
				ExactInference(unboundLambda.ParameterType(i), immutableArray[i].Type, ref useSiteDiagnostics);
			}
		}
	}

	private void ExactInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!ExactTypeParameterInference(source, target) && !ExactArrayInference(source, target, ref useSiteDiagnostics) && !ExactNullableInference(source, target, ref useSiteDiagnostics) && !ExactTupleInference(source, target, ref useSiteDiagnostics) && !ExactConstructedInference(source, target, ref useSiteDiagnostics))
		{
			ExactPointerInference(source, target, ref useSiteDiagnostics);
		}
	}

	private bool ExactTypeParameterInference(TypeSymbol source, TypeSymbol target)
	{
		if (IsUnfixedTypeParameter(target))
		{
			AddExactBound((TypeParameterSymbol)target, source);
			return true;
		}
		return false;
	}

	private bool ExactArrayInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.IsArray() || !target.IsArray())
		{
			return false;
		}
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)source;
		ArrayTypeSymbol arrayTypeSymbol2 = (ArrayTypeSymbol)target;
		if (!arrayTypeSymbol.HasSameShapeAs(arrayTypeSymbol2))
		{
			return false;
		}
		ExactInference(arrayTypeSymbol.ElementType, arrayTypeSymbol2.ElementType, ref useSiteDiagnostics);
		return true;
	}

	private bool ExactNullableInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.IsNullableType() || !target.IsNullableType())
		{
			return false;
		}
		ExactInference(source.GetNullableUnderlyingType(), target.GetNullableUnderlyingType(), ref useSiteDiagnostics);
		return true;
	}

	private bool ExactTupleInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.TryGetElementTypesIfTupleOrCompatible(out var elementTypes) || !target.TryGetElementTypesIfTupleOrCompatible(out var elementTypes2) || elementTypes.Length != elementTypes2.Length)
		{
			return false;
		}
		for (int i = 0; i < elementTypes.Length; i++)
		{
			ExactInference(elementTypes[i], elementTypes2[i], ref useSiteDiagnostics);
		}
		return true;
	}

	private bool ExactConstructedInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!(source.TupleUnderlyingTypeOrSelf() is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		if (!(target.TupleUnderlyingTypeOrSelf() is NamedTypeSymbol namedTypeSymbol2))
		{
			return false;
		}
		if (namedTypeSymbol.OriginalDefinition != namedTypeSymbol2.OriginalDefinition)
		{
			return false;
		}
		ExactTypeArgumentInference(namedTypeSymbol, namedTypeSymbol2, ref useSiteDiagnostics);
		return true;
	}

	private bool ExactPointerInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.TypeKind == TypeKind.Pointer && target.TypeKind == TypeKind.Pointer)
		{
			ExactInference(((PointerTypeSymbol)source).PointedAtType, ((PointerTypeSymbol)target).PointedAtType, ref useSiteDiagnostics);
			return true;
		}
		return false;
	}

	private void ExactTypeArgumentInference(NamedTypeSymbol source, NamedTypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
		source.GetAllTypeArguments(instance, ref useSiteDiagnostics);
		target.GetAllTypeArguments(instance2, ref useSiteDiagnostics);
		for (int i = 0; i < instance.Count; i++)
		{
			ExactInference(instance[i], instance2[i], ref useSiteDiagnostics);
		}
		instance.Free();
		instance2.Free();
	}

	private void LowerBoundInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!LowerBoundTypeParameterInference(source, target) && !LowerBoundArrayInference(source, target, ref useSiteDiagnostics) && !LowerBoundNullableInference(source, target, ref useSiteDiagnostics) && !LowerBoundTupleInference(source, target, ref useSiteDiagnostics))
		{
			LowerBoundConstructedInference(source, target, ref useSiteDiagnostics);
		}
	}

	private bool LowerBoundTypeParameterInference(TypeSymbol source, TypeSymbol target)
	{
		if (IsUnfixedTypeParameter(target))
		{
			AddLowerBound((TypeParameterSymbol)target, source);
			return true;
		}
		return false;
	}

	private static TypeSymbol GetMatchingElementType(ArrayTypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (target.IsArray())
		{
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)target;
			if (!arrayTypeSymbol.HasSameShapeAs(source))
			{
				return null;
			}
			return arrayTypeSymbol.ElementType;
		}
		if (!source.IsSZArray)
		{
			return null;
		}
		if (!target.IsPossibleArrayGenericInterface())
		{
			return null;
		}
		return ((NamedTypeSymbol)target).TypeArgumentWithDefinitionUseSiteDiagnostics(0, ref useSiteDiagnostics);
	}

	private bool LowerBoundArrayInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.IsArray())
		{
			return false;
		}
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)source;
		TypeSymbol elementType = arrayTypeSymbol.ElementType;
		TypeSymbol matchingElementType = GetMatchingElementType(arrayTypeSymbol, target, ref useSiteDiagnostics);
		if ((object)matchingElementType == null)
		{
			return false;
		}
		if (elementType.IsReferenceType)
		{
			LowerBoundInference(elementType, matchingElementType, ref useSiteDiagnostics);
		}
		else
		{
			ExactInference(elementType, matchingElementType, ref useSiteDiagnostics);
		}
		return true;
	}

	private bool LowerBoundNullableInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.IsNullableType() || !target.IsNullableType())
		{
			return false;
		}
		LowerBoundInference(source.GetNullableUnderlyingType(), target.GetNullableUnderlyingType(), ref useSiteDiagnostics);
		return true;
	}

	private bool LowerBoundTupleInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.TryGetElementTypesIfTupleOrCompatible(out var elementTypes) || !target.TryGetElementTypesIfTupleOrCompatible(out var elementTypes2) || elementTypes.Length != elementTypes2.Length)
		{
			return false;
		}
		for (int i = 0; i < elementTypes.Length; i++)
		{
			LowerBoundInference(elementTypes[i], elementTypes2[i], ref useSiteDiagnostics);
		}
		return true;
	}

	private bool LowerBoundConstructedInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		source = source.TupleUnderlyingTypeOrSelf();
		target = target.TupleUnderlyingTypeOrSelf();
		if (!(target is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		if (namedTypeSymbol.AllTypeArgumentCount() == 0)
		{
			return false;
		}
		if (source is NamedTypeSymbol namedTypeSymbol2 && namedTypeSymbol2.OriginalDefinition == namedTypeSymbol.OriginalDefinition)
		{
			if (namedTypeSymbol2.IsInterface || namedTypeSymbol2.IsDelegateType())
			{
				LowerBoundTypeArgumentInference(namedTypeSymbol2, namedTypeSymbol, ref useSiteDiagnostics);
			}
			else
			{
				ExactTypeArgumentInference(namedTypeSymbol2, namedTypeSymbol, ref useSiteDiagnostics);
			}
			return true;
		}
		if (LowerBoundClassInference(source, namedTypeSymbol, ref useSiteDiagnostics))
		{
			return true;
		}
		if (LowerBoundInterfaceInference(source, namedTypeSymbol, ref useSiteDiagnostics))
		{
			return true;
		}
		return false;
	}

	private bool LowerBoundClassInference(TypeSymbol source, NamedTypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (target.TypeKind != TypeKind.Class)
		{
			return false;
		}
		NamedTypeSymbol namedTypeSymbol = null;
		if (source.TypeKind == TypeKind.Class)
		{
			namedTypeSymbol = source.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		else if (source.TypeKind == TypeKind.TypeParameter)
		{
			namedTypeSymbol = ((TypeParameterSymbol)source).EffectiveBaseClass(ref useSiteDiagnostics);
		}
		while ((object)namedTypeSymbol != null)
		{
			if (namedTypeSymbol.OriginalDefinition == target.OriginalDefinition)
			{
				ExactTypeArgumentInference(namedTypeSymbol, target, ref useSiteDiagnostics);
				return true;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		return false;
	}

	private bool LowerBoundInterfaceInference(TypeSymbol source, NamedTypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!target.IsInterface)
		{
			return false;
		}
		ImmutableArray<NamedTypeSymbol> interfaces;
		switch (source.TypeKind)
		{
		case TypeKind.Class:
		case TypeKind.Interface:
		case TypeKind.Struct:
			interfaces = source.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
			break;
		case TypeKind.TypeParameter:
		{
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)source;
			interfaces = typeParameterSymbol.EffectiveBaseClass(ref useSiteDiagnostics).AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics).Concat(typeParameterSymbol.AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics));
			break;
		}
		default:
			return false;
		}
		NamedTypeSymbol interfaceInferenceBound = GetInterfaceInferenceBound(interfaces, target);
		if ((object)interfaceInferenceBound == null)
		{
			return false;
		}
		LowerBoundTypeArgumentInference(interfaceInferenceBound, target, ref useSiteDiagnostics);
		return true;
	}

	private void LowerBoundTypeArgumentInference(NamedTypeSymbol source, NamedTypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance3 = ArrayBuilder<TypeSymbol>.GetInstance();
		source.OriginalDefinition.GetAllTypeParameters(instance);
		source.GetAllTypeArguments(instance2, ref useSiteDiagnostics);
		target.GetAllTypeArguments(instance3, ref useSiteDiagnostics);
		for (int i = 0; i < instance2.Count; i++)
		{
			TypeParameterSymbol typeParameterSymbol = instance[i];
			TypeSymbol typeSymbol = instance2[i];
			TypeSymbol target2 = instance3[i];
			if (typeSymbol.IsReferenceType && typeParameterSymbol.Variance == VarianceKind.Out)
			{
				LowerBoundInference(typeSymbol, target2, ref useSiteDiagnostics);
			}
			else if (typeSymbol.IsReferenceType && typeParameterSymbol.Variance == VarianceKind.In)
			{
				UpperBoundInference(typeSymbol, target2, ref useSiteDiagnostics);
			}
			else
			{
				ExactInference(typeSymbol, target2, ref useSiteDiagnostics);
			}
		}
		instance.Free();
		instance2.Free();
		instance3.Free();
	}

	private void UpperBoundInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!UpperBoundTypeParameterInference(source, target) && !UpperBoundArrayInference(source, target, ref useSiteDiagnostics))
		{
			UpperBoundConstructedInference(source, target, ref useSiteDiagnostics);
		}
	}

	private bool UpperBoundTypeParameterInference(TypeSymbol source, TypeSymbol target)
	{
		if (IsUnfixedTypeParameter(target))
		{
			AddUpperBound((TypeParameterSymbol)target, source);
			return true;
		}
		return false;
	}

	private bool UpperBoundArrayInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!target.IsArray())
		{
			return false;
		}
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)target;
		TypeSymbol elementType = arrayTypeSymbol.ElementType;
		TypeSymbol matchingElementType = GetMatchingElementType(arrayTypeSymbol, source, ref useSiteDiagnostics);
		if ((object)matchingElementType == null)
		{
			return false;
		}
		if (matchingElementType.IsReferenceType)
		{
			UpperBoundInference(matchingElementType, elementType, ref useSiteDiagnostics);
		}
		else
		{
			ExactInference(matchingElementType, elementType, ref useSiteDiagnostics);
		}
		return true;
	}

	private bool UpperBoundConstructedInference(TypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		source = source.TupleUnderlyingTypeOrSelf();
		target = target.TupleUnderlyingTypeOrSelf();
		if (!(source is NamedTypeSymbol namedTypeSymbol))
		{
			return false;
		}
		if (namedTypeSymbol.AllTypeArgumentCount() == 0)
		{
			return false;
		}
		if (target is NamedTypeSymbol namedTypeSymbol2 && namedTypeSymbol.OriginalDefinition == target.OriginalDefinition)
		{
			if (namedTypeSymbol2.IsInterface || namedTypeSymbol2.IsDelegateType())
			{
				UpperBoundTypeArgumentInference(namedTypeSymbol, namedTypeSymbol2, ref useSiteDiagnostics);
			}
			else
			{
				ExactTypeArgumentInference(namedTypeSymbol, namedTypeSymbol2, ref useSiteDiagnostics);
			}
			return true;
		}
		if (UpperBoundClassInference(namedTypeSymbol, target, ref useSiteDiagnostics))
		{
			return true;
		}
		if (UpperBoundInterfaceInference(namedTypeSymbol, target, ref useSiteDiagnostics))
		{
			return true;
		}
		return false;
	}

	private bool UpperBoundClassInference(NamedTypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.TypeKind != TypeKind.Class || target.TypeKind != TypeKind.Class)
		{
			return false;
		}
		NamedTypeSymbol namedTypeSymbol = target.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		while ((object)namedTypeSymbol != null)
		{
			if (namedTypeSymbol.OriginalDefinition == source.OriginalDefinition)
			{
				ExactTypeArgumentInference(source, namedTypeSymbol, ref useSiteDiagnostics);
				return true;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		return false;
	}

	private bool UpperBoundInterfaceInference(NamedTypeSymbol source, TypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!source.IsInterface)
		{
			return false;
		}
		TypeKind typeKind = target.TypeKind;
		if (typeKind != TypeKind.Class && typeKind != TypeKind.Interface && typeKind != TypeKind.Struct)
		{
			return false;
		}
		NamedTypeSymbol interfaceInferenceBound = GetInterfaceInferenceBound(target.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics), source);
		if ((object)interfaceInferenceBound == null)
		{
			return false;
		}
		UpperBoundTypeArgumentInference(source, interfaceInferenceBound, ref useSiteDiagnostics);
		return true;
	}

	private void UpperBoundTypeArgumentInference(NamedTypeSymbol source, NamedTypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<TypeParameterSymbol> instance = ArrayBuilder<TypeParameterSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance3 = ArrayBuilder<TypeSymbol>.GetInstance();
		source.OriginalDefinition.GetAllTypeParameters(instance);
		source.GetAllTypeArguments(instance2, ref useSiteDiagnostics);
		target.GetAllTypeArguments(instance3, ref useSiteDiagnostics);
		for (int i = 0; i < instance2.Count; i++)
		{
			TypeParameterSymbol typeParameterSymbol = instance[i];
			TypeSymbol typeSymbol = instance2[i];
			TypeSymbol target2 = instance3[i];
			if (typeSymbol.IsReferenceType && typeParameterSymbol.Variance == VarianceKind.Out)
			{
				UpperBoundInference(typeSymbol, target2, ref useSiteDiagnostics);
			}
			else if (typeSymbol.IsReferenceType && typeParameterSymbol.Variance == VarianceKind.In)
			{
				LowerBoundInference(typeSymbol, target2, ref useSiteDiagnostics);
			}
			else
			{
				ExactInference(typeSymbol, target2, ref useSiteDiagnostics);
			}
		}
		instance.Free();
		instance2.Free();
		instance3.Free();
	}

	private bool Fix(int iParam, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		HashSet<TypeSymbol> hashSet = _exactBounds[iParam];
		HashSet<TypeSymbol> hashSet2 = _lowerBounds[iParam];
		HashSet<TypeSymbol> hashSet3 = _upperBounds[iParam];
		Dictionary<TypeSymbol, TypeSymbol> dictionary = new Dictionary<TypeSymbol, TypeSymbol>(EqualsIgnoringDynamicAndTupleNamesComparer.Instance);
		if (hashSet == null)
		{
			if (hashSet2 != null)
			{
				foreach (TypeSymbol item in hashSet2)
				{
					AddOrMerge(dictionary, item);
				}
			}
			if (hashSet3 != null)
			{
				foreach (TypeSymbol item2 in hashSet3)
				{
					AddOrMerge(dictionary, item2);
				}
			}
		}
		else
		{
			foreach (TypeSymbol item3 in hashSet)
			{
				AddOrMerge(dictionary, item3);
			}
			if (dictionary.Count >= 2)
			{
				return false;
			}
		}
		if (dictionary.Count == 0)
		{
			return false;
		}
		List<TypeSymbol> list = dictionary.Keys.ToList();
		if (hashSet2 != null)
		{
			foreach (TypeSymbol item4 in hashSet2)
			{
				foreach (TypeSymbol item5 in list)
				{
					if (!item4.Equals(item5))
					{
						if (!ImplicitConversionExists(item4, item5, ref useSiteDiagnostics))
						{
							dictionary.Remove(item5);
						}
						else if (item4.Equals(item5, TypeCompareKind.IgnoreDynamicAndTupleNames))
						{
							MergeAndReplaceIfStillCandidate(dictionary, item5, item4);
						}
					}
				}
			}
		}
		if (hashSet3 != null)
		{
			foreach (TypeSymbol item6 in hashSet3)
			{
				foreach (TypeSymbol item7 in list)
				{
					if (!item6.Equals(item7))
					{
						if (!ImplicitConversionExists(item7, item6, ref useSiteDiagnostics))
						{
							dictionary.Remove(item7);
						}
						else if (item6.Equals(item7, TypeCompareKind.IgnoreDynamicAndTupleNames))
						{
							MergeAndReplaceIfStillCandidate(dictionary, item7, item6);
						}
					}
				}
			}
		}
		TypeSymbol typeSymbol = null;
		foreach (TypeSymbol key in dictionary.Keys)
		{
			foreach (TypeSymbol key2 in dictionary.Keys)
			{
				if (key.Equals(key2) || ImplicitConversionExists(key2, key, ref useSiteDiagnostics))
				{
					continue;
				}
				goto IL_02c8;
			}
			if ((object)typeSymbol == null)
			{
				typeSymbol = key;
				continue;
			}
			if (typeSymbol.Equals(key, TypeCompareKind.IgnoreDynamicAndTupleNames))
			{
				typeSymbol = MergeTupleNames(MergeDynamic(typeSymbol, key, _conversions.CorLibrary), key);
				continue;
			}
			return false;
			IL_02c8:;
		}
		if ((object)typeSymbol == null)
		{
			return false;
		}
		_fixedResults[iParam] = typeSymbol;
		UpdateDependenciesAfterFix(iParam);
		return true;
	}

	internal static TypeSymbol MergeDynamic(TypeSymbol first, TypeSymbol second, AssemblySymbol corLibrary)
	{
		if (first.Equals(second, TypeCompareKind.AllIgnoreOptionsForVB))
		{
			return first;
		}
		ImmutableArray<bool> self = CSharpCompilation.DynamicTransformsEncoder.EncodeWithoutCustomModifierFlags(first, RefKind.None);
		ImmutableArray<bool> other = CSharpCompilation.DynamicTransformsEncoder.EncodeWithoutCustomModifierFlags(second, RefKind.None);
		ImmutableArray<bool> dynamicTransformFlags = self.ZipAsArray(other, (bool f1, bool f2) => f1 | f2);
		return DynamicTypeDecoder.TransformTypeWithoutCustomModifierFlags(first, corLibrary, RefKind.None, dynamicTransformFlags);
	}

	internal static TypeSymbol MergeTupleNames(TypeSymbol first, TypeSymbol second)
	{
		if (first.Equals(second, TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds | TypeCompareKind.IgnoreDynamic) || !first.ContainsTupleNames())
		{
			return first;
		}
		ImmutableArray<string> self = CSharpCompilation.TupleNamesEncoder.Encode(first);
		ImmutableArray<string> other = CSharpCompilation.TupleNamesEncoder.Encode(second);
		ImmutableArray<string> immutableArray;
		if (self.IsDefault || other.IsDefault)
		{
			immutableArray = default;
		}
		else
		{
			immutableArray = self.ZipAsArray(other, (string n1, string n2) => (string.CompareOrdinal(n1, n2) != 0) ? null : n1);
			if (immutableArray.All((string n) => n == null))
			{
				immutableArray = default;
			}
		}
		return TupleTypeDecoder.DecodeTupleTypesIfApplicable(first, immutableArray);
	}

	private bool ImplicitConversionExists(TypeSymbol source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.IsDynamic() && !destination.IsDynamic())
		{
			return false;
		}
		return _conversions.ClassifyImplicitConversionFromType(source, destination, ref useSiteDiagnostics).Exists;
	}

	private TypeSymbol InferReturnType(BoundExpression source, NamedTypeSymbol target, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (source.Kind != BoundKind.UnboundLambda)
		{
			return null;
		}
		UnboundLambda unboundLambda = (UnboundLambda)source;
		if (unboundLambda.HasSignature)
		{
			ImmutableArray<ParameterSymbol> immutableArray = target.DelegateParameters();
			if (immutableArray.IsDefault)
			{
				return null;
			}
			if (immutableArray.Length != unboundLambda.ParameterCount)
			{
				return null;
			}
		}
		NamedTypeSymbol fixedDelegate = GetFixedDelegate(target);
		ImmutableArray<ParameterSymbol> immutableArray2 = fixedDelegate.DelegateParameters();
		if (unboundLambda.HasExplicitlyTypedParameterList)
		{
			for (int i = 0; i < unboundLambda.ParameterCount; i++)
			{
				if (!unboundLambda.ParameterType(i).Equals(immutableArray2[i].Type, TypeCompareKind.IgnoreDynamicAndTupleNames))
				{
					return null;
				}
			}
		}
		return unboundLambda.InferReturnType(fixedDelegate, ref useSiteDiagnostics);
	}

	private static NamedTypeSymbol GetInterfaceInferenceBound(ImmutableArray<NamedTypeSymbol> interfaces, NamedTypeSymbol target)
	{
		NamedTypeSymbol namedTypeSymbol = null;
		foreach (NamedTypeSymbol item in interfaces)
		{
			if (item.OriginalDefinition == target.OriginalDefinition)
			{
				if ((object)namedTypeSymbol == null)
				{
					namedTypeSymbol = item;
				}
				else if (namedTypeSymbol != item)
				{
					return null;
				}
			}
		}
		return namedTypeSymbol;
	}

	public static ImmutableArray<TypeSymbol> InferTypeArgumentsFromFirstArgument(ConversionsBase conversions, MethodSymbol method, ImmutableArray<BoundExpression> arguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (method.ParameterCount < 1 || arguments.Length < 1)
		{
			return default;
		}
		MethodSymbol constructedFrom = method.ConstructedFrom;
		MethodTypeInferrer methodTypeInferrer = new MethodTypeInferrer(conversions, constructedFrom.TypeParameters, constructedFrom.ContainingType, constructedFrom.GetParameterTypes(), constructedFrom.ParameterRefKinds, arguments);
		if (!methodTypeInferrer.InferTypeArgumentsFromFirstArgument(ref useSiteDiagnostics))
		{
			return default;
		}
		return methodTypeInferrer.GetInferredTypeArguments();
	}

	private bool InferTypeArgumentsFromFirstArgument(ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol typeSymbol = _formalParameterTypes[0];
		TypeSymbol type = _arguments[0].Type;
		if (!IsReallyAType(type))
		{
			return false;
		}
		LowerBoundInference(type, typeSymbol, ref useSiteDiagnostics);
		for (int i = 0; i < _methodTypeParameters.Length; i++)
		{
			TypeParameterSymbol parameter = _methodTypeParameters[i];
			if (typeSymbol.ContainsTypeParameter(parameter) && (!HasBound(i) || !Fix(i, ref useSiteDiagnostics)))
			{
				return false;
			}
		}
		return true;
	}

	private ImmutableArray<TypeSymbol> GetInferredTypeArguments()
	{
		return _fixedResults.AsImmutable();
	}

	private static bool IsReallyAType(TypeSymbol type)
	{
		if ((object)type != null && !type.IsErrorType())
		{
			return type.SpecialType != SpecialType.System_Void;
		}
		return false;
	}

	private void AddOrMerge(Dictionary<TypeSymbol, TypeSymbol> candidates, TypeSymbol @new)
	{
		if (candidates.TryGetValue(@new, out var value))
		{
			MergeAndReplaceIfStillCandidate(candidates, value, @new);
		}
		else
		{
			candidates.Add(@new, @new);
		}
	}

	private void MergeAndReplaceIfStillCandidate(Dictionary<TypeSymbol, TypeSymbol> dict, TypeSymbol old, TypeSymbol @new)
	{
		if (!@new.IsDynamic() && dict.TryGetValue(old, out var value))
		{
			dict.Remove(old);
			TypeSymbol typeSymbol = MergeTupleNames(MergeDynamic(value, @new, _conversions.CorLibrary), @new);
			dict.Add(typeSymbol, typeSymbol);
		}
	}
}
