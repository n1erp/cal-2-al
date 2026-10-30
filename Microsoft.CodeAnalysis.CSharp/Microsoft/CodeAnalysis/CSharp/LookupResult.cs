using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LookupResult
{
	private LookupResultKind _kind;

	private readonly ArrayBuilder<Symbol> _symbolList;

	private DiagnosticInfo _error;

	private readonly ObjectPool<LookupResult> _pool;

	private static readonly ObjectPool<LookupResult> s_poolInstance = CreatePool();

	internal bool IsClear
	{
		get
		{
			if (_kind == LookupResultKind.Empty && _error == null)
			{
				return _symbolList.Count == 0;
			}
			return false;
		}
	}

	internal LookupResultKind Kind => _kind;

	internal Symbol SingleSymbolOrDefault
	{
		get
		{
			if (_symbolList.Count != 1)
			{
				return null;
			}
			return _symbolList[0];
		}
	}

	internal ArrayBuilder<Symbol> Symbols => _symbolList;

	internal DiagnosticInfo Error => _error;

	internal bool IsMultiViable => Kind == LookupResultKind.Viable;

	internal bool IsSingleViable
	{
		get
		{
			if (Kind == LookupResultKind.Viable)
			{
				return _symbolList.Count == 1;
			}
			return false;
		}
	}

	private LookupResult(ObjectPool<LookupResult> pool)
	{
		_pool = pool;
		_kind = LookupResultKind.Empty;
		_symbolList = new ArrayBuilder<Symbol>();
		_error = null;
	}

	internal void Clear()
	{
		_kind = LookupResultKind.Empty;
		_symbolList.Clear();
		_error = null;
	}

	internal static SingleLookupResult Good(Symbol symbol)
	{
		return new SingleLookupResult(LookupResultKind.Viable, symbol, null);
	}

	internal static SingleLookupResult WrongArity(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.WrongArity, symbol, error);
	}

	internal static SingleLookupResult Empty()
	{
		return new SingleLookupResult(LookupResultKind.Empty, null, null);
	}

	internal static SingleLookupResult NotReferencable(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.NotReferencable, symbol, error);
	}

	internal static SingleLookupResult StaticInstanceMismatch(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.StaticInstanceMismatch, symbol, error);
	}

	internal static SingleLookupResult Inaccessible(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.Inaccessible, symbol, error);
	}

	internal static SingleLookupResult NotInvocable(Symbol unwrappedSymbol, Symbol symbol, bool diagnose)
	{
		CSDiagnosticInfo error = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_NonInvocableMemberCalled, unwrappedSymbol) : null);
		return new SingleLookupResult(LookupResultKind.NotInvocable, symbol, error);
	}

	internal static SingleLookupResult NotLabel(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.NotLabel, symbol, error);
	}

	internal static SingleLookupResult NotTypeOrNamespace(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.NotATypeOrNamespace, symbol, error);
	}

	internal static SingleLookupResult NotTypeOrNamespace(Symbol unwrappedSymbol, Symbol symbol, bool diagnose)
	{
		CSDiagnosticInfo error = (diagnose ? new CSDiagnosticInfo(ErrorCode.ERR_BadSKknown, unwrappedSymbol.Name, unwrappedSymbol.GetKindText(), MessageID.IDS_SK_TYPE.Localize()) : null);
		return new SingleLookupResult(LookupResultKind.NotATypeOrNamespace, symbol, error);
	}

	internal static SingleLookupResult NotAnAttributeType(Symbol symbol, DiagnosticInfo error)
	{
		return new SingleLookupResult(LookupResultKind.NotAnAttributeType, symbol, error);
	}

	internal void SetFrom(SingleLookupResult other)
	{
		_kind = other.Kind;
		_symbolList.Clear();
		_symbolList.Add(other.Symbol);
		_error = other.Error;
	}

	internal void SetFrom(LookupResult other)
	{
		_kind = other._kind;
		_symbolList.Clear();
		_symbolList.AddRange(other._symbolList);
		_error = other._error;
	}

	internal void SetFrom(DiagnosticInfo error)
	{
		Clear();
		_error = error;
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
		if ((int)Kind <= (int)other.Kind)
		{
			if ((int)other.Kind > (int)Kind)
			{
				SetFrom(other);
			}
			else if (Kind == LookupResultKind.Viable)
			{
				_symbolList.AddRange(other._symbolList);
			}
		}
	}

	internal void MergeEqual(SingleLookupResult result)
	{
		if ((int)Kind <= (int)result.Kind)
		{
			if ((int)result.Kind > (int)Kind)
			{
				SetFrom(result);
			}
			else if ((object)result.Symbol != null)
			{
				_symbolList.Add(result.Symbol);
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
		return s_poolInstance.Allocate();
	}

	internal void Free()
	{
		Clear();
		if (_pool != null)
		{
			_pool.Free(this);
		}
	}
}
