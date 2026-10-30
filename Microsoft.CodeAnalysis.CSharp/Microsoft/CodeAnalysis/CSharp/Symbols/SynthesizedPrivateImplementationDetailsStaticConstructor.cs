using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedPrivateImplementationDetailsStaticConstructor : SynthesizedGlobalMethodSymbol
{
	public override MethodKind MethodKind => MethodKind.StaticConstructor;

	internal override bool HasSpecialName => true;

	internal SynthesizedPrivateImplementationDetailsStaticConstructor(SourceModuleSymbol containingModule, PrivateImplementationDetails privateImplementationType, NamedTypeSymbol voidType)
		: base(containingModule, privateImplementationType, voidType, ".cctor")
	{
		SetParameters(ImmutableArray<ParameterSymbol>.Empty);
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		CSharpSyntaxNode nonNullSyntaxNode = this.GetNonNullSyntaxNode();
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, nonNullSyntaxNode, compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		try
		{
			foreach (KeyValuePair<int, InstrumentationPayloadRootField> instrumentationPayloadRoot in ContainingPrivateImplementationDetailsType.GetInstrumentationPayloadRoots())
			{
				int key = instrumentationPayloadRoot.Key;
				ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)instrumentationPayloadRoot.Value.Type;
				BoundStatement item = syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.InstrumentationPayloadRoot(key, arrayTypeSymbol), syntheticBoundNodeFactory.Array(arrayTypeSymbol.ElementType, syntheticBoundNodeFactory.Binary(BinaryOperatorKind.Addition, syntheticBoundNodeFactory.SpecialType(SpecialType.System_Int32), syntheticBoundNodeFactory.MaximumMethodDefIndex(), syntheticBoundNodeFactory.Literal(1))));
				instance.Add(item);
			}
			instance.Add(syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.ModuleVersionId(), syntheticBoundNodeFactory.New(syntheticBoundNodeFactory.WellKnownMethod(WellKnownMember.System_Guid__ctor), syntheticBoundNodeFactory.ModuleVersionIdString())));
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
		}
		BoundStatement item2 = syntheticBoundNodeFactory.Return();
		instance.Add(item2);
		syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.Block(instance.ToImmutableAndFree()));
	}
}
