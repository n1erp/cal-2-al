using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Model;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

public interface IDotNetTypeMapper
{
	IDotNetTypeSymbol MapDeclaredType(IDotNetTypeDeclarationSymbol declaringSymbol, DotNetTypeInfo typeInfo);

	IDotNetTypeSymbol MapReferencedType(IModuleSymbol containingModule, DotNetTypeInfo typeInfo);

	ImmutableArray<ISymbol> MapTypeMembers(IDotNetTypeSymbol declaringSymbol);
}
