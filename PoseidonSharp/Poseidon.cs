using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blake2Fast;

namespace PoseidonSharp
{

    public struct BigIntegerVector
    {
        private BigInteger[] _elements;

        public BigIntegerVector(List<BigInteger> values, int index)
        {
            _elements = new BigInteger[Vector<long>.Count];
            for (int i = 0; i < Vector<long>.Count; i++)
            {
                _elements[i] = values[index + i];
            }
        }

        public static BigInteger Dot(BigIntegerVector v1, BigIntegerVector v2)
        {
            BigInteger result = BigInteger.Zero;
            for (int i = 0; i < Vector<long>.Count; i++)
            {
                result += v1._elements[i] * v2._elements[i];
            }
            return result;
        }
    }


    public class Poseidon
    {
        private BigInteger SNARK_SCALAR_FIELD = BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");
        private BigInteger FR_ORDER = BigInteger.Parse("21888242871839275222246405745257275088614511777268538073601725287587578984328");
        private int T { get; set; }
        private int NRoundsF { get; set; }
        private int NRoundsP { get; set; }
        private string Seed { get; set; }
        private int E { get; set; }

        private List<BigInteger> ConstantsC { get; set; }
        private List<List<BigInteger>> ConstantsM { get; set; }
        private int SecurityTarget { get; set; }

        private BigInteger ReducedExponent { get; set; }

        public Poseidon(int _t, int _nRoundsF, int _nRoundsP, string _seed, int _e, List<BigInteger> _constantsC = null, List<BigInteger> _constantsM = null, int _securityTarget = 0)
        {
            Debug.Assert(_nRoundsF % 2 == 0 && _nRoundsF > 0, "_nRoundsF needs to have modulus 2 of 0 and be more than 0");
            Debug.Assert(_nRoundsP > 0, "_nRoundsP needs to be more than 0");

            double n = Math.Floor(Math.Log((double)SNARK_SCALAR_FIELD,2));
            double M;
            if (_securityTarget == 0)
            {
                M = n; //Security Taget in bit
            }
            else
            {
                M = _securityTarget;
            }
            Debug.Assert(n >= M, "n needs to be more than or equal to M");

            double N = n * _t;

            double grobnerAttackRatioRounds;
            double grobnerAttackRatioSBoxes;
            double interpolationAttackRatio;

            if (SNARK_SCALAR_FIELD % 2 == 3)
            {
                Debug.Assert(_e == 3, "_e needs to be equal to 3");
                grobnerAttackRatioRounds = 0.32;
                grobnerAttackRatioSBoxes = 0.18;
                interpolationAttackRatio = 0.63;
            }
            else if (SNARK_SCALAR_FIELD % 5 != 1)
            {
                Debug.Assert(_e == 5, "_e needs to be equal to 5");
                grobnerAttackRatioRounds = 0.21;
                grobnerAttackRatioSBoxes = 0.14;
                interpolationAttackRatio = 0.43;
            }
            else
            {
                throw new ArgumentException("Invalid SNARK_SCALAR_FIELD for congruency");
            }

            Debug.Assert((_nRoundsF + _nRoundsP) > ((interpolationAttackRatio * Math.Min(n, M)) + Math.Log(_t,2)), "(nRoundsF + nRoundsP) > ((2 + min(M, n)) * grobner_attack_ratio_rounds)");
            Debug.Assert((_nRoundsF + _nRoundsP) > ((2 + Math.Min(M, n)) * grobnerAttackRatioRounds), "(nRoundsF + nRoundsP) > ((2 + min(M, n)) * grobner_attack_ratio_rounds)");
            Debug.Assert((_nRoundsF + (_t * _nRoundsP)) > (M * grobnerAttackRatioSBoxes), "(nRoundsF + (t * nRoundsP)) > (M * grobner_attack_ratio_sboxes)");

    
            if (_constantsC == null)
            {
                string constantsCseed = _seed + "_constants";
                byte[] constantsCseedBytes = Encoding.ASCII.GetBytes(constantsCseed);
                if (_nRoundsF + _nRoundsP == 58)
                {
                    ConstantsC = ConstantsHelper.ConstantsC58;
                }
                else
                {
                    ConstantsC = ConstantsHelper.ConstantsC59;
                }
            }
      

            if (_constantsM == null)
            {
                string constantsMseed = _seed + "_matrix_0000";
                byte[] constantsMseedBytes = Encoding.ASCII.GetBytes(constantsMseed);
                ConstantsM = CalculatePoseidonMatrix(SNARK_SCALAR_FIELD, constantsMseedBytes, _t);
            }

            int nConstraints = (_nRoundsF * _t) + _nRoundsP;
            if (_e == 5)
            {
                nConstraints *= 3;
            }
            else if (_e == 3)
            {
                nConstraints *= 2;
            }

            T = _t;
            NRoundsF = _nRoundsF;
            NRoundsP = _nRoundsP;
            Seed = _seed;
            E = _e;
            ReducedExponent = E % (SNARK_SCALAR_FIELD - 1);
        }

        private List<List<BigInteger>> CalculatePoseidonMatrix(BigInteger p, byte[] seed, int t)
        {
            List<BigInteger> constants = null;
            switch(t * 2)
            {
                case 4:
                    constants = ConstantsHelper.ConstantsC4;
                    break;
                case 6:
                    constants = ConstantsHelper.ConstantsC6;
                    break;
                case 8:
                    constants = ConstantsHelper.ConstantsC8;
                    break;
                case 10:
                    constants = ConstantsHelper.ConstantsC10;
                    break;
                case 12:
                    constants = ConstantsHelper.ConstantsC12;
                    break;
                case 14:
                    constants = ConstantsHelper.ConstantsC14;
                    break;
                case 16:
                    constants = ConstantsHelper.ConstantsC16;
                    break;
                case 18:
                    constants = ConstantsHelper.ConstantsC18;
                    break;
                case 20:
                    constants = ConstantsHelper.ConstantsC20;
                    break;
                case 22:
                    constants = ConstantsHelper.ConstantsC22;
                    break;
                case 24:
                    constants = ConstantsHelper.ConstantsC24;
                    break;
                case 26:
                    constants = ConstantsHelper.ConstantsC26;
                    break;
                case 28:
                    constants = ConstantsHelper.ConstantsC28;
                    break;
            }
            //StringBuilder sb = new StringBuilder();
            List<List<BigInteger>> poseidonMatrix = null;
            switch (t)
            {
                case 2:
                    poseidonMatrix = ConstantsHelper.ConstantsM2;
                    break;
                case 3:
                    poseidonMatrix = ConstantsHelper.ConstantsM3;
                    break;
                case 4:
                    poseidonMatrix = ConstantsHelper.ConstantsM4;
                    break;
                case 5:
                    poseidonMatrix = ConstantsHelper.ConstantsM5;
                    break;
                case 6:
                    poseidonMatrix = ConstantsHelper.ConstantsM6;
                    break;
                case 7:
                    poseidonMatrix = ConstantsHelper.ConstantsM7;
                    break;
                case 8:
                    poseidonMatrix = ConstantsHelper.ConstantsM8;
                    break;
                case 9:
                    poseidonMatrix = ConstantsHelper.ConstantsM9;
                    break;
                case 10:
                    poseidonMatrix = ConstantsHelper.ConstantsM10;
                    break;
                case 11:
                    poseidonMatrix = ConstantsHelper.ConstantsM11;
                    break;
                case 12:
                    poseidonMatrix = ConstantsHelper.ConstantsM12;
                    break;
                case 13:
                    poseidonMatrix = ConstantsHelper.ConstantsM13;
                    break;
                case 14:
                    poseidonMatrix = ConstantsHelper.ConstantsM14;
                    break;
            }
            return poseidonMatrix;
        }

        public BigInteger CalculatePoseidonHash(BigInteger[] inputs, bool chained = false, bool trace = false)
        {
            Debug.Assert(inputs.Length > 0, "Inputs should be more than 0");
            if (!chained)
            {
                Debug.Assert(inputs.Length < T, "Inputs should be less than t");
            }
            BigInteger[] state = new BigInteger[T];

            for (long i = 0; i < T; i++)
            {
                state[i] = 0;
            }

            for (int i = 0; i < inputs.Length; i++)
            {
                state[i] = inputs[i];
            }

            int k = 0;
            int halfF = NRoundsF / 2;
            foreach (BigInteger bigInt in ConstantsC)
            {
                for (int i = 0; i < state.Length; i++)
                {
                    state[i] = state[i] + bigInt;
                }

                //CalculatePoseidonSBox
                if (k < halfF || k >= (halfF + NRoundsP))
                {
                    for (int j = 0; j < state.Length; j++)
                    {
                        state[j] = BigInteger.ModPow(state[j], ReducedExponent, SNARK_SCALAR_FIELD);
                    }
                }
                else
                {
                    state[0] = BigInteger.ModPow(state[0], ReducedExponent, SNARK_SCALAR_FIELD);
                }

                //CalculatePoseidonMix
                int n = state.Length;
                BigInteger[] results = new BigInteger[n];

                int blockSize = 64; // Adjust the block size based on cache size and performance tuning

                Parallel.For(0, n, i =>
                {
                    BigInteger resultsSumModulus = BigInteger.Zero;

                    int jj = 0;
                    for (; jj < n - blockSize + 1; jj += blockSize)
                    {
                        BigInteger blockSum = BigInteger.Zero;
                        for (int j = jj; j < jj + blockSize; j += Vector<long>.Count)
                        {
                            if (j + Vector<long>.Count <= jj + blockSize)
                            {
                                BigIntegerVector constantsVector = new BigIntegerVector(ConstantsM[i], j);
                                BigIntegerVector stateVector = new BigIntegerVector(state.ToList(), j);
                                blockSum += BigIntegerVector.Dot(constantsVector, stateVector);
                            }
                            else
                            {
                                for (int l = j; l < jj + blockSize; l++)
                                {
                                    blockSum += ConstantsM[i][l] * state[l];
                                }
                            }
                        }
                        resultsSumModulus += blockSum;
                    }

                    for (int j = jj; j < n; j++)
                    {
                        resultsSumModulus += ConstantsM[i][j] * state[j];
                    }

                    results[i] = resultsSumModulus % SNARK_SCALAR_FIELD;
                });






                state = results;

                if (trace == true)
                {
                    for (int j = 0; j < state.Length; j++)
                    {
                        Debug.WriteLine($"{k},{j} = {state[j]}");
                    }
                }
                k++;
            }
            if (chained == true)
            {
                //To do
            }
            return state[0];
        }
    }
}
