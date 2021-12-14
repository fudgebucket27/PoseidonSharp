using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
        private static BigInteger JUBJUB_L = BigInteger.DivRem(JUBJUB_E, JUBJUB_C, out JUBJUB_L);


        public Eddsa(BigInteger _originalPoseidonHash, string _privateKey)
        {
            OriginalPoseidonHash = _originalPoseidonHash;
            PrivateKey = BigInteger.Parse(_privateKey.Substring(2, _privateKey.Length - 2), NumberStyles.AllowHexSpecifier); ;
        }

        public string Sign(object _points = null)
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

            Debug.WriteLine($"original poseidon hash: {OriginalPoseidonHash}");
            BigInteger M = OriginalPoseidonHash;
            Debug.WriteLine($"M: {M}");
           
            BigInteger key = PrivateKey;
            BigInteger r = HashSecret(key, M);
            Debug.WriteLine($"r: {r}");

            (BigInteger x, BigInteger y) R = Point.Multiply(r, B);
            Debug.WriteLine($"R: {R}");

            BigInteger t = HashPublic(R, A, M);
            Debug.WriteLine($"t: {t}");

            BigInteger S = (r + (key * t)) % JUBJUB_E;
            if (S.Sign == -1)
            {
                S = S + JUBJUB_E;
            }

            Debug.WriteLine($"S: {S}");

            Signature signature = new Signature(R, S);
            SignedMessage signedMessage = new SignedMessage(A, signature, OriginalPoseidonHash);
 
            string rX = signedMessage.Sig.R.x.ToString("x").PadLeft(64,'0');
            string rY = signedMessage.Sig.R.y.ToString("x").PadLeft(64, '0');
            string rS = signedMessage.Sig.S.ToString("x").PadLeft(64, '0');

            Debug.WriteLine(rX);
            Debug.WriteLine(rY);
            Debug.WriteLine(rS);

            string finalMessage = "0x" + rX + rY + rS;
            return finalMessage;
        }



        private BigInteger HashPublic((BigInteger x, BigInteger y) r, (BigInteger x, BigInteger y) a, BigInteger m)
        {
            BigInteger[] inputs = { r.x, r.y, a.x, a.y, m};
            Poseidon poseidon = new Poseidon(6, 6, 52, "poseidon", 5, _securityTarget: 128);
            return poseidon.CalculatePoseidonHash(inputs);
        }

        private BigInteger HashSecret(BigInteger k, BigInteger args)
        {
            var secretBytes = CalculateNumberOfBytes(k);
            var mBytes = CalculateNumberOfBytes(args);
            var combinedBytes = CombineBytes(secretBytes, mBytes); 
            byte[] sha512Hash;
            SHA512 sha512 = new SHA512Managed();
            sha512Hash = sha512.ComputeHash(combinedBytes);

            BigInteger sha512Num = new BigInteger(sha512Hash);
            if(sha512Num.Sign == -1)
            {
                string bigIntHex = "0" + sha512Num.ToString("x");
                sha512Num = BigInteger.Parse(bigIntHex, NumberStyles.AllowHexSpecifier);
            }

            BigInteger result = sha512Num %  JUBJUB_L;
            if (result.Sign == -1)
            {
                result = result + JUBJUB_L;
            }

            return result;
        }

        private byte[] CalculateNumberOfBytes(BigInteger self)
        {
            BigInteger nBits = (BigInteger)Math.Ceiling(BigInteger.Log(self, 2));
            nBits += 8 - (nBits % 8);
            BigInteger nBytes = new BigInteger();
            nBytes = BigInteger.DivRem(nBits, 8, out nBytes);
            return self.ToByteArray();
        }

        public static byte[] CombineBytes(byte[] first, byte[] second)
        {
            return first.Concat(second).ToArray();
        }

    }
}