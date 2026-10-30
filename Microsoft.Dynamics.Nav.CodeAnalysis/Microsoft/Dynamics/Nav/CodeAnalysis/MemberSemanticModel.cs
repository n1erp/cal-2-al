using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
internal abstract class MemberSemanticModel : SemanticModel
{
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
			DebugAssertHelper.Assert(node == null || root == null || !(root.Syntax is StatementSyntax), "individually added nodes are not supposed to be statements.");
			if (root == null || map.ContainsKey(root.Syntax))
			{
				return;
			}
			OrderPreservingMultiDictionary<SyntaxNode, BoundNode> instance = OrderPreservingMultiDictionary<SyntaxNode, BoundNode>.GetInstance();
			new NodeMapBuilder(instance, node).Visit(root);
			foreach (SyntaxNode key in instance.Keys)
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

		public override BoundNode VisitBinaryOperator(BoundBinaryOperator node)
		{
			throw ExceptionUtilities.UnexpectedValue(node.Syntax?.ToReportingString() ?? node.OperatorKind.ToString());
		}

		protected override bool ConvertInsufficientExecutionStackExceptionToCancelledByStackGuardException()
		{
			return false;
		}
	}

	internal class IncrementalBinder : Binder
	{
		private readonly MemberSemanticModel semanticModel;

		internal IncrementalBinder(MemberSemanticModel semanticModel, Binder next)
			: base(next)
		{
			this.semanticModel = semanticModel;
		}

		internal override Binder GetBinder(SyntaxNode node)
		{
			Binder binder = Next.GetBinder(node);
			if (binder != null)
			{
				DebugAssertHelper.Assert(!(binder is IncrementalBinder));
				return new IncrementalBinder(semanticModel, binder.WithAdditionalFlags(BinderFlags.SemanticModel));
			}
			return null;
		}

		public override BoundStatement BindStatement(StatementSyntax node, DiagnosticBag diagnostics)
		{
			ImmutableArray<BoundNode> immutableArray = semanticModel.GuardedGetBoundNodesFromMap(node);
			if (immutableArray.IsDefaultOrEmpty)
			{
				return base.BindStatement(node, diagnostics);
			}
			return (BoundStatement)immutableArray[0];
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	private readonly Compilation compilation;

	[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	private readonly SyntaxNode root;

	private readonly DiagnosticBag ignoredDiagnostics = new DiagnosticBag();

	private readonly ReaderWriterLockSlim nodeMapLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private readonly Dictionary<SyntaxNode, ImmutableArray<BoundNode>> guardedNodeMap = new Dictionary<SyntaxNode, ImmutableArray<BoundNode>>();

	internal readonly Binder RootBinder;

	protected ReaderWriterLockSlim NodeMapLock => nodeMapLock;

	internal override SyntaxNode Root => root;

	protected override SyntaxTree SyntaxTreeCore => root.SyntaxTree;

	protected Symbol MemberSymbol { get; }

	protected MemberSemanticModel(Compilation compilation, SyntaxNode root, Symbol memberSymbol, Binder rootBinder)
		: base(compilation)
	{
		DebugAssertHelper.Assert(compilation != null);
		DebugAssertHelper.Assert(root != null);
		DebugAssertHelper.Assert((object)memberSymbol != null);
		this.compilation = compilation;
		this.root = root;
		MemberSymbol = memberSymbol;
		RootBinder = rootBinder.WithAdditionalFlags(GetSemanticModelBinderFlags());
	}

	internal override IObjectTypeSymbol GetDeclaredSymbol(ObjectSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	internal override IMethodSymbol GetDeclaredSymbol(MethodOrTriggerDeclarationSyntax declarationSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return null;
	}

	public override ImmutableArray<Diagnostic> GetDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		throw new NotSupportedException();
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

	internal override MemberSemanticModel GetMemberModel(SyntaxNode node)
	{
		if (!IsInTree(node))
		{
			return null;
		}
		return this;
	}

	internal override SymbolInfo GetSymbolInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		GetBoundNodes(node, out var _, out var lowestBoundNode, out var highestBoundNode, out var boundParent);
		DebugAssertHelper.Assert(IsInTree(node), "Since the node is in the tree, we can always recompute the binder later");
		return GetSymbolInfoForNode(lowestBoundNode, highestBoundNode, boundParent, null);
	}

	internal override TypeInfo GetTypeInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		GetBoundNodes(node, out var _, out var lowestBoundNode, out var highestBoundNode, out var boundParent);
		return GetTypeInfoForNode(lowestBoundNode, highestBoundNode, boundParent);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "boundNodeForSyntacticParent")]
	internal static TypeInfo GetTypeInfoForNode(BoundNode lowestBoundNode, BoundNode highestBoundNode, BoundNode boundNodeForSyntacticParent)
	{
		BoundExpression boundExpression = lowestBoundNode as BoundExpression;
		BoundExpression boundExpression2 = highestBoundNode as BoundExpression;
		if (boundExpression != null)
		{
			TypeSymbol convertedType = null;
			TypeSymbol type = boundExpression.Type;
			if (boundExpression2 != null && boundExpression2 != boundExpression && boundExpression2.Type != null)
			{
				convertedType = boundExpression2.Type;
			}
			return new TypeInfo(type, convertedType);
		}
		return TypeInfo.None;
	}

	internal override Binder GetEnclosingBinderInternal(int position)
	{
		if (!Root.FullSpan.Contains(position))
		{
			return RootBinder;
		}
		SyntaxNode parent = Root.FindToken(position).Parent;
		return GetEnclosingBinder(parent, position);
	}

	protected virtual void GetBoundNodes(SyntaxNode node, out SyntaxNode bindableNode, out BoundNode lowestBoundNode, out BoundNode highestBoundNode, out BoundNode boundParent)
	{
		bindableNode = GetBindableSyntaxNode(node);
		SyntaxNode bindableParentNode = GetBindableParentNode(bindableNode);
		if (bindableParentNode != null && bindableParentNode.Kind == SyntaxKind.MemberAccessExpression && ((MemberAccessExpressionSyntax)bindableParentNode).Expression == bindableNode)
		{
			bindableParentNode = GetBindableParentNode(bindableParentNode);
		}
		boundParent = ((bindableParentNode == null) ? null : GetLowerBoundNode(bindableParentNode));
		lowestBoundNode = GetLowerBoundNode(bindableNode);
		highestBoundNode = GetUpperBoundNode(bindableNode);
	}

	protected virtual SyntaxNode GetBindableParentNode(SyntaxNode node)
	{
		if (!(node is ExpressionSyntax))
		{
			return null;
		}
		SyntaxNode syntaxNode = node.Parent;
		if (syntaxNode == null)
		{
			throw new ArgumentException("parent");
		}
		while (syntaxNode.Kind == SyntaxKind.ParenthesizedExpression)
		{
			SyntaxNode parent = syntaxNode.Parent;
			if (parent == null)
			{
				break;
			}
			syntaxNode = parent;
		}
		if (TryGetWithStatementParent(syntaxNode, out var withStatement))
		{
			return withStatement;
		}
		SyntaxNode syntaxNode2 = GetBindableSyntaxNode(syntaxNode);
		DebugAssertHelper.Assert(syntaxNode2 != null);
		if (syntaxNode2.Kind == SyntaxKind.MemberAccessExpression && syntaxNode2.Parent.Kind == SyntaxKind.InvocationExpression)
		{
			syntaxNode2 = syntaxNode2.Parent;
		}
		return syntaxNode2;
	}

	internal static bool TryGetWithStatementParent(SyntaxNode node, out SyntaxNode withStatement)
	{
		withStatement = null;
		while (node.Kind != SyntaxKind.MethodBody)
		{
			if (node.Kind == SyntaxKind.WithStatement)
			{
				withStatement = node;
			}
			SyntaxNode parent = node.Parent;
			if (parent == null)
			{
				break;
			}
			node = parent;
		}
		return withStatement != null;
	}

	internal virtual BoundNode GetBoundRoot()
	{
		return GetUpperBoundNode(GetBindableSyntaxNode(Root));
	}

	internal BoundNode GetUpperBoundNode(SyntaxNode node, bool promoteToBindable = false)
	{
		if (promoteToBindable)
		{
			node = GetBindableSyntaxNode(node);
		}
		else
		{
			DebugAssertHelper.Assert(node == GetBindableSyntaxNode(node));
		}
		ImmutableArray<BoundNode> boundNodes = GetBoundNodes(node);
		if (boundNodes.Length == 0)
		{
			return null;
		}
		return boundNodes[0];
	}

	internal BoundNode GetLowerBoundNode(SyntaxNode node)
	{
		DebugAssertHelper.Assert(node == GetBindableSyntaxNode(node));
		ImmutableArray<BoundNode> boundNodes = GetBoundNodes(node);
		if (boundNodes.Length == 0)
		{
			return null;
		}
		return boundNodes[boundNodes.Length - 1];
	}

	internal virtual ImmutableArray<BoundNode> GetBoundNodes(SyntaxNode node)
	{
		if (node == null)
		{
			node = GetBindableSyntaxNode(Root);
		}
		DebugAssertHelper.Assert(node == GetBindableSyntaxNode(node));
		ImmutableArray<BoundNode> result;
		using (nodeMapLock.DisposableRead())
		{
			result = GuardedGetBoundNodesFromMap(node);
		}
		if (!result.IsDefaultOrEmpty)
		{
			return result;
		}
		SyntaxNode bindingRoot = GetBindingRoot(node);
		Binder enclosingBinder = GetEnclosingBinder(GetAdjustedNodePosition(bindingRoot));
		Binder binder = new IncrementalBinder(this, enclosingBinder);
		using (nodeMapLock.DisposableWrite())
		{
			BoundNode bound = Bind(binder, bindingRoot, ignoredDiagnostics);
			result = GuardedAddBoundTreeAndGetBoundNodeFromMap(node, bound);
		}
		if (!result.IsDefaultOrEmpty)
		{
			return result;
		}
		Binder enclosingBinder2 = GetEnclosingBinder(GetAdjustedNodePosition(node));
		using (nodeMapLock.DisposableRead())
		{
			result = GuardedGetBoundNodesFromMap(node);
		}
		if (result.IsDefaultOrEmpty)
		{
			using (nodeMapLock.DisposableWrite())
			{
				BoundNode bound2 = Bind(enclosingBinder2, node, ignoredDiagnostics);
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

	internal void UnguardedAddBoundTreeForStandaloneSyntax(SyntaxNode syntax, BoundNode bound)
	{
		using (nodeMapLock.DisposableWrite())
		{
			GuardedAddBoundTreeForStandaloneSyntax(syntax, bound);
		}
	}

	protected void GuardedAddBoundTreeForStandaloneSyntax(SyntaxNode syntax, BoundNode bound)
	{
		DebugAssertHelper.Assert(nodeMapLock.IsWriteLockHeld);
		bool flag = false;
		if (bound != null)
		{
			flag = guardedNodeMap.ContainsKey(bound.Syntax);
		}
		if (!flag)
		{
			if ((IsSpeculativeSemanticModel && syntax == root) || syntax is StatementSyntax)
			{
				NodeMapBuilder.AddToMap(bound, guardedNodeMap);
			}
			else
			{
				NodeMapBuilder.AddToMap(bound, guardedNodeMap, syntax);
			}
		}
	}

	protected ImmutableArray<BoundNode> GuardedAddBoundTreeAndGetBoundNodeFromMap(SyntaxNode syntax, BoundNode bound)
	{
		DebugAssertHelper.Assert(nodeMapLock.IsWriteLockHeld);
		bool flag = false;
		if (bound != null)
		{
			flag = guardedNodeMap.ContainsKey(bound.Syntax);
		}
		if (!flag)
		{
			NodeMapBuilder.AddToMap(bound, guardedNodeMap);
		}
		if (!guardedNodeMap.TryGetValue(syntax, out var value))
		{
			return default;
		}
		return value;
	}

	protected ImmutableArray<BoundNode> GuardedGetBoundNodesFromMap(SyntaxNode node)
	{
		DebugAssertHelper.Assert(nodeMapLock.IsWriteLockHeld || nodeMapLock.IsReadLockHeld);
		if (!guardedNodeMap.TryGetValue(node, out var value))
		{
			return default;
		}
		return value;
	}

	protected internal virtual SyntaxNode GetBindableSyntaxNode(SyntaxNode node)
	{
		SyntaxKind kind = node.Kind;
		if (kind == SyntaxKind.TriggerDeclaration || (uint)(kind - 238) <= 1u)
		{
			return ((MethodOrTriggerDeclarationSyntax)node).Body;
		}
		while (true)
		{
			kind = node.Kind;
			if (kind != SyntaxKind.ParenthesizedExpression)
			{
				break;
			}
			node = ((ParenthesizedExpressionSyntax)node).Expression;
		}
		if (node.Parent != null)
		{
			kind = node.Kind;
			if (kind == SyntaxKind.MemberAccessExpression || kind == SyntaxKind.IdentifierName)
			{
				SyntaxNode standaloneNode = SyntaxFactory.GetStandaloneNode(node);
				if (standaloneNode != node)
				{
					return GetBindableSyntaxNode(standaloneNode);
				}
			}
		}
		return node;
	}

	protected virtual SyntaxNode GetBindingRoot(SyntaxNode node)
	{
		DebugAssertHelper.Assert(node != null);
		StatementSyntax statementSyntax = null;
		for (SyntaxNode syntaxNode = node; syntaxNode != Root; syntaxNode = syntaxNode.ParentOrStructuredTriviaParent)
		{
			DebugAssertHelper.Assert(syntaxNode != null, "How did we get outside the root?");
			if (statementSyntax == null)
			{
				statementSyntax = syntaxNode as StatementSyntax;
			}
		}
		return statementSyntax ?? Root;
	}

	private Binder GetEnclosingBinder(SyntaxNode node, int position)
	{
		if (node == root)
		{
			return RootBinder;
		}
		Binder binder = null;
		SyntaxNode syntaxNode = node;
		while (binder == null)
		{
			DebugAssertHelper.Assert(syntaxNode != null);
			if (syntaxNode is StatementSyntax statement)
			{
				if (LookupPosition.IsInStatementScope(position, statement))
				{
					binder = RootBinder.GetBinder(syntaxNode);
				}
			}
			else
			{
				DebugAssertHelper.Assert(!syntaxNode.CanHaveAssociatedLocalBinder());
			}
			if (syntaxNode == root)
			{
				break;
			}
			syntaxNode = syntaxNode.ParentOrStructuredTriviaParent;
		}
		binder = binder ?? RootBinder;
		DebugAssertHelper.Assert(binder != null);
		return binder.WithAdditionalFlags(GetSemanticModelBinderFlags());
	}

	internal override IOperation GetOperationWorker(SyntaxNode node, GetOperationOptions options, CancellationToken cancellationToken)
	{
		GetBoundNodes(node, out var _, out var lowestBoundNode, out var highestBoundNode, out var boundParent);
		return (options switch
		{
			GetOperationOptions.Parent => boundParent, 
			GetOperationOptions.Highest => highestBoundNode, 
			_ => lowestBoundNode, 
		}) as IOperation;
	}
}
