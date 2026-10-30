using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class BuiltInMethodTypeSymbol : MethodSymbol
{
	private readonly Lazy<ReturnValueSymbol> lazyReturnValue;

	private readonly ReturnValueSymbol eagerReturnValue;

	public override MethodKind MethodKind => MethodKind.BuiltInMethod;

	public override string Name { get; }

	public override string MetadataName { get; }

	public override bool IsStatic { get; }

	public bool IsProperty { get; }

	public bool AddDataError { get; }

	public bool AddCompilationTarget { get; }

	public bool AddThis { get; }

	public override bool IsLocal { get; }

	public override bool IsDeprecated { get; }

	public override Symbol ContainingSymbol { get; }

	public bool SuppressTarget { get; }

	public override TypeSymbol ContainingType => (TypeSymbol)(((object)(ContainingSymbol as LanguageClassTypeSymbol)) ?? ((object)(ContainingSymbol as CodeunitTypeSymbol)));

	public override Accessibility DeclaredAccessibility
	{
		get
		{
			if (!IsLocal)
			{
				return Accessibility.Public;
			}
			return Accessibility.Protected;
		}
	}

	public override ReturnValueSymbol ReturnValue => eagerReturnValue ?? lazyReturnValue.Value;

	public override ImmutableArray<ParameterSymbol> Parameters { get; }

	public override ImmutableArray<AttributeSymbol> Attributes => ImmutableArray<AttributeSymbol>.Empty;

	public override ImmutableArray<VariableSymbol> LocalVariables => ImmutableArray<VariableSymbol>.Empty;

	public override SymbolKind Kind => SymbolKind.Method;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public string StaticClassName { get; }

	public override SymbolCompilationScope CompilationScope { get; }

	public override VersionCompatibility Compatibility { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitBuiltInMethodType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitBuiltInMethodType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitBuiltInMethodType(this, a);
	}

	public BuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, bool addCompilationTarget = false, VersionCompatibility compatibility = null, bool suppressTarget = false)
		: this(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, getParameters, addCompilationTarget, compatibility, suppressTarget)
	{
		eagerReturnValue = ((returnValue != null) ? new BuiltInReturnValueSymbol(returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument) : Microsoft.Dynamics.Nav.CodeAnalysis.Symbols.ReturnValueSymbol.None);
	}

	public BuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, Func<TypeSymbol> returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, bool addCompilationTarget = false, VersionCompatibility compatibility = null)
		: this(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, getParameters, addCompilationTarget, compatibility)
	{
		if (returnValue != null)
		{
			lazyReturnValue = new Lazy<ReturnValueSymbol>(() => new BuiltInReturnValueSymbol(returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument));
		}
		else
		{
			lazyReturnValue = new Lazy<ReturnValueSymbol>(() => Microsoft.Dynamics.Nav.CodeAnalysis.Symbols.ReturnValueSymbol.None);
		}
	}

	private BuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, Func<Symbol, ParameterSymbol[]> getParameters = null, bool addCompilationTarget = false, VersionCompatibility compatibility = null, bool suppressTarget = false)
	{
		ContainingSymbol = containingSymbol;
		Name = name;
		MetadataName = metadataName ?? ("AL" + name);
		AddCompilationTarget = addCompilationTarget;
		CompilationScope = compilationScope;
		IsStatic = isStatic;
		StaticClassName = staticClassName;
		IsProperty = isProperty;
		AddDataError = addDataError;
		AddThis = addThis;
		IsLocal = isLocal;
		IsDeprecated = isObsolete;
		Parameters = getParameters?.Invoke(this).AsImmutableOrEmpty() ?? ImmutableArray<ParameterSymbol>.Empty;
		Compatibility = compatibility;
		SuppressTarget = suppressTarget;
	}

	public override string GetDocumentation()
	{
		return DocumentationFacts.GetDocumentation(this);
	}
}
