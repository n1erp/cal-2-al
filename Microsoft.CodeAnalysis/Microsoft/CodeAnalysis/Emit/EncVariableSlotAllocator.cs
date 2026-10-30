using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit;

internal sealed class EncVariableSlotAllocator : VariableSlotAllocator
{
	private readonly SymbolMatcher _symbolMap;

	private readonly Func<SyntaxNode, SyntaxNode> _syntaxMapOpt;

	private readonly IMethodSymbolInternal _previousTopLevelMethod;

	private readonly DebugId _methodId;

	private readonly IReadOnlyDictionary<EncLocalInfo, int> _previousLocalSlots;

	private readonly ImmutableArray<EncLocalInfo> _previousLocals;

	private readonly string _stateMachineTypeNameOpt;

	private readonly int _hoistedLocalSlotCount;

	private readonly IReadOnlyDictionary<EncHoistedLocalInfo, int> _hoistedLocalSlotsOpt;

	private readonly int _awaiterCount;

	private readonly IReadOnlyDictionary<ITypeReference, int> _awaiterMapOpt;

	private readonly IReadOnlyDictionary<int, KeyValuePair<DebugId, int>> _lambdaMapOpt;

	private readonly IReadOnlyDictionary<int, DebugId> _closureMapOpt;

	private readonly LambdaSyntaxFacts _lambdaSyntaxFacts;

	public override DebugId? MethodId => _methodId;

	public override string PreviousStateMachineTypeName => _stateMachineTypeNameOpt;

	public override int PreviousHoistedLocalSlotCount => _hoistedLocalSlotCount;

	public override int PreviousAwaiterSlotCount => _awaiterCount;

	public EncVariableSlotAllocator(SymbolMatcher symbolMap, Func<SyntaxNode, SyntaxNode> syntaxMapOpt, IMethodSymbolInternal previousTopLevelMethod, DebugId methodId, ImmutableArray<EncLocalInfo> previousLocals, IReadOnlyDictionary<int, KeyValuePair<DebugId, int>> lambdaMapOpt, IReadOnlyDictionary<int, DebugId> closureMapOpt, string stateMachineTypeNameOpt, int hoistedLocalSlotCount, IReadOnlyDictionary<EncHoistedLocalInfo, int> hoistedLocalSlotsOpt, int awaiterCount, IReadOnlyDictionary<ITypeReference, int> awaiterMapOpt, LambdaSyntaxFacts lambdaSyntaxFacts)
	{
		_symbolMap = symbolMap;
		_syntaxMapOpt = syntaxMapOpt;
		_previousLocals = previousLocals;
		_previousTopLevelMethod = previousTopLevelMethod;
		_methodId = methodId;
		_hoistedLocalSlotsOpt = hoistedLocalSlotsOpt;
		_hoistedLocalSlotCount = hoistedLocalSlotCount;
		_stateMachineTypeNameOpt = stateMachineTypeNameOpt;
		_awaiterCount = awaiterCount;
		_awaiterMapOpt = awaiterMapOpt;
		_lambdaMapOpt = lambdaMapOpt;
		_closureMapOpt = closureMapOpt;
		_lambdaSyntaxFacts = lambdaSyntaxFacts;
		Dictionary<EncLocalInfo, int> dictionary = new Dictionary<EncLocalInfo, int>();
		for (int i = 0; i < previousLocals.Length; i++)
		{
			EncLocalInfo key = previousLocals[i];
			if (!key.IsUnused)
			{
				dictionary.Add(key, i);
			}
		}
		_previousLocalSlots = dictionary;
	}

	private int CalculateSyntaxOffsetInPreviousMethod(int position, SyntaxTree tree)
	{
		return _previousTopLevelMethod.CalculateLocalSyntaxOffset(position, tree);
	}

	public override void AddPreviousLocals(ArrayBuilder<ILocalDefinition> builder)
	{
		builder.AddRange(_previousLocals.Select((EncLocalInfo info, int index) => new SignatureOnlyLocalDefinition(info.Signature, index)));
	}

	private bool TryGetPreviousLocalId(SyntaxNode currentDeclarator, LocalDebugId currentId, out LocalDebugId previousId)
	{
		if (_syntaxMapOpt == null)
		{
			previousId = currentId;
			return true;
		}
		SyntaxNode syntaxNode = _syntaxMapOpt(currentDeclarator);
		if (syntaxNode == null)
		{
			previousId = default;
			return false;
		}
		int syntaxOffset = CalculateSyntaxOffsetInPreviousMethod(syntaxNode.SpanStart, syntaxNode.SyntaxTree);
		previousId = new LocalDebugId(syntaxOffset, currentId.Ordinal);
		return true;
	}

	public override LocalDefinition GetPreviousLocal(ITypeReference currentType, ILocalSymbolInternal currentLocalSymbol, string nameOpt, SynthesizedLocalKind kind, LocalDebugId id, LocalVariableAttributes pdbAttributes, LocalSlotConstraints constraints, ImmutableArray<bool> dynamicTransformFlags, ImmutableArray<string> tupleElementNames)
	{
		if (id.IsNone)
		{
			return null;
		}
		if (!TryGetPreviousLocalId(currentLocalSymbol.GetDeclaratorSyntax(), id, out var previousId))
		{
			return null;
		}
		ITypeReference typeReference = _symbolMap.MapReference(currentType);
		if (typeReference == null)
		{
			return null;
		}
		EncLocalInfo key = new EncLocalInfo(new LocalSlotDebugInfo(kind, previousId), typeReference, constraints, null);
		if (!_previousLocalSlots.TryGetValue(key, out var value))
		{
			return null;
		}
		return new LocalDefinition(currentLocalSymbol, nameOpt, currentType, value, kind, id, pdbAttributes, constraints, dynamicTransformFlags, tupleElementNames);
	}

	public override bool TryGetPreviousHoistedLocalSlotIndex(SyntaxNode currentDeclarator, ITypeReference currentType, SynthesizedLocalKind synthesizedKind, LocalDebugId currentId, DiagnosticBag diagnostics, out int slotIndex)
	{
		if (_hoistedLocalSlotsOpt == null)
		{
			slotIndex = -1;
			return false;
		}
		if (!TryGetPreviousLocalId(currentDeclarator, currentId, out var previousId))
		{
			slotIndex = -1;
			return false;
		}
		ITypeReference typeReference = _symbolMap.MapReference(currentType);
		if (typeReference == null)
		{
			slotIndex = -1;
			return false;
		}
		EncHoistedLocalInfo key = new EncHoistedLocalInfo(new LocalSlotDebugInfo(synthesizedKind, previousId), typeReference);
		return _hoistedLocalSlotsOpt.TryGetValue(key, out slotIndex);
	}

	public override bool TryGetPreviousAwaiterSlotIndex(ITypeReference currentType, DiagnosticBag diagnostics, out int slotIndex)
	{
		if (_awaiterMapOpt == null)
		{
			slotIndex = -1;
			return false;
		}
		return _awaiterMapOpt.TryGetValue(_symbolMap.MapReference(currentType), out slotIndex);
	}

	private bool TryGetPreviousSyntaxOffset(SyntaxNode currentSyntax, out int previousSyntaxOffset)
	{
		SyntaxNode syntaxNode = _syntaxMapOpt?.Invoke(currentSyntax);
		if (syntaxNode == null)
		{
			previousSyntaxOffset = 0;
			return false;
		}
		previousSyntaxOffset = CalculateSyntaxOffsetInPreviousMethod(syntaxNode.SpanStart, syntaxNode.SyntaxTree);
		return true;
	}

	private bool TryGetPreviousLambdaSyntaxOffset(SyntaxNode lambdaOrLambdaBodySyntax, bool isLambdaBody, out int previousSyntaxOffset)
	{
		SyntaxNode arg = (isLambdaBody ? _lambdaSyntaxFacts.GetLambda(lambdaOrLambdaBodySyntax) : lambdaOrLambdaBodySyntax);
		SyntaxNode syntaxNode = _syntaxMapOpt?.Invoke(arg);
		if (syntaxNode == null)
		{
			previousSyntaxOffset = 0;
			return false;
		}
		SyntaxNode syntaxNode2;
		if (isLambdaBody)
		{
			syntaxNode2 = _lambdaSyntaxFacts.TryGetCorrespondingLambdaBody(syntaxNode, lambdaOrLambdaBodySyntax);
			if (syntaxNode2 == null)
			{
				previousSyntaxOffset = 0;
				return false;
			}
		}
		else
		{
			syntaxNode2 = syntaxNode;
		}
		previousSyntaxOffset = CalculateSyntaxOffsetInPreviousMethod(syntaxNode2.SpanStart, syntaxNode2.SyntaxTree);
		return true;
	}

	public override bool TryGetPreviousClosure(SyntaxNode scopeSyntax, out DebugId closureId)
	{
		if (_closureMapOpt != null && TryGetPreviousSyntaxOffset(scopeSyntax, out var previousSyntaxOffset) && _closureMapOpt.TryGetValue(previousSyntaxOffset, out closureId))
		{
			return true;
		}
		closureId = default;
		return false;
	}

	public override bool TryGetPreviousLambda(SyntaxNode lambdaOrLambdaBodySyntax, bool isLambdaBody, out DebugId lambdaId)
	{
		if (_lambdaMapOpt != null && TryGetPreviousLambdaSyntaxOffset(lambdaOrLambdaBodySyntax, isLambdaBody, out var previousSyntaxOffset) && _lambdaMapOpt.TryGetValue(previousSyntaxOffset, out var value))
		{
			lambdaId = value.Key;
			return true;
		}
		lambdaId = default;
		return false;
	}
}
