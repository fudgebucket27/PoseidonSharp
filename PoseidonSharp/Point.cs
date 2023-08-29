using NeinMath;
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
        private static readonly Integer SNARK_SCALAR_FIELD = Integer.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");
        private static readonly Integer ONE = Integer.Parse("1");
        private static readonly Integer JUBJUB_D = Integer.Parse("168696");
        private static readonly Integer JUBJUB_A = Integer.Parse("168700");
        public static (Integer, Integer) Generator()
        {
            (Integer x, Integer y) points = (Integer.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"), Integer.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));
            return points;
        }

        public static (Integer, Integer) Multiply(Integer scalar, (Integer x, Integer y) _points)
        {
            (Integer x, Integer y) p = ((Integer x, Integer y))_points;
            (Integer x, Integer y) a = Infinity();
            int i = 0;
            while (scalar != 0)
            {
                Integer one = Integer.Parse("1");
                Integer result = (scalar & one);
                if (result != 0)
                {
                    a = Add(a, p);
                }
                p = Add(p, p);
                scalar = IntegerFunctions.DivRem(scalar, 2, out scalar);
                i += 1;
            }

            return a;
        }

        public static (Integer x, Integer y) Add((Integer x, Integer y) self, (Integer x, Integer y) other)
        {
            if (self.x == 0 && self.y == 0)
            {
                return other;
            }

            Integer u1v2 = Multiply(self.x, other.y);
            Integer v1u2 = Multiply(self.y, other.x);
            Integer sumUV = Add(u1v2, v1u2);

            Integer du1 = Multiply(JUBJUB_D, self.x);
            Integer du1u2 = Multiply(du1, other.x);
            Integer du1u2v1 = Multiply(du1u2, self.y);
            Integer du1u2v1v2 = Multiply(du1u2v1, other.y);
            Integer denominatorU3 = Add(ONE, du1u2v1v2);

            Integer u3Inverse = IntegerFunctions.ModInv(denominatorU3, SNARK_SCALAR_FIELD);
            Integer u3 = Multiply(sumUV, u3Inverse);

            Integer v1v2 = Multiply(self.y, other.y);
            Integer au1 = Multiply(JUBJUB_A, self.x);
            Integer au1u2 = Multiply(au1, other.x);
            Integer differenceV = Subtract(v1v2, au1u2);

            Integer du1u2v1v2Difference = Subtract(ONE, du1u2v1v2);
            Integer v3Inverse = IntegerFunctions.ModInv(du1u2v1v2Difference, SNARK_SCALAR_FIELD);
            Integer v3 = Multiply(differenceV, v3Inverse);

            return (u3, v3);
        }

        public static (Integer, Integer) Infinity()
        {
            (Integer x, Integer y) points = (Integer.Parse("0"), Integer.Parse("1"));
            return points;
        }

        public static Integer Multiply(Integer self, Integer other)
        {
            return FQ(self * other, SNARK_SCALAR_FIELD);
        }

        public static Integer FQ(Integer n, Integer fieldModulus)
        {
            Integer nReturn = n % fieldModulus;
            if (nReturn.Sgn() == -1)
            {
                nReturn += fieldModulus;
            }
            return nReturn;
        }

        public static Integer Add(Integer self, Integer other)
        {
            return FQ(self + other, SNARK_SCALAR_FIELD);
        }

        public static Integer Subtract(Integer self, Integer other)
        {
            return FQ(self - other, SNARK_SCALAR_FIELD);
        }

        public static Integer ExtendedEuclideanInverse(Integer a, Integer modulus)
        {
            Integer t = 0, newt = 1;
            Integer r = modulus, newr = a;
            while (newr != 0)
            {
                Integer quotient = r / newr;

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

