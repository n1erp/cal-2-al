using System;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.ExtensionProxySvc.Client;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.ExternalDeploy;

internal class NavAppDiagnosticParameter : IDiagnosticParameter, IFormattable
{
	public object Content { get; set; }

	public PrivacyClassification Classification
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return string.Format(formatProvider, Content?.ToString() ?? string.Empty);
	}

	public DiagnosticParameter ToDiagnosticParameter()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected I4, but got Unknown
		return new DiagnosticParameter(Content, (PrivacyClassification)Classification);
	}
}
