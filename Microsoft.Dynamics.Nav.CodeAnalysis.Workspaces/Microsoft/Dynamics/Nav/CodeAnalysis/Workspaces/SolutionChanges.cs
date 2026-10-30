using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
public struct SolutionChanges
{
	private readonly Solution newSolution;

	private readonly Solution oldSolution;

	internal SolutionChanges(Solution newSolution, Solution oldSolution)
	{
		this.newSolution = newSolution;
		this.oldSolution = oldSolution;
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public IEnumerable<Project> GetAddedProjects()
	{
		foreach (ProjectId projectId in newSolution.ProjectIds)
		{
			if (!oldSolution.ContainsProject(projectId))
			{
				yield return newSolution.GetProject(projectId);
			}
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public IEnumerable<ProjectChanges> GetProjectChanges()
	{
		Solution old = oldSolution;
		foreach (ProjectId projectId in newSolution.ProjectIds)
		{
			ProjectState projectState = newSolution.GetProjectState(projectId);
			ProjectState projectState2 = old.GetProjectState(projectId);
			if (projectState2 != null && projectState != null && projectState != projectState2)
			{
				yield return newSolution.GetProject(projectId).GetChanges(oldSolution.GetProject(projectId));
			}
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public IEnumerable<Project> GetRemovedProjects()
	{
		foreach (ProjectId projectId in oldSolution.ProjectIds)
		{
			if (!newSolution.ContainsProject(projectId))
			{
				yield return oldSolution.GetProject(projectId);
			}
		}
	}
}
