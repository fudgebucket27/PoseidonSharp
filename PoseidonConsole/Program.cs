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
            int iterations = 1000;
            Console.WriteLine(working);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for(int i = 0; i < 1000; i++)
            {
                    BigInteger[] inputs = {
                    BigInteger.Parse("11111111111111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("91111111111111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99111111111111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99911111111111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99991111111111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99999111111111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99999911111111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99999991111111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99999999111111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99999999911111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99999999991111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99999999999111111111111111111111111111111111111111111111111111111111111111"),
                    BigInteger.Parse("99999999999911111111111111111111111111111111111111111111111111111111111111")
                };
                Poseidon poseidon = new Poseidon(inputs.Length + 1, 6, 53, "poseidon", 5, _securityTarget: 128);

                BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
                Eddsa eddsa = new Eddsa(poseidonHash, "0x4485ade3c570854e240c72e9a9162e629f8e30db4d8130856da31787e7400f0");
                string signedMessage = eddsa.Sign();
            }
            sw.Stop();
            double averageTimePerHash = (double)sw.ElapsedMilliseconds / iterations;
            Console.WriteLine($"Ellapsed time for {iterations} iterations of hash and sign is {sw.ElapsedMilliseconds / 1000} seconds...Average Time per hash is {averageTimePerHash} millieseconds...");


            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
