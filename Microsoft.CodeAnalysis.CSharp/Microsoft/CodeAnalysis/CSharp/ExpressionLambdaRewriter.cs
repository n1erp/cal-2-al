using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class ExpressionLambdaRewriter
{
	private enum InitializerKind
	{
		Expression,
		MemberInitializer,
		CollectionInitializer
	}

	private readonly SyntheticBoundNodeFactory _bound;

	private readonly TypeMap _typeMap;

	private readonly Dictionary<ParameterSymbol, BoundExpression> _parameterMap = new Dictionary<ParameterSymbol, BoundExpression>();

	private readonly bool _ignoreAccessibility;

	private int _recursionDepth;

	private NamedTypeSymbol _ExpressionType;

	private NamedTypeSymbol _ParameterExpressionType;

	private NamedTypeSymbol _ElementInitType;

	private NamedTypeSymbol _MemberBindingType;

	private readonly NamedTypeSymbol _int32Type;

	private readonly NamedTypeSymbol _objectType;

	private readonly NamedTypeSymbol _nullableType;

	private NamedTypeSymbol _MemberInfoType;

	private readonly NamedTypeSymbol _IEnumerableType;

	private NamedTypeSymbol ExpressionType
	{
		get
		{
			if ((object)_ExpressionType == null)
			{
				_ExpressionType = _bound.WellKnownType(WellKnownType.System_Linq_Expressions_Expression);
			}
			return _ExpressionType;
		}
	}

	private NamedTypeSymbol ParameterExpressionType
	{
		get
		{
			if ((object)_ParameterExpressionType == null)
			{
				_ParameterExpressionType = _bound.WellKnownType(WellKnownType.System_Linq_Expressions_ParameterExpression);
			}
			return _ParameterExpressionType;
		}
	}

	private NamedTypeSymbol ElementInitType
	{
		get
		{
			if ((object)_ElementInitType == null)
			{
				_ElementInitType = _bound.WellKnownType(WellKnownType.System_Linq_Expressions_ElementInit);
			}
			return _ElementInitType;
		}
	}

	public NamedTypeSymbol MemberBindingType
	{
		get
		{
			if ((object)_MemberBindingType == null)
			{
				_MemberBindingType = _bound.WellKnownType(WellKnownType.System_Linq_Expressions_MemberBinding);
			}
			return _MemberBindingType;
		}
	}

	private NamedTypeSymbol MemberInfoType
	{
		get
		{
			if ((object)_MemberInfoType == null)
			{
				_MemberInfoType = _bound.WellKnownType(WellKnownType.System_Reflection_MemberInfo);
			}
			return _MemberInfoType;
		}
	}

	private DiagnosticBag Diagnostics => _bound.Diagnostics;

	private ExpressionLambdaRewriter(TypeCompilationState compilationState, TypeMap typeMap, SyntaxNode node, int recursionDepth, DiagnosticBag diagnostics)
	{
		_bound = new SyntheticBoundNodeFactory(null, compilationState.Type, node, compilationState, diagnostics);
		_ignoreAccessibility = compilationState.ModuleBuilderOpt.IgnoreAccessibility;
		_int32Type = _bound.SpecialType(SpecialType.System_Int32);
		_objectType = _bound.SpecialType(SpecialType.System_Object);
		_nullableType = _bound.SpecialType(SpecialType.System_Nullable_T);
		_IEnumerableType = _bound.SpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);
		_typeMap = typeMap;
		_recursionDepth = recursionDepth;
	}

	internal static BoundNode RewriteLambda(BoundLambda node, TypeCompilationState compilationState, TypeMap typeMap, int recursionDepth, DiagnosticBag diagnostics)
	{
		try
		{
			ExpressionLambdaRewriter expressionLambdaRewriter = new ExpressionLambdaRewriter(compilationState, typeMap, node.Syntax, recursionDepth, diagnostics);
			BoundExpression boundExpression = expressionLambdaRewriter.VisitLambdaInternal(node);
			if (node.Type != boundExpression.Type)
			{
				diagnostics.Add(ErrorCode.ERR_MissingPredefinedMember, node.Syntax.Location, expressionLambdaRewriter.ExpressionType, "Lambda");
			}
			return boundExpression;
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			return node;
		}
	}

	private BoundExpression TranslateLambdaBody(BoundBlock block)
	{
		foreach (BoundStatement statement in block.Statements)
		{
			BoundStatement boundStatement = statement;
			while (boundStatement != null)
			{
				switch (boundStatement.Kind)
				{
				case BoundKind.ReturnStatement:
				{
					BoundExpression boundExpression = Visit(((BoundReturnStatement)boundStatement).ExpressionOpt);
					if (boundExpression != null)
					{
						return boundExpression;
					}
					boundStatement = null;
					break;
				}
				case BoundKind.ExpressionStatement:
					return Visit(((BoundExpressionStatement)boundStatement).Expression);
				case BoundKind.SequencePoint:
					boundStatement = ((BoundSequencePoint)boundStatement).StatementOpt;
					break;
				case BoundKind.SequencePointWithSpan:
					boundStatement = ((BoundSequencePointWithSpan)boundStatement).StatementOpt;
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(boundStatement.Kind);
				}
			}
		}
		return null;
	}

	private BoundExpression Visit(BoundExpression node)
	{
		if (node == null)
		{
			return null;
		}
		SyntaxNode syntax = _bound.Syntax;
		_bound.Syntax = node.Syntax;
		BoundExpression arg = VisitInternal(node);
		_bound.Syntax = syntax;
		return _bound.Convert(ExpressionType, arg);
	}

	private BoundExpression VisitExpressionWithoutStackGuard(BoundExpression node)
	{
		switch (node.Kind)
		{
		case BoundKind.ArrayAccess:
			return VisitArrayAccess((BoundArrayAccess)node);
		case BoundKind.ArrayCreation:
			return VisitArrayCreation((BoundArrayCreation)node);
		case BoundKind.ArrayLength:
			return VisitArrayLength((BoundArrayLength)node);
		case BoundKind.AsOperator:
			return VisitAsOperator((BoundAsOperator)node);
		case BoundKind.BaseReference:
			return VisitBaseReference((BoundBaseReference)node);
		case BoundKind.BinaryOperator:
		{
			BoundBinaryOperator boundBinaryOperator = (BoundBinaryOperator)node;
			return VisitBinaryOperator(boundBinaryOperator.OperatorKind, boundBinaryOperator.MethodOpt, boundBinaryOperator.Type, boundBinaryOperator.Left, boundBinaryOperator.Right);
		}
		case BoundKind.UserDefinedConditionalLogicalOperator:
		{
			BoundUserDefinedConditionalLogicalOperator boundUserDefinedConditionalLogicalOperator = (BoundUserDefinedConditionalLogicalOperator)node;
			return VisitBinaryOperator(boundUserDefinedConditionalLogicalOperator.OperatorKind, boundUserDefinedConditionalLogicalOperator.LogicalOperator, boundUserDefinedConditionalLogicalOperator.Type, boundUserDefinedConditionalLogicalOperator.Left, boundUserDefinedConditionalLogicalOperator.Right);
		}
		case BoundKind.Call:
			return VisitCall((BoundCall)node);
		case BoundKind.ConditionalOperator:
			return VisitConditionalOperator((BoundConditionalOperator)node);
		case BoundKind.Conversion:
			return VisitConversion((BoundConversion)node);
		case BoundKind.PassByCopy:
			return Visit(((BoundPassByCopy)node).Expression);
		case BoundKind.DelegateCreationExpression:
			return VisitDelegateCreationExpression((BoundDelegateCreationExpression)node);
		case BoundKind.FieldAccess:
		{
			BoundFieldAccess boundFieldAccess = (BoundFieldAccess)node;
			if (boundFieldAccess.FieldSymbol.IsCapturedFrame)
			{
				return Constant(boundFieldAccess);
			}
			return VisitFieldAccess(boundFieldAccess);
		}
		case BoundKind.IsOperator:
			return VisitIsOperator((BoundIsOperator)node);
		case BoundKind.Lambda:
			return VisitLambda((BoundLambda)node);
		case BoundKind.NewT:
			return VisitNewT((BoundNewT)node);
		case BoundKind.NullCoalescingOperator:
			return VisitNullCoalescingOperator((BoundNullCoalescingOperator)node);
		case BoundKind.ObjectCreationExpression:
			return VisitObjectCreationExpression((BoundObjectCreationExpression)node);
		case BoundKind.Parameter:
			return VisitParameter((BoundParameter)node);
		case BoundKind.PointerIndirectionOperator:
			return VisitPointerIndirectionOperator((BoundPointerIndirectionOperator)node);
		case BoundKind.PointerElementAccess:
			return VisitPointerElementAccess((BoundPointerElementAccess)node);
		case BoundKind.PropertyAccess:
			return VisitPropertyAccess((BoundPropertyAccess)node);
		case BoundKind.SizeOfOperator:
			return VisitSizeOfOperator((BoundSizeOfOperator)node);
		case BoundKind.UnaryOperator:
			return VisitUnaryOperator((BoundUnaryOperator)node);
		case BoundKind.TypeOfOperator:
		case BoundKind.MethodInfo:
		case BoundKind.DefaultExpression:
		case BoundKind.Literal:
		case BoundKind.ThisReference:
		case BoundKind.PreviousSubmissionReference:
		case BoundKind.HostObjectMemberReference:
		case BoundKind.Local:
			return Constant(node);
		default:
			throw ExceptionUtilities.UnexpectedValue(node.Kind);
		}
	}

	private BoundExpression VisitInternal(BoundExpression node)
	{
		_recursionDepth++;
		BoundExpression result;
		if (_recursionDepth > 1)
		{
			StackGuard.EnsureSufficientExecutionStack(_recursionDepth);
			result = VisitExpressionWithoutStackGuard(node);
		}
		else
		{
			result = VisitExpressionWithStackGuard(node);
		}
		_recursionDepth--;
		return result;
	}

	private BoundExpression VisitExpressionWithStackGuard(BoundExpression node)
	{
		try
		{
			return VisitExpressionWithoutStackGuard(node);
		}
		catch (Exception ex) when (StackGuard.IsInsufficientExecutionStackException(ex))
		{
			throw new BoundTreeVisitor.CancelledByStackGuardException(ex, node);
		}
	}

	private BoundExpression VisitArrayAccess(BoundArrayAccess node)
	{
		BoundExpression boundExpression = Visit(node.Expression);
		if (node.Indices.Length == 1)
		{
			BoundExpression boundExpression2 = node.Indices[0];
			BoundExpression boundExpression3 = Visit(boundExpression2);
			if (boundExpression3.Type != _int32Type)
			{
				boundExpression3 = ConvertIndex(boundExpression3, boundExpression2.Type, _int32Type);
			}
			return ExprFactory("ArrayIndex", boundExpression, boundExpression3);
		}
		return ExprFactory("ArrayIndex", boundExpression, Indices(node.Indices));
	}

	private BoundExpression Indices(ImmutableArray<BoundExpression> expressions)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (BoundExpression item in expressions)
		{
			BoundExpression boundExpression = Visit(item);
			if (boundExpression.Type != _int32Type)
			{
				boundExpression = ConvertIndex(boundExpression, item.Type, _int32Type);
			}
			instance.Add(boundExpression);
		}
		return _bound.ArrayOrEmpty(ExpressionType, instance.ToImmutableAndFree());
	}

	private BoundExpression Expressions(ImmutableArray<BoundExpression> expressions)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (BoundExpression item in expressions)
		{
			instance.Add(Visit(item));
		}
		return _bound.ArrayOrEmpty(ExpressionType, instance.ToImmutableAndFree());
	}

	private BoundExpression VisitArrayCreation(BoundArrayCreation node)
	{
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)node.Type;
		BoundExpression boundExpression = _bound.Typeof(arrayTypeSymbol.ElementType);
		if (node.InitializerOpt != null)
		{
			if (arrayTypeSymbol.IsSZArray)
			{
				return ExprFactory("NewArrayInit", boundExpression, Expressions(node.InitializerOpt.Initializers));
			}
			return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), ExpressionType);
		}
		return ExprFactory("NewArrayBounds", boundExpression, Expressions(node.Bounds));
	}

	private BoundExpression VisitArrayLength(BoundArrayLength node)
	{
		return ExprFactory("ArrayLength", Visit(node.Expression));
	}

	private BoundExpression VisitAsOperator(BoundAsOperator node)
	{
		if (node.Operand.IsLiteralNull() && (object)node.Operand.Type == null)
		{
			BoundExpression operand = _bound.Null(_bound.SpecialType(SpecialType.System_Object));
			node = node.Update(operand, node.TargetType, node.Conversion, node.Type);
		}
		return ExprFactory("TypeAs", Visit(node.Operand), _bound.Typeof(node.Type));
	}

	private BoundExpression VisitBaseReference(BoundBaseReference node)
	{
		return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), ExpressionType);
	}

	private static string GetBinaryOperatorName(BinaryOperatorKind opKind, out bool isChecked, out bool isLifted, out bool requiresLifted)
	{
		isChecked = opKind.IsChecked();
		isLifted = opKind.IsLifted();
		requiresLifted = opKind.IsComparison();
		switch (opKind.Operator())
		{
		case BinaryOperatorKind.Addition:
			if (!isChecked)
			{
				return "Add";
			}
			return "AddChecked";
		case BinaryOperatorKind.Multiplication:
			if (!isChecked)
			{
				return "Multiply";
			}
			return "MultiplyChecked";
		case BinaryOperatorKind.Subtraction:
			if (!isChecked)
			{
				return "Subtract";
			}
			return "SubtractChecked";
		case BinaryOperatorKind.Division:
			return "Divide";
		case BinaryOperatorKind.Remainder:
			return "Modulo";
		case BinaryOperatorKind.And:
			if (!opKind.IsLogical())
			{
				return "And";
			}
			return "AndAlso";
		case BinaryOperatorKind.Xor:
			return "ExclusiveOr";
		case BinaryOperatorKind.Or:
			if (!opKind.IsLogical())
			{
				return "Or";
			}
			return "OrElse";
		case BinaryOperatorKind.LeftShift:
			return "LeftShift";
		case BinaryOperatorKind.RightShift:
			return "RightShift";
		case BinaryOperatorKind.Equal:
			return "Equal";
		case BinaryOperatorKind.NotEqual:
			return "NotEqual";
		case BinaryOperatorKind.LessThan:
			return "LessThan";
		case BinaryOperatorKind.LessThanOrEqual:
			return "LessThanOrEqual";
		case BinaryOperatorKind.GreaterThan:
			return "GreaterThan";
		case BinaryOperatorKind.GreaterThanOrEqual:
			return "GreaterThanOrEqual";
		default:
			throw ExceptionUtilities.UnexpectedValue(opKind.Operator());
		}
	}

	private BoundExpression VisitBinaryOperator(BinaryOperatorKind opKind, MethodSymbol methodOpt, TypeSymbol type, BoundExpression left, BoundExpression right)
	{
		string binaryOperatorName = GetBinaryOperatorName(opKind, out var isChecked, out var isLifted, out var requiresLifted);
		if ((object)left.Type == null && left.IsLiteralNull())
		{
			left = _bound.Default(right.Type);
		}
		if ((object)right.Type == null && right.IsLiteralNull())
		{
			right = _bound.Default(left.Type);
		}
		BinaryOperatorKind binaryOperatorKind = opKind.OperandTypes();
		if ((uint)(binaryOperatorKind - 35) <= 2u)
		{
			BoundExpression boundExpression = ((opKind.OperandTypes() == BinaryOperatorKind.UnderlyingAndEnum) ? right : left);
			TypeSymbol typeSymbol = PromotedType(boundExpression.Type.StrippedType().GetEnumUnderlyingType());
			if (opKind.IsLifted())
			{
				typeSymbol = _nullableType.Construct(typeSymbol);
			}
			BoundExpression loweredLeft = VisitAndPromoteEnumOperand(left, typeSymbol, isChecked);
			BoundExpression loweredRight = VisitAndPromoteEnumOperand(right, typeSymbol, isChecked);
			BoundExpression node = MakeBinary(methodOpt, type, isLifted, requiresLifted, binaryOperatorName, loweredLeft, loweredRight);
			return Demote(node, type, isChecked);
		}
		BoundExpression loweredLeft2 = Visit(left);
		BoundExpression loweredRight2 = Visit(right);
		return MakeBinary(methodOpt, type, isLifted, requiresLifted, binaryOperatorName, loweredLeft2, loweredRight2);
	}

	private static BoundExpression DemoteEnumOperand(BoundExpression operand)
	{
		if (operand.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)operand;
			if (!boundConversion.ConversionKind.IsUserDefinedConversion() && boundConversion.ConversionKind.IsImplicitConversion() && boundConversion.ConversionKind != ConversionKind.DefaultOrNullLiteral && boundConversion.Type.StrippedType().IsEnumType())
			{
				operand = boundConversion.Operand;
			}
		}
		return operand;
	}

	private BoundExpression VisitAndPromoteEnumOperand(BoundExpression operand, TypeSymbol promotedType, bool isChecked)
	{
		if (operand is BoundLiteral boundLiteral)
		{
			return Constant(boundLiteral.Update(boundLiteral.ConstantValue, promotedType));
		}
		BoundExpression node = DemoteEnumOperand(operand);
		BoundExpression operand2 = Visit(node);
		return Convert(operand2, operand.Type, promotedType, isChecked, isExplicit: false);
	}

	private BoundExpression MakeBinary(MethodSymbol methodOpt, TypeSymbol type, bool isLifted, bool requiresLifted, string opName, BoundExpression loweredLeft, BoundExpression loweredRight)
	{
		if ((object)methodOpt != null)
		{
			if (!requiresLifted)
			{
				return ExprFactory(opName, loweredLeft, loweredRight, _bound.MethodInfo(methodOpt));
			}
			return ExprFactory(opName, loweredLeft, loweredRight, _bound.Literal(isLifted && methodOpt.ReturnType != type), _bound.MethodInfo(methodOpt));
		}
		return ExprFactory(opName, loweredLeft, loweredRight);
	}

	private TypeSymbol PromotedType(TypeSymbol underlying)
	{
		if (underlying.SpecialType == SpecialType.System_Boolean)
		{
			return underlying;
		}
		SpecialType enumPromotedType = Binder.GetEnumPromotedType(underlying.SpecialType);
		if (enumPromotedType == underlying.SpecialType)
		{
			return underlying;
		}
		return _bound.SpecialType(enumPromotedType);
	}

	private BoundExpression Demote(BoundExpression node, TypeSymbol type, bool isChecked)
	{
		if (type is NamedTypeSymbol namedTypeSymbol)
		{
			if (namedTypeSymbol.StrippedType().TypeKind == TypeKind.Enum)
			{
				return Convert(node, type, isChecked);
			}
			if ((namedTypeSymbol.IsNullableType() ? _nullableType.Construct(PromotedType(namedTypeSymbol.GetNullableUnderlyingType())) : PromotedType(namedTypeSymbol)) != type)
			{
				return Convert(node, type, isChecked);
			}
		}
		return node;
	}

	private BoundExpression ConvertIndex(BoundExpression expr, TypeSymbol oldType, TypeSymbol newType)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		return _bound.Compilation.Conversions.ClassifyConversionFromType(oldType, newType, ref useSiteDiagnostics).Kind switch
		{
			ConversionKind.Identity => expr, 
			ConversionKind.ExplicitNumeric => Convert(expr, newType, isChecked: true), 
			_ => Convert(expr, _int32Type, isChecked: false), 
		};
	}

	private BoundExpression VisitCall(BoundCall node)
	{
		if (node.IsDelegateCall)
		{
			return ExprFactory("Invoke", Visit(node.ReceiverOpt), Expressions(node.Arguments));
		}
		MethodSymbol method = node.Method;
		return ExprFactory("Call", method.IsStatic ? _bound.Null(ExpressionType) : Visit(node.ReceiverOpt), _bound.MethodInfo(method), Expressions(node.Arguments));
	}

	private BoundExpression VisitConditionalOperator(BoundConditionalOperator node)
	{
		BoundExpression boundExpression = Visit(node.Condition);
		BoundExpression boundExpression2 = VisitExactType(node.Consequence);
		BoundExpression boundExpression3 = VisitExactType(node.Alternative);
		return ExprFactory("Condition", boundExpression, boundExpression2, boundExpression3);
	}

	private BoundExpression VisitExactType(BoundExpression e)
	{
		if (e is BoundConversion { ExplicitCastInCode: false } boundConversion)
		{
			e = boundConversion.Update(boundConversion.Operand, boundConversion.Conversion, boundConversion.IsBaseConversion, boundConversion.Checked, explicitCastInCode: true, boundConversion.ConstantValueOpt, boundConversion.Type);
		}
		return Visit(e);
	}

	private BoundExpression VisitConversion(BoundConversion node)
	{
		switch (node.ConversionKind)
		{
		case ConversionKind.MethodGroup:
		{
			BoundMethodGroup boundMethodGroup = (BoundMethodGroup)node.Operand;
			return DelegateCreation(boundMethodGroup.ReceiverOpt, node.SymbolOpt, node.Type, node.SymbolOpt.IsStatic && !node.IsExtensionMethod);
		}
		case ConversionKind.ImplicitUserDefined:
		case ConversionKind.ExplicitUserDefined:
		case ConversionKind.IntPtr:
		{
			MethodSymbol symbolOpt = node.SymbolOpt;
			TypeSymbol type = node.Operand.Type;
			TypeSymbol typeSymbol2 = type.StrippedType();
			TypeSymbol type2 = symbolOpt.Parameters[0].Type;
			bool num = type != type2 && typeSymbol2 == type2;
			bool flag = typeSymbol2 != ((node.ConversionKind == ConversionKind.ExplicitUserDefined) ? type2 : type2.StrippedType());
			TypeSymbol typeSymbol3 = ((num && symbolOpt.ReturnType.IsNonNullableValueType() && node.Type.IsNullableType()) ? _nullableType.Construct(symbolOpt.ReturnType) : symbolOpt.ReturnType);
			BoundExpression boundExpression = (flag ? Convert(Visit(node.Operand), node.Operand.Type, symbolOpt.Parameters[0].Type, node.Checked, isExplicit: false) : Visit(node.Operand));
			BoundExpression operand2 = ExprFactory("Convert", boundExpression, _bound.Typeof(typeSymbol3), _bound.MethodInfo(symbolOpt));
			return Convert(operand2, typeSymbol3, node.Type, node.Checked, isExplicit: false);
		}
		case ConversionKind.Identity:
		case ConversionKind.ImplicitReference:
		{
			BoundExpression boundExpression2 = Visit(node.Operand);
			if (!node.ExplicitCastInCode)
			{
				return boundExpression2;
			}
			return Convert(boundExpression2, node.Type, isChecked: false);
		}
		case ConversionKind.ImplicitNullable:
		{
			if (node.Operand.Type.IsNullableType())
			{
				return Convert(Visit(node.Operand), node.Operand.Type, node.Type, node.Checked, node.ExplicitCastInCode);
			}
			TypeSymbol typeSymbol = ((NamedTypeSymbol)node.Type).TypeArgumentsNoUseSiteDiagnostics[0];
			BoundExpression operand = Convert(Visit(node.Operand), node.Operand.Type, typeSymbol, node.Checked, isExplicit: false);
			return Convert(operand, typeSymbol, node.Type, node.Checked, isExplicit: false);
		}
		case ConversionKind.DefaultOrNullLiteral:
			return Convert(Constant(_bound.Null(_objectType)), _objectType, node.Type, isChecked: false, node.ExplicitCastInCode);
		default:
			return Convert(Visit(node.Operand), node.Operand.Type, node.Type, node.Checked, node.ExplicitCastInCode);
		}
	}

	private BoundExpression Convert(BoundExpression operand, TypeSymbol oldType, TypeSymbol newType, bool isChecked, bool isExplicit)
	{
		if (!(oldType == newType) || isExplicit)
		{
			return Convert(operand, newType, isChecked);
		}
		return operand;
	}

	private BoundExpression Convert(BoundExpression expr, TypeSymbol type, bool isChecked)
	{
		return ExprFactory(isChecked ? "ConvertChecked" : "Convert", expr, _bound.Typeof(type));
	}

	private BoundExpression DelegateCreation(BoundExpression receiver, MethodSymbol method, TypeSymbol delegateType, bool staticMember)
	{
		BoundExpression boundExpression = _bound.Null(_objectType);
		receiver = (staticMember ? boundExpression : (receiver.Type.IsReferenceType ? receiver : _bound.Convert(_objectType, receiver)));
		MethodSymbol methodSymbol = _bound.WellKnownMethod(WellKnownMember.System_Reflection_MethodInfo__CreateDelegate, isOptional: true);
		BoundExpression node = (((object)methodSymbol == null) ? _bound.StaticCall(_bound.SpecialType(SpecialType.System_Delegate), "CreateDelegate", _bound.Typeof(delegateType), receiver, _bound.MethodInfo(method)) : _bound.Call(_bound.MethodInfo(method), methodSymbol, _bound.Typeof(delegateType), receiver));
		return Convert(Visit(node), delegateType, isChecked: false);
	}

	private BoundExpression VisitDelegateCreationExpression(BoundDelegateCreationExpression node)
	{
		if (node.Argument.Kind == BoundKind.MethodGroup)
		{
			throw ExceptionUtilities.UnexpectedValue(BoundKind.MethodGroup);
		}
		if ((object)node.MethodOpt != null)
		{
			bool staticMember = node.MethodOpt.IsStatic && !node.IsExtensionMethod;
			return DelegateCreation(node.Argument, node.MethodOpt, node.Type, staticMember);
		}
		if (node.Argument.Type is NamedTypeSymbol { TypeKind: TypeKind.Delegate } namedTypeSymbol)
		{
			return DelegateCreation(node.Argument, namedTypeSymbol.DelegateInvokeMethod, node.Type, staticMember: false);
		}
		throw ExceptionUtilities.UnexpectedValue(node.Argument);
	}

	private BoundExpression VisitFieldAccess(BoundFieldAccess node)
	{
		BoundExpression boundExpression = (node.FieldSymbol.IsStatic ? _bound.Null(ExpressionType) : Visit(node.ReceiverOpt));
		return ExprFactory("Field", boundExpression, _bound.FieldInfo(node.FieldSymbol));
	}

	private BoundExpression VisitIsOperator(BoundIsOperator node)
	{
		BoundExpression boundExpression = node.Operand;
		if ((object)boundExpression.Type == null && boundExpression.ConstantValue != null && boundExpression.ConstantValue.IsNull)
		{
			boundExpression = _bound.Null(_objectType);
		}
		return ExprFactory("TypeIs", Visit(boundExpression), _bound.Typeof(node.TargetType.Type));
	}

	private BoundExpression VisitLambda(BoundLambda node)
	{
		BoundExpression boundExpression = VisitLambdaInternal(node);
		if (!node.Type.IsExpressionTree())
		{
			return boundExpression;
		}
		return ExprFactory("Quote", boundExpression);
	}

	private BoundExpression VisitLambdaInternal(BoundLambda node)
	{
		ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
		ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<BoundExpression> instance3 = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (ParameterSymbol parameter in node.Symbol.Parameters)
		{
			LocalSymbol localSymbol = _bound.SynthesizedLocal(ParameterExpressionType);
			instance.Add(localSymbol);
			BoundLocal boundLocal = _bound.Local(localSymbol);
			instance3.Add(boundLocal);
			BoundExpression right = ExprFactory("Parameter", _bound.Typeof(_typeMap.SubstituteType(parameter.Type).Type), _bound.Literal(parameter.Name));
			instance2.Add(_bound.AssignmentExpression(boundLocal, right));
			_parameterMap[parameter] = boundLocal;
		}
		NamedTypeSymbol delegateType = node.Type.GetDelegateType();
		BoundSequence result = _bound.Sequence(instance.ToImmutableAndFree(), instance2.ToImmutableAndFree(), ExprFactory("Lambda", ImmutableArray.Create((TypeSymbol)delegateType), TranslateLambdaBody(node.Body), _bound.ArrayOrEmpty(ParameterExpressionType, instance3.ToImmutableAndFree())));
		foreach (ParameterSymbol parameter2 in node.Symbol.Parameters)
		{
			_parameterMap.Remove(parameter2);
		}
		return result;
	}

	private BoundExpression VisitNewT(BoundNewT node)
	{
		return VisitObjectCreationContinued(ExprFactory("New", _bound.Typeof(node.Type)), node.InitializerExpressionOpt);
	}

	private BoundExpression VisitNullCoalescingOperator(BoundNullCoalescingOperator node)
	{
		BoundExpression boundExpression = Visit(node.LeftOperand);
		BoundExpression boundExpression2 = Visit(node.RightOperand);
		if (node.LeftConversion.IsUserDefined)
		{
			TypeSymbol fromType = node.LeftOperand.Type.StrippedType();
			return ExprFactory("Coalesce", boundExpression, boundExpression2, MakeConversionLambda(node.LeftConversion, fromType, node.Type));
		}
		return ExprFactory("Coalesce", boundExpression, boundExpression2);
	}

	private BoundExpression MakeConversionLambda(Conversion conversion, TypeSymbol fromType, TypeSymbol toType)
	{
		string text = "p";
		ParameterSymbol parameterSymbol = _bound.SynthesizedParameter(fromType, text);
		LocalSymbol localSymbol = _bound.SynthesizedLocal(ParameterExpressionType);
		BoundLocal boundLocal = _bound.Local(localSymbol);
		BoundExpression right = ExprFactory("Parameter", _bound.Typeof(fromType), _bound.Literal(text));
		_parameterMap[parameterSymbol] = boundLocal;
		BoundExpression boundExpression = Visit(_bound.Convert(toType, _bound.Parameter(parameterSymbol), conversion));
		_parameterMap.Remove(parameterSymbol);
		return _bound.Sequence(ImmutableArray.Create(localSymbol), ImmutableArray.Create((BoundExpression)_bound.AssignmentExpression(boundLocal, right)), ExprFactory("Lambda", boundExpression, _bound.ArrayOrEmpty(ParameterExpressionType, ImmutableArray.Create((BoundExpression)boundLocal))));
	}

	private BoundExpression InitializerMemberSetter(Symbol symbol)
	{
		return symbol.Kind switch
		{
			SymbolKind.Field => _bound.Convert(MemberInfoType, _bound.FieldInfo((FieldSymbol)symbol)), 
			SymbolKind.Property => _bound.MethodInfo(((PropertySymbol)symbol).GetOwnOrInheritedSetMethod()), 
			SymbolKind.Event => _bound.Convert(MemberInfoType, _bound.FieldInfo(((EventSymbol)symbol).AssociatedField)), 
			_ => throw ExceptionUtilities.UnexpectedValue(symbol.Kind), 
		};
	}

	private BoundExpression InitializerMemberGetter(Symbol symbol)
	{
		return symbol.Kind switch
		{
			SymbolKind.Field => _bound.Convert(MemberInfoType, _bound.FieldInfo((FieldSymbol)symbol)), 
			SymbolKind.Property => _bound.MethodInfo(((PropertySymbol)symbol).GetOwnOrInheritedGetMethod()), 
			SymbolKind.Event => _bound.Convert(MemberInfoType, _bound.FieldInfo(((EventSymbol)symbol).AssociatedField)), 
			_ => throw ExceptionUtilities.UnexpectedValue(symbol.Kind), 
		};
	}

	private BoundExpression VisitInitializer(BoundExpression node, out InitializerKind kind)
	{
		switch (node.Kind)
		{
		case BoundKind.ObjectInitializerExpression:
		{
			BoundObjectInitializerExpression boundObjectInitializerExpression = (BoundObjectInitializerExpression)node;
			ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
			foreach (BoundAssignmentOperator initializer in boundObjectInitializerExpression.Initializers)
			{
				Symbol memberSymbol = ((BoundObjectInitializerMember)initializer.Left).MemberSymbol;
				BoundExpression boundExpression = VisitInitializer(initializer.Right, out var kind2);
				switch (kind2)
				{
				case InitializerKind.CollectionInitializer:
				{
					BoundExpression boundExpression4 = InitializerMemberGetter(memberSymbol);
					instance2.Add(ExprFactory("ListBind", boundExpression4, boundExpression));
					break;
				}
				case InitializerKind.Expression:
				{
					BoundExpression boundExpression3 = InitializerMemberSetter(memberSymbol);
					instance2.Add(ExprFactory("Bind", boundExpression3, boundExpression));
					break;
				}
				case InitializerKind.MemberInitializer:
				{
					BoundExpression boundExpression2 = InitializerMemberGetter(memberSymbol);
					instance2.Add(ExprFactory("MemberBind", boundExpression2, boundExpression));
					break;
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(kind2);
				}
			}
			kind = InitializerKind.MemberInitializer;
			return _bound.ArrayOrEmpty(MemberBindingType, instance2.ToImmutableAndFree());
		}
		case BoundKind.CollectionInitializerExpression:
		{
			BoundCollectionInitializerExpression boundCollectionInitializerExpression = (BoundCollectionInitializerExpression)node;
			kind = InitializerKind.CollectionInitializer;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			foreach (BoundCollectionElementInitializer initializer2 in boundCollectionInitializerExpression.Initializers)
			{
				BoundExpression item = ExprFactory("ElementInit", _bound.MethodInfo(initializer2.AddMethod), Expressions(initializer2.Arguments));
				instance.Add(item);
			}
			return _bound.ArrayOrEmpty(ElementInitType, instance.ToImmutableAndFree());
		}
		default:
			kind = InitializerKind.Expression;
			return Visit(node);
		}
	}

	private BoundExpression VisitObjectCreationExpression(BoundObjectCreationExpression node)
	{
		return VisitObjectCreationContinued(VisitObjectCreationExpressionInternal(node), node.InitializerExpressionOpt);
	}

	private BoundExpression VisitObjectCreationContinued(BoundExpression creation, BoundExpression initializerExpressionOpt)
	{
		if (initializerExpressionOpt == null)
		{
			return creation;
		}
		BoundExpression boundExpression = VisitInitializer(initializerExpressionOpt, out var kind);
		return kind switch
		{
			InitializerKind.CollectionInitializer => ExprFactory("ListInit", creation, boundExpression), 
			InitializerKind.MemberInitializer => ExprFactory("MemberInit", creation, boundExpression), 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	private BoundExpression VisitObjectCreationExpressionInternal(BoundObjectCreationExpression node)
	{
		if (node.ConstantValue != null)
		{
			return Constant(node);
		}
		if ((object)node.Constructor == null || (node.Arguments.Length == 0 && !node.Type.IsStructType()) || node.Constructor.IsDefaultValueTypeConstructor())
		{
			return ExprFactory("New", _bound.Typeof(node.Type));
		}
		BoundExpression boundExpression = _bound.ConstructorInfo(node.Constructor);
		BoundExpression boundExpression2 = _bound.Convert(_IEnumerableType.Construct(ExpressionType), Expressions(node.Arguments));
		if (node.Type.IsAnonymousType && node.Arguments.Length != 0)
		{
			NamedTypeSymbol type = (NamedTypeSymbol)node.Type;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			for (int i = 0; i < node.Arguments.Length; i++)
			{
				instance.Add(_bound.MethodInfo(AnonymousTypeManager.GetAnonymousTypeProperty(type, i).GetMethod));
			}
			return ExprFactory("New", boundExpression, boundExpression2, _bound.ArrayOrEmpty(MemberInfoType, instance.ToImmutableAndFree()));
		}
		return ExprFactory("New", boundExpression, boundExpression2);
	}

	private BoundExpression VisitParameter(BoundParameter node)
	{
		return _parameterMap[node.ParameterSymbol];
	}

	private static BoundExpression VisitPointerIndirectionOperator(BoundPointerIndirectionOperator node)
	{
		return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), node.Type);
	}

	private static BoundExpression VisitPointerElementAccess(BoundPointerElementAccess node)
	{
		return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), node.Type);
	}

	private BoundExpression VisitPropertyAccess(BoundPropertyAccess node)
	{
		BoundExpression boundExpression = (node.PropertySymbol.IsStatic ? _bound.Null(ExpressionType) : Visit(node.ReceiverOpt));
		MethodSymbol ownOrInheritedGetMethod = node.PropertySymbol.GetOwnOrInheritedGetMethod();
		BoundExpression receiverOpt = node.ReceiverOpt;
		if (receiverOpt != null && receiverOpt.Type.IsTypeParameter() && !node.ReceiverOpt.Type.IsReferenceType)
		{
			boundExpression = Convert(boundExpression, ownOrInheritedGetMethod.ReceiverType, isChecked: false);
		}
		return ExprFactory("Property", boundExpression, _bound.MethodInfo(ownOrInheritedGetMethod));
	}

	private static BoundExpression VisitSizeOfOperator(BoundSizeOfOperator node)
	{
		return new BoundBadExpression(node.Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundExpression)node), node.Type);
	}

	private BoundExpression VisitUnaryOperator(BoundUnaryOperator node)
	{
		BoundExpression operand = node.Operand;
		BoundExpression boundExpression = Visit(operand);
		UnaryOperatorKind operatorKind = node.OperatorKind;
		UnaryOperatorKind unaryOperatorKind = operatorKind & UnaryOperatorKind.OpMask;
		bool flag = (operatorKind & UnaryOperatorKind.Checked) != 0;
		string name;
		switch (unaryOperatorKind)
		{
		case UnaryOperatorKind.UnaryPlus:
			if ((object)node.MethodOpt == null)
			{
				return boundExpression;
			}
			name = "UnaryPlus";
			break;
		case UnaryOperatorKind.UnaryMinus:
			name = (flag ? "NegateChecked" : "Negate");
			break;
		case UnaryOperatorKind.LogicalNegation:
		case UnaryOperatorKind.BitwiseComplement:
			name = "Not";
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(unaryOperatorKind);
		}
		if (node.OperatorKind.OperandTypes() == UnaryOperatorKind.Enum && (operatorKind & UnaryOperatorKind.Lifted) != 0)
		{
			TypeSymbol typeSymbol = PromotedType(operand.Type.StrippedType().GetEnumUnderlyingType());
			typeSymbol = _nullableType.Construct(typeSymbol);
			boundExpression = Convert(boundExpression, operand.Type, typeSymbol, flag, isExplicit: false);
			BoundExpression node2 = ExprFactory(name, boundExpression);
			return Demote(node2, node.Type, flag);
		}
		if ((object)node.MethodOpt != null)
		{
			return ExprFactory(name, boundExpression, _bound.MethodInfo(node.MethodOpt));
		}
		return ExprFactory(name, boundExpression);
	}

	private BoundExpression ExprFactory(string name, params BoundExpression[] arguments)
	{
		return _bound.StaticCall(ExpressionType, name, arguments);
	}

	private BoundExpression ExprFactory(string name, ImmutableArray<TypeSymbol> typeArgs, params BoundExpression[] arguments)
	{
		return _bound.StaticCall(_ignoreAccessibility ? BinderFlags.IgnoreAccessibility : BinderFlags.None, ExpressionType, name, typeArgs, arguments);
	}

	private BoundExpression Constant(BoundExpression node)
	{
		return ExprFactory("Constant", _bound.Convert(_objectType, node), _bound.Typeof(node.Type));
	}
}
