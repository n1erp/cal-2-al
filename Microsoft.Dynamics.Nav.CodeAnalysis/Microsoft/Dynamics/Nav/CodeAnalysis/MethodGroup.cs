using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class MethodGroup
{
	public static readonly ObjectPool<MethodGroup> Pool = CreatePool();

	internal BoundExpression Receiver { get; private set; }

	internal ArrayBuilder<MethodSymbol> Methods { get; private set; }

	internal DiagnosticInfo Error { get; private set; }

	internal LookupResultKind ResultKind { get; private set; }

	public string Name
	{
		get
		{
			if (Methods.Count <= 0)
			{
				return null;
			}
			return Methods[0].Name;
		}
	}

	public BoundExpression InstanceOpt
	{
		get
		{
			if (Receiver == null)
			{
				return null;
			}
			return Receiver;
		}
	}

	private MethodGroup()
	{
		Methods = new ArrayBuilder<MethodSymbol>();
	}

	internal void PopulateWithSingleMethod(BoundExpression receiverOpt, MethodSymbol method, LookupResultKind resultKind = LookupResultKind.Viable, DiagnosticInfo error = null)
	{
		PopulateHelper(receiverOpt, resultKind, error);
		Methods.Add(method);
	}

	internal void PopulateWithNonExtensionMethods(BoundExpression receiverOpt, ImmutableArray<MethodSymbol> methods, LookupResultKind resultKind = LookupResultKind.Viable, DiagnosticInfo error = null)
	{
		PopulateHelper(receiverOpt, resultKind, error);
		Methods.AddRange(methods);
	}

	private void PopulateHelper(BoundExpression receiverOpt, LookupResultKind resultKind, DiagnosticInfo error)
	{
		Receiver = receiverOpt;
		Error = error;
		ResultKind = resultKind;
	}

	public void Clear()
	{
		Receiver = null;
		Methods.Clear();
		Error = null;
		ResultKind = LookupResultKind.Empty;
	}

	[Conditional("DEBUG")]
	private void VerifyClear()
	{
		DebugAssertHelper.Assert(Receiver == null);
		DebugAssertHelper.Assert(Methods.Count == 0);
		DebugAssertHelper.Assert(Error == null);
		DebugAssertHelper.Assert(ResultKind == LookupResultKind.Empty);
	}

	public static MethodGroup GetInstance()
	{
		return Pool.Allocate();
	}

	public void Free()
	{
		Clear();
		Pool.Free(this);
	}

	private static ObjectPool<MethodGroup> CreatePool()
	{
		return new ObjectPool<MethodGroup>(() => new MethodGroup(), 10);
	}
}
