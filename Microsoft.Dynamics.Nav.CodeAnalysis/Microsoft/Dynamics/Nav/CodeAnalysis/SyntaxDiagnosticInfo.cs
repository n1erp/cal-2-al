using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class SyntaxDiagnosticInfo : DiagnosticInfo
{
	internal readonly int Offset;

	internal readonly int Width;

	internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code, params object[] args)
		: base(Microsoft.Dynamics.Nav.CodeAnalysis.MessageProvider.Instance, (int)code, args)
	{
		DebugAssertHelper.Assert(width >= 0);
		Offset = offset;
		Width = width;
	}

	internal SyntaxDiagnosticInfo(int offset, int width, ErrorCode code)
		: this(offset, width, code, SpecializedCollections.EmptyObjects)
	{
	}

	internal SyntaxDiagnosticInfo(ErrorCode code, params object[] args)
		: this(0, 0, code, args)
	{
	}

	internal SyntaxDiagnosticInfo(ErrorCode code)
		: this(0, 0, code)
	{
	}

	public SyntaxDiagnosticInfo WithOffset(int offset)
	{
		return new SyntaxDiagnosticInfo(offset, Width, (ErrorCode)Code, Arguments);
	}
}
