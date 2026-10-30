using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Utility;

public static class CompilationTargetHelper
{
	public static Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget CompilationTargetMapping(Microsoft.Dynamics.Nav.Types.CompilationTarget target)
	{
		Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget result = Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Extension;
		switch (target)
		{
		case Microsoft.Dynamics.Nav.Types.CompilationTarget.Personalization:
			result = Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Personalization;
			break;
		case Microsoft.Dynamics.Nav.Types.CompilationTarget.Extension:
			result = Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Extension;
			break;
		case Microsoft.Dynamics.Nav.Types.CompilationTarget.Solution:
			result = Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Solution;
			break;
		case Microsoft.Dynamics.Nav.Types.CompilationTarget.Internal:
			result = Microsoft.Dynamics.Nav.CodeAnalysis.CompilationTarget.Internal;
			break;
		}
		return result;
	}
}
