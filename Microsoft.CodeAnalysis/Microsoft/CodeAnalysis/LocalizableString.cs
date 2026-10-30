using System;

namespace Microsoft.CodeAnalysis;

public abstract class LocalizableString : IFormattable, IEquatable<LocalizableString>
{
	private sealed class FixedLocalizableString : LocalizableString
	{
		private static readonly FixedLocalizableString s_empty = new FixedLocalizableString(string.Empty);

		private readonly string _fixedString;

		internal override bool CanThrowExceptions => false;

		public static FixedLocalizableString Create(string fixedResource)
		{
			if (string.IsNullOrEmpty(fixedResource))
			{
				return s_empty;
			}
			return new FixedLocalizableString(fixedResource);
		}

		private FixedLocalizableString(string fixedResource)
		{
			_fixedString = fixedResource;
		}

		protected override string GetText(IFormatProvider formatProvider)
		{
			return _fixedString;
		}

		protected override bool AreEqual(object other)
		{
			if (other is FixedLocalizableString fixedLocalizableString)
			{
				return string.Equals(_fixedString, fixedLocalizableString._fixedString);
			}
			return false;
		}

		protected override int GetHash()
		{
			return _fixedString?.GetHashCode() ?? 0;
		}
	}

	internal virtual bool CanThrowExceptions => true;

	public event EventHandler<Exception> OnException;

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

	public sealed override bool Equals(object other)
	{
		try
		{
			return AreEqual(other);
		}
		catch (Exception ex)
		{
			RaiseOnException(ex);
			return false;
		}
	}

	public bool Equals(LocalizableString other)
	{
		return Equals((object?)other);
	}

	protected abstract string GetText(IFormatProvider formatProvider);

	protected abstract int GetHash();

	protected abstract bool AreEqual(object other);

	private void RaiseOnException(Exception ex)
	{
		if (ex is OperationCanceledException)
		{
			return;
		}
		try
		{
			OnException?.Invoke(this, ex);
		}
		catch
		{
		}
	}
}
