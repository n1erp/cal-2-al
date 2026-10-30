using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Translation;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class Binder
{
	internal enum BindValueKind : byte
	{
		RValue,
		RValueOrMethodGroup,
		Assignment,
		CompoundAssignment,
		OutParameter
	}

	private Conversions lazyConversions;

	private OverloadResolution lazyOverloadResolution;

	private static readonly HashSet<string> SupportedLinkSchemes = new HashSet<string>(new string[2] { "http", "https" }, StringComparer.OrdinalIgnoreCase);

	private static readonly Regex ApiVersionRegex = new Regex("'v[0-9]+\\.[0-9]+'$", RegexOptions.Compiled);

	private static readonly Func<Symbol, MethodSymbol> ToMethodSymbolFunc = (Symbol s) => (MethodSymbol)s;

	internal BinderFlags Flags { get; }

	internal bool IsSemanticModelBinder => Flags.Includes(BinderFlags.SemanticModel);

	protected internal Binder Next { get; }

	internal Compilation Compilation { get; private set; }

	internal virtual Symbol ContainingMember => Next.ContainingMember;

	internal virtual bool IsInMethodBody => Next.IsInMethodBody;

	internal virtual GeneratedLabelSymbol BreakLabel => Next.BreakLabel;

	internal TypeSymbol ContainingType
	{
		get
		{
			Symbol containingMember = ContainingMember;
			DebugAssertHelper.Assert((object)containingMember == null || containingMember.Kind != SymbolKind.ErrorType);
			if ((object)containingMember != null)
			{
				if (containingMember.Kind != SymbolKind.NamedType)
				{
					return containingMember.ContainingType;
				}
				return (TypeSymbol)containingMember;
			}
			return null;
		}
	}

	internal Conversions Conversions
	{
		get
		{
			if (lazyConversions == null)
			{
				Interlocked.CompareExchange(ref lazyConversions, new Conversions(), null);
			}
			return lazyConversions;
		}
	}

	internal OverloadResolution OverloadResolution
	{
		get
		{
			if (lazyOverloadResolution == null)
			{
				Interlocked.CompareExchange(ref lazyOverloadResolution, new OverloadResolution(this), null);
			}
			return lazyOverloadResolution;
		}
	}

	internal virtual ImmutableArray<VariableSymbol> Locals => Next.Locals;

	internal Binder(Binder next)
	{
		DebugAssertHelper.Assert(next != null);
		Next = next;
		Flags = next.Flags;
		Compilation = next.Compilation;
	}

	internal Binder(Compilation compilation)
	{
		DebugAssertHelper.Assert(compilation != null);
		Flags = BinderFlags.None;
		Compilation = compilation;
	}

	protected Binder(Binder next, BinderFlags flags)
	{
		DebugAssertHelper.Assert(next != null);
		Next = next;
		Flags = flags;
		Compilation = next.Compilation;
	}

	internal virtual Binder GetBinder(SyntaxNode node)
	{
		return Next.GetBinder(node);
	}

	internal virtual ImmutableArray<VariableSymbol> GetDeclaredLocalsForScope(SyntaxNode node)
	{
		return Next.GetDeclaredLocalsForScope(node);
	}

	internal static void Error(DiagnosticBag diagnostics, DiagnosticInfo info, SyntaxNode syntax)
	{
		diagnostics.Add(new NavDiagnostic(info, syntax.Location));
	}

	internal static void Error(DiagnosticBag diagnostics, DiagnosticInfo info, Location location)
	{
		diagnostics.Add(new NavDiagnostic(info, location));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxNode syntax)
	{
		diagnostics.Add(new NavDiagnostic(new NavDiagnosticInfo(code), syntax.Location));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxNode syntax, params object[] args)
	{
		diagnostics.Add(new NavDiagnostic(new NavDiagnosticInfo(code, args), syntax.Location));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxToken token)
	{
		diagnostics.Add(new NavDiagnostic(new NavDiagnosticInfo(code), token.GetLocation()));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxToken token, params object[] args)
	{
		diagnostics.Add(new NavDiagnostic(new NavDiagnosticInfo(code, args), token.GetLocation()));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxNodeOrToken syntax)
	{
		Error(diagnostics, code, syntax.GetLocation());
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, SyntaxNodeOrToken syntax, params object[] args)
	{
		Error(diagnostics, code, syntax.GetLocation(), args);
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, Location location)
	{
		diagnostics.Add(new NavDiagnostic(new NavDiagnosticInfo(code), location));
	}

	internal static void Error(DiagnosticBag diagnostics, ErrorCode code, Location location, params object[] args)
	{
		diagnostics.Add(new NavDiagnostic(new NavDiagnosticInfo(code, args), location));
	}

	internal void ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(DiagnosticBag diagnostics, Symbol symbol, SyntaxNodeOrToken node, bool isTableExtensionTarget = false)
	{
		ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, symbol, node, IsUpgradeOrInstallCode, isTableExtensionTarget);
	}

	internal static void ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(DiagnosticBag diagnostics, Symbol symbol, SyntaxNodeOrToken node, Func<bool> isUpgradeCode, bool isTableExtensionTarget = false)
	{
		if (symbol == null)
		{
			return;
		}
		ValidateCompatibility(symbol, node, diagnostics);
		if (symbol.IsDeprecated)
		{
			if (HasObsoleteState(symbol))
			{
				if (isUpgradeCode == null || !isUpgradeCode())
				{
					diagnostics.Add(ErrorCode.WRN_ObsoleteStatePending, node.GetLocation(), symbol.Kind, symbol.Name, symbol.ObsoleteReason ?? string.Empty);
				}
			}
			else
			{
				diagnostics.Add(ErrorCode.WRN_DeprecatedSymbol, node.GetLocation(), symbol.Name);
			}
		}
		else
		{
			if (!symbol.IsObsolete)
			{
				return;
			}
			if (HasObsoleteState(symbol))
			{
				if (isUpgradeCode == null || !isUpgradeCode())
				{
					if (isTableExtensionTarget)
					{
						diagnostics.Add(ErrorCode.WRN_ObsoleteStateObsolete, node.GetLocation(), symbol.Kind, symbol.Name, symbol.ObsoleteReason ?? string.Empty);
					}
					else
					{
						diagnostics.Add(ErrorCode.ERR_ObsoleteStateObsolete, node.GetLocation(), symbol.Kind, symbol.Name, symbol.ObsoleteReason ?? string.Empty);
					}
				}
			}
			else
			{
				diagnostics.Add(ErrorCode.ERR_ObsoleteSymbol, node.GetLocation(), symbol.Name);
			}
		}
	}

	private static bool HasObsoleteState(Symbol symbol)
	{
		SymbolKind kind = symbol.Kind;
		if (kind == SymbolKind.Table || kind == SymbolKind.Field || kind == SymbolKind.Key)
		{
			return true;
		}
		return false;
	}

	private bool IsUpgradeOrInstallCode()
	{
		CodeunitTypeSymbol containingSymbolOfType = ContainingMember.GetContainingSymbolOfType<CodeunitTypeSymbol>();
		if (containingSymbolOfType == null)
		{
			return false;
		}
		if (containingSymbolOfType.Subtype != CodeunitSubtypeKind.Upgrade)
		{
			return containingSymbolOfType.Subtype == CodeunitSubtypeKind.Install;
		}
		return true;
	}

	private static bool ValidateCompatibility(Symbol symbol, SyntaxNodeOrToken node, DiagnosticBag diagnostics)
	{
		ParseOptions options = node.SyntaxTree.Options;
		if (options == null)
		{
			return true;
		}
		if (symbol.Compatibility == null)
		{
			return true;
		}
		bool flag = symbol.Compatibility.IsSupported(options.RuntimeVersion);
		if (!flag)
		{
			diagnostics.Add(ErrorCode.ERR_FeatureNotAvailable, node.GetLocation(), symbol.Name, options.RuntimeVersion.ToMajorMinorString(), symbol.Compatibility);
		}
		return flag;
	}

	[Conditional("DEBUG")]
	internal void ReportDiagnosticsIfObsolete(DiagnosticBag diagnostics, Conversion conversion, SyntaxNodeOrToken node)
	{
	}

	internal Binder WithAdditionalFlags(BinderFlags flags)
	{
		if (!Flags.Includes(flags))
		{
			return new Binder(this, Flags | flags);
		}
		return this;
	}

	internal void LookupSymbolsSimpleName(LookupResult result, TypeSymbol container, string plainName, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		LookupSymbolsOrMembersInternal(result, container, plainName, options, diagnose, ref useSiteDiagnostics);
	}

	internal void LookupSymbols(LookupResult result, string name, ref HashSet<DiagnosticInfo> useSiteDiagnostics, LookupOptions options = LookupOptions.Default)
	{
		LookupSymbolsWithFallback(result, name, ref useSiteDiagnostics, options);
	}

	private void LookupSymbolsWithFallback(LookupResult result, string name, ref HashSet<DiagnosticInfo> useSiteDiagnostics, LookupOptions options = LookupOptions.Default)
	{
		DebugAssertHelper.Assert(options.AreValid());
		LookupSymbolsInternal(result, name, options, diagnose: false, ref useSiteDiagnostics);
		if (result.Kind != LookupResultKind.Viable && result.Kind != LookupResultKind.Empty)
		{
			result.Clear();
			LookupSymbolsInternal(result, name, options, diagnose: true, ref useSiteDiagnostics);
		}
		DebugAssertHelper.Assert(result.IsMultiViable || result.IsClear || result.Error != null);
	}

	private void LookupSymbolsOrMembersInternal(LookupResult result, TypeSymbol qualifierOpt, string name, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)qualifierOpt == null)
		{
			LookupSymbolsInternal(result, name, options, diagnose, ref useSiteDiagnostics);
		}
		else
		{
			LookupMembersInternal(result, qualifierOpt, name, options, this, diagnose, ref useSiteDiagnostics);
		}
	}

	private void LookupSymbolsInternal(LookupResult result, string name, LookupOptions options, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(result.IsClear);
		DebugAssertHelper.Assert(options.AreValid());
		Binder binder = this;
		while (binder != null && !result.IsMultiViable)
		{
			if (!result.IsClear)
			{
				LookupResult instance = LookupResult.GetInstance();
				binder.LookupSymbolsInSingleBinder(instance, name, options, this, diagnose, ref useSiteDiagnostics);
				result.MergeEqual(instance);
				instance.Free();
			}
			else
			{
				binder.LookupSymbolsInSingleBinder(result, name, options, this, diagnose, ref useSiteDiagnostics);
			}
			binder = binder.Next;
		}
	}

	internal virtual void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
	}

	private void LookupMembersWithFallback(LookupResult result, ContainerSymbol namespaceOrType, string name, ref HashSet<DiagnosticInfo> useSiteDiagnostics, LookupOptions options = LookupOptions.Default)
	{
		DebugAssertHelper.Assert(options.AreValid());
		LookupMembersInternal(result, namespaceOrType, name, options, this, diagnose: false, ref useSiteDiagnostics);
		if (!result.IsMultiViable && !result.IsClear)
		{
			result.Clear();
			LookupMembersInternal(result, namespaceOrType, name, options, this, diagnose: true, ref useSiteDiagnostics);
		}
		DebugAssertHelper.Assert(result.IsMultiViable || result.IsClear || result.Error != null);
	}

	private static bool IsMethodOrTrigger(Symbol symbol)
	{
		if (symbol.Kind != SymbolKind.Method)
		{
			return symbol.IsTrigger();
		}
		return true;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "useSiteDiagnostics")]
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal SingleLookupResult CheckViability(Symbol symbol, LookupOptions options, TypeSymbol accessThroughType, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, BoundExpression receiver = null)
	{
		if ((options & LookupOptions.Trigger) == 0 && symbol.IsTrigger())
		{
			object obj;
			if (!diagnose)
			{
				obj = null;
			}
			else
			{
				object[] args = new FormattedSymbol[1]
				{
					new FormattedSymbol(symbol, SymbolDisplayFormat.ErrorMessageFormat)
				};
				obj = new NavDiagnosticInfo(ErrorCode.ERR_TriggersCannotBeCalledDirectly, args, ImmutableArray.Create(symbol), ImmutableArray<Location>.Empty);
			}
			NavDiagnosticInfo error = (NavDiagnosticInfo)obj;
			return LookupResult.NotReferencable(symbol, error);
		}
		if ((options & LookupOptions.MustBeInvocableIfMember) != 0 && symbol.IsNonInvocableMember())
		{
			return LookupResult.NotInvocable(symbol, diagnose);
		}
		if ((options & LookupOptions.MustBeOption) != 0 && !symbol.IsOptionMembers())
		{
			NavDiagnosticInfo error2 = (diagnose ? new NavDiagnosticInfo(ErrorCode.ERR_InvalidOptionValue, new FormattedSymbol[1]
			{
				new FormattedSymbol(symbol, SymbolDisplayFormat.ErrorMessageFormat)
			}, ImmutableArray.Create(symbol)) : null);
			return LookupResult.NotOptionMembers(symbol, error2);
		}
		if ((options & LookupOptions.MustBeOption) == 0 && symbol.MustBeAccessedAsOptionMember())
		{
			NavDiagnosticInfo error3 = (diagnose ? new NavDiagnosticInfo(ErrorCode.ERR_OptionReferencedAsMember) : null);
			return LookupResult.Inaccessible(symbol, error3);
		}
		if ((options & LookupOptions.MustBePageElement) != 0 && !symbol.IsPageElement())
		{
			return LookupResult.NotPageElement(symbol, diagnose);
		}
		if ((options & LookupOptions.MustBePageElement) == 0 && symbol.IsPageElement())
		{
			ControlSymbol controlSymbol = symbol as ControlSymbol;
			if (controlSymbol == null || !controlSymbol.IsAccessibleFromCode())
			{
				object obj2;
				if (!diagnose)
				{
					obj2 = null;
				}
				else
				{
					object[] args = new string[1] { symbol.Name };
					obj2 = new NavDiagnosticInfo(ErrorCode.ERR_NameNotInContext, args, ImmutableArray.Create(symbol), ImmutableArray<Location>.Empty);
				}
				NavDiagnosticInfo error4 = (NavDiagnosticInfo)obj2;
				return LookupResult.NotReferencable(symbol, error4);
			}
		}
		if ((options & LookupOptions.MustBePageElement) == 0 && !IsAccessible(symbol, ref useSiteDiagnostics, options, accessThroughType))
		{
			object obj3;
			if (!diagnose)
			{
				obj3 = null;
			}
			else
			{
				object[] args = new FormattedSymbol[1]
				{
					new FormattedSymbol(symbol, SymbolDisplayFormat.ErrorMessageFormat)
				};
				obj3 = new NavDiagnosticInfo(ErrorCode.ERR_BadAccess, args, ImmutableArray.Create(symbol), ImmutableArray<Location>.Empty);
			}
			NavDiagnosticInfo error5 = (NavDiagnosticInfo)obj3;
			return LookupResult.Inaccessible(symbol, error5);
		}
		if ((options & LookupOptions.MustBeInstance) != 0 && !symbol.IsInstance())
		{
			NavDiagnosticInfo error6 = (diagnose ? new NavDiagnosticInfo(ErrorCode.ERR_ObjectProhibited, symbol, symbol.ContainingType.Name) : null);
			return LookupResult.StaticInstanceMismatch(symbol, error6);
		}
		if ((options & LookupOptions.MustNotBeInstance) != 0 && symbol.IsInstance())
		{
			NavDiagnosticInfo error7 = (diagnose ? new NavDiagnosticInfo(ErrorCode.ERR_ObjectRequired, symbol) : null);
			return LookupResult.StaticInstanceMismatch(symbol, error7);
		}
		if ((options & LookupOptions.ValidateSymbolCompilationScope) != 0 && !symbol.CanCompileWithTarget(Compilation.Options.Target, accessThroughType))
		{
			object obj4;
			if (!diagnose)
			{
				obj4 = null;
			}
			else
			{
				object[] args = new string[2]
				{
					symbol.Name,
					Compilation.Options.Target.ToString()
				};
				obj4 = new NavDiagnosticInfo(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, args, ImmutableArray.Create(symbol), ImmutableArray<Location>.Empty);
			}
			NavDiagnosticInfo error8 = (NavDiagnosticInfo)obj4;
			return LookupResult.NotReferencable(symbol, error8);
		}
		return LookupResult.Good(symbol, receiver);
	}

	internal bool CanAddLookupSymbolInfo(Symbol symbol, LookupOptions options, TypeSymbol accessThroughType)
	{
		DebugAssertHelper.Assert(options.AreValid());
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		if ((options & LookupOptions.MustBeInvocableIfMember) != 0 && symbol.IsNonInvocableMember())
		{
			return false;
		}
		if (!IsAccessible(symbol, ref useSiteDiagnostics, options, accessThroughType))
		{
			return false;
		}
		if ((options & LookupOptions.MustBeInstance) != 0 && !symbol.IsInstance())
		{
			return false;
		}
		if ((options & LookupOptions.MustNotBeInstance) != 0 && symbol.IsInstance())
		{
			return false;
		}
		return true;
	}

	internal bool IsAccessible(Symbol symbol, ref HashSet<DiagnosticInfo> useSiteDiagnostics, LookupOptions options, TypeSymbol accessThroughType = null)
	{
		bool failedThroughTypeCheck;
		return IsAccessible(symbol, accessThroughType, options, out failedThroughTypeCheck, ref useSiteDiagnostics);
	}

	internal bool IsAccessible(Symbol symbol, TypeSymbol accessThroughType, LookupOptions options, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (Flags.Includes(BinderFlags.IgnoreAccessibility))
		{
			failedThroughTypeCheck = false;
			return true;
		}
		return IsAccessibleHelper(symbol, accessThroughType, options, out failedThroughTypeCheck, ref useSiteDiagnostics);
	}

	internal virtual bool IsAccessibleHelper(Symbol symbol, TypeSymbol accessThroughType, LookupOptions options, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return Next.IsAccessibleHelper(symbol, accessThroughType, options, out failedThroughTypeCheck, ref useSiteDiagnostics);
	}

	internal static BoundLiteral CreateBoundLiteral(LiteralExpressionSyntax literalExpression, DiagnosticBag diagnostics, bool unquoteLiteral = false)
	{
		if (literalExpression.Literal.IsKind(SyntaxKind.StringLiteralValue))
		{
			return CreateBoundStringLiteral(((StringLiteralValueSyntax)literalExpression.Literal).Value.ValueText, literalExpression, diagnostics, unquoteLiteral);
		}
		return BindLiteralConstant(literalExpression, diagnostics);
	}

	internal static BoundLiteral CreateBoundStringLiteral(string literalValue, SyntaxNode node, DiagnosticBag diagnostics, bool unquoteLiteral = false)
	{
		ConstantValue constantValueOpt = ConstantValue.Create(unquoteLiteral ? literalValue.UnquoteLiteral() : literalValue);
		TypeSymbol type = CreateTypeSymbol(node, diagnostics);
		return new BoundLiteral(node, constantValueOpt, type);
	}

	internal static TypeSymbol CreateTypeSymbol(SyntaxNode node, DiagnosticBag diagnostics)
	{
		NavTypeKind navTypeKind = NavTypeExtensions.FromLiteralSyntaxKind(node.Kind);
		if (navTypeKind == NavTypeKind.None)
		{
			navTypeKind = NavTypeKind.Text;
		}
		return GetNavType(navTypeKind, diagnostics, node);
	}

	internal static bool IsSymbolAccessibleConditional(Symbol symbol, ModuleSymbol within, LookupOptions options, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		return AccessCheck.IsSymbolAccessible(symbol, within, options, ref useSiteDiagnostics);
	}

	internal bool IsSymbolAccessibleConditional(Symbol symbol, TypeSymbol within, TypeSymbol throughTypeOpt, LookupOptions options, out bool failedThroughTypeCheck, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (Flags.Includes(BinderFlags.IgnoreAccessibility))
		{
			failedThroughTypeCheck = false;
			return true;
		}
		return AccessCheck.IsSymbolAccessible(symbol, within, options, throughTypeOpt, out failedThroughTypeCheck, ref useSiteDiagnostics);
	}

	internal BoundAttribute BindAttribute(MemberAttributeSyntax syntax, Symbol containingSymbol, DiagnosticBag diagnostics)
	{
		GlobalLogger.LogVerbose("Binding Attribute: '{0}' defined on '{1}'.", syntax, containingSymbol.Name);
		AttributeTypeInfo attributeInfo = (AttributeTypeInfo)(syntax.GetAttributeTypeAnnotation()?.Data);
		bool hasErrors = false;
		if (!IsAttributeValid(syntax, diagnostics, attributeInfo))
		{
			return new BoundAttribute(syntax, ImmutableArray<BoundExpression>.Empty, hasErrors: true);
		}
		ValidateAttributeOwner(syntax, containingSymbol, diagnostics, attributeInfo, ref hasErrors);
		ValidateCodeunitSubtype(syntax, containingSymbol, diagnostics, attributeInfo, ref hasErrors);
		ValidateAttributeArgumentCount(syntax, diagnostics, attributeInfo, ref hasErrors);
		ImmutableArray<BoundExpression> arguments = BindAttributeArguments(syntax, diagnostics, attributeInfo, ref hasErrors);
		return new BoundAttribute(syntax, arguments, hasErrors);
	}

	private ImmutableArray<BoundExpression> BindAttributeArguments(MemberAttributeSyntax syntax, DiagnosticBag diagnostics, AttributeTypeInfo attributeInfo, ref bool hasErrors)
	{
		ImmutableArray<BoundExpression> result = ImmutableArray<BoundExpression>.Empty;
		if (syntax.ArgumentList != null)
		{
			SeparatedSyntaxList<AttributeArgumentSyntax> arguments = syntax.ArgumentList.Arguments;
			ImmutableArray<AttributeParameter> parameters = attributeInfo.Parameters;
			ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
			for (int i = 0; i < arguments.Count && i < parameters.Length; i++)
			{
				BoundExpression boundExpression = BindAttributeArgument(arguments[i], diagnostics, attributeInfo);
				if (boundExpression.HasErrors)
				{
					hasErrors = true;
				}
				else if (boundExpression.Type != parameters[i].Type && !IsAllowedAttributeConversion(boundExpression, parameters[i]))
				{
					diagnostics.Add(ErrorCode.ERR_BadArgType, arguments[i].Location, i + 1, boundExpression.Type, parameters[i].Type);
					hasErrors = true;
				}
				instance.Add(boundExpression);
			}
			result = instance.ToImmutableAndFree();
		}
		return result;
	}

	private static bool IsAllowedAttributeConversion(BoundExpression argument, AttributeParameter parameter)
	{
		return IsAllowedAttributeConversion(argument.Type.NavTypeKind, parameter.Type.NavTypeKind);
	}

	private static bool IsAllowedAttributeConversion(NavTypeKind argumentKind, NavTypeKind parameterKind)
	{
		if (argumentKind != NavTypeKind.Char || parameterKind != NavTypeKind.Text)
		{
			if (argumentKind == NavTypeKind.String)
			{
				return parameterKind == NavTypeKind.Text;
			}
			return false;
		}
		return true;
	}

	private static void ValidateAttributeArgumentCount(MemberAttributeSyntax syntax, DiagnosticBag diagnostics, AttributeTypeInfo attributeInfo, ref bool hasErrors)
	{
		if (attributeInfo != null)
		{
			int num = syntax.ArgumentList?.Arguments.Count ?? 0;
			if (num < attributeInfo.MinParameterCount || num > attributeInfo.MaxParameterCount)
			{
				diagnostics.Add(ErrorCode.ERR_BadAttributeArgCount, syntax.Location, syntax.Name.Identifier.ValueText, num);
				hasErrors = true;
			}
		}
	}

	private static bool IsAttributeValid(MemberAttributeSyntax syntax, DiagnosticBag diagnostics, AttributeTypeInfo attributeInfo)
	{
		if (attributeInfo == null)
		{
			diagnostics.Add(ErrorCode.ERR_InvalidAttribute, syntax.Location, syntax.Name.Identifier.ValueText);
			return false;
		}
		return true;
	}

	private static void ValidateCodeunitSubtype(MemberAttributeSyntax syntax, Symbol containingSymbol, DiagnosticBag diagnostics, AttributeTypeInfo attributeInfo, ref bool hasErrors)
	{
		if (attributeInfo.RequiredCodeunitSubtype.HasValue)
		{
			CodeunitTypeSymbol containingSymbolOfKind = containingSymbol.GetContainingSymbolOfKind<CodeunitTypeSymbol>(SymbolKind.Codeunit);
			if (containingSymbolOfKind != null && containingSymbolOfKind.Subtype != attributeInfo.RequiredCodeunitSubtype)
			{
				diagnostics.Add(ErrorCode.ERR_BadAttributeCodeunitSubtype, syntax.Location, syntax.Name.Identifier.ValueText, attributeInfo.RequiredCodeunitSubtype.ToString());
				hasErrors = true;
			}
		}
	}

	private static void ValidateAttributeOwner(MemberAttributeSyntax syntax, Symbol containingSymbol, DiagnosticBag diagnostics, AttributeTypeInfo attributeInfo, ref bool hasErrors)
	{
		switch (containingSymbol.Kind)
		{
		case SymbolKind.Method:
			if (attributeInfo.IsAllowedOn(AttributeAllowedOn.Method))
			{
				MethodSymbol obj = containingSymbol as MethodSymbol;
				if ((object)obj != null && obj.MethodKind == MethodKind.Method)
				{
					break;
				}
			}
			goto default;
		case SymbolKind.GlobalVariable:
		case SymbolKind.LocalVariable:
			if (attributeInfo.IsAllowedOn(AttributeAllowedOn.Variable))
			{
				break;
			}
			goto default;
		default:
			diagnostics.Add(ErrorCode.ERR_AttributeAllowedOnlyOn, syntax.Location, syntax.Name.Identifier.ValueText, attributeInfo.GetIsAllowedOnText());
			hasErrors = true;
			break;
		}
		if (attributeInfo.AllowedIn.HasValue && containingSymbol.ContainingType.Kind != attributeInfo.AllowedIn)
		{
			diagnostics.Add(ErrorCode.ERR_AttributeOnlyAllowedIn, syntax.Location, syntax.Name.Identifier.ValueText, attributeInfo.AllowedIn);
			hasErrors = true;
		}
	}

	private BoundExpression BindAttributeArgument(AttributeArgumentSyntax syntax, DiagnosticBag diagnostics, AttributeTypeInfo attributeInfo)
	{
		switch (syntax.Kind)
		{
		case SyntaxKind.MethodReferenceAttributeArgument:
			return BindMethodReference(((MethodReferenceAttributeArgumentSyntax)syntax).MethodReference, diagnostics);
		case SyntaxKind.OptionAccessAttributeArgument:
			return BindOptionAccess(((OptionAccessAttributeArgumentSyntax)syntax).OptionAccess, diagnostics);
		case SyntaxKind.LiteralAttributeArgument:
		{
			LiteralExpressionSyntax literal = ((LiteralAttributeArgumentSyntax)syntax).Literal;
			ValidateScopeTypeAttribute(literal, diagnostics, attributeInfo);
			return BindValue(literal, diagnostics, BindValueKind.RValue);
		}
		default:
			return new BoundBadExpression(syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundNode>.Empty, new ExtendedErrorTypeSymbol(Compilation, string.Empty, null), hasErrors: true);
		}
	}

	private static BoundMethodReference BindMethodReference(MemberAccessExpressionSyntax node, DiagnosticBag diagnostics)
	{
		diagnostics.Add(ErrorCode.WRN_PropertyIsObsolete, node.Location, node.Name.Identifier.ValueText);
		return new BoundMethodReference(node, null, NavCorLib.MethodReferenceType);
	}

	private static void ValidateScopeTypeAttribute(LiteralExpressionSyntax syntax, DiagnosticBag diagnostics, AttributeTypeInfo attributeInfo)
	{
		if (attributeInfo.Kind == AttributeKind.Scope)
		{
			string text = syntax.Literal.ToString();
			if (!Enum.TryParse<SymbolCompilationScope>(text.UnquoteLiteral(), out var _))
			{
				diagnostics.Add(ErrorCode.ERR_InvalidAttribute, syntax.Location, text);
			}
		}
	}

	protected BoundExpression CreateConversion(BoundExpression source, Conversion conversion, TypeSymbol targetType, BoundExpression target, DiagnosticBag diagnostics)
	{
		return CreateConversion(source.Syntax, source, conversion, isCast: false, targetType, target, diagnostics);
	}

	protected BoundExpression CreateConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, bool isCast, TypeSymbol targetType, BoundExpression target, DiagnosticBag diagnostics)
	{
		return CreateConversion(syntax, source, conversion, isCast, source.WasCompilerGenerated, targetType, target, diagnostics);
	}

	protected BoundExpression CreateConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, bool isCast, bool wasCompilerGenerated, TypeSymbol targetType, BoundExpression target, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(source != null);
		DebugAssertHelper.Assert((object)targetType != null);
		if (conversion.Kind == ConversionKind.Identity && !isCast && source.Type == targetType)
		{
			return source;
		}
		ConstantValue constantValueOpt = FoldConstantConversion(syntax, source, conversion, targetType, diagnostics);
		return new BoundConversion(syntax, source, conversion, constantValueOpt, targetType, target)
		{
			WasCompilerGenerated = wasCompilerGenerated
		};
	}

	public static ConstantValue FoldConstantConversion(SyntaxNode syntax, BoundExpression source, Conversion conversion, TypeSymbol destinationType, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(source != null);
		DebugAssertHelper.Assert((object)destinationType != null);
		if (source.HasAnyErrors)
		{
			return null;
		}
		ConstantValue constantValue = source.ConstantValue;
		if (constantValue == null || constantValue.IsBad)
		{
			return constantValue;
		}
		switch (conversion.Kind)
		{
		case ConversionKind.Identity:
			return constantValue;
		case ConversionKind.ImplicitNumericToSmaller:
		case ConversionKind.ToDecimal:
		case ConversionKind.ImplicitNumeric:
		case ConversionKind.ImplicitOption:
		case ConversionKind.ImplicitConstant:
			return FoldConstantNumericConversion(syntax, constantValue, destinationType, diagnostics);
		case ConversionKind.ToOption:
			return FoldConstantNumericToOptionConversion(syntax, constantValue, destinationType, diagnostics);
		case ConversionKind.ImplicitTextToSmaller:
		case ConversionKind.ToText:
		case ConversionKind.ImplicitText:
			if ((object)constantValue != null && constantValue.IsString)
			{
				return constantValue;
			}
			if ((object)constantValue != null && constantValue.IsChar)
			{
				return ConstantValue.Create(constantValue.Value.ToString(), ConstantValueTypeDiscriminator.String);
			}
			break;
		}
		return null;
	}

	private ConstantValue FoldConditionalLogicalOperator(BinaryOperatorKind kind, BoundExpression left, BoundExpression right)
	{
		DebugAssertHelper.Assert(kind == BinaryOperatorKind.LogicalAnd || kind == BinaryOperatorKind.LogicalOr || kind == BinaryOperatorKind.LogicalXor);
		DebugAssertHelper.Assert(left != null);
		DebugAssertHelper.Assert(right != null);
		if (left.HasAnyErrors || right.HasAnyErrors)
		{
			return null;
		}
		ConstantValue constantValue = left.ConstantValue;
		ConstantValue constantValue2 = right.ConstantValue;
		if (constantValue == null || constantValue2 == null)
		{
			return null;
		}
		if (constantValue.IsBad || constantValue2.IsBad)
		{
			return ConstantValue.Bad;
		}
		if (!constantValue.IsBoolean || !constantValue2.IsBoolean)
		{
			return null;
		}
		bool value;
		switch (kind)
		{
		case BinaryOperatorKind.LogicalAnd:
			value = constantValue.BooleanValue && constantValue2.BooleanValue;
			break;
		case BinaryOperatorKind.LogicalOr:
			value = constantValue.BooleanValue || constantValue2.BooleanValue;
			break;
		case BinaryOperatorKind.LogicalXor:
			value = constantValue.BooleanValue ^ constantValue2.BooleanValue;
			break;
		default:
			return null;
		}
		return ConstantValue.Create(value);
	}

	private static ConstantValue FoldConstantNumericToOptionConversion(SyntaxNode syntax, ConstantValue sourceValue, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(sourceValue != null);
		DebugAssertHelper.Assert(!sourceValue.IsBad);
		NavTypeKind navTypeKindSafe = destination.GetNavTypeKindSafe();
		DebugAssertHelper.Assert(navTypeKindSafe == NavTypeKind.Option || navTypeKindSafe == NavTypeKind.Enum);
		if (!sourceValue.IsNumeric)
		{
			return null;
		}
		if (sourceValue.IsDecimal)
		{
			if (Math.Abs(sourceValue.DecimalValue % 1m - 1m) > 0.0000000000000000000000001m)
			{
				Error(diagnostics, ErrorCode.ERR_ConstantValueNotAValidOptionValue, syntax, sourceValue.GetValueToDisplay(), "Option");
				return null;
			}
			sourceValue = ConstantValue.Create((int)Math.Round(sourceValue.DecimalValue));
		}
		sourceValue = FoldConstantNumericConversion(syntax, sourceValue, NavCorLib.IntegerType, diagnostics);
		if (sourceValue.IsBad)
		{
			return null;
		}
		if (sourceValue.Int32Value < 0)
		{
			Error(diagnostics, ErrorCode.WRN_ConstantValueNotAValidOptionValue, syntax, sourceValue.GetValueToDisplay());
		}
		OptionTypeSymbol optionTypeSymbol = destination as OptionTypeSymbol;
		if (optionTypeSymbol != null)
		{
			_ = sourceValue.Int32Value;
			_ = optionTypeSymbol.GetMembers().Length - 1;
		}
		return sourceValue;
	}

	private static ConstantValue FoldConstantNumericConversion(SyntaxNode syntax, ConstantValue sourceValue, TypeSymbol destination, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(sourceValue != null);
		DebugAssertHelper.Assert(!sourceValue.IsBad);
		NavTypeKind navTypeKindSafe = destination.GetNavTypeKindSafe();
		if (!CheckConstantBounds(navTypeKindSafe, sourceValue))
		{
			Error(diagnostics, ErrorCode.ERR_ConstOutOfRange, syntax, sourceValue.GetValueToDisplay(), destination);
			return ConstantValue.Bad;
		}
		return ConstantValue.Create(DoUncheckedConversion(navTypeKindSafe, sourceValue), navTypeKindSafe);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	private static object DoUncheckedConversion(NavTypeKind destinationType, ConstantValue value)
	{
		switch (value.Discriminator)
		{
		case ConstantValueTypeDiscriminator.Byte:
		{
			byte byteValue = value.ByteValue;
			switch (destinationType)
			{
			case NavTypeKind.Byte:
				return byteValue;
			case NavTypeKind.Char:
				return (char)byteValue;
			case NavTypeKind.Integer:
			case NavTypeKind.Option:
			case NavTypeKind.Enum:
				return (int)byteValue;
			case NavTypeKind.BigInteger:
			case NavTypeKind.Duration:
				return (long)byteValue;
			case NavTypeKind.Decimal:
				return (decimal)byteValue;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.Char:
		{
			char charValue = value.CharValue;
			switch (destinationType)
			{
			case NavTypeKind.Byte:
				return (byte)charValue;
			case NavTypeKind.Char:
				return charValue;
			case NavTypeKind.Integer:
			case NavTypeKind.Option:
			case NavTypeKind.Enum:
				return (int)charValue;
			case NavTypeKind.BigInteger:
			case NavTypeKind.Duration:
				return (long)charValue;
			case NavTypeKind.Decimal:
				return (decimal)charValue;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.Int32:
		{
			int int32Value = value.Int32Value;
			switch (destinationType)
			{
			case NavTypeKind.Byte:
				return (byte)int32Value;
			case NavTypeKind.Char:
				return (char)int32Value;
			case NavTypeKind.Integer:
			case NavTypeKind.Option:
			case NavTypeKind.Enum:
				return int32Value;
			case NavTypeKind.BigInteger:
			case NavTypeKind.Duration:
				return (long)int32Value;
			case NavTypeKind.Decimal:
				return (decimal)int32Value;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.Int64:
		{
			long int64Value = value.Int64Value;
			switch (destinationType)
			{
			case NavTypeKind.Byte:
				return (byte)int64Value;
			case NavTypeKind.Char:
				return (char)int64Value;
			case NavTypeKind.Integer:
			case NavTypeKind.Option:
			case NavTypeKind.Enum:
				return (int)int64Value;
			case NavTypeKind.BigInteger:
			case NavTypeKind.Duration:
				return int64Value;
			case NavTypeKind.Decimal:
				return (decimal)int64Value;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.Decimal:
		{
			decimal num = (CheckConstantBounds(destinationType, value.DecimalValue) ? value.DecimalValue : 0m);
			switch (destinationType)
			{
			case NavTypeKind.Byte:
				return (byte)num;
			case NavTypeKind.Char:
				return (char)num;
			case NavTypeKind.Integer:
			case NavTypeKind.Option:
			case NavTypeKind.Enum:
				return (int)num;
			case NavTypeKind.BigInteger:
			case NavTypeKind.Duration:
				return (long)num;
			case NavTypeKind.Decimal:
				return num;
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		case ConstantValueTypeDiscriminator.String:
		{
			string stringValue = value.StringValue;
			switch (destinationType)
			{
			case NavTypeKind.Byte:
				DebugAssertHelper.Assert(stringValue.Length == 1);
				return (byte)stringValue[0];
			case NavTypeKind.Char:
				DebugAssertHelper.Assert(stringValue.Length == 1);
				return stringValue[0];
			default:
				throw ExceptionUtilities.UnexpectedValue(destinationType);
			}
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(value.Discriminator);
		}
	}

	private static object CanonicalizeConstant(ConstantValue value)
	{
		return value.Discriminator switch
		{
			ConstantValueTypeDiscriminator.Byte => (object)(decimal)value.ByteValue, 
			ConstantValueTypeDiscriminator.Char => (decimal)value.CharValue, 
			ConstantValueTypeDiscriminator.Int32 => (decimal)value.Int32Value, 
			ConstantValueTypeDiscriminator.Int64 => (decimal)value.Int64Value, 
			ConstantValueTypeDiscriminator.Decimal => value.DecimalValue, 
			_ => throw ExceptionUtilities.UnexpectedValue(value.Discriminator), 
		};
	}

	public static bool CheckConstantBounds(NavTypeKind destinationType, ConstantValue value)
	{
		if (value.IsBad)
		{
			return true;
		}
		if (value.IsString)
		{
			return true;
		}
		object obj = CanonicalizeConstant(value);
		return CheckConstantBounds(destinationType, (decimal)obj);
	}

	private static bool CheckConstantBounds(NavTypeKind destinationType, decimal value)
	{
		switch (destinationType)
		{
		case NavTypeKind.Byte:
			if (-1m < value)
			{
				return value < 256m;
			}
			return false;
		case NavTypeKind.Char:
			if (-1m < value)
			{
				return value < 65536m;
			}
			return false;
		case NavTypeKind.Integer:
			if (-2147483649m < value)
			{
				return value < 2147483648m;
			}
			return false;
		case NavTypeKind.BigInteger:
			if (-9223372036854775809m < value)
			{
				return value < 9223372036854775808m;
			}
			return false;
		default:
			return true;
		}
	}

	private BoundExpression BindInvocationExpression(InvocationExpressionSyntax node, DiagnosticBag diagnostics, bool isStatement)
	{
		BoundExpression expr = BindMethodGroup(node.Expression, invoked: true, diagnostics);
		expr = CheckValue(expr, BindValueKind.RValueOrMethodGroup, diagnostics);
		string methodName = ((expr.Kind == BoundKind.MethodGroup) ? GetName(node.Expression) : null);
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		BindArgumentsAndNames(node.ArgumentList, instance);
		BoundExpression result = BindInvocationExpression(node, node.Expression, methodName, expr, isStatement, asProperty: false, instance, diagnostics);
		instance.PopulateDiagnostics(diagnostics);
		instance.Free();
		return result;
	}

	private BoundExpression BindInvocationExpression(ExpressionSyntax node, BoundMethodGroup boundMethodGroup, bool isStatement, DiagnosticBag diagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		BoundExpression boundExpression = CheckValue(boundMethodGroup, BindValueKind.RValueOrMethodGroup, diagnostics);
		string methodName = ((boundExpression.Kind == BoundKind.MethodGroup) ? GetName(node) : null);
		BoundExpression result = BindInvocationExpression(node, node, methodName, boundExpression, isStatement, asProperty: true, instance, diagnostics);
		instance.Free();
		return result;
	}

	private BoundExpression BindInvocationExpression(SyntaxNode node, SyntaxNode expression, string methodName, BoundExpression boundExpression, bool isStatement, bool asProperty, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression2;
		if (boundExpression.Kind == BoundKind.MethodGroup)
		{
			boundExpression2 = BindMethodGroupInvocation(node, expression, methodName, (BoundMethodGroup)boundExpression, isStatement, analyzedArguments, diagnostics);
		}
		else
		{
			if (!boundExpression.HasAnyErrors)
			{
				diagnostics.Add(new NavDiagnosticInfo(ErrorCode.ERR_MethodNameExpected), expression.Location);
			}
			boundExpression2 = CreateBadCall(node, boundExpression, LookupResultKind.NotInvocable, analyzedArguments);
		}
		if (boundExpression2 is BoundCall { HasErrors: false } boundCall)
		{
			if (boundCall.Method.MethodKind == MethodKind.Property && !asProperty)
			{
				diagnostics.Add(new NavDiagnosticInfo(ErrorCode.ERR_PropertyUsedAsMethod, boundCall.Method.Name), expression.Location);
			}
			else if (((boundCall.Method.ReturnValue != null) & isStatement) && !boundCall.Method.ReturnValue.IsOptional)
			{
				diagnostics.Add(new NavDiagnosticInfo(ErrorCode.ERR_ReturnValueMustBeUsed, boundCall.Method.Name), expression.Location);
			}
		}
		return boundExpression2;
	}

	private BoundExpression BindMethodGroupInvocation(SyntaxNode syntax, SyntaxNode expression, string methodName, BoundMethodGroup methodGroup, bool isStatement, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		MethodGroupResolution methodGroupResolution = ResolveMethodGroup(methodGroup, analyzedArguments, ref useSiteDiagnostics);
		diagnostics.Add(expression, useSiteDiagnostics);
		if (!methodGroup.HasAnyErrors)
		{
			diagnostics.AddRange(methodGroupResolution.Diagnostics);
		}
		BoundExpression result;
		if (methodGroupResolution.HasAnyErrors)
		{
			ImmutableArray<MethodSymbol> methods;
			LookupResultKind resultKind;
			if (methodGroupResolution.OverloadResolutionResult != null)
			{
				methods = GetOriginalMethods(methodGroupResolution.OverloadResolutionResult);
				resultKind = methodGroupResolution.MethodGroup.ResultKind;
			}
			else
			{
				methods = methodGroup.Methods;
				resultKind = methodGroup.ResultKind;
			}
			result = CreateBadCall(syntax, methodName, methodGroup.ReceiverOpt, methods, resultKind, analyzedArguments);
		}
		else if (!methodGroupResolution.IsEmpty)
		{
			if (methodGroupResolution.ResultKind != LookupResultKind.Viable)
			{
				if (methodGroupResolution.MethodGroup != null)
				{
					throw ExceptionUtilities.SkippedForNow;
				}
				result = CreateBadCall(syntax, methodGroup, methodGroup.ResultKind, analyzedArguments);
			}
			else
			{
				result = BindInvocationExpressionContinued(syntax, expression, methodName, methodGroupResolution.OverloadResolutionResult, methodGroupResolution.AnalyzedArguments, methodGroupResolution.MethodGroup, isStatement, diagnostics);
			}
		}
		else
		{
			result = CreateBadCall(syntax, methodGroup, methodGroup.ResultKind, analyzedArguments);
		}
		methodGroupResolution.Free();
		return result;
	}

	private BoundExpression BindInvocationExpressionContinued(SyntaxNode node, SyntaxNode expression, string methodName, OverloadResolutionResult<MethodSymbol> result, AnalyzedArguments analyzedArguments, MethodGroup methodGroup, bool isStatement, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		DebugAssertHelper.Assert(methodGroup != null, node);
		DebugAssertHelper.Assert(methodGroup.Error == null, node);
		DebugAssertHelper.Assert(methodGroup.Methods.Count > 0, node);
		if (!result.Succeeded)
		{
			if (!analyzedArguments.HasErrors)
			{
				result.ReportDiagnostics(GetLocationForOverloadResolutionDiagnostic(node, expression), diagnostics, methodName, analyzedArguments, methodGroup.Methods.ToImmutable());
			}
			return CreateBadCall(node, methodGroup.Name, methodGroup.Receiver, GetOriginalMethods(result), methodGroup.ResultKind, analyzedArguments);
		}
		MemberResolutionResult<MethodSymbol> validResult = result.ValidResult;
		TypeSymbol returnType = validResult.Member.ReturnType;
		CoerceArguments(validResult, analyzedArguments.Arguments, diagnostics);
		MethodSymbol member = validResult.Member;
		BoundExpression receiver = methodGroup.Receiver;
		bool flag = false;
		ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, member, node);
		ImmutableArray<BoundExpression> arguments = analyzedArguments.Arguments.ToImmutable();
		TypeSymbol type = returnType;
		bool hasErrors = flag;
		bool isStatement2 = isStatement;
		BoundCall boundCall = new BoundCall(node, receiver, member, arguments, validResult.Result.EffectiveParameters, LookupResultKind.Viable, isStatement2, type, hasErrors);
		if (member.ReturnValue != null && member.ReturnValue.TypeFromFirstArgument && analyzedArguments.Arguments.Count > 0)
		{
			returnType = analyzedArguments.Argument(0).SkipConversion().Type;
			return GenerateConversionForAssignment(returnType, boundCall, diagnostics);
		}
		return boundCall;
	}

	private static TypeSymbol GetCommonTypeOrReturnType<TMember>(ImmutableArray<TMember> members) where TMember : Symbol
	{
		TypeSymbol typeSymbol = null;
		int i = 0;
		for (int length = members.Length; i < length; i++)
		{
			TypeSymbol typeOrReturnType = members[i].GetTypeOrReturnType();
			if ((object)typeSymbol == null)
			{
				typeSymbol = typeOrReturnType;
			}
			else if (typeSymbol != typeOrReturnType)
			{
				return null;
			}
		}
		return typeSymbol;
	}

	private static Location GetLocationForOverloadResolutionDiagnostic(SyntaxNode node, SyntaxNode expression)
	{
		if (node != expression)
		{
			SyntaxKind kind = expression.Kind;
			if (kind == SyntaxKind.MemberAccessExpression)
			{
				return ((MemberAccessExpressionSyntax)expression).Name.GetLocation();
			}
		}
		return expression.GetLocation();
	}

	private BoundExpression BindMethodGroup(ExpressionSyntax node, bool invoked, DiagnosticBag diagnostics)
	{
		return node.Kind switch
		{
			SyntaxKind.IdentifierName => BindIdentifier((IdentifierNameSyntax)node, invoked, diagnostics), 
			SyntaxKind.MemberAccessExpression => BindMemberAccess((MemberAccessExpressionSyntax)node, invoked, isStatement: false, diagnostics), 
			SyntaxKind.ParenthesizedExpression => BindMethodGroup(((ParenthesizedExpressionSyntax)node).Expression, invoked: false, diagnostics), 
			_ => BindExpression(node, diagnostics, invoked), 
		};
	}

	private static ImmutableArray<MethodSymbol> GetOriginalMethods(OverloadResolutionResult<MethodSymbol> overloadResolutionResult)
	{
		if (overloadResolutionResult == null)
		{
			return ImmutableArray<MethodSymbol>.Empty;
		}
		ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
		foreach (MemberResolutionResult<MethodSymbol> result in overloadResolutionResult.Results)
		{
			instance.Add(result.Member);
		}
		return instance.ToImmutableAndFree();
	}

	internal void AddLookupSymbolsInfo(LookupSymbolsInfo result, LookupOptions options = LookupOptions.Default)
	{
		for (Binder binder = this; binder != null; binder = binder.Next)
		{
			binder.AddLookupSymbolsInfoInSingleBinder(result, options, this);
		}
	}

	internal static void AddMemberLookupSymbolsInfo(LookupSymbolsInfo result, TypeSymbol type, LookupOptions options, Binder originalBinder)
	{
		switch (type.TypeCategory)
		{
		case TypeCategoryKind.ApplicationObject:
			if (type.Kind.IsExtensionObject())
			{
				AddMemberLookupSymbolsInfoInApplicationObjectExtension(result, type, options, originalBinder, type);
			}
			else
			{
				AddMemberLookupSymbolsInfoInClass(result, type, options, originalBinder, type);
			}
			break;
		case TypeCategoryKind.Complex:
		case TypeCategoryKind.Option:
		case TypeCategoryKind.QueryElement:
		case TypeCategoryKind.ReportDataElement:
		case TypeCategoryKind.RequestPage:
			AddMemberLookupSymbolsInfoInClass(result, type, options, originalBinder, type);
			break;
		}
	}

	internal static void AddMemberLookupSymbolsInfoInExtensions(LookupSymbolsInfo result, TypeSymbol type, LookupOptions options, Binder originalBinder)
	{
		IApplicationObjectTypeSymbol symbol = type as IApplicationObjectTypeSymbol;
		foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in originalBinder.Compilation.GetApplicationObjectExtensionTypeSymbolsAcrossModules(symbol))
		{
			AddMemberLookupSymbolsInfoWithoutInheritance(result, applicationObjectExtensionTypeSymbolsAcrossModule as TypeSymbol, options, originalBinder, type);
		}
	}

	protected virtual void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo info, LookupOptions options, Binder originalBinder)
	{
	}

	protected static void LookupMembersInternal(LookupResult result, ContainerSymbol container, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, BoundExpression receiver = null, ApplicationObjectExtensionTypeSymbol excludedExtension = null)
	{
		DebugAssertHelper.Assert(options.AreValid());
		if (!container.IsNullOrTypeMissing())
		{
			LookupMembersInType(result, (TypeSymbol)container, name, receiver, options, originalBinder, diagnose, ref useSiteDiagnostics, excludedExtension);
		}
	}

	protected static void LookupMembersInType(LookupResult result, TypeSymbol type, string name, BoundExpression receiver, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ApplicationObjectExtensionTypeSymbol excludedExtension = null)
	{
		if (type == null)
		{
			return;
		}
		switch (type.TypeCategory)
		{
		case TypeCategoryKind.ApplicationObject:
			if ((options & LookupOptions.MustBeOption) != 0)
			{
				IEnumerable<ApplicationObjectTypeSymbol> source = originalBinder.Compilation.GetObjectSymbolsByNameAcrossModules(type.NavTypeKind.ToSymbolKind(), name).Cast<ApplicationObjectTypeSymbol>();
				if (source.HasMoreThan(1))
				{
					ApplicationObjectTypeSymbol applicationObjectTypeSymbol = source.First();
					result.MergeEqual(LookupResult.Ambiguous(applicationObjectTypeSymbol, new NavDiagnosticInfo(ErrorCode.ERR_AmbigReference, applicationObjectTypeSymbol.Name)));
					break;
				}
				ApplicationObjectTypeSymbol applicationObjectTypeSymbol2 = source.FirstOrDefault();
				if (applicationObjectTypeSymbol2 != null)
				{
					result.MergeEqual(LookupResult.Good(applicationObjectTypeSymbol2, receiver));
				}
			}
			else
			{
				LookupMembersInClass(result, type, name, receiver, options, originalBinder, diagnose, ref useSiteDiagnostics, excludedExtension);
			}
			break;
		case TypeCategoryKind.Complex:
		case TypeCategoryKind.Option:
		case TypeCategoryKind.QueryElement:
		case TypeCategoryKind.ReportDataElement:
		case TypeCategoryKind.RequestPage:
			LookupMembersInClass(result, type, name, receiver, options, originalBinder, diagnose, ref useSiteDiagnostics, excludedExtension);
			break;
		case TypeCategoryKind.Error:
			LookupMembersInErrorType(result, (ErrorTypeSymbol)type, name, receiver, options, originalBinder, diagnose, ref useSiteDiagnostics);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(type.TypeCategory);
		case TypeCategoryKind.Simple:
		case TypeCategoryKind.Array:
		case TypeCategoryKind.Module:
			break;
		}
	}

	private static void LookupMembersInErrorType(LookupResult result, ErrorTypeSymbol errorType, string name, BoundExpression receiver, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!errorType.CandidateSymbols.IsDefault && errorType.CandidateSymbols.Length == 1 && errorType.ResultKind == LookupResultKind.Inaccessible && errorType.CandidateSymbols.First() is TypeSymbol type)
		{
			LookupMembersInType(result, type, name, receiver, options, originalBinder, diagnose, ref useSiteDiagnostics);
		}
		else
		{
			result.Clear();
		}
	}

	private static void LookupMembersInClass(LookupResult result, TypeSymbol type, string name, BoundExpression receiver, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, ApplicationObjectExtensionTypeSymbol excludedExtension = null)
	{
		DebugAssertHelper.Assert((object)type != null);
		TypeSymbol typeSymbol = type;
		LookupResult instance = LookupResult.GetInstance();
		while ((object)typeSymbol != null)
		{
			instance.Clear();
			LookupMembersWithoutInheritance(instance, typeSymbol, name, options, originalBinder, type, diagnose, ref useSiteDiagnostics, receiver);
			LookupMembersFromExtensions(instance, typeSymbol, name, options, originalBinder, type, diagnose, ref useSiteDiagnostics, receiver, excludedExtension);
			MergeHidingLookupResults(result, instance);
			bool flag = instance.IsMultiViable && !IsMethodOrTrigger(instance.Symbols[0]);
			if (result.IsMultiViable && (flag || !IsMethodOrTrigger(result.Symbols[0])))
			{
				break;
			}
			typeSymbol = typeSymbol.BaseType;
		}
		instance.Free();
	}

	private static void AddMemberLookupSymbolsInfoInClass(LookupSymbolsInfo result, TypeSymbol type, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType)
	{
		while (type != null)
		{
			AddMemberLookupSymbolsInfoWithoutInheritance(result, type, options, originalBinder, accessThroughType);
			AddMemberLookupSymbolsInfoInExtensions(result, type, options, originalBinder);
			type = type.BaseType;
		}
	}

	private static void AddMemberLookupSymbolsInfoInApplicationObjectExtension(LookupSymbolsInfo result, TypeSymbol type, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType)
	{
		ApplicationObjectTypeSymbol type2 = (type as ApplicationObjectExtensionTypeSymbol)?.Target;
		while (type != null)
		{
			AddMemberLookupSymbolsInfoWithoutInheritance(result, type, options, originalBinder, accessThroughType);
			type = type.BaseType;
		}
		AddMemberLookupSymbolsInfoInClass(result, type2, options, originalBinder, accessThroughType);
	}

	private static void MergeHidingLookupResults(LookupResult resultHiding, LookupResult resultHidden)
	{
		if (resultHiding.IsMultiViable && resultHidden.IsMultiViable)
		{
			ArrayBuilder<Symbol> symbols = resultHiding.Symbols;
			int count = symbols.Count;
			ArrayBuilder<Symbol> symbols2 = resultHidden.Symbols;
			int count2 = symbols2.Count;
			for (int i = 0; i < count2; i++)
			{
				Symbol symbol = symbols2[i];
				bool flag = false;
				int num = 0;
				while (!flag && num < count)
				{
					if (!IsMethodOrTrigger(symbols[num]) || !IsMethodOrTrigger(symbol))
					{
						flag = true;
						break;
					}
					num++;
				}
				if (!flag)
				{
					symbols.Add(symbol);
				}
			}
		}
		else
		{
			resultHiding.MergePrioritized(resultHidden);
		}
	}

	protected static void LookupMembersWithoutInheritance(LookupResult result, TypeSymbol type, string name, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, BoundExpression receiver = null)
	{
		foreach (Symbol candidateMember in GetCandidateMembers(type, name))
		{
			SingleLookupResult result2 = originalBinder.CheckViability(candidateMember, options, accessThroughType, diagnose, ref useSiteDiagnostics, receiver);
			result.MergeEqual(result2);
		}
	}

	protected static void LookupMembersFromExtensions(LookupResult result, TypeSymbol type, string name, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics, BoundExpression receiver = null, ApplicationObjectExtensionTypeSymbol excludedExtension = null)
	{
		if (!(type is IApplicationObjectTypeSymbol symbol))
		{
			return;
		}
		ImmutableArray<IApplicationObjectExtensionTypeSymbol> applicationObjectExtensionTypeSymbolsAcrossModules = originalBinder.Compilation.GetApplicationObjectExtensionTypeSymbolsAcrossModules(symbol);
		if (!(applicationObjectExtensionTypeSymbolsAcrossModules != null))
		{
			return;
		}
		foreach (IApplicationObjectExtensionTypeSymbol item in applicationObjectExtensionTypeSymbolsAcrossModules)
		{
			if (!(excludedExtension == (Symbol)item))
			{
				foreach (Symbol candidateMember in GetCandidateMembers((ApplicationObjectTypeSymbol)item, name))
				{
					SingleLookupResult result2 = originalBinder.CheckViability(candidateMember, options, accessThroughType, diagnose, ref useSiteDiagnostics, receiver);
					result.MergeEqual(result2);
				}
			}
		}
	}

	private static void AddMemberLookupSymbolsInfoWithoutInheritance(LookupSymbolsInfo result, TypeSymbol type, LookupOptions options, Binder originalBinder, TypeSymbol accessThroughType)
	{
		foreach (Symbol codeMember in type.GetCodeMembers())
		{
			if (originalBinder.CanAddLookupSymbolInfo(codeMember, options, accessThroughType))
			{
				result.AddSymbol(codeMember, codeMember.Name, 0);
			}
		}
	}

	internal static ImmutableArray<Symbol> GetCandidateMembers(ContainerSymbol container, string name)
	{
		if (name != null)
		{
			return container.GetCodeMembers(name);
		}
		return container.GetCodeMembers();
	}

	internal static void ValidateForDuplicateNames(IEnumerable<IdentifierNameSyntax> syntaxNodes, DiagnosticBag diagnostic, ErrorCode errorCode = ErrorCode.ERR_DuplicateListMember)
	{
		MultiDictionary<string, IdentifierNameSyntax> multiDictionary = new MultiDictionary<string, IdentifierNameSyntax>();
		foreach (IdentifierNameSyntax syntaxNode in syntaxNodes)
		{
			string k = syntaxNode.Unquoted().ToUpperInvariant();
			multiDictionary.Add(k, syntaxNode);
		}
		foreach (KeyValuePair<string, MultiDictionary<string, IdentifierNameSyntax>.ValueSet> item in multiDictionary)
		{
			if (item.Key == null || item.Value.Count <= 1)
			{
				continue;
			}
			foreach (IdentifierNameSyntax item2 in item.Value)
			{
				diagnostic.Add(errorCode, item2.GetLocation(), item2.Unquoted());
			}
		}
	}

	protected static bool IsSimpleBinaryOperator(SyntaxKind kind)
	{
		if ((uint)(kind - 194) <= 11u || kind == SyntaxKind.RangeExpression)
		{
			return true;
		}
		return false;
	}

	private BoundExpression BindSimpleBinaryOperator(BinaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(IsSimpleBinaryOperator(node.Kind));
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ArrayBuilder<BinaryExpressionSyntax> instance2 = ArrayBuilder<BinaryExpressionSyntax>.GetInstance();
		ExpressionSyntax expressionSyntax = node;
		while (IsSimpleBinaryOperator(expressionSyntax.Kind))
		{
			BinaryExpressionSyntax binaryExpressionSyntax = (BinaryExpressionSyntax)expressionSyntax;
			instance2.Push(binaryExpressionSyntax);
			instance.Push(BindValue(binaryExpressionSyntax.Right, diagnostics, BindValueKind.RValue));
			expressionSyntax = binaryExpressionSyntax.Left;
		}
		BoundExpression e = BindExpression(expressionSyntax, diagnostics);
		instance.Push(e);
		DebugAssertHelper.Assert(instance2.Count + 1 == instance.Count);
		while (instance2.Count > 0)
		{
			BinaryExpressionSyntax binaryExpressionSyntax2 = instance2.Pop();
			BindValueKind valueKind = BindValueKind.RValue;
			BoundExpression expr = ApplyFromDotNetConversionIfNeeded(instance.Pop());
			BoundExpression right = ApplyFromDotNetConversionIfNeeded(instance.Pop());
			expr = CheckValue(expr, valueKind, diagnostics);
			BoundExpression e2 = BindSimpleBinaryOperator(binaryExpressionSyntax2, binaryExpressionSyntax2.OperatorToken, diagnostics, expr, right);
			instance.Push(e2);
		}
		DebugAssertHelper.Assert(instance.Count == 1);
		BoundExpression result = instance.Peek();
		instance.Free();
		instance2.Free();
		return result;
	}

	protected BoundExpression ApplyFromDotNetConversionIfNeeded(BoundExpression boundExpression)
	{
		if (boundExpression.Kind == BoundKind.BadExpression)
		{
			return boundExpression;
		}
		if (!ConversionDotNet.IsStronglyTypedDotNetSymbol(boundExpression.Type))
		{
			return boundExpression;
		}
		if (!ConversionDotNet.TryGetMatchingAlType(boundExpression.Type, out var alTypeSymbol))
		{
			return boundExpression;
		}
		return new BoundConversion(boundExpression.Syntax, boundExpression, ConversionKind.PreservingFromDotNet, alTypeSymbol, null);
	}

	private BoundExpression BindSimpleBinaryOperator(SyntaxNode node, SyntaxToken operatorToken, DiagnosticBag diagnostics, BoundExpression left, BoundExpression right)
	{
		BinaryOperatorKind binaryOperatorKind = ((!SyntaxFacts.IsCompoundAssignmentOperatorToken(operatorToken.Kind)) ? SyntaxKindToBinaryOperatorKind(node.Kind) : CompoundAssignmentOperatorKindToBinaryOperatorType(SyntaxKindToCompoundAssignmentOperatorKind(operatorToken.Kind)));
		if (left.HasAnyErrors || right.HasAnyErrors)
		{
			return new BoundBinaryOperator(node, binaryOperatorKind, left, right, null, null, LookupResultKind.Empty, GetBinaryOperatorErrorType(binaryOperatorKind, diagnostics, node), hasErrors: true);
		}
		BinaryOperatorAnalysisResult binaryOperatorAnalysisResult = BinaryOperatorOverloadResolution(binaryOperatorKind, left, right, node, diagnostics, out var resultKind, out var _);
		BoundExpression left2 = left;
		BoundExpression right2 = right;
		MethodSymbol methodOpt = null;
		ConstantValue constantValue = null;
		bool flag = false;
		BinaryOperatorKind binaryOperatorKind2;
		TypeSymbol typeSymbol;
		if (!binaryOperatorAnalysisResult.HasValue)
		{
			binaryOperatorKind2 = binaryOperatorKind;
			typeSymbol = CreateErrorType();
			flag = true;
		}
		else
		{
			BinaryOperatorSignature signature = binaryOperatorAnalysisResult.Signature;
			binaryOperatorKind2 = signature.Kind;
			typeSymbol = signature.ReturnType;
			methodOpt = signature.Method;
			left2 = CreateConversion(left, binaryOperatorAnalysisResult.LeftConversion, signature.LeftType, null, diagnostics);
			right2 = CreateConversion(right, binaryOperatorAnalysisResult.RightConversion, signature.RightType, null, diagnostics);
			constantValue = FoldBinaryOperator(node, binaryOperatorKind2, left2, right2, typeSymbol.NavTypeKind, diagnostics);
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			diagnostics.Add(node, useSiteDiagnostics);
		}
		if (flag)
		{
			ReportBinaryOperatorError(node, diagnostics, operatorToken, left, right, resultKind);
			binaryOperatorKind2 &= ~BinaryOperatorKind.TypeMask;
		}
		if ((object)constantValue != null && !constantValue.IsBad)
		{
			return new BoundLiteral(node, constantValue, typeSymbol);
		}
		return new BoundBinaryOperator(node, binaryOperatorKind2, left2, right2, constantValue, methodOpt, resultKind, typeSymbol, flag);
	}

	private static ConstantValue FoldBinaryOperator(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression left, BoundExpression right, NavTypeKind resultType, DiagnosticBag diagnostics)
	{
		if (left.HasAnyErrors || right.HasAnyErrors)
		{
			return null;
		}
		ConstantValue constantValue = left.ConstantValue;
		ConstantValue constantValue2 = right.ConstantValue;
		if (constantValue == null || constantValue2 == null)
		{
			return null;
		}
		if (constantValue.IsBad || constantValue2.IsBad)
		{
			return ConstantValue.Bad;
		}
		if (IsDivisionByZero(kind, constantValue2))
		{
			Error(diagnostics, ErrorCode.ERR_DivByZero, syntax);
			return ConstantValue.Bad;
		}
		object obj = FoldNeverOverflowBinaryOperators(kind, constantValue, constantValue2);
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		ConstantValue constantValue3 = FoldStringConcatenation(kind, constantValue, constantValue2);
		if (constantValue3 != null)
		{
			if (constantValue3.IsBad)
			{
				Error(diagnostics, ErrorCode.ERR_ConstantStringTooLong, syntax);
			}
			return constantValue3;
		}
		try
		{
			obj = FoldDecimalBinaryOperators(kind, constantValue, constantValue2);
		}
		catch (OverflowException)
		{
			Error(diagnostics, ErrorCode.ERR_CheckedOverflow, syntax);
			return ConstantValue.Bad;
		}
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		try
		{
			obj = FoldCheckedIntegralBinaryOperator(kind, constantValue, constantValue2);
		}
		catch (OverflowException)
		{
			Error(diagnostics, ErrorCode.ERR_CheckedOverflow, syntax);
			return ConstantValue.Bad;
		}
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		return null;
	}

	private static bool IsDivisionByZero(BinaryOperatorKind kind, ConstantValue valueRight)
	{
		switch (kind)
		{
		case BinaryOperatorKind.DecimalDivision:
		case BinaryOperatorKind.DecimalRemainder:
			return valueRight.DecimalValue == 0.0m;
		case BinaryOperatorKind.IntDivision:
		case BinaryOperatorKind.IntRemainder:
			return valueRight.Int32Value == 0;
		case BinaryOperatorKind.LongDivision:
		case BinaryOperatorKind.LongRemainder:
			return valueRight.Int64Value == 0;
		default:
			return false;
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
	private static object FoldNeverOverflowBinaryOperators(BinaryOperatorKind kind, ConstantValue valueLeft, ConstantValue valueRight)
	{
		return kind switch
		{
			BinaryOperatorKind.DoubleAddition => (object)(valueLeft.DoubleValue + valueRight.DoubleValue), 
			BinaryOperatorKind.FloatAddition => valueLeft.SingleValue + valueRight.SingleValue, 
			BinaryOperatorKind.DoubleSubtraction => valueLeft.DoubleValue - valueRight.DoubleValue, 
			BinaryOperatorKind.FloatSubtraction => valueLeft.SingleValue - valueRight.SingleValue, 
			BinaryOperatorKind.DoubleMultiplication => valueLeft.DoubleValue * valueRight.DoubleValue, 
			BinaryOperatorKind.FloatMultiplication => valueLeft.SingleValue * valueRight.SingleValue, 
			BinaryOperatorKind.DoubleDivision => valueLeft.DoubleValue / valueRight.DoubleValue, 
			BinaryOperatorKind.FloatDivision => valueLeft.SingleValue / valueRight.SingleValue, 
			BinaryOperatorKind.DoubleRemainder => valueLeft.DoubleValue % valueRight.DoubleValue, 
			BinaryOperatorKind.FloatRemainder => valueLeft.SingleValue % valueRight.SingleValue, 
			BinaryOperatorKind.BoolAnd => valueLeft.BooleanValue & valueRight.BooleanValue, 
			BinaryOperatorKind.IntAnd => valueLeft.Int32Value & valueRight.Int32Value, 
			BinaryOperatorKind.LongAnd => valueLeft.Int64Value & valueRight.Int64Value, 
			BinaryOperatorKind.UIntAnd => valueLeft.UInt32Value & valueRight.UInt32Value, 
			BinaryOperatorKind.ULongAnd => valueLeft.UInt64Value & valueRight.UInt64Value, 
			BinaryOperatorKind.BoolOr => valueLeft.BooleanValue | valueRight.BooleanValue, 
			BinaryOperatorKind.IntOr => valueLeft.Int32Value | valueRight.Int32Value, 
			BinaryOperatorKind.LongOr => valueLeft.Int64Value | valueRight.Int64Value, 
			BinaryOperatorKind.UIntOr => valueLeft.UInt32Value | valueRight.UInt32Value, 
			BinaryOperatorKind.ULongOr => valueLeft.UInt64Value | valueRight.UInt64Value, 
			BinaryOperatorKind.BoolXor => valueLeft.BooleanValue ^ valueRight.BooleanValue, 
			BinaryOperatorKind.IntXor => valueLeft.Int32Value ^ valueRight.Int32Value, 
			BinaryOperatorKind.LongXor => valueLeft.Int64Value ^ valueRight.Int64Value, 
			BinaryOperatorKind.UIntXor => valueLeft.UInt32Value ^ valueRight.UInt32Value, 
			BinaryOperatorKind.ULongXor => valueLeft.UInt64Value ^ valueRight.UInt64Value, 
			BinaryOperatorKind.LogicalBoolAnd => valueLeft.BooleanValue && valueRight.BooleanValue, 
			BinaryOperatorKind.LogicalBoolOr => valueLeft.BooleanValue || valueRight.BooleanValue, 
			BinaryOperatorKind.BoolEqual => valueLeft.BooleanValue == valueRight.BooleanValue, 
			BinaryOperatorKind.StringEqual => valueLeft.StringValue == valueRight.StringValue, 
			BinaryOperatorKind.DecimalEqual => valueLeft.DecimalValue == valueRight.DecimalValue, 
			BinaryOperatorKind.FloatEqual => valueLeft.SingleValue == valueRight.SingleValue, 
			BinaryOperatorKind.DoubleEqual => valueLeft.DoubleValue == valueRight.DoubleValue, 
			BinaryOperatorKind.IntEqual => valueLeft.Int32Value == valueRight.Int32Value, 
			BinaryOperatorKind.LongEqual => valueLeft.Int64Value == valueRight.Int64Value, 
			BinaryOperatorKind.UIntEqual => valueLeft.UInt32Value == valueRight.UInt32Value, 
			BinaryOperatorKind.ULongEqual => valueLeft.UInt64Value == valueRight.UInt64Value, 
			BinaryOperatorKind.BoolNotEqual => valueLeft.BooleanValue != valueRight.BooleanValue, 
			BinaryOperatorKind.StringNotEqual => valueLeft.StringValue != valueRight.StringValue, 
			BinaryOperatorKind.DecimalNotEqual => valueLeft.DecimalValue != valueRight.DecimalValue, 
			BinaryOperatorKind.FloatNotEqual => valueLeft.SingleValue != valueRight.SingleValue, 
			BinaryOperatorKind.DoubleNotEqual => valueLeft.DoubleValue != valueRight.DoubleValue, 
			BinaryOperatorKind.IntNotEqual => valueLeft.Int32Value != valueRight.Int32Value, 
			BinaryOperatorKind.LongNotEqual => valueLeft.Int64Value != valueRight.Int64Value, 
			BinaryOperatorKind.UIntNotEqual => valueLeft.UInt32Value != valueRight.UInt32Value, 
			BinaryOperatorKind.ULongNotEqual => valueLeft.UInt64Value != valueRight.UInt64Value, 
			BinaryOperatorKind.DecimalLessThan => valueLeft.DecimalValue < valueRight.DecimalValue, 
			BinaryOperatorKind.FloatLessThan => valueLeft.SingleValue < valueRight.SingleValue, 
			BinaryOperatorKind.DoubleLessThan => valueLeft.DoubleValue < valueRight.DoubleValue, 
			BinaryOperatorKind.IntLessThan => valueLeft.Int32Value < valueRight.Int32Value, 
			BinaryOperatorKind.LongLessThan => valueLeft.Int64Value < valueRight.Int64Value, 
			BinaryOperatorKind.UIntLessThan => valueLeft.UInt32Value < valueRight.UInt32Value, 
			BinaryOperatorKind.ULongLessThan => valueLeft.UInt64Value < valueRight.UInt64Value, 
			BinaryOperatorKind.DecimalGreaterThan => valueLeft.DecimalValue > valueRight.DecimalValue, 
			BinaryOperatorKind.FloatGreaterThan => valueLeft.SingleValue > valueRight.SingleValue, 
			BinaryOperatorKind.DoubleGreaterThan => valueLeft.DoubleValue > valueRight.DoubleValue, 
			BinaryOperatorKind.IntGreaterThan => valueLeft.Int32Value > valueRight.Int32Value, 
			BinaryOperatorKind.LongGreaterThan => valueLeft.Int64Value > valueRight.Int64Value, 
			BinaryOperatorKind.UIntGreaterThan => valueLeft.UInt32Value > valueRight.UInt32Value, 
			BinaryOperatorKind.ULongGreaterThan => valueLeft.UInt64Value > valueRight.UInt64Value, 
			BinaryOperatorKind.DecimalLessThanOrEqual => valueLeft.DecimalValue <= valueRight.DecimalValue, 
			BinaryOperatorKind.FloatLessThanOrEqual => valueLeft.SingleValue <= valueRight.SingleValue, 
			BinaryOperatorKind.DoubleLessThanOrEqual => valueLeft.DoubleValue <= valueRight.DoubleValue, 
			BinaryOperatorKind.IntLessThanOrEqual => valueLeft.Int32Value <= valueRight.Int32Value, 
			BinaryOperatorKind.LongLessThanOrEqual => valueLeft.Int64Value <= valueRight.Int64Value, 
			BinaryOperatorKind.UIntLessThanOrEqual => valueLeft.UInt32Value <= valueRight.UInt32Value, 
			BinaryOperatorKind.ULongLessThanOrEqual => valueLeft.UInt64Value <= valueRight.UInt64Value, 
			BinaryOperatorKind.DecimalGreaterThanOrEqual => valueLeft.DecimalValue >= valueRight.DecimalValue, 
			BinaryOperatorKind.FloatGreaterThanOrEqual => valueLeft.SingleValue >= valueRight.SingleValue, 
			BinaryOperatorKind.DoubleGreaterThanOrEqual => valueLeft.DoubleValue >= valueRight.DoubleValue, 
			BinaryOperatorKind.IntGreaterThanOrEqual => valueLeft.Int32Value >= valueRight.Int32Value, 
			BinaryOperatorKind.LongGreaterThanOrEqual => valueLeft.Int64Value >= valueRight.Int64Value, 
			BinaryOperatorKind.UIntGreaterThanOrEqual => valueLeft.UInt32Value >= valueRight.UInt32Value, 
			BinaryOperatorKind.ULongGreaterThanOrEqual => valueLeft.UInt64Value >= valueRight.UInt64Value, 
			BinaryOperatorKind.UIntDivision => (decimal)valueLeft.UInt32Value / (decimal)valueRight.UInt32Value, 
			BinaryOperatorKind.ULongDivision => (decimal)valueLeft.UInt64Value / (decimal)valueRight.UInt64Value, 
			BinaryOperatorKind.IntRemainder => (valueRight.Int32Value != -1) ? (valueLeft.Int32Value % valueRight.Int32Value) : 0, 
			BinaryOperatorKind.LongRemainder => (valueRight.Int64Value != -1) ? (valueLeft.Int64Value % valueRight.Int64Value) : 0, 
			BinaryOperatorKind.UIntRemainder => valueLeft.UInt32Value % valueRight.UInt32Value, 
			BinaryOperatorKind.ULongRemainder => valueLeft.UInt64Value % valueRight.UInt64Value, 
			_ => null, 
		};
	}

	private static ConstantValue FoldStringConcatenation(BinaryOperatorKind kind, ConstantValue valueLeft, ConstantValue valueRight)
	{
		if (kind == BinaryOperatorKind.StringConcatenation)
		{
			string text = valueLeft.StringValue ?? string.Empty;
			string text2 = valueRight.StringValue ?? string.Empty;
			if ((long)text.Length + (long)text.Length + text2.Length > int.MaxValue)
			{
				return ConstantValue.Bad;
			}
			try
			{
				return ConstantValue.Create(text + text2);
			}
			catch (OutOfMemoryException)
			{
				return ConstantValue.Bad;
			}
		}
		return null;
	}

	private static object FoldDecimalBinaryOperators(BinaryOperatorKind kind, ConstantValue valueLeft, ConstantValue valueRight)
	{
		return kind switch
		{
			BinaryOperatorKind.DecimalAddition => (object)(valueLeft.DecimalValue + valueRight.DecimalValue), 
			BinaryOperatorKind.DecimalSubtraction => valueLeft.DecimalValue - valueRight.DecimalValue, 
			BinaryOperatorKind.DecimalMultiplication => valueLeft.DecimalValue * valueRight.DecimalValue, 
			BinaryOperatorKind.DecimalDivision => valueLeft.DecimalValue / valueRight.DecimalValue, 
			BinaryOperatorKind.DecimalRemainder => valueLeft.DecimalValue % valueRight.DecimalValue, 
			_ => null, 
		};
	}

	private static object FoldCheckedIntegralBinaryOperator(BinaryOperatorKind kind, ConstantValue valueLeft, ConstantValue valueRight)
	{
		return checked(kind switch
		{
			BinaryOperatorKind.IntAddition => (object)(valueLeft.Int32Value + valueRight.Int32Value), 
			BinaryOperatorKind.LongAddition => valueLeft.Int64Value + valueRight.Int64Value, 
			BinaryOperatorKind.UIntAddition => valueLeft.UInt32Value + valueRight.UInt32Value, 
			BinaryOperatorKind.ULongAddition => valueLeft.UInt64Value + valueRight.UInt64Value, 
			BinaryOperatorKind.IntSubtraction => valueLeft.Int32Value - valueRight.Int32Value, 
			BinaryOperatorKind.LongSubtraction => valueLeft.Int64Value - valueRight.Int64Value, 
			BinaryOperatorKind.UIntSubtraction => valueLeft.UInt32Value - valueRight.UInt32Value, 
			BinaryOperatorKind.ULongSubtraction => valueLeft.UInt64Value - valueRight.UInt64Value, 
			BinaryOperatorKind.IntMultiplication => valueLeft.Int32Value * valueRight.Int32Value, 
			BinaryOperatorKind.LongMultiplication => valueLeft.Int64Value * valueRight.Int64Value, 
			BinaryOperatorKind.UIntMultiplication => valueLeft.UInt32Value * valueRight.UInt32Value, 
			BinaryOperatorKind.ULongMultiplication => valueLeft.UInt64Value * valueRight.UInt64Value, 
			BinaryOperatorKind.IntDivision => (decimal)valueLeft.Int32Value / (decimal)valueRight.Int32Value, 
			BinaryOperatorKind.LongDivision => (decimal)valueLeft.Int64Value / (decimal)valueRight.Int64Value, 
			_ => null, 
		});
	}

	private BinaryOperatorAnalysisResult BinaryOperatorOverloadResolution(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, SyntaxNode node, DiagnosticBag diagnostics, out LookupResultKind resultKind, out ImmutableArray<MethodSymbol> originalUserDefinedOperators)
	{
		BinaryOperatorOverloadResolutionResult instance = BinaryOperatorOverloadResolutionResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		OverloadResolution.BinaryOperatorOverloadResolution(kind, left, right, instance, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		BinaryOperatorAnalysisResult best = instance.Best;
		if (instance.Results.Any())
		{
			ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
			foreach (BinaryOperatorAnalysisResult result in instance.Results)
			{
				MethodSymbol method = result.Signature.Method;
				if ((object)method != null)
				{
					instance2.Add(method);
				}
			}
			originalUserDefinedOperators = instance2.ToImmutableAndFree();
			if (best.HasValue)
			{
				resultKind = LookupResultKind.Viable;
			}
			else if (instance.AnyValid())
			{
				resultKind = LookupResultKind.Ambiguous;
			}
			else
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
		}
		else
		{
			originalUserDefinedOperators = ImmutableArray<MethodSymbol>.Empty;
			resultKind = (best.HasValue ? LookupResultKind.Viable : LookupResultKind.Empty);
		}
		if (best.HasValue && (object)best.Signature.Method != null)
		{
			Symbol method2 = best.Signature.Method;
			ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, method2, node);
		}
		instance.Free();
		return best;
	}

	private static UnaryOperatorKind SyntaxKindToUnaryOperatorKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.UnaryPlusExpression => UnaryOperatorKind.UnaryPlus, 
			SyntaxKind.UnaryMinusExpression => UnaryOperatorKind.UnaryMinus, 
			SyntaxKind.UnaryNotExpression => UnaryOperatorKind.LogicalNegation, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	private static BinaryOperatorKind SyntaxKindToBinaryOperatorKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.MultiplyExpression => BinaryOperatorKind.Multiplication, 
			SyntaxKind.IntegerDivideExpression => BinaryOperatorKind.IDivision, 
			SyntaxKind.DivideExpression => BinaryOperatorKind.Division, 
			SyntaxKind.ModuloExpression => BinaryOperatorKind.Remainder, 
			SyntaxKind.AddExpression => BinaryOperatorKind.Addition, 
			SyntaxKind.SubtractExpression => BinaryOperatorKind.Subtraction, 
			SyntaxKind.EqualsExpression => BinaryOperatorKind.Equal, 
			SyntaxKind.NotEqualsExpression => BinaryOperatorKind.NotEqual, 
			SyntaxKind.GreaterThanExpression => BinaryOperatorKind.GreaterThan, 
			SyntaxKind.LessThanExpression => BinaryOperatorKind.LessThan, 
			SyntaxKind.GreaterThanOrEqualExpression => BinaryOperatorKind.GreaterThanOrEqual, 
			SyntaxKind.LessThanOrEqualExpression => BinaryOperatorKind.LessThanOrEqual, 
			SyntaxKind.LogicalAndExpression => BinaryOperatorKind.LogicalAnd, 
			SyntaxKind.LogicalOrExpression => BinaryOperatorKind.LogicalOr, 
			SyntaxKind.LogicalXorExpression => BinaryOperatorKind.LogicalXor, 
			SyntaxKind.RangeExpression => BinaryOperatorKind.Range, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	private static CompoundAssignmentOperatorKind SyntaxKindToCompoundAssignmentOperatorKind(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.AssignPlusToken => CompoundAssignmentOperatorKind.Addition, 
			SyntaxKind.AssignMinusToken => CompoundAssignmentOperatorKind.Subtraction, 
			SyntaxKind.AssignMultiplyToken => CompoundAssignmentOperatorKind.Multiplication, 
			SyntaxKind.AssignRDivToken => CompoundAssignmentOperatorKind.Division, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	private static BinaryOperatorKind CompoundAssignmentOperatorKindToBinaryOperatorType(CompoundAssignmentOperatorKind kind)
	{
		return kind switch
		{
			CompoundAssignmentOperatorKind.Addition => BinaryOperatorKind.Addition, 
			CompoundAssignmentOperatorKind.Subtraction => BinaryOperatorKind.Subtraction, 
			CompoundAssignmentOperatorKind.Multiplication => BinaryOperatorKind.Multiplication, 
			CompoundAssignmentOperatorKind.Division => BinaryOperatorKind.Division, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}

	private TypeSymbol GetBinaryOperatorErrorType(BinaryOperatorKind kind, DiagnosticBag diagnostics, SyntaxNode node)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
			return GetNavType(NavTypeKind.Boolean, diagnostics, node);
		default:
			return CreateErrorType();
		}
	}

	private static void ReportBinaryOperatorError(SyntaxNode node, DiagnosticBag diagnostics, SyntaxToken operatorToken, BoundExpression left, BoundExpression right, LookupResultKind resultKind)
	{
		ErrorCode code = ((resultKind == LookupResultKind.Ambiguous) ? ErrorCode.ERR_AmbigBinaryOps : ErrorCode.ERR_BadBinaryOps);
		Error(diagnostics, code, node, operatorToken.Text, left.Display, right.Display);
	}

	private UnaryOperatorAnalysisResult UnaryOperatorOverloadResolution(UnaryOperatorKind kind, BoundExpression operand, SyntaxNode node, DiagnosticBag diagnostics, out LookupResultKind resultKind, out ImmutableArray<MethodSymbol> originalUserDefinedOperators)
	{
		UnaryOperatorOverloadResolutionResult instance = UnaryOperatorOverloadResolutionResult.GetInstance();
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		OverloadResolution.UnaryOperatorOverloadResolution(kind, operand, instance, ref useSiteDiagnostics);
		diagnostics.Add(node, useSiteDiagnostics);
		UnaryOperatorAnalysisResult best = instance.Best;
		if (instance.Results.Any())
		{
			ArrayBuilder<MethodSymbol> instance2 = ArrayBuilder<MethodSymbol>.GetInstance();
			foreach (UnaryOperatorAnalysisResult result in instance.Results)
			{
				MethodSymbol method = result.Signature.Method;
				if ((object)method != null)
				{
					instance2.Add(method);
				}
			}
			originalUserDefinedOperators = instance2.ToImmutableAndFree();
			if (best.HasValue)
			{
				resultKind = LookupResultKind.Viable;
			}
			else if (instance.AnyValid())
			{
				resultKind = LookupResultKind.Ambiguous;
			}
			else
			{
				resultKind = LookupResultKind.OverloadResolutionFailure;
			}
		}
		else
		{
			originalUserDefinedOperators = ImmutableArray<MethodSymbol>.Empty;
			resultKind = (best.HasValue ? LookupResultKind.Viable : LookupResultKind.Empty);
		}
		if (best.HasValue && (object)best.Signature.Method != null)
		{
			Symbol method2 = best.Signature.Method;
			ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, method2, node);
		}
		instance.Free();
		return best;
	}

	private static void ReportUnaryOperatorError(SyntaxNode node, DiagnosticBag diagnostics, string operatorName, BoundExpression operand, LookupResultKind resultKind)
	{
		ErrorCode code = ((resultKind == LookupResultKind.Ambiguous) ? ErrorCode.ERR_AmbigUnaryOp : ErrorCode.ERR_BadUnaryOp);
		Error(diagnostics, code, node, operatorName, operand.Display);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
	internal BoundProperty BindProperty(PropertySyntax syntax, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(syntax != null);
		DebugAssertHelper.Assert(diagnostics != null);
		PropertyValueSyntax value = syntax.Value;
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			string valueText = syntax.Name.Identifier.ValueText;
			BoundLiteral propertyName = CreateBoundStringLiteral(valueText, syntax.Name, instance);
			if (value == null || value.HasErrors)
			{
				BoundBadPropertyValue propertyValue = new BoundBadPropertyValue(syntax, BadExpression(syntax, LookupResultKind.Empty), null, hasErrors: true);
				BoundBadStatement badProperty = new BoundBadStatement(syntax, ImmutableArray<BoundNode>.Empty, hasErrors: true);
				return new BoundBadProperty(syntax, badProperty, propertyName, propertyValue, hasErrors: true);
			}
			PropertyTypeInfo propertyTypeInfo = value.GetPropertyTypeInfo();
			BoundPropertyValue boundPropertyValue = null;
			if (ValidateProperty(valueText, propertyTypeInfo, syntax, instance))
			{
				boundPropertyValue = GetBoundPropertyValue(syntax, instance, value, propertyTypeInfo);
			}
			if (boundPropertyValue == null)
			{
				if (instance.HasAnyErrors())
				{
					boundPropertyValue = new BoundBadPropertyValue(syntax, BadExpression(syntax, LookupResultKind.Empty), null, hasErrors: true);
				}
				else
				{
					boundPropertyValue = ((!Compilation.Options.Target.IsPersonalizationTarget()) ? CreateBoundBadPropertyValue(ErrorCode.ERR_InvalidPropertyValue, value, value, instance, value.GetPropertyValueText() ?? value.ToString(), valueText) : CreateBoundBadPropertyValue(value));
				}
			}
			if (boundPropertyValue.Kind == BoundKind.BadPropertyValue)
			{
				return new BoundBadProperty(syntax, new BoundBadStatement(syntax, ImmutableArray<BoundNode>.Empty, hasErrors: true), propertyName, boundPropertyValue, hasErrors: true);
			}
			CheckCompilationScope(boundPropertyValue, instance);
			return new BoundProperty(syntax, propertyName, boundPropertyValue, instance.HasAnyErrors() || boundPropertyValue.HasAnyErrors);
		}
		finally
		{
			diagnostics.AddRange(instance);
			instance.Free();
		}
	}

	private BoundPropertyValue GetBoundPropertyValue(PropertySyntax syntax, DiagnosticBag diagnostics, PropertyValueSyntax propertyValue, PropertyTypeInfo info)
	{
		SyntaxKind kind = propertyValue.Kind;
		kind = kind;
		if (!kind.IsCalculationFormulaStatementKind())
		{
			switch ((int)kind)
			{
			case 332:
			{
				CompilerFeatures compilerFeatures = Compilation.Options.CompilerFeatures;
				return BindMultiLanguagePropertyValue((MultilanguagePropertyValueSyntax)propertyValue, compilerFeatures, diagnostics);
			}
			case 333:
				return BindLabelPropertyValue((LabelPropertyValueSyntax)propertyValue, diagnostics);
			case 342:
				return BindTableRelationPropertyValue((TableRelationPropertyValueSyntax)propertyValue, info, diagnostics);
			case 328:
				if (info.IsStringScriptProperty())
				{
					return BindScriptLiteralPropertyValue(propertyValue, info, diagnostics);
				}
				if (info.IsStringLinkProperty())
				{
					return BindLinkLiteralPropertyValue(propertyValue, info, diagnostics);
				}
				if (info.Kind == PropertyKind.ContextSensitiveHelpPage)
				{
					return BindContextHelpLiteralPropertyValue(propertyValue, diagnostics);
				}
				if (info.IsStringAlphaNumericProperty())
				{
					return BindAlphaNumericPropertyValue(propertyValue, diagnostics);
				}
				goto case 325;
			case 325:
			case 326:
			case 327:
			case 329:
			case 330:
			case 331:
			case 335:
				return BindLiteralPropertyValue(propertyValue, diagnostics);
			case 334:
				return BindEnumPropertyValue((EnumPropertyValueSyntax)propertyValue, info as EnumPropertyTypeInfo, diagnostics);
			case 365:
				return BindOptionValuesPropertyValue((OptionValuesPropertyValueSyntax)propertyValue, diagnostics);
			case 366:
				return BindPermissionPropertyValue((PermissionPropertyValueSyntax)propertyValue, diagnostics);
			case 373:
				return BindCommaSeparatedIdentifierEqualsStringListPropertyValue((CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax)propertyValue, diagnostics);
			case 369:
				return BindCommaSeparatedListPropertyValue((CommaSeparatedPropertyValueSyntax)propertyValue, info, diagnostics);
			case 370:
				return BindCommaSeparatedStringsPropertyValue((CommaSeparatedStringsPropertyValueSyntax)propertyValue, info, diagnostics);
			case 378:
				return BindObjectReferencePropertyValue((ObjectReferencePropertyValueSyntax)propertyValue, info, diagnostics);
			case 377:
				return BindQualifiedObjectReferencePropertyValue((QualifiedObjectReferencePropertyValueSyntax)propertyValue, info, diagnostics);
			case 383:
				return BindTextExpressionPropertyValue((TextExpressionPropertyValueSyntax)propertyValue, diagnostics);
			case 380:
			case 381:
				return BindBooleanExpressionPropertyValue((ExpressionPropertyValueSyntax)propertyValue, diagnostics);
			case 384:
				return BindDecimalPlacesPropertyValue((DecimalPlacesPropertyValueSyntax)propertyValue);
			case 386:
				return BindTableViewStatement((TableViewPropertyValueSyntax)propertyValue, info, diagnostics);
			case 387:
				return BindFiltersPropertyValue((FiltersPropertyValueSyntax)propertyValue, info, diagnostics);
			case 385:
				return BindTableFilterStatement((TableFilterPropertyValueSyntax)propertyValue, info, diagnostics);
			case 337:
				return BindOptionValuePropertyValue((OptionValuePropertyValueSyntax)propertyValue);
			case 336:
				return BindIdentifierNamePropertyValue((IdentifierNamePropertyValueSyntax)propertyValue);
			case 391:
				return BindMemberReferencePropertyValue((IdentifierNamePropertyValueSyntax)propertyValue, info, diagnostics);
			case 390:
				return BindStyleExpressionPropertyValue((StyleExpressionPropertyValueSyntax)propertyValue, diagnostics);
			case 340:
			{
				ImmutableArray<IdentifierNameSyntax> immutableArray = ImmutableArray.Create(((PageFieldReferencePropertyValueSyntax)propertyValue).Value);
				return BindPageFieldReferences((PageFieldReferencePropertyValueSyntax)propertyValue, immutableArray, info?.Kind, diagnostics);
			}
			case 382:
				return BindIntegerExpressionPropertyValue((IntegerExpressionPropertyValueSyntax)propertyValue, info, diagnostics);
			case 393:
				return BindOrderByPropertyValue((OrderByPropertyValueSyntax)propertyValue, diagnostics);
			case 395:
				return BindQueryDataItemLinkPropertyValue((QueryDataItemLinkPropertyValueSyntax)propertyValue, diagnostics);
			case 397:
				return BindReportDataItemLinkPropertyValue((ReportDataItemLinkPropertyValueSyntax)propertyValue, diagnostics);
			case 374:
				return BindCommaSeparatedIdentifierOrLiteralPropertyValue((CommaSeparatedIdentifierOrLiteralPropertyValueSyntax)propertyValue, info, diagnostics);
			default:
				throw ExceptionUtilities.UnexpectedValue(syntax.Name);
			}
		}
		return BindCalculationFormulaPropertyValue((CalculationFormulaPropertyValueSyntax)propertyValue, info, diagnostics);
	}

	private BoundBadPropertyValue CreateBoundBadPropertyValue(SyntaxNode propertyValue)
	{
		return new BoundBadPropertyValue(propertyValue, BadExpression(propertyValue, LookupResultKind.Empty), null, hasErrors: true);
	}

	private BoundPropertyValue BindCommaSeparatedStringsPropertyValue(CommaSeparatedStringsPropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		switch (info.Kind)
		{
		case PropertyKind.APIVersion:
			return BindApiVersionPropertyValue(propertyValue, diagnostics);
		case PropertyKind.QueryCategory:
			return BindQueryCategoryPropertyValue(info, propertyValue, diagnostics);
		case PropertyKind.Scripts:
		case PropertyKind.StyleSheets:
		case PropertyKind.Images:
			return BindLiteralListProperty(info, propertyValue, diagnostics);
		default:
			throw ExceptionUtilities.UnexpectedValue(info.Kind);
		}
	}

	private static BoundPropertyValue BindQueryCategoryPropertyValue(PropertyTypeInfo propertyInfo, CommaSeparatedStringsPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		ArrayBuilder<ConstantValue> instance = ArrayBuilder<ConstantValue>.GetInstance(propertyValue.Values.Count);
		PooledHashSet<string> instance2 = PooledHashSet<string>.GetInstance();
		try
		{
			if (propertyValue.Values.Count == 0)
			{
				diagnostics.Add(ErrorCode.ERR_PropertyCannotBeBlank, propertyValue.GetLocation(), propertyInfo.Name);
			}
			foreach (StringLiteralValueSyntax value in propertyValue.Values)
			{
				string identifierOrLiteralValue = value.GetIdentifierOrLiteralValue();
				if (string.IsNullOrEmpty(identifierOrLiteralValue))
				{
					diagnostics.Add(ErrorCode.ERR_EmptyListMember, value.GetLocation(), propertyInfo.Name);
					continue;
				}
				string item = identifierOrLiteralValue.ToUpperInvariant();
				if (instance2.Contains(item))
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateListMember, value.GetLocation(), identifierOrLiteralValue);
				}
				instance2.Add(item);
				instance.Add(ConstantValue.Create(identifierOrLiteralValue));
			}
			TypeSymbol navType = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
			return new BoundConstantValueListPropertyValue(propertyValue, instance.ToImmutable(), navType);
		}
		finally
		{
			instance2.Free();
			instance.Free();
		}
	}

	private BoundPropertyValue BindCommaSeparatedIdentifierOrLiteralPropertyValue(CommaSeparatedIdentifierOrLiteralPropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		return info.Kind switch
		{
			PropertyKind.ValuesAllowed => (BoundPropertyValue)BindValuesAllowed(propertyValue, diagnostics), 
			PropertyKind.OptionOrdinalValues => BindOptionOrdinalValues(propertyValue, diagnostics), 
			_ => throw ExceptionUtilities.UnexpectedValue(info.Kind), 
		};
	}

	private BoundConstantValueListPropertyValue BindOptionOrdinalValues(CommaSeparatedIdentifierOrLiteralPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = ContainingMember.GetTypeSymbol();
		if (typeSymbol == null || typeSymbol.GetNavTypeKindSafe() != NavTypeKind.Option)
		{
			return null;
		}
		if (propertyValue.Values.Count == 0)
		{
			Error(diagnostics, ErrorCode.ERR_PropertyCannotBeBlank, propertyValue.Parent.GetLocation(), "OptionOrdinalValues");
		}
		ArrayBuilder<ConstantValue> instance = ArrayBuilder<ConstantValue>.GetInstance();
		try
		{
			foreach (IdentifierOrLiteralExpressionSyntax value in propertyValue.Values)
			{
				if (value.Identifier.Kind != SyntaxKind.LiteralExpression)
				{
					return null;
				}
				ConstantValue constantValue = BindConstantValue((LiteralExpressionSyntax)value.Identifier, diagnostics);
				if (constantValue.Discriminator != ConstantValueTypeDiscriminator.Int32 || constantValue.Int32Value < -1)
				{
					diagnostics.Add(ErrorCode.ERR_InvalidOptionOrdinalValue, value.GetLocation(), value.ToString());
				}
				instance.Add(constantValue);
			}
			TypeSymbol navType = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
			return new BoundConstantValueListPropertyValue(propertyValue, instance.ToImmutable(), navType);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundConstantValueListPropertyValue BindValuesAllowed(CommaSeparatedIdentifierOrLiteralPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = ContainingMember.GetTypeSymbol();
		if (typeSymbol == null || typeSymbol.GetNavTypeKindSafe() == NavTypeKind.None)
		{
			return null;
		}
		ArrayBuilder<ConstantValue> instance = ArrayBuilder<ConstantValue>.GetInstance();
		PooledDictionary<string, (SyntaxNode, bool)> instance2 = PooledDictionary<string, (SyntaxNode, bool)>.GetInstance();
		bool flag = true;
		try
		{
			foreach (IdentifierOrLiteralExpressionSyntax value in propertyValue.Values)
			{
				if (!IsDuplicateValueInValuesAllowedList(value, instance2, diagnostics))
				{
					flag = false;
					continue;
				}
				ConstantValue constantValue = TryGetConstantValueFromValuesAllowedListMember(value, typeSymbol, diagnostics);
				if (constantValue == null)
				{
					flag = false;
				}
				else if (!IsValueTypeCompatibleWithFieldType(typeSymbol, constantValue, value, diagnostics))
				{
					flag = false;
				}
				else
				{
					instance.Add(constantValue);
				}
			}
			if (!flag)
			{
				return null;
			}
			TypeSymbol navType = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
			return new BoundConstantValueListPropertyValue(propertyValue, instance.ToImmutable(), navType);
		}
		finally
		{
			instance2.Free();
			instance.Free();
		}
	}

	private bool IsValueTypeCompatibleWithFieldType(TypeSymbol typeOfContainingField, ConstantValue valueInList, IdentifierOrLiteralExpressionSyntax listMember, DiagnosticBag diagnostics)
	{
		TypeSymbol navTypeSymbol = TypeSymbol.GetNavTypeSymbol(valueInList.NavTypeKind);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromType(navTypeSymbol, typeOfContainingField, ref useSiteDiagnostics);
		if (typeOfContainingField.NavTypeKind == NavTypeKind.Enum && navTypeSymbol.NavTypeKind == NavTypeKind.Integer)
		{
			EnumTypeSymbol enumTypeSymbol = (EnumTypeSymbol)typeOfContainingField;
			if (valueInList.Discriminator != ConstantValueTypeDiscriminator.Int32 || !enumTypeSymbol.Values.Any((EnumValueSymbol v) => v.Ordinal == valueInList.Int32Value))
			{
				diagnostics.Add(ErrorCode.ERR_ConstantValueNotAValidOptionValue, listMember.GetLocation(), listMember.ToString(), "Enum");
				return false;
			}
		}
		else if (!conversion.IsImplicit)
		{
			diagnostics.Add(ErrorCode.ERR_ValueTypeDoesNotMatchFieldType, listMember.GetLocation(), listMember.ToString());
			return false;
		}
		if (typeOfContainingField.NavTypeKind == NavTypeKind.Option && conversion.Kind == ConversionKind.ToOption)
		{
			OptionTypeSymbol optionSymbol = (OptionTypeSymbol)typeOfContainingField;
			if (valueInList.Discriminator != ConstantValueTypeDiscriminator.Int32 || !optionSymbol.IsValidOptionMember(valueInList.Int32Value))
			{
				diagnostics.Add(ErrorCode.ERR_ConstantValueNotAValidOptionValue, listMember.GetLocation(), listMember.ToString(), "Option");
				return false;
			}
		}
		return true;
	}

	private ConstantValue TryGetConstantValueFromValuesAllowedListMember(IdentifierOrLiteralExpressionSyntax listMember, TypeSymbol typeOfContainingField, DiagnosticBag diagnostics)
	{
		switch (listMember.Identifier.Kind)
		{
		case SyntaxKind.LiteralExpression:
			return BindConstantValue((LiteralExpressionSyntax)listMember.Identifier, diagnostics);
		case SyntaxKind.IdentifierName:
			switch (typeOfContainingField.NavTypeKind)
			{
			case NavTypeKind.Option:
			{
				OptionSymbol optionSymbol = TryGetOptionSymbolFromOptionType((OptionTypeSymbol)typeOfContainingField, listMember.Identifier, ContainingMember.Name, diagnostics);
				if (optionSymbol == null)
				{
					return null;
				}
				return ConstantValue.Create(optionSymbol.Ordinal);
			}
			case NavTypeKind.Enum:
			{
				EnumValueSymbol enumValueSymbol = TryGetEnumValueSymbolFromEnumType((EnumTypeSymbol)typeOfContainingField, listMember.Identifier, ContainingMember.Name, diagnostics);
				if (enumValueSymbol == null)
				{
					return null;
				}
				return ConstantValue.Create(enumValueSymbol.Ordinal);
			}
			default:
				diagnostics.Add(ErrorCode.ERR_ValueTypeDoesNotMatchFieldType, listMember.GetLocation(), listMember.ToString());
				return null;
			}
		default:
			throw ExceptionUtilities.UnexpectedValue(listMember.Identifier.Kind);
		}
	}

	private static bool IsDuplicateValueInValuesAllowedList(IdentifierOrLiteralExpressionSyntax listMember, IDictionary<string, (SyntaxNode FirstNodeWithValue, bool HasError)> valuesInList, DiagnosticBag diagnostics)
	{
		bool flag = false;
		string identifierOrLiteralValue = listMember.GetIdentifierOrLiteralValue();
		string key = identifierOrLiteralValue.ToUpperInvariant();
		if (valuesInList.TryGetValue(key, out (SyntaxNode, bool) value))
		{
			diagnostics.Add(ErrorCode.ERR_DuplicateListMember, listMember.GetLocation(), identifierOrLiteralValue);
			if (!value.Item2)
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateListMember, value.Item1.GetLocation(), identifierOrLiteralValue);
			}
			flag = true;
		}
		valuesInList[key] = (listMember, flag);
		return !flag;
	}

	private static ConstantValue BindConstantValue(LiteralExpressionSyntax literalExpression, DiagnosticBag diagnostics)
	{
		LiteralValueSyntax literal = literalExpression.Literal;
		return ConstantValue.Create(st: NavTypeExtensions.FromLiteralSyntaxKind(literal.Kind), value: literal.GetLiteralValue());
	}

	private BoundPropertyValue BindApiVersionPropertyValue(CommaSeparatedStringsPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		ArrayBuilder<ConstantValue> instance = ArrayBuilder<ConstantValue>.GetInstance();
		PooledNameComparisonHashSet instance2 = PooledNameComparisonHashSet.GetInstance();
		try
		{
			foreach (StringLiteralValueSyntax value in propertyValue.Values)
			{
				if (!IsValidApiVersionValue(value))
				{
					return CreateBoundBadPropertyValue(ErrorCode.ERR_InvalidApiVersionId, propertyValue, propertyValue.Parent, diagnostics, value);
				}
				string identifierOrLiteralValue = value.GetIdentifierOrLiteralValue();
				if (instance2.Contains(identifierOrLiteralValue))
				{
					return CreateBoundBadPropertyValue(ErrorCode.ERR_DuplicateApiVersionId, propertyValue, propertyValue.Parent, diagnostics, value);
				}
				instance2.Add(identifierOrLiteralValue);
				ConstantValue item = ConstantValue.Create(identifierOrLiteralValue);
				instance.Add(item);
			}
			TypeSymbol navType = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
			return new BoundConstantValueListPropertyValue(propertyValue, instance.ToImmutable(), navType);
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	private BoundPropertyValue BindAlphaNumericPropertyValue(PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		NavTypeKind navTypeKind = NavTypeExtensions.FromLiteralSyntaxKind(propertyValueSyntax.Kind);
		TypeSymbol navType = GetNavType(navTypeKind, diagnostics, propertyValueSyntax);
		ConstantValue constantValue = ConstantValue.Create(propertyValueSyntax.GetPropertyValue(), navTypeKind);
		string stringValue = constantValue.StringValue;
		if (string.IsNullOrEmpty(stringValue) || !stringValue.IsValidAlphaNumeric())
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_InvalidAlphaNumeric, propertyValueSyntax, propertyValueSyntax.Parent, diagnostics, stringValue);
		}
		return new BoundLiteralPropertyValue(propertyValueSyntax, constantValue, navType);
	}

	private static bool IsValidApiVersionValue(StringLiteralValueSyntax apiVersion)
	{
		if (!string.Equals(apiVersion.ToString(), "'beta'"))
		{
			return ApiVersionRegex.IsMatch(apiVersion.ToString());
		}
		return true;
	}

	private BoundPropertyValue BindReportDataItemLinkPropertyValue(ReportDataItemLinkPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		SourceReportDataItemSymbol sourceReportDataItemSymbol = ContainingMember as SourceReportDataItemSymbol;
		DebugAssertHelper.Assert(sourceReportDataItemSymbol != null);
		if (propertyValue.Joins.IsEmpty())
		{
			return null;
		}
		if (sourceReportDataItemSymbol.RelatedTable == null)
		{
			return null;
		}
		TableTypeSymbol relatedTableForReportDataItemLink = GetRelatedTableForReportDataItemLink();
		if (relatedTableForReportDataItemLink == null)
		{
			return null;
		}
		ArrayBuilder<BoundDataItemLinkExpression> instance = ArrayBuilder<BoundDataItemLinkExpression>.GetInstance(propertyValue.Joins.Count);
		try
		{
			foreach (ReportDataItemLinkExpressionSyntax join in propertyValue.Joins)
			{
				BoundDataItemLinkExpression dataItemLinkExpression = null;
				BoundBadPropertyValue boundBadPropertyValue = BindReportDataItemLinkExpression((TableTypeSymbol)sourceReportDataItemSymbol.RelatedTable.OriginalDefinition, relatedTableForReportDataItemLink, propertyValue, join, diagnostics, out dataItemLinkExpression);
				if (boundBadPropertyValue != null)
				{
					return boundBadPropertyValue;
				}
				instance.Add(dataItemLinkExpression);
			}
			return new BoundDataItemLinkPropertyValue(propertyValue, instance.ToImmutableArray(), NavCorLib.NoneType);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundBadPropertyValue BindReportDataItemLinkExpression(TableTypeSymbol relatedTable, TableTypeSymbol referencedTable, ReportDataItemLinkPropertyValueSyntax propertyValue, ReportDataItemLinkExpressionSyntax join, DiagnosticBag diagnostics, out BoundDataItemLinkExpression dataItemLinkExpression)
	{
		dataItemLinkExpression = null;
		string identifierOrLiteralValue = join.SourceField.GetIdentifierOrLiteralValue();
		if (!BindFieldAccess(relatedTable, join.SourceField, identifierOrLiteralValue, diagnostics, out var boundExpression))
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_MissingTypeReference, propertyValue, join, diagnostics, identifierOrLiteralValue, relatedTable.Name);
		}
		string identifierOrLiteralValue2 = join.RelatedField.GetIdentifierOrLiteralValue();
		if (!BindFieldAccess(referencedTable, join.RelatedField, identifierOrLiteralValue2, diagnostics, out var boundExpression2))
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_MissingTypeReference, propertyValue, join, diagnostics, identifierOrLiteralValue2, referencedTable.Name);
		}
		dataItemLinkExpression = new BoundDataItemLinkExpression(join, boundExpression as BoundFieldAccess, boundExpression2 as BoundFieldAccess, NavCorLib.None);
		return null;
	}

	private void CheckCompilationScope(BoundPropertyValue value, DiagnosticBag diagnostics)
	{
		new CompilationScopeOnBoundPropertyValueVisitor(value.Syntax.Location, Compilation.Options.Target, diagnostics).Visit(value);
	}

	private BoundPropertyValue BindQueryDataItemLinkPropertyValue(QueryDataItemLinkPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		QueryDataItemSymbol queryDataItemSymbol = ContainingMember as QueryDataItemSymbol;
		DebugAssertHelper.Assert(queryDataItemSymbol != null);
		if (queryDataItemSymbol.ContainingSymbol.Kind == SymbolKind.Query)
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_TopLevelDataItemCannotHaveLinkProperty, propertyValue, propertyValue, diagnostics);
		}
		if (propertyValue.Joins.IsEmpty())
		{
			return null;
		}
		if (queryDataItemSymbol.RelatedTable == null)
		{
			return null;
		}
		ArrayBuilder<BoundDataItemLinkExpression> instance = ArrayBuilder<BoundDataItemLinkExpression>.GetInstance();
		try
		{
			foreach (QueryDataItemLinkExpressionSyntax join in propertyValue.Joins)
			{
				BoundDataItemLinkExpression queryDataItemLinkExpression = null;
				BoundBadPropertyValue boundBadPropertyValue = BindQueryDataItemLinkExpression(queryDataItemSymbol.RelatedTable, propertyValue, join, diagnostics, out queryDataItemLinkExpression);
				if (boundBadPropertyValue != null)
				{
					return boundBadPropertyValue;
				}
				instance.Add(queryDataItemLinkExpression);
			}
			return new BoundDataItemLinkPropertyValue(propertyValue, instance.ToImmutable(), NavCorLib.NoneType);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundBadPropertyValue BindQueryDataItemLinkExpression(TableTypeSymbol relatedTable, QueryDataItemLinkPropertyValueSyntax propertyValue, QueryDataItemLinkExpressionSyntax join, DiagnosticBag diagnostics, out BoundDataItemLinkExpression queryDataItemLinkExpression)
	{
		queryDataItemLinkExpression = null;
		string text = join.SourceField.Unquoted();
		if (!BindFieldAccess(relatedTable, join.SourceField, text, diagnostics, out var boundExpression))
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_MissingTypeReference, propertyValue, join, diagnostics, text, relatedTable.Name);
		}
		if (string.IsNullOrEmpty(join.RelatedField.Name.Unquoted()))
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_DataItemLinkMustReferenceFieldOnAncestorDataItem, propertyValue, join, diagnostics);
		}
		if (!(BindMemberAccess(join.RelatedField, invoked: false, isStatement: false, diagnostics) is BoundFieldAccess boundFieldAccess))
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_DataItemLinkMustReferenceFieldOnAncestorDataItem, propertyValue, join, diagnostics);
		}
		if (!(boundFieldAccess.ReceiverOpt is BoundQueryDataItemAccess boundQueryDataItemAccess))
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_DataItemLinkMustReferenceFieldOnAncestorDataItem, propertyValue, join, diagnostics);
		}
		if (!ContainingMember.IsContainedBy(boundQueryDataItemAccess.QueryDataItemSymbol))
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_DataItemLinkMustReferenceFieldOnAncestorDataItem, propertyValue, join, diagnostics);
		}
		queryDataItemLinkExpression = new BoundDataItemLinkExpression(join, boundExpression as BoundFieldAccess, boundFieldAccess, NavCorLib.None);
		return null;
	}

	private BoundPropertyValue BindOrderByPropertyValue(OrderByPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		switch (ContainingMember.Kind)
		{
		case SymbolKind.Page:
			return BindOrderByPropertyValue(propertyValue, (PageTypeSymbol)ContainingMember, diagnostics);
		case SymbolKind.PageExtension:
		case SymbolKind.PageCustomization:
		{
			ApplicationObjectTypeSymbol target = ((PageExtensionBaseTypeSymbol)ContainingMember).Target;
			return BindOrderByPropertyValue(propertyValue, (PageTypeSymbol)target, diagnostics);
		}
		case SymbolKind.Query:
			return BindOrderByPropertyValue(propertyValue, (QueryTypeSymbol)ContainingMember, diagnostics);
		default:
			throw ExceptionUtilities.UnexpectedValue(ContainingMember.Kind);
		}
	}

	private BoundPropertyValue BindOrderByPropertyValue(OrderByPropertyValueSyntax propertyValue, PageTypeSymbol containingPage, DiagnosticBag diagnostics)
	{
		if (containingPage == null)
		{
			return new BoundBadPropertyValue(propertyValue, BadExpression(propertyValue, LookupResultKind.Empty), null, hasErrors: true);
		}
		if (containingPage.RelatedTable == null)
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_PropertyNotAllowedOnPageWithoutSourceTable, propertyValue, propertyValue, diagnostics, PropertyKind.OrderBy, containingPage.Name);
		}
		ArrayBuilder<BoundOrderByExpression> instance = ArrayBuilder<BoundOrderByExpression>.GetInstance();
		PooledHashSet<Symbol> instance2 = PooledHashSet<Symbol>.GetInstance();
		try
		{
			if (propertyValue.Order.Count > 1)
			{
				return CreateBoundBadPropertyValue(ErrorCode.ERR_ViewsOrderByOnlySupportOneDirection, propertyValue, propertyValue, diagnostics);
			}
			TableTypeSymbol relatedTableSymbol = containingPage.GetRelatedTableSymbol();
			foreach (OrderByExpressionSyntax item in propertyValue.Order)
			{
				bool isAscending = item.OrderSpecifier.Kind == SyntaxKind.AscendingKeyword;
				foreach (IdentifierNameSyntax sortingField in item.SortingFields)
				{
					if (!BindFieldAccess(relatedTableSymbol, item, sortingField.Identifier.ValueText, diagnostics, out var boundExpression))
					{
						if (!Compilation.Options.Target.IsPersonalizationTarget())
						{
							return new BoundBadPropertyValue(propertyValue, BadExpression(propertyValue, LookupResultKind.Empty), null, hasErrors: true);
						}
						continue;
					}
					if (instance2.Contains(boundExpression.ExpressionSymbol))
					{
						return CreateBoundBadPropertyValue(ErrorCode.ERR_DuplicateTableFieldReference, propertyValue, sortingField, diagnostics, sortingField.Unquoted());
					}
					instance2.Add(boundExpression.ExpressionSymbol);
					instance.Add(new BoundOrderByExpression(sortingField, isAscending, boundExpression, NavCorLib.NoneType));
				}
			}
			return new BoundOrderByPropertyValue(propertyValue, instance.ToImmutableArray(), null);
		}
		finally
		{
			instance2.Free();
			instance.Free();
		}
	}

	private BoundPropertyValue BindOrderByPropertyValue(OrderByPropertyValueSyntax propertyValue, QueryTypeSymbol containingQuery, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(containingQuery != null);
		ArrayBuilder<BoundOrderByExpression> instance = ArrayBuilder<BoundOrderByExpression>.GetInstance();
		PooledHashSet<Symbol> instance2 = PooledHashSet<Symbol>.GetInstance();
		try
		{
			foreach (OrderByExpressionSyntax item in propertyValue.Order)
			{
				bool isAscending = item.OrderSpecifier.Kind == SyntaxKind.AscendingKeyword;
				foreach (IdentifierNameSyntax sortingField in item.SortingFields)
				{
					BoundQueryElementAccess boundExpression = null;
					if (!BindQueryColumnAccess(containingQuery, sortingField, out boundExpression))
					{
						return CreateBoundBadPropertyValue(ErrorCode.ERR_MissingColumnReference, propertyValue, sortingField, diagnostics, sortingField.Unquoted(), containingQuery.Name);
					}
					if (instance2.Contains(boundExpression.QueryElementSymbol))
					{
						return CreateBoundBadPropertyValue(ErrorCode.ERR_DuplicateColumnReference, propertyValue, sortingField, diagnostics, sortingField.Unquoted());
					}
					instance2.Add(boundExpression.QueryElementSymbol);
					instance.Add(new BoundOrderByExpression(sortingField, isAscending, boundExpression, NavCorLib.NoneType));
				}
			}
			return new BoundOrderByPropertyValue(propertyValue, instance.ToImmutableArray(), null);
		}
		finally
		{
			instance2.Free();
			instance.Free();
		}
	}

	private BoundPropertyValue BindMemberReferencePropertyValue(IdentifierNamePropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		return info.Kind switch
		{
			PropertyKind.WordMergeDataItem => BindReportDataItemForWordMergedProperty(propertyValue, info, diagnostics), 
			PropertyKind.DataItemLinkReference => BindReportDataItemForLinkReference(propertyValue, diagnostics), 
			PropertyKind.LinkTable => BindXmlPortTableElementLinkTableReference(propertyValue, diagnostics), 
			_ => throw ExceptionUtilities.UnexpectedValue(info.Kind), 
		};
	}

	private BoundPropertyValue BindXmlPortTableElementLinkTableReference(IdentifierNamePropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		XmlPortNodeSymbol referencedXmlPortNode = GetReferencedXmlPortNode(propertyValue.Value, diagnostics);
		if (referencedXmlPortNode == null)
		{
			return null;
		}
		if (referencedXmlPortNode.NodeXmlKind != XmlPortNodeXmlKind.Element || referencedXmlPortNode.SourceTypeKind != XmlPortSourceTypeKind.Table)
		{
			diagnostics.Add(ErrorCode.ERR_LinkTableValueMustReferenceTableElement, propertyValue.Location);
			return null;
		}
		if (!(referencedXmlPortNode != null))
		{
			return null;
		}
		return new BoundMemberReferencePropertyValue(propertyValue, referencedXmlPortNode, referencedXmlPortNode);
	}

	private XmlPortNodeSymbol GetReferencedXmlPortNode(IdentifierNameSyntax nodeName, DiagnosticBag diagnostics)
	{
		return GetReferencedMemberSymbol<XmlPortNodeSymbol, XmlPortTypeSymbol>(nodeName, (Symbol s) => s.IsXmlPortNodeTableElement(), SymbolKind.XmlPort, diagnostics);
	}

	private ReportDataItemSymbol GetReferencedReportDataItemSymbol(IdentifierNameSyntax dataItemNameSyntax, DiagnosticBag diagnostics)
	{
		return GetReferencedMemberSymbol<ReportDataItemSymbol, ReportTypeSymbol>(dataItemNameSyntax, (Symbol s) => s.Kind == SymbolKind.ReportDataItem, SymbolKind.Report, diagnostics);
	}

	private TSymbol GetReferencedMemberSymbol<TSymbol, TParentSymbol>(IdentifierNameSyntax memberNameSyntax, Func<Symbol, bool> symbolFilter, SymbolKind parentSymbolKind, DiagnosticBag diagnostics) where TSymbol : Symbol where TParentSymbol : ContainerSymbol
	{
		TParentSymbol containingSymbolOfKind = ContainingMember.GetContainingSymbolOfKind<TParentSymbol>(parentSymbolKind);
		string text = memberNameSyntax.Unquoted();
		IEnumerable<Symbol> source = containingSymbolOfKind?.GetCodeMembers(text).Where(symbolFilter);
		if (source.HasMoreThan(1))
		{
			Error(diagnostics, ErrorCode.ERR_AmbigMemberReference, memberNameSyntax, text, containingSymbolOfKind?.Name);
			return null;
		}
		TSymbol val = source.FirstOrDefault() as TSymbol;
		if (val == null)
		{
			Error(diagnostics, ErrorCode.ERR_MissingTypeReference, memberNameSyntax, text, containingSymbolOfKind?.Name);
			return null;
		}
		return val;
	}

	private BoundPropertyValue BindReportDataItemForLinkReference(IdentifierNamePropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		ReportDataItemSymbol referencedReportDataItemSymbol = GetReferencedReportDataItemSymbol(propertyValue.Value, diagnostics);
		if (!(referencedReportDataItemSymbol != null))
		{
			return null;
		}
		return new BoundMemberReferencePropertyValue(propertyValue, referencedReportDataItemSymbol, referencedReportDataItemSymbol);
	}

	private BoundPropertyValue BindReportDataItemForWordMergedProperty(IdentifierNamePropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		ReportDataItemSymbol referencedReportDataItemSymbol = GetReferencedReportDataItemSymbol(propertyValue.Value, diagnostics);
		if (referencedReportDataItemSymbol == null || referencedReportDataItemSymbol.ContainingSymbol.Kind != SymbolKind.Report)
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_WordMergeDataItemMustBeTopLevel, propertyValue, propertyValue, diagnostics, info.MetadataName);
		}
		return new BoundMemberReferencePropertyValue(propertyValue, referencedReportDataItemSymbol, referencedReportDataItemSymbol);
	}

	private BoundPropertyValue BindPageFieldReferences(SyntaxNode propertyValue, IEnumerable<IdentifierNameSyntax> fieldIdentifiers, PropertyKind? propertyKind, DiagnosticBag diagnostics)
	{
		ControlKind? controlKind = propertyKind?.GetRequiredReferenceControlKind();
		ControlKind? controlKind2 = propertyKind?.GetRequiredParentControlKind();
		var (controlSymbol, pageBaseTypeSymbol) = GetPageFieldAccessContext(propertyValue);
		if (controlSymbol == null || pageBaseTypeSymbol == null)
		{
			return null;
		}
		if (controlKind2.HasValue && controlSymbol.ControlKind != controlKind2.Value)
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_PropertyOnlyValidInControlOfType, propertyValue, propertyValue, diagnostics, propertyKind, controlKind2.Value);
		}
		ArrayBuilder<BoundControlAccess> instance = ArrayBuilder<BoundControlAccess>.GetInstance();
		try
		{
			ValidateForDuplicateNames(fieldIdentifiers, diagnostics);
			foreach (IdentifierNameSyntax fieldIdentifier in fieldIdentifiers)
			{
				if (fieldIdentifier.Identifier.IsMissing)
				{
					return CreateBoundBadPropertyValue(ErrorCode.ERR_MissingTypeReference, propertyValue, fieldIdentifier, diagnostics, fieldIdentifier.Identifier.ValueText, pageBaseTypeSymbol.Name);
				}
				BoundBadPropertyValue boundBadPropertyValue = BindPageFieldAccess(pageBaseTypeSymbol, propertyValue, fieldIdentifier, diagnostics, out var controlAccess);
				if (boundBadPropertyValue != null)
				{
					return boundBadPropertyValue;
				}
				if (!propertyValue.GetContainingApplicationObjectSyntax().Kind.IsExtensionObject() && controlKind2.HasValue && controlAccess.ControlSymbol.ContainingSymbol != controlSymbol)
				{
					return CreateBoundBadPropertyValue(ErrorCode.ERR_ControlNotInGroup, propertyValue, fieldIdentifier, diagnostics, controlAccess.ControlSymbol.Name, controlSymbol.Name);
				}
				if (controlKind.HasValue && controlAccess.ControlSymbol.ControlKind != controlKind)
				{
					return CreateBoundBadPropertyValue(ErrorCode.ERR_InvalidControlType, propertyValue, fieldIdentifier, diagnostics, controlAccess.ControlSymbol.Name, controlKind.Value);
				}
				instance.Add(controlAccess);
			}
			ImmutableArray<BoundControlAccess> immutableArray = instance.ToImmutable();
			TypeSymbol navType = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
			return new BoundIdentifierListPropertyValue(propertyValue, immutableArray.As<BoundExpression>(), navType);
		}
		finally
		{
			instance.Free();
		}
	}

	private (ControlSymbol, IPageBaseTypeSymbol) GetPageFieldAccessContext(SyntaxNode propertyValue)
	{
		IPageBaseTypeSymbol pageBaseTypeSymbol = null;
		ControlSymbol extendedPageControlOrSelf = ContainingMember.GetExtendedPageControlOrSelf();
		if (extendedPageControlOrSelf != null)
		{
			pageBaseTypeSymbol = extendedPageControlOrSelf.GetContainingPage();
			return (extendedPageControlOrSelf, pageBaseTypeSymbol);
		}
		switch (ContainingMember.Kind)
		{
		case SymbolKind.Page:
			pageBaseTypeSymbol = (PageTypeSymbol)ContainingMember;
			break;
		case SymbolKind.PageExtension:
		case SymbolKind.PageCustomization:
			pageBaseTypeSymbol = (PageTypeSymbol)((PageExtensionBaseTypeSymbol)ContainingMember).Target;
			break;
		}
		if (pageBaseTypeSymbol == null)
		{
			return (null, null);
		}
		ControlModifyChangeSyntax ancestor = propertyValue.GetAncestor<ControlModifyChangeSyntax>();
		if (ancestor == null)
		{
			return (null, pageBaseTypeSymbol);
		}
		string containingGroupName = ancestor.GetNameStringValue();
		extendedPageControlOrSelf = (ControlSymbol)pageBaseTypeSymbol.FlattenedControls.FirstOrDefault((IControlSymbol x) => SemanticFacts.IsSameName(x.Name, containingGroupName));
		return (extendedPageControlOrSelf, pageBaseTypeSymbol);
	}

	private BoundPropertyValue BindStyleExpressionPropertyValue(StyleExpressionPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindExpression(propertyValue.Expression, diagnostics);
		if (!boundExpression.Type.GetNavTypeKindSafe().IsValidStyleExpressionValue())
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_InvalidStyleExpressionPropertyValue, propertyValue, propertyValue.Expression, diagnostics);
		}
		return new BoundStyleExpressionPropertyValue(propertyValue, boundExpression, boundExpression.Type, boundExpression.HasErrors);
	}

	private static BoundPropertyValue BindOptionValuePropertyValue(OptionValuePropertyValueSyntax propertyValue)
	{
		return new BoundIdentifierNamePropertyValue(propertyValue, ConstantValue.Create(propertyValue.Value.Unquoted()), NavCorLib.OptionType);
	}

	private BoundPropertyValue BindIdentifierNamePropertyValue(IdentifierNamePropertyValueSyntax propertyValue)
	{
		return new BoundIdentifierNamePropertyValue(propertyValue, ConstantValue.Create(propertyValue.Value.Unquoted()), NavCorLib.None);
	}

	private static BoundPropertyValue BindDecimalPlacesPropertyValue(DecimalPlacesPropertyValueSyntax propertyValue)
	{
		int num = (int)propertyValue.Value.MinimumToken.Value;
		int? maximum = ((propertyValue.Value.ColonToken.Kind != SyntaxKind.ColonToken) ? new int?(num) : ((int?)propertyValue.Value.MaximumToken.Value));
		return new BoundDecimalPlacesPropertyValue(propertyValue, num, maximum, NavCorLib.None);
	}

	internal bool BindQueryColumnAccess(QueryTypeSymbol containingQuery, IdentifierNameSyntax elementIdentifier, out BoundQueryElementAccess boundExpression)
	{
		boundExpression = null;
		string text = elementIdentifier?.Unquoted();
		DebugAssertHelper.Assert(containingQuery != null);
		DebugAssertHelper.Assert(!string.IsNullOrEmpty(text));
		QueryColumnSymbol queryColumnSymbol = (from symbol in this.FindSymbols(text, LookupOptions.MemberAccess)
			where symbol.Kind == SymbolKind.QueryColumn
			select symbol).Cast<QueryColumnSymbol>().FirstIfSingle();
		if (queryColumnSymbol == null)
		{
			return false;
		}
		BoundClassType receiverOpt = new BoundClassType(elementIdentifier, QueryClassTypeSymbol.Instance, containingQuery);
		boundExpression = new BoundQueryElementAccess(elementIdentifier, receiverOpt, queryColumnSymbol, LookupResultKind.Viable, queryColumnSymbol.Type);
		return true;
	}

	internal bool BindFieldAccess(TableTypeSymbol relatedTable, ExpressionSyntax relatedSymbolSyntax, string relatedFieldName, DiagnosticBag diagnostics, out BoundExpression boundExpression)
	{
		DebugAssertHelper.Assert(relatedTable != null);
		DebugAssertHelper.Assert(relatedSymbolSyntax != null);
		DebugAssertHelper.Assert(relatedFieldName != null);
		string text = relatedFieldName.UnquoteIdentifier();
		boundExpression = null;
		int num = 0;
		if (relatedSymbolSyntax.GetLiteralExpressionSyntaxKind() == SyntaxKind.Int32SignedLiteralValue)
		{
			string identifierOrLiteralValue = relatedSymbolSyntax.GetIdentifierOrLiteralValue();
			if (string.Compare(identifierOrLiteralValue, text, StringComparison.OrdinalIgnoreCase) == 0)
			{
				num = int.Parse(identifierOrLiteralValue, CultureInfo.InvariantCulture);
				DebugAssertHelper.Assert(num > 0);
			}
		}
		if (!TryGetFieldByNameOrId(relatedTable, num, text, out var fieldSymbol))
		{
			if (!Compilation.Options.Target.IsPersonalizationTarget())
			{
				Error(diagnostics, ErrorCode.ERR_MissingTypeReference, relatedSymbolSyntax, relatedFieldName, relatedTable?.Name);
			}
			return false;
		}
		ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, fieldSymbol, relatedSymbolSyntax);
		BoundClassType receiverOpt = new BoundClassType((relatedSymbolSyntax as MemberAccessExpressionSyntax)?.Expression ?? relatedSymbolSyntax, DatabaseClassTypeSymbol.Instance, relatedTable);
		boundExpression = new BoundFieldAccess(relatedSymbolSyntax, receiverOpt, fieldSymbol, LookupResultKind.Viable, fieldSymbol.Type);
		if (boundExpression.Kind != BoundKind.BadExpression)
		{
			return !diagnostics.HasAnyErrors();
		}
		return false;
	}

	[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Cast is done on different code path")]
	internal bool BindOptionAccess(TableTypeSymbol relatedTable, string relatedFieldName, ExpressionSyntax relatedSymbolSyntax, DiagnosticBag diagnostics, out BoundExpression boundExpression)
	{
		DebugAssertHelper.Assert(relatedSymbolSyntax != null);
		boundExpression = null;
		if (relatedTable == null || relatedFieldName == null)
		{
			boundExpression = CreateBadOptionExpression(relatedSymbolSyntax);
			return false;
		}
		if (!TryGetFieldByNameOrId(relatedTable, 0, relatedFieldName.UnquoteIdentifier(), out var fieldSymbol) || (fieldSymbol.Type.Kind != SymbolKind.OptionType && fieldSymbol.Type.Kind != SymbolKind.Enum))
		{
			Error(diagnostics, ErrorCode.ERR_MissingTypeReference, relatedSymbolSyntax, relatedFieldName, relatedTable.Name);
			boundExpression = CreateBadOptionExpression(relatedSymbolSyntax);
			return false;
		}
		new BoundClassType(relatedSymbolSyntax, DatabaseClassTypeSymbol.Instance, relatedTable);
		switch (fieldSymbol.Type.Kind)
		{
		case SymbolKind.OptionType:
			boundExpression = BindOptionValues(relatedTable, fieldSymbol, relatedSymbolSyntax, diagnostics);
			break;
		case SymbolKind.Enum:
			boundExpression = BindEnumValues(relatedTable, fieldSymbol, relatedSymbolSyntax, diagnostics);
			break;
		default:
			ExceptionUtilities.UnexpectedValue(fieldSymbol.Type.Kind);
			break;
		}
		if (boundExpression == null)
		{
			boundExpression = CreateBadOptionExpression(relatedSymbolSyntax);
		}
		if (boundExpression.Kind != BoundKind.BadExpression)
		{
			return !diagnostics.HasAnyErrors();
		}
		return false;
	}

	private static BoundExpression CreateBadOptionExpression(ExpressionSyntax syntax)
	{
		return new BoundBadExpression(syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundNode>.Empty, TypeSymbol.GetNavTypeSymbol(NavTypeKind.Option));
	}

	private static BoundExpression BindOptionValues(TableTypeSymbol relatedTable, FieldSymbol fieldSymbol, ExpressionSyntax relatedSymbolSyntax, DiagnosticBag diagnostics)
	{
		BoundClassType receiverOpt = new BoundClassType(relatedSymbolSyntax, DatabaseClassTypeSymbol.Instance, relatedTable);
		OptionTypeSymbol optionTypeSymbol = (OptionTypeSymbol)fieldSymbol.Type;
		BoundFieldAccess receiverOpt2 = new BoundFieldAccess(relatedSymbolSyntax, receiverOpt, fieldSymbol, LookupResultKind.Viable, optionTypeSymbol);
		OptionSymbol optionSymbol = TryGetOptionSymbolFromOptionType(optionTypeSymbol, relatedSymbolSyntax, fieldSymbol.Name, diagnostics);
		if (optionSymbol == null)
		{
			return null;
		}
		return new BoundOptionAccess(relatedSymbolSyntax, receiverOpt2, optionSymbol, LookupResultKind.Viable, TypeSymbol.GetNavTypeSymbol(NavTypeKind.Option));
	}

	private BoundExpression BindEnumValues(TableTypeSymbol relatedTable, FieldSymbol fieldSymbol, ExpressionSyntax relatedSymbolSyntax, DiagnosticBag diagnostics)
	{
		BoundClassType receiverOpt = new BoundClassType(relatedSymbolSyntax, DatabaseClassTypeSymbol.Instance, relatedTable);
		EnumTypeSymbol enumTypeSymbol = (EnumTypeSymbol)fieldSymbol.Type;
		BoundFieldAccess receiverOpt2 = new BoundFieldAccess(relatedSymbolSyntax, receiverOpt, fieldSymbol, LookupResultKind.Viable, enumTypeSymbol);
		OptionSymbol optionSymbol = TryGetEnumValueSymbolFromEnumType(enumTypeSymbol, relatedSymbolSyntax, fieldSymbol.Name, diagnostics);
		if (optionSymbol == null)
		{
			return null;
		}
		return new BoundOptionAccess(relatedSymbolSyntax, receiverOpt2, optionSymbol, LookupResultKind.Viable, enumTypeSymbol);
	}

	internal static OptionSymbol TryGetOptionSymbolFromOptionType(OptionTypeSymbol optionTypeSymbol, ExpressionSyntax optionMemberSyntax, string relatedFieldName, DiagnosticBag diagnostics)
	{
		string text = optionMemberSyntax.GetIdentifierOrLiteralValue().UnquoteIdentifier();
		int optionAsOrdinal = -1;
		if (optionMemberSyntax.GetLiteralExpressionSyntaxKind() == SyntaxKind.Int32SignedLiteralValue)
		{
			optionAsOrdinal = int.Parse(text, CultureInfo.InvariantCulture);
			DebugAssertHelper.Assert(optionAsOrdinal > 0);
		}
		ImmutableArray<Symbol> value = ((optionAsOrdinal == -1) ? optionTypeSymbol.GetMembers(text) : optionTypeSymbol.GetMembers().WhereAsArray((Symbol fieldMember) => fieldMember.Kind == SymbolKind.Option && ((OptionSymbol)fieldMember).Ordinal + 1 == optionAsOrdinal));
		if (value == null || value.Length == 0)
		{
			Error(diagnostics, ErrorCode.ERR_UndefinedOptionValue, optionMemberSyntax, text, relatedFieldName);
			return null;
		}
		if (value.Length != 1)
		{
			Error(diagnostics, ErrorCode.ERR_AmbiguosOptionAccess, optionMemberSyntax, text, relatedFieldName);
			return null;
		}
		return (OptionSymbol)value[0];
	}

	internal EnumValueSymbol TryGetEnumValueSymbolFromEnumType(EnumTypeSymbol enumTypeSymbol, ExpressionSyntax enumValueSyntax, string relatedFieldName, DiagnosticBag diagnostics)
	{
		string optionValue = enumValueSyntax.GetIdentifierOrLiteralValue().UnquoteIdentifier();
		int optionAsOrdinal = -1;
		if (enumValueSyntax.GetLiteralExpressionSyntaxKind() == SyntaxKind.Int32SignedLiteralValue)
		{
			optionAsOrdinal = int.Parse(optionValue, CultureInfo.InvariantCulture);
			DebugAssertHelper.Assert(optionAsOrdinal > 0);
		}
		EnumValueSymbol[] array = null;
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			LookupMembersInternal(instance, enumTypeSymbol, null, LookupOptions.Default, this, diagnose: false, ref useSiteDiagnostics);
			array = ((optionAsOrdinal == -1) ? instance.Symbols.Where((Symbol s) => s != null && s.Kind == SymbolKind.EnumValue && SemanticFacts.IsSameName(s.Name, optionValue)) : instance.Symbols.Where((Symbol f) => f != null && f.Kind == SymbolKind.EnumValue && ((FieldSymbol)f).Id == optionAsOrdinal)).Cast<EnumValueSymbol>().ToArray();
		}
		finally
		{
			instance.Free();
		}
		if (array == null || array.Length == 0)
		{
			Error(diagnostics, ErrorCode.ERR_UndefinedOptionValue, enumValueSyntax, optionValue, relatedFieldName);
			return null;
		}
		if (array.Length != 1)
		{
			Error(diagnostics, ErrorCode.ERR_AmbiguosOptionAccess, enumValueSyntax, optionValue, relatedFieldName);
			return null;
		}
		return array[0];
	}

	internal bool TryGetFieldByNameOrId(TableTypeSymbol relatedTable, int fieldId, string unquotedRelatedFieldName, out FieldSymbol fieldSymbol)
	{
		fieldSymbol = null;
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			LookupMembersInternal(instance, relatedTable, null, LookupOptions.Default, this, diagnose: false, ref useSiteDiagnostics);
			fieldSymbol = ((fieldId == 0) ? instance.Symbols.FirstOrDefault((Symbol s) => s != null && s.Kind == SymbolKind.Field && string.Compare(s.Name, unquotedRelatedFieldName, StringComparison.OrdinalIgnoreCase) == 0) : instance.Symbols.FirstOrDefault((Symbol f) => f != null && f.Kind == SymbolKind.Field && ((FieldSymbol)f).Id == fieldId)) as FieldSymbol;
		}
		finally
		{
			instance.Free();
		}
		return fieldSymbol != null;
	}

	private BoundPropertyValue BindCalculationFormulaPropertyValue(CalculationFormulaPropertyValueSyntax calculationFormula, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		SyntaxKind kind = calculationFormula.Kind;
		if (!(calculationFormula.Parent.Parent.Parent is FieldSyntax fieldSyntax))
		{
			return null;
		}
		if (!CalculationFormulaBindingValidatorHelper.ValidateCalculationFormulaDeclaringFieldType(kind, fieldSyntax, diagnostics))
		{
			return new BoundBadPropertyValue(fieldSyntax, BadExpression(calculationFormula, LookupResultKind.Empty), null, hasErrors: true);
		}
		bool isMinus = false;
		WhereExpressionSyntax whereExpressionSyntax = null;
		ExpressionSyntax expressionSyntax = null;
		switch (kind)
		{
		case SyntaxKind.ExistCalculationFormulaStatement:
		case SyntaxKind.CountCalculationFormulaStatement:
		{
			TableCalculationFormulaSyntax tableCalculationFormulaSyntax = (TableCalculationFormulaSyntax)calculationFormula;
			isMinus = tableCalculationFormulaSyntax.Sign.Value != null;
			expressionSyntax = tableCalculationFormulaSyntax.Table;
			whereExpressionSyntax = tableCalculationFormulaSyntax.WhereExpression;
			break;
		}
		case SyntaxKind.SumCalculationFormulaStatement:
		case SyntaxKind.AverageCalculationFormulaStatement:
		case SyntaxKind.MinCalculationFormulaStatement:
		case SyntaxKind.MaxCalculationFormulaStatement:
		case SyntaxKind.LookupCalculationFormulaStatement:
		{
			FieldCalculationFormulaSyntax fieldCalculationFormulaSyntax = (FieldCalculationFormulaSyntax)calculationFormula;
			isMinus = fieldCalculationFormulaSyntax.Sign.Value != null;
			expressionSyntax = fieldCalculationFormulaSyntax.Field;
			whereExpressionSyntax = fieldCalculationFormulaSyntax.WhereExpression;
			break;
		}
		}
		expressionSyntax.SplitMemberAccess(out var lhs, out var rhs);
		if (!TryGetTableTypeByName(calculationFormula, diagnostics, expressionSyntax, lhs, out var relatedTable, out var boundBadPropertyValue))
		{
			return boundBadPropertyValue;
		}
		bool hasErrors = false;
		BoundExpression boundExpression;
		if (kind != SyntaxKind.ExistCalculationFormulaStatement && kind != SyntaxKind.CountCalculationFormulaStatement)
		{
			hasErrors = !BindFieldAccess(relatedTable, expressionSyntax, rhs, diagnostics, out boundExpression);
			BoundFieldAccess boundFieldAccess = boundExpression as BoundFieldAccess;
			if (hasErrors || !CalculationFormulaBindingValidatorHelper.ValidateCalculationFormulaSourceFieldType(kind, boundFieldAccess, diagnostics) || !CalculationFormulaBindingValidatorHelper.ValidateCalculationFormulaSourceFieldTypeForMinMaxLookupFormulas(kind, fieldSyntax, boundFieldAccess, diagnostics))
			{
				return new BoundBadPropertyValue(expressionSyntax, BadExpression(expressionSyntax, LookupResultKind.Empty), null, hasErrors: true);
			}
		}
		else
		{
			BoundClassType boundClassType = new BoundClassType(expressionSyntax, DatabaseClassTypeSymbol.Instance, relatedTable);
			boundExpression = new BoundApplicationObjectAccess(expressionSyntax, boundClassType, relatedTable, LookupResultKind.Viable, boundClassType.Type);
		}
		BoundTableRelationConditionExpression tableFilter = null;
		if (whereExpressionSyntax != null && whereExpressionSyntax.Filter != null)
		{
			TableExpressionContext expressionContext = CreateDefaultTableContext(relatedTable);
			tableFilter = BindPropertyExpressions(whereExpressionSyntax, info, whereExpressionSyntax.Filter.Conditions, expressionContext, Compilation.Options, diagnostics, out hasErrors);
		}
		TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, calculationFormula);
		return new BoundCalculationFormulaPropertyValue(calculationFormula, kind, isMinus, boundExpression, tableFilter, navType, hasErrors);
	}

	private bool TryGetTableTypeByName(SyntaxNode owner, DiagnosticBag diagnostics, SyntaxNode nodeWithError, string relatedTableName, out TableTypeSymbol relatedTable, out BoundPropertyValue boundBadPropertyValue)
	{
		relatedTable = null;
		boundBadPropertyValue = null;
		if (string.IsNullOrEmpty(relatedTableName))
		{
			boundBadPropertyValue = CreateBoundBadPropertyValue(ErrorCode.ERR_MissingApplicationObject, owner, nodeWithError, diagnostics, SymbolKind.Table.ToString(), nodeWithError);
			return false;
		}
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = Compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Table, relatedTableName);
		if (objectSymbolsByNameAcrossModules.Length == 0)
		{
			boundBadPropertyValue = CreateBoundBadPropertyValue(ErrorCode.ERR_MissingApplicationObject, owner, nodeWithError, diagnostics, SymbolKind.Table.ToString(), relatedTableName);
			return false;
		}
		if (objectSymbolsByNameAcrossModules.Length > 1)
		{
			boundBadPropertyValue = CreateBoundBadPropertyValue(ErrorCode.ERR_AmbigReference, owner, nodeWithError, diagnostics, relatedTableName);
			return false;
		}
		relatedTable = objectSymbolsByNameAcrossModules[0] as TableTypeSymbol;
		return true;
	}

	private BoundPropertyValue BindTableFilterStatement(TableFilterPropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		ExpressionContext expressionContext = TryGetExpressionContext(propertyValue, info, diagnostics, out var boundBadPropertyValue);
		if (boundBadPropertyValue != null)
		{
			return boundBadPropertyValue;
		}
		if (expressionContext == null)
		{
			return null;
		}
		BoundTableRelationConditionExpression boundTableRelationConditionExpression = BindPropertyExpressions(propertyValue, info, propertyValue.Conditions, expressionContext, Compilation.Options, diagnostics, out var hasErrors);
		if (boundTableRelationConditionExpression == null)
		{
			return null;
		}
		TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, propertyValue);
		return new BoundTableFilterStatementPropertyValue(propertyValue, expressionContext.RelatedSymbol, boundTableRelationConditionExpression, navType, hasErrors);
	}

	private BoundPropertyValue BindFiltersPropertyValue(FiltersPropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		TableExpressionContext tableExpressionContext = TryGetTableExpressionContext(propertyValue, info, diagnostics, out var boundBadPropertyValue);
		if (boundBadPropertyValue != null)
		{
			return boundBadPropertyValue;
		}
		if (tableExpressionContext == null)
		{
			return null;
		}
		WhereExpressionSyntax tableFilter = propertyValue.TableFilter;
		BoundTableRelationConditionExpression tableFilter2 = BindPropertyExpressions(tableFilter, info, tableFilter.Filter.Conditions, tableExpressionContext, Compilation.Options, diagnostics, out var hasErrors);
		TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, propertyValue);
		ImmutableArray<BoundExpression> empty = ImmutableArray<BoundExpression>.Empty;
		return new BoundTableViewPropertyValue(propertyValue, tableExpressionContext.RelatedTable, empty, null, tableFilter2, navType, hasErrors);
	}

	private BoundPropertyValue BindTableViewStatement(TableViewPropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		TableExpressionContext tableExpressionContext = TryGetTableExpressionContext(propertyValue, info, diagnostics, out var boundBadPropertyValue);
		if (boundBadPropertyValue != null)
		{
			return boundBadPropertyValue;
		}
		if (tableExpressionContext == null)
		{
			return null;
		}
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		ImmutableArray<BoundExpression> sorting;
		try
		{
			boundBadPropertyValue = BindSorting(instance, propertyValue.Sorting, tableExpressionContext, diagnostics);
			if (boundBadPropertyValue != null)
			{
				return boundBadPropertyValue;
			}
			sorting = instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
		bool? isAscending = null;
		if (propertyValue.Order != null)
		{
			isAscending = propertyValue.Order.OrderBy.Kind == SyntaxKind.AscendingKeyword;
		}
		bool hasErrors = false;
		BoundTableRelationConditionExpression tableFilter = null;
		if (propertyValue.TableFilter != null)
		{
			WhereExpressionSyntax tableFilter2 = propertyValue.TableFilter;
			if (tableFilter2?.Filter != null)
			{
				tableFilter = BindPropertyExpressions(tableFilter2, info, tableFilter2.Filter.Conditions, tableExpressionContext, Compilation.Options, diagnostics, out hasErrors);
			}
		}
		TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, propertyValue);
		return new BoundTableViewPropertyValue(propertyValue, tableExpressionContext.RelatedTable, sorting, isAscending, tableFilter, navType, hasErrors);
	}

	private BoundBadPropertyValue BindSorting(ArrayBuilder<BoundExpression> builder, SortingSyntax sorting, TableExpressionContext tableContext, DiagnosticBag diagnostics)
	{
		TableTypeSymbol relatedTable = tableContext.RelatedTable;
		BoundBadPropertyValue boundBadPropertyValue = ((sorting == null) ? null : BindFieldList(builder, relatedTable, sorting.Parent, sorting.SortingFields, diagnostics));
		if (boundBadPropertyValue != null || ((sorting == null) | (builder.Count == 0)))
		{
			return boundBadPropertyValue;
		}
		ArrayBuilder<KeySymbol> instance = ArrayBuilder<KeySymbol>.GetInstance();
		try
		{
			foreach (TableExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in Compilation.GetApplicationObjectExtensionTypeSymbolsAcrossModules(relatedTable))
			{
				instance.AddRange(applicationObjectExtensionTypeSymbolsAcrossModule.GetAddedKeys());
			}
			instance.AddRange(relatedTable.GetKeys());
			if (instance.Count == 0 && builder.Count > 0)
			{
				Error(diagnostics, ErrorCode.WRN_SortingFieldsShouldBePartOfKey, sorting, relatedTable.Name);
				return null;
			}
			CheckSortingFieldIsInAKey(builder, instance, relatedTable, sorting, diagnostics);
		}
		finally
		{
			instance.Free();
		}
		return null;
	}

	private static void CheckSortingFieldIsInAKey(ArrayBuilder<BoundExpression> builder, ArrayBuilder<KeySymbol> keys, TableTypeSymbol table, SortingSyntax sorting, DiagnosticBag diagnostics)
	{
		if (builder.Count == 0)
		{
			return;
		}
		PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
		try
		{
			foreach (KeySymbol key in keys)
			{
				foreach (FieldSymbol field in key.Fields)
				{
					instance.Add(field.Name);
				}
			}
			foreach (BoundFieldAccess item in builder)
			{
				if (!instance.Contains(item.FieldSymbol.Name))
				{
					Error(diagnostics, ErrorCode.WRN_SortingFieldShouldBePartOfKey, sorting, item.FieldSymbol.Name, table.Name);
					break;
				}
			}
		}
		finally
		{
			instance.Free();
		}
	}

	private TableExpressionContext TryGetTableExpressionContext(PropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics, out BoundBadPropertyValue boundBadPropertyValue)
	{
		boundBadPropertyValue = null;
		DebugAssertHelper.Assert(propertyValue.Parent != null && propertyValue.Parent.Parent != null);
		DebugAssertHelper.Assert(info != null);
		switch (info?.Kind)
		{
		case PropertyKind.SourceTableView:
		{
			TableTypeSymbol relatedTable = GetRelatedTableForSourceTableView(propertyValue);
			return CreateDefaultTableContext(relatedTable);
		}
		case PropertyKind.LinkFields:
			return GetTableExpressionContextForLinkFields(propertyValue, diagnostics, ref boundBadPropertyValue);
		case PropertyKind.SubPageView:
		case PropertyKind.SubPageLink:
			return GetTableExpressionContextForSubPage(propertyValue, diagnostics, ref boundBadPropertyValue);
		case PropertyKind.RunPageView:
		case PropertyKind.RunPageLink:
		{
			TableTypeSymbol relatedTable = GetRelatedTableForRunPage(propertyValue, diagnostics, ref boundBadPropertyValue);
			return CreateDefaultTableContext(relatedTable);
		}
		case PropertyKind.Filters:
		{
			TableTypeSymbol relatedTable = GetTableTypeSymbolForFiltersProperty(propertyValue, info, diagnostics);
			return CreateDefaultTableContext(relatedTable);
		}
		case PropertyKind.DataItemTableView:
		{
			TableTypeSymbol relatedTable = GetRelatedTableForDataItemTableView();
			return CreateDefaultTableContext(relatedTable);
		}
		case PropertyKind.DataItemTableFilter:
		{
			TableTypeSymbol relatedTable = ((QueryDataItemSymbol)ContainingMember).RelatedTable;
			return CreateDefaultTableContext(relatedTable);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(info.Kind);
		}
	}

	private TableTypeSymbol GetTableTypeSymbolForFiltersProperty(PropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		PageTypeSymbol targetPage = GetTargetPage();
		if (targetPage == null)
		{
			return null;
		}
		if (targetPage.RelatedTable == null)
		{
			Error(diagnostics, ErrorCode.ERR_PropertyNotAllowedOnPageWithoutSourceTable, propertyValue, info.Kind, targetPage.Name);
		}
		return targetPage.RelatedTable;
	}

	private PageTypeSymbol GetTargetPage()
	{
		switch (ContainingMember.Kind)
		{
		case SymbolKind.Page:
			return (PageTypeSymbol)ContainingMember;
		case SymbolKind.PageExtension:
		case SymbolKind.PageCustomization:
			return (PageTypeSymbol)((PageExtensionBaseTypeSymbol)ContainingMember).Target;
		default:
			return null;
		}
	}

	private TableExpressionContext GetTableExpressionContextForLinkFields(PropertyValueSyntax propertyValue, DiagnosticBag diagnostics, ref BoundBadPropertyValue boundBadPropertyValue)
	{
		SourceXmlPortNodeSymbol symbol = (SourceXmlPortNodeSymbol)ContainingMember;
		BoundMemberReferencePropertyValue boundPropertyValue = symbol.GetBoundPropertyValue<BoundMemberReferencePropertyValue>(PropertyKind.LinkTable);
		if (boundPropertyValue == null)
		{
			return null;
		}
		XmlPortNodeSymbol xmlPortNodeSymbol = boundPropertyValue.MemberReference as XmlPortNodeSymbol;
		if (xmlPortNodeSymbol == null)
		{
			return null;
		}
		TableTypeSymbol relatedTableSymbol = xmlPortNodeSymbol.GetRelatedTableSymbol();
		if (relatedTableSymbol == null)
		{
			return null;
		}
		TableTypeSymbol relatedTableSymbol2 = symbol.GetRelatedTableSymbol();
		if (relatedTableSymbol2 == null)
		{
			return null;
		}
		Binder lookupBinder = new BinderFactory(Compilation, propertyValue.SyntaxTree).GetLookupBinder(relatedTableSymbol2);
		return new TableExpressionContext(relatedTableSymbol2, lookupBinder, relatedTableSymbol);
	}

	private ExpressionContext TryGetExpressionContext(PropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics, out BoundBadPropertyValue boundBadPropertyValue)
	{
		boundBadPropertyValue = null;
		DebugAssertHelper.Assert(propertyValue.Parent != null && propertyValue.Parent.Parent != null);
		DebugAssertHelper.Assert(info != null);
		PropertyKind? propertyKind = info?.Kind;
		if (propertyKind.HasValue)
		{
			PropertyKind valueOrDefault = propertyKind.GetValueOrDefault();
			if (valueOrDefault == PropertyKind.ColumnFilter)
			{
				return new QueryColumnFilterContext(ContainingMember as SynthesizeQueryElementContainer, this);
			}
		}
		return TryGetTableExpressionContext(propertyValue, info, diagnostics, out boundBadPropertyValue);
	}

	private TableTypeSymbol GetRelatedTableForReportDataItemLink()
	{
		return ((SourceReportDataItemSymbol)ContainingMember).GetDataItemLinkReferencedDataItem()?.RelatedTable;
	}

	private TableTypeSymbol GetRelatedTableForDataItemTableView()
	{
		return ((ReportDataItemSymbol)ContainingMember).RelatedTable;
	}

	private TableTypeSymbol GetRelatedTableForRunPage(PropertyValueSyntax propertyValue, DiagnosticBag diagnostics, ref BoundBadPropertyValue boundBadPropertyValue)
	{
		PageActionSyntax pageAction = propertyValue.Parent.Parent.Parent as PageActionSyntax;
		BoundApplicationObjectReferencePropertyValue boundApplicationObjectToActionReferencePropertyValue = GetBoundApplicationObjectToActionReferencePropertyValue(pageAction);
		if (boundApplicationObjectToActionReferencePropertyValue == null)
		{
			string text = ((PropertySyntax)propertyValue.Parent).Name.Identifier.Text;
			boundBadPropertyValue = CreateBoundBadPropertyValue(ErrorCode.ERR_RequiresApplicationObjectRunObject, propertyValue, propertyValue, diagnostics, text);
			return null;
		}
		return boundApplicationObjectToActionReferencePropertyValue.Value?.GetRelatedTableSymbol();
	}

	private TableExpressionContext GetTableExpressionContextForSubPage(PropertyValueSyntax propertyValue, DiagnosticBag diagnostics, ref BoundBadPropertyValue boundBadPropertyValue)
	{
		ControlSymbol extendedPageControlOrSelf = ContainingMember.GetExtendedPageControlOrSelf();
		if (extendedPageControlOrSelf == null)
		{
			return null;
		}
		PageTypeSymbol relatedPartSymbol = extendedPageControlOrSelf.RelatedPartSymbol;
		if (relatedPartSymbol == null)
		{
			return null;
		}
		if (relatedPartSymbol?.RelatedTable == null)
		{
			boundBadPropertyValue = CreateBoundBadPropertyValue(ErrorCode.ERR_PageMustSpecifySourceTable, propertyValue, propertyValue, diagnostics, relatedPartSymbol?.Name);
			return null;
		}
		TableTypeSymbol relatedTable = relatedPartSymbol.RelatedTable;
		TableTypeSymbol tableTypeSymbol = extendedPageControlOrSelf.TryGetSourceTableByProviderId();
		if (tableTypeSymbol == null)
		{
			return CreateDefaultTableContext(relatedTable);
		}
		Binder lookupBinder = new BinderFactory(Compilation, propertyValue.SyntaxTree).GetLookupBinder(tableTypeSymbol);
		return new TableExpressionContext(tableTypeSymbol, lookupBinder, relatedTable);
	}

	private TableTypeSymbol GetRelatedTableForSourceTableView(PropertyValueSyntax propertyValue)
	{
		SyntaxNode parent = propertyValue.Parent;
		if (parent != null && parent.Parent?.Parent.Kind.IsXmlPortNode() == true)
		{
			return GetTableTypeSymbolFromXmlPortNode(propertyValue.Parent.Parent.Parent);
		}
		return GetTableTypeSymbolFromContainingApplicationObject();
	}

	private TableTypeSymbol GetTableTypeSymbolFromXmlPortNode(SyntaxNode parentParent)
	{
		if (parentParent is XmlPortTableElementSyntax tableSyntax)
		{
			return SourceXmlPortNodeSymbol.GetRelatedTable(tableSyntax, ContainingMember.DeclaringCompilation)?.BaseType as TableTypeSymbol;
		}
		return null;
	}

	private BoundPropertyValue BindTableRelationPropertyValue(TableRelationPropertyValueSyntax syntax, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		if (!TryBindTableRelationExpression(syntax, info, diagnostics, out var tableRelationExpression, out var badPropertyvalue))
		{
			return badPropertyvalue;
		}
		TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, syntax);
		return new BoundTableRelationPropertyValue(syntax, tableRelationExpression, navType);
	}

	private bool TryBindTableRelationExpression(TableRelationPropertyValueSyntax syntax, PropertyTypeInfo info, DiagnosticBag diagnostics, out BoundTableRelationExpression tableRelationExpression, out BoundBadPropertyValue badPropertyvalue)
	{
		DebugAssertHelper.Assert(syntax != null);
		badPropertyvalue = null;
		tableRelationExpression = null;
		syntax.RelatedTableField.SplitMemberAccess(out var lhs, out var rhs);
		if (string.IsNullOrEmpty(lhs))
		{
			badPropertyvalue = CreateBoundBadPropertyValue(ErrorCode.ERR_ExpectedIdentifierOrMemberAccess, syntax, syntax, diagnostics);
			return false;
		}
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = Compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Table, lhs);
		TableTypeSymbol tableTypeSymbol = null;
		if (objectSymbolsByNameAcrossModules.Length > 1)
		{
			diagnostics.Add(ErrorCode.ERR_AmbigReference, syntax.Location, lhs);
		}
		else if (objectSymbolsByNameAcrossModules.Length == 1)
		{
			tableTypeSymbol = (TableTypeSymbol)objectSymbolsByNameAcrossModules[0];
		}
		if (tableTypeSymbol == null)
		{
			badPropertyvalue = CreateBoundBadPropertyValue(ErrorCode.ERR_MissingApplicationObject, syntax, syntax.RelatedTableField, diagnostics, SymbolKind.Table.ToString(), lhs);
			return false;
		}
		bool hasErrors = false;
		BoundExpression boundExpression;
		if (string.IsNullOrEmpty(rhs))
		{
			boundExpression = new BoundClassType(syntax.RelatedTableField.Identifier, DatabaseClassTypeSymbol.Instance, tableTypeSymbol);
		}
		else
		{
			hasErrors = !BindFieldAccess(tableTypeSymbol, syntax.RelatedTableField.Identifier, rhs, diagnostics, out boundExpression);
			if (hasErrors)
			{
				badPropertyvalue = new BoundBadPropertyValue(syntax, BadExpression(syntax, LookupResultKind.Empty), null, hasErrors: true);
				return false;
			}
		}
		BoundTableRelationConditionExpression ifExpression = null;
		TableExpressionContext expressionContext = CreateDefaultTableContext(tableTypeSymbol);
		if (syntax.IfExpression != null && syntax.IfExpression.IfTableRelationCondition != null)
		{
			ifExpression = BindPropertyExpressions(syntax.IfExpression, info, syntax.IfExpression.IfTableRelationCondition.Conditions, expressionContext, Compilation.Options, diagnostics, out hasErrors);
		}
		BoundTableRelationConditionExpression tableFilter = null;
		if (!hasErrors && syntax.TableFilter != null && syntax.TableFilter.Filter != null)
		{
			tableFilter = BindPropertyExpressions(syntax.TableFilter, info, syntax.TableFilter.Filter.Conditions, expressionContext, Compilation.Options, diagnostics, out hasErrors);
		}
		BoundTableRelationExpression tableRelationExpression2 = null;
		if (!hasErrors && syntax.ElseExpression != null && syntax.ElseExpression.ElseTableRelationCondition != null && !TryBindTableRelationExpression(syntax.ElseExpression.ElseTableRelationCondition, info, diagnostics, out tableRelationExpression2, out badPropertyvalue))
		{
			return false;
		}
		if (hasErrors)
		{
			badPropertyvalue = new BoundBadPropertyValue(syntax, BadExpression(syntax, LookupResultKind.Empty), null, hasErrors: true);
			return false;
		}
		ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, boundExpression.ExpressionSymbol, syntax);
		TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, syntax);
		tableRelationExpression = new BoundTableRelationExpression(syntax, ifExpression, boundExpression, tableFilter, tableRelationExpression2, navType);
		return true;
	}

	private static BoundTableRelationConditionExpression BindPropertyExpressions(SyntaxNode ownerSyntax, PropertyTypeInfo info, SeparatedSyntaxList<PropertyExpressionSyntax> propertyExpressions, ExpressionContext expressionContext, CompilationOptions options, DiagnosticBag diagnostics, out bool hasErrors)
	{
		hasErrors = false;
		ArrayBuilder<BoundEqualsPropertyExpression> instance = ArrayBuilder<BoundEqualsPropertyExpression>.GetInstance();
		ImmutableArray<BoundEqualsPropertyExpression> condition;
		try
		{
			bool isSourceTableBound = ShouldBindToSourceTableScope(ownerSyntax, info);
			DebugAssertHelper.Assert(expressionContext != null);
			foreach (PropertyExpressionSyntax item in propertyExpressions)
			{
				if (!ValidatePropertyExpressionKind(ownerSyntax.Kind, item.Kind, info.Kind, out var errorCode))
				{
					Error(diagnostics, errorCode, item);
					hasErrors = true;
					return null;
				}
				BoundEqualsPropertyExpression boundEqualsPropertyExpression;
				switch (item.Kind)
				{
				case SyntaxKind.ConstExpression:
				{
					ConstExpressionSyntax constExpressionSyntax = (ConstExpressionSyntax)item;
					boundEqualsPropertyExpression = new BoundPropertyExpressionFactory<ConstExpressionSyntax>(constExpressionSyntax, isSourceTableBound, expressionContext).CreateExpression(constExpressionSyntax.LeftHandSide, constExpressionSyntax.Identifier, diagnostics);
					break;
				}
				case SyntaxKind.SimpleFieldExpression:
				{
					SimpleFieldExpressionSyntax simpleFieldExpressionSyntax = (SimpleFieldExpressionSyntax)item;
					boundEqualsPropertyExpression = new BoundPropertyExpressionFactory<SimpleFieldExpressionSyntax>(simpleFieldExpressionSyntax, isSourceTableBound, expressionContext).CreateExpression(simpleFieldExpressionSyntax.LeftHandSide, simpleFieldExpressionSyntax.Identifier, diagnostics);
					break;
				}
				case SyntaxKind.FilterExpression:
				{
					FilterExpressionSyntax filterExpressionSyntax = (FilterExpressionSyntax)item;
					boundEqualsPropertyExpression = new BoundPropertyExpressionFactory<FilterExpressionSyntax>(filterExpressionSyntax, isSourceTableBound, expressionContext).CreateExpression(filterExpressionSyntax.LeftHandSide, filterExpressionSyntax.Filter, diagnostics);
					break;
				}
				case SyntaxKind.FieldFilterExpression:
				{
					FieldFilterExpressionSyntax fieldFilterExpressionSyntax = (FieldFilterExpressionSyntax)item;
					boundEqualsPropertyExpression = new BoundPropertyExpressionFactory<FieldFilterExpressionSyntax>(fieldFilterExpressionSyntax, isSourceTableBound, expressionContext).CreateExpression(fieldFilterExpressionSyntax.LeftHandSide, fieldFilterExpressionSyntax.Identifier, diagnostics);
					break;
				}
				case SyntaxKind.FieldUpperLimitFilterExpression:
				{
					FieldUpperLimitFilterExpressionSyntax fieldUpperLimitFilterExpressionSyntax = (FieldUpperLimitFilterExpressionSyntax)item;
					boundEqualsPropertyExpression = new BoundPropertyExpressionFactory<FieldUpperLimitFilterExpressionSyntax>(fieldUpperLimitFilterExpressionSyntax, isSourceTableBound, expressionContext).CreateExpression(fieldUpperLimitFilterExpressionSyntax.LeftHandSide, fieldUpperLimitFilterExpressionSyntax.Identifier, diagnostics);
					break;
				}
				case SyntaxKind.FieldUpperLimitExpression:
				{
					FieldUpperLimitExpressionSyntax fieldUpperLimitExpressionSyntax = (FieldUpperLimitExpressionSyntax)item;
					boundEqualsPropertyExpression = new BoundPropertyExpressionFactory<FieldUpperLimitExpressionSyntax>(fieldUpperLimitExpressionSyntax, isSourceTableBound, expressionContext).CreateExpression(fieldUpperLimitExpressionSyntax.LeftHandSide, fieldUpperLimitExpressionSyntax.Identifier, diagnostics);
					break;
				}
				case SyntaxKind.InvalidPropertyExpression:
					hasErrors = true;
					return null;
				default:
					throw ExceptionUtilities.UnexpectedValue(item.Kind);
				}
				if (boundEqualsPropertyExpression != null)
				{
					instance.Add(boundEqualsPropertyExpression);
				}
				else if (!options.Target.IsPersonalizationTarget() || info.Kind != PropertyKind.Filters)
				{
					hasErrors = true;
					return null;
				}
			}
		}
		finally
		{
			condition = instance.ToImmutableAndFree();
		}
		TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, ownerSyntax);
		return new BoundTableRelationConditionExpression(ownerSyntax, condition, navType, hasErrors);
	}

	private static bool ValidatePropertyExpressionKind(SyntaxKind ownerSyntaxKind, SyntaxKind propertyExpressionKind, PropertyKind propertyKind, out ErrorCode errorCode)
	{
		errorCode = ErrorCode.ERR_PropertyExpressionKindIsNotValidExpectedConstOrFieldOrFilter;
		bool flag = propertyExpressionKind == SyntaxKind.FilterExpression || propertyExpressionKind == SyntaxKind.ConstExpression;
		bool flag2 = flag || propertyExpressionKind == SyntaxKind.SimpleFieldExpression;
		bool result = flag2 || propertyExpressionKind == SyntaxKind.FieldFilterExpression || propertyExpressionKind == SyntaxKind.FieldUpperLimitFilterExpression || propertyExpressionKind == SyntaxKind.FieldUpperLimitExpression;
		switch (propertyKind)
		{
		case PropertyKind.TableRelation:
			if (ownerSyntaxKind == SyntaxKind.IfTableRelationExpression)
			{
				errorCode = ErrorCode.ERR_PropertyExpressionKindIsNotValidExpectedConstOrFilter;
				return flag;
			}
			break;
		case PropertyKind.SourceTableView:
		case PropertyKind.SubPageView:
		case PropertyKind.RunPageView:
		case PropertyKind.DataItemTableView:
			errorCode = ErrorCode.ERR_PropertyExpressionKindIsNotValidExpectedConstOrFilter;
			return flag;
		case PropertyKind.CalcFormula:
		case PropertyKind.SubPageLink:
		case PropertyKind.RunPageLink:
			errorCode = ErrorCode.ERR_PropertyExpressionKindIsNotValidExpectedAllExpressionKind;
			return result;
		}
		return flag2;
	}

	private static bool ShouldBindToSourceTableScope(SyntaxNode ownerSyntax, PropertyTypeInfo info)
	{
		if (ownerSyntax.Kind != SyntaxKind.IfTableRelationExpression && info.Kind != PropertyKind.DataItemLink && info.Kind != PropertyKind.LinkFields)
		{
			return info.Kind == PropertyKind.ColumnFilter;
		}
		return true;
	}

	internal static BoundPropertyValue BindMultiLanguagePropertyValue(MultilanguagePropertyValueSyntax syntax, CompilerFeatures compilerFeatures, DiagnosticBag diagnostics)
	{
		ImmutableArray<BoundMultilanguageExpression> value = BindMultiLanguageValue(syntax.Value, compilerFeatures, diagnostics, out var hasErrors);
		TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, syntax);
		return new BoundMultilanguagePropertyValue(syntax, value, navType, hasErrors);
	}

	internal static BoundPropertyValue BindLabelPropertyValue(LabelPropertyValueSyntax syntax, DiagnosticBag diagnostics)
	{
		BoundLabel boundLabel = BindLabel(syntax.Value, diagnostics);
		TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, syntax);
		return new BoundLabelPropertyValue(syntax, boundLabel, navType, diagnostics.HasAnyErrors());
	}

	private static BoundPropertyValue BindLiteralPropertyValue(PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		NavTypeKind navTypeKind = NavTypeExtensions.FromLiteralSyntaxKind(propertyValueSyntax.Kind);
		TypeSymbol navType = GetNavType(navTypeKind, diagnostics, propertyValueSyntax);
		ConstantValue value = ConstantValue.Create(propertyValueSyntax.GetPropertyValue(), navTypeKind);
		return new BoundLiteralPropertyValue(propertyValueSyntax, value, navType);
	}

	private BoundPropertyValue BindScriptLiteralPropertyValue(PropertyValueSyntax propertyValueSyntax, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		NavTypeKind navTypeKind = NavTypeExtensions.FromLiteralSyntaxKind(propertyValueSyntax.Kind);
		TypeSymbol navType = GetNavType(navTypeKind, diagnostics, propertyValueSyntax);
		ConstantValue constantValue = ConstantValue.Create(propertyValueSyntax.GetPropertyValue(), navTypeKind);
		string stringValue = constantValue.StringValue;
		Uri uri = UriUtilities.ParseUri(stringValue);
		if ((object)uri != null && uri.IsAbsoluteUri && !uri.IsFile)
		{
			diagnostics.Add(ErrorCode.ERR_ExternalResourceNotAllowed, propertyValueSyntax.Location, info.Name);
		}
		else if (!string.IsNullOrEmpty(stringValue) && !Compilation.FileSystem.Exists(stringValue))
		{
			diagnostics.Add(ErrorCode.ERR_MissingDocument, propertyValueSyntax.Location, stringValue);
		}
		return new BoundLiteralPropertyValue(propertyValueSyntax, constantValue, navType);
	}

	private BoundPropertyValue BindContextHelpLiteralPropertyValue(PropertyValueSyntax propertyValueSyntax, DiagnosticBag diagnostics)
	{
		if (string.IsNullOrEmpty(Compilation.Options.ContextSensitiveHelpUrl))
		{
			diagnostics.Add(ErrorCode.ERR_ContextSentiveHelpRequiresManifestProperty, propertyValueSyntax.Location);
		}
		string propertyValueText = propertyValueSyntax.GetPropertyValueText();
		if (propertyValueText.Contains("{0}") || !NavAppManifest.ValidateHelpUrlPlaceholders(propertyValueText))
		{
			diagnostics.Add(ErrorCode.ERR_ContextSentiveHelpWithPlaceholder, propertyValueSyntax.Location);
		}
		return BindLiteralPropertyValue(propertyValueSyntax, diagnostics);
	}

	private BoundPropertyValue BindLinkLiteralPropertyValue(PropertyValueSyntax propertyValueSyntax, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		NavTypeKind navTypeKind = NavTypeExtensions.FromLiteralSyntaxKind(propertyValueSyntax.Kind);
		TypeSymbol navType = GetNavType(navTypeKind, diagnostics, propertyValueSyntax);
		ConstantValue constantValue = ConstantValue.Create(propertyValueSyntax.GetPropertyValue(), navTypeKind);
		string stringValue = constantValue.StringValue;
		Uri uri = UriUtilities.ParseUri(stringValue);
		if (uri == null || !uri.IsAbsoluteUri || !SupportedLinkSchemes.Contains(uri.Scheme))
		{
			diagnostics.Add(ErrorCode.ERR_WrongLinkFormat, propertyValueSyntax.Location, stringValue);
		}
		ValidateUrlWithEventuallyPlaceHolderForLocale(stringValue, propertyValueSyntax.Location, info, diagnostics);
		return new BoundLiteralPropertyValue(propertyValueSyntax, constantValue, navType);
	}

	private void ValidateUrlWithEventuallyPlaceHolderForLocale(string url, Location location, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		if (url.Contains("{0}"))
		{
			if (!NavAppManifest.ValidateHelpUrlPlaceholders(url))
			{
				diagnostics.Add(ErrorCode.ERR_HelpUrlWithMissingLocalePlaceHolders, location, url);
			}
			if (Compilation.Options.SupportedLocales == null || Compilation.Options.SupportedLocales.IsEmpty)
			{
				diagnostics.Add(ErrorCode.ERR_MissingSupportedLocalesManifestProperty, location, info.Kind);
			}
		}
		else if (!NavAppManifest.ValidateHelpUrlPlaceholders(url))
		{
			diagnostics.Add(ErrorCode.ERR_InvalidHelpLinkProperty, location, url);
		}
	}

	private BoundPropertyValue BindEnumPropertyValue(EnumPropertyValueSyntax propertyValueSyntax, EnumPropertyTypeInfo propInfo, DiagnosticBag diagnostics)
	{
		TypeSymbol navType = GetNavType(NavTypeKind.Option, diagnostics, propertyValueSyntax);
		string propertyValueText = propertyValueSyntax.GetPropertyValueText();
		int num = propInfo?.Options.IndexOf(propertyValueText, 0, StringComparer.OrdinalIgnoreCase) ?? (-1);
		if (num < 0)
		{
			Error(diagnostics, ErrorCode.ERR_InvalidOptionValue, propertyValueSyntax, propertyValueText);
			return new BoundBadPropertyValue(propertyValueSyntax, BadExpression(propertyValueSyntax, LookupResultKind.NotAnOption), navType, hasErrors: true);
		}
		VersionChecker.CheckPropertyEnumValueAvailability(propInfo, propertyValueSyntax, diagnostics);
		OptionSymbol value = SourceOptionSymbol.Create(navType, propertyValueText, num, propertyValueSyntax.Value);
		return new BoundEnumPropertyValue(propertyValueSyntax, value, navType);
	}

	private BoundPropertyValue BindOptionValuesPropertyValue(OptionValuesPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		SourceFieldSymbol sourceFieldSymbol = ContainingMember as SourceFieldSymbol;
		if (sourceFieldSymbol != null)
		{
			return new BoundOptionMembersPropertyValue(propertyValue, sourceFieldSymbol.Type, sourceFieldSymbol.Type);
		}
		TypeSymbol navType = GetNavType(NavTypeKind.Option, diagnostics, propertyValue);
		SourceOptionTypeSymbol value = SourceOptionTypeSymbol.Create(navType, propertyValue.Value, diagnostics);
		return new BoundOptionMembersPropertyValue(propertyValue, value, navType);
	}

	private BoundPropertyValue BindPermissionPropertyValue(PermissionPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		ArrayBuilder<BoundPermissionExpression> instance = ArrayBuilder<BoundPermissionExpression>.GetInstance();
		try
		{
			TypeSymbol navType = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
			foreach (PermissionSyntax permissionProperty in propertyValue.PermissionProperties)
			{
				SyntaxKind kind = permissionProperty.ObjectType.Kind;
				ConstantValue permission = ((permissionProperty.Permissions.ValueText != null) ? ConstantValue.Create((int)permissionProperty.Permissions.ValueText.GetPermissionMask()) : ConstantValue.Create(0));
				SymbolKind symbolKind = kind.ExpectedFromApplicationObjectSyntaxKind();
				ObjectNameOrIdSyntax objectReference = permissionProperty.ObjectReference;
				ApplicationObjectTypeSymbol applicationObjectTypeSymbol = GetApplicationObjectTypeSymbol(symbolKind, objectReference, diagnostics);
				TypeSymbol navType2 = GetNavType(NavTypeKind.String, diagnostics, permissionProperty);
				if (kind == SyntaxKind.SystemKeyword)
				{
					string identifierOrLiteralValue = objectReference.Identifier.GetIdentifierOrLiteralValue();
					if (!SystemObjects.SystemNameAndObjectsDictionary.ContainsKey(identifierOrLiteralValue))
					{
						return CreateBoundBadPropertyValue(ErrorCode.ERR_InvalidSystemObject, permissionProperty, objectReference, diagnostics);
					}
					BoundLiteral target = new BoundLiteral(permissionProperty, ConstantValue.Create(identifierOrLiteralValue), navType2);
					instance.Add(new BoundPermissionExpression(permissionProperty, kind, target, permission, navType2));
				}
				else
				{
					if (applicationObjectTypeSymbol == null)
					{
						return CreateBoundBadPropertyValue(ErrorCode.ERR_MissingApplicationObject, permissionProperty, objectReference, diagnostics, symbolKind.ToString(), objectReference.ToString());
					}
					instance.Add(new BoundPermissionExpression(permissionProperty, kind, new BoundApplicationObjectAccess(objectReference, null, applicationObjectTypeSymbol, LookupResultKind.Viable, applicationObjectTypeSymbol), permission, navType2));
				}
			}
			return new BoundPermissionPropertyValue(propertyValue, instance.ToImmutable(), navType);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundPropertyValue BindCommaSeparatedListPropertyValue(CommaSeparatedPropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		BoundBadPropertyValue boundBadProperty = null;
		ImmutableArray<BoundExpression> value;
		try
		{
			switch (info.Kind)
			{
			case PropertyKind.DataCaptionFields:
			case PropertyKind.SumIndexFields:
			case PropertyKind.SqlIndex:
			case PropertyKind.ODataKeyFields:
			case PropertyKind.RequestFilterFields:
			case PropertyKind.CalcFields:
				value = BindTableFieldReferences(instance, propertyValue, info, diagnostics, out boundBadProperty);
				break;
			case PropertyKind.ApplicationArea:
				BuildLiteralExpressionsFromCommaSeparatedIdentifiers(instance, propertyValue.Values, diagnostics);
				value = instance.ToImmutable();
				break;
			case PropertyKind.IndentationControls:
				return BindPageFieldReferences(propertyValue, propertyValue.Values, info.Kind, diagnostics);
			case PropertyKind.Customizations:
				value = BindPageCustomizationsReferences(instance, propertyValue, diagnostics, out boundBadProperty);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(info.Kind);
			}
		}
		finally
		{
			instance.Free();
		}
		if (boundBadProperty != null)
		{
			return boundBadProperty;
		}
		TypeSymbol navType = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
		return new BoundIdentifierListPropertyValue(propertyValue, value, navType);
	}

	private static BoundPropertyValue BindCommaSeparatedIdentifierEqualsStringListPropertyValue(CommaSeparatedIdentifierEqualsStringListPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		ArrayBuilder<BoundPairOfConstantValues> instance = ArrayBuilder<BoundPairOfConstantValues>.GetInstance();
		ImmutableArray<BoundPairOfConstantValues> value;
		try
		{
			foreach (IdentifierEqualsStringSyntax value2 in propertyValue.List.Values)
			{
				ConstantValue first = ConstantValue.Create(value2.Identifier.ValueText.UnquoteIdentifier());
				ConstantValue second = ConstantValue.Create(value2.StringLiteral.Value.ValueText.UnquoteLiteral());
				TypeSymbol navType = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
				BoundPairOfConstantValues item = new BoundPairOfConstantValues(value2, first, second, navType);
				instance.Add(item);
			}
		}
		finally
		{
			value = instance.ToImmutableAndFree();
		}
		TypeSymbol navType2 = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
		return new BoundListOfConstantValuePairsPropertyValue(propertyValue, value, navType2);
	}

	private ImmutableArray<BoundExpression> BindPageCustomizationsReferences(ArrayBuilder<BoundExpression> builder, CommaSeparatedPropertyValueSyntax propertyValue, DiagnosticBag diagnostics, out BoundBadPropertyValue boundBadPropertyValue)
	{
		boundBadPropertyValue = null;
		ValidateForDuplicateNames(propertyValue.Values, diagnostics);
		foreach (IdentifierNameSyntax value in propertyValue.Values)
		{
			string text = value.Identifier.ValueText.UnquoteIdentifier();
			ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = Compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.PageCustomization, text);
			if (objectSymbolsByNameAcrossModules.Length == 0)
			{
				boundBadPropertyValue = CreateBoundBadPropertyValue(ErrorCode.ERR_MissingApplicationObject, propertyValue.Parent, propertyValue, diagnostics, SymbolKind.PageCustomization.ToString(), text);
				return ImmutableArray<BoundExpression>.Empty;
			}
			if (objectSymbolsByNameAcrossModules.Length > 1)
			{
				boundBadPropertyValue = CreateBoundBadPropertyValue(ErrorCode.ERR_AmbigReference, propertyValue.Parent, propertyValue, diagnostics, value.Location, text);
				return ImmutableArray<BoundExpression>.Empty;
			}
			PageCustomizationTypeSymbol pageCustomizationTypeSymbol = (PageCustomizationTypeSymbol)objectSymbolsByNameAcrossModules[0];
			BoundClassType receiverOpt = new BoundClassType(value, PageClassTypeSymbol.Instance, pageCustomizationTypeSymbol);
			BoundApplicationObjectAccess item = new BoundApplicationObjectAccess(value, receiverOpt, pageCustomizationTypeSymbol, LookupResultKind.Viable, TypeSymbol.GetNavTypeSymbol(NavTypeKind.String));
			builder.Add(item);
		}
		return builder.ToImmutable();
	}

	private BoundPropertyValue BindLiteralListProperty(PropertyTypeInfo propertyInfo, CommaSeparatedStringsPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		ArrayBuilder<ConstantValue> instance = ArrayBuilder<ConstantValue>.GetInstance();
		PooledHashSet<string> instance2 = PooledHashSet<string>.GetInstance();
		if (propertyValue.Values.Count == 0)
		{
			diagnostics.Add(ErrorCode.ERR_PropertyCannotBeBlank, propertyValue.GetLocation(), propertyInfo.Name);
		}
		foreach (StringLiteralValueSyntax value in propertyValue.Values)
		{
			string identifierOrLiteralValue = value.GetIdentifierOrLiteralValue();
			if (string.IsNullOrEmpty(identifierOrLiteralValue))
			{
				diagnostics.Add(ErrorCode.ERR_EmptyListMember, value.GetLocation(), propertyInfo.Name);
				continue;
			}
			ValidateScriptExistsInFileSystem(value, diagnostics);
			string item = identifierOrLiteralValue.ToUpperInvariant();
			if (instance2.Contains(item))
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateListMember, value.GetLocation(), identifierOrLiteralValue);
			}
			instance2.Add(item);
			instance.Add(ConstantValue.Create(identifierOrLiteralValue));
		}
		instance2.Free();
		TypeSymbol navType = GetNavType(NavTypeKind.String, diagnostics, propertyValue);
		return new BoundConstantValueListPropertyValue(propertyValue, instance.ToImmutableAndFree(), navType);
	}

	private void ValidateScriptExistsInFileSystem(StringLiteralValueSyntax listMember, DiagnosticBag diagnostics)
	{
		string identifierOrLiteralValue = listMember.GetIdentifierOrLiteralValue();
		Uri uri = UriUtilities.ParseUri(identifierOrLiteralValue);
		if (((object)uri != null && !uri.IsAbsoluteUri) || ((object)uri != null && uri.IsAbsoluteUri && uri.IsFile))
		{
			if (!Compilation.FileSystem.Exists(identifierOrLiteralValue))
			{
				diagnostics.Add(ErrorCode.ERR_MissingDocument, listMember.GetLocation(), identifierOrLiteralValue);
			}
		}
		else if (uri == null || (uri.IsAbsoluteUri && !SupportedLinkSchemes.Contains(uri.Scheme)))
		{
			diagnostics.Add(ErrorCode.ERR_ControlAddWrongLinkFormat, listMember.GetLocation(), identifierOrLiteralValue);
		}
	}

	private ImmutableArray<BoundExpression> BindTableFieldReferences(ArrayBuilder<BoundExpression> builder, CommaSeparatedPropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics, out BoundBadPropertyValue boundBadProperty)
	{
		TableTypeSymbol relatedTableForProperty = GetRelatedTableForProperty(info.Kind, propertyValue);
		if (relatedTableForProperty == null)
		{
			boundBadProperty = CreateBoundBadPropertyValue(ErrorCode.ERR_SourceTableUnreachable, propertyValue.Parent, propertyValue, diagnostics);
			return ImmutableArray<BoundExpression>.Empty;
		}
		DebugAssertHelper.Assert(info != null);
		Func<BoundFieldAccess, ErrorCode> validateFields = null;
		if (info.Kind == PropertyKind.SumIndexFields)
		{
			validateFields = (BoundFieldAccess fieldAccess) => (!fieldAccess.Type.NavTypeKind.IsValidSumIndexFieldValue()) ? ErrorCode.ERR_InvalidSumIndexKeyType : ErrorCode.None;
		}
		boundBadProperty = BindFieldList(builder, relatedTableForProperty, propertyValue, propertyValue.Values, diagnostics, validateFields);
		if (boundBadProperty != null)
		{
			return ImmutableArray<BoundExpression>.Empty;
		}
		ImmutableArray<BoundExpression> immutableArray = builder.ToImmutable();
		if (info.Kind == PropertyKind.SqlIndex && !ValidateSqlIndexPropertyValue(relatedTableForProperty, propertyValue, from BoundFieldAccess f in immutableArray
			select f.FieldSymbol, diagnostics))
		{
			boundBadProperty = new BoundBadPropertyValue(propertyValue, BadExpression(propertyValue, LookupResultKind.Empty), null, hasErrors: true);
			return ImmutableArray<BoundExpression>.Empty;
		}
		return immutableArray;
	}

	private static void BuildLiteralExpressionsFromCommaSeparatedIdentifiers(ArrayBuilder<BoundExpression> builder, SeparatedSyntaxList<IdentifierNameSyntax> propertyValueValues, DiagnosticBag diagnostics)
	{
		foreach (IdentifierNameSyntax item2 in propertyValueValues)
		{
			BoundLiteral item = CreateBoundStringLiteral(item2.Identifier.ValueText, item2, diagnostics);
			builder.Add(item);
		}
	}

	private static bool ValidateSqlIndexPropertyValue(TableTypeSymbol table, CommaSeparatedPropertyValueSyntax propertyValue, IEnumerable<FieldSymbol> fields, DiagnosticBag diagnostics)
	{
		KeySymbol primaryKey = table.GetPrimaryKey();
		DebugAssertHelper.Assert(primaryKey != null);
		KeySyntax keySyntax = propertyValue.GetAncestor<KeySyntax>();
		FieldNameListSymbol fieldNameListSymbol = table.GetKeys().FirstOrDefault((KeySymbol k) => string.Compare(k.Name, keySyntax.Name.GetIdentifierOrLiteralValue(), StringComparison.OrdinalIgnoreCase) == 0);
		if ((object)primaryKey == fieldNameListSymbol)
		{
			ImmutableHashSet<string> immutableHashSet = fields.Select((FieldSymbol f) => f.Name).ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);
			if (fieldNameListSymbol.Fields.Length != immutableHashSet.Count)
			{
				Error(diagnostics, ErrorCode.ERR_SqlIndexOnPKMustBeIdentical, propertyValue, table.Name);
				return false;
			}
			foreach (FieldSymbol field in fieldNameListSymbol.Fields)
			{
				if (!immutableHashSet.Contains(field.Name, StringComparer.OrdinalIgnoreCase))
				{
					Error(diagnostics, ErrorCode.ERR_SqlIndexOnPKMustBeIdentical, propertyValue, table.Name);
					return false;
				}
			}
		}
		return true;
	}

	private BoundBadPropertyValue BindFieldList(ArrayBuilder<BoundExpression> builder, TableTypeSymbol table, SyntaxNode propertyValue, SeparatedSyntaxList<IdentifierNameSyntax> fieldList, DiagnosticBag diagnostics, Func<BoundFieldAccess, ErrorCode> validateFields = null)
	{
		ValidateForDuplicateNames(fieldList, diagnostics);
		foreach (IdentifierNameSyntax item in fieldList)
		{
			string relatedFieldName = item.Unquoted();
			if (item.Identifier.IsMissing)
			{
				return CreateBoundBadPropertyValue(ErrorCode.ERR_MissingTypeReference, propertyValue, item, diagnostics, item.Identifier.ValueText, table.Name);
			}
			if (!BindFieldAccess(table, item, relatedFieldName, diagnostics, out var boundExpression))
			{
				return new BoundBadPropertyValue(propertyValue, BadExpression(propertyValue, LookupResultKind.Empty), null, hasErrors: true);
			}
			BoundFieldAccess arg = (BoundFieldAccess)boundExpression;
			ErrorCode errorCode = validateFields?.Invoke(arg) ?? ErrorCode.None;
			if (errorCode != ErrorCode.None)
			{
				return CreateBoundBadPropertyValue(errorCode, propertyValue, item, diagnostics, item.Identifier.ValueText);
			}
			builder.Add(boundExpression);
		}
		return null;
	}

	internal BoundBadPropertyValue BindPageFieldAccess(IPageBaseTypeSymbol containingPage, SyntaxNode propertyValue, IdentifierNameSyntax controlIdentifier, DiagnosticBag diagnostics, out BoundControlAccess controlAccess)
	{
		controlAccess = null;
		string text = controlIdentifier?.Unquoted();
		DebugAssertHelper.Assert(containingPage != null);
		DebugAssertHelper.Assert(!string.IsNullOrEmpty(text));
		ControlSymbol controlSymbol = this.FindSymbols(text, LookupOptions.MustBePageElement).OfType<ControlSymbol>().FirstIfSingle();
		if (controlSymbol == null)
		{
			if (Compilation.Options.Target.IsPersonalizationTarget())
			{
				return CreateBoundBadPropertyValue(propertyValue);
			}
			return CreateBoundBadPropertyValue(ErrorCode.ERR_MissingTypeReference, propertyValue, controlIdentifier, diagnostics, text, containingPage.Name);
		}
		BoundClassType receiverOpt = new BoundClassType(controlIdentifier, PageClassTypeSymbol.Instance, containingPage.GetTypeSymbol() as TypeSymbol);
		controlAccess = new BoundControlAccess(controlIdentifier, receiverOpt, controlSymbol, LookupResultKind.Viable, controlSymbol.Type);
		return null;
	}

	private BoundPropertyValue BindObjectReferencePropertyValue(ObjectReferencePropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		SymbolKind symbolKind = info.Kind.GetSymbolKind();
		return BindObjectReferencePropertyValue(symbolKind, propertyValue.ObjectNameOrId, info, diagnostics);
	}

	private BoundPropertyValue BindQualifiedObjectReferencePropertyValue(QualifiedObjectReferencePropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		PropertyKind kind = info.Kind;
		if (kind == PropertyKind.RunObject)
		{
			return BindRunObjectPropertyValue(propertyValue, info, diagnostics);
		}
		throw ExceptionUtilities.UnexpectedValue(info.Kind);
	}

	private BoundPropertyValue BindRunObjectPropertyValue(QualifiedObjectReferencePropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		SymbolKind symbolKind = propertyValue.ObjectType.Kind.ToSymbolKind();
		if ((uint)(symbolKind - 11) <= 4u)
		{
			return BindObjectReferencePropertyValue(symbolKind, propertyValue.ObjectNameOrId, info, diagnostics);
		}
		return CreateBoundBadPropertyValue(ErrorCode.ERR_InvalidRunObjectPropertyValue, propertyValue, propertyValue, diagnostics);
	}

	private BoundPropertyValue BindObjectReferencePropertyValue(SymbolKind kind, ObjectNameOrIdSyntax objectReference, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		ApplicationObjectTypeSymbol applicationObjectTypeSymbol = GetApplicationObjectTypeSymbol(kind, objectReference, diagnostics);
		if (applicationObjectTypeSymbol == null)
		{
			if (UseSoftValidationForProperty(info))
			{
				return new BoundApplicationObjectReferencePropertyValue(objectReference.Parent, null, NavCorLib.IntegerType);
			}
			return CreateBoundBadPropertyValue(ErrorCode.ERR_MissingApplicationObject, objectReference, objectReference, diagnostics, kind.ToString(), objectReference.ToString());
		}
		ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, applicationObjectTypeSymbol, objectReference);
		return new BoundApplicationObjectReferencePropertyValue(objectReference.Parent, applicationObjectTypeSymbol, applicationObjectTypeSymbol.ContainingType);
	}

	private bool UseSoftValidationForProperty(PropertyTypeInfo info)
	{
		PropertyKind kind = info.Kind;
		if ((uint)(kind - 219) <= 1u)
		{
			return Compilation.Options.Target.IsPersonalizationTarget();
		}
		return false;
	}

	internal ApplicationObjectTypeSymbol GetApplicationObjectTypeSymbol(SymbolKind symbolKind, ObjectNameOrIdSyntax objectReferenceSyntax, DiagnosticBag diagnostics)
	{
		ApplicationObjectTypeSymbol result = null;
		ImmutableArray<IApplicationObjectTypeSymbol> immutableArray = ImmutableArray<IApplicationObjectTypeSymbol>.Empty;
		string identifierOrLiteralValue = objectReferenceSyntax.Identifier.GetIdentifierOrLiteralValue();
		if (!int.TryParse(identifierOrLiteralValue, out var result2))
		{
			immutableArray = Compilation.GetApplicationObjectTypeSymbolsByNameAcrossModules(symbolKind, identifierOrLiteralValue);
		}
		if (immutableArray.Length == 0 && result2 != 0)
		{
			immutableArray = Compilation.GetApplicationObjectTypeSymbolsByIdAcrossModules(symbolKind, result2);
		}
		if (immutableArray.Length > 1)
		{
			diagnostics.Add(ErrorCode.ERR_AmbigReference, objectReferenceSyntax.Location, identifierOrLiteralValue);
		}
		if (immutableArray.Length > 0)
		{
			result = immutableArray[0] as ApplicationObjectTypeSymbol;
		}
		return result;
	}

	internal ObjectTypeSymbol GetObjectTypeSymbol(SymbolKind symbolKind, ObjectNameReferenceSyntax identifier, DiagnosticBag diagnostics)
	{
		ObjectTypeSymbol result = null;
		string identifierOrLiteralValue = identifier.Identifier.GetIdentifierOrLiteralValue();
		ImmutableArray<IObjectTypeSymbol> objectTypeSymbolsByNameAcrossModules = Compilation.GetObjectTypeSymbolsByNameAcrossModules(symbolKind, identifierOrLiteralValue);
		if (objectTypeSymbolsByNameAcrossModules.Length > 1)
		{
			diagnostics.Add(ErrorCode.ERR_AmbigReference, identifier.Location, identifierOrLiteralValue);
		}
		if (objectTypeSymbolsByNameAcrossModules.Length > 0)
		{
			result = objectTypeSymbolsByNameAcrossModules[0] as ObjectTypeSymbol;
		}
		return result;
	}

	internal TableTypeSymbol GetTableTypeSymbolFromContainingApplicationObject()
	{
		if (ContainingMember == null)
		{
			return null;
		}
		if (ContainingMember.IsKind(SymbolKind.RequestPage))
		{
			return ContainingMember.GetRelatedTableSymbol();
		}
		return ContainingMember.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>()?.GetRelatedTableSymbol();
	}

	internal TableTypeSymbol GetRelatedTableForProperty(PropertyKind kind, CommaSeparatedPropertyValueSyntax propertyValue)
	{
		if (kind == PropertyKind.RequestFilterFields || kind == PropertyKind.CalcFields)
		{
			return GetRelatedTableForCalcFields(propertyValue);
		}
		return GetTableTypeSymbolFromContainingApplicationObject();
	}

	private TableTypeSymbol GetRelatedTableForCalcFields(CommaSeparatedPropertyValueSyntax propertyValue)
	{
		switch (ContainingMember.Kind)
		{
		case SymbolKind.ReportDataItem:
			return ContainingMember.GetRelatedTableSymbol();
		case SymbolKind.XmlPort:
		case SymbolKind.XmlPortNode:
			return GetRelatedTableForXmlPortNode(propertyValue);
		default:
			return null;
		}
	}

	private TableTypeSymbol GetRelatedTableForXmlPortNode(CommaSeparatedPropertyValueSyntax propertyValue)
	{
		string text = ((XmlPortTableElementSyntax)propertyValue.Parent.Parent.Parent).SourceTable.Identifier.ToString();
		if (int.TryParse(text, out var result))
		{
			return Compilation.GetApplicationObjectTypeSymbolsByIdAcrossModules(SymbolKind.Table, result).FirstOrDefault() as TableTypeSymbol;
		}
		return Compilation.GetApplicationObjectTypeSymbolsByNameAcrossModules(SymbolKind.Table, text.UnquoteIdentifier()).FirstOrDefault() as TableTypeSymbol;
	}

	private BoundPropertyValue BindTextExpressionPropertyValue(TextExpressionPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindExpression(propertyValue.Expression, diagnostics);
		if (boundExpression.Kind == BoundKind.Literal && boundExpression.Type.NavTypeKind == NavTypeKind.Char)
		{
			boundExpression = new BoundConversion(boundExpression.Syntax, boundExpression, ConversionKind.ImplicitText, NavCorLib.StringType, null);
		}
		if (!boundExpression.Type.IsTextType())
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_ExpressionMustBeTextType, propertyValue, propertyValue.Expression, diagnostics);
		}
		return new BoundCodeExpressionPropertyValue(propertyValue, boundExpression, boundExpression.Type, boundExpression.HasErrors);
	}

	private BoundPropertyValue BindIntegerExpressionPropertyValue(IntegerExpressionPropertyValueSyntax propertyValue, PropertyTypeInfo info, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindExpression(propertyValue.Expression, diagnostics);
		if (!boundExpression.Type.IsIntegralType())
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_ExpressionMustBeIntegerType, propertyValue, propertyValue.Expression, diagnostics);
		}
		PropertyKind kind = info.Kind;
		if (kind == PropertyKind.IndentationColumn)
		{
			BoundBadPropertyValue boundBadPropertyValue = ValidateIndentationColumn(propertyValue, diagnostics);
			if (boundBadPropertyValue != null)
			{
				return boundBadPropertyValue;
			}
			return new BoundCodeExpressionPropertyValue(propertyValue, boundExpression, boundExpression.Type, boundExpression.HasErrors);
		}
		throw ExceptionUtilities.UnexpectedValue(propertyValue);
	}

	private BoundBadPropertyValue ValidateIndentationColumn(ExpressionPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		ControlSymbol extendedPageControlOrSelf = ContainingMember.GetExtendedPageControlOrSelf();
		if ((object)extendedPageControlOrSelf == null || extendedPageControlOrSelf.ControlKind != ControlKind.Repeater)
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_PropertyOnlyValidInControlOfType, propertyValue, propertyValue, diagnostics, PropertyKind.IndentationColumn, ControlKind.Repeater);
		}
		return null;
	}

	private BoundPropertyValue BindBooleanExpressionPropertyValue(ExpressionPropertyValueSyntax propertyValue, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindExpression(propertyValue.Expression, diagnostics);
		if (boundExpression.Type.NavTypeKind != NavTypeKind.Boolean)
		{
			return CreateBoundBadPropertyValue(ErrorCode.ERR_ExpressionMustBeBooleanType, propertyValue, propertyValue.Expression, diagnostics);
		}
		return new BoundCodeExpressionPropertyValue(propertyValue, boundExpression, boundExpression.Type, boundExpression.HasErrors);
	}

	private BoundBadPropertyValue CreateBoundBadPropertyValue(ErrorCode errorCode, SyntaxNode ownerPropertySyntax, SyntaxNode nodeWithError, DiagnosticBag diagnostics, params object[] args)
	{
		Error(diagnostics, errorCode, nodeWithError, args);
		return new BoundBadPropertyValue(ownerPropertySyntax, BadExpression(ownerPropertySyntax, LookupResultKind.Empty), null, hasErrors: true);
	}

	private static bool ValidateProperty(string propertyName, PropertyTypeInfo info, PropertySyntax propertySyntax, DiagnosticBag diagnostics)
	{
		if (info == null)
		{
			return false;
		}
		if (IsPropertyModifiedInExtension(propertySyntax))
		{
			VersionChecker.CheckPropertyModificationAvailability(ModificationKind.Extensions, info, propertySyntax, diagnostics);
		}
		if (info.ValidatorFunc != null)
		{
			string propertyValueText = propertySyntax.Value.GetPropertyValueText();
			return info.ValidatorFunc(propertyName, propertyValueText, propertySyntax.Value, diagnostics);
		}
		return true;
	}

	private static bool IsPropertyModifiedInExtension(PropertySyntax propertySyntax)
	{
		SyntaxKind kind = propertySyntax.Parent.Parent.Kind;
		if (!kind.IsApplicationObjectExtensionSyntax() && !kind.IsControlChange())
		{
			return kind.IsFieldModificationChange();
		}
		return true;
	}

	private BoundApplicationObjectReferencePropertyValue GetBoundApplicationObjectToActionReferencePropertyValue(PageActionSyntax pageAction)
	{
		if (pageAction == null || pageAction.Name == null)
		{
			return null;
		}
		string actionName = pageAction.Name.GetIdentifierOrLiteralValue();
		if (string.IsNullOrEmpty(actionName))
		{
			return null;
		}
		ApplicationObjectTypeSymbol applicationObjectTypeSymbol = ContainingMember?.GetContainingSymbolOfType<ApplicationObjectTypeSymbol>();
		if (applicationObjectTypeSymbol == null)
		{
			return null;
		}
		SourceActionSymbol sourceActionSymbol = applicationObjectTypeSymbol.GetCodeMembers().FirstOrDefault((Symbol m) => m.Kind == SymbolKind.Action && string.Compare(actionName, m.Name, StringComparison.OrdinalIgnoreCase) == 0) as SourceActionSymbol;
		if (sourceActionSymbol == null)
		{
			return null;
		}
		PropertySymbol property = sourceActionSymbol.GetProperty(PropertyKind.RunObject);
		if (property == null)
		{
			return null;
		}
		return property.Property?.PropertyValue as BoundApplicationObjectReferencePropertyValue;
	}

	private TableExpressionContext CreateDefaultTableContext(TableTypeSymbol relatedTable)
	{
		if (relatedTable == null)
		{
			return null;
		}
		return TableExpressionContext.CreateWithDefaultBinder(this, relatedTable);
	}

	protected virtual SourceLocalVariableSymbol LookupLocal(SyntaxToken nameToken)
	{
		return Next.LookupLocal(nameToken);
	}

	public BoundBlock BindBlock(BlockSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		return BindBlock(node, diagnostics, binder);
	}

	internal static BoundBlock BindBlock(BlockSyntax node, DiagnosticBag diagnostics, Binder blockBinder)
	{
		DebugAssertHelper.Assert(blockBinder != null);
		ImmutableArray<BoundStatement> statements = BindStatements(node.Statements, diagnostics, blockBinder);
		return new BoundBlock(node, statements);
	}

	internal static ImmutableArray<BoundStatement> BindStatements(SyntaxList<StatementSyntax> syntaxStatements, DiagnosticBag diagnostics, Binder binder)
	{
		DebugAssertHelper.Assert(binder != null);
		if (binder == null)
		{
			return ImmutableArray<BoundStatement>.Empty;
		}
		int count = syntaxStatements.Count;
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance(count);
		for (int i = 0; i < count; i++)
		{
			BoundStatement item = binder.BindStatement(syntaxStatements[i], diagnostics);
			instance.Add(item);
		}
		return instance.ToImmutableAndFree();
	}

	public BoundStatementList BindStatementList(SyntaxList<StatementSyntax> statements, DiagnosticBag diagnostics)
	{
		if (statements.IsEmpty())
		{
			return null;
		}
		return new BoundStatementList(statements[0], BindStatements(statements, diagnostics, this));
	}

	public virtual BoundStatement BindStatement(StatementSyntax node, DiagnosticBag diagnostics)
	{
		GlobalLogger.LogVerbose("Binding Statement: {0}.", node);
		DebugAssertHelper.Assert(node != null);
		BoundStatement boundStatement;
		switch (node.Kind)
		{
		case SyntaxKind.Block:
			boundStatement = BindBlock((BlockSyntax)node, diagnostics);
			break;
		case SyntaxKind.ExpressionStatement:
			boundStatement = BindExpressionStatement((ExpressionStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.AssignmentStatement:
			boundStatement = BindAssignmentStatement((AssignmentStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.CompoundAssignmentStatement:
			boundStatement = BindCompoundAssignmentStatement((CompoundAssignmentStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.IfStatement:
			boundStatement = BindIfStatement((IfStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.OrphanedElseStatement:
			boundStatement = new BoundBadStatement(node, ImmutableArray<BoundNode>.Empty, hasErrors: true);
			break;
		case SyntaxKind.WhileStatement:
			boundStatement = BindWhile((WhileStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.RepeatStatement:
			boundStatement = BindRepeat((RepeatStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.ForStatement:
			boundStatement = BindFor((ForStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.ForEachStatement:
			boundStatement = BindForEach((ForEachStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.BreakStatement:
			boundStatement = BindBreak((BreakStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.EmptyStatement:
			boundStatement = BindEmpty((EmptyStatementSyntax)node);
			break;
		case SyntaxKind.CaseStatement:
			boundStatement = BindCase((CaseStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.ExitStatement:
			boundStatement = BindExit((ExitStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.WithStatement:
			boundStatement = BindWith((WithStatementSyntax)node, diagnostics);
			break;
		case SyntaxKind.AssertErrorStatement:
			boundStatement = BindAssertError((AssertErrorStatementSyntax)node, diagnostics);
			break;
		default:
			DebugAssertHelper.Assert(condition: false, "Unexpected SyntaxKind " + node.Kind);
			boundStatement = new BoundBadStatement(node, ImmutableArray<BoundNode>.Empty, hasErrors: true);
			break;
		}
		DebugAssertHelper.Assert(!boundStatement.WasCompilerGenerated, "Synthetic node would not get cached");
		DebugAssertHelper.Assert(boundStatement.Syntax is StatementSyntax, "BoundStatement should be associated with a statement syntax.");
		DebugAssertHelper.Assert(boundStatement.Syntax.AncestorsAndSelf().Contains(node), "Bound statement (or one of its parents) \r\n                                                                            should have same syntax as the given syntax node. \r\n                                                                            Otherwise it may be confusing to the binder cache that uses syntax node as keys.");
		return boundStatement;
	}

	private BoundStatement BindAssertError(AssertErrorStatementSyntax node, DiagnosticBag diagnostics)
	{
		BoundStatement statement = BindStatement(node.Statement, diagnostics);
		return new BoundAssertErrorStatement(node, statement);
	}

	private BoundStatement BindExit(ExitStatementSyntax node, DiagnosticBag diagnostics)
	{
		ReturnValueSymbol methodReturnValueSymbol = GetMethodReturnValueSymbol();
		BoundExpression boundExpression = null;
		bool hasErrors = false;
		if (node.ExitValue != null)
		{
			boundExpression = BindValue(node.ExitValue, diagnostics, BindValueKind.RValue);
			if (methodReturnValueSymbol == null)
			{
				Error(diagnostics, ErrorCode.ERR_ExitValueNotAllowed, node, GetMethodName());
				hasErrors = true;
			}
			else
			{
				boundExpression = GenerateConversionForAssignment(methodReturnValueSymbol.Type, boundExpression, diagnostics);
			}
		}
		else if (methodReturnValueSymbol != null && methodReturnValueSymbol.IsNamed)
		{
			boundExpression = new BoundReturnValue(node, methodReturnValueSymbol, methodReturnValueSymbol.Type);
		}
		return new BoundExitStatement(node, boundExpression, hasErrors);
	}

	private ReturnValueSymbol GetMethodReturnValueSymbol()
	{
		MethodSymbol obj = ContainingMember as MethodSymbol;
		DebugAssertHelper.Assert(obj != null);
		return obj.ReturnValue;
	}

	private string GetMethodName()
	{
		MethodSymbol obj = ContainingMember as MethodSymbol;
		DebugAssertHelper.Assert(obj != null);
		return obj.Name;
	}

	private BoundStatement BindWith(WithStatementSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		Binder binder = GetBinder(node);
		DebugAssertHelper.Assert(binder != null);
		return binder.BindWithParts(diagnostics, this);
	}

	internal virtual BoundWithStatement BindWithParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindWithParts(diagnostics, originalBinder);
	}

	private BoundStatement BindCase(CaseStatementSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		Binder binder = GetBinder(node);
		DebugAssertHelper.Assert(binder != null);
		BoundCaseStatement boundCaseStatement = binder.BindCaseParts(diagnostics, this);
		CheckBoundCaseStatement(boundCaseStatement, diagnostics);
		return boundCaseStatement;
	}

	private static void CheckBoundCaseStatement(BoundCaseStatement boundStatement, DiagnosticBag diagnostics)
	{
		boundStatement.CaseLines.IterateOverManyDuplicatesBy((BoundCaseLine l) => l.Expressions, (BoundExpression expr) => GetCaseExpressionSyntax(expr).ToString(), (BoundExpression expr, string key) =>
		{
			diagnostics.Add(ErrorCode.ERR_DuplicateCaseStatement, GetCaseExpressionSyntax(expr).GetLocation(), key);
		});
	}

	private static SyntaxNode GetCaseExpressionSyntax(BoundExpression expression)
	{
		if (expression.Syntax.ParentIsKind(SyntaxKind.OptionAccessExpression))
		{
			return expression.Syntax.Parent;
		}
		return expression.Syntax;
	}

	internal virtual BoundCaseStatement BindCaseParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindCaseParts(diagnostics, originalBinder);
	}

	public BoundStatement BindBreak(BreakStatementSyntax node, DiagnosticBag diagnostics)
	{
		GeneratedLabelSymbol breakLabel = BreakLabel;
		if ((object)breakLabel == null)
		{
			Error(diagnostics, ErrorCode.ERR_NoBreak, node);
			return new BoundBadStatement(node, ImmutableArray<BoundNode>.Empty, hasErrors: true);
		}
		return new BoundBreakStatement(node, breakLabel);
	}

	private BoundStatement BindWhile(WhileStatementSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		Binder binder = GetBinder(node);
		DebugAssertHelper.Assert(binder != null);
		return binder.BindWhileParts(diagnostics, binder);
	}

	internal virtual BoundWhileStatement BindWhileParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindWhileParts(diagnostics, originalBinder);
	}

	private BoundStatement BindRepeat(RepeatStatementSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		Binder binder = GetBinder(node);
		DebugAssertHelper.Assert(binder != null);
		return binder.BindRepeatParts(diagnostics, binder);
	}

	internal virtual BoundRepeatStatement BindRepeatParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindRepeatParts(diagnostics, originalBinder);
	}

	private BoundStatement BindFor(ForStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		DebugAssertHelper.Assert(binder != null);
		return binder.BindForParts(diagnostics, binder);
	}

	private BoundStatement BindForEach(ForEachStatementSyntax node, DiagnosticBag diagnostics)
	{
		Binder binder = GetBinder(node);
		DebugAssertHelper.Assert(binder != null);
		return binder.BindForEachParts(diagnostics, binder);
	}

	internal virtual BoundForStatement BindForParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindForParts(diagnostics, originalBinder);
	}

	internal virtual BoundForEachStatement BindForEachParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		return Next.BindForEachParts(diagnostics, originalBinder);
	}

	private BoundStatement BindAssignmentStatement(AssignmentStatementSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		DebugAssertHelper.Assert(node.Source != null);
		DebugAssertHelper.Assert(node.Target != null);
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			BoundExpression boundExpression = BindExpression(node.Target, instance, invoked: false);
			if (boundExpression.Kind == BoundKind.Call && ((BoundCall)boundExpression).Method.ParameterCount == 1)
			{
				BoundExpression expr = BindMethodGroup(node.Target, invoked: true, diagnostics);
				expr = CheckValue(expr, BindValueKind.RValueOrMethodGroup, diagnostics);
				string methodName = ((expr.Kind == BoundKind.MethodGroup) ? GetName(node.Target) : null);
				AnalyzedArguments instance2 = AnalyzedArguments.GetInstance();
				bool hasErrors = BindArgumentAndName(instance2, hadError: false, node.Source);
				BoundExpression expression = BindInvocationExpression(node, node.Target, methodName, expr, isStatement: false, asProperty: true, instance2, diagnostics);
				instance2.PopulateDiagnostics(diagnostics);
				instance2.Free();
				return new BoundExpressionStatement(node, expression, hasErrors);
			}
			diagnostics.AddRange(instance);
			boundExpression = CheckValue(boundExpression, BindValueKind.Assignment, diagnostics);
			BoundExpression source = BindValue(node.Source, diagnostics, BindValueKind.RValue);
			return BindAssignment(node, boundExpression, source, diagnostics);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundAssignmentStatement BindAssignment(StatementSyntax node, BoundExpression target, BoundExpression source, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(target != null);
		DebugAssertHelper.Assert(source != null);
		bool hasErrors = target.HasAnyErrors || source.HasAnyErrors;
		if (target.Type.IsArray())
		{
			Error(diagnostics, ErrorCode.ERR_ArrayNotValidForAssign, target.Syntax.Location);
			hasErrors = true;
		}
		else if (!target.HasAnyErrors)
		{
			source = GenerateConversionForAssignment(target, source, diagnostics);
		}
		return new BoundAssignmentStatement(node, target, source, hasErrors);
	}

	private BoundAssignmentStatement BindCompoundAssignmentStatement(CompoundAssignmentStatementSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		DebugAssertHelper.Assert(node.Source != null);
		DebugAssertHelper.Assert(node.Target != null);
		BoundExpression boundExpression = BindValue(node.Target, diagnostics, BindValueKind.Assignment);
		BoundExpression boundExpression2 = BindValue(node.Source, diagnostics, BindValueKind.RValue);
		if (boundExpression.Type.IsArray())
		{
			Error(diagnostics, ErrorCode.ERR_ArrayNotValidForAssign, node.Target.Location);
			return new BoundAssignmentStatement(node, boundExpression, boundExpression2, hasErrors: true);
		}
		if (boundExpression.HasAnyErrors || boundExpression2.HasAnyErrors)
		{
			return new BoundAssignmentStatement(node, boundExpression, boundExpression2, hasErrors: true);
		}
		BoundExpression left = ApplyFromDotNetConversionIfNeeded(boundExpression);
		boundExpression2 = ApplyFromDotNetConversionIfNeeded(boundExpression2);
		BoundParenthesizedExpression right = new BoundParenthesizedExpression(boundExpression2.Syntax, boundExpression2, boundExpression2.ResultKind, boundExpression2.Type);
		BoundExpression boundExpression3 = BindSimpleBinaryOperator(node, node.AssignmentToken, diagnostics, left, right);
		if (boundExpression3.HasErrors)
		{
			return new BoundAssignmentStatement(node, boundExpression, boundExpression2, boundExpression3.HasErrors);
		}
		return BindAssignment(node, boundExpression, boundExpression3, diagnostics);
	}

	internal BoundExpression GenerateConversionForAssignment(TypeSymbol targetType, BoundExpression expression, DiagnosticBag diagnostics)
	{
		return GenerateConversionForAssignment(null, targetType, expression, diagnostics);
	}

	internal BoundExpression GenerateConversionForAssignment(BoundExpression target, BoundExpression expression, DiagnosticBag diagnostics)
	{
		return GenerateConversionForAssignment(target, target.Type, expression, diagnostics);
	}

	internal BoundExpression GenerateConversionForAssignment(BoundExpression targetExpression, TypeSymbol targetType, BoundExpression expression, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert((object)targetType != null);
		DebugAssertHelper.Assert(expression != null);
		if (expression.HasAnyErrors)
		{
			diagnostics = DiagnosticBag.GetInstance();
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromExpression(expression, targetType, ref useSiteDiagnostics);
		diagnostics.Add(expression.Syntax, useSiteDiagnostics);
		if (!conversion.IsImplicit || !conversion.IsValid)
		{
			GenerateImplicitConversionError(diagnostics, expression.Syntax, conversion, expression, targetType);
			return new BoundConversion(expression.Syntax, expression, conversion, null, targetType, targetExpression, hasErrors: true);
		}
		return CreateConversion(expression.Syntax, expression, conversion, isCast: false, targetType, targetExpression, diagnostics);
	}

	protected void GenerateImplicitConversionError(DiagnosticBag diagnostics, SyntaxNode syntax, Conversion conversion, BoundExpression expression, TypeSymbol targetType)
	{
		DebugAssertHelper.Assert(expression != null);
		DebugAssertHelper.Assert((object)targetType != null);
		if (targetType.TypeCategory != TypeCategoryKind.Error && expression.Kind != BoundKind.BadExpression)
		{
			TypeSymbol type = expression.Type;
			if ((object)type != null)
			{
				GenerateImplicitConversionError(diagnostics, Compilation, syntax, conversion, type, targetType, expression.ConstantValue);
			}
			else
			{
				DebugAssertHelper.Assert(expression.HasAnyErrors, "Missing a case in implicit conversion error reporting");
			}
		}
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "sourceConstantValueOpt")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "compilation", Justification = "by design")]
	protected static void GenerateImplicitConversionError(DiagnosticBag diagnostics, Compilation compilation, SyntaxNode syntax, Conversion conversion, TypeSymbol sourceType, TypeSymbol targetType, ConstantValue sourceConstantValueOpt = null)
	{
		DebugAssertHelper.Assert(!conversion.IsImplicit || !conversion.IsValid);
		if (!sourceType.IsErrorType() && !targetType.IsErrorType())
		{
			Error(diagnostics, ErrorCode.ERR_NoImplicitConversion, syntax, new FormattedSymbol(sourceType, SymbolDisplayFormat.ErrorMessageFormat), new FormattedSymbol(targetType, SymbolDisplayFormat.ErrorMessageFormat));
		}
	}

	public BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax node, DiagnosticBag diagnostics)
	{
		return BindExpressionStatement(node, node.Expression, allowsAnyExpression: false, diagnostics);
	}

	public BoundExpressionStatement BindExpressionStatement(SyntaxNode node, ExpressionSyntax syntax, bool allowsAnyExpression, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(syntax, diagnostics, BindValueKind.RValue, isStatement: true);
		if (!allowsAnyExpression && !SemanticFacts.IsStatementExpression(boundExpression))
		{
			if (!node.HasErrors && !boundExpression.HasErrors)
			{
				Error(diagnostics, ErrorCode.ERR_IllegalStatement, syntax);
			}
			return new BoundExpressionStatement(node, boundExpression, hasErrors: true);
		}
		return new BoundExpressionStatement(node, boundExpression);
	}

	private static BoundStatement BindEmpty(EmptyStatementSyntax node)
	{
		return new BoundNoOpStatement(node);
	}

	private BoundIfStatement BindIfStatement(IfStatementSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression condition = BindBooleanExpression(node.Condition, diagnostics);
		BoundStatement consequence = BindStatement(node.Statement, diagnostics);
		if (node.ElseStatement == null)
		{
			return new BoundIfStatement(node, condition, consequence, null);
		}
		BoundStatement alternativeOpt = BindStatement(node.ElseStatement, diagnostics);
		return new BoundIfStatement(node, condition, consequence, alternativeOpt);
	}

	protected BoundExpression BindBooleanExpression(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		if (node.Kind == SyntaxKind.ParenthesizedExpression)
		{
			node = ((ParenthesizedExpressionSyntax)node).Expression;
		}
		BoundExpression boundExpression = BindValue(node, diagnostics, BindValueKind.RValue);
		TypeSymbol navType = GetNavType(NavTypeKind.Boolean, diagnostics, node);
		if (boundExpression.HasAnyErrors)
		{
			return BoundConversion.Synthesized(node, boundExpression, Conversion.NoConversion, null, navType, hasErrors: true);
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = Conversions.ClassifyConversionFromExpression(boundExpression, navType, ref useSiteDiagnostics);
		diagnostics.Add(boundExpression.Syntax, useSiteDiagnostics);
		if (conversion.IsImplicit)
		{
			if (conversion.Kind != ConversionKind.Identity)
			{
				return CreateConversion(boundExpression.Syntax, boundExpression, conversion, isCast: false, wasCompilerGenerated: true, navType, null, diagnostics);
			}
			return boundExpression;
		}
		GenerateImplicitConversionError(diagnostics, node, conversion, boundExpression, navType);
		return BoundConversion.Synthesized(node, boundExpression, Conversion.NoConversion, null, navType, hasErrors: true);
	}

	private BoundExpression CheckValue(BoundExpression expr, BindValueKind valueKind, DiagnosticBag diagnostics)
	{
		if (!CheckCompoundValue(expr, diagnostics))
		{
			return ToBadExpression(expr, LookupResultKind.NotAValue);
		}
		if (CheckValueKind(expr, valueKind, diagnostics) || (expr.HasAnyErrors && valueKind == BindValueKind.RValueOrMethodGroup))
		{
			return expr;
		}
		LookupResultKind resultKind = ((valueKind == BindValueKind.RValue || valueKind == BindValueKind.RValueOrMethodGroup) ? LookupResultKind.NotAValue : LookupResultKind.NotAVariable);
		return ToBadExpression(expr, resultKind);
	}

	private static bool CheckCompoundValue(BoundExpression expr, DiagnosticBag diagnostics)
	{
		if ((expr.Kind == BoundKind.SystemOptionType || expr.Kind == BoundKind.EnumType) && expr.ConstantValue == null)
		{
			diagnostics.Add(ErrorCode.ERR_ExpectedOptionValue, expr.Syntax.Location);
			return false;
		}
		return true;
	}

	internal static bool CheckValueKind(BoundExpression expr, BindValueKind valueKind)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			return CheckValueKind(expr, valueKind, instance);
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool CheckValueKind(BoundExpression expr, BindValueKind valueKind, DiagnosticBag diagnostics)
	{
		if (expr.HasAnyErrors)
		{
			return false;
		}
		if (RequiresSettingValue(valueKind) && !CheckIsVariable(expr.Syntax, expr, valueKind, checkingReceiver: false, diagnostics))
		{
			return false;
		}
		return true;
	}

	private static bool CheckIsVariable(SyntaxNode node, BoundExpression expr, BindValueKind kind, bool checkingReceiver, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(expr != null);
		if (expr.HasAnyErrors)
		{
			return false;
		}
		if (expr.ConstantValue != null || expr.Type.GetNavTypeKindSafe() == NavTypeKind.None)
		{
			Error(diagnostics, GetStandardLvalueError(kind), node);
			return false;
		}
		if (expr is BoundParameter)
		{
			return true;
		}
		if (expr is BoundReturnValue)
		{
			return true;
		}
		if (expr is BoundLocal boundLocal)
		{
			return CheckVariable(node, boundLocal.LocalSymbol, kind, diagnostics);
		}
		if (expr is BoundGlobal boundGlobal)
		{
			return CheckVariable(node, boundGlobal.GlobalSymbol, kind, diagnostics);
		}
		if (expr is BoundFieldAccess)
		{
			return true;
		}
		if (expr is BoundReportDataItemAccess)
		{
			return true;
		}
		if (expr is BoundXmlPortNodeAccess)
		{
			XmlPortNodeSymbol xmlPortNodeSymbol = expr.ExpressionSymbol as XmlPortNodeSymbol;
			if ((object)xmlPortNodeSymbol == null || xmlPortNodeSymbol.SourceTypeKind != XmlPortSourceTypeKind.Table)
			{
				if ((object)xmlPortNodeSymbol == null)
				{
					return false;
				}
				return xmlPortNodeSymbol.SourceTypeKind == XmlPortSourceTypeKind.Text;
			}
			return true;
		}
		if (expr is BoundArrayAccess)
		{
			return true;
		}
		if (expr is BoundTextIndexAccess boundTextIndexAccess)
		{
			return CheckIsVariable(node, boundTextIndexAccess.Expression, kind, checkingReceiver, diagnostics);
		}
		Error(diagnostics, GetStandardLvalueError(kind), node);
		return false;
	}

	private static bool CheckVariable(SyntaxNode tree, VariableSymbol variable, BindValueKind kind, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert((object)variable != null);
		DebugAssertHelper.Assert(kind != BindValueKind.RValue);
		if (variable.IsWritable)
		{
			return true;
		}
		Error(diagnostics, GetStandardLvalueError(kind), tree);
		return false;
	}

	private static bool RequiresSettingValue(BindValueKind kind)
	{
		switch (kind)
		{
		case BindValueKind.RValue:
		case BindValueKind.RValueOrMethodGroup:
			return false;
		case BindValueKind.Assignment:
		case BindValueKind.CompoundAssignment:
		case BindValueKind.OutParameter:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private static ErrorCode GetStandardLvalueError(BindValueKind kind)
	{
		switch (kind)
		{
		case BindValueKind.Assignment:
		case BindValueKind.CompoundAssignment:
			return ErrorCode.ERR_AssgLvalueExpected;
		case BindValueKind.OutParameter:
			return ErrorCode.ERR_RefLvalueExpected;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal TypeSymbol BindType(Symbol ownerSymbol, DataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(diagnostics != null);
		TypeSymbol typeSymbol = null;
		switch (syntax.Kind)
		{
		case SyntaxKind.LengthDataType:
			typeSymbol = BindTextOrCodeType((LengthDataTypeSyntax)syntax);
			break;
		case SyntaxKind.SubtypedDataType:
			typeSymbol = BindSubTypedDataType(ownerSymbol, (SubtypedDataTypeSyntax)syntax, diagnostics);
			break;
		case SyntaxKind.OptionDataType:
			typeSymbol = BindOptionDataType(ownerSymbol, (OptionDataTypeSyntax)syntax, diagnostics);
			break;
		case SyntaxKind.EnumDataType:
			typeSymbol = BindEnumDataType(ownerSymbol, (EnumDataTypeSyntax)syntax, diagnostics);
			break;
		case SyntaxKind.TextConstDataType:
			typeSymbol = BindTextConstDataType(ownerSymbol, (TextConstDataTypeSyntax)syntax, diagnostics);
			break;
		case SyntaxKind.LabelDataType:
			typeSymbol = BindLabelDataType(ownerSymbol, (LabelDataTypeSyntax)syntax, diagnostics);
			break;
		case SyntaxKind.GenericDataType:
			typeSymbol = BindGenericDataType(ownerSymbol, (GenericNamedDataTypeSyntax)syntax, diagnostics);
			break;
		default:
		{
			NavTypeKind navTypeKind = NavTypeExtensions.GetNavTypeKind(syntax.TypeName.ValueText, throwIfUnknown: false);
			if (navTypeKind != NavTypeKind.None)
			{
				typeSymbol = Compilation.GetNavTypeSymbol(navTypeKind);
			}
			break;
		}
		}
		if (typeSymbol == null)
		{
			NavDiagnosticInfo errorInfo = null;
			if (!syntax.ContainsDiagnostics)
			{
				errorInfo = diagnostics.Add(ErrorCode.ERR_UnknownType, syntax.Location, syntax.TypeName.ValueText);
			}
			typeSymbol = new ExtendedErrorTypeSymbol(Compilation, string.Empty, errorInfo);
		}
		if (!typeSymbol.CanCompileWithTarget(Compilation.Options.Target))
		{
			diagnostics.Add(ErrorCode.ERR_SymbolCannotBeUsedInThisContext, syntax.Location, typeSymbol.Name, Compilation.Options.Target.ToString());
		}
		if (syntax.Kind != SyntaxKind.SubtypedDataType)
		{
			ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, typeSymbol, syntax);
		}
		return typeSymbol;
	}

	private TypeSymbol BindGenericDataType(Symbol ownerSymbol, GenericNamedDataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		TypeSymbol[] array = new TypeSymbol[syntax.TypeArguments.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = BindType(ownerSymbol, syntax.TypeArguments[i], diagnostics);
		}
		NavTypeKind navTypeKind = NavTypeExtensions.GetNavTypeKind(syntax.TypeName.ValueText, throwIfUnknown: false);
		if (navTypeKind == NavTypeKind.None)
		{
			return null;
		}
		if (!navTypeKind.IsGenericType())
		{
			diagnostics.Add(ErrorCode.ERR_TypeIsNotGeneric, syntax.TypeName.GetLocation(), syntax.TypeName.ValueText);
			return null;
		}
		IGenericLanguageClassTypeSymbolFactory genericNavTypeSymbolFactory = Compilation.GetGenericNavTypeSymbolFactory(navTypeKind);
		DebugAssertHelper.Assert(genericNavTypeSymbolFactory != null, "Update the generic type factory.");
		if (genericNavTypeSymbolFactory.NumberOfTemplateArguments != array.Length)
		{
			diagnostics.Add(ErrorCode.ERR_GenericTypeArgumentCountMismatch, syntax.Location, syntax.TypeName.ValueText, genericNavTypeSymbolFactory.NumberOfTemplateArguments);
			return null;
		}
		if (genericNavTypeSymbolFactory.RequiresTemplateArgumentValidation)
		{
			ImmutableArray<bool> immutableArray = genericNavTypeSymbolFactory.ValidateTemplateArguments(array);
			bool flag = true;
			for (int j = 0; j < immutableArray.Length; j++)
			{
				flag = flag && immutableArray[j];
				if (!immutableArray[j])
				{
					diagnostics.Add(ErrorCode.ERR_InvalidTypeArgument, syntax.TypeArguments[j].Location, syntax.TypeArguments[j].TypeName);
				}
			}
			if (!flag)
			{
				return null;
			}
		}
		return genericNavTypeSymbolFactory.Create(array);
	}

	private static TypeSymbol BindOptionDataType(Symbol ownerSymbol, OptionDataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		if (syntax.OptionValues == null || syntax.OptionValues.Kind != SyntaxKind.OptionValues)
		{
			return Compilation.GetNavTypeSymbol(NavTypeKind.Option);
		}
		return SourceOptionTypeSymbol.Create(ownerSymbol, syntax, diagnostics);
	}

	private TypeSymbol BindEnumDataType(Symbol ownerSymbol, EnumDataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = Compilation.GetObjectSymbolsByNameAcrossModules(SymbolKind.Enum, syntax.EnumTypeName.GetIdentifierOrLiteralValue());
		if (objectSymbolsByNameAcrossModules.Length > 1)
		{
			diagnostics.Add(ErrorCode.ERR_AmbigReference, syntax.Location, syntax.EnumTypeName);
		}
		if (objectSymbolsByNameAcrossModules.Length == 0)
		{
			NavDiagnosticInfo errorInfo = diagnostics.Add(ErrorCode.ERR_MissingApplicationObject, syntax.Location, SymbolKind.Enum, syntax.EnumTypeName);
			return new ExtendedErrorTypeSymbol(Compilation, string.Empty, errorInfo, variableUsedBeforeDeclaration: false, NavTypeKind.Enum);
		}
		return objectSymbolsByNameAcrossModules[0] as TypeSymbol;
	}

	private static TypeSymbol BindTextConstDataType(Symbol ownerSymbol, TextConstDataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		return SourceTextConstTypeSymbol.Create(ownerSymbol, syntax, diagnostics);
	}

	private static TypeSymbol BindLabelDataType(Symbol ownerSymbol, LabelDataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		return SourceLabelTypeSymbol.Create(ownerSymbol, syntax, diagnostics);
	}

	private TypeSymbol BindSubTypedDataType(Symbol ownerSymbol, SubtypedDataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		NavTypeKind navTypeKind = NavTypeExtensions.GetNavTypeKind(syntax.TypeName.ValueText);
		SymbolKind kind = SymbolKind.Codeunit;
		switch (navTypeKind)
		{
		case NavTypeKind.TestPage:
		case NavTypeKind.Page:
			kind = SymbolKind.Page;
			break;
		case NavTypeKind.Record:
			kind = SymbolKind.Table;
			break;
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.Report:
			kind = SymbolKind.Report;
			break;
		case NavTypeKind.Query:
			kind = SymbolKind.Query;
			break;
		case NavTypeKind.XmlPort:
			kind = SymbolKind.XmlPort;
			break;
		case NavTypeKind.ControlAddIn:
			kind = SymbolKind.ControlAddIn;
			break;
		case NavTypeKind.DotNet:
			kind = SymbolKind.DotNet;
			break;
		default:
			return null;
		case NavTypeKind.Codeunit:
			break;
		}
		var (text, typeSymbol) = GetSubtypedDataType(syntax, kind, diagnostics);
		if (typeSymbol != null)
		{
			switch (navTypeKind)
			{
			case NavTypeKind.Page:
				typeSymbol = ownerSymbol.ContainingModule.GetPageReference((PageTypeSymbol)typeSymbol);
				break;
			case NavTypeKind.TestPage:
				typeSymbol = ownerSymbol.ContainingModule.GetTestPage((PageTypeSymbol)typeSymbol);
				break;
			case NavTypeKind.TestRequestPage:
			{
				TestRequestPageTypeSymbol testRequestPage = ownerSymbol.ContainingModule.GetTestRequestPage(((ReportTypeSymbol)typeSymbol).RequestPage);
				if (testRequestPage == null)
				{
					NavDiagnosticInfo errorInfo = diagnostics.Add(ErrorCode.ERR_NoRequestPage, syntax.Location, typeSymbol.Name);
					typeSymbol = new ExtendedErrorTypeSymbol(Compilation, string.Empty, errorInfo);
				}
				else
				{
					typeSymbol = testRequestPage;
				}
				break;
			}
			}
			ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, typeSymbol, syntax.Subtype);
		}
		else
		{
			string text2 = ((navTypeKind == NavTypeKind.DotNet) ? navTypeKind.ToString() : kind.ToString());
			NavDiagnosticInfo errorInfo2 = diagnostics.Add(ErrorCode.ERR_MissingApplicationObject, syntax.Location, text2, text);
			typeSymbol = new ExtendedErrorTypeSymbol(Compilation, string.Empty, errorInfo2, variableUsedBeforeDeclaration: false, navTypeKind);
		}
		return typeSymbol;
	}

	private (string Identifier, TypeSymbol Type) GetSubtypedDataType(SubtypedDataTypeSyntax syntax, SymbolKind kind, DiagnosticBag diagnostics)
	{
		var (text, source) = GetApplicationObjectTypeSymbols(syntax.Subtype, kind);
		if (source.HasMoreThan(1))
		{
			diagnostics.Add(ErrorCode.ERR_AmbigReference, syntax.Location, text);
		}
		return (Identifier: text, Type: source.FirstOrDefault() as TypeSymbol);
	}

	private (string Identifier, IEnumerable<ISymbol> SymbolsForIdentifier) GetApplicationObjectTypeSymbols(ObjectNameOrIdSyntax objectNameOrId, SymbolKind kind)
	{
		string text = objectNameOrId.Identifier.ToString();
		IEnumerable<ISymbol> item;
		switch (objectNameOrId.Identifier.Kind)
		{
		case SyntaxKind.IdentifierName:
			text = ((IdentifierNameSyntax)objectNameOrId.Identifier).GetIdentifierOrLiteralValue() ?? string.Empty;
			item = Compilation.GetObjectSymbolsByNameAcrossModules(kind, text);
			break;
		case SyntaxKind.ObjectId:
		{
			int id = ((int?)((ObjectIdSyntax)objectNameOrId.Identifier).Value.Value) ?? 0;
			item = Compilation.GetObjectSymbolsByIdAcrossModules(kind, id).Cast<ISymbol>();
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(objectNameOrId.Identifier.Kind);
		}
		return (Identifier: text, SymbolsForIdentifier: item);
	}

	private static TypeSymbol BindTextOrCodeType(LengthDataTypeSyntax syntax)
	{
		switch (NavTypeExtensions.GetNavTypeKind(syntax.TypeName.ValueText))
		{
		case NavTypeKind.Text:
			if (syntax.Length.Value == null)
			{
				return TextTypeSymbol.GetInstance(NavTypeKind.Text);
			}
			return TextTypeSymbol.GetInstance(NavTypeKind.Text, (int)syntax.Length.Value);
		case NavTypeKind.Code:
			if (syntax.Length.Value == null)
			{
				return CodeTypeSymbol.GetInstance(NavTypeKind.Code);
			}
			return CodeTypeSymbol.GetInstance(NavTypeKind.Code, (int)syntax.Length.Value);
		default:
			throw ExceptionUtilities.UnexpectedValue(syntax.TypeName.ValueText);
		}
	}

	internal TypeSymbol BindRecordType(Symbol ownerSymbol, RecordTypeReferenceSyntax syntax, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = BindType(ownerSymbol, syntax.DataType, diagnostics);
		if (typeSymbol.IsErrorType())
		{
			return typeSymbol;
		}
		TableTypeSymbol table = (TableTypeSymbol)typeSymbol;
		return ownerSymbol.ContainingModule.GetRecord(table, syntax.Temporary.Kind == SyntaxKind.TemporaryKeyword);
	}

	internal TypeSymbol BindReturnValueType(Symbol ownerSymbol, DataTypeSyntax syntax, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = BindType(ownerSymbol, syntax, diagnostics);
		if (!typeSymbol.IsErrorType() && !typeSymbol.IsValidReturnType())
		{
			Error(diagnostics, ErrorCode.ERR_InvalidReturnType, syntax.TypeName, syntax.TypeName);
		}
		else
		{
			ValidateTypeLength(syntax, typeSymbol, diagnostics);
		}
		return typeSymbol;
	}

	internal TypeSymbol BindVariableType(Symbol ownerSymbol, TypeReferenceBaseSyntax syntax, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = BindVariableOrParameterType(ownerSymbol, syntax, diagnostics);
		if (!typeSymbol.IsErrorType() && !typeSymbol.IsValidVariableType())
		{
			Error(diagnostics, ErrorCode.ERR_InvalidVariableType, syntax.DataType.TypeName, typeSymbol.Name);
		}
		else
		{
			ValidateTypeLength(syntax.DataType, typeSymbol, diagnostics);
		}
		return typeSymbol;
	}

	private static void ValidateTypeLength(DataTypeSyntax syntax, TypeSymbol type, DiagnosticBag diagnostics)
	{
		if (type.NavTypeKind == NavTypeKind.Code)
		{
			int num = int.MaxValue;
			if (syntax.SyntaxTree.Options.RuntimeVersion >= RuntimeVersion.Spring2019)
			{
				num = 2048;
			}
			if (type.Length > num)
			{
				Error(diagnostics, ErrorCode.ERR_TypeLengthTooBig, syntax.TypeName, type.Name, num);
			}
		}
	}

	internal TypeSymbol BindParameterType(Symbol ownerSymbol, TypeReferenceBaseSyntax syntax, bool isVar, DiagnosticBag diagnostics)
	{
		TypeSymbol typeSymbol = BindVariableOrParameterType(ownerSymbol, syntax, diagnostics);
		if (!typeSymbol.IsErrorType() && !typeSymbol.IsValidParameterType(isVar))
		{
			Error(diagnostics, ErrorCode.ERR_InvalidParameterType, syntax.DataType.TypeName, typeSymbol.Name);
		}
		else
		{
			ValidateTypeLength(syntax.DataType, typeSymbol, diagnostics);
		}
		return typeSymbol;
	}

	internal TypeSymbol BindVariableOrParameterType(Symbol ownerSymbol, TypeReferenceBaseSyntax syntax, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(diagnostics != null);
		SyntaxKind kind = syntax.Kind;
		TypeSymbol typeSymbol = ((kind != SyntaxKind.RecordTypeReference) ? BindType(ownerSymbol, syntax.DataType, diagnostics) : BindRecordType(ownerSymbol, (RecordTypeReferenceSyntax)syntax, diagnostics));
		if (syntax.Array != null)
		{
			ImmutableArray<int> dimensions = syntax.Array.DimensionList.Dimensions.Select((DimensionSyntax d) => d.Value.Value).Cast<int>().ToImmutableArrayOrEmpty();
			bool flag = false;
			for (int num = 0; num < dimensions.Length; num++)
			{
				if (dimensions[num] <= 0)
				{
					Error(diagnostics, ErrorCode.ERR_ArrayDimensionMustBePositiveNumber, syntax.Array.DimensionList.Dimensions[num]);
					flag = true;
				}
			}
			if (!flag)
			{
				CheckForTooManyElements(syntax, diagnostics, dimensions);
			}
			typeSymbol = ArrayTypeSymbol.Create(typeSymbol, dimensions);
		}
		return typeSymbol;
	}

	private static void CheckForTooManyElements(TypeReferenceBaseSyntax syntax, DiagnosticBag diagnostics, ImmutableArray<int> dimensions)
	{
		decimal num = 1m;
		foreach (int item in dimensions)
		{
			num *= (decimal)item;
			if (num > 1000000m)
			{
				Error(diagnostics, ErrorCode.ERR_ArrayIsTooBig, syntax.Array.DimensionList.Location, 1000000, num);
				break;
			}
		}
	}

	internal TypeSymbol BindFieldType(Symbol ownerSymbol, FieldSyntax syntax, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(diagnostics != null);
		TypeSymbol typeSymbol = BindType(ownerSymbol, syntax.Type, diagnostics);
		if (!typeSymbol.IsErrorType() && !typeSymbol.IsValidFieldType())
		{
			Error(diagnostics, ErrorCode.ERR_InvalidFieldType, syntax.Type, typeSymbol.NavTypeKind.ToString());
		}
		switch (typeSymbol.NavTypeKind)
		{
		case NavTypeKind.Option:
		{
			OptionValuesPropertyValueSyntax optionValuesPropertyValueSyntax = syntax.GetPropertyValue("OptionMembers") as OptionValuesPropertyValueSyntax;
			if (optionValuesPropertyValueSyntax != null && optionValuesPropertyValueSyntax.Value.Options.Any((IdentifierNameOrEmptySyntax o) => o.IdentifierName != null))
			{
				return SourceOptionTypeSymbol.Create(ownerSymbol, optionValuesPropertyValueSyntax.Value, diagnostics);
			}
			Error(diagnostics, ErrorCode.ERR_PropertyCannotBeBlank, optionValuesPropertyValueSyntax?.Parent.Location ?? syntax.Location, "OptionMembers");
			break;
		}
		case NavTypeKind.Text:
		{
			TextTypeSymbol textTypeSymbol = typeSymbol as TextTypeSymbol;
			int num2 = 250;
			if (syntax.SyntaxTree.Options.RuntimeVersion >= RuntimeVersion.Spring2019)
			{
				num2 = 2048;
			}
			DebugAssertHelper.Assert(textTypeSymbol != null);
			if (textTypeSymbol.Length > num2)
			{
				Error(diagnostics, ErrorCode.ERR_FieldLengthTooBig, syntax.Type, textTypeSymbol.Name, num2);
			}
			break;
		}
		case NavTypeKind.Code:
		{
			CodeTypeSymbol codeTypeSymbol = typeSymbol as CodeTypeSymbol;
			int num = 250;
			if (syntax.SyntaxTree.Options.RuntimeVersion >= RuntimeVersion.Spring2019)
			{
				num = 2048;
			}
			DebugAssertHelper.Assert(codeTypeSymbol != null);
			if (codeTypeSymbol.Length > num)
			{
				Error(diagnostics, ErrorCode.ERR_FieldLengthTooBig, syntax.Type, codeTypeSymbol.Name, num);
			}
			break;
		}
		}
		return typeSymbol;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "diagnostics")]
	private static BoundMethodOrPropertyGroup ConstructBoundMemberGroup(SyntaxNode syntax, BoundExpression receiver, string plainName, ArrayBuilder<Symbol> members, LookupResult lookupResult, bool hasErrors, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(members.Count > 0);
		SymbolKind kind = members[0].Kind;
		if (kind == SymbolKind.Method)
		{
			return new BoundMethodGroup(syntax, receiver, plainName, members.SelectAsArray(ToMethodSymbolFunc), lookupResult, hasErrors);
		}
		throw ExceptionUtilities.UnexpectedValue(members[0].Kind);
	}

	internal static TypeSymbol GetNavType(NavTypeKind typeId, DiagnosticBag diagnostics, SyntaxNode node)
	{
		TypeSymbol navTypeSymbol = Compilation.GetNavTypeSymbol(typeId);
		DebugAssertHelper.Assert((object)navTypeSymbol != null, "Expect an error type if special type isn't found");
		ReportUseSiteDiagnostics(navTypeSymbol, diagnostics, node);
		return navTypeSymbol;
	}

	internal static bool ReportUseSiteDiagnostics(Symbol symbol, DiagnosticBag diagnostics, SyntaxNode node)
	{
		DiagnosticInfo useSiteDiagnostic = symbol.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			return Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, node.Location);
		}
		return false;
	}

	internal static bool ReportUseSiteDiagnostics(Symbol symbol, DiagnosticBag diagnostics, Location location)
	{
		DiagnosticInfo useSiteDiagnostic = symbol.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			return Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, location);
		}
		return false;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "options")]
	internal Symbol ResultSymbol(LookupResult result, string simpleName, SyntaxNode where, DiagnosticBag diagnostics, bool suppressUseSiteDiagnostics, out bool wasError, LookupOptions options = LookupOptions.Default)
	{
		DebugAssertHelper.Assert(where != null);
		DebugAssertHelper.Assert(diagnostics != null);
		ArrayBuilder<Symbol> symbols = result.Symbols;
		wasError = false;
		if (result.IsMultiViable && symbols.Count <= 1)
		{
			Symbol symbol = symbols[0];
			if (!suppressUseSiteDiagnostics)
			{
				wasError = ReportUseSiteDiagnostics(symbol, diagnostics, where);
			}
			else if (symbol.Kind == SymbolKind.ErrorType)
			{
				throw ExceptionUtilities.SkippedForNow;
			}
			return symbol;
		}
		wasError = true;
		if (result.Kind == LookupResultKind.Empty)
		{
			NavDiagnosticInfo errorInfoParameter = NotFound(where, simpleName, diagnostics);
			return new ExtendedErrorTypeSymbol(Compilation.CompiledModule, simpleName, errorInfoParameter);
		}
		DebugAssertHelper.Assert(symbols.Count > 0);
		if (!suppressUseSiteDiagnostics)
		{
			for (int i = 0; i < symbols.Count; i++)
			{
				ReportUseSiteDiagnostics(symbols[i], diagnostics, where);
			}
		}
		if (result.Error != null)
		{
			diagnostics.Add(new NavDiagnostic(result.Error, where.Location));
		}
		if (symbols.Count > 1)
		{
			return new ExtendedErrorTypeSymbol(symbols[0].GetContainingSymbolOfType<ObjectTypeSymbol>(), symbols.ToImmutable(), LookupResultKind.Ambiguous, result.Error);
		}
		return symbols[0];
	}

	private static NavDiagnosticInfo NotFound(SyntaxNode where, string whereText, DiagnosticBag diagnostics)
	{
		Location location = where.Location;
		return diagnostics.Add(ErrorCode.ERR_IdentifierExpected, location, whereText);
	}

	public BoundExpression BindExpression(ExpressionSyntax node, DiagnosticBag diagnostics)
	{
		return BindExpression(node, diagnostics, invoked: false);
	}

	protected BoundExpression BindExpression(ExpressionSyntax node, DiagnosticBag diagnostics, bool invoked, bool isStatement = false)
	{
		return BindExpressionInternal(node, diagnostics, invoked, isStatement);
	}

	[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
	private BoundExpression BindExpressionInternal(ExpressionSyntax node, DiagnosticBag diagnostics, bool invoked, bool isStatement = false)
	{
		DebugAssertHelper.Assert(node != null);
		switch (node.Kind)
		{
		case SyntaxKind.InvocationExpression:
			return BindInvocationExpression((InvocationExpressionSyntax)node, diagnostics, isStatement);
		case SyntaxKind.IdentifierName:
			return BindIdentifier((IdentifierNameSyntax)node, invoked, diagnostics);
		case SyntaxKind.MemberAccessExpression:
			return BindMemberAccess((MemberAccessExpressionSyntax)node, invoked, isStatement, diagnostics);
		case SyntaxKind.LiteralExpression:
			return BindLiteralConstant((LiteralExpressionSyntax)node, diagnostics);
		case SyntaxKind.AddExpression:
		case SyntaxKind.SubtractExpression:
		case SyntaxKind.MultiplyExpression:
		case SyntaxKind.DivideExpression:
		case SyntaxKind.IntegerDivideExpression:
		case SyntaxKind.ModuloExpression:
		case SyntaxKind.EqualsExpression:
		case SyntaxKind.NotEqualsExpression:
		case SyntaxKind.GreaterThanExpression:
		case SyntaxKind.GreaterThanOrEqualExpression:
		case SyntaxKind.LessThanExpression:
		case SyntaxKind.LessThanOrEqualExpression:
		case SyntaxKind.RangeExpression:
			return BindSimpleBinaryOperator((BinaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.ArrayIndexExpression:
			return BindElementAccess((ElementAccessExpressionSyntax)node, diagnostics);
		case SyntaxKind.OptionAccessExpression:
			return BindOptionAccess((OptionAccessExpressionSyntax)node, diagnostics);
		case SyntaxKind.InListExpression:
			return BindInList((InListExpressionSyntax)node, diagnostics);
		case SyntaxKind.LogicalOrExpression:
		case SyntaxKind.LogicalAndExpression:
		case SyntaxKind.LogicalXorExpression:
			return BindConditionalLogicalOperator((BinaryExpressionSyntax)node, diagnostics);
		case SyntaxKind.ParenthesizedExpression:
			return BindParenthesizedExpression((ParenthesizedExpressionSyntax)node, diagnostics);
		case SyntaxKind.UnaryPlusExpression:
		case SyntaxKind.UnaryMinusExpression:
		case SyntaxKind.UnaryNotExpression:
			return BindUnaryOperator((UnaryExpressionSyntax)node, diagnostics);
		default:
			DebugAssertHelper.Assert(condition: false, "Unexpected SyntaxKind " + node.Kind);
			return BadExpression(node);
		}
	}

	private BoundExpression BindUnaryOperator(UnaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindValue(node.Expression, diagnostics, BindValueKind.RValue);
		boundExpression = ApplyFromDotNetConversionIfNeeded(boundExpression);
		return BindUnaryOperatorCore(node, node.OperatorToken.Text, boundExpression, diagnostics);
	}

	private BoundExpression BindUnaryOperatorCore(SyntaxNode node, string operatorText, BoundExpression operand, DiagnosticBag diagnostics)
	{
		UnaryOperatorKind unaryOperatorKind = SyntaxKindToUnaryOperatorKind(node.Kind);
		if (operand.HasAnyErrors)
		{
			return new BoundUnaryOperator(node, unaryOperatorKind, operand, null, null, LookupResultKind.Empty, operand.Type, hasErrors: true);
		}
		UnaryOperatorAnalysisResult unaryOperatorAnalysisResult = UnaryOperatorOverloadResolution(unaryOperatorKind, operand, node, diagnostics, out var resultKind, out var _);
		if (!unaryOperatorAnalysisResult.HasValue)
		{
			ReportUnaryOperatorError(node, diagnostics, operatorText, operand, resultKind);
			return new BoundUnaryOperator(node, unaryOperatorKind, operand, null, null, resultKind, operand.Type, hasErrors: true);
		}
		UnaryOperatorSignature signature = unaryOperatorAnalysisResult.Signature;
		BoundExpression operand2 = CreateConversion(operand.Syntax, operand, unaryOperatorAnalysisResult.Conversion, isCast: false, signature.OperandType, null, diagnostics);
		TypeSymbol returnType = signature.ReturnType;
		UnaryOperatorKind kind = signature.Kind;
		MethodSymbol method = signature.Method;
		ConstantValue constantValueOpt = FoldUnaryOperator(node, kind, operand2, returnType.NavTypeKind, diagnostics);
		return new BoundUnaryOperator(node, kind, operand2, constantValueOpt, method, resultKind, returnType);
	}

	private static ConstantValue FoldUnaryOperator(SyntaxNode syntax, UnaryOperatorKind kind, BoundExpression operand, NavTypeKind resultType, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(operand != null);
		if (operand.HasAnyErrors)
		{
			return null;
		}
		ConstantValue constantValue = operand.ConstantValue;
		if (constantValue == null || constantValue.IsBad)
		{
			return constantValue;
		}
		object obj = FoldNeverOverflowUnaryOperator(kind, constantValue);
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		try
		{
			obj = FoldCheckedIntegralUnaryOperator(kind, constantValue);
		}
		catch (OverflowException)
		{
			Error(diagnostics, ErrorCode.ERR_CheckedOverflow, syntax);
			return ConstantValue.Bad;
		}
		if (obj != null)
		{
			return ConstantValue.Create(obj, resultType);
		}
		return null;
	}

	private static object FoldNeverOverflowUnaryOperator(UnaryOperatorKind kind, ConstantValue value)
	{
		switch (kind)
		{
		case UnaryOperatorKind.DecimalUnaryMinus:
			return -value.DecimalValue;
		case UnaryOperatorKind.FloatUnaryMinus:
		case UnaryOperatorKind.DoubleUnaryMinus:
			return 0.0 - value.DoubleValue;
		case UnaryOperatorKind.DecimalUnaryPlus:
			return value.DecimalValue;
		case UnaryOperatorKind.FloatUnaryPlus:
		case UnaryOperatorKind.DoubleUnaryPlus:
			return value.DoubleValue;
		case UnaryOperatorKind.LongUnaryPlus:
			return value.Int64Value;
		case UnaryOperatorKind.ULongUnaryPlus:
			return value.UInt64Value;
		case UnaryOperatorKind.IntUnaryPlus:
			return value.Int32Value;
		case UnaryOperatorKind.UIntUnaryPlus:
			return value.UInt32Value;
		case UnaryOperatorKind.BoolLogicalNegation:
			return !value.BooleanValue;
		default:
			return null;
		}
	}

	private static object FoldCheckedIntegralUnaryOperator(UnaryOperatorKind kind, ConstantValue value)
	{
		return checked(kind switch
		{
			UnaryOperatorKind.LongUnaryMinus => (object)(-value.Int64Value), 
			UnaryOperatorKind.IntUnaryMinus => -value.Int32Value, 
			_ => null, 
		});
	}

	private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundExpression = BindExpression(node.Expression, diagnostics);
		return new BoundParenthesizedExpression(node, boundExpression, boundExpression.ResultKind, boundExpression.Type);
	}

	private BoundExpression BindConditionalLogicalOperator(BinaryExpressionSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node.Kind == SyntaxKind.LogicalOrExpression || node.Kind == SyntaxKind.LogicalAndExpression || node.Kind == SyntaxKind.LogicalXorExpression);
		BinaryExpressionSyntax binaryExpressionSyntax = node;
		ExpressionSyntax expressionSyntax;
		while (true)
		{
			expressionSyntax = binaryExpressionSyntax.Left;
			if (!(expressionSyntax is BinaryExpressionSyntax binaryExpressionSyntax2) || (binaryExpressionSyntax2.Kind != SyntaxKind.LogicalOrExpression && binaryExpressionSyntax2.Kind != SyntaxKind.LogicalAndExpression && binaryExpressionSyntax2.Kind != SyntaxKind.LogicalXorExpression))
			{
				break;
			}
			binaryExpressionSyntax = binaryExpressionSyntax2;
		}
		BoundExpression boundExpression = ApplyFromDotNetConversionIfNeeded(BindValue(expressionSyntax, diagnostics, BindValueKind.RValue));
		do
		{
			binaryExpressionSyntax = (BinaryExpressionSyntax)expressionSyntax.Parent;
			BoundExpression right = ApplyFromDotNetConversionIfNeeded(BindValue(binaryExpressionSyntax.Right, diagnostics, BindValueKind.RValue));
			boundExpression = BindConditionalLogicalOperator(binaryExpressionSyntax, boundExpression, right, diagnostics);
			expressionSyntax = binaryExpressionSyntax;
		}
		while (expressionSyntax != node);
		return boundExpression;
	}

	private BoundExpression BindConditionalLogicalOperator(BinaryExpressionSyntax node, BoundExpression left, BoundExpression right, DiagnosticBag diagnostics)
	{
		BinaryOperatorKind binaryOperatorKind = SyntaxKindToBinaryOperatorKind(node.Kind);
		DebugAssertHelper.Assert(binaryOperatorKind == BinaryOperatorKind.LogicalAnd || binaryOperatorKind == BinaryOperatorKind.LogicalOr || binaryOperatorKind == BinaryOperatorKind.LogicalXor);
		if (left.HasAnyErrors || right.HasAnyErrors)
		{
			return new BoundBinaryOperator(node, binaryOperatorKind, left, right, null, null, LookupResultKind.Empty, GetBinaryOperatorErrorType(binaryOperatorKind, diagnostics, node), hasErrors: true);
		}
		BinaryOperatorAnalysisResult binaryOperatorAnalysisResult = BinaryOperatorOverloadResolution(binaryOperatorKind, left, right, node, diagnostics, out var resultKind, out var _);
		if (!binaryOperatorAnalysisResult.HasValue)
		{
			ReportBinaryOperatorError(node, diagnostics, node.OperatorToken, left, right, resultKind);
		}
		else
		{
			BinaryOperatorSignature signature = binaryOperatorAnalysisResult.Signature;
			bool flag = signature.LeftType.NavTypeKind == NavTypeKind.Boolean && signature.RightType.NavTypeKind == NavTypeKind.Boolean;
			if (flag)
			{
				DebugAssertHelper.Assert(flag);
				BoundExpression left2 = CreateConversion(left, binaryOperatorAnalysisResult.LeftConversion, signature.LeftType, null, diagnostics);
				BoundExpression right2 = CreateConversion(right, binaryOperatorAnalysisResult.RightConversion, signature.RightType, null, diagnostics);
				BinaryOperatorKind operatorKind = binaryOperatorKind | signature.Kind.OperandTypes();
				ConstantValue constantValueOpt = FoldConditionalLogicalOperator(binaryOperatorKind, left2, right2) ?? null;
				return new BoundBinaryOperator(node, operatorKind, left2, right2, constantValueOpt, signature.Method, resultKind, signature.ReturnType);
			}
			ReportBinaryOperatorError(node, diagnostics, node.OperatorToken, left, right, resultKind);
		}
		return new BoundBinaryOperator(node, binaryOperatorKind, left, right, null, null, resultKind, CreateErrorType(), hasErrors: true);
	}

	internal BoundExpression BindMemberArgument(IdentifierNameSyntax node, BoundExpression receiver, DiagnosticBag diagnostics)
	{
		return BindMemberAccessWithBoundLeft(node, receiver, node, invoked: false, isStatement: false, diagnostics);
	}

	private BoundExpression BindInList(InListExpressionSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node.ListExpression.Arguments.Count > 0);
		BoundExpression boundExpression = BindValue(node.Expression, diagnostics, BindValueKind.RValue);
		boundExpression = ApplyFromDotNetConversionIfNeeded(boundExpression);
		TypeSymbol type = boundExpression.Type;
		bool hasErrors = false;
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		foreach (CodeExpressionSyntax argument in node.ListExpression.Arguments)
		{
			BoundExpression boundExpression2 = BindValue(argument, diagnostics, BindValueKind.RValue);
			if (!boundExpression.HasAnyErrors && !boundExpression2.HasAnyErrors)
			{
				boundExpression2 = GenerateConversionForAssignment(type, boundExpression2, diagnostics);
			}
			instance.Add(boundExpression2);
		}
		return new BoundInListExpression(node, boundExpression, instance.ToImmutableAndFree(), Compilation.GetNavTypeSymbol(NavTypeKind.Boolean), hasErrors);
	}

	private BoundExpression BindOptionAccess(OptionAccessExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression boundLeft = BindLeftOfMemberAccess(node.Expression, diagnostics);
		return BindOptionAccess(node, boundLeft, diagnostics);
	}

	private BoundExpression BindOptionAccess(OptionAccessExpressionSyntax node, BoundExpression boundLeft, DiagnosticBag diagnostics)
	{
		if (boundLeft.HasAnyErrors || !IsValidOptionLeftHandSideExpression(boundLeft))
		{
			if (!boundLeft.HasAnyErrors)
			{
				Error(diagnostics, ErrorCode.ERR_LeftMustBeOption, node.Expression);
			}
			return ToBadExpression(boundLeft, LookupResultKind.NotAnOption);
		}
		TypeSymbol leftTypeForOptionAccess = GetLeftTypeForOptionAccess(boundLeft);
		LookupOptions options = LookupOptions.MustBeOption;
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			string text = node.Name.Unquoted();
			if (!string.IsNullOrEmpty(text))
			{
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				LookupMembersWithFallback(instance, leftTypeForOptionAccess, text, ref useSiteDiagnostics, options);
				diagnostics.Add(node.Name, useSiteDiagnostics);
				if (instance.IsMultiViable)
				{
					return BindMemberOfType(node, node.Name, text, boundLeft, invoked: false, isStatement: false, instance, diagnostics);
				}
			}
			BindOptionAccessReportError(node.Name, text, GetName(node.Expression), instance.Error, diagnostics);
			Symbol symbol = ((instance.Symbols.Count == 1) ? instance.Symbols[0] : null);
			return new BoundBadExpression(node, instance.Kind, ((object)symbol == null) ? ImmutableArray<Symbol>.Empty : ImmutableArray.Create(symbol), ImmutableArray.Create((BoundNode)boundLeft), boundLeft.Type);
		}
		finally
		{
			instance.Free();
		}
	}

	private static bool IsValidOptionLeftHandSideExpression(BoundExpression expression)
	{
		if (expression.Type.IsValidOptionType())
		{
			return true;
		}
		if (IsBoundQueryFilterWithOptionSource(expression))
		{
			return true;
		}
		return false;
	}

	private static bool IsBoundQueryFilterWithOptionSource(BoundExpression expression)
	{
		if (expression.Kind != BoundKind.QueryElementAccess)
		{
			return false;
		}
		BoundQueryElementAccess boundQueryElementAccess = (BoundQueryElementAccess)expression;
		if (boundQueryElementAccess.QueryElementSymbol.Kind != SymbolKind.QueryFilter)
		{
			return false;
		}
		TypeSymbol typeSymbol = boundQueryElementAccess.QueryElementSymbol?.RelatedField?.Type;
		if (typeSymbol == null)
		{
			return false;
		}
		return typeSymbol.IsValidOptionType();
	}

	private static TypeSymbol GetLeftTypeForOptionAccess(BoundExpression boundLeft)
	{
		TypeSymbol typeSymbol = boundLeft.Type;
		if (typeSymbol.IsArray())
		{
			typeSymbol = ((ArrayTypeSymbol)typeSymbol).ElementType;
		}
		if (typeSymbol.NavTypeKind == NavTypeKind.QueryFilter)
		{
			typeSymbol = (((BoundQueryElementAccess)boundLeft)?.QueryElementSymbol)?.RelatedField?.Type ?? typeSymbol;
		}
		return typeSymbol;
	}

	private static void BindOptionAccessReportError(SyntaxNode name, string memberName, string optionName, DiagnosticInfo lookupError, DiagnosticBag diagnostics)
	{
		if (lookupError != null)
		{
			diagnostics.Add(new NavDiagnostic(lookupError, name.Location));
			return;
		}
		Error(diagnostics, ErrorCode.ERR_NoSuchMember, name, optionName, memberName);
	}

	private BoundExpression BindElementAccess(ElementAccessExpressionSyntax node, DiagnosticBag diagnostics)
	{
		BoundExpression receiver = BindExpression(node.Expression, diagnostics, invoked: false);
		return BindElementAccess(node, receiver, node.ArgumentList, diagnostics);
	}

	private BoundExpression BindElementAccess(ExpressionSyntax node, BoundExpression receiver, BracketedArgumentListSyntax argumentList, DiagnosticBag diagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		try
		{
			BindArgumentsAndNames(argumentList, diagnostics, instance);
			receiver = CheckValue(receiver, BindValueKind.RValue, diagnostics);
			return BindElementOrIndexerAccess(node, receiver, instance, diagnostics);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundExpression BindElementOrIndexerAccess(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments analyzedArguments, DiagnosticBag diagnostics)
	{
		if ((object)expr.Type == null)
		{
			return BadIndexerExpression(node, expr, analyzedArguments, null, diagnostics);
		}
		if (analyzedArguments.HasErrors || expr.HasAnyErrors)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				return BindElementAccessCore(node, expr, analyzedArguments, instance);
			}
			finally
			{
				instance.Free();
			}
		}
		return BindElementAccessCore(node, expr, analyzedArguments, diagnostics);
	}

	private BoundExpression BindElementAccessCore(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments arguments, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		DebugAssertHelper.Assert(expr != null);
		DebugAssertHelper.Assert((object)expr.Type != null);
		DebugAssertHelper.Assert(arguments != null);
		if (expr.Type.TypeCategory == TypeCategoryKind.Array)
		{
			return BindArrayAccess(node, expr, arguments, diagnostics);
		}
		NavTypeKind navTypeKind = expr.Type.NavTypeKind;
		if (navTypeKind == NavTypeKind.String || (uint)(navTypeKind - 16646153) <= 1u)
		{
			return BindTextCharAccess(node, expr, arguments, diagnostics);
		}
		return BadIndexerExpression(node, expr, arguments, null, diagnostics);
	}

	private BoundExpression BindArrayAccess(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments arguments, DiagnosticBag diagnostics)
	{
		ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)expr.Type;
		int length = arrayTypeSymbol.Dimensions.Length;
		bool hasErrors = false;
		if (length < arguments.Arguments.Count)
		{
			Error(diagnostics, ErrorCode.ERR_BadIndexCount, node, length);
			hasErrors = true;
		}
		BoundExpression[] array = new BoundExpression[arguments.Arguments.Count];
		for (int i = 0; i < arguments.Arguments.Count; i++)
		{
			BoundExpression index = arguments.Arguments[i];
			BoundExpression boundExpression = ConvertToArrayIndex(index, node, diagnostics);
			array[i] = boundExpression;
		}
		TypeSymbol type = ((length > arguments.Arguments.Count) ? ArrayTypeSymbol.Create(arrayTypeSymbol.ElementType, arrayTypeSymbol.Dimensions.Skip(arguments.Arguments.Count).ToImmutableArray()) : arrayTypeSymbol.ElementType);
		return new BoundArrayAccess(node, expr, array.AsImmutableOrNull(), type, hasErrors);
	}

	private BoundExpression BindTextCharAccess(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments arguments, DiagnosticBag diagnostics)
	{
		bool hasErrors = false;
		BoundExpression index;
		if (arguments.Arguments.Count != 1)
		{
			if (arguments.Arguments.Count > 1)
			{
				Error(diagnostics, ErrorCode.ERR_BadIndexCount, node, 1);
			}
			hasErrors = true;
			index = new BoundBadExpression(node, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundNode>.Empty, TypeSymbol.GetNavTypeSymbol(NavTypeKind.Integer));
		}
		else
		{
			index = ConvertToArrayIndex(arguments.Arguments[0], node, diagnostics);
		}
		return new BoundTextIndexAccess(node, expr, index, TypeSymbol.GetNavTypeSymbol(NavTypeKind.Char), hasErrors);
	}

	private BoundExpression ConvertToArrayIndex(BoundExpression index, SyntaxNode node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(index != null);
		BoundExpression boundExpression = TryImplicitConversionToArrayIndex(index, NavTypeKind.Integer, node, diagnostics);
		if (boundExpression == null)
		{
			TypeSymbol navType = GetNavType(NavTypeKind.Integer, diagnostics, node);
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			Conversion conversion = Conversions.ClassifyConversionFromExpression(index, navType, ref useSiteDiagnostics);
			diagnostics.Add(node, useSiteDiagnostics);
			GenerateImplicitConversionError(diagnostics, node, conversion, index, navType);
			return new BoundConversion(index.Syntax, index, conversion, null, navType, null, hasErrors: true);
		}
		return boundExpression;
	}

	private BoundExpression TryImplicitConversionToArrayIndex(BoundExpression expr, NavTypeKind navTypeKind, SyntaxNode node, DiagnosticBag diagnostics)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			TypeSymbol navType = GetNavType(navTypeKind, instance, node);
			DebugAssertHelper.Assert(expr != null);
			DebugAssertHelper.Assert((object)navType != null);
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			Conversion conversion = Conversions.ClassifyImplicitConversionFromExpression(expr, navType, ref useSiteDiagnostics);
			diagnostics.Add(node, useSiteDiagnostics);
			if (!conversion.Exists)
			{
				return null;
			}
			BoundExpression boundExpression = CreateConversion(expr.Syntax, expr, conversion.ToArrayIndexConversion(), isCast: false, navType, null, instance);
			DebugAssertHelper.Assert(boundExpression != null);
			diagnostics.AddRange(instance);
			return boundExpression;
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundExpression BadIndexerExpression(ExpressionSyntax node, BoundExpression expr, AnalyzedArguments analyzedArguments, DiagnosticInfo errorOpt, DiagnosticBag diagnostics)
	{
		if (!expr.HasAnyErrors)
		{
			diagnostics.Add(errorOpt ?? new NavDiagnosticInfo(ErrorCode.ERR_BadIndexLHS, expr.Display), node.Location);
		}
		ImmutableArray<BoundNode> childBoundNodes = StaticCast<BoundNode>.From(analyzedArguments.Arguments.ToImmutable()).Add(expr);
		return new BoundBadExpression(node, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, childBoundNodes, CreateErrorType(), hasErrors: true);
	}

	internal BoundExpression BindArgument(ExpressionSyntax node, DiagnosticBag diagnostics, BindValueKind valueKind)
	{
		BoundExpression expr = BindExpression(node, diagnostics, invoked: false);
		return CheckValue(expr, valueKind, diagnostics);
	}

	internal BoundExpression BindValue(ExpressionSyntax node, DiagnosticBag diagnostics, BindValueKind valueKind, bool isStatement = false)
	{
		BoundExpression expr = BindExpression(node, diagnostics, invoked: false, isStatement);
		return CheckValue(expr, valueKind, diagnostics);
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax)
	{
		return BadExpression(syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty);
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax, BoundNode childNode)
	{
		return BadExpression(syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, childNode);
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol> symbols, params BoundNode[] childNodes)
	{
		return new BoundBadExpression(syntax, resultKind, symbols, ImmutableArray.Create(childNodes), CreateErrorType());
	}

	private BoundCall CreateBadCall(SyntaxNode node, BoundExpression expr, LookupResultKind resultKind, AnalyzedArguments analyzedArguments)
	{
		TypeSymbol returnType = new ExtendedErrorTypeSymbol(Compilation, string.Empty, null);
		MethodSymbol method = new ErrorMethodSymbol(expr.Type ?? ContainingType, returnType, string.Empty);
		ImmutableArray<BoundExpression> arguments = analyzedArguments.Arguments.ToImmutable();
		ImmutableArray<MethodSymbol> originalMethods = ((expr.Kind == BoundKind.MethodGroup) ? ((BoundMethodGroup)expr).Methods : ImmutableArray<MethodSymbol>.Empty);
		return BoundCall.ErrorCall(node, expr, method, arguments, originalMethods, resultKind);
	}

	private BoundCall CreateBadCall(SyntaxNode node, string name, BoundExpression receiver, ImmutableArray<MethodSymbol> methods, LookupResultKind resultKind, AnalyzedArguments analyzedArguments)
	{
		ImmutableArray<BoundExpression> arguments = analyzedArguments.Arguments.ToImmutable();
		MethodSymbol method;
		if (methods.Length == 1)
		{
			method = methods[0];
		}
		else
		{
			TypeSymbol returnType = GetCommonTypeOrReturnType(methods) ?? new ExtendedErrorTypeSymbol(Compilation, string.Empty, null);
			TypeSymbol containingType = ContainingType;
			if (receiver != null && (object)receiver.Type != null)
			{
				containingType = receiver.Type;
			}
			else if (methods.Length > 0)
			{
				containingType = methods[0].ContainingType;
			}
			method = new ErrorMethodSymbol(containingType, returnType, name);
		}
		return BoundCall.ErrorCall(node, receiver, method, arguments, methods, resultKind);
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax, LookupResultKind resultKind)
	{
		return new BoundBadExpression(syntax, resultKind, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundNode>.Empty, CreateErrorType());
	}

	private BoundBadExpression BadExpression(SyntaxNode syntax, LookupResultKind resultKind, ImmutableArray<Symbol> symbols)
	{
		return new BoundBadExpression(syntax, resultKind, symbols, ImmutableArray<BoundNode>.Empty, CreateErrorType());
	}

	private BoundExpression BindIdentifier(IdentifierNameSyntax node, bool invoked, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		if (node.IsMissing)
		{
			return BadExpression(node);
		}
		BoundExpression boundExpression = null;
		LookupResult instance = LookupResult.GetInstance();
		LookupOptions lookupOptions = LookupOptions.AllMethods | LookupOptions.ValidateSymbolCompilationScope;
		if (invoked)
		{
			lookupOptions |= LookupOptions.MustBeInvocableIfMember;
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		LookupSymbolsWithFallback(instance, node.Unquoted(), ref useSiteDiagnostics, lookupOptions);
		diagnostics.Add(node, useSiteDiagnostics);
		if (instance.Kind != LookupResultKind.Empty)
		{
			ArrayBuilder<Symbol> instance2 = ArrayBuilder<Symbol>.GetInstance();
			try
			{
				bool flag = false;
				Symbol symbolOrMethodOrPropertyGroup = GetSymbolOrMethodOrPropertyGroup(instance, node, node.Identifier.ValueText, instance2, diagnostics, out var wasError);
				flag |= wasError;
				if ((object)symbolOrMethodOrPropertyGroup == null)
				{
					DebugAssertHelper.Assert(instance2.Count > 0);
					BoundExpression receiver = instance.GetReceiver(instance2[0]);
					boundExpression = ConstructBoundMemberGroup(node, receiver, node.Identifier.ValueText, instance2, instance, flag, diagnostics);
					if (!invoked)
					{
						boundExpression = BindInvocationExpression(node, (BoundMethodGroup)boundExpression, isStatement: false, diagnostics);
					}
				}
				else
				{
					BoundExpression receiver2 = instance.GetReceiver(symbolOrMethodOrPropertyGroup);
					ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, symbolOrMethodOrPropertyGroup, node);
					boundExpression = BindNonMethod(node, symbolOrMethodOrPropertyGroup, flag, receiver2);
				}
			}
			finally
			{
				instance2.Free();
			}
		}
		else
		{
			boundExpression = BadExpression(node);
			if (instance.Error != null)
			{
				Error(diagnostics, instance.Error, node);
			}
			else
			{
				Error(diagnostics, ErrorCode.ERR_NameNotInContext, node, node.Identifier.ValueText);
			}
		}
		instance.Free();
		return boundExpression;
	}

	private static BoundLiteral BindLiteralConstant(LiteralExpressionSyntax node, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node.Kind == SyntaxKind.LiteralExpression);
		object literalValue = node.Literal.GetLiteralValue();
		TypeSymbol type = null;
		ConstantValue constantValueOpt;
		if (literalValue == null)
		{
			constantValueOpt = ConstantValue.Null;
		}
		else
		{
			NavTypeKind navTypeKind = NavTypeExtensions.FromLiteralSyntaxKind(node.Literal.Kind);
			if (IsViableCharLiteral(node, literalValue, navTypeKind))
			{
				navTypeKind = NavTypeKind.Char;
			}
			DebugAssertHelper.Assert(navTypeKind != NavTypeKind.None);
			constantValueOpt = ConstantValue.Create(literalValue, navTypeKind);
			type = GetNavType(navTypeKind, diagnostics, node);
		}
		return new BoundLiteral(node, constantValueOpt, type);
	}

	private static bool IsViableCharLiteral(LiteralExpressionSyntax node, object value, NavTypeKind navTypeKind)
	{
		if (navTypeKind == NavTypeKind.String && node.Parent.Kind != SyntaxKind.ArgumentList)
		{
			string obj = value as string;
			if (obj == null)
			{
				return false;
			}
			return obj.Length == 1;
		}
		return false;
	}

	private void CoerceArguments<TMember>(MemberResolutionResult<TMember> methodResult, ArrayBuilder<BoundExpression> arguments, DiagnosticBag diagnostics) where TMember : Symbol
	{
		MemberAnalysisResult result = methodResult.Result;
		EffectiveParameters effectiveParameters = result.EffectiveParameters;
		for (int i = 0; i < arguments.Count; i++)
		{
			BoundExpression boundExpression = arguments[i];
			CheckValue(boundExpression, GetCorrespondingParameter(effectiveParameters.Parameters, i).IsVar ? BindValueKind.OutParameter : BindValueKind.RValue, diagnostics);
			Conversion conversion = result.ConversionForArg(i);
			if (!conversion.IsIdentity)
			{
				TypeSymbol correspondingParameterType = GetCorrespondingParameterType(effectiveParameters.Parameters, i);
				arguments[i] = CreateConversion(boundExpression.Syntax, boundExpression, conversion, isCast: false, correspondingParameterType, null, diagnostics);
			}
		}
	}

	private static ParameterSymbol GetCorrespondingParameter(ImmutableArray<ParameterSymbol> parameters, int arg)
	{
		return parameters[arg];
	}

	private static TypeSymbol GetCorrespondingParameterType(ImmutableArray<ParameterSymbol> parameters, int arg)
	{
		return parameters[arg].Type;
	}

	private void BindArgumentsAndNames(BracketedArgumentListSyntax argumentListOpt, DiagnosticBag diagnostics, AnalyzedArguments result)
	{
		if (argumentListOpt != null)
		{
			BindArgumentsAndNames(argumentListOpt.Arguments, result);
			result.PopulateDiagnostics(diagnostics);
		}
	}

	private void BindArgumentsAndNames(ArgumentListSyntax argumentListOpt, AnalyzedArguments result)
	{
		if (argumentListOpt != null)
		{
			BindArgumentsAndNames(argumentListOpt.Arguments, result);
		}
	}

	private void BindArgumentsAndNames(SeparatedSyntaxList<CodeExpressionSyntax> arguments, AnalyzedArguments result)
	{
		int i = 0;
		for (int count = arguments.Count; i < count; i++)
		{
			CodeExpressionSyntax argumentExpression = arguments[i];
			BindArgumentAndName(result, hadError: false, argumentExpression);
		}
	}

	private bool BindArgumentAndName(AnalyzedArguments result, bool hadError, ExpressionSyntax argumentExpression)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		BindValueKind valueKind = BindValueKind.RValue;
		BoundExpression item = BindValue(argumentExpression, instance, valueKind);
		result.Arguments.Add(item);
		result.ArgumentDiagnostics.Add(instance);
		return hadError;
	}

	private Symbol GetSymbolOrMethodOrPropertyGroup(LookupResult result, SyntaxNode node, string plainName, ArrayBuilder<Symbol> methodOrPropertyGroup, DiagnosticBag diagnostics, out bool wasError)
	{
		DebugAssertHelper.Assert(!methodOrPropertyGroup.Any());
		node = GetSimpleNameSyntax(node) ?? node;
		wasError = false;
		DebugAssertHelper.Assert(result.Kind != LookupResultKind.Empty);
		Symbol symbol = null;
		foreach (Symbol symbol2 in result.Symbols)
		{
			SymbolKind kind = symbol2.Kind;
			if (methodOrPropertyGroup.Count > 0)
			{
				SymbolKind kind2 = methodOrPropertyGroup[0].Kind;
				if (kind2 != kind)
				{
					if (kind2 == SymbolKind.Method)
					{
						symbol = symbol2;
						continue;
					}
					symbol = methodOrPropertyGroup[0];
					methodOrPropertyGroup.Clear();
				}
			}
			if (kind == SymbolKind.Method)
			{
				methodOrPropertyGroup.Add(symbol2);
			}
			else
			{
				symbol = symbol2;
			}
		}
		DebugAssertHelper.Assert(methodOrPropertyGroup.Any() || (object)symbol != null);
		if (methodOrPropertyGroup.Count > 0 && IsMethodOrPropertyGroup(methodOrPropertyGroup) && (methodOrPropertyGroup[0].Kind == SymbolKind.Method || (object)symbol == null))
		{
			if (result.Error != null)
			{
				Error(diagnostics, result.Error, node);
				wasError = result.Error.Severity == DiagnosticSeverity.Error;
			}
			return null;
		}
		methodOrPropertyGroup.Clear();
		return ResultSymbol(result, plainName, node, diagnostics, suppressUseSiteDiagnostics: false, out wasError);
	}

	private static bool IsMethodOrPropertyGroup(ArrayBuilder<Symbol> members)
	{
		DebugAssertHelper.Assert(members.Count > 0);
		Symbol member = members[0];
		DebugAssertHelper.Assert(members.All((Symbol m) => m.Kind == member.Kind));
		SymbolKind kind = member.Kind;
		if (kind == SymbolKind.Method)
		{
			return true;
		}
		throw ExceptionUtilities.UnexpectedValue(member.Kind);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
	private static BoundExpression BindNonMethod(NameSyntax node, Symbol symbol, bool isError, BoundExpression receiver)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.LocalVariable:
		{
			VariableSymbol variableSymbol2 = (VariableSymbol)symbol;
			return new BoundLocal(node, variableSymbol2, variableSymbol2.Type, isError);
		}
		case SymbolKind.Parameter:
		{
			ParameterSymbol parameterSymbol = (ParameterSymbol)symbol;
			return new BoundParameter(node, parameterSymbol, parameterSymbol.Type, isError);
		}
		case SymbolKind.ReturnValue:
		{
			ReturnValueSymbol returnValueSymbol = (ReturnValueSymbol)symbol;
			return new BoundReturnValue(node, returnValueSymbol, returnValueSymbol.Type, isError);
		}
		case SymbolKind.GlobalVariable:
		{
			VariableSymbol variableSymbol = (VariableSymbol)symbol;
			return new BoundGlobal(node, variableSymbol, variableSymbol.Type, isError);
		}
		case SymbolKind.Field:
		{
			FieldSymbol fieldSymbol = (FieldSymbol)symbol;
			return new BoundFieldAccess(node, receiver, fieldSymbol, LookupResultKind.Viable, fieldSymbol.Type, isError);
		}
		case SymbolKind.QueryFilter:
		case SymbolKind.QueryColumn:
		{
			QueryElementSymbol queryElementSymbol = (QueryElementSymbol)symbol;
			return new BoundQueryElementAccess(node, receiver, queryElementSymbol, LookupResultKind.Viable, queryElementSymbol.Type, isError);
		}
		case SymbolKind.Class:
		{
			LanguageClassTypeSymbol languageClassTypeSymbol = (LanguageClassTypeSymbol)symbol;
			return new BoundClassType(node, languageClassTypeSymbol, languageClassTypeSymbol);
		}
		case SymbolKind.Enum:
		{
			EnumTypeSymbol enumTypeSymbol = (EnumTypeSymbol)symbol;
			return new BoundEnumType(node, enumTypeSymbol, enumTypeSymbol);
		}
		case SymbolKind.Control:
			return BindControlAccess(node, receiver, LookupResultKind.Viable, (ControlSymbol)symbol, isError);
		case SymbolKind.ErrorType:
		{
			ErrorTypeSymbol errorTypeSymbol = (ErrorTypeSymbol)symbol;
			return new BoundBadExpression(node, errorTypeSymbol.ResultKind, errorTypeSymbol.CandidateSymbols, ImmutableArray<BoundNode>.Empty, errorTypeSymbol, isError);
		}
		case SymbolKind.Action:
		{
			ActionSymbol actionSymbol = (ActionSymbol)symbol;
			return new BoundBadExpression(node, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundNode>.Empty, actionSymbol.Type, isError);
		}
		case SymbolKind.ReportDataItem:
			return BindReportDataItemAccess(node, receiver, (ReportDataItemSymbol)symbol, LookupResultKind.Viable, isError);
		case SymbolKind.RequestPage:
			return BindRequestPageAccess(node, receiver, (RequestPageTypeSymbol)symbol, LookupResultKind.Viable, isError);
		case SymbolKind.XmlPortNode:
			return BindXmlPortNodeAccess(node, receiver, (SourceXmlPortNodeSymbol)symbol, LookupResultKind.Viable, isError);
		case SymbolKind.QueryDataItem:
			return BindQueryDataItemAccess(node, receiver, (QueryDataItemSymbol)symbol, LookupResultKind.Viable, isError);
		case SymbolKind.Key:
			return BindTableKeyAccess(node, receiver, (FieldNameListSymbol)symbol, isError);
		case SymbolKind.FieldGroup:
			return BindTableFieldGroupAccess(node, receiver, (FieldGroupSymbol)symbol, isError);
		case SymbolKind.ReportLabel:
			return BindReportLabelAccess(node, receiver, (ReportLabelSymbol)symbol, isError);
		case SymbolKind.TestField:
			return BindTestFieldAccess(node, receiver, (TestFieldSymbol)symbol, LookupResultKind.Viable, isError);
		case SymbolKind.TestPart:
			return BindTestPartAccess(node, receiver, (TestPartSymbol)symbol, LookupResultKind.Viable, isError);
		case SymbolKind.TestAction:
			return BindTestActionAccess(node, receiver, (TestActionSymbol)symbol, LookupResultKind.Viable, isError);
		case SymbolKind.TestFilter:
			return BindTestFilterAccess(node, receiver, (TestFilterSymbol)symbol, LookupResultKind.Viable, isError);
		case SymbolKind.TestFilterField:
			return BindTestFilterFieldAccess(node, receiver, (TestFilterFieldSymbol)symbol, LookupResultKind.Viable, isError);
		case SymbolKind.OptionType:
		{
			SystemOptionTypeSymbol systemOptionTypeSymbol = (SystemOptionTypeSymbol)symbol;
			return new BoundSystemOptionType(node, systemOptionTypeSymbol, systemOptionTypeSymbol);
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.Kind);
		}
	}

	private BoundExpression BindMemberAccess(MemberAccessExpressionSyntax node, bool invoked, bool isStatement, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		BoundExpression boundLeft = BindLeftOfMemberAccess(node.Expression, diagnostics);
		return BindMemberAccessWithBoundLeft(node, boundLeft, node.Name, invoked, isStatement, diagnostics);
	}

	private BoundExpression BindMemberAccessWithBoundLeft(ExpressionSyntax node, BoundExpression boundLeft, SimpleNameSyntax right, bool invoked, bool isStatement, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		DebugAssertHelper.Assert(boundLeft != null);
		boundLeft = MakeMemberAccessValue(boundLeft, diagnostics);
		TypeSymbol type = boundLeft.Type;
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			string text = right.Identifier.ValueText.UnquoteIdentifier();
			_ = boundLeft.Kind;
			if (boundLeft.Kind == BoundKind.Literal && ((BoundLiteral)boundLeft).ConstantValueOpt == ConstantValue.Null)
			{
				if (!boundLeft.HasAnyErrors)
				{
					throw ExceptionUtilities.SkippedForNow;
				}
				return BadExpression(node, boundLeft);
			}
			if (!type.IsNullOrTypeMissing())
			{
				boundLeft = CheckValue(boundLeft, BindValueKind.RValue, diagnostics);
				return BindInstanceMemberAccess(node, right, boundLeft, text, invoked, isStatement, diagnostics);
			}
			BindMemberAccessReportError(node, right, text, boundLeft, instance.Error, diagnostics);
			return BindMemberAccessBadResult(node, text, boundLeft, instance.Error, instance.Symbols.ToImmutable(), instance.Kind);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundExpression BindInstanceMemberAccess(SyntaxNode node, SyntaxNode right, BoundExpression boundLeft, string rightName, bool invoked, bool isStatement, DiagnosticBag diagnostics)
	{
		TypeSymbol type = boundLeft.Type;
		if (right.HasErrors)
		{
			return new BoundBadExpression(node, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray.Create((BoundNode)boundLeft), CreateErrorType());
		}
		LookupOptions lookupOptions = type.GetLookupOptions() | LookupOptions.ValidateSymbolCompilationScope;
		if (invoked)
		{
			lookupOptions |= LookupOptions.MustBeInvocableIfMember;
		}
		LookupResult instance = LookupResult.GetInstance();
		try
		{
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			LookupMembersWithFallback(instance, type, rightName, ref useSiteDiagnostics, lookupOptions);
			diagnostics.Add(right, useSiteDiagnostics);
			if (instance.IsMultiViable)
			{
				return BindMemberOfType(node, right, rightName, boundLeft, invoked, isStatement, instance, diagnostics);
			}
			BindMemberAccessReportError(node, right, rightName, boundLeft, instance.Error, diagnostics);
			return BindMemberAccessBadResult(node, rightName, boundLeft, instance.Error, instance.Symbols.ToImmutable(), instance.Kind);
		}
		finally
		{
			instance.Free();
		}
	}

	private BoundExpression BindMemberOfType(SyntaxNode node, SyntaxNode right, string plainName, BoundExpression left, bool invoked, bool isStatement, LookupResult lookupResult, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(node != null);
		DebugAssertHelper.Assert(left != null);
		DebugAssertHelper.Assert(lookupResult.IsMultiViable);
		DebugAssertHelper.Assert(lookupResult.Symbols.Any());
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		Symbol symbolOrMethodOrPropertyGroup = GetSymbolOrMethodOrPropertyGroup(lookupResult, right, plainName, instance, diagnostics, out var wasError);
		BoundExpression boundExpression;
		if ((object)symbolOrMethodOrPropertyGroup == null)
		{
			DebugAssertHelper.Assert(instance.Count > 0);
			boundExpression = ConstructBoundMemberGroup(node, left, plainName, instance, lookupResult, wasError, diagnostics);
			if (!invoked)
			{
				boundExpression = BindInvocationExpression(node as ExpressionSyntax, (BoundMethodGroup)boundExpression, isStatement, diagnostics);
			}
		}
		else
		{
			DebugAssertHelper.Assert(symbolOrMethodOrPropertyGroup.Kind != SymbolKind.Method);
			ReportDiagnosticsIfObsoleteDeprecatedOrIncompatible(diagnostics, symbolOrMethodOrPropertyGroup, right);
			switch (symbolOrMethodOrPropertyGroup.Kind)
			{
			case SymbolKind.Field:
				boundExpression = BindFieldAccess(node, left, (FieldSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.Option:
			case SymbolKind.EnumValue:
				boundExpression = BindOptionAccess(node, left, (OptionSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.Table:
			case SymbolKind.Codeunit:
			case SymbolKind.Page:
			case SymbolKind.Report:
			case SymbolKind.Query:
			case SymbolKind.XmlPort:
				boundExpression = BindApplicationObjectAccess(node, left, (ApplicationObjectTypeSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.TestField:
				boundExpression = BindTestFieldAccess(node, left, (TestFieldSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.TestPart:
				boundExpression = BindTestPartAccess(node, left, (TestPartSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.TestAction:
				boundExpression = BindTestActionAccess(node, left, (TestActionSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.TestFilter:
				boundExpression = BindTestFilterAccess(node, left, (TestFilterSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.TestFilterField:
				boundExpression = BindTestFilterFieldAccess(node, left, (TestFilterFieldSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.Control:
				boundExpression = BindControlAccess(node, left, lookupResult.Kind, (ControlSymbol)symbolOrMethodOrPropertyGroup, wasError);
				break;
			case SymbolKind.QueryFilter:
			case SymbolKind.QueryColumn:
				boundExpression = BindQueryColumnAccess(node, left, (QueryElementSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.RequestPage:
				boundExpression = BindRequestPageAccess(node, left, (RequestPageTypeSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.ReportDataItem:
				boundExpression = BindReportDataItemAccess(node, left, (ReportDataItemSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.XmlPortNode:
				boundExpression = BindXmlPortNodeAccess(node, left, (SourceXmlPortNodeSymbol)symbolOrMethodOrPropertyGroup, lookupResult.Kind, wasError);
				break;
			case SymbolKind.ErrorType:
				boundExpression = new BoundBadExpression(node, lookupResult.Kind, instance.ToImmutable(), ImmutableArray<BoundNode>.Empty, NavCorLib.None);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(symbolOrMethodOrPropertyGroup.Kind);
			}
		}
		instance.Free();
		return boundExpression;
	}

	private static BoundExpression BindControlAccess(SyntaxNode node, BoundExpression left, LookupResultKind lookupResultKind, ControlSymbol control, bool wasError)
	{
		if (left == null)
		{
			return new BoundBadExpression(node, lookupResultKind, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundNode>.Empty, NavCorLib.None);
		}
		return new BoundControlAccess(node, left, control, lookupResultKind, control, wasError);
	}

	private static BoundExpression BindFieldAccess(SyntaxNode node, BoundExpression left, FieldSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundFieldAccess(node, left, symbol, lookupResult, symbol.Type, hasErrors);
	}

	private static BoundExpression BindQueryColumnAccess(SyntaxNode node, BoundExpression left, QueryElementSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundQueryElementAccess(node, left, symbol, lookupResult, symbol.Type, hasErrors);
	}

	private static BoundExpression BindRequestPageAccess(SyntaxNode node, BoundExpression left, RequestPageTypeSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundObjectAccess(node, left, symbol, lookupResult, symbol.Type, hasErrors);
	}

	private static BoundExpression BindReportDataItemAccess(SyntaxNode node, BoundExpression left, ReportDataItemSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundReportDataItemAccess(node, left, symbol, lookupResult, symbol.Type, hasErrors);
	}

	private static BoundExpression BindQueryDataItemAccess(NameSyntax node, BoundExpression left, QueryDataItemSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundQueryDataItemAccess(node, left, symbol, lookupResult, symbol.Type, hasErrors);
	}

	private static BoundExpression BindTableKeyAccess(NameSyntax node, BoundExpression receiver, FieldNameListSymbol symbol, bool hasErrors)
	{
		return new BoundTableKeyAccess(node, receiver, symbol, LookupResultKind.NotReferencable, NavCorLib.NoneType, hasErrors);
	}

	private static BoundExpression BindTableFieldGroupAccess(NameSyntax node, BoundExpression receiver, FieldGroupSymbol symbol, bool hasErrors)
	{
		return new BoundTableFieldGroupAccess(node, receiver, symbol, LookupResultKind.NotReferencable, NavCorLib.NoneType, hasErrors);
	}

	private static BoundExpression BindReportLabelAccess(NameSyntax node, BoundExpression receiver, ReportLabelSymbol symbol, bool hasErrors)
	{
		return new BoundReportLabelAccess(node, receiver, symbol, LookupResultKind.NotReferencable, NavCorLib.NoneType, hasErrors);
	}

	private static BoundExpression BindXmlPortNodeAccess(SyntaxNode node, BoundExpression receiver, SourceXmlPortNodeSymbol symbol, LookupResultKind viable, bool hasErrors)
	{
		return new BoundXmlPortNodeAccess(node, receiver, symbol, viable, symbol.Type, hasErrors);
	}

	private static BoundExpression BindOptionAccess(SyntaxNode node, BoundExpression left, OptionSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundOptionAccess(node, left, symbol, lookupResult, GetOptionAccessType(symbol), hasErrors);
	}

	private static TypeSymbol GetOptionAccessType(OptionSymbol symbol)
	{
		switch (symbol.Type.NavTypeKind)
		{
		case NavTypeKind.Option:
			return TypeSymbol.GetNavTypeSymbol(NavTypeKind.Integer);
		case NavTypeKind.EnumExtension:
		case NavTypeKind.DotNet:
		case NavTypeKind.Enum:
			return symbol.Type;
		default:
			return TypeSymbol.GetNavTypeSymbol(symbol.Type.NavTypeKind);
		}
	}

	private static BoundExpression BindApplicationObjectAccess(SyntaxNode node, BoundExpression left, ApplicationObjectTypeSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundApplicationObjectAccess(node, left, symbol, lookupResult, TypeSymbol.GetNavTypeSymbol(NavTypeKind.Integer), hasErrors);
	}

	private static BoundExpression BindTestFieldAccess(SyntaxNode node, BoundExpression left, TestFieldSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundTestFieldAccess(node, left, symbol, lookupResult, NavCorLib.TestFieldType, hasErrors);
	}

	private static BoundExpression BindTestPartAccess(SyntaxNode node, BoundExpression left, TestPartSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundTestPartAccess(node, left, symbol, lookupResult, symbol, hasErrors);
	}

	private static BoundExpression BindTestActionAccess(SyntaxNode node, BoundExpression left, TestActionSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundTestActionAccess(node, left, symbol, lookupResult, NavCorLib.TestActionType, hasErrors);
	}

	private static BoundExpression BindTestFilterAccess(SyntaxNode node, BoundExpression left, TestFilterSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundTestFilterAccess(node, left, symbol, lookupResult, symbol, hasErrors);
	}

	private static BoundExpression BindTestFilterFieldAccess(SyntaxNode node, BoundExpression left, TestFilterFieldSymbol symbol, LookupResultKind lookupResult, bool hasErrors)
	{
		return new BoundTestFilterFieldAccess(node, left, symbol, lookupResult, NavCorLib.TestFilterFieldType, hasErrors);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "node")]
	private static void BindMemberAccessReportError(SyntaxNode node, SyntaxNode name, string plainName, BoundExpression boundLeft, DiagnosticInfo lookupError, DiagnosticBag diagnostics)
	{
		if (!boundLeft.HasAnyErrors)
		{
			if (lookupError != null)
			{
				diagnostics.Add(new NavDiagnostic(lookupError, name.Location));
				return;
			}
			Error(diagnostics, ErrorCode.ERR_NoSuchMember, name, boundLeft.Type, plainName);
		}
	}

	private BoundExpression MakeMemberAccessValue(BoundExpression expr, DiagnosticBag diagnostics)
	{
		BoundKind kind = expr.Kind;
		if (kind == BoundKind.MethodGroup)
		{
			BoundMethodGroup node = (BoundMethodGroup)expr;
			HashSet<DiagnosticInfo> useSiteDiagnostics = null;
			MethodGroupResolution methodGroupResolution = ResolveMethodGroup(node, null, ref useSiteDiagnostics);
			diagnostics.Add(expr.Syntax, useSiteDiagnostics);
			if (!expr.HasAnyErrors)
			{
				diagnostics.AddRange(methodGroupResolution.Diagnostics);
				if (methodGroupResolution.MethodGroup != null && !methodGroupResolution.HasAnyErrors)
				{
					DebugAssertHelper.Assert(!methodGroupResolution.IsEmpty);
					throw ExceptionUtilities.SkippedForNow;
				}
			}
			expr = BindMemberAccessBadResult(node);
			methodGroupResolution.Free();
			return expr;
		}
		return expr;
	}

	private BoundExpression BindMemberAccessBadResult(BoundMethodGroup node)
	{
		SyntaxNode nameSyntax = node.NameSyntax;
		SyntaxNode node2 = node.MemberAccessExpressionSyntax ?? nameSyntax;
		return BindMemberAccessBadResult(node2, node.Name, node.ReceiverOpt, node.LookupError, StaticCast<Symbol>.From(node.Methods), node.ResultKind);
	}

	private BoundExpression BindMemberAccessBadResult(SyntaxNode node, string nameString, BoundExpression boundLeft, DiagnosticInfo lookupError, ImmutableArray<Symbol> symbols, LookupResultKind lookupKind)
	{
		if (symbols.Length > 1 && symbols[0].Kind == SymbolKind.Method)
		{
			ArrayBuilder<MethodSymbol> instance = ArrayBuilder<MethodSymbol>.GetInstance();
			foreach (Symbol item2 in symbols)
			{
				if (item2 is MethodSymbol item)
				{
					instance.Add(item);
				}
			}
			ImmutableArray<MethodSymbol> methods = instance.ToImmutableAndFree();
			return new BoundMethodGroup(node, nameString, methods, (methods.Length == 1) ? methods[0] : null, lookupError, boundLeft, lookupKind, hasErrors: true);
		}
		Symbol symbol = ((symbols.Length == 1) ? symbols[0] : null);
		return new BoundBadExpression(node, lookupKind, ((object)symbol == null) ? ImmutableArray<Symbol>.Empty : ImmutableArray.Create(symbol), (boundLeft == null) ? ImmutableArray<BoundNode>.Empty : ImmutableArray.Create((BoundNode)boundLeft), CreateErrorType());
	}

	private BoundExpression BindLeftOfMemberAccess(ExpressionSyntax left, DiagnosticBag diagnostics)
	{
		if (left.Kind == SyntaxKind.IdentifierName)
		{
			IdentifierNameSyntax node = (IdentifierNameSyntax)left;
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			BoundExpression result = BindIdentifier(node, invoked: false, instance);
			diagnostics.AddRangeAndFree(instance);
			return result;
		}
		return BindExpression(left, diagnostics);
	}

	internal static ImmutableArray<BoundMultilanguageExpression> BindMultiLanguageValue(CommaSeparatedIdentifierEqualsStringListSyntax syntax, CompilerFeatures compilerFeatures, DiagnosticBag diagnostics)
	{
		bool hasErrors;
		return BindMultiLanguageValue(syntax, compilerFeatures, diagnostics, out hasErrors);
	}

	internal static ImmutableArray<BoundMultilanguageExpression> BindMultiLanguageValue(CommaSeparatedIdentifierEqualsStringListSyntax syntax, CompilerFeatures compilerFeatures, DiagnosticBag diagnostics, out bool hasErrors)
	{
		hasErrors = false;
		PooledHashSet<int> instance = PooledHashSet<int>.GetInstance();
		if (compilerFeatures.ShouldGenerateTranslationFile())
		{
			diagnostics.Add(ErrorCode.WRN_ObsoleteMultilanguageSyntax, syntax.Parent.GetLocation());
		}
		ArrayBuilder<BoundMultilanguageExpression> instance2 = ArrayBuilder<BoundMultilanguageExpression>.GetInstance();
		ImmutableArray<BoundMultilanguageExpression> result;
		try
		{
			foreach (IdentifierEqualsStringSyntax value in syntax.Values)
			{
				bool flag = false;
				string text = value.Identifier.Text;
				if (!SyntaxFacts.IsIdentifierMultilanguageComment(text))
				{
					if (!MultiLanguageHelper.TryFindCulture(text, out var lcid))
					{
						Error(diagnostics, ErrorCode.ERR_InvalidLanguageId, value.Identifier.GetLocation(), text);
						flag = true;
					}
					if (instance.Contains(lcid))
					{
						diagnostics.Add(ErrorCode.ERR_LanguageSpecifiedMoreThanOnce, value.GetLocation(), value.Identifier.ValueText);
						continue;
					}
					instance.Add(lcid);
					TypeSymbol navType = GetNavType(NavTypeKind.Text, diagnostics, value);
					BoundLiteral languageId = CreateBoundStringLiteral(text, syntax, diagnostics);
					ConstantValue languageCultureId = ConstantValue.Create(lcid);
					ConstantValue languageValue = ConstantValue.Create(value.StringLiteral.Value.ValueText.UnquoteLiteral());
					BoundMultilanguageExpression item = new BoundMultilanguageExpression(syntax, languageId, languageCultureId, languageValue, navType, flag);
					instance2.Add(item);
					hasErrors |= flag;
				}
			}
		}
		finally
		{
			instance.Free();
			result = instance2.ToImmutableAndFree();
		}
		return result;
	}

	internal static BoundLabel BindLabel(LabelSyntax syntax, DiagnosticBag diagnostics)
	{
		ConstantValue comment = null;
		ConstantValue locked = null;
		ConstantValue maxLength = null;
		if (syntax.Properties != null)
		{
			SeparatedSyntaxList<IdentifierEqualsLiteralSyntax> values = syntax.Properties.Values;
			StringLiteralValueSyntax labelProperty = GetLabelProperty<StringLiteralValueSyntax>(values, diagnostics, "Comment");
			if (labelProperty != null)
			{
				string text = labelProperty.Value.ValueText.UnquoteLiteral();
				comment = ConstantValue.Create(text);
				CheckForIncorrectSyntax(diagnostics, labelProperty, text);
			}
			BooleanLiteralValueSyntax labelProperty2 = GetLabelProperty<BooleanLiteralValueSyntax>(values, diagnostics, "Locked");
			if (labelProperty2 != null)
			{
				locked = ConstantValue.Create(bool.Parse(labelProperty2.Value.ValueText.ToLowerInvariant()));
			}
			Int32SignedLiteralValueSyntax labelProperty3 = GetLabelProperty<Int32SignedLiteralValueSyntax>(values, diagnostics, "MaxLength");
			if (labelProperty3 != null)
			{
				maxLength = ConstantValue.Create(int.Parse(labelProperty3.ToString(), CultureInfo.InvariantCulture));
			}
			foreach (IdentifierEqualsLiteralSyntax item in values.Where((IdentifierEqualsLiteralSyntax p) => !p.Identifier.ValueText.OneOf(StringComparison.OrdinalIgnoreCase, LabelPropertyHelper.GetAllLabelProperties())))
			{
				diagnostics.Add(ErrorCode.ERR_InvalidLabelProperty, item.GetLocation(), LabelPropertyHelper.GetAllLabelPropertiesString());
			}
		}
		ConstantValue text2 = ConstantValue.Create(syntax.LabelText.Value.ValueText.UnquoteLiteral());
		return new BoundLabel(syntax, text2, comment, maxLength, locked, NavCorLib.NoneType, diagnostics.HasAnyErrors());
	}

	private static void CheckForIncorrectSyntax(DiagnosticBag diagnostics, StringLiteralValueSyntax commentProperty, string commentValue)
	{
		string text = null;
		if (commentValue.Contains("{Locked}"))
		{
			text = "Locked";
		}
		if (commentValue.Contains("MaxLength"))
		{
			text = "MaxLength";
		}
		if (!string.IsNullOrEmpty(text))
		{
			diagnostics.Add(ErrorCode.WRN_IncorrectLabelSyntax, commentProperty.GetLocation(), text);
		}
	}

	private static T GetLabelProperty<T>(SeparatedSyntaxList<IdentifierEqualsLiteralSyntax> list, DiagnosticBag diagnostics, string propertyName) where T : LiteralValueSyntax
	{
		IEnumerable<IdentifierEqualsLiteralSyntax> enumerable = list.Where((IdentifierEqualsLiteralSyntax p) => SemanticFacts.IsSameName(p.Identifier.ValueText, propertyName));
		if (enumerable.HasMoreThan(1))
		{
			foreach (IdentifierEqualsLiteralSyntax item in enumerable)
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateLabelProperty, item.GetLocation(), propertyName);
			}
			return null;
		}
		IdentifierEqualsLiteralSyntax identifierEqualsLiteralSyntax = enumerable.FirstOrDefault();
		if (identifierEqualsLiteralSyntax == null)
		{
			return null;
		}
		if (identifierEqualsLiteralSyntax.Literal is T result)
		{
			return result;
		}
		string text = SyntaxFacts.GetTypeFromSyntaxKind(identifierEqualsLiteralSyntax.Literal.Kind).ToString();
		string propertyReturnType = LabelPropertyHelper.GetPropertyReturnType(propertyName);
		diagnostics.Add(ErrorCode.ERR_InvalidTypeForSomething, identifierEqualsLiteralSyntax.Literal.GetLocation(), propertyName, propertyReturnType, text);
		return null;
	}

	private static SimpleNameSyntax GetSimpleNameSyntax(SyntaxNode syntax)
	{
		while (true)
		{
			switch (syntax.Kind)
			{
			case SyntaxKind.ParenthesizedExpression:
				break;
			case SyntaxKind.MemberAccessExpression:
				return ((MemberAccessExpressionSyntax)syntax).Name;
			default:
				return syntax as SimpleNameSyntax;
			}
			syntax = ((ParenthesizedExpressionSyntax)syntax).Expression;
		}
	}

	internal MethodGroupResolution ResolveMethodGroup(BoundMethodGroup node, AnalyzedArguments analyzedArguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		MethodGroupResolution result = ResolveMethodGroupInternal(node, analyzedArguments, ref useSiteDiagnostics);
		if (result.IsEmpty && !result.HasAnyErrors)
		{
			DebugAssertHelper.Assert(node.LookupError == null);
			throw ExceptionUtilities.SkippedForNow;
		}
		return result;
	}

	private MethodGroupResolution ResolveMethodGroupInternal(BoundMethodGroup methodGroup, AnalyzedArguments analyzedArguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		MethodGroupResolution result = ResolveDefaultMethodGroup(methodGroup, analyzedArguments, ref useSiteDiagnostics);
		_ = result.HasAnyApplicableMethod;
		return result;
	}

	private MethodGroupResolution ResolveDefaultMethodGroup(BoundMethodGroup node, AnalyzedArguments analyzedArguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ImmutableArray<MethodSymbol> methods = node.Methods;
		if (methods.Length == 0 && node.LookupSymbolOpt is MethodSymbol item)
		{
			methods = ImmutableArray.Create(item);
		}
		ImmutableArray<Diagnostic> diagnostics = ImmutableArray<Diagnostic>.Empty;
		if (node.LookupError != null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			Error(instance, node.LookupError, node.NameSyntax);
			diagnostics = instance.ToReadOnlyAndFree();
		}
		if (methods.Length == 0)
		{
			return new MethodGroupResolution(node.LookupSymbolOpt, node.ResultKind, diagnostics);
		}
		MethodGroup instance2 = MethodGroup.GetInstance();
		instance2.PopulateWithNonExtensionMethods(node.ReceiverOpt, methods, node.ResultKind, node.LookupError);
		if (node.LookupError != null)
		{
			return new MethodGroupResolution(instance2, diagnostics);
		}
		if (analyzedArguments == null)
		{
			return new MethodGroupResolution(instance2, diagnostics);
		}
		OverloadResolutionResult<MethodSymbol> instance3 = OverloadResolutionResult<MethodSymbol>.GetInstance();
		OverloadResolution.MethodInvocationOverloadResolution(instance2.Methods, analyzedArguments, node.ReceiverOpt, instance3, ref useSiteDiagnostics);
		return new MethodGroupResolution(instance2, null, instance3, analyzedArguments, instance2.ResultKind, diagnostics);
	}

	private static string GetName(ExpressionSyntax syntax)
	{
		return GetSimpleNameSyntax(syntax)?.Identifier.ValueText ?? string.Empty;
	}

	private BoundExpression ToBadExpression(BoundExpression expr, LookupResultKind resultKind = LookupResultKind.Empty)
	{
		DebugAssertHelper.Assert(expr != null);
		DebugAssertHelper.Assert(resultKind != LookupResultKind.Viable);
		TypeSymbol type = expr.Type;
		BoundKind kind = expr.Kind;
		if (expr.HasAnyErrors && (object)type != null)
		{
			return expr;
		}
		if (kind == BoundKind.BadExpression)
		{
			BoundBadExpression boundBadExpression = (BoundBadExpression)expr;
			return boundBadExpression.Update(resultKind, boundBadExpression.Symbols, boundBadExpression.ChildBoundNodes, type);
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		expr.GetExpressionSymbols(instance, null, this);
		return new BoundBadExpression(expr.Syntax, resultKind, instance.ToImmutableAndFree(), ImmutableArray.Create((BoundNode)expr), type ?? CreateErrorType());
	}

	internal TypeSymbol CreateErrorType(string name = "")
	{
		return new ExtendedErrorTypeSymbol(Compilation, name, null);
	}
}
