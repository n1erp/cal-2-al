using System;

namespace Microsoft.CodeAnalysis;

internal sealed class MetadataLocation : Location, IEquatable<MetadataLocation>
{
	private readonly IModuleSymbol _module;

	public override LocationKind Kind => LocationKind.MetadataFile;

	public override IModuleSymbol MetadataModule => _module;

	internal MetadataLocation(IModuleSymbol module)
	{
		_module = module;
	}

	public override int GetHashCode()
	{
		return _module.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as MetadataLocation);
	}

	public bool Equals(MetadataLocation other)
	{
		if (other != null)
		{
			return other._module == _module;
		}
		return false;
	}
}
