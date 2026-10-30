using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.CodeActions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeFixes;

internal sealed class CodeFix
{
	internal readonly Project Project;

	internal readonly CodeAction Action;

	internal readonly ImmutableArray<Diagnostic> Diagnostics;

	internal Diagnostic PrimaryDiagnostic => Diagnostics[0];

	internal CodeFix(Project project, CodeAction action, Diagnostic diagnostic)
	{
		Project = project;
		Action = action;
		Diagnostics = ImmutableArray.Create(diagnostic);
	}

	internal CodeFix(Project project, CodeAction action, ImmutableArray<Diagnostic> diagnostics)
	{
		Project = project;
		Action = action;
		Diagnostics = diagnostics;
	}
}
