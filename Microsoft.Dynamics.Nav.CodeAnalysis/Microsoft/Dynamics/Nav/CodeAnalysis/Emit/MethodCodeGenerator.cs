using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class MethodCodeGenerator : CodeGenerator
{
	private readonly MethodSymbol method;

	private readonly BoundBlock boundBlock;

	private readonly ModuleBuilder moduleBuilder;

	internal readonly Compilation compilation;

	private readonly Microsoft.CodeAnalysis.SyntaxToken methodScopeIdentifier;

	private readonly TypeSyntax methodScopeType;

	private readonly bool isCodeunitOnRunTrigger;

	private readonly bool hasImplicitOnRunRecVariable;

	private int stmtCount;

	private readonly List<DebuggerSourceSpan> sourceSpans = new List<DebuggerSourceSpan>();

	private int variableId;

	private readonly List<FieldDeclarationSyntax> fields = new List<FieldDeclarationSyntax>();

	private int assertErrorLevel;

	private static readonly Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax[] EmptyParameterArray = new Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax[0];

	private static readonly MemberDeclarationSyntax[] EmptyMemberArray = new MemberDeclarationSyntax[0];

	internal MethodSymbol Method => method;

	internal Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitExpressionStatement(BoundExpressionStatement stmt)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(EmitExpression(stmt.Expression, suppressTarget: false, isStatement: true));
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitAssignmentStatement(BoundAssignmentStatement stmt)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax sourceSyntax = EmitRightHandSideOfAssignmentStatement(stmt.Left.Type, stmt);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(EmitAssignmentExpression(stmt.Left.Type, stmt.Left, stmt.Right.Type, sourceSyntax));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitRightHandSideOfAssignmentStatement(TypeSymbol typeOfLhs, BoundAssignmentStatement stmt)
	{
		BoundExpression boundExpression = stmt.Right;
		if (typeOfLhs.NavTypeKind == NavTypeKind.Variant && CodeGenFacts.UseAssignMethod(typeOfLhs) && boundExpression is BoundConversion { ConversionKind: ConversionKind.ToVariant } boundConversion)
		{
			boundExpression = boundConversion.Operand;
			if (ConversionDotNet.IsStronglyTypedDotNetSymbol(boundConversion.Operand.Type))
			{
				return CodeGenerator.EmitGetUnderlyingValue((DotNetTypeSymbol)boundConversion.Operand.Type, EmitExpression(boundExpression, suppressTarget: true));
			}
		}
		return EmitExpression(boundExpression, suppressTarget: true);
	}

	internal Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitCompoundAssignmentStatement(BoundCompoundAssignmentStatement stmt)
	{
		NavTypeKind navTypeKind = stmt.Left.Type.NavTypeKind;
		if ((uint)(navTypeKind - 16646153) <= 1u)
		{
			DebugAssertHelper.Assert(stmt.Operator == CompoundAssignmentOperatorKind.Addition);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, EmitExpression(stmt.Left), CodeGenerator.EmitToTextConversion(stmt.Left.Type, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddExpression, EmitExpression(stmt.Left), EmitExpression(stmt.Right)))));
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(stmt.Operator switch
		{
			CompoundAssignmentOperatorKind.Addition => Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddAssignmentExpression, 
			CompoundAssignmentOperatorKind.Subtraction => Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractAssignmentExpression, 
			CompoundAssignmentOperatorKind.Multiplication => Microsoft.CodeAnalysis.CSharp.SyntaxKind.MultiplyAssignmentExpression, 
			CompoundAssignmentOperatorKind.Division => Microsoft.CodeAnalysis.CSharp.SyntaxKind.DivideAssignmentExpression, 
			_ => throw ExceptionUtilities.UnexpectedValue(stmt.Operator), 
		}, EmitExpression(stmt.Left), EmitExpression(stmt.Right)));
	}

	private static NavTypeKind GetTypeOfNextForLoopCounterValue(BoundForStatement stmt)
	{
		return BuiltInOperators.GetSignature(OverloadResolution.BinopEasyOut.OpKind(stmt.IsDownto ? BinaryOperatorKind.Subtraction : BinaryOperatorKind.Addition, stmt.LoopVariable.Type, TypeSymbol.GetNavTypeSymbol(NavTypeKind.Integer))).ReturnType.NavTypeKind;
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax EmitBlock(BoundBlock block)
	{
		ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax>.GetInstance();
		if (block != null)
		{
			foreach (BoundStatement statement in block.Statements)
			{
				EmitSingleStatement(instance, statement);
			}
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(instance.ToArrayAndFree());
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitStatement(BoundStatement stmt)
	{
		ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax>.GetInstance();
		EmitSingleStatement(instance, stmt);
		if (instance.Count == 1)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax result = instance[0];
			instance.Free();
			return result;
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(instance.ToArrayAndFree());
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitStatements(ImmutableArray<BoundStatement> stmts)
	{
		ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax>.GetInstance();
		for (int i = 0; i < stmts.Length; i++)
		{
			EmitSingleStatement(instance, stmts[i]);
		}
		if (instance.Count == 1)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax result = instance[0];
			instance.Free();
			return result;
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(instance.ToArrayAndFree());
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitStatementList(BoundStatementList stmts)
	{
		ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax>.GetInstance();
		foreach (BoundStatement statement in stmts.Statements)
		{
			EmitSingleStatement(instance, statement);
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(instance.ToArrayAndFree());
	}

	private void EmitStmtHitStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundStatement stmt)
	{
		if (!moduleBuilder.Options.SkipStmtHit)
		{
			AddSourceSpan(stmt);
			statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(CodeGenFacts.StmtHitCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(stmtCount++)))));
		}
	}

	private void EmitSingleStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundStatement stmt)
	{
		switch (stmt.Kind)
		{
		case BoundKind.ExpressionStatement:
			EmitStmtHitStatement(statementList, stmt);
			if (!IsBuiltInPropertyInvocationWithoutAssignment(stmt))
			{
				statementList.Add(EmitExpressionStatement((BoundExpressionStatement)stmt));
			}
			break;
		case BoundKind.AssignmentStatement:
			EmitStmtHitStatement(statementList, stmt);
			statementList.Add(EmitAssignmentStatement((BoundAssignmentStatement)stmt));
			break;
		case BoundKind.CompoundAssignmentStatement:
			EmitStmtHitStatement(statementList, stmt);
			statementList.Add(EmitCompoundAssignmentStatement((BoundCompoundAssignmentStatement)stmt));
			break;
		case BoundKind.NoOpStatement:
			statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EmptyStatement());
			break;
		case BoundKind.Block:
			statementList.Add(EmitBlock((BoundBlock)stmt));
			break;
		case BoundKind.IfStatement:
			statementList.Add(EmitIfStatement((BoundIfStatement)stmt));
			break;
		case BoundKind.ForStatement:
			EmitForStatement(statementList, (BoundForStatement)stmt);
			break;
		case BoundKind.ForEachStatement:
			EmitForEachStatement(statementList, (BoundForEachStatement)stmt);
			break;
		case BoundKind.RepeatStatement:
			statementList.Add(EmitRepeatStatement((BoundRepeatStatement)stmt));
			break;
		case BoundKind.WhileStatement:
			statementList.Add(EmitWhileStatement((BoundWhileStatement)stmt));
			break;
		case BoundKind.CaseStatement:
			EmitCaseStatement(statementList, (BoundCaseStatement)stmt);
			break;
		case BoundKind.WithStatement:
			statementList.Add(EmitWithStatement((BoundWithStatement)stmt));
			break;
		case BoundKind.AssertErrorStatement:
			statementList.Add(EmitAssertErrorStatement((BoundAssertErrorStatement)stmt));
			break;
		case BoundKind.BreakStatement:
			statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BreakStatement());
			break;
		case BoundKind.ExitStatement:
			EmitStmtHitStatement(statementList, stmt);
			EmitExitStatement(statementList, (BoundExitStatement)stmt);
			break;
		default:
			throw ExceptionUtilities.UnreachableWithMessage(string.Format(CultureInfo.InvariantCulture, "The statement {0} of kind {1} could not be emitted.", stmt.Syntax.ToReportingString(), stmt.Kind));
		}
	}

	private static bool IsBuiltInPropertyInvocationWithoutAssignment(BoundStatement stmt)
	{
		BoundExpressionStatement boundExpressionStatement = (BoundExpressionStatement)stmt;
		if (boundExpressionStatement.Expression.Kind == BoundKind.Call)
		{
			BoundCall boundCall = (BoundCall)boundExpressionStatement.Expression;
			MethodSymbol methodSymbol = boundCall.Method;
			if (methodSymbol.MethodKind == MethodKind.BuiltInMethod && boundCall.Arguments.Length == 0 && ((BuiltInMethodTypeSymbol)methodSymbol).IsProperty)
			{
				return true;
			}
		}
		return false;
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitIfStatement(BoundIfStatement stmt)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax ifStatementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(EmitConditionExpression(stmt.Condition), EmitStatement(stmt.Consequence));
		if (stmt.AlternativeOpt != null)
		{
			ifStatementSyntax = ifStatementSyntax.WithElse(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ElseClause(EmitStatement(stmt.AlternativeOpt)));
		}
		return ifStatementSyntax;
	}

	private void EmitForEachStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundForEachStatement stmt)
	{
		if (stmt.Expression.Type.NavTypeKind == NavTypeKind.DotNet)
		{
			EmitDotNetForEachStatement(statementList, stmt);
		}
		else
		{
			EmitNativeForEachStatement(statementList, stmt);
		}
	}

	private void EmitDotNetForEachStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundForEachStatement stmt)
	{
		string text = CreateTemporaryVariableName();
		statementList.Add(EmitDotNetEnumeratorVariableInitialization(stmt.Expression, text));
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax targetExpression = EmitExpression(stmt.Expression);
		statementList.Add(EmitDotNetCallToGetEnumerator(text, targetExpression));
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax condition = EmitSyntheticDotNetMethodCall(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(text), CodeGenFacts.NavDotNetInvokeMethodNameIdentifier, "MoveNext", CodeGenFacts.ClrBoolType);
		string text2 = CreateTemporaryVariableName();
		LocalDeclarationStatementSyntax localDeclarationStatementSyntax = EmitDotNetForEachCurrentValue(text, text2);
		Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax statement = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(localDeclarationStatementSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(EmitAssignmentToForEachVariable(stmt, text2)), EmitStatement(stmt.Body));
		Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax item = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.WhileStatement(condition, statement);
		statementList.Add(item);
	}

	private LocalDeclarationStatementSyntax EmitDotNetForEachCurrentValue(string iEnumeratorVariableName, string currentValueTemporaryVariable)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax value = EmitSyntheticDotNetMethodCall(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(iEnumeratorVariableName), CodeGenFacts.NavDotNetInvokePropertyGetMethodNameIdentifier, "Current", CodeGenFacts.NavDotNetType);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.ClrVarType).WithVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(currentValueTemporaryVariable)).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(value)))));
	}

	private static Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitSyntheticDotNetMethodCall(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax receiver, Microsoft.CodeAnalysis.SyntaxToken methodInvoke, string methodName, TypeSyntax returnType)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(receiver, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(methodInvoke).WithTypeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(returnType))))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(methodName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(268435455u)));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitDotNetCallToGetEnumerator(string tempIterationVarName, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax targetExpression)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = EmitSyntheticDotNetMethodCall(targetExpression, CodeGenFacts.NavDotNetInvokeMethodNameIdentifier, "GetEnumerator", CodeGenFacts.NavDotNetTypeIdentifier);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(tempIterationVarName), CodeGenFacts.ALAssignMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(value: true))));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitDotNetEnumeratorVariableInitialization(BoundExpression forEachExpression, string tempIterationVarName)
	{
		bool value = forEachExpression.ExpressionSymbol.HasAttribute(AttributeKind.RunOnClient);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.NavDotNetType).WithVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(tempIterationVarName)).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.NavDotNetType, CodeGenFacts.NavDotNetCreateDotNetEnumeratorMethodNameIdentifier)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(value))))))));
	}

	private void EmitNativeForEachStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundForEachStatement stmt)
	{
		string text = CreateTemporaryVariableName();
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = EmitExpression(stmt.Expression);
		Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax statement = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(EmitAssignmentToForEachVariable(stmt, text)), EmitStatement(stmt.Body));
		Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax item = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ForEachStatement(CodeGenFacts.ClrVarType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(text), expression, statement);
		statementList.Add(item);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitAssignmentToForEachVariable(BoundForEachStatement forEachStatement, string tempIterationVarName)
	{
		TypeSymbol enumeratorElementType = forEachStatement.Expression.Type.GetEnumeratorElementType();
		Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax sourceSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(tempIterationVarName);
		if (!forEachStatement.ElementConversion.IsIdentity)
		{
			BoundTemporary operand = new BoundTemporary(forEachStatement.Syntax, tempIterationVarName, enumeratorElementType);
			BoundConversion boundConversion = new BoundConversion(forEachStatement.Syntax, operand, forEachStatement.ElementConversion.Kind, LookupResultKind.Viable, null, null, isArrayIndex: false, null, forEachStatement.IterationVariable.Type);
			return EmitAssignmentExpression(boundConversion.Type, forEachStatement.IterationVariable, boundConversion.Type, EmitExpression(boundConversion));
		}
		return EmitAssignmentExpression(forEachStatement.IterationVariable.Type, forEachStatement.IterationVariable, forEachStatement.IterationVariable.Type, sourceSyntax);
	}

	private void EmitForStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundForStatement stmt)
	{
		switch (stmt.LoopVariable.Type.NavTypeKind)
		{
		case NavTypeKind.Boolean:
			EmitBooleanForStatement(statementList, stmt);
			return;
		case NavTypeKind.Option:
		case NavTypeKind.Enum:
			EmitOptionForStatement(statementList, stmt);
			return;
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax left = EmitExpression(stmt.LoopVariable);
		statementList.Add(EmitForStatementInitExpression(stmt));
		EmitStmtHitStatement(statementList, stmt);
		Microsoft.CodeAnalysis.SyntaxToken identifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(CreateTemporaryVariableName());
		Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax right = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(identifier);
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(stmt.LoopVariable.ExpressionSymbol.GetRuntimeTypeSyntax()).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(identifier).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(EmitExpression(stmt.EndValue))))));
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(stmt.IsDownto ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractExpression : Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddExpression, left, CodeGenHelper.LiteralExpression(1));
		if (stmt.LoopVariable.Type.IsNumericType())
		{
			expressionSyntax = CodeGenerator.EmitExplicitNumericConversion(GetTypeOfNextForLoopCounterValue(stmt), stmt.LoopVariable.Type.NavTypeKind, expressionSyntax);
		}
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ForStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(EmitStatement(stmt.Body), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(stmt.IsDownto ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanOrEqualExpression : Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression, left, right), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BreakStatement()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(EmitAssignmentExpression(stmt.LoopVariable.Type, stmt.LoopVariable, stmt.LoopVariable.Type, expressionSyntax)))).WithCondition(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(stmt.IsDownto ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression : Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanOrEqualExpression, left, right)));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax EmitForStatementInitExpression(BoundForStatement stmt)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax sourceSyntax = EmitExpression(stmt.InitialValue);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(EmitAssignmentExpression(stmt.LoopVariable.Type, stmt.LoopVariable, stmt.InitialValue.Type, sourceSyntax));
	}

	private void EmitBooleanForStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundForStatement stmt)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression = EmitExpression(stmt.LoopVariable);
		statementList.Add(EmitForStatementInitExpression(stmt));
		Microsoft.CodeAnalysis.SyntaxToken identifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(CreateTemporaryVariableName());
		Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax identifierNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(identifier);
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.ClrInt32Type).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(identifier).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(CodeGenFacts.ALCompilerToIntegerCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)))))));
		EmitStmtHitStatement(statementList, stmt);
		Microsoft.CodeAnalysis.SyntaxToken identifier2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(CreateTemporaryVariableName());
		Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax right = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(identifier2);
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.ClrInt32Type).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(identifier2).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(CodeGenFacts.ALCompilerToIntegerCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(EmitExpression(stmt.EndValue))))))));
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ForStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(EmitAssignmentExpression(stmt.LoopVariable.Type, stmt.LoopVariable, stmt.LoopVariable.Type, CodeGenFacts.ALCompilerToBooleanCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(identifierNameSyntax)))), EmitStatement(stmt.Body), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(stmt.IsDownto ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanOrEqualExpression : Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression, identifierNameSyntax, right), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BreakStatement()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, identifierNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(stmt.IsDownto ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractExpression : Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddExpression, identifierNameSyntax, CodeGenHelper.LiteralExpression(1)))))).WithCondition(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(stmt.IsDownto ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression : Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanOrEqualExpression, identifierNameSyntax, right)));
	}

	private void EmitOptionForStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundForStatement stmt)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = EmitExpression(stmt.LoopVariable);
		statementList.Add(EmitForStatementInitExpression(stmt));
		Microsoft.CodeAnalysis.SyntaxToken identifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(CreateTemporaryVariableName());
		Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax right = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(identifier);
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.NavOptionType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(identifier).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(EmitExpression(stmt.EndValue))))));
		EmitStmtHitStatement(statementList, stmt);
		ArgumentSyntax argumentSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(stmt.IsDownto ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.SubtractExpression : Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddExpression, expressionSyntax, CodeGenHelper.LiteralExpression(1)));
		Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax sourceSyntax = CodeGenFacts.NavOptionCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.MemberAccessExpression(expressionSyntax, CodeGenFacts.NavOptionMetadataPropertyNameSyntax)), argumentSyntax);
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax expressionStatementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(EmitAssignmentExpression(stmt.LoopVariable.Type, stmt.LoopVariable, stmt.LoopVariable.Type, sourceSyntax));
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ForStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(EmitStatement(stmt.Body), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(stmt.IsDownto ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanOrEqualExpression : Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression, expressionSyntax, right), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BreakStatement()), expressionStatementSyntax)).WithCondition(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(stmt.IsDownto ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression : Microsoft.CodeAnalysis.CSharp.SyntaxKind.LessThanOrEqualExpression, expressionSyntax, right)));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitRepeatStatement(BoundRepeatStatement stmt)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.DoStatement(EmitStatementList(stmt.Body), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PrefixUnaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LogicalNotExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(EmitConditionExpression(stmt.Condition))));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitWhileStatement(BoundWhileStatement stmt)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.WhileStatement(EmitConditionExpression(stmt.Condition), EmitStatement(stmt.Body));
	}

	private void EmitCaseStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundCaseStatement stmt)
	{
		bool flag = true;
		foreach (BoundCaseLine caseLine in stmt.CaseLines)
		{
			if (Enumerable.Any(caseLine.Expressions, (BoundExpression expr) =>
			{
				if (expr.Kind == BoundKind.Literal)
				{
					TypeSymbol type = expr.Type;
					if ((object)type == null)
					{
						return false;
					}
					return type.NavTypeKind == NavTypeKind.Decimal;
				}
				return true;
			}))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			EmitCaseStatementAsSwitch(statementList, stmt);
		}
		else
		{
			EmitCaseStatementAsIfThenElse(statementList, stmt);
		}
	}

	private void EmitCaseStatementAsSwitch(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundCaseStatement stmt)
	{
		ArrayBuilder<SwitchSectionSyntax> instance = ArrayBuilder<SwitchSectionSyntax>.GetInstance();
		foreach (BoundCaseLine caseLine in stmt.CaseLines)
		{
			SwitchSectionSyntax switchSectionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection();
			SwitchLabelSyntax[] items = caseLine.Expressions.Select((BoundExpression e) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(EmitExpression(e))).ToArray();
			instance.Add(switchSectionSyntax.AddLabels(items).AddStatements(EmitStatement(caseLine.Statement), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BreakStatement()));
		}
		if (stmt.CaseElse != null)
		{
			instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.DefaultSwitchLabel()).AddStatements(stmt.CaseElse.Statements.Select(EmitStatement).ToArray())
				.AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BreakStatement()));
		}
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchStatement(EmitExpression(stmt.Expression)).AddSections(instance.ToArrayAndFree()));
	}

	private void EmitCaseStatementAsIfThenElse(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundCaseStatement stmt)
	{
		Microsoft.CodeAnalysis.SyntaxToken syntaxToken = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(CreateTemporaryVariableName());
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(stmt.Expression.ExpressionSymbol.GetRuntimeTypeSyntax()).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(syntaxToken).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(EmitExpression(stmt.Expression))))));
		Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statementSyntax = null;
		if (stmt.CaseElse != null)
		{
			statementSyntax = EmitStatements(stmt.CaseElse.Statements);
		}
		for (int num = stmt.CaseLines.Length - 1; num >= 0; num--)
		{
			statementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(EmitCaseLineConditionExpression(stmt.Expression, stmt.CaseLines[num], syntaxToken), EmitCaseLineStatement(stmt.CaseLines[num].Statement), (statementSyntax != null) ? Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ElseClause(statementSyntax) : null);
		}
		statementList.Add(statementSyntax);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitCaseLineStatement(BoundStatement stmt)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statementSyntax = EmitStatement(stmt);
		if (stmt.Kind == BoundKind.IfStatement)
		{
			statementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(statementSyntax);
		}
		return statementSyntax;
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitCaseLineConditionExpression(BoundExpression caseValue, BoundCaseLine caseLine, Microsoft.CodeAnalysis.SyntaxToken switchExpression)
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = null;
		for (int i = 0; i < caseLine.Expressions.Length; i++)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax2 = EmitCaseLineSingleExpression(caseValue, caseLine.Expressions[i], switchExpression);
			expressionSyntax = ((expressionSyntax == null) ? expressionSyntax2 : Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.LogicalOrExpression, expressionSyntax, expressionSyntax2));
		}
		return EmitConditionExpression(expressionSyntax, caseLine);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitCaseLineSingleExpression(BoundExpression switchValue, BoundExpression caseExpression, Microsoft.CodeAnalysis.SyntaxToken switchExpression)
	{
		return EmitComparisonExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(switchExpression), switchValue, new BoundExpression[1] { caseExpression });
	}

	private void EmitExitStatement(ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax> statementList, BoundExitStatement stmt)
	{
		if (stmt.ReturnValue != null)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = EmitExpression(stmt.ReturnValue);
			if (assertErrorLevel > 0)
			{
				statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(method.ReturnValue.GetRuntimeTypeSyntax()).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator("__\\u03b3tmp__" + assertErrorLevel).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(expressionSyntax)))));
			}
			else
			{
				statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(GetReturnValueFieldIdentifier())), expressionSyntax)));
			}
		}
		statementList.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement());
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitWithStatement(BoundWithStatement stmt)
	{
		return EmitStatement(stmt.Statement);
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax EmitAssertErrorStatement(BoundAssertErrorStatement stmt)
	{
		assertErrorLevel++;
		try
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statementSyntax = EmitStatement(stmt.Statement);
			if (statementSyntax.Kind() != Microsoft.CodeAnalysis.CSharp.SyntaxKind.Block)
			{
				statementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(statementSyntax);
			}
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(CodeGenFacts.AssertErrorCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedLambdaExpression(statementSyntax))));
		}
		finally
		{
			assertErrorLevel--;
		}
	}

	private string CreateTemporaryVariableName()
	{
		return "@tmp" + variableId++.ToString(CultureInfo.InvariantCulture);
	}

	public static MethodCodeGenerator Create(MethodSymbol method, BoundBlock block, ModuleBuilder moduleBuilder, CompilationOptions compilationOptions)
	{
		return new MethodCodeGenerator(method, block, moduleBuilder, compilationOptions);
	}

	private MethodCodeGenerator(MethodSymbol method, BoundBlock boundBlock, ModuleBuilder moduleBuilder, CompilationOptions compilationOptions)
		: base(compilationOptions, moduleBuilder.Options)
	{
		this.method = method;
		this.boundBlock = boundBlock;
		this.moduleBuilder = moduleBuilder;
		string methodScopeName = GetMethodScopeName(method);
		methodScopeIdentifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(methodScopeName);
		methodScopeType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName(methodScopeIdentifier.Text);
		if (this.method.IsCodeunitOnRunTrigger())
		{
			isCodeunitOnRunTrigger = true;
			hasImplicitOnRunRecVariable = this.method.LocalVariables.Any((VariableSymbol v) => v.Name == "Rec" && v is SynthesizedLocalVariableSymbol);
		}
	}

	private string GetMethodScopeName(MethodSymbol methodSymbol)
	{
		if (methodSymbol.CanBeOverloaded())
		{
			string text = method.MetadataName;
			string overloadedMethodMetadataNameSuffix = methodSymbol.GetOverloadedMethodMetadataNameSuffix();
			if (text.EndsWith(overloadedMethodMetadataNameSuffix, StringComparison.Ordinal))
			{
				text = text.Substring(0, text.LastIndexOf(overloadedMethodMetadataNameSuffix, StringComparison.Ordinal));
			}
			return text + "_Scope_" + ((method.Id < 0) ? ("_" + Math.Abs(method.Id).ToString(CultureInfo.InvariantCulture)) : method.Id.ToString(CultureInfo.InvariantCulture));
		}
		return method.MetadataName + "_Scope";
	}

	internal void Generate()
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDeclaration = EmitMethod();
		ClassDeclarationSyntax methodScopeDeclaration = EmitScopeClass();
		moduleBuilder.SetMethodBody(method, new EmittedMethod(method, methodDeclaration, methodScopeDeclaration, fields.ToImmutableArray()));
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax EmitMethod()
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.ArgumentListSyntax argumentListSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())).AddArguments((from p in GetMethodParameters()
			select Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(p.MetadataName))).ToArray());
		if (isCodeunitOnRunTrigger)
		{
			argumentListSyntax = argumentListSyntax.AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(CodeGenFacts.RecParameterIdentifier)));
		}
		Microsoft.CodeAnalysis.CSharp.Syntax.StatementSyntax statementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ScopeLocalIdentifier, CodeGenFacts.RunMethodIdentifier)));
		if (method.ReturnValue != null)
		{
			statementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(statementSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ScopeLocalIdentifier, GetReturnValueFieldIdentifier())));
		}
		UsingStatementSyntax usingStatementSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(methodScopeType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(CodeGenFacts.ScopeLocalIdentifier).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(methodScopeType, argumentListSyntax, null)))), null, statementSyntax);
		Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDecl = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(method.ReturnValue.GetRuntimeTypeSyntax(), method.MetadataName).AddParameterListParameters(EmitMethodParameters().ToArray()).WithBody(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(usingStatementSyntax));
		EmitAccessModifiers(ref methodDecl);
		EmitAttributes(ref methodDecl);
		return methodDecl;
	}

	private void EmitAccessModifiers(ref Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDecl)
	{
		if (method.IsTrigger())
		{
			if (method.ContainingSymbol.Kind.IsExtensionObject() || IsTestRunnerTrigger())
			{
				methodDecl = methodDecl.AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword));
			}
			else if (method.ContainingSymbol.Kind.IsApplicationObject() || Method.ContainingSymbol.Kind == SymbolKind.RequestPage)
			{
				methodDecl = methodDecl.AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword));
			}
			else
			{
				methodDecl = ((method.ContainingSymbol is FieldSymbol || method.ContainingSymbol is SourceChangeModifyFieldSymbol) ? methodDecl.AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)) : methodDecl.AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)));
			}
		}
		else if (method.IsDotNetEventTrigger())
		{
			methodDecl = methodDecl.AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword));
		}
		else
		{
			methodDecl = methodDecl.AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(method.IsLocal ? Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword : Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword));
		}
	}

	private bool IsTestRunnerTrigger()
	{
		if (method.ContainingSymbol.Kind == SymbolKind.Codeunit && method is SourceTriggerSymbol sourceTriggerSymbol)
		{
			return sourceTriggerSymbol.TriggerInfo.IsCodeunitTestRunnerTriggerTypeInfo();
		}
		return false;
	}

	private void EmitAttributes(ref Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDecl)
	{
		PooledList<AttributeSyntax> instance = PooledList<AttributeSyntax>.GetInstance();
		try
		{
			foreach (AttributeSymbol attribute in method.Attributes)
			{
				AttributeTypeInfo attributeInfo = attribute.AttributeInfo;
				if (attributeInfo != null && !attributeInfo.NoEmit)
				{
					AttributeSyntax item = attributeInfo.Emitter.EmitAttribute(this, attributeInfo, attribute);
					instance.Add(item);
				}
			}
			if (method.IsDotNetEventTrigger())
			{
				instance.Add(CodeGenFacts.NavFunctionVisibilityAttribute_Internal);
			}
			if (instance.Count > 0)
			{
				methodDecl = methodDecl.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(instance)));
			}
		}
		finally
		{
			instance.Free();
		}
	}

	private ClassDeclarationSyntax EmitScopeClass()
	{
		TypeSyntax objectTypeWithSuffixTypeSyntax = method.GetContainingSymbolOfType<ObjectTypeSymbol>().GetObjectTypeWithSuffixTypeSyntax(moduleBuilder.Options);
		ClassDeclarationSyntax classDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ClassDeclaration(methodScopeIdentifier).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)).AddBaseListTypes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SimpleBaseType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(method.IsTrigger() ? CodeGenFacts.NavTriggerMethodScopeTypeIdentifier : (method.IsEvent ? CodeGenFacts.NavEventMethodScopeTypeIdentifier : CodeGenFacts.NavMethodScopeTypeIdentifier), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList().AddArguments(objectTypeWithSuffixTypeSyntax))))
			.AddMembers(CodeGenHelper.FieldDeclaration(CodeGenFacts.ScopeIdFieldIdentifier, CodeGenFacts.ScopeIdType, Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword, Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword), CodeGenHelper.FieldDeclaration(CodeGenFacts.EventScopeFieldIdentifier, CodeGenFacts.NavEventScopeType, Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword, Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword))
			.AddMembers(EmitMethodScopeParameterFields());
		if (method.ReturnValue != null)
		{
			classDeclarationSyntax = classDeclarationSyntax.AddMembers(EmitMethodScopeReturnValueField());
		}
		if (!HasEmptyMethodBody())
		{
			classDeclarationSyntax = classDeclarationSyntax.AddMembers(EmitMethodScopeLocalFields());
		}
		bool flag = false;
		if (EmitOptions.NonDebuggableEmit || method.IsNonDebuggable)
		{
			classDeclarationSyntax = classDeclarationSyntax.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(CodeGenFacts.ClrBoolType, CodeGenFacts.IsDebuggablePropertyIdentifier).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).WithExpressionBody(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrowExpressionClause(CodeGenFacts.FalseLiteralExpression))
				.WithSemicolonToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SemicolonToken)));
			flag = true;
		}
		classDeclarationSyntax = classDeclarationSyntax.AddMembers(CodeGenHelper.PropertyDeclaration("RawScopeId", CodeGenFacts.ScopeIdType, CodeGenHelper.MemberAccessExpression(methodScopeIdentifier, CodeGenFacts.ScopeIdFieldIdentifier), Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword, Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword), CodeGenHelper.PropertyDeclaration("EventScope", CodeGenFacts.NavEventScopeType, CodeGenHelper.MemberAccessExpression(methodScopeIdentifier, CodeGenFacts.EventScopeFieldIdentifier), Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword, Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword), EmitScopeConstructor(objectTypeWithSuffixTypeSyntax), EmitScopeRunMethod());
		if (method.MethodKind == MethodKind.Method || !method.Name.Equals(method.MetadataName, StringComparison.OrdinalIgnoreCase))
		{
			classDeclarationSyntax = classDeclarationSyntax.AddAttributeLists(CodeGenHelper.NavNameAttribute(method.Name));
		}
		if (!moduleBuilder.Options.SkipStmtHit)
		{
			if (boundBlock != null)
			{
				Microsoft.Dynamics.Nav.CodeAnalysis.Text.Location applicationObjectFirstLocationIncludingTrivia = boundBlock.Syntax.GetApplicationObjectFirstLocationIncludingTrivia();
				sourceSpans.Add(new DebuggerSourceSpan(boundBlock.Syntax.GetLastToken().GetLocation().GetLineSpan(), applicationObjectFirstLocationIncludingTrivia.GetLineSpan().StartLinePosition.Line));
				classDeclarationSyntax = classDeclarationSyntax.AddAttributeLists(CodeGenHelper.SourceSpansAttribute(sourceSpans));
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			classDeclarationSyntax = classDeclarationSyntax.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(CodeGenFacts.DebuggerNonUserCodeAttribute));
		}
		return classDeclarationSyntax;
	}

	private ConstructorDeclarationSyntax EmitScopeConstructor(TypeSyntax applicationObjectType)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ConstructorDeclaration(methodScopeIdentifier).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InternalKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.InternalParentParameterIdentifier).WithType(applicationObjectType))
			.AddParameterListParameters(EmitMethodParameters().ToArray())
			.WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ConstructorInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxKind.BaseConstructorInitializer, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(CodeGenFacts.InternalParentParameterIdentifier)))))
			.WithBody(EmitScopeConstructorBody());
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax EmitScopeConstructorBody()
	{
		Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax blockSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block();
		foreach (ParameterSymbol methodParameter in GetMethodParameters())
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax identifierNameSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(methodParameter.MetadataName);
			Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax left = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), identifierNameSyntax);
			Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax right = EmitParameterToMethodScopeFieldTransformation(methodParameter, identifierNameSyntax);
			blockSyntax = blockSyntax.AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, left, right)));
		}
		if (hasImplicitOnRunRecVariable && !HasEmptyMethodBody())
		{
			blockSyntax = blockSyntax.AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(CodeGenFacts.RecFieldIdentifier)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(CodeGenFacts.RecParameterIdentifier))));
		}
		if (method.LocalVariableCount != 0 && !HasEmptyMethodBody())
		{
			foreach (VariableSymbol localVariable in method.LocalVariables)
			{
				if (!(localVariable is SynthesizedLocalVariableSymbol))
				{
					Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expressionSyntax = CodeGenerator.EmitVariableInitializer(localVariable, moduleBuilder.Options);
					if (expressionSyntax != null)
					{
						blockSyntax = blockSyntax.AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(localVariable.MetadataName)), expressionSyntax)));
					}
				}
			}
		}
		return blockSyntax;
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitParameterToMethodScopeFieldTransformation(ParameterSymbol parameterSymbol, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax parameter)
	{
		if (!parameterSymbol.IsVar)
		{
			if (parameterSymbol.Type.IsTextType())
			{
				TypeSymbol type = parameterSymbol.Type;
				return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(parameter, CodeGenFacts.ModifyLengthMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(type.HasLength ? type.Length : 0)));
			}
			if (parameterSymbol.Type.NavTypeKind == NavTypeKind.Option)
			{
				SourceOptionTypeSymbol sourceOptionTypeSymbol = parameterSymbol.Type as SourceOptionTypeSymbol;
				string value = ((sourceOptionTypeSymbol != null) ? sourceOptionTypeSymbol.OptionMembers : string.Empty);
				string text = methodScopeIdentifier.ToString() + "_" + parameterSymbol.MetadataName + "_metadata";
				fields.Add(CodeGenHelper.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(text), CodeGenFacts.NclOptionMetadataType, CodeGenFacts.NclOptionMetadataCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(value))), Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword, Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword));
				return CodeGenFacts.NavOptionCreateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(text)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(parameter));
			}
			if (CodeGenFacts.ArgumentShouldBeCloned(parameterSymbol))
			{
				Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(parameter, CodeGenFacts.NavValueByValueMethodNameSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()));
				NavTypeKind navTypeKind = parameterSymbol.Type.NavTypeKind;
				if (navTypeKind == NavTypeKind.Record)
				{
					RecordTypeSymbol recordTypeSymbol = (RecordTypeSymbol)parameterSymbol.Type;
					invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(recordTypeSymbol.Temporary)));
				}
				if (parameterSymbol.Type.IsArray())
				{
					ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)parameterSymbol.Type;
					if (arrayTypeSymbol.ElementType.NavTypeKind == NavTypeKind.Record)
					{
						invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(((RecordTypeSymbol)arrayTypeSymbol.ElementType).Temporary)));
					}
				}
				return invocationExpressionSyntax;
			}
		}
		return parameter;
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax[] EmitMethodParameters()
	{
		if (method.ParameterCount == 0 && !isCodeunitOnRunTrigger)
		{
			return EmptyParameterArray;
		}
		ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax> instance = ArrayBuilder<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax>.GetInstance();
		foreach (ParameterSymbol methodParameter in GetMethodParameters())
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameterSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(methodParameter.MetadataName));
			ApplicationObjectTypeSymbol applicationObjectTypeSymbol = ((methodParameter.Type.Kind != SymbolKind.Enum) ? (methodParameter.Type.GetAliasedTypeSymbol() as ApplicationObjectTypeSymbol) : null);
			parameterSyntax = ((!(applicationObjectTypeSymbol != null)) ? parameterSyntax.WithType(EmitParameterType(methodParameter)) : parameterSyntax.WithType(((Symbol)methodParameter).GetRuntimeTypeSyntax()).AddAttributeLists(applicationObjectTypeSymbol.NavObjectIdAttribute()));
			if (methodParameter.IsVar && (applicationObjectTypeSymbol != null || methodParameter.IsArray))
			{
				parameterSyntax = parameterSyntax.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(CodeGenFacts.NavByReferenceAttributeNameSyntax)));
			}
			parameterSyntax = AddTypeInfoAttributeIfNeeded(methodParameter, parameterSyntax);
			instance.Add(parameterSyntax);
		}
		if (isCodeunitOnRunTrigger)
		{
			Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax item = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.RecParameterIdentifier).WithType(CodeGenFacts.INavRecordHandleType).AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(CodeGenFacts.NavByReferenceAttributeNameSyntax)))
				.AddAttributeLists(CodeGenHelper.NavObjectIdAttribute(0));
			instance.Add(item);
		}
		return instance.ToArrayAndFree();
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax AddTypeInfoAttributeIfNeeded(ParameterSymbol paramSymbol, Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter)
	{
		if (method.MethodKind == MethodKind.EventTrigger && paramSymbol.Type.NavTypeKind == NavTypeKind.DotNet)
		{
			DotNetTypeSymbol dotNetTypeSymbol = (DotNetTypeSymbol)paramSymbol.Type;
			string metadataName = dotNetTypeSymbol.MetadataName;
			string assemblyName = dotNetTypeSymbol.AssemblyName;
			AttributeSyntax attributeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(CodeGenFacts.NavDotNetTypeInfoAttributeNameSyntax).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.LiteralExpression(metadataName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.LiteralExpression(assemblyName)));
			parameter = parameter.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(attributeSyntax));
		}
		return parameter;
	}

	private static TypeSyntax EmitParameterType(ParameterSymbol parameter)
	{
		if (parameter.ShouldBePassedByRef())
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName("ByRef").AddTypeArgumentListArguments(((Symbol)parameter).GetRuntimeTypeSyntax());
		}
		if (parameter.Type.NavTypeKind == NavTypeKind.Option)
		{
			return CodeGenFacts.RuntimeParameterOptionType;
		}
		return ((Symbol)parameter).GetRuntimeTypeSyntax();
	}

	private static TypeSyntax EmitMethodScopeFieldType(ParameterSymbol parameter)
	{
		if (parameter.ShouldBePassedByRef())
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName("ByRef").AddTypeArgumentListArguments(((Symbol)parameter).GetRuntimeTypeSyntax());
		}
		return ((Symbol)parameter).GetRuntimeTypeSyntax();
	}

	private MemberDeclarationSyntax[] EmitMethodScopeParameterFields()
	{
		if (method.ParameterCount == 0 && !isCodeunitOnRunTrigger)
		{
			return EmptyMemberArray;
		}
		ArrayBuilder<MemberDeclarationSyntax> instance = ArrayBuilder<MemberDeclarationSyntax>.GetInstance();
		foreach (ParameterSymbol methodParameter in GetMethodParameters())
		{
			TypeSyntax type = EmitMethodScopeFieldType(methodParameter);
			FieldDeclarationSyntax fieldDeclarationSyntax = CodeGenHelper.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(methodParameter.MetadataName), type, Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword).AddAttributeLists(CodeGenHelper.NavNameAttribute(methodParameter.Name));
			if (EmitOptions.NonDebuggableEmit || method.IsNonDebuggable)
			{
				fieldDeclarationSyntax = fieldDeclarationSyntax.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(CodeGenFacts.DebuggerBrowsableNeverAttribute));
			}
			instance.Add(fieldDeclarationSyntax);
		}
		return instance.ToArrayAndFree();
	}

	private MemberDeclarationSyntax[] EmitMethodScopeLocalFields()
	{
		if (method.LocalVariableCount == 0)
		{
			return EmptyMemberArray;
		}
		ArrayBuilder<MemberDeclarationSyntax> instance = ArrayBuilder<MemberDeclarationSyntax>.GetInstance();
		foreach (VariableSymbol localVariable in method.LocalVariables)
		{
			TypeSyntax type = CodeGenerator.EmitVariableType(localVariable);
			FieldDeclarationSyntax fieldDeclarationSyntax = CodeGenHelper.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(localVariable.MetadataName), type, CodeGenerator.EmitVariableFieldInitializer(localVariable), CodeGenerator.EmitMethodLocalFieldDeclarationModifiers(localVariable.Type)).AddAttributeLists(CodeGenHelper.NavNameAttribute(localVariable.Name));
			if (EmitOptions.NonDebuggableEmit || method.IsNonDebuggable || localVariable.IsNonDebuggable)
			{
				fieldDeclarationSyntax = fieldDeclarationSyntax.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(CodeGenFacts.DebuggerBrowsableNeverAttribute));
			}
			instance.Add(fieldDeclarationSyntax);
		}
		return instance.ToArrayAndFree();
	}

	private MemberDeclarationSyntax EmitMethodScopeReturnValueField()
	{
		DebugAssertHelper.Assert(method.ReturnValue != null);
		FieldDeclarationSyntax fieldDeclarationSyntax = CodeGenHelper.FieldDeclaration(method.ReturnValue.IsNamed ? Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(method.ReturnValue.MetadataName) : CodeGenFacts.DefaultReturnValueIdentifier, method.ReturnValue.GetRuntimeTypeSyntax(), CodeGenerator.EmitReturnValueFieldInitializer(method), Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword);
		fieldDeclarationSyntax = ((!method.ReturnValue.IsNamed) ? fieldDeclarationSyntax.AddAttributeLists(CodeGenHelper.ReturnValueAttribute()) : fieldDeclarationSyntax.AddAttributeLists(CodeGenHelper.ReturnValueAttribute(method.ReturnValue.Name), CodeGenHelper.NavNameAttribute(method.ReturnValue.Name)));
		if (EmitOptions.NonDebuggableEmit || method.IsNonDebuggable)
		{
			fieldDeclarationSyntax = fieldDeclarationSyntax.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(CodeGenFacts.DebuggerBrowsableNeverAttribute));
		}
		return fieldDeclarationSyntax;
	}

	private Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax EmitScopeRunMethod()
	{
		EmitBaseParent = true;
		try
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrVoidType, "OnRun").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).WithBody(EmitBlock(boundBlock));
		}
		finally
		{
			EmitBaseParent = false;
		}
	}

	private Microsoft.CodeAnalysis.SyntaxToken GetReturnValueFieldIdentifier()
	{
		if (!method.ReturnValue.IsNamed)
		{
			return CodeGenFacts.DefaultReturnValueIdentifier;
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(method.ReturnValue.MetadataName);
	}

	private bool HasEmptyMethodBody()
	{
		if (boundBlock != null)
		{
			return boundBlock.Statements.Length == 0;
		}
		return true;
	}

	private void AddSourceSpan(BoundNode node)
	{
		Microsoft.Dynamics.Nav.CodeAnalysis.Text.Location applicationObjectFirstLocationIncludingTrivia = node.Syntax.GetApplicationObjectFirstLocationIncludingTrivia();
		sourceSpans.Add(new DebuggerSourceSpan(node.Syntax.Location.GetLineSpan(), applicationObjectFirstLocationIncludingTrivia.GetLineSpan().StartLinePosition.Line));
	}

	private ImmutableArray<ParameterSymbol> GetMethodParameters()
	{
		if (isCodeunitOnRunTrigger)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		return method.Parameters;
	}

	protected override Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitConditionExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression, BoundNode node)
	{
		if (moduleBuilder.Options.SkipStmtHit)
		{
			return expression;
		}
		AddSourceSpan(node);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.BitwiseAndExpression, CodeGenFacts.CStmtHitCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(stmtCount++))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(expression));
	}

	protected override Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitGlobalScopeAccess(BoundExpression node)
	{
		if (CodeGenerator.IsExpressionNestedAndAccessingParent(node))
		{
			return CodeGenerator.EmitAccessToParentScopeThroughCurrObject(CodeGenerator.EmitBaseDotParentExpression(), node);
		}
		return CodeGenerator.EmitBaseDotParentExpression();
	}

	protected internal override Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotParentObjectExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax expression)
	{
		if (Method.GetContainingSymbolOfType<ApplicationObjectExtensionTypeSymbol>() != null)
		{
			expression = CodeGenHelper.MemberAccessExpression(expression, CodeGenFacts.ParentObjectPropertyNameSyntax);
		}
		return expression;
	}

	protected internal override Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax EmitDotParentExpression()
	{
		return EmitDotParentObjectExpression(CodeGenerator.EmitBaseDotParentExpression());
	}

	protected override bool ShouldEmitLocalCall(BoundCall call)
	{
		if (base.ShouldEmitLocalCall(call))
		{
			return Method.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>() == call.Method.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		}
		return false;
	}
}
