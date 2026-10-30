using System;
using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SingleTypeDeclaration : SingleNamespaceOrTypeDeclaration
{
	[Flags]
	internal enum TypeDeclarationFlags : byte
	{
		None = 0,
		AnyMemberHasExtensionMethodSyntax = 2,
		HasAnyAttributes = 4,
		HasBaseDeclarations = 8,
		AnyMemberHasAttributes = 0x10,
		HasAnyNontypeMembers = 0x20
	}

	internal struct TypeDeclarationIdentity : IEquatable<TypeDeclarationIdentity>
	{
		private readonly SingleTypeDeclaration _decl;

		internal TypeDeclarationIdentity(SingleTypeDeclaration decl)
		{
			_decl = decl;
		}

		public override bool Equals(object obj)
		{
			if (obj is TypeDeclarationIdentity)
			{
				return Equals((TypeDeclarationIdentity)obj);
			}
			return false;
		}

		public bool Equals(TypeDeclarationIdentity other)
		{
			SingleTypeDeclaration decl = _decl;
			SingleTypeDeclaration decl2 = other._decl;
			if (decl == decl2)
			{
				return true;
			}
			if (decl._arity != decl2._arity || decl._kind != decl2._kind || decl.name != decl2.name)
			{
				return false;
			}
			if (decl._kind == DeclarationKind.Enum || decl._kind == DeclarationKind.Delegate)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			SingleTypeDeclaration decl = _decl;
			return Hash.Combine(decl.Name.GetHashCode(), Hash.Combine(decl.Arity.GetHashCode(), (int)decl.Kind));
		}
	}

	private readonly DeclarationKind _kind;

	private readonly TypeDeclarationFlags _flags;

	private readonly ushort _arity;

	private readonly DeclarationModifiers _modifiers;

	private readonly ImmutableArray<SingleTypeDeclaration> _children;

	public override DeclarationKind Kind => _kind;

	public new ImmutableArray<SingleTypeDeclaration> Children => _children;

	public int Arity => _arity;

	public DeclarationModifiers Modifiers => _modifiers;

	public ImmutableHashSet<string> MemberNames { get; }

	public bool AnyMemberHasExtensionMethodSyntax => (_flags & TypeDeclarationFlags.AnyMemberHasExtensionMethodSyntax) != 0;

	public bool HasAnyAttributes => (_flags & TypeDeclarationFlags.HasAnyAttributes) != 0;

	public bool HasBaseDeclarations => (_flags & TypeDeclarationFlags.HasBaseDeclarations) != 0;

	public bool AnyMemberHasAttributes => (_flags & TypeDeclarationFlags.AnyMemberHasAttributes) != 0;

	public bool HasAnyNontypeMembers => (_flags & TypeDeclarationFlags.HasAnyNontypeMembers) != 0;

	internal TypeDeclarationIdentity Identity => new TypeDeclarationIdentity(this);

	internal SingleTypeDeclaration(DeclarationKind kind, string name, int arity, DeclarationModifiers modifiers, TypeDeclarationFlags declFlags, SyntaxReference syntaxReference, SourceLocation nameLocation, ImmutableHashSet<string> memberNames, ImmutableArray<SingleTypeDeclaration> children, ImmutableArray<Diagnostic> diagnostics)
		: base(name, syntaxReference, nameLocation, diagnostics)
	{
		_kind = kind;
		_arity = (ushort)arity;
		_modifiers = modifiers;
		MemberNames = memberNames;
		_children = children;
		_flags = declFlags;
	}

	protected override ImmutableArray<SingleNamespaceOrTypeDeclaration> GetNamespaceOrTypeDeclarationChildren()
	{
		return StaticCast<SingleNamespaceOrTypeDeclaration>.From(_children);
	}
}
