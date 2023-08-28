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
            Poseidon poseidon = new Poseidon(13 + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = { BigInteger.Parse("15262223708097584402615283257936266522564860189809682357548133077263290491192") };
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Eddsa eddsa = new Eddsa(poseidonHash, "0xff2f95f7f25dd17d160595603d49f9bd0bae765403d5d171fe1db2a3218c91");
            string signedMessage = eddsa.Sign();



            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
