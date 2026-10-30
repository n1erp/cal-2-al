using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class MergedModuleDeclaration : MergedDeclaration
{
	private readonly ImmutableArray<SingleObjectCollectionDeclaration> collections;

	private ImmutableArray<SingleObjectDeclaration> lazyChildren;

	public override DeclarationKind Kind => DeclarationKind.ObjectCollection;

	public ImmutableArray<Location> NameLocations
	{
		get
		{
			if (collections.Length == 1)
			{
				return ImmutableArray.Create((Location)collections[0].NameLocation);
			}
			ArrayBuilder<Location> instance = ArrayBuilder<Location>.GetInstance();
			foreach (SingleObjectCollectionDeclaration collection in collections)
			{
				SourceLocation nameLocation = collection.NameLocation;
				if (nameLocation != null)
				{
					instance.Add(nameLocation);
				}
			}
			return instance.ToImmutableAndFree();
		}
	}

	public ImmutableArray<SingleObjectCollectionDeclaration> Collections => collections;

	public new ImmutableArray<SingleObjectDeclaration> Children
	{
		get
		{
			if (lazyChildren.IsDefault)
			{
				ImmutableInterlocked.InterlockedInitialize(ref lazyChildren, MakeChildren());
			}
			return lazyChildren;
		}
	}

	private MergedModuleDeclaration(ImmutableArray<SingleObjectCollectionDeclaration> collections)
		: base(collections.IsEmpty ? string.Empty : collections[0].Name)
	{
		this.collections = collections;
	}

	public static MergedModuleDeclaration Create(ImmutableArray<SingleObjectCollectionDeclaration> declarations)
	{
		return new MergedModuleDeclaration(declarations);
	}

	public static MergedModuleDeclaration Create(SingleObjectCollectionDeclaration declaration)
	{
		return new MergedModuleDeclaration(ImmutableArray.Create(declaration));
	}

	public static MergedModuleDeclaration Create(MergedModuleDeclaration mergedDeclaration, SingleObjectCollectionDeclaration declaration)
	{
		return new MergedModuleDeclaration(mergedDeclaration.collections.Add(declaration));
	}

	protected override ImmutableArray<Declaration> GetDeclarationChildren()
	{
		return StaticCast<Declaration>.From(Children);
	}

	private ImmutableArray<SingleObjectDeclaration> MakeChildren()
	{
		ArrayBuilder<SingleObjectDeclaration> instance = ArrayBuilder<SingleObjectDeclaration>.GetInstance();
		foreach (SingleObjectCollectionDeclaration collection in collections)
		{
			foreach (SingleDeclaration child in collection.Children)
			{
				if (child is SingleObjectDeclaration item)
				{
					instance.Add(item);
				}
			}
		}
		return instance.ToImmutableAndFree();
	}
}
