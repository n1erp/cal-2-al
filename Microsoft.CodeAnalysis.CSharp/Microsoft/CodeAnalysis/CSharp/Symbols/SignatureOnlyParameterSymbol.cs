using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SignatureOnlyParameterSymbol : ParameterSymbol
{
	private readonly TypeSymbol _type;

	private readonly ImmutableArray<CustomModifier> _customModifiers;

	private readonly ImmutableArray<CustomModifier> _refCustomModifiers;

	private readonly bool _isParams;

	private readonly RefKind _refKind;

	public override TypeSymbol Type => _type;

	public override ImmutableArray<CustomModifier> CustomModifiers => _customModifiers;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => _refCustomModifiers;

	public override bool IsParams => _isParams;

	public override RefKind RefKind => _refKind;

	public override string Name => "";

	public override bool IsImplicitlyDeclared => true;

	internal override bool IsMetadataIn
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override bool IsMetadataOut
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override MarshalPseudoCustomAttributeData MarshallingInformation
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public override int Ordinal
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override bool IsMetadataOptional
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override ConstantValue ExplicitDefaultConstantValue
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override bool IsIDispatchConstant
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override bool IsIUnknownConstant
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override bool IsCallerFilePath
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override bool IsCallerLineNumber
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override bool IsCallerMemberName
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public override Symbol ContainingSymbol
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public override ImmutableArray<Location> Locations
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public override AssemblySymbol ContainingAssembly
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	internal override ModuleSymbol ContainingModule
	{
		get
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	public SignatureOnlyParameterSymbol(TypeSymbol type, ImmutableArray<CustomModifier> customModifiers, ImmutableArray<CustomModifier> refCustomModifiers, bool isParams, RefKind refKind)
	{
		_type = type;
		_customModifiers = customModifiers;
		_refCustomModifiers = refCustomModifiers;
		_isParams = isParams;
		_refKind = refKind;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is SignatureOnlyParameterSymbol signatureOnlyParameterSymbol && _type == signatureOnlyParameterSymbol._type && _customModifiers.SequenceEqual(signatureOnlyParameterSymbol._customModifiers) && _refCustomModifiers.SequenceEqual(signatureOnlyParameterSymbol._refCustomModifiers) && _isParams == signatureOnlyParameterSymbol._isParams)
		{
			return _refKind == signatureOnlyParameterSymbol._refKind;
		}
		return false;
	}

	public override int GetHashCode()
	{
		int hashCode = _type.GetHashCode();
		bool isParams = _isParams;
		return Hash.Combine(hashCode, Hash.Combine(isParams.GetHashCode(), _refKind.GetHashCode()));
	}
}
