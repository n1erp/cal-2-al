using System.Collections.Immutable;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class MethodBody : IMethodBody
{
	private readonly IMethodDefinition _parent;

	private readonly ImmutableArray<byte> _ilBits;

	private readonly ushort _maxStack;

	private readonly ImmutableArray<ILocalDefinition> _locals;

	private readonly ImmutableArray<ExceptionHandlerRegion> _exceptionHandlers;

	private readonly ImmutableArray<SequencePoint> _sequencePoints;

	private readonly ImmutableArray<LocalScope> _localScopes;

	private readonly IImportScope _importScopeOpt;

	private readonly string _stateMachineTypeNameOpt;

	private readonly ImmutableArray<StateMachineHoistedLocalScope> _stateMachineHoistedLocalScopes;

	private readonly bool _hasDynamicLocalVariables;

	private readonly StateMachineMoveNextBodyDebugInfo _stateMachineMoveNextDebugInfoOpt;

	private readonly DebugId _methodId;

	private readonly ImmutableArray<EncHoistedLocalInfo> _stateMachineHoistedLocalSlots;

	private readonly ImmutableArray<LambdaDebugInfo> _lambdaDebugInfo;

	private readonly ImmutableArray<ClosureDebugInfo> _closureDebugInfo;

	private readonly ImmutableArray<ITypeReference> _stateMachineAwaiterSlots;

	private readonly DynamicAnalysisMethodBodyData _dynamicAnalysisDataOpt;

	DynamicAnalysisMethodBodyData IMethodBody.DynamicAnalysisData => _dynamicAnalysisDataOpt;

	ImmutableArray<ExceptionHandlerRegion> IMethodBody.ExceptionRegions => _exceptionHandlers;

	bool IMethodBody.LocalsAreZeroed => true;

	ImmutableArray<ILocalDefinition> IMethodBody.LocalVariables => _locals;

	IMethodDefinition IMethodBody.MethodDefinition => _parent;

	StateMachineMoveNextBodyDebugInfo IMethodBody.MoveNextBodyInfo => _stateMachineMoveNextDebugInfoOpt;

	ushort IMethodBody.MaxStack => _maxStack;

	public ImmutableArray<byte> IL => _ilBits;

	public ImmutableArray<SequencePoint> SequencePoints => _sequencePoints;

	ImmutableArray<LocalScope> IMethodBody.LocalScopes => _localScopes;

	IImportScope IMethodBody.ImportScope => _importScopeOpt;

	string IMethodBody.StateMachineTypeName => _stateMachineTypeNameOpt;

	ImmutableArray<StateMachineHoistedLocalScope> IMethodBody.StateMachineHoistedLocalScopes => _stateMachineHoistedLocalScopes;

	ImmutableArray<EncHoistedLocalInfo> IMethodBody.StateMachineHoistedLocalSlots => _stateMachineHoistedLocalSlots;

	ImmutableArray<ITypeReference> IMethodBody.StateMachineAwaiterSlots => _stateMachineAwaiterSlots;

	bool IMethodBody.HasDynamicLocalVariables => _hasDynamicLocalVariables;

	public DebugId MethodId => _methodId;

	public ImmutableArray<LambdaDebugInfo> LambdaDebugInfo => _lambdaDebugInfo;

	public ImmutableArray<ClosureDebugInfo> ClosureDebugInfo => _closureDebugInfo;

	public MethodBody(ImmutableArray<byte> ilBits, ushort maxStack, IMethodDefinition parent, DebugId methodId, ImmutableArray<ILocalDefinition> locals, SequencePointList sequencePoints, DebugDocumentProvider debugDocumentProvider, ImmutableArray<ExceptionHandlerRegion> exceptionHandlers, ImmutableArray<LocalScope> localScopes, bool hasDynamicLocalVariables, IImportScope importScopeOpt, ImmutableArray<LambdaDebugInfo> lambdaDebugInfo, ImmutableArray<ClosureDebugInfo> closureDebugInfo, string stateMachineTypeNameOpt, ImmutableArray<StateMachineHoistedLocalScope> stateMachineHoistedLocalScopes, ImmutableArray<EncHoistedLocalInfo> stateMachineHoistedLocalSlots, ImmutableArray<ITypeReference> stateMachineAwaiterSlots, StateMachineMoveNextBodyDebugInfo stateMachineMoveNextDebugInfoOpt, DynamicAnalysisMethodBodyData dynamicAnalysisDataOpt)
	{
		_ilBits = ilBits;
		_maxStack = maxStack;
		_parent = parent;
		_methodId = methodId;
		_locals = locals;
		_exceptionHandlers = exceptionHandlers;
		_localScopes = localScopes;
		_hasDynamicLocalVariables = hasDynamicLocalVariables;
		_importScopeOpt = importScopeOpt;
		_lambdaDebugInfo = lambdaDebugInfo;
		_closureDebugInfo = closureDebugInfo;
		_stateMachineTypeNameOpt = stateMachineTypeNameOpt;
		_stateMachineHoistedLocalScopes = stateMachineHoistedLocalScopes;
		_stateMachineHoistedLocalSlots = stateMachineHoistedLocalSlots;
		_stateMachineAwaiterSlots = stateMachineAwaiterSlots;
		_stateMachineMoveNextDebugInfoOpt = stateMachineMoveNextDebugInfoOpt;
		_dynamicAnalysisDataOpt = dynamicAnalysisDataOpt;
		_sequencePoints = GetSequencePoints(sequencePoints, debugDocumentProvider);
	}

	private static ImmutableArray<SequencePoint> GetSequencePoints(SequencePointList sequencePoints, DebugDocumentProvider debugDocumentProvider)
	{
		if (sequencePoints == null || sequencePoints.IsEmpty)
		{
			return ImmutableArray<SequencePoint>.Empty;
		}
		ArrayBuilder<SequencePoint> instance = ArrayBuilder<SequencePoint>.GetInstance();
		sequencePoints.GetSequencePoints(debugDocumentProvider, instance);
		return instance.ToImmutableAndFree();
	}
}
