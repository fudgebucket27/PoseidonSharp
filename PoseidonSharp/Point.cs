using NeinMath;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace PoseidonSharp
{
    public static class Point
    {
        private static readonly Integer SNARK_SCALAR_FIELD = Integer.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");
        private static readonly Integer ONE = 1;
        private static readonly Integer JUBJUB_D = 168696;
        private static readonly  Integer JUBJUB_A = 168700;

        private static readonly (Integer x, Integer y) GENERATOR = (Integer.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"), Integer.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));

        public static (Integer, Integer) Generator() => GENERATOR;

        public static (Integer, Integer) Multiply(Integer scalar, (Integer x, Integer y) points)
        {
            (Integer x, Integer y) p = points;
            (Integer x, Integer y) a = (0, 1);

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

        public static (Integer x, Integer y) Add((Integer x, Integer y) self, (Integer x, Integer y) other)
        {
            if (self.x == 0 && self.y == 0)
            {
                return other;
            }

            Integer u1v2 = FQ(self.x * other.y);
            Integer v1u2 = FQ(self.y * other.x);
            Integer sumUV = FQ(u1v2 + v1u2);
            Integer du1 = FQ(JUBJUB_D * self.x);
            Integer du1u2 = FQ(du1 * other.x);
            Integer du1u2v1 = FQ(du1u2 * self.y);
            Integer du1u2v1v2 = FQ(du1u2v1 * other.y);
            Integer denominatorU3 = FQ(ONE + du1u2v1v2);
            Integer u3Inverse = IntegerFunctions.ModInv(denominatorU3, SNARK_SCALAR_FIELD);
            Integer u3 = FQ(sumUV * u3Inverse);
            Integer v1v2 = FQ(self.y * other.y);
            Integer au1 = FQ(JUBJUB_A * self.x);
            Integer au1u2 = FQ(au1 * other.x);
            Integer differenceV = FQ(v1v2 - au1u2);
            Integer du1u2v1v2Difference = FQ(ONE - du1u2v1v2);
            Integer v3Inverse = IntegerFunctions.ModInv(du1u2v1v2Difference, SNARK_SCALAR_FIELD);
            Integer v3 = FQ(differenceV * v3Inverse);

            return (u3, v3);
        }

        public static (Integer, Integer) Infinity() => (0, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Integer FQ(Integer n)
        {
            Integer nReturn = n % SNARK_SCALAR_FIELD;
            return nReturn.Sgn() == -1 ? nReturn + SNARK_SCALAR_FIELD : nReturn;
        }
    }
}