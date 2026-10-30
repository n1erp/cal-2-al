using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTreeWalker : BoundTreeVisitor
{
	public void VisitList<T>(ImmutableArray<T> list) where T : BoundNode
	{
		if (!list.IsDefault)
		{
			for (int i = 0; i < list.Length; i++)
			{
				Visit(list[i]);
			}
		}
	}

	protected void VisitUnoptimizedForm(BoundQueryClause queryClause)
	{
		BoundExpression boundExpression = queryClause.UnoptimizedForm;
		if (boundExpression is BoundQueryClause boundQueryClause)
		{
			boundExpression = boundQueryClause.Value;
		}
		if (boundExpression is BoundCall { Method: not null, Arguments: var arguments } boundCall)
		{
			if (boundCall.Method.Name == "Select")
			{
				Visit(arguments[arguments.Length - 1]);
			}
			else if (boundCall.Method.Name == "GroupBy")
			{
				Visit(arguments[arguments.Length - 2]);
			}
		}
	}

	public override BoundNode VisitFieldEqualsValue(BoundFieldEqualsValue node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitPropertyEqualsValue(BoundPropertyEqualsValue node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitParameterEqualsValue(BoundParameterEqualsValue node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitGlobalStatementInitializer(BoundGlobalStatementInitializer node)
	{
		Visit(node.Statement);
		return null;
	}

	public override BoundNode VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node)
	{
		return null;
	}

	public override BoundNode VisitTupleOperandPlaceholder(BoundTupleOperandPlaceholder node)
	{
		return null;
	}

	public override BoundNode VisitDup(BoundDup node)
	{
		return null;
	}

	public override BoundNode VisitPassByCopy(BoundPassByCopy node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		VisitList(node.ChildBoundNodes);
		return null;
	}

	public override BoundNode VisitBadStatement(BoundBadStatement node)
	{
		VisitList(node.ChildBoundNodes);
		return null;
	}

	public override BoundNode VisitTypeExpression(BoundTypeExpression node)
	{
		Visit(node.BoundContainingTypeOpt);
		return null;
	}

	public override BoundNode VisitTypeOrValueExpression(BoundTypeOrValueExpression node)
	{
		return null;
	}

	public override BoundNode VisitNamespaceExpression(BoundNamespaceExpression node)
	{
		return null;
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitIncrementOperator(BoundIncrementOperator node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		Visit(node.Expression);
		Visit(node.Index);
		return null;
	}

	public override BoundNode VisitRefTypeOperator(BoundRefTypeOperator node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitRefValueOperator(BoundRefValueOperator node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		Visit(node.Left);
		Visit(node.Right);
		return null;
	}

	public override BoundNode VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		Visit(node.LeftOperand);
		Visit(node.RightOperand);
		return null;
	}

	public override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		Visit(node.Condition);
		Visit(node.Consequence);
		Visit(node.Alternative);
		return null;
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		Visit(node.Expression);
		VisitList(node.Indices);
		return null;
	}

	public override BoundNode VisitArrayLength(BoundArrayLength node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitTypeOfOperator(BoundTypeOfOperator node)
	{
		Visit(node.SourceType);
		return null;
	}

	public override BoundNode VisitMethodDefIndex(BoundMethodDefIndex node)
	{
		return null;
	}

	public override BoundNode VisitMaximumMethodDefIndex(BoundMaximumMethodDefIndex node)
	{
		return null;
	}

	public override BoundNode VisitInstrumentationPayloadRoot(BoundInstrumentationPayloadRoot node)
	{
		return null;
	}

	public override BoundNode VisitModuleVersionId(BoundModuleVersionId node)
	{
		return null;
	}

	public override BoundNode VisitModuleVersionIdString(BoundModuleVersionIdString node)
	{
		return null;
	}

	public override BoundNode VisitSourceDocumentIndex(BoundSourceDocumentIndex node)
	{
		return null;
	}

	public override BoundNode VisitMethodInfo(BoundMethodInfo node)
	{
		return null;
	}

	public override BoundNode VisitFieldInfo(BoundFieldInfo node)
	{
		return null;
	}

	public override BoundNode VisitDefaultExpression(BoundDefaultExpression node)
	{
		return null;
	}

	public override BoundNode VisitIsOperator(BoundIsOperator node)
	{
		Visit(node.Operand);
		Visit(node.TargetType);
		return null;
	}

	public override BoundNode VisitAsOperator(BoundAsOperator node)
	{
		Visit(node.Operand);
		Visit(node.TargetType);
		return null;
	}

	public override BoundNode VisitSizeOfOperator(BoundSizeOfOperator node)
	{
		Visit(node.SourceType);
		return null;
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		Visit(node.Operand);
		return null;
	}

	public override BoundNode VisitArgList(BoundArgList node)
	{
		return null;
	}

	public override BoundNode VisitArgListOperator(BoundArgListOperator node)
	{
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitSequencePoint(BoundSequencePoint node)
	{
		Visit(node.StatementOpt);
		return null;
	}

	public override BoundNode VisitSequencePointExpression(BoundSequencePointExpression node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitSequencePointWithSpan(BoundSequencePointWithSpan node)
	{
		Visit(node.StatementOpt);
		return null;
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		VisitList(node.Statements);
		return null;
	}

	public override BoundNode VisitScope(BoundScope node)
	{
		VisitList(node.Statements);
		return null;
	}

	public override BoundNode VisitStateMachineScope(BoundStateMachineScope node)
	{
		Visit(node.Statement);
		return null;
	}

	public override BoundNode VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		Visit(node.DeclaredType);
		Visit(node.InitializerOpt);
		VisitList(node.ArgumentsOpt);
		return null;
	}

	public override BoundNode VisitMultipleLocalDeclarations(BoundMultipleLocalDeclarations node)
	{
		VisitList(node.LocalDeclarations);
		return null;
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		Visit(node.BlockBody);
		Visit(node.ExpressionBody);
		return null;
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		VisitList(node.SideEffects);
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		return null;
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		Visit(node.ExpressionOpt);
		return null;
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		return null;
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		Visit(node.ExpressionOpt);
		return null;
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		Visit(node.LoweredPreambleOpt);
		Visit(node.Expression);
		VisitList(node.SwitchSections);
		return null;
	}

	public override BoundNode VisitSwitchSection(BoundSwitchSection node)
	{
		VisitList(node.SwitchLabels);
		VisitList(node.Statements);
		return null;
	}

	public override BoundNode VisitSwitchLabel(BoundSwitchLabel node)
	{
		Visit(node.ExpressionOpt);
		return null;
	}

	public override BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		return null;
	}

	public override BoundNode VisitContinueStatement(BoundContinueStatement node)
	{
		return null;
	}

	public override BoundNode VisitPatternSwitchStatement(BoundPatternSwitchStatement node)
	{
		Visit(node.Expression);
		VisitList(node.SwitchSections);
		Visit(node.DefaultLabel);
		return null;
	}

	public override BoundNode VisitPatternSwitchSection(BoundPatternSwitchSection node)
	{
		VisitList(node.SwitchLabels);
		VisitList(node.Statements);
		return null;
	}

	public override BoundNode VisitPatternSwitchLabel(BoundPatternSwitchLabel node)
	{
		Visit(node.Pattern);
		Visit(node.Guard);
		return null;
	}

	public override BoundNode VisitIfStatement(BoundIfStatement node)
	{
		Visit(node.Condition);
		Visit(node.Consequence);
		Visit(node.AlternativeOpt);
		return null;
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		Visit(node.Condition);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		Visit(node.Condition);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		Visit(node.Initializer);
		Visit(node.Condition);
		Visit(node.Increment);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		Visit(node.IterationVariableType);
		Visit(node.IterationErrorExpressionOpt);
		Visit(node.Expression);
		Visit(node.DeconstructionOpt);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitForEachDeconstructStep(BoundForEachDeconstructStep node)
	{
		Visit(node.DeconstructionAssignment);
		Visit(node.TargetPlaceholder);
		return null;
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		Visit(node.DeclarationsOpt);
		Visit(node.ExpressionOpt);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitFixedStatement(BoundFixedStatement node)
	{
		Visit(node.Declarations);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitLockStatement(BoundLockStatement node)
	{
		Visit(node.Argument);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		Visit(node.TryBlock);
		VisitList(node.CatchBlocks);
		Visit(node.FinallyBlockOpt);
		return null;
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		Visit(node.ExceptionSourceOpt);
		Visit(node.ExceptionFilterOpt);
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		return null;
	}

	public override BoundNode VisitThisReference(BoundThisReference node)
	{
		return null;
	}

	public override BoundNode VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node)
	{
		return null;
	}

	public override BoundNode VisitHostObjectMemberReference(BoundHostObjectMemberReference node)
	{
		return null;
	}

	public override BoundNode VisitBaseReference(BoundBaseReference node)
	{
		return null;
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		return null;
	}

	public override BoundNode VisitPseudoVariable(BoundPseudoVariable node)
	{
		return null;
	}

	public override BoundNode VisitRangeVariable(BoundRangeVariable node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitParameter(BoundParameter node)
	{
		return null;
	}

	public override BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		return null;
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		Visit(node.CaseExpressionOpt);
		Visit(node.LabelExpressionOpt);
		return null;
	}

	public override BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitLabel(BoundLabel node)
	{
		return null;
	}

	public override BoundNode VisitStatementList(BoundStatementList node)
	{
		VisitList(node.Statements);
		return null;
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		Visit(node.Condition);
		return null;
	}

	public override BoundNode VisitDynamicMemberAccess(BoundDynamicMemberAccess node)
	{
		Visit(node.Receiver);
		return null;
	}

	public override BoundNode VisitDynamicInvocation(BoundDynamicInvocation node)
	{
		Visit(node.Expression);
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitConditionalAccess(BoundConditionalAccess node)
	{
		Visit(node.Receiver);
		Visit(node.AccessExpression);
		return null;
	}

	public override BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		Visit(node.Receiver);
		Visit(node.WhenNotNull);
		Visit(node.WhenNullOpt);
		return null;
	}

	public override BoundNode VisitConditionalReceiver(BoundConditionalReceiver node)
	{
		return null;
	}

	public override BoundNode VisitComplexConditionalReceiver(BoundComplexConditionalReceiver node)
	{
		Visit(node.ValueTypeReceiver);
		Visit(node.ReferenceTypeReceiver);
		return null;
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitPropertyGroup(BoundPropertyGroup node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		Visit(node.ReceiverOpt);
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitEventAssignmentOperator(BoundEventAssignmentOperator node)
	{
		Visit(node.ReceiverOpt);
		Visit(node.Argument);
		return null;
	}

	public override BoundNode VisitAttribute(BoundAttribute node)
	{
		VisitList(node.ConstructorArguments);
		VisitList(node.NamedArguments);
		return null;
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		VisitList(node.Arguments);
		Visit(node.InitializerExpressionOpt);
		return null;
	}

	public override BoundNode VisitTupleLiteral(BoundTupleLiteral node)
	{
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node)
	{
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		VisitList(node.Arguments);
		Visit(node.InitializerExpressionOpt);
		return null;
	}

	public override BoundNode VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		Visit(node.InitializerExpressionOpt);
		return null;
	}

	public override BoundNode VisitObjectInitializerExpression(BoundObjectInitializerExpression node)
	{
		VisitList(node.Initializers);
		return null;
	}

	public override BoundNode VisitObjectInitializerMember(BoundObjectInitializerMember node)
	{
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node)
	{
		return null;
	}

	public override BoundNode VisitCollectionInitializerExpression(BoundCollectionInitializerExpression node)
	{
		VisitList(node.Initializers);
		return null;
	}

	public override BoundNode VisitCollectionElementInitializer(BoundCollectionElementInitializer node)
	{
		VisitList(node.Arguments);
		Visit(node.ImplicitReceiverOpt);
		return null;
	}

	public override BoundNode VisitDynamicCollectionElementInitializer(BoundDynamicCollectionElementInitializer node)
	{
		VisitList(node.Arguments);
		Visit(node.ImplicitReceiver);
		return null;
	}

	public override BoundNode VisitImplicitReceiver(BoundImplicitReceiver node)
	{
		return null;
	}

	public override BoundNode VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node)
	{
		VisitList(node.Arguments);
		VisitList(node.Declarations);
		return null;
	}

	public override BoundNode VisitAnonymousPropertyDeclaration(BoundAnonymousPropertyDeclaration node)
	{
		return null;
	}

	public override BoundNode VisitNewT(BoundNewT node)
	{
		Visit(node.InitializerExpressionOpt);
		return null;
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		Visit(node.Argument);
		return null;
	}

	public override BoundNode VisitArrayCreation(BoundArrayCreation node)
	{
		VisitList(node.Bounds);
		Visit(node.InitializerOpt);
		return null;
	}

	public override BoundNode VisitArrayInitialization(BoundArrayInitialization node)
	{
		VisitList(node.Initializers);
		return null;
	}

	public override BoundNode VisitStackAllocArrayCreation(BoundStackAllocArrayCreation node)
	{
		Visit(node.Count);
		Visit(node.InitializerOpt);
		return null;
	}

	public override BoundNode VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node)
	{
		Visit(node.Count);
		Visit(node.InitializerOpt);
		return null;
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitHoistedFieldAccess(BoundHoistedFieldAccess node)
	{
		return null;
	}

	public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitEventAccess(BoundEventAccess node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitIndexerAccess(BoundIndexerAccess node)
	{
		Visit(node.ReceiverOpt);
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
	{
		Visit(node.ReceiverOpt);
		VisitList(node.Arguments);
		return null;
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		Visit(node.Body);
		return null;
	}

	public override BoundNode VisitUnboundLambda(UnboundLambda node)
	{
		return null;
	}

	public override BoundNode VisitQueryClause(BoundQueryClause node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitTypeOrInstanceInitializers(BoundTypeOrInstanceInitializers node)
	{
		VisitList(node.Statements);
		return null;
	}

	public override BoundNode VisitNameOfOperator(BoundNameOfOperator node)
	{
		Visit(node.Argument);
		return null;
	}

	public override BoundNode VisitInterpolatedString(BoundInterpolatedString node)
	{
		VisitList(node.Parts);
		return null;
	}

	public override BoundNode VisitStringInsert(BoundStringInsert node)
	{
		Visit(node.Value);
		Visit(node.Alignment);
		Visit(node.Format);
		return null;
	}

	public override BoundNode VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		Visit(node.Expression);
		Visit(node.Pattern);
		return null;
	}

	public override BoundNode VisitDeclarationPattern(BoundDeclarationPattern node)
	{
		Visit(node.VariableAccess);
		Visit(node.DeclaredType);
		return null;
	}

	public override BoundNode VisitConstantPattern(BoundConstantPattern node)
	{
		Visit(node.Value);
		return null;
	}

	public override BoundNode VisitWildcardPattern(BoundWildcardPattern node)
	{
		return null;
	}

	public override BoundNode VisitDiscardExpression(BoundDiscardExpression node)
	{
		return null;
	}

	public override BoundNode VisitThrowExpression(BoundThrowExpression node)
	{
		Visit(node.Expression);
		return null;
	}

	public override BoundNode VisitOutVariablePendingInference(OutVariablePendingInference node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitDeconstructionVariablePendingInference(DeconstructionVariablePendingInference node)
	{
		Visit(node.ReceiverOpt);
		return null;
	}

	public override BoundNode VisitOutDeconstructVarPendingInference(OutDeconstructVarPendingInference node)
	{
		return null;
	}

	public override BoundNode VisitNonConstructorMethodBody(BoundNonConstructorMethodBody node)
	{
		Visit(node.BlockBody);
		Visit(node.ExpressionBody);
		return null;
	}

	public override BoundNode VisitConstructorMethodBody(BoundConstructorMethodBody node)
	{
		Visit(node.Initializer);
		Visit(node.BlockBody);
		Visit(node.ExpressionBody);
		return null;
	}
}
