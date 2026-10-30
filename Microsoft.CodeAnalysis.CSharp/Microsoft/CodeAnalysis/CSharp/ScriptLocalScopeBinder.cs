using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class ScriptLocalScopeBinder : LocalScopeBinder
{
	internal new sealed class Labels
	{
		private readonly SynthesizedInteractiveInitializerMethod _scriptInitializer;

		private readonly CompilationUnitSyntax _syntax;

		private ImmutableArray<LabelSymbol> _lazyLabels;

		internal SynthesizedInteractiveInitializerMethod ScriptInitializer => _scriptInitializer;

		internal Labels(SynthesizedInteractiveInitializerMethod scriptInitializer, CompilationUnitSyntax syntax)
		{
			_scriptInitializer = scriptInitializer;
			_syntax = syntax;
		}

		internal ImmutableArray<LabelSymbol> GetLabels()
		{
			if (_lazyLabels == null)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyLabels, GetLabels(_scriptInitializer, _syntax));
			}
			return _lazyLabels;
		}

		private static ImmutableArray<LabelSymbol> GetLabels(SynthesizedInteractiveInitializerMethod scriptInitializer, CompilationUnitSyntax syntax)
		{
			ArrayBuilder<LabelSymbol> labels = ArrayBuilder<LabelSymbol>.GetInstance();
			foreach (MemberDeclarationSyntax member in syntax.Members)
			{
				if (member.Kind() == SyntaxKind.GlobalStatement)
				{
					LocalScopeBinder.BuildLabels(scriptInitializer, ((GlobalStatementSyntax)member).Statement, ref labels);
				}
			}
			return labels.ToImmutableAndFree();
		}
	}

	private readonly Labels _labels;

	internal override Symbol ContainingMemberOrLambda => _labels.ScriptInitializer;

	internal override bool IsLabelsScopeBinder => true;

	internal ScriptLocalScopeBinder(Labels labels, Binder next)
		: base(next)
	{
		_labels = labels;
	}

	protected override ImmutableArray<LabelSymbol> BuildLabels()
	{
		return _labels.GetLabels();
	}

	internal override ImmutableArray<LocalSymbol> GetDeclaredLocalsForScope(SyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal override ImmutableArray<LocalFunctionSymbol> GetDeclaredLocalFunctionsForScope(CSharpSyntaxNode scopeDesignator)
	{
		throw ExceptionUtilities.Unreachable;
	}
}
