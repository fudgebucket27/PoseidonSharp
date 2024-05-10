using NeinMath;
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
        private BigInteger OriginalHash { get; set; }
        private BigInteger PrivateKey { get; set; }
        private static BigInteger LastPrivateKey { get; set; } // Static field to store the last private key

        private static BigInteger JUBJUB_E = BigInteger.Parse("21888242871839275222246405745257275088614511777268538073601725287587578984328");
        private static BigInteger JUBJUB_C = BigInteger.Parse("8");
        private static BigInteger JUBJUB_L = BigInteger.DivRem(JUBJUB_E, JUBJUB_C, out JUBJUB_L);

        private static (BigInteger x, BigInteger y) PrecomputedPointA;

        public Eddsa(BigInteger _originalHash, string _privateKey)
        {
            OriginalHash = _originalHash;
            BigInteger privateKeyBigBigInteger = BigInteger.Parse(_privateKey.Substring(2, _privateKey.Length - 2), NumberStyles.AllowHexSpecifier);
            if (privateKeyBigBigInteger.Sign == -1) //hex parse in big BigInteger can make a negative number so we need to convert as below
            {
                string privateKeyAsPositiveHexString = "0" + _privateKey.Substring(2, _privateKey.Length - 2); //add a zero to the front of the string to make it positive
                privateKeyBigBigInteger = BigInteger.Parse(privateKeyAsPositiveHexString, NumberStyles.AllowHexSpecifier);
            }

            // Check if new private key is different from the last used private key
            if (privateKeyBigBigInteger != LastPrivateKey)
            {
                LastPrivateKey = privateKeyBigBigInteger; // Update the last private key
                ResetPreComputedPointA(); // Reset PrecomputedPointA

            }

            PrivateKey = privateKeyBigBigInteger;

            if (PrecomputedPointA == default)
            {
                var B = (BigInteger.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"), BigInteger.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));
                PrecomputedPointA = Point.Multiply(BigInteger.Parse(PrivateKey.ToString()), B);
                Debug.WriteLine("EDDSA: Precomputed Point A was generated");
            }
        }

        public static void ResetPreComputedPointA()
        {
            PrecomputedPointA = default;
            Debug.WriteLine("EDDSA: Precomputed Point A was reset");           
        }

        public string Sign(object _points = null)
        {
            (BigInteger x, BigInteger y) B;
            if (_points != null)
            {
                B = ((BigInteger x, BigInteger))_points;
            }
            else
            {
                B = (BigInteger.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"), BigInteger.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));
            }

            (BigInteger x, BigInteger y) A = PrecomputedPointA; ; //= PrecomputedPointA != default ? PrecomputedPointA : Point.Multiply(BigInteger.Parse(PrivateKey.ToString()), B);
            BigInteger r = HashPrivateKey(BigInteger.Parse(PrivateKey.ToString()), BigInteger.Parse(OriginalHash.ToString()));
            (BigInteger x, BigInteger y) R = Point.Multiply(BigInteger.Parse(r.ToString()), B);
            BigInteger t = BigInteger.Parse(HashPublic(R, A, BigInteger.Parse(OriginalHash.ToString())).ToString());
            BigInteger S = (r + (BigInteger.Parse(PrivateKey.ToString()) * t)) % JUBJUB_E;
            if (S.Sign == -1)
            {
                S = S + JUBJUB_E;
            }

            Signature signature = new Signature(R, S);
            SignedMessage signedMessage = new SignedMessage(A, signature, BigInteger.Parse(OriginalHash.ToString()));
            string rX = signedMessage.Signature.R.x.ToString("x").PadLeft(64, '0');
            string rY = signedMessage.Signature.R.y.ToString("x").PadLeft(64, '0');
            string rS = signedMessage.Signature.S.ToString("x").PadLeft(64, '0');
            string finalSignedMessage = "0x" + rX + rY + rS;
            return finalSignedMessage.ToLower();
        }

        public bool Verify(SignedMessage signedMessage)
        {
            var A = signedMessage.A;
            var sig = signedMessage.Signature;
            var msg = signedMessage.Message;
            var B = (BigInteger.Parse("16540640123574156134436876038791482806971768689494387082833631921987005038935"), BigInteger.Parse("20819045374670962167435360035096875258406992893633759881276124905556507972311"));
            var lhs = Point.Multiply(sig.S, B);
            var hashPublic = BigInteger.Parse(HashPublic(sig.R, A, BigInteger.Parse(OriginalHash.ToString())).ToString());
            var aMultiplyHashPublic = Point.Multiply(hashPublic, A);
            var rhs = Point.Add(sig.R, aMultiplyHashPublic);
            return lhs == rhs;
        }

        private BigInteger HashPublic((BigInteger x, BigInteger y) r, (BigInteger x, BigInteger y) a, BigInteger m)
        {
            BigInteger[] inputs = { BigInteger.Parse(r.x.ToString()),BigInteger.Parse(r.y.ToString()), BigInteger.Parse(a.x.ToString()), BigInteger.Parse(a.y.ToString()), BigInteger.Parse(m.ToString()) };
            Poseidon poseidon = new Poseidon(6, 6, 52, "poseidon", 5, _securityTarget: 128);
            return poseidon.CalculatePoseidonHash(inputs);
        }

        private BigInteger HashPrivateKey(BigInteger privateKey, BigInteger originalHash)
        {
            var secretBytes = CalculateNumberOfBytesAndReturnByteArray(privateKey);
            var originalHashBytes = CalculateNumberOfBytesAndReturnByteArray(originalHash);
            byte[] originalHashPaddedBytes = null;
            if (originalHashBytes.Length < 32) //Pad out byte array to 32 bytes as the original hash can sometimes give less than a 32 byte array
            {
                originalHashPaddedBytes = new byte[originalHashBytes.Length + 1];
                Array.Copy(originalHashBytes, originalHashPaddedBytes, originalHashBytes.Length);
            }
            else
            {
                originalHashPaddedBytes = originalHashBytes;
            }
            var combinedPrivateKeyAndPoseidonHashBytes = CombineBytes(secretBytes, originalHashPaddedBytes);
            byte[] sha512HashBytes;
            SHA512 sha512Managed = new SHA512Managed();
            sha512HashBytes = sha512Managed.ComputeHash(combinedPrivateKeyAndPoseidonHashBytes);

            BigInteger sha512HashedNumber = new BigInteger(sha512HashBytes);
            if (sha512HashedNumber.Sign == -1) //sha512 in bytes is a hex number so sometimes can return negative
            {
                string sha512HexString = "0" + sha512HashedNumber.ToString("x"); //add a zero to the front of the hex string to make it a  positive number
                sha512HashedNumber = BigInteger.Parse(sha512HexString, NumberStyles.AllowHexSpecifier);
            }

            BigInteger result = sha512HashedNumber % JUBJUB_L;
            if (result.Sign == -1)
            {
                result = result + JUBJUB_L;
            }

            return result;
        }

        private byte[] CalculateNumberOfBytesAndReturnByteArray(BigInteger bigBigIntegerValue) //Don't really need to calcuate the number of bytes but is helpful for debugging
        {
            /*
            BigBigInteger numberOfBits = (BigBigInteger)Math.Ceiling(BigBigInteger.Log(bigBigIntegerValue, 2));
            numberOfBits += 8 - (numberOfBits % 8);
            BigBigInteger numberOfBytes = new BigBigInteger();
            numberOfBytes = BigBigInteger.DivRem(numberOfBits, 8, out numberOfBytes); //We want 32 bytes otherwise we will have to pad out the byte array
            */
            return bigBigIntegerValue.ToByteArray();
        }

        public static byte[] CombineBytes(byte[] first, byte[] second)
        {
            return first.Concat(second).ToArray();
        }

    }
}