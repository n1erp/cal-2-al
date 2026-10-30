using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class EnumValuesRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal EnumValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		Next = new ImageValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (PropertyTypeInfo == null || PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (!(PropertyTypeInfo is EnumPropertyTypeInfo info))
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return PropertyValuesRecommender.GetPropertyValueRecommendationsFromEnumPropertyTypeInfo(info);
	}
}
