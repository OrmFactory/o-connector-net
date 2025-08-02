namespace OBridgeConnector.ValueObjects;

public class BinaryValue : ValueObject
{
	private byte[] value = [];

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		var byteCount = await reader.Read7BitEncodedInt(token);
		value = await reader.ReadBytes(byteCount, token);
	}

	public override Guid GetGuid()
	{
		if (value.Length != 16) throw new InvalidCastException();
		return new Guid(value);
	}

	public override string GetString()
	{
		return ToString();
	}

	public override string ToString()
	{
		if (value.Length == 16) return GetGuid().ToString();
		throw new InvalidCastException();
	}

	public override object GetValue()
	{
		return value;
	}

	public override Type GetDefaultType()
	{
		return typeof(byte[]);
	}
}