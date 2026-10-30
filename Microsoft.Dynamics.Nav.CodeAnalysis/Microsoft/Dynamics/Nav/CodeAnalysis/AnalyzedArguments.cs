using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class AnalyzedArguments
{
	public readonly ArrayBuilder<BoundExpression> Arguments;

	public readonly ArrayBuilder<IdentifierNameSyntax> Names;

	public readonly ArrayBuilder<DiagnosticBag> ArgumentDiagnostics;

	public static readonly ObjectPool<AnalyzedArguments> Pool = CreatePool();

	public bool HasErrors
	{
		get
		{
			foreach (BoundExpression argument in Arguments)
			{
				if (argument.HasAnyErrors)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal AnalyzedArguments()
	{
		Arguments = new ArrayBuilder<BoundExpression>(32);
		Names = new ArrayBuilder<IdentifierNameSyntax>(32);
		ArgumentDiagnostics = new ArrayBuilder<DiagnosticBag>(32);
	}

	public void Clear()
	{
		Arguments.Clear();
		Names.Clear();
		ArgumentDiagnostics.Clear();
	}

	public BoundExpression Argument(int i)
	{
		return Arguments[i];
	}

	public string Name(int i)
	{
		if (Names.Count == 0)
		{
			return null;
		}
		return Names[i]?.Identifier.ValueText;
	}

	public ImmutableArray<string> GetNames()
	{
		if (Names.Count == 0)
		{
			return default;
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance(Names.Count);
		for (int i = 0; i < Names.Count; i++)
		{
			instance.Add(Name(i));
		}
		return instance.ToImmutableAndFree();
	}

	public void PopulateDiagnostics(DiagnosticBag diagnostics)
	{
		foreach (DiagnosticBag argumentDiagnostic in ArgumentDiagnostics)
		{
			diagnostics.AddRangeAndFree(argumentDiagnostic);
		}
	}

	public static AnalyzedArguments GetInstance()
	{
		return Pool.Allocate();
	}

	public void Free()
	{
		Clear();
		Pool.Free(this);
	}

	private static ObjectPool<AnalyzedArguments> CreatePool()
	{
		return new ObjectPool<AnalyzedArguments>(() => new AnalyzedArguments(), 10);
	}
}
