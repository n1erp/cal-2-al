using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

internal class NullDotNetResolver : IDotNetResolver, IDotNetTypeLoader, IDisposable, IDotNetTypeMapper
{
	public static IDotNetResolver Instance { get; } = new NullDotNetResolver();

	public IAssemblyLocator AssemblyLocator { get; } = new NullAssemblyLocator();

	public void Dispose()
	{
	}

	public DotNetAssemblyInfo LoadAssembly(string fullAssemblyName)
	{
		return null;
	}

	public DotNetTypeInfo LoadType(DotNetAssemblyInfo assemblyInfo, string fullTypeName)
	{
		return null;
	}

	public IDotNetTypeSymbol MapDeclaredType(IDotNetTypeDeclarationSymbol declaringSymbol, DotNetTypeInfo typeInfo)
	{
		throw new NotImplementedException();
	}

	public IDotNetTypeSymbol MapReferencedType(IModuleSymbol containingModule, DotNetTypeInfo typeInfo)
	{
		throw new NotImplementedException();
	}

	public ImmutableArray<ISymbol> MapTypeMembers(IDotNetTypeSymbol declaringSymbol)
	{
		throw new NotImplementedException();
	}
}
