﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using Blake2Fast;

namespace PoseidonSharp
{
    public class Poseidon
    {
        public BigInteger SNARK_SCALAR_FIELD = BigInteger.Parse("21888242871839275222246405745257275088548364400416034343698204186575808495617");
        public BigInteger FR_ORDER = BigInteger.Parse("21888242871839275222246405745257275088614511777268538073601725287587578984328");
        public int T { get; set; }
        public int NRoundsF { get; set; }
        public int NRoundsP { get; set; }
        public string Seed { get; set; }
        public int E { get; set; }

        public List<BigInteger> ConstantsC { get; set; }
        public List<List<BigInteger>> ConstantsM { get; set; }
        public int securityTarget { get; set; }

        public Poseidon(int _t, int _nRoundsF, int _nRoundsP, string _seed, int _e, List<BigInteger> _constantsC = null, List<BigInteger> _constantsM = null, int _securityTarget = 0)
        {
            Debug.Assert(_nRoundsF % 2 == 0 && _nRoundsF > 0, "_nRoundsF needs to have modulus 2 of 0 and be more than 0");
            Debug.Assert(_nRoundsP > 0, "_nRoundsP needs to be more than 0");

            double n = Math.Floor(Math.Log2((double)SNARK_SCALAR_FIELD));
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

            Debug.Assert((_nRoundsF + _nRoundsP) > ((interpolationAttackRatio * Math.Min(n, M)) + Math.Log2(_t)), "(nRoundsF + nRoundsP) > ((2 + min(M, n)) * grobner_attack_ratio_rounds)");
            Debug.Assert((_nRoundsF + _nRoundsP) > ((2 + Math.Min(M, n)) * grobnerAttackRatioRounds), "(nRoundsF + nRoundsP) > ((2 + min(M, n)) * grobner_attack_ratio_rounds)");
            Debug.Assert((_nRoundsF + (_t * _nRoundsP)) > (M * grobnerAttackRatioSBoxes), "(nRoundsF + (t * nRoundsP)) > (M * grobner_attack_ratio_sboxes)");

            if (_constantsC == null)
            {
                string constantsCseed = _seed + "_constants";
                byte[] constantsCseedBytes = Encoding.ASCII.GetBytes(constantsCseed);
                ConstantsC = CalculatePoseidonConstants(SNARK_SCALAR_FIELD, constantsCseedBytes, _nRoundsF + _nRoundsP);
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
        }

        private BigInteger CalculateBlake2BHash(BigInteger data)
        {
            var sourceData = data.ToByteArray();
            if (sourceData.Length <= 32)
            {
                var hash = Blake2b.ComputeHash(32, sourceData);
                var positiveHash = new byte[hash.Length + 1];
                Array.Copy(hash, positiveHash, hash.Length);
                BigInteger positiveBigInt = new BigInteger(positiveHash);
                return positiveBigInt;
            }
            else
            {
                var truncated = new byte[32];
                Array.Copy(sourceData, truncated, truncated.Length);
                var hash = Blake2b.ComputeHash(32, truncated);
                var positiveHash = new byte[hash.Length + 1];
                Array.Copy(hash, positiveHash, hash.Length);
                BigInteger positiveBigInt = new BigInteger(positiveHash);
                return positiveBigInt;
            }
        }

        private List<BigInteger> CalculatePoseidonConstants(BigInteger p, byte[] seed, int nRounds)
        {
            Debug.Assert(nRounds is int, "nRounds must be int");
            List<BigInteger> poseidonConstants = new List<BigInteger>();
            BigInteger seedBigInt = new BigInteger(seed);
            for (int i = 0; i < nRounds; i++)
            {
               seedBigInt = CalculateBlake2BHash(seedBigInt);
               poseidonConstants.Add(seedBigInt % p);
            }
            return poseidonConstants;
        }

        private List<List<BigInteger>> CalculatePoseidonMatrix(BigInteger p, byte[] seed, int t)
        {
            List<BigInteger> c = CalculatePoseidonConstants(p, seed, t * 2);
            List<List<BigInteger>> poseidonMatrix = new List<List<BigInteger>>();

            for (int i = 0; i < t; i++)
            {
                List<BigInteger> bigIntegers = new List<BigInteger>();
                for (int j = 0; j < t; j++)
                {
                    bigIntegers.Add(BigInteger.ModPow((c[i] - c[t + j]) % p, p - 2, p));                    
                }
                poseidonMatrix.Add(bigIntegers);
            }
            return poseidonMatrix;
        }

        public BigInteger CalculatePoseidonHash(int[] inputs, bool chained = false, bool trace = false)
        {
            Debug.Assert(inputs.Length > 0, "Inputs should be more than 0");
            if (!chained)
            {
                Debug.Assert(inputs.Length < T, "Inputs should be less than t");
            }
            BigInteger[] state = new BigInteger[T];
            Array.Fill(state, 0);

            for (int i = 0; i < inputs.Length; i++)
            {
                state[i] = inputs[i];
            }

            int k = 0;
            foreach (BigInteger bigInt in ConstantsC)
            {
                for (int i = 0; i < state.Length; i++)
                {
                    state[i] = state[i] + bigInt;
                }
                state = CalculatePoseidonSBox(state, k);
                state = CalculatePoseidonMix(state);
                k++;
            }
            return state[0];
        }

        private BigInteger[] CalculatePoseidonSBox(BigInteger[] state, int i)
        {
            int halfF = NRoundsF / 2;

            if (i < halfF || i >= (halfF + NRoundsP))
            {
                for(int j = 0; j < state.Length; j++)
                {
                    state[j] = BigInteger.ModPow(state[j], E, SNARK_SCALAR_FIELD);
                }
            }
            else
            {
                state[0] = BigInteger.ModPow(state[0], E, SNARK_SCALAR_FIELD);
            }
            return state;
        }

        private BigInteger[] CalculatePoseidonMix(BigInteger[] originalState)
        {
            BigInteger[] results = new BigInteger[originalState.Length];
            BigInteger resultsSum = BigInteger.Parse("0");
            for(int i = 0; i < ConstantsM.Count; i++)
            {
                for(int j = 0; j < originalState.Length; j++)
                {
                    BigInteger valuesMultiped = ConstantsM[i][j] * originalState[j];
                    resultsSum = BigInteger.Add(resultsSum, valuesMultiped); 
                }
                BigInteger resultsSumModulus = resultsSum % SNARK_SCALAR_FIELD;
                results[i] = resultsSumModulus;   
                resultsSum = BigInteger.Parse("0");
            }
            return results;
        }        
    }
}
