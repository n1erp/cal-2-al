using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TimestampLog
{
	private class InnerSequence
	{
		private enum ControlFlags : byte
		{
			Timestamp,
			Offset6,
			Offset32,
			Range
		}

		private const byte SixBitMax = 63;

		private const byte ControlFlagsWidth = 2;

		private const byte ControlFlagsMask = 3;

		private readonly byte[] buffer;

		private int currentIndex;

		private long currentTimestamp = long.MinValue;

		private byte currentRangeCount;

		public InnerSequence(int capacity)
		{
			buffer = new byte[capacity];
		}

		public bool AddTimestamp(long newTimestamp)
		{
			long num = newTimestamp - currentTimestamp;
			if (num == 0L)
			{
				return true;
			}
			if (num < 0 && currentIndex != 0)
			{
				throw new ArgumentOutOfRangeException("newTimestamp", "The new timestamp must be greater than the last timestamp.");
			}
			if (num > uint.MaxValue || currentIndex == 0)
			{
				FlushPendingRange();
				if (!IsCapacitySufficient(9))
				{
					return false;
				}
				WriteByte(0);
				WriteInt64(newTimestamp);
			}
			else if (num > 63)
			{
				FlushPendingRange();
				if (!IsCapacitySufficient(5))
				{
					return false;
				}
				WriteByte(2);
				WriteInt32((int)num);
			}
			else if (num > 1)
			{
				FlushPendingRange();
				if (!IsCapacitySufficient(1))
				{
					return false;
				}
				WriteByte((byte)((num << 2) | 1));
			}
			else if (num == 1)
			{
				if (!IsCapacitySufficient(1))
				{
					return false;
				}
				currentRangeCount++;
				if (currentRangeCount == 63)
				{
					FlushPendingRange();
				}
			}
			currentTimestamp = newTimestamp;
			return true;
		}

		public unsafe ContainsResult ContainsTimestamp(long timestamp)
		{
			int i = 0;
			long num = long.MinValue;
			if (timestamp > currentTimestamp)
			{
				return ContainsResult.DoesNotContain;
			}
			fixed (byte* ptr = &buffer[0])
			{
				for (; i < currentIndex; i++)
				{
					if (num >= timestamp)
					{
						break;
					}
					byte b = buffer[i];
					switch ((ControlFlags)(byte)(b & 3))
					{
					case ControlFlags.Timestamp:
						num = *(long*)(ptr + i + 1);
						if (num == timestamp)
						{
							return ContainsResult.Contains;
						}
						if (i == 0 && timestamp < num)
						{
							return ContainsResult.UnknownTooOld;
						}
						i += 8;
						break;
					case ControlFlags.Offset6:
					{
						byte b3 = (byte)(b >> 2);
						num += b3;
						if (num == timestamp)
						{
							return ContainsResult.Contains;
						}
						break;
					}
					case ControlFlags.Offset32:
					{
						uint num2 = *(uint*)(ptr + i + 1);
						num += num2;
						if (num == timestamp)
						{
							return ContainsResult.Contains;
						}
						i += 4;
						break;
					}
					case ControlFlags.Range:
					{
						byte b2 = (byte)(b >> 2);
						num += b2;
						if (timestamp <= num)
						{
							return ContainsResult.Contains;
						}
						break;
					}
					default:
						throw new NotSupportedException();
					}
				}
			}
			if (currentRangeCount > 0 && currentTimestamp - timestamp <= currentRangeCount)
			{
				return ContainsResult.Contains;
			}
			return ContainsResult.DoesNotContain;
		}

		private void FlushPendingRange()
		{
			if (currentRangeCount > 0)
			{
				WriteByte((byte)((currentRangeCount << 2) | 3));
				currentRangeCount = 0;
			}
		}

		private void WriteByte(byte value)
		{
			buffer[currentIndex++] = value;
		}

		private unsafe void WriteInt64(long value)
		{
			fixed (byte* ptr = &buffer[currentIndex])
			{
				*(long*)ptr = value;
			}
			currentIndex += 8;
		}

		private unsafe void WriteInt32(int value)
		{
			fixed (byte* ptr = &buffer[currentIndex])
			{
				*(int*)ptr = value;
			}
			currentIndex += 4;
		}

		private bool IsCapacitySufficient(int numOfBytes)
		{
			return buffer.Length - currentIndex >= numOfBytes;
		}
	}

	internal enum ContainsResult
	{
		Contains,
		DoesNotContain,
		UnknownTooOld
	}

	private readonly int sequenceSizeInBytes;

	private readonly List<InnerSequence> innerSequences;

	private bool hasBeenCleanedUp;

	public TimestampLog()
		: this(4, 512)
	{
	}

	public TimestampLog(int numberOfSequences, int sequenceSizeInBytes)
	{
		this.sequenceSizeInBytes = sequenceSizeInBytes;
		innerSequences = new List<InnerSequence>(numberOfSequences)
		{
			new InnerSequence(sequenceSizeInBytes)
		};
	}

	public void AddTimestamp(long timestamp)
	{
		InnerSequence innerSequence = innerSequences[innerSequences.Count - 1];
		if (!innerSequence.AddTimestamp(timestamp))
		{
			if (innerSequences.Count == innerSequences.Capacity)
			{
				innerSequences.RemoveAt(0);
				hasBeenCleanedUp = true;
			}
			innerSequence = new InnerSequence(sequenceSizeInBytes);
			innerSequences.Add(innerSequence);
			innerSequence.AddTimestamp(timestamp);
		}
	}

	public ContainsResult ContainsTimestamp(long timestamp)
	{
		for (int i = 0; i < innerSequences.Count; i++)
		{
			InnerSequence innerSequence = innerSequences[i];
			switch (innerSequence.ContainsTimestamp(timestamp))
			{
			case ContainsResult.Contains:
				return ContainsResult.Contains;
			case ContainsResult.UnknownTooOld:
				if (i != 0 || !hasBeenCleanedUp)
				{
					return ContainsResult.DoesNotContain;
				}
				return ContainsResult.UnknownTooOld;
			default:
				throw new NotSupportedException();
			case ContainsResult.DoesNotContain:
				break;
			}
		}
		return ContainsResult.DoesNotContain;
	}
}
