using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal abstract class PropertyValuesRecommender
{
	private ObjectTypeSymbol declaringObject;

	protected internal PropertyValueSyntax PropertyValue { get; private set; }

	protected internal PropertyTypeInfo PropertyTypeInfo { get; private set; }

	protected internal ObjectTypeSymbol DeclaringObject => declaringObject ?? (declaringObject = Context.SemanticModel.GetDeclaredSymbol(Context.DeclaringObject) as ObjectTypeSymbol);

	protected internal MemberSyntaxContext Context { get; }

	protected internal PropertyValuesRecommender Next { get; set; }

	protected internal virtual bool IsExclusive
	{
		get
		{
			if (Next != null)
			{
				return Next.IsExclusive;
			}
			return false;
		}
	}

	protected PropertyValuesRecommender(MemberSyntaxContext context)
	{
		Context = context;
		SetCurrentState();
	}

	protected internal virtual async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (Next == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		cancellationToken.ThrowIfCancellationRequested();
		if (PropertyValue == null || PropertyTypeInfo == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		return await Next.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	protected static IEnumerable<PropertyValueRecommendation> GetPropertyValueRecommendationsFromSymbols(IEnumerable<ISymbol> symbols)
	{
		return symbols.Select((ISymbol s) => new PropertyValueRecommendation
		{
			DisplayText = s.Name
		});
	}

	protected static IEnumerable<PropertyValueRecommendation> GetPropertyValueRecommendationsFromArray(ImmutableArray<string> array)
	{
		return array.SelectAsArray((string x) => new PropertyValueRecommendation
		{
			DisplayText = x
		});
	}

	protected static IEnumerable<PropertyValueRecommendation> GetPropertyValueRecommendationsFromEnumPropertyTypeInfo(EnumPropertyTypeInfo info)
	{
		return info.Options.SelectAsArray((string x) => new PropertyValueRecommendation
		{
			DisplayText = x,
			DetailText = info.GetEnumValuePropertyDetailText(x),
			DescriptionValue = info.GetEnumValuePropertyDocumentation(x)
		});
	}

	private void SetPropertyValueSyntax()
	{
		PropertyValue = Microsoft.Dynamics.Nav.CodeAnalysis.Syntax.SyntaxTokenExtensions.GetAncestor<PropertySyntax>(Context.LeftToken)?.Value;
	}

	protected void SetCurrentState()
	{
		SetPropertyValueSyntax();
		if (PropertyValue != null)
		{
			PropertyTypeInfo = PropertyValue.GetPropertyTypeInfo();
		}
	}
}
