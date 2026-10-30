using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

public interface ICodeFixProvider
{
	ImmutableArray<string> FixableDiagnosticIds { get; }

	Task RegisterCodeFixesAsync(CodeFixContext context);
}
