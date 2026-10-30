using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.MetaModel;

internal delegate bool ContinueDifferencePredicate<T>(int originalIndex, IList<T> originalSequence, int longestMatchSoFar);
