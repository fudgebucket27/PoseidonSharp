using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace PoseidonSharp
{
    public static class Point
    {
        private static BigInteger SNARK_SCALAR_FIELD = BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");
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
            BigInteger one = BigInteger.Parse("1");
            BigInteger jubJubD = BigInteger.Parse("168696");
            BigInteger jubJubA = BigInteger.Parse("168700");
            (BigInteger u1, BigInteger v1) c = (self.x, self.y);
            (BigInteger u2, BigInteger v2) d = (other.x, other.y);

            BigInteger u3Part1 = Multiply(c.u1, d.v2);
            BigInteger u3Part2 = Multiply(c.v1, d.u2);
            BigInteger u3Part3 = Add(u3Part1, u3Part2);
            BigInteger u3Part4 = Multiply(jubJubD, c.u1);
            BigInteger u3Part5 = Multiply(u3Part4, d.u2);
            BigInteger u3Part6 = Multiply(u3Part5, c.v1);
            BigInteger u3Part7 = Multiply(u3Part6, d.v2);
            BigInteger u3Part8 = Add(one, u3Part7);
            BigInteger u3Inverse = ExtendedEuclideanInverse(u3Part8, SNARK_SCALAR_FIELD);
            BigInteger u3Final = Multiply(u3Part3, u3Inverse);


            BigInteger v3Part1 = Multiply(c.v1, d.v2);
            BigInteger v3Part2 = Multiply(jubJubA, c.u1);
            BigInteger v3Part3 = Multiply(v3Part2, d.u2);
            BigInteger v3Part4 = Subtract(v3Part1, v3Part3);

            BigInteger v3Part5 = Multiply(jubJubD, c.u1);
            BigInteger v3Part6 = Multiply(v3Part5, d.u2);
            BigInteger v3Part7 = Multiply(v3Part6, c.v1);
            BigInteger v3Part8 = Multiply(v3Part7, d.v2);
            BigInteger v3Part9 = Subtract(one, v3Part8);

            BigInteger v3Inverse = ExtendedEuclideanInverse(v3Part9, SNARK_SCALAR_FIELD);
            BigInteger v3Final = Multiply(v3Part4, v3Inverse);

            (BigInteger x, BigInteger y) points = (u3Final, v3Final);
            return points;
        }

        public static (BigInteger, BigInteger) Infinity()
        {
            (BigInteger x, BigInteger y) points = (BigInteger.Parse("0"), BigInteger.Parse("1"));
            return points;
        }

        public static BigInteger Multiply(BigInteger self, BigInteger other)
        {
            (BigInteger m, BigInteger n) points = FQ((self * other) % SNARK_SCALAR_FIELD, SNARK_SCALAR_FIELD);
            if (points.n.Sign == -1)
            {
                points.n = points.n + SNARK_SCALAR_FIELD;
            }
            return points.n;
        }

        public static (BigInteger m, BigInteger n) FQ(BigInteger n, BigInteger fieldModulus)
        {
            BigInteger nReturn = n % fieldModulus;
            if (nReturn.Sign == -1)
            {
                nReturn = n + fieldModulus;
            }
            return (fieldModulus, nReturn);
        }

        public static BigInteger Add(BigInteger self, BigInteger other)
        {
            (BigInteger m, BigInteger n) points = FQ((self + other) % SNARK_SCALAR_FIELD, SNARK_SCALAR_FIELD);
            if (points.n.Sign == -1)
            {
                points.n = points.n + SNARK_SCALAR_FIELD;
            }
            return points.n;
        }

        public static BigInteger Subtract(BigInteger self, BigInteger other)
        {
            (BigInteger m, BigInteger n) points = FQ((self - other) % SNARK_SCALAR_FIELD, SNARK_SCALAR_FIELD);
            if (points.n.Sign == -1)
            {
                points.n = points.n + SNARK_SCALAR_FIELD;
            }
            return points.n;
        }

        public static BigInteger Divide(BigInteger self, BigInteger other)
        {
            (BigInteger m, BigInteger n) points = FQ((self * BigInteger.ModPow(other, SNARK_SCALAR_FIELD - 2, SNARK_SCALAR_FIELD)) % SNARK_SCALAR_FIELD, SNARK_SCALAR_FIELD);
            if (points.n.Sign == -1)
            {
                points.n = points.n + SNARK_SCALAR_FIELD;
            }
            return points.n;
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

