using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace PoseidonSharp
{
    public class Eddsa
    {
        private BigInteger OriginalPoseidonHash = new BigInteger();
        private BigInteger PrivateKey { get; set; }

        private static BigInteger JUBJUB_E = BigInteger.Parse("21888242871839275222246405745257275088614511777268538073601725287587578984328");
        private static BigInteger JUBJUB_C = BigInteger.Parse("8");
        private static double JUBJUB_L = Math.Floor((double)(JUBJUB_E / JUBJUB_C));


        public Eddsa(BigInteger _originalPoseidonHash, string _privateKey)
        {
            OriginalPoseidonHash = _originalPoseidonHash;
            PrivateKey = BigInteger.Parse(_privateKey.Substring(2, _privateKey.Length - 2), NumberStyles.AllowHexSpecifier); ;
        }

        public void Sign(object _points = null)
        {
            (BigInteger x, BigInteger y) B;;
            if (_points != null)
            {
                B = ((BigInteger x, BigInteger))_points;
            }
            else
            {
                B = Point.Generator();
            }
            Debug.WriteLine($"Private key: {PrivateKey}");
            Debug.WriteLine($"B: {B}");
            (BigInteger x, BigInteger y) A = Point.Multiply(PrivateKey, B);
            Debug.WriteLine($"A: {A}");

            BigInteger M = OriginalPoseidonHash;
            Debug.WriteLine($"M: {M}");
            BigInteger key = PrivateKey;

            HashSecret(key, M);

        }

        private void HashSecret(BigInteger k, BigInteger args)
        {
            var numberOfBytes = CalculateNumberOfBytes(k, args);
            byte[] result;
            SHA512 shaM = new SHA512Managed();
            result = shaM.ComputeHash(numberOfBytes);

            byte[] finalArray = new byte[result.Length + 1];

            for (int i = 0; i < finalArray.Length; i++)
            {
                if (i == 0)
                {
                    //
                }
                else if (i == result.Length)
                {
                    //
                }
                else
                {
                    finalArray[i] = result[i];
                }
            }

            BigInteger sha512Num = new BigInteger(finalArray);
            Debug.WriteLine(sha512Num);
        }

        private byte[] CalculateNumberOfBytes(BigInteger self, BigInteger args)
        {
            BigInteger nBits = (BigInteger)Math.Ceiling(BigInteger.Log(self, 2));
            nBits += 8 - (nBits % 8);
            BigInteger nBytes = new BigInteger();
            nBytes = BigInteger.DivRem(nBits, 8, out nBytes);
            Debug.WriteLine(nBytes);
            return nBytes.ToByteArray();
        }
    }
}