using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class PENestedNamespaceSymbol : PENamespaceSymbol
{
	private readonly PENamespaceSymbol _containingNamespaceSymbol;

	private readonly string _name;

	private IEnumerable<IGrouping<string, TypeDefinitionHandle>> _typesByNS;

	public override Symbol ContainingSymbol => _containingNamespaceSymbol;

	internal override PEModuleSymbol ContainingPEModule => _containingNamespaceSymbol.ContainingPEModule;

	public override string Name => _name;

	public override bool IsGlobalNamespace => false;

	public override AssemblySymbol ContainingAssembly => ContainingPEModule.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _containingNamespaceSymbol.ContainingPEModule;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	internal PENestedNamespaceSymbol(string name, PENamespaceSymbol containingNamespace, IEnumerable<IGrouping<string, TypeDefinitionHandle>> typesByNS)
	{
		_containingNamespaceSymbol = containingNamespace;
		_name = name;
		_typesByNS = typesByNS;
	}

	protected override void EnsureAllMembersLoaded()
	{
		IEnumerable<IGrouping<string, TypeDefinitionHandle>> typesByNS = _typesByNS;
		if (lazyTypes == null || lazyNamespaces == null)
		{
			LoadAllMembers(typesByNS);
			Interlocked.Exchange(ref _typesByNS, null);
		}
	}
}
