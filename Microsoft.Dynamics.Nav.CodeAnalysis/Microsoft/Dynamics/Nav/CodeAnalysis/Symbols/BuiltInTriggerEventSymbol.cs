using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class BuiltInTriggerEventSymbol : MethodSymbol
{
	private ImmutableArray<ParameterSymbol> parameters;

	public TriggerEventTypeInfo Info { get; }

	public override ImmutableArray<ParameterSymbol> Parameters
	{
		get
		{
			LazyCreateParameters();
			return parameters;
		}
	}

	public override string Name => Info.Name;

	public override Symbol ContainingSymbol { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ReturnValueSymbol ReturnValue => Microsoft.Dynamics.Nav.CodeAnalysis.Symbols.ReturnValueSymbol.None;

	public override MethodKind MethodKind => MethodKind.Trigger;

	public override ImmutableArray<AttributeSymbol> Attributes => ImmutableArray<AttributeSymbol>.Empty;

	public override ImmutableArray<VariableSymbol> LocalVariables => ImmutableArray<VariableSymbol>.Empty;

	public override bool IsStatic => false;

	public override bool IsLocal => false;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitBuiltInTriggerEvent(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitBuiltInTriggerEvent(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitBuiltInTriggerEvent(this, a);
	}

	public BuiltInTriggerEventSymbol(Symbol containingSymbol, TriggerEventTypeInfo info)
	{
		if (containingSymbol == null)
		{
			throw new ArgumentNullException("containingSymbol");
		}
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		ContainingSymbol = containingSymbol;
		Info = info;
	}

	private void LazyCreateParameters()
	{
		if (!(parameters != null) && !(Info.Parameters == null))
		{
			ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
			int num = 0;
			foreach (TriggerParameter parameter in Info.Parameters)
			{
				instance.Add(new BuiltInParameterSymbol(this, parameter.Type, parameter.Name, num++, parameter.IsVar));
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref parameters, instance.ToImmutableAndFree(), default);
		}
	}
}
