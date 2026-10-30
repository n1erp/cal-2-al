using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.FlowAnalysis;

public sealed class ControlFlowGraph
{
	private readonly ControlFlowGraphBuilder.CaptureIdDispenser _captureIdDispenser;

	private readonly ImmutableDictionary<IMethodSymbol, (ControlFlowRegion region, ILocalFunctionOperation operation, int ordinal)> _localFunctionsMap;

	private ControlFlowGraph[] _lazyLocalFunctionsGraphs;

	private readonly ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)> _anonymousFunctionsMap;

	private ControlFlowGraph[] _lazyAnonymousFunctionsGraphs;

	public IOperation OriginalOperation { get; }

	public ImmutableArray<BasicBlock> Blocks { get; }

	public ControlFlowRegion Root { get; }

	public ImmutableArray<IMethodSymbol> LocalFunctions { get; }

	internal ControlFlowGraph(IOperation originalOperation, ControlFlowGraphBuilder.CaptureIdDispenser captureIdDispenser, ImmutableArray<BasicBlock> blocks, ControlFlowRegion root, ImmutableArray<IMethodSymbol> localFunctions, ImmutableDictionary<IMethodSymbol, (ControlFlowRegion region, ILocalFunctionOperation operation, int ordinal)> localFunctionsMap, ImmutableDictionary<IFlowAnonymousFunctionOperation, (ControlFlowRegion region, int ordinal)> anonymousFunctionsMap)
	{
		OriginalOperation = originalOperation;
		Blocks = blocks;
		Root = root;
		LocalFunctions = localFunctions;
		_localFunctionsMap = localFunctionsMap;
		_anonymousFunctionsMap = anonymousFunctionsMap;
		_captureIdDispenser = captureIdDispenser;
	}

	public static ControlFlowGraph Create(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (semanticModel == null)
		{
			throw new ArgumentNullException("semanticModel");
		}
		IOperation operation = semanticModel.GetOperation(node, cancellationToken);
		cancellationToken.ThrowIfCancellationRequested();
		if (operation != null)
		{
			return CreateCore(operation, "operation", cancellationToken);
		}
		return null;
	}

	public static ControlFlowGraph Create(IBlockOperation body, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CreateCore(body, "body", cancellationToken);
	}

	public static ControlFlowGraph Create(IFieldInitializerOperation initializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CreateCore(initializer, "initializer", cancellationToken);
	}

	public static ControlFlowGraph Create(IPropertyInitializerOperation initializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CreateCore(initializer, "initializer", cancellationToken);
	}

	public static ControlFlowGraph Create(IParameterInitializerOperation initializer, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CreateCore(initializer, "initializer", cancellationToken);
	}

	public static ControlFlowGraph Create(IConstructorBodyOperation constructorBody, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CreateCore(constructorBody, "constructorBody", cancellationToken);
	}

	public static ControlFlowGraph Create(IMethodBodyOperation methodBody, CancellationToken cancellationToken = default(CancellationToken))
	{
		return CreateCore(methodBody, "methodBody", cancellationToken);
	}

	internal static ControlFlowGraph CreateCore(IOperation operation, string argumentNameForException, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (operation == null)
		{
			throw new ArgumentNullException(argumentNameForException);
		}
		if (operation.Parent != null)
		{
			throw new ArgumentException(CodeAnalysisResources.NotARootOperation, argumentNameForException);
		}
		if (((Operation)operation).OwningSemanticModel == null)
		{
			throw new ArgumentException(CodeAnalysisResources.OperationHasNullSemanticModel, argumentNameForException);
		}
		if (!operation.Syntax.SyntaxTree.Options.Features.ContainsKey("flow-analysis"))
		{
			throw new InvalidOperationException(CodeAnalysisResources.FlowAnalysisFeatureDisabled);
		}
		try
		{
			return ControlFlowGraphBuilder.Create(operation, null, null, default(ControlFlowGraphBuilder.Context));
		}
		catch (Exception exception) when (FatalError.ReportWithoutCrashUnlessCanceled(exception))
		{
		}
		return null;
	}

	public ControlFlowGraph GetLocalFunctionControlFlowGraph(IMethodSymbol localFunction, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (localFunction == null)
		{
			throw new ArgumentNullException("localFunction");
		}
		if (!_localFunctionsMap.TryGetValue(localFunction, out (ControlFlowRegion, ILocalFunctionOperation, int) value))
		{
			throw new ArgumentOutOfRangeException("localFunction");
		}
		if (_lazyLocalFunctionsGraphs == null)
		{
			Interlocked.CompareExchange(ref _lazyLocalFunctionsGraphs, new ControlFlowGraph[LocalFunctions.Length], null);
		}
		if (_lazyLocalFunctionsGraphs[value.Item3] == null)
		{
			ControlFlowGraph value2 = ControlFlowGraphBuilder.Create(value.Item2, value.Item1, _captureIdDispenser, default(ControlFlowGraphBuilder.Context));
			Interlocked.CompareExchange(ref _lazyLocalFunctionsGraphs[value.Item3], value2, null);
		}
		return _lazyLocalFunctionsGraphs[value.Item3];
	}

	public ControlFlowGraph GetAnonymousFunctionControlFlowGraph(IFlowAnonymousFunctionOperation anonymousFunction, CancellationToken cancellationToken = default(CancellationToken))
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (anonymousFunction == null)
		{
			throw new ArgumentNullException("anonymousFunction");
		}
		if (!_anonymousFunctionsMap.TryGetValue(anonymousFunction, out (ControlFlowRegion, int) value))
		{
			throw new ArgumentOutOfRangeException("anonymousFunction");
		}
		if (_lazyAnonymousFunctionsGraphs == null)
		{
			Interlocked.CompareExchange(ref _lazyAnonymousFunctionsGraphs, new ControlFlowGraph[_anonymousFunctionsMap.Count], null);
		}
		if (_lazyAnonymousFunctionsGraphs[value.Item2] == null)
		{
			FlowAnonymousFunctionOperation flowAnonymousFunctionOperation = (FlowAnonymousFunctionOperation)anonymousFunction;
			ControlFlowGraph value2 = ControlFlowGraphBuilder.Create(flowAnonymousFunctionOperation.Original, value.Item1, _captureIdDispenser, in flowAnonymousFunctionOperation.Context);
			Interlocked.CompareExchange(ref _lazyAnonymousFunctionsGraphs[value.Item2], value2, null);
		}
		return _lazyAnonymousFunctionsGraphs[value.Item2];
	}
}
