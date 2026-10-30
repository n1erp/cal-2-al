using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

[DebuggerDisplay("ModuleDefinition: {Name,nq} {Publisher,nq} {Version,nq}")]
public class ModuleDefinition
{
	public Guid AppId { get; set; }

	public string Name { get; set; }

	public string Publisher { get; set; }

	public string Version { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public TableDefinition[] Tables { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public CodeunitDefinition[] Codeunits { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public PageDefinition[] Pages { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public PageExtensionDefinition[] PageExtensions { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public PageCustomizationDefinition[] PageCustomizations { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public TableExtensionDefinition[] TableExtensions { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ReportDefinition[] Reports { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public XmlPortDefinition[] XmlPorts { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public QueryDefinition[] Queries { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ProfileDefinition[] Profiles { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public ControlAddInDefinition[] ControlAddIns { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public EnumTypeDefinition[] EnumTypes { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public EnumExtensionTypeDefinition[] EnumExtensionTypes { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	public DotNetPackageDefinition[] DotNetPackages { get; set; }

	public NavAppManifest Manifest { get; set; }
}
