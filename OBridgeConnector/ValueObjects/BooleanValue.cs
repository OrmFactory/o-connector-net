namespace OBridgeConnector.ValueObjects;

public class BooleanValue : ValueObject
{
	private bool value;

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		value = await reader.ReadByte(token) > 0;
	}

	public override string GetString()
	{
		return value.ToString();
	}

	public override string ToString()
	{
		return value.ToString();
	}

	public override object GetValue()
	{
		return value;
	}

	public override Type GetDefaultType()
	{
		return typeof(bool);
	}
}