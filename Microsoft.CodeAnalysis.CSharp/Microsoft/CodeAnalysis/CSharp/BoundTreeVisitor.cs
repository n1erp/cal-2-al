using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTreeVisitor<A, R>
{
	public virtual R Visit(BoundNode node, A arg)
	{
		if (node == null)
		{
			return default;
		}
		return node.Kind switch
		{
			BoundKind.TypeExpression => VisitTypeExpression(node as BoundTypeExpression, arg), 
			BoundKind.NamespaceExpression => VisitNamespaceExpression(node as BoundNamespaceExpression, arg), 
			BoundKind.UnaryOperator => VisitUnaryOperator(node as BoundUnaryOperator, arg), 
			BoundKind.IncrementOperator => VisitIncrementOperator(node as BoundIncrementOperator, arg), 
			BoundKind.BinaryOperator => VisitBinaryOperator(node as BoundBinaryOperator, arg), 
			BoundKind.CompoundAssignmentOperator => VisitCompoundAssignmentOperator(node as BoundCompoundAssignmentOperator, arg), 
			BoundKind.AssignmentOperator => VisitAssignmentOperator(node as BoundAssignmentOperator, arg), 
			BoundKind.NullCoalescingOperator => VisitNullCoalescingOperator(node as BoundNullCoalescingOperator, arg), 
			BoundKind.ConditionalOperator => VisitConditionalOperator(node as BoundConditionalOperator, arg), 
			BoundKind.ArrayAccess => VisitArrayAccess(node as BoundArrayAccess, arg), 
			BoundKind.TypeOfOperator => VisitTypeOfOperator(node as BoundTypeOfOperator, arg), 
			BoundKind.DefaultExpression => VisitDefaultExpression(node as BoundDefaultExpression, arg), 
			BoundKind.IsOperator => VisitIsOperator(node as BoundIsOperator, arg), 
			BoundKind.AsOperator => VisitAsOperator(node as BoundAsOperator, arg), 
			BoundKind.Conversion => VisitConversion(node as BoundConversion, arg), 
			BoundKind.SequencePointExpression => VisitSequencePointExpression(node as BoundSequencePointExpression, arg), 
			BoundKind.SequencePoint => VisitSequencePoint(node as BoundSequencePoint, arg), 
			BoundKind.SequencePointWithSpan => VisitSequencePointWithSpan(node as BoundSequencePointWithSpan, arg), 
			BoundKind.Block => VisitBlock(node as BoundBlock, arg), 
			BoundKind.LocalDeclaration => VisitLocalDeclaration(node as BoundLocalDeclaration, arg), 
			BoundKind.MultipleLocalDeclarations => VisitMultipleLocalDeclarations(node as BoundMultipleLocalDeclarations, arg), 
			BoundKind.Sequence => VisitSequence(node as BoundSequence, arg), 
			BoundKind.NoOpStatement => VisitNoOpStatement(node as BoundNoOpStatement, arg), 
			BoundKind.ReturnStatement => VisitReturnStatement(node as BoundReturnStatement, arg), 
			BoundKind.ThrowStatement => VisitThrowStatement(node as BoundThrowStatement, arg), 
			BoundKind.ExpressionStatement => VisitExpressionStatement(node as BoundExpressionStatement, arg), 
			BoundKind.SwitchStatement => VisitSwitchStatement(node as BoundSwitchStatement, arg), 
			BoundKind.BreakStatement => VisitBreakStatement(node as BoundBreakStatement, arg), 
			BoundKind.ContinueStatement => VisitContinueStatement(node as BoundContinueStatement, arg), 
			BoundKind.IfStatement => VisitIfStatement(node as BoundIfStatement, arg), 
			BoundKind.ForEachStatement => VisitForEachStatement(node as BoundForEachStatement, arg), 
			BoundKind.TryStatement => VisitTryStatement(node as BoundTryStatement, arg), 
			BoundKind.Literal => VisitLiteral(node as BoundLiteral, arg), 
			BoundKind.ThisReference => VisitThisReference(node as BoundThisReference, arg), 
			BoundKind.Local => VisitLocal(node as BoundLocal, arg), 
			BoundKind.Parameter => VisitParameter(node as BoundParameter, arg), 
			BoundKind.LabelStatement => VisitLabelStatement(node as BoundLabelStatement, arg), 
			BoundKind.GotoStatement => VisitGotoStatement(node as BoundGotoStatement, arg), 
			BoundKind.LabeledStatement => VisitLabeledStatement(node as BoundLabeledStatement, arg), 
			BoundKind.StatementList => VisitStatementList(node as BoundStatementList, arg), 
			BoundKind.ConditionalGoto => VisitConditionalGoto(node as BoundConditionalGoto, arg), 
			BoundKind.Call => VisitCall(node as BoundCall, arg), 
			BoundKind.ObjectCreationExpression => VisitObjectCreationExpression(node as BoundObjectCreationExpression, arg), 
			BoundKind.DelegateCreationExpression => VisitDelegateCreationExpression(node as BoundDelegateCreationExpression, arg), 
			BoundKind.FieldAccess => VisitFieldAccess(node as BoundFieldAccess, arg), 
			BoundKind.PropertyAccess => VisitPropertyAccess(node as BoundPropertyAccess, arg), 
			BoundKind.Lambda => VisitLambda(node as BoundLambda, arg), 
			BoundKind.NameOfOperator => VisitNameOfOperator(node as BoundNameOfOperator, arg), 
			_ => VisitInternal(node, arg), 
		};
	}

	public virtual R DefaultVisit(BoundNode node, A arg)
	{
		return default;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	internal R VisitInternal(BoundNode node, A arg)
	{
		return node.Kind switch
		{
			BoundKind.FieldEqualsValue => VisitFieldEqualsValue(node as BoundFieldEqualsValue, arg), 
			BoundKind.PropertyEqualsValue => VisitPropertyEqualsValue(node as BoundPropertyEqualsValue, arg), 
			BoundKind.ParameterEqualsValue => VisitParameterEqualsValue(node as BoundParameterEqualsValue, arg), 
			BoundKind.GlobalStatementInitializer => VisitGlobalStatementInitializer(node as BoundGlobalStatementInitializer, arg), 
			BoundKind.DeconstructValuePlaceholder => VisitDeconstructValuePlaceholder(node as BoundDeconstructValuePlaceholder, arg), 
			BoundKind.TupleOperandPlaceholder => VisitTupleOperandPlaceholder(node as BoundTupleOperandPlaceholder, arg), 
			BoundKind.Dup => VisitDup(node as BoundDup, arg), 
			BoundKind.PassByCopy => VisitPassByCopy(node as BoundPassByCopy, arg), 
			BoundKind.BadExpression => VisitBadExpression(node as BoundBadExpression, arg), 
			BoundKind.BadStatement => VisitBadStatement(node as BoundBadStatement, arg), 
			BoundKind.TypeExpression => VisitTypeExpression(node as BoundTypeExpression, arg), 
			BoundKind.TypeOrValueExpression => VisitTypeOrValueExpression(node as BoundTypeOrValueExpression, arg), 
			BoundKind.NamespaceExpression => VisitNamespaceExpression(node as BoundNamespaceExpression, arg), 
			BoundKind.UnaryOperator => VisitUnaryOperator(node as BoundUnaryOperator, arg), 
			BoundKind.IncrementOperator => VisitIncrementOperator(node as BoundIncrementOperator, arg), 
			BoundKind.AddressOfOperator => VisitAddressOfOperator(node as BoundAddressOfOperator, arg), 
			BoundKind.PointerIndirectionOperator => VisitPointerIndirectionOperator(node as BoundPointerIndirectionOperator, arg), 
			BoundKind.PointerElementAccess => VisitPointerElementAccess(node as BoundPointerElementAccess, arg), 
			BoundKind.RefTypeOperator => VisitRefTypeOperator(node as BoundRefTypeOperator, arg), 
			BoundKind.MakeRefOperator => VisitMakeRefOperator(node as BoundMakeRefOperator, arg), 
			BoundKind.RefValueOperator => VisitRefValueOperator(node as BoundRefValueOperator, arg), 
			BoundKind.BinaryOperator => VisitBinaryOperator(node as BoundBinaryOperator, arg), 
			BoundKind.TupleBinaryOperator => VisitTupleBinaryOperator(node as BoundTupleBinaryOperator, arg), 
			BoundKind.UserDefinedConditionalLogicalOperator => VisitUserDefinedConditionalLogicalOperator(node as BoundUserDefinedConditionalLogicalOperator, arg), 
			BoundKind.CompoundAssignmentOperator => VisitCompoundAssignmentOperator(node as BoundCompoundAssignmentOperator, arg), 
			BoundKind.AssignmentOperator => VisitAssignmentOperator(node as BoundAssignmentOperator, arg), 
			BoundKind.DeconstructionAssignmentOperator => VisitDeconstructionAssignmentOperator(node as BoundDeconstructionAssignmentOperator, arg), 
			BoundKind.NullCoalescingOperator => VisitNullCoalescingOperator(node as BoundNullCoalescingOperator, arg), 
			BoundKind.ConditionalOperator => VisitConditionalOperator(node as BoundConditionalOperator, arg), 
			BoundKind.ArrayAccess => VisitArrayAccess(node as BoundArrayAccess, arg), 
			BoundKind.ArrayLength => VisitArrayLength(node as BoundArrayLength, arg), 
			BoundKind.AwaitExpression => VisitAwaitExpression(node as BoundAwaitExpression, arg), 
			BoundKind.TypeOfOperator => VisitTypeOfOperator(node as BoundTypeOfOperator, arg), 
			BoundKind.MethodDefIndex => VisitMethodDefIndex(node as BoundMethodDefIndex, arg), 
			BoundKind.MaximumMethodDefIndex => VisitMaximumMethodDefIndex(node as BoundMaximumMethodDefIndex, arg), 
			BoundKind.InstrumentationPayloadRoot => VisitInstrumentationPayloadRoot(node as BoundInstrumentationPayloadRoot, arg), 
			BoundKind.ModuleVersionId => VisitModuleVersionId(node as BoundModuleVersionId, arg), 
			BoundKind.ModuleVersionIdString => VisitModuleVersionIdString(node as BoundModuleVersionIdString, arg), 
			BoundKind.SourceDocumentIndex => VisitSourceDocumentIndex(node as BoundSourceDocumentIndex, arg), 
			BoundKind.MethodInfo => VisitMethodInfo(node as BoundMethodInfo, arg), 
			BoundKind.FieldInfo => VisitFieldInfo(node as BoundFieldInfo, arg), 
			BoundKind.DefaultExpression => VisitDefaultExpression(node as BoundDefaultExpression, arg), 
			BoundKind.IsOperator => VisitIsOperator(node as BoundIsOperator, arg), 
			BoundKind.AsOperator => VisitAsOperator(node as BoundAsOperator, arg), 
			BoundKind.SizeOfOperator => VisitSizeOfOperator(node as BoundSizeOfOperator, arg), 
			BoundKind.Conversion => VisitConversion(node as BoundConversion, arg), 
			BoundKind.ArgList => VisitArgList(node as BoundArgList, arg), 
			BoundKind.ArgListOperator => VisitArgListOperator(node as BoundArgListOperator, arg), 
			BoundKind.FixedLocalCollectionInitializer => VisitFixedLocalCollectionInitializer(node as BoundFixedLocalCollectionInitializer, arg), 
			BoundKind.SequencePoint => VisitSequencePoint(node as BoundSequencePoint, arg), 
			BoundKind.SequencePointExpression => VisitSequencePointExpression(node as BoundSequencePointExpression, arg), 
			BoundKind.SequencePointWithSpan => VisitSequencePointWithSpan(node as BoundSequencePointWithSpan, arg), 
			BoundKind.Block => VisitBlock(node as BoundBlock, arg), 
			BoundKind.Scope => VisitScope(node as BoundScope, arg), 
			BoundKind.StateMachineScope => VisitStateMachineScope(node as BoundStateMachineScope, arg), 
			BoundKind.LocalDeclaration => VisitLocalDeclaration(node as BoundLocalDeclaration, arg), 
			BoundKind.MultipleLocalDeclarations => VisitMultipleLocalDeclarations(node as BoundMultipleLocalDeclarations, arg), 
			BoundKind.LocalFunctionStatement => VisitLocalFunctionStatement(node as BoundLocalFunctionStatement, arg), 
			BoundKind.Sequence => VisitSequence(node as BoundSequence, arg), 
			BoundKind.NoOpStatement => VisitNoOpStatement(node as BoundNoOpStatement, arg), 
			BoundKind.ReturnStatement => VisitReturnStatement(node as BoundReturnStatement, arg), 
			BoundKind.YieldReturnStatement => VisitYieldReturnStatement(node as BoundYieldReturnStatement, arg), 
			BoundKind.YieldBreakStatement => VisitYieldBreakStatement(node as BoundYieldBreakStatement, arg), 
			BoundKind.ThrowStatement => VisitThrowStatement(node as BoundThrowStatement, arg), 
			BoundKind.ExpressionStatement => VisitExpressionStatement(node as BoundExpressionStatement, arg), 
			BoundKind.SwitchStatement => VisitSwitchStatement(node as BoundSwitchStatement, arg), 
			BoundKind.SwitchSection => VisitSwitchSection(node as BoundSwitchSection, arg), 
			BoundKind.SwitchLabel => VisitSwitchLabel(node as BoundSwitchLabel, arg), 
			BoundKind.BreakStatement => VisitBreakStatement(node as BoundBreakStatement, arg), 
			BoundKind.ContinueStatement => VisitContinueStatement(node as BoundContinueStatement, arg), 
			BoundKind.PatternSwitchStatement => VisitPatternSwitchStatement(node as BoundPatternSwitchStatement, arg), 
			BoundKind.PatternSwitchSection => VisitPatternSwitchSection(node as BoundPatternSwitchSection, arg), 
			BoundKind.PatternSwitchLabel => VisitPatternSwitchLabel(node as BoundPatternSwitchLabel, arg), 
			BoundKind.IfStatement => VisitIfStatement(node as BoundIfStatement, arg), 
			BoundKind.DoStatement => VisitDoStatement(node as BoundDoStatement, arg), 
			BoundKind.WhileStatement => VisitWhileStatement(node as BoundWhileStatement, arg), 
			BoundKind.ForStatement => VisitForStatement(node as BoundForStatement, arg), 
			BoundKind.ForEachStatement => VisitForEachStatement(node as BoundForEachStatement, arg), 
			BoundKind.ForEachDeconstructStep => VisitForEachDeconstructStep(node as BoundForEachDeconstructStep, arg), 
			BoundKind.UsingStatement => VisitUsingStatement(node as BoundUsingStatement, arg), 
			BoundKind.FixedStatement => VisitFixedStatement(node as BoundFixedStatement, arg), 
			BoundKind.LockStatement => VisitLockStatement(node as BoundLockStatement, arg), 
			BoundKind.TryStatement => VisitTryStatement(node as BoundTryStatement, arg), 
			BoundKind.CatchBlock => VisitCatchBlock(node as BoundCatchBlock, arg), 
			BoundKind.Literal => VisitLiteral(node as BoundLiteral, arg), 
			BoundKind.ThisReference => VisitThisReference(node as BoundThisReference, arg), 
			BoundKind.PreviousSubmissionReference => VisitPreviousSubmissionReference(node as BoundPreviousSubmissionReference, arg), 
			BoundKind.HostObjectMemberReference => VisitHostObjectMemberReference(node as BoundHostObjectMemberReference, arg), 
			BoundKind.BaseReference => VisitBaseReference(node as BoundBaseReference, arg), 
			BoundKind.Local => VisitLocal(node as BoundLocal, arg), 
			BoundKind.PseudoVariable => VisitPseudoVariable(node as BoundPseudoVariable, arg), 
			BoundKind.RangeVariable => VisitRangeVariable(node as BoundRangeVariable, arg), 
			BoundKind.Parameter => VisitParameter(node as BoundParameter, arg), 
			BoundKind.LabelStatement => VisitLabelStatement(node as BoundLabelStatement, arg), 
			BoundKind.GotoStatement => VisitGotoStatement(node as BoundGotoStatement, arg), 
			BoundKind.LabeledStatement => VisitLabeledStatement(node as BoundLabeledStatement, arg), 
			BoundKind.Label => VisitLabel(node as BoundLabel, arg), 
			BoundKind.StatementList => VisitStatementList(node as BoundStatementList, arg), 
			BoundKind.ConditionalGoto => VisitConditionalGoto(node as BoundConditionalGoto, arg), 
			BoundKind.DynamicMemberAccess => VisitDynamicMemberAccess(node as BoundDynamicMemberAccess, arg), 
			BoundKind.DynamicInvocation => VisitDynamicInvocation(node as BoundDynamicInvocation, arg), 
			BoundKind.ConditionalAccess => VisitConditionalAccess(node as BoundConditionalAccess, arg), 
			BoundKind.LoweredConditionalAccess => VisitLoweredConditionalAccess(node as BoundLoweredConditionalAccess, arg), 
			BoundKind.ConditionalReceiver => VisitConditionalReceiver(node as BoundConditionalReceiver, arg), 
			BoundKind.ComplexConditionalReceiver => VisitComplexConditionalReceiver(node as BoundComplexConditionalReceiver, arg), 
			BoundKind.MethodGroup => VisitMethodGroup(node as BoundMethodGroup, arg), 
			BoundKind.PropertyGroup => VisitPropertyGroup(node as BoundPropertyGroup, arg), 
			BoundKind.Call => VisitCall(node as BoundCall, arg), 
			BoundKind.EventAssignmentOperator => VisitEventAssignmentOperator(node as BoundEventAssignmentOperator, arg), 
			BoundKind.Attribute => VisitAttribute(node as BoundAttribute, arg), 
			BoundKind.ObjectCreationExpression => VisitObjectCreationExpression(node as BoundObjectCreationExpression, arg), 
			BoundKind.TupleLiteral => VisitTupleLiteral(node as BoundTupleLiteral, arg), 
			BoundKind.ConvertedTupleLiteral => VisitConvertedTupleLiteral(node as BoundConvertedTupleLiteral, arg), 
			BoundKind.DynamicObjectCreationExpression => VisitDynamicObjectCreationExpression(node as BoundDynamicObjectCreationExpression, arg), 
			BoundKind.NoPiaObjectCreationExpression => VisitNoPiaObjectCreationExpression(node as BoundNoPiaObjectCreationExpression, arg), 
			BoundKind.ObjectInitializerExpression => VisitObjectInitializerExpression(node as BoundObjectInitializerExpression, arg), 
			BoundKind.ObjectInitializerMember => VisitObjectInitializerMember(node as BoundObjectInitializerMember, arg), 
			BoundKind.DynamicObjectInitializerMember => VisitDynamicObjectInitializerMember(node as BoundDynamicObjectInitializerMember, arg), 
			BoundKind.CollectionInitializerExpression => VisitCollectionInitializerExpression(node as BoundCollectionInitializerExpression, arg), 
			BoundKind.CollectionElementInitializer => VisitCollectionElementInitializer(node as BoundCollectionElementInitializer, arg), 
			BoundKind.DynamicCollectionElementInitializer => VisitDynamicCollectionElementInitializer(node as BoundDynamicCollectionElementInitializer, arg), 
			BoundKind.ImplicitReceiver => VisitImplicitReceiver(node as BoundImplicitReceiver, arg), 
			BoundKind.AnonymousObjectCreationExpression => VisitAnonymousObjectCreationExpression(node as BoundAnonymousObjectCreationExpression, arg), 
			BoundKind.AnonymousPropertyDeclaration => VisitAnonymousPropertyDeclaration(node as BoundAnonymousPropertyDeclaration, arg), 
			BoundKind.NewT => VisitNewT(node as BoundNewT, arg), 
			BoundKind.DelegateCreationExpression => VisitDelegateCreationExpression(node as BoundDelegateCreationExpression, arg), 
			BoundKind.ArrayCreation => VisitArrayCreation(node as BoundArrayCreation, arg), 
			BoundKind.ArrayInitialization => VisitArrayInitialization(node as BoundArrayInitialization, arg), 
			BoundKind.StackAllocArrayCreation => VisitStackAllocArrayCreation(node as BoundStackAllocArrayCreation, arg), 
			BoundKind.ConvertedStackAllocExpression => VisitConvertedStackAllocExpression(node as BoundConvertedStackAllocExpression, arg), 
			BoundKind.FieldAccess => VisitFieldAccess(node as BoundFieldAccess, arg), 
			BoundKind.HoistedFieldAccess => VisitHoistedFieldAccess(node as BoundHoistedFieldAccess, arg), 
			BoundKind.PropertyAccess => VisitPropertyAccess(node as BoundPropertyAccess, arg), 
			BoundKind.EventAccess => VisitEventAccess(node as BoundEventAccess, arg), 
			BoundKind.IndexerAccess => VisitIndexerAccess(node as BoundIndexerAccess, arg), 
			BoundKind.DynamicIndexerAccess => VisitDynamicIndexerAccess(node as BoundDynamicIndexerAccess, arg), 
			BoundKind.Lambda => VisitLambda(node as BoundLambda, arg), 
			BoundKind.UnboundLambda => VisitUnboundLambda(node as UnboundLambda, arg), 
			BoundKind.QueryClause => VisitQueryClause(node as BoundQueryClause, arg), 
			BoundKind.TypeOrInstanceInitializers => VisitTypeOrInstanceInitializers(node as BoundTypeOrInstanceInitializers, arg), 
			BoundKind.NameOfOperator => VisitNameOfOperator(node as BoundNameOfOperator, arg), 
			BoundKind.InterpolatedString => VisitInterpolatedString(node as BoundInterpolatedString, arg), 
			BoundKind.StringInsert => VisitStringInsert(node as BoundStringInsert, arg), 
			BoundKind.IsPatternExpression => VisitIsPatternExpression(node as BoundIsPatternExpression, arg), 
			BoundKind.DeclarationPattern => VisitDeclarationPattern(node as BoundDeclarationPattern, arg), 
			BoundKind.ConstantPattern => VisitConstantPattern(node as BoundConstantPattern, arg), 
			BoundKind.WildcardPattern => VisitWildcardPattern(node as BoundWildcardPattern, arg), 
			BoundKind.DiscardExpression => VisitDiscardExpression(node as BoundDiscardExpression, arg), 
			BoundKind.ThrowExpression => VisitThrowExpression(node as BoundThrowExpression, arg), 
			BoundKind.OutVariablePendingInference => VisitOutVariablePendingInference(node as OutVariablePendingInference, arg), 
			BoundKind.DeconstructionVariablePendingInference => VisitDeconstructionVariablePendingInference(node as DeconstructionVariablePendingInference, arg), 
			BoundKind.OutDeconstructVarPendingInference => VisitOutDeconstructVarPendingInference(node as OutDeconstructVarPendingInference, arg), 
			BoundKind.NonConstructorMethodBody => VisitNonConstructorMethodBody(node as BoundNonConstructorMethodBody, arg), 
			BoundKind.ConstructorMethodBody => VisitConstructorMethodBody(node as BoundConstructorMethodBody, arg), 
			_ => default, 
		};
	}

	public virtual R VisitFieldEqualsValue(BoundFieldEqualsValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPropertyEqualsValue(BoundPropertyEqualsValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitParameterEqualsValue(BoundParameterEqualsValue node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitGlobalStatementInitializer(BoundGlobalStatementInitializer node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTupleOperandPlaceholder(BoundTupleOperandPlaceholder node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDup(BoundDup node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPassByCopy(BoundPassByCopy node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBadExpression(BoundBadExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBadStatement(BoundBadStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTypeExpression(BoundTypeExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTypeOrValueExpression(BoundTypeOrValueExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitNamespaceExpression(BoundNamespaceExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitUnaryOperator(BoundUnaryOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitIncrementOperator(BoundIncrementOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAddressOfOperator(BoundAddressOfOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPointerElementAccess(BoundPointerElementAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitRefTypeOperator(BoundRefTypeOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMakeRefOperator(BoundMakeRefOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitRefValueOperator(BoundRefValueOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBinaryOperator(BoundBinaryOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTupleBinaryOperator(BoundTupleBinaryOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAssignmentOperator(BoundAssignmentOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitNullCoalescingOperator(BoundNullCoalescingOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConditionalOperator(BoundConditionalOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitArrayAccess(BoundArrayAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitArrayLength(BoundArrayLength node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAwaitExpression(BoundAwaitExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTypeOfOperator(BoundTypeOfOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMethodDefIndex(BoundMethodDefIndex node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMaximumMethodDefIndex(BoundMaximumMethodDefIndex node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitInstrumentationPayloadRoot(BoundInstrumentationPayloadRoot node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitModuleVersionId(BoundModuleVersionId node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitModuleVersionIdString(BoundModuleVersionIdString node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSourceDocumentIndex(BoundSourceDocumentIndex node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMethodInfo(BoundMethodInfo node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitFieldInfo(BoundFieldInfo node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDefaultExpression(BoundDefaultExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitIsOperator(BoundIsOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAsOperator(BoundAsOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSizeOfOperator(BoundSizeOfOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConversion(BoundConversion node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitArgList(BoundArgList node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitArgListOperator(BoundArgListOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSequencePoint(BoundSequencePoint node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSequencePointExpression(BoundSequencePointExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSequencePointWithSpan(BoundSequencePointWithSpan node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBlock(BoundBlock node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitScope(BoundScope node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitStateMachineScope(BoundStateMachineScope node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLocalDeclaration(BoundLocalDeclaration node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMultipleLocalDeclarations(BoundMultipleLocalDeclarations node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLocalFunctionStatement(BoundLocalFunctionStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSequence(BoundSequence node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitNoOpStatement(BoundNoOpStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitReturnStatement(BoundReturnStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitYieldReturnStatement(BoundYieldReturnStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitYieldBreakStatement(BoundYieldBreakStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitThrowStatement(BoundThrowStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitExpressionStatement(BoundExpressionStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSwitchStatement(BoundSwitchStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSwitchSection(BoundSwitchSection node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitSwitchLabel(BoundSwitchLabel node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBreakStatement(BoundBreakStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitContinueStatement(BoundContinueStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPatternSwitchStatement(BoundPatternSwitchStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPatternSwitchSection(BoundPatternSwitchSection node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPatternSwitchLabel(BoundPatternSwitchLabel node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitIfStatement(BoundIfStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDoStatement(BoundDoStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitWhileStatement(BoundWhileStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitForStatement(BoundForStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitForEachStatement(BoundForEachStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitForEachDeconstructStep(BoundForEachDeconstructStep node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitUsingStatement(BoundUsingStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitFixedStatement(BoundFixedStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLockStatement(BoundLockStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTryStatement(BoundTryStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCatchBlock(BoundCatchBlock node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLiteral(BoundLiteral node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitThisReference(BoundThisReference node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitHostObjectMemberReference(BoundHostObjectMemberReference node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitBaseReference(BoundBaseReference node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLocal(BoundLocal node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPseudoVariable(BoundPseudoVariable node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitRangeVariable(BoundRangeVariable node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitParameter(BoundParameter node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLabelStatement(BoundLabelStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitGotoStatement(BoundGotoStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLabeledStatement(BoundLabeledStatement node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLabel(BoundLabel node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitStatementList(BoundStatementList node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConditionalGoto(BoundConditionalGoto node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDynamicMemberAccess(BoundDynamicMemberAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDynamicInvocation(BoundDynamicInvocation node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConditionalAccess(BoundConditionalAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConditionalReceiver(BoundConditionalReceiver node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitComplexConditionalReceiver(BoundComplexConditionalReceiver node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitMethodGroup(BoundMethodGroup node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPropertyGroup(BoundPropertyGroup node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCall(BoundCall node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitEventAssignmentOperator(BoundEventAssignmentOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAttribute(BoundAttribute node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitObjectCreationExpression(BoundObjectCreationExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTupleLiteral(BoundTupleLiteral node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitObjectInitializerExpression(BoundObjectInitializerExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitObjectInitializerMember(BoundObjectInitializerMember node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCollectionInitializerExpression(BoundCollectionInitializerExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitCollectionElementInitializer(BoundCollectionElementInitializer node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDynamicCollectionElementInitializer(BoundDynamicCollectionElementInitializer node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitImplicitReceiver(BoundImplicitReceiver node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitAnonymousPropertyDeclaration(BoundAnonymousPropertyDeclaration node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitNewT(BoundNewT node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDelegateCreationExpression(BoundDelegateCreationExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitArrayCreation(BoundArrayCreation node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitArrayInitialization(BoundArrayInitialization node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitStackAllocArrayCreation(BoundStackAllocArrayCreation node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitFieldAccess(BoundFieldAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitHoistedFieldAccess(BoundHoistedFieldAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitPropertyAccess(BoundPropertyAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitEventAccess(BoundEventAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitIndexerAccess(BoundIndexerAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitLambda(BoundLambda node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitUnboundLambda(UnboundLambda node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitQueryClause(BoundQueryClause node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitTypeOrInstanceInitializers(BoundTypeOrInstanceInitializers node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitNameOfOperator(BoundNameOfOperator node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitInterpolatedString(BoundInterpolatedString node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitStringInsert(BoundStringInsert node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitIsPatternExpression(BoundIsPatternExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDeclarationPattern(BoundDeclarationPattern node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConstantPattern(BoundConstantPattern node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitWildcardPattern(BoundWildcardPattern node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDiscardExpression(BoundDiscardExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitThrowExpression(BoundThrowExpression node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitOutVariablePendingInference(OutVariablePendingInference node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitDeconstructionVariablePendingInference(DeconstructionVariablePendingInference node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitOutDeconstructVarPendingInference(OutDeconstructVarPendingInference node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitNonConstructorMethodBody(BoundNonConstructorMethodBody node, A arg)
	{
		return DefaultVisit(node, arg);
	}

	public virtual R VisitConstructorMethodBody(BoundConstructorMethodBody node, A arg)
	{
		return DefaultVisit(node, arg);
	}
}
internal abstract class BoundTreeVisitor
{
	public class CancelledByStackGuardException : Exception
	{
		public readonly BoundNode Node;

		public CancelledByStackGuardException(Exception inner, BoundNode node)
			: base(inner.Message, inner)
		{
			Node = node;
		}

		public void AddAnError(DiagnosticBag diagnostics)
		{
			diagnostics.Add(ErrorCode.ERR_InsufficientStack, GetTooLongOrComplexExpressionErrorLocation(Node));
		}

		public static Location GetTooLongOrComplexExpressionErrorLocation(BoundNode node)
		{
			SyntaxNode syntaxNode = node.Syntax;
			if (!(syntaxNode is ExpressionSyntax))
			{
				syntaxNode = syntaxNode.DescendantNodes((SyntaxNode n) => !(n is ExpressionSyntax)).OfType<ExpressionSyntax>().FirstOrDefault() ?? syntaxNode;
			}
			return syntaxNode.GetFirstToken().GetLocation();
		}
	}

	[DebuggerHidden]
	public virtual BoundNode Visit(BoundNode node)
	{
		return node?.Accept(this);
	}

	[DebuggerHidden]
	public virtual BoundNode DefaultVisit(BoundNode node)
	{
		return null;
	}

	protected BoundExpression VisitExpressionWithStackGuard(ref int recursionDepth, BoundExpression node)
	{
		recursionDepth++;
		BoundExpression result;
		if (recursionDepth > 1 || !ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException())
		{
			StackGuard.EnsureSufficientExecutionStack(recursionDepth);
			result = VisitExpressionWithoutStackGuard(node);
		}
		else
		{
			result = VisitExpressionWithStackGuard(node);
		}
		recursionDepth--;
		return result;
	}

	protected virtual bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
	{
		return true;
	}

	private BoundExpression VisitExpressionWithStackGuard(BoundExpression node)
	{
		try
		{
			return VisitExpressionWithoutStackGuard(node);
		}
		catch (Exception ex) when (StackGuard.IsInsufficientExecutionStackException(ex))
		{
			throw new CancelledByStackGuardException(ex, node);
		}
	}

	protected abstract BoundExpression VisitExpressionWithoutStackGuard(BoundExpression node);

	public virtual BoundNode VisitFieldEqualsValue(BoundFieldEqualsValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPropertyEqualsValue(BoundPropertyEqualsValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitParameterEqualsValue(BoundParameterEqualsValue node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitGlobalStatementInitializer(BoundGlobalStatementInitializer node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTupleOperandPlaceholder(BoundTupleOperandPlaceholder node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDup(BoundDup node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPassByCopy(BoundPassByCopy node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBadExpression(BoundBadExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBadStatement(BoundBadStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTypeExpression(BoundTypeExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTypeOrValueExpression(BoundTypeOrValueExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitNamespaceExpression(BoundNamespaceExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitIncrementOperator(BoundIncrementOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitRefTypeOperator(BoundRefTypeOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitRefValueOperator(BoundRefValueOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitArrayLength(BoundArrayLength node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTypeOfOperator(BoundTypeOfOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMethodDefIndex(BoundMethodDefIndex node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMaximumMethodDefIndex(BoundMaximumMethodDefIndex node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitInstrumentationPayloadRoot(BoundInstrumentationPayloadRoot node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitModuleVersionId(BoundModuleVersionId node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitModuleVersionIdString(BoundModuleVersionIdString node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSourceDocumentIndex(BoundSourceDocumentIndex node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMethodInfo(BoundMethodInfo node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitFieldInfo(BoundFieldInfo node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDefaultExpression(BoundDefaultExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitIsOperator(BoundIsOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAsOperator(BoundAsOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSizeOfOperator(BoundSizeOfOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConversion(BoundConversion node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitArgList(BoundArgList node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitArgListOperator(BoundArgListOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSequencePoint(BoundSequencePoint node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSequencePointExpression(BoundSequencePointExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSequencePointWithSpan(BoundSequencePointWithSpan node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBlock(BoundBlock node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitScope(BoundScope node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitStateMachineScope(BoundStateMachineScope node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMultipleLocalDeclarations(BoundMultipleLocalDeclarations node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSequence(BoundSequence node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSwitchSection(BoundSwitchSection node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitSwitchLabel(BoundSwitchLabel node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitContinueStatement(BoundContinueStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPatternSwitchStatement(BoundPatternSwitchStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPatternSwitchSection(BoundPatternSwitchSection node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPatternSwitchLabel(BoundPatternSwitchLabel node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitIfStatement(BoundIfStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDoStatement(BoundDoStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitForStatement(BoundForStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitForEachDeconstructStep(BoundForEachDeconstructStep node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitFixedStatement(BoundFixedStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLockStatement(BoundLockStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTryStatement(BoundTryStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLiteral(BoundLiteral node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitThisReference(BoundThisReference node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitHostObjectMemberReference(BoundHostObjectMemberReference node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitBaseReference(BoundBaseReference node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLocal(BoundLocal node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPseudoVariable(BoundPseudoVariable node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitRangeVariable(BoundRangeVariable node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitParameter(BoundParameter node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLabel(BoundLabel node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitStatementList(BoundStatementList node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDynamicMemberAccess(BoundDynamicMemberAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDynamicInvocation(BoundDynamicInvocation node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConditionalAccess(BoundConditionalAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConditionalReceiver(BoundConditionalReceiver node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitComplexConditionalReceiver(BoundComplexConditionalReceiver node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPropertyGroup(BoundPropertyGroup node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCall(BoundCall node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitEventAssignmentOperator(BoundEventAssignmentOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAttribute(BoundAttribute node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTupleLiteral(BoundTupleLiteral node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitObjectInitializerExpression(BoundObjectInitializerExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitObjectInitializerMember(BoundObjectInitializerMember node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCollectionInitializerExpression(BoundCollectionInitializerExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitCollectionElementInitializer(BoundCollectionElementInitializer node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDynamicCollectionElementInitializer(BoundDynamicCollectionElementInitializer node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitImplicitReceiver(BoundImplicitReceiver node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitAnonymousPropertyDeclaration(BoundAnonymousPropertyDeclaration node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitNewT(BoundNewT node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitArrayCreation(BoundArrayCreation node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitArrayInitialization(BoundArrayInitialization node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitStackAllocArrayCreation(BoundStackAllocArrayCreation node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitHoistedFieldAccess(BoundHoistedFieldAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitEventAccess(BoundEventAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitIndexerAccess(BoundIndexerAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitLambda(BoundLambda node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitUnboundLambda(UnboundLambda node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitQueryClause(BoundQueryClause node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitTypeOrInstanceInitializers(BoundTypeOrInstanceInitializers node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitNameOfOperator(BoundNameOfOperator node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitInterpolatedString(BoundInterpolatedString node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitStringInsert(BoundStringInsert node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDeclarationPattern(BoundDeclarationPattern node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConstantPattern(BoundConstantPattern node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitWildcardPattern(BoundWildcardPattern node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDiscardExpression(BoundDiscardExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitThrowExpression(BoundThrowExpression node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitOutVariablePendingInference(OutVariablePendingInference node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitDeconstructionVariablePendingInference(DeconstructionVariablePendingInference node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitOutDeconstructVarPendingInference(OutDeconstructVarPendingInference node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitNonConstructorMethodBody(BoundNonConstructorMethodBody node)
	{
		return DefaultVisit(node);
	}

	public virtual BoundNode VisitConstructorMethodBody(BoundConstructorMethodBody node)
	{
		return DefaultVisit(node);
	}
}
