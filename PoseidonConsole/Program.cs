using System;
using PoseidonSharp;

namespace PoseidonConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int MAX_INPUT = 13;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1,6,53,"poseidon",5, _securityTarget: 128);
            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
