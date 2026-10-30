using System;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class AttributeValidatorHelper
{
	private delegate NavDiagnosticInfo AttributeOverloadScoreCalculator(ErrorCode code, Location location, params object[] args);

	internal const string Sender = "sender";

	internal static bool HasErrors(this AttributeSymbol symbol)
	{
		if (!(symbol == null) && symbol.Attribute != null)
		{
			return symbol.Attribute.HasAnyErrors;
		}
		return true;
	}

	internal static void CheckEventReturn(SourceMethodOrTriggerSymbol methodSymbol, DiagnosticBag diagnostics)
	{
		if (methodSymbol.ReturnValue != null && methodSymbol.ReturnValue.ReturnType.NavTypeKind != NavTypeKind.None && !methodSymbol.HasAttribute(AttributeKind.TryFunction))
		{
			diagnostics.Add(ErrorCode.ERR_EventReturnNotValid, methodSymbol.ReturnValue.GetLocation(), methodSymbol.Name);
		}
	}

	internal static bool IsAttributeOnVariableOfCompatibleType(this AttributeSymbol symbol, DiagnosticBag diagnostics, params NavTypeKind[] compatibleKinds)
	{
		TypeSymbol attributeTargetVariableType = symbol.GetAttributeTargetVariableType();
		if (!compatibleKinds.Contains(attributeTargetVariableType.NavTypeKind))
		{
			diagnostics.Add(ErrorCode.ERR_InvalidAttributeForVariableType, symbol.GetLocation(), symbol.AttributeInfo.Kind.ToString(), attributeTargetVariableType.NavTypeKind.ToString());
			return false;
		}
		return true;
	}

	internal static void CheckMethodSignature(SourceMethodOrTriggerSymbol methodSymbol, DiagnosticBag diagnostics)
	{
		Version version = methodSymbol.SyntaxTree.Options?.RuntimeVersion;
		foreach (AttributeSymbol attribute in methodSymbol.Attributes)
		{
			if (attribute.AttributeInfo == null || !attribute.AttributeInfo.HasRequiredSignature)
			{
				continue;
			}
			if (attribute.AttributeInfo.RequiredMethodSignature.Length == 1)
			{
				AttributeRequiredMethodSignature signature = attribute.AttributeInfo.RequiredMethodSignature[0];
				VerifyAttributeMethodSignature(version, methodSymbol, attribute, signature, diagnostics);
				continue;
			}
			AttributeMethodSignatureResolutionResult attributeMethodSignatureResolutionResult = ResolveRequiredSignatureOverload(version, methodSymbol, attribute);
			if (!attributeMethodSignatureResolutionResult.IsExactMatch || !attributeMethodSignatureResolutionResult.IsVersionCompatible)
			{
				VerifyAttributeMethodSignature(version, methodSymbol, attribute, attributeMethodSignatureResolutionResult.Signature, diagnostics);
			}
		}
	}

	internal static void CheckMutuallyExclusiveAndDuplicateAttributes(SourceMethodOrTriggerSymbol methodSymbol, DiagnosticBag diagnostics)
	{
		for (int i = 0; i < methodSymbol.Attributes.Length; i++)
		{
			for (int j = i + 1; j < methodSymbol.Attributes.Length; j++)
			{
				if (methodSymbol.Attributes[i].Name == methodSymbol.Attributes[j].Name)
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateAttribute, methodSymbol.Attributes[i].GetLocation(), methodSymbol.Attributes[i].Attribute.ValueText);
				}
				else if (IsMutuallyExclusiveAttributes(methodSymbol.Attributes[i], methodSymbol.Attributes[j]))
				{
					diagnostics.Add(ErrorCode.ERR_MutuallyExclusiveAttributes, methodSymbol.Attributes[i].GetLocation(), methodSymbol.Attributes[i].Attribute.ValueText, methodSymbol.Attributes[j].Attribute.ValueText);
				}
			}
		}
	}

	private static void VerifyAttributeMethodSignature(Version version, SourceMethodOrTriggerSymbol methodSymbol, AttributeSymbol attribute, AttributeRequiredMethodSignature signature, DiagnosticBag diagnostics)
	{
		if (IsAttributeVersionCompatible(version, methodSymbol, signature, diagnostics))
		{
			VerifyAttributeMethodSignature(methodSymbol, attribute, signature, diagnostics.Add);
		}
	}

	private static AttributeMethodSignatureResolutionResult ResolveRequiredSignatureOverload(Version version, SourceMethodOrTriggerSymbol methodSymbol, AttributeSymbol attribute)
	{
		AttributeMethodSignatureResolutionResult attributeMethodSignatureResolutionResult = AttributeMethodSignatureResolutionResult.Default;
		foreach (AttributeRequiredMethodSignature item in attribute.AttributeInfo.RequiredMethodSignature)
		{
			bool flag = IsAttributeVersionCompatible(version, item);
			if (!attributeMethodSignatureResolutionResult.IsVersionCompatible || flag)
			{
				AttributeMethodSignatureResolutionResult attributeMethodSignatureResolutionResult2 = new AttributeMethodSignatureResolutionResult(item, flag);
				VerifyAttributeMethodSignature(methodSymbol, attribute, item, attributeMethodSignatureResolutionResult2.ScoreCalculator);
				if (attributeMethodSignatureResolutionResult2.IsExactMatch && attributeMethodSignatureResolutionResult2.IsVersionCompatible)
				{
					return attributeMethodSignatureResolutionResult2;
				}
				if (attributeMethodSignatureResolutionResult2.IsBetterThan(attributeMethodSignatureResolutionResult))
				{
					attributeMethodSignatureResolutionResult = attributeMethodSignatureResolutionResult2;
				}
			}
		}
		return attributeMethodSignatureResolutionResult;
	}

	private static void VerifyAttributeMethodSignature(SourceMethodOrTriggerSymbol methodSymbol, AttributeSymbol attribute, AttributeRequiredMethodSignature signature, AttributeOverloadScoreCalculator scoreCalc)
	{
		if (signature.RequirePublic && methodSymbol.IsLocal)
		{
			scoreCalc(ErrorCode.ERR_VisibilityDoesNotMatchAttribute, methodSymbol.GetLocation(), methodSymbol.Name, attribute.Name);
		}
		if (signature.RequiredParameters != null)
		{
			if (methodSymbol.Parameters.Length < signature.MinRequiredSignatureParameterCount || methodSymbol.Parameters.Length > signature.MaxRequiredSignatureParameterCount)
			{
				scoreCalc(ErrorCode.ERR_ParameterCountDoesNotMatchAttribute, methodSymbol.GetLocation(), methodSymbol.Name, attribute.Name, signature.GetRequiredMethodSignature(methodSymbol.Name));
				return;
			}
			CheckParameterTypes(methodSymbol, signature, attribute, scoreCalc);
		}
		TypeSymbol typeSymbol = signature.RequiredReturnValue?.Type;
		if (typeSymbol != null)
		{
			TypeSymbol typeSymbol2 = ((methodSymbol.ReturnValue != null) ? methodSymbol.ReturnValue.Type : NavCorLib.NoneType);
			if (typeSymbol != typeSymbol2)
			{
				scoreCalc(ErrorCode.ERR_ReturnValueTypeDoesNotMatchAttribute, methodSymbol.GetLocation(), methodSymbol.Name, attribute.Name, typeSymbol.ToDisplayString(), typeSymbol2.ToDisplayString(), signature.GetRequiredMethodSignature(methodSymbol.Name));
			}
		}
	}

	private static void CheckParameterTypes(SourceMethodOrTriggerSymbol methodSymbol, AttributeRequiredMethodSignature signature, AttributeSymbol attribute, AttributeOverloadScoreCalculator scoreCalc)
	{
		for (int i = 0; i < signature.RequiredParameters.Length && i < methodSymbol.ParameterCount; i++)
		{
			ParameterSymbol parameterSymbol = methodSymbol.Parameters[i];
			AttributeParameter attributeParameter = signature.RequiredParameters[i];
			if (!IsParameterTypeCompatibleWithAttributeExpectedType(parameterSymbol, attributeParameter))
			{
				string text = (parameterSymbol.IsVar ? "var " : string.Empty);
				string text2 = (attributeParameter.IsVar ? "var " : string.Empty);
				scoreCalc(ErrorCode.ERR_ParameterTypeDoesNotMatchAttribute, parameterSymbol.GetLocation(), methodSymbol.Name, attribute.Name, i + 1, text2 + attributeParameter.Type.ToDisplayString(), text + parameterSymbol.Type.ToDisplayString(), signature.GetRequiredMethodSignature(methodSymbol.Name));
			}
		}
	}

	private static bool IsAttributeVersionCompatible(Version runtimeVersion, AttributeRequiredMethodSignature signature)
	{
		if (runtimeVersion == null)
		{
			return true;
		}
		if (signature.Compatibility == null)
		{
			return true;
		}
		return signature.Compatibility.IsSupported(runtimeVersion);
	}

	private static bool IsAttributeVersionCompatible(Version runtimeVersion, SourceMethodOrTriggerSymbol methodSymbol, AttributeRequiredMethodSignature signature, DiagnosticBag diagnostics)
	{
		bool flag = IsAttributeVersionCompatible(runtimeVersion, signature);
		if (!flag)
		{
			diagnostics.Add(ErrorCode.ERR_FeatureNotAvailable, methodSymbol.GetLocation(), signature.GetRequiredMethodSignature(methodSymbol.Name), runtimeVersion.ToMajorMinorString(), signature.Compatibility);
		}
		return flag;
	}

	private static bool IsParameterTypeCompatibleWithAttributeExpectedType(ParameterSymbol actualParameter, AttributeParameter expectedParameter)
	{
		NavTypeKind navTypeKind = expectedParameter.Type.NavTypeKind;
		if ((navTypeKind != NavTypeKind.TestRequestPage && navTypeKind != NavTypeKind.TestPage && (uint)(navTypeKind - 393318) > 1u) ? (actualParameter.Type == expectedParameter.Type) : (actualParameter.Type.NavTypeKind == expectedParameter.Type.NavTypeKind))
		{
			return actualParameter.IsVar == expectedParameter.IsVar;
		}
		return false;
	}

	private static bool IsMutuallyExclusiveAttributes(AttributeSymbol a, AttributeSymbol b)
	{
		if (a.AttributeInfo?.Category == b.AttributeInfo?.Category)
		{
			return true;
		}
		AttributeTypeInfo attributeInfo = a.AttributeInfo;
		if (attributeInfo != null && attributeInfo.ContainsTag(AttributeTagKind.Event))
		{
			AttributeTypeInfo attributeInfo2 = b.AttributeInfo;
			if (attributeInfo2 != null && attributeInfo2.ContainsTag(AttributeTagKind.Event))
			{
				return true;
			}
		}
		AttributeTypeInfo attributeInfo3 = a.AttributeInfo;
		if (attributeInfo3 != null && attributeInfo3.Kind == AttributeKind.TryFunction && !IsCompatibleWithTryFunctionAttribute(b))
		{
			return true;
		}
		return false;
	}

	private static bool IsCompatibleWithTryFunctionAttribute(AttributeSymbol attribute)
	{
		if ((object)attribute != null)
		{
			AttributeTypeInfo attributeInfo = attribute.AttributeInfo;
			if (attributeInfo != null)
			{
				_ = attributeInfo.Category;
				if (0 == 0)
				{
					if (attribute.AttributeInfo.Category == AttributeCategory.FunctionType)
					{
						return attribute.AttributeInfo.Kind == AttributeKind.Normal;
					}
					return true;
				}
			}
		}
		return true;
	}
}
