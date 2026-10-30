using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class ModuleReference : IModuleReference, IUnitReference, IReference, INamedEntity, IFileReference
{
	private readonly PEModuleBuilder _moduleBeingBuilt;

	private readonly ModuleSymbol _underlyingModule;

	string INamedEntity.Name => _underlyingModule.MetadataName;

	bool IFileReference.HasMetadata => true;

	string IFileReference.FileName => _underlyingModule.Name;

	internal ModuleReference(PEModuleBuilder moduleBeingBuilt, ModuleSymbol underlyingModule)
	{
		_moduleBeingBuilt = moduleBeingBuilt;
		_underlyingModule = underlyingModule;
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit((IModuleReference)this);
	}

	ImmutableArray<byte> IFileReference.GetHashValue(AssemblyHashAlgorithm algorithmId)
	{
		return _underlyingModule.GetHash(algorithmId);
	}

	IAssemblyReference IModuleReference.GetContainingAssembly(EmitContext context)
	{
		if (_moduleBeingBuilt.OutputKind.IsNetModule() && (object)_moduleBeingBuilt.SourceModule.ContainingAssembly == _underlyingModule.ContainingAssembly)
		{
			return null;
		}
		return _moduleBeingBuilt.Translate(_underlyingModule.ContainingAssembly, context.Diagnostics);
	}

	public override string ToString()
	{
		return _underlyingModule.ToString();
	}

	IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
	{
		return SpecializedCollections.EmptyEnumerable<ICustomAttribute>();
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		return null;
	}
}
