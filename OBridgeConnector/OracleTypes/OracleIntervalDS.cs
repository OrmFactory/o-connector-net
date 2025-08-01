namespace OBridgeConnector.OracleTypes;

public class OracleIntervalDS
{
	public int Days;
	public int Hours;
	public int Minutes;
	public int Seconds;
	public int Nanoseconds;

	public override string ToString()
	{
		bool isNegative = Days < 0 || Hours < 0 || Minutes < 0 || Seconds < 0 || Nanoseconds < 0;

		int absDays = Math.Abs(Days);
		int absHours = Math.Abs(Hours);
		int absMinutes = Math.Abs(Minutes);
		int absSeconds = Math.Abs(Seconds);
		int absNanos = Math.Abs(Nanoseconds);

		var sign = isNegative ? "-" : "";

		var fractional = absNanos == 0 ? "" :
			"." + absNanos.ToString("D9").TrimEnd('0');

		return $"{sign}{absDays} {absHours:D2}:{absMinutes:D2}:{absSeconds:D2}{fractional}";
	}

	public TimeSpan ToTimeSpan()
	{
		try
		{
			var seconds = (long)Days * 24 * 60 * 60 +
			              Hours * 60 * 60 +
			              Minutes * 60 +
			              Seconds;
			var totalTicks = seconds * TimeSpan.TicksPerSecond + Nanoseconds / 100;
			return new TimeSpan(totalTicks);
		}
		catch (OverflowException)
		{
			throw new InvalidOperationException("OracleIntervalDS is too large to convert to TimeSpan.");
		}
	}
}