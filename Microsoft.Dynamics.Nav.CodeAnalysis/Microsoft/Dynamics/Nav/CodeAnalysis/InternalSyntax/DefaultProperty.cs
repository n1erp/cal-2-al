using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerDisplay("Name={Name}, Value={DefaultValue}")]
internal class DefaultProperty
{
	internal PropertyTypeInfo PropertyTypeInfo { get; }

	internal string DefaultValue { get; set; }

	internal bool AlwaysOutputMetadata { get; }

	internal bool IsBoolean { get; }

	internal bool EmitAsAttribute { get; }

	internal string Name { get; }

	internal string MetadataName { get; }

	internal ImmutableList<DependentProperty> DependentProperties { get; }

	internal ImmutableList<DependentProperty> DependentParentProperties { get; }

	internal ImmutableList<DependentProperty> DependentApplicationObjectProperties { get; }

	internal DefaultProperty(PropertyTypeInfo info)
	{
		PropertyTypeInfo = info;
		Name = info.Name;
		MetadataName = (string.IsNullOrEmpty(info.Name) ? info.Name : info.MetadataName);
		if (string.Compare(info.TypeName, "Boolean", StringComparison.OrdinalIgnoreCase) == 0)
		{
			DefaultValue = (bool.Parse(info.DefaultValue) ? "1" : "0");
			IsBoolean = true;
		}
		else
		{
			DefaultValue = info.DefaultValue;
		}
		DependentProperties = info.DependentProperties;
		DependentParentProperties = info.DependentParentProperties;
		DependentApplicationObjectProperties = info.DependentDeclaringApplicationObjectProperties;
		EmitAsAttribute = info.EmitAsAttribute;
		AlwaysOutputMetadata = info.AlwaysOutputValue;
	}
}
