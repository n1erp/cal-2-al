using System;
using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit;

internal sealed class AssemblyReference : IAssemblyReference, IModuleReference, IUnitReference, IReference, INamedEntity
{
	private readonly AssemblySymbol _targetAssembly;

	public AssemblyIdentity Identity => _targetAssembly.Identity;

	public Version AssemblyVersionPattern => _targetAssembly.AssemblyVersionPattern;

	string INamedEntity.Name => Identity.Name;

	internal AssemblyReference(AssemblySymbol assemblySymbol)
	{
		_targetAssembly = assemblySymbol;
	}

	public override string ToString()
	{
		return _targetAssembly.ToString();
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		visitor.Visit(this);
	}

	IAssemblyReference IModuleReference.GetContainingAssembly(EmitContext context)
	{
		return this;
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
