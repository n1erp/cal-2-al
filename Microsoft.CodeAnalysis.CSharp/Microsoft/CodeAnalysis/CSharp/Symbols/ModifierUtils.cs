using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ModifierUtils
{
	internal static DeclarationModifiers MakeAndCheckNontypeMemberModifiers(SyntaxTokenList modifiers, DeclarationModifiers defaultAccess, DeclarationModifiers allowedModifiers, Location errorLocation, DiagnosticBag diagnostics, out bool modifierErrors)
	{
		DeclarationModifiers modifiers2 = modifiers.ToDeclarationModifiers(diagnostics);
		modifiers2 = CheckModifiers(modifiers2, allowedModifiers, errorLocation, diagnostics, modifiers, out modifierErrors);
		if ((modifiers2 & DeclarationModifiers.AccessibilityMask) == 0)
		{
			modifiers2 |= defaultAccess;
		}
		return modifiers2;
	}

	internal static DeclarationModifiers CheckModifiers(DeclarationModifiers modifiers, DeclarationModifiers allowedModifiers, Location errorLocation, DiagnosticBag diagnostics, SyntaxTokenList? modifierTokens, out bool modifierErrors)
	{
		modifierErrors = false;
		DeclarationModifiers declarationModifiers = modifiers & ~allowedModifiers;
		DeclarationModifiers declarationModifiers2 = modifiers & allowedModifiers;
		while (declarationModifiers != DeclarationModifiers.None)
		{
			DeclarationModifiers declarationModifiers3 = declarationModifiers & ~(declarationModifiers - 1);
			declarationModifiers &= ~declarationModifiers3;
			if (declarationModifiers3 == DeclarationModifiers.Partial)
			{
				ReportPartialError(errorLocation, diagnostics, modifierTokens);
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_BadMemberFlag, errorLocation, ConvertSingleModifierToSyntaxText(declarationModifiers3));
			}
			modifierErrors = true;
		}
		if ((allowedModifiers & (DeclarationModifiers.Partial | DeclarationModifiers.Virtual)) == (DeclarationModifiers.Partial | DeclarationModifiers.Virtual) && (declarationModifiers2 & (DeclarationModifiers.Private | DeclarationModifiers.Partial)) == (DeclarationModifiers.Private | DeclarationModifiers.Partial))
		{
			diagnostics.Add(ErrorCode.ERR_PartialMethodInvalidModifier, errorLocation);
			modifierErrors = true;
		}
		if ((declarationModifiers2 & DeclarationModifiers.PrivateProtected) != 0)
		{
			modifierErrors |= !Binder.CheckFeatureAvailability(errorLocation.SourceTree, MessageID.IDS_FeaturePrivateProtected, diagnostics, errorLocation);
		}
		return declarationModifiers2;
	}

	private static void ReportPartialError(Location errorLocation, DiagnosticBag diagnostics, SyntaxTokenList? modifierTokens)
	{
		if (modifierTokens.HasValue)
		{
			SyntaxToken syntaxToken = modifierTokens.Value.FirstOrDefault(SyntaxKind.PartialKeyword);
			if (syntaxToken != default(SyntaxToken))
			{
				diagnostics.Add(ErrorCode.ERR_PartialMisplaced, syntaxToken.GetLocation());
				return;
			}
		}
		diagnostics.Add(ErrorCode.ERR_PartialMisplaced, errorLocation);
	}

	private static string ConvertSingleModifierToSyntaxText(DeclarationModifiers modifier)
	{
		return modifier switch
		{
			DeclarationModifiers.Abstract => SyntaxFacts.GetText(SyntaxKind.AbstractKeyword), 
			DeclarationModifiers.Sealed => SyntaxFacts.GetText(SyntaxKind.SealedKeyword), 
			DeclarationModifiers.Static => SyntaxFacts.GetText(SyntaxKind.StaticKeyword), 
			DeclarationModifiers.New => SyntaxFacts.GetText(SyntaxKind.NewKeyword), 
			DeclarationModifiers.Public => SyntaxFacts.GetText(SyntaxKind.PublicKeyword), 
			DeclarationModifiers.Protected => SyntaxFacts.GetText(SyntaxKind.ProtectedKeyword), 
			DeclarationModifiers.Internal => SyntaxFacts.GetText(SyntaxKind.InternalKeyword), 
			DeclarationModifiers.ProtectedInternal => SyntaxFacts.GetText(SyntaxKind.ProtectedKeyword) + " " + SyntaxFacts.GetText(SyntaxKind.InternalKeyword), 
			DeclarationModifiers.Private => SyntaxFacts.GetText(SyntaxKind.PrivateKeyword), 
			DeclarationModifiers.PrivateProtected => SyntaxFacts.GetText(SyntaxKind.PrivateKeyword) + " " + SyntaxFacts.GetText(SyntaxKind.ProtectedKeyword), 
			DeclarationModifiers.ReadOnly => SyntaxFacts.GetText(SyntaxKind.ReadOnlyKeyword), 
			DeclarationModifiers.Const => SyntaxFacts.GetText(SyntaxKind.ConstKeyword), 
			DeclarationModifiers.Volatile => SyntaxFacts.GetText(SyntaxKind.VolatileKeyword), 
			DeclarationModifiers.Extern => SyntaxFacts.GetText(SyntaxKind.ExternKeyword), 
			DeclarationModifiers.Partial => SyntaxFacts.GetText(SyntaxKind.PartialKeyword), 
			DeclarationModifiers.Unsafe => SyntaxFacts.GetText(SyntaxKind.UnsafeKeyword), 
			DeclarationModifiers.Fixed => SyntaxFacts.GetText(SyntaxKind.FixedKeyword), 
			DeclarationModifiers.Virtual => SyntaxFacts.GetText(SyntaxKind.VirtualKeyword), 
			DeclarationModifiers.Override => SyntaxFacts.GetText(SyntaxKind.OverrideKeyword), 
			DeclarationModifiers.Async => SyntaxFacts.GetText(SyntaxKind.AsyncKeyword), 
			DeclarationModifiers.Ref => SyntaxFacts.GetText(SyntaxKind.RefKeyword), 
			_ => throw ExceptionUtilities.UnexpectedValue(modifier), 
		};
	}

	private static DeclarationModifiers ToDeclarationModifier(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AbstractKeyword => DeclarationModifiers.Abstract, 
			SyntaxKind.AsyncKeyword => DeclarationModifiers.Async, 
			SyntaxKind.SealedKeyword => DeclarationModifiers.Sealed, 
			SyntaxKind.StaticKeyword => DeclarationModifiers.Static, 
			SyntaxKind.NewKeyword => DeclarationModifiers.New, 
			SyntaxKind.PublicKeyword => DeclarationModifiers.Public, 
			SyntaxKind.ProtectedKeyword => DeclarationModifiers.Protected, 
			SyntaxKind.InternalKeyword => DeclarationModifiers.Internal, 
			SyntaxKind.PrivateKeyword => DeclarationModifiers.Private, 
			SyntaxKind.ExternKeyword => DeclarationModifiers.Extern, 
			SyntaxKind.ReadOnlyKeyword => DeclarationModifiers.ReadOnly, 
			SyntaxKind.PartialKeyword => DeclarationModifiers.Partial, 
			SyntaxKind.UnsafeKeyword => DeclarationModifiers.Unsafe, 
			SyntaxKind.VirtualKeyword => DeclarationModifiers.Virtual, 
			SyntaxKind.OverrideKeyword => DeclarationModifiers.Override, 
			SyntaxKind.ConstKeyword => DeclarationModifiers.Const, 
			SyntaxKind.FixedKeyword => DeclarationModifiers.Fixed, 
			SyntaxKind.VolatileKeyword => DeclarationModifiers.Volatile, 
			SyntaxKind.RefKeyword => DeclarationModifiers.Ref, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	public static DeclarationModifiers ToDeclarationModifiers(this SyntaxTokenList modifiers, DiagnosticBag diagnostics)
	{
		DeclarationModifiers declarationModifiers = DeclarationModifiers.None;
		bool seenNoDuplicates = true;
		bool seenNoAccessibilityDuplicates = true;
		foreach (SyntaxToken item in modifiers)
		{
			DeclarationModifiers declarationModifiers2 = ToDeclarationModifier(item.ContextualKind());
			ReportDuplicateModifiers(item, declarationModifiers2, declarationModifiers, ref seenNoDuplicates, ref seenNoAccessibilityDuplicates, diagnostics);
			declarationModifiers |= declarationModifiers2;
		}
		switch (declarationModifiers & DeclarationModifiers.AccessibilityMask)
		{
		case DeclarationModifiers.Protected | DeclarationModifiers.Internal:
			declarationModifiers &= ~DeclarationModifiers.AccessibilityMask;
			declarationModifiers |= DeclarationModifiers.ProtectedInternal;
			break;
		case DeclarationModifiers.Protected | DeclarationModifiers.Private:
			declarationModifiers &= ~DeclarationModifiers.AccessibilityMask;
			declarationModifiers |= DeclarationModifiers.PrivateProtected;
			break;
		}
		return declarationModifiers;
	}

	private static void ReportDuplicateModifiers(SyntaxToken modifierToken, DeclarationModifiers modifierKind, DeclarationModifiers allModifiers, ref bool seenNoDuplicates, ref bool seenNoAccessibilityDuplicates, DiagnosticBag diagnostics)
	{
		if ((allModifiers & modifierKind) != 0 && seenNoDuplicates)
		{
			diagnostics.Add(ErrorCode.ERR_DuplicateModifier, modifierToken.GetLocation(), SyntaxFacts.GetText(modifierToken.Kind()));
			seenNoDuplicates = false;
		}
	}

	internal static CSDiagnosticInfo CheckAccessibility(DeclarationModifiers modifiers)
	{
		if (!IsValidAccessibility(modifiers))
		{
			return new CSDiagnosticInfo(ErrorCode.ERR_BadMemberProtection);
		}
		return null;
	}

	internal static Accessibility EffectiveAccessibility(DeclarationModifiers modifiers)
	{
		return (modifiers & DeclarationModifiers.AccessibilityMask) switch
		{
			DeclarationModifiers.None => Accessibility.NotApplicable, 
			DeclarationModifiers.Private => Accessibility.Private, 
			DeclarationModifiers.Protected => Accessibility.Protected, 
			DeclarationModifiers.Internal => Accessibility.Internal, 
			DeclarationModifiers.Public => Accessibility.Public, 
			DeclarationModifiers.ProtectedInternal => Accessibility.ProtectedOrInternal, 
			DeclarationModifiers.PrivateProtected => Accessibility.ProtectedAndInternal, 
			_ => Accessibility.Public, 
		};
	}

	internal static bool IsValidAccessibility(DeclarationModifiers modifiers)
	{
		switch (modifiers & DeclarationModifiers.AccessibilityMask)
		{
		case DeclarationModifiers.None:
		case DeclarationModifiers.Public:
		case DeclarationModifiers.Protected:
		case DeclarationModifiers.Internal:
		case DeclarationModifiers.ProtectedInternal:
		case DeclarationModifiers.Private:
		case DeclarationModifiers.PrivateProtected:
			return true;
		default:
			return false;
		}
	}
}
