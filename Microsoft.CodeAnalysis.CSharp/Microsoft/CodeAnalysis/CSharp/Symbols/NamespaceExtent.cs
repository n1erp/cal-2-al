using System;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal struct NamespaceExtent : IEquatable<NamespaceExtent>
{
	private readonly NamespaceKind _kind;

	private readonly object _symbolOrCompilation;

	public NamespaceKind Kind => _kind;

	public ModuleSymbol Module
	{
		get
		{
			if (_kind == NamespaceKind.Module)
			{
				return (ModuleSymbol)_symbolOrCompilation;
			}
			throw new InvalidOperationException();
		}
	}

	public AssemblySymbol Assembly
	{
		get
		{
			if (_kind == NamespaceKind.Assembly)
			{
				return (AssemblySymbol)_symbolOrCompilation;
			}
			throw new InvalidOperationException();
		}
	}

	public CSharpCompilation Compilation
	{
		get
		{
			if (_kind == NamespaceKind.Compilation)
			{
				return (CSharpCompilation)_symbolOrCompilation;
			}
			throw new InvalidOperationException();
		}
	}

	public override string ToString()
	{
		return $"{_kind}: {_symbolOrCompilation}";
	}

	internal NamespaceExtent(ModuleSymbol module)
	{
		_kind = NamespaceKind.Module;
		_symbolOrCompilation = module;
	}

	internal NamespaceExtent(AssemblySymbol assembly)
	{
		_kind = NamespaceKind.Assembly;
		_symbolOrCompilation = assembly;
	}

	internal NamespaceExtent(CSharpCompilation compilation)
	{
		_kind = NamespaceKind.Compilation;
		_symbolOrCompilation = compilation;
	}

	public override bool Equals(object obj)
	{
		if (obj is NamespaceExtent)
		{
			return Equals((NamespaceExtent)obj);
		}
		return false;
	}

	public bool Equals(NamespaceExtent other)
	{
		return object.Equals(_symbolOrCompilation, other._symbolOrCompilation);
	}

	public override int GetHashCode()
	{
		if (_symbolOrCompilation != null)
		{
			return _symbolOrCompilation.GetHashCode();
		}
		return 0;
	}
}
