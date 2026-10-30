using System;
using System.Collections.Immutable;
using System.Threading;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class CustomAttributesBag<T> where T : AttributeData
{
	[Flags]
	internal enum CustomAttributeBagCompletionPart : byte
	{
		None = 0,
		EarlyDecodedWellKnownAttributeData = 1,
		DecodedWellKnownAttributeData = 2,
		Attributes = 4,
		All = EarlyDecodedWellKnownAttributeData | DecodedWellKnownAttributeData | Attributes
	}

	private ImmutableArray<T> _customAttributes;

	private WellKnownAttributeData _decodedWellKnownAttributeData;

	private EarlyWellKnownAttributeData _earlyDecodedWellKnownAttributeData;

	private int _state;

	public static readonly CustomAttributesBag<T> Empty = new CustomAttributesBag<T>(CustomAttributeBagCompletionPart.All, ImmutableArray<T>.Empty);

	public bool IsEmpty
	{
		get
		{
			if (IsSealed && _customAttributes.IsEmpty && _decodedWellKnownAttributeData == null)
			{
				return _earlyDecodedWellKnownAttributeData == null;
			}
			return false;
		}
	}

	public ImmutableArray<T> Attributes => _customAttributes;

	public WellKnownAttributeData DecodedWellKnownAttributeData => _decodedWellKnownAttributeData;

	public EarlyWellKnownAttributeData EarlyDecodedWellKnownAttributeData => _earlyDecodedWellKnownAttributeData;

	private CustomAttributeBagCompletionPart State
	{
		get
		{
			return (CustomAttributeBagCompletionPart)_state;
		}
		set
		{
			_state = (int)value;
		}
	}

	internal bool IsSealed => IsPartComplete(CustomAttributeBagCompletionPart.All);

	internal bool IsEarlyDecodedWellKnownAttributeDataComputed => IsPartComplete(CustomAttributeBagCompletionPart.EarlyDecodedWellKnownAttributeData);

	internal bool IsDecodedWellKnownAttributeDataComputed => IsPartComplete(CustomAttributeBagCompletionPart.DecodedWellKnownAttributeData);

	private CustomAttributesBag(CustomAttributeBagCompletionPart part, ImmutableArray<T> customAttributes)
	{
		_customAttributes = customAttributes;
		NotePartComplete(part);
	}

	public CustomAttributesBag()
		: this(CustomAttributeBagCompletionPart.None, default(ImmutableArray<T>))
	{
	}

	public static CustomAttributesBag<T> WithEmptyData()
	{
		return new CustomAttributesBag<T>(CustomAttributeBagCompletionPart.EarlyDecodedWellKnownAttributeData | CustomAttributeBagCompletionPart.DecodedWellKnownAttributeData, default);
	}

	public bool SetEarlyDecodedWellKnownAttributeData(EarlyWellKnownAttributeData data)
	{
		bool result = Interlocked.CompareExchange(ref _earlyDecodedWellKnownAttributeData, data, null) == null;
		NotePartComplete(CustomAttributeBagCompletionPart.EarlyDecodedWellKnownAttributeData);
		return result;
	}

	public bool SetDecodedWellKnownAttributeData(WellKnownAttributeData data)
	{
		bool result = Interlocked.CompareExchange(ref _decodedWellKnownAttributeData, data, null) == null;
		NotePartComplete(CustomAttributeBagCompletionPart.DecodedWellKnownAttributeData);
		return result;
	}

	public bool SetAttributes(ImmutableArray<T> newCustomAttributes)
	{
		bool result = ImmutableInterlocked.InterlockedCompareExchange(ref _customAttributes, newCustomAttributes, default) == default(ImmutableArray<T>);
		NotePartComplete(CustomAttributeBagCompletionPart.Attributes);
		return result;
	}

	private void NotePartComplete(CustomAttributeBagCompletionPart part)
	{
		ThreadSafeFlagOperations.Set(ref _state, (int)(State | part));
	}

	internal bool IsPartComplete(CustomAttributeBagCompletionPart part)
	{
		return (State & part) == part;
	}
}
