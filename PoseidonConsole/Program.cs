using System;
using PoseidonSharp;

namespace PoseidonConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Poseidon poseidon = new Poseidon(2,6,53,"POSEIDON",5, _securityTarget: 128);
            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
