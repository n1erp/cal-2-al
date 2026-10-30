using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SynthesizedClosureEnvironment : SynthesizedContainer, ISynthesizedMethodBodyImplementationSymbol
{
	private readonly MethodSymbol _topLevelMethod;

	internal readonly SyntaxNode ScopeSyntaxOpt;

	internal readonly int ClosureOrdinal;

	internal readonly MethodSymbol OriginalContainingMethodOpt;

	internal readonly FieldSymbol SingletonCache;

	internal readonly MethodSymbol StaticConstructor;

	private ArrayBuilder<Symbol> _membersBuilder = ArrayBuilder<Symbol>.GetInstance();

	private ImmutableArray<Symbol> _members;

	public override TypeKind TypeKind { get; }

	internal override MethodSymbol Constructor { get; }

	public override bool IsSerializable => (object)SingletonCache != null;

	public override Symbol ContainingSymbol => _topLevelMethod.ContainingSymbol;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => true;

	IMethodSymbol ISynthesizedMethodBodyImplementationSymbol.Method => _topLevelMethod;

	internal SynthesizedClosureEnvironment(MethodSymbol topLevelMethod, MethodSymbol containingMethod, bool isStruct, SyntaxNode scopeSyntaxOpt, DebugId methodId, DebugId closureId)
		: base(MakeName(scopeSyntaxOpt, methodId, closureId), containingMethod)
	{
		TypeKind = (isStruct ? TypeKind.Struct : TypeKind.Class);
		_topLevelMethod = topLevelMethod;
		OriginalContainingMethodOpt = containingMethod;
		Constructor = (isStruct ? null : new SynthesizedClosureEnvironmentConstructor(this));
		ClosureOrdinal = closureId.Ordinal;
		if (scopeSyntaxOpt == null)
		{
			StaticConstructor = new SynthesizedStaticConstructor(this);
			string name = GeneratedNames.MakeCachedFrameInstanceFieldName();
			SingletonCache = new SynthesizedLambdaCacheFieldSymbol(this, this, name, topLevelMethod, isReadOnly: true, isStatic: true);
		}
		ScopeSyntaxOpt = scopeSyntaxOpt;
	}

	internal void AddHoistedField(LambdaCapturedVariable captured)
	{
		_membersBuilder.Add(captured);
	}

	private static string MakeName(SyntaxNode scopeSyntaxOpt, DebugId methodId, DebugId closureId)
	{
		if (scopeSyntaxOpt == null)
		{
			return GeneratedNames.MakeStaticLambdaDisplayClassName(methodId.Ordinal, methodId.Generation);
		}
		return GeneratedNames.MakeLambdaDisplayClassName(methodId.Ordinal, methodId.Generation, closureId.Ordinal, closureId.Generation);
	}

	[Conditional("DEBUG")]
	private static void AssertIsClosureScopeSyntax(SyntaxNode syntaxOpt)
	{
		if (syntaxOpt == null || LambdaUtilities.IsClosureScope(syntaxOpt))
		{
			return;
		}
		throw ExceptionUtilities.UnexpectedValue(syntaxOpt.Kind());
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (_members.IsDefault)
		{
			ArrayBuilder<Symbol> membersBuilder = _membersBuilder;
			if ((object)StaticConstructor != null)
			{
				membersBuilder.Add(StaticConstructor);
				membersBuilder.Add(SingletonCache);
			}
			membersBuilder.AddRange(base.GetMembers());
			_members = membersBuilder.ToImmutableAndFree();
			_membersBuilder = null;
		}
		return _members;
	}

	internal override IEnumerable<FieldSymbol> GetFieldsToEmit()
	{
		if ((object)SingletonCache == null)
		{
			return SpecializedCollections.EmptyEnumerable<FieldSymbol>();
		}
		return SpecializedCollections.SingletonEnumerable(SingletonCache);
	}
}
