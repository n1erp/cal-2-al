using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.Cci;

internal interface IAssemblyReference : IModuleReference, IUnitReference, IReference, INamedEntity
{
	AssemblyIdentity Identity { get; }

	Version AssemblyVersionPattern { get; }
}
