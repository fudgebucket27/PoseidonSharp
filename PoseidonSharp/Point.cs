
using System;
using System.Numerics;

namespace PoseidonSharp
{
    public static class Point
    {
        private static readonly BigInteger SNARK_SCALAR_FIELD = BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");
        private static readonly BigInteger ONE = BigInteger.One;
        private static readonly BigInteger TWO = 2;
        private static readonly BigInteger JUBJUB_D = BigInteger.Parse("168696");
        private static readonly BigInteger JUBJUB_A = BigInteger.Parse("168700");

        public static (BigInteger, BigInteger) Generator()
        {
            return (BigInteger.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"),
                    BigInteger.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));
        }

        public static (BigInteger, BigInteger) Multiply(BigInteger scalar, (BigInteger x, BigInteger y) points)
        {
            (BigInteger x, BigInteger y) p = points;
            (BigInteger x, BigInteger y) result = (0, 1);

            while (scalar != 0)
            {
                if ((scalar & ONE) != 0)
                {
                    result = Add(result, p);
                }
                p = Add(p, p);
                scalar >>= 1;
            }

            return result;
        }

        public static (BigInteger x, BigInteger y) Add((BigInteger x, BigInteger y) self, (BigInteger x, BigInteger y) other)
        {
            if (self.x == 0 && self.y == 0)
            {
                return other;
            }
            if (other.x == 0 && other.y == 0)
            {
                return self;
            }

            BigInteger u1 = ModMul(self.x, self.y, SNARK_SCALAR_FIELD);
            BigInteger u2 = ModMul(other.x, other.y, SNARK_SCALAR_FIELD);
            BigInteger v1 = ModMul(self.x, other.x, SNARK_SCALAR_FIELD);
            BigInteger v2 = ModMul(self.y, other.y, SNARK_SCALAR_FIELD);

            BigInteger u = ModMul(u1 + u2, ModInv(TWO + JUBJUB_D * v1 * v2, SNARK_SCALAR_FIELD), SNARK_SCALAR_FIELD);
            BigInteger v = ModMul(v1 + v2, ModInv(TWO - JUBJUB_A * u1 * u2, SNARK_SCALAR_FIELD), SNARK_SCALAR_FIELD);

            return (u, v);
        }

        public static (BigInteger x, BigInteger y) Infinity()
        {
            return (0, 1);
        }

        public static BigInteger Add(BigInteger self, BigInteger other)
        {
            return ModAdd(self, other, SNARK_SCALAR_FIELD);
        }

        public static BigInteger Subtract(BigInteger self, BigInteger other)
        {
            return ModSub(self, other, SNARK_SCALAR_FIELD);
        }

        public static BigInteger Multiply(BigInteger self, BigInteger other)
        {
            return ModMul(self, other, SNARK_SCALAR_FIELD);
        }

        public static BigInteger Divide(BigInteger self, BigInteger other)
        {
            return ModMul(self, ModInv(other, SNARK_SCALAR_FIELD), SNARK_SCALAR_FIELD);
        }

        private static BigInteger Mod(BigInteger value, BigInteger modulus)
        {
            BigInteger result = value % modulus;
            return result < 0 ? result + modulus : result;
        }

        private static BigInteger ModAdd(BigInteger a, BigInteger b, BigInteger modulus)
        {
            return Mod(a + b, modulus);
        }

        private static BigInteger ModSub(BigInteger a, BigInteger b, BigInteger modulus)
        {
            return Mod(a - b, modulus);
        }

        private static BigInteger ModMul(BigInteger a, BigInteger b, BigInteger modulus)
        {
            return Mod(a * b, modulus);
        }

        private static BigInteger ModInv(BigInteger value, BigInteger modulus)
        {
            return BigInteger.ModPow(value, modulus - TWO, modulus);
        }
    }
}
