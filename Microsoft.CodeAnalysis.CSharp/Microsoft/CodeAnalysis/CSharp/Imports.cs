using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class Imports
{
	private class UsingTargetComparer : IEqualityComparer<NamespaceOrTypeAndUsingDirective>
	{
		public static readonly IEqualityComparer<NamespaceOrTypeAndUsingDirective> Instance = new UsingTargetComparer();

		private UsingTargetComparer()
		{
		}

		bool IEqualityComparer<NamespaceOrTypeAndUsingDirective>.Equals(NamespaceOrTypeAndUsingDirective x, NamespaceOrTypeAndUsingDirective y)
		{
			return x.NamespaceOrType.Equals(y.NamespaceOrType);
		}

		int IEqualityComparer<NamespaceOrTypeAndUsingDirective>.GetHashCode(NamespaceOrTypeAndUsingDirective obj)
		{
			return obj.NamespaceOrType.GetHashCode();
		}
	}

	internal static readonly Imports Empty = new Imports(null, ImmutableDictionary<string, AliasAndUsingDirective>.Empty, ImmutableArray<NamespaceOrTypeAndUsingDirective>.Empty, ImmutableArray<AliasAndExternAliasDirective>.Empty, null);

	private readonly CSharpCompilation _compilation;

	private readonly DiagnosticBag _diagnostics;

	private SymbolCompletionState _state;

	public readonly ImmutableDictionary<string, AliasAndUsingDirective> UsingAliases;

	public readonly ImmutableArray<NamespaceOrTypeAndUsingDirective> Usings;

	public readonly ImmutableArray<AliasAndExternAliasDirective> ExternAliases;

	private Imports(CSharpCompilation compilation, ImmutableDictionary<string, AliasAndUsingDirective> usingAliases, ImmutableArray<NamespaceOrTypeAndUsingDirective> usings, ImmutableArray<AliasAndExternAliasDirective> externs, DiagnosticBag diagnostics)
	{
		_compilation = compilation;
		UsingAliases = usingAliases;
		Usings = usings;
		_diagnostics = diagnostics;
		ExternAliases = externs;
	}

	internal string GetDebuggerDisplay()
	{
		return string.Join("; ", (from ua in UsingAliases
			orderby ua.Value.UsingDirective.Location.SourceSpan.Start
			select $"{ua.Key} = {ua.Value.Alias.Target}").Concat(Usings.Select((NamespaceOrTypeAndUsingDirective u) => u.NamespaceOrType.ToString())).Concat(ExternAliases.Select((AliasAndExternAliasDirective ea) => "extern alias " + ea.Alias.Name)));
	}

	public static Imports FromSyntax(CSharpSyntaxNode declarationSyntax, InContainerBinder binder, ConsList<Symbol> basesBeingResolved, bool inUsing)
	{
		SyntaxList<UsingDirectiveSyntax> syntaxList;
		SyntaxList<ExternAliasDirectiveSyntax> externs;
		if (declarationSyntax.Kind() == SyntaxKind.CompilationUnit)
		{
			CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)declarationSyntax;
			syntaxList = (inUsing ? default(SyntaxList<UsingDirectiveSyntax>) : compilationUnitSyntax.Usings);
			externs = compilationUnitSyntax.Externs;
		}
		else
		{
			if (declarationSyntax.Kind() != SyntaxKind.NamespaceDeclaration)
			{
				return Empty;
			}
			NamespaceDeclarationSyntax namespaceDeclarationSyntax = (NamespaceDeclarationSyntax)declarationSyntax;
			syntaxList = (inUsing ? default(SyntaxList<UsingDirectiveSyntax>) : namespaceDeclarationSyntax.Usings);
			externs = namespaceDeclarationSyntax.Externs;
		}
		if (syntaxList.Count == 0 && externs.Count == 0)
		{
			return Empty;
		}
		DiagnosticBag diagnosticBag = new DiagnosticBag();
		CSharpCompilation compilation = binder.Compilation;
		ImmutableArray<AliasAndExternAliasDirective> externs2 = BuildExternAliases(externs, binder, diagnosticBag);
		ArrayBuilder<NamespaceOrTypeAndUsingDirective> instance = ArrayBuilder<NamespaceOrTypeAndUsingDirective>.GetInstance();
		ImmutableDictionary<string, AliasAndUsingDirective>.Builder builder = null;
		if (syntaxList.Count > 0)
		{
			Binder binder2;
			if (declarationSyntax.SyntaxTree.Options.Kind != SourceCodeKind.Regular)
			{
				binder2 = compilation.GetBinderFactory(declarationSyntax.SyntaxTree).GetImportsBinder(declarationSyntax, inUsing: true);
			}
			else
			{
				Imports imports = ((externs2.Length == 0) ? Empty : new Imports(compilation, ImmutableDictionary<string, AliasAndUsingDirective>.Empty, ImmutableArray<NamespaceOrTypeAndUsingDirective>.Empty, externs2, null));
				binder2 = new InContainerBinder(binder.Container, binder.Next, imports);
			}
			PooledHashSet<NamespaceOrTypeSymbol> instance2 = PooledHashSet<NamespaceOrTypeSymbol>.GetInstance();
			foreach (UsingDirectiveSyntax item in syntaxList)
			{
				compilation.RecordImport(item);
				if (item.Alias != null)
				{
					if (item.Alias.Name.Identifier.ContextualKind() == SyntaxKind.GlobalKeyword)
					{
						diagnosticBag.Add(ErrorCode.WRN_GlobalAliasDefn, item.Alias.Name.Location);
					}
					if (item.StaticKeyword != default(SyntaxToken))
					{
						diagnosticBag.Add(ErrorCode.ERR_NoAliasHere, item.Alias.Name.Location);
					}
					string valueText = item.Alias.Name.Identifier.ValueText;
					if (builder != null && builder.ContainsKey(valueText))
					{
						if (!item.Name.IsMissing)
						{
							diagnosticBag.Add(ErrorCode.ERR_DuplicateAlias, item.Alias.Name.Location, valueText);
						}
						continue;
					}
					foreach (AliasAndExternAliasDirective item2 in externs2)
					{
						if (item2.Alias.Name == valueText)
						{
							diagnosticBag.Add(ErrorCode.ERR_DuplicateAlias, item.Location, valueText);
							break;
						}
					}
					if (builder == null)
					{
						builder = ImmutableDictionary.CreateBuilder<string, AliasAndUsingDirective>();
					}
					builder.Add(valueText, new AliasAndUsingDirective(new AliasSymbol(binder2, item), item));
				}
				else
				{
					if (item.Name.IsMissing)
					{
						continue;
					}
					Binder binder3 = binder2.WithAdditionalFlags(BinderFlags.SuppressConstraintChecks);
					NamespaceOrTypeSymbol namespaceOrTypeSymbol = binder3.BindNamespaceOrTypeSymbol(item.Name, diagnosticBag, basesBeingResolved);
					if (namespaceOrTypeSymbol.Kind == SymbolKind.Namespace)
					{
						if (item.StaticKeyword != default(SyntaxToken))
						{
							diagnosticBag.Add(ErrorCode.ERR_BadUsingType, item.Name.Location, namespaceOrTypeSymbol);
						}
						else if (instance2.Contains(namespaceOrTypeSymbol))
						{
							diagnosticBag.Add(ErrorCode.WRN_DuplicateUsing, item.Name.Location, namespaceOrTypeSymbol);
						}
						else
						{
							instance2.Add(namespaceOrTypeSymbol);
							instance.Add(new NamespaceOrTypeAndUsingDirective(namespaceOrTypeSymbol, item));
						}
					}
					else if (namespaceOrTypeSymbol.Kind == SymbolKind.NamedType)
					{
						if (item.StaticKeyword == default(SyntaxToken))
						{
							diagnosticBag.Add(ErrorCode.ERR_BadUsingNamespace, item.Name.Location, namespaceOrTypeSymbol);
							continue;
						}
						NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)namespaceOrTypeSymbol;
						if (instance2.Contains(namedTypeSymbol))
						{
							diagnosticBag.Add(ErrorCode.WRN_DuplicateUsing, item.Name.Location, namedTypeSymbol);
						}
						else
						{
							binder3.ReportDiagnosticsIfObsolete(diagnosticBag, namedTypeSymbol, item.Name, hasBaseReceiver: false);
							instance2.Add(namedTypeSymbol);
							instance.Add(new NamespaceOrTypeAndUsingDirective(namedTypeSymbol, item));
						}
					}
					else if (namespaceOrTypeSymbol.Kind != SymbolKind.ErrorType)
					{
						diagnosticBag.Add(ErrorCode.ERR_BadSKknown, item.Name.Location, item.Name, namespaceOrTypeSymbol.GetKindText(), MessageID.IDS_SK_TYPE_OR_NAMESPACE.Localize());
					}
				}
			}
			instance2.Free();
		}
		if (diagnosticBag.IsEmptyWithoutResolution)
		{
			diagnosticBag = null;
		}
		return new Imports(compilation, builder.ToImmutableDictionaryOrEmpty(), instance.ToImmutableAndFree(), externs2, diagnosticBag);
	}

	public static Imports FromGlobalUsings(CSharpCompilation compilation)
	{
		ImmutableArray<string> usings = compilation.Options.Usings;
		if (usings.Length == 0 && compilation.PreviousSubmission == null)
		{
			return Empty;
		}
		DiagnosticBag diagnosticBag = new DiagnosticBag();
		InContainerBinder inContainerBinder = new InContainerBinder(compilation.GlobalNamespace, new BuckStopsHereBinder(compilation));
		ArrayBuilder<NamespaceOrTypeAndUsingDirective> instance = ArrayBuilder<NamespaceOrTypeAndUsingDirective>.GetInstance();
		PooledHashSet<NamespaceOrTypeSymbol> instance2 = PooledHashSet<NamespaceOrTypeSymbol>.GetInstance();
		foreach (string item in usings)
		{
			if (item.IsValidClrNamespaceName())
			{
				string[] array = item.Split(new char[1] { '.' });
				NameSyntax nameSyntax = SyntaxFactory.IdentifierName(array[0]);
				for (int i = 1; i < array.Length; i++)
				{
					nameSyntax = SyntaxFactory.QualifiedName(nameSyntax, SyntaxFactory.IdentifierName(array[i]));
				}
				NamespaceOrTypeSymbol namespaceOrTypeSymbol = inContainerBinder.BindNamespaceOrTypeSymbol(nameSyntax, diagnosticBag);
				if (instance2.Add(namespaceOrTypeSymbol))
				{
					instance.Add(new NamespaceOrTypeAndUsingDirective(namespaceOrTypeSymbol, null));
				}
			}
		}
		if (diagnosticBag.IsEmptyWithoutResolution)
		{
			diagnosticBag = null;
		}
		Imports imports = compilation.PreviousSubmission?.GlobalImports;
		if (imports != null)
		{
			foreach (NamespaceOrTypeAndUsingDirective @using in ExpandPreviousSubmissionImports(imports, compilation).Usings)
			{
				if (instance2.Add(@using.NamespaceOrType))
				{
					instance.Add(@using);
				}
			}
		}
		instance2.Free();
		if (instance.Count == 0)
		{
			instance.Free();
			return Empty;
		}
		return new Imports(compilation, ImmutableDictionary<string, AliasAndUsingDirective>.Empty, instance.ToImmutableAndFree(), ImmutableArray<AliasAndExternAliasDirective>.Empty, diagnosticBag);
	}

	internal static Imports ExpandPreviousSubmissionImports(Imports previousSubmissionImports, CSharpCompilation newSubmission)
	{
		if (previousSubmissionImports == Empty)
		{
			return Empty;
		}
		NamespaceSymbol globalNamespace = newSubmission.GlobalNamespace;
		ImmutableDictionary<string, AliasAndUsingDirective> usingAliases = ImmutableDictionary<string, AliasAndUsingDirective>.Empty;
		if (!previousSubmissionImports.UsingAliases.IsEmpty)
		{
			ImmutableDictionary<string, AliasAndUsingDirective>.Builder builder = ImmutableDictionary.CreateBuilder<string, AliasAndUsingDirective>();
			foreach (KeyValuePair<string, AliasAndUsingDirective> usingAlias in previousSubmissionImports.UsingAliases)
			{
				string key = usingAlias.Key;
				AliasAndUsingDirective value = usingAlias.Value;
				builder.Add(key, new AliasAndUsingDirective(value.Alias.ToNewSubmission(newSubmission), value.UsingDirective));
			}
			usingAliases = builder.ToImmutable();
		}
		ImmutableArray<NamespaceOrTypeAndUsingDirective> usings = ImmutableArray<NamespaceOrTypeAndUsingDirective>.Empty;
		if (!previousSubmissionImports.Usings.IsEmpty)
		{
			ArrayBuilder<NamespaceOrTypeAndUsingDirective> instance = ArrayBuilder<NamespaceOrTypeAndUsingDirective>.GetInstance(previousSubmissionImports.Usings.Length);
			foreach (NamespaceOrTypeAndUsingDirective @using in previousSubmissionImports.Usings)
			{
				NamespaceOrTypeSymbol namespaceOrType = @using.NamespaceOrType;
				if (namespaceOrType.IsType)
				{
					instance.Add(@using);
					continue;
				}
				NamespaceSymbol namespaceOrType2 = ExpandPreviousSubmissionNamespace((NamespaceSymbol)namespaceOrType, globalNamespace);
				instance.Add(new NamespaceOrTypeAndUsingDirective(namespaceOrType2, @using.UsingDirective));
			}
			usings = instance.ToImmutableAndFree();
		}
		return new Imports(newSubmission, usingAliases, usings, previousSubmissionImports.ExternAliases, null);
	}

	internal static NamespaceSymbol ExpandPreviousSubmissionNamespace(NamespaceSymbol originalNamespace, NamespaceSymbol expandedGlobalNamespace)
	{
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		NamespaceSymbol namespaceSymbol = originalNamespace;
		while (!namespaceSymbol.IsGlobalNamespace)
		{
			instance.Add(namespaceSymbol.Name);
			namespaceSymbol = namespaceSymbol.ContainingNamespace;
		}
		NamespaceSymbol namespaceSymbol2 = expandedGlobalNamespace;
		for (int num = instance.Count - 1; num >= 0; num--)
		{
			namespaceSymbol2 = namespaceSymbol2.GetMembers(instance[num]).OfType<NamespaceSymbol>().Single();
		}
		instance.Free();
		return namespaceSymbol2;
	}

	public static Imports FromCustomDebugInfo(CSharpCompilation compilation, ImmutableDictionary<string, AliasAndUsingDirective> usingAliases, ImmutableArray<NamespaceOrTypeAndUsingDirective> usings, ImmutableArray<AliasAndExternAliasDirective> externs)
	{
		return new Imports(compilation, usingAliases, usings, externs, null);
	}

	internal Imports Concat(Imports otherImports)
	{
		if (this == Empty)
		{
			return otherImports;
		}
		if (otherImports == Empty)
		{
			return this;
		}
		ImmutableDictionary<string, AliasAndUsingDirective> usingAliases = UsingAliases.SetItems(otherImports.UsingAliases);
		ImmutableArray<NamespaceOrTypeAndUsingDirective> usings = Usings.AddRange(otherImports.Usings).Distinct(UsingTargetComparer.Instance);
		ImmutableArray<AliasAndExternAliasDirective> externs = ConcatExternAliases(ExternAliases, otherImports.ExternAliases);
		return new Imports(_compilation, usingAliases, usings, externs, null);
	}

	private static ImmutableArray<AliasAndExternAliasDirective> ConcatExternAliases(ImmutableArray<AliasAndExternAliasDirective> externs1, ImmutableArray<AliasAndExternAliasDirective> externs2)
	{
		if (externs1.Length == 0)
		{
			return externs2;
		}
		if (externs2.Length == 0)
		{
			return externs1;
		}
		PooledHashSet<string> replacedExternAliases = PooledHashSet<string>.GetInstance();
		replacedExternAliases.AddAll(externs2.Select((AliasAndExternAliasDirective e) => e.Alias.Name));
		return externs1.WhereAsArray((AliasAndExternAliasDirective e) => !replacedExternAliases.Contains(e.Alias.Name)).AddRange(externs2);
	}

	private static ImmutableArray<AliasAndExternAliasDirective> BuildExternAliases(SyntaxList<ExternAliasDirectiveSyntax> syntaxList, InContainerBinder binder, DiagnosticBag diagnostics)
	{
		CSharpCompilation compilation = binder.Compilation;
		ArrayBuilder<AliasAndExternAliasDirective> instance = ArrayBuilder<AliasAndExternAliasDirective>.GetInstance();
		foreach (ExternAliasDirectiveSyntax item in syntaxList)
		{
			compilation.RecordImport(item);
			if (compilation.IsSubmission)
			{
				diagnostics.Add(ErrorCode.ERR_ExternAliasNotAllowed, item.Location);
				continue;
			}
			foreach (AliasAndExternAliasDirective item2 in instance)
			{
				if (item2.Alias.Name == item.Identifier.ValueText)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateAlias, item2.Alias.Locations[0], item2.Alias.Name);
					break;
				}
			}
			if (item.Identifier.ContextualKind() == SyntaxKind.GlobalKeyword)
			{
				diagnostics.Add(ErrorCode.ERR_GlobalExternAlias, item.Identifier.GetLocation());
			}
			instance.Add(new AliasAndExternAliasDirective(new AliasSymbol(binder, item), item));
		}
		return instance.ToImmutableAndFree();
	}

	private void MarkImportDirective(CSharpSyntaxNode directive, bool callerIsSemanticModel)
	{
		MarkImportDirective(_compilation, directive, callerIsSemanticModel);
	}

	private static void MarkImportDirective(CSharpCompilation compilation, CSharpSyntaxNode directive, bool callerIsSemanticModel)
	{
		if (directive != null && !callerIsSemanticModel)
		{
			compilation.MarkImportDirectiveAsUsed(directive);
		}
	}

	internal void Complete(CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = _state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.StartBaseType:
				if (_state.NotePartComplete(CompletionPart.StartBaseType))
				{
					Validate();
					_state.NotePartComplete(CompletionPart.FinishBaseType);
				}
				break;
			case CompletionPart.FinishBaseType:
				_state.SpinWaitComplete(CompletionPart.FinishBaseType, cancellationToken);
				break;
			case CompletionPart.None:
				return;
			default:
				_state.NotePartComplete(CompletionPart.MethodSymbolAll | CompletionPart.StartInterfaces | CompletionPart.FinishInterfaces | CompletionPart.EnumUnderlyingType | CompletionPart.TypeArguments | CompletionPart.FinishMemberChecks | CompletionPart.MembersCompleted);
				break;
			}
			_state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	private void Validate()
	{
		if (this == Empty)
		{
			return;
		}
		DiagnosticBag declarationDiagnostics = _compilation.DeclarationDiagnostics;
		string key;
		AliasAndUsingDirective value;
		foreach (KeyValuePair<string, AliasAndUsingDirective> usingAlias in UsingAliases)
		{
			KeyValuePairUtil.Deconstruct(usingAlias, out key, out value);
			AliasAndUsingDirective aliasAndUsingDirective = value;
			aliasAndUsingDirective.Alias.GetAliasTarget(null);
			declarationDiagnostics.AddRange(aliasAndUsingDirective.Alias.AliasTargetDiagnostics);
		}
		foreach (KeyValuePair<string, AliasAndUsingDirective> usingAlias2 in UsingAliases)
		{
			KeyValuePairUtil.Deconstruct(usingAlias2, out key, out value);
			value.Alias.CheckConstraints(declarationDiagnostics);
		}
		foreach (AliasAndExternAliasDirective externAlias in ExternAliases)
		{
			externAlias.Alias.GetAliasTarget(null);
			declarationDiagnostics.AddRange(externAlias.Alias.AliasTargetDiagnostics);
		}
		if (_diagnostics != null && !_diagnostics.IsEmptyWithoutResolution)
		{
			declarationDiagnostics.AddRange(_diagnostics.AsEnumerable());
		}
	}

	internal bool IsUsingAlias(string name, bool callerIsSemanticModel)
	{
		if (UsingAliases.TryGetValue(name, out var value))
		{
			MarkImportDirective(value.UsingDirective, callerIsSemanticModel);
			return true;
		}
		return false;
	}

	internal void LookupSymbol(Binder originalBinder, LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		LookupSymbolInAliases(originalBinder, result, name, arity, basesBeingResolved, options, diagnose, ref useSiteDiagnostics);
		if (!result.IsMultiViable && (options & LookupOptions.NamespaceAliasesOnly) == 0)
		{
			LookupSymbolInUsings(Usings, originalBinder, result, name, arity, basesBeingResolved, options, diagnose, ref useSiteDiagnostics);
		}
	}

	internal void LookupSymbolInAliases(Binder originalBinder, LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool isSemanticModelBinder = originalBinder.IsSemanticModelBinder;
		if (UsingAliases.TryGetValue(name, out var value))
		{
			SingleLookupResult result2 = originalBinder.CheckViability(value.Alias, arity, options, null, diagnose, ref useSiteDiagnostics, basesBeingResolved);
			if (result2.Kind == LookupResultKind.Viable)
			{
				MarkImportDirective(value.UsingDirective, isSemanticModelBinder);
			}
			result.MergeEqual(result2);
		}
		foreach (AliasAndExternAliasDirective externAlias in ExternAliases)
		{
			if (externAlias.Alias.Name == name)
			{
				SingleLookupResult result3 = originalBinder.CheckViability(externAlias.Alias, arity, options, null, diagnose, ref useSiteDiagnostics, basesBeingResolved);
				if (result3.Kind == LookupResultKind.Viable)
				{
					MarkImportDirective(externAlias.ExternAliasDirective, isSemanticModelBinder);
				}
				result.MergeEqual(result3);
			}
		}
	}

	internal static void LookupSymbolInUsings(ImmutableArray<NamespaceOrTypeAndUsingDirective> usings, Binder originalBinder, LookupResult result, string name, int arity, ConsList<Symbol> basesBeingResolved, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (originalBinder.Flags.Includes(BinderFlags.InScriptUsing))
		{
			return;
		}
		bool isSemanticModelBinder = originalBinder.IsSemanticModelBinder;
		foreach (NamespaceOrTypeAndUsingDirective item in usings)
		{
			foreach (Symbol candidateMember in Binder.GetCandidateMembers(item.NamespaceOrType, name, options, originalBinder))
			{
				if (IsValidLookupCandidateInUsings(candidateMember))
				{
					SingleLookupResult result2 = originalBinder.CheckViability(candidateMember, arity, options, null, diagnose, ref useSiteDiagnostics, basesBeingResolved);
					if (result2.Kind == LookupResultKind.Viable)
					{
						MarkImportDirective(originalBinder.Compilation, item.UsingDirective, isSemanticModelBinder);
					}
					result.MergeEqual(result2);
				}
			}
		}
	}

	private static bool IsValidLookupCandidateInUsings(Symbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Namespace:
			return false;
		case SymbolKind.Method:
			if (!symbol.IsStatic || ((MethodSymbol)symbol).IsExtensionMethod)
			{
				return false;
			}
			break;
		default:
			if (!symbol.IsStatic)
			{
				return false;
			}
			break;
		case SymbolKind.NamedType:
			break;
		}
		return true;
	}

	internal void LookupExtensionMethodsInUsings(ArrayBuilder<MethodSymbol> methods, string name, int arity, LookupOptions options, Binder originalBinder)
	{
		BinderFlags flags = originalBinder.Flags;
		if (flags.Includes(BinderFlags.InScriptUsing))
		{
			return;
		}
		bool callerIsSemanticModel = flags.Includes(BinderFlags.SemanticModel);
		bool flag = false;
		bool flag2 = false;
		foreach (NamespaceOrTypeAndUsingDirective @using in Usings)
		{
			switch (@using.NamespaceOrType.Kind)
			{
			case SymbolKind.Namespace:
			{
				int count2 = methods.Count;
				((NamespaceSymbol)@using.NamespaceOrType).GetExtensionMethods(methods, name, arity, options);
				if (methods.Count != count2)
				{
					MarkImportDirective(@using.UsingDirective, callerIsSemanticModel);
					flag = true;
				}
				break;
			}
			case SymbolKind.NamedType:
			{
				int count = methods.Count;
				((NamedTypeSymbol)@using.NamespaceOrType).GetExtensionMethods(methods, name, arity, options);
				if (methods.Count != count)
				{
					MarkImportDirective(@using.UsingDirective, callerIsSemanticModel);
					flag2 = true;
				}
				break;
			}
			}
		}
		if (flag & flag2)
		{
			methods.RemoveDuplicates();
		}
	}

	internal void AddLookupSymbolsInfo(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		AddLookupSymbolsInfoInAliases(result, options, originalBinder);
		LookupOptions options2 = (options & ~(LookupOptions.NamespaceAliasesOnly | LookupOptions.NamespacesOrTypesOnly)) | LookupOptions.MustNotBeNamespace;
		AddLookupSymbolsInfoInUsings(Usings, result, options2, originalBinder);
	}

	internal void AddLookupSymbolsInfoInAliases(LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		foreach (var (_, aliasAndUsingDirective2) in UsingAliases)
		{
			AddAliasSymbolToResult(result, aliasAndUsingDirective2.Alias, options, originalBinder);
		}
		foreach (AliasAndExternAliasDirective externAlias in ExternAliases)
		{
			AddAliasSymbolToResult(result, externAlias.Alias, options, originalBinder);
		}
	}

	private static void AddAliasSymbolToResult(LookupSymbolsInfo result, AliasSymbol aliasSymbol, LookupOptions options, Binder originalBinder)
	{
		NamespaceOrTypeSymbol aliasTarget = aliasSymbol.GetAliasTarget(null);
		if (originalBinder.CanAddLookupSymbolInfo(aliasTarget, options, result, null, aliasSymbol))
		{
			result.AddSymbol(aliasSymbol, aliasSymbol.Name, 0);
		}
	}

	private static void AddLookupSymbolsInfoInUsings(ImmutableArray<NamespaceOrTypeAndUsingDirective> usings, LookupSymbolsInfo result, LookupOptions options, Binder originalBinder)
	{
		if (originalBinder.Flags.Includes(BinderFlags.InScriptUsing))
		{
			return;
		}
		foreach (NamespaceOrTypeAndUsingDirective item in usings)
		{
			foreach (Symbol item2 in item.NamespaceOrType.GetMembersUnordered())
			{
				if (IsValidLookupCandidateInUsings(item2) && originalBinder.CanAddLookupSymbolInfo(item2, options, result, null))
				{
					result.AddSymbol(item2, item2.Name, item2.GetArity());
				}
			}
		}
	}
}
