using OBridgeConnector.OracleTypes;

namespace OBridgeConnector.ValueObjects;

public class IntervalDayToSecondValue : ValueObject
{
	private readonly int precision;
	private OracleIntervalDS interval = new();

	public IntervalDayToSecondValue(int precision)
	{
		this.precision = precision;
	}

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		interval = new();
		var meta = await reader.ReadByte(token);
		var hasDays = (meta & 0x01) != 0;
		var hasHours = (meta & 0x02) != 0;
		var hasMinutes = (meta & 0x04) != 0;
		var hasSeconds = (meta & 0x08) != 0;
		var hasFractionalSeconds = (meta & 0x10) != 0;
		var isNegative = (meta & 0x80) != 0;

		if (hasDays) interval.Days = await reader.Read7BitEncodedInt(token);
		if (hasHours) interval.Hours = await reader.Read7BitEncodedInt(token);
		if (hasMinutes) interval.Minutes = await reader.Read7BitEncodedInt(token);
		if (hasSeconds) interval.Seconds = await reader.Read7BitEncodedInt(token);
		if (hasFractionalSeconds && precision > 0)
		{
			var fractional = await reader.Read7BitEncodedInt(token);
			interval.Nanoseconds = fractional * PowersOf10[9 - precision];
		}

		if (isNegative)
		{
			interval.Days = -interval.Days;
			interval.Hours = -interval.Hours;
			interval.Minutes = -interval.Minutes;
			interval.Seconds = -interval.Seconds;
			interval.Nanoseconds = -interval.Nanoseconds;
		}
	}

	public override OracleIntervalDS GetOracleIntervalDS()
	{
		return interval;
	}

	public override TimeSpan GetTimeSpan()
	{
		return interval.ToTimeSpan();
	}

	public override string GetString()
	{
		return interval.ToString();
	}

	public override object GetValue()
	{
		return interval;
	}

	public override Type GetDefaultType()
	{
		return typeof(OracleIntervalDS);
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