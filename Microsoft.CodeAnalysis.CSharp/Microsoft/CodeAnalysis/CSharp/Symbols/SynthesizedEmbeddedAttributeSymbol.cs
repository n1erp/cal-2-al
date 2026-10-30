using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedEmbeddedAttributeSymbol : NamedTypeSymbol
{
	internal sealed class SynthesizedEmbeddedAttributeConstructorSymbol : SynthesizedInstanceConstructor
	{
		public SynthesizedEmbeddedAttributeConstructorSymbol(NamedTypeSymbol containingType)
			: base(containingType)
		{
		}

		internal override void GenerateMethodBody(TypeCompilationState compilationState, DiagnosticBag diagnostics)
		{
			if (!(ContainingType.BaseTypeNoUseSiteDiagnostics is MissingMetadataTypeSymbol))
			{
				SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
				syntheticBoundNodeFactory.CurrentFunction = this;
				BoundCall boundCall = MethodCompiler.GenerateBaseParameterlessConstructorInitializer(this, diagnostics);
				if (boundCall != null)
				{
					BoundBlock body = syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.ExpressionStatement(boundCall), syntheticBoundNodeFactory.Return());
					syntheticBoundNodeFactory.CloseMethod(body);
				}
			}
		}
	}

	private readonly string _name;

	private readonly NamedTypeSymbol _baseType;

	private readonly NamespaceSymbol _namespace;

	private readonly ImmutableArray<Symbol> _members;

	private readonly ModuleSymbol _module;

	public SynthesizedEmbeddedAttributeConstructorSymbol Constructor { get; private set; }

	public override int Arity => 0;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override NamedTypeSymbol ConstructedFrom => this;

	public override bool MightContainExtensionMethods => false;

	public override string Name => _name;

	public override IEnumerable<string> MemberNames => SpecializedCollections.SingletonEnumerable(Constructor.Name);

	public override Accessibility DeclaredAccessibility => Accessibility.Internal;

	public override TypeKind TypeKind => TypeKind.Class;

	public override Symbol ContainingSymbol => _namespace;

	internal override ModuleSymbol ContainingModule => _module;

	public override AssemblySymbol ContainingAssembly => _module.ContainingAssembly;

	public override NamespaceSymbol ContainingNamespace => _namespace;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override bool IsStatic => false;

	internal override bool IsByRefLikeType => false;

	internal override bool IsReadOnly => false;

	public override bool IsAbstract => false;

	public override bool IsSealed => true;

	internal override bool HasTypeArgumentsCustomModifiers => false;

	internal override ImmutableArray<TypeSymbol> TypeArgumentsNoUseSiteDiagnostics => ImmutableArray<TypeSymbol>.Empty;

	internal override bool MangleName => false;

	internal override bool HasCodeAnalysisEmbeddedAttribute => true;

	internal override bool HasSpecialName => false;

	internal override bool IsComImport => false;

	internal override bool IsWindowsRuntimeImport => false;

	internal override bool ShouldAddWinRTMembers => false;

	public override bool IsSerializable => false;

	internal override TypeLayout Layout => default;

	internal override CharSet MarshallingCharSet => DefaultMarshallingCharSet;

	internal override bool HasDeclarativeSecurity => false;

	internal override bool IsInterface => false;

	internal override NamedTypeSymbol BaseTypeNoUseSiteDiagnostics => _baseType;

	internal override ObsoleteAttributeData ObsoleteAttributeData => null;

	public SynthesizedEmbeddedAttributeSymbol(AttributeDescription description, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		_name = description.Name;
		_baseType = compilation.GetWellKnownType(WellKnownType.System_Attribute);
		Binder.ReportUseSiteDiagnostics(_baseType, diagnostics, Location.None);
		Constructor = new SynthesizedEmbeddedAttributeConstructorSymbol(this);
		_members = ImmutableArray.Create((Symbol)Constructor);
		_module = compilation.SourceModule;
		_namespace = compilation.SourceModule.GlobalNamespace;
		string[] array = description.Namespace.Split(new char[1] { '.' });
		foreach (string name in array)
		{
			_namespace = new MissingNamespaceSymbol(_namespace, name);
		}
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return _members;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		if (!(Constructor.Name == name))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return _members;
	}

	public override ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal)
	{
		return ImmutableArray<CustomModifier>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override AttributeUsageInfo GetAttributeUsageInfo()
	{
		return AttributeUsageInfo.Default;
	}

	internal override NamedTypeSymbol GetDeclaredBaseType(ConsList<Symbol> basesBeingResolved)
	{
		return _baseType;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetDeclaredInterfaces(ConsList<Symbol> basesBeingResolved)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return GetMembers();
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		return GetMembers(name);
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		return SpecializedCollections.EmptyEnumerable<FieldSymbol>();
	}

	internal override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		return null;
	}

	internal override ImmutableArray<NamedTypeSymbol> InterfacesNoUseSiteDiagnostics(ConsList<Symbol> basesBeingResolved = null)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.Compilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		Symbol.AddSynthesizedAttribute(ref attributes, moduleBuilder.SynthesizeEmbeddedAttribute());
	}
}
