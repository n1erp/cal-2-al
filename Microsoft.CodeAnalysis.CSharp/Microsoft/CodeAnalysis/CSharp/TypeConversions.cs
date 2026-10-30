using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class TypeConversions : ConversionsBase
{
	public TypeConversions(AssemblySymbol corLibrary)
		: this(corLibrary, 0)
	{
	}

	private TypeConversions(AssemblySymbol corLibrary, int currentRecursionDepth)
		: base(corLibrary, currentRecursionDepth)
	{
	}

	protected override ConversionsBase CreateInstance(int currentRecursionDepth)
	{
		return new TypeConversions(corLibrary, currentRecursionDepth);
	}

	public override Conversion GetMethodGroupConversion(BoundMethodGroup source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		throw ExceptionUtilities.Unreachable;
	}

	public override Conversion GetStackAllocConversion(BoundStackAllocArrayCreation sourceExpression, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		throw ExceptionUtilities.Unreachable;
	}

	protected override Conversion GetInterpolatedStringConversion(BoundInterpolatedString source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
