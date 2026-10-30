using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Emit.NoPia;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Emit.NoPia;

internal sealed class EmbeddedType : EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>.CommonEmbeddedType
{
	private bool _embeddedAllMembersOfImplementedInterface;

	protected override bool IsPublic => UnderlyingNamedType.DeclaredAccessibility == Accessibility.Public;

	protected override bool IsAbstract => UnderlyingNamedType.IsMetadataAbstract;

	protected override bool IsBeforeFieldInit
	{
		get
		{
			switch (UnderlyingNamedType.TypeKind)
			{
			case TypeKind.Delegate:
			case TypeKind.Enum:
			case TypeKind.Interface:
				return false;
			default:
				return true;
			}
		}
	}

	protected override bool IsComImport => UnderlyingNamedType.IsComImport;

	protected override bool IsInterface => UnderlyingNamedType.IsInterfaceType();

	protected override bool IsSerializable => UnderlyingNamedType.IsSerializable;

	protected override bool IsSpecialName => UnderlyingNamedType.HasSpecialName;

	protected override bool IsWindowsRuntimeImport => UnderlyingNamedType.IsWindowsRuntimeImport;

	protected override bool IsSealed => UnderlyingNamedType.IsMetadataSealed;

	protected override CharSet StringFormat => UnderlyingNamedType.MarshallingCharSet;

	public EmbeddedType(EmbeddedTypesManager typeManager, NamedTypeSymbol underlyingNamedType)
		: base(typeManager, underlyingNamedType)
	{
	}

	public void EmbedAllMembersOfImplementedInterface(SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		if (_embeddedAllMembersOfImplementedInterface)
		{
			return;
		}
		_embeddedAllMembersOfImplementedInterface = true;
		foreach (MethodSymbol item in UnderlyingNamedType.GetMethodsToEmit())
		{
			if ((object)item != null)
			{
				TypeManager.EmbedMethod(this, item, syntaxNodeOpt, diagnostics);
			}
		}
		foreach (NamedTypeSymbol item2 in UnderlyingNamedType.GetInterfacesToEmit())
		{
			TypeManager.ModuleBeingBuilt.Translate(item2, syntaxNodeOpt, diagnostics, fromImplements: true);
		}
	}

	protected override int GetAssemblyRefIndex()
	{
		return TypeManager.ModuleBeingBuilt.SourceModule.GetReferencedAssemblySymbols().IndexOf(UnderlyingNamedType.ContainingAssembly, Roslyn.Utilities.ReferenceEqualityComparer.Instance);
	}

	protected override ITypeReference GetBaseClass(PEModuleBuilder moduleBuilder, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = UnderlyingNamedType.BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics == null)
		{
			return null;
		}
		return moduleBuilder.Translate(baseTypeNoUseSiteDiagnostics, syntaxNodeOpt, diagnostics);
	}

	protected override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		return UnderlyingNamedType.GetFieldsToEmit();
	}

	protected override IEnumerable<MethodSymbol> GetMethodsToEmit()
	{
		return UnderlyingNamedType.GetMethodsToEmit();
	}

	protected override IEnumerable<EventSymbol> GetEventsToEmit()
	{
		return UnderlyingNamedType.GetEventsToEmit();
	}

	protected override IEnumerable<PropertySymbol> GetPropertiesToEmit()
	{
		return UnderlyingNamedType.GetPropertiesToEmit();
	}

	protected override IEnumerable<TypeReferenceWithAttributes> GetInterfaces(EmitContext context)
	{
		PEModuleBuilder moduleBeingBuilt = (PEModuleBuilder)context.Module;
		foreach (NamedTypeSymbol item in UnderlyingNamedType.GetInterfacesToEmit())
		{
			INamedTypeReference typeRef = moduleBeingBuilt.Translate(item, (CSharpSyntaxNode)context.SyntaxNodeOpt, context.Diagnostics);
			yield return item.GetTypeRefWithAttributes(UnderlyingNamedType.DeclaringCompilation, typeRef);
		}
	}

	protected override TypeLayout? GetTypeLayoutIfStruct()
	{
		if (UnderlyingNamedType.IsStructType())
		{
			return UnderlyingNamedType.Layout;
		}
		return null;
	}

	protected override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return UnderlyingNamedType.GetCustomAttributesToEmit(moduleBuilder);
	}

	protected override CSharpAttributeData CreateTypeIdentifierAttribute(bool hasGuid, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		WellKnownMember method = (hasGuid ? WellKnownMember.System_Runtime_InteropServices_TypeIdentifierAttribute__ctor : WellKnownMember.System_Runtime_InteropServices_TypeIdentifierAttribute__ctorStringString);
		MethodSymbol wellKnownMethod = TypeManager.GetWellKnownMethod(method, syntaxNodeOpt, diagnostics);
		if ((object)wellKnownMethod == null)
		{
			return null;
		}
		if (hasGuid)
		{
			return new SynthesizedAttributeData(wellKnownMethod, ImmutableArray<TypedConstant>.Empty, ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty);
		}
		NamedTypeSymbol systemStringType = TypeManager.GetSystemStringType(syntaxNodeOpt, diagnostics);
		if ((object)systemStringType != null)
		{
			string assemblyGuidString = TypeManager.GetAssemblyGuidString(UnderlyingNamedType.ContainingAssembly);
			return new SynthesizedAttributeData(wellKnownMethod, ImmutableArray.Create(new TypedConstant(systemStringType, TypedConstantKind.Primitive, assemblyGuidString), new TypedConstant(systemStringType, TypedConstantKind.Primitive, UnderlyingNamedType.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat))), ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty);
		}
		return null;
	}

	protected override void ReportMissingAttribute(AttributeDescription description, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		EmbeddedTypesManager.Error(diagnostics, ErrorCode.ERR_InteropTypeMissingAttribute, syntaxNodeOpt, UnderlyingNamedType, description.FullName);
	}

	protected override void EmbedDefaultMembers(string defaultMember, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		foreach (Symbol member in UnderlyingNamedType.GetMembers(defaultMember))
		{
			switch (member.Kind)
			{
			case SymbolKind.Field:
				TypeManager.EmbedField(this, (FieldSymbol)member, syntaxNodeOpt, diagnostics);
				break;
			case SymbolKind.Method:
				TypeManager.EmbedMethod(this, (MethodSymbol)member, syntaxNodeOpt, diagnostics);
				break;
			case SymbolKind.Property:
				TypeManager.EmbedProperty(this, (PropertySymbol)member, syntaxNodeOpt, diagnostics);
				break;
			case SymbolKind.Event:
				TypeManager.EmbedEvent(this, (EventSymbol)member, syntaxNodeOpt, diagnostics, isUsedForComAwareEventBinding: false);
				break;
			}
		}
	}
}
