namespace OBridgeConnector.ValueObjects;

public class BinaryValue : ValueObject
{
	private byte[] value = [];

	public override async Task ReadFromStream(AsyncBinaryReader reader, CancellationToken token)
	{
		var byteCount = await reader.Read7BitEncodedInt(token).ConfigureAwait(false);
		value = await reader.ReadBytes(byteCount, token).ConfigureAwait(false);
	}

	public override void ReadFromBatch(BatchReader reader)
	{
		var byteCount = reader.Read7BitEncodedInt();
		value = reader.ReadBytes(byteCount).ToArray();
	}

	public override long GetBytes(long dataOffset, byte[]? buffer, int bufferOffset, int length)
	{
		if (dataOffset < 0 || dataOffset > value.Length)
			throw new ArgumentOutOfRangeException(nameof(dataOffset));

		if (buffer == null) return value.Length;

		if (bufferOffset < 0 || bufferOffset > buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(bufferOffset));

		if (length < 0 || (bufferOffset + length) > buffer.Length)
			throw new ArgumentOutOfRangeException(nameof(length));

		int available = value.Length - (int)dataOffset;
		if (available <= 0) return 0;

		int toCopy = Math.Min(available, length);
		Array.Copy(value, (int)dataOffset, buffer, bufferOffset, toCopy);

		return toCopy;
	}

	public override byte[] GetBinary()
	{
		return value;
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