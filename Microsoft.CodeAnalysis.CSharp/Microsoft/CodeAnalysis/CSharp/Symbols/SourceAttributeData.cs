using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SourceAttributeData : CSharpAttributeData
{
	private readonly NamedTypeSymbol _attributeClass;

	private readonly MethodSymbol _attributeConstructor;

	private readonly ImmutableArray<TypedConstant> _constructorArguments;

	private readonly ImmutableArray<int> _constructorArgumentsSourceIndices;

	private readonly ImmutableArray<KeyValuePair<string, TypedConstant>> _namedArguments;

	private readonly bool _isConditionallyOmitted;

	private readonly bool _hasErrors;

	private readonly SyntaxReference _applicationNode;

	public override NamedTypeSymbol AttributeClass => _attributeClass;

	public override MethodSymbol AttributeConstructor => _attributeConstructor;

	public override SyntaxReference ApplicationSyntaxReference => _applicationNode;

	internal ImmutableArray<int> ConstructorArgumentsSourceIndices => _constructorArgumentsSourceIndices;

	internal override bool IsConditionallyOmitted => _isConditionallyOmitted;

	internal override bool HasErrors => _hasErrors;

	protected sealed override ImmutableArray<TypedConstant> CommonConstructorArguments
	{
		protected internal get
		{
			return _constructorArguments;
		}
	}

	protected sealed override ImmutableArray<KeyValuePair<string, TypedConstant>> CommonNamedArguments
	{
		protected internal get
		{
			return _namedArguments;
		}
	}

	internal SourceAttributeData(SyntaxReference applicationNode, NamedTypeSymbol attributeClass, MethodSymbol attributeConstructor, ImmutableArray<TypedConstant> constructorArguments, ImmutableArray<int> constructorArgumentsSourceIndices, ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments, bool hasErrors, bool isConditionallyOmitted)
	{
		_attributeClass = attributeClass;
		_attributeConstructor = attributeConstructor;
		_constructorArguments = constructorArguments;
		_constructorArgumentsSourceIndices = constructorArgumentsSourceIndices;
		_namedArguments = namedArguments;
		_isConditionallyOmitted = isConditionallyOmitted;
		_hasErrors = hasErrors;
		_applicationNode = applicationNode;
	}

	internal SourceAttributeData(SyntaxReference applicationNode, NamedTypeSymbol attributeClass, MethodSymbol attributeConstructor, bool hasErrors)
		: this(applicationNode, attributeClass, attributeConstructor, ImmutableArray<TypedConstant>.Empty, default, ImmutableArray<KeyValuePair<string, TypedConstant>>.Empty, hasErrors, isConditionallyOmitted: false)
	{
	}

	internal CSharpSyntaxNode GetAttributeArgumentSyntax(int parameterIndex, AttributeSyntax attributeSyntax)
	{
		if (_constructorArgumentsSourceIndices.IsDefault)
		{
			return attributeSyntax.ArgumentList.Arguments[parameterIndex];
		}
		int num = _constructorArgumentsSourceIndices[parameterIndex];
		if (num == -1)
		{
			return attributeSyntax.Name;
		}
		return attributeSyntax.ArgumentList.Arguments[num];
	}

	internal SourceAttributeData WithOmittedCondition(bool isConditionallyOmitted)
	{
		if (IsConditionallyOmitted == isConditionallyOmitted)
		{
			return this;
		}
		return new SourceAttributeData(ApplicationSyntaxReference, AttributeClass, AttributeConstructor, CommonConstructorArguments, ConstructorArgumentsSourceIndices, CommonNamedArguments, HasErrors, isConditionallyOmitted);
	}

	internal override int GetTargetAttributeSignatureIndex(Symbol targetSymbol, AttributeDescription description)
	{
		if (!IsTargetAttribute(description.Namespace, description.Name))
		{
			return -1;
		}
		MethodSymbol attributeConstructor = AttributeConstructor;
		if ((object)attributeConstructor == null)
		{
			return -1;
		}
		TypeSymbol typeSymbol = null;
		ImmutableArray<ParameterSymbol> parameters = attributeConstructor.Parameters;
		for (int i = 0; i < description.Signatures.Length; i++)
		{
			byte[] array = description.Signatures[i];
			if (array[0] != 32 || array[1] != parameters.Length || array[2] != 1)
			{
				continue;
			}
			bool flag = array.Length == 3;
			int num = 0;
			for (int j = 3; j < array.Length; j++)
			{
				if (num >= parameters.Length)
				{
					break;
				}
				TypeSymbol type = parameters[num].Type;
				SpecialType specialType = type.SpecialType;
				byte b = array[j];
				if (b == 64)
				{
					j++;
					if (type.Kind != SymbolKind.NamedType && type.Kind != SymbolKind.ErrorType)
					{
						flag = false;
						break;
					}
					NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)type;
					AttributeDescription.TypeHandleTargetInfo typeHandleTargetInfo = AttributeDescription.TypeHandleTargets[array[j]];
					if (!string.Equals(namedTypeSymbol.MetadataName, typeHandleTargetInfo.Name, StringComparison.Ordinal) || !namedTypeSymbol.HasNameQualifier(typeHandleTargetInfo.Namespace))
					{
						flag = false;
						break;
					}
					b = (byte)typeHandleTargetInfo.Underlying;
					if (type.IsEnumType())
					{
						specialType = type.GetEnumUnderlyingType().SpecialType;
					}
				}
				else if (type.IsArray())
				{
					specialType = ((ArrayTypeSymbol)type).ElementType.SpecialType;
				}
				switch (b)
				{
				case 2:
					flag = specialType == SpecialType.System_Boolean;
					num++;
					break;
				case 3:
					flag = specialType == SpecialType.System_Char;
					num++;
					break;
				case 4:
					flag = specialType == SpecialType.System_SByte;
					num++;
					break;
				case 5:
					flag = specialType == SpecialType.System_Byte;
					num++;
					break;
				case 6:
					flag = specialType == SpecialType.System_Int16;
					num++;
					break;
				case 7:
					flag = specialType == SpecialType.System_UInt16;
					num++;
					break;
				case 8:
					flag = specialType == SpecialType.System_Int32;
					num++;
					break;
				case 9:
					flag = specialType == SpecialType.System_UInt32;
					num++;
					break;
				case 10:
					flag = specialType == SpecialType.System_Int64;
					num++;
					break;
				case 11:
					flag = specialType == SpecialType.System_UInt64;
					num++;
					break;
				case 12:
					flag = specialType == SpecialType.System_Single;
					num++;
					break;
				case 13:
					flag = specialType == SpecialType.System_Double;
					num++;
					break;
				case 14:
					flag = specialType == SpecialType.System_String;
					num++;
					break;
				case 28:
					flag = specialType == SpecialType.System_Object;
					num++;
					break;
				case 80:
					if ((object)typeSymbol == null)
					{
						typeSymbol = GetSystemType(targetSymbol);
					}
					flag = type == typeSymbol;
					num++;
					break;
				case 29:
					flag = type.IsArray();
					break;
				default:
					return -1;
				}
				if (!flag)
				{
					break;
				}
			}
			if (flag)
			{
				return i;
			}
		}
		return -1;
	}

	internal virtual TypeSymbol GetSystemType(Symbol targetSymbol)
	{
		return targetSymbol.DeclaringCompilation.GetWellKnownType(WellKnownType.System_Type);
	}
}
