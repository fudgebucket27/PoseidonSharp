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
            BigInteger[] inputs = {BigInteger.Parse("1")};
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128);
            
            BigInteger testOne = poseidon.CalculatePoseidonHash(inputs);
            Debug.Assert(testOne == BigInteger.Parse("14018714854885098128064817341184136022863799846023165041562300563859625887667"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test one is {testOne}");
            
            BigInteger[] inputsTwo = { BigInteger.Parse("1"), BigInteger.Parse("9400000000000000000000000000") };
            BigInteger testTwo = poseidon.CalculatePoseidonHash(inputsTwo);
            Debug.Assert(testTwo == BigInteger.Parse("2838802984016459847807835899395446048073460738419316401120440074779237106208"), "Hash doesn't match expected hash!");
            Console.WriteLine($"Hash of test two is {testTwo}");

            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
