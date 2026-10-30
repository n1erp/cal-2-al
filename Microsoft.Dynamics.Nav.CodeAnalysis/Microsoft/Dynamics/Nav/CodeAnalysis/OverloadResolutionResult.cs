using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
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
			if (!_bestResultState.HasValue())
			{
				_bestResultState = TryGetBestResult(ResultsBuilder, out _bestResult);
			}
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
			DebugAssertHelper.Assert(_bestResultState == ThreeState.True && _bestResult.Result.IsValid);
			return _bestResult;
		}
	}

	public MemberResolutionResult<TMember> BestResult
	{
		get
		{
			DebugAssertHelper.Assert(_bestResultState == ThreeState.True);
			return _bestResult;
		}
	}

	private bool HasBestResult => _bestResultState.Value();

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
					DebugAssertHelper.Assert(condition: false, string.Format(CultureInfo.InvariantCulture, "How did we manage to get two methods in the overload resolution results that were both better than every other method?First: memberKind: {0}, resolutionKind: {1}, paramTypes:{2} . Second: memberKind: {3}, resolutionKind: {4}, paramTypes:{5}", best.Member.Kind, best.Resolution, best.Result.EffectiveParameters.ParameterTypes, allResult.Member.Kind, allResult.Resolution, allResult.Result.EffectiveParameters.ParameterTypes));
					best = default;
					return ThreeState.False;
				}
				threeState = ThreeState.True;
				best = allResult;
			}
		}
		return threeState;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal void ReportDiagnostics<T>(Location location, DiagnosticBag diagnostics, string name, AnalyzedArguments arguments, ImmutableArray<T> memberGroup, bool isMethodGroupConversion = false) where T : Symbol
	{
		DebugAssertHelper.Assert(!Succeeded, "Don't ask for diagnostic info on a successful overload resolution result.");
		ImmutableArray<Symbol> symbols = StaticCast<Symbol>.From(memberGroup);
		if (HadAmbiguousBestMethods(diagnostics, symbols, location) || HadAmbiguousWorseMethods(diagnostics, symbols, location) || HadBadArguments(diagnostics, arguments, symbols) || HadBadTypeFromFirstArguments(diagnostics, arguments, symbols) || HadBadMemberArguments(diagnostics, arguments, symbols))
		{
			return;
		}
		bool flag = false;
		MemberResolutionResult<TMember> bad = default;
		MemberResolutionResult<TMember> firstUnsupported = default;
		MemberResolutionResult<TMember>[] array = new MemberResolutionResult<TMember>[4];
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			switch (item.Result.Kind)
			{
			case MemberResolutionKind.NoCorrespondingParameter:
				if (array[3].IsNull)
				{
					array[3] = item;
				}
				break;
			case MemberResolutionKind.RequiredParameterMissing:
				if (array[0].IsNull)
				{
					DebugAssertHelper.Assert(!flag);
					array[0] = item;
				}
				else
				{
					flag = true;
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(item.Result.Kind);
			}
		}
		MemberResolutionResult<TMember>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = array2[i];
			if (memberResolutionResult.IsNotNull)
			{
				bad = memberResolutionResult;
				break;
			}
		}
		if (bad.IsNotNull)
		{
			if (!((bad.Result.Kind == MemberResolutionKind.RequiredParameterMissing) & flag) && !isMethodGroupConversion)
			{
				MemberResolutionKind kind = bad.Result.Kind;
				if (kind != MemberResolutionKind.NoCorrespondingParameter && kind == MemberResolutionKind.RequiredParameterMissing)
				{
					ReportMissingRequiredParameter(bad, diagnostics, symbols, location);
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
			ReportBadParameterCount(diagnostics, name, arguments, symbols, location);
		}
	}

	private static void ReportUnsupportedMetadata(Location location, DiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, MemberResolutionResult<TMember> firstUnsupported)
	{
		DiagnosticInfo useSiteDiagnostic = firstUnsupported.Member.GetUseSiteDiagnostic();
		DebugAssertHelper.Assert(useSiteDiagnostic != null);
		DebugAssertHelper.Assert(useSiteDiagnostic.Severity == DiagnosticSeverity.Error);
		useSiteDiagnostic = new DiagnosticInfoWithSymbols((ErrorCode)useSiteDiagnostic.Code, useSiteDiagnostic.Arguments, symbols);
		Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, location);
	}

	private static void ReportMissingRequiredParameter(MemberResolutionResult<TMember> bad, DiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location)
	{
		TMember member = bad.Member;
		ImmutableArray<ParameterSymbol> parameters = member.GetParameters();
		int badParameter = bad.Result.BadParameter;
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_NoCorrespondingArgument, new object[2]
		{
			new FormattedSymbol(parameters[badParameter], SymbolDisplayFormat.NameOnlyFormat),
			new FormattedSymbol(member, SymbolDisplayFormat.ErrorMessageFormat)
		}, symbols), location);
	}

	private static void ReportBadParameterCount(DiagnosticBag diagnostics, string name, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, Location location)
	{
		int count = arguments.Arguments.Count;
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_BadArgCount, new object[2] { name, count }, symbols), location);
	}

	private bool HadBadArguments(DiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.BadArguments);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		TMember member = firstMemberKind.Member;
		foreach (int item in firstMemberKind.Result.BadArgumentsOpt)
		{
			ReportBadArgumentError(diagnostics, arguments, symbols, member, item);
		}
		return true;
	}

	private static void ReportBadArgumentError(DiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, TMember method, int arg)
	{
		BoundExpression boundExpression = arguments.Argument(arg);
		SourceLocation location = new SourceLocation(boundExpression.Syntax);
		ImmutableArray<ParameterSymbol> parameters = method.GetParameters();
		int index = Math.Min(parameters.Length - 1, arg);
		ParameterSymbol parameterSymbol = parameters[index];
		TypeSymbol typeSymbol = boundExpression.Display as TypeSymbol;
		DebugAssertHelper.Assert((object)typeSymbol != null);
		diagnostics.Add(ErrorCode.ERR_BadArgType, location, symbols, arg + 1, typeSymbol.ToDisplayString(SymbolDisplayFormat.ShortFormat), parameterSymbol.ToDisplayString(SymbolDisplayFormat.ShortFormat));
	}

	private bool HadBadTypeFromFirstArguments(DiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.BadTypeFromFirstArguments);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		foreach (int item in firstMemberKind.Result.BadArgumentsOpt)
		{
			ReportBadTypeFromFirstArgumentError(diagnostics, arguments, symbols, item);
		}
		return true;
	}

	private static void ReportBadTypeFromFirstArgumentError(DiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, int arg)
	{
		BoundExpression boundExpression = arguments.Argument(arg);
		SourceLocation location = new SourceLocation(boundExpression.Syntax);
		BoundExpression boundExpression2 = arguments.Argument(0);
		TypeSymbol typeSymbol = boundExpression.Display as TypeSymbol;
		DebugAssertHelper.Assert((object)typeSymbol != null);
		diagnostics.Add(ErrorCode.ERR_BadArgTypeMustBeSameAsFirstArgument, location, symbols, arg + 1, typeSymbol, boundExpression2.Type);
	}

	private bool HadBadMemberArguments(DiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols)
	{
		MemberResolutionResult<TMember> firstMemberKind = GetFirstMemberKind(MemberResolutionKind.MemberRequired);
		if (firstMemberKind.IsNull)
		{
			return false;
		}
		TMember member = firstMemberKind.Member;
		foreach (int item in firstMemberKind.Result.BadArgumentsOpt)
		{
			ReportBadMemberArgumentError(diagnostics, arguments, symbols, member, item);
		}
		return true;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "method")]
	private static void ReportBadMemberArgumentError(DiagnosticBag diagnostics, AnalyzedArguments arguments, ImmutableArray<Symbol> symbols, TMember method, int arg)
	{
		SourceLocation location = new SourceLocation(arguments.Argument(arg).Syntax);
		diagnostics.Add(ErrorCode.ERR_BadArgMember, location, symbols, arg + 1);
	}

	private bool HadAmbiguousWorseMethods(DiagnosticBag diagnostics, ImmutableArray<Symbol> symbols, Location location)
	{
		int num = TryGetFirstTwoWorseResults(out var first, out var second);
		if (num <= 1)
		{
			DebugAssertHelper.Assert(num == 0, "How is it that there is exactly one applicable but worse method, and exactly zero applicable best methods?  What was better than this thing?");
			return false;
		}
		Symbol originalDefinition = first.Member.OriginalDefinition;
		Symbol originalDefinition2 = second.Member.OriginalDefinition;
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_AmbigCall, new object[2] { originalDefinition, originalDefinition2 }, symbols), location);
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
		int num = TryGetFirstTwoValidResults(out var first, out var second);
		if (num <= 1)
		{
			DebugAssertHelper.Assert(num == 0, "Why are we doing error reporting on an overload resolution problem that had one valid result?");
			return false;
		}
		Symbol originalDefinition = first.Member.OriginalDefinition;
		Symbol originalDefinition2 = second.Member.OriginalDefinition;
		diagnostics.Add(new DiagnosticInfoWithSymbols(ErrorCode.ERR_AmbigCall, new object[2] { originalDefinition, originalDefinition2 }, symbols), location);
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

	internal string GetDebuggerDisplay()
	{
		if (ResultsBuilder.Count == 0)
		{
			return "Overload resolution failed because the method group was empty.";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (Succeeded)
		{
			stringBuilder.AppendLine("Overload resolution succeeded and chose " + ValidResult.Member);
		}
		else if (ResultsBuilder.Count((MemberResolutionResult<TMember> x) => x.Result.IsValid) > 1)
		{
			stringBuilder.AppendLine("Overload resolution failed because of ambiguous possible best methods.");
		}
		stringBuilder.AppendLine("Detailed results:");
		foreach (MemberResolutionResult<TMember> item in ResultsBuilder)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "method: {0} reason: {1}\n", item.Member, item.Result.Kind);
		}
		return stringBuilder.ToString();
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
