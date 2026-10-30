using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class BoundTreeRewriter : BoundTreeVisitor
{
	public virtual TypeSymbol VisitType(TypeSymbol type)
	{
		return type;
	}

	public ImmutableArray<T> VisitList<T>(ImmutableArray<T> list) where T : BoundNode
	{
		if (list.IsDefault)
		{
			return list;
		}
		return DoVisitList(list);
	}

	private ImmutableArray<T> DoVisitList<T>(ImmutableArray<T> list) where T : BoundNode
	{
		ArrayBuilder<T> arrayBuilder = null;
		for (int i = 0; i < list.Length; i++)
		{
			T val = list[i];
			BoundNode boundNode = Visit(val);
			if (val != boundNode && arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<T>.GetInstance();
				if (i > 0)
				{
					arrayBuilder.AddRange(list, i);
				}
			}
			if (arrayBuilder != null && boundNode != null)
			{
				arrayBuilder.Add((T)boundNode);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? list;
	}

	public override BoundNode VisitFieldEqualsValue(BoundFieldEqualsValue node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		return node.Update(node.Field, node.Locals, value);
	}

	public override BoundNode VisitPropertyEqualsValue(BoundPropertyEqualsValue node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		return node.Update(node.Property, node.Locals, value);
	}

	public override BoundNode VisitParameterEqualsValue(BoundParameterEqualsValue node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		return node.Update(node.Parameter, node.Locals, value);
	}

	public override BoundNode VisitGlobalStatementInitializer(BoundGlobalStatementInitializer node)
	{
		BoundStatement statement = (BoundStatement)Visit(node.Statement);
		return node.Update(statement);
	}

	public override BoundNode VisitDeconstructValuePlaceholder(BoundDeconstructValuePlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ValEscape, type);
	}

	public override BoundNode VisitTupleOperandPlaceholder(BoundTupleOperandPlaceholder node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitDup(BoundDup node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.RefKind, type);
	}

	public override BoundNode VisitPassByCopy(BoundPassByCopy node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode VisitBadExpression(BoundBadExpression node)
	{
		ImmutableArray<BoundExpression> childBoundNodes = VisitList(node.ChildBoundNodes);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ResultKind, node.Symbols, childBoundNodes, type);
	}

	public override BoundNode VisitBadStatement(BoundBadStatement node)
	{
		ImmutableArray<BoundNode> childBoundNodes = VisitList(node.ChildBoundNodes);
		return node.Update(childBoundNodes);
	}

	public override BoundNode VisitTypeExpression(BoundTypeExpression node)
	{
		BoundTypeExpression boundContainingTypeOpt = (BoundTypeExpression)Visit(node.BoundContainingTypeOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.AliasOpt, node.InferredType, boundContainingTypeOpt, type);
	}

	public override BoundNode VisitTypeOrValueExpression(BoundTypeOrValueExpression node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Data, type);
	}

	public override BoundNode VisitNamespaceExpression(BoundNamespaceExpression node)
	{
		VisitType(node.Type);
		return node.Update(node.NamespaceSymbol, node.AliasOpt);
	}

	public override BoundNode VisitUnaryOperator(BoundUnaryOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, operand, node.ConstantValueOpt, node.MethodOpt, node.ResultKind, type);
	}

	public override BoundNode VisitIncrementOperator(BoundIncrementOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, operand, node.MethodOpt, node.OperandConversion, node.ResultConversion, node.ResultKind, type);
	}

	public override BoundNode VisitAddressOfOperator(BoundAddressOfOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, node.IsManaged, type);
	}

	public override BoundNode VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, type);
	}

	public override BoundNode VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundExpression index = (BoundExpression)Visit(node.Index);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, index, node.Checked, type);
	}

	public override BoundNode VisitRefTypeOperator(BoundRefTypeOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, node.GetTypeFromHandle, type);
	}

	public override BoundNode VisitMakeRefOperator(BoundMakeRefOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, type);
	}

	public override BoundNode VisitRefValueOperator(BoundRefValueOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, type);
	}

	public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, left, right, node.ConstantValueOpt, node.MethodOpt, node.ResultKind, type);
	}

	public override BoundNode VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		BoundExpression convertedLeft = node.ConvertedLeft;
		BoundExpression convertedRight = node.ConvertedRight;
		TypeSymbol type = VisitType(node.Type);
		return node.Update(left, right, convertedLeft, convertedRight, node.OperatorKind, node.Operators, type);
	}

	public override BoundNode VisitUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.OperatorKind, left, right, node.LogicalOperator, node.TrueOperator, node.FalseOperator, node.ResultKind, type);
	}

	public override BoundNode VisitCompoundAssignmentOperator(BoundCompoundAssignmentOperator node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Operator, left, right, node.LeftConversion, node.FinalConversion, node.ResultKind, type);
	}

	public override BoundNode VisitAssignmentOperator(BoundAssignmentOperator node)
	{
		BoundExpression left = (BoundExpression)Visit(node.Left);
		BoundExpression right = (BoundExpression)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(left, right, node.IsRef, type);
	}

	public override BoundNode VisitDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator node)
	{
		BoundTupleExpression left = (BoundTupleExpression)Visit(node.Left);
		BoundConversion right = (BoundConversion)Visit(node.Right);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(left, right, node.IsUsed, type);
	}

	public override BoundNode VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		BoundExpression leftOperand = (BoundExpression)Visit(node.LeftOperand);
		BoundExpression rightOperand = (BoundExpression)Visit(node.RightOperand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(leftOperand, rightOperand, node.LeftConversion, type);
	}

	public override BoundNode VisitConditionalOperator(BoundConditionalOperator node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundExpression consequence = (BoundExpression)Visit(node.Consequence);
		BoundExpression alternative = (BoundExpression)Visit(node.Alternative);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.IsRef, condition, consequence, alternative, node.ConstantValueOpt, type);
	}

	public override BoundNode VisitArrayAccess(BoundArrayAccess node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundExpression> indices = VisitList(node.Indices);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, indices, type);
	}

	public override BoundNode VisitArrayLength(BoundArrayLength node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode VisitAwaitExpression(BoundAwaitExpression node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, node.GetAwaiter, node.IsCompleted, node.GetResult, type);
	}

	public override BoundNode VisitTypeOfOperator(BoundTypeOfOperator node)
	{
		BoundTypeExpression sourceType = (BoundTypeExpression)Visit(node.SourceType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(sourceType, node.GetTypeFromHandle, type);
	}

	public override BoundNode VisitMethodDefIndex(BoundMethodDefIndex node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Method, type);
	}

	public override BoundNode VisitMaximumMethodDefIndex(BoundMaximumMethodDefIndex node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitInstrumentationPayloadRoot(BoundInstrumentationPayloadRoot node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.AnalysisKind, type);
	}

	public override BoundNode VisitModuleVersionId(BoundModuleVersionId node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitModuleVersionIdString(BoundModuleVersionIdString node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitSourceDocumentIndex(BoundSourceDocumentIndex node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Document, type);
	}

	public override BoundNode VisitMethodInfo(BoundMethodInfo node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Method, node.GetMethodFromHandle, type);
	}

	public override BoundNode VisitFieldInfo(BoundFieldInfo node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Field, node.GetFieldFromHandle, type);
	}

	public override BoundNode VisitDefaultExpression(BoundDefaultExpression node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ConstantValueOpt, type);
	}

	public override BoundNode VisitIsOperator(BoundIsOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		BoundTypeExpression targetType = (BoundTypeExpression)Visit(node.TargetType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, targetType, node.Conversion, type);
	}

	public override BoundNode VisitAsOperator(BoundAsOperator node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		BoundTypeExpression targetType = (BoundTypeExpression)Visit(node.TargetType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, targetType, node.Conversion, type);
	}

	public override BoundNode VisitSizeOfOperator(BoundSizeOfOperator node)
	{
		BoundTypeExpression sourceType = (BoundTypeExpression)Visit(node.SourceType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(sourceType, node.ConstantValueOpt, type);
	}

	public override BoundNode VisitConversion(BoundConversion node)
	{
		BoundExpression operand = (BoundExpression)Visit(node.Operand);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(operand, node.Conversion, node.IsBaseConversion, node.Checked, node.ExplicitCastInCode, node.ConstantValueOpt, type);
	}

	public override BoundNode VisitArgList(BoundArgList node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitArgListOperator(BoundArgListOperator node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(arguments, node.ArgumentRefKindsOpt, type);
	}

	public override BoundNode VisitFixedLocalCollectionInitializer(BoundFixedLocalCollectionInitializer node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol elementPointerType = VisitType(node.ElementPointerType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(elementPointerType, node.ElementPointerTypeConversion, expression, node.GetPinnableOpt, type);
	}

	public override BoundNode VisitSequencePoint(BoundSequencePoint node)
	{
		BoundStatement statementOpt = (BoundStatement)Visit(node.StatementOpt);
		return node.Update(statementOpt);
	}

	public override BoundNode VisitSequencePointExpression(BoundSequencePointExpression node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode VisitSequencePointWithSpan(BoundSequencePointWithSpan node)
	{
		BoundStatement statementOpt = (BoundStatement)Visit(node.StatementOpt);
		return node.Update(statementOpt, node.Span);
	}

	public override BoundNode VisitBlock(BoundBlock node)
	{
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(node.Locals, node.LocalFunctions, statements);
	}

	public override BoundNode VisitScope(BoundScope node)
	{
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(node.Locals, statements);
	}

	public override BoundNode VisitStateMachineScope(BoundStateMachineScope node)
	{
		BoundStatement statement = (BoundStatement)Visit(node.Statement);
		return node.Update(node.Fields, statement);
	}

	public override BoundNode VisitLocalDeclaration(BoundLocalDeclaration node)
	{
		BoundTypeExpression declaredType = (BoundTypeExpression)Visit(node.DeclaredType);
		BoundExpression initializerOpt = (BoundExpression)Visit(node.InitializerOpt);
		ImmutableArray<BoundExpression> argumentsOpt = VisitList(node.ArgumentsOpt);
		return node.Update(node.LocalSymbol, declaredType, initializerOpt, argumentsOpt);
	}

	public override BoundNode VisitMultipleLocalDeclarations(BoundMultipleLocalDeclarations node)
	{
		ImmutableArray<BoundLocalDeclaration> localDeclarations = VisitList(node.LocalDeclarations);
		return node.Update(localDeclarations);
	}

	public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
	{
		BoundBlock blockBody = (BoundBlock)Visit(node.BlockBody);
		BoundBlock expressionBody = (BoundBlock)Visit(node.ExpressionBody);
		return node.Update(node.Symbol, blockBody, expressionBody);
	}

	public override BoundNode VisitSequence(BoundSequence node)
	{
		ImmutableArray<BoundExpression> sideEffects = VisitList(node.SideEffects);
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Locals, sideEffects, value, type);
	}

	public override BoundNode VisitNoOpStatement(BoundNoOpStatement node)
	{
		return node;
	}

	public override BoundNode VisitReturnStatement(BoundReturnStatement node)
	{
		BoundExpression expressionOpt = (BoundExpression)Visit(node.ExpressionOpt);
		return node.Update(node.RefKind, expressionOpt);
	}

	public override BoundNode VisitYieldReturnStatement(BoundYieldReturnStatement node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		return node.Update(expression);
	}

	public override BoundNode VisitYieldBreakStatement(BoundYieldBreakStatement node)
	{
		return node;
	}

	public override BoundNode VisitThrowStatement(BoundThrowStatement node)
	{
		BoundExpression expressionOpt = (BoundExpression)Visit(node.ExpressionOpt);
		return node.Update(expressionOpt);
	}

	public override BoundNode VisitExpressionStatement(BoundExpressionStatement node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		return node.Update(expression);
	}

	public override BoundNode VisitSwitchStatement(BoundSwitchStatement node)
	{
		BoundStatement loweredPreambleOpt = (BoundStatement)Visit(node.LoweredPreambleOpt);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundSwitchSection> switchSections = VisitList(node.SwitchSections);
		return node.Update(loweredPreambleOpt, expression, node.ConstantTargetOpt, node.InnerLocals, node.InnerLocalFunctions, switchSections, node.BreakLabel, node.StringEquality);
	}

	public override BoundNode VisitSwitchSection(BoundSwitchSection node)
	{
		ImmutableArray<BoundSwitchLabel> switchLabels = VisitList(node.SwitchLabels);
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(node.Locals, switchLabels, statements);
	}

	public override BoundNode VisitSwitchLabel(BoundSwitchLabel node)
	{
		BoundExpression expressionOpt = (BoundExpression)Visit(node.ExpressionOpt);
		return node.Update(node.Label, expressionOpt, node.ConstantValueOpt);
	}

	public override BoundNode VisitBreakStatement(BoundBreakStatement node)
	{
		return node;
	}

	public override BoundNode VisitContinueStatement(BoundContinueStatement node)
	{
		return node;
	}

	public override BoundNode VisitPatternSwitchStatement(BoundPatternSwitchStatement node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundPatternSwitchSection> switchSections = VisitList(node.SwitchSections);
		BoundPatternSwitchLabel defaultLabel = (BoundPatternSwitchLabel)Visit(node.DefaultLabel);
		return node.Update(expression, node.SomeLabelAlwaysMatches, node.InnerLocals, node.InnerLocalFunctions, switchSections, defaultLabel, node.BreakLabel, node.Binder, node.IsComplete);
	}

	public override BoundNode VisitPatternSwitchSection(BoundPatternSwitchSection node)
	{
		ImmutableArray<BoundPatternSwitchLabel> switchLabels = VisitList(node.SwitchLabels);
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(node.Locals, switchLabels, statements);
	}

	public override BoundNode VisitPatternSwitchLabel(BoundPatternSwitchLabel node)
	{
		BoundPattern pattern = (BoundPattern)Visit(node.Pattern);
		BoundExpression guard = (BoundExpression)Visit(node.Guard);
		return node.Update(node.Label, pattern, guard, node.IsReachable);
	}

	public override BoundNode VisitIfStatement(BoundIfStatement node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement consequence = (BoundStatement)Visit(node.Consequence);
		BoundStatement alternativeOpt = (BoundStatement)Visit(node.AlternativeOpt);
		return node.Update(condition, consequence, alternativeOpt);
	}

	public override BoundNode VisitDoStatement(BoundDoStatement node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(node.Locals, condition, body, node.BreakLabel, node.ContinueLabel);
	}

	public override BoundNode VisitWhileStatement(BoundWhileStatement node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(node.Locals, condition, body, node.BreakLabel, node.ContinueLabel);
	}

	public override BoundNode VisitForStatement(BoundForStatement node)
	{
		BoundStatement initializer = (BoundStatement)Visit(node.Initializer);
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		BoundStatement increment = (BoundStatement)Visit(node.Increment);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(node.OuterLocals, initializer, node.InnerLocals, condition, increment, body, node.BreakLabel, node.ContinueLabel);
	}

	public override BoundNode VisitForEachStatement(BoundForEachStatement node)
	{
		BoundTypeExpression iterationVariableType = (BoundTypeExpression)Visit(node.IterationVariableType);
		BoundExpression iterationErrorExpressionOpt = (BoundExpression)Visit(node.IterationErrorExpressionOpt);
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundForEachDeconstructStep deconstructionOpt = (BoundForEachDeconstructStep)Visit(node.DeconstructionOpt);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(node.EnumeratorInfoOpt, node.ElementConversion, iterationVariableType, node.IterationVariables, iterationErrorExpressionOpt, expression, deconstructionOpt, body, node.Checked, node.BreakLabel, node.ContinueLabel);
	}

	public override BoundNode VisitForEachDeconstructStep(BoundForEachDeconstructStep node)
	{
		BoundDeconstructionAssignmentOperator deconstructionAssignment = (BoundDeconstructionAssignmentOperator)Visit(node.DeconstructionAssignment);
		BoundDeconstructValuePlaceholder targetPlaceholder = (BoundDeconstructValuePlaceholder)Visit(node.TargetPlaceholder);
		return node.Update(deconstructionAssignment, targetPlaceholder);
	}

	public override BoundNode VisitUsingStatement(BoundUsingStatement node)
	{
		BoundMultipleLocalDeclarations declarationsOpt = (BoundMultipleLocalDeclarations)Visit(node.DeclarationsOpt);
		BoundExpression expressionOpt = (BoundExpression)Visit(node.ExpressionOpt);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(node.Locals, declarationsOpt, expressionOpt, node.IDisposableConversion, body);
	}

	public override BoundNode VisitFixedStatement(BoundFixedStatement node)
	{
		BoundMultipleLocalDeclarations declarations = (BoundMultipleLocalDeclarations)Visit(node.Declarations);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(node.Locals, declarations, body);
	}

	public override BoundNode VisitLockStatement(BoundLockStatement node)
	{
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(argument, body);
	}

	public override BoundNode VisitTryStatement(BoundTryStatement node)
	{
		BoundBlock tryBlock = (BoundBlock)Visit(node.TryBlock);
		ImmutableArray<BoundCatchBlock> catchBlocks = VisitList(node.CatchBlocks);
		BoundBlock finallyBlockOpt = (BoundBlock)Visit(node.FinallyBlockOpt);
		return node.Update(tryBlock, catchBlocks, finallyBlockOpt, node.PreferFaultHandler);
	}

	public override BoundNode VisitCatchBlock(BoundCatchBlock node)
	{
		BoundExpression exceptionSourceOpt = (BoundExpression)Visit(node.ExceptionSourceOpt);
		BoundExpression exceptionFilterOpt = (BoundExpression)Visit(node.ExceptionFilterOpt);
		BoundBlock body = (BoundBlock)Visit(node.Body);
		TypeSymbol exceptionTypeOpt = VisitType(node.ExceptionTypeOpt);
		return node.Update(node.Locals, exceptionSourceOpt, exceptionTypeOpt, exceptionFilterOpt, body, node.IsSynthesizedAsyncCatchAll);
	}

	public override BoundNode VisitLiteral(BoundLiteral node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ConstantValueOpt, type);
	}

	public override BoundNode VisitThisReference(BoundThisReference node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitPreviousSubmissionReference(BoundPreviousSubmissionReference node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitHostObjectMemberReference(BoundHostObjectMemberReference node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitBaseReference(BoundBaseReference node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitLocal(BoundLocal node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.LocalSymbol, node.IsDeclaration, node.ConstantValueOpt, type);
	}

	public override BoundNode VisitPseudoVariable(BoundPseudoVariable node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.LocalSymbol, node.EmitExpressions, type);
	}

	public override BoundNode VisitRangeVariable(BoundRangeVariable node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.RangeVariableSymbol, value, type);
	}

	public override BoundNode VisitParameter(BoundParameter node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ParameterSymbol, type);
	}

	public override BoundNode VisitLabelStatement(BoundLabelStatement node)
	{
		return node;
	}

	public override BoundNode VisitGotoStatement(BoundGotoStatement node)
	{
		BoundExpression caseExpressionOpt = (BoundExpression)Visit(node.CaseExpressionOpt);
		BoundLabel labelExpressionOpt = (BoundLabel)Visit(node.LabelExpressionOpt);
		return node.Update(node.Label, caseExpressionOpt, labelExpressionOpt);
	}

	public override BoundNode VisitLabeledStatement(BoundLabeledStatement node)
	{
		BoundStatement body = (BoundStatement)Visit(node.Body);
		return node.Update(node.Label, body);
	}

	public override BoundNode VisitLabel(BoundLabel node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Label, type);
	}

	public override BoundNode VisitStatementList(BoundStatementList node)
	{
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(statements);
	}

	public override BoundNode VisitConditionalGoto(BoundConditionalGoto node)
	{
		BoundExpression condition = (BoundExpression)Visit(node.Condition);
		return node.Update(condition, node.JumpIfTrue, node.Label);
	}

	public override BoundNode VisitDynamicMemberAccess(BoundDynamicMemberAccess node)
	{
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, node.TypeArgumentsOpt, node.Name, node.Invoked, node.Indexed, type);
	}

	public override BoundNode VisitDynamicInvocation(BoundDynamicInvocation node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.ApplicableMethods, type);
	}

	public override BoundNode VisitConditionalAccess(BoundConditionalAccess node)
	{
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		BoundExpression accessExpression = (BoundExpression)Visit(node.AccessExpression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, accessExpression, type);
	}

	public override BoundNode VisitLoweredConditionalAccess(BoundLoweredConditionalAccess node)
	{
		BoundExpression receiver = (BoundExpression)Visit(node.Receiver);
		BoundExpression whenNotNull = (BoundExpression)Visit(node.WhenNotNull);
		BoundExpression whenNullOpt = (BoundExpression)Visit(node.WhenNullOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiver, node.HasValueMethodOpt, whenNotNull, whenNullOpt, node.Id, type);
	}

	public override BoundNode VisitConditionalReceiver(BoundConditionalReceiver node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Id, type);
	}

	public override BoundNode VisitComplexConditionalReceiver(BoundComplexConditionalReceiver node)
	{
		BoundExpression valueTypeReceiver = (BoundExpression)Visit(node.ValueTypeReceiver);
		BoundExpression referenceTypeReceiver = (BoundExpression)Visit(node.ReferenceTypeReceiver);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(valueTypeReceiver, referenceTypeReceiver, type);
	}

	public override BoundNode VisitMethodGroup(BoundMethodGroup node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		VisitType(node.Type);
		return node.Update(node.TypeArgumentsOpt, node.Name, node.Methods, node.LookupSymbolOpt, node.LookupError, node.Flags, receiverOpt, node.ResultKind);
	}

	public override BoundNode VisitPropertyGroup(BoundPropertyGroup node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		VisitType(node.Type);
		return node.Update(node.Properties, receiverOpt, node.ResultKind);
	}

	public override BoundNode VisitCall(BoundCall node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.Method, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.IsDelegateCall, node.Expanded, node.InvokedAsExtensionMethod, node.ArgsToParamsOpt, node.ResultKind, node.BinderOpt, type);
	}

	public override BoundNode VisitEventAssignmentOperator(BoundEventAssignmentOperator node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Event, node.IsAddition, node.IsDynamic, receiverOpt, argument, type);
	}

	public override BoundNode VisitAttribute(BoundAttribute node)
	{
		ImmutableArray<BoundExpression> constructorArguments = VisitList(node.ConstructorArguments);
		ImmutableArray<BoundExpression> namedArguments = VisitList(node.NamedArguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Constructor, constructorArguments, node.ConstructorArgumentNamesOpt, namedArguments, node.ResultKind, type);
	}

	public override BoundNode VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		BoundObjectInitializerExpressionBase initializerExpressionOpt = (BoundObjectInitializerExpressionBase)Visit(node.InitializerExpressionOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Constructor, node.ConstructorsGroup, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.ConstantValueOpt, initializerExpressionOpt, node.BinderOpt, type);
	}

	public override BoundNode VisitTupleLiteral(BoundTupleLiteral node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.ArgumentNamesOpt, node.InferredNamesOpt, arguments, type);
	}

	public override BoundNode VisitConvertedTupleLiteral(BoundConvertedTupleLiteral node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol naturalTypeOpt = VisitType(node.NaturalTypeOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(naturalTypeOpt, arguments, type);
	}

	public override BoundNode VisitDynamicObjectCreationExpression(BoundDynamicObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		BoundObjectInitializerExpressionBase initializerExpressionOpt = (BoundObjectInitializerExpressionBase)Visit(node.InitializerExpressionOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Name, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, initializerExpressionOpt, node.ApplicableMethods, type);
	}

	public override BoundNode VisitNoPiaObjectCreationExpression(BoundNoPiaObjectCreationExpression node)
	{
		BoundObjectInitializerExpressionBase initializerExpressionOpt = (BoundObjectInitializerExpressionBase)Visit(node.InitializerExpressionOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.GuidString, initializerExpressionOpt, type);
	}

	public override BoundNode VisitObjectInitializerExpression(BoundObjectInitializerExpression node)
	{
		ImmutableArray<BoundExpression> initializers = VisitList(node.Initializers);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(initializers, type);
	}

	public override BoundNode VisitObjectInitializerMember(BoundObjectInitializerMember node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol receiverType = VisitType(node.ReceiverType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.MemberSymbol, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.ResultKind, receiverType, node.BinderOpt, type);
	}

	public override BoundNode VisitDynamicObjectInitializerMember(BoundDynamicObjectInitializerMember node)
	{
		TypeSymbol receiverType = VisitType(node.ReceiverType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.MemberName, receiverType, type);
	}

	public override BoundNode VisitCollectionInitializerExpression(BoundCollectionInitializerExpression node)
	{
		ImmutableArray<BoundExpression> initializers = VisitList(node.Initializers);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(initializers, type);
	}

	public override BoundNode VisitCollectionElementInitializer(BoundCollectionElementInitializer node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		BoundExpression implicitReceiverOpt = (BoundExpression)Visit(node.ImplicitReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.AddMethod, arguments, implicitReceiverOpt, node.Expanded, node.ArgsToParamsOpt, node.InvokedAsExtensionMethod, node.ResultKind, node.BinderOpt, type);
	}

	public override BoundNode VisitDynamicCollectionElementInitializer(BoundDynamicCollectionElementInitializer node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		BoundImplicitReceiver implicitReceiver = (BoundImplicitReceiver)Visit(node.ImplicitReceiver);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(arguments, implicitReceiver, node.ApplicableMethods, type);
	}

	public override BoundNode VisitImplicitReceiver(BoundImplicitReceiver node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitAnonymousObjectCreationExpression(BoundAnonymousObjectCreationExpression node)
	{
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		ImmutableArray<BoundAnonymousPropertyDeclaration> declarations = VisitList(node.Declarations);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Constructor, arguments, declarations, type);
	}

	public override BoundNode VisitAnonymousPropertyDeclaration(BoundAnonymousPropertyDeclaration node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Property, type);
	}

	public override BoundNode VisitNewT(BoundNewT node)
	{
		BoundObjectInitializerExpressionBase initializerExpressionOpt = (BoundObjectInitializerExpressionBase)Visit(node.InitializerExpressionOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(initializerExpressionOpt, type);
	}

	public override BoundNode VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(argument, node.MethodOpt, node.IsExtensionMethod, type);
	}

	public override BoundNode VisitArrayCreation(BoundArrayCreation node)
	{
		ImmutableArray<BoundExpression> bounds = VisitList(node.Bounds);
		BoundArrayInitialization initializerOpt = (BoundArrayInitialization)Visit(node.InitializerOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(bounds, initializerOpt, type);
	}

	public override BoundNode VisitArrayInitialization(BoundArrayInitialization node)
	{
		ImmutableArray<BoundExpression> initializers = VisitList(node.Initializers);
		VisitType(node.Type);
		return node.Update(initializers);
	}

	public override BoundNode VisitStackAllocArrayCreation(BoundStackAllocArrayCreation node)
	{
		BoundExpression count = (BoundExpression)Visit(node.Count);
		BoundArrayInitialization initializerOpt = (BoundArrayInitialization)Visit(node.InitializerOpt);
		TypeSymbol elementType = VisitType(node.ElementType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(elementType, count, initializerOpt, type);
	}

	public override BoundNode VisitConvertedStackAllocExpression(BoundConvertedStackAllocExpression node)
	{
		BoundExpression count = (BoundExpression)Visit(node.Count);
		BoundArrayInitialization initializerOpt = (BoundArrayInitialization)Visit(node.InitializerOpt);
		TypeSymbol elementType = VisitType(node.ElementType);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(elementType, count, initializerOpt, type);
	}

	public override BoundNode VisitFieldAccess(BoundFieldAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.FieldSymbol, node.ConstantValueOpt, node.ResultKind, node.IsByValue, node.IsDeclaration, type);
	}

	public override BoundNode VisitHoistedFieldAccess(BoundHoistedFieldAccess node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.FieldSymbol, type);
	}

	public override BoundNode VisitPropertyAccess(BoundPropertyAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.PropertySymbol, node.ResultKind, type);
	}

	public override BoundNode VisitEventAccess(BoundEventAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.EventSymbol, node.IsUsableAsField, node.ResultKind, type);
	}

	public override BoundNode VisitIndexerAccess(BoundIndexerAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, node.Indexer, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.Expanded, node.ArgsToParamsOpt, node.BinderOpt, node.UseSetterForDefaultArgumentGeneration, type);
	}

	public override BoundNode VisitDynamicIndexerAccess(BoundDynamicIndexerAccess node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		ImmutableArray<BoundExpression> arguments = VisitList(node.Arguments);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(receiverOpt, arguments, node.ArgumentNamesOpt, node.ArgumentRefKindsOpt, node.ApplicableIndexers, type);
	}

	public override BoundNode VisitLambda(BoundLambda node)
	{
		BoundBlock body = (BoundBlock)Visit(node.Body);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(node.Symbol, body, node.Diagnostics, node.Binder, type);
	}

	public override BoundNode VisitUnboundLambda(UnboundLambda node)
	{
		VisitType(node.Type);
		return node.Update(node.Data);
	}

	public override BoundNode VisitQueryClause(BoundQueryClause node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(value, node.DefinedSymbol, node.Binder, type);
	}

	public override BoundNode VisitTypeOrInstanceInitializers(BoundTypeOrInstanceInitializers node)
	{
		ImmutableArray<BoundStatement> statements = VisitList(node.Statements);
		return node.Update(statements);
	}

	public override BoundNode VisitNameOfOperator(BoundNameOfOperator node)
	{
		BoundExpression argument = (BoundExpression)Visit(node.Argument);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(argument, node.ConstantValueOpt, type);
	}

	public override BoundNode VisitInterpolatedString(BoundInterpolatedString node)
	{
		ImmutableArray<BoundExpression> parts = VisitList(node.Parts);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(parts, type);
	}

	public override BoundNode VisitStringInsert(BoundStringInsert node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		BoundExpression alignment = (BoundExpression)Visit(node.Alignment);
		BoundLiteral format = (BoundLiteral)Visit(node.Format);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(value, alignment, format, type);
	}

	public override BoundNode VisitIsPatternExpression(BoundIsPatternExpression node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		BoundPattern pattern = (BoundPattern)Visit(node.Pattern);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, pattern, type);
	}

	public override BoundNode VisitDeclarationPattern(BoundDeclarationPattern node)
	{
		BoundExpression variableAccess = (BoundExpression)Visit(node.VariableAccess);
		BoundTypeExpression declaredType = (BoundTypeExpression)Visit(node.DeclaredType);
		return node.Update(node.Variable, variableAccess, declaredType, node.IsVar);
	}

	public override BoundNode VisitConstantPattern(BoundConstantPattern node)
	{
		BoundExpression value = (BoundExpression)Visit(node.Value);
		return node.Update(value, node.ConstantValue);
	}

	public override BoundNode VisitWildcardPattern(BoundWildcardPattern node)
	{
		return node;
	}

	public override BoundNode VisitDiscardExpression(BoundDiscardExpression node)
	{
		TypeSymbol type = VisitType(node.Type);
		return node.Update(type);
	}

	public override BoundNode VisitThrowExpression(BoundThrowExpression node)
	{
		BoundExpression expression = (BoundExpression)Visit(node.Expression);
		TypeSymbol type = VisitType(node.Type);
		return node.Update(expression, type);
	}

	public override BoundNode VisitOutVariablePendingInference(OutVariablePendingInference node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		VisitType(node.Type);
		return node.Update(node.VariableSymbol, receiverOpt);
	}

	public override BoundNode VisitDeconstructionVariablePendingInference(DeconstructionVariablePendingInference node)
	{
		BoundExpression receiverOpt = (BoundExpression)Visit(node.ReceiverOpt);
		VisitType(node.Type);
		return node.Update(node.VariableSymbol, receiverOpt);
	}

	public override BoundNode VisitOutDeconstructVarPendingInference(OutDeconstructVarPendingInference node)
	{
		VisitType(node.Type);
		return node.Update();
	}

	public override BoundNode VisitNonConstructorMethodBody(BoundNonConstructorMethodBody node)
	{
		BoundBlock blockBody = (BoundBlock)Visit(node.BlockBody);
		BoundBlock expressionBody = (BoundBlock)Visit(node.ExpressionBody);
		return node.Update(blockBody, expressionBody);
	}

	public override BoundNode VisitConstructorMethodBody(BoundConstructorMethodBody node)
	{
		BoundExpressionStatement initializer = (BoundExpressionStatement)Visit(node.Initializer);
		BoundBlock blockBody = (BoundBlock)Visit(node.BlockBody);
		BoundBlock expressionBody = (BoundBlock)Visit(node.ExpressionBody);
		return node.Update(node.Locals, initializer, blockBody, expressionBody);
	}
}
