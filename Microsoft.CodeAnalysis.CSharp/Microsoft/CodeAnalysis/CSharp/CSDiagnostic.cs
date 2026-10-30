using System;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class CSDiagnostic : DiagnosticWithInfo
{
	internal CSDiagnostic(DiagnosticInfo info, Location location, bool isSuppressed = false)
		: base(info, location, isSuppressed)
	{
	}

	public override string ToString()
	{
		return CSharpDiagnosticFormatter.Instance.Format(this);
	}

	internal override Diagnostic WithLocation(Location location)
	{
		if (location == null)
		{
			throw new ArgumentNullException("location");
		}
		if (location != Location)
		{
			return new CSDiagnostic(Info, location, IsSuppressed);
		}
		return this;
	}

	internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
	{
		if (Severity != severity)
		{
			return new CSDiagnostic(Info.GetInstanceWithSeverity(severity), Location, IsSuppressed);
		}
		return this;
	}

	internal override Diagnostic WithIsSuppressed(bool isSuppressed)
	{
		if (IsSuppressed != isSuppressed)
		{
			return new CSDiagnostic(Info, Location, isSuppressed);
		}
		return this;
	}
}
