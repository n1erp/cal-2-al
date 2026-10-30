using System.Collections.Immutable;
using System.Linq;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DynamicAnalysisInjector : CompoundInstrumenter
{
	private readonly MethodSymbol _method;

	private readonly BoundStatement _methodBody;

	private readonly MethodSymbol _createPayloadForMethodsSpanningSingleFile;

	private readonly MethodSymbol _createPayloadForMethodsSpanningMultipleFiles;

	private readonly ArrayBuilder<SourceSpan> _spansBuilder;

	private ImmutableArray<SourceSpan> _dynamicAnalysisSpans = ImmutableArray<SourceSpan>.Empty;

	private readonly BoundStatement _methodEntryInstrumentation;

	private readonly ArrayTypeSymbol _payloadType;

	private readonly LocalSymbol _methodPayload;

	private readonly DiagnosticBag _diagnostics;

	private readonly DebugDocumentProvider _debugDocumentProvider;

	private readonly SyntheticBoundNodeFactory _methodBodyFactory;

	public ImmutableArray<SourceSpan> DynamicAnalysisSpans => _dynamicAnalysisSpans;

	public static DynamicAnalysisInjector TryCreate(MethodSymbol method, BoundStatement methodBody, SyntheticBoundNodeFactory methodBodyFactory, DiagnosticBag diagnostics, DebugDocumentProvider debugDocumentProvider, Instrumenter previous)
	{
		if (method.IsImplicitlyDeclared && !method.IsImplicitConstructor)
		{
			return null;
		}
		if (IsExcludedFromCodeCoverage(method))
		{
			return null;
		}
		MethodSymbol createPayloadOverload = GetCreatePayloadOverload(methodBodyFactory.Compilation, WellKnownMember.Microsoft_CodeAnalysis_Runtime_Instrumentation__CreatePayloadForMethodsSpanningSingleFile, methodBody.Syntax, diagnostics);
		MethodSymbol createPayloadOverload2 = GetCreatePayloadOverload(methodBodyFactory.Compilation, WellKnownMember.Microsoft_CodeAnalysis_Runtime_Instrumentation__CreatePayloadForMethodsSpanningMultipleFiles, methodBody.Syntax, diagnostics);
		if ((object)createPayloadOverload == null || (object)createPayloadOverload2 == null)
		{
			return null;
		}
		if (method.Equals(createPayloadOverload) || method.Equals(createPayloadOverload2))
		{
			return null;
		}
		return new DynamicAnalysisInjector(method, methodBody, methodBodyFactory, createPayloadOverload, createPayloadOverload2, diagnostics, debugDocumentProvider, previous);
	}

	private DynamicAnalysisInjector(MethodSymbol method, BoundStatement methodBody, SyntheticBoundNodeFactory methodBodyFactory, MethodSymbol createPayloadForMethodsSpanningSingleFile, MethodSymbol createPayloadForMethodsSpanningMultipleFiles, DiagnosticBag diagnostics, DebugDocumentProvider debugDocumentProvider, Instrumenter previous)
		: base(previous)
	{
		_createPayloadForMethodsSpanningSingleFile = createPayloadForMethodsSpanningSingleFile;
		_createPayloadForMethodsSpanningMultipleFiles = createPayloadForMethodsSpanningMultipleFiles;
		_method = method;
		_methodBody = methodBody;
		_spansBuilder = ArrayBuilder<SourceSpan>.GetInstance();
		TypeSymbol elementType = methodBodyFactory.SpecialType(SpecialType.System_Boolean);
		_payloadType = ArrayTypeSymbol.CreateCSharpArray(methodBodyFactory.Compilation.Assembly, elementType);
		_diagnostics = diagnostics;
		_debugDocumentProvider = debugDocumentProvider;
		_methodBodyFactory = methodBodyFactory;
		MethodSymbol currentFunction = methodBodyFactory.CurrentFunction;
		methodBodyFactory.CurrentFunction = method;
		_methodPayload = methodBodyFactory.SynthesizedLocal(_payloadType, methodBody.Syntax, isPinned: false, RefKind.None, SynthesizedLocalKind.InstrumentationPayload);
		SyntaxNode syntaxNode = MethodDeclarationIfAvailable(methodBody.Syntax);
		if (!method.IsImplicitlyDeclared)
		{
			_methodEntryInstrumentation = AddAnalysisPoint(syntaxNode, SkipAttributes(syntaxNode), methodBodyFactory);
		}
		methodBodyFactory.CurrentFunction = currentFunction;
	}

	private static bool IsExcludedFromCodeCoverage(MethodSymbol method)
	{
		NamedTypeSymbol containingType = method.ContainingType;
		while ((object)containingType != null)
		{
			if (containingType.IsDirectlyExcludedFromCodeCoverage)
			{
				return true;
			}
			containingType = containingType.ContainingType;
		}
		Symbol symbol = method.ContainingNonLambdaMember();
		if ((object)symbol != null && symbol.Kind == SymbolKind.Method)
		{
			method = (MethodSymbol)symbol;
			if (method.IsDirectlyExcludedFromCodeCoverage)
			{
				return true;
			}
			Symbol associatedSymbol = method.AssociatedSymbol;
			switch (associatedSymbol?.Kind)
			{
			case SymbolKind.Property:
				if (((PropertySymbol)associatedSymbol).IsDirectlyExcludedFromCodeCoverage)
				{
					return true;
				}
				break;
			case SymbolKind.Event:
				if (((EventSymbol)associatedSymbol).IsDirectlyExcludedFromCodeCoverage)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	private static BoundExpressionStatement GetCreatePayloadStatement(ImmutableArray<SourceSpan> dynamicAnalysisSpans, SyntaxNode methodBodySyntax, LocalSymbol methodPayload, MethodSymbol createPayloadForMethodsSpanningSingleFile, MethodSymbol createPayloadForMethodsSpanningMultipleFiles, BoundExpression mvid, BoundExpression methodToken, BoundExpression payloadSlot, SyntheticBoundNodeFactory methodBodyFactory, DebugDocumentProvider debugDocumentProvider)
	{
		MethodSymbol method;
		BoundExpression boundExpression;
		if (dynamicAnalysisSpans.IsEmpty)
		{
			method = createPayloadForMethodsSpanningSingleFile;
			DebugSourceDocument sourceDocument = GetSourceDocument(debugDocumentProvider, methodBodySyntax);
			boundExpression = methodBodyFactory.SourceDocumentIndex(sourceDocument);
		}
		else
		{
			PooledHashSet<DebugSourceDocument> instance = PooledHashSet<DebugSourceDocument>.GetInstance();
			ArrayBuilder<BoundExpression> instance2 = ArrayBuilder<BoundExpression>.GetInstance();
			foreach (SourceSpan item in dynamicAnalysisSpans)
			{
				DebugSourceDocument document = item.Document;
				if (instance.Add(document))
				{
					instance2.Add(methodBodyFactory.SourceDocumentIndex(document));
				}
			}
			instance.Free();
			if (instance2.Count == 1)
			{
				method = createPayloadForMethodsSpanningSingleFile;
				boundExpression = instance2.Single();
			}
			else
			{
				method = createPayloadForMethodsSpanningMultipleFiles;
				boundExpression = methodBodyFactory.Array(methodBodyFactory.SpecialType(SpecialType.System_Int32), instance2.ToImmutable());
			}
			instance2.Free();
		}
		return methodBodyFactory.Assignment(methodBodyFactory.Local(methodPayload), methodBodyFactory.Call(null, method, mvid, methodToken, boundExpression, payloadSlot, methodBodyFactory.Literal(dynamicAnalysisSpans.Length)));
	}

	public override BoundStatement CreateBlockPrologue(BoundBlock original, out LocalSymbol synthesizedLocal)
	{
		BoundStatement boundStatement = base.CreateBlockPrologue(original, out synthesizedLocal);
		if (_methodBody == original)
		{
			_dynamicAnalysisSpans = _spansBuilder.ToImmutableAndFree();
			ArrayTypeSymbol payloadType = ArrayTypeSymbol.CreateCSharpArray(_methodBodyFactory.Compilation.Assembly, _payloadType);
			BoundStatement item = _methodBodyFactory.Assignment(_methodBodyFactory.Local(_methodPayload), _methodBodyFactory.ArrayAccess(_methodBodyFactory.InstrumentationPayloadRoot(0, payloadType), ImmutableArray.Create(_methodBodyFactory.MethodDefIndex(_method))));
			BoundExpression mvid = _methodBodyFactory.ModuleVersionId();
			BoundExpression methodToken = _methodBodyFactory.MethodDefIndex(_method);
			BoundExpression payloadSlot = _methodBodyFactory.ArrayAccess(_methodBodyFactory.InstrumentationPayloadRoot(0, payloadType), ImmutableArray.Create(_methodBodyFactory.MethodDefIndex(_method)));
			BoundStatement createPayloadStatement = GetCreatePayloadStatement(_dynamicAnalysisSpans, _methodBody.Syntax, _methodPayload, _createPayloadForMethodsSpanningSingleFile, _createPayloadForMethodsSpanningMultipleFiles, mvid, methodToken, payloadSlot, _methodBodyFactory, _debugDocumentProvider);
			BoundExpression condition = _methodBodyFactory.Binary(BinaryOperatorKind.ObjectEqual, _methodBodyFactory.SpecialType(SpecialType.System_Boolean), _methodBodyFactory.Local(_methodPayload), _methodBodyFactory.Null(_payloadType));
			BoundStatement item2 = _methodBodyFactory.If(condition, createPayloadStatement);
			synthesizedLocal = _methodPayload;
			ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance((boundStatement == null) ? 3 : 4);
			instance.Add(item);
			instance.Add(item2);
			if (_methodEntryInstrumentation != null)
			{
				instance.Add(_methodEntryInstrumentation);
			}
			if (boundStatement != null)
			{
				instance.Add(boundStatement);
			}
			return _methodBodyFactory.StatementList(instance.ToImmutableAndFree());
		}
		return boundStatement;
	}

	public override BoundStatement InstrumentNoOpStatement(BoundNoOpStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentNoOpStatement(original, rewritten));
	}

	public override BoundStatement InstrumentBreakStatement(BoundBreakStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentBreakStatement(original, rewritten));
	}

	public override BoundStatement InstrumentContinueStatement(BoundContinueStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentContinueStatement(original, rewritten));
	}

	public override BoundStatement InstrumentExpressionStatement(BoundExpressionStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentExpressionStatement(original, rewritten));
	}

	public override BoundStatement InstrumentFieldOrPropertyInitializer(BoundStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentFieldOrPropertyInitializer(original, rewritten));
	}

	public override BoundStatement InstrumentGotoStatement(BoundGotoStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentGotoStatement(original, rewritten));
	}

	public override BoundStatement InstrumentThrowStatement(BoundThrowStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentThrowStatement(original, rewritten));
	}

	public override BoundStatement InstrumentYieldBreakStatement(BoundYieldBreakStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentYieldBreakStatement(original, rewritten));
	}

	public override BoundStatement InstrumentYieldReturnStatement(BoundYieldReturnStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentYieldReturnStatement(original, rewritten));
	}

	public override BoundStatement InstrumentForEachStatementIterationVarDeclaration(BoundForEachStatement original, BoundStatement iterationVarDecl)
	{
		return AddDynamicAnalysis(original, base.InstrumentForEachStatementIterationVarDeclaration(original, iterationVarDecl));
	}

	public override BoundStatement InstrumentForEachStatementDeconstructionVariablesDeclaration(BoundForEachStatement original, BoundStatement iterationVarDecl)
	{
		return AddDynamicAnalysis(original, base.InstrumentForEachStatementDeconstructionVariablesDeclaration(original, iterationVarDecl));
	}

	public override BoundStatement InstrumentIfStatement(BoundIfStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentIfStatement(original, rewritten));
	}

	public override BoundStatement InstrumentWhileStatementConditionalGotoStartOrBreak(BoundWhileStatement original, BoundStatement ifConditionGotoStart)
	{
		return AddDynamicAnalysis(original, base.InstrumentWhileStatementConditionalGotoStartOrBreak(original, ifConditionGotoStart));
	}

	public override BoundStatement InstrumentLocalInitialization(BoundLocalDeclaration original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentLocalInitialization(original, rewritten));
	}

	public override BoundStatement InstrumentLockTargetCapture(BoundLockStatement original, BoundStatement lockTargetCapture)
	{
		return AddDynamicAnalysis(original, base.InstrumentLockTargetCapture(original, lockTargetCapture));
	}

	public override BoundStatement InstrumentReturnStatement(BoundReturnStatement original, BoundStatement rewritten)
	{
		rewritten = base.InstrumentReturnStatement(original, rewritten);
		if (ReturnsValueWithinExpressionBodiedConstruct(original))
		{
			return CollectDynamicAnalysis(original, rewritten);
		}
		return AddDynamicAnalysis(original, rewritten);
	}

	private static bool ReturnsValueWithinExpressionBodiedConstruct(BoundReturnStatement returnStatement)
	{
		if (returnStatement.WasCompilerGenerated && returnStatement.ExpressionOpt != null && returnStatement.ExpressionOpt.Syntax != null)
		{
			SyntaxKind syntaxKind = returnStatement.ExpressionOpt.Syntax.Parent.Kind();
			if (syntaxKind - 8642 <= SyntaxKind.List || syntaxKind == SyntaxKind.ArrowExpressionClause)
			{
				return true;
			}
		}
		return false;
	}

	public override BoundStatement InstrumentSwitchStatement(BoundSwitchStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentSwitchStatement(original, rewritten));
	}

	public override BoundStatement InstrumentPatternSwitchStatement(BoundPatternSwitchStatement original, BoundStatement rewritten)
	{
		return AddDynamicAnalysis(original, base.InstrumentPatternSwitchStatement(original, rewritten));
	}

	public override BoundStatement InstrumentPatternSwitchWhenClauseConditionalGotoBody(BoundExpression original, BoundStatement ifConditionGotoBody)
	{
		ifConditionGotoBody = base.InstrumentPatternSwitchWhenClauseConditionalGotoBody(original, ifConditionGotoBody);
		WhenClauseSyntax whenClauseSyntax = original.Syntax.FirstAncestorOrSelf<WhenClauseSyntax>();
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(_method, whenClauseSyntax, _methodBodyFactory.CompilationState, _diagnostics);
		return syntheticBoundNodeFactory.StatementList(AddAnalysisPoint(whenClauseSyntax, syntheticBoundNodeFactory), ifConditionGotoBody);
	}

	public override BoundStatement InstrumentUsingTargetCapture(BoundUsingStatement original, BoundStatement usingTargetCapture)
	{
		return AddDynamicAnalysis(original, base.InstrumentUsingTargetCapture(original, usingTargetCapture));
	}

	private BoundStatement AddDynamicAnalysis(BoundStatement original, BoundStatement rewritten)
	{
		if (!original.WasCompilerGenerated && (!original.IsConstructorInitializer() || original.Syntax.Kind() != SyntaxKind.ConstructorDeclaration))
		{
			return CollectDynamicAnalysis(original, rewritten);
		}
		return rewritten;
	}

	private BoundStatement CollectDynamicAnalysis(BoundStatement original, BoundStatement rewritten)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(_method, original.Syntax, _methodBodyFactory.CompilationState, _diagnostics);
		return syntheticBoundNodeFactory.StatementList(AddAnalysisPoint(SyntaxForSpan(original), syntheticBoundNodeFactory), rewritten);
	}

	private static DebugSourceDocument GetSourceDocument(DebugDocumentProvider debugDocumentProvider, SyntaxNode syntax)
	{
		return GetSourceDocument(debugDocumentProvider, syntax, syntax.GetLocation().GetMappedLineSpan());
	}

	private static DebugSourceDocument GetSourceDocument(DebugDocumentProvider debugDocumentProvider, SyntaxNode syntax, FileLinePositionSpan span)
	{
		string text = span.Path;
		if (text.Length == 0)
		{
			text = syntax.SyntaxTree.FilePath;
		}
		return debugDocumentProvider(text, "");
	}

	private BoundStatement AddAnalysisPoint(SyntaxNode syntaxForSpan, TextSpan alternateSpan, SyntheticBoundNodeFactory statementFactory)
	{
		return AddAnalysisPoint(syntaxForSpan, syntaxForSpan.SyntaxTree.GetMappedLineSpan(alternateSpan), statementFactory);
	}

	private BoundStatement AddAnalysisPoint(SyntaxNode syntaxForSpan, SyntheticBoundNodeFactory statementFactory)
	{
		return AddAnalysisPoint(syntaxForSpan, syntaxForSpan.GetLocation().GetMappedLineSpan(), statementFactory);
	}

	private BoundStatement AddAnalysisPoint(SyntaxNode syntaxForSpan, FileLinePositionSpan span, SyntheticBoundNodeFactory statementFactory)
	{
		int count = _spansBuilder.Count;
		_spansBuilder.Add(new SourceSpan(GetSourceDocument(_debugDocumentProvider, syntaxForSpan, span), span.StartLinePosition.Line, span.StartLinePosition.Character, span.EndLinePosition.Line, span.EndLinePosition.Character));
		BoundArrayAccess left = statementFactory.ArrayAccess(statementFactory.Local(_methodPayload), statementFactory.Literal(count));
		return statementFactory.Assignment(left, statementFactory.Literal(value: true));
	}

	private static SyntaxNode SyntaxForSpan(BoundStatement statement)
	{
		switch (statement.Kind)
		{
		case BoundKind.IfStatement:
			return ((BoundIfStatement)statement).Condition.Syntax;
		case BoundKind.WhileStatement:
			return ((BoundWhileStatement)statement).Condition.Syntax;
		case BoundKind.ForEachStatement:
			return ((BoundForEachStatement)statement).Expression.Syntax;
		case BoundKind.DoStatement:
			return ((BoundDoStatement)statement).Condition.Syntax;
		case BoundKind.UsingStatement:
		{
			BoundUsingStatement boundUsingStatement = (BoundUsingStatement)statement;
			return ((BoundNode)(((object)boundUsingStatement.ExpressionOpt) ?? ((object)boundUsingStatement.DeclarationsOpt))).Syntax;
		}
		case BoundKind.FixedStatement:
			return ((BoundFixedStatement)statement).Declarations.Syntax;
		case BoundKind.LockStatement:
			return ((BoundLockStatement)statement).Argument.Syntax;
		case BoundKind.SwitchStatement:
			return ((BoundSwitchStatement)statement).Expression.Syntax;
		case BoundKind.PatternSwitchStatement:
			return ((BoundPatternSwitchStatement)statement).Expression.Syntax;
		default:
			return statement.Syntax;
		}
	}

	private static MethodSymbol GetCreatePayloadOverload(CSharpCompilation compilation, WellKnownMember overload, SyntaxNode syntax, DiagnosticBag diagnostics)
	{
		return (MethodSymbol)Binder.GetWellKnownTypeMember(compilation, overload, diagnostics, null, syntax);
	}

	private static SyntaxNode MethodDeclarationIfAvailable(SyntaxNode body)
	{
		SyntaxNode parent = body.Parent;
		if (parent != null)
		{
			switch (parent.Kind())
			{
			case SyntaxKind.MethodDeclaration:
			case SyntaxKind.OperatorDeclaration:
			case SyntaxKind.ConstructorDeclaration:
			case SyntaxKind.PropertyDeclaration:
			case SyntaxKind.GetAccessorDeclaration:
			case SyntaxKind.SetAccessorDeclaration:
				return parent;
			}
		}
		return body;
	}

	private static TextSpan SkipAttributes(SyntaxNode syntax)
	{
		switch (syntax.Kind())
		{
		case SyntaxKind.MethodDeclaration:
		{
			MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)syntax;
			return SkipAttributes(syntax, methodDeclarationSyntax.AttributeLists, methodDeclarationSyntax.Modifiers, default, methodDeclarationSyntax.ReturnType);
		}
		case SyntaxKind.PropertyDeclaration:
		{
			PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)syntax;
			return SkipAttributes(syntax, propertyDeclarationSyntax.AttributeLists, propertyDeclarationSyntax.Modifiers, default, propertyDeclarationSyntax.Type);
		}
		case SyntaxKind.GetAccessorDeclaration:
		case SyntaxKind.SetAccessorDeclaration:
		{
			AccessorDeclarationSyntax accessorDeclarationSyntax = (AccessorDeclarationSyntax)syntax;
			return SkipAttributes(syntax, accessorDeclarationSyntax.AttributeLists, accessorDeclarationSyntax.Modifiers, accessorDeclarationSyntax.Keyword, null);
		}
		case SyntaxKind.ConstructorDeclaration:
		{
			ConstructorDeclarationSyntax constructorDeclarationSyntax = (ConstructorDeclarationSyntax)syntax;
			return SkipAttributes(syntax, constructorDeclarationSyntax.AttributeLists, constructorDeclarationSyntax.Modifiers, constructorDeclarationSyntax.Identifier, null);
		}
		case SyntaxKind.OperatorDeclaration:
		{
			OperatorDeclarationSyntax operatorDeclarationSyntax = (OperatorDeclarationSyntax)syntax;
			return SkipAttributes(syntax, operatorDeclarationSyntax.AttributeLists, operatorDeclarationSyntax.Modifiers, operatorDeclarationSyntax.OperatorKeyword, null);
		}
		default:
			return syntax.Span;
		}
	}

	private static TextSpan SkipAttributes(SyntaxNode syntax, SyntaxList<AttributeListSyntax> attributes, SyntaxTokenList modifiers, SyntaxToken keyword, TypeSyntax type)
	{
		TextSpan span = syntax.Span;
		if (attributes.Count > 0)
		{
			TextSpan textSpan;
			if (modifiers.Node != null)
			{
				textSpan = modifiers.Span;
			}
			else
			{
				textSpan = ((keyword.Node != null) ? keyword.Span : type.Span);
			}
			return new TextSpan(textSpan.Start, span.Length - (textSpan.Start - span.Start));
		}
		return span;
	}
}
