using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class LookupResult
{
	private LookupResultKind kind;

	private readonly ArrayBuilder<Symbol> symbolList;

	private readonly Dictionary<Symbol, BoundExpression> receivers;

	private DiagnosticInfo error;

	private readonly ObjectPool<LookupResult> pool;

	private static readonly ObjectPool<LookupResult> poolInstance = CreatePool();

	internal bool IsClear
	{
		get
		{
			if (kind == LookupResultKind.Empty && error == null && symbolList.Count == 0)
			{
				return receivers.Count == 0;
			}
			return false;
		}
	}

	internal LookupResultKind Kind => kind;

	internal Symbol SingleSymbolOrDefault
	{
		get
		{
			if (symbolList.Count != 1)
			{
				return null;
			}
			return symbolList[0];
		}
	}

	internal ArrayBuilder<Symbol> Symbols => symbolList;

	internal DiagnosticInfo Error => error;

	internal bool IsMultiViable => Kind == LookupResultKind.Viable;

	internal bool IsSingleViable
	{
		get
		{
			if (Kind == LookupResultKind.Viable)
			{
				return symbolList.Count == 1;
			}
			return false;
		}
	}

	private LookupResult(ObjectPool<LookupResult> pool)
	{
		this.pool = pool;
		kind = LookupResultKind.Empty;
		symbolList = new ArrayBuilder<Symbol>();
		receivers = new Dictionary<Symbol, BoundExpression>();
		error = null;
	}

	internal void Clear()
	{
		kind = LookupResultKind.Empty;
		symbolList.Clear();
		receivers.Clear();
		error = null;
	}

	internal BoundExpression GetReceiver(Symbol symbol)
	{
		BoundExpression value = null;
		receivers.TryGetValue(symbol, out value);
		return value;
	}

	internal static SingleLookupResult Good(Symbol symbol, BoundExpression receiver)
	{
		return new SingleLookupResult(LookupResultKind.Viable, symbol, null, receiver);
	}

	internal static SingleLookupResult NotReferencable(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.NotReferencable, symbol, error);
	}

	internal static SingleLookupResult Inaccessible(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.Inaccessible, symbol, error);
	}

	internal static SingleLookupResult Ambiguous(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.Ambiguous, symbol, error);
	}

	internal static SingleLookupResult NotInvocable(Symbol symbol, bool diagnose)
	{
		NavDiagnosticInfo navDiagnosticInfo = (diagnose ? new NavDiagnosticInfo(ErrorCode.ERR_NonInvocableMemberCalled, symbol) : null);
		return new SingleLookupResult(LookupResultKind.NotInvocable, symbol, navDiagnosticInfo);
	}

	internal static SingleLookupResult NotOptionMembers(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.NotAnOption, symbol, error);
	}

	internal static SingleLookupResult NotPageElement(Symbol symbol, bool diagnose)
	{
		NavDiagnosticInfo navDiagnosticInfo = (diagnose ? new NavDiagnosticInfo(ErrorCode.ERR_NonInvocableMemberCalled, symbol) : null);
		return new SingleLookupResult(LookupResultKind.NotAPageElement, symbol, navDiagnosticInfo);
	}

	internal static SingleLookupResult NotAnAttributeType(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.NotAnAttributeType, symbol, error);
	}

	internal static SingleLookupResult StaticInstanceMismatch(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.StaticInstanceMismatch, symbol, error);
	}

	internal void SetFrom(SingleLookupResult other)
	{
		kind = other.Kind;
		symbolList.Clear();
		symbolList.Add(other.Symbol);
		receivers.Clear();
		if (other.Receiver != null)
		{
			receivers.Add(other.Symbol, other.Receiver);
		}
		error = other.Error;
	}

	internal void SetFrom(LookupResult other)
	{
		kind = other.kind;
		symbolList.Clear();
		symbolList.AddRange(other.symbolList);
		receivers.Clear();
		foreach (KeyValuePair<Symbol, BoundExpression> receiver in other.receivers)
		{
			receivers.Add(receiver.Key, receiver.Value);
		}
		error = other.error;
	}

	internal void SetFrom(DiagnosticInfo fromError)
	{
		Clear();
		error = fromError;
	}

	internal void MergePrioritized(LookupResult other)
	{
		if ((int)other.Kind > (int)Kind)
		{
			SetFrom(other);
		}
	}

	internal void MergeEqual(LookupResult other)
	{
		_ = Kind;
		_ = other.Kind;
		if ((int)other.Kind > (int)Kind)
		{
			SetFrom(other);
		}
		else
		{
			if (Kind != LookupResultKind.Viable)
			{
				return;
			}
			symbolList.AddRange(other.symbolList);
			foreach (KeyValuePair<Symbol, BoundExpression> receiver in other.receivers)
			{
				receivers.Add(receiver.Key, receiver.Value);
			}
		}
	}

	internal void MergeEqual(SingleLookupResult result)
	{
		if ((int)Kind > (int)result.Kind)
		{
			return;
		}
		if ((int)result.Kind > (int)Kind)
		{
			SetFrom(result);
		}
		else if ((object)result.Symbol != null)
		{
			symbolList.Add(result.Symbol);
			if (result.Receiver != null)
			{
				receivers.Add(result.Symbol, result.Receiver);
			}
		}
	}

	internal static ObjectPool<LookupResult> CreatePool()
	{
		ObjectPool<LookupResult> pool = null;
		pool = new ObjectPool<LookupResult>(() => new LookupResult(pool), 128);
		return pool;
	}

	internal static LookupResult GetInstance()
	{
		LookupResult lookupResult = poolInstance.Allocate();
		DebugAssertHelper.Assert(lookupResult.IsClear);
		return lookupResult;
	}

	internal void Free()
	{
		Clear();
		if (pool != null)
		{
			pool.Free(this);
		}
	}
}
