using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[Flags]
public enum CompilerFeatures
{
	None = 0,
	GenerateLcgTranslationFile = 2,
	GenerateXliffTranslationFile = 4
}
