using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ExecutableCodeBinder : Binder
{
	private readonly Symbol _memberSymbol;

	private readonly SyntaxNode _root;

	private readonly Action<Binder, SyntaxNode> _binderUpdatedHandler;

	private SmallDictionary<SyntaxNode, Binder> _lazyBinderMap;

	private ImmutableArray<MethodSymbol> _methodSymbolsWithYield;

	internal override Symbol ContainingMemberOrLambda => _memberSymbol ?? Next.ContainingMemberOrLambda;

	internal Symbol MemberSymbol => _memberSymbol;

	private SmallDictionary<SyntaxNode, Binder> BinderMap
	{
		get
		{
			if (_lazyBinderMap == null)
			{
				ComputeBinderMap();
			}
			return _lazyBinderMap;
		}
	}

	private ImmutableArray<MethodSymbol> MethodSymbolsWithYield
	{
		get
		{
			if (_methodSymbolsWithYield.IsDefault)
			{
				ComputeBinderMap();
			}
			return _methodSymbolsWithYield;
		}
	}

	internal ExecutableCodeBinder(SyntaxNode root, Symbol memberSymbol, Binder next, Action<Binder, SyntaxNode> binderUpdatedHandler = null)
		: this(root, memberSymbol, next, next.Flags)
	{
		_binderUpdatedHandler = binderUpdatedHandler;
	}

	internal ExecutableCodeBinder(SyntaxNode root, Symbol memberSymbol, Binder next, BinderFlags additionalFlags)
		: base(next, (next.Flags | additionalFlags) & ~BinderFlags.AllClearedAtExecutableCodeBoundary)
	{
		_memberSymbol = memberSymbol;
		_root = root;
	}

	internal override Binder GetBinder(SyntaxNode node)
	{
		if (!BinderMap.TryGetValue(node, out var value))
		{
			return Next.GetBinder(node);
		}
		return value;
	}

	private void ComputeBinderMap()
	{
		SmallDictionary<SyntaxNode, Binder> smallDictionary;
		ImmutableArray<MethodSymbol> value2;
		if ((object)_memberSymbol != null && _root != null)
		{
			ArrayBuilder<SyntaxNode> instance = ArrayBuilder<SyntaxNode>.GetInstance();
			ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
			smallDictionary = LocalBinderFactory.BuildMap(_memberSymbol, _root, this, instance, _binderUpdatedHandler);
			foreach (SyntaxNode item in instance)
			{
				Binder value = this;
				if (item.Kind() == SyntaxKind.GlobalStatement || (item != _root && !smallDictionary.TryGetValue(item, out value)))
				{
					continue;
				}
				Symbol containingMemberOrLambda = value.ContainingMemberOrLambda;
				InMethodBinder inMethodBinder = null;
				while (value != null)
				{
					inMethodBinder = value as InMethodBinder;
					if (inMethodBinder != null)
					{
						break;
					}
					value = value.Next;
				}
				if (inMethodBinder != null && (object)inMethodBinder.ContainingMemberOrLambda == containingMemberOrLambda)
				{
					inMethodBinder.MakeIterator();
					instance2.Add((MethodSymbol)inMethodBinder.ContainingMemberOrLambda);
				}
			}
			instance.Free();
			value2 = instance2.ToImmutableAndFree();
		}
		else
		{
			smallDictionary = SmallDictionary<SyntaxNode, Binder>.Empty;
			value2 = ImmutableArray<MethodSymbol>.Empty;
		}
		Interlocked.CompareExchange(ref _lazyBinderMap, smallDictionary, null);
		ImmutableInterlocked.InterlockedCompareExchange(ref _methodSymbolsWithYield, value2, default);
	}

	public void ValidateIteratorMethods(DiagnosticBag diagnostics)
	{
		foreach (MethodSymbol item in MethodSymbolsWithYield)
		{
			foreach (ParameterSymbol parameter in item.Parameters)
			{
				if (parameter.RefKind != RefKind.None)
				{
					diagnostics.Add(ErrorCode.ERR_BadIteratorArgType, parameter.Locations[0]);
				}
				else if (parameter.Type.IsUnsafe())
				{
					diagnostics.Add(ErrorCode.ERR_UnsafeIteratorArgType, parameter.Locations[0]);
				}
			}
			if (item.IsVararg)
			{
				diagnostics.Add(ErrorCode.ERR_VarargsIterator, item.Locations[0]);
			}
			SourceMemberMethodSymbol obj = item as SourceMemberMethodSymbol;
			if ((object)obj == null || !obj.IsUnsafe)
			{
				LocalFunctionSymbol obj2 = item as LocalFunctionSymbol;
				if ((object)obj2 == null || !obj2.IsUnsafe)
				{
					continue;
				}
			}
			if (Compilation.Options.AllowUnsafe)
			{
				diagnostics.Add(ErrorCode.ERR_IllegalInnerUnsafe, item.Locations[0]);
			}
		}
	}
}
