using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SynthesizedClosureMethod : SynthesizedMethodBaseSymbol, ISynthesizedMethodBodyImplementationSymbol
{
	private readonly MethodSymbol _topLevelMethod;

	private readonly ImmutableArray<NamedTypeSymbol> _structEnvironments;

	internal readonly DebugId LambdaId;

	protected override ImmutableArray<ParameterSymbol> BaseMethodParameters => BaseMethod.Parameters;

	protected override ImmutableArray<TypeSymbol> ExtraSynthesizedRefParameters => ImmutableArray<TypeSymbol>.CastUp(_structEnvironments);

	internal int ExtraSynthesizedParameterCount
	{
		get
		{
			if (!_structEnvironments.IsDefault)
			{
				return _structEnvironments.Length;
			}
			return 0;
		}
	}

	internal override bool GenerateDebugInfo => !IsAsync;

	internal override bool IsExpressionBodied => false;

	internal MethodSymbol TopLevelMethod => _topLevelMethod;

	IMethodSymbol ISynthesizedMethodBodyImplementationSymbol.Method => _topLevelMethod;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => true;

	public ClosureKind ClosureKind { get; }

	internal SynthesizedClosureMethod(NamedTypeSymbol containingType, ImmutableArray<SynthesizedClosureEnvironment> structEnvironments, ClosureKind closureKind, MethodSymbol topLevelMethod, DebugId topLevelMethodId, MethodSymbol originalMethod, SyntaxReference blockSyntax, DebugId lambdaId)
		: base(containingType, originalMethod, blockSyntax, originalMethod.DeclaringSyntaxReferences[0].GetLocation(), (originalMethod is LocalFunctionSymbol) ? MakeName(topLevelMethod.Name, originalMethod.Name, topLevelMethodId, closureKind, lambdaId) : MakeName(topLevelMethod.Name, topLevelMethodId, closureKind, lambdaId), MakeDeclarationModifiers(closureKind, originalMethod))
	{
		_topLevelMethod = topLevelMethod;
		ClosureKind = closureKind;
		LambdaId = lambdaId;
		SynthesizedClosureEnvironment synthesizedClosureEnvironment = ContainingType as SynthesizedClosureEnvironment;
		TypeMap typeMap;
		ImmutableArray<TypeParameterSymbol> newTypeParameters;
		ImmutableArray<TypeParameterSymbol> oldTypeParameters;
		switch (closureKind)
		{
		case ClosureKind.Singleton:
		case ClosureKind.General:
			typeMap = synthesizedClosureEnvironment.TypeMap.WithConcatAlphaRename(originalMethod, this, out newTypeParameters, out oldTypeParameters, synthesizedClosureEnvironment.OriginalContainingMethodOpt);
			break;
		case ClosureKind.Static:
		case ClosureKind.ThisOnly:
			typeMap = TypeMap.Empty.WithConcatAlphaRename(originalMethod, this, out newTypeParameters, out oldTypeParameters);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(closureKind);
		}
		if (!structEnvironments.IsDefaultOrEmpty && newTypeParameters.Length != 0)
		{
			ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
			foreach (SynthesizedClosureEnvironment item2 in structEnvironments)
			{
				NamedTypeSymbol item;
				if (item2.Arity == 0)
				{
					item = item2;
				}
				else
				{
					ImmutableArray<TypeParameterSymbol> constructedFromTypeParameters = item2.ConstructedFromTypeParameters;
					ImmutableArray<TypeParameterSymbol> immutableArray = typeMap.SubstituteTypeParameters(constructedFromTypeParameters);
					item = item2.Construct(immutableArray);
				}
				instance.Add(item);
			}
			_structEnvironments = instance.ToImmutableAndFree();
		}
		else
		{
			_structEnvironments = ImmutableArray<NamedTypeSymbol>.CastUp(structEnvironments);
		}
		AssignTypeMapAndTypeParameters(typeMap, newTypeParameters);
	}

	private static DeclarationModifiers MakeDeclarationModifiers(ClosureKind closureKind, MethodSymbol originalMethod)
	{
		DeclarationModifiers declarationModifiers = ((closureKind == ClosureKind.ThisOnly) ? DeclarationModifiers.Private : DeclarationModifiers.Internal);
		if (closureKind == ClosureKind.Static)
		{
			declarationModifiers |= DeclarationModifiers.Static;
		}
		if (originalMethod.IsAsync)
		{
			declarationModifiers |= DeclarationModifiers.Async;
		}
		return declarationModifiers;
	}

	private static string MakeName(string topLevelMethodName, string localFunctionName, DebugId topLevelMethodId, ClosureKind closureKind, DebugId lambdaId)
	{
		return GeneratedNames.MakeLocalFunctionName(topLevelMethodName, localFunctionName, (closureKind == ClosureKind.General) ? (-1) : topLevelMethodId.Ordinal, topLevelMethodId.Generation, lambdaId.Ordinal, lambdaId.Generation);
	}

	private static string MakeName(string topLevelMethodName, DebugId topLevelMethodId, ClosureKind closureKind, DebugId lambdaId)
	{
		return GeneratedNames.MakeLambdaMethodName(topLevelMethodName, (closureKind == ClosureKind.General) ? (-1) : topLevelMethodId.Ordinal, topLevelMethodId.Generation, lambdaId.Ordinal, lambdaId.Generation);
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		return _topLevelMethod.CalculateLocalSyntaxOffset(localPosition, localTree);
	}
}
