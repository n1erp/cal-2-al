using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal abstract class Operation : IOperation
{
	private abstract class BaseNoneOperation : Operation
	{
		protected BaseNoneOperation(SemanticModel semanticModel, SyntaxNode syntax, Optional<object> constantValue, bool isImplicit)
			: base(OperationKind.None, semanticModel, syntax, null, constantValue, isImplicit)
		{
		}

		public override void Accept(OperationVisitor visitor)
		{
			visitor.VisitNoneOperation(this);
		}

		public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
		{
			return visitor.VisitNoneOperation(this, argument);
		}
	}

	private class NoneOperation : BaseNoneOperation
	{
		public override IEnumerable<IOperation> Children { get; }

		public NoneOperation(ImmutableArray<IOperation> children, SemanticModel semanticModel, SyntaxNode syntax, Optional<object> constantValue, bool isImplicit)
			: base(semanticModel, syntax, constantValue, isImplicit)
		{
			Children = SetParentOperation(children, this);
		}
	}

	private class LazyNoneOperation : BaseNoneOperation
	{
		private readonly Lazy<ImmutableArray<IOperation>> _lazyChildren;

		public override IEnumerable<IOperation> Children => SetParentOperation(_lazyChildren.Value, this);

		public LazyNoneOperation(Func<ImmutableArray<IOperation>> getChildren, SemanticModel semanticModel, SyntaxNode node, Optional<object> constantValue, bool isImplicit)
			: base(semanticModel, node, constantValue, isImplicit)
		{
			_lazyChildren = new Lazy<ImmutableArray<IOperation>>(getChildren);
		}
	}

	private static readonly IOperation s_unset = new EmptyStatement(null, null, null, default, isImplicit: true);

	private readonly SemanticModel _owningSemanticModelOpt;

	private IOperation _parentDoNotAccessDirectly;

	private static readonly ObjectPool<Queue<IOperation>> s_queuePool = new ObjectPool<Queue<IOperation>>(() => new Queue<IOperation>(), 10);

	public IOperation Parent
	{
		get
		{
			if (_parentDoNotAccessDirectly == s_unset)
			{
				SetParentOperation(SearchParentOperation());
			}
			return _parentDoNotAccessDirectly;
		}
	}

	public bool IsImplicit { get; }

	public OperationKind Kind { get; }

	public SyntaxNode Syntax { get; }

	public ITypeSymbol Type { get; }

	public string Language => Syntax.Language;

	public Optional<object> ConstantValue { get; }

	public abstract IEnumerable<IOperation> Children { get; }

	SemanticModel IOperation.SemanticModel => _owningSemanticModelOpt?.ContainingModelOrSelf;

	internal SemanticModel OwningSemanticModel => _owningSemanticModelOpt;

	protected Operation(OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
	{
		_owningSemanticModelOpt = semanticModel;
		Kind = kind;
		Syntax = syntax;
		Type = type;
		ConstantValue = constantValue;
		IsImplicit = isImplicit;
		_parentDoNotAccessDirectly = s_unset;
	}

	public abstract void Accept(OperationVisitor visitor);

	public abstract TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument);

	protected void SetParentOperation(IOperation parent)
	{
		Interlocked.CompareExchange(ref _parentDoNotAccessDirectly, parent, s_unset);
	}

	public static IOperation CreateOperationNone(SemanticModel semanticModel, SyntaxNode node, Optional<object> constantValue, Func<ImmutableArray<IOperation>> getChildren, bool isImplicit)
	{
		return new LazyNoneOperation(getChildren, semanticModel, node, constantValue, isImplicit);
	}

	public static IOperation CreateOperationNone(SemanticModel semanticModel, SyntaxNode node, Optional<object> constantValue, ImmutableArray<IOperation> children, bool isImplicit)
	{
		return new NoneOperation(children, semanticModel, node, constantValue, isImplicit);
	}

	public static T SetParentOperation<T>(T operation, IOperation parent) where T : IOperation
	{
		(operation as Operation)?.SetParentOperation(parent);
		return operation;
	}

	public static ImmutableArray<T> SetParentOperation<T>(ImmutableArray<T> operations, IOperation parent) where T : IOperation
	{
		if (operations.Length == 0)
		{
			return operations;
		}
		if ((operations[0] as Operation)?._parentDoNotAccessDirectly != s_unset)
		{
			return operations;
		}
		foreach (T item in operations)
		{
			SetParentOperation(item, parent);
		}
		return operations;
	}

	private IOperation WalkDownOperationToFindParent(HashSet<IOperation> operationAlreadyProcessed, IOperation root)
	{
		Queue<IOperation> queue = s_queuePool.Allocate();
		try
		{
			EnqueueChildOperations(queue, root);
			while (queue.Count > 0)
			{
				IOperation operation = queue.Dequeue();
				if (operationAlreadyProcessed.Add(operation))
				{
					if (operation == this)
					{
						return operation.Parent;
					}
					EnqueueChildOperations(queue, operation);
				}
			}
			return null;
		}
		finally
		{
			queue.Clear();
			s_queuePool.Free(queue);
		}
		static void EnqueueChildOperations(Queue<IOperation> queue2, IOperation parent)
		{
			foreach (IOperation item in parent.Children.WhereNotNull())
			{
				queue2.Enqueue(item);
			}
		}
	}

	internal IOperation SearchParentOperation()
	{
		PooledHashSet<IOperation> instance = PooledHashSet<IOperation>.GetInstance();
		if (OwningSemanticModel.Root == Syntax)
		{
			return null;
		}
		SyntaxNode parent = Syntax.Parent;
		try
		{
			while (parent != null)
			{
				IOperation operation = OwningSemanticModel.GetOperation(parent);
				if (operation != null)
				{
					IOperation operation2 = WalkDownOperationToFindParent(instance, operation);
					if (operation2 != null)
					{
						return operation2;
					}
				}
				if (OwningSemanticModel.Root == parent)
				{
					break;
				}
				parent = parent.Parent;
			}
			return null;
		}
		finally
		{
			instance.Free();
		}
	}
}
