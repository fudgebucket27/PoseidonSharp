using System;
using System.Diagnostics;
using System.Numerics;
using PoseidonSharp;

namespace PoseidonConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int MAX_INPUT = 13;
            int[] inputs = {1};
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128);
            BigInteger result = poseidon.CalculatePoseidonHash(inputs);
            Debug.Assert(result == BigInteger.Parse("14018714854885098128064817341184136022863799846023165041562300563859625887667"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash is {result}");
            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
