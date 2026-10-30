using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

internal class DotNetResolver : IDotNetResolver, IDotNetTypeLoader, IDisposable, IDotNetTypeMapper
{
	private readonly IDotNetTypeLoader typeLoader;

	private readonly IDotNetTypeMapper typeMapper;

	public IAssemblyLocator AssemblyLocator => typeLoader.AssemblyLocator;

	public DotNetResolver(IDotNetTypeLoader typeLoader, IDotNetTypeMapper typeMapper)
	{
		this.typeLoader = typeLoader;
		this.typeMapper = typeMapper;
	}

	public void Dispose()
	{
		typeLoader?.Dispose();
	}

	public DotNetAssemblyInfo LoadAssembly(string fullAssemblyName)
	{
		return typeLoader.LoadAssembly(fullAssemblyName);
	}

	public DotNetTypeInfo LoadType(DotNetAssemblyInfo assemblyInfo, string fullTypeName)
	{
		return typeLoader.LoadType(assemblyInfo, fullTypeName);
	}

	public IDotNetTypeSymbol MapDeclaredType(IDotNetTypeDeclarationSymbol declaringSymbol, DotNetTypeInfo typeInfo)
	{
		return typeMapper.MapDeclaredType(declaringSymbol, typeInfo);
	}

	public IDotNetTypeSymbol MapReferencedType(IModuleSymbol containingModule, DotNetTypeInfo typeInfo)
	{
		return typeMapper.MapReferencedType(containingModule, typeInfo);
	}

	public ImmutableArray<ISymbol> MapTypeMembers(IDotNetTypeSymbol declaringSymbol)
	{
		return typeMapper.MapTypeMembers(declaringSymbol);
	}
}
