using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class PEGlobalNamespaceSymbol : PENamespaceSymbol
{
	private readonly PEModuleSymbol _moduleSymbol;

	public override Symbol ContainingSymbol => _moduleSymbol;

	internal override PEModuleSymbol ContainingPEModule => _moduleSymbol;

	public override string Name => string.Empty;

	public override bool IsGlobalNamespace => true;

	public override AssemblySymbol ContainingAssembly => _moduleSymbol.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _moduleSymbol;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal PEGlobalNamespaceSymbol(PEModuleSymbol moduleSymbol)
	{
		_moduleSymbol = moduleSymbol;
	}

	protected override void EnsureAllMembersLoaded()
	{
		if (lazyTypes == null || lazyNamespaces == null)
		{
			IEnumerable<IGrouping<string, TypeDefinitionHandle>> typesByNS;
			try
			{
				typesByNS = _moduleSymbol.Module.GroupTypesByNamespaceOrThrow(StringComparer.Ordinal);
			}
			catch (BadImageFormatException)
			{
				typesByNS = SpecializedCollections.EmptyEnumerable<IGrouping<string, TypeDefinitionHandle>>();
			}
			LoadAllMembers(typesByNS);
		}
	}
}
