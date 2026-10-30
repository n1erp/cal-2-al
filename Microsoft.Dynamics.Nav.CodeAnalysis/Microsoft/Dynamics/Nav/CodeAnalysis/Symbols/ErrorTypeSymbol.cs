using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ErrorTypeSymbol : TypeSymbol
{
	internal abstract DiagnosticInfo ErrorInfo { get; }

	internal virtual LookupResultKind ResultKind => LookupResultKind.Empty;

	public virtual ImmutableArray<Symbol> CandidateSymbols => ImmutableArray<Symbol>.Empty;

	public CandidateReason CandidateReason
	{
		get
		{
			if (!CandidateSymbols.IsEmpty)
			{
				DebugAssertHelper.Assert(ResultKind != LookupResultKind.Viable, "Shouldn't have viable result kind on error symbol");
				return ResultKind.ToCandidateReason();
			}
			return CandidateReason.None;
		}
	}

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public sealed override SymbolKind Kind => SymbolKind.ErrorType;

	public sealed override TypeCategoryKind TypeCategory => TypeCategoryKind.Error;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitErrorType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitErrorType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitErrorType(this, a);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}
