using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class MessageIDExtensions
{
	public static LocalizableErrorArgument Localize(this MessageID id)
	{
		return new LocalizableErrorArgument(id);
	}

	internal static LanguageVersion RequiredVersion(this MessageID feature)
	{
		switch (feature)
		{
		case MessageID.IDS_FeatureAttributesOnBackingFields:
		case MessageID.IDS_FeatureImprovedOverloadCandidates:
		case MessageID.IDS_FeatureRefReassignment:
		case MessageID.IDS_FeatureRefFor:
		case MessageID.IDS_FeatureRefForEach:
		case MessageID.IDS_FeatureEnumGenericTypeConstraint:
		case MessageID.IDS_FeatureDelegateGenericTypeConstraint:
		case MessageID.IDS_FeatureUnmanagedGenericTypeConstraint:
		case MessageID.IDS_FeatureStackAllocInitializer:
		case MessageID.IDS_FeatureTupleEquality:
		case MessageID.IDS_FeatureExpressionVariablesInQueriesAndInitializers:
		case MessageID.IDS_FeatureExtensibleFixedStatement:
		case MessageID.IDS_FeatureIndexingMovableFixedBuffers:
			return LanguageVersion.CSharp7_3;
		case MessageID.IDS_FeatureLeadingDigitSeparator:
		case MessageID.IDS_FeatureNonTrailingNamedArguments:
		case MessageID.IDS_FeatureReadOnlyReferences:
		case MessageID.IDS_FeatureRefStructs:
		case MessageID.IDS_FeatureReadOnlyStructs:
		case MessageID.IDS_FeatureRefExtensionMethods:
		case MessageID.IDS_FeaturePrivateProtected:
		case MessageID.IDS_FeatureRefConditional:
			return LanguageVersion.CSharp7_2;
		case MessageID.IDS_FeatureDefaultLiteral:
		case MessageID.IDS_FeatureInferredTupleNames:
		case MessageID.IDS_FeatureGenericPatternMatching:
		case MessageID.IDS_FeatureAsyncMain:
			return LanguageVersion.CSharp7_1;
		case MessageID.IDS_FeaturePatternMatching:
		case MessageID.IDS_FeatureThrowExpression:
		case MessageID.IDS_FeatureBinaryLiteral:
		case MessageID.IDS_FeatureDigitSeparator:
		case MessageID.IDS_FeatureLocalFunctions:
		case MessageID.IDS_FeatureRefLocalsReturns:
		case MessageID.IDS_FeatureTuples:
		case MessageID.IDS_FeatureOutVar:
		case MessageID.IDS_FeatureExpressionBodiedAccessor:
		case MessageID.IDS_FeatureExpressionBodiedDeOrConstructor:
			return LanguageVersion.CSharp7;
		case MessageID.IDS_FeatureExceptionFilter:
		case MessageID.IDS_FeatureAutoPropertyInitializer:
		case MessageID.IDS_FeatureNullPropagatingOperator:
		case MessageID.IDS_FeatureExpressionBodiedMethod:
		case MessageID.IDS_FeatureExpressionBodiedProperty:
		case MessageID.IDS_FeatureExpressionBodiedIndexer:
		case MessageID.IDS_FeatureNameof:
		case MessageID.IDS_FeatureDictionaryInitializer:
		case MessageID.IDS_FeatureUsingStatic:
		case MessageID.IDS_FeatureInterpolatedStrings:
		case MessageID.IDS_AwaitInCatchAndFinally:
		case MessageID.IDS_FeatureReadonlyAutoImplementedProperties:
			return LanguageVersion.CSharp6;
		case MessageID.IDS_FeatureAsync:
			return LanguageVersion.CSharp5;
		case MessageID.IDS_FeatureDynamic:
		case MessageID.IDS_FeatureTypeVariance:
		case MessageID.IDS_FeatureNamedArgument:
		case MessageID.IDS_FeatureOptionalParameter:
			return LanguageVersion.CSharp4;
		case MessageID.IDS_FeatureImplicitArray:
		case MessageID.IDS_FeatureImplicitLocal:
		case MessageID.IDS_FeatureAnonymousTypes:
		case MessageID.IDS_FeatureAutoImplementedProperties:
		case MessageID.IDS_FeatureObjectInitializer:
		case MessageID.IDS_FeatureCollectionInitializer:
		case MessageID.IDS_FeatureLambda:
		case MessageID.IDS_FeatureQueryExpression:
		case MessageID.IDS_FeatureExtensionMethod:
		case MessageID.IDS_FeaturePartialMethod:
			return LanguageVersion.CSharp3;
		case MessageID.IDS_FeatureGenerics:
		case MessageID.IDS_FeatureAnonDelegates:
		case MessageID.IDS_FeatureGlobalNamespace:
		case MessageID.IDS_FeatureFixedBuffer:
		case MessageID.IDS_FeaturePragma:
		case MessageID.IDS_FeatureStaticClasses:
		case MessageID.IDS_FeaturePartialTypes:
		case MessageID.IDS_FeatureSwitchOnBool:
		case MessageID.IDS_FeaturePropertyAccessorMods:
		case MessageID.IDS_FeatureExternAlias:
		case MessageID.IDS_FeatureIterators:
		case MessageID.IDS_FeatureDefault:
		case MessageID.IDS_FeatureNullable:
			return LanguageVersion.CSharp2;
		case MessageID.IDS_FeatureModuleAttrLoc:
			return LanguageVersion.CSharp1;
		default:
			throw ExceptionUtilities.UnexpectedValue(feature);
		}
	}
}
