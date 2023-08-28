using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using Blake2Fast;

namespace PoseidonSharp
{
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
        }

        private BigInteger CalculateBlake2BHash(BigInteger data)
        {
            var sourceData = data.ToByteArray();
            if (sourceData.Length <= 32) //pad out bytes
            {
                var blake2bHash = Blake2b.ComputeHash(32, sourceData);
                var positiveHashBytes = new byte[blake2bHash.Length + 1];
                Array.Copy(blake2bHash, positiveHashBytes, blake2bHash.Length);
                BigInteger positiveBigInt = new BigInteger(positiveHashBytes);
                return positiveBigInt;
            }
            else //truncate bytes
            {
                var truncatedBytes = new byte[32];
                Array.Copy(sourceData, truncatedBytes, truncatedBytes.Length);
                var blake2BHashBytes = Blake2b.ComputeHash(32, truncatedBytes);
                var positiveHashBytes = new byte[blake2BHashBytes.Length + 1];
                Array.Copy(blake2BHashBytes, positiveHashBytes, blake2BHashBytes.Length);
                BigInteger positiveBigInt = new BigInteger(positiveHashBytes);
                return positiveBigInt;
            }
        }

        private List<BigInteger> CalculatePoseidonConstants(BigInteger p, byte[] seed, int nRounds)
        {
            List<BigInteger> poseidonConstants = new List<BigInteger>();
            BigInteger seedBigInt = new BigInteger(seed);
            for (int i = 0; i < nRounds; i++)
            {
               seedBigInt = CalculateBlake2BHash(seedBigInt);
               poseidonConstants.Add(seedBigInt % p);
            }
            
            StringBuilder output = new StringBuilder();
            output.AppendLine($"public static List<BigInteger> ConstantsC{nRounds}" +  " = new List<BigInteger>(){");
            int count = 0;
            foreach(var poseidonConstant in poseidonConstants)
            {
                count++;
                if (count != poseidonConstants.Count)
                {
                    output.AppendLine($"BigInteger.Parse(\"{poseidonConstant.ToString()}\"),");
                }
                else
                {
                    output.AppendLine($"BigInteger.Parse(\"{poseidonConstant.ToString()}\")");
                }
            }
            output.AppendLine("};");
            Console.WriteLine(output.ToString());
            
            return poseidonConstants;
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
            /*
            BigInteger two = BigInteger.Parse("2"); 
            //sb.AppendLine($"public static List<List<BigInteger>> ConstantsM{t} = new List<List<BigInteger>>");
            //sb.AppendLine("{");
            for (int i = 0; i < t; i++)
            {
                //sb.Append("    new List<BigInteger> { ");
                List<BigInteger> bigIntegers = new List<BigInteger>();
                for (int j = 0; j < t; j++)
                {

                    BigInteger result = BigInteger.ModPow(constants[i] - constants[t+j] % p, p - two, p);
                    if (result.Sign == -1)
                    {
                        result = result + p;
                    }
                    //sb.Append($"BigInteger.Parse(\"{result}\")");
                    bigIntegers.Add(result);
                    //if (j < t - 1) sb.Append(", ");
                }
                //sb.AppendLine(" },");
                poseidonMatrix.Add(bigIntegers);
            }
            //sb.AppendLine("};");
            //Console.WriteLine(sb.ToString());
            */
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

            for(long i = 0; i < T;i++)
            {
                state[i] = 0;
            }            

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
                if (trace == true)
                {
                    for (int j = 0; j < state.Length; j++)
                    {
                        Debug.WriteLine($"{k},{j} = {state[j]}");
                    }
                }
                k++;            
            }
            if(chained == true)
            {
                //To do
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
                    if(state[j].Sign == -1)
                    {
                        state[j] = state[j] + SNARK_SCALAR_FIELD;
                    }
                }
            }
            else
            {
                state[0] = BigInteger.ModPow(state[0], E, SNARK_SCALAR_FIELD);
                if (state[0].Sign == -1)
                {
                    state[0] = state[0] + SNARK_SCALAR_FIELD;
                }
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
                if (resultsSumModulus.Sign == -1)
                {
                    resultsSumModulus = resultsSumModulus + SNARK_SCALAR_FIELD;
                }
                results[i] = resultsSumModulus;   
                resultsSum = BigInteger.Parse("0");
            }
            return results;
        }        
    }
}
