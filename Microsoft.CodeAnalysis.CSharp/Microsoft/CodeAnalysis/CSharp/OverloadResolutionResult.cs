using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class OverloadResolutionResult<TMember> where TMember : Symbol
{
	private MemberResolutionResult<TMember> _bestResult;

	private ThreeState _bestResultState;

	internal readonly ArrayBuilder<MemberResolutionResult<TMember>> ResultsBuilder;

	private static readonly ObjectPool<OverloadResolutionResult<TMember>> s_pool = CreatePool();

	public bool Succeeded
	{
		get
		{
			EnsureBestResultLoaded();
			if (_bestResultState == ThreeState.True)
			{
				return _bestResult.Result.IsValid;
			}
			return false;
		}
	}

	public MemberResolutionResult<TMember> ValidResult
	{
		get
		{
			EnsureBestResultLoaded();
			return _bestResult;
		}
	}

	public MemberResolutionResult<TMember> BestResult
	{
		get
		{
			EnsureBestResultLoaded();
			return _bestResult;
		}
	}

	public ImmutableArray<MemberResolutionResult<TMember>> Results => ResultsBuilder.ToImmutable();

	internal bool HasAnyApplicableMember
	{
		get
		{
			foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
			{
				if (item.Result.IsApplicable)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal OverloadResolutionResult()
	{
		ResultsBuilder = new ArrayBuilder<MemberResolutionResult<TMember>>();
	}

	internal void Clear()
	{
		_bestResult = default;
		_bestResultState = ThreeState.Unknown;
		ResultsBuilder.Clear();
	}

	private void EnsureBestResultLoaded()
	{
		if (!_bestResultState.HasValue())
		{
			_bestResultState = TryGetBestResult(ResultsBuilder, out _bestResult);
		}
	}

	internal ImmutableArray<TMember> GetAllApplicableMembers()
	{
		ArrayBuilder<TMember> instance = ArrayBuilder<TMember>.GetInstance();
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.IsApplicable)
			{
				instance.Add(item.Member);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private static ThreeState TryGetBestResult(ArrayBuilder<MemberResolutionResult<TMember>> allResults, out MemberResolutionResult<TMember> best)
	{
		best = default;
		ThreeState threeState = ThreeState.False;
		foreach (MemberResolutionResult<TMember> allResult in allResults)
		{
			if (allResult.Result.IsValid)
			{
				if (threeState == ThreeState.True)
				{
					best = default;
					return ThreeState.False;
				}
				threeState = ThreeState.True;
				best = allResult;
			}
		}
		return threeState;
	}

	internal void ReportDiagnostics<T>(Binder binder, Location location, SyntaxNode nodeOpt, DiagnosticBag diagnostics, string name, BoundExpression receiver, SyntaxNode invokedExpression, AnalyzedArguments arguments, ImmutableArray<T> memberGroup, NamedTypeSymbol typeContainingConstructor, NamedTypeSymbol delegateTypeBeingInvoked, CSharpSyntaxNode queryClause = null, bool isMethodGroupConversion = false, RefKind? returnRefKind = null, TypeSymbol delegateType = null) where T : Symbol
	{
		ImmutableArray<Symbol> symbols = StaticCast<Symbol>.From(memberGroup);
		if (HadAmbiguousBestMethods(diagnostics, symbols, location) || HadAmbiguousWorseMethods(diagnostics, symbols, location, queryClause != null, receiver, name) || HadLambdaConversionError(diagnostics, arguments) || HadStaticInstanceMismatch(diagnostics, symbols, invokedExpression?.GetLocation() ?? location, binder, receiver, nodeOpt) || (isMethodGroupConversion && returnRefKind.HasValue && HadReturnMismatch(location, diagnostics, returnRefKind.GetValueOrDefault(), delegateType)) || HadBadArguments(diagnostics, binder, name, arguments, symbols, location, binder.Flags, isMethodGroupConversion) || HadConstraintFailure(location, diagnostics) || HadConstructedParameterFailedConstraintCheck(binder.Conversions, binder.Compilation, diagnostics, location) || InaccessibleTypeArgument(diagnostics, symbols, location) || TypeInferenceFailed(binder, diagnostics, symbols, receiver, arguments, location, queryClause) || UseSiteError())
		{
			return;
		}
		bool flag = false;
		MemberResolutionResult<TMember> memberResolutionResult = default;
		MemberResolutionResult<TMember> firstUnsupported = default;
		MemberResolutionResult<TMember>[] array = new MemberResolutionResult<TMember>[6];
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			switch (item.Result.Kind)
			{
			case MemberResolutionKind.UnsupportedMetadata:
				if (memberResolutionResult.IsNull)
				{
					firstUnsupported = item;
				}
				break;
			case MemberResolutionKind.NoCorrespondingNamedParameter:
				if (array[3].IsNull || item.Result.BadArgumentsOpt[0] > array[3].Result.BadArgumentsOpt[0])
				{
					array[3] = item;
				}
				break;
			case MemberResolutionKind.NoCorrespondingParameter:
				if (array[4].IsNull)
				{
					array[4] = item;
				}
				break;
			case MemberResolutionKind.RequiredParameterMissing:
				if (array[1].IsNull)
				{
					array[1] = item;
				}
				else
				{
					flag = true;
				}
				break;
			case MemberResolutionKind.NameUsedForPositional:
				if (array[2].IsNull || item.Result.BadArgumentsOpt[0] > array[2].Result.BadArgumentsOpt[0])
				{
					array[2] = item;
				}
				break;
			case MemberResolutionKind.BadNonTrailingNamedArgument:
				if (array[5].IsNull || item.Result.BadArgumentsOpt[0] > array[5].Result.BadArgumentsOpt[0])
				{
					array[5] = item;
				}
				break;
			case MemberResolutionKind.DuplicateNamedArgument:
				if (array[0].IsNull || item.Result.BadArgumentsOpt[0] > array[0].Result.BadArgumentsOpt[0])
				{
					array[0] = item;
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(item.Result.Kind);
			}
		}
		MemberResolutionResult<TMember>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult2 = array2[i];
			if (memberResolutionResult2.IsNotNull)
			{
				memberResolutionResult = memberResolutionResult2;
				break;
			}
		}
		if (memberResolutionResult.IsNotNull)
		{
			if (!((memberResolutionResult.Result.Kind == MemberResolutionKind.RequiredParameterMissing) & flag) && !isMethodGroupConversion)
			{
				switch (memberResolutionResult.Result.Kind)
				{
				case MemberResolutionKind.NameUsedForPositional:
					ReportNameUsedForPositional(memberResolutionResult, diagnostics, arguments, symbols);
					return;
				case MemberResolutionKind.NoCorrespondingNamedParameter:
					ReportNoCorrespondingNamedParameter(memberResolutionResult, name, diagnostics, arguments, delegateTypeBeingInvoked, symbols);
					return;
				case MemberResolutionKind.RequiredParameterMissing:
					ReportMissingRequiredParameter(memberResolutionResult, diagnostics, delegateTypeBeingInvoked, symbols, location);
					return;
				case MemberResolutionKind.BadNonTrailingNamedArgument:
					ReportBadNonTrailingNamedArgument(memberResolutionResult, diagnostics, arguments, symbols);
					return;
				case MemberResolutionKind.DuplicateNamedArgument:
					ReportDuplicateNamedArgument(memberResolutionResult, diagnostics, arguments);
					return;
				}
			}
		}
		else if (firstUnsupported.IsNotNull)
		{
			ReportUnsupportedMetadata(location, diagnostics, symbols, firstUnsupported);
			return;
		}
		if (!isMethodGroupConversion)
		{
			ReportBadParameterCount(diagnostics, name, arguments, symbols, location, typeContainingConstructor, delegateTypeBeingInvoked);
		}
	}

	private static void ReportUnsupportedMetadata(Location location, DiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, MemberResolutionResult<TMember> firstUnsupported)
	{
		DiagnosticInfo useSiteDiagnostic = firstUnsupported.Member.GetUseSiteDiagnostic();
		useSiteDiagnostic = new DiagnosticInfoWithSymbols((ErrorCode)useSiteDiagnostic.Code, useSiteDiagnostic.Arguments, symbols);
		Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, location);
	}

	private bool UseSiteError()
	{
		if (GetFirstMemberKind(MemberResolutionKind.UseSiteError).IsNull)
		{
			return false;
		}
		return true;
	}

	private bool InaccessibleTypeArgument(DiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.InaccessibleTypeArgument);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_BadAccess, new object[1] { firstMemberKind.Member }, symbols), location);
		return true;
	}

	private bool HadStaticInstanceMismatch(DiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location, Binder binder, BoundExpression receiverOpt, SyntaxNode nodeOpt)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.StaticInstanceMismatch);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		Symbol member = firstMemberKind.Member;
		if (receiverOpt != null && receiverOpt.Kind == BoundKind.QueryClause)
		{
			diagnostics.Add(ErrorCode.ERR_QueryNoProvider, location, receiverOpt.Type, member.Name);
		}
		else if (binder.Flags.Includes(BinderFlags.CollectionInitializerAddMethod))
		{
			diagnostics.Add(ErrorCode.ERR_InitializerAddHasWrongSignature, location, member);
		}
		else if (nodeOpt != null && nodeOpt.Kind() == SyntaxKind.AwaitExpression && member.Name == "GetAwaiter")
		{
			diagnostics.Add(ErrorCode.ERR_BadAwaitArg, location, receiverOpt.Type);
		}
		else
		{
			ErrorCode errorCode;
			if (member.IsStatic)
			{
				errorCode = ErrorCode.ERR_ObjectProhibited;
			}
			else
			{
				errorCode = ((Binder.WasImplicitReceiver(receiverOpt) && binder.InFieldInitializer && !binder.BindingTopLevelScriptCode) ? ErrorCode.ERR_FieldInitRefNonstatic : ErrorCode.ERR_ObjectRequired);
			}
			diagnostics.Add(new DiagnosticInfoWithSymbols(errorCode, new object[1] { member }, symbols), location);
		}
		return true;
	}

	private bool HadReturnMismatch(Location location, DiagnosticBag diagnostics, RefKind refKind, TypeSymbol delegateType)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.WrongRefKind);
		if (!firstMemberKind.IsNull)
		{
			diagnostics.Add(ErrorCode.ERR_DelegateRefMismatch, location, firstMemberKind.Member, delegateType);
			return true;
		}
		firstMemberKind = GetFirstMemberKind(MemberResolutionKind.WrongReturnType);
		if (!firstMemberKind.IsNull)
		{
			MethodSymbol methodSymbol = (MethodSymbol)(object)firstMemberKind.Member;
			diagnostics.Add(ErrorCode.ERR_BadRetType, location, methodSymbol, methodSymbol.ReturnType);
			return true;
		}
		return false;
	}

	private bool HadConstraintFailure(Location location, DiagnosticBag diagnostics)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.ConstraintFailure);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		foreach (TypeParameterDiagnosticInfo constraintFailureDiagnostic in firstMemberKind.Result.ConstraintFailureDiagnostics)
		{
			diagnostics.Add(new CSDiagnostic(constraintFailureDiagnostic.DiagnosticInfo, location));
		}
		return true;
	}

	private bool TypeInferenceFailed(Binder binder, DiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, BoundExpression receiver, AnalyzedArguments arguments, Location location, CSharpSyntaxNode queryClause = null)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.TypeInferenceFailed);
		if (firstMemberKind.IsNotNull)
		{
			if (queryClause != null)
			{
				Binder.ReportQueryInferenceFailed(queryClause, firstMemberKind.Member.Name, receiver, arguments, symbols, diagnostics);
			}
			else
			{
				diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_CantInferMethTypeArgs, new object[1] { firstMemberKind.Member }, symbols), location);
			}
			return true;
		}
		firstMemberKind = GetFirstMemberKind(MemberResolutionKind.TypeInferenceExtensionInstanceArgument);
		if (firstMemberKind.IsNotNull)
		{
			BoundExpression boundExpression = arguments.Arguments[0];
			if (queryClause != null)
			{
				binder.ReportQueryLookupFailed(queryClause, boundExpression, firstMemberKind.Member.Name, symbols, diagnostics);
			}
			else
			{
				diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_NoSuchMemberOrExtension, new object[2]
				{
					boundExpression.Type,
					firstMemberKind.Member.Name
				}, symbols), location);
			}
			return true;
		}
		return false;
	}

	private static void ReportNameUsedForPositional(MemberResolutionResult<TMember> bad, DiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols)
	{
		int index = bad.Result.BadArgumentsOpt[0];
		IdentifierNameSyntax identifierNameSyntax = arguments.Names[index];
		Location location = new SourceLocation(identifierNameSyntax);
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_NamedArgumentUsedInPositional, new object[1] { identifierNameSyntax.Identifier.ValueText }, symbols), location);
	}

	private static void ReportBadNonTrailingNamedArgument(MemberResolutionResult<TMember> bad, DiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols)
	{
		int index = bad.Result.BadArgumentsOpt[0];
		IdentifierNameSyntax identifierNameSyntax = arguments.Names[index];
		Location location = new SourceLocation(identifierNameSyntax);
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_BadNonTrailingNamedArgument, new object[1] { identifierNameSyntax.Identifier.ValueText }, symbols), location);
	}

	private void ReportDuplicateNamedArgument(MemberResolutionResult<TMember> result, DiagnosticBag diagnostics, AnalyzedArguments arguments)
	{
		IdentifierNameSyntax identifierNameSyntax = arguments.Names[result.Result.BadArgumentsOpt[0]];
		diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_DuplicateNamedArgument, identifierNameSyntax.Identifier.Text), identifierNameSyntax.Location);
	}

	private static void ReportNoCorrespondingNamedParameter(MemberResolutionResult<TMember> bad, string methodName, DiagnosticBag diagnostics, AnalyzedArguments arguments, NamedTypeSymbol delegateTypeBeingInvoked, ImmutableArray<Symbol> symbols)
	{
		int index = bad.Result.BadArgumentsOpt[0];
		IdentifierNameSyntax identifierNameSyntax = arguments.Names[index];
		Location location = new SourceLocation(identifierNameSyntax);
		ErrorCode errorCode = (((object)delegateTypeBeingInvoked != null) ? ErrorCode.ERR_BadNamedArgumentForDelegateInvoke : ErrorCode.ERR_BadNamedArgument);
		object obj = ((object)delegateTypeBeingInvoked) ?? ((object)methodName);
		diagnostics.Add(new DiagnosticInfoWithSymbols(errorCode, new object[2]
		{
			obj,
			identifierNameSyntax.Identifier.ValueText
		}, symbols), location);
	}

	private static void ReportMissingRequiredParameter(MemberResolutionResult<TMember> bad, DiagnosticBag diagnostics, NamedTypeSymbol delegateTypeBeingInvoked, ImmutableArray<Symbol> symbols, Location location)
	{
		TMember member = bad.Member;
		ImmutableArray<ParameterSymbol> parameters = member.GetParameters();
		int badParameter = bad.Result.BadParameter;
		string text = ((badParameter != parameters.Length) ? parameters[badParameter].Name : SyntaxFacts.GetText(SyntaxKind.ArgListKeyword));
		object obj = ((object)delegateTypeBeingInvoked) ?? ((object)member);
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_NoCorrespondingArgument, new object[2] { text, obj }, symbols), location);
	}

	private static void ReportBadParameterCount(DiagnosticBag diagnostics, string name, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, Location location, NamedTypeSymbol typeContainingConstructor, NamedTypeSymbol delegateTypeBeingInvoked)
	{
		ErrorCode errorCode;
		if ((object)typeContainingConstructor != null)
		{
			errorCode = ErrorCode.ERR_BadCtorArgCount;
		}
		else
		{
			errorCode = (((object)delegateTypeBeingInvoked != null) ? ErrorCode.ERR_BadDelArgCount : ErrorCode.ERR_BadArgCount);
		}
		object obj = typeContainingConstructor ?? ((object)delegateTypeBeingInvoked) ?? ((object)name);
		int num = arguments.Arguments.Count;
		if (arguments.IsExtensionMethodInvocation)
		{
			num--;
		}
		diagnostics.Add(new DiagnosticInfoWithSymbols(errorCode, new object[2] { obj, num }, symbols), location);
	}

	private bool HadConstructedParameterFailedConstraintCheck(ConversionsBase conversions, Compilation compilation, DiagnosticBag diagnostics, Location location)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.ConstructedParameterFailedConstraintCheck);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		MethodSymbol methodSymbol = (MethodSymbol)(object)firstMemberKind.Member;
		if (!methodSymbol.CheckConstraints(conversions, location, compilation, diagnostics))
		{
			return true;
		}
		methodSymbol.ParameterTypes[firstMemberKind.Result.BadParameter].CheckAllConstraints(conversions, location, diagnostics);
		return true;
	}

	private static bool HadLambdaConversionError(DiagnosticBag diagnostics, AnalyzedArguments arguments)
	{
		bool flag = false;
		foreach (BoundExpression argument in arguments.Arguments)
		{
			if (argument.Kind == BoundKind.UnboundLambda)
			{
				flag |= ((UnboundLambda)argument).GenerateSummaryErrors(diagnostics);
			}
		}
		return flag;
	}

	private bool HadBadArguments(DiagnosticBag diagnostics, Binder binder, string name, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, Location location, BinderFlags flags, bool isMethodGroupConversion)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.BadArgumentConversion);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		if (isMethodGroupConversion)
		{
			return true;
		}
		TMember member = firstMemberKind.Member;
		if (flags.Includes(BinderFlags.CollectionInitializerAddMethod))
		{
			foreach (ParameterSymbol parameter in member.GetParameters())
			{
				if (parameter.RefKind != RefKind.None)
				{
					diagnostics.Add(ErrorCode.ERR_InitializerAddHasParamModifiers, location, symbols, member);
					return true;
				}
			}
			diagnostics.Add(ErrorCode.ERR_BadArgTypesForCollectionAdd, location, symbols, member);
		}
		foreach (int item in firstMemberKind.Result.BadArgumentsOpt)
		{
			ReportBadArgumentError(diagnostics, binder, name, arguments, symbols, location, firstMemberKind, member, item);
		}
		return true;
	}

	private void ReportBadArgumentError(DiagnosticBag diagnostics, Binder binder, string name, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, Location location, MemberResolutionResult<TMember> badArg, TMember method, int arg)
	{
		BoundExpression boundExpression = arguments.Argument(arg);
		if (boundExpression.HasAnyErrors)
		{
			return;
		}
		int num = badArg.Result.ParameterFromArgument(arg);
		SourceLocation location2 = new SourceLocation(boundExpression.Syntax);
		if (method.GetIsVararg() && num == method.GetParameterCount())
		{
			diagnostics.Add(ErrorCode.ERR_BadArgType, location2, symbols, arg + 1, boundExpression.Display, "__arglist");
			return;
		}
		ParameterSymbol parameterSymbol = method.GetParameters()[num];
		bool isLastParameter = method.GetParameterCount() == num + 1;
		RefKind refKind = arguments.RefKind(arg);
		RefKind refKind2 = parameterSymbol.RefKind;
		if (arguments.IsExtensionMethodThisArgument(arg) && (refKind2 == RefKind.Ref || refKind2 == RefKind.In))
		{
			refKind = refKind2;
		}
		if (!boundExpression.HasExpressionType() && boundExpression.Kind != BoundKind.OutDeconstructVarPendingInference && boundExpression.Kind != BoundKind.OutVariablePendingInference && boundExpression.Kind != BoundKind.DiscardExpression)
		{
			TypeSymbol typeSymbol = ((UnwrapIfParamsArray(parameterSymbol, isLastParameter) is TypeSymbol typeSymbol2) ? typeSymbol2 : parameterSymbol.Type);
			if (boundExpression.Kind == BoundKind.UnboundLambda && refKind == refKind2)
			{
				((UnboundLambda)boundExpression).GenerateAnonymousFunctionConversionError(diagnostics, typeSymbol);
			}
			else if (boundExpression.Kind != BoundKind.MethodGroup || typeSymbol.TypeKind != TypeKind.Delegate || !Conversions.ReportDelegateMethodGroupDiagnostics(binder, (BoundMethodGroup)boundExpression, typeSymbol, diagnostics))
			{
				diagnostics.Add(ErrorCode.ERR_BadArgType, location2, symbols, arg + 1, boundExpression.Display, UnwrapIfParamsArray(parameterSymbol, isLastParameter));
			}
		}
		else if (refKind != refKind2 && (refKind != RefKind.None || refKind2 != RefKind.In))
		{
			if (refKind2 == RefKind.None || refKind2 == RefKind.In)
			{
				diagnostics.Add(ErrorCode.ERR_BadArgExtraRef, location2, symbols, arg + 1, refKind.ToArgumentDisplayString());
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_BadArgRef, location2, symbols, arg + 1, refKind2.ToParameterDisplayString());
			}
		}
		else if (arguments.IsExtensionMethodThisArgument(arg))
		{
			diagnostics.Add(ErrorCode.ERR_BadInstanceArgType, location2, symbols, boundExpression.Display, name, method, parameterSymbol);
		}
		else if (boundExpression.Display is TypeSymbol type)
		{
			SignatureOnlyParameterSymbol symbol = new SignatureOnlyParameterSymbol(type, ImmutableArray<CustomModifier>.Empty, ImmutableArray<CustomModifier>.Empty, isParams: false, refKind);
			SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(binder.Compilation, symbol, UnwrapIfParamsArray(parameterSymbol, isLastParameter));
			diagnostics.Add(ErrorCode.ERR_BadArgType, location2, symbols, arg + 1, symbolDistinguisher.First, symbolDistinguisher.Second);
		}
		else
		{
			diagnostics.Add(ErrorCode.ERR_BadArgType, location2, symbols, arg + 1, boundExpression.Display, UnwrapIfParamsArray(parameterSymbol, isLastParameter));
		}
	}

	private static Symbol UnwrapIfParamsArray(ParameterSymbol parameter, bool isLastParameter)
	{
		if ((parameter.IsParams & isLastParameter) && parameter.Type is ArrayTypeSymbol { IsSZArray: not false } arrayTypeSymbol)
		{
			return arrayTypeSymbol.ElementType;
		}
		return parameter;
	}

	private bool HadAmbiguousWorseMethods(DiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location, bool isQuery, BoundExpression receiver, string name)
	{
		if (TryGetFirstTwoWorseResults(out var first, out var second) <= 1)
		{
			return false;
		}
		if (isQuery)
		{
			diagnostics.Add(ErrorCode.ERR_QueryMultipleProviders, location, receiver.Type, name);
		}
		else
		{
			diagnostics.Add(CreateAmbiguousCallDiagnosticInfo(first.LeastOverriddenMember.OriginalDefinition, second.LeastOverriddenMember.OriginalDefinition, symbols), location);
		}
		return true;
	}

	private int TryGetFirstTwoWorseResults(out MemberResolutionResult<TMember> first, out MemberResolutionResult<TMember> second)
	{
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		first = default;
		second = default;
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.Kind == MemberResolutionKind.Worse)
			{
				num++;
				if (!flag)
				{
					first = item;
					flag = true;
				}
				else if (!flag2)
				{
					second = item;
					flag2 = true;
				}
			}
		}
		return num;
	}

	private bool HadAmbiguousBestMethods(DiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location)
	{
		if (TryGetFirstTwoValidResults(out var first, out var second) <= 1)
		{
			return false;
		}
		diagnostics.Add(CreateAmbiguousCallDiagnosticInfo(first.LeastOverriddenMember.OriginalDefinition, second.LeastOverriddenMember.OriginalDefinition, symbols), location);
		return true;
	}

	private int TryGetFirstTwoValidResults(out MemberResolutionResult<TMember> first, out MemberResolutionResult<TMember> second)
	{
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		first = default;
		second = default;
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.IsValid)
			{
				num++;
				if (!flag)
				{
					first = item;
					flag = true;
				}
				else if (!flag2)
				{
					second = item;
					flag2 = true;
				}
			}
		}
		return num;
	}

	private static DiagnosticInfoWithSymbols CreateAmbiguousCallDiagnosticInfo(Symbol first, Symbol second, ImmutableArray<Symbol> symbols)
	{
		object[] arguments = ((!(first.ContainingNamespace != second.ContainingNamespace)) ? new object[2] { first, second } : new object[2]
		{
			new FormattedSymbol(first, SymbolDisplayFormat.CSharpErrorMessageFormat),
			new FormattedSymbol(second, SymbolDisplayFormat.CSharpErrorMessageFormat)
		});
		return new DiagnosticInfoWithSymbols(ErrorCode.ERR_AmbigCall, arguments, symbols);
	}

	[Conditional("DEBUG")]
	private void AssertNone(MemberResolutionKind kind)
	{
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.Kind == kind)
			{
				throw ExceptionUtilities.UnexpectedValue(kind);
			}
		}
	}

	private MemberResolutionResult<TMember> GetFirstMemberKind(MemberResolutionKind kind)
	{
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			if (item.Result.Kind == kind)
			{
				return item;
			}
		}
		return default;
	}

	internal static OverloadResolutionResult<TMember> GetInstance()
	{
		return s_pool.Allocate();
	}

	internal void Free()
	{
		Clear();
		s_pool.Free(this);
	}

	private static ObjectPool<OverloadResolutionResult<TMember>> CreatePool()
	{
		return new ObjectPool<OverloadResolutionResult<TMember>>(() => new OverloadResolutionResult<TMember>(), 10);
	}
}
