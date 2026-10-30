using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;

internal sealed class PEAttributeData : CSharpAttributeData
{
	private readonly MetadataDecoder _decoder;

	private readonly CustomAttributeHandle _handle;

	private NamedTypeSymbol _lazyAttributeClass = ErrorTypeSymbol.UnknownResultType;

	private MethodSymbol _lazyAttributeConstructor;

	private ImmutableArray<TypedConstant> _lazyConstructorArguments;

	private ImmutableArray<KeyValuePair<string, TypedConstant>> _lazyNamedArguments;

	private ThreeState _lazyHasErrors;

	public override NamedTypeSymbol AttributeClass
	{
		get
		{
			EnsureClassAndConstructorSymbolsAreLoaded();
			return _lazyAttributeClass;
		}
	}

	public override MethodSymbol AttributeConstructor
	{
		get
		{
			EnsureClassAndConstructorSymbolsAreLoaded();
			return _lazyAttributeConstructor;
		}
	}

	public override SyntaxReference ApplicationSyntaxReference => null;

	protected override ImmutableArray<TypedConstant> CommonConstructorArguments
	{
		protected internal get
		{
			EnsureAttributeArgumentsAreLoaded();
			return _lazyConstructorArguments;
		}
	}

	protected override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments
	{
		protected internal get
		{
			EnsureAttributeArgumentsAreLoaded();
			return _lazyNamedArguments;
		}
	}

	internal override bool HasErrors
	{
		get
		{
			if (_lazyHasErrors == ThreeState.Unknown)
			{
				EnsureClassAndConstructorSymbolsAreLoaded();
				EnsureAttributeArgumentsAreLoaded();
				if (_lazyHasErrors == ThreeState.Unknown)
				{
					_lazyHasErrors = ThreeState.False;
				}
			}
			return _lazyHasErrors.Value();
		}
	}

	internal PEAttributeData(PEModuleSymbol moduleSymbol, CustomAttributeHandle handle)
	{
		_decoder = new MetadataDecoder(moduleSymbol);
		_handle = handle;
	}

	private void EnsureClassAndConstructorSymbolsAreLoaded()
	{
		if ((object)_lazyAttributeClass == ErrorTypeSymbol.UnknownResultType)
		{
			if (!_decoder.GetCustomAttribute(_handle, out TypeSymbol attributeClass, out MethodSymbol attributeCtor))
			{
				_lazyHasErrors = ThreeState.True;
			}
			else if ((object)attributeClass == null || attributeClass.IsErrorType() || (object)attributeCtor == null)
			{
				_lazyHasErrors = ThreeState.True;
			}
			Interlocked.CompareExchange(ref _lazyAttributeConstructor, attributeCtor, null);
			Interlocked.CompareExchange(ref _lazyAttributeClass, (NamedTypeSymbol)attributeClass, ErrorTypeSymbol.UnknownResultType);
		}
	}

	private void EnsureAttributeArgumentsAreLoaded()
	{
		if (_lazyConstructorArguments.IsDefault || _lazyNamedArguments.IsDefault)
		{
			TypedConstant[] positionalArgs = null;
			KeyValuePair<string, TypedConstant>[] namedArgs = null;
			if (!_decoder.GetCustomAttribute(_handle, out positionalArgs, out namedArgs))
			{
				_lazyHasErrors = ThreeState.True;
			}
			ImmutableInterlocked.InterlockedInitialize(ref _lazyConstructorArguments, ImmutableArray.Create(positionalArgs));
			ImmutableInterlocked.InterlockedInitialize(ref _lazyNamedArguments, ImmutableArray.Create(namedArgs));
		}
	}

	internal override bool IsTargetAttribute(string namespaceName, string typeName)
	{
		return _decoder.IsTargetAttribute(_handle, namespaceName, typeName);
	}

	internal override int GetTargetAttributeSignatureIndex(Symbol targetSymbol, AttributeDescription description)
	{
		return _decoder.GetTargetAttributeSignatureIndex(_handle, description);
	}
}
