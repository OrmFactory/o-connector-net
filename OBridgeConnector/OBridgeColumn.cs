using System.Data.Common;

namespace OBridgeConnector;

public class OBridgeColumn : DbColumn
{
	public static async Task<OBridgeColumn> FromReader(int columnOrdinal, AsyncBinaryReader reader, CancellationToken token)
	{

	}


}

public abstract class ValueObject
{
	public abstract Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token);

	public virtual bool GetBoolean() => throw new NotSupportedException();
	public virtual byte GetByte() => throw new NotSupportedException();
	public virtual char GetChar() => throw new NotSupportedException();
	public virtual long GetBytes(long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
	public virtual long GetChars(long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotSupportedException();
	public virtual DateTime GetDateTime() => throw new NotSupportedException();
	public virtual decimal GetDecimal() => throw new NotSupportedException();
	public virtual double GetDouble() => throw new NotSupportedException();
	public virtual float GetFloat() => throw new NotSupportedException();
	public virtual Guid GetGuid() => throw new NotSupportedException();
	public virtual short GetInt16() => throw new NotSupportedException();
	public virtual int GetInt32() => throw new NotSupportedException();
	public virtual long GetInt64() => throw new NotSupportedException();

	public abstract string GetString();
}

public class DateTimeValue : ValueObject
{
	private readonly int precision;

	private int year;
	private int month;
	private int day;
	private int hour;
	private int minute;
	private int second;
	private int nanosecond;
	private int timeZoneOffsetMinutes;

	public DateTimeValue(int precision)
	{
		this.precision = precision;
	}

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		uint low = await reader.ReadUInt32(token);

		bool isDateOnly = (low & (1U << 0)) != 0;
		bool hasFraction = (low & (1U << 1)) != 0;
		bool hasTimezone = (low & (1U << 2)) != 0;

		// Year: sign + 14 bits
		uint yearBits = (low >> 7) & 0x7FFF;
		year = (short)(yearBits & 0x3FFF);
		if ((yearBits & (1U << 14)) != 0) year = -year;

		month = (byte)((low >> 22) & 0xF);
		day = (byte)((low >> 26) & 0x1F);

		if (isDateOnly) return;

		ushort high = await reader.ReadUInt16(token);
		ulong full = ((ulong)high << 32) | low;

		hour = (byte)((full >> 31) & 0x1F);
		minute = (byte)((full >> 36) & 0x3F);
		second = (byte)((full >> 42) & 0x3F);

		nanosecond = 0;
		if (hasFraction && precision > 0)
		{
			int scaled = 0;
			if (precision <= 2)
			{
				scaled = await reader.ReadByte(token);
			}
			else if (precision <= 4)
			{
				scaled = await reader.ReadUInt16(token);
			}
			else if (precision <= 6)
			{
				scaled = (await reader.ReadByte(token) << 16)
				         | (await reader.ReadByte(token) << 8)
				         | await reader.ReadByte(token);
			}
			else
			{
				scaled = (int)await reader.ReadUInt32(token);
			}

			int scale = 9 - precision;
			nanosecond = scaled * PowersOf10[scale];
		}

		if (hasTimezone)
		{
			timeZoneOffsetMinutes = await reader.ReadInt16(token);
		}
	}

	private static readonly int[] PowersOf10 = new int[]
	{
		1,              // 10^0
		10,             // 10^1
		100,            // 10^2
		1_000,          // 10^3
		10_000,         // 10^4
		100_000,        // 10^5
		1_000_000,      // 10^6
		10_000_000,     // 10^7
		100_000_000,    // 10^8
		1_000_000_000   // 10^9
	};
}