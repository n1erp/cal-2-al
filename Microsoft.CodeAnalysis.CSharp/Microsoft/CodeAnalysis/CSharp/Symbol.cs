using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class Symbol : IReference, ISymbol, IEquatable<ISymbol>, IFormattable
{
	internal virtual bool RequiresCompletion => false;

	public virtual string Name => string.Empty;

	public virtual string MetadataName => Name;

	public abstract SymbolKind Kind { get; }

	public abstract Symbol ContainingSymbol { get; }

	public virtual NamedTypeSymbol ContainingType
	{
		get
		{
			Symbol containingSymbol = ContainingSymbol;
			NamedTypeSymbol namedTypeSymbol = containingSymbol as NamedTypeSymbol;
			if ((object)namedTypeSymbol == containingSymbol)
			{
				return namedTypeSymbol;
			}
			return containingSymbol.ContainingType;
		}
	}

	public virtual NamespaceSymbol ContainingNamespace
	{
		get
		{
			Symbol containingSymbol = ContainingSymbol;
			while ((object)containingSymbol != null)
			{
				if (containingSymbol is NamespaceSymbol result)
				{
					return result;
				}
				containingSymbol = containingSymbol.ContainingSymbol;
			}
			return null;
		}
	}

	public virtual AssemblySymbol ContainingAssembly => ContainingSymbol?.ContainingAssembly;

	internal virtual CSharpCompilation DeclaringCompilation
	{
		get
		{
			switch (Kind)
			{
			case SymbolKind.ErrorType:
				return null;
			case SymbolKind.Assembly:
				return null;
			case SymbolKind.NetModule:
				return null;
			default:
				if (ContainingModule is SourceModuleSymbol sourceModuleSymbol)
				{
					return sourceModuleSymbol.DeclaringCompilation;
				}
				return null;
			}
		}
	}

	internal virtual ModuleSymbol ContainingModule => ContainingSymbol?.ContainingModule;

	public Symbol OriginalDefinition => OriginalSymbolDefinition;

	protected virtual Symbol OriginalSymbolDefinition => this;

	public bool IsDefinition => (object)this == OriginalDefinition;

	public abstract ImmutableArray<Location> Locations { get; }

	public abstract ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; }

	public abstract Accessibility DeclaredAccessibility { get; }

	public abstract bool IsStatic { get; }

	public abstract bool IsVirtual { get; }

	public abstract bool IsOverride { get; }

	public abstract bool IsAbstract { get; }

	public abstract bool IsSealed { get; }

	public abstract bool IsExtern { get; }

	public virtual bool IsImplicitlyDeclared => false;

	public bool CanBeReferencedByName
	{
		get
		{
			switch (Kind)
			{
			case SymbolKind.Alias:
			case SymbolKind.Label:
			case SymbolKind.Local:
			case SymbolKind.RangeVariable:
				return true;
			case SymbolKind.NamedType:
				if (((NamedTypeSymbol)this).IsSubmissionClass)
				{
					return false;
				}
				break;
			case SymbolKind.Property:
			{
				PropertySymbol propertySymbol = (PropertySymbol)this;
				if (propertySymbol.IsIndexer || propertySymbol.MustCallMethodsDirectly)
				{
					return false;
				}
				break;
			}
			case SymbolKind.Method:
			{
				MethodSymbol methodSymbol = (MethodSymbol)this;
				switch (methodSymbol.MethodKind)
				{
				case MethodKind.Destructor:
					return true;
				case MethodKind.DelegateInvoke:
					return true;
				case MethodKind.PropertyGet:
				case MethodKind.PropertySet:
					if (!((PropertySymbol)methodSymbol.AssociatedSymbol).CanCallMethodsDirectly())
					{
						return false;
					}
					break;
				default:
					return false;
				case MethodKind.Ordinary:
				case MethodKind.ReducedExtension:
				case MethodKind.LocalFunction:
					break;
				}
				break;
			}
			case SymbolKind.ArrayType:
			case SymbolKind.Assembly:
			case SymbolKind.DynamicType:
			case SymbolKind.NetModule:
			case SymbolKind.PointerType:
			case SymbolKind.Discard:
				return false;
			default:
				throw ExceptionUtilities.UnexpectedValue(Kind);
			case SymbolKind.ErrorType:
			case SymbolKind.Event:
			case SymbolKind.Field:
			case SymbolKind.Namespace:
			case SymbolKind.Parameter:
			case SymbolKind.TypeParameter:
				break;
			}
			if (SyntaxFacts.IsValidIdentifier(Name))
			{
				return !SyntaxFacts.ContainsDroppedIdentifierCharacters(Name);
			}
			return false;
		}
	}

	internal bool CanBeReferencedByNameIgnoringIllegalCharacters
	{
		get
		{
			if (Kind == SymbolKind.Method)
			{
				MethodSymbol methodSymbol = (MethodSymbol)this;
				switch (methodSymbol.MethodKind)
				{
				case MethodKind.DelegateInvoke:
				case MethodKind.Destructor:
				case MethodKind.Ordinary:
				case MethodKind.LocalFunction:
					return true;
				case MethodKind.PropertyGet:
				case MethodKind.PropertySet:
					return ((PropertySymbol)methodSymbol.AssociatedSymbol).CanCallMethodsDirectly();
				default:
					return false;
				}
			}
			return true;
		}
	}

	internal bool Dangerous_IsFromSomeCompilation => DeclaringCompilation != null;

	internal bool HasUseSiteError
	{
		get
		{
			DiagnosticInfo useSiteDiagnostic = GetUseSiteDiagnostic();
			if (useSiteDiagnostic != null)
			{
				return useSiteDiagnostic.Severity == DiagnosticSeverity.Error;
			}
			return false;
		}
	}

	protected virtual int HighestPriorityUseSiteError => int.MaxValue;

	public virtual bool HasUnsupportedMetadata => false;

	internal ThreeState ObsoleteState
	{
		get
		{
			switch (ObsoleteKind)
			{
			case ObsoleteAttributeKind.None:
			case ObsoleteAttributeKind.Experimental:
				return ThreeState.False;
			case ObsoleteAttributeKind.Uninitialized:
				return ThreeState.Unknown;
			default:
				return ThreeState.True;
			}
		}
	}

	internal ObsoleteAttributeKind ObsoleteKind => ObsoleteAttributeData?.Kind ?? ObsoleteAttributeKind.None;

	internal abstract ObsoleteAttributeData ObsoleteAttributeData { get; }

	public string Language => "C#";

	string ISymbol.Name => Name;

	bool ISymbol.IsImplicitlyDeclared => IsImplicitlyDeclared;

	ISymbol ISymbol.ContainingSymbol => ContainingSymbol;

	IAssemblySymbol ISymbol.ContainingAssembly => ContainingAssembly;

	IModuleSymbol ISymbol.ContainingModule => ContainingModule;

	INamedTypeSymbol ISymbol.ContainingType => ContainingType;

	INamespaceSymbol ISymbol.ContainingNamespace => ContainingNamespace;

	bool ISymbol.IsDefinition => IsDefinition;

	bool ISymbol.IsStatic => IsStatic;

	bool ISymbol.IsVirtual => IsVirtual;

	bool ISymbol.IsOverride => IsOverride;

	bool ISymbol.IsAbstract => IsAbstract;

	bool ISymbol.IsSealed => IsSealed;

	ImmutableArray<Location> ISymbol.Locations => Locations;

	ImmutableArray<SyntaxReference> ISymbol.DeclaringSyntaxReferences => DeclaringSyntaxReferences;

	Accessibility ISymbol.DeclaredAccessibility => DeclaredAccessibility;

	ISymbol ISymbol.OriginalDefinition => OriginalDefinition;

	public static bool IsSymbolAccessible(Symbol symbol, NamedTypeSymbol within, NamedTypeSymbol throughTypeOpt = null)
	{
		if ((object)symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		if ((object)within == null)
		{
			throw new ArgumentNullException("within");
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		return AccessCheck.IsSymbolAccessible(symbol, within, ref useSiteDiagnostics, throughTypeOpt);
	}

	public static bool IsSymbolAccessible(Symbol symbol, AssemblySymbol within)
	{
		if ((object)symbol == null)
		{
			throw new ArgumentNullException("symbol");
		}
		if ((object)within == null)
		{
			throw new ArgumentNullException("within");
		}
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		return AccessCheck.IsSymbolAccessible(symbol, within, ref useSiteDiagnostics);
	}

	[Conditional("DEBUG")]
	protected internal void CheckDefinitionInvariant()
	{
	}

	IDefinition IReference.AsDefinition(EmitContext context)
	{
		throw ExceptionUtilities.Unreachable;
	}

	void IReference.Dispatch(MetadataVisitor visitor)
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal bool IsDefinitionOrDistinct()
	{
		if (!IsDefinition)
		{
			return !Equals(OriginalDefinition);
		}
		return true;
	}

	IEnumerable<ICustomAttribute> IReference.GetAttributes(EmitContext context)
	{
		return GetCustomAttributesToEmit((PEModuleBuilder)context.Module);
	}

	internal virtual IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder)
	{
		return GetCustomAttributesToEmit(moduleBuilder, emittingAssemblyAttributesInNetModule: false);
	}

	internal IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(PEModuleBuilder moduleBuilder, bool emittingAssemblyAttributesInNetModule)
	{
		ArrayBuilder<SynthesizedAttributeData> attributes = null;
		ImmutableArray<CSharpAttributeData> attributes2 = GetAttributes();
		AddSynthesizedAttributes(moduleBuilder, ref attributes);
		return GetCustomAttributesToEmit(attributes2, attributes, isReturnType: false, emittingAssemblyAttributesInNetModule);
	}

	internal IEnumerable<CSharpAttributeData> GetCustomAttributesToEmit(ImmutableArray<CSharpAttributeData> userDefined, ArrayBuilder<SynthesizedAttributeData> synthesized, bool isReturnType, bool emittingAssemblyAttributesInNetModule)
	{
		if (userDefined.IsEmpty && synthesized == null)
		{
			return SpecializedCollections.EmptyEnumerable<CSharpAttributeData>();
		}
		return GetCustomAttributesToEmitIterator(userDefined, synthesized, isReturnType, emittingAssemblyAttributesInNetModule);
	}

	private IEnumerable<CSharpAttributeData> GetCustomAttributesToEmitIterator(ImmutableArray<CSharpAttributeData> userDefined, ArrayBuilder<SynthesizedAttributeData> synthesized, bool isReturnType, bool emittingAssemblyAttributesInNetModule)
	{
		if (synthesized != null)
		{
			foreach (SynthesizedAttributeData item in synthesized)
			{
				yield return item;
			}
			synthesized.Free();
		}
		for (int i = 0; i < userDefined.Length; i++)
		{
			CSharpAttributeData cSharpAttributeData = userDefined[i];
			if ((Kind != SymbolKind.Assembly || !((SourceAssemblySymbol)this).IsIndexOfOmittedAssemblyAttribute(i)) && cSharpAttributeData.ShouldEmitAttribute(this, isReturnType, emittingAssemblyAttributesInNetModule))
			{
				yield return cSharpAttributeData;
			}
		}
	}

	internal virtual void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
	}

	internal virtual bool HasComplete(CompletionPart part)
	{
		return true;
	}

	internal virtual LexicalSortKey GetLexicalSortKey()
	{
		ImmutableArray<Location> locations = Locations;
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (locations.Length <= 0)
		{
			return LexicalSortKey.NotInSource;
		}
		return new LexicalSortKey(locations[0], declaringCompilation);
	}

	internal static ImmutableArray<SyntaxReference> GetDeclaringSyntaxReferenceHelper<TNode>(ImmutableArray<Location> locations) where TNode : CSharpSyntaxNode
	{
		if (locations.IsEmpty)
		{
			return ImmutableArray<SyntaxReference>.Empty;
		}
		ArrayBuilder<SyntaxReference> instance = ArrayBuilder<SyntaxReference>.GetInstance();
		foreach (Location item in locations)
		{
			if (!item.IsInSource)
			{
				continue;
			}
			SyntaxToken token = item.SourceTree.GetRoot().FindToken(item.SourceSpan.Start);
			if (token.Kind() != SyntaxKind.None)
			{
				CSharpSyntaxNode cSharpSyntaxNode = token.Parent.FirstAncestorOrSelf<TNode>();
				if (cSharpSyntaxNode != null)
				{
					instance.Add(cSharpSyntaxNode.GetReference());
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	internal virtual void AfterAddingTypeMembersChecks(ConversionsBase conversions, DiagnosticBag diagnostics)
	{
	}

	public static bool operator ==(Symbol left, Symbol right)
	{
		if ((object)right == null)
		{
			return (object)left == null;
		}
		if ((object)left != right)
		{
			return right.Equals(left);
		}
		return true;
	}

	public static bool operator !=(Symbol left, Symbol right)
	{
		if ((object)right == null)
		{
			return (object)left != null;
		}
		if ((object)left != right)
		{
			return !right.Equals(left);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return this == obj;
	}

	public bool Equals(ISymbol other)
	{
		return Equals((object?)other);
	}

	public override int GetHashCode()
	{
		return RuntimeHelpers.GetHashCode(this);
	}

	public sealed override string ToString()
	{
		return ToDisplayString();
	}

	internal abstract TResult Accept<TArgument, TResult>(CSharpSymbolVisitor<TArgument, TResult> visitor, TArgument a);

	internal Symbol()
	{
	}

	internal virtual void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
	}

	internal static void AddSynthesizedAttribute(ref ArrayBuilder<SynthesizedAttributeData> attributes, SynthesizedAttributeData attribute)
	{
		if (attribute != null)
		{
			if (attributes == null)
			{
				attributes = new ArrayBuilder<SynthesizedAttributeData>(1);
			}
			attributes.Add(attribute);
		}
	}

	internal CharSet? GetEffectiveDefaultMarshallingCharSet()
	{
		return ContainingModule.DefaultMarshallingCharSet;
	}

	internal bool IsFromCompilation(CSharpCompilation compilation)
	{
		return compilation == DeclaringCompilation;
	}

	internal virtual bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		ImmutableArray<SyntaxReference> declaringSyntaxReferences = DeclaringSyntaxReferences;
		if (IsImplicitlyDeclared && declaringSyntaxReferences.Length == 0)
		{
			return ContainingSymbol.IsDefinedInSourceTree(tree, definedWithinSpan, cancellationToken);
		}
		foreach (SyntaxReference item in declaringSyntaxReferences)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (item.SyntaxTree == tree && (!definedWithinSpan.HasValue || item.Span.IntersectsWith(definedWithinSpan.Value)))
			{
				return true;
			}
		}
		return false;
	}

	internal static void ForceCompleteMemberByLocation(SourceLocation locationOpt, Symbol member, CancellationToken cancellationToken)
	{
		if (locationOpt == null || member.IsDefinedInSourceTree(locationOpt.SourceTree, locationOpt.SourceSpan, cancellationToken))
		{
			cancellationToken.ThrowIfCancellationRequested();
			member.ForceComplete(locationOpt, cancellationToken);
		}
	}

	public virtual string GetDocumentationCommentId()
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			StringBuilder builder = instance.Builder;
			DocumentationCommentIDVisitor.Instance.Visit(this, builder);
			return (builder.Length == 0) ? null : builder.ToString();
		}
		finally
		{
			instance.Free();
		}
	}

	public virtual string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
	{
		return "";
	}

	internal string GetDebuggerDisplay()
	{
		return $"{Kind} {ToDisplayString(SymbolDisplayFormat.TestFormat)}";
	}

	internal virtual void AddDeclarationDiagnostics(DiagnosticBag diagnostics)
	{
		if (!diagnostics.IsEmptyWithoutResolution)
		{
			DeclaringCompilation.DeclarationDiagnostics.AddRange(diagnostics);
		}
	}

	internal virtual DiagnosticInfo GetUseSiteDiagnostic()
	{
		return null;
	}

	internal DiagnosticInfo GetUseSiteDiagnosticForSymbolOrContainingType()
	{
		DiagnosticInfo useSiteDiagnostic = GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null && useSiteDiagnostic.Severity == DiagnosticSeverity.Error)
		{
			return useSiteDiagnostic;
		}
		return ContainingType.GetUseSiteDiagnostic() ?? useSiteDiagnostic;
	}

	internal bool MergeUseSiteDiagnostics(ref DiagnosticInfo result, DiagnosticInfo info)
	{
		if (info == null)
		{
			return false;
		}
		if (info.Severity == DiagnosticSeverity.Error && (info.Code == HighestPriorityUseSiteError || HighestPriorityUseSiteError == int.MaxValue))
		{
			result = info;
			return true;
		}
		if (result == null || (result.Severity == DiagnosticSeverity.Warning && info.Severity == DiagnosticSeverity.Error))
		{
			result = info;
			return false;
		}
		return false;
	}

	internal static bool ReportUseSiteDiagnostic(DiagnosticInfo info, DiagnosticBag diagnostics, Location location)
	{
		if (info.Code == 1702 || info.Code == 1701 || info.Code == 1705)
		{
			location = NoLocation.Singleton;
		}
		diagnostics.Add(info, location);
		return info.Severity == DiagnosticSeverity.Error;
	}

	internal bool DeriveUseSiteDiagnosticFromType(ref DiagnosticInfo result, TypeSymbol type)
	{
		DiagnosticInfo diagnosticInfo = type.GetUseSiteDiagnostic();
		if (diagnosticInfo != null && diagnosticInfo.Code == 648)
		{
			SymbolKind kind = Kind;
			if ((uint)(kind - 5) <= 1u || kind == SymbolKind.Method || kind == SymbolKind.Property)
			{
				diagnosticInfo = new CSDiagnosticInfo(ErrorCode.ERR_BindToBogus, this);
			}
		}
		return MergeUseSiteDiagnostics(ref result, diagnosticInfo);
	}

	internal bool DeriveUseSiteDiagnosticFromParameter(ref DiagnosticInfo result, ParameterSymbol param)
	{
		if (!DeriveUseSiteDiagnosticFromType(ref result, param.Type) && !DeriveUseSiteDiagnosticFromCustomModifiers(ref result, param.RefCustomModifiers))
		{
			return DeriveUseSiteDiagnosticFromCustomModifiers(ref result, param.CustomModifiers);
		}
		return true;
	}

	internal bool DeriveUseSiteDiagnosticFromParameters(ref DiagnosticInfo result, ImmutableArray<ParameterSymbol> parameters)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (DeriveUseSiteDiagnosticFromParameter(ref result, item))
			{
				return true;
			}
		}
		return false;
	}

	internal bool DeriveUseSiteDiagnosticFromCustomModifiers(ref DiagnosticInfo result, ImmutableArray<CustomModifier> customModifiers)
	{
		foreach (CustomModifier item in customModifiers)
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)item.Modifier;
			if (namedTypeSymbol.IsUnboundGenericType)
			{
				namedTypeSymbol = namedTypeSymbol.OriginalDefinition;
			}
			if (DeriveUseSiteDiagnosticFromType(ref result, namedTypeSymbol))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetUnificationUseSiteDiagnosticRecursive<T>(ref DiagnosticInfo result, ImmutableArray<T> types, Symbol owner, ref HashSet<TypeSymbol> checkedTypes) where T : TypeSymbol
	{
		foreach (T item in types)
		{
			if (item.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, ImmutableArray<CustomModifier> modifiers, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		foreach (CustomModifier item in modifiers)
		{
			if (((TypeSymbol)item.Modifier).GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, ImmutableArray<ParameterSymbol> parameters, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (item.Type.GetUnificationUseSiteDiagnosticRecursive(ref result, owner, ref checkedTypes) || GetUnificationUseSiteDiagnosticRecursive(ref result, item.RefCustomModifiers, owner, ref checkedTypes) || GetUnificationUseSiteDiagnosticRecursive(ref result, item.CustomModifiers, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool GetUnificationUseSiteDiagnosticRecursive(ref DiagnosticInfo result, ImmutableArray<TypeParameterSymbol> typeParameters, Symbol owner, ref HashSet<TypeSymbol> checkedTypes)
	{
		foreach (TypeParameterSymbol item in typeParameters)
		{
			if (GetUnificationUseSiteDiagnosticRecursive(ref result, item.ConstraintTypesNoUseSiteDiagnostics, owner, ref checkedTypes))
			{
				return true;
			}
		}
		return false;
	}

	internal bool GetGuidStringDefaultImplementation(out string guidString)
	{
		foreach (CSharpAttributeData attribute in GetAttributes())
		{
			if (attribute.IsTargetAttribute(this, AttributeDescription.GuidAttribute) && attribute.TryGetGuidAttributeValue(out guidString))
			{
				return true;
			}
		}
		guidString = null;
		return false;
	}

	public string ToDisplayString(SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToDisplayString(this, format);
	}

	public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToDisplayParts(this, format);
	}

	public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToMinimalDisplayString(this, semanticModel, position, format);
	}

	public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null)
	{
		return SymbolDisplay.ToMinimalDisplayParts(this, semanticModel, position, format);
	}

	internal static void ReportErrorIfHasConstraints(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, DiagnosticBag diagnostics)
	{
		if (constraintClauses.Count > 0)
		{
			diagnostics.Add(ErrorCode.ERR_ConstraintOnlyAllowedOnGenericDecl, constraintClauses[0].WhereKeyword.GetLocation());
		}
	}

	internal static void CheckForBlockAndExpressionBody(CSharpSyntaxNode block, CSharpSyntaxNode expression, CSharpSyntaxNode syntax, DiagnosticBag diagnostics)
	{
		if (block != null && expression != null)
		{
			diagnostics.Add(ErrorCode.ERR_BlockBodyAndExpressionBody, syntax.GetLocation());
		}
	}

	string ISymbol.ToDisplayString(SymbolDisplayFormat format)
	{
		return SymbolDisplay.ToDisplayString(this, format);
	}

	ImmutableArray<SymbolDisplayPart> ISymbol.ToDisplayParts(SymbolDisplayFormat format)
	{
		return SymbolDisplay.ToDisplayParts(this, format);
	}

	string ISymbol.ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format)
	{
		if (!(semanticModel is CSharpSemanticModel semanticModel2))
		{
			throw new ArgumentException(CSharpResources.WrongSemanticModelType, Language);
		}
		return SymbolDisplay.ToMinimalDisplayString(this, semanticModel2, position, format);
	}

	ImmutableArray<SymbolDisplayPart> ISymbol.ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format)
	{
		if (!(semanticModel is CSharpSemanticModel semanticModel2))
		{
			throw new ArgumentException(CSharpResources.WrongSemanticModelType, Language);
		}
		return SymbolDisplay.ToMinimalDisplayParts(this, semanticModel2, position, format);
	}

	ImmutableArray<AttributeData> ISymbol.GetAttributes()
	{
		return StaticCast<AttributeData>.From(GetAttributes());
	}

	public abstract void Accept(SymbolVisitor visitor);

	public abstract TResult Accept<TResult>(SymbolVisitor<TResult> visitor);

	public abstract void Accept(CSharpSymbolVisitor visitor);

	public abstract TResult Accept<TResult>(CSharpSymbolVisitor<TResult> visitor);

	string IFormattable.ToString(string format, IFormatProvider formatProvider)
	{
		return ToString();
	}

	public virtual ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		return ImmutableArray<CSharpAttributeData>.Empty;
	}

	internal virtual AttributeTargets GetAttributeTarget()
	{
		switch (Kind)
		{
		case SymbolKind.Assembly:
			return AttributeTargets.Assembly;
		case SymbolKind.Field:
			return AttributeTargets.Field;
		case SymbolKind.Method:
		{
			MethodKind methodKind = ((MethodSymbol)this).MethodKind;
			if (methodKind == MethodKind.Constructor || methodKind == MethodKind.StaticConstructor)
			{
				return AttributeTargets.Constructor;
			}
			return AttributeTargets.Method;
		}
		case SymbolKind.NamedType:
		{
			NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)this;
			switch (namedTypeSymbol.TypeKind)
			{
			case TypeKind.Class:
				return AttributeTargets.Class;
			case TypeKind.Delegate:
				return AttributeTargets.Delegate;
			case TypeKind.Enum:
				return AttributeTargets.Enum;
			case TypeKind.Interface:
				return AttributeTargets.Interface;
			case TypeKind.Struct:
				return AttributeTargets.Struct;
			case TypeKind.TypeParameter:
				return AttributeTargets.GenericParameter;
			case TypeKind.Submission:
				throw ExceptionUtilities.UnexpectedValue(namedTypeSymbol.TypeKind);
			}
			break;
		}
		case SymbolKind.NetModule:
			return AttributeTargets.Module;
		case SymbolKind.Parameter:
			return AttributeTargets.Parameter;
		case SymbolKind.Property:
			return AttributeTargets.Property;
		case SymbolKind.Event:
			return AttributeTargets.Event;
		case SymbolKind.TypeParameter:
			return AttributeTargets.GenericParameter;
		}
		return (AttributeTargets)0;
	}

	internal virtual void EarlyDecodeWellKnownAttributeType(NamedTypeSymbol attributeType, AttributeSyntax attributeSyntax)
	{
	}

	internal virtual void PostEarlyDecodeWellKnownAttributeTypes()
	{
	}

	internal virtual CSharpAttributeData EarlyDecodeWellKnownAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments)
	{
		return null;
	}

	internal static bool EarlyDecodeDeprecatedOrExperimentalOrObsoleteAttribute(ref EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments, out CSharpAttributeData attributeData, out ObsoleteAttributeData obsoleteData)
	{
		NamedTypeSymbol attributeType = arguments.AttributeType;
		AttributeSyntax attributeSyntax = arguments.AttributeSyntax;
		ObsoleteAttributeKind kind;
		if (CSharpAttributeData.IsTargetEarlyAttribute(attributeType, attributeSyntax, AttributeDescription.ObsoleteAttribute))
		{
			kind = ObsoleteAttributeKind.Obsolete;
		}
		else if (CSharpAttributeData.IsTargetEarlyAttribute(attributeType, attributeSyntax, AttributeDescription.DeprecatedAttribute))
		{
			kind = ObsoleteAttributeKind.Deprecated;
		}
		else
		{
			if (!CSharpAttributeData.IsTargetEarlyAttribute(attributeType, attributeSyntax, AttributeDescription.ExperimentalAttribute))
			{
				obsoleteData = null;
				attributeData = null;
				return false;
			}
			kind = ObsoleteAttributeKind.Experimental;
		}
		attributeData = arguments.Binder.GetAttribute(attributeSyntax, attributeType, out var generatedDiagnostics);
		if (!attributeData.HasErrors)
		{
			obsoleteData = attributeData.DecodeObsoleteAttribute(kind);
			if (generatedDiagnostics)
			{
				attributeData = null;
			}
		}
		else
		{
			obsoleteData = null;
			attributeData = null;
		}
		return true;
	}

	internal virtual void DecodeWellKnownAttribute(ref DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments)
	{
	}

	internal virtual void PostDecodeWellKnownAttributes(ImmutableArray<CSharpAttributeData> boundAttributes, ImmutableArray<AttributeSyntax> allAttributeSyntaxNodes, DiagnosticBag diagnostics, AttributeLocation symbolPart, WellKnownAttributeData decodedData)
	{
	}

	internal bool LoadAndValidateAttributes(OneOrMany<SyntaxList<AttributeListSyntax>> attributesSyntaxLists, ref CustomAttributesBag<CSharpAttributeData> lazyCustomAttributesBag, AttributeLocation symbolPart = AttributeLocation.None, bool earlyDecodingOnly = false, Binder binderOpt = null, Func<AttributeSyntax, bool> attributeMatchesOpt = null)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		ImmutableArray<AttributeSyntax> attributesToBind = GetAttributesToBind(attributesSyntaxLists, symbolPart, instance, declaringCompilation, attributeMatchesOpt, binderOpt, out var binders);
		ImmutableArray<CSharpAttributeData> immutableArray2;
		WellKnownAttributeData wellKnownAttributeData;
		if (attributesToBind.Any())
		{
			if (lazyCustomAttributesBag == null)
			{
				Interlocked.CompareExchange(ref lazyCustomAttributesBag, new CustomAttributesBag<CSharpAttributeData>(), null);
			}
			int length = attributesToBind.Length;
			NamedTypeSymbol[] array = new NamedTypeSymbol[length];
			Binder.BindAttributeTypes(binders, attributesToBind, this, array, instance);
			ImmutableArray<NamedTypeSymbol> immutableArray = array.AsImmutableOrNull();
			EarlyDecodeWellKnownAttributeTypes(immutableArray, attributesToBind);
			PostEarlyDecodeWellKnownAttributeTypes();
			CSharpAttributeData[] array2 = new CSharpAttributeData[length];
			EarlyWellKnownAttributeData earlyDecodedWellKnownAttributeData = EarlyDecodeWellKnownAttributes(binders, immutableArray, attributesToBind, symbolPart, array2);
			lazyCustomAttributesBag.SetEarlyDecodedWellKnownAttributeData(earlyDecodedWellKnownAttributeData);
			if (earlyDecodingOnly)
			{
				instance.Free();
				return false;
			}
			Binder.GetAttributes(binders, attributesToBind, immutableArray, array2, instance);
			immutableArray2 = array2.AsImmutableOrNull();
			wellKnownAttributeData = ValidateAttributeUsageAndDecodeWellKnownAttributes(binders, attributesToBind, immutableArray2, instance, symbolPart);
			lazyCustomAttributesBag.SetDecodedWellKnownAttributeData(wellKnownAttributeData);
		}
		else
		{
			if (earlyDecodingOnly)
			{
				instance.Free();
				return false;
			}
			immutableArray2 = ImmutableArray<CSharpAttributeData>.Empty;
			wellKnownAttributeData = null;
			Interlocked.CompareExchange(ref lazyCustomAttributesBag, CustomAttributesBag<CSharpAttributeData>.WithEmptyData(), null);
			PostEarlyDecodeWellKnownAttributeTypes();
		}
		PostDecodeWellKnownAttributes(immutableArray2, attributesToBind, instance, symbolPart, wellKnownAttributeData);
		bool result = false;
		if (lazyCustomAttributesBag.SetAttributes(immutableArray2))
		{
			if (attributeMatchesOpt == null)
			{
				RecordPresenceOfBadAttributes(immutableArray2);
				AddDeclarationDiagnostics(instance);
			}
			result = true;
			if (lazyCustomAttributesBag.IsEmpty)
			{
				lazyCustomAttributesBag = CustomAttributesBag<CSharpAttributeData>.Empty;
			}
		}
		instance.Free();
		return result;
	}

	private void RecordPresenceOfBadAttributes(ImmutableArray<CSharpAttributeData> boundAttributes)
	{
		foreach (CSharpAttributeData item in boundAttributes)
		{
			if (item.HasErrors)
			{
				((SourceModuleSymbol)DeclaringCompilation.SourceModule).RecordPresenceOfBadAttributes();
				break;
			}
		}
	}

	private ImmutableArray<AttributeSyntax> GetAttributesToBind(OneOrMany<SyntaxList<AttributeListSyntax>> attributeDeclarationSyntaxLists, AttributeLocation symbolPart, DiagnosticBag diagnostics, CSharpCompilation compilation, Func<AttributeSyntax, bool> attributeMatchesOpt, Binder rootBinderOpt, out ImmutableArray<Binder> binders)
	{
		IAttributeTargetSymbol attributeTarget = (IAttributeTargetSymbol)this;
		ArrayBuilder<AttributeSyntax> arrayBuilder = null;
		ArrayBuilder<Binder> arrayBuilder2 = null;
		int num = 0;
		for (int i = 0; i < attributeDeclarationSyntaxLists.Count; i++)
		{
			SyntaxList<AttributeListSyntax> syntaxList = attributeDeclarationSyntaxLists[i];
			if (!syntaxList.Any())
			{
				continue;
			}
			int num2 = num;
			foreach (AttributeListSyntax item in syntaxList)
			{
				if (!MatchAttributeTarget(attributeTarget, symbolPart, item.Target, diagnostics))
				{
					continue;
				}
				if (arrayBuilder == null)
				{
					arrayBuilder = new ArrayBuilder<AttributeSyntax>();
					arrayBuilder2 = new ArrayBuilder<Binder>();
				}
				SeparatedSyntaxList<AttributeSyntax> attributes = item.Attributes;
				if (attributeMatchesOpt == null)
				{
					arrayBuilder.AddRange(attributes);
					num += attributes.Count;
					continue;
				}
				foreach (AttributeSyntax item2 in attributes)
				{
					if (attributeMatchesOpt(item2))
					{
						arrayBuilder.Add(item2);
						num++;
					}
				}
			}
			if (num != num2)
			{
				SyntaxTree syntaxTree = syntaxList.Node.SyntaxTree;
				Binder enclosing = rootBinderOpt ?? compilation.GetBinderFactory(syntaxTree).GetBinder(syntaxList.Node);
				enclosing = new ContextualAttributeBinder(enclosing, this);
				for (int j = 0; j < num - num2; j++)
				{
					arrayBuilder2.Add(enclosing);
				}
			}
		}
		if (arrayBuilder != null)
		{
			binders = arrayBuilder2.ToImmutableAndFree();
			return arrayBuilder.ToImmutableAndFree();
		}
		binders = ImmutableArray<Binder>.Empty;
		return ImmutableArray<AttributeSyntax>.Empty;
	}

	private static bool MatchAttributeTarget(IAttributeTargetSymbol attributeTarget, AttributeLocation symbolPart, AttributeTargetSpecifierSyntax targetOpt, DiagnosticBag diagnostics)
	{
		IAttributeTargetSymbol attributesOwner = attributeTarget.AttributesOwner;
		bool flag = symbolPart == AttributeLocation.None && attributesOwner == attributeTarget;
		if (targetOpt == null)
		{
			return flag;
		}
		AttributeLocation allowedAttributeLocations = attributesOwner.AllowedAttributeLocations;
		AttributeLocation attributeLocation = targetOpt.GetAttributeLocation();
		if (attributeLocation == AttributeLocation.None)
		{
			if (flag)
			{
				diagnostics.Add(ErrorCode.WRN_InvalidAttributeLocation, targetOpt.Identifier.GetLocation(), targetOpt.Identifier.ValueText, allowedAttributeLocations.ToDisplayString());
			}
			return false;
		}
		if ((attributeLocation & allowedAttributeLocations) == 0)
		{
			if (flag)
			{
				if (allowedAttributeLocations == AttributeLocation.None)
				{
					AttributeLocation defaultAttributeLocation = attributeTarget.DefaultAttributeLocation;
					if ((uint)(defaultAttributeLocation - 1) > 1u)
					{
						throw ExceptionUtilities.UnexpectedValue(attributeTarget.DefaultAttributeLocation);
					}
					diagnostics.Add(ErrorCode.ERR_GlobalAttributesNotAllowed, targetOpt.Identifier.GetLocation());
				}
				else
				{
					diagnostics.Add(ErrorCode.WRN_AttributeLocationOnBadDeclaration, targetOpt.Identifier.GetLocation(), targetOpt.Identifier.ToString(), allowedAttributeLocations.ToDisplayString());
				}
			}
			return false;
		}
		if (symbolPart == AttributeLocation.None)
		{
			return attributeLocation == attributeTarget.DefaultAttributeLocation;
		}
		return attributeLocation == symbolPart;
	}

	internal EarlyWellKnownAttributeData EarlyDecodeWellKnownAttributes(ImmutableArray<Binder> binders, ImmutableArray<NamedTypeSymbol> boundAttributeTypes, ImmutableArray<AttributeSyntax> attributesToBind, AttributeLocation symbolPart, CSharpAttributeData[] boundAttributesBuilder)
	{
		EarlyWellKnownAttributeBinder earlyWellKnownAttributeBinder = new EarlyWellKnownAttributeBinder(binders[0]);
		EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation> arguments = new EarlyDecodeWellKnownAttributeArguments<EarlyWellKnownAttributeBinder, NamedTypeSymbol, AttributeSyntax, AttributeLocation>
		{
			SymbolPart = symbolPart
		};
		for (int i = 0; i < boundAttributeTypes.Length; i++)
		{
			NamedTypeSymbol namedTypeSymbol = boundAttributeTypes[i];
			if (!namedTypeSymbol.IsErrorType())
			{
				if (binders[i] != earlyWellKnownAttributeBinder.Next)
				{
					earlyWellKnownAttributeBinder = new EarlyWellKnownAttributeBinder(binders[i]);
				}
				arguments.Binder = earlyWellKnownAttributeBinder;
				arguments.AttributeType = namedTypeSymbol;
				arguments.AttributeSyntax = attributesToBind[i];
				CSharpAttributeData cSharpAttributeData = EarlyDecodeWellKnownAttribute(ref arguments);
				boundAttributesBuilder[i] = cSharpAttributeData;
			}
		}
		if (!arguments.HasDecodedData)
		{
			return null;
		}
		return arguments.DecodedData;
	}

	private void EarlyDecodeWellKnownAttributeTypes(ImmutableArray<NamedTypeSymbol> attributeTypes, ImmutableArray<AttributeSyntax> attributeSyntaxList)
	{
		for (int i = 0; i < attributeTypes.Length; i++)
		{
			NamedTypeSymbol namedTypeSymbol = attributeTypes[i];
			if (!namedTypeSymbol.IsErrorType())
			{
				EarlyDecodeWellKnownAttributeType(namedTypeSymbol, attributeSyntaxList[i]);
			}
		}
	}

	private WellKnownAttributeData ValidateAttributeUsageAndDecodeWellKnownAttributes(ImmutableArray<Binder> binders, ImmutableArray<AttributeSyntax> attributeSyntaxList, ImmutableArray<CSharpAttributeData> boundAttributes, DiagnosticBag diagnostics, AttributeLocation symbolPart)
	{
		int length = boundAttributes.Length;
		HashSet<NamedTypeSymbol> uniqueAttributeTypes = new HashSet<NamedTypeSymbol>();
		DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation> arguments = new DecodeWellKnownAttributeArguments<AttributeSyntax, CSharpAttributeData, AttributeLocation>
		{
			Diagnostics = diagnostics,
			AttributesCount = length,
			SymbolPart = symbolPart
		};
		for (int i = 0; i < length; i++)
		{
			CSharpAttributeData cSharpAttributeData = boundAttributes[i];
			AttributeSyntax attributeSyntax = attributeSyntaxList[i];
			Binder binder = binders[i];
			if (!cSharpAttributeData.HasErrors && ValidateAttributeUsage(cSharpAttributeData, attributeSyntax, binder.Compilation, symbolPart, diagnostics, uniqueAttributeTypes))
			{
				arguments.Attribute = cSharpAttributeData;
				arguments.AttributeSyntaxOpt = attributeSyntax;
				arguments.Index = i;
				DecodeWellKnownAttribute(ref arguments);
			}
		}
		if (!arguments.HasDecodedData)
		{
			return null;
		}
		return arguments.DecodedData;
	}

	private bool ValidateAttributeUsage(CSharpAttributeData attribute, AttributeSyntax node, CSharpCompilation compilation, AttributeLocation symbolPart, DiagnosticBag diagnostics, HashSet<NamedTypeSymbol> uniqueAttributeTypes)
	{
		NamedTypeSymbol attributeClass = attribute.AttributeClass;
		AttributeUsageInfo attributeUsageInfo = attributeClass.GetAttributeUsageInfo();
		if (!uniqueAttributeTypes.Add(attributeClass) && !attributeUsageInfo.AllowMultiple)
		{
			diagnostics.Add(ErrorCode.ERR_DuplicateAttribute, node.Name.Location, node.GetErrorDisplayName());
			return false;
		}
		AttributeTargets attributeTargets = ((symbolPart != AttributeLocation.Return) ? GetAttributeTarget() : AttributeTargets.ReturnValue);
		if ((attributeTargets & attributeUsageInfo.ValidTargets) == 0)
		{
			diagnostics.Add(ErrorCode.ERR_AttributeOnBadSymbolType, node.Name.Location, node.GetErrorDisplayName(), attributeUsageInfo.GetValidTargetsErrorArgument());
			return false;
		}
		if (attribute.IsSecurityAttribute(compilation))
		{
			SymbolKind kind = Kind;
			if (kind != SymbolKind.Assembly && kind != SymbolKind.Method && kind != SymbolKind.NamedType)
			{
				diagnostics.Add(ErrorCode.ERR_SecurityAttributeInvalidTarget, node.Name.Location, node.GetErrorDisplayName());
				return false;
			}
		}
		return true;
	}

	internal void ForceCompleteObsoleteAttribute()
	{
		if (ObsoleteState == ThreeState.Unknown)
		{
			GetAttributes();
		}
	}
}
