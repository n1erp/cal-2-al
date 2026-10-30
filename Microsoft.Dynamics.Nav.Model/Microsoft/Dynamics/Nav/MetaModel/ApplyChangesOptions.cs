using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.MetaModel;

public class ApplyChangesOptions
{
	public bool InlineCodeConflicts { get; set; }

	public bool SkipCodeValidation { get; set; }

	public bool DevBaseCompatibility { get; set; }

	public DocumentationConflictAction DocumentationConflict { get; set; }

	public ConflictSeverityType ConflictSeverityLevel { get; set; }

	public bool SkipOriginalValueCheck { get; set; }

	public bool AutoAlignControlIndent { get; set; }
}
