using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class LocalSlotManager
{
	private struct LocalSignature : IEquatable<LocalSignature>
	{
		private readonly ITypeReference _type;

		private readonly LocalSlotConstraints _constraints;

		internal LocalSignature(ITypeReference valType, LocalSlotConstraints constraints)
		{
			_constraints = constraints;
			_type = valType;
		}

		public bool Equals(LocalSignature other)
		{
			if (_constraints == other._constraints)
			{
				if (_type != other._type)
				{
					return _type.Equals(other._type);
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.Combine(_type, (int)_constraints);
		}

		public override bool Equals(object obj)
		{
			return Equals((LocalSignature)obj);
		}
	}

	private Dictionary<ILocalSymbol, LocalDefinition> _localMap;

	private KeyedStack<LocalSignature, LocalDefinition> _freeSlots;

	private ArrayBuilder<ILocalDefinition> _lazyAllLocals;

	private readonly VariableSlotAllocator _slotAllocatorOpt;

	private Dictionary<ILocalSymbol, LocalDefinition> LocalMap
	{
		get
		{
			Dictionary<ILocalSymbol, LocalDefinition> dictionary = _localMap;
			if (dictionary == null)
			{
				dictionary = (_localMap = new Dictionary<ILocalSymbol, LocalDefinition>(Roslyn.Utilities.ReferenceEqualityComparer.Instance));
			}
			return dictionary;
		}
	}

	private KeyedStack<LocalSignature, LocalDefinition> FreeSlots
	{
		get
		{
			KeyedStack<LocalSignature, LocalDefinition> keyedStack = _freeSlots;
			if (keyedStack == null)
			{
				keyedStack = (_freeSlots = new KeyedStack<LocalSignature, LocalDefinition>());
			}
			return keyedStack;
		}
	}

	public LocalSlotManager(VariableSlotAllocator slotAllocatorOpt)
	{
		_slotAllocatorOpt = slotAllocatorOpt;
		if (slotAllocatorOpt != null)
		{
			_lazyAllLocals = new ArrayBuilder<ILocalDefinition>();
			slotAllocatorOpt.AddPreviousLocals(_lazyAllLocals);
		}
	}

	internal LocalDefinition DeclareLocal(ITypeReference type, ILocalSymbolInternal symbol, string name, SynthesizedLocalKind kind, LocalDebugId id, LocalVariableAttributes pdbAttributes, LocalSlotConstraints constraints, ImmutableArray<bool> dynamicTransformFlags, ImmutableArray<string> tupleElementNames, bool isSlotReusable)
	{
		if (!isSlotReusable || !FreeSlots.TryPop(new LocalSignature(type, constraints), out var value))
		{
			value = DeclareLocalImpl(type, symbol, name, kind, id, pdbAttributes, constraints, dynamicTransformFlags, tupleElementNames);
		}
		LocalMap.Add(symbol, value);
		return value;
	}

	internal LocalDefinition GetLocal(ILocalSymbol symbol)
	{
		return LocalMap[symbol];
	}

	internal void FreeLocal(ILocalSymbol symbol)
	{
		LocalDefinition local = GetLocal(symbol);
		LocalMap.Remove(symbol);
		FreeSlot(local);
	}

	internal LocalDefinition AllocateSlot(ITypeReference type, LocalSlotConstraints constraints, ImmutableArray<bool> dynamicTransformFlags = default(ImmutableArray<bool>), ImmutableArray<string> tupleElementNames = default(ImmutableArray<string>))
	{
		if (!FreeSlots.TryPop(new LocalSignature(type, constraints), out var value))
		{
			return DeclareLocalImpl(type, null, null, SynthesizedLocalKind.EmitterTemp, LocalDebugId.None, LocalVariableAttributes.DebuggerHidden, constraints, dynamicTransformFlags, tupleElementNames);
		}
		return value;
	}

	private LocalDefinition DeclareLocalImpl(ITypeReference type, ILocalSymbolInternal symbolOpt, string nameOpt, SynthesizedLocalKind kind, LocalDebugId id, LocalVariableAttributes pdbAttributes, LocalSlotConstraints constraints, ImmutableArray<bool> dynamicTransformFlags, ImmutableArray<string> tupleElementNames)
	{
		if (_lazyAllLocals == null)
		{
			_lazyAllLocals = new ArrayBuilder<ILocalDefinition>(1);
		}
		LocalDefinition previousLocal;
		if (symbolOpt != null && _slotAllocatorOpt != null)
		{
			previousLocal = _slotAllocatorOpt.GetPreviousLocal(type, symbolOpt, nameOpt, kind, id, pdbAttributes, constraints, dynamicTransformFlags, tupleElementNames);
			if (previousLocal != null)
			{
				int slotIndex = previousLocal.SlotIndex;
				_lazyAllLocals[slotIndex] = previousLocal;
				return previousLocal;
			}
		}
		previousLocal = new LocalDefinition(symbolOpt, nameOpt, type, _lazyAllLocals.Count, kind, id, pdbAttributes, constraints, dynamicTransformFlags, tupleElementNames);
		_lazyAllLocals.Add(previousLocal);
		return previousLocal;
	}

	internal void FreeSlot(LocalDefinition slot)
	{
		FreeSlots.Push(new LocalSignature(slot.Type, slot.Constraints), slot);
	}

	public ImmutableArray<ILocalDefinition> LocalsInOrder()
	{
		if (_lazyAllLocals == null)
		{
			return ImmutableArray<ILocalDefinition>.Empty;
		}
		return _lazyAllLocals.ToImmutable();
	}
}
