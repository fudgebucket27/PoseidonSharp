using NeinMath;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace PoseidonSharp
{
    public static class EddsaHelper
    {
        public static Signature SignatureStringToSignatureObject(string sig)
        {
            if (sig.Length != 194)
            {
                throw new ArgumentException("The length of sig must be 194");
            }

            string pureHexSig = sig.Substring(2);
            var r = IntegerConverter.FromHexString(pureHexSig.Substring(0, 64));
            var s = IntegerConverter.FromHexString(pureHexSig.Substring(64, 64));
            var v = IntegerConverter.FromHexString(pureHexSig.Substring(128));
            return new Signature((r,s), v);
        }

        public static BigInteger PrivateKeyHexStringToBigInteger(string privateKey)
        {
            BigInteger privateKeyBigInteger = BigInteger.Parse(privateKey.Substring(2, privateKey.Length - 2), NumberStyles.AllowHexSpecifier);
            if (privateKeyBigInteger.Sign == -1) //hex parse in big integer can make a negative number so we need to convert as below
            {
                string privateKeyAsPositiveHexString = "0" + privateKey.Substring(2, privateKey.Length - 2); //add a zero to the front of the string to make it positive
                privateKeyBigInteger = BigInteger.Parse(privateKeyAsPositiveHexString, NumberStyles.AllowHexSpecifier);
            }
            return privateKeyBigInteger;
        }

        public static (Integer a, Integer b) CalculatePointA(BigInteger privatekeyBigInteger)
        {
            (Integer x, Integer y) A = Point.Multiply(Integer.Parse(privatekeyBigInteger.ToString()), Point.Generator());
            return A;
        }
    }
}
