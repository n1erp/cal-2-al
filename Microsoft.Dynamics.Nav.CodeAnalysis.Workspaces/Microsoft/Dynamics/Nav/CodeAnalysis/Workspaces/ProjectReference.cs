using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
public sealed class ProjectReference : IEquatable<ProjectReference>
{
	public ProjectId ProjectId { get; }

	public ImmutableArray<string> Aliases { get; }

	public ProjectReference(ProjectId projectId, ImmutableArray<string> aliases = default(ImmutableArray<string>))
	{
		Contract.ThrowIfNull(projectId);
		ProjectId = projectId;
		Aliases = aliases.NullToEmpty();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ProjectReference);
	}

	public bool Equals(ProjectReference other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if ((object)other != null && ProjectId == other.ProjectId)
		{
			return Aliases.SequenceEqual(other.Aliases);
		}
		return false;
	}

	public static bool operator ==(ProjectReference left, ProjectReference right)
	{
		return EqualityComparer<ProjectReference>.Default.Equals(left, right);
	}

	public static bool operator !=(ProjectReference left, ProjectReference right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Hash.CombineValues(Aliases, ProjectId.GetHashCode());
	}

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
	private string GetDebuggerDisplay()
	{
		return ProjectId.ToString();
	}
}
