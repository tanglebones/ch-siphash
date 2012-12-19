using System.Runtime.CompilerServices;

namespace CH.SipHash
{
    public static class SipHash
    {
        // included only to show that casting to long isn't any faster than using
        // the bit-shift function that is inlined.
        internal static unsafe ulong SipHash_2_4_UlongCast(byte[] inba, ulong k0, ulong k1)
        {
            var inlen = inba.Length;

            var v0 = 0x736f6d6570736575 ^ k0;
            var v1 = 0x646f72616e646f6d ^ k1;
            var v2 = 0x6c7967656e657261 ^ k0;
            var v3 = 0x7465646279746573 ^ k1;

            var b = ((ulong)inlen) << 56;

            if (inlen > 0)
                fixed (byte* finb = &inba[0])
                {
                    var inb = finb;
                    var left = inlen & 7;
                    var end = inb + inlen - left;
                    var linb = (ulong*) finb;
                    var lend = (ulong*) end;
                    for (; linb < lend; ++linb)
                    {
                        v3 ^= *linb;

                        SipRound(ref v0, ref v1, ref v2, ref v3);
                        SipRound(ref v0, ref v1, ref v2, ref v3);

                        v0 ^= *linb;
                    }
                    for (var i = 0; i < left; ++i)
                    {
                        b |= ((ulong)end[i]) << (8 * i);
                    }
                }

            v3 ^= b;
            SipRound(ref v0, ref v1, ref v2, ref v3);
            SipRound(ref v0, ref v1, ref v2, ref v3);
            v0 ^= b;
            v2 ^= 0xff;

            SipRound(ref v0, ref v1, ref v2, ref v3);
            SipRound(ref v0, ref v1, ref v2, ref v3);
            SipRound(ref v0, ref v1, ref v2, ref v3);
            SipRound(ref v0, ref v1, ref v2, ref v3);

            return v0 ^ v1 ^ v2 ^ v3;
        }

        public static unsafe ulong SipHash_2_4(byte[] inba, ulong k0, ulong k1)
        {
            var inlen = inba.Length;

            var v0 = 0x736f6d6570736575 ^ k0;
            var v1 = 0x646f72616e646f6d ^ k1;
            var v2 = 0x6c7967656e657261 ^ k0;
            var v3 = 0x7465646279746573 ^ k1;

            var b = ((ulong) inlen) << 56;

            if (inlen > 0)
                fixed (byte* finb = &inba[0])
                {
                    var inb = finb;
                    var left = inlen & 7;
                    var end = inb + inlen - left;
                    for (; inb < end; inb += 8)
                    {
                        var m = U8To64_Le(inb);
                        v3 ^= m;

                        SipRound(ref v0, ref v1, ref v2, ref v3);
                        SipRound(ref v0, ref v1, ref v2, ref v3);

                        v0 ^= m;
                    }
                    for (var i = 0; i < left; ++i)
                    {
                        b |= ((ulong) end[i]) << (8*i);
                    }
                }

            v3 ^= b;
            SipRound(ref v0, ref v1, ref v2, ref v3);
            SipRound(ref v0, ref v1, ref v2, ref v3);
            v0 ^= b;
            v2 ^= 0xff;

            SipRound(ref v0, ref v1, ref v2, ref v3);
            SipRound(ref v0, ref v1, ref v2, ref v3);
            SipRound(ref v0, ref v1, ref v2, ref v3);
            SipRound(ref v0, ref v1, ref v2, ref v3);

            return v0 ^ v1 ^ v2 ^ v3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SipRound(ref ulong v0, ref ulong v1, ref ulong v2, ref ulong v3)
        {
            v0 += v1;
            v1 = Rotl(v1, 13);
            v1 ^= v0;
            v0 = Rotl(v0, 32);

            v2 += v3;
            v3 = Rotl(v3, 16);
            v3 ^= v2;

            v0 += v3;
            v3 = Rotl(v3, 21);
            v3 ^= v0;

            v2 += v1;
            v1 = Rotl(v1, 17);
            v1 ^= v2;
            v2 = Rotl(v2, 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Rotl(ulong x, int b)
        {
            return (x << b) | (x >> (64 - b));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong U8To64_Le(byte* inb)
        {
            return (inb[0] |
                    (((ulong) inb[1]) << 8) |
                    (((ulong) inb[2]) << 16) |
                    (((ulong) inb[3]) << 24) |
                    (((ulong) inb[4]) << 32) |
                    (((ulong) inb[5]) << 40) |
                    (((ulong) inb[6]) << 48) |
                    (((ulong) inb[7]) << 56)
                   );
        }
    }
}