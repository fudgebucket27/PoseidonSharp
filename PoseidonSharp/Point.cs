using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace PoseidonSharp
{
    public static class Point
    {
        private static readonly BigInteger SNARK_SCALAR_FIELD = BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");
        private static readonly BigInteger ONE = BigInteger.One;
        private static readonly BigInteger JUBJUB_D = BigInteger.Parse("168696");
        private static readonly BigInteger JUBJUB_A = BigInteger.Parse("168700");
        public static (BigInteger, BigInteger) Generator()
        {
            (BigInteger x, BigInteger y) points = (BigInteger.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"), BigInteger.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));
            return points;
        }

        public static (BigInteger, BigInteger) Multiply(BigInteger scalar, (BigInteger x, BigInteger y) _points)
        {
            (BigInteger x, BigInteger y) p = ((BigInteger x, BigInteger y))_points;
            (BigInteger x, BigInteger y) a = Infinity();
            int i = 0;
            while (scalar != 0)
            {
                BigInteger one = BigInteger.Parse("1");
                BigInteger result = (scalar & one);
                if (result != 0)
                {
                    a = Add(a, p);
                }
                p = Add(p, p);
                scalar = BigInteger.DivRem(scalar, 2, out scalar);
                i += 1;
            }

            return a;
        }

        public static (BigInteger x, BigInteger y) Add((BigInteger x, BigInteger y) self, (BigInteger x, BigInteger y) other)
        {
            if (self.x == 0 && self.y == 0)
            {
                return other;
            }

            BigInteger u1v2 = Multiply(self.x, other.y);
            BigInteger v1u2 = Multiply(self.y, other.x);
            BigInteger sumUV = Add(u1v2, v1u2);

            BigInteger du1 = Multiply(JUBJUB_D, self.x);
            BigInteger du1u2 = Multiply(du1, other.x);
            BigInteger du1u2v1 = Multiply(du1u2, self.y);
            BigInteger du1u2v1v2 = Multiply(du1u2v1, other.y);
            BigInteger denominatorU3 = Add(ONE, du1u2v1v2);

            BigInteger u3Inverse = ExtendedEuclideanInverse(denominatorU3, SNARK_SCALAR_FIELD);
            BigInteger u3 = Multiply(sumUV, u3Inverse);

            BigInteger v1v2 = Multiply(self.y, other.y);
            BigInteger au1 = Multiply(JUBJUB_A, self.x);
            BigInteger au1u2 = Multiply(au1, other.x);
            BigInteger differenceV = Subtract(v1v2, au1u2);

            BigInteger du1u2v1v2Difference = Subtract(ONE, du1u2v1v2);
            BigInteger v3Inverse = ExtendedEuclideanInverse(du1u2v1v2Difference, SNARK_SCALAR_FIELD);
            BigInteger v3 = Multiply(differenceV, v3Inverse);

            return (u3, v3);
        }

        public static (BigInteger, BigInteger) Infinity()
        {
            (BigInteger x, BigInteger y) points = (BigInteger.Parse("0"), BigInteger.Parse("1"));
            return points;
        }

        public static BigInteger Multiply(BigInteger self, BigInteger other)
        {
            return FQ(self * other, SNARK_SCALAR_FIELD);
        }

        public static BigInteger FQ(BigInteger n, BigInteger fieldModulus)
        {
            BigInteger nReturn = n % fieldModulus;
            if (nReturn.Sign == -1)
            {
                nReturn += fieldModulus;
            }
            return nReturn;
        }

        public static BigInteger Add(BigInteger self, BigInteger other)
        {
            return FQ(self + other, SNARK_SCALAR_FIELD);
        }

        public static BigInteger Subtract(BigInteger self, BigInteger other)
        {
            return FQ(self - other, SNARK_SCALAR_FIELD);
        }

        public static BigInteger ExtendedEuclideanInverse(BigInteger a, BigInteger modulus)
        {
            BigInteger t = 0, newt = 1;
            BigInteger r = modulus, newr = a;
            while (newr != 0)
            {
                BigInteger quotient = r / newr;

                (t, newt) = (newt, t - quotient * newt);
                (r, newr) = (newr, r - quotient * newr);
            }

            if (r > 1)
                throw new InvalidOperationException("a is not invertible");
            if (t < 0)
                t = t + modulus;

            return t;
        }

    }
}

