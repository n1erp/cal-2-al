using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class MergedNamespaceDeclaration : MergedNamespaceOrTypeDeclaration
{
	private readonly ImmutableArray<SingleNamespaceDeclaration> _declarations;

	private ImmutableArray<MergedNamespaceOrTypeDeclaration> _lazyChildren;

	public override DeclarationKind Kind => DeclarationKind.Namespace;

	public ImmutableArray<Location> NameLocations
	{
		get
		{
			if (_declarations.Length == 1)
			{
				return ImmutableArray.Create((Location)_declarations[0].NameLocation);
			}
			ArrayBuilder<Location> instance = ArrayBuilder<Location>.GetInstance();
			foreach (SingleNamespaceDeclaration declaration in _declarations)
			{
				SourceLocation nameLocation = declaration.NameLocation;
				if (nameLocation != null)
				{
					instance.Add(nameLocation);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	public ImmutableArray<SingleNamespaceDeclaration> Declarations => _declarations;

	public new ImmutableArray<MergedNamespaceOrTypeDeclaration> Children
	{
		get
		{
			if (_lazyChildren.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref _lazyChildren, MakeChildren());
			}
			return _lazyChildren;
		}
	}

	private MergedNamespaceDeclaration(ImmutableArray<SingleNamespaceDeclaration> declarations)
		: base(declarations.IsEmpty ? string.Empty : declarations[0].Name)
	{
		_declarations = declarations;
	}

	public static MergedNamespaceDeclaration Create(ImmutableArray<SingleNamespaceDeclaration> declarations)
	{
		return new MergedNamespaceDeclaration(declarations);
	}

	public static MergedNamespaceDeclaration Create(SingleNamespaceDeclaration declaration)
	{
		return new MergedNamespaceDeclaration(ImmutableArray.Create(declaration));
	}

	public LexicalSortKey GetLexicalSortKey(CSharpCompilation compilation)
	{
		LexicalSortKey lexicalSortKey = new LexicalSortKey(_declarations[0].NameLocation, compilation);
		for (int i = 1; i < _declarations.Length; i++)
		{
			lexicalSortKey = LexicalSortKey.First(lexicalSortKey, new LexicalSortKey(_declarations[i].NameLocation, compilation));
		}
		return lexicalSortKey;
	}

	protected override ImmutableArray<Declaration> GetDeclarationChildren()
	{
		return StaticCast<Declaration>.From(Children);
	}

	private ImmutableArray<MergedNamespaceOrTypeDeclaration> MakeChildren()
	{
		ArrayBuilder<SingleNamespaceDeclaration> arrayBuilder = null;
		ArrayBuilder<SingleTypeDeclaration> arrayBuilder2 = null;
		bool flag = true;
		bool flag2 = true;
		foreach (SingleNamespaceDeclaration declaration in _declarations)
		{
			foreach (SingleNamespaceOrTypeDeclaration child in declaration.Children)
			{
				if (child is SingleTypeDeclaration singleTypeDeclaration)
				{
					if (arrayBuilder2 == null)
					{
						arrayBuilder2 = ArrayBuilder<SingleTypeDeclaration>.GetInstance();
					}
					else if (flag2 && !singleTypeDeclaration.Identity.Equals(arrayBuilder2[0].Identity))
					{
						flag2 = false;
					}
					arrayBuilder2.Add(singleTypeDeclaration);
				}
				else if (child is SingleNamespaceDeclaration singleNamespaceDeclaration)
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<SingleNamespaceDeclaration>.GetInstance();
					}
					else if (flag && !singleNamespaceDeclaration.Name.Equals(arrayBuilder[0].Name))
					{
						flag = false;
					}
					arrayBuilder.Add(singleNamespaceDeclaration);
				}
			}
		}
		ArrayBuilder<MergedNamespaceOrTypeDeclaration> instance = ArrayBuilder<MergedNamespaceOrTypeDeclaration>.GetInstance();
		if (arrayBuilder != null)
		{
			if (flag)
			{
				instance.Add(Create(arrayBuilder.ToImmutableAndFree()));
			}
			else
			{
				Dictionary<string, ImmutableArray<SingleNamespaceDeclaration>> dictionary = arrayBuilder.ToDictionary((SingleNamespaceDeclaration n) => n.Name, StringOrdinalComparer.Instance);
				arrayBuilder.Free();
				foreach (ImmutableArray<SingleNamespaceDeclaration> value in dictionary.Values)
				{
					instance.Add(Create(value));
				}
			}
		}
		if (arrayBuilder2 != null)
		{
			if (flag2)
			{
				instance.Add(new MergedTypeDeclaration(arrayBuilder2.ToImmutableAndFree()));
			}
			else
			{
				Dictionary<SingleTypeDeclaration.TypeDeclarationIdentity, ImmutableArray<SingleTypeDeclaration>> dictionary2 = arrayBuilder2.ToDictionary((SingleTypeDeclaration t) => t.Identity);
				arrayBuilder2.Free();
				foreach (ImmutableArray<SingleTypeDeclaration> value2 in dictionary2.Values)
				{
					instance.Add(new MergedTypeDeclaration(value2));
				}
			}
		}
		return instance.ToImmutableAndFree();
	}
}
