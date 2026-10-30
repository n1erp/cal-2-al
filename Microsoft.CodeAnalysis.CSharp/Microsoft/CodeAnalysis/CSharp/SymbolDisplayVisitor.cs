using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.SymbolDisplay;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal class SymbolDisplayVisitor : AbstractSymbolDisplayVisitor
{
	private readonly bool _escapeKeywordIdentifiers;

	private IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> _lazyAliasMap;

	private const string IL_KEYWORD_MODOPT = "modopt";

	private const string IL_KEYWORD_MODREQ = "modreq";

	private IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> AliasMap
	{
		get
		{
			IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> lazyAliasMap = _lazyAliasMap;
			if (lazyAliasMap != null)
			{
				return lazyAliasMap;
			}
			lazyAliasMap = CreateAliasMap();
			return Interlocked.CompareExchange(ref _lazyAliasMap, lazyAliasMap, null) ?? lazyAliasMap;
		}
	}

	internal SymbolDisplayVisitor(ArrayBuilder<SymbolDisplayPart> builder, SymbolDisplayFormat format, SemanticModel semanticModelOpt, int positionOpt)
		: base(builder, format, isFirstSymbolVisited: true, semanticModelOpt, positionOpt)
	{
		_escapeKeywordIdentifiers = format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
	}

	private SymbolDisplayVisitor(ArrayBuilder<SymbolDisplayPart> builder, SymbolDisplayFormat format, SemanticModel semanticModelOpt, int positionOpt, bool escapeKeywordIdentifiers, IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> aliasMap, bool isFirstSymbolVisited, bool inNamespaceOrType = false)
		: base(builder, format, isFirstSymbolVisited, semanticModelOpt, positionOpt, inNamespaceOrType)
	{
		_escapeKeywordIdentifiers = escapeKeywordIdentifiers;
		_lazyAliasMap = aliasMap;
	}

	protected override AbstractSymbolDisplayVisitor MakeNotFirstVisitor(bool inNamespaceOrType = false)
	{
		return new SymbolDisplayVisitor(builder, format, semanticModelOpt, positionOpt, _escapeKeywordIdentifiers, _lazyAliasMap, isFirstSymbolVisited: false, inNamespaceOrType);
	}

	internal SymbolDisplayPart CreatePart(SymbolDisplayPartKind kind, ISymbol symbol, string text)
	{
		text = ((text == null) ? "?" : ((_escapeKeywordIdentifiers && IsEscapable(kind)) ? EscapeIdentifier(text) : text));
		return new SymbolDisplayPart(kind, symbol, text);
	}

	private static bool IsEscapable(SymbolDisplayPartKind kind)
	{
		switch (kind)
		{
		case SymbolDisplayPartKind.AliasName:
		case SymbolDisplayPartKind.ClassName:
		case SymbolDisplayPartKind.DelegateName:
		case SymbolDisplayPartKind.EnumName:
		case SymbolDisplayPartKind.FieldName:
		case SymbolDisplayPartKind.InterfaceName:
		case SymbolDisplayPartKind.LocalName:
		case SymbolDisplayPartKind.MethodName:
		case SymbolDisplayPartKind.NamespaceName:
		case SymbolDisplayPartKind.ParameterName:
		case SymbolDisplayPartKind.PropertyName:
		case SymbolDisplayPartKind.StructName:
		case SymbolDisplayPartKind.TypeParameterName:
			return true;
		default:
			return false;
		}
	}

	private static string EscapeIdentifier(string identifier)
	{
		if (SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None)
		{
			return "@" + identifier;
		}
		return identifier;
	}

	public override void VisitAssembly(IAssemblySymbol symbol)
	{
		string text = ((format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameOnly) ? symbol.Identity.Name : symbol.Identity.GetDisplayName());
		builder.Add(CreatePart(SymbolDisplayPartKind.AssemblyName, symbol, text));
	}

	public override void VisitModule(IModuleSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.ModuleName, symbol, symbol.Name));
	}

	public override void VisitNamespace(INamespaceSymbol symbol)
	{
		if (IsMinimizing)
		{
			if (!TryAddAlias(symbol, builder))
			{
				MinimallyQualify(symbol);
			}
			return;
		}
		if (isFirstSymbolVisited && format.KindOptions.IncludesOption(SymbolDisplayKindOptions.IncludeNamespaceKeyword))
		{
			AddKeyword(SyntaxKind.NamespaceKeyword);
			AddSpace();
		}
		if (format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)
		{
			INamespaceSymbol containingNamespace = symbol.ContainingNamespace;
			if (ShouldVisitNamespace(containingNamespace))
			{
				containingNamespace.Accept(NotFirstVisitor);
				AddPunctuation(containingNamespace.IsGlobalNamespace ? SyntaxKind.ColonColonToken : SyntaxKind.DotToken);
			}
		}
		if (symbol.IsGlobalNamespace)
		{
			AddGlobalNamespace(symbol);
		}
		else
		{
			builder.Add(CreatePart(SymbolDisplayPartKind.NamespaceName, symbol, symbol.Name));
		}
	}

	private void AddGlobalNamespace(INamespaceSymbol globalNamespace)
	{
		switch (format.GlobalNamespaceStyle)
		{
		case SymbolDisplayGlobalNamespaceStyle.Included:
			if (isFirstSymbolVisited)
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.Text, globalNamespace, "<global namespace>"));
			}
			else
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, globalNamespace, SyntaxFacts.GetText(SyntaxKind.GlobalKeyword)));
			}
			break;
		case SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining:
			builder.Add(CreatePart(SymbolDisplayPartKind.Text, globalNamespace, "<global namespace>"));
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(format.GlobalNamespaceStyle);
		case SymbolDisplayGlobalNamespaceStyle.Omitted:
			break;
		}
	}

	public override void VisitLocal(ILocalSymbol symbol)
	{
		if (symbol.IsRef && format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeRef))
		{
			AddKeyword(SyntaxKind.RefKeyword);
			AddSpace();
			if (symbol.RefKind == RefKind.In)
			{
				AddKeyword(SyntaxKind.ReadOnlyKeyword);
				AddSpace();
			}
		}
		if (format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
		{
			symbol.Type.Accept(this);
			AddSpace();
		}
		builder.Add(CreatePart(SymbolDisplayPartKind.LocalName, symbol, symbol.Name));
		if (format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeConstantValue) && symbol.IsConst && symbol.HasConstantValue && CanAddConstant(symbol.Type, symbol.ConstantValue))
		{
			AddSpace();
			AddPunctuation(SyntaxKind.EqualsToken);
			AddSpace();
			AddConstantValue(symbol.Type, symbol.ConstantValue);
		}
	}

	public override void VisitDiscard(IDiscardSymbol symbol)
	{
		if (format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
		{
			symbol.Type.Accept(this);
			AddSpace();
		}
		builder.Add(CreatePart(SymbolDisplayPartKind.Punctuation, symbol, "_"));
	}

	public override void VisitRangeVariable(IRangeVariableSymbol symbol)
	{
		if (format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
		{
			ITypeSymbol rangeVariableType = GetRangeVariableType(symbol);
			if (rangeVariableType != null && rangeVariableType.TypeKind != TypeKind.Error)
			{
				rangeVariableType.Accept(this);
			}
			else
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.ErrorTypeName, rangeVariableType, "?"));
			}
			AddSpace();
		}
		builder.Add(CreatePart(SymbolDisplayPartKind.RangeVariableName, symbol, symbol.Name));
	}

	public override void VisitLabel(ILabelSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.LabelName, symbol, symbol.Name));
	}

	public override void VisitAlias(IAliasSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.AliasName, symbol, symbol.Name));
		if (format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
		{
			AddPunctuation(SyntaxKind.EqualsToken);
			symbol.Target.Accept(this);
		}
	}

	protected override void AddSpace()
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.Space, null, " "));
	}

	private void AddPunctuation(SyntaxKind punctuationKind)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.Punctuation, null, SyntaxFacts.GetText(punctuationKind)));
	}

	private void AddKeyword(SyntaxKind keywordKind)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, null, SyntaxFacts.GetText(keywordKind)));
	}

	private void AddAccessibilityIfRequired(ISymbol symbol)
	{
		INamedTypeSymbol containingType = symbol.ContainingType;
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeAccessibility) && (containingType == null || (containingType.TypeKind != TypeKind.Interface && (!IsEnumMember(symbol) & !IsLocalFunction(symbol)))))
		{
			AddAccessibility(symbol);
		}
	}

	private static bool IsLocalFunction(ISymbol symbol)
	{
		if (symbol.Kind != SymbolKind.Method)
		{
			return false;
		}
		return ((IMethodSymbol)symbol).MethodKind == MethodKind.LocalFunction;
	}

	private void AddAccessibility(ISymbol symbol)
	{
		switch (symbol.DeclaredAccessibility)
		{
		case Accessibility.Private:
			AddKeyword(SyntaxKind.PrivateKeyword);
			break;
		case Accessibility.Internal:
			AddKeyword(SyntaxKind.InternalKeyword);
			break;
		case Accessibility.ProtectedAndInternal:
			AddKeyword(SyntaxKind.PrivateKeyword);
			AddSpace();
			AddKeyword(SyntaxKind.ProtectedKeyword);
			break;
		case Accessibility.Protected:
			AddKeyword(SyntaxKind.ProtectedKeyword);
			break;
		case Accessibility.ProtectedOrInternal:
			AddKeyword(SyntaxKind.ProtectedKeyword);
			AddSpace();
			AddKeyword(SyntaxKind.InternalKeyword);
			break;
		case Accessibility.Public:
			AddKeyword(SyntaxKind.PublicKeyword);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.DeclaredAccessibility);
		}
		AddSpace();
	}

	private bool ShouldVisitNamespace(ISymbol containingSymbol)
	{
		if (!(containingSymbol is INamespaceSymbol namespaceSymbol))
		{
			return false;
		}
		if (format.TypeQualificationStyle != SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)
		{
			return false;
		}
		if (namespaceSymbol.IsGlobalNamespace)
		{
			return format.GlobalNamespaceStyle == SymbolDisplayGlobalNamespaceStyle.Included;
		}
		return true;
	}

	private bool IncludeNamedType(INamedTypeSymbol namedType)
	{
		if (namedType != null)
		{
			if (namedType.IsScriptClass)
			{
				return format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.IncludeScriptType);
			}
			return true;
		}
		return false;
	}

	private static bool IsEnumMember(ISymbol symbol)
	{
		if (symbol != null && symbol.Kind == SymbolKind.Field && symbol.ContainingType != null && symbol.ContainingType.TypeKind == TypeKind.Enum)
		{
			return symbol.Name != "value__";
		}
		return false;
	}

	public override void VisitField(IFieldSymbol symbol)
	{
		AddAccessibilityIfRequired(symbol);
		AddMemberModifiersIfRequired(symbol);
		AddFieldModifiersIfRequired(symbol);
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeType) && isFirstSymbolVisited && !IsEnumMember(symbol))
		{
			symbol.Type.Accept(NotFirstVisitor);
			AddSpace();
			AddCustomModifiersIfRequired(symbol.CustomModifiers);
		}
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeContainingType) && IncludeNamedType(symbol.ContainingType))
		{
			symbol.ContainingType.Accept(NotFirstVisitor);
			AddPunctuation(SyntaxKind.DotToken);
		}
		builder.Add(CreatePart(SymbolDisplayPartKind.FieldName, symbol, symbol.Name));
		if (isFirstSymbolVisited && format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeConstantValue) && symbol.IsConst && symbol.HasConstantValue && CanAddConstant(symbol.Type, symbol.ConstantValue))
		{
			AddSpace();
			AddPunctuation(SyntaxKind.EqualsToken);
			AddSpace();
			AddConstantValue(symbol.Type, symbol.ConstantValue, IsEnumMember(symbol));
		}
	}

	public override void VisitProperty(IPropertySymbol symbol)
	{
		AddAccessibilityIfRequired(symbol);
		AddMemberModifiersIfRequired(symbol);
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeType))
		{
			if (symbol.ReturnsByRef)
			{
				AddRefIfRequired();
			}
			else if (symbol.ReturnsByRefReadonly)
			{
				AddRefReadonlyIfRequired();
			}
			AddCustomModifiersIfRequired(symbol.RefCustomModifiers);
			symbol.Type.Accept(NotFirstVisitor);
			AddSpace();
			AddCustomModifiersIfRequired(symbol.TypeCustomModifiers);
		}
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeContainingType) && IncludeNamedType(symbol.ContainingType))
		{
			symbol.ContainingType.Accept(NotFirstVisitor);
			AddPunctuation(SyntaxKind.DotToken);
		}
		AddPropertyNameAndParameters(symbol);
		if (format.PropertyStyle == SymbolDisplayPropertyStyle.ShowReadWriteDescriptor)
		{
			AddSpace();
			AddPunctuation(SyntaxKind.OpenBraceToken);
			AddAccessor(symbol, symbol.GetMethod, SyntaxKind.GetKeyword);
			AddAccessor(symbol, symbol.SetMethod, SyntaxKind.SetKeyword);
			AddSpace();
			AddPunctuation(SyntaxKind.CloseBraceToken);
		}
	}

	private void AddPropertyNameAndParameters(IPropertySymbol symbol)
	{
		bool flag = symbol.Name.LastIndexOf('.') > 0;
		if (flag)
		{
			AddExplicitInterfaceIfRequired(symbol.ExplicitInterfaceImplementations);
		}
		if (symbol.IsIndexer)
		{
			AddKeyword(SyntaxKind.ThisKeyword);
		}
		else if (flag)
		{
			builder.Add(CreatePart(SymbolDisplayPartKind.PropertyName, symbol, ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(symbol.Name)));
		}
		else
		{
			builder.Add(CreatePart(SymbolDisplayPartKind.PropertyName, symbol, symbol.Name));
		}
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeParameters) && symbol.Parameters.Any())
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
			AddParametersIfRequired(hasThisParameter: false, isVarargs: false, symbol.Parameters);
			AddPunctuation(SyntaxKind.CloseBracketToken);
		}
	}

	public override void VisitEvent(IEventSymbol symbol)
	{
		AddAccessibilityIfRequired(symbol);
		AddMemberModifiersIfRequired(symbol);
		if (format.KindOptions.IncludesOption(SymbolDisplayKindOptions.IncludeMemberKeyword))
		{
			AddKeyword(SyntaxKind.EventKeyword);
			AddSpace();
		}
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeType))
		{
			symbol.Type.Accept(NotFirstVisitor);
			AddSpace();
		}
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeContainingType) && IncludeNamedType(symbol.ContainingType))
		{
			symbol.ContainingType.Accept(NotFirstVisitor);
			AddPunctuation(SyntaxKind.DotToken);
		}
		AddEventName(symbol);
	}

	private void AddEventName(IEventSymbol symbol)
	{
		if (symbol.Name.LastIndexOf('.') > 0)
		{
			AddExplicitInterfaceIfRequired(symbol.ExplicitInterfaceImplementations);
			builder.Add(CreatePart(SymbolDisplayPartKind.EventName, symbol, ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(symbol.Name)));
		}
		else
		{
			builder.Add(CreatePart(SymbolDisplayPartKind.EventName, symbol, symbol.Name));
		}
	}

	public override void VisitMethod(IMethodSymbol symbol)
	{
		if (symbol.MethodKind == MethodKind.AnonymousFunction)
		{
			builder.Add(CreatePart(SymbolDisplayPartKind.NumericLiteral, symbol, "lambda expression"));
			return;
		}
		if (symbol is SynthesizedGlobalMethodSymbol)
		{
			builder.Add(CreatePart(SymbolDisplayPartKind.NumericLiteral, symbol, symbol.Name));
			return;
		}
		if (symbol.IsExtensionMethod && format.ExtensionMethodStyle != SymbolDisplayExtensionMethodStyle.Default)
		{
			if (symbol.MethodKind == MethodKind.ReducedExtension && format.ExtensionMethodStyle == SymbolDisplayExtensionMethodStyle.StaticMethod)
			{
				symbol = symbol.GetConstructedReducedFrom();
			}
			else if (symbol.MethodKind != MethodKind.ReducedExtension && format.ExtensionMethodStyle == SymbolDisplayExtensionMethodStyle.InstanceMethod)
			{
				symbol = symbol.ReduceExtensionMethod(symbol.Parameters.First().Type) ?? symbol;
			}
		}
		if (symbol.ContainingType != null || symbol.ContainingSymbol is ITypeSymbol)
		{
			AddAccessibilityIfRequired(symbol);
			AddMemberModifiersIfRequired(symbol);
			if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeType))
			{
				switch (symbol.MethodKind)
				{
				case MethodKind.Conversion:
				case MethodKind.Destructor:
					if (!format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMethodNames))
					{
						break;
					}
					goto default;
				default:
					if (symbol.ReturnsByRef)
					{
						AddRefIfRequired();
					}
					else if (symbol.ReturnsByRefReadonly)
					{
						AddRefReadonlyIfRequired();
					}
					AddCustomModifiersIfRequired(symbol.RefCustomModifiers);
					if (symbol.ReturnsVoid)
					{
						AddKeyword(SyntaxKind.VoidKeyword);
					}
					else if (symbol.ReturnType != null)
					{
						symbol.ReturnType.Accept(NotFirstVisitor);
					}
					AddSpace();
					AddCustomModifiersIfRequired(symbol.ReturnTypeCustomModifiers);
					break;
				case MethodKind.Constructor:
				case MethodKind.StaticConstructor:
					break;
				}
			}
			if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeContainingType))
			{
				bool flag;
				ITypeSymbol typeSymbol;
				if (symbol.MethodKind == MethodKind.LocalFunction)
				{
					flag = false;
					typeSymbol = null;
				}
				else if (symbol.MethodKind == MethodKind.ReducedExtension)
				{
					typeSymbol = symbol.ReceiverType;
					flag = true;
				}
				else
				{
					typeSymbol = symbol.ContainingType;
					if (typeSymbol != null)
					{
						flag = IncludeNamedType(symbol.ContainingType);
					}
					else
					{
						typeSymbol = (ITypeSymbol)symbol.ContainingSymbol;
						flag = true;
					}
				}
				if (flag)
				{
					typeSymbol.Accept(NotFirstVisitor);
					AddPunctuation(SyntaxKind.DotToken);
				}
			}
		}
		bool flag2 = false;
		switch (symbol.MethodKind)
		{
		case MethodKind.DelegateInvoke:
		case MethodKind.Ordinary:
		case MethodKind.ReducedExtension:
		case MethodKind.LocalFunction:
			builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.Name));
			break;
		case MethodKind.PropertyGet:
		case MethodKind.PropertySet:
		{
			flag2 = true;
			IPropertySymbol propertySymbol = (IPropertySymbol)symbol.AssociatedSymbol;
			if (propertySymbol != null)
			{
				AddPropertyNameAndParameters(propertySymbol);
				AddPunctuation(SyntaxKind.DotToken);
				AddKeyword((symbol.MethodKind == MethodKind.PropertyGet) ? SyntaxKind.GetKeyword : SyntaxKind.SetKeyword);
				break;
			}
			goto case MethodKind.DelegateInvoke;
		}
		case MethodKind.EventAdd:
		case MethodKind.EventRemove:
		{
			flag2 = true;
			IEventSymbol eventSymbol = (IEventSymbol)symbol.AssociatedSymbol;
			if (eventSymbol != null)
			{
				AddEventName(eventSymbol);
				AddPunctuation(SyntaxKind.DotToken);
				AddKeyword((symbol.MethodKind == MethodKind.EventAdd) ? SyntaxKind.AddKeyword : SyntaxKind.RemoveKeyword);
				break;
			}
			goto case MethodKind.DelegateInvoke;
		}
		case MethodKind.Constructor:
		case MethodKind.StaticConstructor:
		{
			string text = ((format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMethodNames) || symbol.ContainingType == null || symbol.ContainingType.IsAnonymousType) ? symbol.Name : symbol.ContainingType.Name);
			builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, text));
			break;
		}
		case MethodKind.Destructor:
			if (format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMethodNames) || symbol.ContainingType == null)
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.Name));
				break;
			}
			AddPunctuation(SyntaxKind.TildeToken);
			builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.ContainingType.Name));
			break;
		case MethodKind.ExplicitInterfaceImplementation:
			AddExplicitInterfaceIfRequired(symbol.ExplicitInterfaceImplementations);
			builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, ExplicitInterfaceHelpers.GetMemberNameWithoutInterfaceName(symbol.Name)));
			break;
		case MethodKind.UserDefinedOperator:
		case MethodKind.BuiltinOperator:
			if (format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMethodNames))
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.MetadataName));
				break;
			}
			AddKeyword(SyntaxKind.OperatorKeyword);
			AddSpace();
			if (symbol.MetadataName == "op_True")
			{
				AddKeyword(SyntaxKind.TrueKeyword);
			}
			else if (symbol.MetadataName == "op_False")
			{
				AddKeyword(SyntaxKind.FalseKeyword);
			}
			else
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, SyntaxFacts.GetText(SyntaxFacts.GetOperatorKind(symbol.MetadataName))));
			}
			break;
		case MethodKind.Conversion:
			if (format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseMetadataMethodNames))
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, symbol.MetadataName));
				break;
			}
			if (symbol.MetadataName == "op_Explicit")
			{
				AddKeyword(SyntaxKind.ExplicitKeyword);
			}
			else if (symbol.MetadataName == "op_Implicit")
			{
				AddKeyword(SyntaxKind.ImplicitKeyword);
			}
			else
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.MethodName, symbol, SyntaxFacts.GetText(SyntaxFacts.GetOperatorKind(symbol.MetadataName))));
			}
			AddSpace();
			AddKeyword(SyntaxKind.OperatorKeyword);
			AddSpace();
			symbol.ReturnType.Accept(NotFirstVisitor);
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.MethodKind);
		}
		if (!flag2)
		{
			AddTypeArguments(symbol.TypeArguments);
			AddParameters(symbol);
			AddTypeParameterConstraints(symbol);
		}
	}

	private void AddTypeParameterConstraints(IMethodSymbol symbol)
	{
		if (format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeTypeConstraints))
		{
			AddTypeParameterConstraints(symbol.TypeArguments);
		}
	}

	private void AddParameters(IMethodSymbol symbol)
	{
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeParameters))
		{
			AddPunctuation(SyntaxKind.OpenParenToken);
			AddParametersIfRequired(symbol.IsExtensionMethod && symbol.MethodKind != MethodKind.ReducedExtension, symbol.IsVararg, symbol.Parameters);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
	}

	public override void VisitParameter(IParameterSymbol symbol)
	{
		bool flag = format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeType);
		bool flag2 = format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeName);
		bool flag3 = format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeOptionalBrackets);
		if (flag3 && symbol.IsOptional)
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
		}
		if (flag)
		{
			AddParameterRefKindIfRequired(symbol.RefKind);
			AddCustomModifiersIfRequired(symbol.RefCustomModifiers);
			if (symbol.IsParams && format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeParamsRefOut))
			{
				AddKeyword(SyntaxKind.ParamsKeyword);
				AddSpace();
			}
			symbol.Type.Accept(NotFirstVisitor);
			AddCustomModifiersIfRequired(symbol.CustomModifiers, leadingSpace: true, trailingSpace: false);
		}
		if (flag2 & flag)
		{
			AddSpace();
		}
		if (flag2)
		{
			SymbolDisplayPartKind kind = (symbol.IsThis ? SymbolDisplayPartKind.Keyword : SymbolDisplayPartKind.ParameterName);
			builder.Add(CreatePart(kind, symbol, symbol.Name));
			if (format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeDefaultValue) && symbol.HasExplicitDefaultValue && CanAddConstant(symbol.Type, symbol.ExplicitDefaultValue))
			{
				AddSpace();
				AddPunctuation(SyntaxKind.EqualsToken);
				AddSpace();
				AddConstantValue(symbol.Type, symbol.ExplicitDefaultValue);
			}
		}
		if (flag3 && symbol.IsOptional)
		{
			AddPunctuation(SyntaxKind.CloseBracketToken);
		}
	}

	private static bool CanAddConstant(ITypeSymbol type, object value)
	{
		if (type.TypeKind == TypeKind.Enum)
		{
			return true;
		}
		if (value == null)
		{
			return true;
		}
		if (!value.GetType().GetTypeInfo().IsPrimitive && !(value is string))
		{
			return value is decimal;
		}
		return true;
	}

	private void AddFieldModifiersIfRequired(IFieldSymbol symbol)
	{
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeModifiers) && !IsEnumMember(symbol))
		{
			if (symbol.IsConst)
			{
				AddKeyword(SyntaxKind.ConstKeyword);
				AddSpace();
			}
			if (symbol.IsReadOnly)
			{
				AddKeyword(SyntaxKind.ReadOnlyKeyword);
				AddSpace();
			}
			if (symbol.IsVolatile)
			{
				AddKeyword(SyntaxKind.VolatileKeyword);
				AddSpace();
			}
		}
	}

	private void AddMemberModifiersIfRequired(ISymbol symbol)
	{
		INamedTypeSymbol containingType = symbol.ContainingType;
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeModifiers) && (containingType == null || (containingType.TypeKind != TypeKind.Interface && !IsEnumMember(symbol) && !IsLocalFunction(symbol))))
		{
			bool flag = symbol is IFieldSymbol && ((IFieldSymbol)symbol).IsConst;
			if (symbol.IsStatic && !flag)
			{
				AddKeyword(SyntaxKind.StaticKeyword);
				AddSpace();
			}
			if (symbol.IsOverride)
			{
				AddKeyword(SyntaxKind.OverrideKeyword);
				AddSpace();
			}
			if (symbol.IsAbstract)
			{
				AddKeyword(SyntaxKind.AbstractKeyword);
				AddSpace();
			}
			if (symbol.IsSealed)
			{
				AddKeyword(SyntaxKind.SealedKeyword);
				AddSpace();
			}
			if (symbol.IsExtern)
			{
				AddKeyword(SyntaxKind.ExternKeyword);
				AddSpace();
			}
			if (symbol.IsVirtual)
			{
				AddKeyword(SyntaxKind.VirtualKeyword);
				AddSpace();
			}
		}
	}

	private void AddParametersIfRequired(bool hasThisParameter, bool isVarargs, ImmutableArray<IParameterSymbol> parameters)
	{
		if (format.ParameterOptions == SymbolDisplayParameterOptions.None)
		{
			return;
		}
		bool flag = true;
		if (!parameters.IsDefault)
		{
			foreach (IParameterSymbol item in parameters)
			{
				if (!flag)
				{
					AddPunctuation(SyntaxKind.CommaToken);
					AddSpace();
				}
				else if (hasThisParameter && format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeExtensionThis))
				{
					AddKeyword(SyntaxKind.ThisKeyword);
					AddSpace();
				}
				flag = false;
				item.Accept(NotFirstVisitor);
			}
		}
		if (isVarargs)
		{
			if (!flag)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
			AddKeyword(SyntaxKind.ArgListKeyword);
		}
	}

	private void AddAccessor(ISymbol property, IMethodSymbol method, SyntaxKind keyword)
	{
		if (method != null)
		{
			AddSpace();
			if (method.DeclaredAccessibility != property.DeclaredAccessibility)
			{
				AddAccessibility(method);
			}
			AddKeyword(keyword);
			AddPunctuation(SyntaxKind.SemicolonToken);
		}
	}

	private void AddExplicitInterfaceIfRequired<T>(ImmutableArray<T> implementedMethods) where T : ISymbol
	{
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeExplicitInterface) && !implementedMethods.IsEmpty)
		{
			INamedTypeSymbol containingType = implementedMethods[0].ContainingType;
			if (containingType != null)
			{
				containingType.Accept(NotFirstVisitor);
				AddPunctuation(SyntaxKind.DotToken);
			}
		}
	}

	private void AddCustomModifiersIfRequired(ImmutableArray<CustomModifier> customModifiers, bool leadingSpace = false, bool trailingSpace = true)
	{
		if (!format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.IncludeCustomModifiers) || customModifiers.IsEmpty)
		{
			return;
		}
		bool flag = true;
		foreach (CustomModifier item in customModifiers)
		{
			if (!flag | leadingSpace)
			{
				AddSpace();
			}
			flag = false;
			builder.Add(CreatePart((SymbolDisplayPartKind)29, null, item.IsOptional ? "modopt" : "modreq"));
			AddPunctuation(SyntaxKind.OpenParenToken);
			item.Modifier.Accept(NotFirstVisitor);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
		if (trailingSpace)
		{
			AddSpace();
		}
	}

	private void AddRefIfRequired()
	{
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeRef))
		{
			AddKeyword(SyntaxKind.RefKeyword);
			AddSpace();
		}
	}

	private void AddRefReadonlyIfRequired()
	{
		if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeRef))
		{
			AddKeyword(SyntaxKind.RefKeyword);
			AddSpace();
			AddKeyword(SyntaxKind.ReadOnlyKeyword);
			AddSpace();
		}
	}

	private void AddParameterRefKindIfRequired(RefKind refKind)
	{
		if (format.ParameterOptions.IncludesOption(SymbolDisplayParameterOptions.IncludeParamsRefOut))
		{
			switch (refKind)
			{
			case RefKind.Out:
				AddKeyword(SyntaxKind.OutKeyword);
				AddSpace();
				break;
			case RefKind.Ref:
				AddKeyword(SyntaxKind.RefKeyword);
				AddSpace();
				break;
			case RefKind.In:
				AddKeyword(SyntaxKind.InKeyword);
				AddSpace();
				break;
			}
		}
	}

	public override void VisitArrayType(IArrayTypeSymbol symbol)
	{
		if (TryAddAlias(symbol, builder))
		{
			return;
		}
		if (format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.ReverseArrayRankSpecifiers))
		{
			symbol.ElementType.Accept(this);
			AddArrayRank(symbol);
			return;
		}
		ITypeSymbol elementType = symbol.ElementType;
		while (elementType.Kind == SymbolKind.ArrayType)
		{
			elementType = ((IArrayTypeSymbol)elementType).ElementType;
		}
		elementType.Accept(NotFirstVisitor);
		for (IArrayTypeSymbol arrayTypeSymbol = symbol; arrayTypeSymbol != null; arrayTypeSymbol = arrayTypeSymbol.ElementType as IArrayTypeSymbol)
		{
			if (!isFirstSymbolVisited)
			{
				AddCustomModifiersIfRequired(arrayTypeSymbol.CustomModifiers, leadingSpace: true);
			}
			AddArrayRank(arrayTypeSymbol);
		}
	}

	private void AddArrayRank(IArrayTypeSymbol symbol)
	{
		bool flag = format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays);
		AddPunctuation(SyntaxKind.OpenBracketToken);
		if (symbol.Rank > 1)
		{
			if (flag)
			{
				AddPunctuation(SyntaxKind.AsteriskToken);
			}
		}
		else if (symbol is ArrayTypeSymbol { IsSZArray: false })
		{
			AddPunctuation(SyntaxKind.AsteriskToken);
		}
		for (int i = 0; i < symbol.Rank - 1; i++)
		{
			AddPunctuation(SyntaxKind.CommaToken);
			if (flag)
			{
				AddPunctuation(SyntaxKind.AsteriskToken);
			}
		}
		AddPunctuation(SyntaxKind.CloseBracketToken);
	}

	public override void VisitPointerType(IPointerTypeSymbol symbol)
	{
		symbol.PointedAtType.Accept(NotFirstVisitor);
		if (!isFirstSymbolVisited)
		{
			AddCustomModifiersIfRequired(symbol.CustomModifiers, leadingSpace: true);
		}
		AddPunctuation(SyntaxKind.AsteriskToken);
	}

	public override void VisitTypeParameter(ITypeParameterSymbol symbol)
	{
		if (isFirstSymbolVisited)
		{
			AddTypeParameterVarianceIfRequired(symbol);
		}
		builder.Add(CreatePart(SymbolDisplayPartKind.TypeParameterName, symbol, symbol.Name));
	}

	public override void VisitDynamicType(IDynamicTypeSymbol symbol)
	{
		builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, symbol, symbol.Name));
	}

	public override void VisitNamedType(INamedTypeSymbol symbol)
	{
		if ((IsMinimizing && TryAddAlias(symbol, builder)) || (format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.UseSpecialTypes) && AddSpecialTypeKeyword(symbol)))
		{
			return;
		}
		if (!format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.ExpandNullable) && ITypeSymbolHelpers.IsNullableType(symbol) && !symbol.IsDefinition && symbol.TypeArguments[0].TypeKind != TypeKind.Pointer)
		{
			symbol.TypeArguments[0].Accept(NotFirstVisitor);
			AddCustomModifiersIfRequired(symbol.GetTypeArgumentCustomModifiers(0), leadingSpace: true, trailingSpace: false);
			AddPunctuation(SyntaxKind.QuestionToken);
			return;
		}
		if (IsMinimizing || symbol.IsTupleType)
		{
			MinimallyQualify(symbol);
			return;
		}
		AddTypeKind(symbol);
		if (CanShowDelegateSignature(symbol) && format.DelegateStyle == SymbolDisplayDelegateStyle.NameAndSignature)
		{
			IMethodSymbol delegateInvokeMethod = symbol.DelegateInvokeMethod;
			if (delegateInvokeMethod.ReturnsByRef)
			{
				AddRefIfRequired();
			}
			else if (delegateInvokeMethod.ReturnsByRefReadonly)
			{
				AddRefReadonlyIfRequired();
			}
			if (delegateInvokeMethod.ReturnsVoid)
			{
				AddKeyword(SyntaxKind.VoidKeyword);
			}
			else
			{
				symbol.DelegateInvokeMethod.ReturnType.Accept(NotFirstVisitor);
			}
			AddSpace();
		}
		ISymbol containingSymbol = symbol.ContainingSymbol;
		if (ShouldVisitNamespace(containingSymbol))
		{
			INamespaceSymbol namespaceSymbol = (INamespaceSymbol)containingSymbol;
			if (!namespaceSymbol.IsGlobalNamespace || symbol.TypeKind != TypeKind.Error)
			{
				namespaceSymbol.Accept(NotFirstVisitor);
				AddPunctuation(namespaceSymbol.IsGlobalNamespace ? SyntaxKind.ColonColonToken : SyntaxKind.DotToken);
			}
		}
		if ((format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameAndContainingTypes || format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces) && IncludeNamedType(symbol.ContainingType))
		{
			symbol.ContainingType.Accept(NotFirstVisitor);
			AddPunctuation(SyntaxKind.DotToken);
		}
		AddNameAndTypeArgumentsOrParameters(symbol);
	}

	private void AddNameAndTypeArgumentsOrParameters(INamedTypeSymbol symbol)
	{
		if (symbol.IsAnonymousType)
		{
			AddAnonymousTypeName(symbol);
			return;
		}
		if (symbol.IsTupleType)
		{
			if (HasNonDefaultTupleElements(symbol) || CanUseTupleTypeName(symbol))
			{
				AddTupleTypeName(symbol);
				return;
			}
			symbol = symbol.TupleUnderlyingType;
		}
		string text = null;
		if (symbol is NoPiaIllegalGenericInstantiationSymbol noPiaIllegalGenericInstantiationSymbol)
		{
			symbol = noPiaIllegalGenericInstantiationSymbol.UnderlyingSymbol;
		}
		else if (symbol is NoPiaAmbiguousCanonicalTypeSymbol noPiaAmbiguousCanonicalTypeSymbol)
		{
			symbol = noPiaAmbiguousCanonicalTypeSymbol.FirstCandidate;
		}
		else if (symbol is NoPiaMissingCanonicalTypeSymbol noPiaMissingCanonicalTypeSymbol)
		{
			text = noPiaMissingCanonicalTypeSymbol.FullTypeName;
		}
		SymbolDisplayPartKind partKind = GetPartKind(symbol);
		if (text == null)
		{
			text = symbol.Name;
		}
		if (format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.UseErrorTypeSymbolName) && partKind == SymbolDisplayPartKind.ErrorTypeName && string.IsNullOrEmpty(text))
		{
			builder.Add(CreatePart(partKind, symbol, "?"));
		}
		else
		{
			text = RemoveAttributeSufficeIfNecessary(symbol, text);
			builder.Add(CreatePart(partKind, symbol, text));
		}
		if (format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.UseArityForGenericTypes))
		{
			if (((NamedTypeSymbol)symbol).MangleName)
			{
				builder.Add(CreatePart((SymbolDisplayPartKind)28, null, MetadataHelpers.GetAritySuffix(symbol.Arity)));
			}
		}
		else if (symbol.Arity > 0 && format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeTypeParameters))
		{
			if (symbol is UnsupportedMetadataTypeSymbol || symbol is MissingMetadataTypeSymbol || symbol.IsUnboundGenericType)
			{
				AddPunctuation(SyntaxKind.LessThanToken);
				for (int i = 0; i < symbol.Arity - 1; i++)
				{
					AddPunctuation(SyntaxKind.CommaToken);
				}
				AddPunctuation(SyntaxKind.GreaterThanToken);
			}
			else
			{
				AddTypeArguments(symbol.TypeArguments, symbol);
				AddDelegateParameters(symbol);
				AddTypeParameterConstraints(symbol.TypeArguments);
			}
		}
		else
		{
			AddDelegateParameters(symbol);
		}
		if (symbol.OriginalDefinition is MissingMetadataTypeSymbol && format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.FlagMissingMetadataTypes))
		{
			AddPunctuation(SyntaxKind.OpenBracketToken);
			builder.Add(CreatePart((SymbolDisplayPartKind)29, symbol, "missing"));
			AddPunctuation(SyntaxKind.CloseBracketToken);
		}
	}

	private void AddDelegateParameters(INamedTypeSymbol symbol)
	{
		if (CanShowDelegateSignature(symbol) && (format.DelegateStyle == SymbolDisplayDelegateStyle.NameAndParameters || format.DelegateStyle == SymbolDisplayDelegateStyle.NameAndSignature))
		{
			IMethodSymbol delegateInvokeMethod = symbol.DelegateInvokeMethod;
			AddPunctuation(SyntaxKind.OpenParenToken);
			AddParametersIfRequired(hasThisParameter: false, delegateInvokeMethod.IsVararg, delegateInvokeMethod.Parameters);
			AddPunctuation(SyntaxKind.CloseParenToken);
		}
	}

	private void AddAnonymousTypeName(INamedTypeSymbol symbol)
	{
		string text = string.Join(", ", symbol.GetMembers().OfType<IPropertySymbol>().Select(CreateAnonymousTypeMember));
		if (text.Length == 0)
		{
			builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.ClassName, symbol, "<empty anonymous type>"));
			return;
		}
		string text2 = "<anonymous type: " + text + ">";
		builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.ClassName, symbol, text2));
	}

	private bool CanUseTupleTypeName(INamedTypeSymbol tupleSymbol)
	{
		INamedTypeSymbol tupleUnderlyingType = tupleSymbol.TupleUnderlyingType;
		if (tupleUnderlyingType.Arity == 1)
		{
			return false;
		}
		while (tupleUnderlyingType.Arity == 8)
		{
			tupleSymbol = (INamedTypeSymbol)tupleUnderlyingType.TypeArguments[7];
			if (HasNonDefaultTupleElements(tupleSymbol))
			{
				return false;
			}
			tupleUnderlyingType = tupleSymbol.TupleUnderlyingType;
		}
		return true;
	}

	private static bool HasNonDefaultTupleElements(INamedTypeSymbol tupleSymbol)
	{
		return tupleSymbol.TupleElements.Any((IFieldSymbol e) => !e.IsDefaultTupleElement());
	}

	private void AddTupleTypeName(INamedTypeSymbol symbol)
	{
		ImmutableArray<IFieldSymbol> tupleElements = symbol.TupleElements;
		AddPunctuation(SyntaxKind.OpenParenToken);
		for (int i = 0; i < tupleElements.Length; i++)
		{
			IFieldSymbol fieldSymbol = tupleElements[i];
			if (i != 0)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
			fieldSymbol.Type.Accept(NotFirstVisitor);
			if (!fieldSymbol.IsImplicitlyDeclared)
			{
				AddSpace();
				builder.Add(CreatePart(SymbolDisplayPartKind.FieldName, symbol, fieldSymbol.Name));
			}
		}
		AddPunctuation(SyntaxKind.CloseParenToken);
	}

	private string CreateAnonymousTypeMember(IPropertySymbol property)
	{
		return property.Type.ToDisplayString(format) + " " + property.Name;
	}

	private bool CanShowDelegateSignature(INamedTypeSymbol symbol)
	{
		if (isFirstSymbolVisited && symbol.TypeKind == TypeKind.Delegate && format.DelegateStyle != SymbolDisplayDelegateStyle.NameOnly)
		{
			return symbol.DelegateInvokeMethod != null;
		}
		return false;
	}

	private static SymbolDisplayPartKind GetPartKind(INamedTypeSymbol symbol)
	{
		switch (symbol.TypeKind)
		{
		case TypeKind.Class:
		case TypeKind.Module:
		case TypeKind.Submission:
			return SymbolDisplayPartKind.ClassName;
		case TypeKind.Delegate:
			return SymbolDisplayPartKind.DelegateName;
		case TypeKind.Enum:
			return SymbolDisplayPartKind.EnumName;
		case TypeKind.Error:
			return SymbolDisplayPartKind.ErrorTypeName;
		case TypeKind.Interface:
			return SymbolDisplayPartKind.InterfaceName;
		case TypeKind.Struct:
			return SymbolDisplayPartKind.StructName;
		default:
			throw ExceptionUtilities.UnexpectedValue(symbol.TypeKind);
		}
	}

	private bool AddSpecialTypeKeyword(INamedTypeSymbol symbol)
	{
		string specialTypeName = GetSpecialTypeName(symbol.SpecialType);
		if (specialTypeName == null)
		{
			return false;
		}
		builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, symbol, specialTypeName));
		return true;
	}

	private static string GetSpecialTypeName(SpecialType specialType)
	{
		return specialType switch
		{
			SpecialType.System_Void => "void", 
			SpecialType.System_SByte => "sbyte", 
			SpecialType.System_Int16 => "short", 
			SpecialType.System_Int32 => "int", 
			SpecialType.System_Int64 => "long", 
			SpecialType.System_Byte => "byte", 
			SpecialType.System_UInt16 => "ushort", 
			SpecialType.System_UInt32 => "uint", 
			SpecialType.System_UInt64 => "ulong", 
			SpecialType.System_Single => "float", 
			SpecialType.System_Double => "double", 
			SpecialType.System_Decimal => "decimal", 
			SpecialType.System_Char => "char", 
			SpecialType.System_Boolean => "bool", 
			SpecialType.System_String => "string", 
			SpecialType.System_Object => "object", 
			_ => null, 
		};
	}

	private void AddTypeKind(INamedTypeSymbol symbol)
	{
		if (!isFirstSymbolVisited || !format.KindOptions.IncludesOption(SymbolDisplayKindOptions.IncludeTypeKeyword))
		{
			return;
		}
		if (symbol.IsAnonymousType)
		{
			builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.AnonymousTypeIndicator, null, "AnonymousType"));
			AddSpace();
			return;
		}
		if (symbol.IsTupleType)
		{
			builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.AnonymousTypeIndicator, null, "Tuple"));
			AddSpace();
			return;
		}
		switch (symbol.TypeKind)
		{
		case TypeKind.Class:
		case TypeKind.Module:
			AddKeyword(SyntaxKind.ClassKeyword);
			AddSpace();
			break;
		case TypeKind.Enum:
			AddKeyword(SyntaxKind.EnumKeyword);
			AddSpace();
			break;
		case TypeKind.Delegate:
			AddKeyword(SyntaxKind.DelegateKeyword);
			AddSpace();
			break;
		case TypeKind.Interface:
			AddKeyword(SyntaxKind.InterfaceKeyword);
			AddSpace();
			break;
		case TypeKind.Struct:
			if (symbol is NamedTypeSymbol namedTypeSymbol)
			{
				if (namedTypeSymbol.IsReadOnly)
				{
					AddKeyword(SyntaxKind.ReadOnlyKeyword);
					AddSpace();
				}
				if (namedTypeSymbol.IsByRefLikeType)
				{
					AddKeyword(SyntaxKind.RefKeyword);
					AddSpace();
				}
			}
			AddKeyword(SyntaxKind.StructKeyword);
			AddSpace();
			break;
		case TypeKind.Dynamic:
		case TypeKind.Error:
		case TypeKind.Pointer:
			break;
		}
	}

	private void AddTypeParameterVarianceIfRequired(ITypeParameterSymbol symbol)
	{
		if (format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeVariance))
		{
			switch (symbol.Variance)
			{
			case VarianceKind.In:
				AddKeyword(SyntaxKind.InKeyword);
				AddSpace();
				break;
			case VarianceKind.Out:
				AddKeyword(SyntaxKind.OutKeyword);
				AddSpace();
				break;
			}
		}
	}

	private void AddTypeArguments(ImmutableArray<ITypeSymbol> typeArguments, INamedTypeSymbol modifiersSourceOpt = null)
	{
		if (typeArguments.Length <= 0 || !format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeTypeParameters))
		{
			return;
		}
		AddPunctuation(SyntaxKind.LessThanToken);
		bool flag = true;
		for (int i = 0; i < typeArguments.Length; i++)
		{
			ITypeSymbol typeSymbol = typeArguments[i];
			if (!flag)
			{
				AddPunctuation(SyntaxKind.CommaToken);
				AddSpace();
			}
			flag = false;
			if (typeSymbol.Kind == SymbolKind.TypeParameter)
			{
				ITypeParameterSymbol typeParameterSymbol = (ITypeParameterSymbol)typeSymbol;
				AddTypeParameterVarianceIfRequired(typeParameterSymbol);
				typeParameterSymbol.Accept(NotFirstVisitor);
			}
			else
			{
				typeSymbol.Accept(NotFirstVisitorNamespaceOrType);
			}
			if (modifiersSourceOpt != null)
			{
				AddCustomModifiersIfRequired(modifiersSourceOpt.GetTypeArgumentCustomModifiers(i), leadingSpace: true, trailingSpace: false);
			}
		}
		AddPunctuation(SyntaxKind.GreaterThanToken);
	}

	private static bool TypeParameterHasConstraints(ITypeParameterSymbol typeParam)
	{
		if (typeParam.ConstraintTypes.IsEmpty && !typeParam.HasConstructorConstraint && !typeParam.HasReferenceTypeConstraint)
		{
			return typeParam.HasValueTypeConstraint;
		}
		return true;
	}

	private void AddTypeParameterConstraints(ImmutableArray<ITypeSymbol> typeArguments)
	{
		if (!isFirstSymbolVisited || !format.GenericsOptions.IncludesOption(SymbolDisplayGenericsOptions.IncludeTypeConstraints))
		{
			return;
		}
		foreach (ITypeSymbol item in typeArguments)
		{
			if (item.Kind != SymbolKind.TypeParameter)
			{
				continue;
			}
			ITypeParameterSymbol typeParameterSymbol = (ITypeParameterSymbol)item;
			if (!TypeParameterHasConstraints(typeParameterSymbol))
			{
				continue;
			}
			AddSpace();
			AddKeyword(SyntaxKind.WhereKeyword);
			AddSpace();
			typeParameterSymbol.Accept(NotFirstVisitor);
			AddSpace();
			AddPunctuation(SyntaxKind.ColonToken);
			AddSpace();
			bool flag = false;
			if (typeParameterSymbol.HasReferenceTypeConstraint)
			{
				AddKeyword(SyntaxKind.ClassKeyword);
				flag = true;
			}
			else if (typeParameterSymbol.HasUnmanagedTypeConstraint)
			{
				builder.Add(new SymbolDisplayPart(SymbolDisplayPartKind.Keyword, null, "unmanaged"));
				flag = true;
			}
			else if (typeParameterSymbol.HasValueTypeConstraint)
			{
				AddKeyword(SyntaxKind.StructKeyword);
				flag = true;
			}
			foreach (ITypeSymbol constraintType in typeParameterSymbol.ConstraintTypes)
			{
				if (flag)
				{
					AddPunctuation(SyntaxKind.CommaToken);
					AddSpace();
				}
				constraintType.Accept(NotFirstVisitor);
				flag = true;
			}
			if (typeParameterSymbol.HasConstructorConstraint)
			{
				if (flag)
				{
					AddPunctuation(SyntaxKind.CommaToken);
					AddSpace();
				}
				AddKeyword(SyntaxKind.NewKeyword);
				AddPunctuation(SyntaxKind.OpenParenToken);
				AddPunctuation(SyntaxKind.CloseParenToken);
			}
		}
	}

	private void AddConstantValue(ITypeSymbol type, object constantValue, bool preferNumericValueOrExpandedFlagsForEnum = false)
	{
		if (constantValue != null)
		{
			AddNonNullConstantValue(type, constantValue, preferNumericValueOrExpandedFlagsForEnum);
			return;
		}
		if (type.IsReferenceType || type.TypeKind == TypeKind.Pointer || ITypeSymbolHelpers.IsNullableType(type))
		{
			AddKeyword(SyntaxKind.NullKeyword);
			return;
		}
		AddKeyword(SyntaxKind.DefaultKeyword);
		AddPunctuation(SyntaxKind.OpenParenToken);
		type.Accept(NotFirstVisitor);
		AddPunctuation(SyntaxKind.CloseParenToken);
	}

	protected override void AddExplicitlyCastedLiteralValue(INamedTypeSymbol namedType, SpecialType type, object value)
	{
		AddPunctuation(SyntaxKind.OpenParenToken);
		namedType.Accept(NotFirstVisitor);
		AddPunctuation(SyntaxKind.CloseParenToken);
		AddLiteralValue(type, value);
	}

	protected override void AddLiteralValue(SpecialType type, object value)
	{
		string text = SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false);
		SymbolDisplayPartKind kind = SymbolDisplayPartKind.NumericLiteral;
		switch (type)
		{
		case SpecialType.System_Boolean:
			kind = SymbolDisplayPartKind.Keyword;
			break;
		case SpecialType.System_Char:
		case SpecialType.System_String:
			kind = SymbolDisplayPartKind.StringLiteral;
			break;
		}
		builder.Add(CreatePart(kind, null, text));
	}

	protected override void AddBitwiseOr()
	{
		AddPunctuation(SyntaxKind.BarToken);
	}

	private bool TryAddAlias(INamespaceOrTypeSymbol symbol, ArrayBuilder<SymbolDisplayPart> builder)
	{
		IAliasSymbol aliasSymbol = GetAliasSymbol(symbol);
		if (aliasSymbol != null)
		{
			string name = aliasSymbol.Name;
			ImmutableArray<ISymbol> immutableArray = semanticModelOpt.LookupNamespacesAndTypes(positionOpt, null, name);
			if (immutableArray.Length == 1 && immutableArray[0] is AliasSymbol && aliasSymbol.Target.Equals(symbol))
			{
				builder.Add(CreatePart(SymbolDisplayPartKind.AliasName, aliasSymbol, name));
				return true;
			}
		}
		return false;
	}

	protected override bool ShouldRestrictMinimallyQualifyLookupToNamespacesAndTypes()
	{
		SyntaxToken token = semanticModelOpt.SyntaxTree.GetRoot().FindToken(positionOpt);
		if (!SyntaxFacts.IsInNamespaceOrTypeContext(token.Parent as ExpressionSyntax) && !token.IsKind(SyntaxKind.NewKeyword))
		{
			return inNamespaceOrType;
		}
		return true;
	}

	private void MinimallyQualify(INamespaceSymbol symbol)
	{
		if (symbol.IsGlobalNamespace)
		{
			return;
		}
		ImmutableArray<ISymbol> immutableArray = (ShouldRestrictMinimallyQualifyLookupToNamespacesAndTypes() ? semanticModelOpt.LookupNamespacesAndTypes(positionOpt, null, symbol.Name) : semanticModelOpt.LookupSymbols(positionOpt, null, symbol.Name));
		ISymbol symbol2 = immutableArray.OfType<ISymbol>().FirstOrDefault();
		if (immutableArray.Length != 1 || symbol2 == null || !symbol2.Equals(symbol))
		{
			INamespaceSymbol namespaceSymbol = ((symbol.ContainingNamespace == null) ? null : semanticModelOpt.Compilation.GetCompilationNamespace(symbol.ContainingNamespace));
			if (namespaceSymbol != null)
			{
				if (namespaceSymbol.IsGlobalNamespace)
				{
					if (format.GlobalNamespaceStyle == SymbolDisplayGlobalNamespaceStyle.Included)
					{
						AddGlobalNamespace(namespaceSymbol);
						AddPunctuation(SyntaxKind.ColonColonToken);
					}
				}
				else
				{
					namespaceSymbol.Accept(NotFirstVisitor);
					AddPunctuation(SyntaxKind.DotToken);
				}
			}
		}
		builder.Add(CreatePart(SymbolDisplayPartKind.NamespaceName, symbol, symbol.Name));
	}

	private void MinimallyQualify(INamedTypeSymbol symbol)
	{
		if (!symbol.IsAnonymousType && !symbol.IsTupleType && !NameBoundSuccessfullyToSameSymbol(symbol))
		{
			if (IncludeNamedType(symbol.ContainingType))
			{
				symbol.ContainingType.Accept(NotFirstVisitor);
				AddPunctuation(SyntaxKind.DotToken);
			}
			else
			{
				INamespaceSymbol namespaceSymbol = ((symbol.ContainingNamespace == null) ? null : semanticModelOpt.Compilation.GetCompilationNamespace(symbol.ContainingNamespace));
				if (namespaceSymbol != null)
				{
					if (namespaceSymbol.IsGlobalNamespace)
					{
						if (symbol.TypeKind != TypeKind.Error)
						{
							AddKeyword(SyntaxKind.GlobalKeyword);
							AddPunctuation(SyntaxKind.ColonColonToken);
						}
					}
					else
					{
						namespaceSymbol.Accept(NotFirstVisitor);
						AddPunctuation(SyntaxKind.DotToken);
					}
				}
			}
		}
		AddNameAndTypeArgumentsOrParameters(symbol);
	}

	private IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> CreateAliasMap()
	{
		if (!IsMinimizing)
		{
			return SpecializedCollections.EmptyDictionary<INamespaceOrTypeSymbol, IAliasSymbol>();
		}
		SemanticModel semanticModel;
		int originalPositionForSpeculation;
		if (semanticModelOpt.IsSpeculativeSemanticModel)
		{
			semanticModel = semanticModelOpt.ParentModel;
			originalPositionForSpeculation = semanticModelOpt.OriginalPositionForSpeculation;
		}
		else
		{
			semanticModel = semanticModelOpt;
			originalPositionForSpeculation = positionOpt;
		}
		SyntaxNode parent = semanticModel.SyntaxTree.GetRoot().FindToken(originalPositionForSpeculation).Parent;
		UsingDirectiveSyntax ancestorOrThis = GetAncestorOrThis<UsingDirectiveSyntax>(parent);
		if (ancestorOrThis != null)
		{
			parent = ancestorOrThis.Parent.Parent;
		}
		IEnumerable<IAliasSymbol> enumerable = from u in GetAncestorsOrThis<NamespaceDeclarationSyntax>(parent).SelectMany((NamespaceDeclarationSyntax n) => n.Usings).Concat(GetAncestorsOrThis<CompilationUnitSyntax>(parent).SelectMany((CompilationUnitSyntax c) => c.Usings))
			where u.Alias != null
			select semanticModel.GetDeclaredSymbol(u) into u
			where u != null
			select u;
		ImmutableDictionary<INamespaceOrTypeSymbol, IAliasSymbol>.Builder builder = ImmutableDictionary.CreateBuilder<INamespaceOrTypeSymbol, IAliasSymbol>();
		foreach (IAliasSymbol item in enumerable)
		{
			if (!builder.ContainsKey(item.Target))
			{
				builder.Add(item.Target, item);
			}
		}
		return builder.ToImmutable();
	}

	private ITypeSymbol GetRangeVariableType(IRangeVariableSymbol symbol)
	{
		ITypeSymbol result = null;
		if (IsMinimizing && !symbol.Locations.IsEmpty)
		{
			Location location = symbol.Locations.First();
			if (location.IsInSource && location.SourceTree == semanticModelOpt.SyntaxTree)
			{
				SyntaxToken token = location.SourceTree.GetRoot().FindToken(positionOpt);
				QueryBodySyntax queryBody = GetQueryBody(token);
				if (queryBody != null)
				{
					IdentifierNameSyntax expression = SyntaxFactory.IdentifierName(symbol.Name);
					result = semanticModelOpt.GetSpeculativeTypeInfo(queryBody.SelectOrGroup.Span.End - 1, expression, SpeculativeBindingOption.BindAsExpression).Type;
				}
				if (token.Parent is IdentifierNameSyntax node)
				{
					result = semanticModelOpt.GetTypeInfo(node).Type;
				}
			}
		}
		return result;
	}

	private static QueryBodySyntax GetQueryBody(SyntaxToken token)
	{
		if (token.Parent is FromClauseSyntax fromClauseSyntax && fromClauseSyntax.Identifier == token)
		{
			return (fromClauseSyntax.Parent as QueryBodySyntax) ?? ((QueryExpressionSyntax)fromClauseSyntax.Parent).Body;
		}
		if (token.Parent is LetClauseSyntax letClauseSyntax && letClauseSyntax.Identifier == token)
		{
			return letClauseSyntax.Parent as QueryBodySyntax;
		}
		if (token.Parent is JoinClauseSyntax joinClauseSyntax && joinClauseSyntax.Identifier == token)
		{
			return joinClauseSyntax.Parent as QueryBodySyntax;
		}
		return null;
	}

	private string RemoveAttributeSufficeIfNecessary(INamedTypeSymbol symbol, string symbolName)
	{
		if (IsMinimizing && format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.RemoveAttributeSuffix) && semanticModelOpt.Compilation.IsAttributeType(symbol) && symbolName.TryGetWithoutAttributeSuffix(out var result) && SyntaxFactory.ParseToken(result).IsKind(SyntaxKind.IdentifierToken))
		{
			symbolName = result;
		}
		return symbolName;
	}

	private static T GetAncestorOrThis<T>(SyntaxNode node) where T : SyntaxNode
	{
		return GetAncestorsOrThis<T>(node).FirstOrDefault();
	}

	private static IEnumerable<T> GetAncestorsOrThis<T>(SyntaxNode node) where T : SyntaxNode
	{
		if (node != null)
		{
			return node.AncestorsAndSelf().OfType<T>();
		}
		return SpecializedCollections.EmptyEnumerable<T>();
	}

	private IAliasSymbol GetAliasSymbol(INamespaceOrTypeSymbol symbol)
	{
		if (!AliasMap.TryGetValue(symbol, out var value))
		{
			return null;
		}
		return value;
	}
}
