using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class BooleanValuesRecommender : PropertyValuesRecommender
{
	private static readonly ImmutableArray<string> booleanValues = new string[2] { "true", "false" }.AsImmutable();

	protected internal override bool IsExclusive => true;

	internal BooleanValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		Next = new EnumValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (PropertyTypeInfo == null || PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (string.Compare(PropertyTypeInfo.TypeName, NavTypeKind.Boolean.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return PropertyValuesRecommender.GetPropertyValueRecommendationsFromArray(booleanValues);
	}
}
