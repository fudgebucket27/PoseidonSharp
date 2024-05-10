using NeinMath;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PoseidonSharp
{
    public static class Point
    {
        private static readonly BigInteger SNARK_SCALAR_FIELD = BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");
        private static readonly BigInteger ONE = 1;
        private static readonly BigInteger JUBJUB_D = 168696;
        private static readonly  BigInteger JUBJUB_A = 168700;

        private static readonly (BigInteger x, BigInteger y) GENERATOR = (BigInteger.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"), BigInteger.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));

        public static (BigInteger, BigInteger) Generator() => GENERATOR;

        public static (BigInteger, BigInteger) Multiply(BigInteger scalar, (BigInteger x, BigInteger y) points)
        {
            (BigInteger x, BigInteger y) p = points;
            (BigInteger x, BigInteger y) a = (0, 1);

            while (scalar != 0)
            {
                if ((scalar & 1) != 0)
                {
                    a = Add(a, p);
                }
                p = Add(p, p);
                scalar >>= 1;
            }

            return a;
        }

        public static (BigInteger x, BigInteger y) Add((BigInteger x, BigInteger y) self, (BigInteger x, BigInteger y) other)
        {
            if (self.x == 0 && self.y == 0)
            {
                return other;
            }

            BigInteger u1v2 = FQ(self.x * other.y);
            BigInteger v1u2 = FQ(self.y * other.x);
            BigInteger sumUV = FQ(u1v2 + v1u2);
            BigInteger du1 = FQ(JUBJUB_D * self.x);
            BigInteger du1u2 = FQ(du1 * other.x);
            BigInteger du1u2v1 = FQ(du1u2 * self.y);
            BigInteger du1u2v1v2 = FQ(du1u2v1 * other.y);
            BigInteger denominatorU3 = FQ(ONE + du1u2v1v2);
            BigInteger u3Inverse = ModInv(denominatorU3, SNARK_SCALAR_FIELD);
            BigInteger u3 = FQ(sumUV * u3Inverse);
            BigInteger v1v2 = FQ(self.y * other.y);
            BigInteger au1 = FQ(JUBJUB_A * self.x);
            BigInteger au1u2 = FQ(au1 * other.x);
            BigInteger differenceV = FQ(v1v2 - au1u2);
            BigInteger du1u2v1v2Difference = FQ(ONE - du1u2v1v2);
            BigInteger v3Inverse = ModInv(du1u2v1v2Difference, SNARK_SCALAR_FIELD);
            BigInteger v3 = FQ(differenceV * v3Inverse);

            return (u3, v3);
        }

        public static (BigInteger, BigInteger) Infinity() => (0, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BigInteger FQ(BigInteger n)
        {
            BigInteger nReturn = n % SNARK_SCALAR_FIELD;
            return nReturn.Sign == -1 ? nReturn + SNARK_SCALAR_FIELD : nReturn;
        }
        public static BigInteger ModInv(BigInteger a, BigInteger p)
        {
            return BigInteger.ModPow(a, p - 2, p);
        }

    }
}