using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class PropertyValueRecommendation : IEquatable<PropertyValueRecommendation>
{
	public string DisplayText { get; set; }

	public string DetailText { get; set; }

	public string DescriptionValue { get; set; }

	public bool IsMarkdownDocs { get; set; }

	public bool Equals(PropertyValueRecommendation other)
	{
		if (other == null)
		{
			return false;
		}
		if (string.Equals(DisplayText, other.DisplayText, StringComparison.OrdinalIgnoreCase) && string.Equals(DetailText, other.DetailText, StringComparison.OrdinalIgnoreCase) && string.Equals(DescriptionValue, other.DescriptionValue, StringComparison.OrdinalIgnoreCase))
		{
			return IsMarkdownDocs == other.IsMarkdownDocs;
		}
		return false;
	}
}
