using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal sealed class CodeGenerator
{
	private enum IndirectReturnState : byte
	{
		NotNeeded,
		Needed,
		Emitted
	}

	private enum AddressKind
	{
		Writeable,
		Constrained,
		ReadOnly,
		ReadOnlyStrict
	}

	private enum ArrayInitializerStyle
	{
		Element,
		Block,
		Mixed
	}

	private struct IndexDesc(int index, ImmutableArray<BoundExpression> initializers)
	{
		public readonly int Index = index;

		public readonly ImmutableArray<BoundExpression> Initializers = initializers;
	}

	private class EmitCancelledException : Exception
	{
	}

	private enum UseKind
	{
		Unused,
		UsedAsValue,
		UsedAsAddress
	}

	private enum CallKind
	{
		Call,
		CallVirt,
		ConstrainedCallVirt
	}

	private class FinallyCloner : BoundTreeRewriterWithStackGuardWithoutRecursionOnTheLeftOfBinaryOperator
	{
		private Dictionary<LabelSymbol, GeneratedLabelSymbol> _labelClones;

		private FinallyCloner()
		{
		}

		public static BoundBlock MakeFinallyClone(BoundTryStatement node)
		{
			return (BoundBlock)new FinallyCloner().Visit(node.FinallyBlockOpt);
		}

		public override BoundNode VisitLabelStatement(BoundLabelStatement node)
		{
			return node.Update(GetLabelClone(node.Label));
		}

		public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
		{
			GeneratedLabelSymbol labelClone = GetLabelClone(node.BreakLabel);
			BoundStatement loweredPreambleOpt = (BoundStatement)Visit(node.LoweredPreambleOpt);
			BoundExpression expression = node.Expression;
			ImmutableArray<BoundSwitchSection> switchSections = VisitList(node.SwitchSections);
			return node.Update(loweredPreambleOpt, expression, node.ConstantTargetOpt, node.InnerLocals, node.InnerLocalFunctions, switchSections, labelClone, node.StringEquality);
		}

		public override BoundNode VisitSwitchLabel(BoundSwitchLabel node)
		{
			GeneratedLabelSymbol labelClone = GetLabelClone(node.Label);
			BoundExpression expressionOpt = node.ExpressionOpt;
			return node.Update(labelClone, expressionOpt, node.ConstantValueOpt);
		}

		public override BoundNode VisitGotoStatement(BoundGotoStatement node)
		{
			GeneratedLabelSymbol labelClone = GetLabelClone(node.Label);
			BoundExpression caseExpressionOpt = node.CaseExpressionOpt;
			BoundLabel labelExpressionOpt = node.LabelExpressionOpt;
			return node.Update(labelClone, caseExpressionOpt, labelExpressionOpt);
		}

		public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
		{
			GeneratedLabelSymbol labelClone = GetLabelClone(node.Label);
			BoundExpression condition = node.Condition;
			return node.Update(condition, node.JumpIfTrue, labelClone);
		}

		public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
		{
			return node;
		}

		private GeneratedLabelSymbol GetLabelClone(LabelSymbol label)
		{
			Dictionary<LabelSymbol, GeneratedLabelSymbol> dictionary = _labelClones;
			if (dictionary == null)
			{
				dictionary = (_labelClones = new Dictionary<LabelSymbol, GeneratedLabelSymbol>());
			}
			if (!dictionary.TryGetValue(label, out var value))
			{
				value = new GeneratedLabelSymbol("cloned_" + label.Name);
				dictionary.Add(label, value);
			}
			return value;
		}
	}

	private readonly MethodSymbol _method;

	private readonly SyntaxNode _methodBodySyntaxOpt;

	private readonly BoundStatement _boundBody;

	private readonly ILBuilder _builder;

	private readonly PEModuleBuilder _module;

	private readonly DiagnosticBag _diagnostics;

	private readonly ILEmitStyle _ilEmitStyle;

	private readonly bool _emitPdbSequencePoints;

	private readonly HashSet<LocalSymbol> _stackLocals;

	private ArrayBuilder<LocalDefinition> _expressionTemps;

	private int _tryNestingLevel;

	private readonly SynthesizedLocalOrdinalsDispenser _synthesizedLocalOrdinals = new SynthesizedLocalOrdinalsDispenser();

	private int _uniqueNameId;

	private static readonly object s_returnLabel = new object();

	private int _asyncCatchHandlerOffset = -1;

	private ArrayBuilder<int> _asyncYieldPoints;

	private ArrayBuilder<int> _asyncResumePoints;

	private IndirectReturnState _indirectReturnState;

	private LocalDefinition _returnTemp;

	private int _recursionDepth;

	private static readonly ILOpCode[] s_compOpCodes = new ILOpCode[12]
	{
		ILOpCode.Clt,
		ILOpCode.Cgt,
		ILOpCode.Cgt,
		ILOpCode.Clt,
		ILOpCode.Clt_un,
		ILOpCode.Cgt_un,
		ILOpCode.Cgt_un,
		ILOpCode.Clt_un,
		ILOpCode.Clt,
		ILOpCode.Cgt_un,
		ILOpCode.Cgt,
		ILOpCode.Clt_un
	};

	private const int IL_OP_CODE_ROW_LENGTH = 4;

	private static readonly ILOpCode[] s_condJumpOpCodes = new ILOpCode[24]
	{
		ILOpCode.Blt,
		ILOpCode.Ble,
		ILOpCode.Bgt,
		ILOpCode.Bge,
		ILOpCode.Bge,
		ILOpCode.Bgt,
		ILOpCode.Ble,
		ILOpCode.Blt,
		ILOpCode.Blt_un,
		ILOpCode.Ble_un,
		ILOpCode.Bgt_un,
		ILOpCode.Bge_un,
		ILOpCode.Bge_un,
		ILOpCode.Bgt_un,
		ILOpCode.Ble_un,
		ILOpCode.Blt_un,
		ILOpCode.Blt,
		ILOpCode.Ble,
		ILOpCode.Bgt,
		ILOpCode.Bge,
		ILOpCode.Bge_un,
		ILOpCode.Bgt_un,
		ILOpCode.Ble_un,
		ILOpCode.Blt_un
	};

	private LocalDefinition LazyReturnTemp
	{
		get
		{
			LocalDefinition localDefinition = _returnTemp;
			if (localDefinition == null)
			{
				LocalSlotConstraints localSlotConstraints = ((_method.RefKind != RefKind.None) ? LocalSlotConstraints.ByRef : LocalSlotConstraints.None);
				SyntaxNode methodBodySyntaxOpt = _methodBodySyntaxOpt;
				if (_ilEmitStyle == ILEmitStyle.Debug && methodBodySyntaxOpt != null)
				{
					int syntaxOffset = _method.CalculateLocalSyntaxOffset(methodBodySyntaxOpt.SpanStart, methodBodySyntaxOpt.SyntaxTree);
					SynthesizedLocal synthesizedLocal = new SynthesizedLocal(_method, _method.ReturnType, SynthesizedLocalKind.FunctionReturnValue, methodBodySyntaxOpt);
					localDefinition = _builder.LocalSlotManager.DeclareLocal(_module.Translate(synthesizedLocal.Type, methodBodySyntaxOpt, _diagnostics), synthesizedLocal, null, synthesizedLocal.SynthesizedKind, new LocalDebugId(syntaxOffset), synthesizedLocal.SynthesizedKind.PdbAttributes(), localSlotConstraints, ImmutableArray<bool>.Empty, ImmutableArray<string>.Empty, isSlotReusable: false);
				}
				else
				{
					localDefinition = AllocateTemp(_method.ReturnType, _boundBody.Syntax, localSlotConstraints);
				}
				_returnTemp = localDefinition;
			}
			return localDefinition;
		}
	}

	public CodeGenerator(MethodSymbol method, BoundStatement boundBody, ILBuilder builder, PEModuleBuilder moduleBuilder, DiagnosticBag diagnostics, OptimizationLevel optimizations, bool emittingPdb)
	{
		_method = method;
		_boundBody = boundBody;
		_builder = builder;
		_module = moduleBuilder;
		_diagnostics = diagnostics;
		if (!method.GenerateDebugInfo)
		{
			_ilEmitStyle = ILEmitStyle.Release;
		}
		else if (optimizations == OptimizationLevel.Debug)
		{
			_ilEmitStyle = ILEmitStyle.Debug;
		}
		else
		{
			_ilEmitStyle = (IsDebugPlus() ? ILEmitStyle.DebugFriendlyRelease : ILEmitStyle.Release);
		}
		_emitPdbSequencePoints = emittingPdb && method.GenerateDebugInfo;
		try
		{
			_boundBody = Optimizer.Optimize(boundBody, _ilEmitStyle != ILEmitStyle.Release, out _stackLocals);
		}
		catch (BoundTreeVisitor.CancelledByStackGuardException ex)
		{
			ex.AddAnError(diagnostics);
			_boundBody = boundBody;
		}
		SourceMemberMethodSymbol sourceMemberMethodSymbol = method as SourceMemberMethodSymbol;
		(BlockSyntax, ArrowExpressionClauseSyntax) tuple = sourceMemberMethodSymbol?.Bodies ?? default((BlockSyntax, ArrowExpressionClauseSyntax));
		BlockSyntax item = tuple.Item1;
		ArrowExpressionClauseSyntax item2 = tuple.Item2;
		_methodBodySyntaxOpt = item ?? item2 ?? sourceMemberMethodSymbol?.SyntaxNode;
	}

	private bool IsDebugPlus()
	{
		return _module.Compilation.Options.DebugPlusMode;
	}

	private bool EnablePEVerifyCompat()
	{
		if (_module.Compilation.LanguageVersion >= LanguageVersion.CSharp7_2)
		{
			return _module.Compilation.FeaturePEVerifyCompatEnabled;
		}
		return true;
	}

	private bool IsStackLocal(LocalSymbol local)
	{
		if (_stackLocals != null)
		{
			return _stackLocals.Contains(local);
		}
		return false;
	}

	public void Generate()
	{
		GenerateImpl();
	}

	public void Generate(out int asyncCatchHandlerOffset, out ImmutableArray<int> asyncYieldPoints, out ImmutableArray<int> asyncResumePoints)
	{
		GenerateImpl();
		asyncCatchHandlerOffset = _builder.GetILOffsetFromMarker(_asyncCatchHandlerOffset);
		ArrayBuilder<int> asyncYieldPoints2 = _asyncYieldPoints;
		ArrayBuilder<int> asyncResumePoints2 = _asyncResumePoints;
		if (asyncYieldPoints2 == null)
		{
			asyncYieldPoints = ImmutableArray<int>.Empty;
			asyncResumePoints = ImmutableArray<int>.Empty;
			return;
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
		ArrayBuilder<int> instance2 = ArrayBuilder<int>.GetInstance();
		int count = asyncYieldPoints2.Count;
		for (int i = 0; i < count; i++)
		{
			int iLOffsetFromMarker = _builder.GetILOffsetFromMarker(asyncYieldPoints2[i]);
			int iLOffsetFromMarker2 = _builder.GetILOffsetFromMarker(asyncResumePoints2[i]);
			if (iLOffsetFromMarker > 0)
			{
				instance.Add(iLOffsetFromMarker);
				instance2.Add(iLOffsetFromMarker2);
			}
		}
		asyncYieldPoints = instance.ToImmutableAndFree();
		asyncResumePoints = instance2.ToImmutableAndFree();
		asyncYieldPoints2.Free();
		asyncResumePoints2.Free();
	}

	private void GenerateImpl()
	{
		SetInitialDebugDocument();
		if (_emitPdbSequencePoints && _method.IsImplicitlyDeclared)
		{
			_builder.DefineInitialHiddenSequencePoint();
		}
		try
		{
			EmitStatement(_boundBody);
			if (_indirectReturnState == IndirectReturnState.Needed)
			{
				HandleReturn();
			}
			if (!_diagnostics.HasAnyErrors())
			{
				_builder.Realize();
			}
		}
		catch (EmitCancelledException)
		{
		}
		_synthesizedLocalOrdinals.Free();
		_expressionTemps?.Free();
	}

	private void HandleReturn()
	{
		_builder.MarkLabel(s_returnLabel);
		if (_emitPdbSequencePoints && !_method.IsIterator && !_method.IsAsync && _methodBodySyntaxOpt is BlockSyntax blockSyntax)
		{
			EmitSequencePoint(blockSyntax.SyntaxTree, blockSyntax.CloseBraceToken.Span);
		}
		if (_returnTemp != null)
		{
			_builder.EmitLocalLoad(LazyReturnTemp);
			_builder.EmitRet(isVoid: false);
		}
		else
		{
			_builder.EmitRet(isVoid: true);
		}
		_indirectReturnState = IndirectReturnState.Emitted;
	}

	private void EmitTypeReferenceToken(ITypeReference symbol, SyntaxNode syntaxNode)
	{
		_builder.EmitToken(symbol, syntaxNode, _diagnostics);
	}

	private void EmitSymbolToken(TypeSymbol symbol, SyntaxNode syntaxNode)
	{
		EmitTypeReferenceToken(_module.Translate(symbol, syntaxNode, _diagnostics), syntaxNode);
	}

	private void EmitSymbolToken(MethodSymbol method, SyntaxNode syntaxNode, BoundArgListOperator optArgList, bool encodeAsRawDefinitionToken = false)
	{
		_builder.EmitToken(_module.Translate(method, syntaxNode, _diagnostics, optArgList, encodeAsRawDefinitionToken), syntaxNode, _diagnostics, encodeAsRawDefinitionToken);
	}

	private void EmitSymbolToken(FieldSymbol symbol, SyntaxNode syntaxNode)
	{
		_builder.EmitToken(_module.Translate(symbol, syntaxNode, _diagnostics), syntaxNode, _diagnostics);
	}

	private void EmitSequencePointStatement(BoundSequencePoint node)
	{
		SyntaxNode syntax = node.Syntax;
		if (_emitPdbSequencePoints)
		{
			if (syntax == null)
			{
				EmitHiddenSequencePoint();
			}
			else
			{
				EmitSequencePoint(syntax);
			}
		}
		BoundStatement statementOpt = node.StatementOpt;
		int num = 0;
		if (statementOpt != null)
		{
			num = EmitStatementAndCountInstructions(statementOpt);
		}
		if (num == 0 && syntax != null && _ilEmitStyle == ILEmitStyle.Debug)
		{
			_builder.EmitOpCode(ILOpCode.Nop);
		}
	}

	private void EmitSequencePointStatement(BoundSequencePointWithSpan node)
	{
		TextSpan span = node.Span;
		if (span != default(TextSpan) && _emitPdbSequencePoints)
		{
			EmitSequencePoint(node.SyntaxTree, span);
		}
		BoundStatement statementOpt = node.StatementOpt;
		int num = 0;
		if (statementOpt != null)
		{
			num = EmitStatementAndCountInstructions(statementOpt);
		}
		if (num == 0 && span != default(TextSpan) && _ilEmitStyle == ILEmitStyle.Debug)
		{
			_builder.EmitOpCode(ILOpCode.Nop);
		}
	}

	private void SetInitialDebugDocument()
	{
		if (_emitPdbSequencePoints && _methodBodySyntaxOpt != null)
		{
			_builder.SetInitialDebugDocument(_methodBodySyntaxOpt.SyntaxTree);
		}
	}

	private void EmitHiddenSequencePoint()
	{
		_builder.DefineHiddenSequencePoint();
	}

	private void EmitSequencePoint(SyntaxNode syntax)
	{
		EmitSequencePoint(syntax.SyntaxTree, syntax.Span);
	}

	private TextSpan EmitSequencePoint(SyntaxTree syntaxTree, TextSpan span)
	{
		_builder.DefineSequencePoint(syntaxTree, span);
		return span;
	}

	private void AddExpressionTemp(LocalDefinition temp)
	{
		if (temp != null)
		{
			ArrayBuilder<LocalDefinition> arrayBuilder = _expressionTemps;
			if (arrayBuilder == null)
			{
				arrayBuilder = (_expressionTemps = ArrayBuilder<LocalDefinition>.GetInstance());
			}
			arrayBuilder.Add(temp);
		}
	}

	private void ReleaseExpressionTemps()
	{
		ArrayBuilder<LocalDefinition> expressionTemps = _expressionTemps;
		if (expressionTemps != null && expressionTemps.Count > 0)
		{
			for (int num = _expressionTemps.Count - 1; num >= 0; num--)
			{
				LocalDefinition temp = _expressionTemps[num];
				FreeTemp(temp);
			}
			_expressionTemps.Clear();
		}
	}

	private static bool IsReadOnly(AddressKind addressKind)
	{
		return addressKind >= AddressKind.ReadOnly;
	}

	private LocalDefinition EmitAddress(BoundExpression expression, AddressKind addressKind)
	{
		switch (expression.Kind)
		{
		case BoundKind.RefValueOperator:
			EmitRefValueAddress((BoundRefValueOperator)expression);
			break;
		case BoundKind.Local:
			return EmitLocalAddress((BoundLocal)expression, addressKind);
		case BoundKind.Dup:
			return EmitDupAddress((BoundDup)expression, addressKind);
		case BoundKind.ComplexConditionalReceiver:
			EmitComplexConditionalReceiverAddress((BoundComplexConditionalReceiver)expression);
			break;
		case BoundKind.Parameter:
			return EmitParameterAddress((BoundParameter)expression, addressKind);
		case BoundKind.FieldAccess:
			return EmitFieldAddress((BoundFieldAccess)expression, addressKind);
		case BoundKind.ArrayAccess:
			if (HasHome(expression, addressKind))
			{
				EmitArrayElementAddress((BoundArrayAccess)expression, addressKind);
				break;
			}
			goto default;
		case BoundKind.ThisReference:
			if (expression.Type.IsValueType)
			{
				_builder.EmitLoadArgumentOpcode(0);
			}
			else
			{
				_builder.EmitLoadArgumentAddrOpcode(0);
			}
			break;
		case BoundKind.PreviousSubmissionReference:
			throw ExceptionUtilities.UnexpectedValue(expression.Kind);
		case BoundKind.PassByCopy:
			return EmitPassByCopyAddress((BoundPassByCopy)expression, addressKind);
		case BoundKind.Sequence:
			return EmitSequenceAddress((BoundSequence)expression, addressKind);
		case BoundKind.PointerIndirectionOperator:
		{
			BoundExpression operand = ((BoundPointerIndirectionOperator)expression).Operand;
			EmitExpression(operand, used: true);
			break;
		}
		case BoundKind.PseudoVariable:
			EmitPseudoVariableAddress((BoundPseudoVariable)expression);
			break;
		case BoundKind.Call:
		{
			BoundCall boundCall = (BoundCall)expression;
			RefKind refKind = boundCall.Method.RefKind;
			if (refKind == RefKind.Ref || (IsReadOnly(addressKind) && refKind == RefKind.In))
			{
				EmitCallExpression(boundCall, UseKind.UsedAsAddress);
				break;
			}
			goto default;
		}
		case BoundKind.DefaultExpression:
		{
			TypeSymbol type = expression.Type;
			LocalDefinition localDefinition = AllocateTemp(type, expression.Syntax);
			_builder.EmitLocalAddress(localDefinition);
			_builder.EmitOpCode(ILOpCode.Dup);
			_builder.EmitOpCode(ILOpCode.Initobj);
			EmitSymbolToken(type, expression.Syntax);
			return localDefinition;
		}
		case BoundKind.ConditionalOperator:
			if (HasHome(expression, addressKind))
			{
				EmitConditionalOperatorAddress((BoundConditionalOperator)expression, addressKind);
				break;
			}
			goto default;
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expression;
			if (boundAssignmentOperator.IsRef && HasHome(boundAssignmentOperator, addressKind))
			{
				EmitAssignmentExpression(boundAssignmentOperator, UseKind.UsedAsAddress);
				break;
			}
			goto default;
		}
		case BoundKind.ThrowExpression:
			EmitExpression(expression, used: true);
			return null;
		default:
			return EmitAddressOfTempClone(expression);
		case BoundKind.BaseReference:
		case BoundKind.ConditionalReceiver:
			break;
		}
		return null;
	}

	private LocalDefinition EmitPassByCopyAddress(BoundPassByCopy passByCopyExpr, AddressKind addressKind)
	{
		if (passByCopyExpr.Expression is BoundSequence boundSequence && DigForValueLocal(boundSequence, boundSequence.Value) != null)
		{
			return EmitSequenceAddress(boundSequence, addressKind);
		}
		return EmitAddressOfTempClone(passByCopyExpr);
	}

	private void EmitConditionalOperatorAddress(BoundConditionalOperator expr, AddressKind addressKind)
	{
		object dest = new object();
		object label = new object();
		EmitCondBranch(expr.Condition, ref dest, sense: true);
		AddExpressionTemp(EmitAddress(expr.Alternative, addressKind));
		_builder.EmitBranch(ILOpCode.Br, label);
		_builder.AdjustStack(-1);
		_builder.MarkLabel(dest);
		AddExpressionTemp(EmitAddress(expr.Consequence, addressKind));
		_builder.MarkLabel(label);
	}

	private void EmitComplexConditionalReceiverAddress(BoundComplexConditionalReceiver expression)
	{
		TypeSymbol type = expression.Type;
		object label = new object();
		object label2 = new object();
		EmitInitObj(type, used: true, expression.Syntax);
		EmitBox(type, expression.Syntax);
		_builder.EmitBranch(ILOpCode.Brtrue, label);
		EmitAddress(expression.ReferenceTypeReceiver, AddressKind.ReadOnly);
		_builder.EmitBranch(ILOpCode.Br, label2);
		_builder.AdjustStack(-1);
		_builder.MarkLabel(label);
		EmitReceiverRef(expression.ValueTypeReceiver, AddressKind.Constrained);
		_builder.MarkLabel(label2);
	}

	private LocalDefinition EmitLocalAddress(BoundLocal localAccess, AddressKind addressKind)
	{
		LocalSymbol localSymbol = localAccess.LocalSymbol;
		if (!HasHome(localAccess, addressKind))
		{
			return EmitAddressOfTempClone(localAccess);
		}
		if (IsStackLocal(localSymbol))
		{
			if (localSymbol.RefKind == RefKind.None)
			{
				throw ExceptionUtilities.UnexpectedValue(localSymbol.RefKind);
			}
		}
		else
		{
			_builder.EmitLocalAddress(GetLocal(localAccess));
		}
		return null;
	}

	private LocalDefinition EmitDupAddress(BoundDup dup, AddressKind addressKind)
	{
		if (!HasHome(dup, addressKind))
		{
			return EmitAddressOfTempClone(dup);
		}
		_builder.EmitOpCode(ILOpCode.Dup);
		return null;
	}

	private void EmitPseudoVariableAddress(BoundPseudoVariable expression)
	{
		EmitExpression(expression.EmitExpressions.GetAddress(expression), used: true);
	}

	private void EmitRefValueAddress(BoundRefValueOperator refValue)
	{
		EmitExpression(refValue.Operand, used: true);
		_builder.EmitOpCode(ILOpCode.Refanyval);
		EmitSymbolToken(refValue.Type, refValue.Syntax);
	}

	private LocalDefinition EmitAddressOfTempClone(BoundExpression expression)
	{
		EmitExpression(expression, used: true);
		LocalDefinition localDefinition = AllocateTemp(expression.Type, expression.Syntax);
		_builder.EmitLocalStore(localDefinition);
		_builder.EmitLocalAddress(localDefinition);
		return localDefinition;
	}

	private LocalDefinition EmitSequenceAddress(BoundSequence sequence, AddressKind addressKind)
	{
		DefineAndRecordLocals(sequence);
		EmitSideEffects(sequence);
		LocalDefinition result = EmitAddress(sequence.Value, addressKind);
		CloseScopeAndKeepLocals(sequence);
		return result;
	}

	private LocalSymbol DigForValueLocal(BoundSequence topSequence, BoundExpression value)
	{
		switch (value.Kind)
		{
		case BoundKind.Local:
		{
			LocalSymbol localSymbol = ((BoundLocal)value).LocalSymbol;
			if (topSequence.Locals.Contains(localSymbol))
			{
				return localSymbol;
			}
			break;
		}
		case BoundKind.Sequence:
			return DigForValueLocal(topSequence, ((BoundSequence)value).Value);
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)value;
			if (!boundFieldAccess.FieldSymbol.IsStatic)
			{
				BoundExpression receiverOpt = boundFieldAccess.ReceiverOpt;
				if (!receiverOpt.Type.IsReferenceType)
				{
					return DigForValueLocal(topSequence, receiverOpt);
				}
			}
			break;
		}
		}
		return null;
	}

	private bool HasHome(BoundExpression expression, AddressKind addressKind)
	{
		switch (expression.Kind)
		{
		case BoundKind.ArrayAccess:
			if (addressKind == AddressKind.ReadOnly && !expression.Type.IsValueType && EnablePEVerifyCompat())
			{
				return false;
			}
			return true;
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.RefValueOperator:
			return true;
		case BoundKind.ThisReference:
		{
			TypeSymbol type = expression.Type;
			if (type.IsReferenceType)
			{
				return true;
			}
			if (!IsReadOnly(addressKind) && type.IsReadOnly)
			{
				return _method.MethodKind == MethodKind.Constructor;
			}
			return true;
		}
		case BoundKind.ThrowExpression:
			return true;
		case BoundKind.Parameter:
			if (!IsReadOnly(addressKind))
			{
				return ((BoundParameter)expression).ParameterSymbol.RefKind != RefKind.In;
			}
			return true;
		case BoundKind.Local:
		{
			LocalSymbol localSymbol = ((BoundLocal)expression).LocalSymbol;
			if (!IsStackLocal(localSymbol) || localSymbol.RefKind != RefKind.None)
			{
				if (!IsReadOnly(addressKind))
				{
					return localSymbol.RefKind != RefKind.In;
				}
				return true;
			}
			return false;
		}
		case BoundKind.Call:
		{
			RefKind refKind3 = ((BoundCall)expression).Method.RefKind;
			if (refKind3 != RefKind.Ref)
			{
				if (IsReadOnly(addressKind))
				{
					return refKind3 == RefKind.In;
				}
				return false;
			}
			return true;
		}
		case BoundKind.Dup:
		{
			RefKind refKind = ((BoundDup)expression).RefKind;
			if (refKind != RefKind.Ref)
			{
				if (IsReadOnly(addressKind))
				{
					return refKind == RefKind.In;
				}
				return false;
			}
			return true;
		}
		case BoundKind.FieldAccess:
			return HasHome((BoundFieldAccess)expression, addressKind);
		case BoundKind.Sequence:
			return HasHome(((BoundSequence)expression).Value, addressKind);
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expression;
			if (!boundAssignmentOperator.IsRef)
			{
				return false;
			}
			RefKind refKind2 = boundAssignmentOperator.Left.GetRefKind();
			if (refKind2 != RefKind.Ref)
			{
				if (IsReadOnly(addressKind))
				{
					return refKind2 == RefKind.In;
				}
				return false;
			}
			return true;
		}
		case BoundKind.ConditionalReceiver:
		case BoundKind.ComplexConditionalReceiver:
			return true;
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator boundConditionalOperator = (BoundConditionalOperator)expression;
			if (!boundConditionalOperator.IsRef)
			{
				return false;
			}
			if (HasHome(boundConditionalOperator.Consequence, addressKind))
			{
				return HasHome(boundConditionalOperator.Alternative, addressKind);
			}
			return false;
		}
		default:
			return false;
		}
	}

	private bool HasHome(BoundFieldAccess fieldAccess, AddressKind addressKind)
	{
		FieldSymbol fieldSymbol = fieldAccess.FieldSymbol;
		if (fieldSymbol.IsConst)
		{
			return false;
		}
		switch (addressKind)
		{
		case AddressKind.ReadOnlyStrict:
			return true;
		case AddressKind.ReadOnly:
			if (!EnablePEVerifyCompat())
			{
				return true;
			}
			break;
		}
		if (fieldAccess.IsByValue)
		{
			return false;
		}
		if (!fieldSymbol.IsReadOnly)
		{
			if (!EnablePEVerifyCompat())
			{
				BoundExpression receiverOpt = fieldAccess.ReceiverOpt;
				if (receiverOpt != null && receiverOpt.Type.IsValueType)
				{
					if (!HasHome(receiverOpt, addressKind))
					{
						return !HasHome(receiverOpt, AddressKind.ReadOnly);
					}
					return true;
				}
			}
			return true;
		}
		if (fieldSymbol.ContainingType != _method.ContainingType)
		{
			return false;
		}
		if (fieldSymbol.IsStatic)
		{
			return _method.MethodKind == MethodKind.StaticConstructor;
		}
		if (_method.MethodKind == MethodKind.Constructor)
		{
			return fieldAccess.ReceiverOpt.Kind == BoundKind.ThisReference;
		}
		return false;
	}

	private void EmitArrayIndices(ImmutableArray<BoundExpression> indices)
	{
		for (int i = 0; i < indices.Length; i++)
		{
			BoundExpression boundExpression = indices[i];
			EmitExpression(boundExpression, used: true);
			TreatLongsAsNative(boundExpression.Type.PrimitiveTypeCode);
		}
	}

	private void EmitArrayElementAddress(BoundArrayAccess arrayAccess, AddressKind addressKind)
	{
		EmitExpression(arrayAccess.Expression, used: true);
		EmitArrayIndices(arrayAccess.Indices);
		if (ShouldEmitReadOnlyPrefix(arrayAccess, addressKind))
		{
			_builder.EmitOpCode(ILOpCode.Readonly);
		}
		if (((ArrayTypeSymbol)arrayAccess.Expression.Type).IsSZArray)
		{
			_builder.EmitOpCode(ILOpCode.Ldelema);
			TypeSymbol type = arrayAccess.Type;
			EmitSymbolToken(type, arrayAccess.Syntax);
		}
		else
		{
			_builder.EmitArrayElementAddress(PEModuleBuilder.Translate((ArrayTypeSymbol)arrayAccess.Expression.Type), arrayAccess.Syntax, _diagnostics);
		}
	}

	private bool ShouldEmitReadOnlyPrefix(BoundArrayAccess arrayAccess, AddressKind addressKind)
	{
		if (addressKind == AddressKind.Constrained)
		{
			return true;
		}
		if (!IsReadOnly(addressKind))
		{
			return false;
		}
		return !arrayAccess.Type.IsValueType;
	}

	private LocalDefinition EmitFieldAddress(BoundFieldAccess fieldAccess, AddressKind addressKind)
	{
		FieldSymbol fieldSymbol = fieldAccess.FieldSymbol;
		if (!HasHome(fieldAccess, addressKind))
		{
			return EmitAddressOfTempClone(fieldAccess);
		}
		if (fieldAccess.FieldSymbol.IsStatic)
		{
			EmitStaticFieldAddress(fieldSymbol, fieldAccess.Syntax);
			return null;
		}
		return EmitInstanceFieldAddress(fieldAccess, addressKind);
	}

	private void EmitStaticFieldAddress(FieldSymbol field, SyntaxNode syntaxNode)
	{
		_builder.EmitOpCode(ILOpCode.Ldsflda);
		EmitSymbolToken(field, syntaxNode);
	}

	private LocalDefinition EmitParameterAddress(BoundParameter parameter, AddressKind addressKind)
	{
		ParameterSymbol parameterSymbol = parameter.ParameterSymbol;
		if (!HasHome(parameter, addressKind))
		{
			return EmitAddressOfTempClone(parameter);
		}
		int argNumber = ParameterSlot(parameter);
		if (parameterSymbol.RefKind == RefKind.None)
		{
			_builder.EmitLoadArgumentAddrOpcode(argNumber);
		}
		else
		{
			_builder.EmitLoadArgumentOpcode(argNumber);
		}
		return null;
	}

	private LocalDefinition EmitReceiverRef(BoundExpression receiver, AddressKind addressKind)
	{
		TypeSymbol type = receiver.Type;
		if (type.IsVerifierReference())
		{
			EmitExpression(receiver, used: true);
			return null;
		}
		if (type.TypeKind == TypeKind.TypeParameter)
		{
			if (addressKind == AddressKind.Constrained)
			{
				return EmitAddress(receiver, addressKind);
			}
			EmitExpression(receiver, used: true);
			if (receiver.Kind != BoundKind.ConditionalReceiver)
			{
				EmitBox(receiver.Type, receiver.Syntax);
			}
			return null;
		}
		return EmitAddress(receiver, addressKind);
	}

	private LocalDefinition EmitInstanceFieldAddress(BoundFieldAccess fieldAccess, AddressKind addressKind)
	{
		FieldSymbol fieldSymbol = fieldAccess.FieldSymbol;
		LocalDefinition result = EmitReceiverRef(fieldAccess.ReceiverOpt, (addressKind != AddressKind.Constrained) ? addressKind : AddressKind.Writeable);
		_builder.EmitOpCode(ILOpCode.Ldflda);
		EmitSymbolToken(fieldSymbol, fieldAccess.Syntax);
		if (fieldSymbol.IsFixed)
		{
			FieldSymbol fixedElementField = fieldSymbol.FixedImplementationType(_module).FixedElementField;
			if ((object)fixedElementField != null)
			{
				_builder.EmitOpCode(ILOpCode.Ldflda);
				EmitSymbolToken(fixedElementField, fieldAccess.Syntax);
			}
		}
		return result;
	}

	private void EmitArrayInitializers(ArrayTypeSymbol arrayType, BoundArrayInitialization inits)
	{
		ImmutableArray<BoundExpression> initializers = inits.Initializers;
		ArrayInitializerStyle arrayInitializerStyle = ShouldEmitBlockInitializer(arrayType.ElementType, initializers);
		if (arrayInitializerStyle == ArrayInitializerStyle.Element)
		{
			EmitElementInitializers(arrayType, initializers, includeConstants: true);
			return;
		}
		ImmutableArray<byte> rawData = GetRawData(initializers);
		_builder.EmitArrayBlockInitializer(rawData, inits.Syntax, _diagnostics);
		if (arrayInitializerStyle == ArrayInitializerStyle.Mixed)
		{
			EmitElementInitializers(arrayType, initializers, includeConstants: false);
		}
	}

	private void EmitElementInitializers(ArrayTypeSymbol arrayType, ImmutableArray<BoundExpression> inits, bool includeConstants)
	{
		if (!IsMultidimensionalInitializer(inits))
		{
			EmitVectorElementInitializers(arrayType, inits, includeConstants);
		}
		else
		{
			EmitMultidimensionalElementInitializers(arrayType, inits, includeConstants);
		}
	}

	private void EmitVectorElementInitializers(ArrayTypeSymbol arrayType, ImmutableArray<BoundExpression> inits, bool includeConstants)
	{
		for (int i = 0; i < inits.Length; i++)
		{
			BoundExpression boundExpression = inits[i];
			if (ShouldEmitInitExpression(includeConstants, boundExpression))
			{
				_builder.EmitOpCode(ILOpCode.Dup);
				_builder.EmitIntConstant(i);
				EmitExpression(boundExpression, used: true);
				EmitVectorElementStore(arrayType, boundExpression.Syntax);
			}
		}
	}

	private static bool ShouldEmitInitExpression(bool includeConstants, BoundExpression init)
	{
		if (init.IsDefaultValue())
		{
			return false;
		}
		if (!includeConstants)
		{
			return init.ConstantValue == null;
		}
		return true;
	}

	private void EmitMultidimensionalElementInitializers(ArrayTypeSymbol arrayType, ImmutableArray<BoundExpression> inits, bool includeConstants)
	{
		ArrayBuilder<IndexDesc> arrayBuilder = new ArrayBuilder<IndexDesc>();
		for (int i = 0; i < inits.Length; i++)
		{
			arrayBuilder.Push(new IndexDesc(i, ((BoundArrayInitialization)inits[i]).Initializers));
			EmitAllElementInitializersRecursive(arrayType, arrayBuilder, includeConstants);
		}
	}

	private void EmitAllElementInitializersRecursive(ArrayTypeSymbol arrayType, ArrayBuilder<IndexDesc> indices, bool includeConstants)
	{
		ImmutableArray<BoundExpression> initializers = indices.Peek().Initializers;
		if (IsMultidimensionalInitializer(initializers))
		{
			for (int i = 0; i < initializers.Length; i++)
			{
				indices.Push(new IndexDesc(i, ((BoundArrayInitialization)initializers[i]).Initializers));
				EmitAllElementInitializersRecursive(arrayType, indices, includeConstants);
			}
		}
		else
		{
			for (int j = 0; j < initializers.Length; j++)
			{
				BoundExpression boundExpression = initializers[j];
				if (!ShouldEmitInitExpression(includeConstants, boundExpression))
				{
					continue;
				}
				_builder.EmitOpCode(ILOpCode.Dup);
				foreach (IndexDesc index in indices)
				{
					_builder.EmitIntConstant(index.Index);
				}
				_builder.EmitIntConstant(j);
				BoundExpression expression = initializers[j];
				EmitExpression(expression, used: true);
				EmitArrayElementStore(arrayType, boundExpression.Syntax);
			}
		}
		indices.Pop();
	}

	private static ConstantValue AsConstOrDefault(BoundExpression init)
	{
		ConstantValue constantValue = init.ConstantValue;
		if (constantValue != null)
		{
			return constantValue;
		}
		return ConstantValue.Default(init.Type.EnumUnderlyingType().SpecialType);
	}

	private ArrayInitializerStyle ShouldEmitBlockInitializer(TypeSymbol elementType, ImmutableArray<BoundExpression> inits)
	{
		if (!_module.SupportsPrivateImplClass)
		{
			return ArrayInitializerStyle.Element;
		}
		if (elementType.IsEnumType())
		{
			if (!_module.Compilation.EnableEnumArrayBlockInitialization)
			{
				return ArrayInitializerStyle.Element;
			}
			elementType = elementType.EnumUnderlyingType();
		}
		if (elementType.SpecialType.IsBlittable())
		{
			if (_module.GetInitArrayHelper() == null)
			{
				return ArrayInitializerStyle.Element;
			}
			int initCount = 0;
			int constInits = 0;
			InitializerCountRecursive(inits, ref initCount, ref constInits);
			if (initCount > 2)
			{
				if (initCount == constInits)
				{
					return ArrayInitializerStyle.Block;
				}
				int num = Math.Max(3, initCount / 3);
				if (constInits >= num)
				{
					return ArrayInitializerStyle.Mixed;
				}
			}
		}
		return ArrayInitializerStyle.Element;
	}

	private void InitializerCountRecursive(ImmutableArray<BoundExpression> inits, ref int initCount, ref int constInits)
	{
		if (inits.Length == 0)
		{
			return;
		}
		foreach (BoundExpression item in inits)
		{
			if (item is BoundArrayInitialization boundArrayInitialization)
			{
				InitializerCountRecursive(boundArrayInitialization.Initializers, ref initCount, ref constInits);
			}
			else if (!item.IsDefaultValue())
			{
				initCount++;
				if (item.ConstantValue != null)
				{
					constInits++;
				}
			}
		}
	}

	private ImmutableArray<byte> GetRawData(ImmutableArray<BoundExpression> initializers)
	{
		BlobBuilder blobBuilder = new BlobBuilder(initializers.Length * 4);
		SerializeArrayRecursive(blobBuilder, initializers);
		return blobBuilder.ToImmutableArray();
	}

	private void SerializeArrayRecursive(BlobBuilder bw, ImmutableArray<BoundExpression> inits)
	{
		if (inits.Length == 0)
		{
			return;
		}
		if (inits[0].Kind == BoundKind.ArrayInitialization)
		{
			foreach (BoundExpression item in inits)
			{
				SerializeArrayRecursive(bw, ((BoundArrayInitialization)item).Initializers);
			}
		}
		else
		{
			foreach (BoundExpression item2 in inits)
			{
				AsConstOrDefault(item2).Serialize(bw);
			}
		}
	}

	private static bool IsMultidimensionalInitializer(ImmutableArray<BoundExpression> inits)
	{
		if (inits.Length != 0)
		{
			return inits[0].Kind == BoundKind.ArrayInitialization;
		}
		return false;
	}

	private bool TryEmitReadonlySpanAsBlobWrapper(NamedTypeSymbol spanType, BoundExpression wrappedExpression, bool used, bool inPlace)
	{
		ImmutableArray<byte> data = default;
		int num = -1;
		TypeSymbol typeSymbol = null;
		if (!_module.SupportsPrivateImplClass)
		{
			return false;
		}
		MethodSymbol methodSymbol = (MethodSymbol)_module.Compilation.GetWellKnownTypeMember(WellKnownMember.System_ReadOnlySpan_T__ctor);
		if (methodSymbol == null)
		{
			return false;
		}
		if (wrappedExpression is BoundArrayCreation boundArrayCreation)
		{
			typeSymbol = ((ArrayTypeSymbol)boundArrayCreation.Type).ElementType.EnumUnderlyingType();
			if (typeSymbol.SpecialType.SizeInBytes() != 1)
			{
				return false;
			}
			num = TryGetRawDataForArrayInit(boundArrayCreation.InitializerOpt, out data);
		}
		if (num < 0)
		{
			return false;
		}
		if (!inPlace && !used)
		{
			return true;
		}
		if (num == 0)
		{
			if (inPlace)
			{
				_builder.EmitOpCode(ILOpCode.Initobj);
				EmitSymbolToken(spanType, wrappedExpression.Syntax);
			}
			else
			{
				EmitDefaultValue(spanType, used, wrappedExpression.Syntax);
			}
		}
		else
		{
			if (EnablePEVerifyCompat())
			{
				return false;
			}
			_builder.EmitArrayBlockFieldRef(data, typeSymbol, wrappedExpression.Syntax, _diagnostics);
			_builder.EmitIntConstant(num);
			if (inPlace)
			{
				_builder.EmitOpCode(ILOpCode.Call, -3);
			}
			else
			{
				_builder.EmitOpCode(ILOpCode.Newobj, -1);
			}
			EmitSymbolToken(methodSymbol.AsMember(spanType), wrappedExpression.Syntax, null);
		}
		return true;
	}

	private int TryGetRawDataForArrayInit(BoundArrayInitialization initializer, out ImmutableArray<byte> data)
	{
		data = default;
		if (initializer == null)
		{
			return -1;
		}
		ImmutableArray<BoundExpression> initializers = initializer.Initializers;
		if (initializers.Any((BoundExpression init) => init.ConstantValue == null))
		{
			return -1;
		}
		int length = initializers.Length;
		if (length == 0)
		{
			data = ImmutableArray<byte>.Empty;
			return 0;
		}
		BlobBuilder blobBuilder = new BlobBuilder(initializers.Length * 4);
		foreach (BoundExpression initializer2 in initializer.Initializers)
		{
			initializer2.ConstantValue.Serialize(blobBuilder);
		}
		data = blobBuilder.ToImmutableArray();
		return length;
	}

	private void EmitConversionExpression(BoundConversion conversion, bool used)
	{
		switch (conversion.ConversionKind)
		{
		case ConversionKind.MethodGroup:
			throw ExceptionUtilities.UnexpectedValue(conversion.ConversionKind);
		case ConversionKind.NullToPointer:
			_builder.EmitIntConstant(0);
			_builder.EmitOpCode(ILOpCode.Conv_u);
			EmitPopIfUnused(used);
			return;
		}
		BoundExpression operand = conversion.Operand;
		if (conversion.ConversionKind.IsUserDefinedConversion())
		{
			EmitSpecialUserDefinedConversion(conversion, used, operand);
			return;
		}
		if (!used && !conversion.ConversionHasSideEffects())
		{
			EmitExpression(operand, used: false);
			return;
		}
		EmitExpression(operand, used: true);
		EmitConversion(conversion);
		EmitPopIfUnused(used);
	}

	private void EmitSpecialUserDefinedConversion(BoundConversion conversion, bool used, BoundExpression operand)
	{
		NamedTypeSymbol spanType = (NamedTypeSymbol)conversion.Type;
		if (!TryEmitReadonlySpanAsBlobWrapper(spanType, operand, used, inPlace: false))
		{
			EmitExpression(operand, used);
			if (used)
			{
				_builder.EmitOpCode(ILOpCode.Call, 0);
				EmitSymbolToken(conversion.SymbolOpt, conversion.Syntax, null);
			}
		}
	}

	private void EmitConversion(BoundConversion conversion)
	{
		switch (conversion.ConversionKind)
		{
		case ConversionKind.Identity:
			EmitIdentityConversion(conversion);
			break;
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ExplicitNumeric:
			EmitNumericConversion(conversion);
			break;
		case ConversionKind.ImplicitReference:
		case ConversionKind.Boxing:
			EmitImplicitReferenceConversion(conversion);
			break;
		case ConversionKind.ExplicitReference:
		case ConversionKind.Unboxing:
			EmitExplicitReferenceConversion(conversion);
			break;
		case ConversionKind.ImplicitEnumeration:
		case ConversionKind.ExplicitEnumeration:
			EmitEnumConversion(conversion);
			break;
		case ConversionKind.ImplicitThrow:
		case ConversionKind.ImplicitTupleLiteral:
		case ConversionKind.ImplicitTuple:
		case ConversionKind.ExplicitTupleLiteral:
		case ConversionKind.ExplicitTuple:
		case ConversionKind.ImplicitDynamic:
		case ConversionKind.ExplicitDynamic:
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.AnonymousFunction:
		case ConversionKind.MethodGroup:
		case ConversionKind.ExplicitUserDefined:
			throw ExceptionUtilities.UnexpectedValue(conversion.ConversionKind);
		case ConversionKind.PointerToVoid:
		case ConversionKind.PointerToPointer:
			break;
		case ConversionKind.IntegerToPointer:
		case ConversionKind.PointerToInteger:
		{
			Microsoft.Cci.PrimitiveTypeCode primitiveTypeCode = conversion.Operand.Type.PrimitiveTypeCode;
			Microsoft.Cci.PrimitiveTypeCode primitiveTypeCode2 = conversion.Type.PrimitiveTypeCode;
			_builder.EmitNumericConversion(primitiveTypeCode, primitiveTypeCode2, conversion.Checked);
			break;
		}
		case ConversionKind.PinnedObjectToPointer:
			_builder.EmitOpCode(ILOpCode.Conv_u);
			break;
		case ConversionKind.NullToPointer:
			throw ExceptionUtilities.UnexpectedValue(conversion.ConversionKind);
		default:
			throw ExceptionUtilities.UnexpectedValue(conversion.ConversionKind);
		}
	}

	private void EmitIdentityConversion(BoundConversion conversion)
	{
		if (conversion.ExplicitCastInCode)
		{
			Microsoft.Cci.PrimitiveTypeCode primitiveTypeCode = conversion.Type.PrimitiveTypeCode;
			if ((uint)(primitiveTypeCode - 3) <= 1u && conversion.Operand.ConstantValue == null)
			{
				EmitNumericConversion(conversion);
			}
		}
	}

	private void EmitNumericConversion(BoundConversion conversion)
	{
		Microsoft.Cci.PrimitiveTypeCode primitiveTypeCode = conversion.Operand.Type.PrimitiveTypeCode;
		Microsoft.Cci.PrimitiveTypeCode primitiveTypeCode2 = conversion.Type.PrimitiveTypeCode;
		_builder.EmitNumericConversion(primitiveTypeCode, primitiveTypeCode2, conversion.Checked);
	}

	private void EmitImplicitReferenceConversion(BoundConversion conversion)
	{
		if (!conversion.Operand.Type.IsVerifierReference())
		{
			EmitBox(conversion.Operand.Type, conversion.Operand.Syntax);
		}
		TypeSymbol type = conversion.Type;
		if (!type.IsVerifierReference())
		{
			_builder.EmitOpCode(ILOpCode.Unbox_any);
			EmitSymbolToken(conversion.Type, conversion.Syntax);
		}
		else if (type.IsArray())
		{
			EmitStaticCast(conversion.Type, conversion.Syntax);
		}
	}

	private void EmitExplicitReferenceConversion(BoundConversion conversion)
	{
		if (!conversion.Operand.Type.IsVerifierReference())
		{
			EmitBox(conversion.Operand.Type, conversion.Operand.Syntax);
		}
		if (conversion.Type.IsVerifierReference())
		{
			_builder.EmitOpCode(ILOpCode.Castclass);
			EmitSymbolToken(conversion.Type, conversion.Syntax);
		}
		else
		{
			_builder.EmitOpCode(ILOpCode.Unbox_any);
			EmitSymbolToken(conversion.Type, conversion.Syntax);
		}
	}

	private void EmitEnumConversion(BoundConversion conversion)
	{
		TypeSymbol typeSymbol = conversion.Operand.Type;
		if (typeSymbol.IsEnumType())
		{
			typeSymbol = ((NamedTypeSymbol)typeSymbol).EnumUnderlyingType;
		}
		Microsoft.Cci.PrimitiveTypeCode primitiveTypeCode = typeSymbol.PrimitiveTypeCode;
		TypeSymbol typeSymbol2 = conversion.Type;
		if (typeSymbol2.IsEnumType())
		{
			typeSymbol2 = ((NamedTypeSymbol)typeSymbol2).EnumUnderlyingType;
		}
		Microsoft.Cci.PrimitiveTypeCode primitiveTypeCode2 = typeSymbol2.PrimitiveTypeCode;
		_builder.EmitNumericConversion(primitiveTypeCode, primitiveTypeCode2, conversion.Checked);
	}

	private void EmitDelegateCreation(BoundExpression node, BoundExpression receiver, bool isExtensionMethod, MethodSymbol method, TypeSymbol delegateType, bool used)
	{
		bool flag = receiver == null || (!isExtensionMethod && method.IsStatic);
		if (!used)
		{
			if (!flag)
			{
				EmitExpression(receiver, used: false);
			}
			return;
		}
		if (flag)
		{
			_builder.EmitNullConstant();
		}
		else
		{
			EmitExpression(receiver, used: true);
			if (!receiver.Type.IsVerifierReference())
			{
				EmitBox(receiver.Type, receiver.Syntax);
			}
		}
		if (method.IsMetadataVirtual() && !method.ContainingType.IsDelegateType() && !receiver.SuppressVirtualCalls)
		{
			_builder.EmitOpCode(ILOpCode.Dup);
			_builder.EmitOpCode(ILOpCode.Ldvirtftn);
			method = method.GetConstructedLeastOverriddenMethod(_method.ContainingType);
		}
		else
		{
			_builder.EmitOpCode(ILOpCode.Ldftn);
		}
		EmitSymbolToken(method, node.Syntax, null);
		_builder.EmitOpCode(ILOpCode.Newobj, -1);
		MethodSymbol methodSymbol = DelegateConstructor(node.Syntax, delegateType);
		if ((object)methodSymbol != null)
		{
			EmitSymbolToken(methodSymbol, node.Syntax, null);
		}
	}

	private MethodSymbol DelegateConstructor(SyntaxNode syntax, TypeSymbol delegateType)
	{
		foreach (Symbol member in delegateType.GetMembers(".ctor"))
		{
			if (member is MethodSymbol { Parameters: { Length: 2 } parameters } methodSymbol && parameters[0].Type.SpecialType == SpecialType.System_Object)
			{
				SpecialType specialType = parameters[1].Type.SpecialType;
				if (specialType == SpecialType.System_IntPtr || specialType == SpecialType.System_UIntPtr)
				{
					return methodSymbol;
				}
			}
		}
		_diagnostics.Add(ErrorCode.ERR_BadDelegateConstructor, syntax.Location, delegateType);
		return null;
	}

	private void EmitExpression(BoundExpression expression, bool used)
	{
		if (expression == null)
		{
			return;
		}
		ConstantValue constantValue = expression.ConstantValue;
		if (constantValue != null)
		{
			if (!used)
			{
				return;
			}
			if ((object)expression.Type == null || expression.Type.SpecialType != SpecialType.System_Decimal)
			{
				EmitConstantExpression(expression.Type, constantValue, used, expression.Syntax);
				return;
			}
		}
		_recursionDepth++;
		if (_recursionDepth > 1)
		{
			StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
			EmitExpressionCore(expression, used);
		}
		else
		{
			EmitExpressionCoreWithStackGuard(expression, used);
		}
		_recursionDepth--;
	}

	private void EmitExpressionCoreWithStackGuard(BoundExpression expression, bool used)
	{
		try
		{
			EmitExpressionCore(expression, used);
		}
		catch (Exception ex) when (StackGuard.IsInsufficientExecutionStackException(ex))
		{
			_diagnostics.Add(ErrorCode.ERR_InsufficientStack, BoundTreeVisitor.CancelledByStackGuardException.GetTooLongOrComplexExpressionErrorLocation(expression));
			throw new EmitCancelledException();
		}
	}

	private void EmitExpressionCore(BoundExpression expression, bool used)
	{
		switch (expression.Kind)
		{
		case BoundKind.AssignmentOperator:
			EmitAssignmentExpression((BoundAssignmentOperator)expression, used ? UseKind.UsedAsValue : UseKind.Unused);
			break;
		case BoundKind.Call:
			EmitCallExpression((BoundCall)expression, used ? UseKind.UsedAsValue : UseKind.Unused);
			break;
		case BoundKind.ObjectCreationExpression:
			EmitObjectCreationExpression((BoundObjectCreationExpression)expression, used);
			break;
		case BoundKind.DelegateCreationExpression:
			EmitDelegateCreationExpression((BoundDelegateCreationExpression)expression, used);
			break;
		case BoundKind.ArrayCreation:
			EmitArrayCreationExpression((BoundArrayCreation)expression, used);
			break;
		case BoundKind.ConvertedStackAllocExpression:
			EmitConvertedStackAllocExpression((BoundConvertedStackAllocExpression)expression, used);
			break;
		case BoundKind.Conversion:
			EmitConversionExpression((BoundConversion)expression, used);
			break;
		case BoundKind.Local:
			EmitLocalLoad((BoundLocal)expression, used);
			break;
		case BoundKind.Dup:
			EmitDupExpression((BoundDup)expression, used);
			break;
		case BoundKind.PassByCopy:
			EmitExpression(((BoundPassByCopy)expression).Expression, used);
			break;
		case BoundKind.Parameter:
			if (used)
			{
				EmitParameterLoad((BoundParameter)expression);
			}
			break;
		case BoundKind.FieldAccess:
			EmitFieldLoad((BoundFieldAccess)expression, used);
			break;
		case BoundKind.ArrayAccess:
			EmitArrayElementLoad((BoundArrayAccess)expression, used);
			break;
		case BoundKind.ArrayLength:
			EmitArrayLength((BoundArrayLength)expression, used);
			break;
		case BoundKind.ThisReference:
			if (used)
			{
				EmitThisReferenceExpression((BoundThisReference)expression);
			}
			break;
		case BoundKind.PreviousSubmissionReference:
			throw ExceptionUtilities.UnexpectedValue(expression.Kind);
		case BoundKind.BaseReference:
			if (used)
			{
				NamedTypeSymbol containingType = _method.ContainingType;
				_builder.EmitOpCode(ILOpCode.Ldarg_0);
				if (containingType.IsValueType)
				{
					EmitLoadIndirect(containingType, expression.Syntax);
					EmitBox(containingType, expression.Syntax);
				}
			}
			break;
		case BoundKind.Sequence:
			EmitSequenceExpression((BoundSequence)expression, used);
			break;
		case BoundKind.SequencePointExpression:
			EmitSequencePointExpression((BoundSequencePointExpression)expression, used);
			break;
		case BoundKind.UnaryOperator:
			EmitUnaryOperatorExpression((BoundUnaryOperator)expression, used);
			break;
		case BoundKind.BinaryOperator:
			EmitBinaryOperatorExpression((BoundBinaryOperator)expression, used);
			break;
		case BoundKind.NullCoalescingOperator:
			EmitNullCoalescingOperator((BoundNullCoalescingOperator)expression, used);
			break;
		case BoundKind.IsOperator:
			EmitIsExpression((BoundIsOperator)expression, used);
			break;
		case BoundKind.AsOperator:
			EmitAsExpression((BoundAsOperator)expression, used);
			break;
		case BoundKind.DefaultExpression:
			EmitDefaultExpression((BoundDefaultExpression)expression, used);
			break;
		case BoundKind.TypeOfOperator:
			if (used)
			{
				EmitTypeOfExpression((BoundTypeOfOperator)expression);
			}
			break;
		case BoundKind.SizeOfOperator:
			if (used)
			{
				EmitSizeOfExpression((BoundSizeOfOperator)expression);
			}
			break;
		case BoundKind.ModuleVersionId:
			EmitModuleVersionIdLoad((BoundModuleVersionId)expression);
			break;
		case BoundKind.ModuleVersionIdString:
			EmitModuleVersionIdStringLoad((BoundModuleVersionIdString)expression);
			break;
		case BoundKind.InstrumentationPayloadRoot:
			EmitInstrumentationPayloadRootLoad((BoundInstrumentationPayloadRoot)expression);
			break;
		case BoundKind.MethodDefIndex:
			EmitMethodDefIndexExpression((BoundMethodDefIndex)expression);
			break;
		case BoundKind.MaximumMethodDefIndex:
			EmitMaximumMethodDefIndexExpression((BoundMaximumMethodDefIndex)expression);
			break;
		case BoundKind.SourceDocumentIndex:
			EmitSourceDocumentIndex((BoundSourceDocumentIndex)expression);
			break;
		case BoundKind.MethodInfo:
			if (used)
			{
				EmitMethodInfoExpression((BoundMethodInfo)expression);
			}
			break;
		case BoundKind.FieldInfo:
			if (used)
			{
				EmitFieldInfoExpression((BoundFieldInfo)expression);
			}
			break;
		case BoundKind.ConditionalOperator:
			EmitConditionalOperator((BoundConditionalOperator)expression, used);
			break;
		case BoundKind.AddressOfOperator:
			EmitAddressOfExpression((BoundAddressOfOperator)expression, used);
			break;
		case BoundKind.PointerIndirectionOperator:
			EmitPointerIndirectionOperator((BoundPointerIndirectionOperator)expression, used);
			break;
		case BoundKind.ArgList:
			EmitArgList(used);
			break;
		case BoundKind.ArgListOperator:
			EmitArgListOperator((BoundArgListOperator)expression);
			break;
		case BoundKind.RefTypeOperator:
			EmitRefTypeOperator((BoundRefTypeOperator)expression, used);
			break;
		case BoundKind.MakeRefOperator:
			EmitMakeRefOperator((BoundMakeRefOperator)expression, used);
			break;
		case BoundKind.RefValueOperator:
			EmitRefValueOperator((BoundRefValueOperator)expression, used);
			break;
		case BoundKind.LoweredConditionalAccess:
			EmitLoweredConditionalAccessExpression((BoundLoweredConditionalAccess)expression, used);
			break;
		case BoundKind.ConditionalReceiver:
			EmitConditionalReceiver((BoundConditionalReceiver)expression, used);
			break;
		case BoundKind.ComplexConditionalReceiver:
			EmitComplexConditionalReceiver((BoundComplexConditionalReceiver)expression, used);
			break;
		case BoundKind.PseudoVariable:
			EmitPseudoVariableValue((BoundPseudoVariable)expression, used);
			break;
		case BoundKind.ThrowExpression:
			EmitThrowExpression((BoundThrowExpression)expression, used);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(expression.Kind);
		}
	}

	private void EmitThrowExpression(BoundThrowExpression node, bool used)
	{
		EmitThrow(node.Expression);
		EmitDefaultValue(node.Type, used, node.Syntax);
	}

	private void EmitComplexConditionalReceiver(BoundComplexConditionalReceiver expression, bool used)
	{
		TypeSymbol type = expression.Type;
		object label = new object();
		object label2 = new object();
		EmitInitObj(type, used: true, expression.Syntax);
		EmitBox(type, expression.Syntax);
		_builder.EmitBranch(ILOpCode.Brtrue, label);
		EmitExpression(expression.ReferenceTypeReceiver, used);
		_builder.EmitBranch(ILOpCode.Br, label2);
		_builder.AdjustStack(-1);
		_builder.MarkLabel(label);
		EmitExpression(expression.ValueTypeReceiver, used);
		_builder.MarkLabel(label2);
	}

	private void EmitLoweredConditionalAccessExpression(BoundLoweredConditionalAccess expression, bool used)
	{
		BoundExpression receiver = expression.Receiver;
		TypeSymbol type = receiver.Type;
		ConstantValue constantValue = receiver.ConstantValue;
		LocalDefinition localDefinition;
		if ((object)constantValue != null && !constantValue.IsNull)
		{
			localDefinition = EmitReceiverRef(receiver, AddressKind.ReadOnly);
			EmitExpression(expression.WhenNotNull, used);
			if (localDefinition != null)
			{
				FreeTemp(localDefinition);
			}
			return;
		}
		object label = new object();
		object label2 = new object();
		LocalDefinition localDefinition2 = null;
		bool flag = !type.IsReferenceType && !type.IsValueType;
		bool flag2 = LocalRewriter.CanChangeValueBetweenReads(receiver, localsMayBeAssignedOrCaptured: false) || (type.IsReferenceType && type.TypeKind == TypeKind.TypeParameter) || (receiver.Kind == BoundKind.Local && IsStackLocal(((BoundLocal)receiver).LocalSymbol));
		if (flag2)
		{
			if (flag)
			{
				localDefinition = EmitReceiverRef(receiver, AddressKind.Constrained);
				EmitDefaultValue(type, used: true, receiver.Syntax);
				EmitBox(type, receiver.Syntax);
				_builder.EmitBranch(ILOpCode.Brtrue, label);
				EmitLoadIndirect(type, receiver.Syntax);
				localDefinition2 = AllocateTemp(type, receiver.Syntax);
				_builder.EmitLocalStore(localDefinition2);
				_builder.EmitLocalAddress(localDefinition2);
				_builder.EmitLocalLoad(localDefinition2);
				EmitBox(receiver.Type, receiver.Syntax);
			}
			else
			{
				AddressKind addressKind = AddressKind.ReadOnly;
				localDefinition = EmitReceiverRef(receiver, addressKind);
				_builder.EmitOpCode(ILOpCode.Dup);
			}
		}
		else
		{
			localDefinition = EmitReceiverRef(receiver, AddressKind.ReadOnly);
		}
		MethodSymbol hasValueMethodOpt = expression.HasValueMethodOpt;
		if (hasValueMethodOpt != null)
		{
			_builder.EmitOpCode(ILOpCode.Call, 0);
			EmitSymbolToken(hasValueMethodOpt, expression.Syntax, null);
		}
		_builder.EmitBranch(ILOpCode.Brtrue, label);
		if (localDefinition != null && !flag2)
		{
			FreeTemp(localDefinition);
			localDefinition = null;
		}
		if (flag2)
		{
			_builder.EmitOpCode(ILOpCode.Pop);
		}
		BoundExpression whenNullOpt = expression.WhenNullOpt;
		if (whenNullOpt == null)
		{
			EmitDefaultValue(expression.Type, used, expression.Syntax);
		}
		else
		{
			EmitExpression(whenNullOpt, used);
		}
		_builder.EmitBranch(ILOpCode.Br, label2);
		if (flag2)
		{
			_builder.AdjustStack(1);
		}
		if (used)
		{
			_builder.AdjustStack(-1);
		}
		_builder.MarkLabel(label);
		if (!flag2)
		{
			localDefinition = EmitReceiverRef(receiver, AddressKind.Constrained);
		}
		EmitExpression(expression.WhenNotNull, used);
		_builder.MarkLabel(label2);
		if (localDefinition2 != null)
		{
			FreeTemp(localDefinition2);
		}
		if (localDefinition != null)
		{
			FreeTemp(localDefinition);
		}
	}

	private void EmitConditionalReceiver(BoundConditionalReceiver expression, bool used)
	{
		if (!expression.Type.IsReferenceType)
		{
			EmitLoadIndirect(expression.Type, expression.Syntax);
		}
		EmitPopIfUnused(used);
	}

	private void EmitRefValueOperator(BoundRefValueOperator expression, bool used)
	{
		EmitRefValueAddress(expression);
		EmitLoadIndirect(expression.Type, expression.Syntax);
		EmitPopIfUnused(used);
	}

	private void EmitMakeRefOperator(BoundMakeRefOperator expression, bool used)
	{
		EmitAddress(expression.Operand, AddressKind.Writeable);
		_builder.EmitOpCode(ILOpCode.Mkrefany);
		EmitSymbolToken(expression.Operand.Type, expression.Operand.Syntax);
		EmitPopIfUnused(used);
	}

	private void EmitRefTypeOperator(BoundRefTypeOperator expression, bool used)
	{
		EmitExpression(expression.Operand, used: true);
		_builder.EmitOpCode(ILOpCode.Refanytype);
		_builder.EmitOpCode(ILOpCode.Call, 0);
		MethodSymbol getTypeFromHandle = expression.GetTypeFromHandle;
		EmitSymbolToken(getTypeFromHandle, expression.Syntax, null);
		EmitPopIfUnused(used);
	}

	private void EmitArgList(bool used)
	{
		_builder.EmitOpCode(ILOpCode.Arglist);
		EmitPopIfUnused(used);
	}

	private void EmitArgListOperator(BoundArgListOperator expression)
	{
		for (int i = 0; i < expression.Arguments.Length; i++)
		{
			BoundExpression argument = expression.Arguments[i];
			RefKind refKind = ((!expression.ArgumentRefKindsOpt.IsDefaultOrEmpty) ? expression.ArgumentRefKindsOpt[i] : RefKind.None);
			EmitArgument(argument, refKind);
		}
	}

	private void EmitArgument(BoundExpression argument, RefKind refKind)
	{
		switch (refKind)
		{
		case RefKind.None:
			EmitExpression(argument, used: true);
			return;
		case RefKind.In:
		{
			LocalDefinition temp = EmitAddress(argument, AddressKind.ReadOnly);
			AddExpressionTemp(temp);
			return;
		}
		}
		LocalDefinition localDefinition = EmitAddress(argument, (refKind == (RefKind)4) ? AddressKind.ReadOnlyStrict : AddressKind.Writeable);
		if (localDefinition != null)
		{
			AddExpressionTemp(localDefinition);
		}
	}

	private void EmitAddressOfExpression(BoundAddressOfOperator expression, bool used)
	{
		EmitAddress(expression.Operand, AddressKind.ReadOnlyStrict);
		if (used && !expression.IsManaged)
		{
			_builder.EmitOpCode(ILOpCode.Conv_u);
		}
		EmitPopIfUnused(used);
	}

	private void EmitPointerIndirectionOperator(BoundPointerIndirectionOperator expression, bool used)
	{
		EmitExpression(expression.Operand, used: true);
		EmitLoadIndirect(expression.Type, expression.Syntax);
		EmitPopIfUnused(used);
	}

	private void EmitDupExpression(BoundDup expression, bool used)
	{
		if (expression.RefKind == RefKind.None)
		{
			if (used)
			{
				_builder.EmitOpCode(ILOpCode.Dup);
			}
		}
		else
		{
			_builder.EmitOpCode(ILOpCode.Dup);
			EmitLoadIndirect(expression.Type, expression.Syntax);
			EmitPopIfUnused(used);
		}
	}

	private void EmitDelegateCreationExpression(BoundDelegateCreationExpression expression, bool used)
	{
		BoundExpression boundExpression = ((expression.Argument is BoundMethodGroup boundMethodGroup) ? boundMethodGroup.ReceiverOpt : expression.Argument);
		MethodSymbol method = expression.MethodOpt ?? boundExpression.Type.DelegateInvokeMethod();
		EmitDelegateCreation(expression, boundExpression, expression.IsExtensionMethod, method, expression.Type, used);
	}

	private void EmitThisReferenceExpression(BoundThisReference thisRef)
	{
		TypeSymbol type = thisRef.Type;
		_builder.EmitOpCode(ILOpCode.Ldarg_0);
		if (type.IsValueType)
		{
			EmitLoadIndirect(type, thisRef.Syntax);
		}
	}

	private void EmitPseudoVariableValue(BoundPseudoVariable expression, bool used)
	{
		EmitExpression(expression.EmitExpressions.GetValue(expression, _diagnostics), used);
	}

	private void EmitSequencePointExpression(BoundSequencePointExpression node, bool used)
	{
		EmitSequencePoint(node);
		EmitExpression(node.Expression, used: true);
		EmitPopIfUnused(used);
	}

	private void EmitSequencePoint(BoundSequencePointExpression node)
	{
		SyntaxNode syntax = node.Syntax;
		if (_emitPdbSequencePoints)
		{
			if (syntax == null)
			{
				EmitHiddenSequencePoint();
			}
			else
			{
				EmitSequencePoint(syntax);
			}
		}
	}

	private void EmitSequenceExpression(BoundSequence sequence, bool used)
	{
		DefineLocals(sequence);
		EmitSideEffects(sequence);
		if (sequence.Value.Kind != BoundKind.TypeExpression)
		{
			EmitExpression(sequence.Value, used);
		}
		FreeLocals(sequence);
	}

	private void DefineLocals(BoundSequence sequence)
	{
		if (!sequence.Locals.IsEmpty)
		{
			_builder.OpenLocalScope();
			foreach (LocalSymbol local in sequence.Locals)
			{
				DefineLocal(local, sequence.Syntax);
			}
		}
	}

	private void FreeLocals(BoundSequence sequence)
	{
		if (!sequence.Locals.IsEmpty)
		{
			_builder.CloseLocalScope();
			foreach (LocalSymbol local in sequence.Locals)
			{
				FreeLocal(local);
			}
		}
	}

	private void DefineAndRecordLocals(BoundSequence sequence)
	{
		if (!sequence.Locals.IsEmpty)
		{
			_builder.OpenLocalScope();
			foreach (LocalSymbol local in sequence.Locals)
			{
				LocalDefinition temp = DefineLocal(local, sequence.Syntax);
				AddExpressionTemp(temp);
			}
		}
	}

	private void CloseScopeAndKeepLocals(BoundSequence sequence)
	{
		if (!sequence.Locals.IsEmpty)
		{
			_builder.CloseLocalScope();
		}
	}

	private void EmitSideEffects(BoundSequence sequence)
	{
		ImmutableArray<BoundExpression> sideEffects = sequence.SideEffects;
		if (!sideEffects.IsDefaultOrEmpty)
		{
			foreach (BoundExpression item in sideEffects)
			{
				EmitExpression(item, used: false);
			}
		}
	}

	private void EmitArguments(ImmutableArray<BoundExpression> arguments, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<RefKind> argRefKindsOpt)
	{
		for (int i = 0; i < arguments.Length; i++)
		{
			RefKind argumentRefKind = GetArgumentRefKind(arguments, parameters, argRefKindsOpt, i);
			EmitArgument(arguments[i], argumentRefKind);
		}
	}

	internal static RefKind GetArgumentRefKind(ImmutableArray<BoundExpression> arguments, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<RefKind> argRefKindsOpt, int i)
	{
		if (i < parameters.Length)
		{
			if (!argRefKindsOpt.IsDefault && i < argRefKindsOpt.Length)
			{
				return argRefKindsOpt[i];
			}
			return parameters[i].RefKind;
		}
		return RefKind.None;
	}

	private void EmitArrayElementLoad(BoundArrayAccess arrayAccess, bool used)
	{
		EmitExpression(arrayAccess.Expression, used: true);
		EmitArrayIndices(arrayAccess.Indices);
		if (((ArrayTypeSymbol)arrayAccess.Expression.Type).IsSZArray)
		{
			TypeSymbol typeSymbol = arrayAccess.Type;
			if (typeSymbol.IsEnumType())
			{
				typeSymbol = ((NamedTypeSymbol)typeSymbol).EnumUnderlyingType;
			}
			switch (typeSymbol.PrimitiveTypeCode)
			{
			case Microsoft.Cci.PrimitiveTypeCode.Int8:
				_builder.EmitOpCode(ILOpCode.Ldelem_i1);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Boolean:
			case Microsoft.Cci.PrimitiveTypeCode.UInt8:
				_builder.EmitOpCode(ILOpCode.Ldelem_u1);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Int16:
				_builder.EmitOpCode(ILOpCode.Ldelem_i2);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Char:
			case Microsoft.Cci.PrimitiveTypeCode.UInt16:
				_builder.EmitOpCode(ILOpCode.Ldelem_u2);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Int32:
				_builder.EmitOpCode(ILOpCode.Ldelem_i4);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.UInt32:
				_builder.EmitOpCode(ILOpCode.Ldelem_u4);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Int64:
			case Microsoft.Cci.PrimitiveTypeCode.UInt64:
				_builder.EmitOpCode(ILOpCode.Ldelem_i8);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.IntPtr:
			case Microsoft.Cci.PrimitiveTypeCode.Pointer:
			case Microsoft.Cci.PrimitiveTypeCode.UIntPtr:
				_builder.EmitOpCode(ILOpCode.Ldelem_i);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Float32:
				_builder.EmitOpCode(ILOpCode.Ldelem_r4);
				break;
			case Microsoft.Cci.PrimitiveTypeCode.Float64:
				_builder.EmitOpCode(ILOpCode.Ldelem_r8);
				break;
			default:
				if (typeSymbol.IsVerifierReference())
				{
					_builder.EmitOpCode(ILOpCode.Ldelem_ref);
					break;
				}
				if (used)
				{
					_builder.EmitOpCode(ILOpCode.Ldelem);
				}
				else
				{
					if (typeSymbol.TypeKind == TypeKind.TypeParameter)
					{
						_builder.EmitOpCode(ILOpCode.Readonly);
					}
					_builder.EmitOpCode(ILOpCode.Ldelema);
				}
				EmitSymbolToken(typeSymbol, arrayAccess.Syntax);
				break;
			}
		}
		else
		{
			_builder.EmitArrayElementLoad(PEModuleBuilder.Translate((ArrayTypeSymbol)arrayAccess.Expression.Type), arrayAccess.Expression.Syntax, _diagnostics);
		}
		EmitPopIfUnused(used);
	}

	private void EmitFieldLoad(BoundFieldAccess fieldAccess, bool used)
	{
		FieldSymbol fieldSymbol = fieldAccess.FieldSymbol;
		if (!used)
		{
			if (fieldSymbol.IsCapturedFrame)
			{
				return;
			}
			if (!fieldSymbol.IsVolatile && !fieldSymbol.IsStatic && fieldAccess.ReceiverOpt.Type.IsVerifierValue())
			{
				EmitExpression(fieldAccess.ReceiverOpt, used: false);
				return;
			}
		}
		if (fieldSymbol.IsStatic)
		{
			if (fieldSymbol.IsVolatile)
			{
				_builder.EmitOpCode(ILOpCode.Volatile);
			}
			_builder.EmitOpCode(ILOpCode.Ldsfld);
			EmitSymbolToken(fieldSymbol, fieldAccess.Syntax);
		}
		else
		{
			BoundExpression receiverOpt = fieldAccess.ReceiverOpt;
			TypeSymbol type = fieldSymbol.Type;
			if (type.IsValueType && (object)type == receiverOpt.Type)
			{
				EmitExpression(receiverOpt, used);
			}
			else
			{
				LocalDefinition localDefinition = EmitFieldLoadReceiver(receiverOpt);
				if (localDefinition != null)
				{
					FreeTemp(localDefinition);
				}
				if (fieldSymbol.IsVolatile)
				{
					_builder.EmitOpCode(ILOpCode.Volatile);
				}
				_builder.EmitOpCode(ILOpCode.Ldfld);
				EmitSymbolToken(fieldSymbol, fieldAccess.Syntax);
			}
		}
		EmitPopIfUnused(used);
	}

	private LocalDefinition EmitFieldLoadReceiver(BoundExpression receiver)
	{
		if (FieldLoadMustUseRef(receiver) || FieldLoadPrefersRef(receiver))
		{
			if (!EmitFieldLoadReceiverAddress(receiver))
			{
				return EmitReceiverRef(receiver, AddressKind.ReadOnly);
			}
			return null;
		}
		EmitExpression(receiver, used: true);
		return null;
	}

	private bool EmitFieldLoadReceiverAddress(BoundExpression receiver)
	{
		if (receiver == null || !receiver.Type.IsValueType)
		{
			return false;
		}
		if (receiver.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)receiver;
			if (boundConversion.ConversionKind == ConversionKind.Unboxing)
			{
				EmitExpression(boundConversion.Operand, used: true);
				_builder.EmitOpCode(ILOpCode.Unbox);
				EmitSymbolToken(receiver.Type, receiver.Syntax);
				return true;
			}
		}
		else if (receiver.Kind == BoundKind.FieldAccess)
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)receiver;
			FieldSymbol fieldSymbol = boundFieldAccess.FieldSymbol;
			if (!fieldSymbol.IsStatic && EmitFieldLoadReceiverAddress(boundFieldAccess.ReceiverOpt))
			{
				_builder.EmitOpCode(ILOpCode.Ldflda);
				EmitSymbolToken(fieldSymbol, boundFieldAccess.Syntax);
				return true;
			}
		}
		return false;
	}

	private bool FieldLoadPrefersRef(BoundExpression receiver)
	{
		if (!receiver.Type.IsVerifierValue())
		{
			return true;
		}
		if (receiver.Kind == BoundKind.Conversion && ((BoundConversion)receiver).ConversionKind == ConversionKind.Unboxing)
		{
			return true;
		}
		if (!HasHome(receiver, AddressKind.ReadOnly))
		{
			return false;
		}
		switch (receiver.Kind)
		{
		case BoundKind.Parameter:
			return ((BoundParameter)receiver).ParameterSymbol.RefKind != RefKind.None;
		case BoundKind.Local:
			return ((BoundLocal)receiver).LocalSymbol.RefKind != RefKind.None;
		case BoundKind.Sequence:
			return FieldLoadPrefersRef(((BoundSequence)receiver).Value);
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)receiver;
			if (boundFieldAccess.FieldSymbol.IsStatic)
			{
				return true;
			}
			if (DiagnosticsPass.IsNonAgileFieldAccess(boundFieldAccess, _module.Compilation))
			{
				return false;
			}
			return FieldLoadPrefersRef(boundFieldAccess.ReceiverOpt);
		}
		default:
			return true;
		}
	}

	internal static bool FieldLoadMustUseRef(BoundExpression expr)
	{
		TypeSymbol type = expr.Type;
		if (type.IsTypeParameter())
		{
			return true;
		}
		switch (type.SpecialType)
		{
		case SpecialType.System_Boolean:
		case SpecialType.System_Char:
		case SpecialType.System_SByte:
		case SpecialType.System_Byte:
		case SpecialType.System_Int16:
		case SpecialType.System_UInt16:
		case SpecialType.System_Int32:
		case SpecialType.System_UInt32:
		case SpecialType.System_Int64:
		case SpecialType.System_UInt64:
		case SpecialType.System_Single:
		case SpecialType.System_Double:
		case SpecialType.System_IntPtr:
		case SpecialType.System_UIntPtr:
		case SpecialType.System_RuntimeArgumentHandle:
		case SpecialType.System_RuntimeFieldHandle:
		case SpecialType.System_RuntimeMethodHandle:
		case SpecialType.System_RuntimeTypeHandle:
			return true;
		default:
			return type.IsEnumType();
		}
	}

	private static int ParameterSlot(BoundParameter parameter)
	{
		ParameterSymbol parameterSymbol = parameter.ParameterSymbol;
		int num = parameterSymbol.Ordinal;
		if (!parameterSymbol.ContainingSymbol.IsStatic)
		{
			num++;
		}
		return num;
	}

	private void EmitLocalLoad(BoundLocal local, bool used)
	{
		if (IsStackLocal(local.LocalSymbol))
		{
			EmitPopIfUnused(used);
		}
		else
		{
			if (!used)
			{
				return;
			}
			LocalDefinition local2 = GetLocal(local);
			_builder.EmitLocalLoad(local2);
		}
		if (used && local.LocalSymbol.RefKind != RefKind.None)
		{
			EmitLoadIndirect(local.LocalSymbol.Type, local.Syntax);
		}
	}

	private void EmitParameterLoad(BoundParameter parameter)
	{
		int argNumber = ParameterSlot(parameter);
		_builder.EmitLoadArgumentOpcode(argNumber);
		if (parameter.ParameterSymbol.RefKind != RefKind.None)
		{
			TypeSymbol type = parameter.ParameterSymbol.Type;
			EmitLoadIndirect(type, parameter.Syntax);
		}
	}

	private void EmitLoadIndirect(TypeSymbol type, SyntaxNode syntaxNode)
	{
		if (type.IsEnumType())
		{
			type = ((NamedTypeSymbol)type).EnumUnderlyingType;
		}
		switch (type.PrimitiveTypeCode)
		{
		case Microsoft.Cci.PrimitiveTypeCode.Int8:
			_builder.EmitOpCode(ILOpCode.Ldind_i1);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Boolean:
		case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			_builder.EmitOpCode(ILOpCode.Ldind_u1);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Int16:
			_builder.EmitOpCode(ILOpCode.Ldind_i2);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Char:
		case Microsoft.Cci.PrimitiveTypeCode.UInt16:
			_builder.EmitOpCode(ILOpCode.Ldind_u2);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Int32:
			_builder.EmitOpCode(ILOpCode.Ldind_i4);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.UInt32:
			_builder.EmitOpCode(ILOpCode.Ldind_u4);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Int64:
		case Microsoft.Cci.PrimitiveTypeCode.UInt64:
			_builder.EmitOpCode(ILOpCode.Ldind_i8);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.IntPtr:
		case Microsoft.Cci.PrimitiveTypeCode.Pointer:
		case Microsoft.Cci.PrimitiveTypeCode.UIntPtr:
			_builder.EmitOpCode(ILOpCode.Ldind_i);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Float32:
			_builder.EmitOpCode(ILOpCode.Ldind_r4);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Float64:
			_builder.EmitOpCode(ILOpCode.Ldind_r8);
			return;
		}
		if (type.IsVerifierReference())
		{
			_builder.EmitOpCode(ILOpCode.Ldind_ref);
			return;
		}
		_builder.EmitOpCode(ILOpCode.Ldobj);
		EmitSymbolToken(type, syntaxNode);
	}

	private bool CanUseCallOnRefTypeReceiver(BoundExpression receiver)
	{
		if (receiver.Type.IsTypeParameter())
		{
			return false;
		}
		ConstantValue constantValue = receiver.ConstantValue;
		if (constantValue != null)
		{
			return !constantValue.IsNull;
		}
		switch (receiver.Kind)
		{
		case BoundKind.ArrayCreation:
			return true;
		case BoundKind.ObjectCreationExpression:
			return true;
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)receiver;
			switch (boundConversion.ConversionKind)
			{
			case ConversionKind.Boxing:
				return true;
			case ConversionKind.AnonymousFunction:
			case ConversionKind.MethodGroup:
				return true;
			case ConversionKind.ImplicitReference:
			case ConversionKind.ExplicitReference:
				return CanUseCallOnRefTypeReceiver(boundConversion.Operand);
			}
			break;
		}
		case BoundKind.ThisReference:
			return true;
		case BoundKind.FieldAccess:
			return ((BoundFieldAccess)receiver).FieldSymbol.IsCapturedFrame;
		case BoundKind.Local:
			return ((BoundLocal)receiver).LocalSymbol.SynthesizedKind == SynthesizedLocalKind.FrameCache;
		case BoundKind.DelegateCreationExpression:
			return true;
		case BoundKind.Sequence:
		{
			BoundExpression value = ((BoundSequence)receiver).Value;
			return CanUseCallOnRefTypeReceiver(value);
		}
		case BoundKind.AssignmentOperator:
		{
			BoundExpression right = ((BoundAssignmentOperator)receiver).Right;
			return CanUseCallOnRefTypeReceiver(right);
		}
		case BoundKind.TypeOfOperator:
			return true;
		case BoundKind.ConditionalReceiver:
			return true;
		}
		return false;
	}

	private bool IsThisReceiver(BoundExpression receiver)
	{
		switch (receiver.Kind)
		{
		case BoundKind.ThisReference:
			return true;
		case BoundKind.Sequence:
		{
			BoundExpression value = ((BoundSequence)receiver).Value;
			return IsThisReceiver(value);
		}
		default:
			return false;
		}
	}

	private void EmitCallExpression(BoundCall call, UseKind useKind)
	{
		MethodSymbol method = call.Method;
		BoundExpression receiverOpt = call.ReceiverOpt;
		LocalDefinition temp = null;
		if (method.IsDefaultValueTypeConstructor())
		{
			temp = EmitReceiverRef(receiverOpt, AddressKind.Writeable);
			_builder.EmitOpCode(ILOpCode.Initobj);
			EmitSymbolToken(method.ContainingType, call.Syntax);
			FreeOptTemp(temp);
			return;
		}
		ImmutableArray<BoundExpression> arguments = call.Arguments;
		CallKind callKind;
		if (method.IsStatic)
		{
			callKind = CallKind.Call;
		}
		else
		{
			TypeSymbol type = receiverOpt.Type;
			if (type.IsVerifierReference())
			{
				EmitExpression(receiverOpt, used: true);
				callKind = ((!receiverOpt.SuppressVirtualCalls && (method.IsMetadataVirtual() || !CanUseCallOnRefTypeReceiver(receiverOpt))) ? CallKind.CallVirt : CallKind.Call);
			}
			else if (type.IsVerifierValue())
			{
				NamedTypeSymbol containingType = method.ContainingType;
				if (containingType.IsVerifierValue())
				{
					AddressKind addressKind = (IsReadOnlyCall(method, containingType) ? AddressKind.ReadOnly : AddressKind.Writeable);
					if (MayUseCallForStructMethod(method))
					{
						temp = EmitReceiverRef(receiverOpt, addressKind);
						callKind = CallKind.Call;
					}
					else
					{
						temp = EmitReceiverRef(receiverOpt, addressKind);
						callKind = CallKind.ConstrainedCallVirt;
					}
				}
				else if (method.IsMetadataVirtual())
				{
					temp = EmitReceiverRef(receiverOpt, AddressKind.ReadOnly);
					callKind = CallKind.ConstrainedCallVirt;
				}
				else
				{
					EmitExpression(receiverOpt, used: true);
					EmitBox(type, receiverOpt.Syntax);
					callKind = CallKind.Call;
				}
			}
			else
			{
				callKind = ((type.IsReferenceType && !IsRef(receiverOpt)) ? CallKind.CallVirt : CallKind.ConstrainedCallVirt);
				temp = EmitReceiverRef(receiverOpt, (callKind == CallKind.ConstrainedCallVirt) ? AddressKind.Constrained : AddressKind.Writeable);
			}
		}
		MethodSymbol methodSymbol = method;
		if (method.IsOverride && callKind != CallKind.Call)
		{
			methodSymbol = method.GetConstructedLeastOverriddenMethod(_method.ContainingType);
		}
		if (callKind == CallKind.ConstrainedCallVirt && methodSymbol.ContainingType.IsValueType)
		{
			callKind = CallKind.Call;
		}
		if (callKind == CallKind.CallVirt)
		{
			if (IsThisReceiver(receiverOpt) && methodSymbol.ContainingType.IsSealed && (object)methodSymbol.ContainingModule == _method.ContainingModule)
			{
				callKind = CallKind.Call;
			}
			else if (methodSymbol.IsMetadataFinal && CanUseCallOnRefTypeReceiver(receiverOpt))
			{
				callKind = CallKind.Call;
			}
		}
		EmitArguments(arguments, method.Parameters, call.ArgumentRefKindsOpt);
		int callStackBehavior = GetCallStackBehavior(call);
		switch (callKind)
		{
		case CallKind.Call:
			_builder.EmitOpCode(ILOpCode.Call, callStackBehavior);
			break;
		case CallKind.CallVirt:
			_builder.EmitOpCode(ILOpCode.Callvirt, callStackBehavior);
			break;
		case CallKind.ConstrainedCallVirt:
			_builder.EmitOpCode(ILOpCode.Constrained);
			EmitSymbolToken(receiverOpt.Type, receiverOpt.Syntax);
			_builder.EmitOpCode(ILOpCode.Callvirt, callStackBehavior);
			break;
		}
		EmitSymbolToken(methodSymbol, call.Syntax, methodSymbol.IsVararg ? ((BoundArgListOperator)call.Arguments[call.Arguments.Length - 1]) : null);
		if (!method.ReturnsVoid)
		{
			EmitPopIfUnused(useKind != UseKind.Unused);
		}
		else if (_ilEmitStyle == ILEmitStyle.Debug)
		{
			_builder.EmitOpCode(ILOpCode.Nop);
		}
		if (useKind == UseKind.UsedAsValue && method.RefKind != RefKind.None)
		{
			EmitLoadIndirect(method.ReturnType, call.Syntax);
		}
		FreeOptTemp(temp);
	}

	private bool IsReadOnlyCall(MethodSymbol method, NamedTypeSymbol methodContainingType)
	{
		if (methodContainingType.IsReadOnly && method.MethodKind != MethodKind.Constructor)
		{
			return true;
		}
		if (methodContainingType.IsNullableType())
		{
			MethodSymbol originalDefinition = method.OriginalDefinition;
			if ((object)originalDefinition == _module.Compilation.GetSpecialTypeMember(SpecialMember.System_Nullable_T_GetValueOrDefault) || (object)originalDefinition == _module.Compilation.GetSpecialTypeMember(SpecialMember.System_Nullable_T_get_Value) || (object)originalDefinition == _module.Compilation.GetSpecialTypeMember(SpecialMember.System_Nullable_T_get_HasValue))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsRef(BoundExpression receiver)
	{
		return receiver.Kind switch
		{
			BoundKind.Local => ((BoundLocal)receiver).LocalSymbol.RefKind != RefKind.None, 
			BoundKind.Parameter => ((BoundParameter)receiver).ParameterSymbol.RefKind != RefKind.None, 
			BoundKind.Call => ((BoundCall)receiver).Method.RefKind != RefKind.None, 
			BoundKind.Dup => ((BoundDup)receiver).RefKind != RefKind.None, 
			BoundKind.Sequence => IsRef(((BoundSequence)receiver).Value), 
			_ => false, 
		};
	}

	private static int GetCallStackBehavior(BoundCall call)
	{
		int num = 0;
		if (!call.Method.ReturnsVoid)
		{
			num++;
		}
		if (!call.Method.IsStatic)
		{
			num--;
		}
		if (call.Method.IsVararg)
		{
			int num2 = call.Arguments.Length - 1;
			int length = ((BoundArgListOperator)call.Arguments[num2]).Arguments.Length;
			num -= num2;
			return num - length;
		}
		return num - call.Arguments.Length;
	}

	private static int GetObjCreationStackBehavior(BoundObjectCreationExpression objCreation)
	{
		int num = 0;
		num++;
		if (objCreation.Constructor.IsVararg)
		{
			int num2 = objCreation.Arguments.Length - 1;
			int length = ((BoundArgListOperator)objCreation.Arguments[num2]).Arguments.Length;
			num -= num2;
			return num - length;
		}
		return num - objCreation.Arguments.Length;
	}

	internal static bool MayUseCallForStructMethod(MethodSymbol method)
	{
		if (!method.IsMetadataVirtual())
		{
			return true;
		}
		MethodSymbol overriddenMethod = method.OverriddenMethod;
		if ((object)overriddenMethod == null || overriddenMethod.IsAbstract)
		{
			return true;
		}
		return method.ContainingType.SpecialType != SpecialType.None;
	}

	private void TreatLongsAsNative(Microsoft.Cci.PrimitiveTypeCode tc)
	{
		switch (tc)
		{
		case Microsoft.Cci.PrimitiveTypeCode.Int64:
			_builder.EmitOpCode(ILOpCode.Conv_ovf_i);
			break;
		case Microsoft.Cci.PrimitiveTypeCode.UInt64:
			_builder.EmitOpCode(ILOpCode.Conv_ovf_i_un);
			break;
		}
	}

	private void EmitArrayLength(BoundArrayLength expression, bool used)
	{
		EmitExpression(expression.Expression, used: true);
		_builder.EmitOpCode(ILOpCode.Ldlen);
		Microsoft.Cci.PrimitiveTypeCode primitiveTypeCode = expression.Type.PrimitiveTypeCode;
		Microsoft.Cci.PrimitiveTypeCode fromPredefTypeKind = (primitiveTypeCode.IsUnsigned() ? Microsoft.Cci.PrimitiveTypeCode.UIntPtr : Microsoft.Cci.PrimitiveTypeCode.IntPtr);
		_builder.EmitNumericConversion(fromPredefTypeKind, primitiveTypeCode, @checked: false);
		EmitPopIfUnused(used);
	}

	private void EmitArrayCreationExpression(BoundArrayCreation expression, bool used)
	{
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)expression.Type;
		EmitArrayIndices(expression.Bounds);
		if (arrayTypeSymbol.IsSZArray)
		{
			_builder.EmitOpCode(ILOpCode.Newarr);
			EmitSymbolToken(arrayTypeSymbol.ElementType, expression.Syntax);
		}
		else
		{
			_builder.EmitArrayCreation(PEModuleBuilder.Translate(arrayTypeSymbol), expression.Syntax, _diagnostics);
		}
		if (expression.InitializerOpt != null)
		{
			EmitArrayInitializers(arrayTypeSymbol, expression.InitializerOpt);
		}
		EmitPopIfUnused(used);
	}

	private void EmitConvertedStackAllocExpression(BoundConvertedStackAllocExpression expression, bool used)
	{
		EmitExpression(expression.Count, used);
		if (used)
		{
			_builder.EmitOpCode(ILOpCode.Localloc);
		}
		BoundArrayInitialization initializerOpt = expression.InitializerOpt;
		if (initializerOpt == null)
		{
			return;
		}
		if (used)
		{
			EmitStackAllocInitializers(expression.Type, initializerOpt);
			return;
		}
		foreach (BoundExpression initializer in initializerOpt.Initializers)
		{
			EmitExpression(initializer, used: false);
		}
	}

	private void EmitObjectCreationExpression(BoundObjectCreationExpression expression, bool used)
	{
		MethodSymbol constructor = expression.Constructor;
		if (constructor.IsDefaultValueTypeConstructor())
		{
			EmitInitObj(expression.Type, used, expression.Syntax);
		}
		else if (!used && ConstructorNotSideEffecting(constructor))
		{
			foreach (BoundExpression argument in expression.Arguments)
			{
				EmitExpression(argument, used: false);
			}
		}
		else if (!_module.Compilation.IsReadOnlySpanType(expression.Type) || expression.Arguments.Length != 1 || !TryEmitReadonlySpanAsBlobWrapper((NamedTypeSymbol)expression.Type, expression.Arguments[0], used, inPlace: false))
		{
			EmitArguments(expression.Arguments, constructor.Parameters, expression.ArgumentRefKindsOpt);
			int objCreationStackBehavior = GetObjCreationStackBehavior(expression);
			_builder.EmitOpCode(ILOpCode.Newobj, objCreationStackBehavior);
			EmitSymbolToken(constructor, expression.Syntax, constructor.IsVararg ? ((BoundArgListOperator)expression.Arguments[expression.Arguments.Length - 1]) : null);
			EmitPopIfUnused(used);
		}
	}

	private bool ConstructorNotSideEffecting(MethodSymbol constructor)
	{
		MethodSymbol originalDefinition = constructor.OriginalDefinition;
		CSharpCompilation compilation = _module.Compilation;
		if (originalDefinition == compilation.GetSpecialTypeMember(SpecialMember.System_Nullable_T__ctor))
		{
			return true;
		}
		if (originalDefinition.ContainingType.Name == "ValueTuple" && (originalDefinition == compilation.GetWellKnownTypeMember(WellKnownMember.System_ValueTuple_T2__ctor) || originalDefinition == compilation.GetWellKnownTypeMember(WellKnownMember.System_ValueTuple_T3__ctor) || originalDefinition == compilation.GetWellKnownTypeMember(WellKnownMember.System_ValueTuple_T4__ctor) || originalDefinition == compilation.GetWellKnownTypeMember(WellKnownMember.System_ValueTuple_T5__ctor) || originalDefinition == compilation.GetWellKnownTypeMember(WellKnownMember.System_ValueTuple_T6__ctor) || originalDefinition == compilation.GetWellKnownTypeMember(WellKnownMember.System_ValueTuple_T7__ctor) || originalDefinition == compilation.GetWellKnownTypeMember(WellKnownMember.System_ValueTuple_TRest__ctor) || originalDefinition == compilation.GetWellKnownTypeMember(WellKnownMember.System_ValueTuple_T1__ctor)))
		{
			return true;
		}
		return false;
	}

	private void EmitAssignmentExpression(BoundAssignmentOperator assignmentOperator, UseKind useKind)
	{
		if (!TryEmitAssignmentInPlace(assignmentOperator, useKind != UseKind.Unused))
		{
			bool lhsUsesStack = EmitAssignmentPreamble(assignmentOperator);
			EmitAssignmentValue(assignmentOperator);
			LocalDefinition temp = EmitAssignmentDuplication(assignmentOperator, useKind, lhsUsesStack);
			EmitStore(assignmentOperator);
			EmitAssignmentPostfix(assignmentOperator, temp, useKind);
		}
	}

	private bool TryEmitAssignmentInPlace(BoundAssignmentOperator assignmentOperator, bool used)
	{
		BoundExpression left = assignmentOperator.Left;
		if (used && !TargetIsNotOnHeap(left))
		{
			return false;
		}
		if (!SafeToGetWriteableReference(left))
		{
			return false;
		}
		BoundExpression right = assignmentOperator.Right;
		TypeSymbol type = right.Type;
		if (!type.IsTypeParameter() && (type.IsReferenceType || (right.ConstantValue != null && type.SpecialType != SpecialType.System_Decimal)))
		{
			return false;
		}
		if (right.IsDefaultValue())
		{
			InPlaceInit(left, used);
			return true;
		}
		if (right.Kind == BoundKind.ObjectCreationExpression && PartialCtorResultCannotEscape(left))
		{
			BoundObjectCreationExpression boundObjectCreationExpression = (BoundObjectCreationExpression)right;
			MethodSymbol constructor = boundObjectCreationExpression.Constructor;
			if (constructor.Parameters.All((ParameterSymbol p) => p.RefKind == RefKind.None) && !constructor.IsVararg)
			{
				InPlaceCtorCall(left, boundObjectCreationExpression, used);
				return true;
			}
		}
		return false;
	}

	private bool SafeToGetWriteableReference(BoundExpression left)
	{
		if (!HasHome(left, AddressKind.Writeable))
		{
			return false;
		}
		if (left.Kind == BoundKind.ArrayAccess && left.Type.TypeKind == TypeKind.TypeParameter && !left.Type.IsValueType)
		{
			return false;
		}
		if (left.Kind == BoundKind.FieldAccess)
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)left;
			if (boundFieldAccess.FieldSymbol.IsVolatile || DiagnosticsPass.IsNonAgileFieldAccess(boundFieldAccess, _module.Compilation))
			{
				return false;
			}
		}
		return true;
	}

	private void InPlaceInit(BoundExpression target, bool used)
	{
		EmitAddress(target, AddressKind.Writeable);
		_builder.EmitOpCode(ILOpCode.Initobj);
		EmitSymbolToken(target.Type, target.Syntax);
		if (used)
		{
			EmitExpression(target, used);
		}
	}

	private void InPlaceCtorCall(BoundExpression target, BoundObjectCreationExpression objCreation, bool used)
	{
		EmitAddress(target, AddressKind.Writeable);
		if (_module.Compilation.IsReadOnlySpanType(objCreation.Type) && objCreation.Arguments.Length == 1 && TryEmitReadonlySpanAsBlobWrapper((NamedTypeSymbol)objCreation.Type, objCreation.Arguments[0], used, inPlace: true))
		{
			if (used)
			{
				EmitExpression(target, used: true);
			}
			return;
		}
		MethodSymbol constructor = objCreation.Constructor;
		EmitArguments(objCreation.Arguments, constructor.Parameters, objCreation.ArgumentRefKindsOpt);
		int stackAdjustment = GetObjCreationStackBehavior(objCreation) - 2;
		_builder.EmitOpCode(ILOpCode.Call, stackAdjustment);
		EmitSymbolToken(constructor, objCreation.Syntax, constructor.IsVararg ? ((BoundArgListOperator)objCreation.Arguments[objCreation.Arguments.Length - 1]) : null);
		if (used)
		{
			EmitExpression(target, used: true);
		}
	}

	private bool PartialCtorResultCannotEscape(BoundExpression left)
	{
		if (TargetIsNotOnHeap(left))
		{
			if (_tryNestingLevel != 0)
			{
				if (left is BoundLocal localExpression && !_builder.PossiblyDefinedOutsideOfTry(GetLocal(localExpression)))
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool TargetIsNotOnHeap(BoundExpression left)
	{
		return left.Kind switch
		{
			BoundKind.Parameter => ((BoundParameter)left).ParameterSymbol.RefKind == RefKind.None, 
			BoundKind.Local => ((BoundLocal)left).LocalSymbol.RefKind == RefKind.None, 
			_ => false, 
		};
	}

	private bool EmitAssignmentPreamble(BoundAssignmentOperator assignmentOperator)
	{
		BoundExpression left = assignmentOperator.Left;
		bool result = false;
		switch (left.Kind)
		{
		case BoundKind.RefValueOperator:
			EmitRefValueAddress((BoundRefValueOperator)left);
			break;
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)left;
			if (!boundFieldAccess.FieldSymbol.IsStatic)
			{
				EmitReceiverRef(boundFieldAccess.ReceiverOpt, AddressKind.Writeable);
				result = true;
			}
			break;
		}
		case BoundKind.Parameter:
		{
			BoundParameter boundParameter = (BoundParameter)left;
			if (boundParameter.ParameterSymbol.RefKind != RefKind.None && !assignmentOperator.IsRef)
			{
				_builder.EmitLoadArgumentOpcode(ParameterSlot(boundParameter));
				result = true;
			}
			break;
		}
		case BoundKind.Local:
		{
			BoundLocal boundLocal = (BoundLocal)left;
			if (boundLocal.LocalSymbol.RefKind != RefKind.None && !assignmentOperator.IsRef)
			{
				if (!IsStackLocal(boundLocal.LocalSymbol))
				{
					LocalDefinition local = GetLocal(boundLocal);
					_builder.EmitLocalLoad(local);
				}
				result = true;
			}
			break;
		}
		case BoundKind.ArrayAccess:
		{
			BoundArrayAccess boundArrayAccess = (BoundArrayAccess)left;
			EmitExpression(boundArrayAccess.Expression, used: true);
			EmitArrayIndices(boundArrayAccess.Indices);
			result = true;
			break;
		}
		case BoundKind.ThisReference:
		{
			BoundThisReference expression3 = (BoundThisReference)left;
			EmitAddress(expression3, AddressKind.Writeable);
			result = true;
			break;
		}
		case BoundKind.Dup:
		{
			BoundDup expression2 = (BoundDup)left;
			EmitAddress(expression2, AddressKind.Writeable);
			result = true;
			break;
		}
		case BoundKind.ConditionalOperator:
		{
			BoundConditionalOperator expression = (BoundConditionalOperator)left;
			EmitAddress(expression, AddressKind.Writeable);
			result = true;
			break;
		}
		case BoundKind.PointerIndirectionOperator:
		{
			BoundPointerIndirectionOperator boundPointerIndirectionOperator = (BoundPointerIndirectionOperator)left;
			EmitExpression(boundPointerIndirectionOperator.Operand, used: true);
			result = true;
			break;
		}
		case BoundKind.Sequence:
		{
			BoundSequence boundSequence = (BoundSequence)left;
			DefineAndRecordLocals(boundSequence);
			EmitSideEffects(boundSequence);
			result = EmitAssignmentPreamble(assignmentOperator.Update(boundSequence.Value, assignmentOperator.Right, assignmentOperator.IsRef, assignmentOperator.Type));
			CloseScopeAndKeepLocals(boundSequence);
			break;
		}
		case BoundKind.Call:
		{
			BoundCall call = (BoundCall)left;
			EmitCallExpression(call, UseKind.UsedAsAddress);
			result = true;
			break;
		}
		case BoundKind.PreviousSubmissionReference:
		case BoundKind.PropertyAccess:
		case BoundKind.IndexerAccess:
			throw ExceptionUtilities.UnexpectedValue(left.Kind);
		case BoundKind.PseudoVariable:
			EmitPseudoVariableAddress((BoundPseudoVariable)left);
			result = true;
			break;
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)left;
			if (boundAssignmentOperator.IsRef)
			{
				EmitAssignmentExpression(boundAssignmentOperator, UseKind.UsedAsAddress);
				break;
			}
			goto default;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(left.Kind);
		case BoundKind.InstrumentationPayloadRoot:
		case BoundKind.ModuleVersionId:
			break;
		}
		return result;
	}

	private void EmitAssignmentValue(BoundAssignmentOperator assignmentOperator)
	{
		if (!assignmentOperator.IsRef)
		{
			EmitExpression(assignmentOperator.Right, used: true);
			return;
		}
		int num = _expressionTemps?.Count ?? 0;
		BoundExpression left = assignmentOperator.Left;
		LocalDefinition temp = EmitAddress(assignmentOperator.Right, (left.GetRefKind() == RefKind.In) ? AddressKind.ReadOnlyStrict : AddressKind.Writeable);
		AddExpressionTemp(temp);
		int num2 = _expressionTemps?.Count ?? 0;
		if (left.Kind == BoundKind.Local && ((BoundLocal)left).LocalSymbol.SynthesizedKind.IsLongLived() && num2 > num)
		{
			_expressionTemps.Count = num;
		}
	}

	private LocalDefinition EmitAssignmentDuplication(BoundAssignmentOperator assignmentOperator, UseKind useKind, bool lhsUsesStack)
	{
		LocalDefinition localDefinition = null;
		if (useKind != UseKind.Unused)
		{
			_builder.EmitOpCode(ILOpCode.Dup);
			if (lhsUsesStack)
			{
				localDefinition = AllocateTemp(assignmentOperator.Left.Type, assignmentOperator.Left.Syntax, assignmentOperator.IsRef ? LocalSlotConstraints.ByRef : LocalSlotConstraints.None);
				_builder.EmitLocalStore(localDefinition);
			}
		}
		return localDefinition;
	}

	private void EmitStore(BoundAssignmentOperator assignment)
	{
		BoundExpression left = assignment.Left;
		switch (left.Kind)
		{
		case BoundKind.FieldAccess:
			EmitFieldStore((BoundFieldAccess)left);
			return;
		case BoundKind.Local:
		{
			BoundLocal boundLocal = (BoundLocal)left;
			if (boundLocal.LocalSymbol.RefKind != RefKind.None && !assignment.IsRef)
			{
				EmitIndirectStore(boundLocal.LocalSymbol.Type, boundLocal.Syntax);
			}
			else if (!IsStackLocal(boundLocal.LocalSymbol))
			{
				_builder.EmitLocalStore(GetLocal(boundLocal));
			}
			return;
		}
		case BoundKind.ArrayAccess:
		{
			ArrayTypeSymbol arrayType = (ArrayTypeSymbol)((BoundArrayAccess)left).Expression.Type;
			EmitArrayElementStore(arrayType, left.Syntax);
			return;
		}
		case BoundKind.ThisReference:
			EmitThisStore((BoundThisReference)left);
			return;
		case BoundKind.Parameter:
			EmitParameterStore((BoundParameter)left, assignment.IsRef);
			return;
		case BoundKind.Dup:
			EmitIndirectStore(left.Type, left.Syntax);
			return;
		case BoundKind.ConditionalOperator:
			EmitIndirectStore(left.Type, left.Syntax);
			return;
		case BoundKind.PointerIndirectionOperator:
		case BoundKind.RefValueOperator:
		case BoundKind.PseudoVariable:
			EmitIndirectStore(left.Type, left.Syntax);
			return;
		case BoundKind.Sequence:
		{
			BoundSequence boundSequence = (BoundSequence)left;
			EmitStore(assignment.Update(boundSequence.Value, assignment.Right, assignment.IsRef, assignment.Type));
			return;
		}
		case BoundKind.Call:
			EmitIndirectStore(left.Type, left.Syntax);
			return;
		case BoundKind.ModuleVersionId:
			EmitModuleVersionIdStore((BoundModuleVersionId)left);
			return;
		case BoundKind.InstrumentationPayloadRoot:
			EmitInstrumentationPayloadRootStore((BoundInstrumentationPayloadRoot)left);
			return;
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)left;
			if (boundAssignmentOperator.IsRef)
			{
				EmitIndirectStore(boundAssignmentOperator.Type, left.Syntax);
				return;
			}
			break;
		}
		}
		throw ExceptionUtilities.UnexpectedValue(left.Kind);
	}

	private void EmitAssignmentPostfix(BoundAssignmentOperator assignment, LocalDefinition temp, UseKind useKind)
	{
		if (temp != null)
		{
			if (useKind == UseKind.UsedAsAddress)
			{
				_builder.EmitLocalAddress(temp);
			}
			else
			{
				_builder.EmitLocalLoad(temp);
			}
			FreeTemp(temp);
		}
		if (useKind == UseKind.UsedAsValue && assignment.IsRef)
		{
			EmitLoadIndirect(assignment.Type, assignment.Syntax);
		}
	}

	private void EmitThisStore(BoundThisReference thisRef)
	{
		_builder.EmitOpCode(ILOpCode.Stobj);
		EmitSymbolToken(thisRef.Type, thisRef.Syntax);
	}

	private void EmitArrayElementStore(ArrayTypeSymbol arrayType, SyntaxNode syntaxNode)
	{
		if (arrayType.IsSZArray)
		{
			EmitVectorElementStore(arrayType, syntaxNode);
		}
		else
		{
			_builder.EmitArrayElementStore(PEModuleBuilder.Translate(arrayType), syntaxNode, _diagnostics);
		}
	}

	private void EmitVectorElementStore(ArrayTypeSymbol arrayType, SyntaxNode syntaxNode)
	{
		TypeSymbol typeSymbol = arrayType.ElementType;
		if (typeSymbol.IsEnumType())
		{
			typeSymbol = ((NamedTypeSymbol)typeSymbol).EnumUnderlyingType;
		}
		switch (typeSymbol.PrimitiveTypeCode)
		{
		case Microsoft.Cci.PrimitiveTypeCode.Boolean:
		case Microsoft.Cci.PrimitiveTypeCode.Int8:
		case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			_builder.EmitOpCode(ILOpCode.Stelem_i1);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Char:
		case Microsoft.Cci.PrimitiveTypeCode.Int16:
		case Microsoft.Cci.PrimitiveTypeCode.UInt16:
			_builder.EmitOpCode(ILOpCode.Stelem_i2);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Int32:
		case Microsoft.Cci.PrimitiveTypeCode.UInt32:
			_builder.EmitOpCode(ILOpCode.Stelem_i4);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Int64:
		case Microsoft.Cci.PrimitiveTypeCode.UInt64:
			_builder.EmitOpCode(ILOpCode.Stelem_i8);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.IntPtr:
		case Microsoft.Cci.PrimitiveTypeCode.Pointer:
		case Microsoft.Cci.PrimitiveTypeCode.UIntPtr:
			_builder.EmitOpCode(ILOpCode.Stelem_i);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Float32:
			_builder.EmitOpCode(ILOpCode.Stelem_r4);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Float64:
			_builder.EmitOpCode(ILOpCode.Stelem_r8);
			return;
		}
		if (typeSymbol.IsVerifierReference())
		{
			_builder.EmitOpCode(ILOpCode.Stelem_ref);
			return;
		}
		_builder.EmitOpCode(ILOpCode.Stelem);
		EmitSymbolToken(typeSymbol, syntaxNode);
	}

	private void EmitFieldStore(BoundFieldAccess fieldAccess)
	{
		FieldSymbol fieldSymbol = fieldAccess.FieldSymbol;
		if (fieldSymbol.IsVolatile)
		{
			_builder.EmitOpCode(ILOpCode.Volatile);
		}
		_builder.EmitOpCode(fieldSymbol.IsStatic ? ILOpCode.Stsfld : ILOpCode.Stfld);
		EmitSymbolToken(fieldSymbol, fieldAccess.Syntax);
	}

	private void EmitParameterStore(BoundParameter parameter, bool refAssign)
	{
		int argNumber = ParameterSlot(parameter);
		if (parameter.ParameterSymbol.RefKind != RefKind.None && !refAssign)
		{
			EmitIndirectStore(parameter.ParameterSymbol.Type, parameter.Syntax);
		}
		else
		{
			_builder.EmitStoreArgumentOpcode(argNumber);
		}
	}

	private void EmitIndirectStore(TypeSymbol type, SyntaxNode syntaxNode)
	{
		if (type.IsEnumType())
		{
			type = ((NamedTypeSymbol)type).EnumUnderlyingType;
		}
		switch (type.PrimitiveTypeCode)
		{
		case Microsoft.Cci.PrimitiveTypeCode.Boolean:
		case Microsoft.Cci.PrimitiveTypeCode.Int8:
		case Microsoft.Cci.PrimitiveTypeCode.UInt8:
			_builder.EmitOpCode(ILOpCode.Stind_i1);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Char:
		case Microsoft.Cci.PrimitiveTypeCode.Int16:
		case Microsoft.Cci.PrimitiveTypeCode.UInt16:
			_builder.EmitOpCode(ILOpCode.Stind_i2);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Int32:
		case Microsoft.Cci.PrimitiveTypeCode.UInt32:
			_builder.EmitOpCode(ILOpCode.Stind_i4);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Int64:
		case Microsoft.Cci.PrimitiveTypeCode.UInt64:
			_builder.EmitOpCode(ILOpCode.Stind_i8);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.IntPtr:
		case Microsoft.Cci.PrimitiveTypeCode.Pointer:
		case Microsoft.Cci.PrimitiveTypeCode.UIntPtr:
			_builder.EmitOpCode(ILOpCode.Stind_i);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Float32:
			_builder.EmitOpCode(ILOpCode.Stind_r4);
			return;
		case Microsoft.Cci.PrimitiveTypeCode.Float64:
			_builder.EmitOpCode(ILOpCode.Stind_r8);
			return;
		}
		if (type.IsVerifierReference())
		{
			_builder.EmitOpCode(ILOpCode.Stind_ref);
			return;
		}
		_builder.EmitOpCode(ILOpCode.Stobj);
		EmitSymbolToken(type, syntaxNode);
	}

	private void EmitPopIfUnused(bool used)
	{
		if (!used)
		{
			_builder.EmitOpCode(ILOpCode.Pop);
		}
	}

	private void EmitIsExpression(BoundIsOperator isOp, bool used)
	{
		BoundExpression operand = isOp.Operand;
		EmitExpression(operand, used);
		if (used)
		{
			if (!operand.Type.IsVerifierReference())
			{
				EmitBox(operand.Type, operand.Syntax);
			}
			_builder.EmitOpCode(ILOpCode.Isinst);
			EmitSymbolToken(isOp.TargetType.Type, isOp.Syntax);
			_builder.EmitOpCode(ILOpCode.Ldnull);
			_builder.EmitOpCode(ILOpCode.Cgt_un);
		}
	}

	private void EmitAsExpression(BoundAsOperator asOp, bool used)
	{
		BoundExpression operand = asOp.Operand;
		EmitExpression(operand, used);
		if (used)
		{
			TypeSymbol type = operand.Type;
			TypeSymbol type2 = asOp.Type;
			if ((object)type != null && !type.IsVerifierReference())
			{
				EmitBox(type, operand.Syntax);
			}
			_builder.EmitOpCode(ILOpCode.Isinst);
			EmitSymbolToken(type2, asOp.Syntax);
			if (!type2.IsVerifierReference())
			{
				_builder.EmitOpCode(ILOpCode.Unbox_any);
				EmitSymbolToken(type2, asOp.Syntax);
			}
		}
	}

	private void EmitDefaultValue(TypeSymbol type, bool used, SyntaxNode syntaxNode)
	{
		if (!used)
		{
			return;
		}
		if (!type.IsTypeParameter() && type.SpecialType != SpecialType.System_Decimal)
		{
			ConstantValue defaultValue = type.GetDefaultValue();
			if (defaultValue != null)
			{
				_builder.EmitConstantValue(defaultValue);
				return;
			}
		}
		if (type.IsPointerType() || type.SpecialType == SpecialType.System_UIntPtr)
		{
			_builder.EmitOpCode(ILOpCode.Ldc_i4_0);
			_builder.EmitOpCode(ILOpCode.Conv_u);
		}
		else if (type.SpecialType == SpecialType.System_IntPtr)
		{
			_builder.EmitOpCode(ILOpCode.Ldc_i4_0);
			_builder.EmitOpCode(ILOpCode.Conv_i);
		}
		else
		{
			EmitInitObj(type, used: true, syntaxNode);
		}
	}

	private void EmitDefaultExpression(BoundDefaultExpression expression, bool used)
	{
		EmitDefaultValue(expression.Type, used, expression.Syntax);
	}

	private void EmitConstantExpression(TypeSymbol type, ConstantValue constantValue, bool used, SyntaxNode syntaxNode)
	{
		if (used)
		{
			if ((object)type != null && type.TypeKind == TypeKind.TypeParameter && constantValue.IsNull)
			{
				EmitInitObj(type, used, syntaxNode);
			}
			else
			{
				_builder.EmitConstantValue(constantValue);
			}
		}
	}

	private void EmitInitObj(TypeSymbol type, bool used, SyntaxNode syntaxNode)
	{
		if (used)
		{
			LocalDefinition localDefinition = AllocateTemp(type, syntaxNode);
			_builder.EmitLocalAddress(localDefinition);
			_builder.EmitOpCode(ILOpCode.Initobj);
			EmitSymbolToken(type, syntaxNode);
			_builder.EmitLocalLoad(localDefinition);
			FreeTemp(localDefinition);
		}
	}

	private void EmitGetTypeFromHandle(BoundTypeOf boundTypeOf)
	{
		_builder.EmitOpCode(ILOpCode.Call, 0);
		MethodSymbol getTypeFromHandle = boundTypeOf.GetTypeFromHandle;
		EmitSymbolToken(getTypeFromHandle, boundTypeOf.Syntax, null);
	}

	private void EmitTypeOfExpression(BoundTypeOfOperator boundTypeOfOperator)
	{
		TypeSymbol type = boundTypeOfOperator.SourceType.Type;
		_builder.EmitOpCode(ILOpCode.Ldtoken);
		EmitSymbolToken(type, boundTypeOfOperator.SourceType.Syntax);
		EmitGetTypeFromHandle(boundTypeOfOperator);
	}

	private void EmitSizeOfExpression(BoundSizeOfOperator boundSizeOfOperator)
	{
		TypeSymbol type = boundSizeOfOperator.SourceType.Type;
		_builder.EmitOpCode(ILOpCode.Sizeof);
		EmitSymbolToken(type, boundSizeOfOperator.SourceType.Syntax);
	}

	private void EmitMethodDefIndexExpression(BoundMethodDefIndex node)
	{
		_builder.EmitOpCode(ILOpCode.Ldtoken);
		MethodSymbol method = node.Method.PartialDefinitionPart ?? node.Method;
		EmitSymbolToken(method, node.Syntax, null, encodeAsRawDefinitionToken: true);
	}

	private void EmitMaximumMethodDefIndexExpression(BoundMaximumMethodDefIndex node)
	{
		_builder.EmitOpCode(ILOpCode.Ldtoken);
		_builder.EmitGreatestMethodToken();
	}

	private void EmitModuleVersionIdLoad(BoundModuleVersionId node)
	{
		_builder.EmitOpCode(ILOpCode.Ldsfld);
		EmitModuleVersionIdToken(node);
	}

	private void EmitModuleVersionIdStore(BoundModuleVersionId node)
	{
		_builder.EmitOpCode(ILOpCode.Stsfld);
		EmitModuleVersionIdToken(node);
	}

	private void EmitModuleVersionIdToken(BoundModuleVersionId node)
	{
		_builder.EmitToken(_module.GetModuleVersionId(_module.Translate(node.Type, node.Syntax, _diagnostics), node.Syntax, _diagnostics), node.Syntax, _diagnostics);
	}

	private void EmitModuleVersionIdStringLoad(BoundModuleVersionIdString node)
	{
		_builder.EmitOpCode(ILOpCode.Ldstr);
		_builder.EmitModuleVersionIdStringToken();
	}

	private void EmitInstrumentationPayloadRootLoad(BoundInstrumentationPayloadRoot node)
	{
		_builder.EmitOpCode(ILOpCode.Ldsfld);
		EmitInstrumentationPayloadRootToken(node);
	}

	private void EmitInstrumentationPayloadRootStore(BoundInstrumentationPayloadRoot node)
	{
		_builder.EmitOpCode(ILOpCode.Stsfld);
		EmitInstrumentationPayloadRootToken(node);
	}

	private void EmitInstrumentationPayloadRootToken(BoundInstrumentationPayloadRoot node)
	{
		_builder.EmitToken(_module.GetInstrumentationPayloadRoot(node.AnalysisKind, _module.Translate(node.Type, node.Syntax, _diagnostics), node.Syntax, _diagnostics), node.Syntax, _diagnostics);
	}

	private void EmitSourceDocumentIndex(BoundSourceDocumentIndex node)
	{
		_builder.EmitOpCode(ILOpCode.Ldtoken);
		_builder.EmitSourceDocumentIndexToken(node.Document);
	}

	private void EmitMethodInfoExpression(BoundMethodInfo node)
	{
		_builder.EmitOpCode(ILOpCode.Ldtoken);
		EmitSymbolToken(node.Method, node.Syntax, null);
		MethodSymbol getMethodFromHandle = node.GetMethodFromHandle;
		if (getMethodFromHandle.ParameterCount == 1)
		{
			_builder.EmitOpCode(ILOpCode.Call, 0);
		}
		else
		{
			_builder.EmitOpCode(ILOpCode.Ldtoken);
			EmitSymbolToken(node.Method.ContainingType, node.Syntax);
			_builder.EmitOpCode(ILOpCode.Call, -1);
		}
		EmitSymbolToken(getMethodFromHandle, node.Syntax, null);
		if (node.Type != getMethodFromHandle.ReturnType)
		{
			_builder.EmitOpCode(ILOpCode.Castclass);
			EmitSymbolToken(node.Type, node.Syntax);
		}
	}

	private void EmitFieldInfoExpression(BoundFieldInfo node)
	{
		_builder.EmitOpCode(ILOpCode.Ldtoken);
		EmitSymbolToken(node.Field, node.Syntax);
		MethodSymbol getFieldFromHandle = node.GetFieldFromHandle;
		if (getFieldFromHandle.ParameterCount == 1)
		{
			_builder.EmitOpCode(ILOpCode.Call, 0);
		}
		else
		{
			_builder.EmitOpCode(ILOpCode.Ldtoken);
			EmitSymbolToken(node.Field.ContainingType, node.Syntax);
			_builder.EmitOpCode(ILOpCode.Call, -1);
		}
		EmitSymbolToken(getFieldFromHandle, node.Syntax, null);
		if (node.Type != getFieldFromHandle.ReturnType)
		{
			_builder.EmitOpCode(ILOpCode.Castclass);
			EmitSymbolToken(node.Type, node.Syntax);
		}
	}

	private void EmitConditionalOperator(BoundConditionalOperator expr, bool used)
	{
		object dest = new object();
		object label = new object();
		EmitCondBranch(expr.Condition, ref dest, sense: true);
		EmitExpression(expr.Alternative, used);
		TypeSymbol typeSymbol = StackMergeType(expr.Alternative);
		if (used)
		{
			if (IsVarianceCast(expr.Type, typeSymbol))
			{
				EmitStaticCast(expr.Type, expr.Syntax);
				_ = expr.Type;
			}
			else if (expr.Type.IsInterfaceType() && expr.Type != typeSymbol)
			{
				EmitStaticCast(expr.Type, expr.Syntax);
			}
		}
		_builder.EmitBranch(ILOpCode.Br, label);
		if (used)
		{
			_builder.AdjustStack(-1);
		}
		_builder.MarkLabel(dest);
		EmitExpression(expr.Consequence, used);
		if (used)
		{
			TypeSymbol typeSymbol2 = StackMergeType(expr.Consequence);
			if (IsVarianceCast(expr.Type, typeSymbol2))
			{
				EmitStaticCast(expr.Type, expr.Syntax);
				_ = expr.Type;
			}
			else if (expr.Type.IsInterfaceType() && expr.Type != typeSymbol2)
			{
				EmitStaticCast(expr.Type, expr.Syntax);
			}
		}
		_builder.MarkLabel(label);
	}

	private void EmitNullCoalescingOperator(BoundNullCoalescingOperator expr, bool used)
	{
		EmitExpression(expr.LeftOperand, used: true);
		TypeSymbol typeSymbol = StackMergeType(expr.LeftOperand);
		if (used)
		{
			if (IsVarianceCast(expr.Type, typeSymbol))
			{
				EmitStaticCast(expr.Type, expr.Syntax);
				_ = expr.Type;
			}
			else if (expr.Type.IsInterfaceType() && expr.Type != typeSymbol)
			{
				EmitStaticCast(expr.Type, expr.Syntax);
			}
			_builder.EmitOpCode(ILOpCode.Dup);
		}
		if (expr.Type.IsTypeParameter())
		{
			EmitBox(expr.Type, expr.LeftOperand.Syntax);
		}
		object label = new object();
		_builder.EmitBranch(ILOpCode.Brtrue, label);
		if (used)
		{
			_builder.EmitOpCode(ILOpCode.Pop);
		}
		EmitExpression(expr.RightOperand, used);
		if (used)
		{
			TypeSymbol typeSymbol2 = StackMergeType(expr.RightOperand);
			if (IsVarianceCast(expr.Type, typeSymbol2))
			{
				EmitStaticCast(expr.Type, expr.Syntax);
				_ = expr.Type;
			}
		}
		_builder.MarkLabel(label);
	}

	private TypeSymbol StackMergeType(BoundExpression expr)
	{
		if (!expr.Type.IsInterfaceType() && !expr.Type.IsDelegateType())
		{
			return expr.Type;
		}
		switch (expr.Kind)
		{
		case BoundKind.Conversion:
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			ConversionKind conversionKind = boundConversion.ConversionKind;
			if (conversionKind.IsImplicitConversion() && conversionKind != ConversionKind.MethodGroup && conversionKind != ConversionKind.DefaultOrNullLiteral)
			{
				return StackMergeType(boundConversion.Operand);
			}
			break;
		}
		case BoundKind.AssignmentOperator:
		{
			BoundAssignmentOperator boundAssignmentOperator = (BoundAssignmentOperator)expr;
			return StackMergeType(boundAssignmentOperator.Right);
		}
		case BoundKind.Sequence:
		{
			BoundSequence boundSequence = (BoundSequence)expr;
			return StackMergeType(boundSequence.Value);
		}
		case BoundKind.Local:
		{
			BoundLocal boundLocal = (BoundLocal)expr;
			if (IsStackLocal(boundLocal.LocalSymbol))
			{
				return null;
			}
			break;
		}
		case BoundKind.Dup:
			return null;
		}
		return expr.Type;
	}

	private static bool IsVarianceCast(TypeSymbol to, TypeSymbol from)
	{
		if (to == from)
		{
			return false;
		}
		if ((object)from == null)
		{
			return true;
		}
		if (to.IsArray())
		{
			return IsVarianceCast(((ArrayTypeSymbol)to).ElementType, ((ArrayTypeSymbol)from).ElementType);
		}
		if (!to.IsDelegateType() || !(to != from))
		{
			if (to.IsInterfaceType() && from.IsInterfaceType())
			{
				return !from.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Contains((NamedTypeSymbol)to);
			}
			return false;
		}
		return true;
	}

	private void EmitStaticCast(TypeSymbol to, SyntaxNode syntax)
	{
		LocalDefinition localDefinition = AllocateTemp(to, syntax);
		_builder.EmitLocalStore(localDefinition);
		_builder.EmitLocalLoad(localDefinition);
		FreeTemp(localDefinition);
	}

	private void EmitBox(TypeSymbol type, SyntaxNode syntaxNode)
	{
		_builder.EmitOpCode(ILOpCode.Box);
		EmitSymbolToken(type, syntaxNode);
	}

	private void EmitUnaryOperatorExpression(BoundUnaryOperator expression, bool used)
	{
		UnaryOperatorKind operatorKind = expression.OperatorKind;
		if (operatorKind.IsChecked())
		{
			EmitUnaryCheckedOperatorExpression(expression, used);
			return;
		}
		if (!used)
		{
			EmitExpression(expression.Operand, used: false);
			return;
		}
		if (operatorKind == UnaryOperatorKind.BoolLogicalNegation)
		{
			EmitCondExpr(expression.Operand, sense: false);
			return;
		}
		EmitExpression(expression.Operand, used: true);
		switch (operatorKind.Operator())
		{
		case UnaryOperatorKind.UnaryMinus:
			_builder.EmitOpCode(ILOpCode.Neg);
			break;
		case UnaryOperatorKind.BitwiseComplement:
			_builder.EmitOpCode(ILOpCode.Not);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(operatorKind.Operator());
		case UnaryOperatorKind.UnaryPlus:
			break;
		}
	}

	private void EmitBinaryOperatorExpression(BoundBinaryOperator expression, bool used)
	{
		BinaryOperatorKind operatorKind = expression.OperatorKind;
		if (operatorKind.EmitsAsCheckedInstruction())
		{
			EmitBinaryOperator(expression);
		}
		else
		{
			if (!used && !operatorKind.IsLogical() && !OperatorHasSideEffects(operatorKind))
			{
				EmitExpression(expression.Left, used: false);
				EmitExpression(expression.Right, used: false);
				return;
			}
			if (IsConditional(operatorKind))
			{
				EmitBinaryCondOperator(expression, sense: true);
			}
			else
			{
				EmitBinaryOperator(expression);
			}
		}
		EmitPopIfUnused(used);
	}

	private void EmitBinaryOperator(BoundBinaryOperator expression)
	{
		BoundExpression left = expression.Left;
		if (left.Kind != BoundKind.BinaryOperator || left.ConstantValue != null)
		{
			EmitBinaryOperatorSimple(expression);
			return;
		}
		BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)left;
		BinaryOperatorKind operatorKind = boundBinaryOperator.OperatorKind;
		if (!operatorKind.EmitsAsCheckedInstruction() && IsConditional(operatorKind))
		{
			EmitBinaryOperatorSimple(expression);
			return;
		}
		ArrayBuilder<BoundBinaryOperator> instance = ArrayBuilder<BoundBinaryOperator>.GetInstance();
		instance.Push(expression);
		do
		{
			instance.Push(boundBinaryOperator);
			left = boundBinaryOperator.Left;
			if (left.Kind != BoundKind.BinaryOperator || left.ConstantValue != null)
			{
				break;
			}
			boundBinaryOperator = (BoundBinaryOperator)left;
			operatorKind = boundBinaryOperator.OperatorKind;
		}
		while (operatorKind.EmitsAsCheckedInstruction() || !IsConditional(operatorKind));
		EmitExpression(left, used: true);
		do
		{
			boundBinaryOperator = instance.Pop();
			EmitExpression(boundBinaryOperator.Right, used: true);
			bool flag = boundBinaryOperator.OperatorKind.EmitsAsCheckedInstruction();
			if (flag)
			{
				EmitBinaryCheckedOperatorInstruction(boundBinaryOperator);
			}
			else
			{
				EmitBinaryOperatorInstruction(boundBinaryOperator);
			}
			EmitConversionToEnumUnderlyingType(boundBinaryOperator, flag);
		}
		while (instance.Count > 0);
		instance.Free();
	}

	private void EmitBinaryOperatorSimple(BoundBinaryOperator expression)
	{
		EmitExpression(expression.Left, used: true);
		EmitExpression(expression.Right, used: true);
		bool flag = expression.OperatorKind.EmitsAsCheckedInstruction();
		if (flag)
		{
			EmitBinaryCheckedOperatorInstruction(expression);
		}
		else
		{
			EmitBinaryOperatorInstruction(expression);
		}
		EmitConversionToEnumUnderlyingType(expression, flag);
	}

	private void EmitBinaryOperatorInstruction(BoundBinaryOperator expression)
	{
		switch (expression.OperatorKind.Operator())
		{
		case BinaryOperatorKind.Multiplication:
			_builder.EmitOpCode(ILOpCode.Mul);
			break;
		case BinaryOperatorKind.Addition:
			_builder.EmitOpCode(ILOpCode.Add);
			break;
		case BinaryOperatorKind.Subtraction:
			_builder.EmitOpCode(ILOpCode.Sub);
			break;
		case BinaryOperatorKind.Division:
			if (IsUnsignedBinaryOperator(expression))
			{
				_builder.EmitOpCode(ILOpCode.Div_un);
			}
			else
			{
				_builder.EmitOpCode(ILOpCode.Div);
			}
			break;
		case BinaryOperatorKind.Remainder:
			if (IsUnsignedBinaryOperator(expression))
			{
				_builder.EmitOpCode(ILOpCode.Rem_un);
			}
			else
			{
				_builder.EmitOpCode(ILOpCode.Rem);
			}
			break;
		case BinaryOperatorKind.LeftShift:
			_builder.EmitOpCode(ILOpCode.Shl);
			break;
		case BinaryOperatorKind.RightShift:
			if (IsUnsignedBinaryOperator(expression))
			{
				_builder.EmitOpCode(ILOpCode.Shr_un);
			}
			else
			{
				_builder.EmitOpCode(ILOpCode.Shr);
			}
			break;
		case BinaryOperatorKind.And:
			_builder.EmitOpCode(ILOpCode.And);
			break;
		case BinaryOperatorKind.Xor:
			_builder.EmitOpCode(ILOpCode.Xor);
			break;
		case BinaryOperatorKind.Or:
			_builder.EmitOpCode(ILOpCode.Or);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(expression.OperatorKind.Operator());
		}
	}

	private void EmitShortCircuitingOperator(BoundBinaryOperator condition, bool sense, bool stopSense, bool stopValue)
	{
		object dest = null;
		EmitCondBranch(condition.Left, ref dest, stopSense);
		EmitCondExpr(condition.Right, sense);
		if (dest != null)
		{
			object label = new object();
			_builder.EmitBranch(ILOpCode.Br, label);
			_builder.AdjustStack(-1);
			_builder.MarkLabel(dest);
			_builder.EmitBoolConstant(stopValue);
			_builder.MarkLabel(label);
		}
	}

	private void EmitBinaryCondOperator(BoundBinaryOperator binOp, bool sense)
	{
		bool flag = sense;
		BinaryOperatorKind binaryOperatorKind = binOp.OperatorKind.OperatorWithLogical();
		int num;
		if (binaryOperatorKind <= BinaryOperatorKind.GreaterThanOrEqual)
		{
			if (binaryOperatorKind <= BinaryOperatorKind.NotEqual)
			{
				if (binaryOperatorKind != BinaryOperatorKind.Equal)
				{
					if (binaryOperatorKind != BinaryOperatorKind.NotEqual)
					{
						goto IL_01d2;
					}
					sense = !sense;
				}
				ConstantValue constantValue = binOp.Left.ConstantValue;
				BoundExpression boundExpression = binOp.Right;
				if (constantValue == null)
				{
					constantValue = boundExpression.ConstantValue;
					boundExpression = binOp.Left;
				}
				if (constantValue != null)
				{
					if (constantValue.IsDefaultValue)
					{
						if (!constantValue.IsFloating)
						{
							if (sense)
							{
								EmitIsNullOrZero(boundExpression, constantValue);
							}
							else
							{
								EmitIsNotNullOrZero(boundExpression, constantValue);
							}
							return;
						}
					}
					else if (constantValue.IsBoolean)
					{
						EmitExpression(boundExpression, used: true);
						EmitIsSense(sense);
						return;
					}
				}
				EmitBinaryCondOperatorHelper(ILOpCode.Ceq, binOp.Left, binOp.Right, sense);
				return;
			}
			if (binaryOperatorKind != BinaryOperatorKind.GreaterThan)
			{
				if (binaryOperatorKind != BinaryOperatorKind.LessThan)
				{
					if (binaryOperatorKind != BinaryOperatorKind.GreaterThanOrEqual)
					{
						goto IL_01d2;
					}
					num = 3;
					sense = !sense;
				}
				else
				{
					num = 0;
				}
			}
			else
			{
				num = 2;
			}
		}
		else
		{
			if (binaryOperatorKind > BinaryOperatorKind.Xor)
			{
				if (binaryOperatorKind != BinaryOperatorKind.Or)
				{
					if (binaryOperatorKind != BinaryOperatorKind.LogicalAnd)
					{
						if (binaryOperatorKind != BinaryOperatorKind.LogicalOr)
						{
							goto IL_01d2;
						}
						flag = !flag;
					}
					if (!flag)
					{
						EmitShortCircuitingOperator(binOp, sense, sense, stopValue: true);
					}
					else
					{
						EmitShortCircuitingOperator(binOp, sense, !sense, stopValue: false);
					}
				}
				else
				{
					EmitBinaryCondOperatorHelper(ILOpCode.Or, binOp.Left, binOp.Right, sense);
				}
				return;
			}
			if (binaryOperatorKind != BinaryOperatorKind.LessThanOrEqual)
			{
				switch (binaryOperatorKind)
				{
				case BinaryOperatorKind.And:
					EmitBinaryCondOperatorHelper(ILOpCode.And, binOp.Left, binOp.Right, sense);
					return;
				case BinaryOperatorKind.Xor:
					if (sense)
					{
						EmitBinaryCondOperatorHelper(ILOpCode.Xor, binOp.Left, binOp.Right, sense: true);
					}
					else
					{
						EmitBinaryCondOperatorHelper(ILOpCode.Ceq, binOp.Left, binOp.Right, sense: true);
					}
					return;
				}
				goto IL_01d2;
			}
			num = 1;
			sense = !sense;
		}
		if (IsUnsignedBinaryOperator(binOp))
		{
			num += 4;
		}
		else if (IsFloat(binOp.OperatorKind))
		{
			num += 8;
		}
		EmitBinaryCondOperatorHelper(s_compOpCodes[num], binOp.Left, binOp.Right, sense);
		return;
		IL_01d2:
		throw ExceptionUtilities.UnexpectedValue(binOp.OperatorKind.OperatorWithLogical());
	}

	private void EmitIsNotNullOrZero(BoundExpression comparand, ConstantValue nullOrZero)
	{
		EmitExpression(comparand, used: true);
		TypeSymbol type = comparand.Type;
		if (type.IsReferenceType && !type.IsVerifierReference())
		{
			EmitBox(type, comparand.Syntax);
		}
		_builder.EmitConstantValue(nullOrZero);
		_builder.EmitOpCode(ILOpCode.Cgt_un);
	}

	private void EmitIsNullOrZero(BoundExpression comparand, ConstantValue nullOrZero)
	{
		EmitExpression(comparand, used: true);
		TypeSymbol type = comparand.Type;
		if (type.IsReferenceType && !type.IsVerifierReference())
		{
			EmitBox(type, comparand.Syntax);
		}
		_builder.EmitConstantValue(nullOrZero);
		_builder.EmitOpCode(ILOpCode.Ceq);
	}

	private void EmitBinaryCondOperatorHelper(ILOpCode opCode, BoundExpression left, BoundExpression right, bool sense)
	{
		EmitExpression(left, used: true);
		EmitExpression(right, used: true);
		_builder.EmitOpCode(opCode);
		EmitIsSense(sense);
	}

	private void EmitCondExpr(BoundExpression condition, bool sense)
	{
		while (condition.Kind == BoundKind.UnaryOperator)
		{
			condition = ((BoundUnaryOperator)condition).Operand;
			sense = !sense;
		}
		ConstantValue constantValue = condition.ConstantValue;
		if (constantValue != null)
		{
			bool booleanValue = constantValue.BooleanValue;
			_builder.EmitBoolConstant(booleanValue == sense);
			return;
		}
		if (condition.Kind == BoundKind.BinaryOperator)
		{
			BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)condition;
			if (IsConditional(boundBinaryOperator.OperatorKind))
			{
				EmitBinaryCondOperator(boundBinaryOperator, sense);
				return;
			}
		}
		EmitExpression(condition, used: true);
		EmitIsSense(sense);
	}

	private void EmitUnaryCheckedOperatorExpression(BoundUnaryOperator expression, bool used)
	{
		UnaryOperatorKind unaryOperatorKind = expression.OperatorKind.OperandTypes();
		_builder.EmitOpCode(ILOpCode.Ldc_i4_0);
		if (unaryOperatorKind == UnaryOperatorKind.Long)
		{
			_builder.EmitOpCode(ILOpCode.Conv_i8);
		}
		EmitExpression(expression.Operand, used: true);
		_builder.EmitOpCode(ILOpCode.Sub_ovf);
		EmitPopIfUnused(used);
	}

	private void EmitConversionToEnumUnderlyingType(BoundBinaryOperator expression, bool @checked)
	{
		TypeSymbol typeSymbol;
		switch (expression.OperatorKind.Operator() | expression.OperatorKind.OperandTypes())
		{
		case BinaryOperatorKind.EnumAndUnderlyingAddition:
		case BinaryOperatorKind.EnumSubtraction:
		case BinaryOperatorKind.EnumAndUnderlyingSubtraction:
			typeSymbol = expression.Left.Type;
			break;
		case BinaryOperatorKind.EnumAnd:
		case BinaryOperatorKind.EnumXor:
		case BinaryOperatorKind.EnumOr:
			typeSymbol = null;
			break;
		case BinaryOperatorKind.UnderlyingAndEnumAddition:
		case BinaryOperatorKind.UnderlyingAndEnumSubtraction:
			typeSymbol = expression.Right.Type;
			break;
		default:
			typeSymbol = null;
			break;
		}
		if ((object)typeSymbol != null)
		{
			switch (typeSymbol.GetEnumUnderlyingType().SpecialType)
			{
			case SpecialType.System_Byte:
				_builder.EmitNumericConversion(Microsoft.Cci.PrimitiveTypeCode.Int32, Microsoft.Cci.PrimitiveTypeCode.UInt8, @checked);
				break;
			case SpecialType.System_SByte:
				_builder.EmitNumericConversion(Microsoft.Cci.PrimitiveTypeCode.Int32, Microsoft.Cci.PrimitiveTypeCode.Int8, @checked);
				break;
			case SpecialType.System_Int16:
				_builder.EmitNumericConversion(Microsoft.Cci.PrimitiveTypeCode.Int32, Microsoft.Cci.PrimitiveTypeCode.Int16, @checked);
				break;
			case SpecialType.System_UInt16:
				_builder.EmitNumericConversion(Microsoft.Cci.PrimitiveTypeCode.Int32, Microsoft.Cci.PrimitiveTypeCode.UInt16, @checked);
				break;
			}
		}
	}

	private void EmitBinaryCheckedOperatorInstruction(BoundBinaryOperator expression)
	{
		bool flag = IsUnsignedBinaryOperator(expression);
		switch (expression.OperatorKind.Operator())
		{
		case BinaryOperatorKind.Multiplication:
			if (flag)
			{
				_builder.EmitOpCode(ILOpCode.Mul_ovf_un);
			}
			else
			{
				_builder.EmitOpCode(ILOpCode.Mul_ovf);
			}
			break;
		case BinaryOperatorKind.Addition:
			if (flag)
			{
				_builder.EmitOpCode(ILOpCode.Add_ovf_un);
			}
			else
			{
				_builder.EmitOpCode(ILOpCode.Add_ovf);
			}
			break;
		case BinaryOperatorKind.Subtraction:
			if (flag)
			{
				_builder.EmitOpCode(ILOpCode.Sub_ovf_un);
			}
			else
			{
				_builder.EmitOpCode(ILOpCode.Sub_ovf);
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(expression.OperatorKind.Operator());
		}
	}

	private static bool OperatorHasSideEffects(BinaryOperatorKind kind)
	{
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		if (binaryOperatorKind == BinaryOperatorKind.Division || binaryOperatorKind == BinaryOperatorKind.Remainder)
		{
			return true;
		}
		return kind.IsChecked();
	}

	private void EmitIsSense(bool sense)
	{
		if (!sense)
		{
			_builder.EmitOpCode(ILOpCode.Ldc_i4_0);
			_builder.EmitOpCode(ILOpCode.Ceq);
		}
	}

	private static bool IsUnsigned(SpecialType type)
	{
		switch (type)
		{
		case SpecialType.System_Byte:
		case SpecialType.System_UInt16:
		case SpecialType.System_UInt32:
		case SpecialType.System_UInt64:
			return true;
		default:
			return false;
		}
	}

	private static bool IsUnsignedBinaryOperator(BoundBinaryOperator op)
	{
		switch (op.OperatorKind.OperandTypes())
		{
		case BinaryOperatorKind.Enum:
		case BinaryOperatorKind.EnumAndUnderlying:
			return IsUnsigned(Binder.GetEnumPromotedType(op.Left.Type.GetEnumUnderlyingType().SpecialType));
		case BinaryOperatorKind.UnderlyingAndEnum:
			return IsUnsigned(Binder.GetEnumPromotedType(op.Right.Type.GetEnumUnderlyingType().SpecialType));
		case BinaryOperatorKind.UInt:
		case BinaryOperatorKind.ULong:
		case BinaryOperatorKind.Pointer:
		case BinaryOperatorKind.PointerAndInt:
		case BinaryOperatorKind.PointerAndUInt:
		case BinaryOperatorKind.PointerAndLong:
		case BinaryOperatorKind.PointerAndULong:
		case BinaryOperatorKind.ULongAndPointer:
			return true;
		default:
			return false;
		}
	}

	private static bool IsConditional(BinaryOperatorKind opKind)
	{
		switch (opKind.OperatorWithLogical())
		{
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
		case BinaryOperatorKind.LogicalAnd:
		case BinaryOperatorKind.LogicalOr:
			return true;
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
			return opKind.OperandTypes() == BinaryOperatorKind.Bool;
		default:
			return false;
		}
	}

	private static bool IsFloat(BinaryOperatorKind opKind)
	{
		BinaryOperatorKind binaryOperatorKind = opKind.OperandTypes();
		if ((uint)(binaryOperatorKind - 26) <= 1u)
		{
			return true;
		}
		return false;
	}

	private void EmitStackAllocInitializers(TypeSymbol type, BoundArrayInitialization inits)
	{
		TypeSymbol typeSymbol = ((type.TypeKind == TypeKind.Pointer) ? ((PointerTypeSymbol)type).PointedAtType : ((NamedTypeSymbol)type).TypeArgumentsNoUseSiteDiagnostics[0]);
		ImmutableArray<BoundExpression> initializers = inits.Initializers;
		ArrayInitializerStyle arrayInitializerStyle = ShouldEmitBlockInitializerForStackAlloc(typeSymbol, initializers);
		if (arrayInitializerStyle == ArrayInitializerStyle.Element)
		{
			EmitElementStackAllocInitializers(typeSymbol, initializers, includeConstants: true);
			return;
		}
		ImmutableArray<byte> data = GetRawData(initializers);
		if (data.All((byte datum) => datum == data[0]))
		{
			_builder.EmitStackAllocBlockInitializer(data, inits.Syntax, emitInitBlock: true, _diagnostics);
		}
		else if (typeSymbol.SpecialType.SizeInBytes() == 1)
		{
			_builder.EmitStackAllocBlockInitializer(data, inits.Syntax, emitInitBlock: false, _diagnostics);
			if (arrayInitializerStyle == ArrayInitializerStyle.Mixed)
			{
				EmitElementStackAllocInitializers(typeSymbol, initializers, includeConstants: false);
			}
		}
		else
		{
			EmitElementStackAllocInitializers(typeSymbol, initializers, includeConstants: true);
		}
	}

	private ArrayInitializerStyle ShouldEmitBlockInitializerForStackAlloc(TypeSymbol elementType, ImmutableArray<BoundExpression> inits)
	{
		if (!_module.SupportsPrivateImplClass)
		{
			return ArrayInitializerStyle.Element;
		}
		elementType = elementType.EnumUnderlyingType();
		if (elementType.SpecialType.IsBlittable())
		{
			int initCount = 0;
			int constInits = 0;
			StackAllocInitializerCount(inits, ref initCount, ref constInits);
			if (initCount > 2)
			{
				if (initCount == constInits)
				{
					return ArrayInitializerStyle.Block;
				}
				int num = Math.Max(3, initCount / 3);
				if (constInits >= num)
				{
					return ArrayInitializerStyle.Mixed;
				}
			}
		}
		return ArrayInitializerStyle.Element;
	}

	private void StackAllocInitializerCount(ImmutableArray<BoundExpression> inits, ref int initCount, ref int constInits)
	{
		if (inits.Length == 0)
		{
			return;
		}
		foreach (BoundExpression item in inits)
		{
			initCount++;
			if (item.ConstantValue != null)
			{
				constInits++;
			}
		}
	}

	private void EmitElementStackAllocInitializers(TypeSymbol elementType, ImmutableArray<BoundExpression> inits, bool includeConstants)
	{
		int num = 0;
		int elementTypeSizeInBytes = elementType.SpecialType.SizeInBytes();
		foreach (BoundExpression item in inits)
		{
			if (includeConstants || item.ConstantValue == null)
			{
				_builder.EmitOpCode(ILOpCode.Dup);
				EmitPointerElementAccess(item, elementType, elementTypeSizeInBytes, num);
				EmitExpression(item, used: true);
				EmitIndirectStore(elementType, item.Syntax);
			}
			num++;
		}
	}

	private void EmitPointerElementAccess(BoundExpression init, TypeSymbol elementType, int elementTypeSizeInBytes, int index)
	{
		if (index != 0)
		{
			if (elementTypeSizeInBytes == 1)
			{
				_builder.EmitIntConstant(index);
				_builder.EmitOpCode(ILOpCode.Add);
				return;
			}
			if (index == 1)
			{
				EmitIntConstantOrSizeOf(init, elementType, elementTypeSizeInBytes);
				_builder.EmitOpCode(ILOpCode.Add);
				return;
			}
			_builder.EmitIntConstant(index);
			_builder.EmitOpCode(ILOpCode.Conv_i);
			EmitIntConstantOrSizeOf(init, elementType, elementTypeSizeInBytes);
			_builder.EmitOpCode(ILOpCode.Mul);
			_builder.EmitOpCode(ILOpCode.Add);
		}
	}

	private void EmitIntConstantOrSizeOf(BoundExpression init, TypeSymbol elementType, int elementTypeSizeInBytes)
	{
		if (elementTypeSizeInBytes == 0)
		{
			_builder.EmitOpCode(ILOpCode.Sizeof);
			EmitSymbolToken(elementType, init.Syntax);
		}
		else
		{
			_builder.EmitIntConstant(elementTypeSizeInBytes);
		}
	}

	private void EmitStatement(BoundStatement statement)
	{
		switch (statement.Kind)
		{
		case BoundKind.Block:
			EmitBlock((BoundBlock)statement);
			break;
		case BoundKind.Scope:
			EmitScope((BoundScope)statement);
			break;
		case BoundKind.SequencePoint:
			EmitSequencePointStatement((BoundSequencePoint)statement);
			break;
		case BoundKind.SequencePointWithSpan:
			EmitSequencePointStatement((BoundSequencePointWithSpan)statement);
			break;
		case BoundKind.ExpressionStatement:
			EmitExpression(((BoundExpressionStatement)statement).Expression, used: false);
			break;
		case BoundKind.StatementList:
			EmitStatementList((BoundStatementList)statement);
			break;
		case BoundKind.ReturnStatement:
			EmitReturnStatement((BoundReturnStatement)statement);
			break;
		case BoundKind.GotoStatement:
			EmitGotoStatement((BoundGotoStatement)statement);
			break;
		case BoundKind.LabelStatement:
			EmitLabelStatement((BoundLabelStatement)statement);
			break;
		case BoundKind.ConditionalGoto:
			EmitConditionalGoto((BoundConditionalGoto)statement);
			break;
		case BoundKind.ThrowStatement:
			EmitThrowStatement((BoundThrowStatement)statement);
			break;
		case BoundKind.TryStatement:
			EmitTryStatement((BoundTryStatement)statement);
			break;
		case BoundKind.SwitchStatement:
			EmitSwitchStatement((BoundSwitchStatement)statement);
			break;
		case BoundKind.StateMachineScope:
			EmitStateMachineScope((BoundStateMachineScope)statement);
			break;
		case BoundKind.NoOpStatement:
			EmitNoOpStatement((BoundNoOpStatement)statement);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(statement.Kind);
		}
		ReleaseExpressionTemps();
	}

	private int EmitStatementAndCountInstructions(BoundStatement statement)
	{
		int instructionsEmitted = _builder.InstructionsEmitted;
		EmitStatement(statement);
		return _builder.InstructionsEmitted - instructionsEmitted;
	}

	private void EmitStatementList(BoundStatementList list)
	{
		int i = 0;
		for (int length = list.Statements.Length; i < length; i++)
		{
			EmitStatement(list.Statements[i]);
		}
	}

	private void EmitNoOpStatement(BoundNoOpStatement statement)
	{
		switch (statement.Flavor)
		{
		case NoOpStatementFlavor.Default:
			if (_ilEmitStyle == ILEmitStyle.Debug)
			{
				_builder.EmitOpCode(ILOpCode.Nop);
			}
			break;
		case NoOpStatementFlavor.AwaitYieldPoint:
			if (_asyncYieldPoints == null)
			{
				_asyncYieldPoints = ArrayBuilder<int>.GetInstance();
				_asyncResumePoints = ArrayBuilder<int>.GetInstance();
			}
			_asyncYieldPoints.Add(_builder.AllocateILMarker());
			break;
		case NoOpStatementFlavor.AwaitResumePoint:
			_asyncResumePoints.Add(_builder.AllocateILMarker());
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(statement.Flavor);
		}
	}

	private void EmitThrowStatement(BoundThrowStatement node)
	{
		EmitThrow(node.ExpressionOpt);
	}

	private void EmitThrow(BoundExpression thrown)
	{
		if (thrown != null)
		{
			EmitExpression(thrown, used: true);
			TypeSymbol type = thrown.Type;
			if ((object)type != null && type.TypeKind == TypeKind.TypeParameter)
			{
				EmitBox(type, thrown.Syntax);
			}
		}
		_builder.EmitThrow(thrown == null);
	}

	private void EmitConditionalGoto(BoundConditionalGoto boundConditionalGoto)
	{
		object dest = boundConditionalGoto.Label;
		EmitCondBranch(boundConditionalGoto.Condition, ref dest, boundConditionalGoto.JumpIfTrue);
	}

	private static bool CanPassToBrfalse(TypeSymbol ts)
	{
		if (ts.IsEnumType())
		{
			return true;
		}
		switch (ts.PrimitiveTypeCode)
		{
		case Microsoft.Cci.PrimitiveTypeCode.Float32:
		case Microsoft.Cci.PrimitiveTypeCode.Float64:
			return false;
		case Microsoft.Cci.PrimitiveTypeCode.NotPrimitive:
			return ts.IsReferenceType;
		default:
			return true;
		}
	}

	private static BoundExpression TryReduce(BoundBinaryOperator condition, ref bool sense)
	{
		BinaryOperatorKind binaryOperatorKind = condition.OperatorKind.Operator();
		BoundExpression boundExpression = ((condition.Left.ConstantValue != null) ? condition.Left : null);
		BoundExpression boundExpression2;
		if (boundExpression != null)
		{
			boundExpression2 = condition.Right;
		}
		else
		{
			boundExpression = ((condition.Right.ConstantValue != null) ? condition.Right : null);
			if (boundExpression == null)
			{
				return null;
			}
			boundExpression2 = condition.Left;
		}
		TypeSymbol type = boundExpression2.Type;
		if (!CanPassToBrfalse(type))
		{
			return null;
		}
		bool flag = type.PrimitiveTypeCode == Microsoft.Cci.PrimitiveTypeCode.Boolean;
		bool isDefaultValue = boundExpression.ConstantValue.IsDefaultValue;
		if (!flag && !isDefaultValue)
		{
			return null;
		}
		if (isDefaultValue)
		{
			sense = !sense;
		}
		if (binaryOperatorKind == BinaryOperatorKind.NotEqual)
		{
			sense = !sense;
		}
		return boundExpression2;
	}

	private static ILOpCode CodeForJump(BoundBinaryOperator op, bool sense, out ILOpCode revOpCode)
	{
		int num;
		switch (op.OperatorKind.Operator())
		{
		case BinaryOperatorKind.Equal:
			revOpCode = ((!sense) ? ILOpCode.Beq : ILOpCode.Bne_un);
			if (!sense)
			{
				return ILOpCode.Bne_un;
			}
			return ILOpCode.Beq;
		case BinaryOperatorKind.NotEqual:
			revOpCode = ((!sense) ? ILOpCode.Bne_un : ILOpCode.Beq);
			if (!sense)
			{
				return ILOpCode.Beq;
			}
			return ILOpCode.Bne_un;
		case BinaryOperatorKind.LessThan:
			num = 0;
			break;
		case BinaryOperatorKind.LessThanOrEqual:
			num = 1;
			break;
		case BinaryOperatorKind.GreaterThan:
			num = 2;
			break;
		case BinaryOperatorKind.GreaterThanOrEqual:
			num = 3;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(op.OperatorKind.Operator());
		}
		if (IsUnsignedBinaryOperator(op))
		{
			num += 8;
		}
		else if (IsFloat(op.OperatorKind))
		{
			num += 16;
		}
		int num2 = num;
		if (!sense)
		{
			num += 4;
		}
		else
		{
			num2 += 4;
		}
		revOpCode = s_condJumpOpCodes[num2];
		return s_condJumpOpCodes[num];
	}

	private void EmitCondBranch(BoundExpression condition, ref object dest, bool sense)
	{
		_recursionDepth++;
		if (_recursionDepth > 1)
		{
			StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
			EmitCondBranchCore(condition, ref dest, sense);
		}
		else
		{
			EmitCondBranchCoreWithStackGuard(condition, ref dest, sense);
		}
		_recursionDepth--;
	}

	private void EmitCondBranchCoreWithStackGuard(BoundExpression condition, ref object dest, bool sense)
	{
		try
		{
			EmitCondBranchCore(condition, ref dest, sense);
		}
		catch (Exception ex) when (StackGuard.IsInsufficientExecutionStackException(ex))
		{
			_diagnostics.Add(ErrorCode.ERR_InsufficientStack, BoundTreeVisitor.CancelledByStackGuardException.GetTooLongOrComplexExpressionErrorLocation(condition));
			throw new EmitCancelledException();
		}
	}

	private void EmitCondBranchCore(BoundExpression condition, ref object dest, bool sense)
	{
		ILOpCode code;
		while (true)
		{
			if (condition.ConstantValue != null)
			{
				if (condition.ConstantValue.IsDefaultValue != sense)
				{
					dest = dest ?? new object();
					_builder.EmitBranch(ILOpCode.Br, dest);
				}
				return;
			}
			switch (condition.Kind)
			{
			case BoundKind.BinaryOperator:
			{
				BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)condition;
				bool flag = sense;
				BinaryOperatorKind binaryOperatorKind = boundBinaryOperator.OperatorKind.OperatorWithLogical();
				if (binaryOperatorKind <= BinaryOperatorKind.LessThan)
				{
					if (binaryOperatorKind <= BinaryOperatorKind.NotEqual)
					{
						if (binaryOperatorKind != BinaryOperatorKind.Equal && binaryOperatorKind != BinaryOperatorKind.NotEqual)
						{
							break;
						}
						BoundExpression boundExpression = TryReduce(boundBinaryOperator, ref sense);
						if (boundExpression != null)
						{
							condition = boundExpression;
							continue;
						}
					}
					else if (binaryOperatorKind != BinaryOperatorKind.GreaterThan && binaryOperatorKind != BinaryOperatorKind.LessThan)
					{
						break;
					}
				}
				else
				{
					if (binaryOperatorKind > BinaryOperatorKind.LessThanOrEqual)
					{
						if (binaryOperatorKind != BinaryOperatorKind.LogicalAnd)
						{
							if (binaryOperatorKind != BinaryOperatorKind.LogicalOr)
							{
								break;
							}
							flag = !flag;
						}
						if (flag)
						{
							object dest3 = null;
							EmitCondBranch(boundBinaryOperator.Left, ref dest3, !sense);
							EmitCondBranch(boundBinaryOperator.Right, ref dest, sense);
							if (dest3 != null)
							{
								_builder.MarkLabel(dest3);
							}
							return;
						}
						EmitCondBranch(boundBinaryOperator.Left, ref dest, sense);
						condition = boundBinaryOperator.Right;
						continue;
					}
					if (binaryOperatorKind != BinaryOperatorKind.GreaterThanOrEqual && binaryOperatorKind != BinaryOperatorKind.LessThanOrEqual)
					{
						break;
					}
				}
				EmitExpression(boundBinaryOperator.Left, used: true);
				EmitExpression(boundBinaryOperator.Right, used: true);
				code = CodeForJump(boundBinaryOperator, sense, out var revOpCode);
				dest = dest ?? new object();
				_builder.EmitBranch(code, dest, revOpCode);
				return;
			}
			case BoundKind.LoweredConditionalAccess:
			{
				BoundLoweredConditionalAccess boundLoweredConditionalAccess = (BoundLoweredConditionalAccess)condition;
				BoundExpression receiver = boundLoweredConditionalAccess.Receiver;
				if (!receiver.Type.IsReferenceType || LocalRewriter.CanChangeValueBetweenReads(receiver, localsMayBeAssignedOrCaptured: false) || (receiver.Kind == BoundKind.Local && IsStackLocal(((BoundLocal)receiver).LocalSymbol)))
				{
					break;
				}
				BoundExpression whenNullOpt = boundLoweredConditionalAccess.WhenNullOpt;
				if (whenNullOpt != null && !whenNullOpt.IsDefaultValue())
				{
					break;
				}
				if (sense)
				{
					object dest2 = null;
					EmitCondBranch(receiver, ref dest2, sense: false);
					EmitReceiverRef(receiver, AddressKind.ReadOnly);
					EmitCondBranch(boundLoweredConditionalAccess.WhenNotNull, ref dest, sense: true);
					if (dest2 != null)
					{
						_builder.MarkLabel(dest2);
					}
					return;
				}
				EmitCondBranch(receiver, ref dest, sense: false);
				EmitReceiverRef(receiver, AddressKind.ReadOnly);
				condition = boundLoweredConditionalAccess.WhenNotNull;
				continue;
			}
			case BoundKind.UnaryOperator:
			{
				BoundUnaryOperator boundUnaryOperator = (BoundUnaryOperator)condition;
				if (boundUnaryOperator.OperatorKind == UnaryOperatorKind.BoolLogicalNegation)
				{
					sense = !sense;
					condition = boundUnaryOperator.Operand;
					continue;
				}
				break;
			}
			case BoundKind.IsOperator:
			{
				BoundIsOperator boundIsOperator = (BoundIsOperator)condition;
				BoundExpression operand = boundIsOperator.Operand;
				EmitExpression(operand, used: true);
				if (!operand.Type.IsVerifierReference())
				{
					EmitBox(operand.Type, operand.Syntax);
				}
				_builder.EmitOpCode(ILOpCode.Isinst);
				EmitSymbolToken(boundIsOperator.TargetType.Type, boundIsOperator.TargetType.Syntax);
				code = (sense ? ILOpCode.Brtrue : ILOpCode.Brfalse);
				dest = dest ?? new object();
				_builder.EmitBranch(code, dest);
				return;
			}
			case BoundKind.Sequence:
			{
				BoundSequence sequence = (BoundSequence)condition;
				EmitSequenceCondBranch(sequence, ref dest, sense);
				return;
			}
			}
			break;
		}
		EmitExpression(condition, used: true);
		TypeSymbol type = condition.Type;
		if (type.IsReferenceType && !type.IsVerifierReference())
		{
			EmitBox(type, condition.Syntax);
		}
		code = (sense ? ILOpCode.Brtrue : ILOpCode.Brfalse);
		dest = dest ?? new object();
		_builder.EmitBranch(code, dest);
	}

	private void EmitSequenceCondBranch(BoundSequence sequence, ref object dest, bool sense)
	{
		DefineLocals(sequence);
		EmitSideEffects(sequence);
		EmitCondBranch(sequence.Value, ref dest, sense);
		FreeLocals(sequence);
	}

	private void EmitLabelStatement(BoundLabelStatement boundLabelStatement)
	{
		_builder.MarkLabel(boundLabelStatement.Label);
	}

	private void EmitGotoStatement(BoundGotoStatement boundGotoStatement)
	{
		_builder.EmitBranch(ILOpCode.Br, boundGotoStatement.Label);
	}

	private bool IsLastBlockInMethod(BoundBlock block)
	{
		if (_boundBody == block)
		{
			return true;
		}
		if (_boundBody is BoundStatementList boundStatementList && boundStatementList.Statements.LastOrDefault() == block)
		{
			return true;
		}
		return false;
	}

	private void EmitBlock(BoundBlock block)
	{
		bool flag = !block.Locals.IsEmpty;
		if (flag)
		{
			_builder.OpenLocalScope();
			foreach (LocalSymbol local in block.Locals)
			{
				ImmutableArray<SyntaxReference> declaringSyntaxReferences = local.DeclaringSyntaxReferences;
				DefineLocal(local, (!declaringSyntaxReferences.IsEmpty) ? ((CSharpSyntaxNode)declaringSyntaxReferences[0].GetSyntax()) : block.Syntax);
			}
		}
		EmitStatements(block.Statements);
		if (_indirectReturnState == IndirectReturnState.Needed && IsLastBlockInMethod(block))
		{
			HandleReturn();
		}
		if (flag)
		{
			foreach (LocalSymbol local2 in block.Locals)
			{
				FreeLocal(local2);
			}
			_builder.CloseLocalScope();
		}
	}

	private void EmitStatements(ImmutableArray<BoundStatement> statements)
	{
		foreach (BoundStatement item in statements)
		{
			EmitStatement(item);
		}
	}

	private void EmitScope(BoundScope block)
	{
		_builder.OpenLocalScope();
		foreach (LocalSymbol local in block.Locals)
		{
			if (!local.IsConst && !IsStackLocal(local))
			{
				_builder.AddLocalToScope(_builder.LocalSlotManager.GetLocal(local));
			}
		}
		EmitStatements(block.Statements);
		_builder.CloseLocalScope();
	}

	private void EmitStateMachineScope(BoundStateMachineScope scope)
	{
		_builder.OpenLocalScope(ScopeType.StateMachineVariable);
		foreach (StateMachineFieldSymbol field in scope.Fields)
		{
			_builder.DefineUserDefinedStateMachineHoistedLocal(field.SlotIndex);
		}
		EmitStatement(scope.Statement);
		_builder.CloseLocalScope();
	}

	private bool ShouldUseIndirectReturn()
	{
		if (_ilEmitStyle == ILEmitStyle.Debug && _method.GenerateDebugInfo)
		{
			SyntaxNode methodBodySyntaxOpt = _methodBodySyntaxOpt;
			if (methodBodySyntaxOpt != null && methodBodySyntaxOpt.IsKind(SyntaxKind.Block))
			{
				return true;
			}
		}
		return _builder.InExceptionHandler;
	}

	private bool CanHandleReturnLabel(BoundReturnStatement boundReturnStatement)
	{
		if (boundReturnStatement.WasCompilerGenerated)
		{
			if (!boundReturnStatement.Syntax.IsKind(SyntaxKind.Block))
			{
				MethodSymbol method = _method;
				if ((object)method == null || !method.IsImplicitConstructor)
				{
					goto IL_003d;
				}
			}
			return !_builder.InExceptionHandler;
		}
		goto IL_003d;
		IL_003d:
		return false;
	}

	private void EmitReturnStatement(BoundReturnStatement boundReturnStatement)
	{
		BoundExpression expressionOpt = boundReturnStatement.ExpressionOpt;
		if (boundReturnStatement.RefKind == RefKind.None)
		{
			EmitExpression(expressionOpt, used: true);
		}
		else
		{
			EmitAddress(expressionOpt, (_method.RefKind == RefKind.In) ? AddressKind.ReadOnlyStrict : AddressKind.Writeable);
		}
		if (ShouldUseIndirectReturn())
		{
			if (expressionOpt != null)
			{
				_builder.EmitLocalStore(LazyReturnTemp);
			}
			if (_indirectReturnState != IndirectReturnState.Emitted && CanHandleReturnLabel(boundReturnStatement))
			{
				HandleReturn();
				return;
			}
			_builder.EmitBranch(ILOpCode.Br, s_returnLabel);
			if (_indirectReturnState == IndirectReturnState.NotNeeded)
			{
				_indirectReturnState = IndirectReturnState.Needed;
			}
		}
		else if (_indirectReturnState == IndirectReturnState.Needed && CanHandleReturnLabel(boundReturnStatement))
		{
			if (expressionOpt != null)
			{
				_builder.EmitLocalStore(LazyReturnTemp);
			}
			HandleReturn();
		}
		else
		{
			if (expressionOpt != null)
			{
				_module.Translate(expressionOpt.Type, boundReturnStatement.Syntax, _diagnostics);
			}
			_builder.EmitRet(expressionOpt == null);
		}
	}

	private void EmitTryStatement(BoundTryStatement statement, bool emitCatchesOnly = false)
	{
		bool num = !emitCatchesOnly && statement.CatchBlocks.Length > 0 && statement.FinallyBlockOpt != null;
		_builder.OpenLocalScope(ScopeType.TryCatchFinally);
		_builder.OpenLocalScope(ScopeType.Try);
		_tryNestingLevel++;
		if (num)
		{
			EmitTryStatement(statement, emitCatchesOnly: true);
		}
		else
		{
			EmitBlock(statement.TryBlock);
		}
		_tryNestingLevel--;
		_builder.CloseLocalScope();
		if (!num)
		{
			foreach (BoundCatchBlock catchBlock in statement.CatchBlocks)
			{
				EmitCatchBlock(catchBlock);
			}
		}
		if (!emitCatchesOnly && statement.FinallyBlockOpt != null)
		{
			_builder.OpenLocalScope(statement.PreferFaultHandler ? ScopeType.Fault : ScopeType.Finally);
			EmitBlock(statement.FinallyBlockOpt);
			_builder.CloseLocalScope();
			_builder.CloseLocalScope();
			if (statement.PreferFaultHandler)
			{
				BoundBlock block = FinallyCloner.MakeFinallyClone(statement);
				EmitBlock(block);
			}
		}
		else
		{
			_builder.CloseLocalScope();
		}
	}

	private void EmitCatchBlock(BoundCatchBlock catchBlock)
	{
		object label = null;
		_builder.AdjustStack(1);
		if (catchBlock.ExceptionFilterOpt == null)
		{
			ITypeReference typeReference;
			if ((object)catchBlock.ExceptionTypeOpt == null)
			{
				ITypeReference specialType = _module.GetSpecialType(SpecialType.System_Object, catchBlock.Syntax, _diagnostics);
				typeReference = specialType;
			}
			else
			{
				typeReference = _module.Translate(catchBlock.ExceptionTypeOpt, catchBlock.Syntax, _diagnostics);
			}
			ITypeReference exceptionType = typeReference;
			_builder.OpenLocalScope(ScopeType.Catch, exceptionType);
			if (catchBlock.IsSynthesizedAsyncCatchAll)
			{
				_asyncCatchHandlerOffset = _builder.AllocateILMarker();
			}
			if (_emitPdbSequencePoints && catchBlock.Syntax is CatchClauseSyntax catchClauseSyntax)
			{
				EmitSequencePoint(span: (catchClauseSyntax.Declaration != null) ? TextSpan.FromBounds(catchClauseSyntax.SpanStart, catchClauseSyntax.Declaration.Span.End) : catchClauseSyntax.CatchKeyword.Span, syntaxTree: catchBlock.SyntaxTree);
			}
		}
		else
		{
			_builder.OpenLocalScope(ScopeType.Filter);
			object label2 = new object();
			label = new object();
			if ((object)catchBlock.ExceptionTypeOpt != null)
			{
				ITypeReference value = _module.Translate(catchBlock.ExceptionTypeOpt, catchBlock.Syntax, _diagnostics);
				_builder.EmitOpCode(ILOpCode.Isinst);
				_builder.EmitToken(value, catchBlock.Syntax, _diagnostics);
				_builder.EmitOpCode(ILOpCode.Dup);
				_builder.EmitBranch(ILOpCode.Brtrue, label2);
				_builder.EmitOpCode(ILOpCode.Pop);
				_builder.EmitIntConstant(0);
				_builder.EmitBranch(ILOpCode.Br, label);
			}
			_builder.MarkLabel(label2);
		}
		foreach (LocalSymbol local in catchBlock.Locals)
		{
			ImmutableArray<SyntaxReference> declaringSyntaxReferences = local.DeclaringSyntaxReferences;
			SyntaxNode syntaxNode = ((!declaringSyntaxReferences.IsEmpty) ? ((CSharpSyntaxNode)declaringSyntaxReferences[0].GetSyntax()) : catchBlock.Syntax);
			DefineLocal(local, syntaxNode);
		}
		BoundExpression exceptionSourceOpt = catchBlock.ExceptionSourceOpt;
		if (exceptionSourceOpt != null)
		{
			if (!exceptionSourceOpt.Type.IsVerifierReference())
			{
				_builder.EmitOpCode(ILOpCode.Unbox_any);
				EmitSymbolToken(exceptionSourceOpt.Type, exceptionSourceOpt.Syntax);
			}
			BoundExpression boundExpression = exceptionSourceOpt;
			while (boundExpression.Kind == BoundKind.Sequence)
			{
				BoundSequence boundSequence = (BoundSequence)boundExpression;
				EmitSideEffects(boundSequence);
				boundExpression = boundSequence.Value;
			}
			switch (boundExpression.Kind)
			{
			case BoundKind.Local:
			{
				BoundLocal boundLocal = (BoundLocal)boundExpression;
				if (!IsStackLocal(boundLocal.LocalSymbol))
				{
					_builder.EmitLocalStore(GetLocal(boundLocal));
				}
				break;
			}
			case BoundKind.FieldAccess:
			{
				BoundFieldAccess boundFieldAccess = (BoundFieldAccess)boundExpression;
				if (boundFieldAccess.FieldSymbol is StateMachineFieldSymbol { SlotIndex: >=0 } stateMachineFieldSymbol)
				{
					_builder.DefineUserDefinedStateMachineHoistedLocal(stateMachineFieldSymbol.SlotIndex);
				}
				LocalDefinition localDefinition = AllocateTemp(boundExpression.Type, boundExpression.Syntax);
				_builder.EmitLocalStore(localDefinition);
				EmitReceiverRef(boundFieldAccess.ReceiverOpt, AddressKind.Writeable);
				_builder.EmitLocalLoad(localDefinition);
				FreeTemp(localDefinition);
				EmitFieldStore(boundFieldAccess);
				break;
			}
			default:
				throw ExceptionUtilities.UnexpectedValue(boundExpression.Kind);
			}
		}
		else
		{
			_builder.EmitOpCode(ILOpCode.Pop);
		}
		if (catchBlock.ExceptionFilterOpt != null)
		{
			EmitCondExpr(catchBlock.ExceptionFilterOpt, sense: true);
			_builder.EmitIntConstant(0);
			_builder.EmitOpCode(ILOpCode.Cgt_un);
			_builder.MarkLabel(label);
			_builder.MarkFilterConditionEnd();
			_builder.EmitOpCode(ILOpCode.Pop);
		}
		EmitBlock(catchBlock.Body);
		_builder.CloseLocalScope();
	}

	private void EmitSwitchStatement(BoundSwitchStatement switchStatement)
	{
		BoundStatement loweredPreambleOpt = switchStatement.LoweredPreambleOpt;
		if (loweredPreambleOpt != null)
		{
			EmitStatement(loweredPreambleOpt);
		}
		BoundExpression expression = switchStatement.Expression;
		ImmutableArray<BoundSwitchSection> switchSections = switchStatement.SwitchSections;
		GeneratedLabelSymbol breakLabel = switchStatement.BreakLabel;
		LabelSymbol constantTargetOpt = switchStatement.ConstantTargetOpt;
		if ((object)constantTargetOpt != null)
		{
			EmitConstantSwitchHeader(expression, constantTargetOpt);
		}
		else
		{
			LabelSymbol fallThroughLabel = breakLabel;
			KeyValuePair<ConstantValue, object>[] switchCaseLabels = GetSwitchCaseLabels(switchSections, ref fallThroughLabel);
			if (switchCaseLabels.Length == 0)
			{
				EmitExpression(expression, used: false);
				_builder.EmitBranch(ILOpCode.Br, fallThroughLabel);
			}
			else
			{
				EmitSwitchHeader(switchStatement, expression, switchCaseLabels, fallThroughLabel);
			}
		}
		EmitSwitchBody(switchStatement.InnerLocals, switchSections, breakLabel, switchStatement.Syntax);
	}

	private KeyValuePair<ConstantValue, object>[] GetSwitchCaseLabels(ImmutableArray<BoundSwitchSection> sections, ref LabelSymbol fallThroughLabel)
	{
		ArrayBuilder<KeyValuePair<ConstantValue, object>> instance = ArrayBuilder<KeyValuePair<ConstantValue, object>>.GetInstance();
		foreach (BoundSwitchSection item in sections)
		{
			object obj = null;
			foreach (BoundSwitchLabel switchLabel in item.SwitchLabels)
			{
				LabelSymbol label = switchLabel.Label;
				if (switchLabel.ConstantValueOpt == null)
				{
					fallThroughLabel = label;
					continue;
				}
				ConstantValue constantValueOpt = switchLabel.ConstantValueOpt;
				if (obj == null)
				{
					obj = label;
				}
				instance.Add(new KeyValuePair<ConstantValue, object>(constantValueOpt, obj));
			}
		}
		return instance.ToArrayAndFree();
	}

	private void EmitConstantSwitchHeader(BoundExpression expression, LabelSymbol target)
	{
		EmitExpression(expression, used: false);
		_builder.EmitBranch(ILOpCode.Br, target);
	}

	private void EmitSwitchHeader(BoundSwitchStatement switchStatement, BoundExpression expression, KeyValuePair<ConstantValue, object>[] switchCaseLabels, LabelSymbol fallThroughLabel)
	{
		LocalDefinition localDefinition = null;
		BoundSequence boundSequence = null;
		if (expression.Kind == BoundKind.Sequence)
		{
			boundSequence = (BoundSequence)expression;
			DefineLocals(boundSequence);
			EmitSideEffects(boundSequence);
			expression = boundSequence.Value;
		}
		if (expression.Kind == BoundKind.SequencePointExpression)
		{
			BoundSequencePointExpression boundSequencePointExpression = (BoundSequencePointExpression)expression;
			EmitSequencePoint(boundSequencePointExpression);
			expression = boundSequencePointExpression.Expression;
		}
		BoundKind kind = expression.Kind;
		LocalOrParameter key;
		if (kind != BoundKind.Local)
		{
			if (kind == BoundKind.Parameter)
			{
				BoundParameter boundParameter = (BoundParameter)expression;
				if (boundParameter.ParameterSymbol.RefKind == RefKind.None)
				{
					key = ParameterSlot(boundParameter);
					goto IL_00e5;
				}
			}
		}
		else
		{
			LocalSymbol localSymbol = ((BoundLocal)expression).LocalSymbol;
			if (localSymbol.RefKind == RefKind.None && !IsStackLocal(localSymbol))
			{
				key = GetLocal(localSymbol);
				goto IL_00e5;
			}
		}
		EmitExpression(expression, used: true);
		localDefinition = AllocateTemp(expression.Type, expression.Syntax);
		_builder.EmitLocalStore(localDefinition);
		key = localDefinition;
		goto IL_00e5;
		IL_00e5:
		if (expression.Type.SpecialType != SpecialType.System_String)
		{
			_builder.EmitIntegerSwitchJumpTable(switchCaseLabels, fallThroughLabel, key, expression.Type.EnumUnderlyingType().PrimitiveTypeCode);
		}
		else
		{
			EmitStringSwitchJumpTable(switchStatement, switchCaseLabels, fallThroughLabel, key, expression.Syntax);
		}
		if (localDefinition != null)
		{
			FreeTemp(localDefinition);
		}
		if (boundSequence != null)
		{
			FreeLocals(boundSequence);
		}
	}

	private void EmitStringSwitchJumpTable(BoundSwitchStatement switchStatement, KeyValuePair<ConstantValue, object>[] switchCaseLabels, LabelSymbol fallThroughLabel, LocalOrParameter key, SyntaxNode syntaxNode)
	{
		LocalDefinition localDefinition = null;
		if (SwitchStringJumpTableEmitter.ShouldGenerateHashTableSwitch(_module, switchCaseLabels.Length))
		{
			IReference method = _module.GetPrivateImplClass(syntaxNode, _diagnostics).GetMethod("ComputeStringHash");
			if (method != null)
			{
				_builder.EmitLoad(key);
				_builder.EmitOpCode(ILOpCode.Call, 0);
				_builder.EmitToken(method, syntaxNode, _diagnostics);
				NamedTypeSymbol specialType = _module.Compilation.GetSpecialType(SpecialType.System_UInt32);
				localDefinition = AllocateTemp(specialType, syntaxNode);
				_builder.EmitLocalStore(localDefinition);
			}
		}
		IReference stringEqualityMethodRef = _module.Translate(switchStatement.StringEquality, syntaxNode, _diagnostics);
		IMethodReference stringLengthRef = null;
		MethodSymbol methodSymbol = _module.Compilation.GetSpecialTypeMember(SpecialMember.System_String__Length) as MethodSymbol;
		if (methodSymbol != null && !methodSymbol.HasUseSiteError)
		{
			stringLengthRef = _module.Translate(methodSymbol, syntaxNode, _diagnostics);
		}
		SwitchStringJumpTableEmitter.EmitStringCompareAndBranch emitStringCondBranchDelegate = (LocalOrParameter keyArg, ConstantValue stringConstant, object targetLabel) =>
		{
			if (stringConstant == ConstantValue.Null)
			{
				_builder.EmitLoad(keyArg);
				_builder.EmitBranch(ILOpCode.Brfalse, targetLabel, ILOpCode.Brtrue);
			}
			else if (stringConstant.StringValue.Length == 0 && stringLengthRef != null)
			{
				object label = new object();
				_builder.EmitLoad(keyArg);
				_builder.EmitBranch(ILOpCode.Brfalse, label, ILOpCode.Brtrue);
				_builder.EmitLoad(keyArg);
				_builder.EmitOpCode(ILOpCode.Call, 0);
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				_builder.EmitToken(stringLengthRef, null, instance);
				instance.Free();
				_builder.EmitBranch(ILOpCode.Brfalse, targetLabel, ILOpCode.Brtrue);
				_builder.MarkLabel(label);
			}
			else
			{
				EmitStringCompareAndBranch(key, syntaxNode, stringConstant, targetLabel, stringEqualityMethodRef);
			}
		};
		_builder.EmitStringSwitchJumpTable(switchCaseLabels, fallThroughLabel, key, localDefinition, emitStringCondBranchDelegate, SynthesizedStringSwitchHashMethod.ComputeStringHash);
		if (localDefinition != null)
		{
			FreeTemp(localDefinition);
		}
	}

	private void EmitStringCompareAndBranch(LocalOrParameter key, SyntaxNode syntaxNode, ConstantValue stringConstant, object targetLabel, IReference stringEqualityMethodRef)
	{
		_builder.EmitLoad(key);
		_builder.EmitConstantValue(stringConstant);
		_builder.EmitOpCode(ILOpCode.Call, -1);
		_builder.EmitToken(stringEqualityMethodRef, syntaxNode, _diagnostics);
		_builder.EmitBranch(ILOpCode.Brtrue, targetLabel, ILOpCode.Brfalse);
	}

	private void EmitSwitchBody(ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundSwitchSection> switchSections, GeneratedLabelSymbol breakLabel, SyntaxNode syntaxNode)
	{
		bool flag = !locals.IsEmpty;
		if (flag)
		{
			_builder.OpenLocalScope();
			foreach (LocalSymbol item in locals)
			{
				DefineLocal(item, syntaxNode);
			}
		}
		foreach (BoundSwitchSection item2 in switchSections)
		{
			EmitSwitchSection(item2);
		}
		_builder.MarkLabel(breakLabel);
		if (flag)
		{
			_builder.CloseLocalScope();
		}
	}

	private void EmitSwitchSection(BoundSwitchSection switchSection)
	{
		foreach (BoundSwitchLabel switchLabel in switchSection.SwitchLabels)
		{
			_builder.MarkLabel(switchLabel.Label);
		}
		foreach (BoundStatement statement in switchSection.Statements)
		{
			EmitStatement(statement);
		}
	}

	private LocalDefinition GetLocal(BoundLocal localExpression)
	{
		LocalSymbol localSymbol = localExpression.LocalSymbol;
		return GetLocal(localSymbol);
	}

	private LocalDefinition GetLocal(LocalSymbol symbol)
	{
		return _builder.LocalSlotManager.GetLocal(symbol);
	}

	private LocalDefinition DefineLocal(LocalSymbol local, SyntaxNode syntaxNode)
	{
		ImmutableArray<bool> dynamicTransformFlags = ((!local.IsCompilerGenerated && local.Type.ContainsDynamic()) ? CSharpCompilation.DynamicTransformsEncoder.Encode(local.Type, RefKind.None, 0) : ImmutableArray<bool>.Empty);
		ImmutableArray<string> tupleElementNames = ((!local.IsCompilerGenerated && local.Type.ContainsTupleNames()) ? CSharpCompilation.TupleNamesEncoder.Encode(local.Type) : ImmutableArray<string>.Empty);
		if (local.IsConst)
		{
			MetadataConstant compileTimeValue = _module.CreateConstant(local.Type, local.ConstantValue, syntaxNode, _diagnostics);
			LocalConstantDefinition localConstant = new LocalConstantDefinition(local.Name, local.Locations.FirstOrDefault() ?? Location.None, compileTimeValue, dynamicTransformFlags, tupleElementNames);
			_builder.AddLocalConstantToScope(localConstant);
			return null;
		}
		if (IsStackLocal(local))
		{
			return null;
		}
		LocalSlotConstraints constraints;
		ITypeReference typeReference2;
		if (local.DeclarationKind == LocalDeclarationKind.FixedVariable && local.IsPinned)
		{
			constraints = LocalSlotConstraints.ByRef | LocalSlotConstraints.Pinned;
			TypeSymbol pointedAtType = ((PointerTypeSymbol)local.Type).PointedAtType;
			ITypeReference typeReference;
			if (pointedAtType.SpecialType != SpecialType.System_Void)
			{
				typeReference = _module.Translate(pointedAtType, syntaxNode, _diagnostics);
			}
			else
			{
				ITypeReference specialType = _module.GetSpecialType(SpecialType.System_IntPtr, syntaxNode, _diagnostics);
				typeReference = specialType;
			}
			typeReference2 = typeReference;
		}
		else
		{
			constraints = (LocalSlotConstraints)((uint)(local.IsPinned ? 2 : 0) | ((local.RefKind != RefKind.None) ? 1u : 0u));
			typeReference2 = _module.Translate(local.Type, syntaxNode, _diagnostics);
		}
		_module.GetFakeSymbolTokenForIL(typeReference2, syntaxNode, _diagnostics);
		string localDebugName = GetLocalDebugName(local, out var localId);
		LocalDefinition localDefinition = _builder.LocalSlotManager.DeclareLocal(typeReference2, local, localDebugName, local.SynthesizedKind, localId, local.SynthesizedKind.PdbAttributes(), constraints, dynamicTransformFlags, tupleElementNames, local.SynthesizedKind.IsSlotReusable(_ilEmitStyle != ILEmitStyle.Release));
		if (localDefinition.Name != null)
		{
			if (local.SynthesizedKind == SynthesizedLocalKind.UserDefined)
			{
				SyntaxNode scopeDesignatorOpt = local.ScopeDesignatorOpt;
				if (scopeDesignatorOpt != null && scopeDesignatorOpt.Kind() == SyntaxKind.SwitchSection)
				{
					goto IL_01e7;
				}
			}
			_builder.AddLocalToScope(localDefinition);
		}
		goto IL_01e7;
		IL_01e7:
		return localDefinition;
	}

	private string GetLocalDebugName(ILocalSymbolInternal local, out LocalDebugId localId)
	{
		localId = LocalDebugId.None;
		if (local.IsImportedFromMetadata)
		{
			return local.Name;
		}
		SynthesizedLocalKind synthesizedKind = local.SynthesizedKind;
		if (!synthesizedKind.IsLongLived() || synthesizedKind == SynthesizedLocalKind.InstrumentationPayload)
		{
			return null;
		}
		if (_ilEmitStyle == ILEmitStyle.Debug)
		{
			SyntaxNode declaratorSyntax = local.GetDeclaratorSyntax();
			int syntaxOffset = _method.CalculateLocalSyntaxOffset(declaratorSyntax.SpanStart, declaratorSyntax.SyntaxTree);
			int ordinal = ((synthesizedKind != SynthesizedLocalKind.SwitchCasePatternMatching) ? _synthesizedLocalOrdinals.AssignLocalOrdinal(synthesizedKind, syntaxOffset) : 0);
			localId = new LocalDebugId(syntaxOffset, ordinal);
		}
		return local.Name ?? GeneratedNames.MakeSynthesizedLocalName(synthesizedKind, ref _uniqueNameId);
	}

	private bool IsSlotReusable(LocalSymbol local)
	{
		return local.SynthesizedKind.IsSlotReusable(_ilEmitStyle != ILEmitStyle.Release);
	}

	private void FreeLocal(LocalSymbol local)
	{
		if (local.Name == null && IsSlotReusable(local) && !IsStackLocal(local))
		{
			_builder.LocalSlotManager.FreeLocal(local);
		}
	}

	private LocalDefinition AllocateTemp(TypeSymbol type, SyntaxNode syntaxNode, LocalSlotConstraints slotConstraints = LocalSlotConstraints.None)
	{
		return _builder.LocalSlotManager.AllocateSlot(_module.Translate(type, syntaxNode, _diagnostics), slotConstraints);
	}

	private void FreeTemp(LocalDefinition temp)
	{
		_builder.LocalSlotManager.FreeSlot(temp);
	}

	private void FreeOptTemp(LocalDefinition temp)
	{
		if (temp != null)
		{
			FreeTemp(temp);
		}
	}
}
