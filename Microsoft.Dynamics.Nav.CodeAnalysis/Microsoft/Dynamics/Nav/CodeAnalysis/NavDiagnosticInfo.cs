using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class NavDiagnosticInfo : DiagnosticInfoWithSymbols
{
	public static readonly DiagnosticInfo EmptyErrorInfo = new NavDiagnosticInfo(ErrorCode.None);

	public static readonly DiagnosticInfo VoidDiagnosticInfo = new NavDiagnosticInfo(ErrorCode.Void);

	private readonly ImmutableArray<Location> additionalLocations;

	public override IReadOnlyList<Location> AdditionalLocations => additionalLocations;

	internal new ErrorCode Code => (ErrorCode)base.Code;

	internal NavDiagnosticInfo(ErrorCode code)
		: this(code, SpecializedCollections.EmptyArray<object>(), ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty)
	{
	}

	internal NavDiagnosticInfo(ErrorCode code, params object[] args)
		: this(code, args, ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty)
	{
	}

	internal NavDiagnosticInfo(ErrorCode code, ImmutableArray<Symbol> symbols, object[] args)
		: this(code, args, symbols, ImmutableArray<Location>.Empty)
	{
	}

	internal NavDiagnosticInfo(ErrorCode code, object[] args, ImmutableArray<Symbol> symbols, ImmutableArray<Location> additionalLocations)
		: base(code, args, symbols)
	{
		this.additionalLocations = additionalLocations;
	}

	internal static bool IsEmpty(DiagnosticInfo info)
	{
		return info == EmptyErrorInfo;
	}
}
