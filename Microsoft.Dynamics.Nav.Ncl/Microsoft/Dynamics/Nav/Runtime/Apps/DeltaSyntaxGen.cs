using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public abstract class DeltaSyntaxGen
{
	protected IDeltaValidator Validator { get; }

	protected DeltaSyntaxGen(IDeltaValidator validator)
	{
		Validator = validator ?? throw new ArgumentNullException("validator");
	}

	public abstract DeltaSyntaxGenResult GenerateSyntaxNodes(ConverterBundle converterBundle, IList<NavXmlBase> deltas);
}
