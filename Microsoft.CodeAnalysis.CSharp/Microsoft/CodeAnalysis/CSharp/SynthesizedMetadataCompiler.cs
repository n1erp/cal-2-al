using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SynthesizedMetadataCompiler : CSharpSymbolVisitor
{
	private readonly PEModuleBuilder _moduleBeingBuilt;

	private readonly CancellationToken _cancellationToken;

	private SynthesizedMetadataCompiler(PEModuleBuilder moduleBeingBuilt, CancellationToken cancellationToken)
	{
		_moduleBeingBuilt = moduleBeingBuilt;
		_cancellationToken = cancellationToken;
	}

	public static void ProcessSynthesizedMembers(CSharpCompilation compilation, PEModuleBuilder moduleBeingBuilt, CancellationToken cancellationToken)
	{
		new SynthesizedMetadataCompiler(moduleBeingBuilt, cancellationToken).Visit(compilation.SourceModule.GlobalNamespace);
	}

	public override void VisitNamespace(NamespaceSymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		foreach (Symbol member in symbol.GetMembers())
		{
			member.Accept(this);
		}
	}

	public override void VisitNamedType(NamedTypeSymbol symbol)
	{
		CancellationToken cancellationToken = _cancellationToken;
		cancellationToken.ThrowIfCancellationRequested();
		if (symbol is SourceMemberContainerTypeSymbol sourceMemberContainerTypeSymbol && _moduleBeingBuilt != null)
		{
			foreach (SynthesizedExplicitImplementationForwardingMethod synthesizedExplicitImplementation in sourceMemberContainerTypeSymbol.GetSynthesizedExplicitImplementations(_cancellationToken))
			{
				_moduleBeingBuilt.AddSynthesizedDefinition(symbol, synthesizedExplicitImplementation);
			}
		}
		foreach (Symbol member in symbol.GetMembers())
		{
			SymbolKind kind = member.Kind;
			if (kind == SymbolKind.NamedType || kind == SymbolKind.Property)
			{
				member.Accept(this);
			}
		}
	}

	public override void VisitProperty(PropertySymbol symbol)
	{
		if (symbol is SourcePropertySymbol { IsSealed: not false, SynthesizedSealedAccessorOpt: { } synthesizedSealedAccessorOpt } sourcePropertySymbol)
		{
			_moduleBeingBuilt.AddSynthesizedDefinition(sourcePropertySymbol.ContainingType, synthesizedSealedAccessorOpt);
		}
	}
}
