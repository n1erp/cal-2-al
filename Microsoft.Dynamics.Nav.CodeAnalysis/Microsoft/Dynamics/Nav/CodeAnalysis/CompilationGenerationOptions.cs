using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
public enum CompilationGenerationOptions
{
	None = 0,
	Code = 1,
	ReportLayout = 2,
	All = Code | ReportLayout
}
