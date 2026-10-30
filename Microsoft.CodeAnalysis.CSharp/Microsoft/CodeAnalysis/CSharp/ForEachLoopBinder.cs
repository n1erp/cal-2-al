using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ForEachLoopBinder : LoopBinder
{
	private const string GetEnumeratorMethodName = "GetEnumerator";

	private const string CurrentPropertyName = "Current";

	private const string MoveNextMethodName = "MoveNext";

	private readonly CommonForEachStatementSyntax _syntax;

	private SourceLocalSymbol IterationVariable
	{
		get
		{
			if (_syntax.Kind() != SyntaxKind.ForEachStatement)
			{
				return null;
			}
			return (SourceLocalSymbol)Locals[0];
		}
	}

	internal override SyntaxNode ScopeDesignator => _syntax;

	public ForEachLoopBinder(Binder enclosing, CommonForEachStatementSyntax syntax)
		: base(enclosing)
	{
		_syntax = syntax;
	}

	protected override ImmutableArray<LocalSymbol> BuildLocals()
	{
		switch (_syntax.Kind())
		{
		case SyntaxKind.ForEachVariableStatement:
		{
			ForEachVariableStatementSyntax forEachVariableStatementSyntax = (ForEachVariableStatementSyntax)_syntax;
			ArrayBuilder<LocalSymbol> instance = ArrayBuilder<LocalSymbol>.GetInstance();
			CollectLocalsFromDeconstruction(forEachVariableStatementSyntax.Variable, LocalDeclarationKind.ForEachIterationVariable, instance, forEachVariableStatementSyntax);
			return instance.ToImmutableAndFree();
		}
		case SyntaxKind.ForEachStatement:
		{
			ForEachStatementSyntax forEachStatementSyntax = (ForEachStatementSyntax)_syntax;
			return ImmutableArray.Create((LocalSymbol)SourceLocalSymbol.MakeForeachLocal((MethodSymbol)ContainingMemberOrLambda, this, forEachStatementSyntax.Type, forEachStatementSyntax.Identifier, forEachStatementSyntax.Expression));
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(_syntax.Kind());
		}
	}

	internal void CollectLocalsFromDeconstruction(ExpressionSyntax declaration, LocalDeclarationKind kind, ArrayBuilder<LocalSymbol> locals, SyntaxNode deconstructionStatement, Binder enclosingBinderOpt = null)
	{
		switch (declaration.Kind())
		{
		case SyntaxKind.TupleExpression:
			foreach (ArgumentSyntax argument in ((TupleExpressionSyntax)declaration).Arguments)
			{
				CollectLocalsFromDeconstruction(argument.Expression, kind, locals, deconstructionStatement, enclosingBinderOpt);
			}
			break;
		case SyntaxKind.DeclarationExpression:
		{
			DeclarationExpressionSyntax declarationExpressionSyntax = (DeclarationExpressionSyntax)declaration;
			CollectLocalsFromDeconstruction(declarationExpressionSyntax.Designation, declarationExpressionSyntax.Type, kind, locals, deconstructionStatement, enclosingBinderOpt);
			break;
		}
		default:
			ExpressionVariableFinder.FindExpressionVariables(this, locals, declaration);
			break;
		case SyntaxKind.IdentifierName:
			break;
		}
	}

	internal void CollectLocalsFromDeconstruction(VariableDesignationSyntax designation, TypeSyntax closestTypeSyntax, LocalDeclarationKind kind, ArrayBuilder<LocalSymbol> locals, SyntaxNode deconstructionStatement, Binder enclosingBinderOpt)
	{
		switch (designation.Kind())
		{
		case SyntaxKind.SingleVariableDesignation:
		{
			SingleVariableDesignationSyntax singleVariableDesignationSyntax = (SingleVariableDesignationSyntax)designation;
			SourceLocalSymbol item = SourceLocalSymbol.MakeDeconstructionLocal(ContainingMemberOrLambda, this, enclosingBinderOpt ?? this, closestTypeSyntax, singleVariableDesignationSyntax.Identifier, kind, deconstructionStatement);
			locals.Add(item);
			break;
		}
		case SyntaxKind.ParenthesizedVariableDesignation:
			foreach (VariableDesignationSyntax variable in ((ParenthesizedVariableDesignationSyntax)designation).Variables)
			{
				CollectLocalsFromDeconstruction(variable, closestTypeSyntax, kind, locals, deconstructionStatement, enclosingBinderOpt);
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(designation.Kind());
		case SyntaxKind.DiscardDesignation:
			break;
		}
	}

	internal override BoundStatement BindForEachParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return BindForEachPartsWorker(diagnostics, originalBinder);
	}

	internal override BoundStatement BindForEachDeconstruction(DiagnosticBag diagnostics, Binder originalBinder)
	{
		BoundExpression collectionExpr = originalBinder.GetBinder(_syntax.Expression).BindValue(_syntax.Expression, diagnostics, BindValueKind.RValue);
		ForEachEnumeratorInfo.Builder builder = default;
		GetEnumeratorInfoAndInferCollectionElementType(ref builder, ref collectionExpr, diagnostics, out var inferredType);
		ExpressionSyntax variable = ((ForEachVariableStatementSyntax)_syntax).Variable;
		BoundDeconstructValuePlaceholder rightPlaceholder = new BoundDeconstructValuePlaceholder(_syntax.Expression, LocalScopeDepth, inferredType ?? CreateErrorType("var"));
		DeclarationExpressionSyntax declaration = null;
		ExpressionSyntax expression = null;
		BoundDeconstructionAssignmentOperator expression2 = BindDeconstruction(variable, variable, _syntax.Expression, diagnostics, ref declaration, ref expression, resultIsUsedOverride: false, rightPlaceholder);
		return new BoundExpressionStatement(_syntax, expression2);
	}

	private BoundForEachStatement BindForEachPartsWorker(DiagnosticBag diagnostics, Binder originalBinder)
	{
		BoundExpression collectionExpr = originalBinder.GetBinder(_syntax.Expression).BindValue(_syntax.Expression, diagnostics, BindValueKind.RValue);
		ForEachEnumeratorInfo.Builder builder = default;
		bool flag = !GetEnumeratorInfoAndInferCollectionElementType(ref builder, ref collectionExpr, diagnostics, out var inferredType) || (object)builder.GetEnumeratorMethod == null || (object)builder.MoveNextMethod == null || (object)builder.CurrentPropertyGetter == null;
		bool flag2 = false;
		BoundForEachDeconstructStep deconstructionOpt = null;
		BoundExpression boundExpression = null;
		uint valEscape = Binder.GetValEscape(collectionExpr, LocalScopeDepth);
		TypeSymbol typeSymbol;
		BoundTypeExpression boundTypeExpression;
		switch (_syntax.Kind())
		{
		case SyntaxKind.ForEachStatement:
		{
			ForEachStatementSyntax forEachStatementSyntax = (ForEachStatementSyntax)_syntax;
			flag2 = originalBinder.ValidateDeclarationNameConflictsInScope(IterationVariable, diagnostics);
			TypeSyntax syntax = forEachStatementSyntax.Type.SkipRef(out var _);
			TypeSymbol typeSymbol2 = BindTypeOrVarKeyword(syntax, diagnostics, out var isVar, out var alias);
			typeSymbol = ((!isVar) ? typeSymbol2 : (inferredType ?? CreateErrorType("var")));
			boundTypeExpression = new BoundTypeExpression(syntax, alias, typeSymbol);
			SourceLocalSymbol iterationVariable = IterationVariable;
			iterationVariable.SetType(typeSymbol);
			iterationVariable.SetValEscape(valEscape);
			if (iterationVariable.RefKind != RefKind.None)
			{
				iterationVariable.SetRefEscape(valEscape);
				if (IsDirectlyInIterator)
				{
					diagnostics.Add(ErrorCode.ERR_BadIteratorLocalType, iterationVariable.IdentifierToken.GetLocation());
				}
				else if (IsInAsyncMethod())
				{
					diagnostics.Add(ErrorCode.ERR_BadAsyncLocalType, iterationVariable.IdentifierToken.GetLocation());
				}
			}
			if (!flag)
			{
				flag |= !CheckMethodReturnValueKind(valueKind: iterationVariable.RefKind switch
				{
					RefKind.None => BindValueKind.RValue, 
					RefKind.Ref => BindValueKind.Assignable | BindValueKind.RefersToLocation, 
					RefKind.In => BindValueKind.RefersToLocation, 
					_ => throw ExceptionUtilities.UnexpectedValue(iterationVariable.RefKind), 
				}, methodSymbol: builder.CurrentPropertyGetter, callSyntaxOpt: null, node: collectionExpr.Syntax, checkingReceiver: false, diagnostics: diagnostics);
			}
			break;
		}
		case SyntaxKind.ForEachVariableStatement:
		{
			ForEachVariableStatementSyntax forEachVariableStatementSyntax = (ForEachVariableStatementSyntax)_syntax;
			typeSymbol = inferredType ?? CreateErrorType("var");
			ExpressionSyntax variable = forEachVariableStatementSyntax.Variable;
			if (variable.IsDeconstructionLeft())
			{
				BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder = new BoundDeconstructValuePlaceholder(_syntax.Expression, valEscape, typeSymbol).MakeCompilerGenerated();
				DeclarationExpressionSyntax declaration = null;
				ExpressionSyntax expression = null;
				BoundDeconstructionAssignmentOperator deconstructionAssignment = BindDeconstruction(variable, variable, _syntax.Expression, diagnostics, ref declaration, ref expression, resultIsUsedOverride: false, boundDeconstructValuePlaceholder);
				if (expression != null)
				{
					Binder.Error(diagnostics, ErrorCode.ERR_MustDeclareForeachIteration, variable);
					flag = true;
				}
				deconstructionOpt = new BoundForEachDeconstructStep(variable, deconstructionAssignment, boundDeconstructValuePlaceholder).MakeCompilerGenerated();
			}
			else
			{
				boundExpression = BindExpression(forEachVariableStatementSyntax.Variable, new DiagnosticBag());
				if (boundExpression.Kind == BoundKind.DiscardExpression)
				{
					boundExpression = ((BoundDiscardExpression)boundExpression).FailInference(this, null);
				}
				flag = true;
				if (!forEachVariableStatementSyntax.HasErrors)
				{
					Binder.Error(diagnostics, ErrorCode.ERR_MustDeclareForeachIteration, variable);
				}
			}
			boundTypeExpression = new BoundTypeExpression(variable, null, typeSymbol).MakeCompilerGenerated();
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(_syntax.Kind());
		}
		BoundStatement body = originalBinder.BindPossibleEmbeddedStatement(_syntax.Statement, diagnostics);
		ImmutableArray<LocalSymbol> locals = Locals;
		flag = flag || boundTypeExpression.HasErrors || typeSymbol.IsErrorType();
		if (flag)
		{
			return new BoundForEachStatement(_syntax, null, default, boundTypeExpression, locals, boundExpression, collectionExpr, deconstructionOpt, body, CheckOverflowAtRuntime, BreakLabel, ContinueLabel, flag);
		}
		flag |= flag2;
		SyntaxToken forEachKeyword = _syntax.ForEachKeyword;
		ReportDiagnosticsIfObsolete(diagnostics, builder.GetEnumeratorMethod, forEachKeyword, hasBaseReceiver: false);
		ReportDiagnosticsIfObsolete(diagnostics, builder.MoveNextMethod, forEachKeyword, hasBaseReceiver: false);
		ReportDiagnosticsIfObsolete(diagnostics, builder.CurrentPropertyGetter, forEachKeyword, hasBaseReceiver: false);
		ReportDiagnosticsIfObsolete(diagnostics, builder.CurrentPropertyGetter.AssociatedSymbol, forEachKeyword, hasBaseReceiver: false);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromType(inferredType, typeSymbol, ref useSiteDiagnostics, forCast: true);
		if (!conversion.IsValid)
		{
			ImmutableArray<MethodSymbol> originalUserDefinedConversions = conversion.OriginalUserDefinedConversions;
			if (originalUserDefinedConversions.Length > 1)
			{
				diagnostics.Add(ErrorCode.ERR_AmbigUDConv, forEachKeyword.GetLocation(), originalUserDefinedConversions[0], originalUserDefinedConversions[1], inferredType, typeSymbol);
			}
			else
			{
				SymbolDistinguisher symbolDistinguisher = new SymbolDistinguisher(Compilation, inferredType, typeSymbol);
				diagnostics.Add(ErrorCode.ERR_NoExplicitConv, forEachKeyword.GetLocation(), symbolDistinguisher.First, symbolDistinguisher.Second);
			}
			flag = true;
		}
		else
		{
			ReportDiagnosticsIfObsolete(diagnostics, conversion, _syntax.ForEachKeyword, hasBaseReceiver: false);
		}
		builder.CollectionConversion = Conversions.ClassifyConversionFromExpression(collectionExpr, builder.CollectionType, ref useSiteDiagnostics);
		builder.CurrentConversion = Conversions.ClassifyConversionFromType(builder.CurrentPropertyGetter.ReturnType, builder.ElementType, ref useSiteDiagnostics);
		TypeSymbol returnType = builder.GetEnumeratorMethod.ReturnType;
		builder.EnumeratorConversion = (returnType.IsValueType ? Conversion.Identity : Conversions.ClassifyConversionFromType(returnType, GetSpecialType(SpecialType.System_Object, diagnostics, _syntax), ref useSiteDiagnostics));
		if (returnType.IsRestrictedType() && (IsDirectlyInIterator || IsInAsyncMethod()))
		{
			diagnostics.Add(ErrorCode.ERR_BadSpecialByRefIterator, forEachKeyword.GetLocation(), returnType);
		}
		diagnostics.Add(_syntax.ForEachKeyword.GetLocation(), useSiteDiagnostics);
		BoundConversion expression2 = new BoundConversion(collectionExpr.Syntax, collectionExpr, builder.CollectionConversion, CheckOverflowAtRuntime, explicitCastInCode: false, null, builder.CollectionType);
		return new BoundForEachStatement(_syntax, builder.Build(Flags), conversion, boundTypeExpression, locals, expression2, deconstructionOpt, body, CheckOverflowAtRuntime, BreakLabel, ContinueLabel, flag);
	}

	internal TypeSymbol InferCollectionElementType(DiagnosticBag diagnostics, ExpressionSyntax collectionSyntax)
	{
		BoundExpression collectionExpr = GetBinder(collectionSyntax).BindValue(collectionSyntax, diagnostics, BindValueKind.RValue);
		ForEachEnumeratorInfo.Builder builder = default;
		GetEnumeratorInfoAndInferCollectionElementType(ref builder, ref collectionExpr, diagnostics, out var inferredType);
		return inferredType;
	}

	private bool GetEnumeratorInfoAndInferCollectionElementType(ref ForEachEnumeratorInfo.Builder builder, ref BoundExpression collectionExpr, DiagnosticBag diagnostics, out TypeSymbol inferredType)
	{
		UnwrapCollectionExpressionIfNullable(ref collectionExpr, diagnostics);
		bool enumeratorInfo = GetEnumeratorInfo(ref builder, collectionExpr, diagnostics);
		if (!enumeratorInfo)
		{
			inferredType = null;
			return enumeratorInfo;
		}
		if (collectionExpr.HasDynamicType())
		{
			inferredType = DynamicTypeSymbol.Instance;
			return enumeratorInfo;
		}
		if (collectionExpr.Type.SpecialType == SpecialType.System_String && builder.CollectionType.SpecialType == SpecialType.System_Collections_IEnumerable)
		{
			inferredType = GetSpecialType(SpecialType.System_Char, diagnostics, collectionExpr.Syntax);
			return enumeratorInfo;
		}
		inferredType = builder.ElementType;
		return enumeratorInfo;
	}

	private void UnwrapCollectionExpressionIfNullable(ref BoundExpression collectionExpr, DiagnosticBag diagnostics)
	{
		TypeSymbol type = collectionExpr.Type;
		if ((object)type != null && type.IsNullableType())
		{
			SyntaxNode syntax = collectionExpr.Syntax;
			MethodSymbol methodSymbol = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Nullable_T_get_Value, diagnostics, syntax);
			if ((object)methodSymbol != null)
			{
				methodSymbol = methodSymbol.AsMember((NamedTypeSymbol)type);
				collectionExpr = BoundCall.Synthesized(syntax, collectionExpr, methodSymbol);
			}
			else
			{
				collectionExpr = new BoundBadExpression(syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(collectionExpr), type.GetNullableUnderlyingType())
				{
					WasCompilerGenerated = true
				};
			}
		}
	}

	private bool GetEnumeratorInfo(ref ForEachEnumeratorInfo.Builder builder, BoundExpression collectionExpr, DiagnosticBag diagnostics)
	{
		TypeSymbol type = collectionExpr.Type;
		if (collectionExpr.ConstantValue != null && collectionExpr.ConstantValue.IsNull)
		{
			diagnostics.Add(ErrorCode.ERR_NullNotValid, _syntax.Expression.Location);
			return false;
		}
		if ((object)type == null)
		{
			if (collectionExpr.Kind == BoundKind.DefaultExpression)
			{
				diagnostics.Add(ErrorCode.ERR_DefaultLiteralNotValid, _syntax.Expression.Location);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_AnonMethGrpInForEach, _syntax.Expression.Location, collectionExpr.Display);
			}
			return false;
		}
		if (collectionExpr.ResultKind == LookupResultKind.NotAValue)
		{
			return false;
		}
		if (type.Kind == SymbolKind.ArrayType || type.Kind == SymbolKind.DynamicType)
		{
			builder = GetDefaultEnumeratorInfo(builder, diagnostics, type);
			return true;
		}
		if (SatisfiesGetEnumeratorPattern(ref builder, type, diagnostics))
		{
			builder.CollectionType = type;
			if (SatisfiesForEachPattern(ref builder, diagnostics))
			{
				builder.ElementType = ((PropertySymbol)builder.CurrentPropertyGetter.AssociatedSymbol).Type;
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				TypeSymbol returnType = builder.GetEnumeratorMethod.ReturnType;
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				if (!returnType.IsSealed || Conversions.ClassifyImplicitConversionFromType(returnType, Compilation.GetSpecialType(SpecialType.System_IDisposable), ref useSiteDiagnostics).IsImplicit)
				{
					builder.NeedsDisposeMethod = true;
					diagnostics.AddRange(instance);
				}
				instance.Free();
				diagnostics.Add(_syntax, useSiteDiagnostics);
				return true;
			}
			MethodSymbol getEnumeratorMethod = builder.GetEnumeratorMethod;
			diagnostics.Add(ErrorCode.ERR_BadGetEnumerator, _syntax.Expression.Location, getEnumeratorMethod.ReturnType, getEnumeratorMethod);
			return false;
		}
		if (IsIEnumerable(type))
		{
			diagnostics.Add(ErrorCode.ERR_ForEachMissingMember, _syntax.Expression.Location, type, "GetEnumerator");
			return false;
		}
		if (AllInterfacesContainsIEnumerable(ref builder, type, diagnostics, out var foundMultiple))
		{
			CSharpSyntaxNode expression = _syntax.Expression;
			if (foundMultiple)
			{
				diagnostics.Add(ErrorCode.ERR_MultipleIEnumOfT, expression.Location, type, Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T));
				return false;
			}
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)builder.CollectionType;
			if (namedTypeSymbol.IsGenericType)
			{
				builder.ElementType = namedTypeSymbol.TypeArgumentsNoUseSiteDiagnostics.Single();
				MethodSymbol methodSymbol = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IEnumerable_T__GetEnumerator, diagnostics, expression);
				if ((object)methodSymbol != null)
				{
					builder.GetEnumeratorMethod = methodSymbol.AsMember(namedTypeSymbol);
					TypeSymbol returnType2 = builder.GetEnumeratorMethod.ReturnType;
					MethodSymbol methodSymbol2 = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Collections_Generic_IEnumerator_T__get_Current, diagnostics, expression);
					if ((object)methodSymbol2 != null)
					{
						builder.CurrentPropertyGetter = methodSymbol2.AsMember((NamedTypeSymbol)returnType2);
					}
				}
				builder.MoveNextMethod = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__MoveNext, diagnostics, expression);
			}
			else
			{
				builder.ElementType = GetSpecialType(SpecialType.System_Object, diagnostics, expression);
				builder.GetEnumeratorMethod = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerable__GetEnumerator, diagnostics, expression);
				builder.CurrentPropertyGetter = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__get_Current, diagnostics, expression);
				builder.MoveNextMethod = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__MoveNext, diagnostics, expression);
			}
			builder.NeedsDisposeMethod = true;
			return true;
		}
		if (type.SpecialType == SpecialType.System_String)
		{
			builder = GetDefaultEnumeratorInfo(builder, diagnostics, type);
			return true;
		}
		if (!string.IsNullOrEmpty(type.Name) || !collectionExpr.HasErrors)
		{
			diagnostics.Add(ErrorCode.ERR_ForEachMissingMember, _syntax.Expression.Location, type, "GetEnumerator");
		}
		return false;
	}

	private ForEachEnumeratorInfo.Builder GetDefaultEnumeratorInfo(ForEachEnumeratorInfo.Builder builder, DiagnosticBag diagnostics, TypeSymbol collectionExprType)
	{
		builder.CollectionType = GetSpecialType(SpecialType.System_Collections_IEnumerable, diagnostics, _syntax);
		if (collectionExprType.IsDynamic())
		{
			ForEachStatementSyntax obj = _syntax as ForEachStatementSyntax;
			builder.ElementType = ((obj != null && obj.Type.IsVar) ? ((TypeSymbol)DynamicTypeSymbol.Instance) : ((TypeSymbol)GetSpecialType(SpecialType.System_Object, diagnostics, _syntax)));
		}
		else
		{
			builder.ElementType = ((collectionExprType.SpecialType == SpecialType.System_String) ? GetSpecialType(SpecialType.System_Char, diagnostics, _syntax) : ((ArrayTypeSymbol)collectionExprType).ElementType);
		}
		builder.GetEnumeratorMethod = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerable__GetEnumerator, diagnostics, _syntax);
		builder.CurrentPropertyGetter = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__get_Current, diagnostics, _syntax);
		builder.MoveNextMethod = (MethodSymbol)GetSpecialTypeMember(SpecialMember.System_Collections_IEnumerator__MoveNext, diagnostics, _syntax);
		builder.NeedsDisposeMethod = true;
		return builder;
	}

	private bool SatisfiesGetEnumeratorPattern(ref ForEachEnumeratorInfo.Builder builder, TypeSymbol collectionExprType, DiagnosticBag diagnostics)
	{
		LookupResult instance = LookupResult.GetInstance();
		MethodSymbol methodSymbol = FindForEachPatternMethod(collectionExprType, "GetEnumerator", instance, warningsOnly: true, diagnostics);
		instance.Free();
		builder.GetEnumeratorMethod = methodSymbol;
		return (object)methodSymbol != null;
	}

	private MethodSymbol FindForEachPatternMethod(TypeSymbol patternType, string methodName, LookupResult lookupResult, bool warningsOnly, DiagnosticBag diagnostics)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupMembersInType(lookupResult, patternType, methodName, 0, null, LookupOptions.Default, this, diagnose: false, ref useSiteDiagnostics);
		diagnostics.Add(_syntax.Expression, useSiteDiagnostics);
		if (!lookupResult.IsMultiViable)
		{
			ReportPatternMemberLookupDiagnostics(lookupResult, patternType, methodName, warningsOnly, diagnostics);
			return null;
		}
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		foreach (Symbol symbol in lookupResult.Symbols)
		{
			if (symbol.Kind != SymbolKind.Method)
			{
				instance.Free();
				if (warningsOnly)
				{
					ReportEnumerableWarning(diagnostics, patternType, symbol);
				}
				return null;
			}
			if (!((MethodSymbol)symbol).Parameters.Any())
			{
				instance.Add((MethodSymbol)symbol);
			}
		}
		MethodSymbol result = PerformForEachPatternOverloadResolution(patternType, instance, warningsOnly, diagnostics);
		instance.Free();
		return result;
	}

	private MethodSymbol PerformForEachPatternOverloadResolution(TypeSymbol patternType, ArrayBuilder<MethodSymbol> candidateMethods, bool warningsOnly, DiagnosticBag diagnostics)
	{
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		AnalyzedArguments instance2 = AnalyzedArguments.GetInstance();
		OverloadResolutionResult<MethodSymbol> instance3 = OverloadResolutionResult<MethodSymbol>.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		BoundImplicitReceiver receiver = new BoundImplicitReceiver(_syntax.Expression, patternType);
		OverloadResolution.MethodInvocationOverloadResolution(candidateMethods, instance, receiver, instance2, instance3, ref useSiteDiagnostics);
		diagnostics.Add(_syntax.Expression, useSiteDiagnostics);
		MethodSymbol methodSymbol = null;
		if (instance3.Succeeded)
		{
			methodSymbol = instance3.ValidResult.Member;
			if (methodSymbol.IsStatic || methodSymbol.DeclaredAccessibility != Accessibility.Public)
			{
				if (warningsOnly)
				{
					diagnostics.Add(ErrorCode.WRN_PatternStaticOrInaccessible, _syntax.Expression.Location, patternType, MessageID.IDS_Collection.Localize(), methodSymbol);
				}
				methodSymbol = null;
			}
			else if (methodSymbol.CallsAreOmitted(_syntax.SyntaxTree))
			{
				methodSymbol = null;
			}
		}
		else if (instance3.Results.Length > 1 && warningsOnly)
		{
			diagnostics.Add(ErrorCode.WRN_PatternIsAmbiguous, _syntax.Expression.Location, patternType, MessageID.IDS_Collection.Localize(), instance3.Results[0].Member, instance3.Results[1].Member);
		}
		instance3.Free();
		instance2.Free();
		instance.Free();
		return methodSymbol;
	}

	private bool SatisfiesForEachPattern(ref ForEachEnumeratorInfo.Builder builder, DiagnosticBag diagnostics)
	{
		TypeSymbol returnType = builder.GetEnumeratorMethod.ReturnType;
		switch (returnType.TypeKind)
		{
		case TypeKind.Submission:
			throw ExceptionUtilities.UnexpectedValue(returnType.TypeKind);
		default:
			return false;
		case TypeKind.Class:
		case TypeKind.Dynamic:
		case TypeKind.Interface:
		case TypeKind.Struct:
		case TypeKind.TypeParameter:
		{
			LookupResult instance = LookupResult.GetInstance();
			try
			{
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				LookupMembersInType(instance, returnType, "Current", 0, null, LookupOptions.Default, this, diagnose: false, ref useSiteDiagnostics);
				diagnostics.Add(_syntax.Expression, useSiteDiagnostics);
				useSiteDiagnostics = null;
				if (!instance.IsSingleViable)
				{
					ReportPatternMemberLookupDiagnostics(instance, returnType, "Current", warningsOnly: false, diagnostics);
					return false;
				}
				Symbol singleSymbolOrDefault = instance.SingleSymbolOrDefault;
				if (singleSymbolOrDefault.IsStatic || singleSymbolOrDefault.DeclaredAccessibility != Accessibility.Public || singleSymbolOrDefault.Kind != SymbolKind.Property)
				{
					return false;
				}
				MethodSymbol ownOrInheritedGetMethod = ((PropertySymbol)singleSymbolOrDefault).GetOwnOrInheritedGetMethod();
				if ((object)ownOrInheritedGetMethod == null)
				{
					return false;
				}
				bool flag = IsAccessible(ownOrInheritedGetMethod, ref useSiteDiagnostics);
				diagnostics.Add(_syntax.Expression, useSiteDiagnostics);
				if (!flag)
				{
					return false;
				}
				builder.CurrentPropertyGetter = ownOrInheritedGetMethod;
				instance.Clear();
				MethodSymbol methodSymbol = FindForEachPatternMethod(returnType, "MoveNext", instance, warningsOnly: false, diagnostics);
				if ((object)methodSymbol == null || methodSymbol.IsStatic || methodSymbol.DeclaredAccessibility != Accessibility.Public || methodSymbol.OriginalDefinition.ReturnType.SpecialType != SpecialType.System_Boolean)
				{
					return false;
				}
				builder.MoveNextMethod = methodSymbol;
				return true;
			}
			finally
			{
				instance.Free();
			}
		}
		}
	}

	private void ReportEnumerableWarning(DiagnosticBag diagnostics, TypeSymbol enumeratorType, Symbol patternMemberCandidate)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (IsAccessible(patternMemberCandidate, ref useSiteDiagnostics))
		{
			diagnostics.Add(ErrorCode.WRN_PatternBadSignature, _syntax.Expression.Location, enumeratorType, MessageID.IDS_Collection.Localize(), patternMemberCandidate);
		}
		diagnostics.Add(_syntax.Expression, useSiteDiagnostics);
	}

	private static bool IsIEnumerable(TypeSymbol type)
	{
		SpecialType specialType = type.OriginalDefinition.SpecialType;
		if ((uint)(specialType - 24) <= 1u)
		{
			return true;
		}
		return false;
	}

	private bool AllInterfacesContainsIEnumerable(ref ForEachEnumeratorInfo.Builder builder, TypeSymbol type, DiagnosticBag diagnostics, out bool foundMultiple)
	{
		NamedTypeSymbol result = null;
		foundMultiple = false;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if (type.TypeKind == TypeKind.TypeParameter)
		{
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)type;
			GetIEnumerableOfT(typeParameterSymbol.EffectiveBaseClass(ref useSiteDiagnostics).AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics), ref result, ref foundMultiple);
			GetIEnumerableOfT(typeParameterSymbol.AllEffectiveInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics), ref result, ref foundMultiple);
		}
		else
		{
			GetIEnumerableOfT(type.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics), ref result, ref foundMultiple);
		}
		if ((object)result == null || !IsAccessible(result, ref useSiteDiagnostics))
		{
			result = null;
			NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
			if ((object)specialType != null && Conversions.ClassifyImplicitConversionFromType(type, specialType, ref useSiteDiagnostics).IsImplicit)
			{
				result = specialType;
			}
		}
		diagnostics.Add(_syntax.Expression, useSiteDiagnostics);
		builder.CollectionType = result;
		return (object)result != null;
	}

	private static void GetIEnumerableOfT(ImmutableArray<NamedTypeSymbol> interfaces, ref NamedTypeSymbol result, ref bool foundMultiple)
	{
		if (foundMultiple)
		{
			return;
		}
		foreach (NamedTypeSymbol item in interfaces)
		{
			if (item.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
			{
				if ((object)result == null)
				{
					result = item;
				}
				else if (item != result)
				{
					foundMultiple = true;
					break;
				}
			}
		}
	}

	private void ReportPatternMemberLookupDiagnostics(LookupResult lookupResult, TypeSymbol patternType, string memberName, bool warningsOnly, DiagnosticBag diagnostics)
	{
		if (lookupResult.Symbols.Any())
		{
			if (warningsOnly)
			{
				ReportEnumerableWarning(diagnostics, patternType, lookupResult.Symbols.First());
				return;
			}
			lookupResult.Clear();
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			LookupMembersInType(lookupResult, patternType, memberName, 0, null, LookupOptions.Default, this, diagnose: true, ref useSiteDiagnostics);
			diagnostics.Add(_syntax.Expression, useSiteDiagnostics);
			if (lookupResult.Error != null)
			{
				diagnostics.Add(lookupResult.Error, _syntax.Expression.Location);
			}
		}
		else if (!warningsOnly)
		{
			diagnostics.Add(ErrorCode.ERR_NoSuchMember, _syntax.Expression.Location, patternType, memberName);
		}
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		if (_syntax == scopeDesignator)
		{
			return Locals;
		}
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
