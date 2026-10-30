using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class EventSymbol : Symbol, IEventSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithAttributes
{
	public abstract ImmutableArray<ParameterSymbol> Parameters { get; }

	public override SymbolKind Kind { get; } = SymbolKind.Event;

	ImmutableArray<IParameterSymbol> IEventSymbol.Parameters => StaticCast<IParameterSymbol>.From(Parameters);

	ImmutableArray<IAttributeSymbol> IEventSymbol.Attributes => StaticCast<IAttributeSymbol>.From(Attributes);

	public abstract ImmutableArray<AttributeSymbol> Attributes { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitEvent(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitEvent(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitEvent(this, a);
	}

	internal void CheckSubscriberParameters(MethodSymbol subscriber, DiagnosticBag diagnostics, bool enforceExactVarMatch)
	{
		int i;
		for (i = 0; i < subscriber.Parameters.Length; i++)
		{
			ParameterSymbol parameterSymbol = subscriber.Parameters[i];
			if (i >= Parameters.Length)
			{
				diagnostics.Add(ErrorCode.ERR_EventUnmatchedParameter, parameterSymbol.Locations.FirstOrDefault(), subscriber.Name, parameterSymbol.Name);
				continue;
			}
			ParameterSymbol parameterSymbol2 = Parameters[i];
			if (!parameterSymbol.Type.NavTypeKind.Equals(parameterSymbol2.Type.NavTypeKind))
			{
				diagnostics.Add(ErrorCode.ERR_EventSubscriberParameterTypeMismatch, parameterSymbol.Locations.FirstOrDefault(), subscriber.Name, parameterSymbol.Name, parameterSymbol2.Type);
			}
			if ((parameterSymbol.IsVar && !parameterSymbol2.IsVar) || (enforceExactVarMatch && parameterSymbol.IsVar != parameterSymbol2.IsVar))
			{
				ErrorCode code = (enforceExactVarMatch ? ErrorCode.ERR_EventParameterByVarMismatch : ErrorCode.ERR_EventParameterByVarIsNotAllowed);
				diagnostics.Add(code, parameterSymbol.Locations.FirstOrDefault(), parameterSymbol.Name);
			}
		}
		for (; i < Parameters.Length; i++)
		{
			diagnostics.Add(ErrorCode.ERR_EventSubscriberMissingParameter, subscriber.Locations.FirstOrDefault(), subscriber.Name, Parameters[i].Type);
		}
	}
}
