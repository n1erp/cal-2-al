using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class SourceOptionTypeSymbol : OptionTypeSymbol
{
	private ImmutableArray<Symbol> members;

	internal SeparatedSyntaxList<IdentifierNameOrEmptySyntax> OptionsDefinition { get; }

	public override string OptionMembers { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public static SourceOptionTypeSymbol Create(Symbol owner, OptionValuesSyntax syntax, DiagnosticBag diagnostics)
	{
		return new SourceOptionTypeSymbol(owner, syntax, diagnostics);
	}

	public static SourceOptionTypeSymbol Create(Symbol owner, OptionDataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		return new SourceOptionTypeSymbol(owner, syntax.OptionValues, diagnostics);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return members;
	}

	private SourceOptionTypeSymbol(Symbol owner, OptionValuesSyntax syntax, DiagnosticBag diagnostics)
	{
		ContainingSymbol = owner;
		DeclaringSyntaxReferences = ImmutableArray.Create(syntax.GetReference());
		Locations = ImmutableArray.Create((Location)new SourceLocation(syntax));
		OptionsDefinition = syntax.Options;
		OptionMembers = ConstructOptionMembers(syntax.Options);
		BuildMembers(diagnostics);
	}

	private void BuildMembers(DiagnosticBag diagnostics)
	{
		if (OptionsDefinition.Count == 0)
		{
			members = ImmutableArray<Symbol>.Empty;
			return;
		}
		ImmutableArray<int> immutableArray = BuildOrdinalValues(ContainingSymbol);
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(OptionsDefinition.Count);
		for (int i = 0; i < OptionsDefinition.Count; i++)
		{
			int ordinal = i;
			if (immutableArray.Length == OptionsDefinition.Count)
			{
				ordinal = immutableArray[i];
			}
			IdentifierNameSyntax identifierName = OptionsDefinition[i].IdentifierName;
			if (identifierName != null)
			{
				string name = identifierName.Unquoted();
				if (instance.Any((Symbol o) => SemanticFacts.NameEqualityComparer.Equals(o.Name, name)))
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateOption, OptionsDefinition[i].GetLocation(), name);
				}
				instance.Add(SourceOptionSymbol.Create(this, name, ordinal, OptionsDefinition.Node));
				if (name.IndexOf(',') >= 0)
				{
					diagnostics.Add(ErrorCode.ERR_OptionCannotContainComma, identifierName.GetLocation(), name);
				}
			}
			else
			{
				instance.Add(SourceOptionSymbol.Create(this, string.Empty, ordinal, OptionsDefinition.Node));
			}
		}
		members = instance.ToImmutableAndFree();
	}

	private static ImmutableArray<int> BuildOrdinalValues(Symbol containingSymbol)
	{
		if (containingSymbol.Kind == SymbolKind.Field && SystemTables.IsOptionalOrdinalValuesAllowed(containingSymbol.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>().Id))
		{
			SourcePropertySymbol sourcePropertySymbol = (SourcePropertySymbol)containingSymbol.Properties.FirstOrDefault((PropertySymbol p) => p.PropertyKind == PropertyKind.OptionOrdinalValues);
			if (sourcePropertySymbol != null)
			{
				CommaSeparatedIdentifierOrLiteralPropertyValueSyntax commaSeparatedIdentifierOrLiteralPropertyValueSyntax = (CommaSeparatedIdentifierOrLiteralPropertyValueSyntax)((PropertySyntax)sourcePropertySymbol.Syntax).Value;
				if (commaSeparatedIdentifierOrLiteralPropertyValueSyntax != null)
				{
					_ = commaSeparatedIdentifierOrLiteralPropertyValueSyntax.Values;
					if (0 == 0)
					{
						ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance();
						try
						{
							foreach (IdentifierOrLiteralExpressionSyntax value in commaSeparatedIdentifierOrLiteralPropertyValueSyntax.Values)
							{
								if (value.Identifier is LiteralExpressionSyntax)
								{
									LiteralValueSyntax literal = ((LiteralExpressionSyntax)value.Identifier).Literal;
									if (literal.Kind == SyntaxKind.Int32SignedLiteralValue)
									{
										Int32SignedLiteralValueSyntax int32SignedLiteralValueSyntax = (Int32SignedLiteralValueSyntax)literal;
										instance.Add((int)int32SignedLiteralValueSyntax.Number.Value);
									}
								}
							}
							return instance.ToImmutableArray();
						}
						finally
						{
							instance.Free();
						}
					}
				}
				return ImmutableArray<int>.Empty;
			}
		}
		return ImmutableArray<int>.Empty;
	}

	private static string ConstructOptionMembers(SeparatedSyntaxList<IdentifierNameOrEmptySyntax> options)
	{
		IEnumerable<string> values = options.Select((IdentifierNameOrEmptySyntax opt) => (opt.IdentifierName == null) ? string.Empty : opt.IdentifierName.Unquoted());
		return string.Join(",", values);
	}
}
