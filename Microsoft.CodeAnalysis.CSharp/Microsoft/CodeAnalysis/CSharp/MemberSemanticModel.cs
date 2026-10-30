using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class MemberSemanticModel : CSharpSemanticModel
{
	internal class IncrementalBinder : Binder
	{
		private readonly MemberSemanticModel _semanticModel;

		internal IncrementalBinder(MemberSemanticModel semanticModel, Binder next)
			: base(next)
		{
			_semanticModel = semanticModel;
		}

		internal override Binder GetBinder(SyntaxNode node)
		{
			Binder binder = Next.GetBinder(node);
			if (binder != null)
			{
				return new IncrementalBinder(_semanticModel, binder.WithAdditionalFlags(BinderFlags.SemanticModel));
			}
			return null;
		}

		public override BoundStatement BindStatement(StatementSyntax node, DiagnosticBag diagnostics)
		{
			BoundStatement boundStatement = _semanticModel.GuardedGetSynthesizedStatementFromMap(node);
			if (boundStatement != null)
			{
				return boundStatement;
			}
			BoundNode boundNode = TryGetBoundNodeFromMap(node);
			if (boundNode == null)
			{
				BoundStatement boundStatement2 = base.BindStatement(node, diagnostics);
				if (boundStatement2.WasCompilerGenerated)
				{
					_semanticModel.GuardedAddSynthesizedStatementToMap(node, boundStatement2);
				}
				return boundStatement2;
			}
			return (BoundStatement)boundNode;
		}

		private BoundNode TryGetBoundNodeFromMap(CSharpSyntaxNode node)
		{
			ImmutableArray<BoundNode> immutableArray = _semanticModel.GuardedGetBoundNodesFromMap(node);
			if (!immutableArray.IsDefaultOrEmpty)
			{
				return immutableArray[0];
			}
			return null;
		}

		public override BoundNode BindMethodBody(CSharpSyntaxNode node, DiagnosticBag diagnostics)
		{
			return TryGetBoundNodeFromMap(node) ?? base.BindMethodBody(node, diagnostics);
		}

		internal override BoundExpressionStatement BindConstructorInitializer(ConstructorInitializerSyntax node, DiagnosticBag diagnostics)
		{
			return ((BoundExpressionStatement)TryGetBoundNodeFromMap(node)) ?? base.BindConstructorInitializer(node, diagnostics);
		}

		internal override BoundBlock BindExpressionBodyAsBlock(ArrowExpressionClauseSyntax node, DiagnosticBag diagnostics)
		{
			return ((BoundBlock)TryGetBoundNodeFromMap(node)) ?? base.BindExpressionBodyAsBlock(node, diagnostics);
		}
	}

	protected sealed class NodeMapBuilder : BoundTreeWalkerWithStackGuard
	{
		private readonly OrderPreservingMultiDictionary<SyntaxNode, BoundNode> _map;

		private readonly SyntaxNode _thisSyntaxNodeOnly;

		private NodeMapBuilder(OrderPreservingMultiDictionary<SyntaxNode, BoundNode> map, SyntaxNode thisSyntaxNodeOnly)
		{
			_map = map;
			_thisSyntaxNodeOnly = thisSyntaxNodeOnly;
		}

		public static void AddToMap(BoundNode root, Dictionary<SyntaxNode, ImmutableArray<BoundNode>> map, SyntaxNode node = null)
		{
			if (root == null || map.ContainsKey(root.Syntax))
			{
				return;
			}
			OrderPreservingMultiDictionary<SyntaxNode, BoundNode> instance = OrderPreservingMultiDictionary<SyntaxNode, BoundNode>.GetInstance();
			new NodeMapBuilder(instance, node).Visit(root);
			foreach (CSharpSyntaxNode key in instance.Keys)
			{
				if (!map.ContainsKey(key))
				{
					map[key] = instance[key];
				}
			}
			instance.Free();
		}

		public override BoundNode Visit(BoundNode node)
		{
			if (node == null)
			{
				return null;
			}
			BoundNode boundNode = node;
			if (node.Kind == BoundKind.UnboundLambda)
			{
				boundNode = ((UnboundLambda)node).BindForErrorRecovery();
			}
			if (ShouldAddNode(boundNode))
			{
				_map.Add(boundNode.Syntax, boundNode);
			}
			if (boundNode is BoundBinaryOperator boundBinaryOperator)
			{
				ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
				instance.Push(boundBinaryOperator.Right);
				boundNode = boundBinaryOperator.Left;
				for (BoundBinaryOperator boundBinaryOperator2 = boundNode as BoundBinaryOperator; boundBinaryOperator2 != null; boundBinaryOperator2 = boundNode as BoundBinaryOperator)
				{
					if (ShouldAddNode(boundBinaryOperator2))
					{
						_map.Add(boundBinaryOperator2.Syntax, boundBinaryOperator2);
					}
					instance.Push(boundBinaryOperator2.Right);
					boundNode = boundBinaryOperator2.Left;
				}
				Visit(boundNode);
				while (instance.Count > 0)
				{
					Visit(instance.Pop());
				}
				instance.Free();
			}
			else
			{
				base.Visit(boundNode);
			}
			return null;
		}

		private bool ShouldAddNode(BoundNode currentBoundNode)
		{
			if (currentBoundNode.WasCompilerGenerated)
			{
				return false;
			}
			if (_thisSyntaxNodeOnly != null && currentBoundNode.Syntax != _thisSyntaxNodeOnly)
			{
				return false;
			}
			return true;
		}

		public override BoundNode VisitQueryClause(BoundQueryClause node)
		{
			Visit(node.Value);
			VisitUnoptimizedForm(node);
			return null;
		}

		public override BoundNode VisitRangeVariable(BoundRangeVariable node)
		{
			return null;
		}

		public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public override BoundNode VisitTupleBinaryOperator(BoundTupleBinaryOperator node)
		{
			Visit(node.ConvertedLeft);
			Visit(node.ConvertedRight);
			return null;
		}

		protected override bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
		{
			return false;
		}
	}

	private sealed class SpeculativeMemberSemanticModel : MemberSemanticModel
	{
		public SpeculativeMemberSemanticModel(SyntaxTreeSemanticModel parentSemanticModel, Symbol owner, TypeSyntax root, Binder rootBinder, int position)
			: base(root, owner, rootBinder, null, parentSemanticModel, position)
		{
		}

		internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ConstructorInitializerSyntax constructorInitializer, out SemanticModel speculativeModel)
		{
			throw ExceptionUtilities.Unreachable;
		}

		internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, EqualsValueClauseSyntax initializer, out SemanticModel speculativeModel)
		{
			throw ExceptionUtilities.Unreachable;
		}

		internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, ArrowExpressionClauseSyntax expressionBody, out SemanticModel speculativeModel)
		{
			throw ExceptionUtilities.Unreachable;
		}

		internal override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, StatementSyntax statement, out SemanticModel speculativeModel)
		{
			throw ExceptionUtilities.Unreachable;
		}

		internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, BaseMethodDeclarationSyntax method, out SemanticModel speculativeModel)
		{
			throw ExceptionUtilities.Unreachable;
		}

		internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, AccessorDeclarationSyntax accessor, out SemanticModel speculativeModel)
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private readonly Symbol _memberSymbol;

	private readonly CSharpSyntaxNode _root;

	private readonly DiagnosticBag _ignoredDiagnostics = new DiagnosticBag();

	private readonly ReaderWriterLockSlim _nodeMapLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private readonly Dictionary<SyntaxNode, ImmutableArray<BoundNode>> _guardedNodeMap = new Dictionary<SyntaxNode, ImmutableArray<BoundNode>>();

	private Dictionary<SyntaxNode, BoundStatement> _lazyGuardedSynthesizedStatementsMap;

	internal readonly Binder RootBinder;

	private readonly SyntaxTreeSemanticModel _containingSemanticModelOpt;

	private readonly SyntaxTreeSemanticModel _parentSemanticModelOpt;

	private readonly int _speculatedPosition;

	private readonly Lazy<CSharpOperationFactory> _operationFactory;

	public override CSharpCompilation Compilation => (_containingSemanticModelOpt ?? _parentSemanticModelOpt).Compilation;

	internal override CSharpSyntaxNode Root => _root;

	internal Symbol MemberSymbol => _memberSymbol;

	public sealed override bool IsSpeculativeSemanticModel => _parentSemanticModelOpt != null;

	public sealed override int OriginalPositionForSpeculation => _speculatedPosition;

	public sealed override CSharpSemanticModel ParentModel => _parentSemanticModelOpt;

	internal sealed override SemanticModel ContainingModelOrSelf => (SemanticModel)(((object)_containingSemanticModelOpt) ?? ((object)this));

	public override SyntaxTree SyntaxTree => _root.SyntaxTree;

	protected MemberSemanticModel(CSharpSyntaxNode root, Symbol memberSymbol, Binder rootBinder, SyntaxTreeSemanticModel containingSemanticModelOpt, SyntaxTreeSemanticModel parentSemanticModelOpt, int speculatedPosition)
	{
		_root = root;
		_memberSymbol = memberSymbol;
		RootBinder = rootBinder.WithAdditionalFlags(GetSemanticModelBinderFlags());
		_containingSemanticModelOpt = containingSemanticModelOpt;
		_parentSemanticModelOpt = parentSemanticModelOpt;
		_speculatedPosition = speculatedPosition;
		_operationFactory = new Lazy<CSharpOperationFactory>(() => new CSharpOperationFactory(this));
	}

	internal override MemberSemanticModel GetMemberModel(SyntaxNode node)
	{
		if (!IsInTree(node))
		{
			return null;
		}
		return this;
	}

	internal sealed override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, TypeSyntax type, SpeculativeBindingOption bindingOption, out SemanticModel speculativeModel)
	{
		ExpressionSyntax standaloneExpression = SyntaxFactory.GetStandaloneExpression(type);
		Binder speculativeBinder = GetSpeculativeBinder(position, standaloneExpression, bindingOption);
		if (speculativeBinder != null)
		{
			speculativeModel = new SpeculativeMemberSemanticModel(parentModel, _memberSymbol, type, speculativeBinder, position);
			return true;
		}
		speculativeModel = null;
		return false;
	}

	internal sealed override bool TryGetSpeculativeSemanticModelCore(SyntaxTreeSemanticModel parentModel, int position, CrefSyntax crefSyntax, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	private Binder GetEnclosingBinderInternalWithinRoot(SyntaxNode node, int position)
	{
		return GetEnclosingBinderInternalWithinRoot(node, position, RootBinder, _root).WithAdditionalFlags(GetSemanticModelBinderFlags());
	}

	private static Binder GetEnclosingBinderInternalWithinRoot(SyntaxNode node, int position, Binder rootBinder, SyntaxNode root)
	{
		if (node == root)
		{
			return rootBinder.GetBinder(node) ?? rootBinder;
		}
		ExpressionSyntax expressionSyntax = null;
		LocalFunctionStatementSyntax localFunctionStatementSyntax = null;
		Binder binder = null;
		SyntaxNode syntaxNode = node;
		while (binder == null)
		{
			StatementSyntax statementSyntax = syntaxNode as StatementSyntax;
			SyntaxKind syntaxKind = syntaxNode.Kind();
			if (statementSyntax != null)
			{
				if (LookupPosition.IsInStatementScope(position, statementSyntax))
				{
					binder = rootBinder.GetBinder(syntaxNode);
					if (binder != null)
					{
						binder = AdjustBinderForPositionWithinStatement(position, binder, statementSyntax);
					}
					else if (syntaxKind == SyntaxKind.LocalFunctionStatement)
					{
						LocalFunctionStatementSyntax localFunctionStatementSyntax2 = (LocalFunctionStatementSyntax)statementSyntax;
						if (localFunctionStatementSyntax2.TypeParameterList != null && !LookupPosition.IsBetweenTokens(position, localFunctionStatementSyntax2.Identifier, localFunctionStatementSyntax2.TypeParameterList.LessThanToken))
						{
							localFunctionStatementSyntax = localFunctionStatementSyntax2;
						}
					}
				}
			}
			else
			{
				switch (syntaxKind)
				{
				case SyntaxKind.CatchClause:
					if (LookupPosition.IsInCatchBlockScope(position, (CatchClauseSyntax)syntaxNode))
					{
						binder = rootBinder.GetBinder(syntaxNode);
					}
					break;
				case SyntaxKind.CatchFilterClause:
					if (LookupPosition.IsInCatchFilterScope(position, (CatchFilterClauseSyntax)syntaxNode))
					{
						binder = rootBinder.GetBinder(syntaxNode);
					}
					break;
				default:
				{
					if (syntaxNode.IsAnonymousFunction())
					{
						if (LookupPosition.IsInAnonymousFunctionOrQuery(position, syntaxNode))
						{
							binder = rootBinder.GetBinder(syntaxNode.AnonymousFunctionBody());
						}
						break;
					}
					TypeOfExpressionSyntax typeOfExpressionSyntax;
					if (syntaxKind == SyntaxKind.TypeOfExpression && expressionSyntax == null && LookupPosition.IsBetweenTokens(position, (typeOfExpressionSyntax = (TypeOfExpressionSyntax)syntaxNode).OpenParenToken, typeOfExpressionSyntax.CloseParenToken))
					{
						expressionSyntax = typeOfExpressionSyntax.Type;
						break;
					}
					switch (syntaxKind)
					{
					case SyntaxKind.SwitchSection:
						if (LookupPosition.IsInSwitchSectionScope(position, (SwitchSectionSyntax)syntaxNode))
						{
							binder = rootBinder.GetBinder(syntaxNode);
						}
						break;
					case SyntaxKind.ArgumentList:
					{
						ArgumentListSyntax argumentListSyntax = (ArgumentListSyntax)syntaxNode;
						if (LookupPosition.IsBetweenTokens(position, argumentListSyntax.OpenParenToken, argumentListSyntax.CloseParenToken))
						{
							binder = rootBinder.GetBinder(syntaxNode);
						}
						break;
					}
					case SyntaxKind.EqualsValueClause:
						binder = rootBinder.GetBinder(syntaxNode);
						break;
					case SyntaxKind.Attribute:
						binder = rootBinder.GetBinder(syntaxNode);
						break;
					case SyntaxKind.ArrowExpressionClause:
						binder = rootBinder.GetBinder(syntaxNode);
						break;
					case SyntaxKind.BaseConstructorInitializer:
					case SyntaxKind.ThisConstructorInitializer:
						binder = rootBinder.GetBinder(syntaxNode);
						break;
					case SyntaxKind.ConstructorDeclaration:
						binder = rootBinder.GetBinder(syntaxNode);
						break;
					default:
						if ((syntaxNode as ExpressionSyntax).IsValidScopeDesignator())
						{
							binder = rootBinder.GetBinder(syntaxNode);
						}
						break;
					}
					break;
				}
				}
			}
			if (syntaxNode == root)
			{
				break;
			}
			syntaxNode = syntaxNode.ParentOrStructuredTriviaParent;
		}
		binder = binder ?? rootBinder.GetBinder(root) ?? rootBinder;
		if (localFunctionStatementSyntax != null)
		{
			LocalFunctionSymbol declaredLocalFunction = GetDeclaredLocalFunction(binder, localFunctionStatementSyntax.Identifier);
			if ((object)declaredLocalFunction != null)
			{
				binder = declaredLocalFunction.SignatureBinder;
			}
		}
		if (expressionSyntax != null)
		{
			binder = new TypeofBinder(expressionSyntax, binder);
		}
		return binder;
	}

	private static Binder AdjustBinderForPositionWithinStatement(int position, Binder binder, StatementSyntax stmt)
	{
		switch (stmt.Kind())
		{
		case SyntaxKind.SwitchStatement:
		{
			SwitchStatementSyntax switchStatementSyntax = (SwitchStatementSyntax)stmt;
			if (LookupPosition.IsBetweenTokens(position, switchStatementSyntax.OpenParenToken, switchStatementSyntax.OpenBraceToken))
			{
				binder = binder.GetBinder(switchStatementSyntax.Expression);
			}
			break;
		}
		case SyntaxKind.ForStatement:
		{
			ForStatementSyntax forStatementSyntax = (ForStatementSyntax)stmt;
			if (LookupPosition.IsBetweenTokens(position, forStatementSyntax.SecondSemicolonToken, forStatementSyntax.CloseParenToken) && forStatementSyntax.Incrementors.Count > 0)
			{
				binder = binder.GetBinder(forStatementSyntax.Incrementors.First());
			}
			else if (LookupPosition.IsBetweenTokens(position, forStatementSyntax.FirstSemicolonToken, LookupPosition.GetFirstExcludedToken(forStatementSyntax)) && forStatementSyntax.Condition != null)
			{
				binder = binder.GetBinder(forStatementSyntax.Condition);
			}
			break;
		}
		case SyntaxKind.ForEachStatement:
		case SyntaxKind.ForEachVariableStatement:
		{
			CommonForEachStatementSyntax commonForEachStatementSyntax = (CommonForEachStatementSyntax)stmt;
			SyntaxToken firstIncluded = ((stmt.Kind() == SyntaxKind.ForEachVariableStatement) ? commonForEachStatementSyntax.InKeyword : commonForEachStatementSyntax.OpenParenToken);
			if (LookupPosition.IsBetweenTokens(position, firstIncluded, commonForEachStatementSyntax.Statement.GetFirstToken()))
			{
				binder = binder.GetBinder(commonForEachStatementSyntax.Expression);
			}
			break;
		}
		}
		return binder;
	}

	public override Conversion ClassifyConversion(ExpressionSyntax expression, ITypeSymbol destination, bool isExplicitInSource = false)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		TypeSymbol destination2 = destination.EnsureCSharpSymbolOrNull<ITypeSymbol, TypeSymbol>("destination");
		if (expression.Kind() == SyntaxKind.DeclarationExpression)
		{
			return Conversion.NoConversion;
		}
		if (expression.IsAnonymousFunction())
		{
			CheckSyntaxNode(expression);
			return ClassifyConversion(expression.SpanStart, expression, destination, isExplicitInSource);
		}
		if (isExplicitInSource)
		{
			return ClassifyConversionForCast(expression, destination2);
		}
		CheckSyntaxNode(expression);
		Binder enclosingBinderInternal = GetEnclosingBinderInternal(expression, GetAdjustedNodePosition(expression));
		CSharpSyntaxNode bindableSyntaxNode = GetBindableSyntaxNode(expression);
		BoundExpression boundExpression = GetLowerBoundNode(bindableSyntaxNode) as BoundExpression;
		if (enclosingBinderInternal == null || boundExpression == null)
		{
			return Conversion.NoConversion;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		return enclosingBinderInternal.Conversions.ClassifyConversionFromExpression(boundExpression, destination2, ref useSiteDiagnostics);
	}

	internal override Conversion ClassifyConversionForCast(ExpressionSyntax expression, TypeSymbol destination)
	{
		CheckSyntaxNode(expression);
		if ((object)destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		Binder enclosingBinderInternal = GetEnclosingBinderInternal(expression, GetAdjustedNodePosition(expression));
		CSharpSyntaxNode bindableSyntaxNode = GetBindableSyntaxNode(expression);
		BoundExpression boundExpression = GetLowerBoundNode(bindableSyntaxNode) as BoundExpression;
		if (enclosingBinderInternal == null || boundExpression == null)
		{
			return Conversion.NoConversion;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		return enclosingBinderInternal.Conversions.ClassifyConversionFromExpression(boundExpression, destination, ref useSiteDiagnostics, forCast: true);
	}

	internal virtual BoundNode GetBoundRoot()
	{
		return GetUpperBoundNode(GetBindableSyntaxNode(Root));
	}

	internal BoundNode GetUpperBoundNode(CSharpSyntaxNode node, bool promoteToBindable = false)
	{
		if (promoteToBindable)
		{
			node = GetBindableSyntaxNode(node);
		}
		ImmutableArray<BoundNode> boundNodes = GetBoundNodes(node);
		if (boundNodes.Length == 0)
		{
			return null;
		}
		return boundNodes[0];
	}

	internal BoundNode GetLowerBoundNode(CSharpSyntaxNode node)
	{
		ImmutableArray<BoundNode> boundNodes = GetBoundNodes(node);
		if (boundNodes.Length == 0)
		{
			return null;
		}
		return GetLowerBoundNode(boundNodes);
	}

	private static BoundNode GetLowerBoundNode(ImmutableArray<BoundNode> boundNodes)
	{
		return boundNodes[boundNodes.Length - 1];
	}

	public override ImmutableArray<Diagnostic> GetSyntaxDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotSupportedException();
	}

	public override ImmutableArray<Diagnostic> GetDeclarationDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotSupportedException();
	}

	public override ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotSupportedException();
	}

	public override ImmutableArray<Diagnostic> GetDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotSupportedException();
	}

	public override INamespaceSymbol GetDeclaredSymbol(NamespaceDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override INamedTypeSymbol GetDeclaredSymbol(BaseTypeDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override INamedTypeSymbol GetDeclaredSymbol(DelegateDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IFieldSymbol GetDeclaredSymbol(EnumMemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override ISymbol GetDeclaredSymbol(LocalFunctionStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetDeclaredLocalFunction(declarationSyntax, declarationSyntax.Identifier);
	}

	public override ISymbol GetDeclaredSymbol(MemberDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IMethodSymbol GetDeclaredSymbol(BaseMethodDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override ISymbol GetDeclaredSymbol(BasePropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IPropertySymbol GetDeclaredSymbol(PropertyDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IPropertySymbol GetDeclaredSymbol(IndexerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IEventSymbol GetDeclaredSymbol(EventDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IMethodSymbol GetDeclaredSymbol(AccessorDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IMethodSymbol GetDeclaredSymbol(ArrowExpressionClauseSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override ISymbol GetDeclaredSymbol(VariableDeclaratorSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetDeclaredLocal(declarationSyntax, declarationSyntax.Identifier);
	}

	public override ISymbol GetDeclaredSymbol(SingleVariableDesignationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetDeclaredLocal(declarationSyntax, declarationSyntax.Identifier);
	}

	private LocalSymbol GetDeclaredLocal(CSharpSyntaxNode declarationSyntax, SyntaxToken declaredIdentifier)
	{
		for (Binder binder = GetEnclosingBinder(GetAdjustedNodePosition(declarationSyntax)); binder != null; binder = binder.Next)
		{
			foreach (LocalSymbol local in binder.Locals)
			{
				if (local.IdentifierToken == declaredIdentifier)
				{
					return local;
				}
			}
		}
		return null;
	}

	private LocalFunctionSymbol GetDeclaredLocalFunction(LocalFunctionStatementSyntax declarationSyntax, SyntaxToken declaredIdentifier)
	{
		return GetDeclaredLocalFunction(GetEnclosingBinder(GetAdjustedNodePosition(declarationSyntax)), declaredIdentifier);
	}

	private static LocalFunctionSymbol GetDeclaredLocalFunction(Binder enclosingBinder, SyntaxToken declaredIdentifier)
	{
		for (Binder binder = enclosingBinder; binder != null; binder = binder.Next)
		{
			foreach (LocalFunctionSymbol localFunction in binder.LocalFunctions)
			{
				if (localFunction.NameToken == declaredIdentifier)
				{
					return localFunction;
				}
			}
		}
		return null;
	}

	public override ILabelSymbol GetDeclaredSymbol(LabeledStatementSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		Binder binder = GetEnclosingBinder(GetAdjustedNodePosition(declarationSyntax));
		while (binder != null && !binder.IsLabelsScopeBinder)
		{
			binder = binder.Next;
		}
		if (binder != null)
		{
			foreach (LabelSymbol label in binder.Labels)
			{
				if (label.IdentifierNodeOrToken.IsToken && label.IdentifierNodeOrToken.AsToken() == declarationSyntax.Identifier)
				{
					return label;
				}
			}
		}
		return null;
	}

	public override ILabelSymbol GetDeclaredSymbol(SwitchLabelSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		Binder binder = GetEnclosingBinder(GetAdjustedNodePosition(declarationSyntax));
		while (binder != null && !(binder is SwitchBinder))
		{
			binder = binder.Next;
		}
		if (binder != null)
		{
			foreach (LabelSymbol label in binder.Labels)
			{
				if (label.IdentifierNodeOrToken.IsNode && label.IdentifierNodeOrToken.AsNode() == declarationSyntax)
				{
					return label;
				}
			}
		}
		return null;
	}

	public override IAliasSymbol GetDeclaredSymbol(UsingDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IAliasSymbol GetDeclaredSymbol(ExternAliasDirectiveSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IParameterSymbol GetDeclaredSymbol(ParameterSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declarationSyntax);
		return GetLambdaOrLocalFunctionParameterSymbol(declarationSyntax, cancellationToken);
	}

	internal override ImmutableArray<ISymbol> GetDeclaredSymbols(BaseFieldDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return ImmutableArray.Create<ISymbol>();
	}

	private ParameterSymbol GetLambdaOrLocalFunctionParameterSymbol(ParameterSyntax parameter, CancellationToken cancellationToken)
	{
		if (parameter.Parent is SimpleLambdaExpressionSyntax lambda)
		{
			return GetLambdaParameterSymbol(parameter, lambda, cancellationToken);
		}
		if (!(parameter.Parent is ParameterListSyntax { Parent: not null } parameterListSyntax))
		{
			return null;
		}
		if (parameterListSyntax.Parent.IsAnonymousFunction())
		{
			return GetLambdaParameterSymbol(parameter, (ExpressionSyntax)parameterListSyntax.Parent, cancellationToken);
		}
		if (parameterListSyntax.Parent.Kind() == SyntaxKind.LocalFunctionStatement)
		{
			MethodSymbol methodSymbol = (MethodSymbol)GetDeclaredSymbol((LocalFunctionStatementSyntax)parameterListSyntax.Parent, cancellationToken);
			if ((object)methodSymbol != null)
			{
				return GetParameterSymbol(methodSymbol.Parameters, parameter, cancellationToken);
			}
		}
		return null;
	}

	private ParameterSymbol GetLambdaParameterSymbol(ParameterSyntax parameter, ExpressionSyntax lambda, CancellationToken cancellationToken)
	{
		SymbolInfo symbolInfo = GetSymbolInfo(lambda, cancellationToken);
		LambdaSymbol lambdaSymbol;
		if (symbolInfo.Symbol != null)
		{
			lambdaSymbol = (LambdaSymbol)symbolInfo.Symbol;
		}
		else
		{
			if (symbolInfo.CandidateSymbols.Length != 1)
			{
				return null;
			}
			lambdaSymbol = (LambdaSymbol)symbolInfo.CandidateSymbols.Single();
		}
		return GetParameterSymbol(lambdaSymbol.Parameters, parameter, cancellationToken);
	}

	public override ITypeParameterSymbol GetDeclaredSymbol(TypeParameterSyntax typeParameter, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override IRangeVariableSymbol GetDeclaredSymbol(JoinIntoClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetBoundQueryClause(node)?.DefinedSymbol;
	}

	public override IRangeVariableSymbol GetDeclaredSymbol(QueryClauseSyntax queryClause, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetBoundQueryClause(queryClause)?.DefinedSymbol;
	}

	public override IRangeVariableSymbol GetDeclaredSymbol(QueryContinuationSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetBoundQueryClause(node)?.DefinedSymbol;
	}

	public override AwaitExpressionInfo GetAwaitExpressionInfo(AwaitExpressionSyntax node)
	{
		if (node.Kind() != SyntaxKind.AwaitExpression)
		{
			throw new ArgumentException("node.Kind==" + node.Kind());
		}
		BoundNode upperBoundNode = GetUpperBoundNode(node);
		if (!(((upperBoundNode as BoundExpressionStatement)?.Expression ?? upperBoundNode) is BoundAwaitExpression boundAwaitExpression))
		{
			return default;
		}
		return new AwaitExpressionInfo(boundAwaitExpression.GetAwaiter, boundAwaitExpression.IsCompleted, boundAwaitExpression.GetResult, boundAwaitExpression.IsDynamic);
	}

	public override ForEachStatementInfo GetForEachStatementInfo(ForEachStatementSyntax node)
	{
		return GetForEachStatementInfo((CommonForEachStatementSyntax)node);
	}

	public override ForEachStatementInfo GetForEachStatementInfo(CommonForEachStatementSyntax node)
	{
		BoundForEachStatement boundForEachStatement = (BoundForEachStatement)GetUpperBoundNode(node);
		if (boundForEachStatement == null)
		{
			return default;
		}
		ForEachEnumeratorInfo enumeratorInfoOpt = boundForEachStatement.EnumeratorInfoOpt;
		if (enumeratorInfoOpt == null)
		{
			return default;
		}
		if (enumeratorInfoOpt.ElementType.IsPointerType())
		{
			return default;
		}
		return new ForEachStatementInfo(enumeratorInfoOpt.GetEnumeratorMethod, enumeratorInfoOpt.MoveNextMethod, (PropertySymbol)enumeratorInfoOpt.CurrentPropertyGetter.AssociatedSymbol, enumeratorInfoOpt.NeedsDisposeMethod ? ((MethodSymbol)Compilation.GetSpecialTypeMember(SpecialMember.System_IDisposable__Dispose)) : null, enumeratorInfoOpt.ElementType, boundForEachStatement.ElementConversion, enumeratorInfoOpt.CurrentConversion);
	}

	public override DeconstructionInfo GetDeconstructionInfo(AssignmentExpressionSyntax node)
	{
		if (!(GetUpperBoundNode(node) is BoundDeconstructionAssignmentOperator { Right: var right }))
		{
			return default;
		}
		if (right == null)
		{
			return default;
		}
		return new DeconstructionInfo(right.Conversion);
	}

	public override DeconstructionInfo GetDeconstructionInfo(ForEachVariableStatementSyntax node)
	{
		BoundForEachStatement boundForEachStatement = (BoundForEachStatement)GetUpperBoundNode(node);
		if (boundForEachStatement == null)
		{
			return default;
		}
		BoundForEachDeconstructStep deconstructionOpt = boundForEachStatement.DeconstructionOpt;
		if (deconstructionOpt == null)
		{
			return default;
		}
		return new DeconstructionInfo(deconstructionOpt.DeconstructionAssignment.Right.Conversion);
	}

	private BoundQueryClause GetBoundQueryClause(CSharpSyntaxNode node)
	{
		CheckSyntaxNode(node);
		return GetLowerBoundNode(node) as BoundQueryClause;
	}

	private QueryClauseInfo GetQueryClauseInfo(BoundQueryClause bound)
	{
		if (bound == null)
		{
			return default;
		}
		SymbolInfo castInfo = ((bound.Cast == null) ? SymbolInfo.None : GetSymbolInfoForNode(SymbolInfoOptions.DefaultOptions, bound.Cast, bound.Cast, null, null));
		SymbolInfo symbolInfoForQuery = GetSymbolInfoForQuery(bound);
		return new QueryClauseInfo(castInfo, symbolInfoForQuery);
	}

	private SymbolInfo GetSymbolInfoForQuery(BoundQueryClause bound)
	{
		if (!(bound?.Operation is BoundCall boundCall))
		{
			return SymbolInfo.None;
		}
		BoundExpression boundExpression = (boundCall.IsDelegateCall ? boundCall.ReceiverOpt : boundCall);
		return GetSymbolInfoForNode(SymbolInfoOptions.DefaultOptions, boundExpression, boundExpression, null, null);
	}

	private CSharpTypeInfo GetTypeInfoForQuery(BoundQueryClause bound)
	{
		if (bound != null)
		{
			return GetTypeInfoForNode(bound, bound, bound);
		}
		return CSharpTypeInfo.None;
	}

	public override QueryClauseInfo GetQueryClauseInfo(QueryClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		BoundQueryClause boundQueryClause = GetBoundQueryClause(node);
		return GetQueryClauseInfo(boundQueryClause);
	}

	public override IPropertySymbol GetDeclaredSymbol(AnonymousObjectMemberDeclaratorSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declaratorSyntax);
		AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpressionSyntax = (AnonymousObjectCreationExpressionSyntax)declaratorSyntax.Parent;
		if (anonymousObjectCreationExpressionSyntax == null)
		{
			return null;
		}
		if (!(GetLowerBoundNode(anonymousObjectCreationExpressionSyntax) is BoundAnonymousObjectCreationExpression boundAnonymousObjectCreationExpression))
		{
			return null;
		}
		if (!(boundAnonymousObjectCreationExpression.Type is NamedTypeSymbol type))
		{
			return null;
		}
		int index = anonymousObjectCreationExpressionSyntax.Initializers.IndexOf(declaratorSyntax);
		return AnonymousTypeManager.GetAnonymousTypeProperty(type, index);
	}

	public override INamedTypeSymbol GetDeclaredSymbol(AnonymousObjectCreationExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declaratorSyntax);
		if (GetLowerBoundNode(declaratorSyntax) is BoundAnonymousObjectCreationExpression boundAnonymousObjectCreationExpression)
		{
			return boundAnonymousObjectCreationExpression.Type as NamedTypeSymbol;
		}
		return null;
	}

	public override INamedTypeSymbol GetDeclaredSymbol(TupleExpressionSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declaratorSyntax);
		return GetTypeOfTupleLiteral(declaratorSyntax);
	}

	public override ISymbol GetDeclaredSymbol(ArgumentSyntax declaratorSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckSyntaxNode(declaratorSyntax);
		if (!(declaratorSyntax?.Parent is TupleExpressionSyntax tupleExpressionSyntax))
		{
			return null;
		}
		NamedTypeSymbol typeOfTupleLiteral = GetTypeOfTupleLiteral(tupleExpressionSyntax);
		if ((object)typeOfTupleLiteral != null)
		{
			ImmutableArray<FieldSymbol> tupleElements = typeOfTupleLiteral.TupleElements;
			if (!tupleElements.IsDefault)
			{
				int index = tupleExpressionSyntax.Arguments.IndexOf(declaratorSyntax);
				return tupleElements[index];
			}
		}
		return null;
	}

	private NamedTypeSymbol GetTypeOfTupleLiteral(TupleExpressionSyntax declaratorSyntax)
	{
		return (GetLowerBoundNode(declaratorSyntax) as BoundTupleExpression)?.Type as NamedTypeSymbol;
	}

	internal override IOperation GetOperationWorker(CSharpSyntaxNode node, CancellationToken cancellationToken)
	{
		CSharpSyntaxNode bindingRootOrInitializer = GetBindingRootOrInitializer(node);
		return GetStatementOrRootOperation(bindingRootOrInitializer, cancellationToken)?.DescendantsAndSelf().FirstOrDefault((IOperation o) => !o.IsImplicit && o.Syntax == node);
	}

	private CSharpSyntaxNode GetBindingRootOrInitializer(CSharpSyntaxNode node)
	{
		CSharpSyntaxNode bindingRoot = GetBindingRoot(node);
		if (bindingRoot is ParameterSyntax { Default: { FullSpan: var fullSpan } } parameterSyntax && fullSpan.Contains(node.Span))
		{
			return parameterSyntax.Default;
		}
		if (bindingRoot is VariableDeclaratorSyntax { Initializer: { FullSpan: var fullSpan2 } } variableDeclaratorSyntax && fullSpan2.Contains(node.Span))
		{
			CSharpSyntaxNode parent = variableDeclaratorSyntax.Parent;
			if (parent == null || !parent.Parent.IsKind(SyntaxKind.FieldDeclaration))
			{
				CSharpSyntaxNode parent2 = variableDeclaratorSyntax.Parent;
				if (parent2 == null || !parent2.Parent.IsKind(SyntaxKind.EventFieldDeclaration))
				{
					goto IL_00ac;
				}
			}
			return variableDeclaratorSyntax.Initializer;
		}
		goto IL_00ac;
		IL_00ac:
		if (bindingRoot is EnumMemberDeclarationSyntax { EqualsValue: { FullSpan: var fullSpan3 } } enumMemberDeclarationSyntax && fullSpan3.Contains(node.Span))
		{
			return enumMemberDeclarationSyntax.EqualsValue;
		}
		if (bindingRoot is PropertyDeclarationSyntax { Initializer: { FullSpan: var fullSpan4 } } propertyDeclarationSyntax && fullSpan4.Contains(node.Span))
		{
			return propertyDeclarationSyntax.Initializer;
		}
		return bindingRoot;
	}

	private IOperation GetStatementOrRootOperation(CSharpSyntaxNode node, CancellationToken cancellationToken)
	{
		GetBoundNodes(node, out var _, out var _, out var highestBoundNode, out var _);
		BoundNode boundNode = highestBoundNode;
		return _operationFactory.Value.Create(boundNode);
	}

	internal override SymbolInfo GetSymbolInfoWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		CSharpSemanticModel.ValidateSymbolInfoOptions(options);
		GetBoundNodes(node, out var _, out var lowestBoundNode, out var highestBoundNode, out var boundParent);
		return GetSymbolInfoForNode(options, lowestBoundNode, highestBoundNode, boundParent, null);
	}

	internal override CSharpTypeInfo GetTypeInfoWorker(CSharpSyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		GetBoundNodes(node, out var _, out var lowestBoundNode, out var highestBoundNode, out var boundParent);
		return GetTypeInfoForNode(lowestBoundNode, highestBoundNode, boundParent);
	}

	internal override ImmutableArray<Symbol> GetMemberGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		GetBoundNodes(node, out var _, out var lowestBoundNode, out var _, out var boundParent);
		return GetMemberGroupForNode(options, lowestBoundNode, boundParent, null);
	}

	internal override ImmutableArray<PropertySymbol> GetIndexerGroupWorker(CSharpSyntaxNode node, SymbolInfoOptions options, CancellationToken cancellationToken = default(CancellationToken))
	{
		GetBoundNodes(node, out var _, out var lowestBoundNode, out var _, out var _);
		return GetIndexerGroupForNode(lowestBoundNode, null);
	}

	internal override Optional<object> GetConstantValueWorker(CSharpSyntaxNode node, CancellationToken cancellationToken)
	{
		CSharpSyntaxNode bindableSyntaxNode = GetBindableSyntaxNode(node);
		if (!(GetLowerBoundNode(bindableSyntaxNode) is BoundExpression { ConstantValue: var constantValue }))
		{
			return default;
		}
		if (!(constantValue == null) && !constantValue.IsBad)
		{
			return new Optional<object>(constantValue.Value);
		}
		return default;
	}

	internal override SymbolInfo GetCollectionInitializerSymbolInfoWorker(InitializerExpressionSyntax collectionInitializer, ExpressionSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (GetLowerBoundNode(collectionInitializer) is BoundCollectionInitializerExpression { Initializers: var initializers })
		{
			BoundExpression boundExpression = initializers[collectionInitializer.Expressions.IndexOf(node)];
			return GetSymbolInfoForNode(SymbolInfoOptions.DefaultOptions, boundExpression, boundExpression, null, null);
		}
		return SymbolInfo.None;
	}

	public override SymbolInfo GetSymbolInfo(OrderingSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		BoundQueryClause boundQueryClause = GetBoundQueryClause(node);
		return GetSymbolInfoForQuery(boundQueryClause);
	}

	public override SymbolInfo GetSymbolInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		BoundQueryClause boundQueryClause = GetBoundQueryClause(node);
		return GetSymbolInfoForQuery(boundQueryClause);
	}

	public override TypeInfo GetTypeInfo(SelectOrGroupClauseSyntax node, CancellationToken cancellationToken = default(CancellationToken))
	{
		BoundQueryClause boundQueryClause = GetBoundQueryClause(node);
		return GetTypeInfoForQuery(boundQueryClause);
	}

	private void GetBoundNodes(CSharpSyntaxNode node, out CSharpSyntaxNode bindableNode, out BoundNode lowestBoundNode, out BoundNode highestBoundNode, out BoundNode boundParent)
	{
		bindableNode = GetBindableSyntaxNode(node);
		CSharpSyntaxNode bindableParentNode = GetBindableParentNode(bindableNode);
		if (bindableParentNode != null && bindableParentNode.Kind() == SyntaxKind.SimpleMemberAccessExpression && ((MemberAccessExpressionSyntax)bindableParentNode).Expression == bindableNode)
		{
			bindableParentNode = GetBindableParentNode(bindableParentNode);
		}
		boundParent = ((bindableParentNode == null) ? null : GetLowerBoundNode(bindableParentNode));
		lowestBoundNode = GetLowerBoundNode(bindableNode);
		highestBoundNode = GetUpperBoundNode(bindableNode);
	}

	private CSharpSyntaxNode GetInnermostLambdaOrQuery(CSharpSyntaxNode node, int position, bool allowStarting = false)
	{
		for (CSharpSyntaxNode cSharpSyntaxNode = node; cSharpSyntaxNode != Root; cSharpSyntaxNode = cSharpSyntaxNode.ParentOrStructuredTriviaParent)
		{
			if ((cSharpSyntaxNode.IsAnonymousFunction() || cSharpSyntaxNode.IsQuery()) && LookupPosition.IsInAnonymousFunctionOrQuery(position, cSharpSyntaxNode) && (allowStarting || cSharpSyntaxNode != node))
			{
				return cSharpSyntaxNode;
			}
		}
		return null;
	}

	private void GuardedAddSynthesizedStatementToMap(StatementSyntax node, BoundStatement statement)
	{
		if (_lazyGuardedSynthesizedStatementsMap == null)
		{
			_lazyGuardedSynthesizedStatementsMap = new Dictionary<SyntaxNode, BoundStatement>();
		}
		_lazyGuardedSynthesizedStatementsMap.Add(node, statement);
	}

	private BoundStatement GuardedGetSynthesizedStatementFromMap(StatementSyntax node)
	{
		if (_lazyGuardedSynthesizedStatementsMap != null && _lazyGuardedSynthesizedStatementsMap.TryGetValue(node, out var value))
		{
			return value;
		}
		return null;
	}

	private ImmutableArray<BoundNode> GuardedGetBoundNodesFromMap(CSharpSyntaxNode node)
	{
		if (!_guardedNodeMap.TryGetValue(node, out var value))
		{
			return default;
		}
		return value;
	}

	internal ImmutableArray<BoundNode> TestOnlyTryGetBoundNodesFromMap(CSharpSyntaxNode node)
	{
		if (!_guardedNodeMap.TryGetValue(node, out var value))
		{
			return default;
		}
		return value;
	}

	private ImmutableArray<BoundNode> GuardedAddBoundTreeAndGetBoundNodeFromMap(CSharpSyntaxNode syntax, BoundNode bound)
	{
		bool flag = false;
		if (bound != null)
		{
			flag = _guardedNodeMap.ContainsKey(bound.Syntax);
		}
		if (!flag)
		{
			NodeMapBuilder.AddToMap(bound, _guardedNodeMap);
		}
		if (!_guardedNodeMap.TryGetValue(syntax, out var value))
		{
			return default;
		}
		return value;
	}

	protected void UnguardedAddBoundTreeForStandaloneSyntax(SyntaxNode syntax, BoundNode bound)
	{
		using (_nodeMapLock.DisposableWrite())
		{
			GuardedAddBoundTreeForStandaloneSyntax(syntax, bound);
		}
	}

	protected void GuardedAddBoundTreeForStandaloneSyntax(SyntaxNode syntax, BoundNode bound)
	{
		bool flag = false;
		if (bound != null)
		{
			flag = _guardedNodeMap.ContainsKey(bound.Syntax);
		}
		if (!flag)
		{
			if (syntax == _root || syntax is StatementSyntax)
			{
				NodeMapBuilder.AddToMap(bound, _guardedNodeMap);
			}
			else
			{
				NodeMapBuilder.AddToMap(bound, _guardedNodeMap, syntax);
			}
		}
	}

	private CSharpSyntaxNode GetBindingRoot(CSharpSyntaxNode node)
	{
		for (CSharpSyntaxNode cSharpSyntaxNode = node; cSharpSyntaxNode != Root; cSharpSyntaxNode = cSharpSyntaxNode.ParentOrStructuredTriviaParent)
		{
			if (cSharpSyntaxNode is StatementSyntax)
			{
				return cSharpSyntaxNode;
			}
			switch (cSharpSyntaxNode.Kind())
			{
			case SyntaxKind.BaseConstructorInitializer:
			case SyntaxKind.ThisConstructorInitializer:
				return cSharpSyntaxNode;
			case SyntaxKind.ArrowExpressionClause:
				if (cSharpSyntaxNode.Parent == null || cSharpSyntaxNode.Parent.Kind() != SyntaxKind.LocalFunctionStatement)
				{
					return cSharpSyntaxNode;
				}
				break;
			}
		}
		return Root;
	}

	internal override Binder GetEnclosingBinderInternal(int position)
	{
		if (!Root.FullSpan.Contains(position))
		{
			return RootBinder;
		}
		CSharpSyntaxNode node = (CSharpSyntaxNode)Root.FindToken(position).Parent;
		return GetEnclosingBinderInternal(node, position);
	}

	private Binder GetEnclosingBinderInternal(CSharpSyntaxNode node, int position)
	{
		CSharpSyntaxNode innermostLambdaOrQuery = GetInnermostLambdaOrQuery(node, position, allowStarting: true);
		if (innermostLambdaOrQuery == null)
		{
			return GetEnclosingBinderInternalWithinRoot(node, position);
		}
		BoundNode boundInnerLambdaOrQuery = GetBoundLambdaOrQuery(innermostLambdaOrQuery);
		return GetEnclosingBinderInLambdaOrQuery(position, node, innermostLambdaOrQuery, ref boundInnerLambdaOrQuery);
	}

	private BoundNode GetBoundLambdaOrQuery(CSharpSyntaxNode lambdaOrQuery)
	{
		ImmutableArray<BoundNode> boundNodes;
		using (_nodeMapLock.DisposableRead())
		{
			boundNodes = GuardedGetBoundNodesFromMap(lambdaOrQuery);
		}
		if (!boundNodes.IsDefaultOrEmpty)
		{
			return GetLowerBoundNode(boundNodes);
		}
		CSharpSyntaxNode bindingRoot = GetBindingRoot(lambdaOrQuery);
		CSharpSyntaxNode innermostLambdaOrQuery = GetInnermostLambdaOrQuery(lambdaOrQuery, lambdaOrQuery.SpanStart);
		BoundNode boundInnerLambdaOrQuery = null;
		CSharpSyntaxNode node;
		Binder next;
		if (innermostLambdaOrQuery == null)
		{
			node = bindingRoot;
			next = GetEnclosingBinderInternalWithinRoot(node, GetAdjustedNodePosition(node));
		}
		else
		{
			node = ((innermostLambdaOrQuery != bindingRoot && innermostLambdaOrQuery.Contains(bindingRoot)) ? bindingRoot : lambdaOrQuery);
			boundInnerLambdaOrQuery = GetBoundLambdaOrQuery(innermostLambdaOrQuery);
			using (_nodeMapLock.DisposableRead())
			{
				boundNodes = GuardedGetBoundNodesFromMap(lambdaOrQuery);
			}
			if (!boundNodes.IsDefaultOrEmpty)
			{
				return GetLowerBoundNode(boundNodes);
			}
			next = GetEnclosingBinderInLambdaOrQuery(GetAdjustedNodePosition(node), node, innermostLambdaOrQuery, ref boundInnerLambdaOrQuery);
		}
		Binder binder = new IncrementalBinder(this, next);
		using (_nodeMapLock.DisposableWrite())
		{
			BoundNode bound = Bind(binder, node, _ignoredDiagnostics);
			boundNodes = GuardedAddBoundTreeAndGetBoundNodeFromMap(lambdaOrQuery, bound);
		}
		if (!boundNodes.IsDefaultOrEmpty)
		{
			return GetLowerBoundNode(boundNodes);
		}
		next = ((innermostLambdaOrQuery != null) ? GetEnclosingBinderInLambdaOrQuery(GetAdjustedNodePosition(lambdaOrQuery), lambdaOrQuery, innermostLambdaOrQuery, ref boundInnerLambdaOrQuery) : GetEnclosingBinderInternalWithinRoot(lambdaOrQuery, GetAdjustedNodePosition(lambdaOrQuery)));
		binder = new IncrementalBinder(this, next);
		using (_nodeMapLock.DisposableWrite())
		{
			BoundNode bound2 = Bind(binder, lambdaOrQuery, _ignoredDiagnostics);
			boundNodes = GuardedAddBoundTreeAndGetBoundNodeFromMap(lambdaOrQuery, bound2);
		}
		return GetLowerBoundNode(boundNodes);
	}

	private Binder GetEnclosingBinderInLambdaOrQuery(int position, CSharpSyntaxNode node, CSharpSyntaxNode innerLambdaOrQuery, ref BoundNode boundInnerLambdaOrQuery)
	{
		Binder binder;
		switch (boundInnerLambdaOrQuery.Kind)
		{
		case BoundKind.UnboundLambda:
			boundInnerLambdaOrQuery = ((UnboundLambda)boundInnerLambdaOrQuery).BindForErrorRecovery();
			goto case BoundKind.Lambda;
		case BoundKind.Lambda:
			binder = GetLambdaEnclosingBinder(position, node, innerLambdaOrQuery, ((BoundLambda)boundInnerLambdaOrQuery).Binder);
			break;
		case BoundKind.QueryClause:
			binder = GetQueryEnclosingBinder(position, node, (BoundQueryClause)boundInnerLambdaOrQuery);
			break;
		default:
			return GetEnclosingBinderInternalWithinRoot(node, position);
		}
		return binder.WithAdditionalFlags(GetSemanticModelBinderFlags());
	}

	private static Binder GetQueryEnclosingBinder(int position, CSharpSyntaxNode startingNode, BoundQueryClause queryClause)
	{
		BoundExpression boundExpression = queryClause;
		do
		{
			switch (boundExpression.Kind)
			{
			case BoundKind.QueryClause:
				queryClause = (BoundQueryClause)boundExpression;
				boundExpression = GetQueryClauseValue(queryClause);
				continue;
			case BoundKind.Call:
			{
				BoundCall boundCall = (BoundCall)boundExpression;
				boundExpression = GetContainingArgument(boundCall.Arguments, position);
				if (boundExpression != null)
				{
					continue;
				}
				BoundExpression receiverOpt = boundCall.ReceiverOpt;
				while (receiverOpt != null && receiverOpt.Kind == BoundKind.MethodGroup)
				{
					receiverOpt = ((BoundMethodGroup)receiverOpt).ReceiverOpt;
				}
				if (receiverOpt != null)
				{
					boundExpression = GetContainingExprOrQueryClause(receiverOpt, position);
					if (boundExpression != null)
					{
						continue;
					}
				}
				boundExpression = boundCall.Arguments.LastOrDefault();
				continue;
			}
			case BoundKind.Conversion:
				boundExpression = ((BoundConversion)boundExpression).Operand;
				continue;
			case BoundKind.UnboundLambda:
			{
				UnboundLambda unboundLambda = (UnboundLambda)boundExpression;
				return GetEnclosingBinderInternalWithinRoot(AdjustStartingNodeAccordingToNewRoot(startingNode, unboundLambda.Syntax), position, unboundLambda.BindForErrorRecovery().Binder, unboundLambda.Syntax);
			}
			case BoundKind.Lambda:
			{
				BoundLambda boundLambda = (BoundLambda)boundExpression;
				return GetEnclosingBinderInternalWithinRoot(AdjustStartingNodeAccordingToNewRoot(startingNode, boundLambda.Body.Syntax), position, boundLambda.Binder, boundLambda.Body.Syntax);
			}
			}
			break;
		}
		while (boundExpression != null);
		return GetEnclosingBinderInternalWithinRoot(AdjustStartingNodeAccordingToNewRoot(startingNode, queryClause.Syntax), position, queryClause.Binder, queryClause.Syntax);
	}

	private static BoundExpression GetContainingArgument(ImmutableArray<BoundExpression> arguments, int position)
	{
		BoundExpression boundExpression = null;
		TextSpan textSpan = default;
		foreach (BoundExpression item in arguments)
		{
			BoundExpression containingExprOrQueryClause = GetContainingExprOrQueryClause(item, position);
			if (containingExprOrQueryClause != null)
			{
				TextSpan fullSpan = containingExprOrQueryClause.Syntax.FullSpan;
				if (boundExpression == null || textSpan.Contains(fullSpan))
				{
					boundExpression = containingExprOrQueryClause;
					textSpan = fullSpan;
				}
			}
		}
		return boundExpression;
	}

	private static BoundExpression GetContainingExprOrQueryClause(BoundExpression expr, int position)
	{
		if (expr.Kind == BoundKind.QueryClause)
		{
			BoundExpression queryClauseValue = GetQueryClauseValue((BoundQueryClause)expr);
			if (queryClauseValue.Syntax.FullSpan.Contains(position))
			{
				return queryClauseValue;
			}
		}
		if (expr.Syntax.FullSpan.Contains(position))
		{
			return expr;
		}
		return null;
	}

	private static BoundExpression GetQueryClauseValue(BoundQueryClause queryClause)
	{
		return queryClause.UnoptimizedForm ?? queryClause.Value;
	}

	private static SyntaxNode AdjustStartingNodeAccordingToNewRoot(SyntaxNode startingNode, SyntaxNode root)
	{
		SyntaxNode syntaxNode = (startingNode.Contains(root) ? root : startingNode);
		if (syntaxNode != root && !root.Contains(syntaxNode))
		{
			syntaxNode = root;
		}
		return syntaxNode;
	}

	private static Binder GetLambdaEnclosingBinder(int position, CSharpSyntaxNode startingNode, CSharpSyntaxNode containingLambda, Binder lambdaBinder)
	{
		return GetEnclosingBinderInternalWithinRoot(startingNode, position, lambdaBinder, containingLambda);
	}

	internal ImmutableArray<BoundNode> GetBoundNodes(CSharpSyntaxNode node)
	{
		if (node == null)
		{
			node = GetBindableSyntaxNode(Root);
		}
		ImmutableArray<BoundNode> result;
		using (_nodeMapLock.DisposableRead())
		{
			result = GuardedGetBoundNodesFromMap(node);
		}
		if (!result.IsDefaultOrEmpty)
		{
			return result;
		}
		CSharpSyntaxNode bindingRoot = GetBindingRoot(node);
		Binder enclosingBinder = GetEnclosingBinder(GetAdjustedNodePosition(bindingRoot));
		Binder binder = new IncrementalBinder(this, enclosingBinder);
		using (_nodeMapLock.DisposableWrite())
		{
			BoundNode bound = Bind(binder, bindingRoot, _ignoredDiagnostics);
			result = GuardedAddBoundTreeAndGetBoundNodeFromMap(node, bound);
		}
		if (!result.IsDefaultOrEmpty)
		{
			return result;
		}
		Binder enclosingBinder2 = GetEnclosingBinder(GetAdjustedNodePosition(node));
		using (_nodeMapLock.DisposableRead())
		{
			result = GuardedGetBoundNodesFromMap(node);
		}
		if (result.IsDefaultOrEmpty)
		{
			using (_nodeMapLock.DisposableWrite())
			{
				BoundNode bound2 = Bind(enclosingBinder2, node, _ignoredDiagnostics);
				GuardedAddBoundTreeForStandaloneSyntax(node, bound2);
				result = GuardedGetBoundNodesFromMap(node);
			}
			if (!result.IsDefaultOrEmpty)
			{
				return result;
			}
			return ImmutableArray<BoundNode>.Empty;
		}
		return result;
	}

	protected internal virtual CSharpSyntaxNode GetBindableSyntaxNode(CSharpSyntaxNode node)
	{
		switch (node.Kind())
		{
		case SyntaxKind.GlobalStatement:
		case SyntaxKind.MethodDeclaration:
		case SyntaxKind.OperatorDeclaration:
		case SyntaxKind.ConversionOperatorDeclaration:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.DestructorDeclaration:
		case SyntaxKind.GetAccessorDeclaration:
		case SyntaxKind.SetAccessorDeclaration:
		case SyntaxKind.AddAccessorDeclaration:
		case SyntaxKind.RemoveAccessorDeclaration:
			return node;
		}
		while (true)
		{
			switch (node.Kind())
			{
			case SyntaxKind.ParenthesizedExpression:
				node = ((ParenthesizedExpressionSyntax)node).Expression;
				continue;
			case SyntaxKind.CheckedExpression:
			case SyntaxKind.UncheckedExpression:
				node = ((CheckedExpressionSyntax)node).Expression;
				continue;
			}
			CSharpSyntaxNode parent = node.Parent;
			if (parent != null && node != Root)
			{
				switch (node.Kind())
				{
				case SyntaxKind.IdentifierName:
				case SyntaxKind.GenericName:
				{
					CSharpSyntaxNode standaloneNode = SyntaxFactory.GetStandaloneNode(node);
					if (standaloneNode != node)
					{
						return GetBindableSyntaxNode(standaloneNode);
					}
					break;
				}
				case SyntaxKind.AnonymousObjectMemberDeclarator:
					return GetBindableSyntaxNode(parent);
				case SyntaxKind.VariableDeclarator:
				{
					CSharpSyntaxNode parent2 = parent.Parent;
					if (parent2 != null && parent2.Kind() == SyntaxKind.LocalDeclarationStatement && ((VariableDeclarationSyntax)parent).Variables.Count == 1)
					{
						return GetBindableSyntaxNode(parent);
					}
					break;
				}
				default:
					if ((node is QueryExpressionSyntax && parent is QueryContinuationSyntax) || (!(node is ExpressionSyntax) && !(node is StatementSyntax) && !(node is SelectOrGroupClauseSyntax) && !(node is QueryClauseSyntax) && !(node is OrderingSyntax) && !(node is JoinIntoClauseSyntax) && !(node is QueryContinuationSyntax) && !(node is ConstructorInitializerSyntax) && !(node is ArrowExpressionClauseSyntax)))
					{
						return GetBindableSyntaxNode(parent);
					}
					break;
				}
			}
			return node;
		}
	}

	protected CSharpSyntaxNode GetBindableParentNode(CSharpSyntaxNode node)
	{
		if (!(node is ExpressionSyntax))
		{
			return null;
		}
		CSharpSyntaxNode cSharpSyntaxNode = node.Parent;
		if (cSharpSyntaxNode == null)
		{
			if (IsSpeculativeSemanticModel && Root == node)
			{
				return null;
			}
			throw new ArgumentException("The parent of node must not be null unless this is a speculative semantic model.", "node");
		}
		while (true)
		{
			SyntaxKind syntaxKind = cSharpSyntaxNode.Kind();
			if (syntaxKind != SyntaxKind.ParenthesizedExpression && syntaxKind - 9050 > SyntaxKind.List)
			{
				break;
			}
			CSharpSyntaxNode parent = cSharpSyntaxNode.Parent;
			if (parent != null)
			{
				cSharpSyntaxNode = parent;
			}
		}
		CSharpSyntaxNode cSharpSyntaxNode2 = GetBindableSyntaxNode(cSharpSyntaxNode);
		if (cSharpSyntaxNode2.Kind() == SyntaxKind.SimpleMemberAccessExpression && cSharpSyntaxNode2.Parent.Kind() == SyntaxKind.InvocationExpression)
		{
			cSharpSyntaxNode2 = cSharpSyntaxNode2.Parent;
		}
		else if (cSharpSyntaxNode2.Kind() == SyntaxKind.ArrayType)
		{
			cSharpSyntaxNode2 = SyntaxFactory.GetStandaloneExpression((ArrayTypeSyntax)cSharpSyntaxNode2);
		}
		return cSharpSyntaxNode2;
	}
}
