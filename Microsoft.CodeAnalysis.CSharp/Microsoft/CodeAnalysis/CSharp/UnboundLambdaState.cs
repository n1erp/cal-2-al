using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class UnboundLambdaState
{
	internal class ReturnInferenceCacheKey
	{
		public readonly ImmutableArray<TypeSymbol> ParameterTypes;

		public readonly ImmutableArray<RefKind> ParameterRefKinds;

		public readonly NamedTypeSymbol TaskLikeReturnTypeOpt;

		public static readonly ReturnInferenceCacheKey Empty = new ReturnInferenceCacheKey(ImmutableArray<TypeSymbol>.Empty, ImmutableArray<Microsoft.CodeAnalysis.RefKind>.Empty, null);

		private ReturnInferenceCacheKey(ImmutableArray<TypeSymbol> parameterTypes, ImmutableArray<RefKind> parameterRefKinds, NamedTypeSymbol taskLikeReturnTypeOpt)
		{
			ParameterTypes = parameterTypes;
			ParameterRefKinds = parameterRefKinds;
			TaskLikeReturnTypeOpt = taskLikeReturnTypeOpt;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj is ReturnInferenceCacheKey returnInferenceCacheKey && returnInferenceCacheKey.ParameterTypes.SequenceEqual(ParameterTypes) && returnInferenceCacheKey.ParameterRefKinds.SequenceEqual(ParameterRefKinds))
			{
				return returnInferenceCacheKey.TaskLikeReturnTypeOpt == TaskLikeReturnTypeOpt;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(TaskLikeReturnTypeOpt?.GetHashCode() ?? 0, Hash.CombineValues(ParameterTypes));
		}

		public static ReturnInferenceCacheKey Create(NamedTypeSymbol delegateType, bool isAsync)
		{
			ImmutableArray<TypeSymbol> parameterTypes = ImmutableArray<TypeSymbol>.Empty;
			ImmutableArray<RefKind> parameterRefKinds = ImmutableArray<Microsoft.CodeAnalysis.RefKind>.Empty;
			NamedTypeSymbol namedTypeSymbol = null;
			MethodSymbol methodSymbol = DelegateInvokeMethod(delegateType);
			if ((object)methodSymbol != null)
			{
				int parameterCount = methodSymbol.ParameterCount;
				if (parameterCount > 0)
				{
					ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance(parameterCount);
					ArrayBuilder<RefKind> instance2 = ArrayBuilder<Microsoft.CodeAnalysis.RefKind>.GetInstance(parameterCount);
					foreach (ParameterSymbol parameter in methodSymbol.Parameters)
					{
						instance2.Add(parameter.RefKind);
						instance.Add(parameter.Type);
					}
					parameterTypes = instance.ToImmutableAndFree();
					parameterRefKinds = instance2.ToImmutableAndFree();
				}
				if (isAsync && methodSymbol.ReturnType is NamedTypeSymbol { SpecialType: not SpecialType.System_Void } namedTypeSymbol2 && namedTypeSymbol2.IsCustomTaskType(out var _))
				{
					namedTypeSymbol = namedTypeSymbol2.ConstructedFrom;
				}
			}
			if (parameterTypes.IsEmpty && parameterRefKinds.IsEmpty && (object)namedTypeSymbol == null)
			{
				return Empty;
			}
			return new ReturnInferenceCacheKey(parameterTypes, parameterRefKinds, namedTypeSymbol);
		}
	}

	private UnboundLambda _unboundLambda;

	protected readonly Binder binder;

	private ImmutableDictionary<NamedTypeSymbol, BoundLambda> _bindingCache = ImmutableDictionary<NamedTypeSymbol, BoundLambda>.Empty;

	private ImmutableDictionary<ReturnInferenceCacheKey, BoundLambda> _returnInferenceCache = ImmutableDictionary<ReturnInferenceCacheKey, BoundLambda>.Empty;

	private BoundLambda _errorBinding;

	public UnboundLambda UnboundLambda => _unboundLambda;

	public abstract MessageID MessageID { get; }

	public abstract bool HasSignature { get; }

	public abstract bool HasExplicitlyTypedParameterList { get; }

	public abstract int ParameterCount { get; }

	public abstract bool IsAsync { get; }

	public UnboundLambdaState(Binder binder, UnboundLambda unboundLambdaOpt)
	{
		_unboundLambda = unboundLambdaOpt;
		this.binder = binder;
	}

	public void SetUnboundLambda(UnboundLambda unbound)
	{
		_unboundLambda = unbound;
	}

	public abstract string ParameterName(int index);

	public abstract Location ParameterLocation(int index);

	public abstract TypeSymbol ParameterType(int index);

	public abstract RefKind RefKind(int index);

	protected abstract BoundBlock BindLambdaBody(LambdaSymbol lambdaSymbol, Binder lambdaBodyBinder, DiagnosticBag diagnostics);

	public virtual void GenerateAnonymousFunctionConversionError(DiagnosticBag diagnostics, TypeSymbol targetType)
	{
		binder.GenerateAnonymousFunctionConversionError(diagnostics, _unboundLambda.Syntax, _unboundLambda, targetType);
	}

	public BoundLambda Bind(NamedTypeSymbol delegateType)
	{
		if (!_bindingCache.TryGetValue(delegateType, out var value))
		{
			value = ReallyBind(delegateType);
			return ImmutableInterlocked.GetOrAdd(ref _bindingCache, delegateType, value);
		}
		return value;
	}

	internal IEnumerable<TypeSymbol> InferredReturnTypes()
	{
		bool any = false;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		foreach (BoundLambda value in _returnInferenceCache.Values)
		{
			TypeSymbol typeSymbol = value.InferredReturnType(ref useSiteDiagnostics);
			if ((object)typeSymbol != null)
			{
				any = true;
				yield return typeSymbol;
			}
		}
		if (!any)
		{
			TypeSymbol typeSymbol2 = BindForErrorRecovery().InferredReturnType(ref useSiteDiagnostics);
			if ((object)typeSymbol2 != null)
			{
				yield return typeSymbol2;
			}
		}
	}

	private static MethodSymbol DelegateInvokeMethod(NamedTypeSymbol delegateType)
	{
		return delegateType.GetDelegateType()?.DelegateInvokeMethod;
	}

	private static TypeSymbol DelegateReturnType(MethodSymbol invokeMethod, out RefKind refKind)
	{
		if ((object)invokeMethod == null)
		{
			refKind = Microsoft.CodeAnalysis.RefKind.None;
			return null;
		}
		refKind = invokeMethod.RefKind;
		return invokeMethod.ReturnType;
	}

	private bool DelegateNeedsReturn(MethodSymbol invokeMethod)
	{
		if ((object)invokeMethod == null || invokeMethod.ReturnsVoid)
		{
			return false;
		}
		if (IsAsync && invokeMethod.ReturnType.IsNonGenericTaskType(binder.Compilation))
		{
			return false;
		}
		return true;
	}

	private BoundLambda ReallyBind(NamedTypeSymbol delegateType)
	{
		MethodSymbol invokeMethod = DelegateInvokeMethod(delegateType);
		TypeSymbol typeSymbol = DelegateReturnType(invokeMethod, out var refKind);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		ReturnInferenceCacheKey returnInferenceCacheKey = ReturnInferenceCacheKey.Create(delegateType, IsAsync);
		LambdaSymbol lambdaSymbol;
		Binder binder;
		BoundBlock boundBlock;
		if (_returnInferenceCache.TryGetValue(returnInferenceCacheKey, out var value) && value.InferredFromSingleType)
		{
			lambdaSymbol = value.Symbol;
			if ((object)LambdaSymbol.InferenceFailureReturnType != lambdaSymbol.ReturnType && lambdaSymbol.ReturnType == typeSymbol && lambdaSymbol.RefKind == refKind)
			{
				binder = value.Binder;
				boundBlock = value.Body;
				instance.AddRange(value.Diagnostics);
				goto IL_013f;
			}
		}
		lambdaSymbol = new LambdaSymbol(this.binder.Compilation, this.binder.ContainingMemberOrLambda, _unboundLambda, returnInferenceCacheKey.ParameterTypes, returnInferenceCacheKey.ParameterRefKinds, refKind, typeSymbol, instance);
		binder = new ExecutableCodeBinder(_unboundLambda.Syntax, lambdaSymbol, ParameterBinder(lambdaSymbol, this.binder));
		if (lambdaSymbol.RefKind == Microsoft.CodeAnalysis.RefKind.In)
		{
			this.binder.Compilation.EnsureIsReadOnlyAttributeExists(instance, lambdaSymbol.DiagnosticLocation, modifyCompilationForRefReadOnly: false);
		}
		ParameterHelpers.EnsureIsReadOnlyAttributeExists(lambdaSymbol.Parameters, instance, modifyCompilationForRefReadOnly: false);
		boundBlock = BindLambdaBody(lambdaSymbol, binder, instance);
		((ExecutableCodeBinder)binder).ValidateIteratorMethods(instance);
		ValidateUnsafeParameters(instance, returnInferenceCacheKey.ParameterTypes);
		goto IL_013f;
		IL_013f:
		if (ControlFlowPass.Analyze(this.binder.Compilation, lambdaSymbol, boundBlock, instance))
		{
			if (DelegateNeedsReturn(invokeMethod))
			{
				instance.Add(ErrorCode.ERR_AnonymousReturnExpected, lambdaSymbol.DiagnosticLocation, MessageID.Localize(), delegateType);
			}
			else
			{
				boundBlock = FlowAnalysisPass.AppendImplicitReturn(boundBlock, lambdaSymbol);
			}
		}
		if (IsAsync && !ErrorFacts.PreventsSuccessfulDelegateConversion(instance) && (object)typeSymbol != null && typeSymbol.SpecialType != SpecialType.System_Void && !typeSymbol.IsNonGenericTaskType(this.binder.Compilation) && !typeSymbol.IsGenericTaskType(this.binder.Compilation))
		{
			instance.Add(ErrorCode.ERR_CantConvAsyncAnonFuncReturns, lambdaSymbol.DiagnosticLocation, lambdaSymbol.MessageID.Localize(), delegateType);
		}
		if (IsAsync)
		{
			SourceOrdinaryMethodSymbol.ReportAsyncParameterErrors(lambdaSymbol.Parameters, instance, lambdaSymbol.DiagnosticLocation);
		}
		return new BoundLambda(_unboundLambda.Syntax, boundBlock, instance.ToReadOnlyAndFree(), binder, delegateType, inferReturnType: false)
		{
			WasCompilerGenerated = _unboundLambda.WasCompilerGenerated
		};
	}

	private void ValidateUnsafeParameters(DiagnosticBag diagnostics, ImmutableArray<TypeSymbol> targetParameterTypes)
	{
		if (!HasSignature)
		{
			return;
		}
		int num = Math.Min(targetParameterTypes.Length, ParameterCount);
		for (int i = 0; i < num; i++)
		{
			if (targetParameterTypes[i].IsUnsafe())
			{
				binder.ReportUnsafeIfNotAllowed(ParameterLocation(i), diagnostics);
			}
		}
	}

	private BoundLambda ReallyInferReturnType(NamedTypeSymbol delegateType, ImmutableArray<TypeSymbol> parameterTypes, ImmutableArray<RefKind> parameterRefKinds)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		LambdaSymbol lambdaSymbol = new LambdaSymbol(binder.Compilation, binder.ContainingMemberOrLambda, _unboundLambda, parameterTypes, parameterRefKinds, Microsoft.CodeAnalysis.RefKind.None, null, instance);
		Binder lambdaBodyBinder = new ExecutableCodeBinder(_unboundLambda.Syntax, lambdaSymbol, ParameterBinder(lambdaSymbol, binder));
		BoundBlock body = BindLambdaBody(lambdaSymbol, lambdaBodyBinder, instance);
		BoundLambda boundLambda = new BoundLambda(_unboundLambda.Syntax, body, instance.ToReadOnlyAndFree(), lambdaBodyBinder, delegateType, inferReturnType: true)
		{
			WasCompilerGenerated = _unboundLambda.WasCompilerGenerated
		};
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		TypeSymbol inferredReturnType = boundLambda.InferredReturnType(ref useSiteDiagnostics) ?? LambdaSymbol.InferenceFailureReturnType;
		lambdaSymbol.SetInferredReturnType(boundLambda.RefKind, inferredReturnType);
		return boundLambda;
	}

	public BoundLambda BindForReturnTypeInference(NamedTypeSymbol delegateType)
	{
		ReturnInferenceCacheKey returnInferenceCacheKey = ReturnInferenceCacheKey.Create(delegateType, IsAsync);
		if (!_returnInferenceCache.TryGetValue(returnInferenceCacheKey, out var value))
		{
			value = ReallyInferReturnType(delegateType, returnInferenceCacheKey.ParameterTypes, returnInferenceCacheKey.ParameterRefKinds);
			return ImmutableInterlocked.GetOrAdd(ref _returnInferenceCache, returnInferenceCacheKey, value);
		}
		return value;
	}

	public TypeSymbol InferReturnType(NamedTypeSymbol delegateType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return BindForReturnTypeInference(delegateType).InferredReturnType(ref useSiteDiagnostics);
	}

	public virtual Binder ParameterBinder(LambdaSymbol lambdaSymbol, Binder binder)
	{
		return new WithLambdaParametersBinder(lambdaSymbol, binder);
	}

	public BoundLambda BindForErrorRecovery()
	{
		if (_errorBinding == null)
		{
			Interlocked.CompareExchange(ref _errorBinding, ReallyBindForErrorRecovery(), null);
		}
		return _errorBinding;
	}

	private BoundLambda ReallyBindForErrorRecovery()
	{
		return GuessBestBoundLambda(_bindingCache) ?? GuessBestBoundLambda(_returnInferenceCache) ?? ReallyInferReturnType(null, ImmutableArray<TypeSymbol>.Empty, ImmutableArray<Microsoft.CodeAnalysis.RefKind>.Empty);
	}

	private static BoundLambda GuessBestBoundLambda<T>(ImmutableDictionary<T, BoundLambda> candidates)
	{
		return candidates.Count switch
		{
			0 => null, 
			1 => candidates.First().Value, 
			_ => (from lambda in (from lambda in candidates
					group lambda by lambda.Value.Diagnostics.Length into @group
					orderby @group.Key
					select @group).First()
				orderby GetLambdaSortString(lambda.Value.Symbol)
				select lambda).FirstOrDefault().Value, 
		};
	}

	private static string GetLambdaSortString(LambdaSymbol lambda)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		foreach (ParameterSymbol parameter in lambda.Parameters)
		{
			instance.Builder.Append(parameter.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
		}
		if ((object)lambda.ReturnType != null)
		{
			instance.Builder.Append(lambda.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
		}
		return instance.ToStringAndFree();
	}

	public bool GenerateSummaryErrors(DiagnosticBag diagnostics)
	{
		IEnumerable<ImmutableArray<Diagnostic>> first = _bindingCache.Select((KeyValuePair<NamedTypeSymbol, BoundLambda> boundLambda) =>
		{
			KeyValuePair<NamedTypeSymbol, BoundLambda> keyValuePair = boundLambda;
			return keyValuePair.Value.Diagnostics;
		});
		IEnumerable<ImmutableArray<Diagnostic>> second = _returnInferenceCache.Values.Select((BoundLambda boundLambda) => boundLambda.Diagnostics);
		IEnumerable<ImmutableArray<Diagnostic>> enumerable = first.Concat(second);
		FirstAmongEqualsSet<Diagnostic> firstAmongEqualsSet = null;
		foreach (ImmutableArray<Diagnostic> item in enumerable)
		{
			if (firstAmongEqualsSet == null)
			{
				firstAmongEqualsSet = CreateFirstAmongEqualsSet(item);
			}
			else
			{
				firstAmongEqualsSet.IntersectWith(item);
			}
		}
		if (firstAmongEqualsSet != null && PreventsSuccessfulDelegateConversion(firstAmongEqualsSet))
		{
			diagnostics.AddRange(firstAmongEqualsSet);
			return true;
		}
		FirstAmongEqualsSet<Diagnostic> firstAmongEqualsSet2 = null;
		foreach (ImmutableArray<Diagnostic> item2 in enumerable)
		{
			if (firstAmongEqualsSet2 == null)
			{
				firstAmongEqualsSet2 = CreateFirstAmongEqualsSet(item2);
			}
			else
			{
				firstAmongEqualsSet2.UnionWith(item2);
			}
		}
		if (firstAmongEqualsSet2 != null && PreventsSuccessfulDelegateConversion(firstAmongEqualsSet2))
		{
			diagnostics.AddRange(firstAmongEqualsSet2);
			return true;
		}
		return false;
	}

	private static bool PreventsSuccessfulDelegateConversion(FirstAmongEqualsSet<Diagnostic> set)
	{
		foreach (Diagnostic item in set)
		{
			if (ErrorFacts.PreventsSuccessfulDelegateConversion((ErrorCode)item.Code))
			{
				return true;
			}
		}
		return false;
	}

	private static FirstAmongEqualsSet<Diagnostic> CreateFirstAmongEqualsSet(ImmutableArray<Diagnostic> bag)
	{
		return new FirstAmongEqualsSet<Diagnostic>(bag, CommonDiagnosticComparer.Instance, CanonicallyCompareDiagnostics);
	}

	private static int CanonicallyCompareDiagnostics(Diagnostic x, Diagnostic y)
	{
		ErrorCode code = (ErrorCode)x.Code;
		ErrorCode code2 = (ErrorCode)y.Code;
		int num = code.CompareTo(code2);
		if (!ErrorFacts.PreventsSuccessfulDelegateConversion(code) || !ErrorFacts.PreventsSuccessfulDelegateConversion(code2))
		{
			return num;
		}
		if (num != 0)
		{
			return num;
		}
		return string.CompareOrdinal(x.ToString(), y.ToString());
	}
}
