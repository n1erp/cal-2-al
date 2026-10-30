using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class NavDiagnostic : DiagnosticWithInfo
{
	internal NavDiagnostic(DiagnosticInfo info, Location location)
		: base(info, location)
	{
	}

	internal static Diagnostic Create(ErrorCode errorCode, params object[] args)
	{
		return new NavDiagnostic(new DiagnosticInfo(MessageProvider.Instance, (int)errorCode, args), Location.None);
	}

	public override string ToString()
	{
		return DiagnosticFormatter.Instance.Format(this);
	}

	internal override Diagnostic WithLocation(Location location)
	{
		if (location == null)
		{
			throw new ArgumentNullException("location");
		}
		if (location != Location)
		{
			return new NavDiagnostic(Info, location);
		}
		return this;
	}

	internal override Diagnostic WithSeverity(DiagnosticSeverity severity)
	{
		if (Severity != severity)
		{
			return new NavDiagnostic(Info.GetInstanceWithSeverity(severity), Location);
		}
		return this;
	}
}
