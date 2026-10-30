using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class CSharpOperationFactory
{
	internal class Helper
	{
		internal static bool IsPostfixIncrementOrDecrement(Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind operatorKind)
		{
			Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind unaryOperatorKind = operatorKind & Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.OpMask;
			if (unaryOperatorKind == Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.PostfixIncrement || unaryOperatorKind == Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.PostfixDecrement)
			{
				return true;
			}
			return false;
		}

		internal static bool IsDecrement(Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind operatorKind)
		{
			Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind unaryOperatorKind = operatorKind & Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.OpMask;
			if (unaryOperatorKind == Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.PostfixDecrement || unaryOperatorKind == Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.PrefixDecrement)
			{
				return true;
			}
			return false;
		}

		internal static UnaryOperatorKind DeriveUnaryOperatorKind(Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind operatorKind)
		{
			return (operatorKind & Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.OpMask) switch
			{
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.UnaryPlus => UnaryOperatorKind.Plus, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.UnaryMinus => UnaryOperatorKind.Minus, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.LogicalNegation => UnaryOperatorKind.Not, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.BitwiseComplement => UnaryOperatorKind.BitwiseNegation, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.True => UnaryOperatorKind.True, 
				Microsoft.CodeAnalysis.CSharp.UnaryOperatorKind.False => UnaryOperatorKind.False, 
				_ => UnaryOperatorKind.None, 
			};
		}

		internal static BinaryOperatorKind DeriveBinaryOperatorKind(Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind operatorKind)
		{
			return operatorKind.OperatorWithLogical() switch
			{
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Addition => BinaryOperatorKind.Add, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Subtraction => BinaryOperatorKind.Subtract, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Multiplication => BinaryOperatorKind.Multiply, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Division => BinaryOperatorKind.Divide, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Remainder => BinaryOperatorKind.Remainder, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LeftShift => BinaryOperatorKind.LeftShift, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.RightShift => BinaryOperatorKind.RightShift, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.And => BinaryOperatorKind.And, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Or => BinaryOperatorKind.Or, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Xor => BinaryOperatorKind.ExclusiveOr, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LessThan => BinaryOperatorKind.LessThan, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LessThanOrEqual => BinaryOperatorKind.LessThanOrEqual, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.Equal => BinaryOperatorKind.Equals, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.NotEqual => BinaryOperatorKind.NotEquals, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.GreaterThanOrEqual => BinaryOperatorKind.GreaterThanOrEqual, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.GreaterThan => BinaryOperatorKind.GreaterThan, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LogicalAnd => BinaryOperatorKind.ConditionalAnd, 
				Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.LogicalOr => BinaryOperatorKind.ConditionalOr, 
				_ => BinaryOperatorKind.None, 
			};
		}
	}

	private readonly ConcurrentDictionary<BoundNode, IOperation> _cache = new ConcurrentDictionary<BoundNode, IOperation>(2, 10);

	private readonly SemanticModel _semanticModel;

	private static readonly IConvertibleConversion s_boxedIdentityConversion = Conversion.Identity;

	public CSharpOperationFactory(SemanticModel semanticModel)
	{
		_semanticModel = semanticModel;
	}

	public IOperation Create(BoundNode boundNode)
	{
		if (boundNode == null)
		{
			return null;
		}
		if (boundNode.Kind == BoundKind.ImplicitReceiver)
		{
			return OperationCloner.CloneOperation(CreateInternal(boundNode));
		}
		return _cache.GetOrAdd(boundNode, (BoundNode n) => CreateInternal(n));
	}

	private IOperation CreateInternal(BoundNode boundNode)
	{
		switch (boundNode.Kind)
		{
		case BoundKind.DeconstructValuePlaceholder:
			return CreateBoundDeconstructValuePlaceholderOperation((BoundDeconstructValuePlaceholder)boundNode);
		case BoundKind.DeconstructionAssignmentOperator:
			return CreateBoundDeconstructionAssignmentOperator((BoundDeconstructionAssignmentOperator)boundNode);
		case BoundKind.Call:
			return CreateBoundCallOperation((BoundCall)boundNode);
		case BoundKind.Local:
			return CreateBoundLocalOperation((BoundLocal)boundNode);
		case BoundKind.FieldAccess:
			return CreateBoundFieldAccessOperation((BoundFieldAccess)boundNode);
		case BoundKind.PropertyAccess:
			return CreateBoundPropertyAccessOperation((BoundPropertyAccess)boundNode);
		case BoundKind.IndexerAccess:
			return CreateBoundIndexerAccessOperation((BoundIndexerAccess)boundNode);
		case BoundKind.EventAccess:
			return CreateBoundEventAccessOperation((BoundEventAccess)boundNode);
		case BoundKind.EventAssignmentOperator:
			return CreateBoundEventAssignmentOperatorOperation((BoundEventAssignmentOperator)boundNode);
		case BoundKind.Parameter:
			return CreateBoundParameterOperation((BoundParameter)boundNode);
		case BoundKind.Literal:
			return CreateBoundLiteralOperation((BoundLiteral)boundNode);
		case BoundKind.DynamicInvocation:
			return CreateBoundDynamicInvocationExpressionOperation((BoundDynamicInvocation)boundNode);
		case BoundKind.DynamicIndexerAccess:
			return CreateBoundDynamicIndexerAccessExpressionOperation((BoundDynamicIndexerAccess)boundNode);
		case BoundKind.ObjectCreationExpression:
			return CreateBoundObjectCreationExpressionOperation((BoundObjectCreationExpression)boundNode);
		case BoundKind.DynamicObjectCreationExpression:
			return CreateBoundDynamicObjectCreationExpressionOperation((BoundDynamicObjectCreationExpression)boundNode);
		case BoundKind.ObjectInitializerExpression:
			return CreateBoundObjectInitializerExpressionOperation((BoundObjectInitializerExpression)boundNode);
		case BoundKind.CollectionInitializerExpression:
			return CreateBoundCollectionInitializerExpressionOperation((BoundCollectionInitializerExpression)boundNode);
		case BoundKind.ObjectInitializerMember:
			return CreateBoundObjectInitializerMemberOperation((BoundObjectInitializerMember)boundNode);
		case BoundKind.CollectionElementInitializer:
			return CreateBoundCollectionElementInitializerOperation((BoundCollectionElementInitializer)boundNode);
		case BoundKind.DynamicObjectInitializerMember:
			return CreateBoundDynamicObjectInitializerMemberOperation((BoundDynamicObjectInitializerMember)boundNode);
		case BoundKind.DynamicMemberAccess:
			return CreateBoundDynamicMemberAccessOperation((BoundDynamicMemberAccess)boundNode);
		case BoundKind.DynamicCollectionElementInitializer:
			return CreateBoundDynamicCollectionElementInitializerOperation((BoundDynamicCollectionElementInitializer)boundNode);
		case BoundKind.UnboundLambda:
			return CreateUnboundLambdaOperation((UnboundLambda)boundNode);
		case BoundKind.Lambda:
			return CreateBoundLambdaOperation((BoundLambda)boundNode);
		case BoundKind.Conversion:
			return CreateBoundConversionOperation((BoundConversion)boundNode);
		case BoundKind.AsOperator:
			return CreateBoundAsOperatorOperation((BoundAsOperator)boundNode);
		case BoundKind.IsOperator:
			return CreateBoundIsOperatorOperation((BoundIsOperator)boundNode);
		case BoundKind.SizeOfOperator:
			return CreateBoundSizeOfOperatorOperation((BoundSizeOfOperator)boundNode);
		case BoundKind.TypeOfOperator:
			return CreateBoundTypeOfOperatorOperation((BoundTypeOfOperator)boundNode);
		case BoundKind.ArrayCreation:
			return CreateBoundArrayCreationOperation((BoundArrayCreation)boundNode);
		case BoundKind.ArrayInitialization:
			return CreateBoundArrayInitializationOperation((BoundArrayInitialization)boundNode);
		case BoundKind.DefaultExpression:
			return CreateBoundDefaultExpressionOperation((BoundDefaultExpression)boundNode);
		case BoundKind.BaseReference:
			return CreateBoundBaseReferenceOperation((BoundBaseReference)boundNode);
		case BoundKind.ThisReference:
			return CreateBoundThisReferenceOperation((BoundThisReference)boundNode);
		case BoundKind.AssignmentOperator:
			return CreateBoundAssignmentOperatorOrMemberInitializerOperation((BoundAssignmentOperator)boundNode);
		case BoundKind.CompoundAssignmentOperator:
			return CreateBoundCompoundAssignmentOperatorOperation((BoundCompoundAssignmentOperator)boundNode);
		case BoundKind.IncrementOperator:
			return CreateBoundIncrementOperatorOperation((BoundIncrementOperator)boundNode);
		case BoundKind.BadExpression:
			return CreateBoundBadExpressionOperation((BoundBadExpression)boundNode);
		case BoundKind.NewT:
			return CreateBoundNewTOperation((BoundNewT)boundNode);
		case BoundKind.NoPiaObjectCreationExpression:
			return CreateNoPiaObjectCreationExpressionOperation((BoundNoPiaObjectCreationExpression)boundNode);
		case BoundKind.UnaryOperator:
			return CreateBoundUnaryOperatorOperation((BoundUnaryOperator)boundNode);
		case BoundKind.BinaryOperator:
			return CreateBoundBinaryOperatorOperation((BoundBinaryOperator)boundNode);
		case BoundKind.UserDefinedConditionalLogicalOperator:
			return CreateBoundUserDefinedConditionalLogicalOperator((BoundUserDefinedConditionalLogicalOperator)boundNode);
		case BoundKind.TupleBinaryOperator:
			return CreateBoundTupleBinaryOperatorOperation((BoundTupleBinaryOperator)boundNode);
		case BoundKind.ConditionalOperator:
			return CreateBoundConditionalOperatorOperation((BoundConditionalOperator)boundNode);
		case BoundKind.NullCoalescingOperator:
			return CreateBoundNullCoalescingOperatorOperation((BoundNullCoalescingOperator)boundNode);
		case BoundKind.AwaitExpression:
			return CreateBoundAwaitExpressionOperation((BoundAwaitExpression)boundNode);
		case BoundKind.ArrayAccess:
			return CreateBoundArrayAccessOperation((BoundArrayAccess)boundNode);
		case BoundKind.NameOfOperator:
			return CreateBoundNameOfOperatorOperation((BoundNameOfOperator)boundNode);
		case BoundKind.ThrowExpression:
			return CreateBoundThrowExpressionOperation((BoundThrowExpression)boundNode);
		case BoundKind.AddressOfOperator:
			return CreateBoundAddressOfOperatorOperation((BoundAddressOfOperator)boundNode);
		case BoundKind.ImplicitReceiver:
			return CreateBoundImplicitReceiverOperation((BoundImplicitReceiver)boundNode);
		case BoundKind.ConditionalAccess:
			return CreateBoundConditionalAccessOperation((BoundConditionalAccess)boundNode);
		case BoundKind.ConditionalReceiver:
			return CreateBoundConditionalReceiverOperation((BoundConditionalReceiver)boundNode);
		case BoundKind.FieldEqualsValue:
			return CreateBoundFieldEqualsValueOperation((BoundFieldEqualsValue)boundNode);
		case BoundKind.PropertyEqualsValue:
			return CreateBoundPropertyEqualsValueOperation((BoundPropertyEqualsValue)boundNode);
		case BoundKind.ParameterEqualsValue:
			return CreateBoundParameterEqualsValueOperation((BoundParameterEqualsValue)boundNode);
		case BoundKind.Block:
			return CreateBoundBlockOperation((BoundBlock)boundNode);
		case BoundKind.ContinueStatement:
			return CreateBoundContinueStatementOperation((BoundContinueStatement)boundNode);
		case BoundKind.BreakStatement:
			return CreateBoundBreakStatementOperation((BoundBreakStatement)boundNode);
		case BoundKind.YieldBreakStatement:
			return CreateBoundYieldBreakStatementOperation((BoundYieldBreakStatement)boundNode);
		case BoundKind.GotoStatement:
			return CreateBoundGotoStatementOperation((BoundGotoStatement)boundNode);
		case BoundKind.NoOpStatement:
			return CreateBoundNoOpStatementOperation((BoundNoOpStatement)boundNode);
		case BoundKind.IfStatement:
			return CreateBoundIfStatementOperation((BoundIfStatement)boundNode);
		case BoundKind.WhileStatement:
			return CreateBoundWhileStatementOperation((BoundWhileStatement)boundNode);
		case BoundKind.DoStatement:
			return CreateBoundDoStatementOperation((BoundDoStatement)boundNode);
		case BoundKind.ForStatement:
			return CreateBoundForStatementOperation((BoundForStatement)boundNode);
		case BoundKind.ForEachStatement:
			return CreateBoundForEachStatementOperation((BoundForEachStatement)boundNode);
		case BoundKind.SwitchStatement:
			return CreateBoundSwitchStatementOperation((BoundSwitchStatement)boundNode);
		case BoundKind.SwitchLabel:
			return CreateBoundSwitchLabelOperation((BoundSwitchLabel)boundNode);
		case BoundKind.TryStatement:
			return CreateBoundTryStatementOperation((BoundTryStatement)boundNode);
		case BoundKind.CatchBlock:
			return CreateBoundCatchBlockOperation((BoundCatchBlock)boundNode);
		case BoundKind.FixedStatement:
			return CreateBoundFixedStatementOperation((BoundFixedStatement)boundNode);
		case BoundKind.UsingStatement:
			return CreateBoundUsingStatementOperation((BoundUsingStatement)boundNode);
		case BoundKind.ThrowStatement:
			return CreateBoundThrowStatementOperation((BoundThrowStatement)boundNode);
		case BoundKind.ReturnStatement:
			return CreateBoundReturnStatementOperation((BoundReturnStatement)boundNode);
		case BoundKind.YieldReturnStatement:
			return CreateBoundYieldReturnStatementOperation((BoundYieldReturnStatement)boundNode);
		case BoundKind.LockStatement:
			return CreateBoundLockStatementOperation((BoundLockStatement)boundNode);
		case BoundKind.BadStatement:
			return CreateBoundBadStatementOperation((BoundBadStatement)boundNode);
		case BoundKind.LocalDeclaration:
			return CreateBoundLocalDeclarationOperation((BoundLocalDeclaration)boundNode);
		case BoundKind.MultipleLocalDeclarations:
			return CreateBoundMultipleLocalDeclarationsOperation((BoundMultipleLocalDeclarations)boundNode);
		case BoundKind.LabelStatement:
			return CreateBoundLabelStatementOperation((BoundLabelStatement)boundNode);
		case BoundKind.LabeledStatement:
			return CreateBoundLabeledStatementOperation((BoundLabeledStatement)boundNode);
		case BoundKind.ExpressionStatement:
			return CreateBoundExpressionStatementOperation((BoundExpressionStatement)boundNode);
		case BoundKind.TupleLiteral:
			return CreateBoundTupleLiteralOperation((BoundTupleLiteral)boundNode);
		case BoundKind.ConvertedTupleLiteral:
			return CreateBoundConvertedTupleLiteralOperation((BoundConvertedTupleLiteral)boundNode);
		case BoundKind.InterpolatedString:
			return CreateBoundInterpolatedStringExpressionOperation((BoundInterpolatedString)boundNode);
		case BoundKind.StringInsert:
			return CreateBoundInterpolationOperation((BoundStringInsert)boundNode);
		case BoundKind.LocalFunctionStatement:
			return CreateBoundLocalFunctionStatementOperation((BoundLocalFunctionStatement)boundNode);
		case BoundKind.AnonymousObjectCreationExpression:
			return CreateBoundAnonymousObjectCreationExpressionOperation((BoundAnonymousObjectCreationExpression)boundNode);
		case BoundKind.AnonymousPropertyDeclaration:
			throw ExceptionUtilities.Unreachable;
		case BoundKind.ConstantPattern:
			return CreateBoundConstantPatternOperation((BoundConstantPattern)boundNode);
		case BoundKind.DeclarationPattern:
			return CreateBoundDeclarationPatternOperation((BoundDeclarationPattern)boundNode);
		case BoundKind.WildcardPattern:
			throw ExceptionUtilities.Unreachable;
		case BoundKind.PatternSwitchStatement:
			return CreateBoundPatternSwitchStatementOperation((BoundPatternSwitchStatement)boundNode);
		case BoundKind.PatternSwitchLabel:
			return CreateBoundPatternSwitchLabelOperation((BoundPatternSwitchLabel)boundNode);
		case BoundKind.IsPatternExpression:
			return CreateBoundIsPatternExpressionOperation((BoundIsPatternExpression)boundNode);
		case BoundKind.QueryClause:
			return CreateBoundQueryClauseOperation((BoundQueryClause)boundNode);
		case BoundKind.DelegateCreationExpression:
			return CreateBoundDelegateCreationExpressionOperation((BoundDelegateCreationExpression)boundNode);
		case BoundKind.RangeVariable:
			return CreateBoundRangeVariableOperation((BoundRangeVariable)boundNode);
		case BoundKind.ConstructorMethodBody:
			return CreateConstructorBodyOperation((BoundConstructorMethodBody)boundNode);
		case BoundKind.NonConstructorMethodBody:
			return CreateMethodBodyOperation((BoundNonConstructorMethodBody)boundNode);
		case BoundKind.DiscardExpression:
			return CreateDiscardExpressionOperation((BoundDiscardExpression)boundNode);
		default:
		{
			Optional<object> constantValue = ConvertToOptional((boundNode as BoundExpression)?.ConstantValue);
			bool flag = boundNode.WasCompilerGenerated;
			if (!flag)
			{
				BoundKind kind = boundNode.Kind;
				if (kind == BoundKind.FixedLocalCollectionInitializer)
				{
					flag = true;
				}
			}
			return Operation.CreateOperationNone(_semanticModel, boundNode.Syntax, constantValue, () => GetIOperationChildren(boundNode), flag);
		}
		}
	}

	private IMethodBodyOperation CreateMethodBodyOperation(BoundNonConstructorMethodBody boundNode)
	{
		Lazy<IBlockOperation> blockBody = new Lazy<IBlockOperation>(() => (IBlockOperation)Create(boundNode.BlockBody));
		Lazy<IBlockOperation> expressionBody = new Lazy<IBlockOperation>(() => (IBlockOperation)Create(boundNode.ExpressionBody));
		return new LazyMethodBodyOperation(_semanticModel, boundNode.Syntax, blockBody, expressionBody);
	}

	private IConstructorBodyOperation CreateConstructorBodyOperation(BoundConstructorMethodBody boundNode)
	{
		Lazy<IOperation> initializer = new Lazy<IOperation>(() => Create(boundNode.Initializer));
		Lazy<IBlockOperation> blockBody = new Lazy<IBlockOperation>(() => (IBlockOperation)Create(boundNode.BlockBody));
		Lazy<IBlockOperation> expressionBody = new Lazy<IBlockOperation>(() => (IBlockOperation)Create(boundNode.ExpressionBody));
		return new LazyConstructorBodyOperation(boundNode.Locals.As<ILocalSymbol>(), _semanticModel, boundNode.Syntax, initializer, blockBody, expressionBody);
	}

	private ImmutableArray<IOperation> GetIOperationChildren(BoundNode boundNode)
	{
		ImmutableArray<BoundNode> children = ((IBoundNodeWithIOperationChildren)boundNode).Children;
		if (children.IsDefaultOrEmpty)
		{
			return ImmutableArray<IOperation>.Empty;
		}
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(children.Length);
		foreach (BoundNode item in children)
		{
			IOperation operation = Create(item);
			if (operation != null)
			{
				instance.Add(operation);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private IVariableDeclaratorOperation CreateVariableDeclarator(BoundLocalDeclaration boundNode)
	{
		return (IVariableDeclaratorOperation)_cache.GetOrAdd(boundNode, (BoundNode n) => CreateVariableDeclaratorInternal((BoundLocalDeclaration)n, n.Syntax));
	}

	private IPlaceholderOperation CreateBoundDeconstructValuePlaceholderOperation(BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder)
	{
		SyntaxNode syntax = boundDeconstructValuePlaceholder.Syntax;
		ITypeSymbol type = boundDeconstructValuePlaceholder.Type;
		Optional<object> constantValue = ConvertToOptional(boundDeconstructValuePlaceholder.ConstantValue);
		bool wasCompilerGenerated = boundDeconstructValuePlaceholder.WasCompilerGenerated;
		return new PlaceholderExpression(PlaceholderKind.Unspecified, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IDeconstructionAssignmentOperation CreateBoundDeconstructionAssignmentOperator(BoundDeconstructionAssignmentOperator boundDeconstructionAssignmentOperator)
	{
		Lazy<IOperation> target = new Lazy<IOperation>(() => Create(boundDeconstructionAssignmentOperator.Left));
		Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundDeconstructionAssignmentOperator.Right.Operand));
		SyntaxNode syntax = boundDeconstructionAssignmentOperator.Syntax;
		ITypeSymbol type = boundDeconstructionAssignmentOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundDeconstructionAssignmentOperator.ConstantValue);
		bool wasCompilerGenerated = boundDeconstructionAssignmentOperator.WasCompilerGenerated;
		return new LazyDeconstructionAssignmentExpression(target, value, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IOperation CreateBoundCallOperation(BoundCall boundCall)
	{
		MethodSymbol targetMethod = boundCall.Method;
		SyntaxNode syntax = boundCall.Syntax;
		ITypeSymbol type = boundCall.Type;
		Optional<object> constantValue = ConvertToOptional(boundCall.ConstantValue);
		bool wasCompilerGenerated = boundCall.WasCompilerGenerated;
		if (!boundCall.OriginalMethodsOpt.IsDefault || IsMethodInvalid(boundCall.ResultKind, targetMethod))
		{
			return CreateInvalidExpressionForHasArgumentsExpression(boundCall.ReceiverOpt, boundCall.Arguments, null, syntax, type, constantValue, wasCompilerGenerated);
		}
		Lazy<IOperation> instance = CreateReceiverOperation(boundCall.ReceiverOpt, targetMethod);
		bool isVirtual = IsCallVirtual(targetMethod, boundCall.ReceiverOpt);
		Lazy<ImmutableArray<IArgumentOperation>> arguments = new Lazy<ImmutableArray<IArgumentOperation>>(() => DeriveArguments(boundCall, boundCall.BinderOpt, targetMethod, targetMethod, boundCall.Arguments, boundCall.ArgumentNamesOpt, boundCall.ArgsToParamsOpt, boundCall.ArgumentRefKindsOpt, boundCall.Method.Parameters, boundCall.Expanded, syntax, boundCall.InvokedAsExtensionMethod));
		return new LazyInvocationExpression(targetMethod, instance, isVirtual, arguments, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IOperation CreateBoundLocalOperation(BoundLocal boundLocal)
	{
		ILocalSymbol local = boundLocal.LocalSymbol;
		bool isDeclaration = boundLocal.IsDeclaration;
		SyntaxNode syntax = boundLocal.Syntax;
		ITypeSymbol type = boundLocal.Type;
		Optional<object> constantValue = ConvertToOptional(boundLocal.ConstantValue);
		bool isImplicit = boundLocal.WasCompilerGenerated;
		if (isDeclaration && syntax is DeclarationExpressionSyntax declarationExpressionSyntax)
		{
			syntax = declarationExpressionSyntax.Designation;
			return new LazyDeclarationExpression(new Lazy<IOperation>(() => new LocalReferenceExpression(local, isDeclaration, _semanticModel, syntax, type, constantValue, isImplicit)), _semanticModel, declarationExpressionSyntax, type, default, isImplicit: false);
		}
		return new LocalReferenceExpression(local, isDeclaration, _semanticModel, syntax, type, constantValue, isImplicit);
	}

	private IOperation CreateBoundFieldAccessOperation(BoundFieldAccess boundFieldAccess)
	{
		IFieldSymbol field = boundFieldAccess.FieldSymbol;
		bool isDeclaration = boundFieldAccess.IsDeclaration;
		Lazy<IOperation> instance = CreateReceiverOperation(boundFieldAccess.ReceiverOpt, field);
		SyntaxNode syntax = boundFieldAccess.Syntax;
		ITypeSymbol type = boundFieldAccess.Type;
		Optional<object> constantValue = ConvertToOptional(boundFieldAccess.ConstantValue);
		bool isImplicit = boundFieldAccess.WasCompilerGenerated;
		if (isDeclaration && syntax is DeclarationExpressionSyntax declarationExpressionSyntax)
		{
			syntax = declarationExpressionSyntax.Designation;
			return new LazyDeclarationExpression(new Lazy<IOperation>(() => new LazyFieldReferenceExpression(field, isDeclaration, instance, _semanticModel, syntax, type, constantValue, isImplicit)), _semanticModel, declarationExpressionSyntax, type, default, isImplicit: false);
		}
		return new LazyFieldReferenceExpression(field, isDeclaration, instance, _semanticModel, syntax, type, constantValue, isImplicit);
	}

	private IPropertyReferenceOperation CreateBoundPropertyAccessOperation(BoundPropertyAccess boundPropertyAccess)
	{
		IPropertySymbol propertySymbol = boundPropertyAccess.PropertySymbol;
		Lazy<IOperation> instance = CreateReceiverOperation(boundPropertyAccess.ReceiverOpt, propertySymbol);
		Lazy<ImmutableArray<IArgumentOperation>> arguments = new Lazy<ImmutableArray<IArgumentOperation>>(() => ImmutableArray<IArgumentOperation>.Empty);
		SyntaxNode syntax = boundPropertyAccess.Syntax;
		ITypeSymbol type = boundPropertyAccess.Type;
		Optional<object> constantValue = ConvertToOptional(boundPropertyAccess.ConstantValue);
		bool wasCompilerGenerated = boundPropertyAccess.WasCompilerGenerated;
		return new LazyPropertyReferenceExpression(propertySymbol, instance, arguments, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IOperation CreateBoundIndexerAccessOperation(BoundIndexerAccess boundIndexerAccess)
	{
		PropertySymbol property = boundIndexerAccess.Indexer;
		SyntaxNode syntax = boundIndexerAccess.Syntax;
		ITypeSymbol type = boundIndexerAccess.Type;
		Optional<object> constantValue = ConvertToOptional(boundIndexerAccess.ConstantValue);
		bool wasCompilerGenerated = boundIndexerAccess.WasCompilerGenerated;
		MethodSymbol accessor = (boundIndexerAccess.UseSetterForDefaultArgumentGeneration ? property.GetOwnOrInheritedSetMethod() : property.GetOwnOrInheritedGetMethod());
		if (!boundIndexerAccess.OriginalIndexersOpt.IsDefault || boundIndexerAccess.ResultKind == LookupResultKind.OverloadResolutionFailure || accessor == null || accessor.OriginalDefinition is ErrorMethodSymbol)
		{
			return CreateInvalidExpressionForHasArgumentsExpression(boundIndexerAccess.ReceiverOpt, boundIndexerAccess.Arguments, null, syntax, type, constantValue, wasCompilerGenerated);
		}
		Lazy<IOperation> instance = CreateReceiverOperation(boundIndexerAccess.ReceiverOpt, property);
		Lazy<ImmutableArray<IArgumentOperation>> arguments = new Lazy<ImmutableArray<IArgumentOperation>>(() => DeriveArguments(boundIndexerAccess, boundIndexerAccess.BinderOpt, property, accessor, boundIndexerAccess.Arguments, boundIndexerAccess.ArgumentNamesOpt, boundIndexerAccess.ArgsToParamsOpt, boundIndexerAccess.ArgumentRefKindsOpt, property.Parameters, boundIndexerAccess.Expanded, syntax));
		return new LazyPropertyReferenceExpression(property, instance, arguments, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IEventReferenceOperation CreateBoundEventAccessOperation(BoundEventAccess boundEventAccess)
	{
		IEventSymbol eventSymbol = boundEventAccess.EventSymbol;
		Lazy<IOperation> instance = CreateReceiverOperation(boundEventAccess.ReceiverOpt, eventSymbol);
		SyntaxNode syntax = boundEventAccess.Syntax;
		ITypeSymbol type = boundEventAccess.Type;
		Optional<object> constantValue = ConvertToOptional(boundEventAccess.ConstantValue);
		bool wasCompilerGenerated = boundEventAccess.WasCompilerGenerated;
		return new LazyEventReferenceExpression(eventSymbol, instance, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IEventAssignmentOperation CreateBoundEventAssignmentOperatorOperation(BoundEventAssignmentOperator boundEventAssignmentOperator)
	{
		Lazy<IOperation> eventReference = new Lazy<IOperation>(() => CreateBoundEventAccessOperation(boundEventAssignmentOperator));
		Lazy<IOperation> handlerValue = new Lazy<IOperation>(() => Create(boundEventAssignmentOperator.Argument));
		SyntaxNode syntax = boundEventAssignmentOperator.Syntax;
		bool isAddition = boundEventAssignmentOperator.IsAddition;
		ITypeSymbol type = boundEventAssignmentOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundEventAssignmentOperator.ConstantValue);
		bool wasCompilerGenerated = boundEventAssignmentOperator.WasCompilerGenerated;
		return new LazyEventAssignmentOperation(eventReference, handlerValue, isAddition, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IParameterReferenceOperation CreateBoundParameterOperation(BoundParameter boundParameter)
	{
		return new ParameterReferenceExpression(boundParameter.ParameterSymbol, syntax: boundParameter.Syntax, type: boundParameter.Type, constantValue: ConvertToOptional(boundParameter.ConstantValue), isImplicit: boundParameter.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ILiteralOperation CreateBoundLiteralOperation(BoundLiteral boundLiteral, bool @implicit = false)
	{
		SyntaxNode syntax = boundLiteral.Syntax;
		ITypeSymbol type = boundLiteral.Type;
		Optional<object> constantValue = ConvertToOptional(boundLiteral.ConstantValue);
		bool isImplicit = boundLiteral.WasCompilerGenerated | @implicit;
		return new LiteralExpression(_semanticModel, syntax, type, constantValue, isImplicit);
	}

	private IAnonymousObjectCreationOperation CreateBoundAnonymousObjectCreationExpressionOperation(BoundAnonymousObjectCreationExpression boundAnonymousObjectCreationExpression)
	{
		Lazy<ImmutableArray<IOperation>> initializers = new Lazy<ImmutableArray<IOperation>>(() => GetAnonymousObjectCreationInitializers(boundAnonymousObjectCreationExpression));
		SyntaxNode syntax = boundAnonymousObjectCreationExpression.Syntax;
		ITypeSymbol type = boundAnonymousObjectCreationExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundAnonymousObjectCreationExpression.ConstantValue);
		bool wasCompilerGenerated = boundAnonymousObjectCreationExpression.WasCompilerGenerated;
		return new LazyAnonymousObjectCreationExpression(initializers, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IPropertyReferenceOperation CreateBoundAnonymousPropertyDeclarationOperation(BoundAnonymousPropertyDeclaration boundAnonymousPropertyDeclaration, InstanceReferenceExpression receiver)
	{
		PropertySymbol property = boundAnonymousPropertyDeclaration.Property;
		Lazy<IOperation> instance = new Lazy<IOperation>(() => receiver);
		Lazy<ImmutableArray<IArgumentOperation>> arguments = new Lazy<ImmutableArray<IArgumentOperation>>(() => ImmutableArray<IArgumentOperation>.Empty);
		SyntaxNode syntax = boundAnonymousPropertyDeclaration.Syntax;
		ITypeSymbol type = boundAnonymousPropertyDeclaration.Type;
		Optional<object> constantValue = ConvertToOptional(boundAnonymousPropertyDeclaration.ConstantValue);
		bool wasCompilerGenerated = boundAnonymousPropertyDeclaration.WasCompilerGenerated;
		return new LazyPropertyReferenceExpression(property, instance, arguments, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IOperation CreateBoundObjectCreationExpressionOperation(BoundObjectCreationExpression boundObjectCreationExpression)
	{
		MethodSymbol constructor = boundObjectCreationExpression.Constructor;
		SyntaxNode syntax = boundObjectCreationExpression.Syntax;
		ITypeSymbol type = boundObjectCreationExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundObjectCreationExpression.ConstantValue);
		bool isImplicit = boundObjectCreationExpression.WasCompilerGenerated;
		if (boundObjectCreationExpression.ResultKind == LookupResultKind.OverloadResolutionFailure || constructor == null || constructor.OriginalDefinition is ErrorMethodSymbol)
		{
			return CreateInvalidExpressionForHasArgumentsExpression(null, boundObjectCreationExpression.Arguments, boundObjectCreationExpression.InitializerExpressionOpt, syntax, type, constantValue, isImplicit);
		}
		if (boundObjectCreationExpression.Type.IsAnonymousType)
		{
			return new LazyAnonymousObjectCreationExpression(new Lazy<ImmutableArray<IOperation>>(() => GetAnonymousObjectCreationInitializers(boundObjectCreationExpression.Arguments, ImmutableArray<BoundAnonymousPropertyDeclaration>.Empty, syntax, type, isImplicit)), _semanticModel, syntax, type, constantValue, isImplicit);
		}
		Lazy<IObjectOrCollectionInitializerOperation> initializer = new Lazy<IObjectOrCollectionInitializerOperation>(() => (IObjectOrCollectionInitializerOperation)Create(boundObjectCreationExpression.InitializerExpressionOpt));
		Lazy<ImmutableArray<IArgumentOperation>> arguments = new Lazy<ImmutableArray<IArgumentOperation>>(() => DeriveArguments(boundObjectCreationExpression, boundObjectCreationExpression.BinderOpt, constructor, constructor, boundObjectCreationExpression.Arguments, boundObjectCreationExpression.ArgumentNamesOpt, boundObjectCreationExpression.ArgsToParamsOpt, boundObjectCreationExpression.ArgumentRefKindsOpt, constructor.Parameters, boundObjectCreationExpression.Expanded, syntax));
		return new LazyObjectCreationExpression(constructor, initializer, arguments, _semanticModel, syntax, type, constantValue, isImplicit);
	}

	private IDynamicObjectCreationOperation CreateBoundDynamicObjectCreationExpressionOperation(BoundDynamicObjectCreationExpression boundDynamicObjectCreationExpression)
	{
		Lazy<ImmutableArray<IOperation>> arguments = new Lazy<ImmutableArray<IOperation>>(() => boundDynamicObjectCreationExpression.Arguments.SelectAsArray((BoundExpression n) => Create(n)));
		ImmutableArray<string> argumentNames = boundDynamicObjectCreationExpression.ArgumentNamesOpt.NullToEmpty();
		ImmutableArray<RefKind> argumentRefKinds = boundDynamicObjectCreationExpression.ArgumentRefKindsOpt.NullToEmpty();
		Lazy<IObjectOrCollectionInitializerOperation> initializer = new Lazy<IObjectOrCollectionInitializerOperation>(() => (IObjectOrCollectionInitializerOperation)Create(boundDynamicObjectCreationExpression.InitializerExpressionOpt));
		SyntaxNode syntax = boundDynamicObjectCreationExpression.Syntax;
		ITypeSymbol type = boundDynamicObjectCreationExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundDynamicObjectCreationExpression.ConstantValue);
		bool wasCompilerGenerated = boundDynamicObjectCreationExpression.WasCompilerGenerated;
		return new LazyDynamicObjectCreationExpression(arguments, argumentNames, argumentRefKinds, initializer, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IDynamicInvocationOperation CreateBoundDynamicInvocationExpressionOperation(BoundDynamicInvocation boundDynamicInvocation)
	{
		Lazy<IOperation> operation;
		if (boundDynamicInvocation.Expression.Kind == BoundKind.MethodGroup)
		{
			BoundMethodGroup methodGroup = (BoundMethodGroup)boundDynamicInvocation.Expression;
			operation = new Lazy<IOperation>(() => CreateBoundDynamicMemberAccessOperation(methodGroup.ReceiverOpt, methodGroup.TypeArgumentsOpt, methodGroup.Name, methodGroup.Syntax, methodGroup.Type, methodGroup.ConstantValue, methodGroup.WasCompilerGenerated));
		}
		else
		{
			operation = new Lazy<IOperation>(() => Create(boundDynamicInvocation.Expression));
		}
		Lazy<ImmutableArray<IOperation>> arguments = new Lazy<ImmutableArray<IOperation>>(() => boundDynamicInvocation.Arguments.SelectAsArray((BoundExpression n) => Create(n)));
		ImmutableArray<string> argumentNames = boundDynamicInvocation.ArgumentNamesOpt.NullToEmpty();
		ImmutableArray<RefKind> argumentRefKinds = boundDynamicInvocation.ArgumentRefKindsOpt.NullToEmpty();
		SyntaxNode syntax = boundDynamicInvocation.Syntax;
		ITypeSymbol type = boundDynamicInvocation.Type;
		Optional<object> constantValue = ConvertToOptional(boundDynamicInvocation.ConstantValue);
		bool wasCompilerGenerated = boundDynamicInvocation.WasCompilerGenerated;
		return new LazyDynamicInvocationExpression(operation, arguments, argumentNames, argumentRefKinds, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IDynamicIndexerAccessOperation CreateBoundDynamicIndexerAccessExpressionOperation(BoundDynamicIndexerAccess boundDynamicIndexerAccess)
	{
		Lazy<IOperation> operation = new Lazy<IOperation>(() => Create(boundDynamicIndexerAccess.ReceiverOpt));
		Lazy<ImmutableArray<IOperation>> arguments = new Lazy<ImmutableArray<IOperation>>(() => boundDynamicIndexerAccess.Arguments.SelectAsArray((BoundExpression n) => Create(n)));
		ImmutableArray<string> argumentNames = boundDynamicIndexerAccess.ArgumentNamesOpt.NullToEmpty();
		ImmutableArray<RefKind> argumentRefKinds = boundDynamicIndexerAccess.ArgumentRefKindsOpt.NullToEmpty();
		SyntaxNode syntax = boundDynamicIndexerAccess.Syntax;
		ITypeSymbol type = boundDynamicIndexerAccess.Type;
		Optional<object> constantValue = ConvertToOptional(boundDynamicIndexerAccess.ConstantValue);
		bool wasCompilerGenerated = boundDynamicIndexerAccess.WasCompilerGenerated;
		return new LazyDynamicIndexerAccessExpression(operation, arguments, argumentNames, argumentRefKinds, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IObjectOrCollectionInitializerOperation CreateBoundObjectInitializerExpressionOperation(BoundObjectInitializerExpression boundObjectInitializerExpression)
	{
		Lazy<ImmutableArray<IOperation>> initializers = new Lazy<ImmutableArray<IOperation>>(() => BoundObjectCreationExpression.GetChildInitializers(boundObjectInitializerExpression).SelectAsArray((BoundExpression n) => Create(n)));
		SyntaxNode syntax = boundObjectInitializerExpression.Syntax;
		ITypeSymbol type = boundObjectInitializerExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundObjectInitializerExpression.ConstantValue);
		bool wasCompilerGenerated = boundObjectInitializerExpression.WasCompilerGenerated;
		return new LazyObjectOrCollectionInitializerExpression(initializers, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IObjectOrCollectionInitializerOperation CreateBoundCollectionInitializerExpressionOperation(BoundCollectionInitializerExpression boundCollectionInitializerExpression)
	{
		Lazy<ImmutableArray<IOperation>> initializers = new Lazy<ImmutableArray<IOperation>>(() => BoundObjectCreationExpression.GetChildInitializers(boundCollectionInitializerExpression).SelectAsArray((BoundExpression n) => Create(n)));
		SyntaxNode syntax = boundCollectionInitializerExpression.Syntax;
		ITypeSymbol type = boundCollectionInitializerExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundCollectionInitializerExpression.ConstantValue);
		bool wasCompilerGenerated = boundCollectionInitializerExpression.WasCompilerGenerated;
		return new LazyObjectOrCollectionInitializerExpression(initializers, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IOperation CreateBoundObjectInitializerMemberOperation(BoundObjectInitializerMember boundObjectInitializerMember, bool isObjectOrCollectionInitializer = false)
	{
		Symbol memberSymbol = boundObjectInitializerMember.MemberSymbol;
		Lazy<IOperation> lazy = (((object)memberSymbol != null && memberSymbol.IsStatic) ? OperationFactory.NullOperation : new Lazy<IOperation>(() => CreateImplicitReciever(boundObjectInitializerMember.Syntax, boundObjectInitializerMember.ReceiverType)));
		SyntaxNode syntax = boundObjectInitializerMember.Syntax;
		ITypeSymbol type = boundObjectInitializerMember.Type;
		Optional<object> constantValue = ConvertToOptional(boundObjectInitializerMember.ConstantValue);
		bool wasCompilerGenerated = boundObjectInitializerMember.WasCompilerGenerated;
		if ((object)memberSymbol == null)
		{
			Lazy<ImmutableArray<IOperation>> arguments = new Lazy<ImmutableArray<IOperation>>(() => boundObjectInitializerMember.Arguments.SelectAsArray((BoundExpression n) => Create(n)));
			ImmutableArray<string> argumentNames = boundObjectInitializerMember.ArgumentNamesOpt.NullToEmpty();
			ImmutableArray<RefKind> argumentRefKinds = boundObjectInitializerMember.ArgumentRefKindsOpt.NullToEmpty();
			return new LazyDynamicIndexerAccessExpression(lazy, arguments, argumentNames, argumentRefKinds, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
		}
		switch (memberSymbol.Kind)
		{
		case SymbolKind.Field:
		{
			FieldSymbol field = (FieldSymbol)memberSymbol;
			bool isDeclaration = false;
			return new LazyFieldReferenceExpression(field, isDeclaration, lazy, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
		}
		case SymbolKind.Event:
			return new LazyEventReferenceExpression((EventSymbol)memberSymbol, lazy, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
		case SymbolKind.Property:
		{
			PropertySymbol property = (PropertySymbol)memberSymbol;
			Lazy<ImmutableArray<IArgumentOperation>> arguments2;
			if (!boundObjectInitializerMember.Arguments.Any())
			{
				arguments2 = new Lazy<ImmutableArray<IArgumentOperation>>(() => ImmutableArray<IArgumentOperation>.Empty);
			}
			else
			{
				MethodSymbol accessor = (isObjectOrCollectionInitializer ? property.GetOwnOrInheritedGetMethod() : property.GetOwnOrInheritedSetMethod());
				if (accessor == null || boundObjectInitializerMember.ResultKind == LookupResultKind.OverloadResolutionFailure || accessor.OriginalDefinition is ErrorMethodSymbol)
				{
					return new LazyInvalidOperation(new Lazy<ImmutableArray<IOperation>>(() => boundObjectInitializerMember.Arguments.SelectAsArray((BoundExpression a) => Create(a))), _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
				}
				arguments2 = new Lazy<ImmutableArray<IArgumentOperation>>(() => DeriveArguments(boundObjectInitializerMember, boundObjectInitializerMember.BinderOpt, property, accessor, boundObjectInitializerMember.Arguments, boundObjectInitializerMember.ArgumentNamesOpt, boundObjectInitializerMember.ArgsToParamsOpt, boundObjectInitializerMember.ArgumentRefKindsOpt, property.Parameters, boundObjectInitializerMember.Expanded, boundObjectInitializerMember.Syntax));
			}
			return new LazyPropertyReferenceExpression(property, lazy, arguments2, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
		}
		default:
			throw ExceptionUtilities.Unreachable;
		}
	}

	private IOperation CreateBoundDynamicObjectInitializerMemberOperation(BoundDynamicObjectInitializerMember boundDynamicObjectInitializerMember)
	{
		Lazy<IOperation> lazyInstance = new Lazy<IOperation>(() => CreateImplicitReciever(boundDynamicObjectInitializerMember.Syntax, boundDynamicObjectInitializerMember.ReceiverType));
		string memberName = boundDynamicObjectInitializerMember.MemberName;
		ImmutableArray<ITypeSymbol> empty = ImmutableArray<ITypeSymbol>.Empty;
		ITypeSymbol receiverType = boundDynamicObjectInitializerMember.ReceiverType;
		SyntaxNode syntax = boundDynamicObjectInitializerMember.Syntax;
		ITypeSymbol type = boundDynamicObjectInitializerMember.Type;
		Optional<object> constantValue = ConvertToOptional(boundDynamicObjectInitializerMember.ConstantValue);
		bool wasCompilerGenerated = boundDynamicObjectInitializerMember.WasCompilerGenerated;
		return new LazyDynamicMemberReferenceExpression(lazyInstance, memberName, empty, receiverType, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IOperation CreateBoundCollectionElementInitializerOperation(BoundCollectionElementInitializer boundCollectionElementInitializer)
	{
		MethodSymbol addMethod = boundCollectionElementInitializer.AddMethod;
		SyntaxNode syntax = boundCollectionElementInitializer.Syntax;
		ITypeSymbol type = boundCollectionElementInitializer.Type;
		Optional<object> constantValue = ConvertToOptional(boundCollectionElementInitializer.ConstantValue);
		bool wasCompilerGenerated = boundCollectionElementInitializer.WasCompilerGenerated;
		if (IsMethodInvalid(boundCollectionElementInitializer.ResultKind, addMethod))
		{
			return CreateInvalidExpressionForHasArgumentsExpression(boundCollectionElementInitializer.ImplicitReceiverOpt, boundCollectionElementInitializer.Arguments, null, syntax, type, constantValue, wasCompilerGenerated);
		}
		Lazy<IOperation> instance = CreateReceiverOperation(boundCollectionElementInitializer.ImplicitReceiverOpt, addMethod);
		Lazy<ImmutableArray<IArgumentOperation>> arguments = new Lazy<ImmutableArray<IArgumentOperation>>(() => DeriveArguments(boundCollectionElementInitializer, boundCollectionElementInitializer.BinderOpt, boundCollectionElementInitializer.AddMethod, boundCollectionElementInitializer.AddMethod, boundCollectionElementInitializer.Arguments, default, boundCollectionElementInitializer.ArgsToParamsOpt, default, boundCollectionElementInitializer.AddMethod.Parameters, boundCollectionElementInitializer.Expanded, boundCollectionElementInitializer.Syntax, boundCollectionElementInitializer.InvokedAsExtensionMethod));
		bool isVirtual = IsCallVirtual(addMethod, boundCollectionElementInitializer.ImplicitReceiverOpt);
		return new LazyInvocationExpression(addMethod, instance, isVirtual, arguments, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IDynamicMemberReferenceOperation CreateBoundDynamicMemberAccessOperation(BoundDynamicMemberAccess boundDynamicMemberAccess)
	{
		return CreateBoundDynamicMemberAccessOperation(boundDynamicMemberAccess.Receiver, boundDynamicMemberAccess.TypeArgumentsOpt, boundDynamicMemberAccess.Name, boundDynamicMemberAccess.Syntax, boundDynamicMemberAccess.Type, boundDynamicMemberAccess.ConstantValue, boundDynamicMemberAccess.WasCompilerGenerated);
	}

	private IDynamicMemberReferenceOperation CreateBoundDynamicMemberAccessOperation(BoundExpression receiverOpt, ImmutableArray<TypeSymbol> typeArgumentsOpt, string memberName, SyntaxNode syntaxNode, ITypeSymbol type, ConstantValue value, bool isImplicit)
	{
		Lazy<IOperation> lazyInstance = ((receiverOpt == null || receiverOpt.Kind == BoundKind.TypeExpression) ? OperationFactory.NullOperation : new Lazy<IOperation>(() => Create(receiverOpt)));
		ImmutableArray<ITypeSymbol> typeArguments = ImmutableArray<ITypeSymbol>.Empty;
		if (!typeArgumentsOpt.IsDefault)
		{
			typeArguments = ImmutableArray<ITypeSymbol>.CastUp(typeArgumentsOpt);
		}
		BoundExpression boundExpression = receiverOpt;
		return new LazyDynamicMemberReferenceExpression(containingType: (boundExpression != null && boundExpression.Kind == BoundKind.TypeExpression) ? receiverOpt.Type : null, constantValue: ConvertToOptional(value), lazyInstance: lazyInstance, memberName: memberName, typeArguments: typeArguments, semanticModel: _semanticModel, syntax: syntaxNode, type: type, isImplicit: isImplicit);
	}

	private IDynamicInvocationOperation CreateBoundDynamicCollectionElementInitializerOperation(BoundDynamicCollectionElementInitializer boundCollectionElementInitializer)
	{
		new Lazy<IOperation>(() => Create(boundCollectionElementInitializer.ImplicitReceiver));
		Lazy<ImmutableArray<IOperation>> arguments = new Lazy<ImmutableArray<IOperation>>(() => boundCollectionElementInitializer.Arguments.SelectAsArray((BoundExpression n) => Create(n)));
		SyntaxNode syntax = boundCollectionElementInitializer.Syntax;
		ITypeSymbol type = boundCollectionElementInitializer.Type;
		Optional<object> constantValue = ConvertToOptional(boundCollectionElementInitializer.ConstantValue);
		bool wasCompilerGenerated = boundCollectionElementInitializer.WasCompilerGenerated;
		BoundImplicitReceiver implicitReceiver = boundCollectionElementInitializer.ImplicitReceiver;
		return new LazyDynamicInvocationExpression(new Lazy<IOperation>(() => CreateBoundDynamicMemberAccessOperation(implicitReceiver, ImmutableArray<TypeSymbol>.Empty, "Add", implicitReceiver.Syntax, null, null, isImplicit: true)), arguments, ImmutableArray<string>.Empty, ImmutableArray<RefKind>.Empty, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IOperation CreateUnboundLambdaOperation(UnboundLambda unboundLambda)
	{
		BoundLambda boundNode = unboundLambda.BindForErrorRecovery();
		return CreateInternal(boundNode);
	}

	private IAnonymousFunctionOperation CreateBoundLambdaOperation(BoundLambda boundLambda)
	{
		IMethodSymbol symbol = boundLambda.Symbol;
		Lazy<IBlockOperation> body = new Lazy<IBlockOperation>(() => (IBlockOperation)Create(boundLambda.Body));
		SyntaxNode syntax = boundLambda.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = ConvertToOptional(boundLambda.ConstantValue);
		bool wasCompilerGenerated = boundLambda.WasCompilerGenerated;
		return new LazyAnonymousFunctionExpression(symbol, body, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ILocalFunctionOperation CreateBoundLocalFunctionStatementOperation(BoundLocalFunctionStatement boundLocalFunctionStatement)
	{
		IMethodSymbol symbol = boundLocalFunctionStatement.Symbol;
		Lazy<IBlockOperation> body = new Lazy<IBlockOperation>(() => (IBlockOperation)Create(boundLocalFunctionStatement.Body));
		Lazy<IBlockOperation> ignoredBody = new Lazy<IBlockOperation>(() => (boundLocalFunctionStatement.BlockBody == null || boundLocalFunctionStatement.ExpressionBody == null) ? null : ((IBlockOperation)Create(boundLocalFunctionStatement.ExpressionBody)));
		SyntaxNode syntax = boundLocalFunctionStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundLocalFunctionStatement.WasCompilerGenerated;
		return new LazyLocalFunctionStatement(symbol, body, ignoredBody, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IOperation CreateBoundConversionOperation(BoundConversion boundConversion)
	{
		bool isImplicit = boundConversion.WasCompilerGenerated || !boundConversion.ExplicitCastInCode;
		BoundExpression boundOperand = boundConversion.Operand;
		if (boundConversion.ConversionKind == ConversionKind.MethodGroup)
		{
			return new LazyDelegateCreationExpression(new Lazy<IOperation>(() => CreateBoundMethodGroupSingleMethodOperation((BoundMethodGroup)boundOperand, boundConversion.SymbolOpt, boundConversion.SuppressVirtualCalls)), syntax: boundConversion.Syntax, type: boundConversion.Type, constantValue: ConvertToOptional(boundConversion.ConstantValue), semanticModel: _semanticModel, isImplicit: isImplicit);
		}
		SyntaxNode syntax = boundConversion.Syntax;
		if (syntax.IsMissing)
		{
			return Create(boundOperand);
		}
		Lazy<IOperation> lazy = null;
		Conversion conversion = boundConversion.Conversion;
		if (boundOperand.Syntax == boundConversion.Syntax)
		{
			if (boundOperand.Kind == BoundKind.ConvertedTupleLiteral && boundOperand.Type == boundConversion.Type)
			{
				return Create(boundOperand);
			}
			isImplicit = true;
		}
		if (boundConversion.ExplicitCastInCode && conversion.IsIdentity && boundOperand.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion2 = (BoundConversion)boundOperand;
			BoundExpression nestedOperand = boundConversion2.Operand;
			if (boundConversion2.Syntax == nestedOperand.Syntax && boundConversion2.ExplicitCastInCode && nestedOperand.Kind == BoundKind.ConvertedTupleLiteral && boundConversion2.Type != nestedOperand.Type)
			{
				conversion = boundConversion2.Conversion;
				lazy = new Lazy<IOperation>(() => Create(nestedOperand));
			}
		}
		if (lazy == null)
		{
			lazy = new Lazy<IOperation>(() => Create(boundOperand));
		}
		ITypeSymbol type = boundConversion.Type;
		Optional<object> constantValue = ConvertToOptional(boundConversion.ConstantValue);
		if ((boundOperand.Kind == BoundKind.Lambda || boundOperand.Kind == BoundKind.UnboundLambda || boundOperand.Kind == BoundKind.MethodGroup) && boundConversion.Type.IsDelegateType())
		{
			return new LazyDelegateCreationExpression(lazy, _semanticModel, syntax, type, constantValue, isImplicit);
		}
		bool isTryCast = false;
		bool isChecked = conversion.IsNumeric && boundConversion.Checked;
		return new LazyConversionOperation(lazy, conversion, isTryCast, isChecked, _semanticModel, syntax, type, constantValue, isImplicit);
	}

	private IConversionOperation CreateBoundAsOperatorOperation(BoundAsOperator boundAsOperator)
	{
		Lazy<IOperation> lazyOperand = new Lazy<IOperation>(() => Create(boundAsOperator.Operand));
		SyntaxNode syntax = boundAsOperator.Syntax;
		Conversion conversion = boundAsOperator.Conversion;
		bool isTryCast = true;
		bool isChecked = false;
		ITypeSymbol type = boundAsOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundAsOperator.ConstantValue);
		bool wasCompilerGenerated = boundAsOperator.WasCompilerGenerated;
		return new LazyConversionOperation(lazyOperand, conversion, isTryCast, isChecked, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IDelegateCreationOperation CreateBoundDelegateCreationExpressionOperation(BoundDelegateCreationExpression boundDelegateCreationExpression)
	{
		Lazy<IOperation> lazyTarget = new Lazy<IOperation>(() =>
		{
			if (boundDelegateCreationExpression.Argument.Kind == BoundKind.MethodGroup && boundDelegateCreationExpression.MethodOpt != null)
			{
				BoundMethodGroup boundMethodGroup = (BoundMethodGroup)boundDelegateCreationExpression.Argument;
				return CreateBoundMethodGroupSingleMethodOperation(boundMethodGroup, boundDelegateCreationExpression.MethodOpt, boundMethodGroup.SuppressVirtualCalls);
			}
			return Create(boundDelegateCreationExpression.Argument);
		});
		SyntaxNode syntax = boundDelegateCreationExpression.Syntax;
		ITypeSymbol type = boundDelegateCreationExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundDelegateCreationExpression.ConstantValue);
		bool wasCompilerGenerated = boundDelegateCreationExpression.WasCompilerGenerated;
		return new LazyDelegateCreationExpression(lazyTarget, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IMethodReferenceOperation CreateBoundMethodGroupSingleMethodOperation(BoundMethodGroup boundMethodGroup, IMethodSymbol methodSymbol, bool suppressVirtualCalls)
	{
		bool isVirtual = (methodSymbol.IsAbstract || methodSymbol.IsOverride || methodSymbol.IsVirtual) && !suppressVirtualCalls;
		Lazy<IOperation> instance = CreateReceiverOperation(boundMethodGroup.ReceiverOpt, methodSymbol);
		SyntaxNode syntax = boundMethodGroup.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = ConvertToOptional(boundMethodGroup.ConstantValue);
		_ = boundMethodGroup.WasCompilerGenerated;
		return new LazyMethodReferenceExpression(methodSymbol, isVirtual, instance, _semanticModel, syntax, type, constantValue, boundMethodGroup.WasCompilerGenerated);
	}

	private IIsTypeOperation CreateBoundIsOperatorOperation(BoundIsOperator boundIsOperator)
	{
		Lazy<IOperation> operand = new Lazy<IOperation>(() => Create(boundIsOperator.Operand));
		ITypeSymbol type = boundIsOperator.TargetType.Type;
		SyntaxNode syntax = boundIsOperator.Syntax;
		ITypeSymbol type2 = boundIsOperator.Type;
		bool isNotTypeExpression = false;
		Optional<object> constantValue = ConvertToOptional(boundIsOperator.ConstantValue);
		bool wasCompilerGenerated = boundIsOperator.WasCompilerGenerated;
		return new LazyIsTypeExpression(operand, type, isNotTypeExpression, _semanticModel, syntax, type2, constantValue, wasCompilerGenerated);
	}

	private ISizeOfOperation CreateBoundSizeOfOperatorOperation(BoundSizeOfOperator boundSizeOfOperator)
	{
		return new SizeOfExpression(boundSizeOfOperator.SourceType.Type, syntax: boundSizeOfOperator.Syntax, type: boundSizeOfOperator.Type, constantValue: ConvertToOptional(boundSizeOfOperator.ConstantValue), isImplicit: boundSizeOfOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ITypeOfOperation CreateBoundTypeOfOperatorOperation(BoundTypeOfOperator boundTypeOfOperator)
	{
		return new TypeOfExpression(boundTypeOfOperator.SourceType.Type, syntax: boundTypeOfOperator.Syntax, type: boundTypeOfOperator.Type, constantValue: ConvertToOptional(boundTypeOfOperator.ConstantValue), isImplicit: boundTypeOfOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IArrayCreationOperation CreateBoundArrayCreationOperation(BoundArrayCreation boundArrayCreation)
	{
		Lazy<ImmutableArray<IOperation>> dimensionSizes = new Lazy<ImmutableArray<IOperation>>(() => boundArrayCreation.Bounds.SelectAsArray((BoundExpression n) => Create(n)));
		Lazy<IArrayInitializerOperation> initializer = new Lazy<IArrayInitializerOperation>(() => (IArrayInitializerOperation)Create(boundArrayCreation.InitializerOpt));
		SyntaxNode syntax = boundArrayCreation.Syntax;
		return new LazyArrayCreationExpression(type: boundArrayCreation.Type, constantValue: ConvertToOptional(boundArrayCreation.ConstantValue), isImplicit: boundArrayCreation.WasCompilerGenerated || (boundArrayCreation.InitializerOpt?.Syntax == syntax && !boundArrayCreation.InitializerOpt.WasCompilerGenerated), dimensionSizes: dimensionSizes, initializer: initializer, semanticModel: _semanticModel, syntax: syntax);
	}

	private IArrayInitializerOperation CreateBoundArrayInitializationOperation(BoundArrayInitialization boundArrayInitialization)
	{
		Lazy<ImmutableArray<IOperation>> elementValues = new Lazy<ImmutableArray<IOperation>>(() => boundArrayInitialization.Initializers.SelectAsArray((BoundExpression n) => Create(n)));
		SyntaxNode syntax = boundArrayInitialization.Syntax;
		Optional<object> constantValue = ConvertToOptional(boundArrayInitialization.ConstantValue);
		bool wasCompilerGenerated = boundArrayInitialization.WasCompilerGenerated;
		return new LazyArrayInitializer(elementValues, _semanticModel, syntax, constantValue, wasCompilerGenerated);
	}

	private IDefaultValueOperation CreateBoundDefaultExpressionOperation(BoundDefaultExpression boundDefaultExpression)
	{
		SyntaxNode syntax = boundDefaultExpression.Syntax;
		ITypeSymbol type = boundDefaultExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundDefaultExpression.ConstantValue);
		bool wasCompilerGenerated = boundDefaultExpression.WasCompilerGenerated;
		return new DefaultValueExpression(_semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IInstanceReferenceOperation CreateBoundBaseReferenceOperation(BoundBaseReference boundBaseReference)
	{
		return new InstanceReferenceExpression(InstanceReferenceKind.ContainingTypeInstance, syntax: boundBaseReference.Syntax, type: boundBaseReference.Type, constantValue: ConvertToOptional(boundBaseReference.ConstantValue), isImplicit: boundBaseReference.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IInstanceReferenceOperation CreateBoundThisReferenceOperation(BoundThisReference boundThisReference)
	{
		return new InstanceReferenceExpression(InstanceReferenceKind.ContainingTypeInstance, syntax: boundThisReference.Syntax, type: boundThisReference.Type, constantValue: ConvertToOptional(boundThisReference.ConstantValue), isImplicit: boundThisReference.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundAssignmentOperatorOrMemberInitializerOperation(BoundAssignmentOperator boundAssignmentOperator)
	{
		if (!IsMemberInitializer(boundAssignmentOperator))
		{
			return CreateBoundAssignmentOperatorOperation(boundAssignmentOperator);
		}
		return CreateBoundMemberInitializerOperation(boundAssignmentOperator);
	}

	private static bool IsMemberInitializer(BoundAssignmentOperator boundAssignmentOperator)
	{
		BoundExpression right = boundAssignmentOperator.Right;
		if (right == null || right.Kind != BoundKind.ObjectInitializerExpression)
		{
			BoundExpression right2 = boundAssignmentOperator.Right;
			if (right2 == null)
			{
				return false;
			}
			return right2.Kind == BoundKind.CollectionInitializerExpression;
		}
		return true;
	}

	private ISimpleAssignmentOperation CreateBoundAssignmentOperatorOperation(BoundAssignmentOperator boundAssignmentOperator)
	{
		Lazy<IOperation> target = new Lazy<IOperation>(() => Create(boundAssignmentOperator.Left));
		bool isRef = boundAssignmentOperator.IsRef;
		Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundAssignmentOperator.Right));
		SyntaxNode syntax = boundAssignmentOperator.Syntax;
		ITypeSymbol type = boundAssignmentOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundAssignmentOperator.ConstantValue);
		bool wasCompilerGenerated = boundAssignmentOperator.WasCompilerGenerated;
		return new LazySimpleAssignmentExpression(target, isRef, value, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IMemberInitializerOperation CreateBoundMemberInitializerOperation(BoundAssignmentOperator boundAssignmentOperator)
	{
		Lazy<IOperation> initializedMember = new Lazy<IOperation>(() => boundAssignmentOperator.Left.Kind switch
		{
			BoundKind.ObjectInitializerMember => _cache.GetOrAdd(boundAssignmentOperator.Left, (BoundNode key) => CreateBoundObjectInitializerMemberOperation((BoundObjectInitializerMember)key, isObjectOrCollectionInitializer: true)), 
			BoundKind.DynamicObjectInitializerMember => _cache.GetOrAdd(boundAssignmentOperator.Left, (BoundNode key) => CreateBoundDynamicObjectInitializerMemberOperation((BoundDynamicObjectInitializerMember)key)), 
			_ => Create(boundAssignmentOperator.Left), 
		});
		Lazy<IObjectOrCollectionInitializerOperation> initializer = new Lazy<IObjectOrCollectionInitializerOperation>(() => (IObjectOrCollectionInitializerOperation)Create(boundAssignmentOperator.Right));
		SyntaxNode syntax = boundAssignmentOperator.Syntax;
		ITypeSymbol type = boundAssignmentOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundAssignmentOperator.ConstantValue);
		bool wasCompilerGenerated = boundAssignmentOperator.WasCompilerGenerated;
		return new LazyMemberInitializerExpression(initializedMember, initializer, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ICompoundAssignmentOperation CreateBoundCompoundAssignmentOperatorOperation(BoundCompoundAssignmentOperator boundCompoundAssignmentOperator)
	{
		BinaryOperatorKind operatorKind = Helper.DeriveBinaryOperatorKind(boundCompoundAssignmentOperator.Operator.Kind);
		Lazy<IOperation> target = new Lazy<IOperation>(() => Create(boundCompoundAssignmentOperator.Left));
		Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundCompoundAssignmentOperator.Right));
		Conversion leftConversion = boundCompoundAssignmentOperator.LeftConversion;
		Conversion finalConversion = boundCompoundAssignmentOperator.FinalConversion;
		bool isLifted = boundCompoundAssignmentOperator.Operator.Kind.IsLifted();
		bool isChecked = boundCompoundAssignmentOperator.Operator.Kind.IsChecked();
		IMethodSymbol method = boundCompoundAssignmentOperator.Operator.Method;
		SyntaxNode syntax = boundCompoundAssignmentOperator.Syntax;
		ITypeSymbol type = boundCompoundAssignmentOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundCompoundAssignmentOperator.ConstantValue);
		bool wasCompilerGenerated = boundCompoundAssignmentOperator.WasCompilerGenerated;
		return new LazyCompoundAssignmentOperation(target, value, leftConversion, finalConversion, operatorKind, isLifted, isChecked, method, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IIncrementOrDecrementOperation CreateBoundIncrementOperatorOperation(BoundIncrementOperator boundIncrementOperator)
	{
		bool isDecrement = Helper.IsDecrement(boundIncrementOperator.OperatorKind);
		bool isPostfix = Helper.IsPostfixIncrementOrDecrement(boundIncrementOperator.OperatorKind);
		bool isLifted = boundIncrementOperator.OperatorKind.IsLifted();
		bool isChecked = boundIncrementOperator.OperatorKind.IsChecked();
		Lazy<IOperation> target = new Lazy<IOperation>(() => Create(boundIncrementOperator.Operand));
		IMethodSymbol methodOpt = boundIncrementOperator.MethodOpt;
		SyntaxNode syntax = boundIncrementOperator.Syntax;
		ITypeSymbol type = boundIncrementOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundIncrementOperator.ConstantValue);
		bool wasCompilerGenerated = boundIncrementOperator.WasCompilerGenerated;
		return new LazyIncrementExpression(isDecrement, isPostfix, isLifted, isChecked, target, methodOpt, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IInvalidOperation CreateBoundBadExpressionOperation(BoundBadExpression boundBadExpression)
	{
		Lazy<ImmutableArray<IOperation>> children = new Lazy<ImmutableArray<IOperation>>(() => boundBadExpression.ChildBoundNodes.Select((BoundExpression n) => Create(n)).WhereNotNull().ToImmutableArray());
		SyntaxNode syntax = boundBadExpression.Syntax;
		return new LazyInvalidOperation(type: syntax.IsMissing ? null : boundBadExpression.Type, constantValue: ConvertToOptional(boundBadExpression.ConstantValue), isImplicit: boundBadExpression.WasCompilerGenerated || boundBadExpression.ChildBoundNodes.Any((BoundExpression e) => e?.Syntax == boundBadExpression.Syntax), children: children, semanticModel: _semanticModel, syntax: syntax);
	}

	private ITypeParameterObjectCreationOperation CreateBoundNewTOperation(BoundNewT boundNewT)
	{
		Lazy<IObjectOrCollectionInitializerOperation> initializer = new Lazy<IObjectOrCollectionInitializerOperation>(() => (IObjectOrCollectionInitializerOperation)Create(boundNewT.InitializerExpressionOpt));
		SyntaxNode syntax = boundNewT.Syntax;
		ITypeSymbol type = boundNewT.Type;
		Optional<object> constantValue = ConvertToOptional(boundNewT.ConstantValue);
		bool wasCompilerGenerated = boundNewT.WasCompilerGenerated;
		return new LazyTypeParameterObjectCreationExpression(initializer, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private INoPiaObjectCreationOperation CreateNoPiaObjectCreationExpressionOperation(BoundNoPiaObjectCreationExpression creation)
	{
		Lazy<IObjectOrCollectionInitializerOperation> initializer = new Lazy<IObjectOrCollectionInitializerOperation>(() => (IObjectOrCollectionInitializerOperation)Create(creation.InitializerExpressionOpt));
		SyntaxNode syntax = creation.Syntax;
		ITypeSymbol type = creation.Type;
		Optional<object> constantValue = ConvertToOptional(creation.ConstantValue);
		bool wasCompilerGenerated = creation.WasCompilerGenerated;
		return new LazyNoPiaObjectCreationOperation(initializer, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IUnaryOperation CreateBoundUnaryOperatorOperation(BoundUnaryOperator boundUnaryOperator)
	{
		UnaryOperatorKind unaryOperationKind = Helper.DeriveUnaryOperatorKind(boundUnaryOperator.OperatorKind);
		Lazy<IOperation> operand = new Lazy<IOperation>(() => Create(boundUnaryOperator.Operand));
		IMethodSymbol methodOpt = boundUnaryOperator.MethodOpt;
		SyntaxNode syntax = boundUnaryOperator.Syntax;
		ITypeSymbol type = boundUnaryOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundUnaryOperator.ConstantValue);
		bool isLifted = boundUnaryOperator.OperatorKind.IsLifted();
		bool isChecked = boundUnaryOperator.OperatorKind.IsChecked();
		bool wasCompilerGenerated = boundUnaryOperator.WasCompilerGenerated;
		return new LazyUnaryOperatorExpression(unaryOperationKind, operand, isLifted, isChecked, methodOpt, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IBinaryOperation CreateBoundBinaryOperatorOperation(BoundBinaryOperator boundBinaryOperator)
	{
		BinaryOperatorKind binaryOperatorKind = Helper.DeriveBinaryOperatorKind(boundBinaryOperator.OperatorKind);
		Lazy<IOperation> leftOperand = new Lazy<IOperation>(() => Create(boundBinaryOperator.Left));
		Lazy<IOperation> rightOperand = new Lazy<IOperation>(() => Create(boundBinaryOperator.Right));
		IMethodSymbol methodSymbol = boundBinaryOperator.MethodOpt;
		IMethodSymbol unaryOperatorMethod = null;
		if (boundBinaryOperator.Type.IsDynamic() && (binaryOperatorKind == BinaryOperatorKind.ConditionalAnd || binaryOperatorKind == BinaryOperatorKind.ConditionalOr) && methodSymbol != null && methodSymbol.Parameters.Length == 1)
		{
			unaryOperatorMethod = methodSymbol;
			methodSymbol = null;
		}
		SyntaxNode syntax = boundBinaryOperator.Syntax;
		ITypeSymbol type = boundBinaryOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundBinaryOperator.ConstantValue);
		bool isLifted = boundBinaryOperator.OperatorKind.IsLifted();
		bool isChecked = boundBinaryOperator.OperatorKind.IsChecked();
		bool isCompareText = false;
		bool wasCompilerGenerated = boundBinaryOperator.WasCompilerGenerated;
		return new LazyBinaryOperatorExpression(binaryOperatorKind, leftOperand, rightOperand, isLifted, isChecked, isCompareText, methodSymbol, unaryOperatorMethod, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IBinaryOperation CreateBoundUserDefinedConditionalLogicalOperator(BoundUserDefinedConditionalLogicalOperator boundBinaryOperator)
	{
		return new LazyBinaryOperatorExpression(Helper.DeriveBinaryOperatorKind(boundBinaryOperator.OperatorKind), new Lazy<IOperation>(() => Create(boundBinaryOperator.Left)), new Lazy<IOperation>(() => Create(boundBinaryOperator.Right)), operatorMethod: boundBinaryOperator.LogicalOperator, unaryOperatorMethod: (boundBinaryOperator.OperatorKind.Operator() == Microsoft.CodeAnalysis.CSharp.BinaryOperatorKind.And) ? boundBinaryOperator.FalseOperator : boundBinaryOperator.TrueOperator, syntax: boundBinaryOperator.Syntax, type: boundBinaryOperator.Type, constantValue: ConvertToOptional(boundBinaryOperator.ConstantValue), isLifted: boundBinaryOperator.OperatorKind.IsLifted(), isChecked: boundBinaryOperator.OperatorKind.IsChecked(), isCompareText: false, isImplicit: boundBinaryOperator.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ITupleBinaryOperation CreateBoundTupleBinaryOperatorOperation(BoundTupleBinaryOperator boundTupleBinaryOperator)
	{
		BinaryOperatorKind operatorKind = Helper.DeriveBinaryOperatorKind(boundTupleBinaryOperator.OperatorKind);
		Lazy<IOperation> leftOperand = new Lazy<IOperation>(() => Create(boundTupleBinaryOperator.ConvertedLeft));
		Lazy<IOperation> rightOperand = new Lazy<IOperation>(() => Create(boundTupleBinaryOperator.ConvertedRight));
		SyntaxNode syntax = boundTupleBinaryOperator.Syntax;
		ITypeSymbol type = boundTupleBinaryOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundTupleBinaryOperator.ConstantValue);
		bool wasCompilerGenerated = boundTupleBinaryOperator.WasCompilerGenerated;
		return new LazyTupleBinaryOperatorExpression(operatorKind, leftOperand, rightOperand, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IConditionalOperation CreateBoundConditionalOperatorOperation(BoundConditionalOperator boundConditionalOperator)
	{
		Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundConditionalOperator.Condition));
		Lazy<IOperation> whenTrue = new Lazy<IOperation>(() => Create(boundConditionalOperator.Consequence));
		Lazy<IOperation> whenFalse = new Lazy<IOperation>(() => Create(boundConditionalOperator.Alternative));
		bool isRef = boundConditionalOperator.IsRef;
		SyntaxNode syntax = boundConditionalOperator.Syntax;
		ITypeSymbol type = boundConditionalOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundConditionalOperator.ConstantValue);
		bool wasCompilerGenerated = boundConditionalOperator.WasCompilerGenerated;
		return new LazyConditionalOperation(condition, whenTrue, whenFalse, isRef, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ICoalesceOperation CreateBoundNullCoalescingOperatorOperation(BoundNullCoalescingOperator boundNullCoalescingOperator)
	{
		Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundNullCoalescingOperator.LeftOperand));
		Lazy<IOperation> whenNull = new Lazy<IOperation>(() => Create(boundNullCoalescingOperator.RightOperand));
		SyntaxNode syntax = boundNullCoalescingOperator.Syntax;
		ITypeSymbol type = boundNullCoalescingOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundNullCoalescingOperator.ConstantValue);
		bool wasCompilerGenerated = boundNullCoalescingOperator.WasCompilerGenerated;
		Conversion conversion = boundNullCoalescingOperator.LeftConversion;
		if (conversion.Exists && !conversion.IsIdentity && boundNullCoalescingOperator.Type.Equals(boundNullCoalescingOperator.LeftOperand.Type?.StrippedType(), TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds))
		{
			conversion = Conversion.Identity;
		}
		return new LazyCoalesceExpression(value, whenNull, conversion, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IAwaitOperation CreateBoundAwaitExpressionOperation(BoundAwaitExpression boundAwaitExpression)
	{
		Lazy<IOperation> operation = new Lazy<IOperation>(() => Create(boundAwaitExpression.Expression));
		SyntaxNode syntax = boundAwaitExpression.Syntax;
		ITypeSymbol type = boundAwaitExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundAwaitExpression.ConstantValue);
		bool wasCompilerGenerated = boundAwaitExpression.WasCompilerGenerated;
		return new LazyAwaitExpression(operation, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IArrayElementReferenceOperation CreateBoundArrayAccessOperation(BoundArrayAccess boundArrayAccess)
	{
		Lazy<IOperation> arrayReference = new Lazy<IOperation>(() => CreateInternal(boundArrayAccess.Expression));
		Lazy<ImmutableArray<IOperation>> indices = new Lazy<ImmutableArray<IOperation>>(() => boundArrayAccess.Indices.SelectAsArray((BoundExpression n) => Create(n)));
		SyntaxNode syntax = boundArrayAccess.Syntax;
		ITypeSymbol type = boundArrayAccess.Type;
		Optional<object> constantValue = ConvertToOptional(boundArrayAccess.ConstantValue);
		bool wasCompilerGenerated = boundArrayAccess.WasCompilerGenerated;
		return new LazyArrayElementReferenceExpression(arrayReference, indices, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private INameOfOperation CreateBoundNameOfOperatorOperation(BoundNameOfOperator boundNameOfOperator)
	{
		Lazy<IOperation> argument = new Lazy<IOperation>(() => Create(boundNameOfOperator.Argument));
		SyntaxNode syntax = boundNameOfOperator.Syntax;
		ITypeSymbol type = boundNameOfOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundNameOfOperator.ConstantValue);
		bool wasCompilerGenerated = boundNameOfOperator.WasCompilerGenerated;
		return new LazyNameOfExpression(argument, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IThrowOperation CreateBoundThrowExpressionOperation(BoundThrowExpression boundThrowExpression)
	{
		Lazy<IOperation> exception = new Lazy<IOperation>(() => Create(boundThrowExpression.Expression));
		SyntaxNode syntax = boundThrowExpression.Syntax;
		ITypeSymbol type = boundThrowExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundThrowExpression.ConstantValue);
		bool wasCompilerGenerated = boundThrowExpression.WasCompilerGenerated;
		return new LazyThrowExpression(exception, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IAddressOfOperation CreateBoundAddressOfOperatorOperation(BoundAddressOfOperator boundAddressOfOperator)
	{
		Lazy<IOperation> reference = new Lazy<IOperation>(() => Create(boundAddressOfOperator.Operand));
		SyntaxNode syntax = boundAddressOfOperator.Syntax;
		ITypeSymbol type = boundAddressOfOperator.Type;
		Optional<object> constantValue = ConvertToOptional(boundAddressOfOperator.ConstantValue);
		bool wasCompilerGenerated = boundAddressOfOperator.WasCompilerGenerated;
		return new LazyAddressOfExpression(reference, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IInstanceReferenceOperation CreateBoundImplicitReceiverOperation(BoundImplicitReceiver boundImplicitReceiver)
	{
		return new InstanceReferenceExpression(InstanceReferenceKind.ImplicitReceiver, syntax: boundImplicitReceiver.Syntax, type: boundImplicitReceiver.Type, constantValue: ConvertToOptional(boundImplicitReceiver.ConstantValue), isImplicit: boundImplicitReceiver.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IConditionalAccessOperation CreateBoundConditionalAccessOperation(BoundConditionalAccess boundConditionalAccess)
	{
		Lazy<IOperation> whenNotNull = new Lazy<IOperation>(() => Create(boundConditionalAccess.AccessExpression));
		Lazy<IOperation> operation = new Lazy<IOperation>(() => Create(boundConditionalAccess.Receiver));
		SyntaxNode syntax = boundConditionalAccess.Syntax;
		ITypeSymbol type = boundConditionalAccess.Type;
		Optional<object> constantValue = ConvertToOptional(boundConditionalAccess.ConstantValue);
		bool wasCompilerGenerated = boundConditionalAccess.WasCompilerGenerated;
		return new LazyConditionalAccessExpression(whenNotNull, operation, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IConditionalAccessInstanceOperation CreateBoundConditionalReceiverOperation(BoundConditionalReceiver boundConditionalReceiver)
	{
		SyntaxNode syntax = boundConditionalReceiver.Syntax;
		ITypeSymbol type = boundConditionalReceiver.Type;
		Optional<object> constantValue = ConvertToOptional(boundConditionalReceiver.ConstantValue);
		bool wasCompilerGenerated = boundConditionalReceiver.WasCompilerGenerated;
		return new ConditionalAccessInstanceExpression(_semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IFieldInitializerOperation CreateBoundFieldEqualsValueOperation(BoundFieldEqualsValue boundFieldEqualsValue)
	{
		return new LazyFieldInitializer(initializedFields: ImmutableArray.Create((IFieldSymbol)boundFieldEqualsValue.Field), value: new Lazy<IOperation>(() => Create(boundFieldEqualsValue.Value)), kind: OperationKind.FieldInitializer, syntax: boundFieldEqualsValue.Syntax, type: null, constantValue: default, isImplicit: boundFieldEqualsValue.WasCompilerGenerated, locals: boundFieldEqualsValue.Locals.As<ILocalSymbol>(), semanticModel: _semanticModel);
	}

	private IPropertyInitializerOperation CreateBoundPropertyEqualsValueOperation(BoundPropertyEqualsValue boundPropertyEqualsValue)
	{
		return new LazyPropertyInitializer(initializedProperties: ImmutableArray.Create((IPropertySymbol)boundPropertyEqualsValue.Property), value: new Lazy<IOperation>(() => Create(boundPropertyEqualsValue.Value)), kind: OperationKind.PropertyInitializer, syntax: boundPropertyEqualsValue.Syntax, type: null, constantValue: default, isImplicit: boundPropertyEqualsValue.WasCompilerGenerated, locals: boundPropertyEqualsValue.Locals.As<ILocalSymbol>(), semanticModel: _semanticModel);
	}

	private IParameterInitializerOperation CreateBoundParameterEqualsValueOperation(BoundParameterEqualsValue boundParameterEqualsValue)
	{
		return new LazyParameterInitializer(parameter: boundParameterEqualsValue.Parameter, value: new Lazy<IOperation>(() => Create(boundParameterEqualsValue.Value)), kind: OperationKind.ParameterInitializer, syntax: boundParameterEqualsValue.Syntax, type: null, constantValue: default, isImplicit: boundParameterEqualsValue.WasCompilerGenerated, locals: boundParameterEqualsValue.Locals.As<ILocalSymbol>(), semanticModel: _semanticModel);
	}

	private IBlockOperation CreateBoundBlockOperation(BoundBlock boundBlock)
	{
		Lazy<ImmutableArray<IOperation>> operations = new Lazy<ImmutableArray<IOperation>>(() => boundBlock.Statements.Select((BoundStatement s) => Create(s)).ToImmutableArray());
		ImmutableArray<ILocalSymbol> locals = boundBlock.Locals.As<ILocalSymbol>();
		SyntaxNode syntax = boundBlock.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundBlock.WasCompilerGenerated;
		return new LazyBlockStatement(operations, locals, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IBranchOperation CreateBoundContinueStatementOperation(BoundContinueStatement boundContinueStatement)
	{
		return new BranchStatement(boundContinueStatement.Label, BranchKind.Continue, syntax: boundContinueStatement.Syntax, type: null, constantValue: default, isImplicit: boundContinueStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IBranchOperation CreateBoundBreakStatementOperation(BoundBreakStatement boundBreakStatement)
	{
		return new BranchStatement(boundBreakStatement.Label, BranchKind.Break, syntax: boundBreakStatement.Syntax, type: null, constantValue: default, isImplicit: boundBreakStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IReturnOperation CreateBoundYieldBreakStatementOperation(BoundYieldBreakStatement boundYieldBreakStatement)
	{
		Lazy<IOperation> returnedValue = new Lazy<IOperation>(() => Create(null));
		SyntaxNode syntax = boundYieldBreakStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundYieldBreakStatement.WasCompilerGenerated;
		return new LazyReturnStatement(OperationKind.YieldBreak, returnedValue, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IBranchOperation CreateBoundGotoStatementOperation(BoundGotoStatement boundGotoStatement)
	{
		return new BranchStatement(boundGotoStatement.Label, BranchKind.GoTo, syntax: boundGotoStatement.Syntax, type: null, constantValue: default, isImplicit: boundGotoStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IEmptyOperation CreateBoundNoOpStatementOperation(BoundNoOpStatement boundNoOpStatement)
	{
		SyntaxNode syntax = boundNoOpStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundNoOpStatement.WasCompilerGenerated;
		return new EmptyStatement(_semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IConditionalOperation CreateBoundIfStatementOperation(BoundIfStatement boundIfStatement)
	{
		Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundIfStatement.Condition));
		Lazy<IOperation> whenTrue = new Lazy<IOperation>(() => Create(boundIfStatement.Consequence));
		Lazy<IOperation> whenFalse = new Lazy<IOperation>(() => Create(boundIfStatement.AlternativeOpt));
		bool isRef = false;
		SyntaxNode syntax = boundIfStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundIfStatement.WasCompilerGenerated;
		return new LazyConditionalOperation(condition, whenTrue, whenFalse, isRef, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IWhileLoopOperation CreateBoundWhileStatementOperation(BoundWhileStatement boundWhileStatement)
	{
		Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundWhileStatement.Condition));
		Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundWhileStatement.Body));
		Lazy<IOperation> nullOperation = OperationFactory.NullOperation;
		ImmutableArray<ILocalSymbol> locals = boundWhileStatement.Locals.As<ILocalSymbol>();
		ILabelSymbol continueLabel = boundWhileStatement.ContinueLabel;
		ILabelSymbol breakLabel = boundWhileStatement.BreakLabel;
		bool conditionIsTop = true;
		bool conditionIsUntil = false;
		SyntaxNode syntax = boundWhileStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundWhileStatement.WasCompilerGenerated;
		return new LazyWhileLoopStatement(condition, body, nullOperation, locals, continueLabel, breakLabel, conditionIsTop, conditionIsUntil, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IWhileLoopOperation CreateBoundDoStatementOperation(BoundDoStatement boundDoStatement)
	{
		Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundDoStatement.Condition));
		Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundDoStatement.Body));
		Lazy<IOperation> nullOperation = OperationFactory.NullOperation;
		ILabelSymbol continueLabel = boundDoStatement.ContinueLabel;
		ILabelSymbol breakLabel = boundDoStatement.BreakLabel;
		bool conditionIsTop = false;
		bool conditionIsUntil = false;
		ImmutableArray<ILocalSymbol> locals = boundDoStatement.Locals.As<ILocalSymbol>();
		SyntaxNode syntax = boundDoStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundDoStatement.WasCompilerGenerated;
		return new LazyWhileLoopStatement(condition, body, nullOperation, locals, continueLabel, breakLabel, conditionIsTop, conditionIsUntil, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IForLoopOperation CreateBoundForStatementOperation(BoundForStatement boundForStatement)
	{
		Lazy<ImmutableArray<IOperation>> before = new Lazy<ImmutableArray<IOperation>>(() => ToStatements(boundForStatement.Initializer));
		Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundForStatement.Condition));
		Lazy<ImmutableArray<IOperation>> atLoopBottom = new Lazy<ImmutableArray<IOperation>>(() => ToStatements(boundForStatement.Increment));
		ImmutableArray<ILocalSymbol> locals = boundForStatement.OuterLocals.As<ILocalSymbol>();
		ImmutableArray<ILocalSymbol> conditionLocals = boundForStatement.InnerLocals.As<ILocalSymbol>();
		Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundForStatement.Body));
		ILabelSymbol continueLabel = boundForStatement.ContinueLabel;
		ILabelSymbol breakLabel = boundForStatement.BreakLabel;
		SyntaxNode syntax = boundForStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundForStatement.WasCompilerGenerated;
		return new LazyForLoopStatement(before, condition, atLoopBottom, locals, conditionLocals, continueLabel, breakLabel, body, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IForEachLoopOperation CreateBoundForEachStatementOperation(BoundForEachStatement boundForEachStatement)
	{
		ImmutableArray<ILocalSymbol> locals = boundForEachStatement.IterationVariables.As<ILocalSymbol>();
		Lazy<IOperation> loopControlVariable;
		if (boundForEachStatement.DeconstructionOpt != null)
		{
			loopControlVariable = new Lazy<IOperation>(() => Create(boundForEachStatement.DeconstructionOpt.DeconstructionAssignment.Left));
		}
		else if (boundForEachStatement.IterationErrorExpressionOpt != null)
		{
			loopControlVariable = new Lazy<IOperation>(() => Create(boundForEachStatement.IterationErrorExpressionOpt));
		}
		else
		{
			LocalSymbol local = (LocalSymbol)locals[0];
			SyntaxNode declaratorSyntax = boundForEachStatement.IterationVariableType.Syntax;
			loopControlVariable = new Lazy<IOperation>(() => new VariableDeclarator(local, null, ImmutableArray<IOperation>.Empty, _semanticModel, declaratorSyntax, null, default, isImplicit: false));
		}
		Lazy<IOperation> collection = new Lazy<IOperation>(() => Create(boundForEachStatement.Expression));
		Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundForEachStatement.Body));
		ForEachEnumeratorInfo enumeratorInfoOpt = boundForEachStatement.EnumeratorInfoOpt;
		ForEachLoopOperationInfo info;
		if (enumeratorInfoOpt != null)
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			CSharpCompilation cSharpCompilation = (CSharpCompilation)_semanticModel.Compilation;
			info = new ForEachLoopOperationInfo(enumeratorInfoOpt.ElementType, enumeratorInfoOpt.GetEnumeratorMethod, (PropertySymbol)enumeratorInfoOpt.CurrentPropertyGetter.AssociatedSymbol, enumeratorInfoOpt.MoveNextMethod, enumeratorInfoOpt.NeedsDisposeMethod, enumeratorInfoOpt.NeedsDisposeMethod && (object)enumeratorInfoOpt.GetEnumeratorMethod != null && cSharpCompilation.Conversions.ClassifyImplicitConversionFromType(enumeratorInfoOpt.GetEnumeratorMethod.ReturnType, cSharpCompilation.GetSpecialType(SpecialType.System_IDisposable), ref useSiteDiagnostics).IsImplicit, enumeratorInfoOpt.CurrentConversion, boundForEachStatement.ElementConversion);
		}
		else
		{
			info = default;
		}
		Lazy<ImmutableArray<IOperation>> nextVariables = new Lazy<ImmutableArray<IOperation>>(() => ImmutableArray<IOperation>.Empty);
		ILabelSymbol continueLabel = boundForEachStatement.ContinueLabel;
		ILabelSymbol breakLabel = boundForEachStatement.BreakLabel;
		SyntaxNode syntax = boundForEachStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundForEachStatement.WasCompilerGenerated;
		return new LazyForEachLoopStatement(locals, continueLabel, breakLabel, loopControlVariable, collection, nextVariables, body, info, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ISwitchOperation CreateBoundSwitchStatementOperation(BoundSwitchStatement boundSwitchStatement)
	{
		Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundSwitchStatement.Expression));
		Lazy<ImmutableArray<ISwitchCaseOperation>> cases = new Lazy<ImmutableArray<ISwitchCaseOperation>>(() => GetSwitchStatementCases(boundSwitchStatement));
		ImmutableArray<ILocalSymbol> locals = boundSwitchStatement.InnerLocals.As<ILocalSymbol>();
		ILabelSymbol breakLabel = boundSwitchStatement.BreakLabel;
		SyntaxNode syntax = boundSwitchStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundSwitchStatement.WasCompilerGenerated;
		return new LazySwitchStatement(locals, value, cases, breakLabel, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ICaseClauseOperation CreateBoundSwitchLabelOperation(BoundSwitchLabel boundSwitchLabel)
	{
		SyntaxNode syntax = boundSwitchLabel.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundSwitchLabel.WasCompilerGenerated;
		if (boundSwitchLabel.ExpressionOpt != null)
		{
			Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundSwitchLabel.ExpressionOpt));
			return new LazySingleValueCaseClause(boundSwitchLabel.Label, value, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
		}
		return new DefaultCaseClause(boundSwitchLabel.Label, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ITryOperation CreateBoundTryStatementOperation(BoundTryStatement boundTryStatement)
	{
		Lazy<IBlockOperation> body = new Lazy<IBlockOperation>(() => (IBlockOperation)Create(boundTryStatement.TryBlock));
		Lazy<ImmutableArray<ICatchClauseOperation>> catches = new Lazy<ImmutableArray<ICatchClauseOperation>>(() => boundTryStatement.CatchBlocks.SelectAsArray((BoundCatchBlock n) => (ICatchClauseOperation)Create(n)));
		Lazy<IBlockOperation> finallyHandler = new Lazy<IBlockOperation>(() => (IBlockOperation)Create(boundTryStatement.FinallyBlockOpt));
		SyntaxNode syntax = boundTryStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundTryStatement.WasCompilerGenerated;
		return new LazyTryStatement(body, catches, finallyHandler, null, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ICatchClauseOperation CreateBoundCatchBlockOperation(BoundCatchBlock boundCatchBlock)
	{
		BoundLocal exceptionSourceOpt = (BoundLocal)boundCatchBlock.ExceptionSourceOpt;
		Lazy<IOperation> exceptionDeclarationOrExpression = new Lazy<IOperation>(() => (exceptionSourceOpt == null) ? null : CreateVariableDeclarator(exceptionSourceOpt));
		ITypeSymbol exceptionTypeOpt = boundCatchBlock.ExceptionTypeOpt;
		ITypeSymbol exceptionType = exceptionTypeOpt ?? _semanticModel.Compilation.ObjectType;
		ImmutableArray<ILocalSymbol> locals = boundCatchBlock.Locals.As<ILocalSymbol>();
		Lazy<IOperation> filter = new Lazy<IOperation>(() => Create(boundCatchBlock.ExceptionFilterOpt));
		Lazy<IBlockOperation> handler = new Lazy<IBlockOperation>(() => (IBlockOperation)Create(boundCatchBlock.Body));
		SyntaxNode syntax = boundCatchBlock.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundCatchBlock.WasCompilerGenerated;
		return new LazyCatchClause(exceptionDeclarationOrExpression, exceptionType, locals, filter, handler, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IFixedOperation CreateBoundFixedStatementOperation(BoundFixedStatement boundFixedStatement)
	{
		ImmutableArray<ILocalSymbol> locals = boundFixedStatement.Locals.As<ILocalSymbol>();
		Lazy<IVariableDeclarationGroupOperation> variables = new Lazy<IVariableDeclarationGroupOperation>(() => (IVariableDeclarationGroupOperation)Create(boundFixedStatement.Declarations));
		Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundFixedStatement.Body));
		SyntaxNode syntax = boundFixedStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundFixedStatement.WasCompilerGenerated;
		return new LazyFixedStatement(locals, variables, body, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IUsingOperation CreateBoundUsingStatementOperation(BoundUsingStatement boundUsingStatement)
	{
		Lazy<IOperation> resources = new Lazy<IOperation>(() => Create((BoundNode)(((object)boundUsingStatement.DeclarationsOpt) ?? ((object)boundUsingStatement.ExpressionOpt))));
		Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundUsingStatement.Body));
		ImmutableArray<ILocalSymbol> locals = ImmutableArray<ILocalSymbol>.CastUp(boundUsingStatement.Locals);
		SyntaxNode syntax = boundUsingStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundUsingStatement.WasCompilerGenerated;
		return new LazyUsingStatement(resources, body, locals, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IThrowOperation CreateBoundThrowStatementOperation(BoundThrowStatement boundThrowStatement)
	{
		Lazy<IOperation> exception = new Lazy<IOperation>(() => Create(boundThrowStatement.ExpressionOpt));
		SyntaxNode syntax = boundThrowStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundThrowStatement.WasCompilerGenerated;
		return new LazyThrowExpression(exception, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IReturnOperation CreateBoundReturnStatementOperation(BoundReturnStatement boundReturnStatement)
	{
		Lazy<IOperation> returnedValue = new Lazy<IOperation>(() => Create(boundReturnStatement.ExpressionOpt));
		SyntaxNode syntax = boundReturnStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundReturnStatement.WasCompilerGenerated;
		return new LazyReturnStatement(OperationKind.Return, returnedValue, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IReturnOperation CreateBoundYieldReturnStatementOperation(BoundYieldReturnStatement boundYieldReturnStatement)
	{
		Lazy<IOperation> returnedValue = new Lazy<IOperation>(() => Create(boundYieldReturnStatement.Expression));
		SyntaxNode syntax = boundYieldReturnStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundYieldReturnStatement.WasCompilerGenerated;
		return new LazyReturnStatement(OperationKind.YieldReturn, returnedValue, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ILockOperation CreateBoundLockStatementOperation(BoundLockStatement boundLockStatement)
	{
		return new LazyLockStatement(new Lazy<IOperation>(() => Create(boundLockStatement.Argument)), new Lazy<IOperation>(() => Create(boundLockStatement.Body)), (_semanticModel.Compilation.CommonGetWellKnownTypeMember(WellKnownMember.System_Threading_Monitor__Enter2) == null) ? null : new SynthesizedLocal(_semanticModel.GetEnclosingSymbol(boundLockStatement.Syntax.SpanStart) as MethodSymbol, (TypeSymbol)_semanticModel.Compilation.GetSpecialType(SpecialType.System_Boolean), SynthesizedLocalKind.LockTaken, boundLockStatement.Argument.Syntax), syntax: boundLockStatement.Syntax, type: null, constantValue: default, isImplicit: boundLockStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IInvalidOperation CreateBoundBadStatementOperation(BoundBadStatement boundBadStatement)
	{
		return new LazyInvalidOperation(new Lazy<ImmutableArray<IOperation>>(() => boundBadStatement.ChildBoundNodes.Select((BoundNode n) => Create(n)).WhereNotNull().ToImmutableArray()), syntax: boundBadStatement.Syntax, type: null, constantValue: default, isImplicit: boundBadStatement.WasCompilerGenerated || boundBadStatement.ChildBoundNodes.Any((BoundNode e) => e?.Syntax == boundBadStatement.Syntax), semanticModel: _semanticModel);
	}

	private IOperation CreateBoundLocalDeclarationOperation(BoundLocalDeclaration boundLocalDeclaration)
	{
		SyntaxNode syntax = boundLocalDeclaration.Syntax;
		SyntaxNode syntaxNode;
		SyntaxNode syntaxNode2;
		SyntaxNode varDeclarator;
		switch (syntax.Kind())
		{
		case SyntaxKind.LocalDeclarationStatement:
		{
			LocalDeclarationStatementSyntax localDeclarationStatementSyntax = (LocalDeclarationStatementSyntax)syntax;
			syntaxNode = localDeclarationStatementSyntax;
			syntaxNode2 = localDeclarationStatementSyntax.Declaration;
			varDeclarator = localDeclarationStatementSyntax.Declaration.Variables.First();
			break;
		}
		case SyntaxKind.VariableDeclarator:
			syntaxNode = syntax.Parent;
			syntaxNode2 = syntax.Parent;
			varDeclarator = syntax;
			break;
		default:
			syntaxNode = (syntaxNode2 = (varDeclarator = syntax));
			break;
		}
		Lazy<ImmutableArray<IVariableDeclaratorOperation>> declarators = new Lazy<ImmutableArray<IVariableDeclaratorOperation>>(() => ImmutableArray.Create(CreateVariableDeclaratorInternal(boundLocalDeclaration, varDeclarator)));
		bool wasCompilerGenerated = boundLocalDeclaration.WasCompilerGenerated;
		Lazy<IVariableInitializerOperation> nullInitializer = OperationFactory.NullInitializer;
		LazyVariableDeclaration item = new LazyVariableDeclaration(declarators, nullInitializer, _semanticModel, syntaxNode2, null, default, wasCompilerGenerated);
		return new VariableDeclarationGroupOperation(type: null, constantValue: default, isImplicit: syntaxNode == syntaxNode2 || boundLocalDeclaration.WasCompilerGenerated, declarations: ImmutableArray.Create((IVariableDeclarationOperation)item), semanticModel: _semanticModel, syntax: syntaxNode);
	}

	private IVariableDeclarationGroupOperation CreateBoundMultipleLocalDeclarationsOperation(BoundMultipleLocalDeclarations boundMultipleLocalDeclarations)
	{
		Lazy<ImmutableArray<IVariableDeclaratorOperation>> declarators = new Lazy<ImmutableArray<IVariableDeclaratorOperation>>(() => boundMultipleLocalDeclarations.LocalDeclarations.SelectAsArray((BoundLocalDeclaration declaration) => CreateVariableDeclarator(declaration)));
		Lazy<IVariableInitializerOperation> nullInitializer = OperationFactory.NullInitializer;
		SyntaxNode syntax = boundMultipleLocalDeclarations.Syntax;
		SyntaxNode syntaxNode = (syntax.IsKind(SyntaxKind.LocalDeclarationStatement) ? ((LocalDeclarationStatementSyntax)syntax).Declaration : syntax);
		LazyVariableDeclaration item = new LazyVariableDeclaration(isImplicit: boundMultipleLocalDeclarations.WasCompilerGenerated, declarators: declarators, initializer: nullInitializer, semanticModel: _semanticModel, syntax: syntaxNode, type: null, constantValue: default);
		return new VariableDeclarationGroupOperation(type: null, constantValue: default, isImplicit: syntax == syntaxNode || boundMultipleLocalDeclarations.WasCompilerGenerated, declarations: ImmutableArray.Create((IVariableDeclarationOperation)item), semanticModel: _semanticModel, syntax: syntax);
	}

	private ILabeledOperation CreateBoundLabelStatementOperation(BoundLabelStatement boundLabelStatement)
	{
		return new LazyLabeledStatement(boundLabelStatement.Label, new Lazy<IOperation>(() => Create(null)), syntax: boundLabelStatement.Syntax, type: null, constantValue: default, isImplicit: boundLabelStatement.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private ILabeledOperation CreateBoundLabeledStatementOperation(BoundLabeledStatement boundLabeledStatement)
	{
		ILabelSymbol label = boundLabeledStatement.Label;
		Lazy<IOperation> operation = new Lazy<IOperation>(() => Create(boundLabeledStatement.Body));
		SyntaxNode syntax = boundLabeledStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundLabeledStatement.WasCompilerGenerated;
		return new LazyLabeledStatement(label, operation, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IExpressionStatementOperation CreateBoundExpressionStatementOperation(BoundExpressionStatement boundExpressionStatement)
	{
		return new LazyExpressionStatement(new Lazy<IOperation>(() => Create(boundExpressionStatement.Expression)), syntax: boundExpressionStatement.Syntax, type: null, constantValue: default, isImplicit: boundExpressionStatement.WasCompilerGenerated || boundExpressionStatement.Syntax == boundExpressionStatement.Expression.Syntax, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundTupleLiteralOperation(BoundTupleLiteral boundTupleLiteral)
	{
		return CreateTupleOperation(boundTupleLiteral, boundTupleLiteral.Type);
	}

	private IOperation CreateBoundConvertedTupleLiteralOperation(BoundConvertedTupleLiteral boundConvertedTupleLiteral)
	{
		return CreateTupleOperation(boundConvertedTupleLiteral, boundConvertedTupleLiteral.NaturalTypeOpt);
	}

	private IOperation CreateTupleOperation(BoundTupleExpression boundTupleExpression, ITypeSymbol naturalType)
	{
		Lazy<ImmutableArray<IOperation>> elements = new Lazy<ImmutableArray<IOperation>>(() => boundTupleExpression.Arguments.SelectAsArray((BoundExpression element) => Create(element)));
		SyntaxNode syntax = boundTupleExpression.Syntax;
		bool isImplicit = boundTupleExpression.WasCompilerGenerated;
		ITypeSymbol type = boundTupleExpression.Type;
		Optional<object> constantValue = default;
		if (syntax is DeclarationExpressionSyntax declarationExpressionSyntax)
		{
			VariableDesignationSyntax tupleSyntax = declarationExpressionSyntax.Designation;
			return new LazyDeclarationExpression(new Lazy<IOperation>(() => new LazyTupleExpression(elements, _semanticModel, tupleSyntax, type, naturalType, constantValue, isImplicit)), _semanticModel, declarationExpressionSyntax, type, default, isImplicit: false);
		}
		return new LazyTupleExpression(elements, _semanticModel, syntax, type, naturalType, constantValue, isImplicit);
	}

	private IInterpolatedStringOperation CreateBoundInterpolatedStringExpressionOperation(BoundInterpolatedString boundInterpolatedString)
	{
		Lazy<ImmutableArray<IInterpolatedStringContentOperation>> parts = new Lazy<ImmutableArray<IInterpolatedStringContentOperation>>(() => boundInterpolatedString.Parts.SelectAsArray((BoundExpression interpolatedStringContent) => CreateBoundInterpolatedStringContentOperation(interpolatedStringContent)));
		SyntaxNode syntax = boundInterpolatedString.Syntax;
		ITypeSymbol type = boundInterpolatedString.Type;
		Optional<object> constantValue = ConvertToOptional(boundInterpolatedString.ConstantValue);
		bool wasCompilerGenerated = boundInterpolatedString.WasCompilerGenerated;
		return new LazyInterpolatedStringExpression(parts, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IInterpolatedStringContentOperation CreateBoundInterpolatedStringContentOperation(BoundNode boundNode)
	{
		if (boundNode.Kind == BoundKind.StringInsert)
		{
			return (IInterpolatedStringContentOperation)Create(boundNode);
		}
		return CreateBoundInterpolatedStringTextOperation((BoundLiteral)boundNode);
	}

	private IInterpolationOperation CreateBoundInterpolationOperation(BoundStringInsert boundStringInsert)
	{
		Lazy<IOperation> expression = new Lazy<IOperation>(() => Create(boundStringInsert.Value));
		Lazy<IOperation> alignment = new Lazy<IOperation>(() => Create(boundStringInsert.Alignment));
		Lazy<IOperation> formatString = new Lazy<IOperation>(() => Create(boundStringInsert.Format));
		SyntaxNode syntax = boundStringInsert.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundStringInsert.WasCompilerGenerated;
		return new LazyInterpolation(expression, alignment, formatString, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IInterpolatedStringTextOperation CreateBoundInterpolatedStringTextOperation(BoundLiteral boundNode)
	{
		Lazy<IOperation> text = new Lazy<IOperation>(() => CreateBoundLiteralOperation(boundNode, @implicit: true));
		SyntaxNode syntax = boundNode.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundNode.WasCompilerGenerated;
		return new LazyInterpolatedStringText(text, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IConstantPatternOperation CreateBoundConstantPatternOperation(BoundConstantPattern boundConstantPattern)
	{
		Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundConstantPattern.Value));
		SyntaxNode syntax = boundConstantPattern.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundConstantPattern.WasCompilerGenerated;
		return new LazyConstantPattern(value, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IDeclarationPatternOperation CreateBoundDeclarationPatternOperation(BoundDeclarationPattern boundDeclarationPattern)
	{
		ISymbol symbol = boundDeclarationPattern.Variable;
		if (symbol == null && boundDeclarationPattern.VariableAccess.Kind == BoundKind.DiscardExpression)
		{
			symbol = ((BoundDiscardExpression)boundDeclarationPattern.VariableAccess).ExpressionSymbol;
		}
		SyntaxNode syntax = boundDeclarationPattern.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundDeclarationPattern.WasCompilerGenerated;
		return new DeclarationPattern(symbol, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ISwitchOperation CreateBoundPatternSwitchStatementOperation(BoundPatternSwitchStatement boundPatternSwitchStatement)
	{
		Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundPatternSwitchStatement.Expression));
		Lazy<ImmutableArray<ISwitchCaseOperation>> cases = new Lazy<ImmutableArray<ISwitchCaseOperation>>(() => GetPatternSwitchStatementCases(boundPatternSwitchStatement));
		ImmutableArray<ILocalSymbol> locals = boundPatternSwitchStatement.InnerLocals.As<ILocalSymbol>();
		ILabelSymbol breakLabel = boundPatternSwitchStatement.BreakLabel;
		SyntaxNode syntax = boundPatternSwitchStatement.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundPatternSwitchStatement.WasCompilerGenerated;
		return new LazySwitchStatement(locals, value, cases, breakLabel, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private ICaseClauseOperation CreateBoundPatternSwitchLabelOperation(BoundPatternSwitchLabel boundPatternSwitchLabel)
	{
		SyntaxNode syntax = boundPatternSwitchLabel.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool wasCompilerGenerated = boundPatternSwitchLabel.WasCompilerGenerated;
		LabelSymbol label = boundPatternSwitchLabel.Label;
		if (boundPatternSwitchLabel.Pattern.Kind == BoundKind.WildcardPattern)
		{
			return new DefaultCaseClause(label, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
		}
		Lazy<IPatternOperation> lazyPattern = new Lazy<IPatternOperation>(() => (IPatternOperation)Create(boundPatternSwitchLabel.Pattern));
		Lazy<IOperation> lazyGuardExpression = new Lazy<IOperation>(() => Create(boundPatternSwitchLabel.Guard));
		return new LazyPatternCaseClause(label, lazyPattern, lazyGuardExpression, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IIsPatternOperation CreateBoundIsPatternExpressionOperation(BoundIsPatternExpression boundIsPatternExpression)
	{
		Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundIsPatternExpression.Expression));
		Lazy<IPatternOperation> lazyPattern = new Lazy<IPatternOperation>(() => (IPatternOperation)Create(boundIsPatternExpression.Pattern));
		SyntaxNode syntax = boundIsPatternExpression.Syntax;
		ITypeSymbol type = boundIsPatternExpression.Type;
		Optional<object> constantValue = ConvertToOptional(boundIsPatternExpression.ConstantValue);
		bool wasCompilerGenerated = boundIsPatternExpression.WasCompilerGenerated;
		return new LazyIsPatternExpression(value, lazyPattern, _semanticModel, syntax, type, constantValue, wasCompilerGenerated);
	}

	private IOperation CreateBoundQueryClauseOperation(BoundQueryClause boundQueryClause)
	{
		if (boundQueryClause.Syntax.Kind() != SyntaxKind.QueryExpression)
		{
			return Create(boundQueryClause.Value);
		}
		return new LazyTranslatedQueryExpression(new Lazy<IOperation>(() => Create(boundQueryClause.Value)), syntax: boundQueryClause.Syntax, type: boundQueryClause.Type, constantValue: ConvertToOptional(boundQueryClause.ConstantValue), isImplicit: boundQueryClause.WasCompilerGenerated, semanticModel: _semanticModel);
	}

	private IOperation CreateBoundRangeVariableOperation(BoundRangeVariable boundRangeVariable)
	{
		return Create(boundRangeVariable.Value);
	}

	private IOperation CreateDiscardExpressionOperation(BoundDiscardExpression boundNode)
	{
		return new DiscardOperation((IDiscardSymbol)boundNode.ExpressionSymbol, _semanticModel, boundNode.Syntax, boundNode.Type, ConvertToOptional(boundNode.ConstantValue), boundNode.WasCompilerGenerated);
	}

	private static Optional<object> ConvertToOptional(ConstantValue value)
	{
		if (!(value != null) || value.IsBad)
		{
			return default;
		}
		return new Optional<object>(value.Value);
	}

	private ImmutableArray<IOperation> ToStatements(BoundStatement statement)
	{
		if (statement == null)
		{
			return ImmutableArray<IOperation>.Empty;
		}
		if (statement.Kind == BoundKind.StatementList)
		{
			return ((BoundStatementList)statement).Statements.SelectAsArray((BoundStatement n) => Create(n));
		}
		return ImmutableArray.Create(Create(statement));
	}

	private IInstanceReferenceOperation CreateImplicitReciever(SyntaxNode syntax, ITypeSymbol type)
	{
		return new InstanceReferenceExpression(InstanceReferenceKind.ImplicitReceiver, _semanticModel, syntax, type, default, isImplicit: true);
	}

	internal IArgumentOperation CreateArgumentOperation(ArgumentKind kind, IParameterSymbol parameter, BoundExpression expression)
	{
		IOperation operation = Create(expression);
		ArgumentSyntax argumentSyntax = operation.Syntax?.Parent as ArgumentSyntax;
		return new ArgumentOperation(operation, kind, parameter, s_boxedIdentityConversion, s_boxedIdentityConversion, _semanticModel, argumentSyntax ?? operation.Syntax, expression.WasCompilerGenerated || argumentSyntax == null);
	}

	private IVariableDeclaratorOperation CreateVariableDeclaratorInternal(BoundLocalDeclaration boundLocalDeclaration, SyntaxNode syntax)
	{
		IVariableInitializerOperation initializer = null;
		if (boundLocalDeclaration.InitializerOpt != null)
		{
			IOperation operation = Create(boundLocalDeclaration.InitializerOpt);
			SyntaxNode syntaxNode = null;
			bool isImplicit = false;
			if (syntax is VariableDeclaratorSyntax variableDeclaratorSyntax)
			{
				syntaxNode = variableDeclaratorSyntax.Initializer;
			}
			if (syntaxNode == null)
			{
				syntaxNode = operation.Syntax;
				isImplicit = true;
			}
			initializer = OperationFactory.CreateVariableInitializer(syntaxNode, operation, _semanticModel, isImplicit);
		}
		ImmutableArray<IOperation> ignoredArguments = (boundLocalDeclaration.ArgumentsOpt.IsDefault ? ImmutableArray<IOperation>.Empty : boundLocalDeclaration.ArgumentsOpt.SelectAsArray((BoundExpression arg) => Create(arg)));
		LocalSymbol localSymbol = boundLocalDeclaration.LocalSymbol;
		_ = boundLocalDeclaration.Syntax;
		ITypeSymbol type = null;
		Optional<object> constantValue = default;
		bool isImplicit2 = false;
		return new VariableDeclarator(localSymbol, initializer, ignoredArguments, _semanticModel, syntax, type, constantValue, isImplicit2);
	}

	private IVariableDeclaratorOperation CreateVariableDeclarator(BoundLocal boundLocal)
	{
		return new VariableDeclarator(boundLocal.LocalSymbol, null, ImmutableArray<IOperation>.Empty, _semanticModel, boundLocal.Syntax, null, default, isImplicit: false);
	}

	private Lazy<IOperation> CreateReceiverOperation(BoundNode instance, ISymbol symbol)
	{
		if (instance == null || instance.Kind == BoundKind.TypeExpression)
		{
			return OperationFactory.NullOperation;
		}
		if (symbol != null && symbol.IsStatic && instance.WasCompilerGenerated && instance.Kind == BoundKind.ThisReference)
		{
			return OperationFactory.NullOperation;
		}
		return new Lazy<IOperation>(() => Create(instance));
	}

	private bool IsCallVirtual(MethodSymbol targetMethod, BoundExpression receiver)
	{
		if ((object)targetMethod != null && receiver != null && (targetMethod.IsVirtual || targetMethod.IsAbstract || targetMethod.IsOverride))
		{
			return !receiver.SuppressVirtualCalls;
		}
		return false;
	}

	private bool IsMethodInvalid(LookupResultKind resultKind, MethodSymbol targetMethod)
	{
		if (resultKind != LookupResultKind.OverloadResolutionFailure)
		{
			return targetMethod?.OriginalDefinition is ErrorMethodSymbol;
		}
		return true;
	}

	private IEventReferenceOperation CreateBoundEventAccessOperation(BoundEventAssignmentOperator boundEventAssignmentOperator)
	{
		SyntaxNode syntax = boundEventAssignmentOperator.Syntax;
		IEventSymbol eventSymbol = boundEventAssignmentOperator.Event;
		Lazy<IOperation> instance = CreateReceiverOperation(boundEventAssignmentOperator.ReceiverOpt, eventSymbol);
		SyntaxNode left = ((AssignmentExpressionSyntax)syntax).Left;
		bool wasCompilerGenerated = boundEventAssignmentOperator.WasCompilerGenerated;
		return new LazyEventReferenceExpression(eventSymbol, instance, _semanticModel, left, eventSymbol.Type, ConvertToOptional(null), wasCompilerGenerated);
	}

	private ImmutableArray<IArgumentOperation> DeriveArguments(BoundNode boundNode, Binder binder, Symbol methodOrIndexer, MethodSymbol optionalParametersMethod, ImmutableArray<BoundExpression> boundArguments, ImmutableArray<string> argumentNamesOpt, ImmutableArray<int> argumentsToParametersOpt, ImmutableArray<RefKind> argumentRefKindsOpt, ImmutableArray<ParameterSymbol> parameters, bool expanded, SyntaxNode invocationSyntax, bool invokedAsExtensionMethod = false)
	{
		if (parameters.IsDefaultOrEmpty && boundArguments.IsDefaultOrEmpty)
		{
			return ImmutableArray<IArgumentOperation>.Empty;
		}
		return LocalRewriter.MakeArgumentsInEvaluationOrder(this, binder, invocationSyntax, boundArguments, methodOrIndexer, optionalParametersMethod, expanded, argumentsToParametersOpt, invokedAsExtensionMethod);
	}

	private IInvalidOperation CreateInvalidExpressionForHasArgumentsExpression(BoundNode receiverOpt, ImmutableArray<BoundExpression> arguments, BoundExpression additionalNodeOpt, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
	{
		return new LazyInvalidOperation(new Lazy<ImmutableArray<IOperation>>(() =>
		{
			ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance();
			if (receiverOpt != null && (!receiverOpt.WasCompilerGenerated || (receiverOpt.Kind != BoundKind.ThisReference && receiverOpt.Kind != BoundKind.BaseReference && receiverOpt.Kind != BoundKind.ImplicitReceiver)))
			{
				instance.Add(Create(receiverOpt));
			}
			instance.AddRange(arguments.Select((BoundExpression a) => Create(a)));
			if (additionalNodeOpt != null)
			{
				instance.Add(Create(additionalNodeOpt));
			}
			return instance.ToImmutableAndFree();
		}), _semanticModel, syntax, type, constantValue, isImplicit);
	}

	private ImmutableArray<IOperation> GetAnonymousObjectCreationInitializers(BoundAnonymousObjectCreationExpression expression)
	{
		return GetAnonymousObjectCreationInitializers(expression.Arguments, expression.Declarations, expression.Syntax, expression.Type, expression.WasCompilerGenerated);
	}

	private ImmutableArray<IOperation> GetAnonymousObjectCreationInitializers(ImmutableArray<BoundExpression> arguments, ImmutableArray<BoundAnonymousPropertyDeclaration> declarations, SyntaxNode syntax, ITypeSymbol type, bool isImplicit)
	{
		ArrayBuilder<IOperation> instance = ArrayBuilder<IOperation>.GetInstance(arguments.Length);
		int num = 0;
		for (int i = 0; i < arguments.Length; i++)
		{
			IOperation operation = Create(arguments[i]);
			InstanceReferenceExpression instanceReferenceExpression = new InstanceReferenceExpression(InstanceReferenceKind.ImplicitReceiver, _semanticModel, syntax, type, default, isImplicit: true);
			IPropertySymbol anonymousTypeProperty = AnonymousTypeManager.GetAnonymousTypeProperty((NamedTypeSymbol)type, i);
			IOperation operation2;
			bool isImplicit2;
			if (num >= declarations.Length || anonymousTypeProperty != declarations[num].Property)
			{
				operation2 = new PropertyReferenceExpression(anonymousTypeProperty, instanceReferenceExpression, ImmutableArray<IArgumentOperation>.Empty, _semanticModel, operation.Syntax, anonymousTypeProperty.Type, default, isImplicit: true);
				isImplicit2 = true;
			}
			else
			{
				operation2 = CreateBoundAnonymousPropertyDeclarationOperation(declarations[num++], instanceReferenceExpression);
				isImplicit2 = isImplicit;
			}
			SyntaxNode syntax2 = operation.Syntax?.Parent ?? syntax;
			ITypeSymbol type2 = operation2.Type;
			Optional<object> constantValue = operation.ConstantValue;
			bool isRef = false;
			SimpleAssignmentExpression item = new SimpleAssignmentExpression(operation2, isRef, operation, _semanticModel, syntax2, type2, constantValue, isImplicit2);
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	private ImmutableArray<ISwitchCaseOperation> GetSwitchStatementCases(BoundSwitchStatement statement)
	{
		return statement.SwitchSections.SelectAsArray((Func<BoundSwitchSection, ISwitchCaseOperation>)((BoundSwitchSection switchSection) =>
		{
			ImmutableArray<ICaseClauseOperation> clauses = switchSection.SwitchLabels.SelectAsArray((BoundSwitchLabel s) => (ICaseClauseOperation)Create(s));
			ImmutableArray<IOperation> body = switchSection.Statements.SelectAsArray((BoundStatement s) => Create(s));
			return new SwitchCase(switchSection.Locals.CastArray<ILocalSymbol>(), null, clauses, body, _semanticModel, switchSection.Syntax, null, default, switchSection.WasCompilerGenerated);
		}));
	}

	private ImmutableArray<ISwitchCaseOperation> GetPatternSwitchStatementCases(BoundPatternSwitchStatement statement)
	{
		return statement.SwitchSections.SelectAsArray((Func<BoundPatternSwitchSection, ISwitchCaseOperation>)((BoundPatternSwitchSection switchSection) =>
		{
			ImmutableArray<ICaseClauseOperation> clauses = switchSection.SwitchLabels.SelectAsArray((BoundPatternSwitchLabel s) => (ICaseClauseOperation)Create(s));
			ImmutableArray<IOperation> body = switchSection.Statements.SelectAsArray((BoundStatement s) => Create(s));
			return new SwitchCase(switchSection.Locals.CastArray<ILocalSymbol>(), null, clauses, body, _semanticModel, switchSection.Syntax, null, default, switchSection.WasCompilerGenerated);
		}));
	}
}
