using System.Collections.Immutable;
using System.Globalization;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Retargeting;

internal sealed class RetargetingNamespaceSymbol : NamespaceSymbol
{
	private readonly RetargetingModuleSymbol _retargetingModule;

	private readonly NamespaceSymbol _underlyingNamespace;

	private RetargetingModuleSymbol.RetargetingSymbolTranslator RetargetingTranslator => _retargetingModule.RetargetingTranslator;

	public NamespaceSymbol UnderlyingNamespace => _underlyingNamespace;

	internal override NamespaceExtent Extent => new NamespaceExtent(_retargetingModule);

	public override Symbol ContainingSymbol => RetargetingTranslator.Retarget(_underlyingNamespace.ContainingSymbol);

	public override ImmutableArray<Location> Locations => _retargetingModule.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _underlyingNamespace.DeclaringSyntaxReferences;

	public override AssemblySymbol ContainingAssembly => _retargetingModule.ContainingAssembly;

	internal override ModuleSymbol ContainingModule => _retargetingModule;

	public override bool IsGlobalNamespace => _underlyingNamespace.IsGlobalNamespace;

	public override string Name => _underlyingNamespace.Name;

	internal sealed override CSharpCompilation DeclaringCompilation => null;

	public RetargetingNamespaceSymbol(RetargetingModuleSymbol retargetingModule, NamespaceSymbol underlyingNamespace)
	{
		_retargetingModule = retargetingModule;
		_underlyingNamespace = underlyingNamespace;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return RetargetMembers(_underlyingNamespace.GetMembers());
	}

	private ImmutableArray<Symbol> RetargetMembers(ImmutableArray<Symbol> underlyingMembers)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(underlyingMembers.Length);
		foreach (Symbol item in underlyingMembers)
		{
			if (item.Kind != SymbolKind.NamedType || !((NamedTypeSymbol)item).IsExplicitDefinitionOfNoPiaLocalType)
			{
				instance.Add(RetargetingTranslator.Retarget(item));
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal override ImmutableArray<Symbol> GetMembersUnordered()
	{
		return RetargetMembers(_underlyingNamespace.GetMembersUnordered());
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return RetargetMembers(_underlyingNamespace.GetMembers(name));
	}

	internal override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return RetargetTypeMembers(_underlyingNamespace.GetTypeMembersUnordered());
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return RetargetTypeMembers(_underlyingNamespace.GetTypeMembers());
	}

	private ImmutableArray<NamedTypeSymbol> RetargetTypeMembers(ImmutableArray<NamedTypeSymbol> underlyingMembers)
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance(underlyingMembers.Length);
		foreach (NamedTypeSymbol item in underlyingMembers)
		{
			if (!item.IsExplicitDefinitionOfNoPiaLocalType)
			{
				instance.Add(RetargetingTranslator.Retarget(item, RetargetOptions.RetargetPrimitiveTypesByName));
			}
		}
		return instance.ToImmutableAndFree();
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		return RetargetTypeMembers(_underlyingNamespace.GetTypeMembers(name));
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return RetargetTypeMembers(_underlyingNamespace.GetTypeMembers(name, arity));
	}

	public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return _underlyingNamespace.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
	}

	internal override NamedTypeSymbol LookupMetadataType(ref MetadataTypeName typeName)
	{
		NamedTypeSymbol namedTypeSymbol = _underlyingNamespace.LookupMetadataType(ref typeName);
		if (!namedTypeSymbol.IsErrorType() && namedTypeSymbol.IsExplicitDefinitionOfNoPiaLocalType)
		{
			return new MissingMetadataTypeSymbol.TopLevel(_retargetingModule, ref typeName);
		}
		return RetargetingTranslator.Retarget(namedTypeSymbol, RetargetOptions.RetargetPrimitiveTypesByName);
	}

	internal override void GetExtensionMethods(ArrayBuilder<MethodSymbol> methods, string nameOpt, int arity, LookupOptions options)
	{
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		_underlyingNamespace.GetExtensionMethods(instance, nameOpt, arity, options);
		foreach (MethodSymbol item in instance)
		{
			methods.Add(RetargetingTranslator.Retarget(item));
		}
		instance.Free();
	}
}
