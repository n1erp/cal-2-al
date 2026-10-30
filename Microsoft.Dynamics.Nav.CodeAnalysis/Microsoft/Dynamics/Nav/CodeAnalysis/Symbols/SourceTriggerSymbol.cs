using System.Diagnostics;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SourceTriggerSymbol : SourceMethodOrTriggerSymbol
{
	private int lazyId;

	private const string MagicalTriggerInfix = " - ";

	private const string EnchantingEventTriggerInfix = "::";

	public override int Id
	{
		get
		{
			if (lazyId == 0)
			{
				Interlocked.CompareExchange(ref lazyId, CalculateMethodId(), 0);
			}
			return lazyId;
		}
	}

	public override MethodKind MethodKind => MethodKind.Trigger;

	public override bool IsLocal { get; } = true;

	public override Accessibility DeclaredAccessibility { get; }

	public override string Name { get; }

	public override string MetadataName { get; }

	public override VersionCompatibility Compatibility => TriggerInfo?.Compatibility;

	internal TriggerTypeInfo TriggerInfo { get; }

	private SourceTriggerSymbol(Symbol containingSymbol, TriggerDeclarationSyntax triggerDeclarationSyntax)
		: base(containingSymbol, triggerDeclarationSyntax)
	{
		TriggerInfo = (TriggerTypeInfo)(triggerDeclarationSyntax.GetTriggerTypeAnnotation()?.Data);
		Name = TriggerInfo?.Name ?? triggerDeclarationSyntax.Name.Unquoted();
		string text = TriggerInfo?.LegacyName ?? Name;
		if (ContainingSymbol != ContainingType || !ContainingType.TypeCategory.IsTypeWithTriggers())
		{
			text = GetFullTriggerName(ContainingSymbol, text);
		}
		MetadataName = text.MangleUnquotedIdentifierName();
	}

	public static SourceTriggerSymbol Create(Symbol containingSymbol, TriggerDeclarationSyntax triggerDeclarationSyntax)
	{
		return new SourceTriggerSymbol(containingSymbol, triggerDeclarationSyntax);
	}

	internal static string GetFullTriggerName(Symbol containingSymbol, string metadataName)
	{
		return containingSymbol.MetadataName + GetTriggerInfix(containingSymbol) + metadataName;
	}

	private static string GetTriggerInfix(Symbol containingType)
	{
		if (containingType.Kind == SymbolKind.Control && ((ControlSymbol)containingType).ControlKind == ControlKind.UserControl)
		{
			return "::";
		}
		return " - ";
	}
}
