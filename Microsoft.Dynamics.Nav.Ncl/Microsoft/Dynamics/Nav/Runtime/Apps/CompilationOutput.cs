using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class CompilationOutput
{
	public Compilation Compilation { get; }

	public CodeModuleOutputter Outputter { get; }

	public CompilationOutput(Compilation compilation, CodeModuleOutputter outputter)
	{
		Compilation = compilation;
		Outputter = outputter;
	}
}
