using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SourceMemberContainerTypeSymbol : NamedTypeSymbol
{
	private struct Flags
	{
		private const int SpecialTypeOffset = 0;

		private const int SpecialTypeSize = 6;

		private const int DeclarationModifiersOffset = 6;

		private const int DeclarationModifiersSize = 23;

		private const int IsManagedTypeOffset = 29;

		private const int IsManagedTypeSize = 2;

		private const int FieldDefinitionsNotedOffset = 31;

		private const int FieldDefinitionsNotedSize = 1;

		private const int SpecialTypeMask = 63;

		private const int DeclarationModifiersMask = 8388607;

		private const int IsManagedTypeMask = 3;

		private const int FieldDefinitionsNotedBit = int.MinValue;

		private int _flags;

		private const int TypeKindOffset = 1;

		private const int TypeKindMask = 15;

		private const int FlattenedMembersIsSortedBit = 1;

		private int _flags2;

		public SpecialType SpecialType => (SpecialType)(_flags & 0x3F);

		public DeclarationModifiers DeclarationModifiers => (DeclarationModifiers)((_flags >> 6) & 0x7FFFFF);

		public ThreeState IsManagedType => (ThreeState)((_flags >> 29) & 3);

		public bool FieldDefinitionsNoted => (_flags & int.MinValue) != 0;

		public bool FlattenedMembersIsSorted => (_flags2 & 1) != 0;

		public TypeKind TypeKind => (TypeKind)((_flags2 >> 1) & 0xF);

		public Flags(SpecialType specialType, DeclarationModifiers declarationModifiers, TypeKind typeKind)
		{
			int num = (int)(specialType & (SpecialType)0x3F);
			int num2 = (int)(declarationModifiers & (DeclarationModifiers.All | DeclarationModifiers.Unset)) << 6;
			int flags = (int)((uint)(typeKind & (TypeKind)0xF) << 1);
			_flags = num | num2;
			_flags2 = flags;
		}

		public void SetFieldDefinitionsNoted()
		{
			ThreadSafeFlagOperations.Set(ref _flags, int.MinValue);
		}

		public void SetFlattenedMembersIsSorted()
		{
			ThreadSafeFlagOperations.Set(ref _flags2, 1);
		}

		private static bool BitsAreUnsetOrSame(int bits, int mask)
		{
			if ((bits & mask) != 0)
			{
				return (bits & mask) == mask;
			}
			return true;
		}

		public void SetIsManagedType(bool isManagedType)
		{
			int toSet = (int)((uint)(isManagedType.ToThreeState() & (ThreeState)3) << 29);
			ThreadSafeFlagOperations.Set(ref _flags, toSet);
		}
	}

	private sealed class MembersAndInitializers
	{
		internal readonly ImmutableArray<Symbol> NonTypeNonIndexerMembers;

		internal readonly ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> StaticInitializers;

		internal readonly ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> InstanceInitializers;

		internal readonly ImmutableArray<SyntaxReference> IndexerDeclarations;

		internal readonly int StaticInitializersSyntaxLength;

		internal readonly int InstanceInitializersSyntaxLength;

		public MembersAndInitializers(ImmutableArray<Symbol> nonTypeNonIndexerMembers, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> staticInitializers, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> instanceInitializers, ImmutableArray<SyntaxReference> indexerDeclarations, int staticInitializersSyntaxLength, int instanceInitializersSyntaxLength)
		{
			NonTypeNonIndexerMembers = nonTypeNonIndexerMembers;
			StaticInitializers = staticInitializers;
			InstanceInitializers = instanceInitializers;
			IndexerDeclarations = indexerDeclarations;
			StaticInitializersSyntaxLength = staticInitializersSyntaxLength;
			InstanceInitializersSyntaxLength = instanceInitializersSyntaxLength;
		}
	}

	private class MembersAndInitializersBuilder
	{
		public readonly ArrayBuilder<Symbol> NonTypeNonIndexerMembers = ArrayBuilder<Symbol>.GetInstance();

		public readonly ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>> StaticInitializers = ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>>.GetInstance();

		public readonly ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>> InstanceInitializers = ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>>.GetInstance();

		public readonly ArrayBuilder<SyntaxReference> IndexerDeclarations = ArrayBuilder<SyntaxReference>.GetInstance();

		public int StaticSyntaxLength;

		public int InstanceSyntaxLength;

		public MembersAndInitializers ToReadOnlyAndFree()
		{
			return new MembersAndInitializers(NonTypeNonIndexerMembers.ToImmutableAndFree(), StaticInitializers.ToImmutableAndFree(), InstanceInitializers.ToImmutableAndFree(), IndexerDeclarations.ToImmutableAndFree(), StaticSyntaxLength, InstanceSyntaxLength);
		}

		public void Free()
		{
			NonTypeNonIndexerMembers.Free();
			StaticInitializers.Free();
			InstanceInitializers.Free();
			IndexerDeclarations.Free();
		}
	}

	protected SymbolCompletionState state;

	private Flags _flags;

	private readonly NamespaceOrTypeSymbol _containingSymbol;

	protected readonly MergedTypeDeclaration declaration;

	private MembersAndInitializers _lazyMembersAndInitializers;

	private Dictionary<string, ImmutableArray<Symbol>> _lazyMembersDictionary;

	private Dictionary<string, ImmutableArray<Symbol>> _lazyEarlyAttributeDecodingMembersDictionary;

	private static readonly Dictionary<string, ImmutableArray<NamedTypeSymbol>> s_emptyTypeMembers = new Dictionary<string, ImmutableArray<NamedTypeSymbol>>(EmptyComparer.Instance);

	private Dictionary<string, ImmutableArray<NamedTypeSymbol>> _lazyTypeMembers;

	private ImmutableArray<Symbol> _lazyMembersFlattened;

	private ImmutableArray<SynthesizedExplicitImplementationForwardingMethod> _lazySynthesizedExplicitImplementations;

	private int _lazyKnownCircularStruct;

	private LexicalSortKey _lazyLexicalSortKey = LexicalSortKey.NotInitialized;

	private ThreeState _lazyContainsExtensionMethods;

	private ThreeState _lazyAnyMemberHasAttributes;

	internal sealed override bool RequiresCompletion => true;

	public sealed override NamedTypeSymbol ContainingType => _containingSymbol as NamedTypeSymbol;

	public sealed override Symbol ContainingSymbol => _containingSymbol;

	public override SpecialType SpecialType => _flags.SpecialType;

	public override TypeKind TypeKind => _flags.TypeKind;

	internal MergedTypeDeclaration MergedDeclaration => declaration;

	internal sealed override bool IsInterface => TypeKind == TypeKind.Interface;

	internal override bool IsManagedType
	{
		get
		{
			ThreeState isManagedType = _flags.IsManagedType;
			if (!isManagedType.HasValue())
			{
				bool isManagedType2 = base.IsManagedType;
				_flags.SetIsManagedType(isManagedType2);
				return isManagedType2;
			}
			return isManagedType.Value();
		}
	}

	public override bool IsStatic => (_flags.DeclarationModifiers & DeclarationModifiers.Static) != 0;

	internal sealed override bool IsByRefLikeType => (_flags.DeclarationModifiers & DeclarationModifiers.Ref) != 0;

	internal override bool IsReadOnly => (_flags.DeclarationModifiers & DeclarationModifiers.ReadOnly) != 0;

	public override bool IsSealed => (_flags.DeclarationModifiers & DeclarationModifiers.Sealed) != 0;

	public override bool IsAbstract => (_flags.DeclarationModifiers & DeclarationModifiers.Abstract) != 0;

	internal bool IsPartial => (_flags.DeclarationModifiers & DeclarationModifiers.Partial) != 0;

	internal bool IsNew => (_flags.DeclarationModifiers & DeclarationModifiers.New) != 0;

	public override Accessibility DeclaredAccessibility => ModifierUtils.EffectiveAccessibility(_flags.DeclarationModifiers);

	public override bool IsScriptClass
	{
		get
		{
			DeclarationKind kind = declaration.Declarations[0].Kind;
			if (kind != DeclarationKind.Script)
			{
				return kind == DeclarationKind.Submission;
			}
			return true;
		}
	}

	public override bool IsImplicitClass => declaration.Declarations[0].Kind == DeclarationKind.ImplicitClass;

	public override bool IsImplicitlyDeclared
	{
		get
		{
			if (!IsImplicitClass)
			{
				return IsScriptClass;
			}
			return true;
		}
	}

	public override int Arity => declaration.Arity;

	public override string Name => declaration.Name;

	internal override bool MangleName => Arity > 0;

	public override ImmutableArray<Location> Locations => declaration.NameLocations.Cast<SourceLocation, Location>();

	public ImmutableArray<SyntaxReference> SyntaxReferences => declaration.SyntaxReferences;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => SyntaxReferences;

	internal ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> StaticInitializers => GetMembersAndInitializers().StaticInitializers;

	internal ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> InstanceInitializers => GetMembersAndInitializers().InstanceInitializers;

	public override IEnumerable<string> MemberNames => declaration.MemberNames;

	internal override bool KnownCircularStruct
	{
		get
		{
			if (_lazyKnownCircularStruct == 0)
			{
				if (TypeKind != TypeKind.Struct)
				{
					Interlocked.CompareExchange(ref _lazyKnownCircularStruct, 1, 0);
				}
				else
				{
					DiagnosticBag instance = DiagnosticBag.GetInstance();
					int value = (int)CheckStructCircularity(instance).ToThreeState();
					if (Interlocked.CompareExchange(ref _lazyKnownCircularStruct, value, 0) == 0)
					{
						AddDeclarationDiagnostics(instance);
					}
					instance.Free();
				}
			}
			return _lazyKnownCircularStruct == 2;
		}
	}

	internal bool ContainsExtensionMethods
	{
		get
		{
			if (!_lazyContainsExtensionMethods.HasValue())
			{
				bool value = ((IsStatic && !IsGenericType) || IsScriptClass) && declaration.ContainsExtensionMethods;
				_lazyContainsExtensionMethods = value.ToThreeState();
			}
			return _lazyContainsExtensionMethods.Value();
		}
	}

	internal bool AnyMemberHasAttributes
	{
		get
		{
			if (!_lazyAnyMemberHasAttributes.HasValue())
			{
				bool anyMemberHasAttributes = declaration.AnyMemberHasAttributes;
				_lazyAnyMemberHasAttributes = anyMemberHasAttributes.ToThreeState();
			}
			return _lazyAnyMemberHasAttributes.Value();
		}
	}

	public override bool MightContainExtensionMethods => ContainsExtensionMethods;

	public sealed override NamedTypeSymbol ConstructedFrom => this;

	internal SourceMemberContainerTypeSymbol(NamespaceOrTypeSymbol containingSymbol, MergedTypeDeclaration declaration, DiagnosticBag diagnostics)
	{
		_containingSymbol = containingSymbol;
		this.declaration = declaration;
		TypeKind typeKind = declaration.Kind.ToTypeKind();
		DeclarationModifiers declarationModifiers = MakeModifiers(typeKind, diagnostics);
		foreach (SingleTypeDeclaration declaration2 in declaration.Declarations)
		{
			diagnostics.AddRange(declaration2.Diagnostics);
		}
		int num = (int)(declarationModifiers & DeclarationModifiers.AccessibilityMask);
		if ((num & (num - 1)) != 0)
		{
			if ((declarationModifiers & DeclarationModifiers.Partial) != 0)
			{
				diagnostics.Add(ErrorCode.ERR_PartialModifierConflict, Locations[0], this);
			}
			num &= ~(num - 1);
			declarationModifiers &= ~DeclarationModifiers.AccessibilityMask;
			declarationModifiers = (DeclarationModifiers)((int)declarationModifiers | num);
		}
		_flags = new Flags((num == 16) ? MakeSpecialType() : SpecialType.None, declarationModifiers, typeKind);
		NamedTypeSymbol containingType = ContainingType;
		if ((object)containingType != null && containingType.IsSealed && DeclaredAccessibility.HasProtected())
		{
			diagnostics.Add(AccessCheck.GetProtectedMemberInSealedTypeError(ContainingType), Locations[0], this);
		}
		state.NotePartComplete(CompletionPart.TypeArguments);
	}

	private SpecialType MakeSpecialType()
	{
		if (ContainingSymbol.Kind == SymbolKind.Namespace && ContainingSymbol.ContainingAssembly.KeepLookingForDeclaredSpecialTypes)
		{
			return SpecialTypes.GetTypeFromMetadataName(MetadataHelpers.BuildQualifiedName(ContainingSymbol.ToDisplayString(SymbolDisplayFormat.QualifiedNameOnlyFormat), MetadataName));
		}
		return SpecialType.None;
	}

	private DeclarationModifiers MakeModifiers(TypeKind typeKind, DiagnosticBag diagnostics)
	{
		DeclarationModifiers defaultAccess = ((ContainingSymbol is NamespaceSymbol) ? DeclarationModifiers.Internal : DeclarationModifiers.Private);
		DeclarationModifiers declarationModifiers = DeclarationModifiers.AccessibilityMask;
		if (ContainingSymbol is TypeSymbol)
		{
			declarationModifiers = ((!ContainingType.IsInterface) ? (declarationModifiers | DeclarationModifiers.New) : (declarationModifiers | DeclarationModifiers.All));
		}
		switch (typeKind)
		{
		case TypeKind.Class:
		case TypeKind.Submission:
			declarationModifiers |= DeclarationModifiers.Abstract | DeclarationModifiers.Sealed | DeclarationModifiers.Static | DeclarationModifiers.Partial | DeclarationModifiers.Unsafe;
			break;
		case TypeKind.Struct:
			declarationModifiers |= DeclarationModifiers.ReadOnly | DeclarationModifiers.Partial | DeclarationModifiers.Unsafe | DeclarationModifiers.Ref;
			break;
		case TypeKind.Interface:
			declarationModifiers |= DeclarationModifiers.Partial | DeclarationModifiers.Unsafe;
			break;
		case TypeKind.Delegate:
			declarationModifiers |= DeclarationModifiers.Unsafe;
			break;
		}
		DeclarationModifiers declarationModifiers2 = MakeAndCheckTypeModifiers(defaultAccess, declarationModifiers, this, diagnostics, out var modifierErrors);
		this.CheckUnsafeModifier(declarationModifiers2, diagnostics);
		if (!modifierErrors && (declarationModifiers2 & DeclarationModifiers.Abstract) != 0 && (declarationModifiers2 & (DeclarationModifiers.Sealed | DeclarationModifiers.Static)) != 0)
		{
			diagnostics.Add(ErrorCode.ERR_AbstractSealedStatic, Locations[0], this);
		}
		if (!modifierErrors && (declarationModifiers2 & (DeclarationModifiers.Sealed | DeclarationModifiers.Static)) == (DeclarationModifiers.Sealed | DeclarationModifiers.Static))
		{
			diagnostics.Add(ErrorCode.ERR_SealedStaticClass, Locations[0], this);
		}
		switch (typeKind)
		{
		case TypeKind.Interface:
			declarationModifiers2 |= DeclarationModifiers.Abstract;
			break;
		case TypeKind.Enum:
		case TypeKind.Struct:
			declarationModifiers2 |= DeclarationModifiers.Sealed;
			break;
		case TypeKind.Delegate:
			declarationModifiers2 |= DeclarationModifiers.Sealed;
			break;
		}
		return declarationModifiers2;
	}

	private DeclarationModifiers MakeAndCheckTypeModifiers(DeclarationModifiers defaultAccess, DeclarationModifiers allowedModifiers, SourceMemberContainerTypeSymbol self, DiagnosticBag diagnostics, out bool modifierErrors)
	{
		modifierErrors = false;
		DeclarationModifiers declarationModifiers = DeclarationModifiers.Unset;
		int length = declaration.Declarations.Length;
		bool flag = false;
		for (int i = 0; i < length; i++)
		{
			DeclarationModifiers declarationModifiers2 = declaration.Declarations[i].Modifiers;
			if (length > 1 && (declarationModifiers2 & DeclarationModifiers.Partial) == 0)
			{
				flag = true;
			}
			if (!modifierErrors)
			{
				declarationModifiers2 = ModifierUtils.CheckModifiers(declarationModifiers2, allowedModifiers, declaration.Declarations[i].NameLocation, diagnostics, null, out modifierErrors);
				if (!modifierErrors)
				{
					CSDiagnosticInfo cSDiagnosticInfo = ModifierUtils.CheckAccessibility(declarationModifiers2);
					if (cSDiagnosticInfo != null)
					{
						diagnostics.Add(cSDiagnosticInfo, self.Locations[0]);
						modifierErrors = true;
					}
				}
			}
			declarationModifiers = ((declarationModifiers != DeclarationModifiers.Unset) ? (declarationModifiers | declarationModifiers2) : declarationModifiers2);
		}
		if ((declarationModifiers & DeclarationModifiers.AccessibilityMask) == 0)
		{
			declarationModifiers |= defaultAccess;
		}
		if (flag)
		{
			if ((declarationModifiers & DeclarationModifiers.Partial) == 0)
			{
				switch (self.ContainingSymbol.Kind)
				{
				case SymbolKind.Namespace:
				{
					for (int k = 1; k < length; k++)
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateNameInNS, declaration.Declarations[k].NameLocation, self.Name, self.ContainingSymbol);
						modifierErrors = true;
					}
					break;
				}
				case SymbolKind.NamedType:
				{
					for (int j = 1; j < length; j++)
					{
						if (ContainingType.Locations.Length == 1 || ContainingType.IsPartial())
						{
							diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, declaration.Declarations[j].NameLocation, self.ContainingSymbol, self.Name);
						}
						modifierErrors = true;
					}
					break;
				}
				}
			}
			else
			{
				for (int l = 0; l < length; l++)
				{
					SingleTypeDeclaration singleTypeDeclaration = declaration.Declarations[l];
					if ((singleTypeDeclaration.Modifiers & DeclarationModifiers.Partial) == 0)
					{
						diagnostics.Add(ErrorCode.ERR_MissingPartial, singleTypeDeclaration.NameLocation, self.Name);
						modifierErrors = true;
					}
				}
			}
		}
		return declarationModifiers;
	}

	internal sealed override bool HasComplete(CompletionPart part)
	{
		return state.HasComplete(part);
	}

	protected abstract void CheckBase(DiagnosticBag diagnostics);

	protected abstract void CheckInterfaces(DiagnosticBag diagnostics);

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Attributes:
				GetAttributes();
				break;
			case CompletionPart.StartBaseType:
			case CompletionPart.FinishBaseType:
				if (state.NotePartComplete(CompletionPart.StartBaseType))
				{
					DiagnosticBag instance = DiagnosticBag.GetInstance();
					CheckBase(instance);
					AddDeclarationDiagnostics(instance);
					state.NotePartComplete(CompletionPart.FinishBaseType);
					instance.Free();
				}
				break;
			case CompletionPart.StartInterfaces:
			case CompletionPart.FinishInterfaces:
				if (state.NotePartComplete(CompletionPart.StartInterfaces))
				{
					DiagnosticBag instance3 = DiagnosticBag.GetInstance();
					CheckInterfaces(instance3);
					AddDeclarationDiagnostics(instance3);
					state.NotePartComplete(CompletionPart.FinishInterfaces);
					instance3.Free();
				}
				break;
			case CompletionPart.EnumUnderlyingType:
				_ = EnumUnderlyingType;
				break;
			case CompletionPart.TypeArguments:
				_ = TypeArgumentsNoUseSiteDiagnostics;
				break;
			case CompletionPart.TypeParameters:
				foreach (TypeParameterSymbol typeParameter in TypeParameters)
				{
					typeParameter.ForceComplete(locationOpt, cancellationToken);
				}
				state.NotePartComplete(CompletionPart.TypeParameters);
				break;
			case CompletionPart.Members:
				GetMembersByName();
				break;
			case CompletionPart.TypeMembers:
				GetTypeMembersUnordered();
				break;
			case CompletionPart.SynthesizedExplicitImplementations:
				GetSynthesizedExplicitImplementations(cancellationToken);
				break;
			case CompletionPart.StartMemberChecks:
			case CompletionPart.FinishMemberChecks:
				if (state.NotePartComplete(CompletionPart.StartMemberChecks))
				{
					DiagnosticBag instance2 = DiagnosticBag.GetInstance();
					AfterMembersChecks(instance2);
					AddDeclarationDiagnostics(instance2);
					DeclaringCompilation.SymbolDeclaredEvent(this);
					state.NotePartComplete(CompletionPart.FinishMemberChecks);
					instance2.Free();
				}
				break;
			case CompletionPart.MembersCompleted:
			{
				ImmutableArray<Symbol> membersUnordered = GetMembersUnordered();
				bool flag = true;
				if (locationOpt == null)
				{
					foreach (Symbol item in membersUnordered)
					{
						cancellationToken.ThrowIfCancellationRequested();
						item.ForceComplete(locationOpt, cancellationToken);
					}
				}
				else
				{
					foreach (Symbol item2 in membersUnordered)
					{
						Symbol.ForceCompleteMemberByLocation(locationOpt, item2, cancellationToken);
						flag = flag && item2.HasComplete(CompletionPart.All);
					}
				}
				if (!flag)
				{
					CompletionPart part = CompletionPart.NamedTypeSymbolWithLocationAll;
					state.SpinWaitComplete(part, cancellationToken);
					return;
				}
				EnsureFieldDefinitionsNoted();
				state.NotePartComplete(CompletionPart.MembersCompleted);
				break;
			}
			case CompletionPart.None:
				return;
			default:
				state.NotePartComplete(CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.Type);
				break;
			}
			state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal void EnsureFieldDefinitionsNoted()
	{
		if (!_flags.FieldDefinitionsNoted)
		{
			NoteFieldDefinitions();
		}
	}

	private void NoteFieldDefinitions()
	{
		lock (GetMembersAndInitializers())
		{
			if (_flags.FieldDefinitionsNoted)
			{
				return;
			}
			SourceAssemblySymbol sourceAssemblySymbol = (SourceAssemblySymbol)ContainingAssembly;
			Accessibility accessibility = EffectiveAccessibility();
			foreach (Symbol nonTypeNonIndexerMember in _lazyMembersAndInitializers.NonTypeNonIndexerMembers)
			{
				if (nonTypeNonIndexerMember.IsFieldOrFieldLikeEvent(out var field) && !field.IsConst && !field.IsFixed)
				{
					Accessibility declaredAccessibility = field.DeclaredAccessibility;
					if (declaredAccessibility == Accessibility.Private)
					{
						sourceAssemblySymbol.NoteFieldDefinition(field, isInternal: false, isUnread: true);
					}
					else if (accessibility == Accessibility.Private)
					{
						sourceAssemblySymbol.NoteFieldDefinition(field, isInternal: false, isUnread: false);
					}
					else if (declaredAccessibility == Accessibility.Internal || accessibility == Accessibility.Internal)
					{
						sourceAssemblySymbol.NoteFieldDefinition(field, isInternal: true, isUnread: false);
					}
				}
			}
			_flags.SetFieldDefinitionsNoted();
		}
	}

	private Accessibility EffectiveAccessibility()
	{
		Accessibility accessibility = DeclaredAccessibility;
		if (accessibility == Accessibility.Private)
		{
			return Accessibility.Private;
		}
		Symbol containingType = ContainingType;
		while ((object)containingType != null)
		{
			switch (containingType.DeclaredAccessibility)
			{
			case Accessibility.Private:
				return Accessibility.Private;
			case Accessibility.Internal:
				accessibility = Accessibility.Internal;
				break;
			}
			containingType = containingType.ContainingType;
		}
		return accessibility;
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		if (!_lazyLexicalSortKey.IsInitialized)
		{
			_lazyLexicalSortKey.SetFrom(declaration.GetLexicalSortKey(DeclaringCompilation));
		}
		return _lazyLexicalSortKey;
	}

	internal override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken)
	{
		ImmutableArray<SingleTypeDeclaration> declarations = declaration.Declarations;
		if (IsImplicitlyDeclared && declarations.IsEmpty)
		{
			return ContainingSymbol.IsDefinedInSourceTree(tree, definedWithinSpan, cancellationToken);
		}
		foreach (SingleTypeDeclaration item in declarations)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SyntaxReference syntaxReference = item.SyntaxReference;
			if (syntaxReference.SyntaxTree == tree && (!definedWithinSpan.HasValue || syntaxReference.Span.IntersectsWith(definedWithinSpan.Value)))
			{
				return true;
			}
		}
		return false;
	}

	internal int CalculateSyntaxOffsetInSynthesizedConstructor(int position, SyntaxTree tree, bool isStatic)
	{
		if (IsScriptClass && !isStatic)
		{
			int num = 0;
			foreach (SingleTypeDeclaration declaration in declaration.Declarations)
			{
				SyntaxReference syntaxReference = declaration.SyntaxReference;
				if (tree == syntaxReference.SyntaxTree)
				{
					return num + position;
				}
				num += syntaxReference.Span.Length;
			}
			throw ExceptionUtilities.Unreachable;
		}
		if (TryCalculateSyntaxOffsetOfPositionInInitializer(position, tree, isStatic, 0, out var syntaxOffset))
		{
			return syntaxOffset;
		}
		if (this.declaration.Declarations.Length >= 1 && position == this.declaration.Declarations[0].Location.SourceSpan.Start)
		{
			return 0;
		}
		throw ExceptionUtilities.Unreachable;
	}

	internal bool TryCalculateSyntaxOffsetOfPositionInInitializer(int position, SyntaxTree tree, bool isStatic, int ctorInitializerLength, out int syntaxOffset)
	{
		MembersAndInitializers membersAndInitializers = GetMembersAndInitializers();
		ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> initializers = (isStatic ? membersAndInitializers.StaticInitializers : membersAndInitializers.InstanceInitializers);
		ImmutableArray<FieldOrPropertyInitializer> initializersInSourceTree = GetInitializersInSourceTree(tree, initializers);
		int num = IndexOfInitializerContainingPosition(initializersInSourceTree, position);
		if (num < 0)
		{
			syntaxOffset = 0;
			return false;
		}
		int num2 = (isStatic ? membersAndInitializers.StaticInitializersSyntaxLength : membersAndInitializers.InstanceInitializersSyntaxLength);
		int num3 = position - initializersInSourceTree[num].Syntax.Span.Start;
		int num4 = num2 + ctorInitializerLength - (initializersInSourceTree[num].PrecedingInitializersLength + num3);
		syntaxOffset = -num4;
		return true;
	}

	private static ImmutableArray<FieldOrPropertyInitializer> GetInitializersInSourceTree(SyntaxTree tree, ImmutableArray<ImmutableArray<FieldOrPropertyInitializer>> initializers)
	{
		ArrayBuilder<FieldOrPropertyInitializer> instance = ArrayBuilder<FieldOrPropertyInitializer>.GetInstance();
		foreach (ImmutableArray<FieldOrPropertyInitializer> item in initializers)
		{
			if (item[0].Syntax.SyntaxTree == tree)
			{
				instance.AddRange(item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private static int IndexOfInitializerContainingPosition(ImmutableArray<FieldOrPropertyInitializer> initializers, int position)
	{
		int num = initializers.BinarySearch(position, (FieldOrPropertyInitializer initializer, int pos) => initializer.Syntax.Span.Start.CompareTo(pos));
		if (num >= 0)
		{
			return num;
		}
		int num2 = ~num - 1;
		if (num2 >= 0 && initializers[num2].Syntax.Span.Contains(position))
		{
			return num2;
		}
		return -1;
	}

	internal override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		return GetTypeMembersDictionary().Flatten();
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return GetTypeMembersDictionary().Flatten(LexicalOrderSymbolComparer.Instance);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		if (GetTypeMembersDictionary().TryGetValue(name, out var value))
		{
			return value;
		}
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol t) => t.Arity == arity);
	}

	private Dictionary<string, ImmutableArray<NamedTypeSymbol>> GetTypeMembersDictionary()
	{
		if (_lazyTypeMembers == null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			if (Interlocked.CompareExchange(ref _lazyTypeMembers, MakeTypeMembers(instance), null) == null)
			{
				AddDeclarationDiagnostics(instance);
				state.NotePartComplete(CompletionPart.TypeMembers);
			}
			instance.Free();
		}
		return _lazyTypeMembers;
	}

	private Dictionary<string, ImmutableArray<NamedTypeSymbol>> MakeTypeMembers(DiagnosticBag diagnostics)
	{
		ArrayBuilder<NamedTypeSymbol> instance = ArrayBuilder<NamedTypeSymbol>.GetInstance();
		Dictionary<(string, int), SourceNamedTypeSymbol> dictionary = new Dictionary<(string, int), SourceNamedTypeSymbol>();
		try
		{
			foreach (MergedTypeDeclaration child in declaration.Children)
			{
				SourceNamedTypeSymbol sourceNamedTypeSymbol = new SourceNamedTypeSymbol(this, child, diagnostics);
				CheckMemberNameDistinctFromType(sourceNamedTypeSymbol, diagnostics);
				(string, int) key = (sourceNamedTypeSymbol.Name, sourceNamedTypeSymbol.Arity);
				if (dictionary.TryGetValue(key, out var value))
				{
					if (Locations.Length == 1 || IsPartial)
					{
						if (sourceNamedTypeSymbol.IsPartial && value.IsPartial)
						{
							diagnostics.Add(ErrorCode.ERR_PartialTypeKindConflict, sourceNamedTypeSymbol.Locations[0], sourceNamedTypeSymbol);
						}
						else
						{
							diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, sourceNamedTypeSymbol.Locations[0], this, sourceNamedTypeSymbol.Name);
						}
					}
				}
				else
				{
					dictionary.Add(key, sourceNamedTypeSymbol);
				}
				instance.Add(sourceNamedTypeSymbol);
			}
			if (IsInterface)
			{
				foreach (NamedTypeSymbol item in instance)
				{
					diagnostics.Add(ErrorCode.ERR_InterfacesCannotContainTypes, item.Locations[0], item);
				}
			}
			return (instance.Count > 0) ? instance.ToDictionary((NamedTypeSymbol s) => s.Name, StringOrdinalComparer.Instance) : s_emptyTypeMembers;
		}
		finally
		{
			instance.Free();
		}
	}

	private void CheckMemberNameDistinctFromType(Symbol member, DiagnosticBag diagnostics)
	{
		TypeKind typeKind = TypeKind;
		if ((typeKind == TypeKind.Class || typeKind == TypeKind.Struct) && member.Name == Name)
		{
			diagnostics.Add(ErrorCode.ERR_MemberNameSameAsType, member.Locations[0], Name);
		}
	}

	internal override ImmutableArray<Symbol> GetMembersUnordered()
	{
		ImmutableArray<Symbol> lazyMembersFlattened = _lazyMembersFlattened;
		if (lazyMembersFlattened.IsDefault)
		{
			lazyMembersFlattened = GetMembersByName().Flatten();
			ImmutableInterlocked.InterlockedInitialize(ref _lazyMembersFlattened, lazyMembersFlattened);
			lazyMembersFlattened = _lazyMembersFlattened;
		}
		return lazyMembersFlattened;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (_flags.FlattenedMembersIsSorted)
		{
			return _lazyMembersFlattened;
		}
		ImmutableArray<Symbol> immutableArray = GetMembersUnordered();
		if (immutableArray.Length > 1)
		{
			immutableArray = immutableArray.Sort(LexicalOrderSymbolComparer.Instance);
			ImmutableInterlocked.InterlockedExchange(ref _lazyMembersFlattened, immutableArray);
		}
		_flags.SetFlattenedMembersIsSorted();
		return immutableArray;
	}

	public sealed override ImmutableArray<Symbol> GetMembers(string name)
	{
		if (GetMembersByName().TryGetValue(name, out var value))
		{
			return value;
		}
		return ImmutableArray<Symbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetSimpleNonTypeMembers(string name)
	{
		if (_lazyMembersDictionary != null || declaration.MemberNames.Contains(name))
		{
			return GetMembers(name);
		}
		return ImmutableArray<Symbol>.Empty;
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		if (TypeKind == TypeKind.Enum)
		{
			yield return ((SourceNamedTypeSymbol)this).EnumValueField;
		}
		foreach (Symbol member in GetMembers())
		{
			switch (member.Kind)
			{
			case SymbolKind.Field:
				yield return (FieldSymbol)member;
				break;
			case SymbolKind.Event:
			{
				FieldSymbol associatedField = ((EventSymbol)member).AssociatedField;
				if ((object)associatedField != null)
				{
					yield return associatedField;
				}
				break;
			}
			}
		}
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers()
	{
		return GetEarlyAttributeDecodingMembersDictionary().Flatten();
	}

	internal override ImmutableArray<Symbol> GetEarlyAttributeDecodingMembers(string name)
	{
		if (!GetEarlyAttributeDecodingMembersDictionary().TryGetValue(name, out var value))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return value;
	}

	private Dictionary<string, ImmutableArray<Symbol>> GetEarlyAttributeDecodingMembersDictionary()
	{
		if (_lazyEarlyAttributeDecodingMembersDictionary == null)
		{
			Dictionary<string, ImmutableArray<Symbol>> dictionary = GetMembersAndInitializers().NonTypeNonIndexerMembers.ToDictionary((Symbol s) => s.Name);
			AddNestedTypesToDictionary(dictionary, GetTypeMembersDictionary());
			Interlocked.CompareExchange(ref _lazyEarlyAttributeDecodingMembersDictionary, dictionary, null);
		}
		return _lazyEarlyAttributeDecodingMembersDictionary;
	}

	private MembersAndInitializers GetMembersAndInitializers()
	{
		MembersAndInitializers lazyMembersAndInitializers = _lazyMembersAndInitializers;
		if (lazyMembersAndInitializers != null)
		{
			return lazyMembersAndInitializers;
		}
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		lazyMembersAndInitializers = BuildMembersAndInitializers(instance);
		MembersAndInitializers membersAndInitializers = Interlocked.CompareExchange(ref _lazyMembersAndInitializers, lazyMembersAndInitializers, null);
		if (membersAndInitializers != null)
		{
			instance.Free();
			return membersAndInitializers;
		}
		AddDeclarationDiagnostics(instance);
		instance.Free();
		return lazyMembersAndInitializers;
	}

	protected Dictionary<string, ImmutableArray<Symbol>> GetMembersByName()
	{
		if (state.HasComplete(CompletionPart.Members))
		{
			return _lazyMembersDictionary;
		}
		return GetMembersByNameSlow();
	}

	private Dictionary<string, ImmutableArray<Symbol>> GetMembersByNameSlow()
	{
		if (_lazyMembersDictionary == null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			Dictionary<string, ImmutableArray<Symbol>> dictionary = MakeAllMembers(instance);
			if (Interlocked.CompareExchange(ref _lazyMembersDictionary, dictionary, null) == null)
			{
				ArrayBuilder<string> instance2 = ArrayBuilder<string>.GetInstance(dictionary.Count);
				instance2.AddRange(dictionary.Keys);
				MergePartialMembers(instance2, dictionary, instance);
				instance2.Free();
				AddDeclarationDiagnostics(instance);
				state.NotePartComplete(CompletionPart.Members);
			}
			instance.Free();
		}
		state.SpinWaitComplete(CompletionPart.Members, default);
		return _lazyMembersDictionary;
	}

	internal override IEnumerable<Symbol> GetInstanceFieldsAndEvents()
	{
		return GetMembersAndInitializers().NonTypeNonIndexerMembers.Where(NamedTypeSymbol.IsInstanceFieldOrEvent);
	}

	protected void AfterMembersChecks(DiagnosticBag diagnostics)
	{
		if (IsInterface)
		{
			CheckInterfaceMembers(GetMembersAndInitializers().NonTypeNonIndexerMembers, diagnostics);
		}
		CheckMemberNamesDistinctFromType(diagnostics);
		CheckMemberNameConflicts(diagnostics);
		CheckSpecialMemberErrors(diagnostics);
		CheckTypeParameterNameConflicts(diagnostics);
		CheckAccessorNameConflicts(diagnostics);
		_ = KnownCircularStruct;
		CheckSequentialOnPartialType(diagnostics);
		CheckForProtectedInStaticClass(diagnostics);
		CheckForUnmatchedOperators(diagnostics);
		if (IsByRefLikeType)
		{
			DeclaringCompilation.EnsureIsByRefLikeAttributeExists(diagnostics, Locations[0], modifyCompilationForIsByRefLike: true);
		}
		if (IsReadOnly)
		{
			DeclaringCompilation.EnsureIsReadOnlyAttributeExists(diagnostics, Locations[0], modifyCompilationForRefReadOnly: true);
		}
	}

	private void CheckMemberNamesDistinctFromType(DiagnosticBag diagnostics)
	{
		foreach (Symbol nonTypeNonIndexerMember in GetMembersAndInitializers().NonTypeNonIndexerMembers)
		{
			CheckMemberNameDistinctFromType(nonTypeNonIndexerMember, diagnostics);
		}
	}

	private void CheckMemberNameConflicts(DiagnosticBag diagnostics)
	{
		Dictionary<string, ImmutableArray<Symbol>> membersByName = GetMembersByName();
		CheckIndexerNameConflicts(diagnostics, membersByName);
		Dictionary<SourceMemberMethodSymbol, SourceMemberMethodSymbol> dictionary = new Dictionary<SourceMemberMethodSymbol, SourceMemberMethodSymbol>(MemberSignatureComparer.DuplicateSourceComparer);
		Dictionary<SourceMemberMethodSymbol, SourceMemberMethodSymbol> dictionary2 = new Dictionary<SourceMemberMethodSymbol, SourceMemberMethodSymbol>(MemberSignatureComparer.DuplicateSourceComparer);
		HashSet<SourceUserDefinedConversionSymbol> hashSet = new HashSet<SourceUserDefinedConversionSymbol>(ConversionSignatureComparer.Comparer);
		foreach (KeyValuePair<string, ImmutableArray<Symbol>> item in membersByName)
		{
			string key = item.Key;
			Symbol symbol = GetTypeMembers(key).FirstOrDefault();
			dictionary.Clear();
			foreach (Symbol item2 in item.Value)
			{
				if (item2.Kind == SymbolKind.NamedType || item2.IsAccessor() || item2.IsIndexer())
				{
					continue;
				}
				if ((object)symbol != null)
				{
					if (item2.Kind != SymbolKind.Method || symbol.Kind != SymbolKind.Method)
					{
						if ((item2.Kind != SymbolKind.Field || !item2.IsImplicitlyDeclared) && (Locations.Length == 1 || IsPartial))
						{
							diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, item2.Locations[0], this, item2.Name);
						}
						if (symbol.Kind == SymbolKind.Method)
						{
							symbol = item2;
						}
					}
				}
				else
				{
					symbol = item2;
				}
				SourceUserDefinedConversionSymbol sourceUserDefinedConversionSymbol = item2 as SourceUserDefinedConversionSymbol;
				SourceMemberMethodSymbol sourceMemberMethodSymbol = item2 as SourceMemberMethodSymbol;
				if ((object)sourceUserDefinedConversionSymbol != null)
				{
					if (!hashSet.Add(sourceUserDefinedConversionSymbol))
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateConversionInClass, sourceUserDefinedConversionSymbol.Locations[0], this);
					}
					else if (!dictionary2.ContainsKey(sourceUserDefinedConversionSymbol))
					{
						dictionary2.Add(sourceUserDefinedConversionSymbol, sourceUserDefinedConversionSymbol);
					}
					if (dictionary.TryGetValue(sourceUserDefinedConversionSymbol, out var value))
					{
						ReportMethodSignatureCollision(diagnostics, sourceUserDefinedConversionSymbol, value);
					}
				}
				else if ((object)sourceMemberMethodSymbol != null)
				{
					if (dictionary2.TryGetValue(sourceMemberMethodSymbol, out var value2))
					{
						ReportMethodSignatureCollision(diagnostics, sourceMemberMethodSymbol, value2);
					}
					if (dictionary.TryGetValue(sourceMemberMethodSymbol, out var value3))
					{
						ReportMethodSignatureCollision(diagnostics, sourceMemberMethodSymbol, value3);
					}
					else
					{
						dictionary.Add(sourceMemberMethodSymbol, sourceMemberMethodSymbol);
					}
				}
			}
		}
	}

	private void ReportMethodSignatureCollision(DiagnosticBag diagnostics, SourceMemberMethodSymbol method1, SourceMemberMethodSymbol method2)
	{
		if ((method1.IsPartial && method2.IsPartial) || (method1.MethodKind == MethodKind.Constructor && ((ConstructorDeclarationSyntax)method1.SyntaxRef.GetSyntax()).Identifier.ValueText != Name))
		{
			return;
		}
		for (int i = 0; i < method1.ParameterCount; i++)
		{
			RefKind refKind = method1.Parameters[i].RefKind;
			RefKind refKind2 = method2.Parameters[i].RefKind;
			if (refKind != refKind2)
			{
				MessageID id = ((method1.MethodKind == MethodKind.Constructor) ? MessageID.IDS_SK_CONSTRUCTOR : MessageID.IDS_SK_METHOD);
				diagnostics.Add(ErrorCode.ERR_OverloadRefKind, method1.Locations[0], this, id.Localize(), refKind.ToParameterDisplayString(), refKind2.ToParameterDisplayString());
				return;
			}
		}
		string text = ((method1.MethodKind == MethodKind.Destructor && method2.MethodKind == MethodKind.Destructor) ? ("~" + Name) : method1.Name);
		diagnostics.Add(ErrorCode.ERR_MemberAlreadyExists, method1.Locations[0], text, this);
	}

	private void CheckIndexerNameConflicts(DiagnosticBag diagnostics, Dictionary<string, ImmutableArray<Symbol>> membersByName)
	{
		PooledHashSet<string> pooledHashSet = null;
		if (Arity > 0)
		{
			pooledHashSet = PooledHashSet<string>.GetInstance();
			foreach (TypeParameterSymbol typeParameter in TypeParameters)
			{
				pooledHashSet.Add(typeParameter.Name);
			}
		}
		Dictionary<PropertySymbol, PropertySymbol> dictionary = new Dictionary<PropertySymbol, PropertySymbol>(MemberSignatureComparer.DuplicateSourceComparer);
		foreach (ImmutableArray<Symbol> value in membersByName.Values)
		{
			string lastIndexerName = null;
			dictionary.Clear();
			foreach (Symbol item in value)
			{
				if (!item.IsIndexer())
				{
					continue;
				}
				PropertySymbol propertySymbol = (PropertySymbol)item;
				CheckIndexerSignatureCollisions(propertySymbol, diagnostics, membersByName, dictionary, ref lastIndexerName);
				if (pooledHashSet != null)
				{
					string metadataName = propertySymbol.MetadataName;
					if (pooledHashSet.Contains(metadataName))
					{
						diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, propertySymbol.Locations[0], this, metadataName);
					}
				}
			}
		}
		pooledHashSet?.Free();
	}

	private void CheckIndexerSignatureCollisions(PropertySymbol indexer, DiagnosticBag diagnostics, Dictionary<string, ImmutableArray<Symbol>> membersByName, Dictionary<PropertySymbol, PropertySymbol> indexersBySignature, ref string lastIndexerName)
	{
		if (!indexer.IsExplicitInterfaceImplementation)
		{
			string metadataName = indexer.MetadataName;
			if (lastIndexerName != null && lastIndexerName != metadataName)
			{
				diagnostics.Add(ErrorCode.ERR_InconsistentIndexerNames, indexer.Locations[0]);
			}
			lastIndexerName = metadataName;
			if ((Locations.Length == 1 || IsPartial) && membersByName.ContainsKey(metadataName))
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, indexer.Locations[0], this, metadataName);
			}
		}
		if (indexersBySignature.TryGetValue(indexer, out var _))
		{
			diagnostics.Add(ErrorCode.ERR_MemberAlreadyExists, indexer.Locations[0], SyntaxFacts.GetText(SyntaxKind.ThisKeyword), this);
		}
		else
		{
			indexersBySignature[indexer] = indexer;
		}
	}

	private void CheckSpecialMemberErrors(DiagnosticBag diagnostics)
	{
		TypeConversions conversions = new TypeConversions(ContainingAssembly.CorLibrary);
		foreach (Symbol item in GetMembersUnordered())
		{
			item.AfterAddingTypeMembersChecks(conversions, diagnostics);
		}
	}

	private void CheckTypeParameterNameConflicts(DiagnosticBag diagnostics)
	{
		if (TypeKind == TypeKind.Delegate || (Locations.Length != 1 && !IsPartial))
		{
			return;
		}
		foreach (TypeParameterSymbol typeParameter in TypeParameters)
		{
			foreach (Symbol member in GetMembers(typeParameter.Name))
			{
				diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, member.Locations[0], this, typeParameter.Name);
			}
		}
	}

	private void CheckAccessorNameConflicts(DiagnosticBag diagnostics)
	{
		foreach (Symbol item in GetMembersUnordered())
		{
			if (!item.IsExplicitInterfaceImplementation())
			{
				switch (item.Kind)
				{
				case SymbolKind.Property:
				{
					PropertySymbol propertySymbol = (PropertySymbol)item;
					CheckForMemberConflictWithPropertyAccessor(propertySymbol, getNotSet: true, diagnostics);
					CheckForMemberConflictWithPropertyAccessor(propertySymbol, getNotSet: false, diagnostics);
					break;
				}
				case SymbolKind.Event:
				{
					EventSymbol eventSymbol = (EventSymbol)item;
					CheckForMemberConflictWithEventAccessor(eventSymbol, isAdder: true, diagnostics);
					CheckForMemberConflictWithEventAccessor(eventSymbol, isAdder: false, diagnostics);
					break;
				}
				}
			}
		}
	}

	private bool CheckStructCircularity(DiagnosticBag diagnostics)
	{
		CheckFiniteFlatteningGraph(diagnostics);
		return HasStructCircularity(diagnostics);
	}

	private bool HasStructCircularity(DiagnosticBag diagnostics)
	{
		foreach (ImmutableArray<Symbol> value in GetMembersByName().Values)
		{
			foreach (Symbol item in value)
			{
				if (item.Kind != SymbolKind.Field)
				{
					continue;
				}
				FieldSymbol fieldSymbol = (FieldSymbol)item;
				if (fieldSymbol.IsStatic)
				{
					continue;
				}
				TypeSymbol type = fieldSymbol.Type;
				if ((object)type != null && type.TypeKind == TypeKind.Struct && BaseTypeAnalysis.StructDependsOn((NamedTypeSymbol)type, this) && !type.IsPrimitiveRecursiveStruct())
				{
					Symbol symbol = fieldSymbol.AssociatedSymbol ?? fieldSymbol;
					if (symbol.Kind == SymbolKind.Parameter)
					{
						symbol = fieldSymbol;
					}
					diagnostics.Add(ErrorCode.ERR_StructLayoutCycle, symbol.Locations[0], symbol, type);
					return true;
				}
			}
		}
		return false;
	}

	private void CheckForProtectedInStaticClass(DiagnosticBag diagnostics)
	{
		if (!IsStatic)
		{
			return;
		}
		foreach (ImmutableArray<Symbol> value in GetMembersByName().Values)
		{
			foreach (Symbol item in value)
			{
				if (!(item is TypeSymbol) && item.DeclaredAccessibility.HasProtected() && (item.Kind != SymbolKind.Method || ((MethodSymbol)item).MethodKind != MethodKind.Destructor))
				{
					diagnostics.Add(ErrorCode.ERR_ProtectedInStatic, item.Locations[0], item);
				}
			}
		}
	}

	private void CheckForUnmatchedOperators(DiagnosticBag diagnostics)
	{
		CheckForUnmatchedOperator(diagnostics, "op_True", "op_False");
		CheckForUnmatchedOperator(diagnostics, "op_Equality", "op_Inequality");
		CheckForUnmatchedOperator(diagnostics, "op_LessThan", "op_GreaterThan");
		CheckForUnmatchedOperator(diagnostics, "op_LessThanOrEqual", "op_GreaterThanOrEqual");
		CheckForEqualityAndGetHashCode(diagnostics);
	}

	private void CheckForUnmatchedOperator(DiagnosticBag diagnostics, string operatorName1, string operatorName2)
	{
		ImmutableArray<MethodSymbol> operators = GetOperators(operatorName1);
		ImmutableArray<MethodSymbol> operators2 = GetOperators(operatorName2);
		CheckForUnmatchedOperator(diagnostics, operators, operators2, operatorName2);
		CheckForUnmatchedOperator(diagnostics, operators2, operators, operatorName1);
	}

	private static void CheckForUnmatchedOperator(DiagnosticBag diagnostics, ImmutableArray<MethodSymbol> ops1, ImmutableArray<MethodSymbol> ops2, string operatorName2)
	{
		foreach (MethodSymbol item in ops1)
		{
			bool flag = false;
			foreach (MethodSymbol item2 in ops2)
			{
				flag = DoOperatorsPair(item, item2);
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				diagnostics.Add(ErrorCode.ERR_OperatorNeedsMatch, item.Locations[0], item, SyntaxFacts.GetText(SyntaxFacts.GetOperatorKind(operatorName2)));
			}
		}
	}

	private static bool DoOperatorsPair(MethodSymbol op1, MethodSymbol op2)
	{
		if (op1.ParameterCount != op2.ParameterCount)
		{
			return false;
		}
		for (int i = 0; i < op1.ParameterCount; i++)
		{
			if (!op1.ParameterTypes[i].Equals(op2.ParameterTypes[i], TypeCompareKind.AllIgnoreOptions))
			{
				return false;
			}
		}
		if (!op1.ReturnType.Equals(op2.ReturnType, TypeCompareKind.AllIgnoreOptions))
		{
			return false;
		}
		return true;
	}

	private void CheckForEqualityAndGetHashCode(DiagnosticBag diagnostics)
	{
		if (this.IsInterfaceType())
		{
			return;
		}
		bool flag = GetOperators("op_Equality").Any() || GetOperators("op_Inequality").Any();
		bool flag2 = TypeOverridesObjectMethod("Equals");
		if (flag | flag2)
		{
			bool flag3 = TypeOverridesObjectMethod("GetHashCode");
			if (flag2 && !flag3)
			{
				diagnostics.Add(ErrorCode.WRN_EqualsWithoutGetHashCode, Locations[0], this);
			}
			if (flag && !flag2)
			{
				diagnostics.Add(ErrorCode.WRN_EqualityOpWithoutEquals, Locations[0], this);
			}
			if (flag && !flag3)
			{
				diagnostics.Add(ErrorCode.WRN_EqualityOpWithoutGetHashCode, Locations[0], this);
			}
		}
	}

	private bool TypeOverridesObjectMethod(string name)
	{
		foreach (MethodSymbol item in GetMembers(name).OfType<MethodSymbol>())
		{
			if (item.IsOverride && item.GetConstructedLeastOverriddenMethod(this).ContainingType.SpecialType == SpecialType.System_Object)
			{
				return true;
			}
		}
		return false;
	}

	private void CheckFiniteFlatteningGraph(DiagnosticBag diagnostics)
	{
		if (AllTypeArgumentCount() == 0)
		{
			return;
		}
		Dictionary<NamedTypeSymbol, NamedTypeSymbol> dictionary = new Dictionary<NamedTypeSymbol, NamedTypeSymbol>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);
		dictionary.Add(this, this);
		foreach (Symbol item in GetMembersUnordered())
		{
			if (item is FieldSymbol { IsStatic: not false } fieldSymbol && fieldSymbol.Type.TypeKind == TypeKind.Struct)
			{
				NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)fieldSymbol.Type;
				if (InfiniteFlatteningGraph(this, namedTypeSymbol, dictionary))
				{
					diagnostics.Add(ErrorCode.ERR_StructLayoutCycle, fieldSymbol.Locations[0], fieldSymbol, namedTypeSymbol);
					break;
				}
			}
		}
	}

	private static bool InfiniteFlatteningGraph(SourceMemberContainerTypeSymbol top, NamedTypeSymbol t, Dictionary<NamedTypeSymbol, NamedTypeSymbol> instanceMap)
	{
		if (!t.ContainsTypeParameter())
		{
			return false;
		}
		NamedTypeSymbol originalDefinition = t.OriginalDefinition;
		if (instanceMap.TryGetValue(originalDefinition, out var value))
		{
			if (value != t)
			{
				return (object)originalDefinition == top;
			}
			return false;
		}
		instanceMap.Add(originalDefinition, t);
		try
		{
			foreach (Symbol item in t.GetMembersUnordered())
			{
				if (item is FieldSymbol { IsStatic: not false } fieldSymbol && fieldSymbol.Type.TypeKind == TypeKind.Struct)
				{
					NamedTypeSymbol t2 = (NamedTypeSymbol)fieldSymbol.Type;
					if (InfiniteFlatteningGraph(top, t2, instanceMap))
					{
						return true;
					}
				}
			}
			return false;
		}
		finally
		{
			instanceMap.Remove(originalDefinition);
		}
	}

	private void CheckSequentialOnPartialType(DiagnosticBag diagnostics)
	{
		if (!IsPartial || Layout.Kind != LayoutKind.Sequential)
		{
			return;
		}
		SyntaxReference syntaxReference = null;
		if (SyntaxReferences.Length <= 1)
		{
			return;
		}
		foreach (SyntaxReference syntaxReference2 in SyntaxReferences)
		{
			if (!(syntaxReference2.GetSyntax() is TypeDeclarationSyntax { Members: var members }))
			{
				continue;
			}
			foreach (MemberDeclarationSyntax item in members)
			{
				if (HasInstanceData(item))
				{
					if (syntaxReference != null && syntaxReference != syntaxReference2)
					{
						diagnostics.Add(ErrorCode.WRN_SequentialOnPartialClass, Locations[0], this);
						return;
					}
					syntaxReference = syntaxReference2;
				}
			}
		}
	}

	private static bool HasInstanceData(MemberDeclarationSyntax m)
	{
		switch (m.Kind())
		{
		case SyntaxKind.FieldDeclaration:
		{
			FieldDeclarationSyntax fieldDeclarationSyntax = (FieldDeclarationSyntax)m;
			if (!ContainsModifier(fieldDeclarationSyntax.Modifiers, SyntaxKind.StaticKeyword))
			{
				return !ContainsModifier(fieldDeclarationSyntax.Modifiers, SyntaxKind.ConstKeyword);
			}
			return false;
		}
		case SyntaxKind.PropertyDeclaration:
		{
			PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)m;
			if (!ContainsModifier(propertyDeclarationSyntax.Modifiers, SyntaxKind.StaticKeyword) && !ContainsModifier(propertyDeclarationSyntax.Modifiers, SyntaxKind.AbstractKeyword) && !ContainsModifier(propertyDeclarationSyntax.Modifiers, SyntaxKind.ExternKeyword) && propertyDeclarationSyntax.AccessorList != null)
			{
				return All(propertyDeclarationSyntax.AccessorList.Accessors, (AccessorDeclarationSyntax a) => a.Body == null && a.ExpressionBody == null);
			}
			return false;
		}
		case SyntaxKind.EventFieldDeclaration:
		{
			EventFieldDeclarationSyntax eventFieldDeclarationSyntax = (EventFieldDeclarationSyntax)m;
			if (!ContainsModifier(eventFieldDeclarationSyntax.Modifiers, SyntaxKind.StaticKeyword) && !ContainsModifier(eventFieldDeclarationSyntax.Modifiers, SyntaxKind.AbstractKeyword))
			{
				return !ContainsModifier(eventFieldDeclarationSyntax.Modifiers, SyntaxKind.ExternKeyword);
			}
			return false;
		}
		default:
			return false;
		}
	}

	private static bool All<T>(SyntaxList<T> list, Func<T, bool> predicate) where T : CSharpSyntaxNode
	{
		foreach (T item in list)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	private static bool ContainsModifier(SyntaxTokenList modifiers, SyntaxKind modifier)
	{
		foreach (SyntaxToken item in modifiers)
		{
			if (item.IsKind(modifier))
			{
				return true;
			}
		}
		return false;
	}

	private Dictionary<string, ImmutableArray<Symbol>> MakeAllMembers(DiagnosticBag diagnostics)
	{
		MembersAndInitializers membersAndInitializers = GetMembersAndInitializers();
		if (membersAndInitializers.IndexerDeclarations.Length == 0)
		{
			return GetEarlyAttributeDecodingMembersDictionary();
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		Binder binder = null;
		SyntaxTree syntaxTree = null;
		foreach (SyntaxReference indexerDeclaration in membersAndInitializers.IndexerDeclarations)
		{
			IndexerDeclarationSyntax indexerDeclarationSyntax = (IndexerDeclarationSyntax)indexerDeclaration.GetSyntax();
			if (binder == null || syntaxTree != indexerDeclaration.SyntaxTree)
			{
				syntaxTree = indexerDeclaration.SyntaxTree;
				binder = DeclaringCompilation.GetBinderFactory(syntaxTree).GetBinder(indexerDeclarationSyntax);
			}
			SourcePropertySymbol sourcePropertySymbol = SourcePropertySymbol.Create(this, binder, indexerDeclarationSyntax, diagnostics);
			CheckMemberNameDistinctFromType(sourcePropertySymbol, diagnostics);
			instance.Add(sourcePropertySymbol);
			AddAccessorIfAvailable(instance, sourcePropertySymbol.GetMethod, diagnostics, checkName: true);
			AddAccessorIfAvailable(instance, sourcePropertySymbol.SetMethod, diagnostics, checkName: true);
		}
		Dictionary<string, ImmutableArray<Symbol>> dictionary = MergeIndexersAndNonIndexers(membersAndInitializers.NonTypeNonIndexerMembers, instance);
		instance.Free();
		AddNestedTypesToDictionary(dictionary, GetTypeMembersDictionary());
		return dictionary;
	}

	private static Dictionary<string, ImmutableArray<Symbol>> MergeIndexersAndNonIndexers(ImmutableArray<Symbol> nonIndexerMembers, ArrayBuilder<Symbol> indexerMembers)
	{
		int length = nonIndexerMembers.Length;
		int count = indexerMembers.Count;
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(length + count);
		int i = 0;
		int j = 0;
		while (i < length && j < count)
		{
			Symbol symbol = nonIndexerMembers[i];
			Symbol symbol2 = indexerMembers[j];
			if (LexicalOrderSymbolComparer.Instance.Compare(symbol, symbol2) < 0)
			{
				instance.Add(symbol);
				i++;
			}
			else
			{
				instance.Add(symbol2);
				j++;
			}
		}
		for (; i < length; i++)
		{
			instance.Add(nonIndexerMembers[i]);
		}
		for (; j < count; j++)
		{
			instance.Add(indexerMembers[j]);
		}
		Dictionary<string, ImmutableArray<Symbol>> result = instance.ToDictionary((Symbol s) => s.Name, StringOrdinalComparer.Instance);
		instance.Free();
		return result;
	}

	private static void AddNestedTypesToDictionary(Dictionary<string, ImmutableArray<Symbol>> membersByName, Dictionary<string, ImmutableArray<NamedTypeSymbol>> typesByName)
	{
		foreach (KeyValuePair<string, ImmutableArray<NamedTypeSymbol>> item in typesByName)
		{
			string key = item.Key;
			ImmutableArray<Symbol> immutableArray = StaticCast<Symbol>.From(item.Value);
			if (membersByName.TryGetValue(key, out var value))
			{
				membersByName[key] = value.Concat(immutableArray);
			}
			else
			{
				membersByName.Add(key, immutableArray);
			}
		}
	}

	private MembersAndInitializers BuildMembersAndInitializers(DiagnosticBag diagnostics)
	{
		MembersAndInitializersBuilder membersAndInitializersBuilder = new MembersAndInitializersBuilder();
		AddDeclaredNontypeMembers(membersAndInitializersBuilder, diagnostics);
		switch (TypeKind)
		{
		case TypeKind.Struct:
			CheckForStructBadInitializers(membersAndInitializersBuilder, diagnostics);
			CheckForStructDefaultConstructors(membersAndInitializersBuilder.NonTypeNonIndexerMembers, isEnum: false, diagnostics);
			AddSynthesizedConstructorsIfNecessary(membersAndInitializersBuilder.NonTypeNonIndexerMembers, membersAndInitializersBuilder.StaticInitializers, diagnostics);
			break;
		case TypeKind.Enum:
			CheckForStructDefaultConstructors(membersAndInitializersBuilder.NonTypeNonIndexerMembers, isEnum: true, diagnostics);
			AddSynthesizedConstructorsIfNecessary(membersAndInitializersBuilder.NonTypeNonIndexerMembers, membersAndInitializersBuilder.StaticInitializers, diagnostics);
			break;
		case TypeKind.Class:
		case TypeKind.Submission:
			AddSynthesizedConstructorsIfNecessary(membersAndInitializersBuilder.NonTypeNonIndexerMembers, membersAndInitializersBuilder.StaticInitializers, diagnostics);
			break;
		}
		if (_lazyMembersAndInitializers != null)
		{
			membersAndInitializersBuilder.Free();
			return null;
		}
		return membersAndInitializersBuilder.ToReadOnlyAndFree();
	}

	private void AddDeclaredNontypeMembers(MembersAndInitializersBuilder builder, DiagnosticBag diagnostics)
	{
		foreach (SingleTypeDeclaration declaration in declaration.Declarations)
		{
			if (declaration.HasAnyNontypeMembers)
			{
				if (_lazyMembersAndInitializers != null)
				{
					break;
				}
				SyntaxNode syntax = declaration.SyntaxReference.GetSyntax();
				switch (syntax.Kind())
				{
				case SyntaxKind.EnumDeclaration:
					AddEnumMembers(builder, (EnumDeclarationSyntax)syntax, diagnostics);
					break;
				case SyntaxKind.DelegateDeclaration:
					SourceDelegateMethodSymbol.AddDelegateMembers(this, builder.NonTypeNonIndexerMembers, (DelegateDeclarationSyntax)syntax, diagnostics);
					break;
				case SyntaxKind.NamespaceDeclaration:
					AddNonTypeMembers(builder, ((NamespaceDeclarationSyntax)syntax).Members, diagnostics);
					break;
				case SyntaxKind.CompilationUnit:
					AddNonTypeMembers(builder, ((CompilationUnitSyntax)syntax).Members, diagnostics);
					break;
				case SyntaxKind.ClassDeclaration:
				{
					ClassDeclarationSyntax classDeclarationSyntax = (ClassDeclarationSyntax)syntax;
					AddNonTypeMembers(builder, classDeclarationSyntax.Members, diagnostics);
					break;
				}
				case SyntaxKind.InterfaceDeclaration:
					AddNonTypeMembers(builder, ((InterfaceDeclarationSyntax)syntax).Members, diagnostics);
					break;
				case SyntaxKind.StructDeclaration:
				{
					StructDeclarationSyntax structDeclarationSyntax = (StructDeclarationSyntax)syntax;
					AddNonTypeMembers(builder, structDeclarationSyntax.Members, diagnostics);
					break;
				}
				default:
					throw ExceptionUtilities.UnexpectedValue(syntax.Kind());
				}
			}
		}
	}

	internal Binder GetBinder(CSharpSyntaxNode syntaxNode)
	{
		return DeclaringCompilation.GetBinder(syntaxNode);
	}

	private static void MergePartialMembers(ArrayBuilder<string> memberNames, Dictionary<string, ImmutableArray<Symbol>> membersByName, DiagnosticBag diagnostics)
	{
		Dictionary<MethodSymbol, SourceMemberMethodSymbol> dictionary = new Dictionary<MethodSymbol, SourceMemberMethodSymbol>(MemberSignatureComparer.PartialMethodsComparer);
		foreach (string memberName in memberNames)
		{
			dictionary.Clear();
			foreach (Symbol item in membersByName[memberName])
			{
				if (!(item is SourceMemberMethodSymbol { IsPartial: not false } sourceMemberMethodSymbol))
				{
					continue;
				}
				if (dictionary.TryGetValue(sourceMemberMethodSymbol, out var value))
				{
					SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol = (SourceOrdinaryMethodSymbol)value;
					SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol2 = (SourceOrdinaryMethodSymbol)sourceMemberMethodSymbol;
					bool num = (object)sourceOrdinaryMethodSymbol.OtherPartOfPartial != null || sourceOrdinaryMethodSymbol.IsPartialImplementation;
					bool flag = (object)sourceOrdinaryMethodSymbol.OtherPartOfPartial != null || sourceOrdinaryMethodSymbol.IsPartialDefinition;
					if (num && sourceOrdinaryMethodSymbol2.IsPartialImplementation)
					{
						diagnostics.Add(ErrorCode.ERR_PartialMethodOnlyOneActual, sourceOrdinaryMethodSymbol2.Locations[0]);
					}
					else if (flag && sourceOrdinaryMethodSymbol2.IsPartialDefinition)
					{
						diagnostics.Add(ErrorCode.ERR_PartialMethodOnlyOneLatent, sourceOrdinaryMethodSymbol2.Locations[0]);
					}
					else
					{
						membersByName[memberName] = FixPartialMember(membersByName[memberName], sourceOrdinaryMethodSymbol, sourceOrdinaryMethodSymbol2);
					}
				}
				else
				{
					dictionary.Add(sourceMemberMethodSymbol, sourceMemberMethodSymbol);
				}
			}
			foreach (SourceOrdinaryMethodSymbol value2 in dictionary.Values)
			{
				if (value2.IsPartialImplementation && (object)value2.OtherPartOfPartial == null)
				{
					diagnostics.Add(ErrorCode.ERR_PartialMethodMustHaveLatent, value2.Locations[0], value2);
				}
				else if ((object)value2.OtherPartOfPartial != null && MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(value2, value2.OtherPartOfPartial))
				{
					diagnostics.Add(ErrorCode.ERR_PartialMethodInconsistentTupleNames, value2.Locations[0], value2, value2.OtherPartOfPartial);
				}
			}
		}
	}

	private static ImmutableArray<Symbol> FixPartialMember(ImmutableArray<Symbol> symbols, SourceOrdinaryMethodSymbol part1, SourceOrdinaryMethodSymbol part2)
	{
		SourceOrdinaryMethodSymbol definition;
		SourceOrdinaryMethodSymbol sourceOrdinaryMethodSymbol;
		if (part1.IsPartialDefinition)
		{
			definition = part1;
			sourceOrdinaryMethodSymbol = part2;
		}
		else
		{
			definition = part2;
			sourceOrdinaryMethodSymbol = part1;
		}
		SourceOrdinaryMethodSymbol.InitializePartialMethodParts(definition, sourceOrdinaryMethodSymbol);
		return Remove(symbols, sourceOrdinaryMethodSymbol);
	}

	private static ImmutableArray<Symbol> Remove(ImmutableArray<Symbol> symbols, Symbol symbol)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		foreach (Symbol item in symbols)
		{
			if ((object)item != symbol)
			{
				instance.Add(item);
			}
		}
		return instance.ToImmutableAndFree();
	}

	private void CheckForMemberConflictWithPropertyAccessor(PropertySymbol propertySymbol, bool getNotSet, DiagnosticBag diagnostics)
	{
		MethodSymbol methodSymbol = (getNotSet ? propertySymbol.GetMethod : propertySymbol.SetMethod);
		string text = (((object)methodSymbol == null) ? SourcePropertyAccessorSymbol.GetAccessorName(propertySymbol.IsIndexer ? propertySymbol.MetadataName : propertySymbol.Name, getNotSet, propertySymbol.IsCompilationOutputWinMdObj()) : methodSymbol.Name);
		foreach (Symbol member in GetMembers(text))
		{
			if (member.Kind != SymbolKind.Method)
			{
				if (Locations.Length == 1 || IsPartial)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, GetAccessorOrPropertyLocation(propertySymbol, getNotSet), this, text);
				}
				break;
			}
			MethodSymbol methodSymbol2 = (MethodSymbol)member;
			if (methodSymbol2.MethodKind == MethodKind.Ordinary && ParametersMatchPropertyAccessor(propertySymbol, getNotSet, methodSymbol2.Parameters))
			{
				diagnostics.Add(ErrorCode.ERR_MemberReserved, GetAccessorOrPropertyLocation(propertySymbol, getNotSet), text, this);
				break;
			}
		}
	}

	private void CheckForMemberConflictWithEventAccessor(EventSymbol eventSymbol, bool isAdder, DiagnosticBag diagnostics)
	{
		string accessorName = SourceEventSymbol.GetAccessorName(eventSymbol.Name, isAdder);
		foreach (Symbol member in GetMembers(accessorName))
		{
			if (member.Kind != SymbolKind.Method)
			{
				if (Locations.Length == 1 || IsPartial)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateNameInClass, GetAccessorOrEventLocation(eventSymbol, isAdder), this, accessorName);
				}
				break;
			}
			MethodSymbol methodSymbol = (MethodSymbol)member;
			if (methodSymbol.MethodKind == MethodKind.Ordinary && ParametersMatchEventAccessor(eventSymbol, methodSymbol.Parameters))
			{
				diagnostics.Add(ErrorCode.ERR_MemberReserved, GetAccessorOrEventLocation(eventSymbol, isAdder), accessorName, this);
				break;
			}
		}
	}

	private static Location GetAccessorOrPropertyLocation(PropertySymbol propertySymbol, bool getNotSet)
	{
		return ((Symbol)(((object)(getNotSet ? propertySymbol.GetMethod : propertySymbol.SetMethod)) ?? ((object)propertySymbol))).Locations[0];
	}

	private static Location GetAccessorOrEventLocation(EventSymbol propertySymbol, bool isAdder)
	{
		return ((Symbol)(((object)(isAdder ? propertySymbol.AddMethod : propertySymbol.RemoveMethod)) ?? ((object)propertySymbol))).Locations[0];
	}

	private static bool ParametersMatchPropertyAccessor(PropertySymbol propertySymbol, bool getNotSet, ImmutableArray<ParameterSymbol> methodParams)
	{
		ImmutableArray<ParameterSymbol> parameters = propertySymbol.Parameters;
		int num = parameters.Length + ((!getNotSet) ? 1 : 0);
		if (num != methodParams.Length)
		{
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			ParameterSymbol parameterSymbol = methodParams[i];
			if (parameterSymbol.RefKind != RefKind.None)
			{
				return false;
			}
			if (!((i == num - 1 && !getNotSet) ? propertySymbol.Type : parameters[i].Type).Equals(parameterSymbol.Type, TypeCompareKind.AllIgnoreOptions))
			{
				return false;
			}
		}
		return true;
	}

	private static bool ParametersMatchEventAccessor(EventSymbol eventSymbol, ImmutableArray<ParameterSymbol> methodParams)
	{
		if (methodParams.Length == 1 && methodParams[0].RefKind == RefKind.None)
		{
			return eventSymbol.Type.Equals(methodParams[0].Type, TypeCompareKind.AllIgnoreOptions);
		}
		return false;
	}

	private void AddEnumMembers(MembersAndInitializersBuilder result, EnumDeclarationSyntax syntax, DiagnosticBag diagnostics)
	{
		SourceEnumConstantSymbol sourceEnumConstantSymbol = null;
		int num = 0;
		foreach (EnumMemberDeclarationSyntax member in syntax.Members)
		{
			EqualsValueClauseSyntax equalsValue = member.EqualsValue;
			SourceEnumConstantSymbol sourceEnumConstantSymbol2 = ((equalsValue == null) ? SourceEnumConstantSymbol.CreateImplicitValuedConstant(this, member, sourceEnumConstantSymbol, num, diagnostics) : SourceEnumConstantSymbol.CreateExplicitValuedConstant(this, member, diagnostics));
			result.NonTypeNonIndexerMembers.Add(sourceEnumConstantSymbol2);
			if (equalsValue != null || (object)sourceEnumConstantSymbol == null)
			{
				sourceEnumConstantSymbol = sourceEnumConstantSymbol2;
				num = 1;
			}
			else
			{
				num++;
			}
		}
	}

	private static void AddInitializer(ref ArrayBuilder<FieldOrPropertyInitializer> initializers, ref int aggregateSyntaxLength, FieldSymbol fieldOpt, CSharpSyntaxNode node)
	{
		if (initializers == null)
		{
			initializers = new ArrayBuilder<FieldOrPropertyInitializer>();
		}
		int precedingInitializersLength = aggregateSyntaxLength;
		if (fieldOpt == null || !fieldOpt.IsMetadataConstant)
		{
			aggregateSyntaxLength += node.Span.Length;
		}
		initializers.Add(new FieldOrPropertyInitializer(fieldOpt, node, precedingInitializersLength));
	}

	private static void AddInitializers(ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>> allInitializers, ArrayBuilder<FieldOrPropertyInitializer> siblingsOpt)
	{
		if (siblingsOpt != null)
		{
			allInitializers.Add(siblingsOpt.ToImmutableAndFree());
		}
	}

	private static void CheckInterfaceMembers(ImmutableArray<Symbol> nonTypeMembers, DiagnosticBag diagnostics)
	{
		foreach (Symbol item in nonTypeMembers)
		{
			CheckInterfaceMember(item, diagnostics);
		}
	}

	private static void CheckInterfaceMember(Symbol member, DiagnosticBag diagnostics)
	{
		switch (member.Kind)
		{
		case SymbolKind.Field:
			if ((object)((FieldSymbol)member).AssociatedSymbol == null)
			{
				diagnostics.Add(ErrorCode.ERR_InterfacesCantContainFields, member.Locations[0]);
			}
			break;
		case SymbolKind.Method:
		{
			MethodSymbol methodSymbol = (MethodSymbol)member;
			switch (methodSymbol.MethodKind)
			{
			case MethodKind.Constructor:
			case MethodKind.StaticConstructor:
				diagnostics.Add(ErrorCode.ERR_InterfacesCantContainConstructors, member.Locations[0]);
				break;
			case MethodKind.Conversion:
			case MethodKind.UserDefinedOperator:
				diagnostics.Add(ErrorCode.ERR_InterfacesCantContainOperators, member.Locations[0]);
				break;
			case MethodKind.Destructor:
				diagnostics.Add(ErrorCode.ERR_OnlyClassesCanContainDestructors, member.Locations[0]);
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(methodSymbol.MethodKind);
			case MethodKind.EventAdd:
			case MethodKind.EventRemove:
			case MethodKind.ExplicitInterfaceImplementation:
			case MethodKind.Ordinary:
			case MethodKind.PropertyGet:
			case MethodKind.PropertySet:
			case MethodKind.LocalFunction:
				break;
			}
			break;
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(member.Kind);
		case SymbolKind.Event:
		case SymbolKind.Property:
			break;
		}
	}

	private static void CheckForStructDefaultConstructors(ArrayBuilder<Symbol> members, bool isEnum, DiagnosticBag diagnostics)
	{
		foreach (Symbol member in members)
		{
			if (member is MethodSymbol { MethodKind: MethodKind.Constructor, ParameterCount: 0 } methodSymbol)
			{
				if (isEnum)
				{
					diagnostics.Add(ErrorCode.ERR_EnumsCantContainDefaultConstructor, methodSymbol.Locations[0]);
				}
				else
				{
					diagnostics.Add(ErrorCode.ERR_StructsCantContainDefaultConstructor, methodSymbol.Locations[0]);
				}
			}
		}
	}

	private void CheckForStructBadInitializers(MembersAndInitializersBuilder builder, DiagnosticBag diagnostics)
	{
		foreach (ImmutableArray<FieldOrPropertyInitializer> instanceInitializer in builder.InstanceInitializers)
		{
			foreach (FieldOrPropertyInitializer item in instanceInitializer)
			{
				diagnostics.Add(ErrorCode.ERR_FieldInitializerInStruct, (item.FieldOpt.AssociatedSymbol ?? item.FieldOpt).Locations[0], this);
			}
		}
	}

	private void AddSynthesizedConstructorsIfNecessary(ArrayBuilder<Symbol> members, ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>> staticInitializers, DiagnosticBag diagnostics)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		foreach (Symbol member in members)
		{
			if (member.Kind == SymbolKind.Method)
			{
				MethodSymbol methodSymbol = (MethodSymbol)member;
				switch (methodSymbol.MethodKind)
				{
				case MethodKind.Constructor:
					flag = true;
					flag2 = flag2 || methodSymbol.ParameterCount == 0;
					break;
				case MethodKind.StaticConstructor:
					flag3 = true;
					break;
				}
			}
			if (flag & flag3)
			{
				break;
			}
		}
		if ((!flag2 && this.IsStructType()) || (!flag && !IsStatic))
		{
			members.Add((TypeKind == TypeKind.Submission) ? new SynthesizedSubmissionConstructor(this, diagnostics) : new SynthesizedInstanceConstructor(this));
		}
		if (!flag3 && HasNonConstantInitializer(staticInitializers))
		{
			members.Add(new SynthesizedStaticConstructor(this));
		}
		if (IsScriptClass)
		{
			SynthesizedInteractiveInitializerMethod synthesizedInteractiveInitializerMethod = new SynthesizedInteractiveInitializerMethod(this, diagnostics);
			members.Add(synthesizedInteractiveInitializerMethod);
			SynthesizedEntryPointSymbol item = SynthesizedEntryPointSymbol.Create(synthesizedInteractiveInitializerMethod, diagnostics);
			members.Add(item);
		}
	}

	private static bool HasNonConstantInitializer(ArrayBuilder<ImmutableArray<FieldOrPropertyInitializer>> initializers)
	{
		return initializers.Any((ImmutableArray<FieldOrPropertyInitializer> siblings) => siblings.Any((FieldOrPropertyInitializer initializer) => !initializer.FieldOpt.IsConst));
	}

	private void AddNonTypeMembers(MembersAndInitializersBuilder builder, SyntaxList<MemberDeclarationSyntax> members, DiagnosticBag diagnostics)
	{
		if (members.Count == 0)
		{
			return;
		}
		MemberDeclarationSyntax syntaxNode = members[0];
		Binder binder = GetBinder(syntaxNode);
		ArrayBuilder<FieldOrPropertyInitializer> initializers = null;
		ArrayBuilder<FieldOrPropertyInitializer> initializers2 = null;
		foreach (MemberDeclarationSyntax item6 in members)
		{
			if (_lazyMembersAndInitializers != null)
			{
				return;
			}
			bool flag = !item6.HasErrors;
			switch (item6.Kind())
			{
			case SyntaxKind.FieldDeclaration:
			{
				FieldDeclarationSyntax fieldDeclarationSyntax = (FieldDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(fieldDeclarationSyntax.Declaration.Variables.First().Identifier));
				}
				DeclarationModifiers declarationModifiers = SourceMemberFieldSymbol.MakeModifiers(this, fieldDeclarationSyntax.Declaration.Variables[0].Identifier, fieldDeclarationSyntax.Modifiers, diagnostics, out var modifierErrors);
				foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
				{
					SourceMemberFieldSymbolFromDeclarator sourceMemberFieldSymbolFromDeclarator = (((declarationModifiers & DeclarationModifiers.Fixed) == 0) ? new SourceMemberFieldSymbolFromDeclarator(this, variable, declarationModifiers, modifierErrors, diagnostics) : new SourceFixedFieldSymbol(this, variable, declarationModifiers, modifierErrors, diagnostics));
					builder.NonTypeNonIndexerMembers.Add(sourceMemberFieldSymbolFromDeclarator);
					if (IsScriptClass)
					{
						ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeNonIndexerMembers, variable, this, DeclarationModifiers.Private | (declarationModifiers & DeclarationModifiers.Static), sourceMemberFieldSymbolFromDeclarator);
					}
					if (variable.Initializer != null)
					{
						if (sourceMemberFieldSymbolFromDeclarator.IsStatic)
						{
							AddInitializer(ref initializers, ref builder.StaticSyntaxLength, sourceMemberFieldSymbolFromDeclarator, variable.Initializer);
						}
						else
						{
							AddInitializer(ref initializers2, ref builder.InstanceSyntaxLength, sourceMemberFieldSymbolFromDeclarator, variable.Initializer);
						}
					}
				}
				break;
			}
			case SyntaxKind.MethodDeclaration:
			{
				MethodDeclarationSyntax methodDeclarationSyntax = (MethodDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(methodDeclarationSyntax.Identifier));
				}
				SourceOrdinaryMethodSymbol item = SourceOrdinaryMethodSymbol.CreateMethodSymbol(this, binder, methodDeclarationSyntax, diagnostics);
				builder.NonTypeNonIndexerMembers.Add(item);
				break;
			}
			case SyntaxKind.ConstructorDeclaration:
			{
				ConstructorDeclarationSyntax constructorDeclarationSyntax = (ConstructorDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(constructorDeclarationSyntax.Identifier));
				}
				SourceConstructorSymbol item2 = SourceConstructorSymbol.CreateConstructorSymbol(this, constructorDeclarationSyntax, diagnostics);
				builder.NonTypeNonIndexerMembers.Add(item2);
				break;
			}
			case SyntaxKind.DestructorDeclaration:
			{
				DestructorDeclarationSyntax destructorDeclarationSyntax = (DestructorDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(destructorDeclarationSyntax.Identifier));
				}
				SourceDestructorSymbol item4 = new SourceDestructorSymbol(this, destructorDeclarationSyntax, diagnostics);
				builder.NonTypeNonIndexerMembers.Add(item4);
				break;
			}
			case SyntaxKind.PropertyDeclaration:
			{
				PropertyDeclarationSyntax propertyDeclarationSyntax = (PropertyDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(propertyDeclarationSyntax.Identifier));
				}
				SourcePropertySymbol sourcePropertySymbol = SourcePropertySymbol.Create(this, binder, propertyDeclarationSyntax, diagnostics);
				builder.NonTypeNonIndexerMembers.Add(sourcePropertySymbol);
				AddAccessorIfAvailable(builder.NonTypeNonIndexerMembers, sourcePropertySymbol.GetMethod, diagnostics);
				AddAccessorIfAvailable(builder.NonTypeNonIndexerMembers, sourcePropertySymbol.SetMethod, diagnostics);
				FieldSymbol backingField = sourcePropertySymbol.BackingField;
				if ((object)backingField == null)
				{
					break;
				}
				builder.NonTypeNonIndexerMembers.Add(backingField);
				EqualsValueClauseSyntax initializer = propertyDeclarationSyntax.Initializer;
				if (initializer != null)
				{
					if (IsScriptClass)
					{
						ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeNonIndexerMembers, initializer, this, (DeclarationModifiers)(0x100 | (sourcePropertySymbol.IsStatic ? 4 : 0)), backingField);
					}
					if (sourcePropertySymbol.IsStatic)
					{
						AddInitializer(ref initializers, ref builder.StaticSyntaxLength, backingField, initializer);
					}
					else
					{
						AddInitializer(ref initializers2, ref builder.InstanceSyntaxLength, backingField, initializer);
					}
				}
				break;
			}
			case SyntaxKind.EventFieldDeclaration:
			{
				EventFieldDeclarationSyntax eventFieldDeclarationSyntax = (EventFieldDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(eventFieldDeclarationSyntax.Declaration.Variables.First().Identifier));
				}
				foreach (VariableDeclaratorSyntax variable2 in eventFieldDeclarationSyntax.Declaration.Variables)
				{
					SourceFieldLikeEventSymbol sourceFieldLikeEventSymbol = new SourceFieldLikeEventSymbol(this, binder, eventFieldDeclarationSyntax.Modifiers, variable2, diagnostics);
					builder.NonTypeNonIndexerMembers.Add(sourceFieldLikeEventSymbol);
					FieldSymbol associatedField = sourceFieldLikeEventSymbol.AssociatedField;
					if (IsScriptClass)
					{
						ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeNonIndexerMembers, variable2, this, (DeclarationModifiers)(0x100 | (sourceFieldLikeEventSymbol.IsStatic ? 4 : 0)), associatedField);
					}
					if ((object)associatedField != null && variable2.Initializer != null)
					{
						if (associatedField.IsStatic)
						{
							AddInitializer(ref initializers, ref builder.StaticSyntaxLength, associatedField, variable2.Initializer);
						}
						else
						{
							AddInitializer(ref initializers2, ref builder.InstanceSyntaxLength, associatedField, variable2.Initializer);
						}
					}
					AddAccessorIfAvailable(builder.NonTypeNonIndexerMembers, sourceFieldLikeEventSymbol.AddMethod, diagnostics);
					AddAccessorIfAvailable(builder.NonTypeNonIndexerMembers, sourceFieldLikeEventSymbol.RemoveMethod, diagnostics);
				}
				break;
			}
			case SyntaxKind.EventDeclaration:
			{
				EventDeclarationSyntax eventDeclarationSyntax = (EventDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(eventDeclarationSyntax.Identifier));
				}
				SourceCustomEventSymbol sourceCustomEventSymbol = new SourceCustomEventSymbol(this, binder, eventDeclarationSyntax, diagnostics);
				builder.NonTypeNonIndexerMembers.Add(sourceCustomEventSymbol);
				AddAccessorIfAvailable(builder.NonTypeNonIndexerMembers, sourceCustomEventSymbol.AddMethod, diagnostics);
				AddAccessorIfAvailable(builder.NonTypeNonIndexerMembers, sourceCustomEventSymbol.RemoveMethod, diagnostics);
				break;
			}
			case SyntaxKind.IndexerDeclaration:
			{
				IndexerDeclarationSyntax indexerDeclarationSyntax = (IndexerDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(indexerDeclarationSyntax.ThisKeyword));
				}
				builder.IndexerDeclarations.Add(indexerDeclarationSyntax.GetReference());
				break;
			}
			case SyntaxKind.ConversionOperatorDeclaration:
			{
				ConversionOperatorDeclarationSyntax conversionOperatorDeclarationSyntax = (ConversionOperatorDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(conversionOperatorDeclarationSyntax.OperatorKeyword));
				}
				SourceUserDefinedConversionSymbol item5 = SourceUserDefinedConversionSymbol.CreateUserDefinedConversionSymbol(this, conversionOperatorDeclarationSyntax, diagnostics);
				builder.NonTypeNonIndexerMembers.Add(item5);
				break;
			}
			case SyntaxKind.OperatorDeclaration:
			{
				OperatorDeclarationSyntax operatorDeclarationSyntax = (OperatorDeclarationSyntax)item6;
				if (IsImplicitClass & flag)
				{
					diagnostics.Add(ErrorCode.ERR_NamespaceUnexpected, new SourceLocation(operatorDeclarationSyntax.OperatorKeyword));
				}
				SourceUserDefinedOperatorSymbol item3 = SourceUserDefinedOperatorSymbol.CreateUserDefinedOperatorSymbol(this, operatorDeclarationSyntax, diagnostics);
				builder.NonTypeNonIndexerMembers.Add(item3);
				break;
			}
			case SyntaxKind.GlobalStatement:
			{
				StatementSyntax statement = ((GlobalStatementSyntax)item6).Statement;
				if (IsScriptClass)
				{
					StatementSyntax statementSyntax = statement;
					while (statementSyntax.Kind() == SyntaxKind.LabeledStatement)
					{
						statementSyntax = ((LabeledStatementSyntax)statementSyntax).Statement;
					}
					switch (statementSyntax.Kind())
					{
					case SyntaxKind.LocalDeclarationStatement:
						foreach (VariableDeclaratorSyntax variable3 in ((LocalDeclarationStatementSyntax)statementSyntax).Declaration.Variables)
						{
							ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeNonIndexerMembers, variable3, this, DeclarationModifiers.Private, null);
						}
						break;
					case SyntaxKind.ExpressionStatement:
					case SyntaxKind.ReturnStatement:
					case SyntaxKind.YieldReturnStatement:
					case SyntaxKind.ThrowStatement:
					case SyntaxKind.LockStatement:
					case SyntaxKind.IfStatement:
					case SyntaxKind.SwitchStatement:
						ExpressionFieldFinder.FindExpressionVariables(builder.NonTypeNonIndexerMembers, statementSyntax, this, DeclarationModifiers.Private, null);
						break;
					}
					AddInitializer(ref initializers2, ref builder.InstanceSyntaxLength, null, statement);
				}
				else if (flag)
				{
					diagnostics.Add(ErrorCode.ERR_GlobalStatement, new SourceLocation(statement));
				}
				break;
			}
			}
		}
		AddInitializers(builder.InstanceInitializers, initializers2);
		AddInitializers(builder.StaticInitializers, initializers);
	}

	private void AddAccessorIfAvailable(ArrayBuilder<Symbol> symbols, MethodSymbol accessorOpt, DiagnosticBag diagnostics, bool checkName = false)
	{
		if ((object)accessorOpt != null)
		{
			symbols.Add(accessorOpt);
			if (checkName)
			{
				CheckMemberNameDistinctFromType(accessorOpt, diagnostics);
			}
		}
	}

	internal ImmutableArray<SynthesizedExplicitImplementationForwardingMethod> GetSynthesizedExplicitImplementations(CancellationToken cancellationToken)
	{
		if (_lazySynthesizedExplicitImplementations.IsDefault)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				CheckMembersAgainstBaseType(instance, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				CheckAbstractClassImplementations(instance);
				cancellationToken.ThrowIfCancellationRequested();
				CheckInterfaceUnification(instance);
				if (IsInterface)
				{
					cancellationToken.ThrowIfCancellationRequested();
					this.CheckInterfaceVarianceSafety(instance);
				}
				if (ImmutableInterlocked.InterlockedCompareExchange(ref _lazySynthesizedExplicitImplementations, ComputeInterfaceImplementations(instance, cancellationToken), default).IsDefault)
				{
					AddDeclarationDiagnostics(instance);
					state.NotePartComplete(CompletionPart.SynthesizedExplicitImplementations);
				}
			}
			finally
			{
				instance.Free();
			}
		}
		return _lazySynthesizedExplicitImplementations;
	}

	private void CheckAbstractClassImplementations(DiagnosticBag diagnostics)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		if (IsAbstract || (object)baseTypeNoUseSiteDiagnostics == null || !baseTypeNoUseSiteDiagnostics.IsAbstract)
		{
			return;
		}
		foreach (Symbol abstractMember in AbstractMembers)
		{
			if (abstractMember.Kind == SymbolKind.Method)
			{
				diagnostics.Add(ErrorCode.ERR_UnimplementedAbstractMethod, Locations[0], this, abstractMember);
			}
		}
	}

	private ImmutableArray<SynthesizedExplicitImplementationForwardingMethod> ComputeInterfaceImplementations(DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		if (IsInterface)
		{
			return ImmutableArray<SynthesizedExplicitImplementationForwardingMethod>.Empty;
		}
		ArrayBuilder<SynthesizedExplicitImplementationForwardingMethod> instance = ArrayBuilder<SynthesizedExplicitImplementationForwardingMethod>.GetInstance();
		ImmutableHashSet<NamedTypeSymbol> interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics = InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics;
		foreach (NamedTypeSymbol allInterfacesNoUseSiteDiagnostic in AllInterfacesNoUseSiteDiagnostics)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!interfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Contains(allInterfacesNoUseSiteDiagnostic))
			{
				continue;
			}
			bool? flag = null;
			foreach (Symbol item in allInterfacesNoUseSiteDiagnostic.GetMembersUnordered())
			{
				cancellationToken.ThrowIfCancellationRequested();
				SymbolKind kind = item.Kind;
				if ((kind != SymbolKind.Event && kind != SymbolKind.Method && kind != SymbolKind.Property) || item.IsStatic)
				{
					continue;
				}
				SymbolAndDiagnostics symbolAndDiagnostics = FindImplementationForInterfaceMemberWithDiagnostics(item);
				Symbol symbol = symbolAndDiagnostics.Symbol;
				SynthesizedExplicitImplementationForwardingMethod synthesizedExplicitImplementationForwardingMethod = SynthesizeInterfaceMemberImplementation(symbolAndDiagnostics, item);
				bool flag2 = (object)symbol != null;
				if ((object)synthesizedExplicitImplementationForwardingMethod != null)
				{
					if (synthesizedExplicitImplementationForwardingMethod.IsVararg)
					{
						diagnostics.Add(ErrorCode.ERR_InterfaceImplementedImplicitlyByVariadic, TypeSymbol.GetImplicitImplementationDiagnosticLocation(item, this, symbol), symbol, item, this);
					}
					else
					{
						instance.Add(synthesizedExplicitImplementationForwardingMethod);
					}
				}
				if (flag2 && kind == SymbolKind.Event)
				{
					EventSymbol eventSymbol = (EventSymbol)item;
					EventSymbol eventSymbol2 = (EventSymbol)symbol;
					EventSymbol eventSymbol3;
					EventSymbol eventSymbol4;
					if (eventSymbol.IsWindowsRuntimeEvent)
					{
						eventSymbol3 = eventSymbol;
						eventSymbol4 = eventSymbol2;
					}
					else
					{
						eventSymbol3 = eventSymbol2;
						eventSymbol4 = eventSymbol;
					}
					if (eventSymbol.IsWindowsRuntimeEvent != eventSymbol2.IsWindowsRuntimeEvent)
					{
						object[] args = new object[4] { eventSymbol2, eventSymbol, eventSymbol3, eventSymbol4 };
						CSDiagnosticInfo info = new CSDiagnosticInfo(ErrorCode.ERR_MixingWinRTEventWithRegular, args, ImmutableArray<Symbol>.Empty, ImmutableArray.Create(Locations[0]));
						diagnostics.Add(info, eventSymbol2.Locations[0]);
					}
				}
				Symbol symbol2 = ((kind == SymbolKind.Method) ? ((MethodSymbol)item).AssociatedSymbol : null);
				if ((object)symbol2 != null && !ReportAccessorOfInterfacePropertyOrEvent(symbol2) && (!flag2 || symbol.IsAccessor()))
				{
					continue;
				}
				if (symbolAndDiagnostics.Diagnostics.Any())
				{
					diagnostics.AddRange(symbolAndDiagnostics.Diagnostics);
				}
				else if (!flag2)
				{
					flag = flag ?? HasImportedBaseTypeDeclaringInterface(allInterfacesNoUseSiteDiagnostic);
					if (flag != true && !item.MustCallMethodsDirectly() && !item.IsIndexedProperty())
					{
						DiagnosticInfo useSiteDiagnostic = item.GetUseSiteDiagnostic();
						if (useSiteDiagnostic != null && useSiteDiagnostic.DefaultSeverity == DiagnosticSeverity.Error)
						{
							diagnostics.Add(useSiteDiagnostic, GetImplementsLocation(allInterfacesNoUseSiteDiagnostic));
							continue;
						}
						diagnostics.Add(ErrorCode.ERR_UnimplementedInterfaceMember, GetImplementsLocation(allInterfacesNoUseSiteDiagnostic) ?? Locations[0], this, item);
					}
				}
				else if (kind == SymbolKind.Method && ((object)synthesizedExplicitImplementationForwardingMethod != null || symbol.ContainingType == this))
				{
					DiagnosticInfo useSiteDiagnostic2 = item.GetUseSiteDiagnostic();
					if (useSiteDiagnostic2 != null && useSiteDiagnostic2.Code != 7085)
					{
						Location location = (symbol.IsFromCompilation(DeclaringCompilation) ? symbol.Locations[0] : Locations[0]);
						Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic2, diagnostics, location);
					}
				}
			}
		}
		return instance.ToImmutableAndFree();
	}

	protected abstract Location GetCorrespondingBaseListLocation(NamedTypeSymbol @base);

	internal Location GetImplementsLocation(NamedTypeSymbol implementedInterface)
	{
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		NamedTypeSymbol namedTypeSymbol = null;
		foreach (NamedTypeSymbol item in InterfacesNoUseSiteDiagnostics())
		{
			if (item == implementedInterface)
			{
				namedTypeSymbol = item;
				break;
			}
			if ((object)namedTypeSymbol == null && item.ImplementsInterface(implementedInterface, ref useSiteDiagnostics))
			{
				namedTypeSymbol = item;
			}
		}
		return GetCorrespondingBaseListLocation(namedTypeSymbol);
	}

	private bool ReportAccessorOfInterfacePropertyOrEvent(Symbol interfacePropertyOrEvent)
	{
		if (interfacePropertyOrEvent.IsIndexedProperty())
		{
			return true;
		}
		Symbol symbol = FindImplementationForInterfaceMemberWithDiagnostics(interfacePropertyOrEvent).Symbol;
		if ((object)symbol == null)
		{
			return false;
		}
		if (interfacePropertyOrEvent.Kind == SymbolKind.Event && symbol.Kind == SymbolKind.Event && ((EventSymbol)interfacePropertyOrEvent).IsWindowsRuntimeEvent != ((EventSymbol)symbol).IsWindowsRuntimeEvent)
		{
			return false;
		}
		return true;
	}

	private bool HasImportedBaseTypeDeclaringInterface(NamedTypeSymbol @interface)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		NamedTypeSymbol namedTypeSymbol = this;
		while ((object)namedTypeSymbol != null)
		{
			if (!namedTypeSymbol.OriginalDefinition.IsFromCompilation(declaringCompilation) && namedTypeSymbol.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Contains(@interface))
			{
				return true;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeNoUseSiteDiagnostics;
		}
		return false;
	}

	private void CheckMembersAgainstBaseType(DiagnosticBag diagnostics, CancellationToken cancellationToken)
	{
		switch (TypeKind)
		{
		case TypeKind.Delegate:
		case TypeKind.Enum:
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(TypeKind);
		case TypeKind.Class:
		case TypeKind.Interface:
		case TypeKind.Struct:
		case TypeKind.Submission:
			foreach (Symbol item in GetMembersUnordered())
			{
				cancellationToken.ThrowIfCancellationRequested();
				bool suppressAccessors;
				switch (item.Kind)
				{
				case SymbolKind.Method:
				{
					MethodSymbol methodSymbol = (MethodSymbol)item;
					if (MethodSymbol.CanOverrideOrHide(methodSymbol.MethodKind) && !methodSymbol.IsAccessor())
					{
						if (item.IsOverride)
						{
							CheckOverrideMember(methodSymbol, methodSymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
						else if (methodSymbol is SourceMemberMethodSymbol { IsNew: var isNew3 })
						{
							CheckNonOverrideMember(methodSymbol, isNew3, methodSymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
					}
					else if (methodSymbol.MethodKind == MethodKind.Destructor)
					{
						MethodSymbol firstRuntimeOverriddenMethodIgnoringNewSlot = methodSymbol.GetFirstRuntimeOverriddenMethodIgnoringNewSlot(ignoreInterfaceImplementationChanges: true);
						if ((object)firstRuntimeOverriddenMethodIgnoringNewSlot != null && firstRuntimeOverriddenMethodIgnoringNewSlot.IsMetadataFinal)
						{
							diagnostics.Add(ErrorCode.ERR_CantOverrideSealed, methodSymbol.Locations[0], methodSymbol, firstRuntimeOverriddenMethodIgnoringNewSlot);
						}
					}
					break;
				}
				case SymbolKind.Property:
				{
					PropertySymbol propertySymbol = (PropertySymbol)item;
					MethodSymbol getMethod = propertySymbol.GetMethod;
					MethodSymbol setMethod = propertySymbol.SetMethod;
					if (item.IsOverride)
					{
						CheckOverrideMember(propertySymbol, propertySymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						if (!suppressAccessors)
						{
							if ((object)getMethod != null)
							{
								CheckOverrideMember(getMethod, getMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
							if ((object)setMethod != null)
							{
								CheckOverrideMember(setMethod, setMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
						}
						break;
					}
					bool isNew2 = ((SourcePropertySymbol)propertySymbol).IsNew;
					CheckNonOverrideMember(propertySymbol, isNew2, propertySymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
					if (!suppressAccessors)
					{
						if ((object)getMethod != null)
						{
							CheckNonOverrideMember(getMethod, isNew2, getMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
						if ((object)setMethod != null)
						{
							CheckNonOverrideMember(setMethod, isNew2, setMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
					}
					break;
				}
				case SymbolKind.Event:
				{
					EventSymbol eventSymbol = (EventSymbol)item;
					MethodSymbol addMethod = eventSymbol.AddMethod;
					MethodSymbol removeMethod = eventSymbol.RemoveMethod;
					if (item.IsOverride)
					{
						CheckOverrideMember(eventSymbol, eventSymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						if (!suppressAccessors)
						{
							if ((object)addMethod != null)
							{
								CheckOverrideMember(addMethod, addMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
							if ((object)removeMethod != null)
							{
								CheckOverrideMember(removeMethod, removeMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
							}
						}
						break;
					}
					bool isNew4 = ((SourceEventSymbol)eventSymbol).IsNew;
					CheckNonOverrideMember(eventSymbol, isNew4, eventSymbol.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
					if (!suppressAccessors)
					{
						if ((object)addMethod != null)
						{
							CheckNonOverrideMember(addMethod, isNew4, addMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
						if ((object)removeMethod != null)
						{
							CheckNonOverrideMember(removeMethod, isNew4, removeMethod.OverriddenOrHiddenMembers, diagnostics, out suppressAccessors);
						}
					}
					break;
				}
				case SymbolKind.Field:
				{
					bool isNew = item is SourceFieldSymbol sourceFieldSymbol && sourceFieldSymbol.IsNew;
					CheckNewModifier(item, isNew, diagnostics);
					break;
				}
				case SymbolKind.NamedType:
					CheckNewModifier(item, ((SourceMemberContainerTypeSymbol)item).IsNew, diagnostics);
					break;
				}
			}
			break;
		}
	}

	private void CheckNewModifier(Symbol symbol, bool isNew, DiagnosticBag diagnostics)
	{
		if ((object)BaseTypeNoUseSiteDiagnostics == null || symbol.IsImplicitlyDeclared)
		{
			return;
		}
		int memberArity = symbol.GetMemberArity();
		Location location = symbol.Locations.FirstOrDefault();
		bool suppressAccessors = false;
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = BaseTypeNoUseSiteDiagnostics;
		while ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			foreach (Symbol member in baseTypeNoUseSiteDiagnostics.GetMembers(symbol.Name))
			{
				if (member.Kind == SymbolKind.Method && !((MethodSymbol)member).CanBeHiddenByMemberKind(symbol.Kind))
				{
					continue;
				}
				HashSet<DiagnosticInfo> useSiteDiagnostics = null;
				bool flag = AccessCheck.IsSymbolAccessible(member, this, ref useSiteDiagnostics);
				diagnostics.Add(location, useSiteDiagnostics);
				if (flag && member.GetMemberArity() == memberArity)
				{
					if (!isNew)
					{
						diagnostics.Add(ErrorCode.WRN_NewRequired, location, symbol, member);
					}
					AddHidingAbstractDiagnostic(symbol, location, member, diagnostics, ref suppressAccessors);
					return;
				}
			}
			baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
		}
		if (isNew)
		{
			diagnostics.Add(ErrorCode.WRN_NewNotRequired, location, symbol);
		}
	}

	private static void CheckOverrideMember(Symbol overridingMember, OverriddenOrHiddenMembersResult overriddenOrHiddenMembers, DiagnosticBag diagnostics, out bool suppressAccessors)
	{
		suppressAccessors = false;
		bool flag = overridingMember.Kind == SymbolKind.Method;
		bool flag2 = overridingMember.Kind == SymbolKind.Property;
		bool flag3 = overridingMember.Kind == SymbolKind.Event;
		Location location = overridingMember.Locations[0];
		ImmutableArray<Symbol> overriddenMembers = overriddenOrHiddenMembers.OverriddenMembers;
		if (overriddenMembers.Length == 0)
		{
			ImmutableArray<Symbol> hiddenMembers = overriddenOrHiddenMembers.HiddenMembers;
			if (hiddenMembers.Any())
			{
				ErrorCode code;
				if (flag)
				{
					code = ErrorCode.ERR_CantOverrideNonFunction;
				}
				else
				{
					code = (flag2 ? ErrorCode.ERR_CantOverrideNonProperty : ErrorCode.ERR_CantOverrideNonEvent);
				}
				diagnostics.Add(code, location, overridingMember, hiddenMembers[0]);
			}
			else
			{
				Symbol symbol = null;
				if (flag)
				{
					symbol = ((MethodSymbol)overridingMember).AssociatedSymbol;
				}
				if ((object)symbol == null)
				{
					diagnostics.Add(ErrorCode.ERR_OverrideNotExpected, location, overridingMember);
				}
				else if (symbol.Kind == SymbolKind.Property)
				{
					PropertySymbol propertySymbol = (PropertySymbol)symbol;
					PropertySymbol overriddenProperty = propertySymbol.OverriddenProperty;
					if ((object)overriddenProperty != null)
					{
						if (propertySymbol.GetMethod == overridingMember && (object)overriddenProperty.GetMethod == null)
						{
							diagnostics.Add(ErrorCode.ERR_NoGetToOverride, location, overridingMember, overriddenProperty);
						}
						else if (propertySymbol.SetMethod == overridingMember && (object)overriddenProperty.SetMethod == null)
						{
							diagnostics.Add(ErrorCode.ERR_NoSetToOverride, location, overridingMember, overriddenProperty);
						}
						else
						{
							diagnostics.Add(ErrorCode.ERR_OverrideNotExpected, location, overridingMember);
						}
					}
				}
			}
		}
		else
		{
			NamedTypeSymbol containingType = overridingMember.ContainingType;
			if (overriddenMembers.Length > 1)
			{
				diagnostics.Add(ErrorCode.ERR_AmbigOverride, location, overriddenMembers[0].OriginalDefinition, overriddenMembers[1].OriginalDefinition, containingType);
				suppressAccessors = true;
			}
			else
			{
				Symbol symbol2 = overriddenMembers[0];
				if (symbol2.MustCallMethodsDirectly())
				{
					diagnostics.Add(ErrorCode.ERR_CantOverrideBogusMethod, location, overridingMember, symbol2);
					suppressAccessors = true;
				}
				else if (!symbol2.IsVirtual && !symbol2.IsAbstract && !symbol2.IsOverride && (!flag || ((MethodSymbol)symbol2).MethodKind != MethodKind.Destructor))
				{
					diagnostics.Add(ErrorCode.ERR_CantOverrideNonVirtual, location, overridingMember, symbol2);
					suppressAccessors = true;
				}
				else if (symbol2.IsSealed)
				{
					diagnostics.Add(ErrorCode.ERR_CantOverrideSealed, location, overridingMember, symbol2);
					suppressAccessors = true;
				}
				else if (!overridingMember.IsPartialMethod() && !OverrideHasCorrectAccessibility(symbol2, overridingMember))
				{
					string text = SyntaxFacts.GetText(symbol2.DeclaredAccessibility);
					diagnostics.Add(ErrorCode.ERR_CantChangeAccessOnOverride, location, overridingMember, text, symbol2);
					suppressAccessors = true;
				}
				else if (overridingMember.ContainsTupleNames() && MemberSignatureComparer.ConsideringTupleNamesCreatesDifference(overridingMember, symbol2))
				{
					diagnostics.Add(ErrorCode.ERR_CantChangeTupleNamesOnOverride, location, overridingMember, symbol2);
				}
				else
				{
					Symbol leastOverriddenMember = symbol2.GetLeastOverriddenMember(symbol2.ContainingType);
					overridingMember.ForceCompleteObsoleteAttribute();
					leastOverriddenMember.ForceCompleteObsoleteAttribute();
					bool flag4 = overridingMember.ObsoleteState == ThreeState.True;
					bool flag5 = leastOverriddenMember.ObsoleteState == ThreeState.True;
					if (flag4 != flag5)
					{
						ErrorCode code2 = (flag4 ? ErrorCode.WRN_ObsoleteOverridingNonObsolete : ErrorCode.WRN_NonObsoleteOverridingObsolete);
						diagnostics.Add(code2, location, overridingMember, leastOverriddenMember);
					}
					if (flag2)
					{
						PropertySymbol propertySymbol2 = (PropertySymbol)overridingMember;
						PropertySymbol propertySymbol3 = (PropertySymbol)symbol2;
						TypeSymbol type = propertySymbol2.Type;
						TypeSymbol type2 = propertySymbol3.Type;
						if (propertySymbol2.RefKind != propertySymbol3.RefKind)
						{
							diagnostics.Add(ErrorCode.ERR_CantChangeRefReturnOnOverride, location, overridingMember, symbol2);
							suppressAccessors = true;
						}
						else if (!type.Equals(type2, TypeCompareKind.AllIgnoreOptions))
						{
							diagnostics.Add(ErrorCode.ERR_CantChangeTypeOnOverride, location, overridingMember, symbol2, type2);
							suppressAccessors = true;
						}
						if (propertySymbol2.IsSealed)
						{
							MethodSymbol ownOrInheritedGetMethod = propertySymbol2.GetOwnOrInheritedGetMethod();
							HashSet<DiagnosticInfo> useSiteDiagnostics = null;
							if (propertySymbol2.GetMethod != ownOrInheritedGetMethod && !AccessCheck.IsSymbolAccessible(ownOrInheritedGetMethod, containingType, ref useSiteDiagnostics))
							{
								diagnostics.Add(ErrorCode.ERR_NoGetToOverride, location, propertySymbol2, propertySymbol3);
							}
							MethodSymbol ownOrInheritedSetMethod = propertySymbol2.GetOwnOrInheritedSetMethod();
							if (propertySymbol2.SetMethod != ownOrInheritedSetMethod && !AccessCheck.IsSymbolAccessible(ownOrInheritedSetMethod, containingType, ref useSiteDiagnostics))
							{
								diagnostics.Add(ErrorCode.ERR_NoSetToOverride, location, propertySymbol2, propertySymbol3);
							}
							diagnostics.Add(location, useSiteDiagnostics);
						}
					}
					else if (flag3)
					{
						EventSymbol eventSymbol = (EventSymbol)overridingMember;
						EventSymbol eventSymbol2 = (EventSymbol)symbol2;
						TypeSymbol type3 = eventSymbol.Type;
						TypeSymbol type4 = eventSymbol2.Type;
						if (!type3.Equals(type4, TypeCompareKind.AllIgnoreOptions))
						{
							diagnostics.Add(ErrorCode.ERR_CantChangeTypeOnOverride, location, overridingMember, symbol2, type4);
							suppressAccessors = true;
						}
					}
					else
					{
						MethodSymbol methodSymbol = (MethodSymbol)overridingMember;
						MethodSymbol methodSymbol2 = (MethodSymbol)symbol2;
						if (methodSymbol.RefKind != methodSymbol2.RefKind)
						{
							diagnostics.Add(ErrorCode.ERR_CantChangeRefReturnOnOverride, location, overridingMember, symbol2);
						}
						else if (!MemberSignatureComparer.HaveSameReturnTypes(methodSymbol, methodSymbol2, considerCustomModifiers: false))
						{
							TypeSymbol typeSymbol = (methodSymbol2.IsGenericMethod ? methodSymbol2.Construct(methodSymbol.TypeParameters.Cast<TypeParameterSymbol, TypeSymbol>()).ReturnType : methodSymbol2.ReturnType);
							diagnostics.Add(ErrorCode.ERR_CantChangeReturnTypeOnOverride, location, overridingMember, symbol2, typeSymbol);
						}
						else if (methodSymbol2.IsRuntimeFinalizer())
						{
							diagnostics.Add(ErrorCode.ERR_OverrideFinalizeDeprecated, location);
						}
					}
					DiagnosticInfo useSiteDiagnostic = symbol2.GetUseSiteDiagnostic();
					if (useSiteDiagnostic != null)
					{
						suppressAccessors = Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, overridingMember.Locations[0]);
					}
				}
			}
		}
		ImmutableArray<Symbol> runtimeOverriddenMembers = overriddenOrHiddenMembers.RuntimeOverriddenMembers;
		if (runtimeOverriddenMembers.Length > 1 && overridingMember.Kind == SymbolKind.Method)
		{
			Symbol symbol3 = runtimeOverriddenMembers[0];
			diagnostics.Add(ErrorCode.WRN_MultipleRuntimeOverrideMatches, symbol3.Locations[0], symbol3, overridingMember);
			suppressAccessors = true;
		}
	}

	private static void CheckNonOverrideMember(Symbol hidingMember, bool hidingMemberIsNew, OverriddenOrHiddenMembersResult overriddenOrHiddenMembers, DiagnosticBag diagnostics, out bool suppressAccessors)
	{
		suppressAccessors = false;
		Location location = hidingMember.Locations[0];
		ImmutableArray<Symbol> hiddenMembers = overriddenOrHiddenMembers.HiddenMembers;
		if (hiddenMembers.Length == 0)
		{
			if (hidingMemberIsNew && !hidingMember.IsAccessor())
			{
				diagnostics.Add(ErrorCode.WRN_NewNotRequired, location, hidingMember);
			}
			return;
		}
		bool flag = false;
		if (!hidingMember.ContainingType.IsInterface)
		{
			foreach (Symbol item in hiddenMembers)
			{
				flag |= AddHidingAbstractDiagnostic(hidingMember, location, item, diagnostics, ref suppressAccessors);
				if (!hidingMemberIsNew && item.Kind == hidingMember.Kind && !hidingMember.IsAccessor() && (item.IsAbstract || item.IsVirtual || item.IsOverride))
				{
					diagnostics.Add(ErrorCode.WRN_NewOrOverrideExpected, location, hidingMember, item);
					flag = true;
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (!hidingMemberIsNew && !flag && !hidingMember.IsAccessor() && !hidingMember.IsOperator())
		{
			diagnostics.Add(ErrorCode.WRN_NewRequired, location, hidingMember, hiddenMembers[0]);
		}
	}

	private static bool AddHidingAbstractDiagnostic(Symbol hidingMember, Location hidingMemberLocation, Symbol hiddenMember, DiagnosticBag diagnostics, ref bool suppressAccessors)
	{
		SymbolKind kind = hiddenMember.Kind;
		if (kind != SymbolKind.Event && kind != SymbolKind.Method && kind != SymbolKind.Property)
		{
			return false;
		}
		if (!hiddenMember.IsAbstract || !hidingMember.ContainingType.IsAbstract)
		{
			return false;
		}
		switch (hidingMember.DeclaredAccessibility)
		{
		case Accessibility.Protected:
		case Accessibility.ProtectedOrInternal:
		case Accessibility.Public:
			kind = hidingMember.Kind;
			if (kind == SymbolKind.Event)
			{
				goto IL_00a8;
			}
			if (kind != SymbolKind.Method)
			{
				if (kind == SymbolKind.Property)
				{
					goto IL_00a8;
				}
			}
			else
			{
				Symbol associatedSymbol = ((MethodSymbol)hidingMember).AssociatedSymbol;
				if ((object)associatedSymbol != null)
				{
					diagnostics.Add(ErrorCode.ERR_HidingAbstractMethod, associatedSymbol.Locations[0], associatedSymbol, hiddenMember);
					goto IL_00c7;
				}
			}
			goto IL_00ac;
		default:
			throw ExceptionUtilities.UnexpectedValue(hidingMember.DeclaredAccessibility);
		case Accessibility.Private:
		case Accessibility.ProtectedAndInternal:
		case Accessibility.Internal:
			{
				return false;
			}
			IL_00c7:
			return true;
			IL_00a8:
			suppressAccessors = true;
			goto IL_00ac;
			IL_00ac:
			diagnostics.Add(ErrorCode.ERR_HidingAbstractMethod, hidingMemberLocation, hidingMember, hiddenMember);
			goto IL_00c7;
		}
	}

	private static bool OverrideHasCorrectAccessibility(Symbol overridden, Symbol overriding)
	{
		if (!overriding.ContainingAssembly.HasInternalAccessTo(overridden.ContainingAssembly) && overridden.DeclaredAccessibility == Accessibility.ProtectedOrInternal)
		{
			return overriding.DeclaredAccessibility == Accessibility.Protected;
		}
		return overridden.DeclaredAccessibility == overriding.DeclaredAccessibility;
	}

	private void CheckInterfaceUnification(DiagnosticBag diagnostics)
	{
		if (!IsGenericType)
		{
			return;
		}
		int count = InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Count;
		if (count < 2)
		{
			return;
		}
		NamedTypeSymbol[] array = InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.ToArray();
		for (int i = 0; i < count; i++)
		{
			for (int j = i + 1; j < count; j++)
			{
				NamedTypeSymbol namedTypeSymbol = array[i];
				NamedTypeSymbol namedTypeSymbol2 = array[j];
				if (namedTypeSymbol.IsGenericType && namedTypeSymbol2.IsGenericType && namedTypeSymbol.OriginalDefinition == namedTypeSymbol2.OriginalDefinition && namedTypeSymbol.CanUnifyWith(namedTypeSymbol2))
				{
					if (GetImplementsLocation(namedTypeSymbol).SourceSpan.Start > GetImplementsLocation(namedTypeSymbol2).SourceSpan.Start)
					{
						NamedTypeSymbol namedTypeSymbol3 = namedTypeSymbol;
						namedTypeSymbol = namedTypeSymbol2;
						namedTypeSymbol2 = namedTypeSymbol3;
					}
					diagnostics.Add(ErrorCode.ERR_UnifyingInterfaceInstantiations, Locations[0], this, namedTypeSymbol, namedTypeSymbol2);
				}
			}
		}
	}

	private SynthesizedExplicitImplementationForwardingMethod SynthesizeInterfaceMemberImplementation(SymbolAndDiagnostics implementingMemberAndDiagnostics, Symbol interfaceMember)
	{
		foreach (Diagnostic diagnostic in implementingMemberAndDiagnostics.Diagnostics)
		{
			if (diagnostic.Severity == DiagnosticSeverity.Error)
			{
				return null;
			}
		}
		Symbol symbol = implementingMemberAndDiagnostics.Symbol;
		if ((object)symbol == null || symbol.Kind != SymbolKind.Method)
		{
			return null;
		}
		MethodSymbol methodSymbol = (MethodSymbol)interfaceMember;
		MethodSymbol methodSymbol2 = (MethodSymbol)symbol;
		if (methodSymbol2.ExplicitInterfaceImplementations.Contains(methodSymbol))
		{
			return null;
		}
		MethodSymbol originalDefinition = methodSymbol2.OriginalDefinition;
		bool flag = true;
		if (MemberSignatureComparer.RuntimeImplicitImplementationComparer.Equals(methodSymbol2, methodSymbol) && IsOverrideOfPossibleImplementationUnderRuntimeRules(methodSymbol2, methodSymbol.ContainingType))
		{
			if ((object)ContainingModule == originalDefinition.ContainingModule)
			{
				if (originalDefinition is SourceMemberMethodSymbol sourceMemberMethodSymbol)
				{
					sourceMemberMethodSymbol.EnsureMetadataVirtual();
					flag = false;
				}
			}
			else if (methodSymbol2.IsMetadataVirtual(ignoreInterfaceImplementationChanges: true))
			{
				flag = false;
			}
		}
		if (!flag)
		{
			return null;
		}
		return new SynthesizedExplicitImplementationForwardingMethod(methodSymbol, methodSymbol2, this);
	}

	private static bool IsPossibleImplementationUnderRuntimeRules(MethodSymbol implementingMethod, NamedTypeSymbol @interface)
	{
		NamedTypeSymbol containingType = implementingMethod.ContainingType;
		if (containingType.InterfacesAndTheirBaseInterfacesNoUseSiteDiagnostics.Contains(@interface))
		{
			return true;
		}
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = containingType.BaseTypeNoUseSiteDiagnostics;
		if ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			return !baseTypeNoUseSiteDiagnostics.AllInterfacesNoUseSiteDiagnostics.Contains(@interface);
		}
		return true;
	}

	private static bool IsOverrideOfPossibleImplementationUnderRuntimeRules(MethodSymbol implementingMethod, NamedTypeSymbol @interface)
	{
		MethodSymbol methodSymbol = implementingMethod;
		while ((object)methodSymbol != null)
		{
			if (IsPossibleImplementationUnderRuntimeRules(methodSymbol, @interface))
			{
				return true;
			}
			methodSymbol = methodSymbol.OverriddenMethod;
		}
		return false;
	}

	internal sealed override ImmutableArray<NamedTypeSymbol> GetInterfacesToEmit()
	{
		return CalculateInterfacesToEmit();
	}
}
