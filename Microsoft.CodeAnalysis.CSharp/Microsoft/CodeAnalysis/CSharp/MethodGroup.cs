using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MethodGroup
{
	public static readonly ObjectPool<MethodGroup> Pool = CreatePool();

	internal BoundExpression Receiver { get; private set; }

	internal ArrayBuilder<MethodSymbol> Methods { get; }

	internal ArrayBuilder<TypeSymbol> TypeArguments { get; }

	internal bool IsExtensionMethodGroup { get; private set; }

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
			if (Receiver.Kind == BoundKind.TypeExpression)
			{
				return null;
			}
			return Receiver;
		}
	}

	private MethodGroup()
	{
		Methods = new ArrayBuilder<MethodSymbol>();
		TypeArguments = new ArrayBuilder<TypeSymbol>();
	}

	internal void PopulateWithSingleMethod(BoundExpression receiverOpt, MethodSymbol method, LookupResultKind resultKind = LookupResultKind.Viable, DiagnosticInfo error = null)
	{
		PopulateHelper(receiverOpt, resultKind, error);
		Methods.Add(method);
	}

	internal void PopulateWithExtensionMethods(BoundExpression receiverOpt, ArrayBuilder<Symbol> members, ImmutableArray<TypeSymbol> typeArguments, LookupResultKind resultKind = LookupResultKind.Viable, DiagnosticInfo error = null)
	{
		PopulateHelper(receiverOpt, resultKind, error);
		IsExtensionMethodGroup = true;
		foreach (Symbol member in members)
		{
			Methods.Add((MethodSymbol)member);
		}
		if (!typeArguments.IsDefault)
		{
			TypeArguments.AddRange(typeArguments);
		}
	}

	internal void PopulateWithNonExtensionMethods(BoundExpression receiverOpt, ImmutableArray<MethodSymbol> methods, ImmutableArray<TypeSymbol> typeArguments, LookupResultKind resultKind = LookupResultKind.Viable, DiagnosticInfo error = null)
	{
		PopulateHelper(receiverOpt, resultKind, error);
		Methods.AddRange(methods);
		if (!typeArguments.IsDefault)
		{
			TypeArguments.AddRange(typeArguments);
		}
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
		TypeArguments.Clear();
		IsExtensionMethodGroup = false;
		Error = null;
		ResultKind = LookupResultKind.Empty;
	}

	[Conditional("DEBUG")]
	private void VerifyClear()
	{
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
