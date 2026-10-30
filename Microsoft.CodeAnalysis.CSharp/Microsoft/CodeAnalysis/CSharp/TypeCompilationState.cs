using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class TypeCompilationState
{
	internal struct MethodWithBody
	{
		public readonly MethodSymbol Method;

		public readonly BoundStatement Body;

		public readonly ImportChain ImportChainOpt;

		internal MethodWithBody(MethodSymbol method, BoundStatement body, ImportChain importChainOpt)
		{
			Method = method;
			Body = body;
			ImportChainOpt = importChainOpt;
		}
	}

	private ArrayBuilder<MethodWithBody> _synthesizedMethods;

	private Dictionary<MethodSymbol, MethodSymbol> _wrappers;

	private readonly NamedTypeSymbol _typeOpt;

	public readonly PEModuleBuilder ModuleBuilderOpt;

	public ImportChain CurrentImportChain;

	public readonly CSharpCompilation Compilation;

	public SynthesizedClosureEnvironment StaticLambdaFrame;

	private SmallDictionary<MethodSymbol, MethodSymbol> _constructorInitializers;

	public NamedTypeSymbol Type => _typeOpt;

	public NamedTypeSymbol DynamicOperationContextType => ModuleBuilderOpt?.GetDynamicOperationContextType(Type);

	public bool Emitting => ModuleBuilderOpt != null;

	public ArrayBuilder<MethodWithBody> SynthesizedMethods
	{
		get
		{
			return _synthesizedMethods;
		}
		set
		{
			_synthesizedMethods = value;
		}
	}

	public int NextWrapperMethodIndex
	{
		get
		{
			if (_wrappers != null)
			{
				return _wrappers.Count;
			}
			return 0;
		}
	}

	public TypeCompilationState(NamedTypeSymbol typeOpt, CSharpCompilation compilation, PEModuleBuilder moduleBuilderOpt)
	{
		Compilation = compilation;
		_typeOpt = typeOpt;
		ModuleBuilderOpt = moduleBuilderOpt;
	}

	public void AddSynthesizedMethod(MethodSymbol method, BoundStatement body)
	{
		if (_synthesizedMethods == null)
		{
			_synthesizedMethods = ArrayBuilder<MethodWithBody>.GetInstance();
		}
		_synthesizedMethods.Add(new MethodWithBody(method, body, CurrentImportChain));
	}

	public void AddMethodWrapper(MethodSymbol method, MethodSymbol wrapper, BoundStatement body)
	{
		AddSynthesizedMethod(wrapper, body);
		if (_wrappers == null)
		{
			_wrappers = new Dictionary<MethodSymbol, MethodSymbol>();
		}
		_wrappers.Add(method, wrapper);
	}

	public MethodSymbol GetMethodWrapper(MethodSymbol method)
	{
		MethodSymbol value = null;
		if (_wrappers == null || !_wrappers.TryGetValue(method, out value))
		{
			return null;
		}
		return value;
	}

	public void Free()
	{
		if (_synthesizedMethods != null)
		{
			_synthesizedMethods.Free();
			_synthesizedMethods = null;
		}
		_wrappers = null;
		_constructorInitializers = null;
	}

	internal void ReportCtorInitializerCycles(MethodSymbol method1, MethodSymbol method2, SyntaxNode syntax, DiagnosticBag diagnostics)
	{
		if (method1 == method2)
		{
			throw ExceptionUtilities.Unreachable;
		}
		if (_constructorInitializers == null)
		{
			_constructorInitializers = new SmallDictionary<MethodSymbol, MethodSymbol>();
			_constructorInitializers.Add(method1, method2);
			return;
		}
		MethodSymbol value = method2;
		while (_constructorInitializers.TryGetValue(value, out value))
		{
			if (method1 == value)
			{
				diagnostics.Add(ErrorCode.ERR_IndirectRecursiveConstructorCall, syntax.Location, method1);
				return;
			}
		}
		_constructorInitializers.Add(method1, method2);
	}
}
