using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LocalRewriter : BoundTreeRewriterWithStackGuard
{
	private enum ConditionalAccessLoweringKind
	{
		LoweredConditionalAccess,
		Ternary,
		TernaryCaptureReceiverByVal
	}

	private class DeconstructionSideEffects
	{
		internal ArrayBuilder<BoundExpression> init;

		internal ArrayBuilder<BoundExpression> deconstructions;

		internal ArrayBuilder<BoundExpression> conversions;

		internal ArrayBuilder<BoundExpression> assignments;

		internal static DeconstructionSideEffects GetInstance()
		{
			return new DeconstructionSideEffects
			{
				init = ArrayBuilder<BoundExpression>.GetInstance(),
				deconstructions = ArrayBuilder<BoundExpression>.GetInstance(),
				conversions = ArrayBuilder<BoundExpression>.GetInstance(),
				assignments = ArrayBuilder<BoundExpression>.GetInstance()
			};
		}

		internal void Consolidate()
		{
			init.AddRange(deconstructions);
			init.AddRange(conversions);
			init.AddRange(assignments);
			deconstructions.Free();
			conversions.Free();
			assignments.Free();
		}

		internal BoundExpression PopLast()
		{
			if (init.Count == 0)
			{
				return null;
			}
			BoundExpression result = init.Last();
			init.RemoveLast();
			return result;
		}

		internal ImmutableArray<BoundExpression> ToImmutableAndFree()
		{
			return init.ToImmutableAndFree();
		}
	}

	private enum EventAssignmentKind
	{
		Assignment,
		Addition,
		Subtraction
	}

	private class PatternSwitchLocalRewriter : DecisionTreeBuilder
	{
		private readonly LocalRewriter _localRewriter;

		private readonly HashSet<LocalSymbol> _declaredTempSet = new HashSet<LocalSymbol>();

		private readonly ArrayBuilder<LocalSymbol> _declaredTemps = ArrayBuilder<LocalSymbol>.GetInstance();

		private readonly SyntheticBoundNodeFactory _factory;

		private readonly Dictionary<SyntaxNode, ArrayBuilder<BoundStatement>> _switchSections = new Dictionary<SyntaxNode, ArrayBuilder<BoundStatement>>();

		private ArrayBuilder<BoundStatement> _loweredDecisionTree = ArrayBuilder<BoundStatement>.GetInstance();

		private PatternSwitchLocalRewriter(LocalRewriter localRewriter, BoundPatternSwitchStatement node)
			: base(localRewriter._factory.CurrentFunction, (SwitchStatementSyntax)node.Syntax, localRewriter._factory.Compilation.Conversions)
		{
			_localRewriter = localRewriter;
			_factory = localRewriter._factory;
			_factory.Syntax = node.Syntax;
			foreach (BoundPatternSwitchSection switchSection in node.SwitchSections)
			{
				_switchSections.Add(switchSection.Syntax, ArrayBuilder<BoundStatement>.GetInstance());
			}
		}

		internal static BoundStatement MakeLoweredForm(LocalRewriter localRewriter, BoundPatternSwitchStatement node)
		{
			return new PatternSwitchLocalRewriter(localRewriter, node).MakeLoweredForm(node);
		}

		private BoundStatement MakeLoweredForm(BoundPatternSwitchStatement node)
		{
			BoundExpression boundExpression = _localRewriter.VisitExpression(node.Expression);
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
			if (!node.WasCompilerGenerated && _localRewriter.Instrument)
			{
				BoundExpression boundExpression2 = _localRewriter._instrumenter.InstrumentSwitchStatementExpression(node, boundExpression, _factory);
				if (boundExpression.ConstantValue == null)
				{
					boundExpression = boundExpression2;
				}
				else
				{
					instance.Add(_factory.ExpressionStatement(boundExpression2));
				}
			}
			LowerPatternSwitch(boundExpression, node, instance);
			if (!node.IsComplete)
			{
				instance.Add(_factory.Goto(node.BreakLabel));
			}
			_declaredTemps.AddRange(node.InnerLocals);
			foreach (BoundPatternSwitchSection switchSection in node.SwitchSections)
			{
				_declaredTemps.AddRange(switchSection.Locals);
				SyntaxNode syntax = switchSection.Syntax;
				ArrayBuilder<BoundStatement> arrayBuilder = _switchSections[syntax];
				foreach (BoundPatternSwitchLabel switchLabel in switchSection.SwitchLabels)
				{
					arrayBuilder.Add(_factory.Label(switchLabel.Label));
				}
				arrayBuilder.AddRange(_localRewriter.VisitList(switchSection.Statements));
				arrayBuilder.Add(_factory.Goto(node.BreakLabel));
				ImmutableArray<BoundStatement> statements = arrayBuilder.ToImmutableAndFree();
				if (switchSection.Locals.IsEmpty)
				{
					instance.Add(_factory.StatementList(statements));
				}
				else
				{
					instance.Add(new BoundScope(switchSection.Syntax, switchSection.Locals, statements));
				}
			}
			instance.Add(_factory.Label(node.BreakLabel));
			BoundStatement boundStatement = _factory.Block(_declaredTemps.ToImmutableArray(), node.InnerLocalFunctions, instance.ToImmutableAndFree());
			if (!node.WasCompilerGenerated && _localRewriter.Instrument)
			{
				boundStatement = _localRewriter._instrumenter.InstrumentPatternSwitchStatement(node, boundStatement);
			}
			return boundStatement;
		}

		private void LowerPatternSwitch(BoundExpression loweredExpression, BoundPatternSwitchStatement node, ArrayBuilder<BoundStatement> loweredDecisionTree)
		{
			DecisionTree decisionTree = LowerToDecisionTree(loweredExpression, node);
			LowerDecisionTree(loweredExpression, decisionTree, loweredDecisionTree);
		}

		private DecisionTree LowerToDecisionTree(BoundExpression loweredExpression, BoundPatternSwitchStatement node)
		{
			DecisionTree decisionTree = CreateEmptyDecisionTree(loweredExpression);
			BoundPatternSwitchLabel defaultLabel = null;
			SyntaxNode defaultSection = null;
			foreach (BoundPatternSwitchSection switchSection in node.SwitchSections)
			{
				SyntaxNode syntax = switchSection.Syntax;
				foreach (BoundPatternSwitchLabel switchLabel in switchSection.SwitchLabels)
				{
					BoundPatternSwitchLabel boundPatternSwitchLabel = LowerSwitchLabel(switchLabel);
					if (boundPatternSwitchLabel.Syntax.Kind() == SyntaxKind.DefaultSwitchLabel)
					{
						if (defaultLabel == null)
						{
							defaultLabel = boundPatternSwitchLabel;
							defaultSection = syntax;
						}
					}
					else
					{
						AddToDecisionTree(decisionTree, syntax, boundPatternSwitchLabel);
					}
				}
			}
			if (defaultLabel != null && !decisionTree.MatchIsComplete)
			{
				Add(decisionTree, (BoundExpression e, TypeSymbol t) => new DecisionTree.Guarded(loweredExpression, loweredExpression.Type, default, defaultSection, null, defaultLabel));
			}
			_useSiteDiagnostics.Clear();
			return decisionTree;
		}

		private BoundPatternSwitchLabel LowerSwitchLabel(BoundPatternSwitchLabel label)
		{
			return label.Update(label.Label, _localRewriter.LowerPattern(label.Pattern), _localRewriter.VisitExpression(label.Guard), label.IsReachable);
		}

		private void LowerDecisionTree(BoundExpression expression, DecisionTree decisionTree, ArrayBuilder<BoundStatement> loweredDecisionTree)
		{
			ArrayBuilder<BoundStatement> loweredDecisionTree2 = _loweredDecisionTree;
			_loweredDecisionTree = loweredDecisionTree;
			LowerDecisionTree(expression, decisionTree);
			_loweredDecisionTree = loweredDecisionTree2;
		}

		private void LowerDecisionTree(BoundExpression expression, DecisionTree decisionTree)
		{
			if (decisionTree == null)
			{
				return;
			}
			if (decisionTree.Temp != null)
			{
				if (decisionTree.Expression != expression)
				{
					BoundExpression right = _factory.Convert(decisionTree.Expression.Type, expression);
					_loweredDecisionTree.Add(_factory.Assignment(decisionTree.Expression, right));
				}
				if (_declaredTempSet.Add(decisionTree.Temp))
				{
					_declaredTemps.Add(decisionTree.Temp);
				}
			}
			switch (decisionTree.Kind)
			{
			case DecisionTree.DecisionKind.ByType:
				LowerDecisionTree((DecisionTree.ByType)decisionTree);
				break;
			case DecisionTree.DecisionKind.ByValue:
				LowerDecisionTree((DecisionTree.ByValue)decisionTree);
				break;
			case DecisionTree.DecisionKind.Guarded:
				LowerDecisionTree((DecisionTree.Guarded)decisionTree);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(decisionTree.Kind);
			}
		}

		private void LowerDecisionTree(DecisionTree.ByType byType)
		{
			bool? flag = byType.Expression.ConstantValue?.IsNull;
			GeneratedLabelSymbol label = _factory.GenerateLabel("byTypeDefault");
			if (byType.Type.CanContainNull())
			{
				if (flag.HasValue)
				{
					bool valueOrDefault = flag == true;
					if (valueOrDefault && valueOrDefault)
					{
						LowerDecisionTree(byType.Expression, byType.WhenNull);
						DecisionTree whenNull = byType.WhenNull;
						if (whenNull == null || !whenNull.MatchIsComplete)
						{
							_loweredDecisionTree.Add(_factory.Goto(label));
						}
					}
				}
				else
				{
					GeneratedLabelSymbol label2 = _factory.GenerateLabel("notNull");
					_ = byType.Expression;
					NamedTypeSymbol type = _factory.SpecialType(SpecialType.System_Object);
					BoundExpression boundExpression = _factory.Null(type);
					BoundExpression condition = (byType.Type.IsNullableType() ? _localRewriter.RewriteNullableNullEquality(_factory.Syntax, BinaryOperatorKind.NullableNullNotEqual, byType.Expression, boundExpression, _factory.SpecialType(SpecialType.System_Boolean)) : _factory.ObjectNotEqual(boundExpression, _factory.Convert(type, byType.Expression)));
					_loweredDecisionTree.Add(_factory.ConditionalGoto(condition, label2, jumpIfTrue: true));
					LowerDecisionTree(byType.Expression, byType.WhenNull);
					DecisionTree whenNull2 = byType.WhenNull;
					if (whenNull2 == null || !whenNull2.MatchIsComplete)
					{
						_loweredDecisionTree.Add(_factory.Goto(label));
					}
					_loweredDecisionTree.Add(_factory.Label(label2));
				}
			}
			if (flag != true)
			{
				foreach (KeyValuePair<TypeSymbol, DecisionTree> item in byType.TypeAndDecision)
				{
					_ = item.Key;
					DecisionTree value = item.Value;
					GeneratedLabelSymbol label3 = _factory.GenerateLabel("failedDecision");
					BoundExpression condition2 = TypeTestAndCopyToTemp(byType.Expression, value.Expression);
					_loweredDecisionTree.Add(_factory.ConditionalGoto(condition2, label3, jumpIfTrue: false));
					LowerDecisionTree(value.Expression, value);
					_loweredDecisionTree.Add(_factory.Label(label3));
				}
			}
			_loweredDecisionTree.Add(_factory.Label(label));
			LowerDecisionTree(byType.Expression, byType.Default);
		}

		private BoundExpression TypeTestAndCopyToTemp(BoundExpression input, BoundExpression temp)
		{
			if (input == temp)
			{
				return _factory.Literal(value: true);
			}
			return _localRewriter.MakeIsDeclarationPattern(_factory.Syntax, input, temp, requiresNullTest: false);
		}

		private void LowerDecisionTree(DecisionTree.ByValue byValue)
		{
			if (byValue.Expression.ConstantValue != null)
			{
				LowerConstantValueDecision(byValue);
			}
			else if (byValue.ValueAndDecision.Count == 0)
			{
				LowerDecisionTree(byValue.Expression, byValue.Default);
			}
			else if (byValue.Type.SpecialType == SpecialType.System_Boolean)
			{
				LowerBooleanSwitch(byValue);
			}
			else
			{
				LowerBasicSwitch(byValue);
			}
		}

		private void LowerConstantValueDecision(DecisionTree.ByValue byValue)
		{
			object value = byValue.Expression.ConstantValue.Value;
			if (byValue.ValueAndDecision.TryGetValue(value, out var value2))
			{
				LowerDecisionTree(byValue.Expression, value2);
				if (value2.MatchIsComplete)
				{
					return;
				}
			}
			LowerDecisionTree(byValue.Expression, byValue.Default);
		}

		private void AddBindingsForCase(ImmutableArray<KeyValuePair<BoundExpression, BoundExpression>> bindings, ArrayBuilder<BoundStatement> addBindings)
		{
			GeneratedLabelSymbol label = _factory.GenerateLabel("patternMatched");
			_loweredDecisionTree.Add(_factory.Goto(label));
			addBindings.Add(_factory.HiddenSequencePoint());
			addBindings.Add(_factory.Label(label));
			if (!bindings.IsDefaultOrEmpty)
			{
				foreach (KeyValuePair<BoundExpression, BoundExpression> item in bindings)
				{
					BoundExpression key = item.Key;
					BoundExpression value = item.Value;
					addBindings.Add(_factory.ExpressionStatement(_localRewriter.MakeStaticAssignmentOperator(_factory.Syntax, value, key, isRef: false, value.Type, used: false)));
				}
			}
		}

		private void LowerDecisionTree(DecisionTree.Guarded guarded)
		{
			ArrayBuilder<BoundStatement> arrayBuilder = _switchSections[guarded.SectionSyntax];
			LabelSymbol label = guarded.Label.Label;
			if (guarded.Guard == null || guarded.Guard.ConstantValue == ConstantValue.True)
			{
				if (guarded.Bindings.IsDefaultOrEmpty)
				{
					_loweredDecisionTree.Add(_factory.Goto(label));
					return;
				}
				AddBindingsForCase(guarded.Bindings, arrayBuilder);
				arrayBuilder.Add(_factory.Goto(label));
				return;
			}
			AddBindingsForCase(guarded.Bindings, arrayBuilder);
			BoundStatement boundStatement = _factory.ConditionalGoto(guarded.Guard, label, jumpIfTrue: true);
			if (!guarded.Guard.WasCompilerGenerated && _localRewriter.Instrument)
			{
				boundStatement = _localRewriter._instrumenter.InstrumentPatternSwitchWhenClauseConditionalGotoBody(guarded.Guard, boundStatement);
			}
			arrayBuilder.Add(boundStatement);
			GeneratedLabelSymbol label2 = _factory.GenerateLabel("guardFailed");
			arrayBuilder.Add(_factory.Goto(label2));
			_loweredDecisionTree.Add(_factory.Label(label2));
			LowerDecisionTree(guarded.Expression, guarded.Default);
		}

		private void LowerBasicSwitch(DecisionTree.ByValue byValue)
		{
			ArrayBuilder<BoundSwitchSection> instance = ArrayBuilder<BoundSwitchSection>.GetInstance();
			GeneratedLabelSymbol generatedLabelSymbol = _factory.GenerateLabel("noValueMatches");
			TypeSymbol typeSymbol = (byValue.Type.IsEnumType() ? byValue.Type.GetEnumUnderlyingType() : byValue.Type);
			foreach (KeyValuePair<object, DecisionTree> item3 in byValue.ValueAndDecision)
			{
				object key = item3.Key;
				DecisionTree value = item3.Value;
				ConstantValue constantValueOpt = ConstantValue.Create(key, typeSymbol.SpecialType);
				BoundLiteral expressionOpt = new BoundLiteral(_factory.Syntax, constantValueOpt, typeSymbol);
				GeneratedLabelSymbol label = _factory.GenerateLabel("case+" + key);
				BoundSwitchLabel item = new BoundSwitchLabel(_factory.Syntax, label, expressionOpt, constantValueOpt);
				ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
				LowerDecisionTree(byValue.Expression, value, instance2);
				if (!value.MatchIsComplete)
				{
					instance2.Add(_factory.Goto(generatedLabelSymbol));
				}
				BoundSwitchSection item2 = new BoundSwitchSection(_factory.Syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(item), instance2.ToImmutableAndFree());
				instance.Add(item2);
			}
			ImmutableArray<BoundSwitchSection> immutableArray = instance.ToImmutableAndFree();
			MethodSymbol stringEquality = null;
			if (typeSymbol.SpecialType == SpecialType.System_String)
			{
				_localRewriter.EnsureStringHashFunction(immutableArray, _factory.Syntax);
				stringEquality = _localRewriter.UnsafeGetSpecialTypeMethod(_factory.Syntax, SpecialMember.System_String__op_Equality);
			}
			GeneratedLabelSymbol constantTargetOpt = (immutableArray.IsEmpty ? generatedLabelSymbol : null);
			BoundSwitchStatement boundSwitchStatement = new BoundSwitchStatement(_factory.Syntax, null, _factory.Convert(typeSymbol, byValue.Expression), constantTargetOpt, ImmutableArray<LocalSymbol>.Empty, ImmutableArray<LocalFunctionSymbol>.Empty, immutableArray, generatedLabelSymbol, stringEquality);
			switch (typeSymbol.SpecialType)
			{
			case SpecialType.System_Boolean:
				throw ExceptionUtilities.Unreachable;
			case SpecialType.System_Char:
			case SpecialType.System_SByte:
			case SpecialType.System_Byte:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
			case SpecialType.System_Int32:
			case SpecialType.System_UInt32:
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
			case SpecialType.System_String:
				_loweredDecisionTree.Add(boundSwitchStatement);
				break;
			default:
				_loweredDecisionTree.Add(LowerNonprimitiveSwitch(boundSwitchStatement));
				break;
			}
			LowerDecisionTree(byValue.Expression, byValue.Default);
		}

		private BoundStatement LowerNonprimitiveSwitch(BoundSwitchStatement switchStatement)
		{
			ImmutableArray<BoundSwitchSection> switchSections = switchStatement.SwitchSections;
			BoundExpression expression = switchStatement.Expression;
			GeneratedLabelSymbol breakLabel = switchStatement.BreakLabel;
			LabelSymbol labelSymbol = null;
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
			foreach (BoundSwitchSection item in switchSections)
			{
				foreach (BoundSwitchLabel switchLabel in item.SwitchLabels)
				{
					if (labelSymbol != null)
					{
						instance.Add(_factory.Label(labelSymbol));
					}
					labelSymbol = _factory.GenerateLabel("failcase+" + item.SwitchLabels[0].ConstantValueOpt.Value);
					BoundExpression receiver = _localRewriter.MakeLiteral(_factory.Syntax, switchLabel.ConstantValueOpt, expression.Type);
					BoundExpression boundExpression = _factory.InstanceCall(receiver, "Equals", expression);
					if (!boundExpression.HasErrors && boundExpression.Type.SpecialType != SpecialType.System_Boolean)
					{
						BoundCall boundCall = (BoundCall)boundExpression;
						_factory.Diagnostics.Add(ErrorCode.ERR_BadRetType, switchLabel.Syntax.GetLocation(), boundCall.Method, boundCall.Type);
					}
					instance.Add(_factory.ConditionalGoto(boundExpression, switchLabel.Label, jumpIfTrue: true));
					instance.Add(_factory.Goto(labelSymbol));
				}
				foreach (BoundSwitchLabel switchLabel2 in item.SwitchLabels)
				{
					instance.Add(_factory.Label(switchLabel2.Label));
				}
				instance.Add(_factory.Block(item.Statements));
			}
			instance.Add(_factory.Label(labelSymbol));
			instance.Add(_factory.Label(breakLabel));
			return _factory.Block(instance.ToImmutableAndFree());
		}

		private void LowerBooleanSwitch(DecisionTree.ByValue byValue)
		{
			switch (byValue.ValueAndDecision.Count)
			{
			case 0:
				throw ExceptionUtilities.Unreachable;
			case 1:
			{
				bool flag = byValue.ValueAndDecision.TryGetValue(true, out var value3);
				if (!flag)
				{
					byValue.ValueAndDecision.TryGetValue(false, out value3);
				}
				GeneratedLabelSymbol label3 = _factory.GenerateLabel("on" + !flag);
				_loweredDecisionTree.Add(_factory.ConditionalGoto(byValue.Expression, label3, !flag));
				LowerDecisionTree(byValue.Expression, value3);
				_loweredDecisionTree.Add(_factory.Label(label3));
				LowerDecisionTree(byValue.Expression, byValue.Default);
				break;
			}
			case 2:
			{
				byValue.ValueAndDecision.TryGetValue(true, out var value);
				byValue.ValueAndDecision.TryGetValue(false, out var value2);
				GeneratedLabelSymbol label = _factory.GenerateLabel("tryAnother");
				GeneratedLabelSymbol label2 = _factory.GenerateLabel("onFalse");
				_loweredDecisionTree.Add(_factory.ConditionalGoto(byValue.Expression, label2, jumpIfTrue: false));
				LowerDecisionTree(byValue.Expression, value);
				_loweredDecisionTree.Add(_factory.Goto(label));
				_loweredDecisionTree.Add(_factory.Label(label2));
				LowerDecisionTree(byValue.Expression, value2);
				_loweredDecisionTree.Add(_factory.Label(label));
				LowerDecisionTree(byValue.Expression, byValue.Default);
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(byValue.ValueAndDecision.Count);
			}
		}
	}

	private readonly CSharpCompilation _compilation;

	private readonly SyntheticBoundNodeFactory _factory;

	private readonly SynthesizedSubmissionFields _previousSubmissionFields;

	private readonly bool _allowOmissionOfConditionalCalls;

	private readonly LoweredDynamicOperationFactory _dynamicFactory;

	private bool _sawLambdas;

	private bool _sawLocalFunctions;

	private bool _inExpressionLambda;

	private bool _sawAwait;

	private bool _sawAwaitInExceptionHandler;

	private readonly DiagnosticBag _diagnostics;

	private readonly Instrumenter _instrumenter;

	private readonly BoundStatement _rootStatement;

	private Dictionary<BoundValuePlaceholderBase, BoundExpression> _placeholderReplacementMapDoNotUseDirectly;

	private BoundExpression _currentConditionalAccessTarget;

	private int _currentConditionalAccessID;

	private Dictionary<BoundNode, HashSet<LabelSymbol>> _lazyUnmatchedLabelCache;

	private bool Instrument => !_inExpressionLambda;

	private PEModuleBuilder EmitModule => _factory.CompilationState.ModuleBuilderOpt;

	private bool IsLambdaOrExpressionBodiedMember
	{
		get
		{
			MethodSymbol currentFunction = _factory.CurrentFunction;
			if (currentFunction is LambdaSymbol)
			{
				return true;
			}
			return (currentFunction as SourceMemberMethodSymbol)?.IsExpressionBodied ?? (currentFunction as LocalFunctionSymbol)?.IsExpressionBodied ?? false;
		}
	}

	private LocalRewriter(CSharpCompilation compilation, MethodSymbol containingMethod, int containingMethodOrdinal, BoundStatement rootStatement, NamedTypeSymbol containingType, SyntheticBoundNodeFactory factory, SynthesizedSubmissionFields previousSubmissionFields, bool allowOmissionOfConditionalCalls, DiagnosticBag diagnostics, Instrumenter instrumenter)
	{
		_compilation = compilation;
		_factory = factory;
		_factory.CurrentFunction = containingMethod;
		_dynamicFactory = new LoweredDynamicOperationFactory(factory, containingMethodOrdinal);
		_previousSubmissionFields = previousSubmissionFields;
		_allowOmissionOfConditionalCalls = allowOmissionOfConditionalCalls;
		_diagnostics = diagnostics;
		_instrumenter = instrumenter;
		_rootStatement = rootStatement;
	}

	public static BoundStatement Rewrite(CSharpCompilation compilation, MethodSymbol method, int methodOrdinal, NamedTypeSymbol containingType, BoundStatement statement, TypeCompilationState compilationState, SynthesizedSubmissionFields previousSubmissionFields, bool allowOmissionOfConditionalCalls, bool instrumentForDynamicAnalysis, ref ImmutableArray<SourceSpan> dynamicAnalysisSpans, DebugDocumentProvider debugDocumentProvider, DiagnosticBag diagnostics, out bool sawLambdas, out bool sawLocalFunctions, out bool sawAwaitInExceptionHandler)
	{
		try
		{
			SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(method, statement.Syntax, compilationState, diagnostics);
			DynamicAnalysisInjector dynamicAnalysisInjector = (instrumentForDynamicAnalysis ? DynamicAnalysisInjector.TryCreate(method, statement, syntheticBoundNodeFactory, diagnostics, debugDocumentProvider, Instrumenter.NoOp) : null);
			LocalRewriter localRewriter = new LocalRewriter(compilation, method, methodOrdinal, statement, containingType, syntheticBoundNodeFactory, previousSubmissionFields, allowOmissionOfConditionalCalls, diagnostics, (dynamicAnalysisInjector != null) ? new DebugInfoInjector(dynamicAnalysisInjector) : DebugInfoInjector.Singleton);
			BoundStatement result = (BoundStatement)localRewriter.Visit(statement);
			sawLambdas = localRewriter._sawLambdas;
			sawLocalFunctions = localRewriter._sawLocalFunctions;
			sawAwaitInExceptionHandler = localRewriter._sawAwaitInExceptionHandler;
			if (dynamicAnalysisInjector != null)
			{
				dynamicAnalysisSpans = dynamicAnalysisInjector.DynamicAnalysisSpans;
			}
			return result;
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			sawLambdas = (sawLocalFunctions = (sawAwaitInExceptionHandler = false));
			return new BoundBadStatement(statement.Syntax, ImmutableArray.Create((BoundNode)statement), hasErrors: true);
		}
	}

	public override BoundNode Visit(BoundNode node)
	{
		if (node == null)
		{
			return node;
		}
		if (node is BoundExpression node2)
		{
			return VisitExpressionImpl(node2);
		}
		return node.Accept(this);
	}

	private BoundExpression VisitExpression(BoundExpression node)
	{
		if (node == null)
		{
			return node;
		}
		return VisitExpressionImpl(node);
	}

	private BoundStatement VisitStatement(BoundStatement node)
	{
		if (node == null)
		{
			return node;
		}
		return (BoundStatement)node.Accept(this);
	}

	private BoundExpression VisitExpressionImpl(BoundExpression node)
	{
		ConstantValue constantValue = node.ConstantValue;
		if (constantValue != null)
		{
			TypeSymbol type = node.Type;
			if ((object)type == null || !type.IsNullableType())
			{
				return MakeLiteral(node.Syntax, constantValue, type);
			}
		}
		BoundExpression boundExpression = VisitExpressionWithStackGuard(node);
		if (boundExpression != null && boundExpression != node && !CanBePassedByReference(node) && CanBePassedByReference(boundExpression))
		{
			boundExpression = RefAccessMustMakeCopy(boundExpression);
		}
		return boundExpression;
	}

	private static BoundExpression RefAccessMustMakeCopy(BoundExpression visited)
	{
		visited = new BoundPassByCopy(visited.Syntax, visited, visited.Type);
		return visited;
	}

	private static bool IsUnusedDeconstruction(BoundExpression node)
	{
		if (node.Kind == BoundKind.DeconstructionAssignmentOperator)
		{
			return !((BoundDeconstructionAssignmentOperator)node).IsUsed;
		}
		return false;
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		_sawLambdas = true;
		CheckRefReadOnlySymbols(node.Symbol);
		MethodSymbol currentFunction = _factory.CurrentFunction;
		try
		{
			_factory.CurrentFunction = node.Symbol;
			return base.VisitLambda(node);
		}
		finally
		{
			_factory.CurrentFunction = currentFunction;
		}
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		_sawLocalFunctions = true;
		CheckRefReadOnlySymbols(node.Symbol);
		if (node.Symbol.TypeParameters.Any((TypeParameterSymbol typeParameter) => typeParameter.HasUnmanagedTypeConstraint))
		{
			_factory.CompilationState.ModuleBuilderOpt?.EnsureIsUnmanagedAttributeExists();
		}
		MethodSymbol currentFunction = _factory.CurrentFunction;
		try
		{
			_factory.CurrentFunction = node.Symbol;
			return base.VisitLocalFunctionStatement(node);
		}
		finally
		{
			_factory.CurrentFunction = currentFunction;
		}
	}

	public override BoundNode VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node)
	{
		return PlaceholderReplacement(node);
	}

	private BoundExpression PlaceholderReplacement(BoundValuePlaceholderBase placeholder)
	{
		return _placeholderReplacementMapDoNotUseDirectly[placeholder];
	}

	[Conditional("DEBUG")]
	private static void AssertPlaceholderReplacement(BoundValuePlaceholderBase placeholder, BoundExpression value)
	{
	}

	private void AddPlaceholderReplacement(BoundValuePlaceholderBase placeholder, BoundExpression value)
	{
		if (_placeholderReplacementMapDoNotUseDirectly == null)
		{
			_placeholderReplacementMapDoNotUseDirectly = new Dictionary<BoundValuePlaceholderBase, BoundExpression>();
		}
		_placeholderReplacementMapDoNotUseDirectly.Add(placeholder, value);
	}

	private void RemovePlaceholderReplacement(BoundValuePlaceholderBase placeholder)
	{
		_placeholderReplacementMapDoNotUseDirectly.Remove(placeholder);
	}

	public sealed override BoundNode VisitOutDeconstructVarPendingInference(OutDeconstructVarPendingInference node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override BoundNode VisitDeconstructionVariablePendingInference(DeconstructionVariablePendingInference node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		return node;
	}

	private static BoundExpression BadExpression(BoundExpression node)
	{
		return BadExpression(node.Syntax, node.Type, ImmutableArray.Create(node));
	}

	private static BoundExpression BadExpression(SyntaxNode syntax, TypeSymbol resultType, BoundExpression child)
	{
		return BadExpression(syntax, resultType, ImmutableArray.Create(child));
	}

	private static BoundExpression BadExpression(SyntaxNode syntax, TypeSymbol resultType, BoundExpression child1, BoundExpression child2)
	{
		return BadExpression(syntax, resultType, ImmutableArray.Create(child1, child2));
	}

	private static BoundExpression BadExpression(SyntaxNode syntax, TypeSymbol resultType, ImmutableArray<BoundExpression> children)
	{
		return new BoundBadExpression(syntax, LookupResultKind.NotReferencable, ImmutableArray<Symbol>.Empty, children, resultType);
	}

	private bool TryGetWellKnownTypeMember<TSymbol>(SyntaxNode syntax, WellKnownMember member, out TSymbol symbol, bool isOptional = false) where TSymbol : Symbol
	{
		symbol = (TSymbol)Binder.GetWellKnownTypeMember(_compilation, member, _diagnostics, null, syntax, isOptional);
		return (object)symbol != null;
	}

	private MethodSymbol UnsafeGetSpecialTypeMethod(SyntaxNode syntax, SpecialMember specialMember)
	{
		return UnsafeGetSpecialTypeMethod(syntax, specialMember, _compilation, _diagnostics);
	}

	private static MethodSymbol UnsafeGetSpecialTypeMethod(SyntaxNode syntax, SpecialMember specialMember, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		if (TryGetSpecialTypeMethod(syntax, specialMember, compilation, diagnostics, out var method))
		{
			return method;
		}
		MemberDescriptor descriptor = SpecialMembers.GetDescriptor(specialMember);
		SpecialType type = (SpecialType)descriptor.DeclaringTypeId;
		NamedTypeSymbol specialType = compilation.Assembly.GetSpecialType(type);
		TypeSymbol returnType = new ExtendedErrorTypeSymbol(compilation, descriptor.Name, descriptor.Arity, null);
		return new ErrorMethodSymbol(specialType, returnType, "Missing");
	}

	private bool TryGetSpecialTypeMethod(SyntaxNode syntax, SpecialMember specialMember, out MethodSymbol method)
	{
		return TryGetSpecialTypeMethod(syntax, specialMember, _compilation, _diagnostics, out method);
	}

	private static bool TryGetSpecialTypeMethod(SyntaxNode syntax, SpecialMember specialMember, CSharpCompilation compilation, DiagnosticBag diagnostics, out MethodSymbol method)
	{
		return Binder.TryGetSpecialTypeMember<MethodSymbol>(compilation, specialMember, syntax, diagnostics, out method);
	}

	public override BoundNode VisitTypeOfOperator(BoundTypeOfOperator node)
	{
		BoundTypeExpression sourceType = (BoundTypeExpression)Visit(node.SourceType);
		TypeSymbol type = VisitType(node.Type);
		if (!TryGetWellKnownTypeMember<MethodSymbol>(node.Syntax, WellKnownMember.System_Type__GetTypeFromHandle, out var symbol))
		{
			return new BoundTypeOfOperator(node.Syntax, sourceType, null, type, hasErrors: true);
		}
		return node.Update(sourceType, symbol, type);
	}

	public override BoundNode VisitRefTypeOperator(BoundRefTypeOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		if (!TryGetWellKnownTypeMember<MethodSymbol>(node.Syntax, WellKnownMember.System_Type__GetTypeFromHandle, out var symbol))
		{
			return new BoundRefTypeOperator(node.Syntax, operand, null, type, hasErrors: true);
		}
		return node.Update(operand, symbol, type);
	}

	public override BoundNode VisitTypeOrInstanceInitializers(BoundTypeOrInstanceInitializers node)
	{
		ImmutableArray<BoundStatement> statements = node.Statements;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(node.Statements.Length);
		foreach (BoundStatement item in statements)
		{
			if (IsFieldOrPropertyInitializer(item))
			{
				if (item.Kind == BoundKind.Block)
				{
					BoundBlock boundBlock = (BoundBlock)item;
					instance.Add(boundBlock.Update(boundBlock.Locals, boundBlock.LocalFunctions, ImmutableArray.Create(RewriteExpressionStatement((BoundExpressionStatement)boundBlock.Statements.Single(), suppressInstrumentation: true))));
				}
				else
				{
					instance.Add(RewriteExpressionStatement((BoundExpressionStatement)item, suppressInstrumentation: true));
				}
			}
			else
			{
				instance.Add(VisitStatement(item));
			}
		}
		int num = 0;
		bool flag = _compilation.Options.OptimizationLevel == OptimizationLevel.Release;
		for (int i = 0; i < instance.Count; i++)
		{
			if (instance[i] == null || (flag && IsFieldOrPropertyInitializer(statements[i]) && ShouldOptimizeOutInitializer(instance[i])))
			{
				num++;
				if (!_factory.CurrentFunction.IsStatic)
				{
					instance[i] = null;
				}
			}
		}
		ImmutableArray<BoundStatement> statements2;
		if (num == instance.Count)
		{
			statements2 = ImmutableArray<BoundStatement>.Empty;
			instance.Free();
		}
		else
		{
			int num2 = 0;
			for (int j = 0; j < instance.Count; j++)
			{
				BoundStatement boundStatement = instance[j];
				if (boundStatement == null)
				{
					continue;
				}
				if (IsFieldOrPropertyInitializer(statements[j]))
				{
					BoundStatement boundStatement2 = statements[j];
					if (Instrument && !boundStatement2.WasCompilerGenerated)
					{
						boundStatement = _instrumenter.InstrumentFieldOrPropertyInitializer(boundStatement2, boundStatement);
					}
				}
				instance[num2] = boundStatement;
				num2++;
			}
			instance.Count = num2;
			statements2 = instance.ToImmutableAndFree();
		}
		return new BoundStatementList(node.Syntax, statements2, node.HasErrors);
	}

	internal static bool IsFieldOrPropertyInitializer(BoundStatement initializer)
	{
		SyntaxNode syntax = initializer.Syntax;
		if (syntax is ExpressionSyntax && syntax != null && syntax.Parent.Kind() == SyntaxKind.EqualsValueClause)
		{
			switch (syntax.Parent?.Parent.Kind())
			{
			case SyntaxKind.VariableDeclarator:
			case SyntaxKind.PropertyDeclaration:
			{
				BoundKind kind = initializer.Kind;
				if (kind != BoundKind.Block)
				{
					if (kind != BoundKind.ExpressionStatement)
					{
						break;
					}
				}
				else
				{
					BoundBlock boundBlock = (BoundBlock)initializer;
					if (boundBlock.Statements.Length != 1)
					{
						break;
					}
					initializer = boundBlock.Statements.First();
					if (initializer.Kind != BoundKind.ExpressionStatement)
					{
						break;
					}
				}
				return ((BoundExpressionStatement)initializer).Expression.Kind == BoundKind.AssignmentOperator;
			}
			}
		}
		return false;
	}

	private static bool ShouldOptimizeOutInitializer(BoundStatement initializer)
	{
		if (initializer.Kind != BoundKind.ExpressionStatement)
		{
			return false;
		}
		if (!(((BoundExpressionStatement)initializer).Expression is BoundAssignmentOperator boundAssignmentOperator))
		{
			return false;
		}
		FieldSymbol fieldSymbol = ((BoundFieldAccess)boundAssignmentOperator.Left).FieldSymbol;
		if (!fieldSymbol.IsStatic && fieldSymbol.ContainingType.IsStructType())
		{
			return false;
		}
		return boundAssignmentOperator.Right.IsDefaultValue();
	}

	internal static bool CanBePassedByReference(BoundExpression expr)
	{
		if (expr.ConstantValue != null)
		{
			return false;
		}
		switch (expr.Kind)
		{
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.RefValueOperator:
		case BoundKind.ArrayAccess:
		case BoundKind.ThisReference:
		case BoundKind.Local:
		case BoundKind.PseudoVariable:
		case BoundKind.Parameter:
		case BoundKind.DiscardExpression:
			return true;
		case BoundKind.DeconstructValuePlaceholder:
			return true;
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)expr;
			if (boundEventAccess.IsUsableAsField)
			{
				if (!boundEventAccess.EventSymbol.IsStatic)
				{
					return CanBePassedByReference(boundEventAccess.ReceiverOpt);
				}
				return true;
			}
			return false;
		}
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)expr;
			if (!boundFieldAccess.FieldSymbol.IsStatic)
			{
				return CanBePassedByReference(boundFieldAccess.ReceiverOpt);
			}
			return true;
		}
		case BoundKind.Sequence:
			return CanBePassedByReference(((BoundSequence)expr).Value);
		case BoundKind.AssignmentOperator:
			return ((BoundAssignmentOperator)expr).IsRef;
		case BoundKind.ConditionalOperator:
			return ((BoundConditionalOperator)expr).IsRef;
		case BoundKind.Call:
			return ((BoundCall)expr).Method.RefKind != RefKind.None;
		case BoundKind.PropertyAccess:
			return ((BoundPropertyAccess)expr).PropertySymbol.RefKind != RefKind.None;
		case BoundKind.IndexerAccess:
			return ((BoundIndexerAccess)expr).Indexer.RefKind != RefKind.None;
		default:
			return false;
		}
	}

	private void CheckRefReadOnlySymbols(MethodSymbol symbol)
	{
		if (symbol.ReturnsByRefReadonly || symbol.Parameters.Any((ParameterSymbol p) => p.RefKind == RefKind.In))
		{
			_factory.CompilationState.ModuleBuilderOpt?.EnsureIsReadOnlyAttributeExists();
		}
	}

	public override BoundNode VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		return new BoundObjectCreationExpression(node.Syntax, node.Constructor, arguments, default, default, expanded: false, default, null, null, null, node.Type);
	}

	public override BoundNode VisitAsOperator(BoundAsOperator node)
	{
		BoundExpression rewrittenOperand = VisitExpression(node.Operand);
		BoundTypeExpression rewrittenTargetType = (BoundTypeExpression)VisitTypeExpression(node.TargetType);
		TypeSymbol rewrittenType = VisitType(node.Type);
		return MakeAsOperator(node, node.Syntax, rewrittenOperand, rewrittenTargetType, node.Conversion, rewrittenType);
	}

	private BoundExpression MakeAsOperator(BoundAsOperator oldNode, SyntaxNode syntax, BoundExpression rewrittenOperand, BoundTypeExpression rewrittenTargetType, Conversion conversion, TypeSymbol rewrittenType)
	{
		if (!_inExpressionLambda)
		{
			ConstantValue asOperatorConstantResult = Binder.GetAsOperatorConstantResult(rewrittenOperand.Type, rewrittenType, conversion.Kind, rewrittenOperand.ConstantValue);
			if (asOperatorConstantResult != null)
			{
				BoundExpression boundExpression = (rewrittenType.IsNullableType() ? new BoundDefaultExpression(syntax, rewrittenType) : MakeLiteral(syntax, asOperatorConstantResult, rewrittenType));
				if (rewrittenOperand.ConstantValue != null)
				{
					return boundExpression;
				}
				return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(rewrittenOperand), boundExpression, rewrittenType);
			}
			if (conversion.IsImplicit)
			{
				return MakeConversionNode(syntax, rewrittenOperand, conversion, rewrittenType, @checked: false);
			}
		}
		return oldNode.Update(rewrittenOperand, rewrittenTargetType, conversion, rewrittenType);
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		return VisitAssignmentOperator(node, used: true);
	}

	private BoundExpression VisitAssignmentOperator(BoundAssignmentOperator node, bool used)
	{
		BoundExpression boundExpression = VisitExpression(node.Right);
		BoundExpression left = node.Left;
		BoundExpression rewrittenLeft;
		switch (left.Kind)
		{
		case BoundKind.PropertyAccess:
			rewrittenLeft = VisitPropertyAccess((BoundPropertyAccess)left, isLeftOfAssignment: true);
			break;
		case BoundKind.IndexerAccess:
			rewrittenLeft = VisitIndexerAccess((BoundIndexerAccess)left, isLeftOfAssignment: true);
			break;
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)left;
			if (boundEventAccess.EventSymbol.IsWindowsRuntimeEvent)
			{
				return VisitWindowsRuntimeEventFieldAssignmentOperator(node.Syntax, boundEventAccess, boundExpression);
			}
			goto default;
		}
		case BoundKind.DynamicMemberAccess:
		{
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)left;
			BoundExpression loweredReceiver2 = VisitExpression(boundDynamicMemberAccess.Receiver);
			return _dynamicFactory.MakeDynamicSetMember(loweredReceiver2, boundDynamicMemberAccess.Name, boundExpression).ToExpression();
		}
		case BoundKind.DynamicIndexerAccess:
		{
			BoundDynamicIndexerAccess boundDynamicIndexerAccess = (BoundDynamicIndexerAccess)left;
			BoundExpression loweredReceiver = VisitExpression(boundDynamicIndexerAccess.ReceiverOpt);
			ImmutableArray<BoundExpression> loweredArguments = VisitList(boundDynamicIndexerAccess.Arguments);
			return MakeDynamicSetIndex(boundDynamicIndexerAccess, loweredReceiver, loweredArguments, boundDynamicIndexerAccess.ArgumentNamesOpt, boundDynamicIndexerAccess.ArgumentRefKindsOpt, boundExpression);
		}
		default:
			rewrittenLeft = VisitExpression(left);
			break;
		}
		return MakeStaticAssignmentOperator(node.Syntax, rewrittenLeft, boundExpression, node.IsRef, node.Type, used);
	}

	private BoundExpression MakeAssignmentOperator(SyntaxNode syntax, BoundExpression rewrittenLeft, BoundExpression rewrittenRight, TypeSymbol type, bool used, bool isChecked, bool isCompoundAssignment)
	{
		switch (rewrittenLeft.Kind)
		{
		case BoundKind.DynamicIndexerAccess:
		{
			BoundDynamicIndexerAccess boundDynamicIndexerAccess = (BoundDynamicIndexerAccess)rewrittenLeft;
			return MakeDynamicSetIndex(boundDynamicIndexerAccess, boundDynamicIndexerAccess.ReceiverOpt, boundDynamicIndexerAccess.Arguments, boundDynamicIndexerAccess.ArgumentNamesOpt, boundDynamicIndexerAccess.ArgumentRefKindsOpt, rewrittenRight, isCompoundAssignment, isChecked);
		}
		case BoundKind.DynamicMemberAccess:
		{
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)rewrittenLeft;
			return _dynamicFactory.MakeDynamicSetMember(boundDynamicMemberAccess.Receiver, boundDynamicMemberAccess.Name, rewrittenRight, isCompoundAssignment, isChecked).ToExpression();
		}
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)rewrittenLeft;
			if (boundEventAccess.EventSymbol.IsWindowsRuntimeEvent)
			{
				return RewriteWindowsRuntimeEventAssignmentOperator(boundEventAccess.Syntax, boundEventAccess.EventSymbol, EventAssignmentKind.Assignment, isDynamic: false, boundEventAccess.ReceiverOpt, rewrittenRight);
			}
			throw ExceptionUtilities.Unreachable;
		}
		default:
			return MakeStaticAssignmentOperator(syntax, rewrittenLeft, rewrittenRight, isRef: false, type, used);
		}
	}

	private BoundExpression MakeDynamicSetIndex(BoundDynamicIndexerAccess indexerAccess, BoundExpression loweredReceiver, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> refKinds, BoundExpression loweredRight, bool isCompoundAssignment = false, bool isChecked = false)
	{
		EmbedIfNeedTo(loweredReceiver, indexerAccess.ApplicableIndexers, indexerAccess.Syntax);
		return _dynamicFactory.MakeDynamicSetIndex(MakeDynamicIndexerAccessReceiver(indexerAccess, loweredReceiver), loweredArguments, argumentNames, refKinds, loweredRight, isCompoundAssignment, isChecked).ToExpression();
	}

	private BoundExpression MakeStaticAssignmentOperator(SyntaxNode syntax, BoundExpression rewrittenLeft, BoundExpression rewrittenRight, bool isRef, TypeSymbol type, bool used)
	{
		switch (rewrittenLeft.Kind)
		{
		case BoundKind.DynamicMemberAccess:
		case BoundKind.DynamicIndexerAccess:
			throw ExceptionUtilities.UnexpectedValue(rewrittenLeft.Kind);
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)rewrittenLeft;
			BoundExpression receiverOpt2 = boundPropertyAccess.ReceiverOpt;
			PropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
			return MakePropertyAssignment(syntax, receiverOpt2, propertySymbol, ImmutableArray<BoundExpression>.Empty, default, expanded: false, default, rewrittenRight, type, used);
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)rewrittenLeft;
			BoundExpression receiverOpt = boundIndexerAccess.ReceiverOpt;
			ImmutableArray<BoundExpression> arguments = boundIndexerAccess.Arguments;
			PropertySymbol indexer = boundIndexerAccess.Indexer;
			return MakePropertyAssignment(syntax, receiverOpt, indexer, arguments, boundIndexerAccess.ArgumentRefKindsOpt, boundIndexerAccess.Expanded, boundIndexerAccess.ArgsToParamsOpt, rewrittenRight, type, used);
		}
		case BoundKind.Local:
			return new BoundAssignmentOperator(syntax, rewrittenLeft, rewrittenRight, type, isRef);
		case BoundKind.Parameter:
			return new BoundAssignmentOperator(syntax, rewrittenLeft, rewrittenRight, isRef, type);
		case BoundKind.DiscardExpression:
			return rewrittenRight;
		default:
			return new BoundAssignmentOperator(syntax, rewrittenLeft, rewrittenRight, type);
		}
	}

	private BoundExpression MakePropertyAssignment(SyntaxNode syntax, BoundExpression rewrittenReceiver, PropertySymbol property, ImmutableArray<BoundExpression> rewrittenArguments, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, BoundExpression rewrittenRight, TypeSymbol type, bool used)
	{
		MethodSymbol ownOrInheritedSetMethod = property.GetOwnOrInheritedSetMethod();
		if ((object)ownOrInheritedSetMethod == null)
		{
			SynthesizedBackingFieldSymbol backingField = (property as SourcePropertySymbol).BackingField;
			return _factory.AssignmentExpression(_factory.Field(rewrittenReceiver, backingField), rewrittenRight);
		}
		rewrittenArguments = MakeArguments(syntax, rewrittenArguments, property, ownOrInheritedSetMethod, expanded, argsToParamsOpt, ref argumentRefKindsOpt, out var temps, invokedAsExtensionMethod: false, ThreeState.True);
		if (used)
		{
			TypeSymbol type2 = rewrittenRight.Type;
			LocalSymbol localSymbol = _factory.SynthesizedLocal(type2);
			BoundExpression boundExpression = new BoundLocal(syntax, localSymbol, null, type2);
			BoundExpression newElement = new BoundAssignmentOperator(syntax, boundExpression, rewrittenRight, type2);
			BoundExpression item = BoundCall.Synthesized(syntax, rewrittenReceiver, ownOrInheritedSetMethod, AppendToPossibleNull(rewrittenArguments, newElement));
			return new BoundSequence(syntax, AppendToPossibleNull(temps, localSymbol), ImmutableArray.Create(item), boundExpression, type);
		}
		BoundCall boundCall = BoundCall.Synthesized(syntax, rewrittenReceiver, ownOrInheritedSetMethod, AppendToPossibleNull(rewrittenArguments, rewrittenRight));
		if (temps.IsDefaultOrEmpty)
		{
			return boundCall;
		}
		return new BoundSequence(syntax, temps, ImmutableArray<BoundExpression>.Empty, boundCall, ownOrInheritedSetMethod.ReturnType);
	}

	private static ImmutableArray<T> AppendToPossibleNull<T>(ImmutableArray<T> possibleNull, T newElement)
	{
		return possibleNull.NullToEmpty().Add(newElement);
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		_sawAwait = true;
		return base.VisitAwaitExpression(node);
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		return VisitBinaryOperator(node, null);
	}

	public override BoundNode VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		SyntaxNode syntax = node.Syntax;
		BinaryOperatorKind operatorKind = node.OperatorKind;
		TypeSymbol type = node.Type;
		BoundExpression boundExpression = VisitExpression(node.Left);
		BoundExpression boundExpression2 = VisitExpression(node.Right);
		if (_inExpressionLambda)
		{
			return node.Update(operatorKind, boundExpression, boundExpression2, node.LogicalOperator, node.TrueOperator, node.FalseOperator, node.ResultKind, type);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store);
		BoundCall rewrittenCondition = BoundCall.Synthesized(syntax, null, (operatorKind.Operator() == BinaryOperatorKind.And) ? node.FalseOperator : node.TrueOperator, boundLocal);
		BoundExpression rewrittenAlternative = LowerUserDefinedBinaryOperator(syntax, operatorKind & ~BinaryOperatorKind.Logical, boundLocal, boundExpression2, type, node.LogicalOperator);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, boundLocal, rewrittenAlternative, null, type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, type);
	}

	public BoundExpression VisitBinaryOperator(BoundBinaryOperator node, BoundUnaryOperator applyParentUnaryOperator)
	{
		ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
		BoundBinaryOperator boundBinaryOperator = node;
		while (boundBinaryOperator != null && boundBinaryOperator.ConstantValue == null)
		{
			instance.Push(boundBinaryOperator);
			boundBinaryOperator = boundBinaryOperator.Left as BoundBinaryOperator;
		}
		BoundExpression boundExpression = VisitExpression(instance.Peek().Left);
		while (instance.Count > 0)
		{
			BoundBinaryOperator boundBinaryOperator2 = instance.Pop();
			BoundExpression loweredRight = VisitExpression(boundBinaryOperator2.Right);
			boundExpression = MakeBinaryOperator(boundBinaryOperator2, boundBinaryOperator2.Syntax, boundBinaryOperator2.OperatorKind, boundExpression, loweredRight, boundBinaryOperator2.Type, boundBinaryOperator2.MethodOpt, isPointerElementAccess: false, isCompoundAssignment: false, (instance.Count == 0) ? applyParentUnaryOperator : null);
		}
		instance.Free();
		return boundExpression;
	}

	private BoundExpression MakeBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol method, bool isPointerElementAccess = false, bool isCompoundAssignment = false, BoundUnaryOperator applyParentUnaryOperator = null)
	{
		return MakeBinaryOperator(null, syntax, operatorKind, loweredLeft, loweredRight, type, method, isPointerElementAccess, isCompoundAssignment, applyParentUnaryOperator);
	}

	private BoundExpression MakeBinaryOperator(BoundBinaryOperator oldNode, SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol method, bool isPointerElementAccess = false, bool isCompoundAssignment = false, BoundUnaryOperator applyParentUnaryOperator = null)
	{
		if (_inExpressionLambda)
		{
			switch (operatorKind.Operator() | operatorKind.OperandTypes())
			{
			case BinaryOperatorKind.StringConcatenation:
			case BinaryOperatorKind.StringAndObjectConcatenation:
			case BinaryOperatorKind.ObjectAndStringConcatenation:
				return RewriteStringConcatenation(syntax, operatorKind, loweredLeft, loweredRight, type);
			case BinaryOperatorKind.DelegateCombination:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__Combine);
			case BinaryOperatorKind.DelegateRemoval:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__Remove);
			case BinaryOperatorKind.DelegateEqual:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__op_Equality);
			case BinaryOperatorKind.DelegateNotEqual:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__op_Inequality);
			}
		}
		else
		{
			if (operatorKind.IsDynamic())
			{
				if (operatorKind.IsLogical())
				{
					return MakeDynamicLogicalBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, method, type, isCompoundAssignment, applyParentUnaryOperator);
				}
				return _dynamicFactory.MakeDynamicBinaryOperator(operatorKind, loweredLeft, loweredRight, isCompoundAssignment, type).ToExpression();
			}
			if (operatorKind.IsLifted())
			{
				return RewriteLiftedBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, type, method);
			}
			if (operatorKind.IsUserDefined())
			{
				return LowerUserDefinedBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, type, method);
			}
			switch (operatorKind.OperatorWithLogical() | operatorKind.OperandTypes())
			{
			case BinaryOperatorKind.NullableNullEqual:
			case BinaryOperatorKind.NullableNullNotEqual:
				return RewriteNullableNullEquality(syntax, operatorKind, loweredLeft, loweredRight, type);
			case BinaryOperatorKind.StringConcatenation:
			case BinaryOperatorKind.StringAndObjectConcatenation:
			case BinaryOperatorKind.ObjectAndStringConcatenation:
				return RewriteStringConcatenation(syntax, operatorKind, loweredLeft, loweredRight, type);
			case BinaryOperatorKind.StringEqual:
				return RewriteStringEquality(oldNode, syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_String__op_Equality);
			case BinaryOperatorKind.StringNotEqual:
				return RewriteStringEquality(oldNode, syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_String__op_Inequality);
			case BinaryOperatorKind.DelegateCombination:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__Combine);
			case BinaryOperatorKind.DelegateRemoval:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__Remove);
			case BinaryOperatorKind.DelegateEqual:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__op_Equality);
			case BinaryOperatorKind.DelegateNotEqual:
				return RewriteDelegateOperation(syntax, operatorKind, loweredLeft, loweredRight, type, SpecialMember.System_Delegate__op_Inequality);
			case BinaryOperatorKind.LogicalBoolAnd:
				if (loweredRight.ConstantValue == ConstantValue.True)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValue == ConstantValue.True)
				{
					return loweredRight;
				}
				if (loweredLeft.ConstantValue == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredRight.Kind == BoundKind.Local || loweredRight.Kind == BoundKind.Parameter)
				{
					operatorKind &= ~BinaryOperatorKind.Logical;
				}
				break;
			case BinaryOperatorKind.LogicalBoolOr:
				if (loweredRight.ConstantValue == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValue == ConstantValue.False)
				{
					return loweredRight;
				}
				if (loweredLeft.ConstantValue == ConstantValue.True)
				{
					return loweredLeft;
				}
				if (loweredRight.Kind == BoundKind.Local || loweredRight.Kind == BoundKind.Parameter)
				{
					operatorKind &= ~BinaryOperatorKind.Logical;
				}
				break;
			case BinaryOperatorKind.BoolAnd:
				if (loweredRight.ConstantValue == ConstantValue.True)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValue == ConstantValue.True)
				{
					return loweredRight;
				}
				if (loweredLeft.IsDefaultValue())
				{
					return _factory.MakeSequence(loweredRight, loweredLeft);
				}
				if (loweredRight.IsDefaultValue())
				{
					return _factory.MakeSequence(loweredLeft, loweredRight);
				}
				break;
			case BinaryOperatorKind.BoolOr:
				if (loweredRight.ConstantValue == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValue == ConstantValue.False)
				{
					return loweredRight;
				}
				break;
			case BinaryOperatorKind.BoolEqual:
				if (loweredLeft.ConstantValue == ConstantValue.True)
				{
					return loweredRight;
				}
				if (loweredRight.ConstantValue == ConstantValue.True)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValue == ConstantValue.False)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, loweredRight, loweredRight.Type);
				}
				if (loweredRight.ConstantValue == ConstantValue.False)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, loweredLeft, loweredLeft.Type);
				}
				break;
			case BinaryOperatorKind.BoolNotEqual:
				if (loweredLeft.ConstantValue == ConstantValue.False)
				{
					return loweredRight;
				}
				if (loweredRight.ConstantValue == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValue == ConstantValue.True)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, loweredRight, loweredRight.Type);
				}
				if (loweredRight.ConstantValue == ConstantValue.True)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, loweredLeft, loweredLeft.Type);
				}
				break;
			case BinaryOperatorKind.BoolXor:
				if (loweredLeft.ConstantValue == ConstantValue.False)
				{
					return loweredRight;
				}
				if (loweredRight.ConstantValue == ConstantValue.False)
				{
					return loweredLeft;
				}
				if (loweredLeft.ConstantValue == ConstantValue.True)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, loweredRight, loweredRight.Type);
				}
				if (loweredRight.ConstantValue == ConstantValue.True)
				{
					return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, loweredLeft, loweredLeft.Type);
				}
				break;
			case BinaryOperatorKind.IntLeftShift:
			case BinaryOperatorKind.UIntLeftShift:
			case BinaryOperatorKind.IntRightShift:
			case BinaryOperatorKind.UIntRightShift:
				return RewriteBuiltInShiftOperation(oldNode, syntax, operatorKind, loweredLeft, loweredRight, type, 31);
			case BinaryOperatorKind.LongLeftShift:
			case BinaryOperatorKind.ULongLeftShift:
			case BinaryOperatorKind.LongRightShift:
			case BinaryOperatorKind.ULongRightShift:
				return RewriteBuiltInShiftOperation(oldNode, syntax, operatorKind, loweredLeft, loweredRight, type, 63);
			case BinaryOperatorKind.DecimalMultiplication:
			case BinaryOperatorKind.DecimalAddition:
			case BinaryOperatorKind.DecimalSubtraction:
			case BinaryOperatorKind.DecimalDivision:
			case BinaryOperatorKind.DecimalRemainder:
			case BinaryOperatorKind.DecimalEqual:
			case BinaryOperatorKind.DecimalNotEqual:
			case BinaryOperatorKind.DecimalGreaterThan:
			case BinaryOperatorKind.DecimalLessThan:
			case BinaryOperatorKind.DecimalGreaterThanOrEqual:
			case BinaryOperatorKind.DecimalLessThanOrEqual:
				return RewriteDecimalBinaryOperation(syntax, loweredLeft, loweredRight, operatorKind);
			case BinaryOperatorKind.PointerAndIntAddition:
			case BinaryOperatorKind.PointerAndUIntAddition:
			case BinaryOperatorKind.PointerAndLongAddition:
			case BinaryOperatorKind.PointerAndULongAddition:
			case BinaryOperatorKind.PointerAndIntSubtraction:
			case BinaryOperatorKind.PointerAndUIntSubtraction:
			case BinaryOperatorKind.PointerAndLongSubtraction:
			case BinaryOperatorKind.PointerAndULongSubtraction:
				if (loweredRight.IsDefaultValue())
				{
					return loweredLeft;
				}
				return RewritePointerNumericOperator(syntax, operatorKind, loweredLeft, loweredRight, type, isPointerElementAccess, isLeftPointer: true);
			case BinaryOperatorKind.IntAndPointerAddition:
			case BinaryOperatorKind.UIntAndPointerAddition:
			case BinaryOperatorKind.LongAndPointerAddition:
			case BinaryOperatorKind.ULongAndPointerAddition:
				if (loweredLeft.IsDefaultValue())
				{
					return loweredRight;
				}
				return RewritePointerNumericOperator(syntax, operatorKind, loweredLeft, loweredRight, type, isPointerElementAccess, isLeftPointer: false);
			case BinaryOperatorKind.PointerSubtraction:
				return RewritePointerSubtraction(operatorKind, loweredLeft, loweredRight, type);
			case BinaryOperatorKind.IntAddition:
			case BinaryOperatorKind.UIntAddition:
			case BinaryOperatorKind.LongAddition:
			case BinaryOperatorKind.ULongAddition:
				if (loweredLeft.IsDefaultValue())
				{
					return loweredRight;
				}
				if (loweredRight.IsDefaultValue())
				{
					return loweredLeft;
				}
				break;
			case BinaryOperatorKind.IntSubtraction:
			case BinaryOperatorKind.UIntSubtraction:
			case BinaryOperatorKind.LongSubtraction:
			case BinaryOperatorKind.ULongSubtraction:
				if (loweredRight.IsDefaultValue())
				{
					return loweredLeft;
				}
				break;
			case BinaryOperatorKind.IntMultiplication:
			case BinaryOperatorKind.UIntMultiplication:
			case BinaryOperatorKind.LongMultiplication:
			case BinaryOperatorKind.ULongMultiplication:
			{
				if (loweredLeft.IsDefaultValue())
				{
					return _factory.MakeSequence(loweredRight, loweredLeft);
				}
				if (loweredRight.IsDefaultValue())
				{
					return _factory.MakeSequence(loweredLeft, loweredRight);
				}
				ConstantValue constantValue = loweredLeft.ConstantValue;
				if ((object)constantValue != null && constantValue.UInt64Value == 1)
				{
					return loweredRight;
				}
				ConstantValue constantValue2 = loweredRight.ConstantValue;
				if ((object)constantValue2 != null && constantValue2.UInt64Value == 1)
				{
					return loweredLeft;
				}
				break;
			}
			case BinaryOperatorKind.IntGreaterThan:
			case BinaryOperatorKind.IntLessThanOrEqual:
				if (loweredLeft.Kind == BoundKind.ArrayLength && loweredRight.IsDefaultValue())
				{
					BinaryOperatorKind binaryOperatorKind2 = ((operatorKind == BinaryOperatorKind.IntGreaterThan) ? BinaryOperatorKind.NotEqual : BinaryOperatorKind.Equal);
					operatorKind &= ~BinaryOperatorKind.OpMask;
					operatorKind |= binaryOperatorKind2;
					loweredLeft = UnconvertArrayLength((BoundArrayLength)loweredLeft);
				}
				break;
			case BinaryOperatorKind.IntLessThan:
			case BinaryOperatorKind.IntGreaterThanOrEqual:
				if (loweredRight.Kind == BoundKind.ArrayLength && loweredLeft.IsDefaultValue())
				{
					BinaryOperatorKind binaryOperatorKind = ((operatorKind == BinaryOperatorKind.IntLessThan) ? BinaryOperatorKind.NotEqual : BinaryOperatorKind.Equal);
					operatorKind &= ~BinaryOperatorKind.OpMask;
					operatorKind |= binaryOperatorKind;
					loweredRight = UnconvertArrayLength((BoundArrayLength)loweredRight);
				}
				break;
			case BinaryOperatorKind.IntEqual:
			case BinaryOperatorKind.IntNotEqual:
				if (loweredLeft.Kind == BoundKind.ArrayLength && loweredRight.IsDefaultValue())
				{
					loweredLeft = UnconvertArrayLength((BoundArrayLength)loweredLeft);
				}
				else if (loweredRight.Kind == BoundKind.ArrayLength && loweredLeft.IsDefaultValue())
				{
					loweredRight = UnconvertArrayLength((BoundArrayLength)loweredRight);
				}
				break;
			}
		}
		if (oldNode == null)
		{
			return new BoundBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, null, null, LookupResultKind.Viable, type);
		}
		return oldNode.Update(operatorKind, loweredLeft, loweredRight, oldNode.ConstantValueOpt, oldNode.MethodOpt, oldNode.ResultKind, type);
	}

	private BoundExpression RewriteLiftedBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol method)
	{
		BoundLoweredConditionalAccess boundLoweredConditionalAccess = loweredLeft as BoundLoweredConditionalAccess;
		int num;
		if (boundLoweredConditionalAccess != null && !ReadIsSideeffecting(loweredRight))
		{
			if (boundLoweredConditionalAccess.WhenNullOpt != null)
			{
				num = (boundLoweredConditionalAccess.WhenNullOpt.IsDefaultValue() ? 1 : 0);
				if (num == 0)
				{
					goto IL_0037;
				}
			}
			else
			{
				num = 1;
			}
			loweredLeft = boundLoweredConditionalAccess.WhenNotNull;
		}
		else
		{
			num = 0;
		}
		goto IL_0037;
		IL_0037:
		BoundExpression boundExpression;
		if (operatorKind.IsComparison())
		{
			boundExpression = (operatorKind.IsUserDefined() ? LowerLiftedUserDefinedComparisonOperator(syntax, operatorKind, loweredLeft, loweredRight, method) : LowerLiftedBuiltInComparisonOperator(syntax, operatorKind, loweredLeft, loweredRight));
		}
		else
		{
			boundExpression = LowerLiftedBinaryArithmeticOperator(syntax, operatorKind, loweredLeft, loweredRight, type, method);
		}
		if (num != 0)
		{
			BoundExpression whenNullOpt = null;
			if (operatorKind.Operator() == BinaryOperatorKind.NotEqual || operatorKind.Operator() == BinaryOperatorKind.Equal)
			{
				whenNullOpt = RewriteLiftedBinaryOperator(syntax, operatorKind, _factory.Default(loweredLeft.Type), loweredRight, type, method);
			}
			boundExpression = boundLoweredConditionalAccess.Update(boundLoweredConditionalAccess.Receiver, boundLoweredConditionalAccess.HasValueMethodOpt, boundExpression, whenNullOpt, boundLoweredConditionalAccess.Id, boundExpression.Type);
		}
		return boundExpression;
	}

	private BoundExpression UnconvertArrayLength(BoundArrayLength arrLength)
	{
		return arrLength.Update(arrLength.Expression, _factory.SpecialType(SpecialType.System_UIntPtr));
	}

	private BoundExpression MakeDynamicLogicalBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, MethodSymbol leftTruthOperator, TypeSymbol type, bool isCompoundAssignment, BoundUnaryOperator applyParentUnaryOperator)
	{
		bool flag = operatorKind.Operator() == BinaryOperatorKind.And;
		UnaryOperatorKind unaryOperatorKind = (flag ? UnaryOperatorKind.DynamicFalse : UnaryOperatorKind.DynamicTrue);
		ConstantValue constantValue = loweredLeft.ConstantValue ?? UnboxConstant(loweredLeft);
		if ((unaryOperatorKind == UnaryOperatorKind.DynamicFalse && constantValue == ConstantValue.False) || (unaryOperatorKind == UnaryOperatorKind.DynamicTrue && constantValue == ConstantValue.True))
		{
			if (applyParentUnaryOperator != null)
			{
				return _factory.Literal(value: true);
			}
			return MakeConversionNode(loweredLeft, type, @checked: false);
		}
		NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundAssignmentOperator boundAssignmentOperator;
		BoundLocal boundLocal2;
		if (constantValue == null && loweredLeft.Kind != BoundKind.Local && loweredLeft.Kind != BoundKind.Parameter)
		{
			BoundExpression boundLocal = (loweredLeft = _factory.StoreToTemp(loweredLeft, out var store));
			boundAssignmentOperator = store;
			boundLocal2 = (BoundLocal)boundLocal;
		}
		else
		{
			boundAssignmentOperator = null;
			boundLocal2 = null;
		}
		BoundExpression boundExpression = _dynamicFactory.MakeDynamicBinaryOperator(operatorKind, loweredLeft, loweredRight, isCompoundAssignment, type).ToExpression();
		bool flag2 = (unaryOperatorKind == UnaryOperatorKind.DynamicFalse && constantValue == ConstantValue.True) || (unaryOperatorKind == UnaryOperatorKind.DynamicTrue && constantValue == ConstantValue.False);
		BoundExpression boundExpression2;
		if (applyParentUnaryOperator != null)
		{
			boundExpression2 = _dynamicFactory.MakeDynamicUnaryOperator(unaryOperatorKind, boundExpression, specialType).ToExpression();
			if (!flag2)
			{
				BoundExpression left = MakeTruthTestForDynamicLogicalOperator(syntax, loweredLeft, specialType, leftTruthOperator, flag);
				boundExpression2 = _factory.Binary(BinaryOperatorKind.LogicalOr, specialType, left, boundExpression2);
			}
		}
		else if (flag2)
		{
			boundExpression2 = boundExpression;
		}
		else
		{
			BoundExpression condition = MakeTruthTestForDynamicLogicalOperator(syntax, loweredLeft, specialType, leftTruthOperator, flag);
			BoundExpression consequence = MakeConversionNode(loweredLeft, type, @checked: false);
			boundExpression2 = _factory.Conditional(condition, consequence, boundExpression, type);
		}
		if (boundAssignmentOperator != null)
		{
			return _factory.Sequence(ImmutableArray.Create(boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)boundAssignmentOperator), boundExpression2);
		}
		return boundExpression2;
	}

	private static ConstantValue UnboxConstant(BoundExpression expression)
	{
		if (expression.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expression;
			if (boundConversion.ConversionKind == ConversionKind.Boxing)
			{
				return boundConversion.Operand.ConstantValue;
			}
		}
		return null;
	}

	private BoundExpression MakeTruthTestForDynamicLogicalOperator(SyntaxNode syntax, BoundExpression loweredLeft, TypeSymbol boolean, MethodSymbol leftTruthOperator, bool negative)
	{
		if (loweredLeft.HasDynamicType())
		{
			return _dynamicFactory.MakeDynamicUnaryOperator(negative ? UnaryOperatorKind.DynamicFalse : UnaryOperatorKind.DynamicTrue, loweredLeft, boolean).ToExpression();
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = _compilation.Conversions.ClassifyConversionFromExpression(loweredLeft, boolean, ref useSiteDiagnostics);
		_diagnostics.Add(loweredLeft.Syntax, useSiteDiagnostics);
		if (conversion.IsImplicit)
		{
			BoundExpression boundExpression = MakeConversionNode(loweredLeft, boolean, @checked: false);
			if (negative)
			{
				return new BoundUnaryOperator(syntax, UnaryOperatorKind.BoolLogicalNegation, boundExpression, null, null, LookupResultKind.Viable, boolean)
				{
					WasCompilerGenerated = true
				};
			}
			return boundExpression;
		}
		return BoundCall.Synthesized(syntax, null, leftTruthOperator, loweredLeft);
	}

	private BoundExpression LowerUserDefinedBinaryOperator(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol method)
	{
		if (operatorKind.IsLifted())
		{
			return RewriteLiftedBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, type, method);
		}
		return BoundCall.Synthesized(syntax, null, method, loweredLeft, loweredRight);
	}

	private BoundExpression TrivialLiftedComparisonOperatorOptimizations(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, MethodSymbol method)
	{
		bool flag = NullableNeverHasValue(left);
		bool flag2 = NullableNeverHasValue(right);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		if (flag & flag2)
		{
			return MakeLiteral(syntax, ConstantValue.Create(kind.Operator() == BinaryOperatorKind.Equal), specialType);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(left);
		BoundExpression boundExpression2 = NullableAlwaysHasValue(right);
		if (boundExpression != null && boundExpression2 != null)
		{
			return MakeBinaryOperator(syntax, kind.Unlifted(), boundExpression, boundExpression2, specialType, method);
		}
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		if ((flag && boundExpression2 != null) || (flag2 && boundExpression != null))
		{
			BoundExpression boundExpression3 = MakeLiteral(syntax, ConstantValue.Create(binaryOperatorKind == BinaryOperatorKind.NotEqual), specialType);
			BoundExpression boundExpression4 = (flag ? boundExpression2 : boundExpression);
			if (ReadIsSideeffecting(boundExpression4))
			{
				boundExpression3 = new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundExpression4), boundExpression3, specialType);
			}
			return boundExpression3;
		}
		if (flag | flag2)
		{
			BoundExpression boundExpression5 = (flag ? right : left);
			if (binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual)
			{
				BoundExpression boundExpression6 = MakeNullableHasValue(syntax, boundExpression5);
				if (binaryOperatorKind != BinaryOperatorKind.Equal)
				{
					return boundExpression6;
				}
				return MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, boundExpression6, specialType);
			}
			BoundExpression boundExpression7 = MakeBooleanConstant(syntax, binaryOperatorKind == BinaryOperatorKind.NotEqual);
			return _factory.MakeSequence(boundExpression5, boundExpression7);
		}
		return null;
	}

	private BoundExpression MakeOptimizedGetValueOrDefault(SyntaxNode syntax, BoundExpression expression)
	{
		if (expression.Type.IsNullableType())
		{
			return BoundCall.Synthesized(syntax, expression, UnsafeGetNullableMethod(syntax, expression.Type, SpecialMember.System_Nullable_T_GetValueOrDefault));
		}
		return expression;
	}

	private BoundExpression MakeBooleanConstant(SyntaxNode syntax, bool value)
	{
		return MakeLiteral(syntax, ConstantValue.Create(value), _compilation.GetSpecialType(SpecialType.System_Boolean));
	}

	private BoundExpression MakeOptimizedHasValue(SyntaxNode syntax, BoundExpression expression)
	{
		if (expression.Type.IsNullableType())
		{
			return MakeNullableHasValue(syntax, expression);
		}
		return MakeBooleanConstant(syntax, value: true);
	}

	private BoundExpression MakeNullableHasValue(SyntaxNode syntax, BoundExpression expression)
	{
		return BoundCall.Synthesized(syntax, expression, UnsafeGetNullableMethod(syntax, expression.Type, SpecialMember.System_Nullable_T_get_HasValue));
	}

	private BoundExpression LowerLiftedBuiltInComparisonOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight)
	{
		BoundExpression boundExpression = TrivialLiftedComparisonOperatorOptimizations(syntax, kind, loweredLeft, loweredRight, null);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(loweredLeft);
		BoundExpression boundExpression3 = NullableAlwaysHasValue(loweredRight);
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression2 ?? loweredLeft, out var store);
		BoundLocal boundLocal2 = _factory.StoreToTemp(boundExpression3 ?? loweredRight, out var store2);
		BoundExpression loweredLeft2 = MakeOptimizedGetValueOrDefault(syntax, boundLocal);
		BoundExpression loweredRight2 = MakeOptimizedGetValueOrDefault(syntax, boundLocal2);
		BoundExpression loweredLeft3 = MakeOptimizedHasValue(syntax, boundLocal);
		BoundExpression loweredRight3 = MakeOptimizedHasValue(syntax, boundLocal2);
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		BinaryOperatorKind kind2;
		BinaryOperatorKind operatorKind;
		if (binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual)
		{
			kind2 = BinaryOperatorKind.Equal;
			operatorKind = BinaryOperatorKind.BoolEqual;
		}
		else
		{
			kind2 = binaryOperatorKind;
			operatorKind = BinaryOperatorKind.BoolAnd;
		}
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundExpression loweredLeft4 = MakeBinaryOperator(syntax, kind2.WithType(kind.OperandTypes()), loweredLeft2, loweredRight2, specialType, null);
		BoundExpression loweredRight4 = MakeBinaryOperator(syntax, operatorKind, loweredLeft3, loweredRight3, specialType, null);
		BoundExpression boundExpression4 = MakeBinaryOperator(syntax, BinaryOperatorKind.BoolAnd, loweredLeft4, loweredRight4, specialType, null);
		if (binaryOperatorKind == BinaryOperatorKind.NotEqual)
		{
			boundExpression4 = _factory.Not(boundExpression4);
		}
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol, boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store, (BoundExpression)store2), boundExpression4, specialType);
	}

	private BoundExpression LowerLiftedUserDefinedComparisonOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, MethodSymbol method)
	{
		BoundExpression boundExpression = TrivialLiftedComparisonOperatorOptimizations(syntax, kind, loweredLeft, loweredRight, method);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(loweredLeft);
		BoundExpression boundExpression3 = NullableAlwaysHasValue(loweredRight);
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression2 ?? loweredLeft, out var store);
		BoundLocal boundLocal2 = _factory.StoreToTemp(boundExpression3 ?? loweredRight, out var store2);
		BoundExpression loweredLeft2 = MakeOptimizedGetValueOrDefault(syntax, boundLocal);
		BoundExpression loweredRight2 = MakeOptimizedGetValueOrDefault(syntax, boundLocal2);
		BoundExpression boundExpression4 = MakeOptimizedHasValue(syntax, boundLocal);
		BoundExpression loweredRight3 = MakeOptimizedHasValue(syntax, boundLocal2);
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		BinaryOperatorKind operatorKind = ((binaryOperatorKind != BinaryOperatorKind.Equal && binaryOperatorKind != BinaryOperatorKind.NotEqual) ? BinaryOperatorKind.BoolAnd : BinaryOperatorKind.BoolEqual);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundExpression rewrittenCondition = MakeBinaryOperator(syntax, operatorKind, boundExpression4, loweredRight3, specialType, null);
		BoundExpression boundExpression5 = MakeBinaryOperator(syntax, kind.Unlifted(), loweredLeft2, loweredRight2, specialType, method);
		BoundExpression rewrittenConsequence = ((binaryOperatorKind != BinaryOperatorKind.Equal && binaryOperatorKind != BinaryOperatorKind.NotEqual) ? boundExpression5 : RewriteConditionalOperator(syntax, boundExpression4, boundExpression5, MakeLiteral(syntax, ConstantValue.Create(binaryOperatorKind == BinaryOperatorKind.Equal), specialType), null, specialType, isRef: false));
		BoundExpression rewrittenAlternative = MakeBooleanConstant(syntax, binaryOperatorKind == BinaryOperatorKind.NotEqual);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, specialType, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol, boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store, (BoundExpression)store2), value, specialType);
	}

	private BoundExpression TrivialLiftedBinaryArithmeticOptimizations(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, TypeSymbol type, MethodSymbol method)
	{
		bool flag = NullableNeverHasValue(left);
		bool flag2 = NullableNeverHasValue(right);
		if (flag & flag2)
		{
			return new BoundDefaultExpression(syntax, null, type);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(left);
		BoundExpression boundExpression2 = NullableAlwaysHasValue(right);
		if (boundExpression != null && boundExpression2 != null)
		{
			return MakeLiftedBinaryOperatorConsequence(syntax, kind, boundExpression, boundExpression2, type, method);
		}
		return null;
	}

	private BoundExpression MakeLiftedBinaryOperatorConsequence(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, TypeSymbol type, MethodSymbol method)
	{
		BoundExpression boundExpression = MakeBinaryOperator(syntax, kind.Unlifted(), left, right, type.GetNullableUnderlyingType(), method);
		return new BoundObjectCreationExpression(syntax, UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T__ctor), null, boundExpression);
	}

	private static BoundExpression OptimizeLiftedArithmeticOperatorOneNull(SyntaxNode syntax, BoundExpression left, BoundExpression right, TypeSymbol type)
	{
		bool flag = NullableNeverHasValue(left);
		bool flag2 = NullableNeverHasValue(right);
		if (!(flag | flag2))
		{
			return null;
		}
		BoundExpression boundExpression = (flag ? right : left);
		BoundExpression boundExpression2 = NullableAlwaysHasValue(boundExpression) ?? boundExpression;
		if (boundExpression2.ConstantValue != null)
		{
			return new BoundDefaultExpression(syntax, null, type);
		}
		return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundExpression2), new BoundDefaultExpression(syntax, null, type), type);
	}

	private BoundExpression LowerLiftedBinaryArithmeticOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, MethodSymbol method)
	{
		BoundExpression boundExpression = OptimizeLiftedBinaryArithmetic(syntax, kind, loweredLeft, loweredRight, type, method);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
		BoundExpression boundExpression2 = NullableAlwaysHasValue(loweredLeft);
		BoundExpression boundExpression3 = NullableAlwaysHasValue(loweredRight);
		if (boundExpression2 == null)
		{
			boundExpression2 = loweredLeft;
		}
		BoundExpression operand = boundExpression2;
		operand = CaptureExpressionInTempIfNeeded(operand, instance, instance2);
		BoundExpression operand2 = boundExpression3 ?? loweredRight;
		operand2 = CaptureExpressionInTempIfNeeded(operand2, instance, instance2);
		BoundExpression left = MakeOptimizedGetValueOrDefault(syntax, operand);
		BoundExpression right = MakeOptimizedGetValueOrDefault(syntax, operand2);
		BoundExpression loweredLeft2 = MakeOptimizedHasValue(syntax, operand);
		BoundExpression loweredRight2 = MakeOptimizedHasValue(syntax, operand2);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundExpression rewrittenCondition = MakeBinaryOperator(syntax, BinaryOperatorKind.BoolAnd, loweredLeft2, loweredRight2, specialType, null);
		BoundExpression rewrittenConsequence = MakeLiftedBinaryOperatorConsequence(syntax, kind, left, right, type, method);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, null, type);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, type, isRef: false);
		return new BoundSequence(syntax, instance2.ToImmutableAndFree(), instance.ToImmutableAndFree(), value, type);
	}

	private BoundExpression CaptureExpressionInTempIfNeeded(BoundExpression operand, ArrayBuilder<BoundExpression> sideeffects, ArrayBuilder<LocalSymbol> locals)
	{
		if (CanChangeValueBetweenReads(operand))
		{
			BoundLocal boundLocal = _factory.StoreToTemp(operand, out var store);
			sideeffects.Add(store);
			locals.Add(boundLocal.LocalSymbol);
			operand = boundLocal;
		}
		return operand;
	}

	private BoundExpression OptimizeLiftedBinaryArithmetic(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, TypeSymbol type, MethodSymbol method)
	{
		BoundExpression boundExpression = TrivialLiftedBinaryArithmeticOptimizations(syntax, kind, left, right, type, method);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		if (kind == BinaryOperatorKind.LiftedBoolAnd || kind == BinaryOperatorKind.LiftedBoolOr)
		{
			return LowerLiftedBooleanOperator(syntax, kind, left, right);
		}
		boundExpression = OptimizeLiftedArithmeticOperatorOneNull(syntax, left, right, type);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(right);
		if (boundExpression2 != null && boundExpression2.ConstantValue != null && left.Kind == BoundKind.Sequence)
		{
			BoundSequence boundSequence = (BoundSequence)left;
			if (boundSequence.Value.Kind == BoundKind.ConditionalOperator)
			{
				BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)boundSequence.Value;
				if (NullableAlwaysHasValue(boundConditionalOperator.Consequence) != null && NullableNeverHasValue(boundConditionalOperator.Alternative))
				{
					return new BoundSequence(syntax, boundSequence.Locals, boundSequence.SideEffects, RewriteConditionalOperator(syntax, boundConditionalOperator.Condition, MakeBinaryOperator(syntax, kind, boundConditionalOperator.Consequence, right, type, method), MakeBinaryOperator(syntax, kind, boundConditionalOperator.Alternative, right, type, method), null, type, isRef: false), type);
				}
			}
		}
		return null;
	}

	private BoundExpression MakeNewNullableBoolean(SyntaxNode syntax, bool? value)
	{
		NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Nullable_T);
		TypeSymbol specialType2 = _compilation.GetSpecialType(SpecialType.System_Boolean);
		NamedTypeSymbol namedTypeSymbol = specialType.Construct(specialType2);
		if (!value.HasValue)
		{
			return new BoundDefaultExpression(syntax, null, namedTypeSymbol);
		}
		return new BoundObjectCreationExpression(syntax, UnsafeGetNullableMethod(syntax, namedTypeSymbol, SpecialMember.System_Nullable_T__ctor), null, MakeBooleanConstant(syntax, value == true));
	}

	private BoundExpression OptimizeLiftedBooleanOperatorOneNull(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right)
	{
		bool flag = NullableNeverHasValue(left);
		bool flag2 = NullableNeverHasValue(right);
		if (!(flag | flag2))
		{
			return null;
		}
		BoundExpression boundExpression = (flag ? left : right);
		BoundExpression boundExpression2 = (flag ? right : left);
		BoundExpression boundExpression3 = NullableAlwaysHasValue(boundExpression2);
		BoundExpression boundExpression4 = new BoundDefaultExpression(syntax, null, boundExpression.Type);
		if (boundExpression3 != null)
		{
			BoundExpression boundExpression5 = MakeNewNullableBoolean(syntax, kind == BinaryOperatorKind.LiftedBoolOr);
			return RewriteConditionalOperator(syntax, boundExpression3, (kind == BinaryOperatorKind.LiftedBoolAnd) ? boundExpression4 : boundExpression5, (kind == BinaryOperatorKind.LiftedBoolAnd) ? boundExpression5 : boundExpression4, null, boundExpression.Type, isRef: false);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression2, out var store);
		BoundExpression rewrittenCondition = MakeOptimizedGetValueOrDefault(syntax, boundLocal);
		BoundExpression rewrittenConsequence = ((kind == BinaryOperatorKind.LiftedBoolAnd) ? boundExpression4 : boundLocal);
		BoundExpression rewrittenAlternative = ((kind == BinaryOperatorKind.LiftedBoolAnd) ? boundLocal : boundExpression4);
		BoundExpression boundExpression6 = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, boundExpression.Type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), boundExpression6, boundExpression6.Type);
	}

	private BoundExpression OptimizeLiftedBooleanOperatorOneNonNull(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right)
	{
		BoundExpression boundExpression = NullableAlwaysHasValue(left);
		BoundExpression boundExpression2 = NullableAlwaysHasValue(right);
		if (boundExpression == null && boundExpression2 == null)
		{
			return null;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression ?? left, out var store);
		BoundLocal boundLocal2 = _factory.StoreToTemp(boundExpression2 ?? right, out var store2);
		BoundLocal boundLocal3 = ((boundExpression == null) ? boundLocal2 : boundLocal);
		BoundExpression boundExpression3 = ((boundExpression == null) ? boundLocal : boundLocal2);
		BoundExpression rewrittenCondition = boundLocal3;
		BoundExpression boundExpression4 = MakeNewNullableBoolean(syntax, kind == BinaryOperatorKind.LiftedBoolOr);
		BoundExpression rewrittenConsequence = ((kind == BinaryOperatorKind.LiftedBoolOr) ? boundExpression4 : boundExpression3);
		BoundExpression rewrittenAlternative = ((kind == BinaryOperatorKind.LiftedBoolOr) ? boundExpression3 : boundExpression4);
		BoundExpression boundExpression5 = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, boundExpression4.Type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol, boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store, (BoundExpression)store2), boundExpression5, boundExpression5.Type);
	}

	private BoundExpression LowerLiftedBooleanOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight)
	{
		BoundExpression boundExpression = OptimizeLiftedBooleanOperatorOneNull(syntax, kind, loweredLeft, loweredRight);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		boundExpression = OptimizeLiftedBooleanOperatorOneNonNull(syntax, kind, loweredLeft, loweredRight);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(loweredLeft, out var store);
		BoundLocal boundLocal2 = _factory.StoreToTemp(loweredRight, out var store2);
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		MethodSymbol method = UnsafeGetNullableMethod(syntax, boundLocal.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		MethodSymbol method2 = UnsafeGetNullableMethod(syntax, boundLocal2.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		BoundExpression loweredLeft2 = BoundCall.Synthesized(syntax, boundLocal, method);
		BoundExpression loweredLeft3 = BoundCall.Synthesized(syntax, boundLocal2, method2);
		BoundExpression loweredRight2 = MakeNullableHasValue(syntax, boundLocal);
		BoundExpression loweredOperand = MakeBinaryOperator(syntax, BinaryOperatorKind.LogicalBoolOr, loweredLeft3, loweredRight2, specialType, null);
		BoundExpression loweredRight3 = MakeUnaryOperator(UnaryOperatorKind.BoolLogicalNegation, syntax, null, loweredOperand, specialType);
		BoundExpression rewrittenCondition = MakeBinaryOperator(syntax, BinaryOperatorKind.LogicalBoolOr, loweredLeft2, loweredRight3, specialType, null);
		BoundExpression rewrittenConsequence = ((kind == BinaryOperatorKind.LiftedBoolAnd) ? boundLocal2 : boundLocal);
		BoundExpression boundExpression2 = ((kind == BinaryOperatorKind.LiftedBoolAnd) ? boundLocal : boundLocal2);
		BoundExpression boundExpression3 = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, boundExpression2, null, boundExpression2.Type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol, boundLocal2.LocalSymbol), ImmutableArray.Create((BoundExpression)store, (BoundExpression)store2), boundExpression3, boundExpression3.Type);
	}

	private MethodSymbol UnsafeGetNullableMethod(SyntaxNode syntax, TypeSymbol nullableType, SpecialMember member)
	{
		return UnsafeGetNullableMethod(syntax, nullableType, member, _compilation, _diagnostics);
	}

	private static MethodSymbol UnsafeGetNullableMethod(SyntaxNode syntax, TypeSymbol nullableType, SpecialMember member, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol newOwner = nullableType as NamedTypeSymbol;
		return UnsafeGetSpecialTypeMethod(syntax, member, compilation, diagnostics).AsMember(newOwner);
	}

	private bool TryGetNullableMethod(SyntaxNode syntax, TypeSymbol nullableType, SpecialMember member, out MethodSymbol result)
	{
		NamedTypeSymbol newOwner = (NamedTypeSymbol)nullableType;
		if (TryGetSpecialTypeMethod(syntax, member, out result))
		{
			result = result.AsMember(newOwner);
			return true;
		}
		return false;
	}

	private BoundExpression RewriteNullableNullEquality(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol returnType)
	{
		BoundExpression boundExpression = (loweredRight.IsLiteralNull() ? loweredLeft : loweredRight);
		if (NullableNeverHasValue(boundExpression))
		{
			return MakeLiteral(syntax, ConstantValue.Create(kind == BinaryOperatorKind.NullableNullEqual), returnType);
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(boundExpression);
		if (boundExpression2 != null)
		{
			return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundExpression2), MakeBooleanConstant(syntax, kind == BinaryOperatorKind.NullableNullNotEqual), returnType);
		}
		if (boundExpression is BoundLoweredConditionalAccess boundLoweredConditionalAccess && (boundLoweredConditionalAccess.WhenNullOpt == null || boundLoweredConditionalAccess.WhenNullOpt.IsDefaultValue()))
		{
			BoundExpression boundExpression3 = RewriteNullableNullEquality(syntax, kind, boundLoweredConditionalAccess.WhenNotNull, loweredLeft.IsLiteralNull() ? loweredLeft : loweredRight, returnType);
			BoundExpression whenNullOpt = ((kind == BinaryOperatorKind.NullableNullEqual) ? MakeBooleanConstant(syntax, value: true) : null);
			return boundLoweredConditionalAccess.Update(boundLoweredConditionalAccess.Receiver, boundLoweredConditionalAccess.HasValueMethodOpt, boundExpression3, whenNullOpt, boundLoweredConditionalAccess.Id, boundExpression3.Type);
		}
		BoundExpression boundExpression4 = MakeNullableHasValue(syntax, boundExpression);
		if (kind != BinaryOperatorKind.NullableNullNotEqual)
		{
			return new BoundUnaryOperator(syntax, UnaryOperatorKind.BoolLogicalNegation, boundExpression4, null, null, LookupResultKind.Viable, returnType);
		}
		return boundExpression4;
	}

	private BoundExpression RewriteStringEquality(BoundBinaryOperator oldNode, SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, SpecialMember member)
	{
		if (oldNode != null && (loweredLeft.ConstantValue == ConstantValue.Null || loweredRight.ConstantValue == ConstantValue.Null))
		{
			return oldNode.Update(operatorKind, loweredLeft, loweredRight, oldNode.ConstantValueOpt, oldNode.MethodOpt, oldNode.ResultKind, type);
		}
		MethodSymbol method = UnsafeGetSpecialTypeMethod(syntax, member);
		return BoundCall.Synthesized(syntax, null, method, loweredLeft, loweredRight);
	}

	private BoundExpression RewriteDelegateOperation(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, SpecialMember member)
	{
		MethodSymbol methodSymbol;
		if (operatorKind == BinaryOperatorKind.DelegateEqual || operatorKind == BinaryOperatorKind.DelegateNotEqual)
		{
			_ = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(member);
			if (loweredRight.IsLiteralNull() || loweredLeft.IsLiteralNull() || (object)(methodSymbol = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(member)) == null)
			{
				operatorKind = (operatorKind & ~BinaryOperatorKind.Delegate) | BinaryOperatorKind.Object;
				return new BoundBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, null, null, LookupResultKind.Empty, type);
			}
		}
		else
		{
			methodSymbol = UnsafeGetSpecialTypeMethod(syntax, member);
		}
		BoundExpression boundExpression = (_inExpressionLambda ? ((BoundExpression)new BoundBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, null, methodSymbol, LookupResultKind.Empty, methodSymbol.ReturnType)) : ((BoundExpression)BoundCall.Synthesized(syntax, null, methodSymbol, loweredLeft, loweredRight)));
		if (methodSymbol.ReturnType.SpecialType != SpecialType.System_Delegate)
		{
			return boundExpression;
		}
		return MakeConversionNode(syntax, boundExpression, Conversion.ExplicitReference, type, @checked: false);
	}

	private BoundExpression RewriteDecimalBinaryOperation(SyntaxNode syntax, BoundExpression loweredLeft, BoundExpression loweredRight, BinaryOperatorKind operatorKind)
	{
		MethodSymbol method = UnsafeGetSpecialTypeMethod(syntax, operatorKind switch
		{
			BinaryOperatorKind.DecimalAddition => SpecialMember.System_Decimal__op_Addition, 
			BinaryOperatorKind.DecimalSubtraction => SpecialMember.System_Decimal__op_Subtraction, 
			BinaryOperatorKind.DecimalMultiplication => SpecialMember.System_Decimal__op_Multiply, 
			BinaryOperatorKind.DecimalDivision => SpecialMember.System_Decimal__op_Division, 
			BinaryOperatorKind.DecimalRemainder => SpecialMember.System_Decimal__op_Modulus, 
			BinaryOperatorKind.DecimalEqual => SpecialMember.System_Decimal__op_Equality, 
			BinaryOperatorKind.DecimalNotEqual => SpecialMember.System_Decimal__op_Inequality, 
			BinaryOperatorKind.DecimalLessThan => SpecialMember.System_Decimal__op_LessThan, 
			BinaryOperatorKind.DecimalLessThanOrEqual => SpecialMember.System_Decimal__op_LessThanOrEqual, 
			BinaryOperatorKind.DecimalGreaterThan => SpecialMember.System_Decimal__op_GreaterThan, 
			BinaryOperatorKind.DecimalGreaterThanOrEqual => SpecialMember.System_Decimal__op_GreaterThanOrEqual, 
			_ => throw ExceptionUtilities.UnexpectedValue(operatorKind), 
		});
		return BoundCall.Synthesized(syntax, null, method, loweredLeft, loweredRight);
	}

	private BoundExpression MakeNullCheck(SyntaxNode syntax, BoundExpression rewrittenExpr, BinaryOperatorKind operatorKind)
	{
		TypeSymbol type = rewrittenExpr.Type;
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
		if (rewrittenExpr.ConstantValue != null)
		{
			switch (operatorKind)
			{
			case BinaryOperatorKind.Equal:
				return MakeLiteral(syntax, ConstantValue.Create(rewrittenExpr.ConstantValue.IsNull, ConstantValueTypeDiscriminator.Boolean), specialType);
			case BinaryOperatorKind.NotEqual:
				return MakeLiteral(syntax, ConstantValue.Create(!rewrittenExpr.ConstantValue.IsNull, ConstantValueTypeDiscriminator.Boolean), specialType);
			}
		}
		TypeSymbol specialType2 = _compilation.GetSpecialType(SpecialType.System_Object);
		if ((object)type != null)
		{
			if (type.Kind == SymbolKind.TypeParameter)
			{
				rewrittenExpr = MakeConversionNode(syntax, rewrittenExpr, Conversion.Boxing, specialType2, @checked: false);
			}
			else if (type.IsNullableType())
			{
				operatorKind |= BinaryOperatorKind.NullableNull;
			}
		}
		return MakeBinaryOperator(syntax, operatorKind, rewrittenExpr, MakeLiteral(syntax, ConstantValue.Null, specialType2), specialType, null);
	}

	private BoundExpression RewriteBuiltInShiftOperation(BoundBinaryOperator oldNode, SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type, int rightMask)
	{
		SyntaxNode syntax2 = loweredRight.Syntax;
		ConstantValue constantValue = loweredRight.ConstantValue;
		TypeSymbol type2 = loweredRight.Type;
		if (constantValue != null && constantValue.IsIntegral)
		{
			int num = constantValue.Int32Value & rightMask;
			if (num == 0)
			{
				return loweredLeft;
			}
			loweredRight = MakeLiteral(syntax2, ConstantValue.Create(num), type2);
		}
		else
		{
			BinaryOperatorKind operatorKind2 = (operatorKind & ~BinaryOperatorKind.OpMask) | BinaryOperatorKind.And;
			loweredRight = new BoundBinaryOperator(syntax2, operatorKind2, loweredRight, MakeLiteral(syntax2, ConstantValue.Create(rightMask), type2), null, null, LookupResultKind.Viable, type2);
		}
		if (oldNode != null)
		{
			return oldNode.Update(operatorKind, loweredLeft, loweredRight, null, null, oldNode.ResultKind, type);
		}
		return new BoundBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, null, null, LookupResultKind.Viable, type);
	}

	private BoundExpression RewritePointerNumericOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol returnType, bool isPointerElementAccess, bool isLeftPointer)
	{
		if (isLeftPointer)
		{
			loweredRight = MakeSizeOfMultiplication(loweredRight, (PointerTypeSymbol)loweredLeft.Type, kind.IsChecked());
		}
		else
		{
			loweredLeft = MakeSizeOfMultiplication(loweredLeft, (PointerTypeSymbol)loweredRight.Type, kind.IsChecked());
		}
		if (isPointerElementAccess)
		{
			kind &= ~BinaryOperatorKind.Checked;
		}
		return new BoundBinaryOperator(syntax, kind, loweredLeft, loweredRight, null, null, LookupResultKind.Viable, returnType);
	}

	private BoundExpression MakeSizeOfMultiplication(BoundExpression numericOperand, PointerTypeSymbol pointerType, bool isChecked)
	{
		BoundExpression boundExpression = _factory.Sizeof(pointerType.PointedAtType);
		ConstantValue constantValue = numericOperand.ConstantValue;
		if ((object)constantValue != null && constantValue.UInt64Value == 1)
		{
			return boundExpression;
		}
		SpecialType specialType = numericOperand.Type.SpecialType;
		ConstantValue constantValue2 = boundExpression.ConstantValue;
		if ((object)constantValue2 != null && constantValue2.Int32Value == 1)
		{
			SpecialType specialType2 = specialType;
			switch (specialType)
			{
			case SpecialType.System_Int32:
				if (isChecked)
				{
					ConstantValue constantValue4 = numericOperand.ConstantValue;
					if (constantValue4 == null || constantValue4.Int32Value < 0)
					{
						specialType2 = SpecialType.System_IntPtr;
					}
				}
				break;
			case SpecialType.System_UInt32:
			{
				ConstantValue constantValue3 = numericOperand.ConstantValue;
				if (constantValue3 == null || constantValue3.UInt32Value > int.MaxValue)
				{
					specialType2 = SpecialType.System_UIntPtr;
				}
				break;
			}
			case SpecialType.System_Int64:
				specialType2 = SpecialType.System_IntPtr;
				break;
			case SpecialType.System_UInt64:
				specialType2 = SpecialType.System_UIntPtr;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(specialType);
			}
			if (specialType2 != specialType)
			{
				return _factory.Convert(_factory.SpecialType(specialType2), numericOperand, Conversion.IntegerToPointer);
			}
			return numericOperand;
		}
		BinaryOperatorKind binaryOperatorKind = BinaryOperatorKind.Multiplication;
		TypeSymbol typeSymbol2;
		TypeSymbol typeSymbol3;
		switch (specialType)
		{
		case SpecialType.System_Int32:
		{
			TypeSymbol typeSymbol6 = _factory.SpecialType(SpecialType.System_IntPtr);
			numericOperand = _factory.Convert(typeSymbol6, numericOperand, Conversion.IntegerToPointer, isChecked);
			binaryOperatorKind |= BinaryOperatorKind.Int;
			typeSymbol2 = typeSymbol6;
			typeSymbol3 = typeSymbol6;
			break;
		}
		case SpecialType.System_UInt32:
		{
			TypeSymbol typeSymbol5 = _factory.SpecialType(SpecialType.System_Int64);
			NamedTypeSymbol namedTypeSymbol3 = _factory.SpecialType(SpecialType.System_IntPtr);
			numericOperand = _factory.Convert(typeSymbol5, numericOperand, Conversion.ExplicitNumeric, isChecked);
			boundExpression = _factory.Convert(typeSymbol5, boundExpression, Conversion.ExplicitNumeric, isChecked);
			binaryOperatorKind |= BinaryOperatorKind.Long;
			typeSymbol2 = typeSymbol5;
			typeSymbol3 = namedTypeSymbol3;
			break;
		}
		case SpecialType.System_Int64:
		{
			TypeSymbol typeSymbol4 = _factory.SpecialType(SpecialType.System_Int64);
			NamedTypeSymbol namedTypeSymbol2 = _factory.SpecialType(SpecialType.System_IntPtr);
			boundExpression = _factory.Convert(typeSymbol4, boundExpression, Conversion.ExplicitNumeric, isChecked);
			binaryOperatorKind |= BinaryOperatorKind.Long;
			typeSymbol2 = typeSymbol4;
			typeSymbol3 = namedTypeSymbol2;
			break;
		}
		case SpecialType.System_UInt64:
		{
			TypeSymbol typeSymbol = _factory.SpecialType(SpecialType.System_UInt64);
			NamedTypeSymbol namedTypeSymbol = _factory.SpecialType(SpecialType.System_UIntPtr);
			boundExpression = _factory.Convert(typeSymbol, boundExpression, Conversion.ExplicitNumeric, isChecked);
			binaryOperatorKind |= BinaryOperatorKind.ULong;
			typeSymbol2 = typeSymbol;
			typeSymbol3 = namedTypeSymbol;
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(specialType);
		}
		if (isChecked)
		{
			binaryOperatorKind |= BinaryOperatorKind.Checked;
		}
		BoundBinaryOperator boundBinaryOperator = _factory.Binary(binaryOperatorKind, typeSymbol2, numericOperand, boundExpression);
		if (!(typeSymbol3 == typeSymbol2))
		{
			return _factory.Convert(typeSymbol3, boundBinaryOperator, Conversion.IntegerToPointer);
		}
		return boundBinaryOperator;
	}

	private BoundExpression RewritePointerSubtraction(BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol returnType)
	{
		PointerTypeSymbol pointerTypeSymbol = (PointerTypeSymbol)loweredLeft.Type;
		BoundExpression right = _factory.Sizeof(pointerTypeSymbol.PointedAtType);
		return _factory.Convert(returnType, _factory.Binary(BinaryOperatorKind.Division, _factory.SpecialType(SpecialType.System_IntPtr), _factory.Binary(kind & ~BinaryOperatorKind.Checked, returnType, loweredLeft, loweredRight), right), Conversion.PointerToInteger);
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		if (!Instrument || (node != _rootStatement && (node.WasCompilerGenerated || node.Syntax.Kind() != SyntaxKind.Block)))
		{
			return node.Update(node.Locals, node.LocalFunctions, VisitList(node.Statements));
		}
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		for (int i = 0; i < node.Statements.Length; i++)
		{
			BoundStatement boundStatement = (BoundStatement)Visit(node.Statements[i]);
			if (boundStatement != null)
			{
				instance.Add(boundStatement);
			}
		}
		BoundStatement boundStatement2 = _instrumenter.CreateBlockPrologue(node, out var synthesizedLocal);
		if (boundStatement2 != null)
		{
			instance.Insert(0, boundStatement2);
		}
		BoundStatement boundStatement3 = _instrumenter.CreateBlockEpilogue(node);
		if (boundStatement3 != null)
		{
			instance.Add(boundStatement3);
		}
		return new BoundBlock(node.Syntax, (synthesizedLocal == null) ? node.Locals : node.Locals.Add(synthesizedLocal), node.LocalFunctions, instance.ToImmutableAndFree(), node.HasErrors);
	}

	public override BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		if (!node.WasCompilerGenerated && Instrument)
		{
			return _instrumenter.InstrumentNoOpStatement(node, node);
		}
		return new BoundBlock(node.Syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray<BoundStatement>.Empty);
	}

	public override BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		BoundStatement boundStatement = new BoundGotoStatement(node.Syntax, node.Label, node.HasErrors);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = _instrumenter.InstrumentBreakStatement(node, boundStatement);
		}
		return boundStatement;
	}

	public override BoundNode VisitDynamicInvocation(BoundDynamicInvocation node)
	{
		return VisitDynamicInvocation(node, resultDiscarded: false);
	}

	public BoundExpression VisitDynamicInvocation(BoundDynamicInvocation node, bool resultDiscarded)
	{
		ImmutableArray<BoundExpression> loweredArguments = VisitList(node.Arguments);
		ImmutableArray<TypeSymbol> typeArgumentsOpt;
		string name;
		bool flag;
		BoundExpression boundExpression;
		switch (node.Expression.Kind)
		{
		case BoundKind.MethodGroup:
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)node.Expression;
			typeArgumentsOpt = boundMethodGroup.TypeArgumentsOpt;
			name = boundMethodGroup.Name;
			flag = (boundMethodGroup.Flags & BoundMethodGroupFlags.HasImplicitReceiver) != 0;
			if (boundMethodGroup.ReceiverOpt != null)
			{
				boundExpression = ((!flag || !_factory.TopLevelMethod.IsStatic) ? VisitExpression(boundMethodGroup.ReceiverOpt) : new BoundTypeExpression(node.Syntax, null, _factory.CurrentType));
			}
			else
			{
				NamedTypeSymbol containingType = node.ApplicableMethods.First().ContainingType;
				boundExpression = new BoundTypeExpression(node.Syntax, null, containingType);
			}
			EmbedIfNeedTo(boundExpression, boundMethodGroup.Methods, node.Syntax);
			break;
		}
		case BoundKind.DynamicMemberAccess:
		{
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)node.Expression;
			name = boundDynamicMemberAccess.Name;
			typeArgumentsOpt = boundDynamicMemberAccess.TypeArgumentsOpt;
			boundExpression = VisitExpression(boundDynamicMemberAccess.Receiver);
			flag = false;
			break;
		}
		default:
		{
			BoundExpression loweredReceiver = VisitExpression(node.Expression);
			return _dynamicFactory.MakeDynamicInvocation(loweredReceiver, loweredArguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, resultDiscarded).ToExpression();
		}
		}
		return _dynamicFactory.MakeDynamicMemberInvocation(name, boundExpression, typeArgumentsOpt, loweredArguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, flag, resultDiscarded).ToExpression();
	}

	private void EmbedIfNeedTo(BoundExpression receiver, ImmutableArray<MethodSymbol> methods, SyntaxNode syntaxNode)
	{
		PEModuleBuilder emitModule = EmitModule;
		if (emitModule == null || receiver == null || (object)receiver.Type == null)
		{
			return;
		}
		AssemblySymbol containingAssembly = receiver.Type.ContainingAssembly;
		if ((object)containingAssembly != null && containingAssembly.IsLinked)
		{
			foreach (MethodSymbol item in methods)
			{
				emitModule.EmbeddedTypesManagerOpt.EmbedMethodIfNeedTo(item.OriginalDefinition, syntaxNode, _diagnostics);
			}
		}
	}

	private void EmbedIfNeedTo(BoundExpression receiver, ImmutableArray<PropertySymbol> properties, SyntaxNode syntaxNode)
	{
		PEModuleBuilder emitModule = EmitModule;
		if (emitModule == null || receiver == null || (object)receiver.Type == null)
		{
			return;
		}
		AssemblySymbol containingAssembly = receiver.Type.ContainingAssembly;
		if ((object)containingAssembly != null && containingAssembly.IsLinked)
		{
			foreach (PropertySymbol item in properties)
			{
				emitModule.EmbeddedTypesManagerOpt.EmbedPropertyIfNeedTo(item.OriginalDefinition, syntaxNode, _diagnostics);
			}
		}
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		BoundExpression rewrittenReceiver = VisitExpression(node.ReceiverOpt);
		ImmutableArray<BoundExpression> rewrittenArguments = VisitList(node.Arguments);
		return MakeCall(node.Syntax, rewrittenReceiver, node.Method, rewrittenArguments, node.ArgumentRefKindsOpt, node.Expanded, node.InvokedAsExtensionMethod, node.ArgsToParamsOpt, node.ResultKind, node.Type, node);
	}

	private BoundExpression MakeCall(SyntaxNode syntax, BoundExpression rewrittenReceiver, MethodSymbol method, ImmutableArray<BoundExpression> rewrittenArguments, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, bool invokedAsExtensionMethod, ImmutableArray<int> argsToParamsOpt, LookupResultKind resultKind, TypeSymbol type, BoundCall nodeOpt = null)
	{
		rewrittenArguments = MakeArguments(syntax, rewrittenArguments, method, method, expanded, argsToParamsOpt, ref argumentRefKindsOpt, out var temps, invokedAsExtensionMethod);
		return MakeCall(nodeOpt, syntax, rewrittenReceiver, method, rewrittenArguments, argumentRefKindsOpt, invokedAsExtensionMethod, resultKind, type, temps);
	}

	private BoundExpression MakeCall(BoundCall node, SyntaxNode syntax, BoundExpression rewrittenReceiver, MethodSymbol method, ImmutableArray<BoundExpression> rewrittenArguments, ImmutableArray<RefKind> argumentRefKinds, bool invokedAsExtensionMethod, LookupResultKind resultKind, TypeSymbol type, ImmutableArray<LocalSymbol> temps = default(ImmutableArray<LocalSymbol>))
	{
		BoundExpression boundExpression;
		if (method.IsStatic && method.ContainingType.IsObjectType() && !_inExpressionLambda && (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_Object__ReferenceEquals))
		{
			boundExpression = new BoundBinaryOperator(syntax, BinaryOperatorKind.ObjectEqual, rewrittenArguments[0], rewrittenArguments[1], null, null, resultKind, type);
		}
		else
		{
			boundExpression = ((node != null) ? node.Update(rewrittenReceiver, method, rewrittenArguments, default, argumentRefKinds, node.IsDelegateCall, expanded: false, node.InvokedAsExtensionMethod, default, node.ResultKind, node.BinderOpt, node.Type) : new BoundCall(syntax, rewrittenReceiver, method, rewrittenArguments, default, argumentRefKinds, isDelegateCall: false, expanded: false, invokedAsExtensionMethod, default, resultKind, null, type));
		}
		if (!temps.IsDefaultOrEmpty)
		{
			return new BoundSequence(syntax, temps, ImmutableArray<BoundExpression>.Empty, boundExpression, type);
		}
		return boundExpression;
	}

	private BoundExpression MakeCall(SyntaxNode syntax, BoundExpression rewrittenReceiver, MethodSymbol method, ImmutableArray<BoundExpression> rewrittenArguments, TypeSymbol type)
	{
		return MakeCall(null, syntax, rewrittenReceiver, method, rewrittenArguments, default, invokedAsExtensionMethod: false, LookupResultKind.Viable, type);
	}

	private static bool IsSafeForReordering(BoundExpression expression, RefKind kind)
	{
		BoundExpression boundExpression = expression;
		while (!(boundExpression.ConstantValue != null))
		{
			switch (boundExpression.Kind)
			{
			default:
				return false;
			case BoundKind.Local:
			case BoundKind.Parameter:
				return kind != RefKind.None;
			case BoundKind.PassByCopy:
				return IsSafeForReordering(((BoundPassByCopy)boundExpression).Expression, kind);
			case BoundKind.Conversion:
			{
				BoundConversion boundConversion = (BoundConversion)boundExpression;
				switch (boundConversion.ConversionKind)
				{
				case ConversionKind.DefaultOrNullLiteral:
				case ConversionKind.ImplicitConstant:
				case ConversionKind.AnonymousFunction:
				case ConversionKind.MethodGroup:
					return true;
				case ConversionKind.Identity:
				case ConversionKind.ImplicitNumeric:
				case ConversionKind.ImplicitEnumeration:
				case ConversionKind.ImplicitNullable:
				case ConversionKind.ImplicitReference:
				case ConversionKind.Boxing:
				case ConversionKind.PointerToVoid:
				case ConversionKind.NullToPointer:
				case ConversionKind.ImplicitDynamic:
				case ConversionKind.ExplicitDynamic:
				case ConversionKind.ExplicitNumeric:
				case ConversionKind.ExplicitEnumeration:
				case ConversionKind.ExplicitNullable:
				case ConversionKind.ExplicitReference:
				case ConversionKind.Unboxing:
				case ConversionKind.PointerToPointer:
				case ConversionKind.IntegerToPointer:
				case ConversionKind.PointerToInteger:
					break;
				case ConversionKind.ImplicitThrow:
				case ConversionKind.ImplicitUserDefined:
				case ConversionKind.ExplicitUserDefined:
				case ConversionKind.IntPtr:
					return false;
				default:
					return false;
				}
				boundExpression = boundConversion.Operand;
				break;
			}
			}
		}
		return true;
	}

	private ImmutableArray<BoundExpression> MakeArguments(SyntaxNode syntax, ImmutableArray<BoundExpression> rewrittenArguments, Symbol methodOrIndexer, MethodSymbol optionalParametersMethod, bool expanded, ImmutableArray<int> argsToParamsOpt, ref ImmutableArray<RefKind> argumentRefKindsOpt, out ImmutableArray<LocalSymbol> temps, bool invokedAsExtensionMethod = false, ThreeState enableCallerInfo = ThreeState.Unknown)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		rewrittenArguments = _factory.MakeTempsForDiscardArguments(rewrittenArguments, instance);
		ImmutableArray<ParameterSymbol> parameters = methodOrIndexer.GetParameters();
		if (CanSkipRewriting(rewrittenArguments, methodOrIndexer, expanded, argsToParamsOpt, invokedAsExtensionMethod, ignoreComReceiver: false, out var isComReceiver))
		{
			temps = instance.ToImmutableAndFree();
			argumentRefKindsOpt = GetEffectiveArgumentRefKinds(argumentRefKindsOpt, parameters);
			return rewrittenArguments;
		}
		BoundExpression[] array = new BoundExpression[parameters.Length];
		ArrayBuilder<BoundAssignmentOperator> instance2 = ArrayBuilder<BoundAssignmentOperator>.GetInstance(rewrittenArguments.Length);
		ArrayBuilder<RefKind> instance3 = ArrayBuilder<RefKind>.GetInstance(parameters.Length, RefKind.None);
		BuildStoresToTemps(expanded, argsToParamsOpt, parameters, argumentRefKindsOpt, rewrittenArguments, forceLambdaSpilling: false, array, instance3, instance2);
		OptimizeTemporaries(array, instance2, instance);
		if (expanded)
		{
			array[^1] = BuildParamsArray(syntax, methodOrIndexer, argsToParamsOpt, rewrittenArguments, parameters, array[^1]);
		}
		InsertMissingOptionalArguments(syntax, optionalParametersMethod.Parameters, array, instance3, enableCallerInfo);
		if (isComReceiver)
		{
			RewriteArgumentsForComCall(parameters, array, instance3, instance);
		}
		temps = instance.ToImmutableAndFree();
		instance2.Free();
		argumentRefKindsOpt = GetRefKindsOrNull(instance3);
		instance3.Free();
		return array.AsImmutableOrNull();
	}

	private static ImmutableArray<RefKind> GetEffectiveArgumentRefKinds(ImmutableArray<RefKind> argumentRefKindsOpt, ImmutableArray<ParameterSymbol> parameters)
	{
		ArrayBuilder<RefKind> arrayBuilder = null;
		for (int i = 0; i < parameters.Length; i++)
		{
			RefKind refKind = parameters[i].RefKind;
			if (refKind != RefKind.In)
			{
				continue;
			}
			RefKind refKind2 = ((!argumentRefKindsOpt.IsDefault) ? argumentRefKindsOpt[i] : RefKind.None);
			if (arrayBuilder == null)
			{
				if (!argumentRefKindsOpt.IsDefault)
				{
					arrayBuilder = ArrayBuilder<RefKind>.GetInstance(parameters.Length);
					arrayBuilder.AddRange(argumentRefKindsOpt);
				}
				else
				{
					arrayBuilder = ArrayBuilder<RefKind>.GetInstance(parameters.Length, RefKind.None);
				}
			}
			arrayBuilder[i] = ((refKind2 == RefKind.None) ? refKind : ((RefKind)4));
		}
		if (arrayBuilder != null)
		{
			argumentRefKindsOpt = arrayBuilder.ToImmutableAndFree();
		}
		return argumentRefKindsOpt;
	}

	internal static ImmutableArray<IArgumentOperation> MakeArgumentsInEvaluationOrder(CSharpOperationFactory operationFactory, Binder binder, SyntaxNode syntax, ImmutableArray<BoundExpression> arguments, Symbol methodOrIndexer, MethodSymbol optionalParametersMethod, bool expanded, ImmutableArray<int> argsToParamsOpt, bool invokedAsExtensionMethod)
	{
		if (CanSkipRewriting(arguments, methodOrIndexer, expanded, argsToParamsOpt, invokedAsExtensionMethod, ignoreComReceiver: true, out var _))
		{
			ImmutableArray<ParameterSymbol> parameters = methodOrIndexer.GetParameters();
			ArrayBuilder<IArgumentOperation> instance = ArrayBuilder<IArgumentOperation>.GetInstance(arguments.Length);
			int i;
			for (i = 0; i < parameters.Length; i++)
			{
				instance.Add(operationFactory.CreateArgumentOperation(ArgumentKind.Explicit, parameters[i], arguments[i]));
			}
			for (; i < arguments.Length; i++)
			{
				instance.Add(operationFactory.CreateArgumentOperation(ArgumentKind.Explicit, null, arguments[i]));
			}
			return instance.ToImmutableAndFree();
		}
		return BuildArgumentsInEvaluationOrder(operationFactory, syntax, methodOrIndexer, optionalParametersMethod, expanded, argsToParamsOpt, arguments, binder);
	}

	private static bool CanSkipRewriting(ImmutableArray<BoundExpression> rewrittenArguments, Symbol methodOrIndexer, bool expanded, ImmutableArray<int> argsToParamsOpt, bool invokedAsExtensionMethod, bool ignoreComReceiver, out bool isComReceiver)
	{
		isComReceiver = false;
		if (methodOrIndexer.GetIsVararg())
		{
			return true;
		}
		if (!ignoreComReceiver)
		{
			isComReceiver = (invokedAsExtensionMethod ? (((MethodSymbol)methodOrIndexer).Parameters[0].Type as NamedTypeSymbol) : methodOrIndexer.ContainingType)?.IsComImport ?? false;
		}
		if (rewrittenArguments.Length == methodOrIndexer.GetParameterCount() && argsToParamsOpt.IsDefault && !expanded)
		{
			return !isComReceiver;
		}
		return false;
	}

	private static ImmutableArray<RefKind> GetRefKindsOrNull(ArrayBuilder<RefKind> refKinds)
	{
		foreach (RefKind refKind in refKinds)
		{
			if (refKind != RefKind.None)
			{
				return refKinds.ToImmutable();
			}
		}
		return default;
	}

	private void BuildStoresToTemps(bool expanded, ImmutableArray<int> argsToParamsOpt, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<RefKind> argumentRefKinds, ImmutableArray<BoundExpression> rewrittenArguments, bool forceLambdaSpilling, BoundExpression[] arguments, ArrayBuilder<RefKind> refKinds, ArrayBuilder<BoundAssignmentOperator> storesToTemps)
	{
		for (int i = 0; i < rewrittenArguments.Length; i++)
		{
			BoundExpression boundExpression = rewrittenArguments[i];
			int num = ((!argsToParamsOpt.IsDefault) ? argsToParamsOpt[i] : i);
			RefKind refKind = argumentRefKinds.RefKinds(i);
			RefKind refKind2 = parameters[num].RefKind;
			if (refKind2 == RefKind.In)
			{
				refKind = ((refKind == RefKind.None) ? refKind2 : ((RefKind)4));
			}
			if (IsBeginningOfParamArray(num, i, expanded, arguments.Length, rewrittenArguments, argsToParamsOpt, out var numberOfParamArrayArguments) && i + numberOfParamArrayArguments == rewrittenArguments.Length)
			{
				break;
			}
			if ((!forceLambdaSpilling || !isLambdaConversion(boundExpression)) && IsSafeForReordering(boundExpression, refKind))
			{
				arguments[num] = boundExpression;
			}
			else
			{
				BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store, refKind);
				storesToTemps.Add(store);
				arguments[num] = boundLocal;
			}
			refKinds[num] = refKind;
		}
		static bool isLambdaConversion(BoundExpression expr)
		{
			if (expr is BoundConversion boundConversion)
			{
				return boundConversion.ConversionKind == ConversionKind.AnonymousFunction;
			}
			return false;
		}
	}

	private static ImmutableArray<IArgumentOperation> BuildArgumentsInEvaluationOrder(CSharpOperationFactory operationFactory, SyntaxNode syntax, Symbol methodOrIndexer, MethodSymbol optionalParametersMethod, bool expanded, ImmutableArray<int> argsToParamsOpt, ImmutableArray<BoundExpression> arguments, Binder binder)
	{
		ImmutableArray<ParameterSymbol> parameters = methodOrIndexer.GetParameters();
		ArrayBuilder<IArgumentOperation> instance = ArrayBuilder<IArgumentOperation>.GetInstance(parameters.Length);
		PooledHashSet<int> instance2 = PooledHashSet<int>.GetInstance();
		for (int i = 0; i < arguments.Length; i++)
		{
			BoundExpression expression = arguments[i];
			int num = ((!argsToParamsOpt.IsDefault) ? argsToParamsOpt[i] : i);
			ParameterSymbol parameterSymbol = parameters[num];
			instance2.Add(num);
			ArgumentKind kind = ArgumentKind.Explicit;
			if (IsBeginningOfParamArray(num, i, expanded, parameters.Length, arguments, argsToParamsOpt, out var numberOfParamArrayArguments))
			{
				int num2 = i + numberOfParamArrayArguments;
				kind = ArgumentKind.ParamArray;
				ArrayBuilder<BoundExpression> instance3 = ArrayBuilder<BoundExpression>.GetInstance(numberOfParamArrayArguments);
				for (int j = i; j < num2; j++)
				{
					instance3.Add(arguments[j]);
				}
				i = num2 - 1;
				expression = CreateParamArrayArgument(syntax, parameterSymbol.Type, instance3.ToImmutableAndFree(), null, binder);
			}
			instance.Add(operationFactory.CreateArgumentOperation(kind, parameterSymbol, expression));
		}
		ArrayBuilder<ParameterSymbol> instance4 = ArrayBuilder<ParameterSymbol>.GetInstance(parameters.Length);
		for (int k = 0; k < parameters.Length; k++)
		{
			if (!instance2.Contains(k))
			{
				instance4.Add(parameters[k]);
			}
		}
		instance2.Free();
		AppendMissingOptionalArguments(operationFactory, syntax, methodOrIndexer, optionalParametersMethod, expanded, binder, instance4, instance);
		instance4.Free();
		return instance.ToImmutableAndFree();
	}

	private static bool IsBeginningOfParamArray(int parameterIndex, int argumentIndex, bool expanded, int parameterCount, ImmutableArray<BoundExpression> arguments, ImmutableArray<int> argsToParamsOpt, out int numberOfParamArrayArguments)
	{
		numberOfParamArrayArguments = 0;
		if (expanded && parameterIndex == parameterCount - 1)
		{
			int i;
			for (i = argumentIndex + 1; i < arguments.Length && ((!argsToParamsOpt.IsDefault) ? argsToParamsOpt[i] : i) == parameterCount - 1; i++)
			{
			}
			numberOfParamArrayArguments = i - argumentIndex;
			return true;
		}
		return false;
	}

	private BoundExpression BuildParamsArray(SyntaxNode syntax, Symbol methodOrIndexer, ImmutableArray<int> argsToParamsOpt, ImmutableArray<BoundExpression> rewrittenArguments, ImmutableArray<ParameterSymbol> parameters, BoundExpression tempStoreArgument)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		int num = parameters.Length - 1;
		if (tempStoreArgument != null)
		{
			instance.Add(tempStoreArgument);
		}
		else
		{
			for (int i = 0; i < rewrittenArguments.Length; i++)
			{
				BoundExpression item = rewrittenArguments[i];
				if (((!argsToParamsOpt.IsDefault) ? argsToParamsOpt[i] : i) == num)
				{
					instance.Add(item);
				}
			}
		}
		TypeSymbol type = parameters[num].Type;
		ImmutableArray<BoundExpression> arrayArgs = instance.ToImmutableAndFree();
		if (arrayArgs.Length == 0 && !_inExpressionLambda)
		{
			ArrayTypeSymbol arrayTypeSymbol = type as ArrayTypeSymbol;
			if (arrayTypeSymbol != null)
			{
				MethodSymbol methodSymbol = _compilation.GetWellKnownTypeMember(WellKnownMember.System_Array__Empty) as MethodSymbol;
				if (methodSymbol != null)
				{
					methodSymbol = methodSymbol.Construct(ImmutableArray.Create(arrayTypeSymbol.ElementType));
					return new BoundCall(syntax, null, methodSymbol, ImmutableArray<BoundExpression>.Empty, default, default, isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default, LookupResultKind.Viable, null, methodSymbol.ReturnType);
				}
			}
		}
		return CreateParamArrayArgument(syntax, type, arrayArgs, this, null);
	}

	private static BoundExpression CreateParamArrayArgument(SyntaxNode syntax, TypeSymbol paramArrayType, ImmutableArray<BoundExpression> arrayArgs, LocalRewriter localRewriter, Binder binder)
	{
		TypeSymbol specialType = ((localRewriter != null) ? localRewriter._compilation : binder.Compilation).GetSpecialType(SpecialType.System_Int32);
		BoundExpression item = MakeLiteral(syntax, ConstantValue.Create(arrayArgs.Length), specialType, localRewriter);
		return new BoundArrayCreation(syntax, ImmutableArray.Create(item), new BoundArrayInitialization(syntax, arrayArgs)
		{
			WasCompilerGenerated = true
		}, paramArrayType)
		{
			WasCompilerGenerated = true
		};
	}

	private static BoundExpression MakeLiteral(SyntaxNode syntax, ConstantValue constantValue, TypeSymbol type, LocalRewriter localRewriter)
	{
		if (localRewriter != null)
		{
			return localRewriter.MakeLiteral(syntax, constantValue, type);
		}
		return new BoundLiteral(syntax, constantValue, type, constantValue.IsBad)
		{
			WasCompilerGenerated = true
		};
	}

	private static void OptimizeTemporaries(BoundExpression[] arguments, ArrayBuilder<BoundAssignmentOperator> storesToTemps, ArrayBuilder<LocalSymbol> temporariesBuilder)
	{
		if (storesToTemps.Count <= 0 || MergeArgumentsAndSideEffects(arguments, storesToTemps) <= 0)
		{
			return;
		}
		foreach (BoundAssignmentOperator storesToTemp in storesToTemps)
		{
			if (storesToTemp != null)
			{
				temporariesBuilder.Add(((BoundLocal)storesToTemp.Left).LocalSymbol);
			}
		}
	}

	private static int MergeArgumentsAndSideEffects(BoundExpression[] arguments, ArrayBuilder<BoundAssignmentOperator> tempStores)
	{
		int num = tempStores.Count;
		int num2 = 0;
		for (int i = 0; i < arguments.Length; i++)
		{
			BoundExpression boundExpression = arguments[i];
			if (boundExpression == null || boundExpression.Kind != BoundKind.Local)
			{
				continue;
			}
			int num3 = -1;
			for (int j = num2; j < tempStores.Count; j++)
			{
				if (tempStores[j].Left == boundExpression)
				{
					num3 = j;
					break;
				}
			}
			if (num3 == -1)
			{
				continue;
			}
			BoundExpression right = tempStores[num3].Right;
			tempStores[num3] = null;
			num--;
			if (num3 == num2)
			{
				arguments[i] = right;
			}
			else
			{
				BoundExpression[] array = new BoundExpression[num3 - num2];
				for (int k = 0; k < array.Length; k++)
				{
					array[k] = tempStores[num2 + k];
				}
				arguments[i] = new BoundSequence(right.Syntax, ImmutableArray<LocalSymbol>.Empty, array.AsImmutableOrNull(), right, right.Type);
			}
			num2 = num3 + 1;
		}
		return num;
	}

	private void InsertMissingOptionalArguments(SyntaxNode syntax, ImmutableArray<ParameterSymbol> parameters, BoundExpression[] arguments, ArrayBuilder<RefKind> refKinds, ThreeState enableCallerInfo = ThreeState.Unknown)
	{
		for (int i = 0; i < arguments.Length; i++)
		{
			if (arguments[i] == null)
			{
				ParameterSymbol parameter = parameters[i];
				arguments[i] = GetDefaultParameterValue(syntax, parameter, enableCallerInfo);
				if (parameters[i].RefKind == RefKind.In)
				{
					refKinds[i] = RefKind.In;
				}
			}
		}
	}

	private static void AppendMissingOptionalArguments(CSharpOperationFactory operationFactory, SyntaxNode syntax, Symbol methodOrIndexer, MethodSymbol optionalParametersMethod, bool expanded, Binder binder, ArrayBuilder<ParameterSymbol> missingParameters, ArrayBuilder<IArgumentOperation> argumentsBuilder)
	{
		ImmutableArray<ParameterSymbol> parameters = methodOrIndexer.GetParameters();
		ImmutableArray<ParameterSymbol> parameters2 = optionalParametersMethod.Parameters;
		foreach (ParameterSymbol missingParameter in missingParameters)
		{
			ParameterSymbol parameterSymbol = parameters2[missingParameter.Ordinal];
			BoundExpression expression;
			ArgumentKind kind;
			if (expanded && parameterSymbol.Ordinal == parameters.Length - 1)
			{
				expression = CreateParamArrayArgument(syntax, parameterSymbol.Type, ImmutableArray<BoundExpression>.Empty, null, binder);
				kind = ArgumentKind.ParamArray;
			}
			else
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				expression = GetDefaultParameterValue(syntax, parameterSymbol, ThreeState.Unknown, null, binder, instance);
				kind = ArgumentKind.DefaultValue;
				instance.Free();
			}
			argumentsBuilder.Add(operationFactory.CreateArgumentOperation(kind, missingParameter, expression));
		}
	}

	private static SourceLocation GetCallerLocation(SyntaxNode syntax, ThreeState enableCallerInfo)
	{
		switch (enableCallerInfo)
		{
		case ThreeState.False:
			return null;
		case ThreeState.True:
			return new SourceLocation(syntax.GetFirstToken());
		default:
			switch (syntax.Kind())
			{
			case SyntaxKind.InvocationExpression:
				return new SourceLocation(((InvocationExpressionSyntax)syntax).ArgumentList.OpenParenToken);
			case SyntaxKind.ObjectCreationExpression:
				return new SourceLocation(((ObjectCreationExpressionSyntax)syntax).NewKeyword);
			case SyntaxKind.BaseConstructorInitializer:
			case SyntaxKind.ThisConstructorInitializer:
				return new SourceLocation(((ConstructorInitializerSyntax)syntax).ArgumentList.OpenParenToken);
			case SyntaxKind.ElementAccessExpression:
				return new SourceLocation(((ElementAccessExpressionSyntax)syntax).ArgumentList.OpenBracketToken);
			case SyntaxKind.FromClause:
			case SyntaxKind.LetClause:
			case SyntaxKind.JoinClause:
			case SyntaxKind.JoinIntoClause:
			case SyntaxKind.WhereClause:
			case SyntaxKind.OrderByClause:
			case SyntaxKind.SelectClause:
			case SyntaxKind.GroupClause:
				return new SourceLocation(syntax.GetFirstToken());
			default:
				return null;
			}
		}
	}

	private BoundExpression GetDefaultParameterValue(SyntaxNode syntax, ParameterSymbol parameter, ThreeState enableCallerInfo)
	{
		return GetDefaultParameterValue(syntax, parameter, enableCallerInfo, this, null, _diagnostics);
	}

	private static BoundExpression GetDefaultParameterValue(SyntaxNode syntax, ParameterSymbol parameter, ThreeState enableCallerInfo, LocalRewriter localRewriter, Binder binder, DiagnosticBag diagnostics)
	{
		LocalRewriter localRewriter2 = localRewriter;
		SyntaxNode syntax2 = syntax;
		DiagnosticBag diagnostics2 = diagnostics;
		bool isLowering;
		CSharpCompilation compilation;
		if (localRewriter2 != null)
		{
			isLowering = true;
			compilation = localRewriter2._compilation;
		}
		else
		{
			isLowering = false;
			compilation = binder.Compilation;
		}
		TypeSymbol type = parameter.Type;
		ConstantValue constantValue = parameter.ExplicitDefaultConstantValue;
		if ((constantValue != null && constantValue.IsBad) & isLowering)
		{
			constantValue = ConstantValue.Null;
		}
		SourceLocation callerLocation;
		BoundExpression boundExpression;
		if (parameter.IsCallerLineNumber && (callerLocation = GetCallerLocation(syntax2, enableCallerInfo)) != null)
		{
			int displayLineNumber = callerLocation.SourceTree.GetDisplayLineNumber(callerLocation.SourceSpan);
			BoundExpression operand = MakeLiteral(syntax2, ConstantValue.Create(displayLineNumber), compilation.GetSpecialType(SpecialType.System_Int32), localRewriter2);
			if (type.IsNullableType())
			{
				TypeSymbol nullableUnderlyingType = type.GetNullableUnderlyingType();
				boundExpression = MakeConversionNode(operand, nullableUnderlyingType, @checked: false, acceptFailingConversion: false);
				return new BoundObjectCreationExpression(syntax2, UnsafeGetNullableMethod(syntax2, type, SpecialMember.System_Nullable_T__ctor, compilation, diagnostics2), null, boundExpression)
				{
					WasCompilerGenerated = true
				};
			}
			return MakeConversionNode(operand, type, @checked: false, acceptFailingConversion: false);
		}
		if (parameter.IsCallerFilePath && (callerLocation = GetCallerLocation(syntax2, enableCallerInfo)) != null)
		{
			string displayPath = callerLocation.SourceTree.GetDisplayPath(callerLocation.SourceSpan, compilation.Options.SourceReferenceResolver);
			return MakeConversionNode(MakeLiteral(syntax2, ConstantValue.Create(displayPath), compilation.GetSpecialType(SpecialType.System_String), localRewriter2), type, @checked: false, acceptFailingConversion: false);
		}
		string text;
		if (parameter.IsCallerMemberName && GetCallerLocation(syntax2, enableCallerInfo) != null)
		{
			if (isLowering)
			{
				MethodSymbol topLevelMethod = localRewriter2._factory.TopLevelMethod;
				MethodKind methodKind = topLevelMethod.MethodKind;
				if (methodKind == MethodKind.Constructor || methodKind == MethodKind.StaticConstructor)
				{
					MemberDeclarationSyntax memberDeclarationSyntax = syntax2.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault();
					if (memberDeclarationSyntax != null)
					{
						if (memberDeclarationSyntax.Kind() == SyntaxKind.PropertyDeclaration)
						{
							PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)memberDeclarationSyntax;
							EqualsValueClauseSyntax initializer = propertyDeclarationSyntax.Initializer;
							if (initializer != null && initializer.Span.Contains(syntax2.Span))
							{
								text = propertyDeclarationSyntax.Identifier.ValueText;
								goto IL_0334;
							}
						}
						else if (memberDeclarationSyntax is BaseFieldDeclarationSyntax baseFieldDeclarationSyntax)
						{
							text = null;
							foreach (VariableDeclaratorSyntax variable in baseFieldDeclarationSyntax.Declaration.Variables)
							{
								EqualsValueClauseSyntax initializer2 = variable.Initializer;
								if (initializer2 != null && initializer2.Span.Contains(syntax2.Span))
								{
									text = variable.Identifier.ValueText;
									break;
								}
							}
							if (text != null)
							{
								goto IL_0334;
							}
						}
					}
				}
				text = topLevelMethod.GetMemberCallerName();
			}
			else
			{
				text = binder.ContainingMember().GetMemberCallerName();
			}
			goto IL_0334;
		}
		if (constantValue == null)
		{
			if (type.IsDynamic() || type.SpecialType == SpecialType.System_Object)
			{
				return isLowering ? localRewriter2.GetDefaultParameterSpecial(syntax2, parameter) : GetDefaultParameterSpecialForIOperation(syntax2, parameter, compilation, diagnostics2);
			}
			return new BoundDefaultExpression(syntax2, type)
			{
				WasCompilerGenerated = true
			};
		}
		if (constantValue.IsNull && (type.IsValueType || (type.IsNullableType() && type.IsErrorType())))
		{
			return new BoundDefaultExpression(syntax2, type)
			{
				WasCompilerGenerated = true
			};
		}
		if (type.IsNullableType())
		{
			TypeSymbol specialType = compilation.GetSpecialType(constantValue.SpecialType);
			boundExpression = MakeLiteral(syntax2, constantValue, specialType, localRewriter2);
			boundExpression = MakeConversionNode(boundExpression, type.GetNullableUnderlyingType(), @checked: false, acceptFailingConversion: true);
			return new BoundObjectCreationExpression(syntax2, UnsafeGetNullableMethod(syntax2, type, SpecialMember.System_Nullable_T__ctor, compilation, diagnostics2), null, boundExpression)
			{
				WasCompilerGenerated = true
			};
		}
		if (constantValue.IsNull || constantValue.IsBad)
		{
			return MakeLiteral(syntax2, constantValue, type, localRewriter2);
		}
		TypeSymbol specialType2 = compilation.GetSpecialType(constantValue.SpecialType);
		boundExpression = MakeLiteral(syntax2, constantValue, specialType2, localRewriter2);
		return MakeConversionNode(boundExpression, type, @checked: false, acceptFailingConversion: true);
		IL_0334:
		return MakeConversionNode(MakeLiteral(syntax2, ConstantValue.Create(text), compilation.GetSpecialType(SpecialType.System_String), localRewriter2), type, @checked: false, acceptFailingConversion: false);
		BoundExpression MakeConversionNode(BoundExpression boundExpression2, TypeSymbol typeSymbol, bool @checked, bool acceptFailingConversion)
		{
			if (isLowering)
			{
				return localRewriter2.MakeConversionNode(boundExpression2, typeSymbol, @checked, acceptFailingConversion);
			}
			return MakeConversionForIOperation(boundExpression2, typeSymbol, syntax2, compilation, diagnostics2, @checked, acceptFailingConversion);
		}
	}

	private BoundExpression GetDefaultParameterSpecial(SyntaxNode syntax, ParameterSymbol parameter)
	{
		BoundExpression defaultParameterSpecialNoConversion = GetDefaultParameterSpecialNoConversion(syntax, parameter, _compilation);
		return MakeConversionNode(defaultParameterSpecialNoConversion, parameter.Type, @checked: false);
	}

	private static BoundExpression GetDefaultParameterSpecialForIOperation(SyntaxNode syntax, ParameterSymbol parameter, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		return MakeConversionForIOperation(GetDefaultParameterSpecialNoConversion(syntax, parameter, compilation), parameter.Type, syntax, compilation, diagnostics, @checked: false);
	}

	private static BoundExpression GetDefaultParameterSpecialNoConversion(SyntaxNode syntax, ParameterSymbol parameter, CSharpCompilation compilation)
	{
		if (parameter.IsMarshalAsObject)
		{
			return new BoundDefaultExpression(syntax, parameter.Type)
			{
				WasCompilerGenerated = true
			};
		}
		if (parameter.IsIUnknownConstant)
		{
			MethodSymbol constructor = (MethodSymbol)compilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_InteropServices_UnknownWrapper__ctor);
			BoundDefaultExpression boundDefaultExpression = new BoundDefaultExpression(syntax, parameter.Type)
			{
				WasCompilerGenerated = true
			};
			return new BoundObjectCreationExpression(syntax, constructor, null, boundDefaultExpression)
			{
				WasCompilerGenerated = true
			};
		}
		if (parameter.IsIDispatchConstant)
		{
			MethodSymbol constructor2 = (MethodSymbol)compilation.GetWellKnownTypeMember(WellKnownMember.System_Runtime_InteropServices_DispatchWrapper__ctor);
			BoundDefaultExpression boundDefaultExpression2 = new BoundDefaultExpression(syntax, parameter.Type)
			{
				WasCompilerGenerated = true
			};
			return new BoundObjectCreationExpression(syntax, constructor2, null, boundDefaultExpression2)
			{
				WasCompilerGenerated = true
			};
		}
		FieldSymbol fieldSymbol = (FieldSymbol)compilation.GetWellKnownTypeMember(WellKnownMember.System_Type__Missing);
		return new BoundFieldAccess(syntax, null, fieldSymbol, null)
		{
			WasCompilerGenerated = true
		};
	}

	private void RewriteArgumentsForComCall(ImmutableArray<ParameterSymbol> parameters, BoundExpression[] actualArguments, ArrayBuilder<RefKind> argsRefKindsBuilder, ArrayBuilder<LocalSymbol> temporariesBuilder)
	{
		int num = actualArguments.Length;
		for (int i = 0; i < num; i++)
		{
			RefKind refKind = parameters[i].RefKind;
			if (argsRefKindsBuilder[i] == RefKind.None && refKind == RefKind.Ref)
			{
				BoundExpression boundExpression = actualArguments[i];
				if (boundExpression.Kind != BoundKind.Local || ((BoundLocal)boundExpression).LocalSymbol.RefKind != RefKind.Ref)
				{
					BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store);
					actualArguments[i] = new BoundSequence(boundExpression.Syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create((BoundExpression)store), boundLocal, boundLocal.Type);
					argsRefKindsBuilder[i] = RefKind.Ref;
					temporariesBuilder.Add(boundLocal.LocalSymbol);
				}
			}
		}
	}

	public override BoundNode VisitDynamicMemberAccess(BoundDynamicMemberAccess node)
	{
		if (node.Invoked)
		{
			return node;
		}
		BoundExpression loweredReceiver = VisitExpression(node.Receiver);
		return _dynamicFactory.MakeDynamicGetMember(loweredReceiver, node.Name, node.Indexed).ToExpression();
	}

	public override BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		return VisitCompoundAssignmentOperator(node, used: true);
	}

	private BoundExpression VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node, bool used)
	{
		BoundExpression boundExpression = VisitExpression(node.Right);
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		BinaryOperatorKind kind = node.Operator.Kind;
		bool flag = kind.IsChecked();
		bool flag2 = kind.IsDynamic();
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		int num;
		if (node.Left.Kind == BoundKind.DynamicMemberAccess)
		{
			if (binaryOperatorKind != BinaryOperatorKind.Addition)
			{
				num = ((binaryOperatorKind == BinaryOperatorKind.Subtraction) ? 1 : 0);
				if (num == 0)
				{
					goto IL_009a;
				}
			}
			else
			{
				num = 1;
			}
			if (CanChangeValueBetweenReads(boundExpression))
			{
				BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store);
				boundExpression = boundLocal;
				instance2.Add(store);
				instance.Add(boundLocal.LocalSymbol);
			}
		}
		else
		{
			num = 0;
		}
		goto IL_009a;
		IL_009a:
		BoundExpression boundExpression2 = TransformCompoundAssignmentLHS(node.Left, instance2, instance, flag2);
		SyntaxNode syntax = node.Syntax;
		BoundExpression boundExpression3 = MakeRValue(boundExpression2);
		BoundExpression rewrittenOperand = MakeBinaryOperator(loweredLeft: flag2 ? boundExpression3 : MakeConversionNode(syntax, boundExpression3, node.LeftConversion, node.Operator.LeftType, flag), syntax: syntax, operatorKind: node.Operator.Kind, loweredRight: boundExpression, type: node.Operator.ReturnType, method: node.Operator.Method, isPointerElementAccess: false, isCompoundAssignment: true);
		Conversion finalConversion = node.FinalConversion;
		TypeSymbol type = node.Left.Type;
		bool explicitCastInCode = flag2;
		BoundExpression rewrittenRight = MakeConversionNode(syntax, rewrittenOperand, finalConversion, type, flag, explicitCastInCode);
		BoundExpression boundExpression4 = MakeAssignmentOperator(syntax, boundExpression2, rewrittenRight, node.Left.Type, used, flag, isCompoundAssignment: true);
		if (num != 0)
		{
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)boundExpression2;
			LoweredDynamicOperation loweredDynamicOperation = _dynamicFactory.MakeDynamicIsEventTest(boundDynamicMemberAccess.Name, boundDynamicMemberAccess.Receiver);
			LoweredDynamicOperation loweredDynamicOperation2 = _dynamicFactory.MakeDynamicEventAccessorInvocation(((binaryOperatorKind == BinaryOperatorKind.Addition) ? "add_" : "remove_") + boundDynamicMemberAccess.Name, boundDynamicMemberAccess.Receiver, boundExpression);
			boundExpression4 = _factory.Conditional(loweredDynamicOperation.ToExpression(), loweredDynamicOperation2.ToExpression(), boundExpression4, boundExpression4.Type);
		}
		BoundExpression result = ((instance.Count == 0 && instance2.Count == 0) ? boundExpression4 : new BoundSequence(syntax, instance.ToImmutable(), instance2.ToImmutable(), boundExpression4, boundExpression4.Type));
		instance.Free();
		instance2.Free();
		return result;
	}

	private BoundExpression TransformPropertyOrEventReceiver(Symbol propertyOrEvent, BoundExpression receiverOpt, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		if (receiverOpt == null || propertyOrEvent.IsStatic || !CanChangeValueBetweenReads(receiverOpt))
		{
			return receiverOpt;
		}
		BoundExpression boundExpression = VisitExpression(receiverOpt);
		bool flag = boundExpression.Type.IsValueType || boundExpression.Type.Kind == SymbolKind.TypeParameter;
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store, flag ? RefKind.Ref : RefKind.None);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		return boundLocal;
	}

	private BoundDynamicMemberAccess TransformDynamicMemberAccess(BoundDynamicMemberAccess memberAccess, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		if (!CanChangeValueBetweenReads(memberAccess.Receiver))
		{
			return memberAccess;
		}
		BoundExpression argument = VisitExpression(memberAccess.Receiver);
		BoundLocal boundLocal = _factory.StoreToTemp(argument, out var store);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		return new BoundDynamicMemberAccess(memberAccess.Syntax, boundLocal, memberAccess.TypeArgumentsOpt, memberAccess.Name, memberAccess.Invoked, memberAccess.Indexed, memberAccess.Type);
	}

	private BoundIndexerAccess TransformIndexerAccess(BoundIndexerAccess indexerAccess, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		BoundExpression receiverOpt = indexerAccess.ReceiverOpt;
		BoundExpression receiverOpt2;
		if (CanChangeValueBetweenReads(receiverOpt))
		{
			BoundExpression boundExpression = VisitExpression(receiverOpt);
			bool flag = boundExpression.Type.IsValueType || boundExpression.Type.Kind == SymbolKind.TypeParameter;
			BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store, flag ? RefKind.Ref : RefKind.None);
			receiverOpt2 = boundLocal;
			stores.Add(store);
			temps.Add(boundLocal.LocalSymbol);
		}
		else
		{
			receiverOpt2 = VisitExpression(receiverOpt);
		}
		ImmutableArray<BoundExpression> rewrittenArguments = VisitList(indexerAccess.Arguments);
		SyntaxNode syntax = indexerAccess.Syntax;
		PropertySymbol indexer = indexerAccess.Indexer;
		ImmutableArray<RefKind> argumentRefKindsOpt = indexerAccess.ArgumentRefKindsOpt;
		bool expanded = indexerAccess.Expanded;
		ImmutableArray<int> argsToParamsOpt = indexerAccess.ArgsToParamsOpt;
		ImmutableArray<ParameterSymbol> parameters = indexer.Parameters;
		BoundExpression[] array = new BoundExpression[parameters.Length];
		ArrayBuilder<BoundAssignmentOperator> instance = ArrayBuilder<BoundAssignmentOperator>.GetInstance(rewrittenArguments.Length);
		ArrayBuilder<RefKind> instance2 = ArrayBuilder<RefKind>.GetInstance(parameters.Length, RefKind.None);
		BuildStoresToTemps(expanded, argsToParamsOpt, parameters, argumentRefKindsOpt, rewrittenArguments, forceLambdaSpilling: true, array, instance2, instance);
		if (expanded)
		{
			BoundExpression argument = BuildParamsArray(syntax, indexer, argsToParamsOpt, rewrittenArguments, parameters, array[^1]);
			BoundLocal boundLocal2 = _factory.StoreToTemp(argument, out var store2);
			stores.Add(store2);
			temps.Add(boundLocal2.LocalSymbol);
			array[^1] = boundLocal2;
		}
		MethodSymbol methodSymbol = indexer.GetOwnOrInheritedGetMethod() ?? indexer.GetOwnOrInheritedSetMethod();
		InsertMissingOptionalArguments(syntax, methodSymbol.Parameters, array, instance2);
		if (indexer.ContainingType.IsComImport)
		{
			RewriteArgumentsForComCall(parameters, array, instance2, temps);
		}
		rewrittenArguments = array.AsImmutableOrNull();
		foreach (BoundAssignmentOperator item in instance)
		{
			temps.Add(((BoundLocal)item.Left).LocalSymbol);
			stores.Add(item);
		}
		instance.Free();
		argumentRefKindsOpt = GetRefKindsOrNull(instance2);
		instance2.Free();
		return new BoundIndexerAccess(syntax, receiverOpt2, indexer, rewrittenArguments, default, argumentRefKindsOpt, expanded: false, default, null, indexerAccess.UseSetterForDefaultArgumentGeneration, indexerAccess.Type);
	}

	private bool TransformCompoundAssignmentFieldOrEventAccessReceiver(Symbol fieldOrEvent, ref BoundExpression receiver, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		if (fieldOrEvent.IsStatic || !CanChangeValueBetweenReads(receiver))
		{
			return true;
		}
		if (!receiver.Type.IsReferenceType)
		{
			return false;
		}
		BoundExpression boundExpression = VisitExpression(receiver);
		if (boundExpression.Type.IsTypeParameter())
		{
			NamedTypeSymbol containingType = fieldOrEvent.ContainingType;
			boundExpression = BoxReceiver(boundExpression, containingType);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		receiver = boundLocal;
		return true;
	}

	private BoundDynamicIndexerAccess TransformDynamicIndexerAccess(BoundDynamicIndexerAccess indexerAccess, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		BoundExpression receiverOpt;
		if (CanChangeValueBetweenReads(indexerAccess.ReceiverOpt))
		{
			BoundLocal boundLocal = _factory.StoreToTemp(VisitExpression(indexerAccess.ReceiverOpt), out var store);
			stores.Add(store);
			temps.Add(boundLocal.LocalSymbol);
			receiverOpt = boundLocal;
		}
		else
		{
			receiverOpt = indexerAccess.ReceiverOpt;
		}
		ImmutableArray<BoundExpression> arguments = indexerAccess.Arguments;
		BoundExpression[] array = new BoundExpression[arguments.Length];
		for (int i = 0; i < arguments.Length; i++)
		{
			if (CanChangeValueBetweenReads(arguments[i]))
			{
				BoundLocal boundLocal2 = _factory.StoreToTemp(VisitExpression(arguments[i]), out var store2, (indexerAccess.ArgumentRefKindsOpt.RefKinds(i) != RefKind.None) ? RefKind.Ref : RefKind.None);
				stores.Add(store2);
				temps.Add(boundLocal2.LocalSymbol);
				array[i] = boundLocal2;
			}
			else
			{
				array[i] = arguments[i];
			}
		}
		return new BoundDynamicIndexerAccess(indexerAccess.Syntax, receiverOpt, array.AsImmutableOrNull(), indexerAccess.ArgumentNamesOpt, indexerAccess.ArgumentRefKindsOpt, indexerAccess.ApplicableIndexers, indexerAccess.Type);
	}

	private BoundExpression TransformCompoundAssignmentLHS(BoundExpression originalLHS, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps, bool isDynamicAssignment)
	{
		switch (originalLHS.Kind)
		{
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)originalLHS;
			if (boundPropertyAccess.PropertySymbol.RefKind == RefKind.None)
			{
				return boundPropertyAccess.Update(TransformPropertyOrEventReceiver(boundPropertyAccess.PropertySymbol, boundPropertyAccess.ReceiverOpt, stores, temps), boundPropertyAccess.PropertySymbol, boundPropertyAccess.ResultKind, boundPropertyAccess.Type);
			}
			break;
		}
		case BoundKind.IndexerAccess:
			if (((BoundIndexerAccess)originalLHS).Indexer.RefKind == RefKind.None)
			{
				return TransformIndexerAccess((BoundIndexerAccess)originalLHS, stores, temps);
			}
			break;
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)originalLHS;
			BoundExpression receiver2 = boundFieldAccess.ReceiverOpt;
			if (TransformCompoundAssignmentFieldOrEventAccessReceiver(boundFieldAccess.FieldSymbol, ref receiver2, stores, temps))
			{
				return MakeFieldAccess(boundFieldAccess.Syntax, receiver2, boundFieldAccess.FieldSymbol, boundFieldAccess.ConstantValueOpt, boundFieldAccess.ResultKind, boundFieldAccess.Type, boundFieldAccess);
			}
			break;
		}
		case BoundKind.ArrayAccess:
		{
			BoundArrayAccess boundArrayAccess = (BoundArrayAccess)originalLHS;
			if (isDynamicAssignment || !IsInvariantArray(boundArrayAccess.Expression.Type))
			{
				BoundExpression loweredExpression = VisitExpression(boundArrayAccess.Expression);
				ImmutableArray<BoundExpression> loweredIndices = VisitList(boundArrayAccess.Indices);
				return SpillArrayElementAccess(loweredExpression, loweredIndices, stores, temps);
			}
			break;
		}
		case BoundKind.DynamicMemberAccess:
			return TransformDynamicMemberAccess((BoundDynamicMemberAccess)originalLHS, stores, temps);
		case BoundKind.DynamicIndexerAccess:
			return TransformDynamicIndexerAccess((BoundDynamicIndexerAccess)originalLHS, stores, temps);
		case BoundKind.ThisReference:
		case BoundKind.Local:
		case BoundKind.PseudoVariable:
		case BoundKind.Parameter:
			return originalLHS;
		case BoundKind.EventAccess:
		{
			BoundEventAccess boundEventAccess = (BoundEventAccess)originalLHS;
			BoundExpression receiver = boundEventAccess.ReceiverOpt;
			if (boundEventAccess.EventSymbol.IsWindowsRuntimeEvent)
			{
				return boundEventAccess.Update(TransformPropertyOrEventReceiver(boundEventAccess.EventSymbol, boundEventAccess.ReceiverOpt, stores, temps), boundEventAccess.EventSymbol, boundEventAccess.IsUsableAsField, boundEventAccess.ResultKind, boundEventAccess.Type);
			}
			if (TransformCompoundAssignmentFieldOrEventAccessReceiver(boundEventAccess.EventSymbol, ref receiver, stores, temps))
			{
				return MakeEventAccess(boundEventAccess.Syntax, receiver, boundEventAccess.EventSymbol, boundEventAccess.ConstantValue, boundEventAccess.ResultKind, boundEventAccess.Type);
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(originalLHS.Kind);
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.PointerElementAccess:
		case BoundKind.RefValueOperator:
		case BoundKind.AssignmentOperator:
		case BoundKind.ConditionalOperator:
		case BoundKind.Call:
			break;
		}
		BoundExpression argument = VisitExpression(originalLHS);
		BoundLocal boundLocal = _factory.StoreToTemp(argument, out var store, RefKind.Ref);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		return boundLocal;
	}

	private static bool IsInvariantArray(TypeSymbol type)
	{
		return (type as ArrayTypeSymbol)?.ElementType.IsSealed ?? false;
	}

	private BoundExpression BoxReceiver(BoundExpression rewrittenReceiver, NamedTypeSymbol memberContainingType)
	{
		return MakeConversionNode(rewrittenReceiver.Syntax, rewrittenReceiver, Conversion.Boxing, memberContainingType, @checked: false, explicitCastInCode: false, rewrittenReceiver.ConstantValue);
	}

	private BoundExpression SpillArrayElementAccess(BoundExpression loweredExpression, ImmutableArray<BoundExpression> loweredIndices, ArrayBuilder<BoundExpression> stores, ArrayBuilder<LocalSymbol> temps)
	{
		BoundLocal boundLocal = _factory.StoreToTemp(loweredExpression, out var store);
		stores.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		BoundLocal array = boundLocal;
		BoundExpression[] array2 = new BoundExpression[loweredIndices.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			if (CanChangeValueBetweenReads(loweredIndices[i]))
			{
				BoundLocal boundLocal2 = _factory.StoreToTemp(loweredIndices[i], out var store2);
				stores.Add(store2);
				temps.Add(boundLocal2.LocalSymbol);
				array2[i] = boundLocal2;
			}
			else
			{
				array2[i] = loweredIndices[i];
			}
		}
		return _factory.ArrayAccess(array, array2);
	}

	internal static bool CanChangeValueBetweenReads(BoundExpression expression, bool localsMayBeAssignedOrCaptured = true, bool structThisCanChangeValueBetweenReads = false)
	{
		if (expression.IsDefaultValue())
		{
			return false;
		}
		if (expression.ConstantValue != null)
		{
			return !ConstantValueIsTrivial(expression.Type);
		}
		switch (expression.Kind)
		{
		case BoundKind.ThisReference:
			if (structThisCanChangeValueBetweenReads)
			{
				return ((BoundThisReference)expression).Type.IsStructType();
			}
			return false;
		case BoundKind.BaseReference:
			return false;
		case BoundKind.Literal:
			return !ConstantValueIsTrivial(expression.Type);
		case BoundKind.Parameter:
			if (!localsMayBeAssignedOrCaptured)
			{
				return ((BoundParameter)expression).ParameterSymbol.RefKind != RefKind.None;
			}
			return true;
		case BoundKind.Local:
			if (!localsMayBeAssignedOrCaptured)
			{
				return ((BoundLocal)expression).LocalSymbol.RefKind != RefKind.None;
			}
			return true;
		default:
			return true;
		}
	}

	internal static bool ReadIsSideeffecting(BoundExpression expression)
	{
		if (expression.ConstantValue != null)
		{
			return false;
		}
		if (expression.IsDefaultValue())
		{
			return false;
		}
		switch (expression.Kind)
		{
		case BoundKind.Literal:
		case BoundKind.ThisReference:
		case BoundKind.BaseReference:
		case BoundKind.Local:
		case BoundKind.Parameter:
		case BoundKind.Lambda:
			return false;
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expression;
			if (!boundConversion.ConversionHasSideEffects())
			{
				return ReadIsSideeffecting(boundConversion.Operand);
			}
			return true;
		}
		case BoundKind.PassByCopy:
			return ReadIsSideeffecting(((BoundPassByCopy)expression).Expression);
		case BoundKind.ObjectCreationExpression:
			if (expression.Type.IsNullableType())
			{
				BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)expression;
				if (boundObjectCreationExpression.Arguments.Length == 1)
				{
					return ReadIsSideeffecting(boundObjectCreationExpression.Arguments[0]);
				}
				return false;
			}
			return true;
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expression;
			MethodSymbol method = boundCall.Method;
			NamedTypeSymbol containingType = method.ContainingType;
			if ((object)containingType != null && containingType.IsNullableType() && (IsSpecialMember(method, SpecialMember.System_Nullable_T_GetValueOrDefault) || IsSpecialMember(method, SpecialMember.System_Nullable_T_get_HasValue)))
			{
				return ReadIsSideeffecting(boundCall.ReceiverOpt);
			}
			return true;
		}
		default:
			return true;
		}
	}

	private static bool IsSpecialMember(MethodSymbol method, SpecialMember specialMember)
	{
		method = method.OriginalDefinition;
		return method.ContainingAssembly?.GetSpecialTypeMember(specialMember) == method;
	}

	private static bool ConstantValueIsTrivial(TypeSymbol type)
	{
		if ((object)type != null && !type.SpecialType.IsClrInteger() && !type.IsReferenceType)
		{
			return type.IsEnumType();
		}
		return true;
	}

	public override BoundNode VisitConditionalAccess(BoundConditionalAccess node)
	{
		return RewriteConditionalAccess(node, used: true);
	}

	public override BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal BoundExpression RewriteConditionalAccess(BoundConditionalAccess node, bool used)
	{
		BoundExpression boundExpression = VisitExpression(node.Receiver);
		TypeSymbol type = boundExpression.Type;
		if (boundExpression.IsDefaultValue() && type.IsReferenceType)
		{
			return _factory.Default(node.Type);
		}
		ConditionalAccessLoweringKind conditionalAccessLoweringKind = (node.AccessExpression.Type.IsDynamic() ? ((!CanChangeValueBetweenReads(boundExpression)) ? ConditionalAccessLoweringKind.Ternary : ConditionalAccessLoweringKind.TernaryCaptureReceiverByVal) : ConditionalAccessLoweringKind.LoweredConditionalAccess);
		BoundExpression currentConditionalAccessTarget = _currentConditionalAccessTarget;
		int id = ++_currentConditionalAccessID;
		LocalSymbol localSymbol = null;
		switch (conditionalAccessLoweringKind)
		{
		case ConditionalAccessLoweringKind.LoweredConditionalAccess:
			_currentConditionalAccessTarget = new BoundConditionalReceiver(boundExpression.Syntax, id, type);
			break;
		case ConditionalAccessLoweringKind.Ternary:
			_currentConditionalAccessTarget = boundExpression;
			break;
		case ConditionalAccessLoweringKind.TernaryCaptureReceiverByVal:
			localSymbol = _factory.SynthesizedLocal(type);
			_currentConditionalAccessTarget = _factory.Local(localSymbol);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(conditionalAccessLoweringKind);
		}
		BoundExpression boundExpression2;
		if (used)
		{
			boundExpression2 = VisitExpression(node.AccessExpression);
		}
		else
		{
			boundExpression2 = VisitUnusedExpression(node.AccessExpression);
			if (boundExpression2 == null)
			{
				return null;
			}
		}
		_currentConditionalAccessTarget = currentConditionalAccessTarget;
		TypeSymbol type2 = VisitType(node.Type);
		TypeSymbol typeSymbol = node.Type;
		TypeSymbol type3 = boundExpression2.Type;
		if (type3.SpecialType == SpecialType.System_Void)
		{
			type2 = (typeSymbol = type3);
		}
		if (type3 != typeSymbol && typeSymbol.IsNullableType())
		{
			boundExpression2 = _factory.New((NamedTypeSymbol)typeSymbol, boundExpression2);
		}
		NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Object);
		BoundExpression boundExpression3;
		switch (conditionalAccessLoweringKind)
		{
		case ConditionalAccessLoweringKind.LoweredConditionalAccess:
			boundExpression3 = new BoundLoweredConditionalAccess(node.Syntax, boundExpression, type.IsNullableType() ? UnsafeGetNullableMethod(node.Syntax, boundExpression.Type, SpecialMember.System_Nullable_T_get_HasValue) : null, boundExpression2, null, id, type2);
			break;
		case ConditionalAccessLoweringKind.TernaryCaptureReceiverByVal:
			boundExpression = _factory.MakeSequence(_factory.AssignmentExpression(_factory.Local(localSymbol), boundExpression), _factory.Local(localSymbol));
			goto case ConditionalAccessLoweringKind.Ternary;
		case ConditionalAccessLoweringKind.Ternary:
		{
			BoundBinaryOperator rewrittenCondition = _factory.ObjectNotEqual(_factory.Convert(specialType, boundExpression), _factory.Null(specialType));
			BoundExpression rewrittenConsequence = boundExpression2;
			boundExpression3 = RewriteConditionalOperator(node.Syntax, rewrittenCondition, rewrittenConsequence, _factory.Default(typeSymbol), null, typeSymbol, isRef: false);
			if (localSymbol != null)
			{
				boundExpression3 = _factory.MakeSequence(localSymbol, boundExpression3);
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(conditionalAccessLoweringKind);
		}
		return boundExpression3;
	}

	public override BoundNode VisitConditionalReceiver(BoundConditionalReceiver node)
	{
		BoundExpression boundExpression = _currentConditionalAccessTarget;
		if (boundExpression.Type.IsNullableType())
		{
			boundExpression = MakeOptimizedGetValueOrDefault(node.Syntax, boundExpression);
		}
		return boundExpression;
	}

	public override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		BoundExpression boundExpression = VisitExpression(node.Condition);
		BoundExpression boundExpression2 = VisitExpression(node.Consequence);
		BoundExpression boundExpression3 = VisitExpression(node.Alternative);
		if (boundExpression.ConstantValue == null)
		{
			return node.Update(node.IsRef, boundExpression, boundExpression2, boundExpression3, node.ConstantValueOpt, node.Type);
		}
		return RewriteConditionalOperator(node.Syntax, boundExpression, boundExpression2, boundExpression3, node.ConstantValueOpt, node.Type, node.IsRef);
	}

	private static BoundExpression RewriteConditionalOperator(SyntaxNode syntax, BoundExpression rewrittenCondition, BoundExpression rewrittenConsequence, BoundExpression rewrittenAlternative, ConstantValue constantValueOpt, TypeSymbol rewrittenType, bool isRef)
	{
		ConstantValue constantValue = rewrittenCondition.ConstantValue;
		if (constantValue == ConstantValue.True)
		{
			return rewrittenConsequence;
		}
		if (constantValue == ConstantValue.False)
		{
			return rewrittenAlternative;
		}
		return new BoundConditionalOperator(syntax, isRef, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, constantValueOpt, rewrittenType);
	}

	public override BoundNode VisitContinueStatement(BoundContinueStatement node)
	{
		BoundStatement boundStatement = new BoundGotoStatement(node.Syntax, node.Label, node.HasErrors);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = _instrumenter.InstrumentContinueStatement(node, boundStatement);
		}
		return boundStatement;
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		if (node.ConversionKind == ConversionKind.InterpolatedString)
		{
			return RewriteInterpolatedStringConversion(node);
		}
		TypeSymbol typeSymbol = VisitType(node.Type);
		bool inExpressionLambda = _inExpressionLambda;
		_inExpressionLambda = _inExpressionLambda || (node.ConversionKind == ConversionKind.AnonymousFunction && !inExpressionLambda && typeSymbol.IsExpressionTree());
		BoundExpression rewrittenOperand = VisitExpression(node.Operand);
		_inExpressionLambda = inExpressionLambda;
		BoundExpression boundExpression = MakeConversionNode(node, node.Syntax, rewrittenOperand, node.Conversion, node.Checked, node.ExplicitCastInCode, node.ConstantValue, typeSymbol);
		TypeSymbol type = node.Type;
		if (!_inExpressionLambda && node.ExplicitCastInCode && IsFloatingPointExpressionOfUnknownPrecision(boundExpression))
		{
			boundExpression = MakeConversionNode(node.Syntax, boundExpression, Conversion.Identity, type, @checked: false, explicitCastInCode: true, boundExpression.ConstantValue);
		}
		return boundExpression;
	}

	private static bool IsFloatingPointExpressionOfUnknownPrecision(BoundExpression rewrittenNode)
	{
		if (rewrittenNode == null)
		{
			return false;
		}
		if (rewrittenNode.ConstantValue != null)
		{
			return false;
		}
		TypeSymbol type = rewrittenNode.Type;
		if (type.SpecialType != SpecialType.System_Double && type.SpecialType != SpecialType.System_Single)
		{
			return false;
		}
		switch (rewrittenNode.Kind)
		{
		case BoundKind.Sequence:
			return IsFloatingPointExpressionOfUnknownPrecision(((BoundSequence)rewrittenNode).Value);
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)rewrittenNode;
			if (boundConversion.ConversionKind == ConversionKind.Identity)
			{
				return !boundConversion.ExplicitCastInCode;
			}
			return false;
		}
		default:
			return true;
		}
	}

	private BoundExpression MakeConversionNode(BoundConversion oldNode, SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, ConstantValue constantValueOpt, TypeSymbol rewrittenType)
	{
		if (_inExpressionLambda)
		{
			@checked = @checked && NeedsCheckedConversionInExpressionTree(rewrittenOperand.Type, rewrittenType, explicitCastInCode);
		}
		switch (conversion.Kind)
		{
		case ConversionKind.Identity:
			if (!_inExpressionLambda && rewrittenOperand.Type.Equals(rewrittenType))
			{
				if (!explicitCastInCode)
				{
					return rewrittenOperand;
				}
				if (!IsFloatingPointExpressionOfUnknownPrecision(rewrittenOperand))
				{
					return rewrittenOperand;
				}
			}
			break;
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.ExplicitUserDefined:
			return RewriteUserDefinedConversion(syntax, rewrittenOperand, conversion, rewrittenType);
		case ConversionKind.IntPtr:
			return RewriteIntPtrConversion(oldNode, syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
		case ConversionKind.ImplicitNullable:
		case ConversionKind.ExplicitNullable:
			return RewriteNullableConversion(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, rewrittenType);
		case ConversionKind.Boxing:
			if (!_inExpressionLambda)
			{
				if (NullableNeverHasValue(rewrittenOperand))
				{
					return new BoundDefaultExpression(syntax, rewrittenType);
				}
				BoundExpression boundExpression = NullableAlwaysHasValue(rewrittenOperand);
				if (boundExpression != null)
				{
					return MakeConversionNode(oldNode, syntax, boundExpression, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
				}
			}
			break;
		case ConversionKind.DefaultOrNullLiteral:
			if (!_inExpressionLambda || !explicitCastInCode)
			{
				return new BoundDefaultExpression(syntax, rewrittenType);
			}
			break;
		case ConversionKind.ImplicitReference:
		case ConversionKind.ExplicitReference:
			if (rewrittenOperand.IsDefaultValue() && (!_inExpressionLambda || !explicitCastInCode))
			{
				return new BoundDefaultExpression(syntax, rewrittenType);
			}
			break;
		case ConversionKind.ImplicitConstant:
			conversion = Conversion.ExplicitNumeric;
			@checked = false;
			goto case ConversionKind.ImplicitNumeric;
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ExplicitNumeric:
			if (rewrittenOperand.IsDefaultValue() && (!_inExpressionLambda || !explicitCastInCode))
			{
				return new BoundDefaultExpression(syntax, rewrittenType);
			}
			if (rewrittenType.SpecialType == SpecialType.System_Decimal || rewrittenOperand.Type.SpecialType == SpecialType.System_Decimal)
			{
				return RewriteDecimalConversion(syntax, rewrittenOperand, rewrittenOperand.Type, rewrittenType, conversion.Kind.IsImplicitConversion(), constantValueOpt);
			}
			break;
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ExplicitTupleLiteral:
			return rewrittenOperand;
		case ConversionKind.ImplicitThrow:
		{
			BoundThrowExpression boundThrowExpression = (BoundThrowExpression)rewrittenOperand;
			return _factory.ThrowExpression(boundThrowExpression.Expression, rewrittenType);
		}
		case ConversionKind.ImplicitEnumeration:
			if (rewrittenType.IsNullableType())
			{
				BoundExpression rewrittenOperand2 = MakeConversionNode(oldNode, syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType.GetNullableUnderlyingType());
				Conversion conversion2 = new Conversion(ConversionKind.ImplicitNullable, Conversion.IdentityUnderlying);
				return MakeConversionNode(oldNode, syntax, rewrittenOperand2, conversion2, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
			}
			goto case ConversionKind.ExplicitEnumeration;
		case ConversionKind.ExplicitEnumeration:
			if (!rewrittenType.IsNullableType() && rewrittenOperand.IsDefaultValue() && (!_inExpressionLambda || !explicitCastInCode))
			{
				return new BoundDefaultExpression(syntax, rewrittenType);
			}
			if (rewrittenType.SpecialType == SpecialType.System_Decimal)
			{
				NamedTypeSymbol enumUnderlyingType = rewrittenOperand.Type.GetEnumUnderlyingType();
				rewrittenOperand = MakeConversionNode(rewrittenOperand, enumUnderlyingType, @checked: false);
				return RewriteDecimalConversion(syntax, rewrittenOperand, enumUnderlyingType, rewrittenType, isImplicit: false, constantValueOpt);
			}
			if (rewrittenOperand.Type.SpecialType == SpecialType.System_Decimal)
			{
				NamedTypeSymbol enumUnderlyingType2 = rewrittenType.GetEnumUnderlyingType();
				BoundExpression operand = RewriteDecimalConversion(syntax, rewrittenOperand, rewrittenOperand.Type, enumUnderlyingType2, isImplicit: false, constantValueOpt);
				return new BoundConversion(syntax, operand, conversion, isBaseConversion: false, @checked: false, explicitCastInCode, constantValueOpt, rewrittenType);
			}
			break;
		case ConversionKind.ImplicitDynamic:
		case ConversionKind.ExplicitDynamic:
			return _dynamicFactory.MakeDynamicConversion(rewrittenOperand, explicitCastInCode || conversion.Kind == ConversionKind.ExplicitDynamic, conversion.IsArrayIndex, @checked, rewrittenType).ToExpression();
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ExplicitTuple:
			return RewriteTupleConversion(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, (NamedTypeSymbol)rewrittenType);
		case ConversionKind.MethodGroup:
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)rewrittenOperand;
			MethodSymbol symbolOpt = oldNode.SymbolOpt;
			SyntaxNode syntax2 = _factory.Syntax;
			_factory.Syntax = (boundMethodGroup.ReceiverOpt ?? boundMethodGroup).Syntax;
			BoundExpression argument = ((symbolOpt.IsStatic && !oldNode.IsExtensionMethod) ? _factory.Type(symbolOpt.ContainingType) : boundMethodGroup.ReceiverOpt);
			_factory.Syntax = syntax2;
			return new BoundDelegateCreationExpression(syntax, argument, symbolOpt, oldNode.IsExtensionMethod, rewrittenType);
		}
		}
		if (oldNode == null)
		{
			return new BoundConversion(syntax, rewrittenOperand, conversion, isBaseConversion: false, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
		}
		return oldNode.Update(rewrittenOperand, conversion, oldNode.IsBaseConversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
	}

	private static bool NeedsCheckedConversionInExpressionTree(TypeSymbol source, TypeSymbol target, bool explicitCastInCode)
	{
		if ((object)source == null)
		{
			return false;
		}
		SpecialType specialType = GetUnderlyingSpecialType(source);
		SpecialType specialType2 = GetUnderlyingSpecialType(target);
		if ((explicitCastInCode || specialType != specialType2) && IsInRange(specialType, SpecialType.System_Char, SpecialType.System_Double))
		{
			return IsInRange(specialType2, SpecialType.System_Char, SpecialType.System_UInt64);
		}
		return false;
		static SpecialType GetUnderlyingSpecialType(TypeSymbol type)
		{
			return type.StrippedType().EnumUnderlyingType().SpecialType;
		}
		static bool IsInRange(SpecialType type, SpecialType low, SpecialType high)
		{
			if (low <= type)
			{
				return type <= high;
			}
			return false;
		}
	}

	private BoundExpression MakeConversionNode(BoundExpression rewrittenOperand, TypeSymbol rewrittenType, bool @checked, bool acceptFailingConversion = false)
	{
		Conversion conversion = MakeConversion(rewrittenOperand, rewrittenType, _compilation, _diagnostics, acceptFailingConversion);
		if (!conversion.IsValid)
		{
			return _factory.NullOrDefault(rewrittenType);
		}
		return MakeConversionNode(rewrittenOperand.Syntax, rewrittenOperand, conversion, rewrittenType, @checked);
	}

	private static Conversion MakeConversion(BoundExpression rewrittenOperand, TypeSymbol rewrittenType, CSharpCompilation compilation, DiagnosticBag diagnostics, bool acceptFailingConversion)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion result = compilation.Conversions.ClassifyConversionFromType(rewrittenOperand.Type, rewrittenType, ref useSiteDiagnostics);
		diagnostics.Add(rewrittenOperand.Syntax, useSiteDiagnostics);
		if (!result.IsValid && (!acceptFailingConversion || (rewrittenOperand.Type.SpecialType != SpecialType.System_Decimal && rewrittenOperand.Type.SpecialType != SpecialType.System_DateTime)))
		{
			diagnostics.Add(ErrorCode.ERR_NoImplicitConv, rewrittenOperand.Syntax.Location, rewrittenOperand.Type, rewrittenType);
		}
		return result;
	}

	private static BoundExpression MakeConversionForIOperation(BoundExpression operand, TypeSymbol type, SyntaxNode syntax, CSharpCompilation compilation, DiagnosticBag diagnostics, bool @checked, bool acceptFailingConversion = false)
	{
		Conversion conversion = MakeConversion(operand, type, compilation, diagnostics, acceptFailingConversion);
		if (conversion.IsIdentity)
		{
			return operand;
		}
		return new BoundConversion(syntax, operand, conversion, @checked, explicitCastInCode: false, null, type, !conversion.IsValid)
		{
			WasCompilerGenerated = true
		};
	}

	private BoundExpression MakeImplicitConversion(BoundExpression rewrittenOperand, TypeSymbol rewrittenType)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = _compilation.Conversions.ClassifyConversionFromType(rewrittenOperand.Type, rewrittenType, ref useSiteDiagnostics);
		_diagnostics.Add(rewrittenOperand.Syntax, useSiteDiagnostics);
		if (!conversion.IsImplicit)
		{
			_diagnostics.Add(ErrorCode.ERR_NoImplicitConv, rewrittenOperand.Syntax.Location, rewrittenOperand.Type, rewrittenType);
			return _factory.NullOrDefault(rewrittenType);
		}
		return MakeConversionNode(rewrittenOperand.Syntax, rewrittenOperand, conversion, rewrittenType, @checked: false);
	}

	private BoundExpression MakeConversionNode(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, TypeSymbol rewrittenType, bool @checked, bool explicitCastInCode = false, ConstantValue constantValueOpt = null)
	{
		if (conversion.Kind.IsUserDefinedConversion())
		{
			if (rewrittenOperand.Type != conversion.BestUserDefinedConversionAnalysis.FromType)
			{
				rewrittenOperand = MakeConversionNode(syntax, rewrittenOperand, conversion.UserDefinedFromConversion, conversion.BestUserDefinedConversionAnalysis.FromType, @checked);
			}
			if (rewrittenOperand.Type != conversion.Method.ParameterTypes[0])
			{
				rewrittenOperand = MakeConversionNode(rewrittenOperand, conversion.BestUserDefinedConversionAnalysis.FromType, @checked);
			}
			TypeSymbol typeSymbol = conversion.Method.ReturnType;
			if (rewrittenOperand.Type.IsNullableType() && conversion.Method.ParameterTypes[0].Equals(rewrittenOperand.Type.GetNullableUnderlyingType(), TypeCompareKind.AllIgnoreOptions) && !typeSymbol.IsNullableType() && typeSymbol.IsValueType)
			{
				typeSymbol = ((NamedTypeSymbol)rewrittenOperand.Type.OriginalDefinition).Construct(typeSymbol);
			}
			BoundExpression boundExpression = RewriteUserDefinedConversion(syntax, rewrittenOperand, conversion, typeSymbol);
			if (boundExpression.Type != conversion.BestUserDefinedConversionAnalysis.ToType)
			{
				boundExpression = MakeConversionNode(boundExpression, conversion.BestUserDefinedConversionAnalysis.ToType, @checked);
			}
			if (boundExpression.Type != rewrittenType)
			{
				boundExpression = MakeConversionNode(syntax, boundExpression, conversion.UserDefinedToConversion, rewrittenType, @checked);
			}
			return boundExpression;
		}
		return MakeConversionNode(null, syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
	}

	private BoundExpression RewriteTupleConversion(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, NamedTypeSymbol rewrittenType)
	{
		ImmutableArray<TypeSymbol> elementTypesOfTupleOrCompatible = rewrittenType.GetElementTypesOfTupleOrCompatible();
		int length = elementTypesOfTupleOrCompatible.Length;
		TypeSymbol type = rewrittenOperand.Type;
		TupleTypeSymbol tupleTypeSymbol = ((!type.IsTupleType) ? TupleTypeSymbol.Create((NamedTypeSymbol)type) : ((TupleTypeSymbol)type));
		ImmutableArray<FieldSymbol> tupleElements = tupleTypeSymbol.TupleElements;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(length);
		BoundLocal boundLocal = _factory.StoreToTemp(rewrittenOperand, out var store);
		ImmutableArray<Conversion> underlyingConversions = conversion.UnderlyingConversions;
		for (int i = 0; i < length; i++)
		{
			BoundExpression rewrittenOperand2 = MakeTupleFieldAccessAndReportUseSiteDiagnostics(boundLocal, syntax, tupleElements[i]);
			BoundExpression item = MakeConversionNode(syntax, rewrittenOperand2, underlyingConversions[i], elementTypesOfTupleOrCompatible[i], @checked, explicitCastInCode);
			instance.Add(item);
		}
		BoundExpression boundExpression = MakeTupleCreationExpression(syntax, rewrittenType, instance.ToImmutableAndFree());
		return _factory.MakeSequence(boundLocal.LocalSymbol, store, boundExpression);
	}

	private static bool NullableNeverHasValue(BoundExpression expression)
	{
		if (expression.IsLiteralNull())
		{
			return true;
		}
		if (!expression.Type.IsNullableType())
		{
			return false;
		}
		if (expression.Kind == BoundKind.DefaultExpression)
		{
			return true;
		}
		if (expression.Kind == BoundKind.ObjectCreationExpression && ((BoundObjectCreationExpression)expression).Arguments.Length == 0)
		{
			return true;
		}
		return false;
	}

	private static BoundExpression NullableAlwaysHasValue(BoundExpression expression)
	{
		if (expression.Type.IsNullableType() && expression.Kind == BoundKind.ObjectCreationExpression)
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)expression;
			if (boundObjectCreationExpression.Arguments.Length == 1)
			{
				return boundObjectCreationExpression.Arguments[0];
			}
		}
		return null;
	}

	private BoundExpression RewriteNullableConversion(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, TypeSymbol rewrittenType)
	{
		if (_inExpressionLambda)
		{
			return RewriteLiftedConversionInExpressionTree(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, rewrittenType);
		}
		TypeSymbol type = rewrittenOperand.Type;
		if (type.IsNullableType() && rewrittenType.IsNullableType())
		{
			return RewriteFullyLiftedBuiltInConversion(syntax, rewrittenOperand, conversion, @checked, rewrittenType);
		}
		if (rewrittenType.IsNullableType())
		{
			BoundExpression boundExpression = MakeConversionNode(syntax, rewrittenOperand, conversion.UnderlyingConversions[0], rewrittenType.GetNullableUnderlyingType(), @checked);
			MethodSymbol constructor = UnsafeGetNullableMethod(syntax, rewrittenType, SpecialMember.System_Nullable_T__ctor);
			return new BoundObjectCreationExpression(syntax, constructor, null, boundExpression);
		}
		BoundExpression boundExpression2 = NullableAlwaysHasValue(rewrittenOperand);
		if (boundExpression2 == null)
		{
			MethodSymbol method = UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T_get_Value);
			boundExpression2 = BoundCall.Synthesized(syntax, rewrittenOperand, method);
		}
		return MakeConversionNode(syntax, boundExpression2, conversion.UnderlyingConversions[0], rewrittenType, @checked);
	}

	private BoundExpression RewriteLiftedConversionInExpressionTree(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, TypeSymbol rewrittenType)
	{
		TypeSymbol type = rewrittenOperand.Type;
		TypeSymbol typeSymbol = type.StrippedType();
		TypeSymbol typeSymbol2 = rewrittenType.StrippedType();
		if (typeSymbol != typeSymbol2 && (typeSymbol.SpecialType == SpecialType.System_Decimal || typeSymbol2.SpecialType == SpecialType.System_Decimal))
		{
			TypeSymbol typeSymbol3 = typeSymbol;
			TypeSymbol typeTo = typeSymbol2;
			if (typeSymbol.IsEnumType())
			{
				typeSymbol3 = typeSymbol.GetEnumUnderlyingType();
				type = (type.IsNullableType() ? ((NamedTypeSymbol)type.OriginalDefinition).Construct(typeSymbol3) : typeSymbol3);
				rewrittenOperand = BoundConversion.SynthesizedNonUserDefined(syntax, rewrittenOperand, Conversion.ImplicitEnumeration, type);
			}
			else if (typeSymbol2.IsEnumType())
			{
				typeTo = typeSymbol2.GetEnumUnderlyingType();
			}
			MethodSymbol conversionMethod = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(DecimalConversionMethod(typeSymbol3, typeTo));
			ConversionKind kind = (conversion.Kind.IsImplicitConversion() ? ConversionKind.ImplicitUserDefined : ConversionKind.ExplicitUserDefined);
			return new BoundConversion(syntax, rewrittenOperand, new Conversion(kind, conversionMethod, isExtensionMethod: false), @checked, explicitCastInCode, null, rewrittenType);
		}
		return new BoundConversion(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, null, rewrittenType);
	}

	private BoundExpression RewriteFullyLiftedBuiltInConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, TypeSymbol type)
	{
		BoundExpression boundExpression = OptimizeLiftedBuiltInConversion(syntax, operand, conversion, @checked, type);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(operand, out var store);
		if (!TryGetNullableMethod(syntax, boundLocal.Type, SpecialMember.System_Nullable_T_GetValueOrDefault, out var result))
		{
			return BadExpression(syntax, type, operand);
		}
		BoundExpression rewrittenCondition = MakeNullableHasValue(syntax, boundLocal);
		BoundExpression rewrittenConsequence = new BoundObjectCreationExpression(syntax, UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T__ctor), null, MakeConversionNode(syntax, BoundCall.Synthesized(syntax, boundLocal, result), conversion.UnderlyingConversions[0], type.GetNullableUnderlyingType(), @checked));
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, null, type);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, type);
	}

	private BoundExpression OptimizeLiftedUserDefinedConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, TypeSymbol type)
	{
		if (NullableNeverHasValue(operand))
		{
			return new BoundDefaultExpression(syntax, type);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(operand);
		if (boundExpression != null)
		{
			return MakeLiftedUserDefinedConversionConsequence(BoundCall.Synthesized(syntax, null, conversion.Method, boundExpression), type);
		}
		return DistributeLiftedConversionIntoLiftedOperand(syntax, operand, conversion, @checked: false, type);
	}

	private BoundExpression OptimizeLiftedBuiltInConversion(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, TypeSymbol type)
	{
		if (NullableNeverHasValue(operand))
		{
			return new BoundDefaultExpression(syntax, null, type);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(operand);
		if (boundExpression != null)
		{
			return new BoundObjectCreationExpression(syntax, UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T__ctor), null, MakeConversionNode(syntax, boundExpression, conversion.UnderlyingConversions[0], type.GetNullableUnderlyingType(), @checked));
		}
		return DistributeLiftedConversionIntoLiftedOperand(syntax, operand, conversion, @checked, type);
	}

	private BoundExpression DistributeLiftedConversionIntoLiftedOperand(SyntaxNode syntax, BoundExpression operand, Conversion conversion, bool @checked, TypeSymbol type)
	{
		if (operand.Kind == BoundKind.Sequence)
		{
			BoundSequence boundSequence = (BoundSequence)operand;
			if (boundSequence.Value.Kind == BoundKind.ConditionalOperator)
			{
				BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)boundSequence.Value;
				if (NullableAlwaysHasValue(boundConditionalOperator.Consequence) != null && NullableNeverHasValue(boundConditionalOperator.Alternative))
				{
					return new BoundSequence(boundSequence.Syntax, boundSequence.Locals, boundSequence.SideEffects, RewriteConditionalOperator(boundConditionalOperator.Syntax, boundConditionalOperator.Condition, MakeConversionNode(null, syntax, boundConditionalOperator.Consequence, conversion, @checked, explicitCastInCode: false, null, type), MakeConversionNode(null, syntax, boundConditionalOperator.Alternative, conversion, @checked, explicitCastInCode: false, null, type), null, type, isRef: false), type);
				}
			}
		}
		return null;
	}

	private BoundExpression RewriteUserDefinedConversion(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, TypeSymbol rewrittenType)
	{
		if (rewrittenOperand.Type.IsNullableType())
		{
			TypeSymbol typeSymbol = conversion.Method.ParameterTypes[0];
			if (typeSymbol.Equals(rewrittenOperand.Type.GetNullableUnderlyingType(), TypeCompareKind.AllIgnoreOptions) && !typeSymbol.IsNullableType() && typeSymbol.IsValueType)
			{
				return RewriteLiftedUserDefinedConversion(syntax, rewrittenOperand, conversion, rewrittenType);
			}
		}
		if (!_inExpressionLambda && (!rewrittenOperand.Type.IsArray() || !_compilation.IsReadOnlySpanType(rewrittenType)))
		{
			return BoundCall.Synthesized(syntax, null, conversion.Method, rewrittenOperand);
		}
		return BoundConversion.Synthesized(syntax, rewrittenOperand, conversion, @checked: false, explicitCastInCode: true, null, rewrittenType);
	}

	private BoundExpression MakeLiftedUserDefinedConversionConsequence(BoundCall call, TypeSymbol resultType)
	{
		if (call.Method.ReturnType.IsNonNullableValueType())
		{
			MethodSymbol constructor = UnsafeGetNullableMethod(call.Syntax, resultType, SpecialMember.System_Nullable_T__ctor);
			return new BoundObjectCreationExpression(call.Syntax, constructor, null, call);
		}
		return call;
	}

	private BoundExpression RewriteLiftedUserDefinedConversion(SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, TypeSymbol rewrittenType)
	{
		if (_inExpressionLambda)
		{
			Conversion conversion2 = TryMakeConversion(syntax, conversion, rewrittenOperand.Type, rewrittenType);
			return BoundConversion.Synthesized(syntax, rewrittenOperand, conversion2, @checked: false, explicitCastInCode: true, null, rewrittenType);
		}
		BoundExpression boundExpression = OptimizeLiftedUserDefinedConversion(syntax, rewrittenOperand, conversion, rewrittenType);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(rewrittenOperand, out var store);
		MethodSymbol method = UnsafeGetNullableMethod(syntax, boundLocal.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		BoundExpression rewrittenCondition = MakeNullableHasValue(syntax, boundLocal);
		BoundCall arg = BoundCall.Synthesized(syntax, boundLocal, method);
		BoundCall call = BoundCall.Synthesized(syntax, null, conversion.Method, arg);
		BoundExpression rewrittenConsequence = MakeLiftedUserDefinedConversionConsequence(call, rewrittenType);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, rewrittenType);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, rewrittenType, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, rewrittenType);
	}

	private BoundExpression RewriteIntPtrConversion(BoundConversion oldNode, SyntaxNode syntax, BoundExpression rewrittenOperand, Conversion conversion, bool @checked, bool explicitCastInCode, ConstantValue constantValueOpt, TypeSymbol rewrittenType)
	{
		TypeSymbol type = rewrittenOperand.Type;
		SpecialMember intPtrConversionMethod = GetIntPtrConversionMethod(rewrittenOperand.Type, rewrittenType);
		if (!TryGetSpecialTypeMethod(syntax, intPtrConversionMethod, out var method))
		{
			return BadExpression(syntax, rewrittenType, rewrittenOperand);
		}
		conversion = conversion.SetConversionMethod(method);
		if (type.IsNullableType() && rewrittenType.IsNullableType())
		{
			return RewriteLiftedUserDefinedConversion(syntax, rewrittenOperand, conversion, rewrittenType);
		}
		if (type.IsNullableType())
		{
			rewrittenOperand = MakeConversionNode(rewrittenOperand, type.StrippedType(), @checked);
		}
		rewrittenOperand = MakeConversionNode(rewrittenOperand, method.ParameterTypes[0], @checked);
		TypeSymbol returnType = method.ReturnType;
		if (_inExpressionLambda)
		{
			return BoundConversion.Synthesized(syntax, rewrittenOperand, conversion, @checked, explicitCastInCode, constantValueOpt, rewrittenType);
		}
		BoundExpression rewrittenOperand2 = MakeCall(syntax, null, method, ImmutableArray.Create(rewrittenOperand), returnType);
		return MakeConversionNode(rewrittenOperand2, rewrittenType, @checked);
	}

	public static SpecialMember GetIntPtrConversionMethod(TypeSymbol source, TypeSymbol target)
	{
		TypeSymbol typeSymbol = target.StrippedType();
		TypeSymbol typeSymbol2 = source.StrippedType();
		SpecialType specialType = (typeSymbol.IsEnumType() ? typeSymbol.GetEnumUnderlyingType().SpecialType : typeSymbol.SpecialType);
		SpecialType specialType2 = (typeSymbol2.IsEnumType() ? typeSymbol2.GetEnumUnderlyingType().SpecialType : typeSymbol2.SpecialType);
		switch (specialType)
		{
		case SpecialType.System_IntPtr:
			if (source.TypeKind == TypeKind.Pointer)
			{
				return SpecialMember.System_IntPtr__op_Explicit_FromPointer;
			}
			switch (specialType2)
			{
			case SpecialType.System_Char:
			case SpecialType.System_SByte:
			case SpecialType.System_Byte:
			case SpecialType.System_Int16:
			case SpecialType.System_UInt16:
			case SpecialType.System_Int32:
				return SpecialMember.System_IntPtr__op_Explicit_FromInt32;
			case SpecialType.System_UInt32:
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
			case SpecialType.System_Decimal:
			case SpecialType.System_Single:
			case SpecialType.System_Double:
				return SpecialMember.System_IntPtr__op_Explicit_FromInt64;
			}
			break;
		case SpecialType.System_UIntPtr:
			if (source.TypeKind == TypeKind.Pointer)
			{
				return SpecialMember.System_UIntPtr__op_Explicit_FromPointer;
			}
			switch (specialType2)
			{
			case SpecialType.System_Char:
			case SpecialType.System_Byte:
			case SpecialType.System_UInt16:
			case SpecialType.System_UInt32:
				return SpecialMember.System_UIntPtr__op_Explicit_FromUInt32;
			case SpecialType.System_SByte:
			case SpecialType.System_Int16:
			case SpecialType.System_Int32:
			case SpecialType.System_Int64:
			case SpecialType.System_UInt64:
			case SpecialType.System_Decimal:
			case SpecialType.System_Single:
			case SpecialType.System_Double:
				return SpecialMember.System_UIntPtr__op_Explicit_FromUInt64;
			}
			break;
		default:
			switch (specialType2)
			{
			case SpecialType.System_IntPtr:
				if (target.TypeKind == TypeKind.Pointer)
				{
					return SpecialMember.System_IntPtr__op_Explicit_ToPointer;
				}
				switch (specialType)
				{
				case SpecialType.System_Char:
				case SpecialType.System_SByte:
				case SpecialType.System_Byte:
				case SpecialType.System_Int16:
				case SpecialType.System_UInt16:
				case SpecialType.System_Int32:
				case SpecialType.System_UInt32:
					return SpecialMember.System_IntPtr__op_Explicit_ToInt32;
				case SpecialType.System_Int64:
				case SpecialType.System_UInt64:
				case SpecialType.System_Decimal:
				case SpecialType.System_Single:
				case SpecialType.System_Double:
					return SpecialMember.System_IntPtr__op_Explicit_ToInt64;
				}
				break;
			case SpecialType.System_UIntPtr:
				if (target.TypeKind == TypeKind.Pointer)
				{
					return SpecialMember.System_UIntPtr__op_Explicit_ToPointer;
				}
				switch (specialType)
				{
				case SpecialType.System_Char:
				case SpecialType.System_SByte:
				case SpecialType.System_Byte:
				case SpecialType.System_Int16:
				case SpecialType.System_UInt16:
				case SpecialType.System_Int32:
				case SpecialType.System_UInt32:
					return SpecialMember.System_UIntPtr__op_Explicit_ToUInt32;
				case SpecialType.System_Int64:
				case SpecialType.System_UInt64:
				case SpecialType.System_Decimal:
				case SpecialType.System_Single:
				case SpecialType.System_Double:
					return SpecialMember.System_UIntPtr__op_Explicit_ToUInt64;
				}
				break;
			}
			break;
		}
		throw ExceptionUtilities.Unreachable;
	}

	internal static SpecialMember DecimalConversionMethod(TypeSymbol typeFrom, TypeSymbol typeTo)
	{
		if (typeFrom.SpecialType == SpecialType.System_Decimal)
		{
			return typeTo.SpecialType switch
			{
				SpecialType.System_Char => SpecialMember.System_Decimal__op_Explicit_ToChar, 
				SpecialType.System_SByte => SpecialMember.System_Decimal__op_Explicit_ToSByte, 
				SpecialType.System_Byte => SpecialMember.System_Decimal__op_Explicit_ToByte, 
				SpecialType.System_Int16 => SpecialMember.System_Decimal__op_Explicit_ToInt16, 
				SpecialType.System_UInt16 => SpecialMember.System_Decimal__op_Explicit_ToUInt16, 
				SpecialType.System_Int32 => SpecialMember.System_Decimal__op_Explicit_ToInt32, 
				SpecialType.System_UInt32 => SpecialMember.System_Decimal__op_Explicit_ToUInt32, 
				SpecialType.System_Int64 => SpecialMember.System_Decimal__op_Explicit_ToInt64, 
				SpecialType.System_UInt64 => SpecialMember.System_Decimal__op_Explicit_ToUInt64, 
				SpecialType.System_Single => SpecialMember.System_Decimal__op_Explicit_ToSingle, 
				SpecialType.System_Double => SpecialMember.System_Decimal__op_Explicit_ToDouble, 
				_ => throw ExceptionUtilities.UnexpectedValue(typeTo.SpecialType), 
			};
		}
		return typeFrom.SpecialType switch
		{
			SpecialType.System_Char => SpecialMember.System_Decimal__op_Implicit_FromChar, 
			SpecialType.System_SByte => SpecialMember.System_Decimal__op_Implicit_FromSByte, 
			SpecialType.System_Byte => SpecialMember.System_Decimal__op_Implicit_FromByte, 
			SpecialType.System_Int16 => SpecialMember.System_Decimal__op_Implicit_FromInt16, 
			SpecialType.System_UInt16 => SpecialMember.System_Decimal__op_Implicit_FromUInt16, 
			SpecialType.System_Int32 => SpecialMember.System_Decimal__op_Implicit_FromInt32, 
			SpecialType.System_UInt32 => SpecialMember.System_Decimal__op_Implicit_FromUInt32, 
			SpecialType.System_Int64 => SpecialMember.System_Decimal__op_Implicit_FromInt64, 
			SpecialType.System_UInt64 => SpecialMember.System_Decimal__op_Implicit_FromUInt64, 
			SpecialType.System_Single => SpecialMember.System_Decimal__op_Explicit_FromSingle, 
			SpecialType.System_Double => SpecialMember.System_Decimal__op_Explicit_FromDouble, 
			_ => throw ExceptionUtilities.UnexpectedValue(typeFrom.SpecialType), 
		};
	}

	private BoundExpression RewriteDecimalConversion(SyntaxNode syntax, BoundExpression operand, TypeSymbol fromType, TypeSymbol toType, bool isImplicit, ConstantValue constantValueOpt)
	{
		SpecialMember member = DecimalConversionMethod(fromType, toType);
		MethodSymbol methodSymbol = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(member);
		if (_inExpressionLambda)
		{
			ConversionKind kind = (isImplicit ? ConversionKind.ImplicitUserDefined : ConversionKind.ExplicitUserDefined);
			Conversion conversion = new Conversion(kind, methodSymbol, isExtensionMethod: false);
			return new BoundConversion(syntax, operand, conversion, @checked: false, explicitCastInCode: false, constantValueOpt, toType);
		}
		return BoundCall.Synthesized(syntax, null, methodSymbol, operand);
	}

	private Conversion TryMakeConversion(SyntaxNode syntax, Conversion conversion, TypeSymbol fromType, TypeSymbol toType)
	{
		switch (conversion.Kind)
		{
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.ExplicitUserDefined:
		{
			MethodSymbol method5 = conversion.Method;
			Conversion conversion2 = TryMakeConversion(syntax, conversion.UserDefinedFromConversion, fromType, method5.Parameters[0].Type);
			if (!conversion2.Exists)
			{
				return Conversion.NoConversion;
			}
			Conversion conversion3 = TryMakeConversion(syntax, conversion.UserDefinedToConversion, method5.ReturnType, toType);
			if (!conversion3.Exists)
			{
				return Conversion.NoConversion;
			}
			if (conversion2 == conversion.UserDefinedFromConversion && conversion3 == conversion.UserDefinedToConversion)
			{
				return conversion;
			}
			return new Conversion(UserDefinedConversionResult.Valid(ImmutableArray.Create(UserDefinedConversionAnalysis.Normal(method5, conversion2, conversion3, fromType, toType)), 0), conversion.IsImplicit);
		}
		case ConversionKind.IntPtr:
		{
			SpecialMember intPtrConversionMethod = GetIntPtrConversionMethod(fromType, toType);
			if (!TryGetSpecialTypeMethod(syntax, intPtrConversionMethod, out var method4))
			{
				return Conversion.NoConversion;
			}
			return TryMakeUserDefinedConversion(syntax, method4, fromType, toType, conversion.IsImplicit);
		}
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ExplicitNumeric:
			if (fromType.SpecialType == SpecialType.System_Decimal || toType.SpecialType == SpecialType.System_Decimal)
			{
				SpecialMember specialMember3 = DecimalConversionMethod(fromType, toType);
				if (!TryGetSpecialTypeMethod(syntax, specialMember3, out var method3))
				{
					return Conversion.NoConversion;
				}
				return TryMakeUserDefinedConversion(syntax, method3, fromType, toType, conversion.IsImplicit);
			}
			return conversion;
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ExplicitEnumeration:
			if (fromType.SpecialType == SpecialType.System_Decimal)
			{
				SpecialMember specialMember = DecimalConversionMethod(fromType, toType.GetEnumUnderlyingType());
				if (!TryGetSpecialTypeMethod(syntax, specialMember, out var method))
				{
					return Conversion.NoConversion;
				}
				return TryMakeUserDefinedConversion(syntax, method, fromType, toType, conversion.IsImplicit);
			}
			if (toType.SpecialType == SpecialType.System_Decimal)
			{
				SpecialMember specialMember2 = DecimalConversionMethod(fromType.GetEnumUnderlyingType(), toType);
				if (!TryGetSpecialTypeMethod(syntax, specialMember2, out var method2))
				{
					return Conversion.NoConversion;
				}
				return TryMakeUserDefinedConversion(syntax, method2, fromType, toType, conversion.IsImplicit);
			}
			return conversion;
		default:
			return conversion;
		}
	}

	private Conversion TryMakeConversion(SyntaxNode syntax, TypeSymbol fromType, TypeSymbol toType)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion result = TryMakeConversion(syntax, _compilation.Conversions.ClassifyConversionFromType(fromType, toType, ref useSiteDiagnostics), fromType, toType);
		_diagnostics.Add(syntax, useSiteDiagnostics);
		return result;
	}

	private Conversion TryMakeUserDefinedConversion(SyntaxNode syntax, MethodSymbol meth, TypeSymbol fromType, TypeSymbol toType, bool isImplicit = true)
	{
		Conversion sourceConversion = TryMakeConversion(syntax, fromType, meth.Parameters[0].Type);
		if (!sourceConversion.Exists)
		{
			return Conversion.NoConversion;
		}
		Conversion targetConversion = TryMakeConversion(syntax, meth.ReturnType, toType);
		if (!targetConversion.Exists)
		{
			return Conversion.NoConversion;
		}
		return new Conversion(UserDefinedConversionResult.Valid(ImmutableArray.Create(UserDefinedConversionAnalysis.Normal(meth, sourceConversion, targetConversion, fromType, toType)), 0), isImplicit);
	}

	public override BoundNode VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		BoundConversion right = node.Right;
		return RewriteDeconstruction(node.Left, right.Conversion, right.Operand, node.IsUsed);
	}

	private BoundExpression RewriteDeconstruction(BoundTupleExpression left, Conversion conversion, BoundExpression right, bool isUsed)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		DeconstructionSideEffects instance2 = DeconstructionSideEffects.GetInstance();
		ArrayBuilder<Binder.DeconstructionVariable> assignmentTargetsAndSideEffects = GetAssignmentTargetsAndSideEffects(left, instance, instance2.init);
		BoundExpression boundExpression = ApplyDeconstructionConversion(assignmentTargetsAndSideEffects, right, conversion, instance, instance2, isUsed, inInit: true);
		instance2.Consolidate();
		BoundExpression result;
		if (!isUsed)
		{
			BoundExpression boundExpression2 = instance2.PopLast();
			if (boundExpression2 == null)
			{
				result = null;
				instance.Free();
				instance2.ToImmutableAndFree();
			}
			else
			{
				result = _factory.Sequence(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), boundExpression2);
			}
		}
		else
		{
			if (!boundExpression.HasErrors)
			{
				boundExpression = VisitExpression(boundExpression);
			}
			result = _factory.Sequence(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), boundExpression);
		}
		Binder.DeconstructionVariable.FreeDeconstructionVariables(assignmentTargetsAndSideEffects);
		return result;
	}

	private BoundExpression ApplyDeconstructionConversion(ArrayBuilder<Binder.DeconstructionVariable> leftTargets, BoundExpression right, Conversion conversion, ArrayBuilder<LocalSymbol> temps, DeconstructionSideEffects effects, bool isUsed, bool inInit)
	{
		ImmutableArray<BoundExpression> rightParts = GetRightParts(right, conversion, ref temps, effects, ref inInit);
		ImmutableArray<Conversion> underlyingConversions = conversion.UnderlyingConversions;
		ArrayBuilder<BoundExpression> arrayBuilder = (isUsed ? ArrayBuilder<BoundExpression>.GetInstance(leftTargets.Count) : null);
		for (int i = 0; i < leftTargets.Count; i++)
		{
			BoundExpression boundExpression;
			if (leftTargets[i].HasNestedVariables)
			{
				boundExpression = ApplyDeconstructionConversion(leftTargets[i].NestedVariables, rightParts[i], underlyingConversions[i], temps, effects, isUsed, inInit);
			}
			else
			{
				BoundExpression boundExpression2 = rightParts[i];
				if (inInit)
				{
					boundExpression2 = EvaluateSideEffectingArgumentToTemp(VisitExpression(boundExpression2), inInit ? effects.init : effects.deconstructions, ref temps);
				}
				BoundExpression single = leftTargets[i].Single;
				boundExpression = EvaluateConversionToTemp(boundExpression2, underlyingConversions[i], single.Type, temps, effects.conversions);
				if (single.Kind != BoundKind.DiscardExpression)
				{
					effects.assignments.Add(MakeAssignmentOperator(boundExpression.Syntax, single, boundExpression, single.Type, used: true, isChecked: false, isCompoundAssignment: false));
				}
			}
			arrayBuilder?.Add(boundExpression);
		}
		if (isUsed)
		{
			NamedTypeSymbol type = TupleTypeSymbol.Create(null, arrayBuilder.SelectAsArray((BoundExpression e) => e.Type), default, default, _compilation, shouldCheckConstraints: false, default);
			return new BoundTupleLiteral(right.Syntax, default, default, arrayBuilder.ToImmutableAndFree(), type);
		}
		return null;
	}

	private ImmutableArray<BoundExpression> GetRightParts(BoundExpression right, Conversion conversion, ref ArrayBuilder<LocalSymbol> temps, DeconstructionSideEffects effects, ref bool inInit)
	{
		DeconstructMethodInfo deconstructionInfo = conversion.DeconstructionInfo;
		if (!deconstructionInfo.IsDefault)
		{
			BoundExpression target = EvaluateSideEffectingArgumentToTemp(VisitExpression(right), inInit ? effects.init : effects.deconstructions, ref temps);
			inInit = false;
			return InvokeDeconstructMethod(deconstructionInfo, target, effects.deconstructions, ref temps);
		}
		if (IsTupleExpression(right.Kind))
		{
			return ((BoundTupleExpression)right).Arguments;
		}
		if (right.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)right;
			if ((boundConversion.Conversion.Kind == ConversionKind.ImplicitTupleLiteral || boundConversion.Conversion.Kind == ConversionKind.Identity) && IsTupleExpression(boundConversion.Operand.Kind))
			{
				return ((BoundTupleExpression)boundConversion.Operand).Arguments;
			}
		}
		if (right.Type.IsTupleType)
		{
			inInit = false;
			return AccessTupleFields(VisitExpression(right), temps, effects.deconstructions);
		}
		throw ExceptionUtilities.Unreachable;
	}

	private static bool IsTupleExpression(BoundKind kind)
	{
		if (kind != BoundKind.TupleLiteral)
		{
			return kind == BoundKind.ConvertedTupleLiteral;
		}
		return true;
	}

	private ImmutableArray<BoundExpression> AccessTupleFields(BoundExpression expression, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		TypeSymbol type = expression.Type;
		int length = type.TupleElementTypes.Length;
		BoundExpression tuple;
		if (CanChangeValueBetweenReads(expression))
		{
			BoundLocal boundLocal = _factory.StoreToTemp(expression, out var store);
			effects.Add(store);
			temps.Add(boundLocal.LocalSymbol);
			tuple = boundLocal;
		}
		else
		{
			tuple = expression;
		}
		ImmutableArray<FieldSymbol> tupleElements = type.TupleElements;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(length);
		for (int i = 0; i < length; i++)
		{
			BoundExpression item = MakeTupleFieldAccessAndReportUseSiteDiagnostics(tuple, expression.Syntax, tupleElements[i]);
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	private BoundExpression EvaluateConversionToTemp(BoundExpression expression, Conversion conversion, TypeSymbol destinationType, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		if (conversion.IsIdentity)
		{
			return expression;
		}
		BoundExpression arg = MakeConversionNode(expression.Syntax, expression, conversion, destinationType, @checked: false);
		return EvaluateSideEffectingArgumentToTemp(arg, effects, ref temps);
	}

	private ImmutableArray<BoundExpression> InvokeDeconstructMethod(DeconstructMethodInfo deconstruction, BoundExpression target, ArrayBuilder<BoundExpression> effects, ref ArrayBuilder<LocalSymbol> temps)
	{
		AddPlaceholderReplacement(deconstruction.InputPlaceholder, target);
		ImmutableArray<BoundDeconstructValuePlaceholder> outputPlaceholders = deconstruction.OutputPlaceholders;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(outputPlaceholders.Length);
		foreach (BoundDeconstructValuePlaceholder item in outputPlaceholders)
		{
			SynthesizedLocal synthesizedLocal = new SynthesizedLocal(_factory.CurrentFunction, item.Type, SynthesizedLocalKind.LoweringTemp);
			BoundLocal boundLocal = new BoundLocal(target.Syntax, synthesizedLocal, null, item.Type)
			{
				WasCompilerGenerated = true
			};
			temps.Add(synthesizedLocal);
			AddPlaceholderReplacement(item, boundLocal);
			instance.Add(boundLocal);
		}
		effects.Add(VisitExpression(deconstruction.Invocation));
		RemovePlaceholderReplacement(deconstruction.InputPlaceholder);
		foreach (BoundDeconstructValuePlaceholder item2 in outputPlaceholders)
		{
			RemovePlaceholderReplacement(item2);
		}
		return instance.ToImmutableAndFree();
	}

	private BoundExpression EvaluateSideEffectingArgumentToTemp(BoundExpression arg, ArrayBuilder<BoundExpression> effects, ref ArrayBuilder<LocalSymbol> temps)
	{
		if (CanChangeValueBetweenReads(arg, localsMayBeAssignedOrCaptured: true, structThisCanChangeValueBetweenReads: true))
		{
			BoundLocal boundLocal = _factory.StoreToTemp(arg, out var store);
			if (temps == null)
			{
				temps = ArrayBuilder<LocalSymbol>.GetInstance();
			}
			temps.Add(boundLocal.LocalSymbol);
			effects.Add(store);
			return boundLocal;
		}
		return arg;
	}

	private ArrayBuilder<Binder.DeconstructionVariable> GetAssignmentTargetsAndSideEffects(BoundTupleExpression variables, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		ArrayBuilder<Binder.DeconstructionVariable> instance = ArrayBuilder<Binder.DeconstructionVariable>.GetInstance(variables.Arguments.Length);
		foreach (BoundExpression argument in variables.Arguments)
		{
			switch (argument.Kind)
			{
			case BoundKind.DiscardExpression:
				instance.Add(new Binder.DeconstructionVariable(argument, argument.Syntax));
				break;
			case BoundKind.TupleLiteral:
			{
				BoundTupleExpression boundTupleExpression = (BoundTupleExpression)argument;
				instance.Add(new Binder.DeconstructionVariable(GetAssignmentTargetsAndSideEffects(boundTupleExpression, temps, effects), boundTupleExpression.Syntax));
				break;
			}
			case BoundKind.ConvertedTupleLiteral:
				throw ExceptionUtilities.UnexpectedValue(argument.Kind);
			default:
			{
				BoundExpression variable = TransformCompoundAssignmentLHS(argument, effects, temps, argument.Type.IsDynamic());
				instance.Add(new Binder.DeconstructionVariable(variable, argument.Syntax));
				break;
			}
			}
		}
		return instance;
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		if (node.Argument.HasDynamicType())
		{
			BoundExpression loweredOperand = VisitExpression(node.Argument);
			BoundExpression argument = _dynamicFactory.MakeDynamicConversion(loweredOperand, isExplicit: false, isArrayIndex: false, isChecked: false, node.Type).ToExpression();
			return new BoundDelegateCreationExpression(node.Syntax, argument, null, isExtensionMethod: false, node.Type);
		}
		if (node.Argument.Kind == BoundKind.MethodGroup)
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)node.Argument;
			MethodSymbol methodOpt = node.MethodOpt;
			SyntaxNode syntax = _factory.Syntax;
			_factory.Syntax = (boundMethodGroup.ReceiverOpt ?? boundMethodGroup).Syntax;
			BoundExpression argument2 = ((methodOpt.IsStatic && !node.IsExtensionMethod) ? _factory.Type(methodOpt.ContainingType) : VisitExpression(boundMethodGroup.ReceiverOpt));
			_factory.Syntax = syntax;
			return node.Update(argument2, methodOpt, node.IsExtensionMethod, node.Type);
		}
		return base.VisitDelegateCreationExpression(node);
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		BoundExpression boundExpression = (BoundExpression)Visit(node.Condition);
		BoundStatement boundStatement = (BoundStatement)Visit(node.Body);
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("start");
		SyntaxNode syntax = node.Syntax;
		if (!node.WasCompilerGenerated && Instrument)
		{
			boundExpression = _instrumenter.InstrumentDoStatementCondition(node, boundExpression, _factory);
		}
		BoundStatement boundStatement2 = new BoundConditionalGoto(syntax, boundExpression, jumpIfTrue: true, label);
		if (!node.WasCompilerGenerated && Instrument)
		{
			boundStatement2 = _instrumenter.InstrumentDoStatementConditionalGotoStart(node, boundStatement2);
		}
		if (node.Locals.IsEmpty)
		{
			return BoundStatementList.Synthesized(syntax, node.HasErrors, new BoundLabelStatement(syntax, label), boundStatement, new BoundLabelStatement(syntax, node.ContinueLabel), boundStatement2, new BoundLabelStatement(syntax, node.BreakLabel));
		}
		return BoundStatementList.Synthesized(syntax, node.HasErrors, new BoundLabelStatement(syntax, label), new BoundBlock(syntax, node.Locals, ImmutableArray.Create(boundStatement, new BoundLabelStatement(syntax, node.ContinueLabel), boundStatement2)), new BoundLabelStatement(syntax, node.BreakLabel));
	}

	public override BoundNode VisitEventAssignmentOperator(BoundEventAssignmentOperator node)
	{
		BoundExpression boundExpression = VisitExpression(node.ReceiverOpt);
		BoundExpression boundExpression2 = VisitExpression(node.Argument);
		if (boundExpression != null && node.Event.ContainingAssembly.IsLinked && node.Event.ContainingType.IsInterfaceType())
		{
			NamedTypeSymbol containingType = node.Event.ContainingType;
			foreach (CSharpAttributeData attribute in containingType.GetAttributes())
			{
				if (attribute.IsTargetAttribute(containingType, AttributeDescription.ComEventInterfaceAttribute) && attribute.CommonConstructorArguments.Length == 2)
				{
					return RewriteNoPiaEventAssignmentOperator(node, boundExpression, boundExpression2);
				}
			}
		}
		if (node.Event.IsWindowsRuntimeEvent)
		{
			EventAssignmentKind kind = (node.IsAddition ? EventAssignmentKind.Addition : EventAssignmentKind.Subtraction);
			return RewriteWindowsRuntimeEventAssignmentOperator(node.Syntax, node.Event, kind, node.IsDynamic, boundExpression, boundExpression2);
		}
		ImmutableArray<BoundExpression> rewrittenArguments = ImmutableArray.Create(boundExpression2);
		MethodSymbol method = (node.IsAddition ? node.Event.AddMethod : node.Event.RemoveMethod);
		return MakeCall(node.Syntax, boundExpression, method, rewrittenArguments, node.Type);
	}

	private BoundExpression RewriteWindowsRuntimeEventAssignmentOperator(SyntaxNode syntax, EventSymbol eventSymbol, EventAssignmentKind kind, bool isDynamic, BoundExpression rewrittenReceiverOpt, BoundExpression rewrittenArgument)
	{
		BoundAssignmentOperator store = null;
		BoundLocal boundLocal = null;
		if (!eventSymbol.IsStatic && CanChangeValueBetweenReads(rewrittenReceiverOpt))
		{
			boundLocal = _factory.StoreToTemp(rewrittenReceiverOpt, out store);
		}
		NamedTypeSymbol namedTypeSymbol = _factory.WellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationToken);
		_factory.WellKnownType(WellKnownType.System_Runtime_InteropServices_WindowsRuntime_WindowsRuntimeMarshal);
		NamedTypeSymbol type = _factory.WellKnownType(WellKnownType.System_Action_T).Construct(namedTypeSymbol);
		TypeSymbol type2 = eventSymbol.Type;
		BoundExpression argument = boundLocal ?? rewrittenReceiverOpt ?? _factory.Type(type2);
		BoundDelegateCreationExpression boundDelegateCreationExpression = new BoundDelegateCreationExpression(syntax, argument, eventSymbol.RemoveMethod, isExtensionMethod: false, type);
		BoundExpression boundExpression = null;
		if (kind == EventAssignmentKind.Assignment)
		{
			boundExpression = ((!TryGetWellKnownTypeMember<MethodSymbol>(syntax, WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_WindowsRuntimeMarshal__RemoveAllEventHandlers, out var symbol)) ? new BoundBadExpression(syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)boundDelegateCreationExpression), ErrorTypeSymbol.UnknownResultType) : MakeCall(syntax, null, symbol, ImmutableArray.Create((BoundExpression)boundDelegateCreationExpression), symbol.ReturnType));
		}
		WellKnownMember member;
		ImmutableArray<BoundExpression> immutableArray;
		if (kind == EventAssignmentKind.Subtraction)
		{
			member = WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_WindowsRuntimeMarshal__RemoveEventHandler_T;
			immutableArray = ImmutableArray.Create(boundDelegateCreationExpression, rewrittenArgument);
		}
		else
		{
			NamedTypeSymbol type3 = _factory.WellKnownType(WellKnownType.System_Func_T2).Construct(type2, namedTypeSymbol);
			BoundDelegateCreationExpression item = new BoundDelegateCreationExpression(syntax, argument, eventSymbol.AddMethod, isExtensionMethod: false, type3);
			member = WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_WindowsRuntimeMarshal__AddEventHandler_T;
			immutableArray = ImmutableArray.Create(item, boundDelegateCreationExpression, rewrittenArgument);
		}
		BoundExpression boundExpression2;
		if (TryGetWellKnownTypeMember<MethodSymbol>(syntax, member, out var symbol2))
		{
			symbol2 = symbol2.Construct(type2);
			boundExpression2 = MakeCall(syntax, null, symbol2, immutableArray, symbol2.ReturnType);
		}
		else
		{
			boundExpression2 = new BoundBadExpression(syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, immutableArray, ErrorTypeSymbol.UnknownResultType);
		}
		if (boundLocal == null && boundExpression == null)
		{
			return boundExpression2;
		}
		ImmutableArray<LocalSymbol> locals = ((boundLocal == null) ? ImmutableArray<LocalSymbol>.Empty : ImmutableArray.Create(boundLocal.LocalSymbol));
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(2);
		if (boundExpression != null)
		{
			instance.Add(boundExpression);
		}
		if (store != null)
		{
			instance.Add(store);
		}
		return new BoundSequence(syntax, locals, instance.ToImmutableAndFree(), boundExpression2, boundExpression2.Type);
	}

	private BoundExpression VisitWindowsRuntimeEventFieldAssignmentOperator(SyntaxNode syntax, BoundEventAccess left, BoundExpression rewrittenRight)
	{
		EventSymbol eventSymbol = left.EventSymbol;
		BoundExpression rewrittenReceiverOpt = ((left.ReceiverOpt == null) ? null : VisitExpression(left.ReceiverOpt));
		return RewriteWindowsRuntimeEventAssignmentOperator(syntax, eventSymbol, EventAssignmentKind.Assignment, isDynamic: false, rewrittenReceiverOpt, rewrittenRight);
	}

	public override BoundNode VisitEventAccess(BoundEventAccess node)
	{
		BoundExpression rewrittenReceiver = VisitExpression(node.ReceiverOpt);
		return MakeEventAccess(node.Syntax, rewrittenReceiver, node.EventSymbol, node.ConstantValue, node.ResultKind, node.Type);
	}

	private BoundExpression MakeEventAccess(SyntaxNode syntax, BoundExpression rewrittenReceiver, EventSymbol eventSymbol, ConstantValue constantValueOpt, LookupResultKind resultKind, TypeSymbol type)
	{
		FieldSymbol associatedField = eventSymbol.AssociatedField;
		if (!eventSymbol.IsWindowsRuntimeEvent)
		{
			return MakeFieldAccess(syntax, rewrittenReceiver, associatedField, constantValueOpt, resultKind, type);
		}
		NamedTypeSymbol newOwner = (NamedTypeSymbol)associatedField.Type;
		BoundFieldAccess boundFieldAccess = new BoundFieldAccess(syntax, associatedField.IsStatic ? null : rewrittenReceiver, associatedField, null)
		{
			WasCompilerGenerated = true
		};
		BoundExpression boundExpression;
		if (TryGetWellKnownTypeMember<MethodSymbol>(syntax, WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T__GetOrCreateEventRegistrationTokenTable, out var symbol))
		{
			symbol = symbol.AsMember(newOwner);
			boundExpression = BoundCall.Synthesized(syntax, null, symbol, boundFieldAccess);
		}
		else
		{
			boundExpression = new BoundBadExpression(syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)boundFieldAccess), ErrorTypeSymbol.UnknownResultType);
		}
		if (TryGetWellKnownTypeMember<PropertySymbol>(syntax, WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T__InvocationList, out var symbol2))
		{
			MethodSymbol getMethod = symbol2.GetMethod;
			if ((object)getMethod != null)
			{
				getMethod = getMethod.AsMember(newOwner);
				return _factory.Call(boundExpression, getMethod);
			}
			string accessorName = SourcePropertyAccessorSymbol.GetAccessorName(symbol2.Name, getNotSet: true, symbol2.IsCompilationOutputWinMdObj());
			_diagnostics.Add(new CSDiagnosticInfo(ErrorCode.ERR_MissingPredefinedMember, symbol2.ContainingType, accessorName), syntax.Location);
		}
		return new BoundBadExpression(syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(boundExpression), ErrorTypeSymbol.UnknownResultType);
	}

	private BoundExpression RewriteNoPiaEventAssignmentOperator(BoundEventAssignmentOperator node, BoundExpression rewrittenReceiver, BoundExpression rewrittenArgument)
	{
		BoundExpression boundExpression = null;
		SyntaxNode syntax = _factory.Syntax;
		_factory.Syntax = node.Syntax;
		MethodSymbol methodSymbol = _factory.WellKnownMethod(WellKnownMember.System_Runtime_InteropServices_ComAwareEventInfo__ctor);
		if ((object)methodSymbol != null)
		{
			MethodSymbol methodSymbol2 = _factory.WellKnownMethod(node.IsAddition ? WellKnownMember.System_Runtime_InteropServices_ComAwareEventInfo__AddEventHandler : WellKnownMember.System_Runtime_InteropServices_ComAwareEventInfo__RemoveEventHandler);
			if ((object)methodSymbol2 != null)
			{
				BoundExpression receiver = _factory.New(methodSymbol, _factory.Typeof(node.Event.ContainingType), _factory.Literal(node.Event.MetadataName));
				boundExpression = _factory.Call(receiver, methodSymbol2, _factory.Convert(methodSymbol2.Parameters[0].Type, rewrittenReceiver), _factory.Convert(methodSymbol2.Parameters[1].Type, rewrittenArgument));
			}
		}
		_factory.Syntax = syntax;
		EmitModule?.EmbeddedTypesManagerOpt.EmbedEventIfNeedTo(node.Event, node.Syntax, _diagnostics, isUsedForComAwareEventBinding: true);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		return new BoundBadExpression(node.Syntax, LookupResultKind.NotCreatable, ImmutableArray.Create((Symbol)node.Event), ImmutableArray.Create(rewrittenReceiver, rewrittenArgument), ErrorTypeSymbol.UnknownResultType);
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		return RewriteExpressionStatement(node) ?? BoundStatementList.Synthesized(node.Syntax);
	}

	private BoundStatement RewriteExpressionStatement(BoundExpressionStatement node, bool suppressInstrumentation = false)
	{
		BoundExpression boundExpression = VisitUnusedExpression(node.Expression);
		if (boundExpression == null)
		{
			return null;
		}
		BoundStatement boundStatement = node.Update(boundExpression);
		if (!suppressInstrumentation && Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = _instrumenter.InstrumentExpressionStatement(node, boundStatement);
		}
		return boundStatement;
	}

	private BoundExpression VisitUnusedExpression(BoundExpression expression)
	{
		if (expression.HasErrors)
		{
			return expression;
		}
		switch (expression.Kind)
		{
		case BoundKind.AssignmentOperator:
			return VisitAssignmentOperator((BoundAssignmentOperator)expression, used: false);
		case BoundKind.CompoundAssignmentOperator:
			return VisitCompoundAssignmentOperator((BoundCompoundAssignmentOperator)expression, used: false);
		case BoundKind.Call:
			if (_allowOmissionOfConditionalCalls)
			{
				BoundCall boundCall = (BoundCall)expression;
				if (boundCall.Method.CallsAreOmitted(boundCall.SyntaxTree))
				{
					return null;
				}
			}
			break;
		case BoundKind.DynamicInvocation:
			return VisitDynamicInvocation((BoundDynamicInvocation)expression, resultDiscarded: true);
		case BoundKind.ConditionalAccess:
			return RewriteConditionalAccess((BoundConditionalAccess)expression, used: false);
		}
		return VisitExpression(expression);
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		BoundExpression rewrittenReceiver = VisitExpression(node.ReceiverOpt);
		return MakeFieldAccess(node.Syntax, rewrittenReceiver, node.FieldSymbol, node.ConstantValue, node.ResultKind, node.Type, node);
	}

	private BoundExpression MakeFieldAccess(SyntaxNode syntax, BoundExpression rewrittenReceiver, FieldSymbol fieldSymbol, ConstantValue constantValueOpt, LookupResultKind resultKind, TypeSymbol type, BoundFieldAccess oldNodeOpt = null)
	{
		if (fieldSymbol.IsTupleField)
		{
			return MakeTupleFieldAccess(syntax, fieldSymbol, rewrittenReceiver, constantValueOpt, resultKind);
		}
		BoundExpression boundExpression = ((oldNodeOpt != null) ? oldNodeOpt.Update(rewrittenReceiver, fieldSymbol, constantValueOpt, resultKind, type) : new BoundFieldAccess(syntax, rewrittenReceiver, fieldSymbol, constantValueOpt, resultKind, type));
		if (fieldSymbol.IsFixed)
		{
			boundExpression = new BoundAddressOfOperator(syntax, boundExpression, type);
		}
		return boundExpression;
	}

	private BoundExpression MakeTupleFieldAccess(SyntaxNode syntax, FieldSymbol tupleField, BoundExpression rewrittenReceiver, ConstantValue constantValueOpt, LookupResultKind resultKind)
	{
		NamedTypeSymbol tupleUnderlyingType = tupleField.ContainingType.TupleUnderlyingType;
		FieldSymbol tupleUnderlyingField = tupleField.TupleUnderlyingField;
		if ((object)tupleUnderlyingField == null)
		{
			return _factory.BadExpression(tupleField.Type);
		}
		if (rewrittenReceiver.Kind == BoundKind.DefaultExpression)
		{
			return new BoundDefaultExpression(syntax, tupleField.Type);
		}
		if (tupleUnderlyingField.ContainingType != tupleUnderlyingType)
		{
			WellKnownMember tupleTypeMember = TupleTypeSymbol.GetTupleTypeMember(8, 8);
			FieldSymbol fieldSymbol = (FieldSymbol)TupleTypeSymbol.GetWellKnownMemberInType(tupleUnderlyingType.OriginalDefinition, tupleTypeMember, _diagnostics, syntax);
			if ((object)fieldSymbol == null)
			{
				return _factory.BadExpression(tupleField.Type);
			}
			do
			{
				FieldSymbol f = fieldSymbol.AsMember(tupleUnderlyingType);
				rewrittenReceiver = _factory.Field(rewrittenReceiver, f);
				tupleUnderlyingType = tupleUnderlyingType.TypeArgumentsNoUseSiteDiagnostics[7].TupleUnderlyingType;
			}
			while (tupleUnderlyingField.ContainingType != tupleUnderlyingType);
		}
		return _factory.Field(rewrittenReceiver, tupleUnderlyingField);
	}

	private BoundExpression MakeTupleFieldAccessAndReportUseSiteDiagnostics(BoundExpression tuple, SyntaxNode syntax, FieldSymbol field)
	{
		field = field.CorrespondingTupleField ?? field;
		DiagnosticInfo useSiteDiagnostic = field.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null && useSiteDiagnostic.Severity == DiagnosticSeverity.Error)
		{
			Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, _diagnostics, syntax.Location);
		}
		return MakeTupleFieldAccess(syntax, field, tuple, null, LookupResultKind.Empty);
	}

	public override BoundNode VisitFixedStatement(BoundFixedStatement node)
	{
		ImmutableArray<BoundLocalDeclaration> localDeclarations = node.Declarations.LocalDeclarations;
		int length = localDeclarations.Length;
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance(node.Locals.Length);
		instance.AddRange(node.Locals);
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance(length + 1 + 1);
		BoundStatement[] array = new BoundStatement[length];
		for (int i = 0; i < length; i++)
		{
			BoundLocalDeclaration localDecl = localDeclarations[i];
			instance2.Add(InitializeFixedStatementLocal(localDecl, _factory, out var pinnedTemp));
			instance.Add(pinnedTemp);
			if (pinnedTemp.RefKind == RefKind.None)
			{
				array[i] = _factory.Assignment(_factory.Local(pinnedTemp), _factory.Null(pinnedTemp.Type));
			}
			else
			{
				array[i] = _factory.Assignment(_factory.Local(pinnedTemp), new BoundPointerIndirectionOperator(_factory.Syntax, _factory.Default(new PointerTypeSymbol(pinnedTemp.Type)), pinnedTemp.Type), isRef: true);
			}
		}
		BoundStatement boundStatement = VisitStatement(node.Body);
		instance2.Add(boundStatement);
		instance2.Add(_factory.HiddenSequencePoint());
		if (IsInTryBlock(node) || HasGotoOut(boundStatement))
		{
			return _factory.Block(instance.ToImmutableAndFree(), new BoundTryStatement(_factory.Syntax, _factory.Block(instance2.ToImmutableAndFree()), ImmutableArray<BoundCatchBlock>.Empty, _factory.Block(array)));
		}
		instance2.AddRange(array);
		return _factory.Block(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree());
	}

	private static bool IsInTryBlock(BoundFixedStatement boundFixed)
	{
		SyntaxNode parent = boundFixed.Syntax.Parent;
		while (parent != null)
		{
			switch (parent.Kind())
			{
			case SyntaxKind.TryStatement:
				return true;
			case SyntaxKind.UsingStatement:
				return true;
			case SyntaxKind.ForEachStatement:
			case SyntaxKind.ForEachVariableStatement:
				return true;
			case SyntaxKind.AnonymousMethodExpression:
			case SyntaxKind.SimpleLambdaExpression:
			case SyntaxKind.ParenthesizedLambdaExpression:
				return false;
			case SyntaxKind.CatchClause:
				if (((TryStatementSyntax)parent.Parent).Finally != null)
				{
					return true;
				}
				goto case SyntaxKind.FinallyClause;
			case SyntaxKind.FinallyClause:
				parent = parent.Parent;
				parent = parent.Parent;
				continue;
			}
			if (parent is MemberDeclarationSyntax)
			{
				return false;
			}
			parent = parent.Parent;
		}
		return false;
	}

	private bool HasGotoOut(BoundNode node)
	{
		if (_lazyUnmatchedLabelCache == null)
		{
			_lazyUnmatchedLabelCache = new Dictionary<BoundNode, HashSet<LabelSymbol>>();
		}
		HashSet<LabelSymbol> hashSet = UnmatchedGotoFinder.Find(node, _lazyUnmatchedLabelCache, RecursionDepth);
		_lazyUnmatchedLabelCache.Add(node, hashSet);
		if (hashSet != null)
		{
			return hashSet.Count > 0;
		}
		return false;
	}

	public override BoundNode VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	private BoundStatement InitializeFixedStatementLocal(BoundLocalDeclaration localDecl, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		BoundExpression initializerOpt = localDecl.InitializerOpt;
		LocalSymbol localSymbol = localDecl.LocalSymbol;
		BoundFixedLocalCollectionInitializer boundFixedLocalCollectionInitializer = (BoundFixedLocalCollectionInitializer)initializerOpt;
		if ((object)boundFixedLocalCollectionInitializer.GetPinnableOpt != null)
		{
			return InitializeFixedStatementGetPinnable(localDecl, localSymbol, boundFixedLocalCollectionInitializer, factory, out pinnedTemp);
		}
		if (boundFixedLocalCollectionInitializer.Expression.Type.SpecialType == SpecialType.System_String)
		{
			return InitializeFixedStatementStringLocal(localDecl, localSymbol, boundFixedLocalCollectionInitializer, factory, out pinnedTemp);
		}
		if (boundFixedLocalCollectionInitializer.Expression.Type.IsArray())
		{
			return InitializeFixedStatementArrayLocal(localDecl, localSymbol, boundFixedLocalCollectionInitializer, factory, out pinnedTemp);
		}
		return InitializeFixedStatementRegularLocal(localDecl, localSymbol, boundFixedLocalCollectionInitializer, factory, out pinnedTemp);
	}

	private BoundStatement InitializeFixedStatementRegularLocal(BoundLocalDeclaration localDecl, LocalSymbol localSymbol, BoundFixedLocalCollectionInitializer fixedInitializer, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		TypeSymbol type = localSymbol.Type;
		BoundExpression boundExpression = VisitExpression(fixedInitializer.Expression);
		TypeSymbol pointedAtType = ((PointerTypeSymbol)boundExpression.Type).PointedAtType;
		boundExpression = ((BoundAddressOfOperator)boundExpression).Operand;
		VariableDeclaratorSyntax syntax = fixedInitializer.Syntax.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
		pinnedTemp = factory.SynthesizedLocal(pointedAtType, syntax, isPinned: true, RefKind.In, SynthesizedLocalKind.FixedReference);
		BoundStatement boundStatement = factory.Assignment(factory.Local(pinnedTemp), boundExpression, isRef: true);
		BoundAddressOfOperator arg = new BoundAddressOfOperator(factory.Syntax, factory.Local(pinnedTemp), fixedInitializer.ElementPointerType);
		BoundExpression right = factory.Convert(type, arg, fixedInitializer.ElementPointerTypeConversion);
		BoundStatement boundStatement2 = InstrumentLocalDeclarationIfNecessary(localDecl, localSymbol, factory.Assignment(factory.Local(localSymbol), right));
		return factory.Block(boundStatement, boundStatement2);
	}

	private BoundStatement InitializeFixedStatementGetPinnable(BoundLocalDeclaration localDecl, LocalSymbol localSymbol, BoundFixedLocalCollectionInitializer fixedInitializer, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		TypeSymbol type = localSymbol.Type;
		BoundExpression boundExpression = VisitExpression(fixedInitializer.Expression);
		TypeSymbol type2 = boundExpression.Type;
		SyntaxNode syntax = boundExpression.Syntax;
		MethodSymbol getPinnableOpt = fixedInitializer.GetPinnableOpt;
		VariableDeclaratorSyntax syntax2 = fixedInitializer.Syntax.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
		pinnedTemp = factory.SynthesizedLocal(getPinnableOpt.ReturnType, syntax2, isPinned: true, RefKind.In, SynthesizedLocalKind.FixedReference);
		int id = 0;
		bool flag = !type2.IsValueType;
		BoundExpression boundExpression2;
		if (flag)
		{
			id = _currentConditionalAccessID++;
			boundExpression2 = new BoundConditionalReceiver(syntax, id, type2);
		}
		else
		{
			boundExpression2 = boundExpression;
		}
		BoundAssignmentOperator item = factory.AssignmentExpression(right: getPinnableOpt.IsStatic ? factory.Call(null, getPinnableOpt, boundExpression2) : factory.Call(boundExpression2, getPinnableOpt), left: factory.Local(pinnedTemp), isRef: true);
		BoundAddressOfOperator arg = new BoundAddressOfOperator(factory.Syntax, factory.Local(pinnedTemp), fixedInitializer.ElementPointerType);
		BoundExpression boundExpression3 = factory.Sequence(result: factory.Convert(type, arg, fixedInitializer.ElementPointerTypeConversion), locals: ImmutableArray<LocalSymbol>.Empty, sideEffects: ImmutableArray.Create((BoundExpression)item));
		if (flag)
		{
			boundExpression3 = new BoundLoweredConditionalAccess(syntax, boundExpression, null, boundExpression3, null, id, type);
		}
		return InstrumentLocalDeclarationIfNecessary(localDecl, localSymbol, factory.Assignment(factory.Local(localSymbol), boundExpression3));
	}

	private BoundStatement InitializeFixedStatementStringLocal(BoundLocalDeclaration localDecl, LocalSymbol localSymbol, BoundFixedLocalCollectionInitializer fixedInitializer, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		TypeSymbol type = localSymbol.Type;
		BoundExpression boundExpression = VisitExpression(fixedInitializer.Expression);
		TypeSymbol type2 = boundExpression.Type;
		VariableDeclaratorSyntax syntax = fixedInitializer.Syntax.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
		pinnedTemp = factory.SynthesizedLocal(type2, syntax, isPinned: true, RefKind.None, SynthesizedLocalKind.FixedReference);
		BoundStatement boundStatement = factory.Assignment(factory.Local(pinnedTemp), boundExpression);
		BoundExpression arg = factory.Convert(fixedInitializer.ElementPointerType, factory.Local(pinnedTemp), Conversion.PinnedObjectToPointer);
		BoundExpression right = factory.Convert(type, arg, fixedInitializer.ElementPointerTypeConversion);
		BoundStatement boundStatement2 = InstrumentLocalDeclarationIfNecessary(localDecl, localSymbol, factory.Assignment(factory.Local(localSymbol), right));
		BoundExpression condition = MakeNullCheck(factory.Syntax, factory.Local(localSymbol), BinaryOperatorKind.NotEqual);
		BoundExpression right2 = factory.Binary(right: (!TryGetWellKnownTypeMember<MethodSymbol>(fixedInitializer.Syntax, WellKnownMember.System_Runtime_CompilerServices_RuntimeHelpers__get_OffsetToStringData, out var symbol)) ? ((BoundExpression)new BoundBadExpression(fixedInitializer.Syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)factory.Call(null, symbol)), kind: BinaryOperatorKind.PointerAndIntAddition, type: type, left: factory.Local(localSymbol));
		BoundStatement boundStatement3 = factory.If(condition, factory.Assignment(factory.Local(localSymbol), right2));
		return factory.Block(boundStatement, boundStatement2, boundStatement3);
	}

	private BoundStatement InitializeFixedStatementArrayLocal(BoundLocalDeclaration localDecl, LocalSymbol localSymbol, BoundFixedLocalCollectionInitializer fixedInitializer, SyntheticBoundNodeFactory factory, out LocalSymbol pinnedTemp)
	{
		TypeSymbol type = localSymbol.Type;
		BoundExpression boundExpression = VisitExpression(fixedInitializer.Expression);
		TypeSymbol type2 = boundExpression.Type;
		pinnedTemp = factory.SynthesizedLocal(type2, null, isPinned: true);
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)pinnedTemp.Type;
		TypeSymbol elementType = arrayTypeSymbol.ElementType;
		BoundExpression left = MakeNullCheck(rewrittenExpr: factory.AssignmentExpression(factory.Local(pinnedTemp), boundExpression), syntax: factory.Syntax, operatorKind: BinaryOperatorKind.NotEqual);
		BoundExpression right = factory.Binary(left: arrayTypeSymbol.IsSZArray ? factory.ArrayLength(factory.Local(pinnedTemp)) : ((!TryGetWellKnownTypeMember<MethodSymbol>(fixedInitializer.Syntax, WellKnownMember.System_Array__get_Length, out var symbol)) ? ((BoundExpression)new BoundBadExpression(fixedInitializer.Syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)factory.Local(pinnedTemp)), ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)factory.Call(factory.Local(pinnedTemp), symbol))), kind: BinaryOperatorKind.IntNotEqual, type: factory.SpecialType(SpecialType.System_Boolean), right: factory.Literal(0));
		BoundExpression condition = factory.Binary(BinaryOperatorKind.LogicalBoolAnd, factory.SpecialType(SpecialType.System_Boolean), left, right);
		BoundExpression operand = factory.ArrayAccessFirstElement(factory.Local(pinnedTemp));
		BoundExpression arg = new BoundAddressOfOperator(factory.Syntax, operand, new PointerTypeSymbol(elementType));
		BoundExpression right2 = factory.Convert(type, arg, fixedInitializer.ElementPointerTypeConversion);
		BoundExpression consequence = factory.AssignmentExpression(factory.Local(localSymbol), right2);
		BoundExpression alternative = factory.AssignmentExpression(factory.Local(localSymbol), factory.Null(type));
		BoundStatement rewrittenLocalDeclaration = factory.ExpressionStatement(new BoundConditionalOperator(factory.Syntax, isRef: false, condition, consequence, alternative, null, type));
		return InstrumentLocalDeclarationIfNecessary(localDecl, localSymbol, rewrittenLocalDeclaration);
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		if (node.HasErrors)
		{
			return node;
		}
		TypeSymbol type = GetUnconvertedCollectionExpression(node).Type;
		if (type.Kind == SymbolKind.ArrayType)
		{
			if (((ArrayTypeSymbol)type).IsSZArray)
			{
				return RewriteSingleDimensionalArrayForEachStatement(node);
			}
			return RewriteMultiDimensionalArrayForEachStatement(node);
		}
		if (CanRewriteForEachAsFor(node.Syntax, type, out var indexerGet, out var lengthGet))
		{
			return RewriteForEachStatementAsFor(node, indexerGet, lengthGet);
		}
		return RewriteEnumeratorForEachStatement(node);
	}

	private bool CanRewriteForEachAsFor(SyntaxNode forEachSyntax, TypeSymbol nodeExpressionType, out MethodSymbol indexerGet, out MethodSymbol lengthGet)
	{
		lengthGet = (indexerGet = null);
		TypeSymbol originalDefinition = nodeExpressionType.OriginalDefinition;
		if (originalDefinition.SpecialType == SpecialType.System_String)
		{
			lengthGet = UnsafeGetSpecialTypeMethod(forEachSyntax, SpecialMember.System_String__Length);
			indexerGet = UnsafeGetSpecialTypeMethod(forEachSyntax, SpecialMember.System_String__Chars);
		}
		else if ((object)originalDefinition == _compilation.GetWellKnownType(WellKnownType.System_Span_T))
		{
			NamedTypeSymbol newOwner = (NamedTypeSymbol)nodeExpressionType;
			lengthGet = (MethodSymbol)(_factory.WellKnownMember(WellKnownMember.System_Span_T__get_Length, isOptional: true)?.SymbolAsMember(newOwner));
			indexerGet = (MethodSymbol)(_factory.WellKnownMember(WellKnownMember.System_Span_T__get_Item, isOptional: true)?.SymbolAsMember(newOwner));
		}
		else if ((object)originalDefinition == _compilation.GetWellKnownType(WellKnownType.System_ReadOnlySpan_T))
		{
			NamedTypeSymbol newOwner2 = (NamedTypeSymbol)nodeExpressionType;
			lengthGet = (MethodSymbol)(_factory.WellKnownMember(WellKnownMember.System_ReadOnlySpan_T__get_Length, isOptional: true)?.SymbolAsMember(newOwner2));
			indexerGet = (MethodSymbol)(_factory.WellKnownMember(WellKnownMember.System_ReadOnlySpan_T__get_Item, isOptional: true)?.SymbolAsMember(newOwner2));
		}
		if ((object)lengthGet != null)
		{
			return (object)indexerGet != null;
		}
		return false;
	}

	private BoundStatement RewriteEnumeratorForEachStatement(BoundForEachStatement node)
	{
		CommonForEachStatementSyntax commonForEachStatementSyntax = (CommonForEachStatementSyntax)node.Syntax;
		ForEachEnumeratorInfo enumeratorInfoOpt = node.EnumeratorInfoOpt;
		BoundExpression unconvertedCollectionExpression = GetUnconvertedCollectionExpression(node);
		BoundExpression receiver = (BoundExpression)Visit(unconvertedCollectionExpression);
		BoundStatement rewrittenBody = (BoundStatement)Visit(node.Body);
		TypeSymbol returnType = enumeratorInfoOpt.GetEnumeratorMethod.ReturnType;
		TypeSymbol elementType = enumeratorInfoOpt.ElementType;
		LocalSymbol localSymbol = _factory.SynthesizedLocal(returnType, commonForEachStatementSyntax, isPinned: false, RefKind.None, SynthesizedLocalKind.ForEachEnumerator);
		BoundLocal boundLocal = MakeBoundLocal(commonForEachStatementSyntax, localSymbol, returnType);
		BoundExpression rewrittenInitialValue = SynthesizeCall(commonForEachStatementSyntax, receiver, enumeratorInfoOpt.GetEnumeratorMethod, enumeratorInfoOpt.CollectionConversion, enumeratorInfoOpt.CollectionType);
		BoundStatement collectionVarDecl = MakeLocalDeclaration(commonForEachStatementSyntax, localSymbol, rewrittenInitialValue);
		InstrumentForEachStatementCollectionVarDeclaration(node, ref collectionVarDecl);
		BoundExpression iterationVarValue = MakeConversionNode(commonForEachStatementSyntax, MakeConversionNode(commonForEachStatementSyntax, BoundCall.Synthesized(commonForEachStatementSyntax, boundLocal, enumeratorInfoOpt.CurrentPropertyGetter), enumeratorInfoOpt.CurrentConversion, elementType, node.Checked), node.ElementConversion, node.IterationVariableType.Type, node.Checked);
		ImmutableArray<LocalSymbol> iterationVariables = node.IterationVariables;
		BoundStatement iterationVarDecl = LocalOrDeconstructionDeclaration(node, iterationVariables, iterationVarValue);
		InstrumentForEachStatementIterationVarDeclaration(node, ref iterationVarDecl);
		BoundBlock rewrittenBody2 = CreateBlockDeclaringIterationVariables(iterationVariables, iterationVarDecl, rewrittenBody, commonForEachStatementSyntax);
		BoundStatement boundStatement = RewriteWhileStatement(node, BoundCall.Synthesized(commonForEachStatementSyntax, boundLocal, enumeratorInfoOpt.MoveNextMethod), rewrittenBody2, node.BreakLabel, node.ContinueLabel, hasErrors: false);
		BoundStatement result;
		if (enumeratorInfoOpt.NeedsDisposeMethod && Binder.TryGetSpecialTypeMember<MethodSymbol>(_compilation, SpecialMember.System_IDisposable__Dispose, commonForEachStatementSyntax, _diagnostics, out var symbol))
		{
			Binder.ReportDiagnosticsIfObsolete(_diagnostics, symbol, commonForEachStatementSyntax, hasBaseReceiver: false, _factory.CurrentFunction, _factory.CurrentType, enumeratorInfoOpt.Location);
			NamedTypeSymbol containingType = symbol.ContainingType;
			TypeConversions typeConversions = new TypeConversions(_factory.CurrentFunction.ContainingAssembly.CorLibrary);
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			bool isImplicit = typeConversions.ClassifyImplicitConversionFromType(returnType, containingType, ref useSiteDiagnostics).IsImplicit;
			_diagnostics.Add(commonForEachStatementSyntax, useSiteDiagnostics);
			BoundBlock finallyBlockOpt;
			if (isImplicit)
			{
				Conversion receiverConversion = (returnType.IsStructType() ? Conversion.Boxing : Conversion.ImplicitReference);
				BoundStatement boundStatement2 = new BoundExpressionStatement(commonForEachStatementSyntax, SynthesizeCall(commonForEachStatementSyntax, boundLocal, symbol, receiverConversion, containingType));
				finallyBlockOpt = new BoundBlock(statements: ImmutableArray.Create((!returnType.IsValueType) ? RewriteIfStatement(commonForEachStatementSyntax, new BoundBinaryOperator(commonForEachStatementSyntax, BinaryOperatorKind.NotEqual, MakeConversionNode(commonForEachStatementSyntax, boundLocal, enumeratorInfoOpt.EnumeratorConversion, _compilation.GetSpecialType(SpecialType.System_Object), @checked: false), MakeLiteral(commonForEachStatementSyntax, ConstantValue.Null, null), null, null, LookupResultKind.Viable, _compilation.GetSpecialType(SpecialType.System_Boolean)), boundStatement2, null, hasErrors: false) : boundStatement2), syntax: commonForEachStatementSyntax, locals: ImmutableArray<LocalSymbol>.Empty);
			}
			else
			{
				LocalSymbol localSymbol2 = _factory.SynthesizedLocal(containingType);
				BoundLocal boundLocal2 = MakeBoundLocal(commonForEachStatementSyntax, localSymbol2, containingType);
				BoundTypeExpression targetType = new BoundTypeExpression(commonForEachStatementSyntax, null, containingType);
				BoundExpression rewrittenInitialValue2 = new BoundAsOperator(commonForEachStatementSyntax, boundLocal, targetType, Conversion.ExplicitReference, containingType);
				BoundStatement item = MakeLocalDeclaration(commonForEachStatementSyntax, localSymbol2, rewrittenInitialValue2);
				BoundStatement item2 = RewriteIfStatement(commonForEachStatementSyntax, new BoundBinaryOperator(commonForEachStatementSyntax, BinaryOperatorKind.NotEqual, boundLocal2, MakeLiteral(commonForEachStatementSyntax, ConstantValue.Null, null), null, null, LookupResultKind.Viable, _compilation.GetSpecialType(SpecialType.System_Boolean)), new BoundExpressionStatement(commonForEachStatementSyntax, BoundCall.Synthesized(commonForEachStatementSyntax, boundLocal2, symbol)), null, hasErrors: false);
				finallyBlockOpt = new BoundBlock(commonForEachStatementSyntax, ImmutableArray.Create(localSymbol2), ImmutableArray.Create(item, item2));
			}
			BoundStatement item3 = new BoundTryStatement(commonForEachStatementSyntax, new BoundBlock(commonForEachStatementSyntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundStatement)), ImmutableArray<BoundCatchBlock>.Empty, finallyBlockOpt);
			result = new BoundBlock(commonForEachStatementSyntax, ImmutableArray.Create(localSymbol), ImmutableArray.Create(collectionVarDecl, item3));
		}
		else
		{
			result = new BoundBlock(commonForEachStatementSyntax, ImmutableArray.Create(localSymbol), ImmutableArray.Create(collectionVarDecl, boundStatement));
		}
		InstrumentForEachStatement(node, ref result);
		return result;
	}

	private BoundExpression SynthesizeCall(CSharpSyntaxNode syntax, BoundExpression receiver, MethodSymbol method, Conversion receiverConversion, TypeSymbol convertedReceiverType)
	{
		if (!receiver.Type.IsReferenceType && method.ContainingType.IsInterface)
		{
			return BoundCall.Synthesized(syntax, receiver, method);
		}
		return BoundCall.Synthesized(syntax, MakeConversionNode(syntax, receiver, receiverConversion, convertedReceiverType, @checked: false), method);
	}

	private BoundStatement RewriteForEachStatementAsFor(BoundForEachStatement node, MethodSymbol indexerGet, MethodSymbol lengthGet)
	{
		CommonForEachStatementSyntax commonForEachStatementSyntax = (CommonForEachStatementSyntax)node.Syntax;
		BoundExpression unconvertedCollectionExpression = GetUnconvertedCollectionExpression(node);
		NamedTypeSymbol type = (NamedTypeSymbol)unconvertedCollectionExpression.Type;
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Int32);
		TypeSymbol specialType2 = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundExpression rewrittenInitialValue = (BoundExpression)Visit(unconvertedCollectionExpression);
		BoundStatement rewrittenBody = (BoundStatement)Visit(node.Body);
		LocalSymbol localSymbol = _factory.SynthesizedLocal(type, commonForEachStatementSyntax, isPinned: false, RefKind.None, SynthesizedLocalKind.ForEachArray);
		BoundStatement collectionVarDecl = MakeLocalDeclaration(commonForEachStatementSyntax, localSymbol, rewrittenInitialValue);
		InstrumentForEachStatementCollectionVarDeclaration(node, ref collectionVarDecl);
		BoundLocal receiverOpt = MakeBoundLocal(commonForEachStatementSyntax, localSymbol, type);
		LocalSymbol localSymbol2 = _factory.SynthesizedLocal(specialType, commonForEachStatementSyntax, isPinned: false, RefKind.None, SynthesizedLocalKind.ForEachArrayIndex);
		BoundLocal boundLocal = MakeBoundLocal(commonForEachStatementSyntax, localSymbol2, specialType);
		BoundStatement item = MakeLocalDeclaration(commonForEachStatementSyntax, localSymbol2, MakeLiteral(commonForEachStatementSyntax, ConstantValue.Default(SpecialType.System_Int32), specialType));
		BoundExpression iterationVarValue = MakeConversionNode(commonForEachStatementSyntax, BoundCall.Synthesized(commonForEachStatementSyntax, receiverOpt, indexerGet, boundLocal), node.ElementConversion, node.IterationVariableType.Type, node.Checked);
		ImmutableArray<LocalSymbol> iterationVariables = node.IterationVariables;
		BoundStatement iterationVarDecl = LocalOrDeconstructionDeclaration(node, iterationVariables, iterationVarValue);
		InstrumentForEachStatementIterationVarDeclaration(node, ref iterationVarDecl);
		BoundStatement rewrittenInitializer = new BoundStatementList(commonForEachStatementSyntax, ImmutableArray.Create(collectionVarDecl, item));
		BoundExpression right = BoundCall.Synthesized(commonForEachStatementSyntax, receiverOpt, lengthGet);
		BoundExpression rewrittenCondition = new BoundBinaryOperator(commonForEachStatementSyntax, BinaryOperatorKind.IntLessThan, boundLocal, right, null, null, LookupResultKind.Viable, specialType2);
		BoundStatement rewrittenIncrement = MakePositionIncrement(commonForEachStatementSyntax, boundLocal, specialType);
		BoundStatement rewrittenBody2 = CreateBlockDeclaringIterationVariables(iterationVariables, iterationVarDecl, rewrittenBody, commonForEachStatementSyntax);
		BoundStatement result = RewriteForStatementWithoutInnerLocals(node, ImmutableArray.Create(localSymbol, localSymbol2), rewrittenInitializer, rewrittenCondition, rewrittenIncrement, rewrittenBody2, node.BreakLabel, node.ContinueLabel, node.HasErrors);
		InstrumentForEachStatement(node, ref result);
		return result;
	}

	private BoundStatement LocalOrDeconstructionDeclaration(BoundForEachStatement forEachBound, ImmutableArray<LocalSymbol> iterationVariables, BoundExpression iterationVarValue)
	{
		CommonForEachStatementSyntax syntax = (CommonForEachStatementSyntax)forEachBound.Syntax;
		BoundForEachDeconstructStep deconstructionOpt = forEachBound.DeconstructionOpt;
		BoundStatement result;
		if (deconstructionOpt == null)
		{
			result = MakeLocalDeclaration(syntax, iterationVariables[0], iterationVarValue);
		}
		else
		{
			BoundDeconstructionAssignmentOperator deconstructionAssignment = deconstructionOpt.DeconstructionAssignment;
			AddPlaceholderReplacement(deconstructionOpt.TargetPlaceholder, iterationVarValue);
			BoundExpression expression = VisitExpression(deconstructionAssignment);
			result = new BoundExpressionStatement(deconstructionAssignment.Syntax, expression);
			RemovePlaceholderReplacement(deconstructionOpt.TargetPlaceholder);
		}
		return result;
	}

	private static BoundBlock CreateBlockDeclaringIterationVariables(ImmutableArray<LocalSymbol> iterationVariables, BoundStatement iteratorVariableInitialization, BoundStatement rewrittenBody, CommonForEachStatementSyntax forEachSyntax)
	{
		return new BoundBlock(forEachSyntax, iterationVariables, ImmutableArray.Create(iteratorVariableInitialization, rewrittenBody));
	}

	private BoundStatement RewriteSingleDimensionalArrayForEachStatement(BoundForEachStatement node)
	{
		CommonForEachStatementSyntax commonForEachStatementSyntax = (CommonForEachStatementSyntax)node.Syntax;
		BoundExpression unconvertedCollectionExpression = GetUnconvertedCollectionExpression(node);
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)unconvertedCollectionExpression.Type;
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Int32);
		TypeSymbol specialType2 = _compilation.GetSpecialType(SpecialType.System_Boolean);
		BoundExpression rewrittenInitialValue = (BoundExpression)Visit(unconvertedCollectionExpression);
		BoundStatement rewrittenBody = (BoundStatement)Visit(node.Body);
		LocalSymbol localSymbol = _factory.SynthesizedLocal(arrayTypeSymbol, commonForEachStatementSyntax, isPinned: false, RefKind.None, SynthesizedLocalKind.ForEachArray);
		BoundStatement collectionVarDecl = MakeLocalDeclaration(commonForEachStatementSyntax, localSymbol, rewrittenInitialValue);
		InstrumentForEachStatementCollectionVarDeclaration(node, ref collectionVarDecl);
		BoundLocal expression = MakeBoundLocal(commonForEachStatementSyntax, localSymbol, arrayTypeSymbol);
		LocalSymbol localSymbol2 = _factory.SynthesizedLocal(specialType, commonForEachStatementSyntax, isPinned: false, RefKind.None, SynthesizedLocalKind.ForEachArrayIndex);
		BoundLocal boundLocal = MakeBoundLocal(commonForEachStatementSyntax, localSymbol2, specialType);
		BoundStatement item = MakeLocalDeclaration(commonForEachStatementSyntax, localSymbol2, MakeLiteral(commonForEachStatementSyntax, ConstantValue.Default(SpecialType.System_Int32), specialType));
		BoundExpression iterationVarValue = MakeConversionNode(commonForEachStatementSyntax, new BoundArrayAccess(commonForEachStatementSyntax, expression, ImmutableArray.Create((BoundExpression)boundLocal), arrayTypeSymbol.ElementType), node.ElementConversion, node.IterationVariableType.Type, node.Checked);
		ImmutableArray<LocalSymbol> iterationVariables = node.IterationVariables;
		BoundStatement iterationVarDecl = LocalOrDeconstructionDeclaration(node, iterationVariables, iterationVarValue);
		InstrumentForEachStatementIterationVarDeclaration(node, ref iterationVarDecl);
		BoundStatement rewrittenInitializer = new BoundStatementList(commonForEachStatementSyntax, ImmutableArray.Create(collectionVarDecl, item));
		BoundExpression right = new BoundArrayLength(commonForEachStatementSyntax, expression, specialType);
		BoundExpression rewrittenCondition = new BoundBinaryOperator(commonForEachStatementSyntax, BinaryOperatorKind.IntLessThan, boundLocal, right, null, null, LookupResultKind.Viable, specialType2);
		BoundStatement rewrittenIncrement = MakePositionIncrement(commonForEachStatementSyntax, boundLocal, specialType);
		BoundStatement rewrittenBody2 = CreateBlockDeclaringIterationVariables(iterationVariables, iterationVarDecl, rewrittenBody, commonForEachStatementSyntax);
		BoundStatement result = RewriteForStatementWithoutInnerLocals(node, ImmutableArray.Create(localSymbol, localSymbol2), rewrittenInitializer, rewrittenCondition, rewrittenIncrement, rewrittenBody2, node.BreakLabel, node.ContinueLabel, node.HasErrors);
		InstrumentForEachStatement(node, ref result);
		return result;
	}

	private BoundStatement RewriteMultiDimensionalArrayForEachStatement(BoundForEachStatement node)
	{
		CommonForEachStatementSyntax commonForEachStatementSyntax = (CommonForEachStatementSyntax)node.Syntax;
		BoundExpression unconvertedCollectionExpression = GetUnconvertedCollectionExpression(node);
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)unconvertedCollectionExpression.Type;
		int rank = arrayTypeSymbol.Rank;
		TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Int32);
		TypeSymbol specialType2 = _compilation.GetSpecialType(SpecialType.System_Boolean);
		MethodSymbol method = UnsafeGetSpecialTypeMethod(commonForEachStatementSyntax, SpecialMember.System_Array__GetLowerBound);
		MethodSymbol method2 = UnsafeGetSpecialTypeMethod(commonForEachStatementSyntax, SpecialMember.System_Array__GetUpperBound);
		BoundExpression rewrittenInitialValue = (BoundExpression)Visit(unconvertedCollectionExpression);
		BoundStatement rewrittenBody = (BoundStatement)Visit(node.Body);
		LocalSymbol localSymbol = _factory.SynthesizedLocal(arrayTypeSymbol, commonForEachStatementSyntax, isPinned: false, RefKind.None, SynthesizedLocalKind.ForEachArray);
		BoundLocal boundLocal = MakeBoundLocal(commonForEachStatementSyntax, localSymbol, arrayTypeSymbol);
		BoundStatement collectionVarDecl = MakeLocalDeclaration(commonForEachStatementSyntax, localSymbol, rewrittenInitialValue);
		InstrumentForEachStatementCollectionVarDeclaration(node, ref collectionVarDecl);
		LocalSymbol[] array = new LocalSymbol[rank];
		BoundLocal[] array2 = new BoundLocal[rank];
		BoundStatement[] array3 = new BoundStatement[rank];
		for (int i = 0; i < rank; i++)
		{
			array[i] = _factory.SynthesizedLocal(specialType, commonForEachStatementSyntax, isPinned: false, RefKind.None, SynthesizedLocalKind.ForEachArrayLimit);
			array2[i] = MakeBoundLocal(commonForEachStatementSyntax, array[i], specialType);
			ImmutableArray<BoundExpression> arguments = ImmutableArray.Create(MakeLiteral(commonForEachStatementSyntax, ConstantValue.Create(i, ConstantValueTypeDiscriminator.Int32), specialType));
			BoundExpression rewrittenInitialValue2 = BoundCall.Synthesized(commonForEachStatementSyntax, boundLocal, method2, arguments);
			array3[i] = MakeLocalDeclaration(commonForEachStatementSyntax, array[i], rewrittenInitialValue2);
		}
		LocalSymbol[] array4 = new LocalSymbol[rank];
		BoundLocal[] array5 = new BoundLocal[rank];
		for (int j = 0; j < rank; j++)
		{
			array4[j] = _factory.SynthesizedLocal(specialType, commonForEachStatementSyntax, isPinned: false, RefKind.None, SynthesizedLocalKind.ForEachArrayIndex);
			array5[j] = MakeBoundLocal(commonForEachStatementSyntax, array4[j], specialType);
		}
		BoundExpression[] items = array5;
		BoundExpression iterationVarValue = MakeConversionNode(commonForEachStatementSyntax, new BoundArrayAccess(commonForEachStatementSyntax, boundLocal, ImmutableArray.Create(items), arrayTypeSymbol.ElementType), node.ElementConversion, node.IterationVariableType.Type, node.Checked);
		ImmutableArray<LocalSymbol> iterationVariables = node.IterationVariables;
		BoundStatement iterationVarDecl = LocalOrDeconstructionDeclaration(node, iterationVariables, iterationVarValue);
		InstrumentForEachStatementIterationVarDeclaration(node, ref iterationVarDecl);
		BoundStatement boundStatement = CreateBlockDeclaringIterationVariables(iterationVariables, iterationVarDecl, rewrittenBody, commonForEachStatementSyntax);
		BoundStatement boundStatement2 = null;
		for (int num = rank - 1; num >= 0; num--)
		{
			ImmutableArray<BoundExpression> arguments2 = ImmutableArray.Create(MakeLiteral(commonForEachStatementSyntax, ConstantValue.Create(num, ConstantValueTypeDiscriminator.Int32), specialType));
			BoundExpression rewrittenInitialValue3 = BoundCall.Synthesized(commonForEachStatementSyntax, boundLocal, method, arguments2);
			BoundStatement rewrittenInitializer = MakeLocalDeclaration(commonForEachStatementSyntax, array4[num], rewrittenInitialValue3);
			GeneratedLabelSymbol breakLabel = ((num == 0) ? node.BreakLabel : new GeneratedLabelSymbol("break"));
			BoundExpression rewrittenCondition = new BoundBinaryOperator(commonForEachStatementSyntax, BinaryOperatorKind.IntLessThanOrEqual, array5[num], array2[num], null, null, LookupResultKind.Viable, specialType2);
			BoundStatement rewrittenIncrement = MakePositionIncrement(commonForEachStatementSyntax, array5[num], specialType);
			BoundStatement rewrittenBody2;
			GeneratedLabelSymbol continueLabel;
			if (boundStatement2 == null)
			{
				rewrittenBody2 = boundStatement;
				continueLabel = node.ContinueLabel;
			}
			else
			{
				rewrittenBody2 = boundStatement2;
				continueLabel = new GeneratedLabelSymbol("continue");
			}
			boundStatement2 = RewriteForStatementWithoutInnerLocals(node, ImmutableArray.Create(array4[num]), rewrittenInitializer, rewrittenCondition, rewrittenIncrement, rewrittenBody2, breakLabel, continueLabel, node.HasErrors);
		}
		BoundStatement result = new BoundBlock(commonForEachStatementSyntax, ImmutableArray.Create(localSymbol).Concat(array.AsImmutableOrNull()), ImmutableArray.Create(collectionVarDecl).Concat(array3.AsImmutableOrNull()).Add(boundStatement2));
		InstrumentForEachStatement(node, ref result);
		return result;
	}

	private static BoundExpression GetUnconvertedCollectionExpression(BoundForEachStatement node)
	{
		BoundExpression expression = node.Expression;
		if (expression.Kind == BoundKind.Conversion)
		{
			return ((BoundConversion)expression).Operand;
		}
		return expression;
	}

	private static BoundLocal MakeBoundLocal(CSharpSyntaxNode syntax, LocalSymbol local, TypeSymbol type)
	{
		return new BoundLocal(syntax, local, null, type);
	}

	private BoundStatement MakeLocalDeclaration(CSharpSyntaxNode syntax, LocalSymbol local, BoundExpression rewrittenInitialValue)
	{
		return RewriteLocalDeclaration(null, syntax, local, rewrittenInitialValue);
	}

	private BoundStatement MakePositionIncrement(CSharpSyntaxNode syntax, BoundLocal boundPositionVar, TypeSymbol intType)
	{
		return new BoundSequencePoint(null, new BoundExpressionStatement(syntax, new BoundAssignmentOperator(syntax, boundPositionVar, new BoundBinaryOperator(syntax, BinaryOperatorKind.IntAddition, boundPositionVar, MakeLiteral(syntax, ConstantValue.Create(1), intType), null, null, LookupResultKind.Viable, intType), intType)));
	}

	private void InstrumentForEachStatementCollectionVarDeclaration(BoundForEachStatement original, ref BoundStatement collectionVarDecl)
	{
		if (Instrument)
		{
			collectionVarDecl = _instrumenter.InstrumentForEachStatementCollectionVarDeclaration(original, collectionVarDecl);
		}
	}

	private void InstrumentForEachStatementIterationVarDeclaration(BoundForEachStatement original, ref BoundStatement iterationVarDecl)
	{
		if (Instrument)
		{
			if (((CommonForEachStatementSyntax)original.Syntax) is ForEachVariableStatementSyntax)
			{
				iterationVarDecl = _instrumenter.InstrumentForEachStatementDeconstructionVariablesDeclaration(original, iterationVarDecl);
			}
			else
			{
				iterationVarDecl = _instrumenter.InstrumentForEachStatementIterationVarDeclaration(original, iterationVarDecl);
			}
		}
	}

	private void InstrumentForEachStatement(BoundForEachStatement original, ref BoundStatement result)
	{
		if (Instrument)
		{
			result = _instrumenter.InstrumentForEachStatement(original, result);
		}
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		BoundStatement rewrittenInitializer = (BoundStatement)Visit(node.Initializer);
		BoundExpression boundExpression = (BoundExpression)Visit(node.Condition);
		BoundStatement rewrittenIncrement = (BoundStatement)Visit(node.Increment);
		BoundStatement rewrittenBody = (BoundStatement)Visit(node.Body);
		if (boundExpression != null && Instrument)
		{
			boundExpression = _instrumenter.InstrumentForStatementCondition(node, boundExpression, _factory);
		}
		return RewriteForStatement(node, rewrittenInitializer, boundExpression, rewrittenIncrement, rewrittenBody);
	}

	private BoundStatement RewriteForStatementWithoutInnerLocals(BoundLoopStatement original, ImmutableArray<LocalSymbol> outerLocals, BoundStatement rewrittenInitializer, BoundExpression rewrittenCondition, BoundStatement rewrittenIncrement, BoundStatement rewrittenBody, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel, bool hasErrors)
	{
		SyntaxNode syntax = original.Syntax;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		if (rewrittenInitializer != null)
		{
			instance.Add(rewrittenInitializer);
		}
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("start");
		GeneratedLabelSymbol label2 = new GeneratedLabelSymbol("end");
		BoundStatement boundStatement = new BoundGotoStatement(syntax, label2);
		if (Instrument)
		{
			boundStatement = new BoundSequencePoint(null, boundStatement);
		}
		instance.Add(boundStatement);
		instance.Add(new BoundLabelStatement(syntax, label));
		instance.Add(rewrittenBody);
		instance.Add(new BoundLabelStatement(syntax, continueLabel));
		if (rewrittenIncrement != null)
		{
			instance.Add(rewrittenIncrement);
		}
		instance.Add(new BoundLabelStatement(syntax, label2));
		BoundStatement boundStatement2 = ((rewrittenCondition == null) ? ((BoundStatement)new BoundGotoStatement(syntax, label)) : ((BoundStatement)new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: true, label)));
		if (Instrument)
		{
			boundStatement2 = original.Kind switch
			{
				BoundKind.ForEachStatement => _instrumenter.InstrumentForEachStatementConditionalGotoStart((BoundForEachStatement)original, boundStatement2), 
				BoundKind.ForStatement => _instrumenter.InstrumentForStatementConditionalGotoStartOrBreak((BoundForStatement)original, boundStatement2), 
				_ => throw ExceptionUtilities.UnexpectedValue(original.Kind), 
			};
		}
		instance.Add(boundStatement2);
		instance.Add(new BoundLabelStatement(syntax, breakLabel));
		ImmutableArray<BoundStatement> statements = instance.ToImmutableAndFree();
		return new BoundBlock(syntax, outerLocals, statements, hasErrors);
	}

	private BoundStatement RewriteForStatement(BoundForStatement node, BoundStatement rewrittenInitializer, BoundExpression rewrittenCondition, BoundStatement rewrittenIncrement, BoundStatement rewrittenBody)
	{
		if (node.InnerLocals.IsEmpty)
		{
			return RewriteForStatementWithoutInnerLocals(node, node.OuterLocals, rewrittenInitializer, rewrittenCondition, rewrittenIncrement, rewrittenBody, node.BreakLabel, node.ContinueLabel, node.HasErrors);
		}
		SyntaxNode syntax = node.Syntax;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		if (rewrittenInitializer != null)
		{
			instance.Add(rewrittenInitializer);
		}
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("start");
		BoundStatement boundStatement = new BoundLabelStatement(syntax, label);
		if (Instrument)
		{
			boundStatement = new BoundSequencePoint(null, boundStatement);
		}
		instance.Add(boundStatement);
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
		if (rewrittenCondition != null)
		{
			BoundStatement boundStatement2 = new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: false, node.BreakLabel);
			if (Instrument)
			{
				boundStatement2 = _instrumenter.InstrumentForStatementConditionalGotoStartOrBreak(node, boundStatement2);
			}
			instance2.Add(boundStatement2);
		}
		instance2.Add(rewrittenBody);
		instance2.Add(new BoundLabelStatement(syntax, node.ContinueLabel));
		if (rewrittenIncrement != null)
		{
			instance2.Add(rewrittenIncrement);
		}
		instance2.Add(new BoundGotoStatement(syntax, label));
		instance.Add(new BoundBlock(syntax, node.InnerLocals, instance2.ToImmutableAndFree()));
		instance.Add(new BoundLabelStatement(syntax, node.BreakLabel));
		ImmutableArray<BoundStatement> statements = instance.ToImmutableAndFree();
		return new BoundBlock(syntax, node.OuterLocals, statements, node.HasErrors);
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		BoundExpression caseExpressionOpt = null;
		BoundLabel labelExpressionOpt = null;
		BoundStatement boundStatement = node.Update(node.Label, caseExpressionOpt, labelExpressionOpt);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = _instrumenter.InstrumentGotoStatement(node, boundStatement);
		}
		return boundStatement;
	}

	public override BoundNode VisitLabel(BoundLabel node)
	{
		return null;
	}

	public override BoundNode VisitSwitchLabel(BoundSwitchLabel node)
	{
		return node.Update(node.Label, null, node.ConstantValueOpt);
	}

	public override BoundNode VisitHostObjectMemberReference(BoundHostObjectMemberReference node)
	{
		SyntaxNode syntax = node.Syntax;
		FieldSymbol hostObjectField = _previousSubmissionFields.GetHostObjectField();
		BoundThisReference receiver = new BoundThisReference(syntax, _factory.CurrentType);
		return new BoundFieldAccess(syntax, receiver, hostObjectField, null);
	}

	public override BoundNode VisitIfStatement(BoundIfStatement node)
	{
		BoundExpression rewrittenCondition = VisitExpression(node.Condition);
		BoundStatement rewrittenConsequence = VisitStatement(node.Consequence);
		BoundStatement rewrittenAlternativeOpt = VisitStatement(node.AlternativeOpt);
		IfStatementSyntax syntax = (IfStatementSyntax)node.Syntax;
		if (Instrument && !node.WasCompilerGenerated)
		{
			rewrittenCondition = _instrumenter.InstrumentIfStatementCondition(node, rewrittenCondition, _factory);
		}
		BoundStatement boundStatement = RewriteIfStatement(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternativeOpt, node.HasErrors);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = _instrumenter.InstrumentIfStatement(node, boundStatement);
		}
		return boundStatement;
	}

	private static BoundStatement RewriteIfStatement(SyntaxNode syntax, BoundExpression rewrittenCondition, BoundStatement rewrittenConsequence, BoundStatement rewrittenAlternativeOpt, bool hasErrors)
	{
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("afterif");
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		if (rewrittenAlternativeOpt == null)
		{
			instance.Add(new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: false, label));
			instance.Add(rewrittenConsequence);
			instance.Add(new BoundSequencePoint(null, null));
			instance.Add(new BoundLabelStatement(syntax, label));
			ImmutableArray<BoundStatement> statements = instance.ToImmutableAndFree();
			return new BoundStatementList(syntax, statements, hasErrors);
		}
		GeneratedLabelSymbol label2 = new GeneratedLabelSymbol("alternative");
		instance.Add(new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: false, label2));
		instance.Add(rewrittenConsequence);
		instance.Add(new BoundGotoStatement(syntax, label));
		instance.Add(new BoundLabelStatement(syntax, label2));
		instance.Add(rewrittenAlternativeOpt);
		instance.Add(new BoundSequencePoint(null, null));
		instance.Add(new BoundLabelStatement(syntax, label));
		return new BoundStatementList(syntax, instance.ToImmutableAndFree(), hasErrors);
	}

	private BoundExpression MakeDynamicIndexerAccessReceiver(BoundDynamicIndexerAccess indexerAccess, BoundExpression loweredReceiver)
	{
		string text = indexerAccess.TryGetIndexedPropertyName();
		if (text != null)
		{
			return _dynamicFactory.MakeDynamicGetMember(loweredReceiver, text, resultIndexed: true).ToExpression();
		}
		return loweredReceiver;
	}

	public override BoundNode VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
	{
		BoundExpression loweredReceiver = VisitExpression(node.ReceiverOpt);
		ImmutableArray<BoundExpression> loweredArguments = VisitList(node.Arguments);
		return MakeDynamicGetIndex(node, loweredReceiver, loweredArguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt);
	}

	private BoundExpression MakeDynamicGetIndex(BoundDynamicIndexerAccess node, BoundExpression loweredReceiver, ImmutableArray<BoundExpression> loweredArguments, ImmutableArray<string> argumentNames, ImmutableArray<RefKind> refKinds)
	{
		EmbedIfNeedTo(loweredReceiver, node.ApplicableIndexers, node.Syntax);
		return _dynamicFactory.MakeDynamicGetIndex(MakeDynamicIndexerAccessReceiver(node, loweredReceiver), loweredArguments, argumentNames, refKinds).ToExpression();
	}

	public override BoundNode VisitIndexerAccess(BoundIndexerAccess node)
	{
		return VisitIndexerAccess(node, isLeftOfAssignment: false);
	}

	private BoundExpression VisitIndexerAccess(BoundIndexerAccess node, bool isLeftOfAssignment)
	{
		PropertySymbol indexer = node.Indexer;
		BoundExpression rewrittenReceiver = VisitExpression(node.ReceiverOpt);
		ImmutableArray<BoundExpression> rewrittenArguments = VisitList(node.Arguments);
		return MakeIndexerAccess(node.Syntax, rewrittenReceiver, indexer, rewrittenArguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.Type, node, isLeftOfAssignment);
	}

	private BoundExpression MakeIndexerAccess(SyntaxNode syntax, BoundExpression rewrittenReceiver, PropertySymbol indexer, ImmutableArray<BoundExpression> rewrittenArguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<RefKind> argumentRefKindsOpt, bool expanded, ImmutableArray<int> argsToParamsOpt, TypeSymbol type, BoundIndexerAccess oldNodeOpt, bool isLeftOfAssignment)
	{
		if (isLeftOfAssignment && indexer.RefKind == RefKind.None)
		{
			if (oldNodeOpt == null)
			{
				return new BoundIndexerAccess(syntax, rewrittenReceiver, indexer, rewrittenArguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, null, isLeftOfAssignment, type);
			}
			return oldNodeOpt.Update(rewrittenReceiver, indexer, rewrittenArguments, argumentNamesOpt, argumentRefKindsOpt, expanded, argsToParamsOpt, null, isLeftOfAssignment, type);
		}
		MethodSymbol ownOrInheritedGetMethod = indexer.GetOwnOrInheritedGetMethod();
		rewrittenArguments = MakeArguments(syntax, rewrittenArguments, indexer, ownOrInheritedGetMethod, expanded, argsToParamsOpt, ref argumentRefKindsOpt, out var temps, invokedAsExtensionMethod: false, ThreeState.True);
		BoundExpression boundExpression = MakePropertyGetAccess(syntax, rewrittenReceiver, indexer, rewrittenArguments, ownOrInheritedGetMethod);
		if (temps.IsDefaultOrEmpty)
		{
			return boundExpression;
		}
		return new BoundSequence(syntax, temps, ImmutableArray<BoundExpression>.Empty, boundExpression, type);
	}

	public override BoundNode VisitIsOperator(BoundIsOperator node)
	{
		BoundExpression rewrittenOperand = VisitExpression(node.Operand);
		BoundTypeExpression rewrittenTargetType = (BoundTypeExpression)VisitTypeExpression(node.TargetType);
		TypeSymbol rewrittenType = VisitType(node.Type);
		return MakeIsOperator(node, node.Syntax, rewrittenOperand, rewrittenTargetType, node.Conversion, rewrittenType);
	}

	private BoundExpression MakeIsOperator(BoundIsOperator oldNode, SyntaxNode syntax, BoundExpression rewrittenOperand, BoundTypeExpression rewrittenTargetType, Conversion conversion, TypeSymbol rewrittenType)
	{
		if (rewrittenOperand.Kind == BoundKind.MethodGroup)
		{
			BoundExpression receiverOpt = ((BoundMethodGroup)rewrittenOperand).ReceiverOpt;
			if (receiverOpt != null && receiverOpt.Kind != BoundKind.ThisReference)
			{
				return RewriteConstantIsOperator(receiverOpt.Syntax, receiverOpt, ConstantValue.False, rewrittenType);
			}
			return MakeLiteral(syntax, ConstantValue.False, rewrittenType);
		}
		TypeSymbol type = rewrittenOperand.Type;
		TypeSymbol type2 = rewrittenTargetType.Type;
		if (!_inExpressionLambda)
		{
			ConstantValue isOperatorConstantResult = Binder.GetIsOperatorConstantResult(type, type2, conversion.Kind, rewrittenOperand.ConstantValue);
			if (isOperatorConstantResult != null)
			{
				return RewriteConstantIsOperator(syntax, rewrittenOperand, isOperatorConstantResult, rewrittenType);
			}
			if (conversion.IsImplicit)
			{
				if (type.TypeKind == TypeKind.TypeParameter)
				{
					rewrittenOperand = MakeConversionNode(rewrittenOperand.Syntax, rewrittenOperand, Conversion.Boxing, _compilation.GetSpecialType(SpecialType.System_Object), @checked: false);
				}
				return MakeNullCheck(syntax, rewrittenOperand, BinaryOperatorKind.NotEqual);
			}
		}
		return oldNode.Update(rewrittenOperand, rewrittenTargetType, conversion, rewrittenType);
	}

	private BoundExpression RewriteConstantIsOperator(SyntaxNode syntax, BoundExpression loweredOperand, ConstantValue constantValue, TypeSymbol type)
	{
		return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(loweredOperand), MakeLiteral(syntax, constantValue, type), type);
	}

	public override BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)Visit(node.Body);
		BoundStatement boundStatement2 = new BoundLabelStatement(node.Syntax, node.Label);
		if (Instrument && node.Syntax is LabeledStatementSyntax)
		{
			boundStatement2 = _instrumenter.InstrumentLabelStatement(node, boundStatement2);
		}
		if (boundStatement == null)
		{
			return boundStatement2;
		}
		return BoundStatementList.Synthesized(node.Syntax, boundStatement2, boundStatement);
	}

	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		return MakeLiteral(node.Syntax, node.ConstantValue, node.Type, node);
	}

	private BoundExpression MakeLiteral(SyntaxNode syntax, ConstantValue constantValue, TypeSymbol type, BoundLiteral oldNodeOpt = null)
	{
		if (constantValue.IsDecimal)
		{
			return MakeDecimalLiteral(syntax, constantValue);
		}
		if (constantValue.IsDateTime)
		{
			return MakeDateTimeLiteral(syntax, constantValue);
		}
		if (oldNodeOpt != null)
		{
			return oldNodeOpt.Update(constantValue, type);
		}
		return new BoundLiteral(syntax, constantValue, type, constantValue.IsBad);
	}

	private BoundExpression MakeDecimalLiteral(SyntaxNode syntax, ConstantValue constantValue)
	{
		decimal decimalValue = constantValue.DecimalValue;
		decimalValue.GetBits(out var isNegative, out var scale, out var low, out var mid, out var high);
		ArrayBuilder<BoundExpression> arrayBuilder = new ArrayBuilder<BoundExpression>();
		SpecialMember member;
		if (scale == 0 && -2147483648m <= decimalValue && decimalValue <= 2147483647m)
		{
			MethodSymbol currentFunction = _factory.CurrentFunction;
			if ((currentFunction.MethodKind != MethodKind.StaticConstructor || currentFunction.ContainingType.SpecialType != SpecialType.System_Decimal) && !_inExpressionLambda)
			{
				Symbol symbol = null;
				if (decimalValue == 0m)
				{
					symbol = _compilation.GetSpecialTypeMember(SpecialMember.System_Decimal__Zero);
				}
				else if (decimalValue == 1m)
				{
					symbol = _compilation.GetSpecialTypeMember(SpecialMember.System_Decimal__One);
				}
				else if (decimalValue == -1m)
				{
					symbol = _compilation.GetSpecialTypeMember(SpecialMember.System_Decimal__MinusOne);
				}
				if ((object)symbol != null && !symbol.HasUseSiteError && !symbol.ContainingType.HasUseSiteError)
				{
					FieldSymbol fieldSymbol = (FieldSymbol)symbol;
					return new BoundFieldAccess(syntax, null, fieldSymbol, constantValue);
				}
			}
			member = SpecialMember.System_Decimal__CtorInt32;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create((int)decimalValue), _compilation.GetSpecialType(SpecialType.System_Int32)));
		}
		else if (scale == 0 && 0m <= decimalValue && decimalValue <= 4294967295m)
		{
			member = SpecialMember.System_Decimal__CtorUInt32;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create((uint)decimalValue), _compilation.GetSpecialType(SpecialType.System_UInt32)));
		}
		else if (scale == 0 && -9223372036854775808m <= decimalValue && decimalValue <= 9223372036854775807m)
		{
			member = SpecialMember.System_Decimal__CtorInt64;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create((long)decimalValue), _compilation.GetSpecialType(SpecialType.System_Int64)));
		}
		else if (scale == 0 && 0m <= decimalValue && decimalValue <= 18446744073709551615m)
		{
			member = SpecialMember.System_Decimal__CtorUInt64;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create((ulong)decimalValue), _compilation.GetSpecialType(SpecialType.System_UInt64)));
		}
		else
		{
			member = SpecialMember.System_Decimal__CtorInt32Int32Int32BooleanByte;
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(low), _compilation.GetSpecialType(SpecialType.System_Int32)));
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(mid), _compilation.GetSpecialType(SpecialType.System_Int32)));
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(high), _compilation.GetSpecialType(SpecialType.System_Int32)));
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(isNegative), _compilation.GetSpecialType(SpecialType.System_Boolean)));
			arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(scale), _compilation.GetSpecialType(SpecialType.System_Byte)));
		}
		MethodSymbol methodSymbol = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(member);
		return new BoundObjectCreationExpression(syntax, methodSymbol, arrayBuilder.ToImmutableAndFree(), default, default, expanded: false, default, constantValue, null, null, methodSymbol.ContainingType);
	}

	private BoundExpression MakeDateTimeLiteral(SyntaxNode syntax, ConstantValue constantValue)
	{
		ArrayBuilder<BoundExpression> arrayBuilder = new ArrayBuilder<BoundExpression>();
		arrayBuilder.Add(new BoundLiteral(syntax, ConstantValue.Create(constantValue.DateTimeValue.Ticks), _compilation.GetSpecialType(SpecialType.System_Int64)));
		MethodSymbol methodSymbol = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(SpecialMember.System_DateTime__CtorInt64);
		return new BoundObjectCreationExpression(syntax, methodSymbol, arrayBuilder.ToImmutableAndFree(), default, default, expanded: false, default, null, null, null, methodSymbol.ContainingType);
	}

	public override BoundNode VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		return RewriteLocalDeclaration(node, node.Syntax, node.LocalSymbol, VisitExpression(node.InitializerOpt), node.HasErrors);
	}

	private BoundStatement RewriteLocalDeclaration(BoundLocalDeclaration originalOpt, SyntaxNode syntax, LocalSymbol localSymbol, BoundExpression rewrittenInitializer, bool hasErrors = false)
	{
		if (rewrittenInitializer == null)
		{
			return null;
		}
		if (localSymbol.IsConst)
		{
			if (localSymbol.Type.IsReferenceType || localSymbol.ConstantValue != null)
			{
				return null;
			}
			hasErrors = true;
		}
		if (syntax is LocalDeclarationStatementSyntax localDeclarationStatementSyntax)
		{
			syntax = localDeclarationStatementSyntax.Declaration.Variables[0];
		}
		BoundStatement rewrittenLocalDeclaration = new BoundExpressionStatement(syntax, new BoundAssignmentOperator(syntax, new BoundLocal(syntax, localSymbol, null, localSymbol.Type), rewrittenInitializer, localSymbol.Type, localSymbol.IsRef), hasErrors);
		return InstrumentLocalDeclarationIfNecessary(originalOpt, localSymbol, rewrittenLocalDeclaration);
	}

	private BoundStatement InstrumentLocalDeclarationIfNecessary(BoundLocalDeclaration originalOpt, LocalSymbol localSymbol, BoundStatement rewrittenLocalDeclaration)
	{
		if (Instrument && originalOpt != null && !originalOpt.WasCompilerGenerated && !localSymbol.IsConst && (originalOpt.Syntax.Kind() == SyntaxKind.VariableDeclarator || (originalOpt.Syntax.Kind() == SyntaxKind.LocalDeclarationStatement && ((LocalDeclarationStatementSyntax)originalOpt.Syntax).Declaration.Variables.Count == 1)))
		{
			rewrittenLocalDeclaration = _instrumenter.InstrumentLocalInitialization(originalOpt, rewrittenLocalDeclaration);
		}
		return rewrittenLocalDeclaration;
	}

	public sealed override BoundNode VisitOutVariablePendingInference(OutVariablePendingInference node)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override BoundNode VisitLockStatement(BoundLockStatement node)
	{
		LockStatementSyntax lockStatementSyntax = (LockStatementSyntax)node.Syntax;
		BoundExpression boundExpression = VisitExpression(node.Argument);
		BoundStatement boundStatement = (BoundStatement)Visit(node.Body);
		TypeSymbol typeSymbol = boundExpression.Type;
		if ((object)typeSymbol == null)
		{
			typeSymbol = _compilation.GetSpecialType(SpecialType.System_Object);
			boundExpression = MakeLiteral(boundExpression.Syntax, boundExpression.ConstantValue, typeSymbol);
		}
		if (typeSymbol.Kind == SymbolKind.TypeParameter)
		{
			typeSymbol = _compilation.GetSpecialType(SpecialType.System_Object);
			boundExpression = MakeConversionNode(boundExpression.Syntax, boundExpression, Conversion.Boxing, typeSymbol, @checked: false, explicitCastInCode: false, boundExpression.ConstantValue);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store, RefKind.None, SynthesizedLocalKind.Lock, lockStatementSyntax);
		BoundStatement lockTargetCapture = new BoundExpressionStatement(lockStatementSyntax, store);
		BoundExpression expression = ((!TryGetWellKnownTypeMember<MethodSymbol>(lockStatementSyntax, WellKnownMember.System_Threading_Monitor__Exit, out var symbol)) ? ((BoundExpression)new BoundBadExpression(lockStatementSyntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)boundLocal), ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)BoundCall.Synthesized(lockStatementSyntax, null, symbol, boundLocal)));
		BoundStatement boundStatement2 = new BoundExpressionStatement(lockStatementSyntax, expression);
		if ((TryGetWellKnownTypeMember<MethodSymbol>(lockStatementSyntax, WellKnownMember.System_Threading_Monitor__Enter2, out var symbol2, isOptional: true) || TryGetWellKnownTypeMember<MethodSymbol>(lockStatementSyntax, WellKnownMember.System_Threading_Monitor__Enter, out symbol2)) && symbol2.ParameterCount == 2)
		{
			TypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Boolean);
			BoundLocal boundLocal2 = _factory.StoreToTemp(MakeLiteral(boundExpression.Syntax, ConstantValue.False, specialType), out var store2, RefKind.None, SynthesizedLocalKind.LockTaken, lockStatementSyntax);
			BoundStatement item = new BoundExpressionStatement(lockStatementSyntax, store2);
			BoundStatement item2 = new BoundExpressionStatement(lockStatementSyntax, BoundCall.Synthesized(lockStatementSyntax, null, symbol2, boundLocal, boundLocal2));
			boundStatement2 = RewriteIfStatement(lockStatementSyntax, boundLocal2, boundStatement2, null, node.HasErrors);
			return new BoundBlock(lockStatementSyntax, ImmutableArray.Create(boundLocal.LocalSymbol, boundLocal2.LocalSymbol), ImmutableArray.Create(InstrumentLockTargetCapture(node, lockTargetCapture), item, new BoundTryStatement(lockStatementSyntax, BoundBlock.SynthesizedNoLocals(lockStatementSyntax, ImmutableArray.Create(item2, boundStatement)), ImmutableArray<BoundCatchBlock>.Empty, BoundBlock.SynthesizedNoLocals(lockStatementSyntax, boundStatement2))));
		}
		BoundExpression expression2 = (((object)symbol2 == null) ? ((BoundExpression)new BoundBadExpression(lockStatementSyntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)boundLocal), ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)BoundCall.Synthesized(lockStatementSyntax, null, symbol2, boundLocal)));
		BoundStatement item3 = new BoundExpressionStatement(lockStatementSyntax, expression2);
		return new BoundBlock(lockStatementSyntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create(InstrumentLockTargetCapture(node, lockTargetCapture), item3, new BoundTryStatement(lockStatementSyntax, BoundBlock.SynthesizedNoLocals(lockStatementSyntax, boundStatement), ImmutableArray<BoundCatchBlock>.Empty, BoundBlock.SynthesizedNoLocals(lockStatementSyntax, boundStatement2))));
	}

	private BoundStatement InstrumentLockTargetCapture(BoundLockStatement original, BoundStatement lockTargetCapture)
	{
		if (!Instrument)
		{
			return lockTargetCapture;
		}
		return _instrumenter.InstrumentLockTargetCapture(original, lockTargetCapture);
	}

	public override BoundNode VisitMultipleLocalDeclarations(BoundMultipleLocalDeclarations node)
	{
		ArrayBuilder<BoundStatement> arrayBuilder = null;
		foreach (BoundLocalDeclaration localDeclaration in node.LocalDeclarations)
		{
			BoundNode boundNode = VisitLocalDeclaration(localDeclaration);
			if (boundNode != null)
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<BoundStatement>.GetInstance();
				}
				arrayBuilder.Add((BoundStatement)boundNode);
			}
		}
		if (arrayBuilder != null)
		{
			return BoundStatementList.Synthesized(node.Syntax, node.HasErrors, arrayBuilder.ToImmutableAndFree());
		}
		return null;
	}

	public override BoundNode VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		BoundExpression rewrittenLeft = (BoundExpression)Visit(node.LeftOperand);
		BoundExpression rewrittenRight = (BoundExpression)Visit(node.RightOperand);
		TypeSymbol rewrittenResultType = VisitType(node.Type);
		return MakeNullCoalescingOperator(node.Syntax, rewrittenLeft, rewrittenRight, node.LeftConversion, rewrittenResultType);
	}

	private BoundExpression MakeNullCoalescingOperator(SyntaxNode syntax, BoundExpression rewrittenLeft, BoundExpression rewrittenRight, Conversion leftConversion, TypeSymbol rewrittenResultType)
	{
		if (_inExpressionLambda)
		{
			TypeSymbol fromType = rewrittenLeft.Type.StrippedType();
			Conversion leftConversion2 = TryMakeConversion(syntax, leftConversion, fromType, rewrittenResultType);
			if (!leftConversion2.Exists)
			{
				return BadExpression(syntax, rewrittenResultType, rewrittenLeft, rewrittenRight);
			}
			return new BoundNullCoalescingOperator(syntax, rewrittenLeft, rewrittenRight, leftConversion2, rewrittenResultType);
		}
		if (rewrittenLeft.IsDefaultValue())
		{
			return rewrittenRight;
		}
		if (rewrittenLeft.ConstantValue != null)
		{
			return GetConvertedLeftForNullCoalescingOperator(rewrittenLeft, leftConversion, rewrittenResultType);
		}
		if (IsStringConcat(rewrittenLeft))
		{
			return GetConvertedLeftForNullCoalescingOperator(rewrittenLeft, leftConversion, rewrittenResultType);
		}
		if (rewrittenLeft.Type.IsReferenceType && leftConversion.IsImplicit && !leftConversion.IsUserDefined)
		{
			if (!leftConversion.IsIdentity)
			{
				rewrittenLeft = MakeConversionNode(rewrittenLeft.Syntax, rewrittenLeft, leftConversion, rewrittenResultType, @checked: false);
			}
			return new BoundNullCoalescingOperator(syntax, rewrittenLeft, rewrittenRight, Conversion.Identity, rewrittenResultType);
		}
		if ((leftConversion.IsIdentity || leftConversion.Kind == ConversionKind.ExplicitNullable) && rewrittenLeft is BoundLoweredConditionalAccess boundLoweredConditionalAccess && (boundLoweredConditionalAccess.WhenNullOpt == null || NullableNeverHasValue(boundLoweredConditionalAccess.WhenNullOpt)))
		{
			BoundExpression boundExpression = NullableAlwaysHasValue(boundLoweredConditionalAccess.WhenNotNull);
			if (boundExpression != null)
			{
				BoundExpression boundExpression2 = rewrittenRight;
				if (boundExpression2.Type.IsNullableType())
				{
					boundExpression = boundLoweredConditionalAccess.WhenNotNull;
				}
				if (boundExpression2.IsDefaultValue() && boundExpression2.Type.SpecialType != SpecialType.System_Decimal)
				{
					boundExpression2 = null;
				}
				return boundLoweredConditionalAccess.Update(boundLoweredConditionalAccess.Receiver, boundLoweredConditionalAccess.HasValueMethodOpt, boundExpression, boundExpression2, boundLoweredConditionalAccess.Id, rewrittenResultType);
			}
		}
		BoundLocal boundLocal = _factory.StoreToTemp(rewrittenLeft, out var store);
		BoundExpression rewrittenCondition = MakeNullCheck(syntax, boundLocal, BinaryOperatorKind.NotEqual);
		BoundExpression convertedLeftForNullCoalescingOperator = GetConvertedLeftForNullCoalescingOperator(boundLocal, leftConversion, rewrittenResultType);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, convertedLeftForNullCoalescingOperator, rewrittenRight, null, rewrittenResultType, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, rewrittenResultType);
	}

	private bool IsStringConcat(BoundExpression expression)
	{
		if (expression.Kind != BoundKind.Call)
		{
			return false;
		}
		MethodSymbol method = ((BoundCall)expression).Method;
		if (method.IsStatic && method.ContainingType.SpecialType == SpecialType.System_String && ((object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringString) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringStringString) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringStringStringString) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObject) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObjectObject) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObjectObjectObject) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringArray) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObjectArray)))
		{
			return true;
		}
		return false;
	}

	private BoundExpression GetConvertedLeftForNullCoalescingOperator(BoundExpression rewrittenLeft, Conversion leftConversion, TypeSymbol rewrittenResultType)
	{
		TypeSymbol type = rewrittenLeft.Type;
		if (type != rewrittenResultType && type.IsNullableType())
		{
			TypeSymbol nullableUnderlyingType = type.GetNullableUnderlyingType();
			rewrittenLeft = BoundCall.Synthesized(method: UnsafeGetNullableMethod(rewrittenLeft.Syntax, type, SpecialMember.System_Nullable_T_GetValueOrDefault), syntax: rewrittenLeft.Syntax, receiverOpt: rewrittenLeft);
			if (nullableUnderlyingType == rewrittenResultType)
			{
				return rewrittenLeft;
			}
		}
		return MakeConversionNode(rewrittenLeft.Syntax, rewrittenLeft, leftConversion, rewrittenResultType, @checked: false);
	}

	public override BoundNode VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> loweredArguments = VisitList(node.Arguments);
		BoundExpression boundExpression = _dynamicFactory.MakeDynamicConstructorInvocation(node.Syntax, node.Type, loweredArguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt).ToExpression();
		if (node.InitializerExpressionOpt == null || node.InitializerExpressionOpt.HasErrors)
		{
			return boundExpression;
		}
		return MakeObjectCreationWithInitializer(node.Syntax, boundExpression, node.InitializerExpressionOpt, node.Type);
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> rewrittenArguments = VisitList(node.Arguments);
		ImmutableArray<RefKind> argumentRefKindsOpt = node.ArgumentRefKindsOpt;
		rewrittenArguments = MakeArguments(node.Syntax, rewrittenArguments, node.Constructor, node.Constructor, node.Expanded, node.ArgsToParamsOpt, ref argumentRefKindsOpt, out var temps);
		BoundExpression boundExpression;
		if (_inExpressionLambda)
		{
			if (!temps.IsDefaultOrEmpty)
			{
				throw ExceptionUtilities.UnexpectedValue(temps.Length);
			}
			boundExpression = node.UpdateArgumentsAndInitializer(rewrittenArguments, argumentRefKindsOpt, MakeObjectCreationInitializerForExpressionTree(node.InitializerExpressionOpt), node.Constructor.ContainingType);
			if (node.Type.IsInterfaceType())
			{
				boundExpression = MakeConversionNode(boundExpression, node.Type, @checked: false);
			}
			return boundExpression;
		}
		boundExpression = node.UpdateArgumentsAndInitializer(rewrittenArguments, argumentRefKindsOpt, null, node.Constructor.ContainingType);
		if (node.Constructor.IsDefaultValueTypeConstructor())
		{
			boundExpression = new BoundDefaultExpression(boundExpression.Syntax, boundExpression.Type);
		}
		if (!temps.IsDefaultOrEmpty)
		{
			boundExpression = new BoundSequence(node.Syntax, temps, ImmutableArray<BoundExpression>.Empty, boundExpression, node.Type);
		}
		if (node.Type.IsInterfaceType())
		{
			boundExpression = MakeConversionNode(boundExpression, node.Type, @checked: false);
		}
		if (node.InitializerExpressionOpt == null || node.InitializerExpressionOpt.HasErrors)
		{
			return boundExpression;
		}
		return MakeObjectCreationWithInitializer(node.Syntax, boundExpression, node.InitializerExpressionOpt, node.Type);
	}

	private BoundObjectInitializerExpressionBase MakeObjectCreationInitializerForExpressionTree(BoundObjectInitializerExpressionBase initializerExpressionOpt)
	{
		if (initializerExpressionOpt != null && !initializerExpressionOpt.HasErrors)
		{
			ImmutableArray<BoundExpression> newInitializers = MakeObjectOrCollectionInitializersForExpressionTree(initializerExpressionOpt);
			return UpdateInitializers(initializerExpressionOpt, newInitializers);
		}
		return null;
	}

	private BoundExpression MakeObjectCreationWithInitializer(SyntaxNode syntax, BoundExpression rewrittenObjectCreation, BoundExpression initializerExpression, TypeSymbol type)
	{
		BoundLocal boundLocal = _factory.StoreToTemp(rewrittenObjectCreation, out var store);
		ArrayBuilder<BoundExpression> dynamicSiteInitializers = null;
		ArrayBuilder<LocalSymbol> temps = null;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		AddObjectOrCollectionInitializers(ref dynamicSiteInitializers, ref temps, instance, boundLocal, initializerExpression);
		int num = dynamicSiteInitializers?.Count ?? 0;
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance(1 + num + instance.Count);
		instance2.Add(store);
		if (num > 0)
		{
			instance2.AddRange(dynamicSiteInitializers);
			dynamicSiteInitializers.Free();
		}
		instance2.AddRange(instance);
		instance.Free();
		ImmutableArray<LocalSymbol> locals;
		if (temps == null)
		{
			locals = ImmutableArray.Create(boundLocal.LocalSymbol);
		}
		else
		{
			temps.Insert(0, boundLocal.LocalSymbol);
			locals = temps.ToImmutableAndFree();
		}
		return new BoundSequence(syntax, locals, instance2.ToImmutableAndFree(), boundLocal, type);
	}

	public override BoundNode VisitNewT(BoundNewT node)
	{
		if (_inExpressionLambda)
		{
			return node.Update(MakeObjectCreationInitializerForExpressionTree(node.InitializerExpressionOpt), node.Type);
		}
		BoundExpression boundExpression = MakeNewT(node.Syntax, (TypeParameterSymbol)node.Type);
		if (node.InitializerExpressionOpt == null || node.InitializerExpressionOpt.HasErrors)
		{
			return boundExpression;
		}
		return MakeObjectCreationWithInitializer(node.Syntax, boundExpression, node.InitializerExpressionOpt, boundExpression.Type);
	}

	private BoundExpression MakeNewT(SyntaxNode syntax, TypeParameterSymbol typeParameter)
	{
		if (!TryGetWellKnownTypeMember<MethodSymbol>(syntax, WellKnownMember.System_Activator__CreateInstance_T, out var symbol))
		{
			return new BoundDefaultExpression(syntax, null, typeParameter, hasErrors: true);
		}
		symbol = symbol.Construct(ImmutableArray.Create((TypeSymbol)typeParameter));
		return new BoundCall(syntax, null, symbol, ImmutableArray<BoundExpression>.Empty, default, default, isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default, LookupResultKind.Viable, null, typeParameter);
	}

	public override BoundNode VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		SyntaxNode syntax = _factory.Syntax;
		_factory.Syntax = node.Syntax;
		MethodSymbol methodSymbol = _factory.WellKnownMethod(WellKnownMember.System_Guid__ctor);
		BoundExpression arg = (((object)methodSymbol == null) ? ((BoundExpression)new BoundBadExpression(node.Syntax, LookupResultKind.NotCreatable, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)_factory.New(methodSymbol, _factory.Literal(node.GuidString))));
		MethodSymbol methodSymbol2 = _factory.WellKnownMethod(WellKnownMember.System_Runtime_InteropServices_Marshal__GetTypeFromCLSID, isOptional: true);
		if ((object)methodSymbol2 == null)
		{
			methodSymbol2 = _factory.WellKnownMethod(WellKnownMember.System_Type__GetTypeFromCLSID);
		}
		BoundExpression arg2 = (((object)methodSymbol2 == null) ? ((BoundExpression)new BoundBadExpression(node.Syntax, LookupResultKind.OverloadResolutionFailure, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)_factory.Call(null, methodSymbol2, arg)));
		MethodSymbol methodSymbol3 = _factory.WellKnownMethod(WellKnownMember.System_Activator__CreateInstance);
		BoundExpression boundExpression = (((object)methodSymbol3 == null) ? new BoundBadExpression(node.Syntax, LookupResultKind.OverloadResolutionFailure, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, node.Type) : _factory.Convert(node.Type, _factory.Call(null, methodSymbol3, arg2)));
		_factory.Syntax = syntax;
		if (node.InitializerExpressionOpt == null || node.InitializerExpressionOpt.HasErrors)
		{
			return boundExpression;
		}
		return MakeObjectCreationWithInitializer(node.Syntax, boundExpression, node.InitializerExpressionOpt, node.Type);
	}

	private static BoundObjectInitializerExpressionBase UpdateInitializers(BoundObjectInitializerExpressionBase initializerExpression, ImmutableArray<BoundExpression> newInitializers)
	{
		if (initializerExpression.Kind == BoundKind.ObjectInitializerExpression)
		{
			return ((BoundObjectInitializerExpression)initializerExpression).Update(newInitializers, initializerExpression.Type);
		}
		return ((BoundCollectionInitializerExpression)initializerExpression).Update(newInitializers, initializerExpression.Type);
	}

	private void AddObjectOrCollectionInitializers(ref ArrayBuilder<BoundExpression> dynamicSiteInitializers, ref ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> result, BoundExpression rewrittenReceiver, BoundExpression initializerExpression)
	{
		switch (initializerExpression.Kind)
		{
		case BoundKind.ObjectInitializerExpression:
			AddObjectInitializers(ref dynamicSiteInitializers, ref temps, result, rewrittenReceiver, ((BoundObjectInitializerExpression)initializerExpression).Initializers);
			break;
		case BoundKind.CollectionInitializerExpression:
			AddCollectionInitializers(ref dynamicSiteInitializers, result, rewrittenReceiver, ((BoundCollectionInitializerExpression)initializerExpression).Initializers);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(initializerExpression.Kind);
		}
	}

	private ImmutableArray<BoundExpression> MakeObjectOrCollectionInitializersForExpressionTree(BoundExpression initializerExpression)
	{
		switch (initializerExpression.Kind)
		{
		case BoundKind.ObjectInitializerExpression:
			return VisitList(((BoundObjectInitializerExpression)initializerExpression).Initializers);
		case BoundKind.CollectionInitializerExpression:
		{
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			ArrayBuilder<BoundExpression> dynamicSiteInitializers = null;
			AddCollectionInitializers(ref dynamicSiteInitializers, instance, null, ((BoundCollectionInitializerExpression)initializerExpression).Initializers);
			return instance.ToImmutableAndFree();
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(initializerExpression.Kind);
		}
	}

	private void AddCollectionInitializers(ref ArrayBuilder<BoundExpression> dynamicSiteInitializers, ArrayBuilder<BoundExpression> result, BoundExpression rewrittenReceiver, ImmutableArray<BoundExpression> initializers)
	{
		foreach (BoundExpression item in initializers)
		{
			BoundExpression boundExpression = ((item.Kind != BoundKind.CollectionElementInitializer) ? MakeDynamicCollectionInitializer(rewrittenReceiver, (BoundDynamicCollectionElementInitializer)item) : MakeCollectionInitializer(rewrittenReceiver, (BoundCollectionElementInitializer)item));
			if (boundExpression != null)
			{
				result.Add(boundExpression);
			}
		}
	}

	private BoundExpression MakeDynamicCollectionInitializer(BoundExpression rewrittenReceiver, BoundDynamicCollectionElementInitializer initializer)
	{
		ImmutableArray<BoundExpression> loweredArguments = VisitList(initializer.Arguments);
		EmbedIfNeedTo(rewrittenReceiver, initializer.ApplicableMethods, initializer.Syntax);
		return _dynamicFactory.MakeDynamicMemberInvocation("Add", rewrittenReceiver, ImmutableArray<TypeSymbol>.Empty, loweredArguments, default, default, hasImplicitReceiver: false, resultDiscarded: true).ToExpression();
	}

	private BoundExpression MakeCollectionInitializer(BoundExpression rewrittenReceiver, BoundCollectionElementInitializer initializer)
	{
		SyntaxNode syntax = initializer.Syntax;
		MethodSymbol addMethod = initializer.AddMethod;
		if (_allowOmissionOfConditionalCalls && addMethod.CallsAreOmitted(initializer.SyntaxTree))
		{
			return null;
		}
		ImmutableArray<BoundExpression> rewrittenArguments = VisitList(initializer.Arguments);
		TypeSymbol type = VisitType(initializer.Type);
		ImmutableArray<RefKind> argumentRefKindsOpt = default;
		rewrittenArguments = MakeArguments(syntax, rewrittenArguments, addMethod, addMethod, initializer.Expanded, initializer.ArgsToParamsOpt, ref argumentRefKindsOpt, out var temps, invokedAsExtensionMethod: false, ThreeState.True);
		if (initializer.InvokedAsExtensionMethod)
		{
			rewrittenArguments = rewrittenArguments.SetItem(0, rewrittenReceiver);
			rewrittenReceiver = null;
		}
		if (_inExpressionLambda)
		{
			return initializer.Update(addMethod, rewrittenArguments, rewrittenReceiver, expanded: false, default, initializer.InvokedAsExtensionMethod, initializer.ResultKind, initializer.BinderOpt, type);
		}
		return MakeCall(null, syntax, rewrittenReceiver, addMethod, rewrittenArguments, default, initializer.InvokedAsExtensionMethod, initializer.ResultKind, addMethod.ReturnType, temps);
	}

	private void AddObjectInitializers(ref ArrayBuilder<BoundExpression> dynamicSiteInitializers, ref ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> result, BoundExpression rewrittenReceiver, ImmutableArray<BoundExpression> initializers)
	{
		foreach (BoundExpression item in initializers)
		{
			AddObjectInitializer(ref dynamicSiteInitializers, ref temps, result, rewrittenReceiver, (BoundAssignmentOperator)item);
		}
	}

	private void AddObjectInitializer(ref ArrayBuilder<BoundExpression> dynamicSiteInitializers, ref ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> result, BoundExpression rewrittenReceiver, BoundAssignmentOperator assignment)
	{
		BoundExpression boundExpression = null;
		if (assignment.Left.Kind != BoundKind.PointerElementAccess)
		{
			boundExpression = VisitExpression(assignment.Left);
		}
		BoundKind kind = assignment.Right.Kind;
		bool flag = kind == BoundKind.ObjectInitializerExpression || kind == BoundKind.CollectionInitializerExpression;
		BoundExpression boundExpression2;
		switch ((boundExpression ?? assignment.Left).Kind)
		{
		case BoundKind.ObjectInitializerMember:
		{
			BoundObjectInitializerMember boundObjectInitializerMember = (BoundObjectInitializerMember)boundExpression;
			if (!boundObjectInitializerMember.Arguments.IsDefaultOrEmpty)
			{
				ImmutableArray<BoundExpression> arguments = EvaluateSideEffectingArgumentsToTemps(boundObjectInitializerMember.Arguments, result, ref temps);
				boundObjectInitializerMember = boundObjectInitializerMember.Update(boundObjectInitializerMember.MemberSymbol, arguments, boundObjectInitializerMember.ArgumentNamesOpt, boundObjectInitializerMember.ArgumentRefKindsOpt, boundObjectInitializerMember.Expanded, boundObjectInitializerMember.ArgsToParamsOpt, boundObjectInitializerMember.ResultKind, boundObjectInitializerMember.ReceiverType, boundObjectInitializerMember.BinderOpt, boundObjectInitializerMember.Type);
			}
			if (boundObjectInitializerMember.MemberSymbol == null && boundObjectInitializerMember.Type.IsDynamic())
			{
				if (dynamicSiteInitializers == null)
				{
					dynamicSiteInitializers = ArrayBuilder<BoundExpression>.GetInstance();
				}
				if (!flag)
				{
					BoundExpression loweredRight = VisitExpression(assignment.Right);
					LoweredDynamicOperation loweredDynamicOperation = _dynamicFactory.MakeDynamicSetIndex(rewrittenReceiver, boundObjectInitializerMember.Arguments, boundObjectInitializerMember.ArgumentNamesOpt, boundObjectInitializerMember.ArgumentRefKindsOpt, loweredRight);
					dynamicSiteInitializers.Add(loweredDynamicOperation.SiteInitialization);
					result.Add(loweredDynamicOperation.SiteInvocation);
					return;
				}
				LoweredDynamicOperation loweredDynamicOperation2 = _dynamicFactory.MakeDynamicGetIndex(rewrittenReceiver, boundObjectInitializerMember.Arguments, boundObjectInitializerMember.ArgumentNamesOpt, boundObjectInitializerMember.ArgumentRefKindsOpt);
				dynamicSiteInitializers.Add(loweredDynamicOperation2.SiteInitialization);
				boundExpression2 = loweredDynamicOperation2.SiteInvocation;
			}
			else
			{
				boundExpression2 = MakeObjectInitializerMemberAccess(rewrittenReceiver, boundObjectInitializerMember, flag);
				if (!flag)
				{
					BoundExpression rewrittenRight2 = VisitExpression(assignment.Right);
					result.Add(MakeStaticAssignmentOperator(assignment.Syntax, boundExpression2, rewrittenRight2, isRef: false, assignment.Type, used: false));
					return;
				}
			}
			break;
		}
		case BoundKind.DynamicObjectInitializerMember:
		{
			if (dynamicSiteInitializers == null)
			{
				dynamicSiteInitializers = ArrayBuilder<BoundExpression>.GetInstance();
			}
			BoundDynamicObjectInitializerMember boundDynamicObjectInitializerMember = (BoundDynamicObjectInitializerMember)boundExpression;
			if (!flag)
			{
				BoundExpression loweredRight2 = VisitExpression(assignment.Right);
				LoweredDynamicOperation loweredDynamicOperation3 = _dynamicFactory.MakeDynamicSetMember(rewrittenReceiver, boundDynamicObjectInitializerMember.MemberName, loweredRight2);
				dynamicSiteInitializers.Add(loweredDynamicOperation3.SiteInitialization);
				result.Add(loweredDynamicOperation3.SiteInvocation);
				return;
			}
			LoweredDynamicOperation loweredDynamicOperation4 = _dynamicFactory.MakeDynamicGetMember(rewrittenReceiver, boundDynamicObjectInitializerMember.MemberName, resultIndexed: false);
			dynamicSiteInitializers.Add(loweredDynamicOperation4.SiteInitialization);
			boundExpression2 = loweredDynamicOperation4.SiteInvocation;
			break;
		}
		case BoundKind.ArrayAccess:
		{
			BoundArrayAccess boundArrayAccess = (BoundArrayAccess)boundExpression;
			ImmutableArray<BoundExpression> indices = EvaluateSideEffectingArgumentsToTemps(boundArrayAccess.Indices, result, ref temps);
			boundExpression2 = boundArrayAccess.Update(rewrittenReceiver, indices, boundArrayAccess.Type);
			if (!flag)
			{
				BoundExpression rewrittenRight = VisitExpression(assignment.Right);
				result.Add(MakeStaticAssignmentOperator(assignment.Syntax, boundExpression2, rewrittenRight, isRef: false, assignment.Type, used: false));
				return;
			}
			break;
		}
		case BoundKind.PointerElementAccess:
		{
			BoundPointerElementAccess boundPointerElementAccess = (BoundPointerElementAccess)assignment.Left;
			BoundExpression boundExpression3 = VisitExpression(boundPointerElementAccess.Index);
			if (CanChangeValueBetweenReads(boundExpression3))
			{
				BoundLocal boundLocal = _factory.StoreToTemp(boundExpression3, out var store);
				boundExpression3 = boundLocal;
				if (temps == null)
				{
					temps = ArrayBuilder<LocalSymbol>.GetInstance();
				}
				temps.Add(boundLocal.LocalSymbol);
				result.Add(store);
			}
			boundExpression2 = RewritePointerElementAccess(boundPointerElementAccess, rewrittenReceiver, boundExpression3);
			if (!flag)
			{
				BoundExpression rewrittenRight3 = VisitExpression(assignment.Right);
				result.Add(MakeStaticAssignmentOperator(assignment.Syntax, boundExpression2, rewrittenRight3, isRef: false, assignment.Type, used: false));
				return;
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue((boundExpression ?? assignment.Left).Kind);
		}
		AddObjectOrCollectionInitializers(ref dynamicSiteInitializers, ref temps, result, boundExpression2, assignment.Right);
	}

	private ImmutableArray<BoundExpression> EvaluateSideEffectingArgumentsToTemps(ImmutableArray<BoundExpression> args, ArrayBuilder<BoundExpression> sideeffects, ref ArrayBuilder<LocalSymbol> temps)
	{
		ArrayBuilder<BoundExpression> arrayBuilder = null;
		for (int i = 0; i < args.Length; i++)
		{
			BoundExpression boundExpression = args[i];
			if (CanChangeValueBetweenReads(boundExpression))
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<BoundExpression>.GetInstance(args.Length);
					arrayBuilder.AddRange(args, i);
				}
				BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store);
				arrayBuilder.Add(boundLocal);
				if (temps == null)
				{
					temps = ArrayBuilder<LocalSymbol>.GetInstance();
				}
				temps.Add(boundLocal.LocalSymbol);
				sideeffects.Add(store);
			}
			else
			{
				arrayBuilder?.Add(boundExpression);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? args;
	}

	private BoundExpression MakeObjectInitializerMemberAccess(BoundExpression rewrittenReceiver, BoundObjectInitializerMember rewrittenLeft, bool isRhsNestedInitializer)
	{
		Symbol memberSymbol = rewrittenLeft.MemberSymbol;
		switch (memberSymbol.Kind)
		{
		case SymbolKind.Field:
		{
			FieldSymbol fieldSymbol = (FieldSymbol)memberSymbol;
			return MakeFieldAccess(rewrittenLeft.Syntax, rewrittenReceiver, fieldSymbol, null, rewrittenLeft.ResultKind, fieldSymbol.Type);
		}
		case SymbolKind.Property:
		{
			PropertySymbol propertySymbol = (PropertySymbol)memberSymbol;
			if (!rewrittenLeft.Arguments.IsEmpty || propertySymbol.IsIndexedProperty)
			{
				return MakeIndexerAccess(rewrittenLeft.Syntax, rewrittenReceiver, propertySymbol, rewrittenLeft.Arguments, rewrittenLeft.ArgumentNamesOpt, rewrittenLeft.ArgumentRefKindsOpt, rewrittenLeft.Expanded, rewrittenLeft.ArgsToParamsOpt, propertySymbol.Type, null, !isRhsNestedInitializer);
			}
			return MakePropertyAccess(rewrittenLeft.Syntax, rewrittenReceiver, propertySymbol, rewrittenLeft.ResultKind, propertySymbol.Type, !isRhsNestedInitializer);
		}
		case SymbolKind.Event:
		{
			EventSymbol eventSymbol = (EventSymbol)memberSymbol;
			return MakeEventAccess(rewrittenLeft.Syntax, rewrittenReceiver, eventSymbol, null, rewrittenLeft.ResultKind, eventSymbol.Type);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(memberSymbol.Kind);
		}
	}

	public override BoundNode VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		BoundExpression loweredInput = VisitExpression(node.Expression);
		BoundPattern loweredPattern = LowerPattern(node.Pattern);
		return MakeIsPattern(loweredPattern, loweredInput);
	}

	private BoundExpression MakeIsPattern(BoundPattern loweredPattern, BoundExpression loweredInput)
	{
		_factory.Syntax = loweredPattern.Syntax;
		switch (loweredPattern.Kind)
		{
		case BoundKind.DeclarationPattern:
		{
			BoundDeclarationPattern loweredPattern3 = (BoundDeclarationPattern)loweredPattern;
			return MakeIsDeclarationPattern(loweredPattern3, loweredInput);
		}
		case BoundKind.WildcardPattern:
			return _factory.Literal(value: true);
		case BoundKind.ConstantPattern:
		{
			BoundConstantPattern loweredPattern2 = (BoundConstantPattern)loweredPattern;
			return MakeIsConstantPattern(loweredPattern2, loweredInput);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(loweredPattern.Kind);
		}
	}

	private BoundPattern LowerPattern(BoundPattern pattern)
	{
		switch (pattern.Kind)
		{
		case BoundKind.DeclarationPattern:
		{
			BoundDeclarationPattern boundDeclarationPattern = (BoundDeclarationPattern)pattern;
			return boundDeclarationPattern.Update(boundDeclarationPattern.Variable, VisitExpression(boundDeclarationPattern.VariableAccess), boundDeclarationPattern.DeclaredType, boundDeclarationPattern.IsVar);
		}
		case BoundKind.ConstantPattern:
		{
			BoundConstantPattern boundConstantPattern = (BoundConstantPattern)pattern;
			return boundConstantPattern.Update(VisitExpression(boundConstantPattern.Value), boundConstantPattern.ConstantValue);
		}
		default:
			return pattern;
		}
	}

	private BoundExpression MakeIsConstantPattern(BoundConstantPattern loweredPattern, BoundExpression loweredInput)
	{
		return CompareWithConstant(loweredInput, loweredPattern.Value);
	}

	private BoundExpression MakeIsDeclarationPattern(BoundDeclarationPattern loweredPattern, BoundExpression loweredInput)
	{
		if (loweredPattern.IsVar)
		{
			BoundLiteral boundLiteral = _factory.Literal(value: true);
			if (loweredPattern.VariableAccess.Kind == BoundKind.DiscardExpression)
			{
				return _factory.MakeSequence(loweredInput, boundLiteral);
			}
			BoundAssignmentOperator boundAssignmentOperator = _factory.AssignmentExpression(loweredPattern.VariableAccess, loweredInput);
			return _factory.MakeSequence(boundAssignmentOperator, boundLiteral);
		}
		if (loweredPattern.VariableAccess.Kind == BoundKind.DiscardExpression)
		{
			BoundLocal loweredTarget = _factory.MakeTempForDiscard((BoundDiscardExpression)loweredPattern.VariableAccess, out var temp);
			return _factory.Sequence(ImmutableArray.Create(temp), ImmutableArray<BoundExpression>.Empty, MakeIsDeclarationPattern(loweredPattern.Syntax, loweredInput, loweredTarget, loweredInput.Type.CanContainNull()));
		}
		return MakeIsDeclarationPattern(loweredPattern.Syntax, loweredInput, loweredPattern.VariableAccess, loweredInput.Type.CanContainNull());
	}

	private bool IsIrrefutablePatternTest(BoundExpression test)
	{
		while (true)
		{
			switch (test.Kind)
			{
			case BoundKind.Literal:
			{
				ConstantValue constantValue = ((BoundLiteral)test).ConstantValue;
				if (constantValue.IsBoolean)
				{
					return constantValue.BooleanValue;
				}
				return false;
			}
			case BoundKind.Sequence:
				break;
			default:
				return false;
			}
			test = ((BoundSequence)test).Value;
		}
	}

	private BoundExpression CompareWithConstant(BoundExpression input, BoundExpression boundConstant)
	{
		NamedTypeSymbol namedTypeSymbol = _factory.SpecialType(SpecialType.System_Object);
		if (boundConstant.ConstantValue == ConstantValue.Null)
		{
			if (input.Type.IsNonNullableValueType())
			{
				NamedTypeSymbol returnType = _factory.SpecialType(SpecialType.System_Boolean);
				return RewriteNullableNullEquality(_factory.Syntax, BinaryOperatorKind.NullableNullEqual, input, boundConstant, returnType);
			}
			return _factory.ObjectEqual(_factory.Convert(namedTypeSymbol, input), boundConstant);
		}
		if (input.Type.IsNullableType() && boundConstant.NullableNeverHasValue())
		{
			return _factory.Not(MakeNullableHasValue(_factory.Syntax, input));
		}
		return _factory.StaticCall(namedTypeSymbol, "Equals", _factory.Convert(namedTypeSymbol, boundConstant), _factory.Convert(namedTypeSymbol, input));
	}

	private bool? MatchConstantValue(BoundExpression source, TypeSymbol targetType, bool requiredNullTest)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		TypeSymbol typeSymbol = (source.Type.IsDynamic() ? _compilation.GetSpecialType(SpecialType.System_Object) : source.Type);
		ConversionKind kind = _compilation.Conversions.ClassifyBuiltInConversion(typeSymbol, targetType, ref useSiteDiagnostics).Kind;
		ConstantValue isOperatorConstantResult = Binder.GetIsOperatorConstantResult(typeSymbol, targetType, kind, source.ConstantValue, requiredNullTest);
		if (!(isOperatorConstantResult == ConstantValue.True))
		{
			if (!(isOperatorConstantResult == ConstantValue.False))
			{
				if (!(isOperatorConstantResult == null))
				{
					throw ExceptionUtilities.UnexpectedValue(isOperatorConstantResult);
				}
				return null;
			}
			return false;
		}
		return true;
	}

	private BoundExpression MakeIsDeclarationPattern(SyntaxNode syntax, BoundExpression loweredInput, BoundExpression loweredTarget, bool requiresNullTest)
	{
		TypeSymbol type = loweredTarget.Type;
		requiresNullTest = requiresNullTest && loweredInput.Type.CanContainNull();
		bool? flag = MatchConstantValue(loweredInput, type, requiredNullTest: false);
		if (flag == false)
		{
			return _factory.MakeSequence(loweredInput, _factory.Literal(value: false));
		}
		if (flag == true)
		{
			requiresNullTest = requiresNullTest && MatchConstantValue(loweredInput, type, requiredNullTest: true) != true;
			if (loweredInput.Type.IsNullableType())
			{
				MethodSymbol method = _factory.SpecialMethod(SpecialMember.System_Nullable_T_GetValueOrDefault).AsMember((NamedTypeSymbol)loweredInput.Type);
				if (requiresNullTest)
				{
					LocalSymbol localSymbol = _factory.SynthesizedLocal(loweredInput.Type, syntax);
					MethodSymbol method2 = _factory.SpecialMethod(SpecialMember.System_Nullable_T_get_HasValue).AsMember((NamedTypeSymbol)loweredInput.Type);
					return _factory.MakeSequence(localSymbol, _factory.AssignmentExpression(_factory.Local(localSymbol), loweredInput), _factory.AssignmentExpression(loweredTarget, _factory.Convert(type, _factory.Call(_factory.Local(localSymbol), method))), _factory.Call(_factory.Local(localSymbol), method2));
				}
				BoundExpression right = _factory.Convert(type, _factory.Call(loweredInput, method));
				BoundAssignmentOperator boundAssignmentOperator = _factory.AssignmentExpression(loweredTarget, right);
				return _factory.MakeSequence(boundAssignmentOperator, _factory.Literal(value: true));
			}
			BoundExpression right2 = _factory.Convert(type, loweredInput);
			BoundAssignmentOperator boundAssignmentOperator2 = _factory.AssignmentExpression(loweredTarget, right2);
			NamedTypeSymbol type2 = _factory.SpecialType(SpecialType.System_Object);
			if (!requiresNullTest)
			{
				return _factory.MakeSequence(boundAssignmentOperator2, _factory.Literal(value: true));
			}
			return _factory.ObjectNotEqual(_factory.Convert(type2, boundAssignmentOperator2), _factory.Null(type2));
		}
		if (type.IsReferenceType)
		{
			return _factory.ObjectNotEqual(_factory.AssignmentExpression(loweredTarget, _factory.As(loweredInput, type)), _factory.Null(type));
		}
		LocalSymbol localSymbol2 = _factory.SynthesizedLocal(loweredInput.Type, syntax);
		BoundExpression right3 = _factory.Convert(type, _factory.Convert(_factory.SpecialType(SpecialType.System_Object), _factory.Local(localSymbol2)));
		return _factory.MakeSequence(localSymbol2, _factory.LogicalAnd(_factory.Is(_factory.AssignmentExpression(_factory.Local(localSymbol2), loweredInput), type), _factory.MakeSequence(_factory.AssignmentExpression(loweredTarget, right3), _factory.Literal(value: true))));
	}

	public override BoundNode VisitPatternSwitchStatement(BoundPatternSwitchStatement node)
	{
		return PatternSwitchLocalRewriter.MakeLoweredForm(this, node);
	}

	public override BoundNode VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		BoundExpression rewrittenExpression = LowerReceiverOfPointerElementAccess(node.Expression);
		BoundExpression rewrittenIndex = VisitExpression(node.Index);
		return RewritePointerElementAccess(node, rewrittenExpression, rewrittenIndex);
	}

	private BoundExpression LowerReceiverOfPointerElementAccess(BoundExpression receiver)
	{
		if (receiver is BoundFieldAccess boundFieldAccess && boundFieldAccess.FieldSymbol.IsFixed)
		{
			BoundExpression receiver2 = VisitExpression(boundFieldAccess.ReceiverOpt);
			BoundFieldAccess boundFieldAccess2 = boundFieldAccess.Update(receiver2, boundFieldAccess.FieldSymbol, boundFieldAccess.ConstantValueOpt, boundFieldAccess.ResultKind, boundFieldAccess.Type);
			return new BoundAddressOfOperator(receiver.Syntax, boundFieldAccess2, isManaged: true, boundFieldAccess2.Type);
		}
		return VisitExpression(receiver);
	}

	private BoundExpression RewritePointerElementAccess(BoundPointerElementAccess node, BoundExpression rewrittenExpression, BoundExpression rewrittenIndex)
	{
		if (rewrittenIndex.IsDefaultValue())
		{
			return new BoundPointerIndirectionOperator(node.Syntax, rewrittenExpression, node.Type);
		}
		BinaryOperatorKind binaryOperatorKind = BinaryOperatorKind.Addition;
		binaryOperatorKind = rewrittenIndex.Type.SpecialType switch
		{
			SpecialType.System_Int32 => binaryOperatorKind | BinaryOperatorKind.PointerAndIntAddition, 
			SpecialType.System_UInt32 => binaryOperatorKind | BinaryOperatorKind.PointerAndUIntAddition, 
			SpecialType.System_Int64 => binaryOperatorKind | BinaryOperatorKind.PointerAndLongAddition, 
			SpecialType.System_UInt64 => binaryOperatorKind | BinaryOperatorKind.PointerAndULongAddition, 
			_ => throw ExceptionUtilities.UnexpectedValue(rewrittenIndex.Type.SpecialType), 
		};
		if (node.Checked)
		{
			binaryOperatorKind |= BinaryOperatorKind.Checked;
		}
		return new BoundPointerIndirectionOperator(node.Syntax, MakeBinaryOperator(node.Syntax, binaryOperatorKind, rewrittenExpression, rewrittenIndex, rewrittenExpression.Type, null, isPointerElementAccess: true), node.Type);
	}

	public override BoundNode VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node)
	{
		ImplicitNamedTypeSymbol previousSubmissionType = (ImplicitNamedTypeSymbol)node.Type;
		SyntaxNode syntax = node.Syntax;
		FieldSymbol orMakeField = _previousSubmissionFields.GetOrMakeField(previousSubmissionType);
		BoundThisReference receiver = new BoundThisReference(syntax, _factory.CurrentType);
		return new BoundFieldAccess(syntax, receiver, orMakeField, null);
	}

	public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		return VisitPropertyAccess(node, isLeftOfAssignment: false);
	}

	private BoundExpression VisitPropertyAccess(BoundPropertyAccess node, bool isLeftOfAssignment)
	{
		BoundExpression rewrittenReceiverOpt = VisitExpression(node.ReceiverOpt);
		return MakePropertyAccess(node.Syntax, rewrittenReceiverOpt, node.PropertySymbol, node.ResultKind, node.Type, isLeftOfAssignment, node);
	}

	private BoundExpression MakePropertyAccess(SyntaxNode syntax, BoundExpression rewrittenReceiverOpt, PropertySymbol propertySymbol, LookupResultKind resultKind, TypeSymbol type, bool isLeftOfAssignment, BoundPropertyAccess oldNodeOpt = null)
	{
		if (rewrittenReceiverOpt != null && rewrittenReceiverOpt.Type.IsArray() && !isLeftOfAssignment && ((ArrayTypeSymbol)rewrittenReceiverOpt.Type).IsSZArray && ((object)propertySymbol == _compilation.GetSpecialTypeMember(SpecialMember.System_Array__Length) || (!_inExpressionLambda && (object)propertySymbol == _compilation.GetSpecialTypeMember(SpecialMember.System_Array__LongLength))))
		{
			return new BoundArrayLength(syntax, rewrittenReceiverOpt, type);
		}
		if (isLeftOfAssignment && propertySymbol.RefKind == RefKind.None)
		{
			if (oldNodeOpt == null)
			{
				return new BoundPropertyAccess(syntax, rewrittenReceiverOpt, propertySymbol, resultKind, type);
			}
			return oldNodeOpt.Update(rewrittenReceiverOpt, propertySymbol, resultKind, type);
		}
		return MakePropertyGetAccess(syntax, rewrittenReceiverOpt, propertySymbol, oldNodeOpt);
	}

	private BoundExpression MakePropertyGetAccess(SyntaxNode syntax, BoundExpression rewrittenReceiver, PropertySymbol property, BoundPropertyAccess oldNodeOpt)
	{
		return MakePropertyGetAccess(syntax, rewrittenReceiver, property, ImmutableArray<BoundExpression>.Empty, null, oldNodeOpt);
	}

	private BoundExpression MakePropertyGetAccess(SyntaxNode syntax, BoundExpression rewrittenReceiver, PropertySymbol property, ImmutableArray<BoundExpression> rewrittenArguments, MethodSymbol getMethodOpt = null, BoundPropertyAccess oldNodeOpt = null)
	{
		if (_inExpressionLambda && rewrittenArguments.IsEmpty)
		{
			if (oldNodeOpt == null)
			{
				return new BoundPropertyAccess(syntax, rewrittenReceiver, property, LookupResultKind.Viable, property.Type);
			}
			return oldNodeOpt.Update(rewrittenReceiver, property, LookupResultKind.Viable, property.Type);
		}
		MethodSymbol method = getMethodOpt ?? property.GetOwnOrInheritedGetMethod();
		return BoundCall.Synthesized(syntax, rewrittenReceiver, method, rewrittenArguments);
	}

	public override BoundNode VisitRangeVariable(BoundRangeVariable node)
	{
		return VisitExpression(node.Value);
	}

	public override BoundNode VisitQueryClause(BoundQueryClause node)
	{
		return VisitExpression(node.Value);
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)base.VisitReturnStatement(node);
		bool num;
		if (Instrument)
		{
			if (!node.WasCompilerGenerated)
			{
				goto IL_005e;
			}
			if (node.ExpressionOpt != null)
			{
				num = IsLambdaOrExpressionBodiedMember;
				goto IL_005c;
			}
			if (node.Syntax.Kind() == SyntaxKind.Block)
			{
				MethodSymbol currentFunction = _factory.CurrentFunction;
				if ((object)currentFunction != null)
				{
					num = !currentFunction.IsAsync;
					goto IL_005c;
				}
			}
		}
		goto IL_006c;
		IL_006c:
		return boundStatement;
		IL_005e:
		boundStatement = _instrumenter.InstrumentReturnStatement(node, boundStatement);
		goto IL_006c;
		IL_005c:
		if (num)
		{
			goto IL_005e;
		}
		goto IL_006c;
	}

	public override BoundNode VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression stackAllocNode)
	{
		return VisitStackAllocArrayCreation(stackAllocNode);
	}

	public override BoundNode VisitStackAllocArrayCreation(BoundStackAllocArrayCreation stackAllocNode)
	{
		BoundExpression boundExpression = VisitExpression(stackAllocNode.Count);
		TypeSymbol type = stackAllocNode.Type;
		ConstantValue constantValue = boundExpression.ConstantValue;
		if ((object)constantValue != null && constantValue.Int32Value == 0)
		{
			return _factory.Default(type);
		}
		TypeSymbol elementType = stackAllocNode.ElementType;
		BoundArrayInitialization boundArrayInitialization = stackAllocNode.InitializerOpt;
		if (boundArrayInitialization != null)
		{
			boundArrayInitialization = boundArrayInitialization.Update(VisitList(boundArrayInitialization.Initializers));
		}
		if (type.IsPointerType())
		{
			BoundExpression count = RewriteStackAllocCountToSize(boundExpression, elementType);
			return new BoundConvertedStackAllocExpression(stackAllocNode.Syntax, elementType, count, boundArrayInitialization, stackAllocNode.Type);
		}
		if (type.OriginalDefinition == _compilation.GetWellKnownType(WellKnownType.System_Span_T))
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)stackAllocNode.Type;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
			BoundExpression boundExpression2 = CaptureExpressionInTempIfNeeded(boundExpression, instance, instance2);
			BoundExpression count2 = RewriteStackAllocCountToSize(boundExpression2, elementType);
			stackAllocNode = new BoundConvertedStackAllocExpression(stackAllocNode.Syntax, elementType, count2, boundArrayInitialization, namedTypeSymbol);
			MethodSymbol symbol;
			return new BoundSequence(value: (!TryGetWellKnownTypeMember<MethodSymbol>(stackAllocNode.Syntax, WellKnownMember.System_Span_T__ctor, out symbol)) ? ((BoundExpression)new BoundBadExpression(stackAllocNode.Syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)_factory.New((MethodSymbol)symbol.SymbolAsMember(namedTypeSymbol), stackAllocNode, boundExpression2)), syntax: stackAllocNode.Syntax, locals: instance2.ToImmutableAndFree(), sideEffects: instance.ToImmutableAndFree(), type: namedTypeSymbol);
		}
		throw ExceptionUtilities.UnexpectedValue(type);
	}

	private BoundExpression RewriteStackAllocCountToSize(BoundExpression countExpression, TypeSymbol elementType)
	{
		TypeSymbol type = _factory.SpecialType(SpecialType.System_UInt32);
		TypeSymbol type2 = _factory.SpecialType(SpecialType.System_UIntPtr);
		BoundExpression boundExpression = _factory.Sizeof(elementType);
		ConstantValue constantValue = boundExpression.ConstantValue;
		if (constantValue != null)
		{
			int int32Value = constantValue.Int32Value;
			ConstantValue constantValue2 = countExpression.ConstantValue;
			if (constantValue2 != null)
			{
				long num = (uint)constantValue2.Int32Value * int32Value;
				if (num < uint.MaxValue)
				{
					return _factory.Convert(type2, _factory.Literal((uint)num), Conversion.IntegerToPointer);
				}
			}
		}
		BoundExpression arg = _factory.Convert(type, countExpression, Conversion.ExplicitNumeric);
		arg = _factory.Convert(type2, arg, Conversion.IntegerToPointer);
		if ((object)constantValue != null && constantValue.Int32Value == 1)
		{
			return arg;
		}
		BinaryOperatorKind kind = BinaryOperatorKind.UIntMultiplication | BinaryOperatorKind.Checked;
		return _factory.Binary(kind, type2, arg, boundExpression);
	}

	private BoundExpression RewriteStringConcatenation(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type)
	{
		if (_inExpressionLambda)
		{
			return RewriteStringConcatInExpressionLambda(syntax, operatorKind, loweredLeft, loweredRight, type);
		}
		loweredLeft = ConvertConcatExprToStringIfPossible(syntax, loweredLeft);
		loweredRight = ConvertConcatExprToStringIfPossible(syntax, loweredRight);
		BoundExpression boundExpression = TryFoldTwoConcatOperands(syntax, loweredLeft, loweredRight);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		FlattenConcatArg(loweredLeft, instance);
		FlattenConcatArg(loweredRight, instance2);
		if (instance.Any() && instance2.Any())
		{
			boundExpression = TryFoldTwoConcatOperands(syntax, instance.Last(), instance2.First());
			if (boundExpression != null)
			{
				instance2[0] = boundExpression;
				instance.RemoveLast();
			}
		}
		instance.AddRange(instance2);
		instance2.Free();
		BoundExpression result;
		switch (instance.Count)
		{
		case 0:
			result = _factory.StringLiteral(string.Empty);
			break;
		case 1:
			result = instance[0];
			break;
		case 2:
		{
			BoundExpression loweredLeft2 = instance[0];
			BoundExpression loweredRight2 = instance[1];
			result = RewriteStringConcatenationTwoExprs(syntax, loweredLeft2, loweredRight2);
			break;
		}
		case 3:
		{
			BoundExpression loweredFirst = instance[0];
			BoundExpression loweredSecond = instance[1];
			BoundExpression loweredThird = instance[2];
			result = RewriteStringConcatenationThreeExprs(syntax, loweredFirst, loweredSecond, loweredThird);
			break;
		}
		default:
			result = RewriteStringConcatenationManyExprs(syntax, instance.ToImmutable());
			break;
		}
		instance.Free();
		return result;
	}

	private void FlattenConcatArg(BoundExpression lowered, ArrayBuilder<BoundExpression> flattened)
	{
		switch (lowered.Kind)
		{
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)lowered;
			MethodSymbol method = boundCall.Method;
			if (method.IsStatic && method.ContainingType.SpecialType == SpecialType.System_String)
			{
				if ((object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringString) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringStringString) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringStringStringString) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObject) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObjectObject) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObjectObjectObject))
				{
					flattened.AddRange(boundCall.Arguments);
					return;
				}
				if (((object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatStringArray) || (object)method == _compilation.GetSpecialTypeMember(SpecialMember.System_String__ConcatObjectArray)) && boundCall.Arguments[0] is BoundArrayCreation { InitializerOpt: { } initializerOpt })
				{
					flattened.AddRange(initializerOpt.Initializers);
					return;
				}
			}
			break;
		}
		case BoundKind.NullCoalescingOperator:
		{
			BoundNullCoalescingOperator boundNullCoalescingOperator = (BoundNullCoalescingOperator)lowered;
			if (boundNullCoalescingOperator.LeftConversion.IsIdentity)
			{
				ConstantValue constantValue = boundNullCoalescingOperator.RightOperand.ConstantValue;
				if (constantValue != null && constantValue.IsString && constantValue.StringValue.Length == 0)
				{
					flattened.Add(boundNullCoalescingOperator.LeftOperand);
					return;
				}
			}
			break;
		}
		}
		flattened.Add(lowered);
	}

	private BoundExpression TryFoldTwoConcatOperands(SyntaxNode syntax, BoundExpression loweredLeft, BoundExpression loweredRight)
	{
		ConstantValue constantValue = loweredLeft.ConstantValue;
		ConstantValue constantValue2 = loweredRight.ConstantValue;
		if (constantValue != null && constantValue2 != null)
		{
			ConstantValue constantValue3 = TryFoldTwoConcatConsts(constantValue, constantValue2);
			if (constantValue3 != null)
			{
				return _factory.StringLiteral(constantValue3);
			}
		}
		if (IsNullOrEmptyStringConstant(loweredLeft))
		{
			if (IsNullOrEmptyStringConstant(loweredRight))
			{
				return _factory.Literal("");
			}
			return RewriteStringConcatenationOneExpr(syntax, loweredRight);
		}
		if (IsNullOrEmptyStringConstant(loweredRight))
		{
			return RewriteStringConcatenationOneExpr(syntax, loweredLeft);
		}
		return null;
	}

	private static bool IsNullOrEmptyStringConstant(BoundExpression operand)
	{
		if (!(operand.ConstantValue != null) || !string.IsNullOrEmpty(operand.ConstantValue.StringValue))
		{
			return operand.IsDefaultValue();
		}
		return true;
	}

	private static ConstantValue TryFoldTwoConcatConsts(ConstantValue leftConst, ConstantValue rightConst)
	{
		string stringValue = leftConst.StringValue;
		string stringValue2 = rightConst.StringValue;
		if (!leftConst.IsDefaultValue && !rightConst.IsDefaultValue && stringValue.Length + stringValue2.Length < 0)
		{
			return null;
		}
		return ConstantValue.Create(stringValue + stringValue2);
	}

	private BoundExpression RewriteStringConcatenationOneExpr(SyntaxNode syntax, BoundExpression loweredOperand)
	{
		if (loweredOperand.Type.SpecialType == SpecialType.System_String)
		{
			return _factory.Coalesce(loweredOperand, _factory.Literal(""));
		}
		MethodSymbol method = UnsafeGetSpecialTypeMethod(syntax, SpecialMember.System_String__ConcatObject);
		return BoundCall.Synthesized(syntax, null, method, loweredOperand);
	}

	private BoundExpression RewriteStringConcatenationTwoExprs(SyntaxNode syntax, BoundExpression loweredLeft, BoundExpression loweredRight)
	{
		SpecialMember specialMember = ((loweredLeft.Type.SpecialType == SpecialType.System_String && loweredRight.Type.SpecialType == SpecialType.System_String) ? SpecialMember.System_String__ConcatStringString : SpecialMember.System_String__ConcatObjectObject);
		MethodSymbol method = UnsafeGetSpecialTypeMethod(syntax, specialMember);
		return BoundCall.Synthesized(syntax, null, method, loweredLeft, loweredRight);
	}

	private BoundExpression RewriteStringConcatenationThreeExprs(SyntaxNode syntax, BoundExpression loweredFirst, BoundExpression loweredSecond, BoundExpression loweredThird)
	{
		SpecialMember specialMember = ((loweredFirst.Type.SpecialType == SpecialType.System_String && loweredSecond.Type.SpecialType == SpecialType.System_String && loweredThird.Type.SpecialType == SpecialType.System_String) ? SpecialMember.System_String__ConcatStringStringString : SpecialMember.System_String__ConcatObjectObjectObject);
		MethodSymbol method = UnsafeGetSpecialTypeMethod(syntax, specialMember);
		return BoundCall.Synthesized(syntax, null, method, ImmutableArray.Create(loweredFirst, loweredSecond, loweredThird));
	}

	private BoundExpression RewriteStringConcatenationManyExprs(SyntaxNode syntax, ImmutableArray<BoundExpression> loweredArgs)
	{
		bool flag = false;
		TypeSymbol typeSymbol = null;
		foreach (BoundExpression item in loweredArgs)
		{
			typeSymbol = item.Type;
			if (typeSymbol.SpecialType != SpecialType.System_String)
			{
				flag = true;
				break;
			}
		}
		if (!flag && loweredArgs.Length == 4)
		{
			SpecialMember specialMember = SpecialMember.System_String__ConcatStringStringStringString;
			MethodSymbol method = UnsafeGetSpecialTypeMethod(syntax, specialMember);
			return BoundCall.Synthesized(syntax, null, method, loweredArgs);
		}
		SpecialMember specialMember2 = (flag ? SpecialMember.System_String__ConcatObjectArray : SpecialMember.System_String__ConcatStringArray);
		MethodSymbol method2 = UnsafeGetSpecialTypeMethod(syntax, specialMember2);
		BoundExpression arg = _factory.ArrayOrEmpty(typeSymbol, loweredArgs);
		return BoundCall.Synthesized(syntax, null, method2, arg);
	}

	private BoundExpression RewriteStringConcatInExpressionLambda(SyntaxNode syntax, BinaryOperatorKind operatorKind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol type)
	{
		SpecialMember specialMember = ((operatorKind == BinaryOperatorKind.StringConcatenation) ? SpecialMember.System_String__ConcatStringString : SpecialMember.System_String__ConcatObjectObject);
		MethodSymbol methodOpt = UnsafeGetSpecialTypeMethod(syntax, specialMember);
		return new BoundBinaryOperator(syntax, operatorKind, loweredLeft, loweredRight, null, methodOpt, LookupResultKind.Empty, type);
	}

	private BoundExpression ConvertConcatExprToStringIfPossible(SyntaxNode syntax, BoundExpression expr)
	{
		if (expr.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			if (boundConversion.ConversionKind == ConversionKind.Boxing)
			{
				BoundExpression operand = boundConversion.Operand;
				if (operand != null)
				{
					if (operand.Kind == BoundKind.Literal)
					{
						ConstantValue constantValue = ((BoundLiteral)operand).ConstantValue;
						if (constantValue != null && constantValue.SpecialType == SpecialType.System_Char)
						{
							return _factory.StringLiteral(constantValue.CharValue.ToString());
						}
					}
					if (ConcatExprCanBeOptimizedWithToString(operand.Type))
					{
						MethodSymbol methodSymbol = UnsafeGetSpecialTypeMethod(syntax, SpecialMember.System_Object__ToString);
						NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)operand.Type;
						foreach (Symbol member in namedTypeSymbol.GetMembers(methodSymbol.Name))
						{
							MethodSymbol methodSymbol2 = member as MethodSymbol;
							if ((object)methodSymbol2.GetLeastOverriddenMethod(namedTypeSymbol) == methodSymbol)
							{
								return BoundCall.Synthesized(syntax, operand, methodSymbol2);
							}
						}
					}
				}
			}
		}
		return expr;
	}

	private static bool ConcatExprCanBeOptimizedWithToString(TypeSymbol symbol)
	{
		SpecialType specialType = symbol.SpecialType;
		if ((uint)(specialType - 7) <= 1u || (uint)(specialType - 21) <= 1u)
		{
			return true;
		}
		return false;
	}

	private BoundExpression RewriteInterpolatedStringConversion(BoundConversion conversion)
	{
		MakeInterpolatedStringFormat((BoundInterpolatedString)conversion.Operand, out var format, out var expressions);
		expressions.Insert(0, format);
		NamedTypeSymbol receiver = _factory.WellKnownType(WellKnownType.System_Runtime_CompilerServices_FormattableStringFactory);
		BoundExpression boundExpression = _factory.StaticCall(receiver, "Create", expressions.ToImmutableAndFree(), allowUnexpandedForm: false);
		if (!boundExpression.HasAnyErrors)
		{
			boundExpression = VisitExpression(boundExpression);
			boundExpression = MakeImplicitConversion(boundExpression, conversion.Type);
		}
		return boundExpression;
	}

	private bool CanLowerToStringConcatenation(BoundInterpolatedString node)
	{
		foreach (BoundExpression part in node.Parts)
		{
			if (!(part is BoundStringInsert boundStringInsert))
			{
				continue;
			}
			if (!_inExpressionLambda && !boundStringInsert.HasErrors)
			{
				TypeSymbol type = boundStringInsert.Value.Type;
				if ((object)type != null && type.SpecialType == SpecialType.System_String && boundStringInsert.Alignment == null && boundStringInsert.Format == null)
				{
					continue;
				}
			}
			return false;
		}
		return true;
	}

	private static string Unescape(string s)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		int length = s.Length;
		for (int i = 0; i < length; i++)
		{
			char c = s[i];
			builder.Append(c);
			if ((c == '{' || c == '}') && i + 1 < length && s[i + 1] == c)
			{
				i++;
			}
		}
		return instance.ToStringAndFree();
	}

	private void MakeInterpolatedStringFormat(BoundInterpolatedString node, out BoundExpression format, out ArrayBuilder<BoundExpression> expressions)
	{
		_factory.Syntax = node.Syntax;
		int num = node.Parts.Length - 1;
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		expressions = ArrayBuilder<BoundExpression>.GetInstance(num + 1);
		int num2 = 0;
		for (int i = 0; i <= num; i++)
		{
			BoundExpression boundExpression = node.Parts[i];
			if (!(boundExpression is BoundStringInsert boundStringInsert))
			{
				builder.Append(boundExpression.ConstantValue.StringValue);
				continue;
			}
			builder.Append('{').Append(num2++);
			if (boundStringInsert.Alignment != null && !boundStringInsert.Alignment.HasErrors)
			{
				builder.Append(',').Append(boundStringInsert.Alignment.ConstantValue.Int64Value);
			}
			if (boundStringInsert.Format != null && !boundStringInsert.Format.HasErrors)
			{
				builder.Append(':').Append(boundStringInsert.Format.ConstantValue.StringValue);
			}
			builder.Append('}');
			BoundExpression boundExpression2 = boundStringInsert.Value;
			TypeSymbol type = boundExpression2.Type;
			if ((object)type != null && type.TypeKind == TypeKind.Dynamic)
			{
				boundExpression2 = MakeConversionNode(boundExpression2, _compilation.ObjectType, @checked: false);
			}
			expressions.Add(boundExpression2);
		}
		format = _factory.StringLiteral(instance.ToStringAndFree());
	}

	public override BoundNode VisitInterpolatedString(BoundInterpolatedString node)
	{
		BoundExpression boundExpression;
		if (CanLowerToStringConcatenation(node))
		{
			int length = node.Parts.Length;
			if (length == 0)
			{
				return _factory.StringLiteral("");
			}
			boundExpression = null;
			for (int i = 0; i < length; i++)
			{
				BoundExpression boundExpression2 = node.Parts[i];
				boundExpression2 = ((!(boundExpression2 is BoundStringInsert boundStringInsert)) ? _factory.StringLiteral(Unescape(boundExpression2.ConstantValue.StringValue)) : boundStringInsert.Value);
				boundExpression = ((boundExpression == null) ? boundExpression2 : _factory.Binary(BinaryOperatorKind.StringConcatenation, node.Type, boundExpression, boundExpression2));
			}
			if (length == 1)
			{
				boundExpression = _factory.Coalesce(boundExpression, _factory.StringLiteral(""));
			}
		}
		else
		{
			MakeInterpolatedStringFormat(node, out var format, out var expressions);
			expressions.Insert(0, format);
			TypeSymbol type = node.Type;
			boundExpression = _factory.StaticCall(type, "Format", expressions.ToImmutableAndFree(), allowUnexpandedForm: false);
		}
		if (!boundExpression.HasAnyErrors)
		{
			boundExpression = VisitExpression(boundExpression);
			boundExpression = MakeImplicitConversion(boundExpression, node.Type);
		}
		return boundExpression;
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		SyntaxNode syntax = node.Syntax;
		BoundExpression rewrittenExpression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundSwitchSection> rewrittenSections = VisitSwitchSections(node.SwitchSections);
		if (!node.WasCompilerGenerated && Instrument)
		{
			rewrittenExpression = _instrumenter.InstrumentSwitchStatementExpression(node, rewrittenExpression, _factory);
		}
		BoundStatement boundStatement = MakeSwitchStatement(syntax, rewrittenExpression, rewrittenSections, node.ConstantTargetOpt, node.InnerLocals, node.InnerLocalFunctions, node.BreakLabel, node);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = _instrumenter.InstrumentSwitchStatement(node, boundStatement);
		}
		return boundStatement;
	}

	private BoundStatement MakeSwitchStatement(SyntaxNode syntax, BoundExpression rewrittenExpression, ImmutableArray<BoundSwitchSection> rewrittenSections, LabelSymbol constantTargetOpt, ImmutableArray<LocalSymbol> locals, ImmutableArray<LocalFunctionSymbol> localFunctions, GeneratedLabelSymbol breakLabel, BoundSwitchStatement oldNode)
	{
		if (!rewrittenExpression.Type.IsNullableType())
		{
			return MakeSwitchStatementWithNonNullableExpression(syntax, null, rewrittenExpression, rewrittenSections, constantTargetOpt, locals, localFunctions, breakLabel, oldNode);
		}
		return MakeSwitchStatementWithNullableExpression(syntax, rewrittenExpression, rewrittenSections, constantTargetOpt, locals, localFunctions, breakLabel, oldNode);
	}

	private BoundStatement MakeSwitchStatementWithNonNullableExpression(SyntaxNode syntax, BoundStatement preambleOpt, BoundExpression rewrittenExpression, ImmutableArray<BoundSwitchSection> rewrittenSections, LabelSymbol constantTargetOpt, ImmutableArray<LocalSymbol> locals, ImmutableArray<LocalFunctionSymbol> localFunctions, GeneratedLabelSymbol breakLabel, BoundSwitchStatement oldNode)
	{
		MethodSymbol stringEquality = null;
		if (rewrittenExpression.Type.SpecialType == SpecialType.System_String)
		{
			EnsureStringHashFunction(rewrittenSections, syntax);
			stringEquality = UnsafeGetSpecialTypeMethod(syntax, SpecialMember.System_String__op_Equality);
		}
		return oldNode.Update(preambleOpt, rewrittenExpression, constantTargetOpt, locals, localFunctions, rewrittenSections, breakLabel, stringEquality);
	}

	private BoundStatement MakeSwitchStatementWithNullableExpression(SyntaxNode syntax, BoundExpression rewrittenExpression, ImmutableArray<BoundSwitchSection> rewrittenSections, LabelSymbol constantTargetOpt, ImmutableArray<LocalSymbol> locals, ImmutableArray<LocalFunctionSymbol> localFunctions, GeneratedLabelSymbol breakLabel, BoundSwitchStatement oldNode)
	{
		SyntaxNode syntax2 = rewrittenExpression.Syntax;
		TypeSymbol type = rewrittenExpression.Type;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		LocalSymbol localSymbol;
		if (rewrittenExpression.Kind != BoundKind.Local)
		{
			BoundLocal boundLocal = _factory.StoreToTemp(rewrittenExpression, out var store);
			BoundExpressionStatement item = new BoundExpressionStatement(syntax2, store);
			instance.Add(item);
			localSymbol = boundLocal.LocalSymbol;
			rewrittenExpression = boundLocal;
		}
		else
		{
			localSymbol = null;
		}
		BoundStatement preambleOpt = new BoundConditionalGoto(syntax2, MakeNullCheck(syntax2, rewrittenExpression, BinaryOperatorKind.NullableNullEqual), jumpIfTrue: true, GetNullValueTargetSwitchLabel(rewrittenSections, breakLabel));
		MethodSymbol method = UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		rewrittenExpression = BoundCall.Synthesized(syntax2, rewrittenExpression, method);
		BoundStatement item2 = MakeSwitchStatementWithNonNullableExpression(syntax, preambleOpt, rewrittenExpression, rewrittenSections, constantTargetOpt, locals, localFunctions, breakLabel, oldNode);
		instance.Add(item2);
		return new BoundBlock(syntax, ((object)localSymbol == null) ? ImmutableArray<LocalSymbol>.Empty : ImmutableArray.Create(localSymbol), instance.ToImmutableAndFree());
	}

	private static LabelSymbol GetNullValueTargetSwitchLabel(ImmutableArray<BoundSwitchSection> sections, GeneratedLabelSymbol breakLabel)
	{
		LabelSymbol result = breakLabel;
		foreach (BoundSwitchSection item in sections)
		{
			foreach (BoundSwitchLabel switchLabel in item.SwitchLabels)
			{
				SourceLabelSymbol sourceLabelSymbol = (SourceLabelSymbol)switchLabel.Label;
				ConstantValue constantValueOpt = switchLabel.ConstantValueOpt;
				if (constantValueOpt == ConstantValue.Null)
				{
					return sourceLabelSymbol;
				}
				if (constantValueOpt == null)
				{
					result = sourceLabelSymbol;
				}
			}
		}
		return result;
	}

	private ImmutableArray<BoundSwitchSection> VisitSwitchSections(ImmutableArray<BoundSwitchSection> sections)
	{
		if (sections.Length > 0)
		{
			ArrayBuilder<BoundSwitchSection> instance = ArrayBuilder<BoundSwitchSection>.GetInstance();
			foreach (BoundSwitchSection item in sections)
			{
				instance.Add((BoundSwitchSection)VisitSwitchSection(item));
			}
			return instance.ToImmutableAndFree();
		}
		return ImmutableArray<BoundSwitchSection>.Empty;
	}

	public override BoundNode VisitSwitchSection(BoundSwitchSection node)
	{
		return node.Update(node.Locals, VisitList(node.SwitchLabels), VisitList(node.Statements));
	}

	private static int CountLabels(ImmutableArray<BoundSwitchSection> rewrittenSections)
	{
		int num = 0;
		foreach (BoundSwitchSection item in rewrittenSections)
		{
			foreach (BoundSwitchLabel switchLabel in item.SwitchLabels)
			{
				if (switchLabel.ConstantValueOpt != null)
				{
					_ = switchLabel.ConstantValueOpt;
					num++;
				}
			}
		}
		return num;
	}

	private void EnsureStringHashFunction(ImmutableArray<BoundSwitchSection> rewrittenSections, SyntaxNode syntaxNode)
	{
		PEModuleBuilder emitModule = EmitModule;
		if (emitModule == null)
		{
			return;
		}
		int labelsCount = CountLabels(rewrittenSections);
		if (!SwitchStringJumpTableEmitter.ShouldGenerateHashTableSwitch(emitModule, labelsCount))
		{
			return;
		}
		PrivateImplementationDetails privateImplClass = emitModule.GetPrivateImplClass(syntaxNode, _diagnostics);
		if (privateImplClass.GetMethod("ComputeStringHash") == null)
		{
			Symbol specialTypeMember = _compilation.GetSpecialTypeMember(SpecialMember.System_String__Chars);
			if ((object)specialTypeMember != null && specialTypeMember.GetUseSiteDiagnostic() == null)
			{
				TypeSymbol returnType = _factory.SpecialType(SpecialType.System_UInt32);
				TypeSymbol paramType = _factory.SpecialType(SpecialType.System_String);
				SynthesizedStringSwitchHashMethod method = new SynthesizedStringSwitchHashMethod(emitModule.SourceModule, privateImplClass, returnType, paramType);
				privateImplClass.TryAddSynthesizedMethod(method);
			}
		}
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)base.VisitThrowStatement(node);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = _instrumenter.InstrumentThrowStatement(node, boundStatement);
		}
		return boundStatement;
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		BoundBlock boundBlock = (BoundBlock)Visit(node.TryBlock);
		bool sawAwait = _sawAwait;
		_sawAwait = false;
		bool flag = _compilation.Options.OptimizationLevel == OptimizationLevel.Release;
		ImmutableArray<BoundCatchBlock> catchBlocks = ((flag && !HasSideEffects(boundBlock)) ? ImmutableArray<BoundCatchBlock>.Empty : VisitList(node.CatchBlocks));
		BoundBlock boundBlock2 = (BoundBlock)Visit(node.FinallyBlockOpt);
		_sawAwaitInExceptionHandler |= _sawAwait;
		_sawAwait |= sawAwait;
		if (flag && !HasSideEffects(boundBlock2))
		{
			boundBlock2 = null;
		}
		if (!catchBlocks.IsDefaultOrEmpty || boundBlock2 != null)
		{
			return node.Update(boundBlock, catchBlocks, boundBlock2, node.PreferFaultHandler);
		}
		return boundBlock;
	}

	private static bool HasSideEffects(BoundStatement statement)
	{
		if (statement == null)
		{
			return false;
		}
		switch (statement.Kind)
		{
		case BoundKind.NoOpStatement:
			return false;
		case BoundKind.Block:
			foreach (BoundStatement statement2 in ((BoundBlock)statement).Statements)
			{
				if (HasSideEffects(statement2))
				{
					return true;
				}
			}
			return false;
		case BoundKind.SequencePoint:
			return HasSideEffects(((BoundSequencePoint)statement).StatementOpt);
		case BoundKind.SequencePointWithSpan:
			return HasSideEffects(((BoundSequencePointWithSpan)statement).StatementOpt);
		default:
			return true;
		}
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		if (node.ExceptionFilterOpt == null)
		{
			return base.VisitCatchBlock(node);
		}
		ConstantValue constantValue = node.ExceptionFilterOpt.ConstantValue;
		if ((object)constantValue != null && !constantValue.BooleanValue)
		{
			return null;
		}
		BoundExpression exceptionSourceOpt = (BoundExpression)Visit(node.ExceptionSourceOpt);
		BoundExpression boundExpression = (BoundExpression)Visit(node.ExceptionFilterOpt);
		BoundBlock body = (BoundBlock)Visit(node.Body);
		TypeSymbol exceptionTypeOpt = VisitType(node.ExceptionTypeOpt);
		if (boundExpression != null && !node.WasCompilerGenerated && Instrument)
		{
			boundExpression = _instrumenter.InstrumentCatchClauseFilter(node, boundExpression, _factory);
		}
		return node.Update(node.Locals, exceptionSourceOpt, exceptionTypeOpt, boundExpression, body, node.IsSynthesizedAsyncCatchAll);
	}

	public override BoundNode VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
	{
		TypeSymbol type = node.Type;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<LocalSymbol> instance2 = ArrayBuilder<LocalSymbol>.GetInstance();
		BoundExpression left = ReplaceTerminalElementsWithTemps(node.Left, node.Operators, instance, instance2);
		BoundExpression right = ReplaceTerminalElementsWithTemps(node.Right, node.Operators, instance, instance2);
		BoundExpression returnValue = RewriteTupleNestedOperators(node.Operators, left, right, type, instance2, node.OperatorKind);
		return MakeSequenceOrResultValue(instance2.ToImmutableAndFree(), instance.ToImmutableAndFree(), returnValue);
	}

	private BoundExpression MakeSequenceOrResultValue(ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundExpression> effects, BoundExpression returnValue)
	{
		if (locals.IsEmpty && effects.IsEmpty)
		{
			return returnValue;
		}
		return _factory.Sequence(locals, effects, returnValue);
	}

	private BoundExpression ReplaceTerminalElementsWithTemps(BoundExpression expr, TupleBinaryOperatorInfo operators, ArrayBuilder<BoundExpression> initEffects, ArrayBuilder<LocalSymbol> temps)
	{
		if (operators.InfoKind == TupleBinaryOperatorInfoKind.Multiple && expr.Kind == BoundKind.TupleLiteral)
		{
			BoundTupleLiteral boundTupleLiteral = (BoundTupleLiteral)expr;
			TupleBinaryOperatorInfo.Multiple multiple = (TupleBinaryOperatorInfo.Multiple)operators;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance(boundTupleLiteral.Arguments.Length);
			for (int i = 0; i < boundTupleLiteral.Arguments.Length; i++)
			{
				BoundExpression expr2 = boundTupleLiteral.Arguments[i];
				BoundExpression item = ReplaceTerminalElementsWithTemps(expr2, multiple.Operators[i], initEffects, temps);
				instance.Add(item);
			}
			return new BoundTupleLiteral(boundTupleLiteral.Syntax, boundTupleLiteral.ArgumentNamesOpt, boundTupleLiteral.InferredNamesOpt, instance.ToImmutableAndFree(), boundTupleLiteral.Type, boundTupleLiteral.HasErrors);
		}
		BoundExpression boundExpression = VisitExpression(expr);
		if ((object)boundExpression.Type != null)
		{
			BoundExpression boundExpression2 = NullableAlwaysHasValue(boundExpression);
			if (boundExpression2 != null)
			{
				BoundExpression item2 = EvaluateSideEffectingArgumentToTemp(boundExpression2, initEffects, ref temps);
				BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)boundExpression;
				return boundObjectCreationExpression.UpdateArgumentsAndInitializer(ImmutableArray.Create(item2), boundObjectCreationExpression.ArgumentRefKindsOpt, boundObjectCreationExpression.InitializerExpressionOpt);
			}
		}
		return EvaluateSideEffectingArgumentToTemp(boundExpression, initEffects, ref temps);
	}

	private BoundExpression RewriteTupleOperator(TupleBinaryOperatorInfo @operator, BoundExpression left, BoundExpression right, TypeSymbol boolType, ArrayBuilder<LocalSymbol> temps, BinaryOperatorKind operatorKind)
	{
		switch (@operator.InfoKind)
		{
		case TupleBinaryOperatorInfoKind.Multiple:
			return RewriteTupleNestedOperators((TupleBinaryOperatorInfo.Multiple)@operator, left, right, boolType, temps, operatorKind);
		case TupleBinaryOperatorInfoKind.Single:
			return RewriteTupleSingleOperator((TupleBinaryOperatorInfo.Single)@operator, left, right, boolType, operatorKind);
		case TupleBinaryOperatorInfoKind.NullNull:
		{
			TupleBinaryOperatorInfo.NullNull nullNull = (TupleBinaryOperatorInfo.NullNull)@operator;
			return new BoundLiteral(left.Syntax, ConstantValue.Create(nullNull.Kind == BinaryOperatorKind.Equal), boolType);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(@operator.InfoKind);
		}
	}

	private BoundExpression RewriteTupleNestedOperators(TupleBinaryOperatorInfo.Multiple operators, BoundExpression left, BoundExpression right, TypeSymbol boolType, ArrayBuilder<LocalSymbol> temps, BinaryOperatorKind operatorKind)
	{
		left = Binder.GiveTupleTypeToDefaultLiteralIfNeeded(left, right.Type);
		right = Binder.GiveTupleTypeToDefaultLiteralIfNeeded(right, left.Type);
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		MakeNullableParts(left, temps, instance2, instance, saveHasValue: true, out var hasValue, out var value, out var isNullable);
		MakeNullableParts(right, temps, instance2, instance, saveHasValue: false, out var hasValue2, out var value2, out var isNullable2);
		BoundExpression returnValue = RewriteNonNullableNestedTupleOperators(operators, value, value2, boolType, temps, instance2, operatorKind);
		BoundExpression boundExpression = MakeSequenceOrResultValue(ImmutableArray<LocalSymbol>.Empty, instance2.ToImmutableAndFree(), returnValue);
		if (!isNullable && !isNullable2)
		{
			return boundExpression;
		}
		bool flag = operatorKind == BinaryOperatorKind.Equal;
		if (hasValue2.ConstantValue == ConstantValue.False)
		{
			return MakeSequenceOrResultValue(ImmutableArray<LocalSymbol>.Empty, instance.ToImmutableAndFree(), flag ? _factory.Not(hasValue) : hasValue);
		}
		if (hasValue.ConstantValue == ConstantValue.False)
		{
			return MakeSequenceOrResultValue(ImmutableArray<LocalSymbol>.Empty, instance.ToImmutableAndFree(), flag ? _factory.Not(hasValue2) : hasValue2);
		}
		return MakeSequenceOrResultValue(ImmutableArray<LocalSymbol>.Empty, instance.ToImmutableAndFree(), _factory.Conditional(_factory.Binary(BinaryOperatorKind.Equal, boolType, hasValue, hasValue2), _factory.Conditional(hasValue, boundExpression, MakeBooleanConstant(right.Syntax, flag), boolType), MakeBooleanConstant(right.Syntax, !flag), boolType));
	}

	private void MakeNullableParts(BoundExpression expr, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> innerEffects, ArrayBuilder<BoundExpression> outerEffects, bool saveHasValue, out BoundExpression hasValue, out BoundExpression value, out bool isNullable)
	{
		isNullable = expr.Kind != BoundKind.TupleLiteral && expr.Type.IsNullableType();
		if (!isNullable)
		{
			hasValue = MakeBooleanConstant(expr.Syntax, value: true);
			value = expr;
			return;
		}
		if (NullableNeverHasValue(expr))
		{
			hasValue = MakeBooleanConstant(expr.Syntax, value: false);
			value = new BoundDefaultExpression(expr.Syntax, expr.Type.StrippedType());
			return;
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(expr);
		if (boundExpression != null)
		{
			hasValue = MakeBooleanConstant(expr.Syntax, value: true);
			value = boundExpression;
			isNullable = false;
			return;
		}
		if (saveHasValue)
		{
			hasValue = MakeHasValueTemp(expr, temps, outerEffects);
		}
		else
		{
			hasValue = MakeNullableHasValue(expr.Syntax, expr);
		}
		value = MakeValueOrDefaultTemp(expr, temps, innerEffects);
	}

	private BoundLocal MakeTemp(BoundExpression loweredExpression, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		BoundLocal boundLocal = _factory.StoreToTemp(loweredExpression, out var store);
		effects.Add(store);
		temps.Add(boundLocal.LocalSymbol);
		return boundLocal;
	}

	private BoundLocal MakeHasValueTemp(BoundExpression expression, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		BoundExpression loweredExpression = MakeNullableHasValue(expression.Syntax, expression);
		return MakeTemp(loweredExpression, temps, effects);
	}

	private BoundLocal MakeValueOrDefaultTemp(BoundExpression expression, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects)
	{
		BoundExpression loweredExpression = MakeOptimizedGetValueOrDefault(expression.Syntax, expression);
		return MakeTemp(loweredExpression, temps, effects);
	}

	private BoundExpression RewriteNonNullableNestedTupleOperators(TupleBinaryOperatorInfo.Multiple operators, BoundExpression left, BoundExpression right, TypeSymbol type, ArrayBuilder<LocalSymbol> temps, ArrayBuilder<BoundExpression> effects, BinaryOperatorKind operatorKind)
	{
		ImmutableArray<TupleBinaryOperatorInfo> operators2 = operators.Operators;
		BoundExpression boundExpression = null;
		for (int i = 0; i < operators2.Length; i++)
		{
			BoundExpression tuplePart = GetTuplePart(left, i);
			BoundExpression tuplePart2 = GetTuplePart(right, i);
			BoundExpression boundExpression2 = RewriteTupleOperator(operators2[i], tuplePart, tuplePart2, type, temps, operatorKind);
			if (boundExpression == null)
			{
				boundExpression = boundExpression2;
				continue;
			}
			BinaryOperatorKind kind = ((operatorKind == BinaryOperatorKind.Equal) ? BinaryOperatorKind.LogicalBoolAnd : BinaryOperatorKind.LogicalBoolOr);
			boundExpression = _factory.Binary(kind, type, boundExpression, boundExpression2);
		}
		return boundExpression;
	}

	private BoundExpression GetTuplePart(BoundExpression tuple, int i)
	{
		if (tuple.Kind == BoundKind.TupleLiteral)
		{
			return ((BoundTupleLiteral)tuple).Arguments[i];
		}
		return MakeTupleFieldAccessAndReportUseSiteDiagnostics(tuple, tuple.Syntax, tuple.Type.TupleElements[i]);
	}

	private BoundExpression RewriteTupleSingleOperator(TupleBinaryOperatorInfo.Single single, BoundExpression left, BoundExpression right, TypeSymbol boolType, BinaryOperatorKind operatorKind)
	{
		if (single.Kind.IsDynamic())
		{
			BoundExpression loweredOperand = _dynamicFactory.MakeDynamicBinaryOperator(single.Kind, left, right, isCompoundAssignment: false, _compilation.DynamicType).ToExpression();
			if (operatorKind == BinaryOperatorKind.Equal)
			{
				return _factory.Not(MakeUnaryOperator(UnaryOperatorKind.DynamicFalse, left.Syntax, null, loweredOperand, boolType));
			}
			return MakeUnaryOperator(UnaryOperatorKind.DynamicTrue, left.Syntax, null, loweredOperand, boolType);
		}
		if (left.IsLiteralNull() && right.IsLiteralNull())
		{
			return new BoundLiteral(left.Syntax, ConstantValue.Create(operatorKind == BinaryOperatorKind.Equal), boolType);
		}
		bool flag = single.Kind.OperandTypes() == BinaryOperatorKind.NullableNull;
		BoundExpression loweredLeft = (flag ? left : MakeConversionNode(left.Syntax, left, single.LeftConversion, single.LeftConvertedTypeOpt, @checked: false));
		BoundExpression loweredRight = (flag ? right : MakeConversionNode(right.Syntax, right, single.RightConversion, single.RightConvertedTypeOpt, @checked: false));
		BoundExpression boundExpression = MakeBinaryOperator(_factory.Syntax, single.Kind, loweredLeft, loweredRight, single.MethodSymbolOpt?.ReturnType ?? boolType, single.MethodSymbolOpt);
		UnaryOperatorSignature boolOperator = single.BoolOperator;
		Conversion conversionForBool = single.ConversionForBool;
		BoundExpression boundExpression2;
		if (boolOperator.Kind == UnaryOperatorKind.Error)
		{
			boundExpression2 = (conversionForBool.IsIdentity ? boundExpression : MakeConversionNode(_factory.Syntax, boundExpression, conversionForBool, boolType, @checked: false));
		}
		else
		{
			BoundExpression loweredOperand2 = MakeConversionNode(_factory.Syntax, boundExpression, conversionForBool, boolOperator.OperandType, @checked: false);
			boundExpression2 = MakeUnaryOperator(boolOperator.Kind, boundExpression.Syntax, boolOperator.Method, loweredOperand2, boolType);
			if (operatorKind == BinaryOperatorKind.Equal)
			{
				boundExpression2 = _factory.Not(boundExpression2);
			}
		}
		return boundExpression2;
	}

	public override BoundNode VisitTupleLiteral(BoundTupleLiteral node)
	{
		return VisitTupleExpression(node);
	}

	public override BoundNode VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node)
	{
		return VisitTupleExpression(node);
	}

	private BoundNode VisitTupleExpression(BoundTupleExpression node)
	{
		ImmutableArray<BoundExpression> rewrittenArguments = VisitList(node.Arguments);
		return RewriteTupleCreationExpression(node, rewrittenArguments);
	}

	private BoundExpression RewriteTupleCreationExpression(BoundTupleExpression node, ImmutableArray<BoundExpression> rewrittenArguments)
	{
		return MakeTupleCreationExpression(node.Syntax, (NamedTypeSymbol)node.Type, rewrittenArguments);
	}

	private BoundExpression MakeTupleCreationExpression(SyntaxNode syntax, NamedTypeSymbol type, ImmutableArray<BoundExpression> rewrittenArguments)
	{
		NamedTypeSymbol underlyingTupleType = type.TupleUnderlyingType ?? type;
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		TupleTypeSymbol.GetUnderlyingTypeChain(underlyingTupleType, instance);
		try
		{
			NamedTypeSymbol namedTypeSymbol = instance.Pop();
			ImmutableArray<BoundExpression> arguments = ImmutableArray.Create(rewrittenArguments, instance.Count * 7, namedTypeSymbol.Arity);
			MethodSymbol methodSymbol = (MethodSymbol)TupleTypeSymbol.GetWellKnownMemberInType(namedTypeSymbol.OriginalDefinition, TupleTypeSymbol.GetTupleCtor(namedTypeSymbol.Arity), _diagnostics, syntax);
			if ((object)methodSymbol == null)
			{
				return _factory.BadExpression(type);
			}
			MethodSymbol constructor = methodSymbol.AsMember(namedTypeSymbol);
			BoundObjectCreationExpression boundObjectCreationExpression = new BoundObjectCreationExpression(syntax, constructor, null, arguments);
			if (instance.Count > 0)
			{
				MethodSymbol methodSymbol2 = (MethodSymbol)TupleTypeSymbol.GetWellKnownMemberInType(instance.Peek().OriginalDefinition, TupleTypeSymbol.GetTupleCtor(8), _diagnostics, syntax);
				if ((object)methodSymbol2 == null)
				{
					return _factory.BadExpression(type);
				}
				do
				{
					ImmutableArray<BoundExpression> arguments2 = ImmutableArray.Create(rewrittenArguments, (instance.Count - 1) * 7, 7).Add(boundObjectCreationExpression);
					MethodSymbol constructor2 = methodSymbol2.AsMember(instance.Pop());
					boundObjectCreationExpression = new BoundObjectCreationExpression(syntax, constructor2, null, arguments2);
				}
				while (instance.Count > 0);
			}
			return boundObjectCreationExpression.Update(boundObjectCreationExpression.Constructor, boundObjectCreationExpression.Arguments, boundObjectCreationExpression.ArgumentNamesOpt, boundObjectCreationExpression.ArgumentRefKindsOpt, boundObjectCreationExpression.Expanded, boundObjectCreationExpression.ArgsToParamsOpt, boundObjectCreationExpression.ConstantValue, boundObjectCreationExpression.InitializerExpressionOpt, boundObjectCreationExpression.BinderOpt, type);
		}
		finally
		{
			instance.Free();
		}
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		switch (node.OperatorKind.Operator())
		{
		case UnaryOperatorKind.PostfixIncrement:
		case UnaryOperatorKind.PostfixDecrement:
		case UnaryOperatorKind.PrefixIncrement:
		case UnaryOperatorKind.PrefixDecrement:
			return base.VisitUnaryOperator(node);
		default:
		{
			if (node.Operand.Kind == BoundKind.BinaryOperator)
			{
				BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)node.Operand;
				if ((node.OperatorKind == UnaryOperatorKind.DynamicTrue && boundBinaryOperator.OperatorKind == BinaryOperatorKind.DynamicLogicalOr) || (node.OperatorKind == UnaryOperatorKind.DynamicFalse && boundBinaryOperator.OperatorKind == BinaryOperatorKind.DynamicLogicalAnd))
				{
					return VisitBinaryOperator(boundBinaryOperator, node);
				}
			}
			BoundExpression loweredOperand = VisitExpression(node.Operand);
			return MakeUnaryOperator(node, node.OperatorKind, node.Syntax, node.MethodOpt, loweredOperand, node.Type);
		}
		}
	}

	private BoundExpression MakeUnaryOperator(UnaryOperatorKind kind, SyntaxNode syntax, MethodSymbol method, BoundExpression loweredOperand, TypeSymbol type)
	{
		return MakeUnaryOperator(null, kind, syntax, method, loweredOperand, type);
	}

	private BoundExpression MakeUnaryOperator(BoundUnaryOperator oldNode, UnaryOperatorKind kind, SyntaxNode syntax, MethodSymbol method, BoundExpression loweredOperand, TypeSymbol type)
	{
		if (kind.IsDynamic())
		{
			ConstantValue constantValue = UnboxConstant(loweredOperand);
			if (constantValue == ConstantValue.True || constantValue == ConstantValue.False)
			{
				switch (kind)
				{
				case UnaryOperatorKind.DynamicTrue:
					return _factory.Literal(constantValue.BooleanValue);
				case UnaryOperatorKind.DynamicLogicalNegation:
					return MakeConversionNode(_factory.Literal(!constantValue.BooleanValue), type, @checked: false);
				}
			}
			return _dynamicFactory.MakeDynamicUnaryOperator(kind, loweredOperand, type).ToExpression();
		}
		if (kind.IsLifted())
		{
			if (!_inExpressionLambda)
			{
				return LowerLiftedUnaryOperator(kind, syntax, method, loweredOperand, type);
			}
		}
		else if (kind.IsUserDefined())
		{
			if (!_inExpressionLambda || kind == UnaryOperatorKind.UserDefinedTrue || kind == UnaryOperatorKind.UserDefinedFalse)
			{
				return BoundCall.Synthesized(syntax, null, method, loweredOperand);
			}
		}
		else if (kind.Operator() == UnaryOperatorKind.UnaryPlus)
		{
			return loweredOperand;
		}
		switch (kind)
		{
		case UnaryOperatorKind.EnumBitwiseComplement:
		{
			NamedTypeSymbol enumUnderlyingType = loweredOperand.Type.GetEnumUnderlyingType();
			SpecialType enumPromotedType = Binder.GetEnumPromotedType(enumUnderlyingType.SpecialType);
			NamedTypeSymbol namedTypeSymbol = ((enumPromotedType == enumUnderlyingType.SpecialType) ? enumUnderlyingType : _compilation.GetSpecialType(enumPromotedType));
			BoundExpression boundExpression = MakeConversionNode(loweredOperand, namedTypeSymbol, @checked: false);
			UnaryOperatorKind operatorKind = kind.Operator().WithType(enumPromotedType);
			BoundUnaryOperator boundUnaryOperator = ((oldNode != null) ? oldNode.Update(operatorKind, boundExpression, oldNode.ConstantValueOpt, method, boundExpression.ResultKind, namedTypeSymbol) : new BoundUnaryOperator(syntax, operatorKind, boundExpression, null, method, LookupResultKind.Viable, namedTypeSymbol));
			return MakeConversionNode(boundUnaryOperator.Syntax, boundUnaryOperator, Conversion.ExplicitEnumeration, type, @checked: false);
		}
		case UnaryOperatorKind.DecimalUnaryMinus:
			method = (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(SpecialMember.System_Decimal__op_UnaryNegation);
			if (!_inExpressionLambda)
			{
				return BoundCall.Synthesized(syntax, null, method, loweredOperand);
			}
			break;
		}
		if (oldNode == null)
		{
			return new BoundUnaryOperator(syntax, kind, loweredOperand, null, method, LookupResultKind.Viable, type);
		}
		return oldNode.Update(kind, loweredOperand, oldNode.ConstantValueOpt, method, oldNode.ResultKind, type);
	}

	private BoundExpression LowerLiftedUnaryOperator(UnaryOperatorKind kind, SyntaxNode syntax, MethodSymbol method, BoundExpression loweredOperand, TypeSymbol type)
	{
		BoundExpression boundExpression = OptimizeLiftedUnaryOperator(kind, syntax, method, loweredOperand, type);
		if (boundExpression != null)
		{
			return boundExpression;
		}
		BoundLocal boundLocal = _factory.StoreToTemp(loweredOperand, out var store);
		MethodSymbol method2 = UnsafeGetNullableMethod(syntax, boundLocal.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		BoundExpression rewrittenCondition = MakeNullableHasValue(syntax, boundLocal);
		BoundExpression nonNullOperand = BoundCall.Synthesized(syntax, boundLocal, method2);
		BoundExpression liftedUnaryOperatorConsequence = GetLiftedUnaryOperatorConsequence(kind, syntax, method, type, nonNullOperand);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, null, type);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, liftedUnaryOperatorConsequence, rewrittenAlternative, null, type, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, type);
	}

	private BoundExpression OptimizeLiftedUnaryOperator(UnaryOperatorKind operatorKind, SyntaxNode syntax, MethodSymbol method, BoundExpression loweredOperand, TypeSymbol type)
	{
		if (NullableNeverHasValue(loweredOperand))
		{
			return new BoundDefaultExpression(syntax, null, type);
		}
		BoundExpression boundExpression = NullableAlwaysHasValue(loweredOperand);
		if (boundExpression != null)
		{
			return GetLiftedUnaryOperatorConsequence(operatorKind, syntax, method, type, boundExpression);
		}
		if (loweredOperand is BoundLoweredConditionalAccess boundLoweredConditionalAccess && (boundLoweredConditionalAccess.WhenNullOpt == null || boundLoweredConditionalAccess.WhenNullOpt.IsDefaultValue()))
		{
			BoundExpression boundExpression2 = LowerLiftedUnaryOperator(operatorKind, syntax, method, boundLoweredConditionalAccess.WhenNotNull, type);
			return boundLoweredConditionalAccess.Update(boundLoweredConditionalAccess.Receiver, boundLoweredConditionalAccess.HasValueMethodOpt, boundExpression2, null, boundLoweredConditionalAccess.Id, boundExpression2.Type);
		}
		if (loweredOperand.Kind == BoundKind.Sequence)
		{
			BoundSequence boundSequence = (BoundSequence)loweredOperand;
			if (boundSequence.Value.Kind == BoundKind.ConditionalOperator)
			{
				BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)boundSequence.Value;
				if (NullableAlwaysHasValue(boundConditionalOperator.Consequence) != null && NullableNeverHasValue(boundConditionalOperator.Alternative))
				{
					return new BoundSequence(syntax, boundSequence.Locals, boundSequence.SideEffects, RewriteConditionalOperator(syntax, boundConditionalOperator.Condition, MakeUnaryOperator(operatorKind, syntax, method, boundConditionalOperator.Consequence, type), MakeUnaryOperator(operatorKind, syntax, method, boundConditionalOperator.Alternative, type), null, type, isRef: false), type);
				}
			}
		}
		return null;
	}

	private BoundExpression GetLiftedUnaryOperatorConsequence(UnaryOperatorKind kind, SyntaxNode syntax, MethodSymbol method, TypeSymbol type, BoundExpression nonNullOperand)
	{
		MethodSymbol constructor = UnsafeGetNullableMethod(syntax, type, SpecialMember.System_Nullable_T__ctor);
		BoundExpression boundExpression = MakeUnaryOperator(null, kind.Unlifted(), syntax, method, nonNullOperand, type.GetNullableUnderlyingType());
		return new BoundObjectCreationExpression(syntax, constructor, null, boundExpression);
	}

	private static bool IsIncrement(BoundIncrementOperator node)
	{
		UnaryOperatorKind unaryOperatorKind = node.OperatorKind.Operator();
		if (unaryOperatorKind != UnaryOperatorKind.PostfixIncrement)
		{
			return unaryOperatorKind == UnaryOperatorKind.PrefixIncrement;
		}
		return true;
	}

	private static bool IsPrefix(BoundIncrementOperator node)
	{
		UnaryOperatorKind unaryOperatorKind = node.OperatorKind.Operator();
		if (unaryOperatorKind != UnaryOperatorKind.PrefixIncrement)
		{
			return unaryOperatorKind == UnaryOperatorKind.PrefixDecrement;
		}
		return true;
	}

	public override BoundNode VisitIncrementOperator(BoundIncrementOperator node)
	{
		bool flag = IsPrefix(node);
		bool isDynamicAssignment = node.OperatorKind.IsDynamic();
		bool isChecked = node.OperatorKind.IsChecked();
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		SyntaxNode syntax = node.Syntax;
		BoundExpression boundExpression = TransformCompoundAssignmentLHS(node.Operand, instance2, instance, isDynamicAssignment);
		TypeSymbol type = boundExpression.Type;
		LocalSymbol localSymbol = _factory.SynthesizedLocal(type);
		instance.Add(localSymbol);
		BoundExpression boundExpression2 = new BoundLocal(syntax, localSymbol, null, type);
		BoundExpression newValue = MakeIncrementOperator(node, flag ? MakeRValue(boundExpression) : boundExpression2);
		if (IsIndirectOrInstanceField(boundExpression))
		{
			return RewriteWithRefOperand(flag, isChecked, instance, instance2, syntax, boundExpression, type, boundExpression2, newValue);
		}
		return RewriteWithNotRefOperand(flag, isChecked, instance, instance2, syntax, boundExpression, type, boundExpression2, newValue);
	}

	private static bool IsIndirectOrInstanceField(BoundExpression expression)
	{
		return expression.Kind switch
		{
			BoundKind.Local => ((BoundLocal)expression).LocalSymbol.RefKind != RefKind.None, 
			BoundKind.Parameter => ((BoundParameter)expression).ParameterSymbol.RefKind != RefKind.None, 
			BoundKind.FieldAccess => !((BoundFieldAccess)expression).FieldSymbol.IsStatic, 
			_ => false, 
		};
	}

	private BoundNode RewriteWithNotRefOperand(bool isPrefix, bool isChecked, ArrayBuilder<LocalSymbol> tempSymbols, ArrayBuilder<BoundExpression> tempInitializers, SyntaxNode syntax, BoundExpression transformedLHS, TypeSymbol operandType, BoundExpression boundTemp, BoundExpression newValue)
	{
		ImmutableArray<BoundExpression> second = ImmutableArray.Create(MakeAssignmentOperator(syntax, boundTemp, isPrefix ? newValue : MakeRValue(transformedLHS), operandType, used: false, isChecked, isCompoundAssignment: false), MakeAssignmentOperator(syntax, transformedLHS, isPrefix ? boundTemp : newValue, operandType, used: false, isChecked, isCompoundAssignment: false));
		return new BoundSequence(syntax, tempSymbols.ToImmutableAndFree(), tempInitializers.ToImmutableAndFree().Concat(second), boundTemp, operandType);
	}

	private BoundNode RewriteWithRefOperand(bool isPrefix, bool isChecked, ArrayBuilder<LocalSymbol> tempSymbols, ArrayBuilder<BoundExpression> tempInitializers, SyntaxNode syntax, BoundExpression operand, TypeSymbol operandType, BoundExpression boundTemp, BoundExpression newValue)
	{
		BoundExpression boundExpression = (isPrefix ? newValue : MakeRValue(operand));
		BoundExpression item = MakeAssignmentOperator(syntax, boundTemp, boundExpression, operandType, used: false, isChecked, isCompoundAssignment: false);
		BoundExpression value = (isPrefix ? boundTemp : newValue);
		BoundSequence rewrittenRight = new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(item), value, boundExpression.Type);
		BoundExpression item2 = MakeAssignmentOperator(syntax, operand, rewrittenRight, operandType, used: false, isChecked, isCompoundAssignment: false);
		tempInitializers.Add(item2);
		return new BoundSequence(syntax, tempSymbols.ToImmutableAndFree(), tempInitializers.ToImmutableAndFree(), boundTemp, operandType);
	}

	private BoundExpression MakeIncrementOperator(BoundIncrementOperator node, BoundExpression rewrittenValueToIncrement)
	{
		if (node.OperatorKind.IsDynamic())
		{
			return _dynamicFactory.MakeDynamicUnaryOperator(node.OperatorKind, rewrittenValueToIncrement, node.Type).ToExpression();
		}
		BoundExpression boundExpression = ((node.OperatorKind.OperandTypes() != UnaryOperatorKind.UserDefined) ? MakeBuiltInIncrementOperator(node, rewrittenValueToIncrement) : MakeUserDefinedIncrementOperator(node, rewrittenValueToIncrement));
		if (!node.ResultConversion.IsIdentity)
		{
			boundExpression = MakeConversionNode(node.Syntax, boundExpression, node.ResultConversion, node.Type, node.OperatorKind.IsChecked());
		}
		return boundExpression;
	}

	private BoundExpression MakeUserDefinedIncrementOperator(BoundIncrementOperator node, BoundExpression rewrittenValueToIncrement)
	{
		bool flag = node.OperatorKind.IsLifted();
		bool flag2 = node.OperatorKind.IsChecked();
		BoundExpression boundExpression = rewrittenValueToIncrement;
		SyntaxNode syntax = node.Syntax;
		TypeSymbol typeSymbol = node.MethodOpt.ParameterTypes[0];
		if (flag)
		{
			typeSymbol = _compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(typeSymbol);
		}
		if (!node.OperandConversion.IsIdentity)
		{
			boundExpression = MakeConversionNode(syntax, rewrittenValueToIncrement, node.OperandConversion, typeSymbol, flag2);
		}
		if (!flag)
		{
			return BoundCall.Synthesized(syntax, null, node.MethodOpt, boundExpression);
		}
		BoundLocal boundLocal = _factory.StoreToTemp(boundExpression, out var store);
		MethodSymbol method = UnsafeGetNullableMethod(syntax, typeSymbol, SpecialMember.System_Nullable_T_GetValueOrDefault);
		MethodSymbol constructor = UnsafeGetNullableMethod(syntax, typeSymbol, SpecialMember.System_Nullable_T__ctor);
		BoundExpression rewrittenCondition = MakeNullableHasValue(node.Syntax, boundLocal);
		BoundExpression arg = BoundCall.Synthesized(syntax, boundLocal, method);
		BoundExpression boundExpression2 = BoundCall.Synthesized(syntax, null, node.MethodOpt, arg);
		BoundExpression rewrittenConsequence = new BoundObjectCreationExpression(syntax, constructor, null, boundExpression2);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, null, typeSymbol);
		BoundExpression value = RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, typeSymbol, isRef: false);
		return new BoundSequence(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundExpression)store), value, typeSymbol);
	}

	private BoundExpression MakeBuiltInIncrementOperator(BoundIncrementOperator node, BoundExpression rewrittenValueToIncrement)
	{
		TypeSymbol unaryOperatorType = GetUnaryOperatorType(node);
		BinaryOperatorKind correspondingBinaryOperator = GetCorrespondingBinaryOperator(node);
		correspondingBinaryOperator = (BinaryOperatorKind)((int)correspondingBinaryOperator | (IsIncrement(node) ? 4352 : 4608));
		ConstantValue constantOneForBinOp = GetConstantOneForBinOp(correspondingBinaryOperator);
		TypeSymbol typeSymbol = _compilation.GetSpecialType(constantOneForBinOp.SpecialType);
		BoundExpression boundExpression = MakeLiteral(node.Syntax, constantOneForBinOp, typeSymbol);
		if (correspondingBinaryOperator.IsLifted())
		{
			typeSymbol = _compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(typeSymbol);
			MethodSymbol constructor = UnsafeGetNullableMethod(node.Syntax, typeSymbol, SpecialMember.System_Nullable_T__ctor);
			boundExpression = new BoundObjectCreationExpression(node.Syntax, constructor, null, boundExpression);
		}
		BoundExpression boundExpression2 = rewrittenValueToIncrement;
		bool flag = node.OperatorKind.IsChecked();
		if (!node.OperandConversion.IsIdentity)
		{
			boundExpression2 = MakeConversionNode(node.Syntax, boundExpression2, node.OperandConversion, unaryOperatorType, flag);
		}
		if (node.OperatorKind.OperandTypes() == UnaryOperatorKind.Pointer)
		{
			return MakeBinaryOperator(node.Syntax, correspondingBinaryOperator, boundExpression2, boundExpression, boundExpression2.Type, null);
		}
		boundExpression2 = MakeConversionNode(boundExpression2, typeSymbol, flag);
		BoundExpression rewrittenOperand;
		if (unaryOperatorType.SpecialType == SpecialType.System_Decimal)
		{
			rewrittenOperand = MakeDecimalIncDecOperator(node.Syntax, correspondingBinaryOperator, boundExpression2);
		}
		else
		{
			rewrittenOperand = ((!unaryOperatorType.IsNullableType() || unaryOperatorType.GetNullableUnderlyingType().SpecialType != SpecialType.System_Decimal) ? MakeBinaryOperator(node.Syntax, correspondingBinaryOperator, boundExpression2, boundExpression, typeSymbol, null) : MakeLiftedDecimalIncDecOperator(node.Syntax, correspondingBinaryOperator, boundExpression2));
		}
		return MakeConversionNode(rewrittenOperand, unaryOperatorType, flag);
	}

	private MethodSymbol GetDecimalIncDecOperator(BinaryOperatorKind oper)
	{
		SpecialMember member = oper.Operator() switch
		{
			BinaryOperatorKind.Addition => SpecialMember.System_Decimal__op_Increment, 
			BinaryOperatorKind.Subtraction => SpecialMember.System_Decimal__op_Decrement, 
			_ => throw ExceptionUtilities.UnexpectedValue(oper.Operator()), 
		};
		return (MethodSymbol)_compilation.Assembly.GetSpecialTypeMember(member);
	}

	private BoundExpression MakeDecimalIncDecOperator(SyntaxNode syntax, BinaryOperatorKind oper, BoundExpression operand)
	{
		MethodSymbol decimalIncDecOperator = GetDecimalIncDecOperator(oper);
		return BoundCall.Synthesized(syntax, null, decimalIncDecOperator, operand);
	}

	private BoundExpression MakeLiftedDecimalIncDecOperator(SyntaxNode syntax, BinaryOperatorKind oper, BoundExpression operand)
	{
		MethodSymbol decimalIncDecOperator = GetDecimalIncDecOperator(oper);
		MethodSymbol method = UnsafeGetNullableMethod(syntax, operand.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
		MethodSymbol constructor = UnsafeGetNullableMethod(syntax, operand.Type, SpecialMember.System_Nullable_T__ctor);
		BoundExpression rewrittenCondition = MakeNullableHasValue(syntax, operand);
		BoundExpression arg = BoundCall.Synthesized(syntax, operand, method);
		BoundExpression boundExpression = BoundCall.Synthesized(syntax, null, decimalIncDecOperator, arg);
		BoundExpression rewrittenConsequence = new BoundObjectCreationExpression(syntax, constructor, null, boundExpression);
		BoundExpression rewrittenAlternative = new BoundDefaultExpression(syntax, null, operand.Type);
		return RewriteConditionalOperator(syntax, rewrittenCondition, rewrittenConsequence, rewrittenAlternative, null, operand.Type, isRef: false);
	}

	private BoundExpression MakeRValue(BoundExpression transformedExpression)
	{
		switch (transformedExpression.Kind)
		{
		case BoundKind.PropertyAccess:
		{
			BoundPropertyAccess boundPropertyAccess = (BoundPropertyAccess)transformedExpression;
			return MakePropertyGetAccess(transformedExpression.Syntax, boundPropertyAccess.ReceiverOpt, boundPropertyAccess.PropertySymbol, boundPropertyAccess);
		}
		case BoundKind.DynamicMemberAccess:
		{
			BoundDynamicMemberAccess boundDynamicMemberAccess = (BoundDynamicMemberAccess)transformedExpression;
			return _dynamicFactory.MakeDynamicGetMember(boundDynamicMemberAccess.Receiver, boundDynamicMemberAccess.Name, resultIndexed: false).ToExpression();
		}
		case BoundKind.IndexerAccess:
		{
			BoundIndexerAccess boundIndexerAccess = (BoundIndexerAccess)transformedExpression;
			return MakePropertyGetAccess(transformedExpression.Syntax, boundIndexerAccess.ReceiverOpt, boundIndexerAccess.Indexer, boundIndexerAccess.Arguments);
		}
		case BoundKind.DynamicIndexerAccess:
		{
			BoundDynamicIndexerAccess boundDynamicIndexerAccess = (BoundDynamicIndexerAccess)transformedExpression;
			return MakeDynamicGetIndex(boundDynamicIndexerAccess, boundDynamicIndexerAccess.ReceiverOpt, boundDynamicIndexerAccess.Arguments, boundDynamicIndexerAccess.ArgumentNamesOpt, boundDynamicIndexerAccess.ArgumentRefKindsOpt);
		}
		default:
			return transformedExpression;
		}
	}

	private TypeSymbol GetUnaryOperatorType(BoundIncrementOperator node)
	{
		UnaryOperatorKind unaryOperatorKind = node.OperatorKind.OperandTypes();
		SpecialType specialType;
		switch (unaryOperatorKind)
		{
		case UnaryOperatorKind.Enum:
			return node.Type;
		case UnaryOperatorKind.Int:
			specialType = SpecialType.System_Int32;
			break;
		case UnaryOperatorKind.SByte:
			specialType = SpecialType.System_SByte;
			break;
		case UnaryOperatorKind.Short:
			specialType = SpecialType.System_Int16;
			break;
		case UnaryOperatorKind.Byte:
			specialType = SpecialType.System_Byte;
			break;
		case UnaryOperatorKind.UShort:
			specialType = SpecialType.System_UInt16;
			break;
		case UnaryOperatorKind.Char:
			specialType = SpecialType.System_Char;
			break;
		case UnaryOperatorKind.UInt:
			specialType = SpecialType.System_UInt32;
			break;
		case UnaryOperatorKind.Long:
			specialType = SpecialType.System_Int64;
			break;
		case UnaryOperatorKind.ULong:
			specialType = SpecialType.System_UInt64;
			break;
		case UnaryOperatorKind.Float:
			specialType = SpecialType.System_Single;
			break;
		case UnaryOperatorKind.Double:
			specialType = SpecialType.System_Double;
			break;
		case UnaryOperatorKind.Decimal:
			specialType = SpecialType.System_Decimal;
			break;
		case UnaryOperatorKind.Pointer:
			return node.Type;
		default:
			throw ExceptionUtilities.UnexpectedValue(unaryOperatorKind);
		}
		NamedTypeSymbol namedTypeSymbol = _compilation.GetSpecialType(specialType);
		if (node.OperatorKind.IsLifted())
		{
			namedTypeSymbol = _compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(namedTypeSymbol);
		}
		return namedTypeSymbol;
	}

	private static BinaryOperatorKind GetCorrespondingBinaryOperator(BoundIncrementOperator node)
	{
		UnaryOperatorKind operatorKind = node.OperatorKind;
		BinaryOperatorKind binaryOperatorKind;
		switch (operatorKind.OperandTypes())
		{
		case UnaryOperatorKind.SByte:
		case UnaryOperatorKind.Short:
		case UnaryOperatorKind.Int:
			binaryOperatorKind = BinaryOperatorKind.Int;
			break;
		case UnaryOperatorKind.Byte:
		case UnaryOperatorKind.UShort:
		case UnaryOperatorKind.UInt:
		case UnaryOperatorKind.Char:
			binaryOperatorKind = BinaryOperatorKind.UInt;
			break;
		case UnaryOperatorKind.Long:
			binaryOperatorKind = BinaryOperatorKind.Long;
			break;
		case UnaryOperatorKind.ULong:
			binaryOperatorKind = BinaryOperatorKind.ULong;
			break;
		case UnaryOperatorKind.Float:
			binaryOperatorKind = BinaryOperatorKind.Float;
			break;
		case UnaryOperatorKind.Double:
			binaryOperatorKind = BinaryOperatorKind.Double;
			break;
		case UnaryOperatorKind.Decimal:
			binaryOperatorKind = BinaryOperatorKind.Decimal;
			break;
		case UnaryOperatorKind.Enum:
		{
			TypeSymbol type = node.Type;
			if (type.IsNullableType())
			{
				type = type.GetNullableUnderlyingType();
			}
			type = type.GetEnumUnderlyingType();
			switch (type.SpecialType)
			{
			case SpecialType.System_SByte:
			case SpecialType.System_Int16:
			case SpecialType.System_Int32:
				binaryOperatorKind = BinaryOperatorKind.Int;
				break;
			case SpecialType.System_Byte:
			case SpecialType.System_UInt16:
			case SpecialType.System_UInt32:
				binaryOperatorKind = BinaryOperatorKind.UInt;
				break;
			case SpecialType.System_Int64:
				binaryOperatorKind = BinaryOperatorKind.Long;
				break;
			case SpecialType.System_UInt64:
				binaryOperatorKind = BinaryOperatorKind.ULong;
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(type.SpecialType);
			}
			break;
		}
		case UnaryOperatorKind.Pointer:
			binaryOperatorKind = BinaryOperatorKind.PointerAndInt;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(operatorKind.OperandTypes());
		}
		if ((uint)(binaryOperatorKind - 21) <= 3u || binaryOperatorKind == BinaryOperatorKind.PointerAndInt)
		{
			binaryOperatorKind = (BinaryOperatorKind)((int)binaryOperatorKind | (int)operatorKind.OverflowChecks());
		}
		if (operatorKind.IsLifted())
		{
			binaryOperatorKind |= BinaryOperatorKind.Lifted;
		}
		return binaryOperatorKind;
	}

	private static ConstantValue GetConstantOneForBinOp(BinaryOperatorKind binaryOperatorKind)
	{
		switch (binaryOperatorKind.OperandTypes())
		{
		case BinaryOperatorKind.Int:
		case BinaryOperatorKind.PointerAndInt:
			return ConstantValue.Create(1);
		case BinaryOperatorKind.UInt:
			return ConstantValue.Create(1u);
		case BinaryOperatorKind.Long:
			return ConstantValue.Create(1L);
		case BinaryOperatorKind.ULong:
			return ConstantValue.Create(1uL);
		case BinaryOperatorKind.Float:
			return ConstantValue.Create(1f);
		case BinaryOperatorKind.Double:
			return ConstantValue.Create(1.0);
		case BinaryOperatorKind.Decimal:
			return ConstantValue.Create(1m);
		default:
			throw ExceptionUtilities.UnexpectedValue(binaryOperatorKind.OperandTypes());
		}
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)Visit(node.Body);
		BoundBlock boundBlock = ((boundStatement.Kind == BoundKind.Block) ? ((BoundBlock)boundStatement) : BoundBlock.SynthesizedNoLocals(node.Syntax, boundStatement));
		if (node.ExpressionOpt != null)
		{
			return RewriteExpressionUsingStatement(node, boundBlock);
		}
		SyntaxNode syntax = node.Syntax;
		Conversion iDisposableConversion = node.IDisposableConversion;
		ImmutableArray<BoundLocalDeclaration> localDeclarations = node.DeclarationsOpt.LocalDeclarations;
		BoundBlock boundBlock2 = boundBlock;
		for (int num = localDeclarations.Length - 1; num >= 0; num--)
		{
			boundBlock2 = RewriteDeclarationUsingStatement(syntax, localDeclarations[num], boundBlock2, iDisposableConversion);
		}
		return new BoundBlock(syntax, node.Locals, ImmutableArray.Create((BoundStatement)boundBlock2));
	}

	private BoundBlock RewriteExpressionUsingStatement(BoundUsingStatement node, BoundBlock tryBlock)
	{
		BoundExpression boundExpression = (BoundExpression)Visit(node.ExpressionOpt);
		if (boundExpression.ConstantValue == ConstantValue.Null)
		{
			return tryBlock;
		}
		TypeSymbol type = boundExpression.Type;
		SyntaxNode syntax = boundExpression.Syntax;
		UsingStatementSyntax usingStatementSyntax = (UsingStatementSyntax)node.Syntax;
		BoundLocal boundLocal;
		BoundAssignmentOperator store;
		if ((object)type == null || type.IsDynamic())
		{
			BoundExpression argument = MakeConversionNode(syntax, boundExpression, Conversion.GetTrivialConversion(node.IDisposableConversion.Kind), _compilation.GetSpecialType(SpecialType.System_IDisposable), @checked: false, explicitCastInCode: false, boundExpression.ConstantValue);
			boundLocal = _factory.StoreToTemp(argument, out store, RefKind.None, SynthesizedLocalKind.Using);
		}
		else
		{
			boundLocal = _factory.StoreToTemp(boundExpression, out store, RefKind.None, SynthesizedLocalKind.Using, usingStatementSyntax);
		}
		BoundStatement boundStatement = new BoundExpressionStatement(syntax, store);
		if (Instrument)
		{
			boundStatement = _instrumenter.InstrumentUsingTargetCapture(node, boundStatement);
		}
		BoundStatement item = RewriteUsingStatementTryFinally(usingStatementSyntax, tryBlock, boundLocal);
		return new BoundBlock(usingStatementSyntax, node.Locals.Add(boundLocal.LocalSymbol), ImmutableArray.Create(boundStatement, item));
	}

	private BoundBlock RewriteDeclarationUsingStatement(SyntaxNode usingSyntax, BoundLocalDeclaration localDeclaration, BoundBlock tryBlock, Conversion idisposableConversion)
	{
		SyntaxNode syntax = localDeclaration.Syntax;
		LocalSymbol localSymbol = localDeclaration.LocalSymbol;
		TypeSymbol type = localSymbol.Type;
		BoundLocal boundLocal = new BoundLocal(syntax, localSymbol, localDeclaration.InitializerOpt.ConstantValue, type);
		BoundStatement boundStatement = (BoundStatement)Visit(localDeclaration);
		if (boundLocal.ConstantValue == ConstantValue.Null)
		{
			return BoundBlock.SynthesizedNoLocals(usingSyntax, boundStatement, tryBlock);
		}
		if (type.IsDynamic())
		{
			BoundExpression argument = MakeConversionNode(syntax, boundLocal, idisposableConversion, _compilation.GetSpecialType(SpecialType.System_IDisposable), @checked: false);
			BoundLocal boundLocal2 = _factory.StoreToTemp(argument, out var store, RefKind.None, SynthesizedLocalKind.Using);
			BoundStatement item = RewriteUsingStatementTryFinally(usingSyntax, tryBlock, boundLocal2);
			return new BoundBlock(usingSyntax, ImmutableArray.Create(boundLocal2.LocalSymbol), ImmutableArray.Create(boundStatement, new BoundExpressionStatement(syntax, store), item));
		}
		BoundStatement boundStatement2 = RewriteUsingStatementTryFinally(usingSyntax, tryBlock, boundLocal);
		return BoundBlock.SynthesizedNoLocals(usingSyntax, boundStatement, boundStatement2);
	}

	private BoundStatement RewriteUsingStatementTryFinally(SyntaxNode syntax, BoundBlock tryBlock, BoundLocal local)
	{
		bool flag = local.Type.IsNullableType();
		BoundExpression boundExpression;
		if (flag)
		{
			MethodSymbol method = UnsafeGetNullableMethod(syntax, local.Type, SpecialMember.System_Nullable_T_GetValueOrDefault);
			boundExpression = BoundCall.Synthesized(syntax, local, method);
		}
		else
		{
			boundExpression = local;
		}
		BoundExpression expression = ((!Binder.TryGetSpecialTypeMember<MethodSymbol>(_compilation, SpecialMember.System_IDisposable__Dispose, syntax, _diagnostics, out var symbol)) ? ((BoundExpression)new BoundBadExpression(syntax, LookupResultKind.NotInvocable, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(boundExpression), ErrorTypeSymbol.UnknownResultType)) : ((BoundExpression)BoundCall.Synthesized(syntax, boundExpression, symbol)));
		BoundStatement boundStatement = new BoundExpressionStatement(syntax, expression);
		BoundExpression boundExpression2;
		if (flag)
		{
			boundExpression2 = MakeNullableHasValue(syntax, local);
		}
		else
		{
			boundExpression2 = ((!local.Type.IsValueType) ? MakeNullCheck(syntax, local, BinaryOperatorKind.NotEqual) : null);
		}
		return new BoundTryStatement(finallyBlockOpt: BoundBlock.SynthesizedNoLocals(syntax, (boundExpression2 != null) ? RewriteIfStatement(syntax, boundExpression2, boundStatement, null, hasErrors: false) : boundStatement), syntax: syntax, tryBlock: tryBlock, catchBlocks: ImmutableArray<BoundCatchBlock>.Empty);
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		BoundExpression rewrittenCondition = (BoundExpression)Visit(node.Condition);
		BoundStatement rewrittenBody = (BoundStatement)Visit(node.Body);
		if (!node.WasCompilerGenerated && Instrument)
		{
			rewrittenCondition = _instrumenter.InstrumentWhileStatementCondition(node, rewrittenCondition, _factory);
		}
		return RewriteWhileStatement(node, node.Locals, rewrittenCondition, rewrittenBody, node.BreakLabel, node.ContinueLabel, node.HasErrors);
	}

	private BoundStatement RewriteWhileStatement(BoundLoopStatement loop, BoundExpression rewrittenCondition, BoundStatement rewrittenBody, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel, bool hasErrors)
	{
		SyntaxNode syntax = loop.Syntax;
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("start");
		BoundStatement boundStatement = new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: true, label);
		BoundStatement boundStatement2 = new BoundGotoStatement(syntax, continueLabel);
		if (Instrument && !loop.WasCompilerGenerated)
		{
			boundStatement = loop.Kind switch
			{
				BoundKind.WhileStatement => _instrumenter.InstrumentWhileStatementConditionalGotoStartOrBreak((BoundWhileStatement)loop, boundStatement), 
				BoundKind.ForEachStatement => _instrumenter.InstrumentForEachStatementConditionalGotoStart((BoundForEachStatement)loop, boundStatement), 
				_ => throw ExceptionUtilities.UnexpectedValue(loop.Kind), 
			};
			boundStatement2 = new BoundSequencePoint(null, boundStatement2);
		}
		return BoundStatementList.Synthesized(syntax, hasErrors, boundStatement2, new BoundLabelStatement(syntax, label), rewrittenBody, new BoundLabelStatement(syntax, continueLabel), boundStatement, new BoundLabelStatement(syntax, breakLabel));
	}

	private BoundStatement RewriteWhileStatement(BoundWhileStatement loop, ImmutableArray<LocalSymbol> locals, BoundExpression rewrittenCondition, BoundStatement rewrittenBody, GeneratedLabelSymbol breakLabel, GeneratedLabelSymbol continueLabel, bool hasErrors)
	{
		if (locals.IsEmpty)
		{
			return RewriteWhileStatement(loop, rewrittenCondition, rewrittenBody, breakLabel, continueLabel, hasErrors);
		}
		SyntaxNode syntax = loop.Syntax;
		BoundStatement boundStatement = new BoundLabelStatement(syntax, continueLabel);
		BoundStatement boundStatement2 = new BoundConditionalGoto(rewrittenCondition.Syntax, rewrittenCondition, jumpIfTrue: false, breakLabel);
		if (Instrument && !loop.WasCompilerGenerated)
		{
			boundStatement2 = _instrumenter.InstrumentWhileStatementConditionalGotoStartOrBreak(loop, boundStatement2);
			boundStatement = new BoundSequencePoint(null, boundStatement);
		}
		return BoundStatementList.Synthesized(syntax, hasErrors, boundStatement, new BoundBlock(syntax, locals, ImmutableArray.Create(boundStatement2, rewrittenBody, new BoundGotoStatement(syntax, continueLabel))), new BoundLabelStatement(syntax, breakLabel));
	}

	public override BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)base.VisitYieldBreakStatement(node);
		if (Instrument)
		{
			if (!node.WasCompilerGenerated)
			{
				goto IL_004b;
			}
			if (node.Syntax.Kind() == SyntaxKind.Block)
			{
				MethodSymbol currentFunction = _factory.CurrentFunction;
				if ((object)currentFunction != null && !currentFunction.IsAsync)
				{
					goto IL_004b;
				}
			}
		}
		goto IL_0059;
		IL_004b:
		boundStatement = _instrumenter.InstrumentYieldBreakStatement(node, boundStatement);
		goto IL_0059;
		IL_0059:
		return boundStatement;
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		BoundStatement boundStatement = (BoundStatement)base.VisitYieldReturnStatement(node);
		if (Instrument && !node.WasCompilerGenerated)
		{
			boundStatement = _instrumenter.InstrumentYieldReturnStatement(node, boundStatement);
		}
		return boundStatement;
	}
}
