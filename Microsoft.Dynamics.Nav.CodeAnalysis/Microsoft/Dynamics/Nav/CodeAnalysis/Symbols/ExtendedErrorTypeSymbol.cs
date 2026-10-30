using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class ExtendedErrorTypeSymbol : ErrorTypeSymbol
{
	private readonly string name;

	private readonly ContainerSymbol containingSymbol;

	private readonly ImmutableArray<Symbol> candidateSymbols;

	internal NavTypeKind IntendedKind { get; private set; }

	public bool VariableUsedBeforeDeclaration { get; private set; }

	internal override DiagnosticInfo ErrorInfo { get; }

	internal override LookupResultKind ResultKind { get; }

	public override ImmutableArray<Symbol> CandidateSymbols
	{
		get
		{
			if (!candidateSymbols.IsDefault)
			{
				return candidateSymbols;
			}
			return ImmutableArray<Symbol>.Empty;
		}
	}

	public override Symbol ContainingSymbol => containingSymbol;

	public override string Name => name;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitExtendedErrorType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitExtendedErrorType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitExtendedErrorType(this, a);
	}

	internal ExtendedErrorTypeSymbol(Compilation compilation, string name, DiagnosticInfo errorInfo, bool variableUsedBeforeDeclaration = false, NavTypeKind intendedKind = NavTypeKind.None)
		: this(compilation.CompiledModule, name, errorInfo, variableUsedBeforeDeclaration, intendedKind)
	{
	}

	internal ExtendedErrorTypeSymbol(ContainerSymbol containingSymbol, string nameParameter, DiagnosticInfo errorInfoParameter, bool variableUsedBeforeDeclaration = false, NavTypeKind intendedKind = NavTypeKind.None)
	{
		DebugAssertHelper.Assert(nameParameter != null);
		name = nameParameter;
		ErrorInfo = errorInfoParameter;
		this.containingSymbol = containingSymbol;
		VariableUsedBeforeDeclaration = variableUsedBeforeDeclaration;
		ResultKind = LookupResultKind.Empty;
		IntendedKind = intendedKind;
	}

	internal ExtendedErrorTypeSymbol(ContainerSymbol guessSymbol, LookupResultKind resultKind, DiagnosticInfo errorInfo)
		: this((ContainerSymbol)guessSymbol.ContainingSymbol, guessSymbol, resultKind, errorInfo)
	{
	}

	internal ExtendedErrorTypeSymbol(ContainerSymbol containingSymbol, Symbol guessSymbol, LookupResultKind resultKind, DiagnosticInfo errorInfo)
		: this(containingSymbol, ImmutableArray.Create(guessSymbol), resultKind, errorInfo)
	{
	}

	internal ExtendedErrorTypeSymbol(ContainerSymbol containingSymbol, ImmutableArray<Symbol> candidateSymbols, LookupResultKind resultKind, DiagnosticInfo errorInfo)
		: this(containingSymbol, candidateSymbols[0].Name, errorInfo)
	{
		this.candidateSymbols = UnwrapErrorCandidates(candidateSymbols);
		ResultKind = resultKind;
		DebugAssertHelper.Assert(candidateSymbols.IsEmpty || resultKind != LookupResultKind.Viable, "Shouldn't use LookupResultKind.Viable with candidate symbols");
	}

	private static ImmutableArray<Symbol> UnwrapErrorCandidates(ImmutableArray<Symbol> candidateSymbols)
	{
		ErrorTypeSymbol errorTypeSymbol = (candidateSymbols.IsEmpty ? null : (candidateSymbols[0] as ErrorTypeSymbol));
		if ((object)errorTypeSymbol == null || errorTypeSymbol.CandidateSymbols.IsEmpty)
		{
			return candidateSymbols;
		}
		return errorTypeSymbol.CandidateSymbols;
	}

	internal static TypeSymbol ExtractNonErrorType(TypeSymbol oldSymbol)
	{
		if ((object)oldSymbol == null || oldSymbol.TypeCategory != TypeCategoryKind.Error)
		{
			return oldSymbol;
		}
		return null;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(((object)ContainingSymbol != null) ? ContainingSymbol.GetHashCode() : 0, (Name != null) ? Name.GetHashCode() : 0);
	}
}
