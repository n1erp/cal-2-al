using System.Collections.Generic;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Emit.NoPia;

namespace Microsoft.CodeAnalysis.CSharp.Emit.NoPia;

internal sealed class EmbeddedEvent : EmbeddedTypesManager<PEModuleBuilder, ModuleCompilationState, EmbeddedTypesManager, SyntaxNode, CSharpAttributeData, Symbol, AssemblySymbol, NamedTypeSymbol, FieldSymbol, MethodSymbol, EventSymbol, PropertySymbol, ParameterSymbol, TypeParameterSymbol, EmbeddedType, EmbeddedField, EmbeddedMethod, EmbeddedEvent, EmbeddedProperty, EmbeddedParameter, EmbeddedTypeParameter>.CommonEmbeddedEvent
{
	protected override bool IsRuntimeSpecial => UnderlyingEvent.HasRuntimeSpecialName;

	protected override bool IsSpecialName => UnderlyingEvent.HasSpecialName;

	protected override EmbeddedType ContainingType => AnAccessor.ContainingType;

	protected override TypeMemberVisibility Visibility => PEModuleBuilder.MemberVisibility(UnderlyingEvent);

	protected override string Name => UnderlyingEvent.MetadataName;

	public EmbeddedEvent(EventSymbol underlyingEvent, EmbeddedMethod adder, EmbeddedMethod remover)
		: base(underlyingEvent, adder, remover, (EmbeddedMethod)null)
	{
	}

	protected override IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return UnderlyingEvent.GetCustomAttributesToEmit(moduleBuilder);
	}

	protected override ITypeReference GetType(PEModuleBuilder moduleBuilder, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		return moduleBuilder.Translate(UnderlyingEvent.Type, syntaxNodeOpt, diagnostics);
	}

	protected override void EmbedCorrespondingComEventInterfaceMethodInternal(SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics, bool isUsedForComAwareEventBinding)
	{
		NamedTypeSymbol underlyingNamedType = ContainingType.UnderlyingNamedType;
		foreach (CSharpAttributeData attribute in underlyingNamedType.GetAttributes())
		{
			if (!attribute.IsTargetAttribute(underlyingNamedType, AttributeDescription.ComEventInterfaceAttribute))
			{
				continue;
			}
			bool flag = false;
			NamedTypeSymbol namedTypeSymbol = null;
			if (attribute.CommonConstructorArguments.Length == 2)
			{
				namedTypeSymbol = attribute.CommonConstructorArguments[0].Value as NamedTypeSymbol;
				if ((object)namedTypeSymbol != null)
				{
					flag = EmbedMatchingInterfaceMethods(namedTypeSymbol, syntaxNodeOpt, diagnostics);
					foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in namedTypeSymbol.AllInterfacesNoUseSiteDiagnostics)
					{
						if (EmbedMatchingInterfaceMethods(allInterfacesNoUseSiteDiagnostic, syntaxNodeOpt, diagnostics))
						{
							flag = true;
						}
					}
				}
			}
			if (!flag & isUsedForComAwareEventBinding)
			{
				if ((object)namedTypeSymbol == null)
				{
					EmbeddedTypesManager.Error(diagnostics, ErrorCode.ERR_MissingSourceInterface, syntaxNodeOpt, underlyingNamedType, UnderlyingEvent);
					break;
				}
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				namedTypeSymbol.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
				diagnostics.Add((syntaxNodeOpt == null) ? NoLocation.Singleton : syntaxNodeOpt.Location, useSiteDiagnostics);
				EmbeddedTypesManager.Error(diagnostics, ErrorCode.ERR_MissingMethodOnSourceInterface, syntaxNodeOpt, namedTypeSymbol, UnderlyingEvent.MetadataName, UnderlyingEvent);
			}
			break;
		}
	}

	private bool EmbedMatchingInterfaceMethods(NamedTypeSymbol sourceInterface, SyntaxNode syntaxNodeOpt, DiagnosticBag diagnostics)
	{
		bool result = false;
		foreach (Symbol member in sourceInterface.GetMembers(UnderlyingEvent.MetadataName))
		{
			if (member.Kind == SymbolKind.Method)
			{
				TypeManager.EmbedMethodIfNeedTo((MethodSymbol)member, syntaxNodeOpt, diagnostics);
				result = true;
			}
		}
		return result;
	}
}
