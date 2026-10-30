using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ConstantFieldsInProgress
{
	private readonly SourceFieldSymbol _fieldOpt;

	private readonly HashSet<SourceFieldSymbolWithSyntaxReference> _dependencies;

	internal static readonly ConstantFieldsInProgress Empty = new ConstantFieldsInProgress(null, null);

	public bool IsEmpty => (object)_fieldOpt == null;

	internal ConstantFieldsInProgress(SourceFieldSymbol fieldOpt, HashSet<SourceFieldSymbolWithSyntaxReference> dependencies)
	{
		_fieldOpt = fieldOpt;
		_dependencies = dependencies;
	}

	internal void AddDependency(SourceFieldSymbolWithSyntaxReference field)
	{
		_dependencies.Add(field);
	}
}
