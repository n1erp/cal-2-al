using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class ExecutableCodeBinder : Binder
{
	private readonly SyntaxNode root;

	private readonly MethodSymbol ownerMethodSymbol;

	private SmallDictionary<SyntaxNode, Binder> lazyBinderMap;

	internal override Symbol ContainingMember => ownerMethodSymbol ?? Next.ContainingMember;

	internal Symbol MemberSymbol { get; }

	private SmallDictionary<SyntaxNode, Binder> BinderMap
	{
		get
		{
			if (lazyBinderMap == null)
			{
				MethodSymbol methodSymbol = ownerMethodSymbol;
				Interlocked.CompareExchange(value: ((object)methodSymbol == null || root == null) ? new SmallDictionary<SyntaxNode, Binder>() : LocalBinderFactory.BuildMap(methodSymbol, root, this), location1: ref lazyBinderMap, comparand: null);
			}
			return lazyBinderMap;
		}
	}

	internal ExecutableCodeBinder(SyntaxNode root, Symbol memberSymbol, Binder next)
		: this(root, memberSymbol, next, next.Flags)
	{
	}

	internal ExecutableCodeBinder(SyntaxNode root, Symbol memberSymbol, Binder next, BinderFlags additionalFlags)
		: base(next, next.Flags | additionalFlags)
	{
		MemberSymbol = memberSymbol;
		this.root = root;
		ownerMethodSymbol = memberSymbol as MethodSymbol;
	}

	internal override Binder GetBinder(SyntaxNode node)
	{
		if (!BinderMap.TryGetValue(node, out var value))
		{
			return Next.GetBinder(node);
		}
		return value;
	}
}
