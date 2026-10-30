using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CSDiagnosticInfo : DiagnosticInfoWithSymbols
{
	public static readonly DiagnosticInfo EmptyErrorInfo = new CSDiagnosticInfo((ErrorCode)0);

	public static readonly DiagnosticInfo VoidDiagnosticInfo = new CSDiagnosticInfo(ErrorCode.Void);

	private readonly ImmutableArray<Location> _additionalLocations;

	public override IReadOnlyList<Location> AdditionalLocations => _additionalLocations;

	internal new ErrorCode Code => (ErrorCode)base.Code;

	internal CSDiagnosticInfo(ErrorCode code)
		: this(code, Array.Empty<object>(), ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty)
	{
	}

	internal CSDiagnosticInfo(ErrorCode code, params object[] args)
		: this(code, args, ImmutableArray<Symbol>.Empty, ImmutableArray<Location>.Empty)
	{
	}

	internal CSDiagnosticInfo(ErrorCode code, ImmutableArray<Symbol> symbols, object[] args)
		: this(code, args, symbols, ImmutableArray<Location>.Empty)
	{
	}

	internal CSDiagnosticInfo(ErrorCode code, object[] args, ImmutableArray<Symbol> symbols, ImmutableArray<Location> additionalLocations)
		: base(code, args, symbols)
	{
		_additionalLocations = additionalLocations;
	}

	internal static bool IsEmpty(DiagnosticInfo info)
	{
		return info == EmptyErrorInfo;
	}
}
