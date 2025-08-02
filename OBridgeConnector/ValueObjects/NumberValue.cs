namespace OBridgeConnector.ValueObjects;

public class NumberValue : ValueObject
{
	private readonly int? precision;
	private readonly int? scale;
	private bool isFormatA;
	private byte formatAValue;

	private bool isNegative;
	private int valueScale;
	private List<byte> base100Digits = new();

	public NumberValue(int? precision, int? scale)
	{
		this.precision = precision;
		this.scale = scale;
	}

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		byte meta = await reader.ReadByte(token);

		// Format A: (meta & 0x80) != 0
		if ((meta & 0x80) != 0)
		{
			isFormatA = true;
			formatAValue = (byte)(meta & 0x7F);
			return;
		}

		isFormatA = false;
		isNegative = (meta & 0x40) != 0;
		int scaleRaw = meta & 0x3F;

		if (scaleRaw == 63)
		{
			// fallback scale
			valueScale = await reader.ReadByte(token) - 130;
		}
		else
		{
			valueScale = scaleRaw - 32;
		}

		// Read base100 digits until MSB is set
		while (true)
		{
			byte b = await reader.ReadByte(token);
			base100Digits.Add((byte)(b & 0x7F));
			if ((b & 0x80) != 0) break;
		}
	}

	public override decimal GetDecimal()
	{
		if (isFormatA) return formatAValue;

		decimal result = 0;
		foreach (var b in base100Digits)
			result = result * 100 + b;

		result *= Pow10Decimal(valueScale);
		if (isNegative) result = -result;
		return result;
	}

	private static readonly decimal[] Pow10Table = new decimal[]
	{
		1e-28m, 1e-27m, 1e-26m, 1e-25m, 1e-24m, 1e-23m, 1e-22m, 1e-21m,
		1e-20m, 1e-19m, 1e-18m, 1e-17m, 1e-16m, 1e-15m, 1e-14m, 1e-13m,
		1e-12m, 1e-11m, 1e-10m, 1e-9m,  1e-8m,  1e-7m,  1e-6m,  1e-5m,
		1e-4m,  1e-3m,  1e-2m,  1e-1m,
		1m,
		1e1m,   1e2m,   1e3m,   1e4m,   1e5m,   1e6m,   1e7m,   1e8m,
		1e9m,   1e10m,  1e11m,  1e12m,  1e13m,  1e14m,  1e15m,  1e16m,
		1e17m,  1e18m,  1e19m,  1e20m,  1e21m,  1e22m,  1e23m,  1e24m,
		1e25m,  1e26m,  1e27m,  1e28m
	};

	/// <summary>
	/// Returns 10^scale as decimal for -28 ≤ scale ≤ 28.
	/// Throws ArgumentOutOfRangeException if outside this range.
	/// </summary>
	public static decimal Pow10Decimal(int scale)
	{
		const int offset = 28;
		if (scale < -offset || scale > offset)
			throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be between -28 and 28 for decimal");
		return Pow10Table[scale + offset];
	}

	public override string GetString()
	{
		return ToString();
	}

	public override string ToString()
	{
		if (isFormatA) return formatAValue.ToString();
		if (base100Digits.Count == 0) return "0";

		var sb = new System.Text.StringBuilder(base100Digits.Count * 2 + 2);
		if (isNegative) sb.Append('-');
		foreach (var d in base100Digits)
		{
			sb.Append((char)('0' + d / 10));
			sb.Append((char)('0' + d % 10));
		}

		while (sb.Length > 1 && sb[0] == '0')
			sb.Remove(0, 1);

		if (valueScale < 0)
		{
			int pointPos = sb.Length + valueScale;
			if (pointPos <= 0)
			{
				sb.Insert(0, new string('0', -pointPos + 1));
				sb.Insert(1, '.');
			}
			else
			{
				sb.Insert(pointPos, '.');
			}
		}
		else if (valueScale > 0)
		{
			sb.Append(new string('0', valueScale));
		}

		if (sb.ToString().Contains('.'))
		{
			int i = sb.Length - 1;
			while (i >= 0 && sb[i] == '0') i--;
			if (i >= 0 && sb[i] == '.') i--;
			sb.Length = i + 1;
		}

		if (isNegative) sb.Insert(0, '-');

		return sb.ToString();
	}

	public override object GetValue()
	{
		return GetDecimal();
	}

	public override Type GetDefaultType()
	{
		return typeof(decimal);
	}
}