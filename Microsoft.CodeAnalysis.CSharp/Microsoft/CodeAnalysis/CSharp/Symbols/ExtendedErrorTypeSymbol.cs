using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class ExtendedErrorTypeSymbol : ErrorTypeSymbol
{
	private readonly string _name;

	private readonly int _arity;

	private readonly DiagnosticInfo _errorInfo;

	private readonly NamespaceOrTypeSymbol _containingSymbol;

	private readonly bool _unreported;

	public readonly bool VariableUsedBeforeDeclaration;

	private readonly ImmutableArray<Symbol> _candidateSymbols;

	private readonly LookupResultKind _resultKind;

	internal override DiagnosticInfo ErrorInfo => _errorInfo;

	internal override LookupResultKind ResultKind => _resultKind;

	public override ImmutableArray<Symbol> CandidateSymbols => _candidateSymbols.NullToEmpty();

	internal override bool Unreported => _unreported;

	public override int Arity => _arity;

	internal override bool MangleName => _arity > 0;

	public override Symbol ContainingSymbol => _containingSymbol;

	public override string Name => _name;

	public override NamedTypeSymbol OriginalDefinition => this;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override NamedTypeSymbol ConstructedFrom => this;

	internal ExtendedErrorTypeSymbol(CSharpCompilation compilation, string name, int arity, DiagnosticInfo errorInfo, bool unreported = false, bool variableUsedBeforeDeclaration = false)
		: this(compilation.Assembly.GlobalNamespace, name, arity, errorInfo, unreported, variableUsedBeforeDeclaration)
	{
	}

	internal ExtendedErrorTypeSymbol(NamespaceOrTypeSymbol containingSymbol, string name, int arity, DiagnosticInfo errorInfo, bool unreported = false, bool variableUsedBeforeDeclaration = false)
	{
		_name = name;
		_errorInfo = errorInfo;
		_containingSymbol = containingSymbol;
		_arity = arity;
		_unreported = unreported;
		VariableUsedBeforeDeclaration = variableUsedBeforeDeclaration;
		_resultKind = LookupResultKind.Empty;
	}

	private ExtendedErrorTypeSymbol(NamespaceOrTypeSymbol containingSymbol, string name, int arity, DiagnosticInfo errorInfo, bool unreported, bool variableUsedBeforeDeclaration, ImmutableArray<Symbol> candidateSymbols, LookupResultKind resultKind)
	{
		_name = name;
		_errorInfo = errorInfo;
		_containingSymbol = containingSymbol;
		_arity = arity;
		_unreported = unreported;
		VariableUsedBeforeDeclaration = variableUsedBeforeDeclaration;
		_candidateSymbols = candidateSymbols;
		_resultKind = resultKind;
	}

	internal ExtendedErrorTypeSymbol(NamespaceOrTypeSymbol guessSymbol, LookupResultKind resultKind, DiagnosticInfo errorInfo, bool unreported = false)
		: this(guessSymbol.ContainingNamespaceOrType(), guessSymbol, resultKind, errorInfo, unreported)
	{
	}

	internal ExtendedErrorTypeSymbol(NamespaceOrTypeSymbol containingSymbol, Symbol guessSymbol, LookupResultKind resultKind, DiagnosticInfo errorInfo, bool unreported = false)
		: this(containingSymbol, ImmutableArray.Create(guessSymbol), resultKind, errorInfo, GetArity(guessSymbol), unreported)
	{
	}

	internal ExtendedErrorTypeSymbol(NamespaceOrTypeSymbol containingSymbol, ImmutableArray<Symbol> candidateSymbols, LookupResultKind resultKind, DiagnosticInfo errorInfo, int arity, bool unreported = false)
		: this(containingSymbol, candidateSymbols[0].Name, arity, errorInfo, unreported)
	{
		_candidateSymbols = UnwrapErrorCandidates(candidateSymbols);
		_resultKind = resultKind;
	}

	internal ExtendedErrorTypeSymbol AsUnreported()
	{
		if (!Unreported)
		{
			return new ExtendedErrorTypeSymbol(_containingSymbol, _name, _arity, _errorInfo, unreported: true, VariableUsedBeforeDeclaration, _candidateSymbols, _resultKind);
		}
		return this;
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

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		if (_unreported)
		{
			return ErrorInfo;
		}
		return null;
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<Symbol> basesBeingResolved)
	{
		return null;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<Symbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal static TypeSymbol ExtractNonErrorType(TypeSymbol oldSymbol)
	{
		if ((object)oldSymbol == null || oldSymbol.TypeKind != TypeKind.Error)
		{
			return oldSymbol;
		}
		if (oldSymbol.OriginalDefinition is ExtendedErrorTypeSymbol { _candidateSymbols: { IsDefault: false }, _candidateSymbols: { Length: 1 }, _candidateSymbols: var candidateSymbols3 } && candidateSymbols3[0] is TypeSymbol type)
		{
			return type.GetNonErrorGuess();
		}
		return null;
	}

	internal static TypeKind ExtractNonErrorTypeKind(TypeSymbol oldSymbol)
	{
		if (oldSymbol.TypeKind != TypeKind.Error)
		{
			return oldSymbol.TypeKind;
		}
		ExtendedErrorTypeSymbol extendedErrorTypeSymbol = oldSymbol.OriginalDefinition as ExtendedErrorTypeSymbol;
		TypeKind typeKind = TypeKind.Error;
		if ((object)extendedErrorTypeSymbol != null && !extendedErrorTypeSymbol._candidateSymbols.IsDefault && extendedErrorTypeSymbol._candidateSymbols.Length > 0)
		{
			foreach (Symbol candidateSymbol in extendedErrorTypeSymbol._candidateSymbols)
			{
				if (candidateSymbol is TypeSymbol { TypeKind: not TypeKind.Error } typeSymbol)
				{
					if (typeKind == TypeKind.Error)
					{
						typeKind = typeSymbol.TypeKind;
					}
					else if (typeKind != typeSymbol.TypeKind)
					{
						return TypeKind.Error;
					}
				}
			}
		}
		return typeKind;
	}

	internal override bool Equals(TypeSymbol t2, TypeCompareKind comparison)
	{
		if ((object)this == t2)
		{
			return true;
		}
		if (!(t2 is ExtendedErrorTypeSymbol extendedErrorTypeSymbol) || _unreported || extendedErrorTypeSymbol._unreported)
		{
			return false;
		}
		bool flag;
		if ((object)ContainingType != null)
		{
			flag = ContainingType.Equals(extendedErrorTypeSymbol.ContainingType, comparison);
		}
		else
		{
			flag = (((object)ContainingSymbol == null) ? ((object)extendedErrorTypeSymbol.ContainingSymbol == null) : ContainingSymbol.Equals(extendedErrorTypeSymbol.ContainingSymbol));
		}
		if (flag && Name == extendedErrorTypeSymbol.Name)
		{
			return Arity == extendedErrorTypeSymbol.Arity;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(Arity, Hash.Combine(((object)ContainingSymbol != null) ? ContainingSymbol.GetHashCode() : 0, (Name != null) ? Name.GetHashCode() : 0));
	}

	private static int GetArity(Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return ((NamedTypeSymbol)symbol).Arity;
		case SymbolKind.Method:
			return ((MethodSymbol)symbol).Arity;
		default:
			return 0;
		}
	}
}
