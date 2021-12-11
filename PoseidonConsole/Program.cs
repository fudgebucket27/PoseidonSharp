using System;
using System.Numerics;
using PoseidonSharp;

namespace PoseidonConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int MAX_INPUT = 13;
            int[] inputs = { 1};
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128);
            BigInteger result = poseidon.CalculatePoseidonHash(inputs);
            Console.WriteLine($"Hash is {result}");
            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
