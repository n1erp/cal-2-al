using System.Threading.Tasks;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.CodeRefactoring;

public interface ICodeRefactoringProvider
{
	Task ComputeRefactoringsAsync(CodeRefactoringContext context);
}
