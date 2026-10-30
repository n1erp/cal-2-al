namespace Microsoft.Dynamics.Nav.Model.Tools;

internal class ApplicationObjectPropertyToolOptions : ReadWriteModelToolOptions
{
	internal SetModifiedPropertyAction? ModifiedProperty { get; set; }

	internal string VersionListProperty { get; set; }

	internal string DateTimeProperty { get; set; }
}
