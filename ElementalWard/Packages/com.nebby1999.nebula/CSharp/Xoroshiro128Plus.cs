using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nebula
{
	public class Xoroshiro128Plus
	{
		private class Initializer
		{
			public ulong x;

			public ulong Next()
			{
				ulong num = (x += 11400714819323198485uL);
				long num2 = (long)(num ^ (num >> 30)) * -4658895280553007687L;
				long num3 = (long)((ulong)num2 ^ ((ulong)num2 >> 27)) * -7723592293110705685L;
				return (ulong)num3 ^ ((ulong)num3 >> 31);
			}
		}
		private static readonly Initializer initializer = new Initializer();
		private ulong state0;

		private ulong state1;

		private const ulong JUMP0 = 16109378705422636197uL;

		private const ulong JUMP1 = 1659688472399708668uL;

		private static readonly ulong[] JUMP = new ulong[2] { 16109378705422636197uL, 1659688472399708668uL };

		private const ulong LONG_JUMP0 = 15179817016004374139uL;

		private const ulong LONG_JUMP1 = 15987667697637423809uL;

		private static readonly ulong[] LONG_JUMP = new ulong[2] { 15179817016004374139uL, 15987667697637423809uL };

		private static readonly int[] logTable256 = GenerateLogTable();

		public bool nextBool => nextLong < 0;

		public uint nextUint => (uint)Next();

		public int nextInt => (int)Next();

		public ulong nextUlong => Next();

		public long nextLong => (long)Next();

		public double nextNormalizedDouble => ToDouble01Fast(Next());

		public float nextNormalizedFloat => ToFloat01(nextUint);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong RotateLeft(ulong x, int k)
		{
			return (x << k) | (x >> 64 - k);
		}

		public Xoroshiro128Plus(ulong seed)
		{
			ResetSeed(seed);
		}

		public Xoroshiro128Plus(Xoroshiro128Plus other)
		{
			state0 = other.state0;
			state1 = other.state1;
		}

		public void ResetSeed(ulong seed)
		{
			initializer.x = seed;
			state0 = initializer.Next();
			state1 = initializer.Next();
		}

		public ulong Next()
		{
			ulong num = state0;
			ulong num2 = state1;
			ulong result = num + num2;
			num2 ^= num;
			state0 = RotateLeft(num, 24) ^ num2 ^ (num2 << 16);
			state1 = RotateLeft(num2, 37);
			return result;
		}

		public void Jump()
		{
			ulong num = 0uL;
			ulong num2 = 0uL;
			for (int i = 0; i < JUMP.Length; i++)
			{
				for (int j = 0; j < 64; j++)
				{
					if ((JUMP[i] & 1) << j != 0L)
					{
						num ^= state0;
						num2 ^= state1;
					}
					Next();
				}
			}
			state0 = num;
			state1 = num2;
		}

		public void LongJump()
		{
			ulong num = 0uL;
			ulong num2 = 0uL;
			for (int i = 0; i < LONG_JUMP.Length; i++)
			{
				for (int j = 0; j < 64; j++)
				{
					if ((LONG_JUMP[i] & 1) << j != 0L)
					{
						num ^= state0;
						num2 ^= state1;
					}
					Next();
				}
			}
			state0 = num;
			state1 = num2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double ToDouble01Fast(ulong x)
		{
			return BitConverter.Int64BitsToDouble((long)(0x3FF0000000000000L | (x >> 12)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static double ToDouble01(ulong x)
		{
			return (double)(x >> 11) * 1.1102230246251565E-16;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float ToFloat01(uint x)
		{
			return (float)(x >> 8) * 5.96046448E-08f;
		}

		public float RangeFloat(float minInclusive, float maxInclusive)
		{
			return minInclusive + (maxInclusive - minInclusive) * nextNormalizedFloat;
		}

		public int RangeInt(int minInclusive, int maxExclusive)
		{
			return minInclusive + (int)RangeUInt32Uniform((uint)(maxExclusive - minInclusive));
		}

		public long RangeLong(long minInclusive, long maxExclusive)
		{
			return minInclusive + (long)RangeUInt64Uniform((ulong)(maxExclusive - minInclusive));
		}

		private ulong RangeUInt64Uniform(ulong maxExclusive)
		{
			if (maxExclusive == 0L)
			{
				throw new ArgumentOutOfRangeException("Range cannot have size of zero.");
			}
			int num = CalcRequiredBits(maxExclusive);
			int num2 = 64 - num;
			ulong num3;
			do
			{
				num3 = nextUlong >> num2;
			}
			while (num3 >= maxExclusive);
			return num3;
		}

		private uint RangeUInt32Uniform(uint maxExclusive)
		{
			if (maxExclusive == 0)
			{
				throw new ArgumentOutOfRangeException("Range cannot have size of zero.");
			}
			int num = CalcRequiredBits(maxExclusive);
			int num2 = 32 - num;
			uint num3;
			do
			{
				num3 = nextUint >> num2;
			}
			while (num3 >= maxExclusive);
			return num3;
		}

		private static int[] GenerateLogTable()
		{
			int[] array = new int[256];
			array[0] = (array[1] = 0);
			for (int i = 2; i < 256; i++)
			{
				array[i] = 1 + array[i / 2];
			}
			array[0] = -1;
			return array;
		}

		private static int CalcRequiredBits(ulong v)
		{
			int num = 0;
			while (v != 0L)
			{
				v >>= 1;
				num++;
			}
			return num;
		}

		private static int CalcRequiredBits(uint v)
		{
			int num = 0;
			while (v != 0)
			{
				v >>= 1;
				num++;
			}
			return num;
		}

		public ref T NextElementUniform<T>(T[] array)
		{
			return ref array[RangeInt(0, array.Length)];
		}

		public T NextElementUniform<T>(List<T> list)
		{
			return list[RangeInt(0, list.Count)];
		}

		public T NextElementUniform<T>(IList<T> list)
		{
			return list[RangeInt(0, list.Count)];
		}
	}
}