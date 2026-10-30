using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.MetaModel;

public interface IElementKeyProvider
{
	string CreateElementKeySegmentString(IElement element);

	IEnumerable<IElementKeySegment> GetElementKeySegments(string elementKey);

	string CombineElementKeySegmentStrings(params string[] segments);

	string ElementKeySegmentToElementKey(IElementKeySegment segment);

	IEnumerable<string> SplitElementKeyList(string elementKeys);
}
