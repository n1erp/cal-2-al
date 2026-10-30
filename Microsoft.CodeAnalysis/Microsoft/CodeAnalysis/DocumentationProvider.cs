using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.CodeAnalysis;

public abstract class DocumentationProvider
{
	private class NullDocumentationProvider : DocumentationProvider
	{
		protected internal override string GetDocumentationForSymbol(string documentationMemberID, CultureInfo preferredCulture, CancellationToken cancellationToken = default(CancellationToken))
		{
			return "";
		}

		public override bool Equals(object obj)
		{
			return this == obj;
		}

		public override int GetHashCode()
		{
			return RuntimeHelpers.GetHashCode(this);
		}
	}

	public static DocumentationProvider Default { get; } = new NullDocumentationProvider();

	protected internal abstract string GetDocumentationForSymbol(string documentationMemberID, CultureInfo preferredCulture, CancellationToken cancellationToken = default(CancellationToken));

	public abstract override bool Equals(object obj);

	public abstract override int GetHashCode();
}
