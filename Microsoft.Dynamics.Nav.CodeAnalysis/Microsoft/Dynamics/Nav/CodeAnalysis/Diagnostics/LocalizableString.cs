using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public abstract class LocalizableString : IFormattable, IEquatable<LocalizableString>
{
	private sealed class FixedLocalizableString : LocalizableString
	{
		private static readonly FixedLocalizableString Empty = new FixedLocalizableString(string.Empty);

		private readonly string fixedString;

		public static FixedLocalizableString Create(string fixedResource)
		{
			if (string.IsNullOrEmpty(fixedResource))
			{
				return Empty;
			}
			return new FixedLocalizableString(fixedResource);
		}

		private FixedLocalizableString(string fixedResource)
		{
			fixedString = fixedResource;
		}

		protected override string GetText(IFormatProvider formatProvider)
		{
			return fixedString;
		}

		protected override bool AreEqual(object other)
		{
			if (other is FixedLocalizableString fixedLocalizableString)
			{
				return string.Equals(fixedString, fixedLocalizableString.fixedString);
			}
			return false;
		}

		protected override int GetHash()
		{
			if (fixedString == null)
			{
				return 0;
			}
			return fixedString.GetHashCode();
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
	public event EventHandler<Exception> OnException;

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public string ToString(IFormatProvider formatProvider)
	{
		try
		{
			return GetText(formatProvider);
		}
		catch (Exception ex)
		{
			RaiseOnException(ex);
			return string.Empty;
		}
	}

	public static explicit operator string(LocalizableString localizableResource)
	{
		return localizableResource.ToString(null);
	}

	public static implicit operator LocalizableString(string fixedResource)
	{
		return FixedLocalizableString.Create(fixedResource);
	}

	public sealed override string ToString()
	{
		return ToString(null);
	}

	string IFormattable.ToString(string ignored, IFormatProvider formatProvider)
	{
		return ToString(formatProvider);
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public sealed override int GetHashCode()
	{
		try
		{
			return GetHash();
		}
		catch (Exception ex)
		{
			RaiseOnException(ex);
			return 0;
		}
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	public sealed override bool Equals(object obj)
	{
		try
		{
			return AreEqual(obj);
		}
		catch (Exception ex)
		{
			RaiseOnException(ex);
			return false;
		}
	}

	public bool Equals(LocalizableString other)
	{
		return Equals((object)other);
	}

	protected abstract string GetText(IFormatProvider formatProvider);

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	protected abstract int GetHash();

	protected abstract bool AreEqual(object other);

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	private void RaiseOnException(Exception ex)
	{
		if (ex is OperationCanceledException)
		{
			return;
		}
		try
		{
			if (OnException != null)
			{
				OnException(this, ex);
			}
		}
		catch
		{
		}
	}
}
