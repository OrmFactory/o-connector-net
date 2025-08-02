using System.Globalization;

namespace OBridgeConnector.ValueObjects;

public class FloatValue : ValueObject
{
	private float value;

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		value = await reader.ReadFloat(token);
	}

	public override string GetString()
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	public override string ToString()
	{
		return value.ToString(CultureInfo.InvariantCulture);
	}

	public override object GetValue()
	{
		return value;
	}

	public override Type GetDefaultType()
	{
		return typeof(float);
	}
}