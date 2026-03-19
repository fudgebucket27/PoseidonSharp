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
        private static readonly (Integer x, Integer y) GENERATOR_POINT = (Integer.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"), Integer.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));
        private static readonly (Integer x, Integer y) INFINITY_POINT = (Integer.Parse("0"), Integer.Parse("1"));

        // wNAF window size
        private const int WNAF_W = 4;
        private const int WNAF_HALF_WINDOW = 1 << (WNAF_W - 1); // 8
        private const int WNAF_FULL_WINDOW = 1 << WNAF_W; // 16

        // Precomputed wNAF table for the generator point (avoids recomputation on every sign)
        private static readonly ExtendedPoint[] GeneratorPrecomp = PrecomputeWNAFTable(GENERATOR_POINT);

        private static ExtendedPoint[] PrecomputeWNAFTable((Integer x, Integer y) point)
        {
            ExtendedPoint basePoint = new ExtendedPoint { X = point.x, Y = point.y, T = MulMod(point.x, point.y), Z = ONE };
            ExtendedPoint[] table = new ExtendedPoint[WNAF_HALF_WINDOW];
            table[0] = basePoint;
            ExtendedPoint doubleP = ExtendedDouble(basePoint);
            for (int i = 1; i < WNAF_HALF_WINDOW; i++)
                table[i] = ExtendedAdd(table[i - 1], doubleP);
            return table;
        }

        // Precomputed small Integer constants for wNAF subtraction
        private static readonly Integer[] SmallInts = InitSmallInts();
        private static Integer[] InitSmallInts()
        {
            var arr = new Integer[WNAF_HALF_WINDOW + 1];
            for (int i = 0; i <= WNAF_HALF_WINDOW; i++)
                arr[i] = Integer.Parse(i.ToString());
            return arr;
        }

        // Extended twisted Edwards coordinates (X:Y:T:Z) where x=X/Z, y=Y/Z, T=XY/Z
        // Eliminates modular inversions from intermediate point operations
        private struct ExtendedPoint
        {
            public Integer X, Y, T, Z;
        }

        private static Integer MulMod(Integer a, Integer b)
        {
            return (a * b) % SNARK_SCALAR_FIELD;
        }

        private static Integer AddMod(Integer a, Integer b)
        {
            Integer result = a + b;
            return result >= SNARK_SCALAR_FIELD ? result - SNARK_SCALAR_FIELD : result;
        }

        private static Integer SubMod(Integer a, Integer b)
        {
            return a >= b ? a - b : SNARK_SCALAR_FIELD - b + a;
        }

        // Extended coordinates addition (add-2008-hwcd-4)
        // Cost: 8M + 1D + 8add (no modular inversions)
        private static ExtendedPoint ExtendedAdd(ExtendedPoint p1, ExtendedPoint p2)
        {
            Integer A = MulMod(p1.X, p2.X);
            Integer B = MulMod(p1.Y, p2.Y);
            Integer C = MulMod(JUBJUB_D, MulMod(p1.T, p2.T));
            Integer D = MulMod(p1.Z, p2.Z);
            Integer E = SubMod(MulMod(AddMod(p1.X, p1.Y), AddMod(p2.X, p2.Y)), AddMod(A, B));
            Integer F = SubMod(D, C);
            Integer G = AddMod(D, C);
            Integer H = SubMod(B, MulMod(JUBJUB_A, A));

            return new ExtendedPoint
            {
                X = MulMod(E, F),
                Y = MulMod(G, H),
                T = MulMod(E, H),
                Z = MulMod(F, G)
            };
        }

        // Extended coordinates doubling (dbl-2008-hwcd)
        // Cost: 4M + 4S + 1D + 6add (no modular inversions)
        private static ExtendedPoint ExtendedDouble(ExtendedPoint p1)
        {
            Integer A = MulMod(p1.X, p1.X);
            Integer B = MulMod(p1.Y, p1.Y);
            Integer zSq = MulMod(p1.Z, p1.Z);
            Integer C = AddMod(zSq, zSq);
            Integer D = MulMod(JUBJUB_A, A);
            Integer sum = AddMod(p1.X, p1.Y);
            Integer E = SubMod(SubMod(MulMod(sum, sum), A), B);
            Integer G = AddMod(D, B);
            Integer F = SubMod(G, C);
            Integer H = SubMod(D, B);

            return new ExtendedPoint
            {
                X = MulMod(E, F),
                Y = MulMod(G, H),
                T = MulMod(E, H),
                Z = MulMod(F, G)
            };
        }

        private static (Integer x, Integer y) FromExtended(ExtendedPoint p)
        {
            Integer zInv = IntegerFunctions.ModInv(p.Z, SNARK_SCALAR_FIELD);
            return (MulMod(p.X, zInv), MulMod(p.Y, zInv));
        }

        public static (Integer, Integer) Generator()
        {
            return GENERATOR_POINT;
        }

        private static ExtendedPoint ExtendedNegate(ExtendedPoint p)
        {
            // For twisted Edwards curve, -(x, y) = (-x, y), so in extended: (-X, Y, -T, Z)
            return new ExtendedPoint
            {
                X = p.X == 0 ? p.X : SNARK_SCALAR_FIELD - p.X,
                Y = p.Y,
                T = p.T == 0 ? p.T : SNARK_SCALAR_FIELD - p.T,
                Z = p.Z
            };
        }

        // Precomputed bit masks for wNAF extraction
        private static readonly Integer WNAF_BIT1 = ONE << 1;
        private static readonly Integer WNAF_BIT2 = ONE << 2;
        private static readonly Integer WNAF_BIT3 = ONE << 3;

        public static (Integer, Integer) Multiply(Integer scalar, (Integer x, Integer y) _points)
        {
            if (scalar == 0)
                return INFINITY_POINT;

            // Use cached precomputed table for generator point, compute table for other points
            ExtendedPoint[] precomp;
            if (_points.x == GENERATOR_POINT.x && _points.y == GENERATOR_POINT.y)
            {
                precomp = GeneratorPrecomp;
            }
            else
            {
                precomp = PrecomputeWNAFTable(_points);
            }

            // Compute wNAF representation using Integer arithmetic
            int[] nafDigits = new int[260]; // 256 bits + margin
            int nafLen = 0;
            Integer s = scalar;
            while (s.Sgn() > 0)
            {
                if ((s & ONE) != 0) // s is odd
                {
                    // Extract low w bits using precomputed masks
                    int mods = 0;
                    if ((s & ONE) != 0) mods |= 1;
                    if ((s & WNAF_BIT1) != 0) mods |= 2;
                    if ((s & WNAF_BIT2) != 0) mods |= 4;
                    if ((s & WNAF_BIT3) != 0) mods |= 8;

                    if (mods >= WNAF_HALF_WINDOW)
                    {
                        mods -= WNAF_FULL_WINDOW;
                        s = s + SmallInts[-mods];
                    }
                    else
                    {
                        s = s - SmallInts[mods];
                    }

                    nafDigits[nafLen++] = mods;
                }
                else
                {
                    nafDigits[nafLen++] = 0;
                }
                s = s >> 1;
            }

            // Process wNAF from MSB to LSB
            ExtendedPoint result = default;
            bool resultIsIdentity = true;

            for (int i = nafLen - 1; i >= 0; i--)
            {
                if (!resultIsIdentity)
                    result = ExtendedDouble(result);

                if (nafDigits[i] > 0)
                {
                    int idx = (nafDigits[i] - 1) >> 1;
                    if (resultIsIdentity)
                    {
                        result = precomp[idx];
                        resultIsIdentity = false;
                    }
                    else
                    {
                        result = ExtendedAdd(result, precomp[idx]);
                    }
                }
                else if (nafDigits[i] < 0)
                {
                    int idx = (-nafDigits[i] - 1) >> 1;
                    if (resultIsIdentity)
                    {
                        result = ExtendedNegate(precomp[idx]);
                        resultIsIdentity = false;
                    }
                    else
                    {
                        result = ExtendedAdd(result, ExtendedNegate(precomp[idx]));
                    }
                }
            }

            if (resultIsIdentity)
                return INFINITY_POINT;

            return FromExtended(result);
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
            return INFINITY_POINT;
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

