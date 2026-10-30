using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.CodeGen;

internal sealed class DummyLocal : LocalSymbol
{
	internal override bool IsImportedFromMetadata => false;

	internal override LocalDeclarationKind DeclarationKind => LocalDeclarationKind.None;

	internal override SynthesizedLocalKind SynthesizedKind => SynthesizedLocalKind.OptimizerTemp;

	internal override SyntaxNode ScopeDesignatorOpt => null;

	internal override SyntaxToken IdentifierToken => default;

	internal override bool IsPinned => false;

	public override Symbol ContainingSymbol
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override TypeSymbol Type
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override ImmutableArray<Location> Locations
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal override bool IsCompilerGenerated => true;

	public override RefKind RefKind => RefKind.None;

	internal override uint ValEscapeScope
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override uint RefEscapeScope
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override LocalSymbol WithSynthesizedLocalKindAndSyntax(SynthesizedLocalKind kind, SyntaxNode syntax)
	{
		throw new NotImplementedException();
	}

	internal override ConstantValue GetConstantValue(SyntaxNode node, LocalSymbol inProgress, DiagnosticBag diagnostics)
	{
		throw new NotImplementedException();
	}

	internal override ImmutableArray<Diagnostic> GetConstantValueDiagnostics(BoundExpression boundInitValue)
	{
		throw new NotImplementedException();
	}

	internal override SyntaxNode GetDeclaratorSyntax()
	{
		throw new NotImplementedException();
	}
}
