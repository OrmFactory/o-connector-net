using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBridgeConnector;

public class AsyncBitReader
{
	private readonly AsyncBinaryReader reader;
	private int currentByte = 0;
	private int bitPosition = 8;

	public AsyncBitReader(AsyncBinaryReader reader)
	{
		this.reader = reader;
	}

	public async ValueTask<bool> ReadBit(CancellationToken cancellationToken)
	{
		if (bitPosition == 8)
		{
			currentByte = await reader.ReadByte(cancellationToken);
			bitPosition = 0;
		}

		bool bit = (currentByte & (1 << (7 - bitPosition))) != 0;
		bitPosition++;
		return bit;
	}

	public async ValueTask<int> ReadBits(int count, CancellationToken cancellationToken)
	{
		int result = 0;

		while (count > 0)
		{
			if (bitPosition == 8)
			{
				currentByte = await reader.ReadByte(cancellationToken);
				bitPosition = 0;
			}

			int bitsAvailable = 8 - bitPosition;
			int bitsToRead = Math.Min(count, bitsAvailable);

			int shift = bitsAvailable - bitsToRead;
			int mask = (1 << bitsToRead) - 1;

			int bits = (currentByte >> shift) & mask;

			result = (result << bitsToRead) | bits;

			bitPosition += bitsToRead;
			count -= bitsToRead;
		}

		return result;
	}

	public async ValueTask<int> ReadSignedBits(int bitCount, CancellationToken cancellationToken)
	{
		var isNegative = await ReadBit(cancellationToken);
		int abs = await ReadBits(bitCount - 1, cancellationToken);

		return isNegative ? -abs : abs;
	}
}