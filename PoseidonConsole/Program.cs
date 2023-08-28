using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using PoseidonSharp;

namespace PoseidonConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            BigInteger[] inputs = {
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111")
            };
            Poseidon poseidon = new Poseidon(inputs.Length + 1, 6, 53, "poseidon", 5, _securityTarget: 128);

            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Eddsa eddsa = new Eddsa(poseidonHash, "0x4485ade3c570854e240c72e9a9162e629f8e30db4d8130856da31787e7400f0");
            string signedMessage = eddsa.Sign();
            Console.WriteLine(signedMessage);


            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
