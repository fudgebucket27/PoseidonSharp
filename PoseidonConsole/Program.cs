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
        private static string PrivateKey2 = "0x19bd8d52d552d2f112b478686f18577b8088e5b1860c3523c53f943304951c3"; //This private key has been unpaired from the real account

        static void Main(string[] args)
        {
            //Test case 3
            Poseidon poseidon1 = new Poseidon(2, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs1 = { 
                BigInteger.Parse("1233333333333333")
            };
     

            Poseidon poseidon2 = new Poseidon(3, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon3 = new Poseidon(4, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon4 = new Poseidon(5, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon5 = new Poseidon(6, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon6 = new Poseidon(7, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon7 = new Poseidon(8, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon8 = new Poseidon(9, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon9 = new Poseidon(10, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon10 = new Poseidon(11, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon11 = new Poseidon(12, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon12 = new Poseidon(13, 6, 53, "poseidon", 5, _securityTarget: 128);
            Poseidon poseidon13= new Poseidon(14, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger pHash1 = poseidon1.CalculatePoseidonHash(inputs1);
            //BigInteger pHash2 = poseidon2.CalculatePoseidonHash(inputs1);
            //BigInteger pHash3 = poseidon3.CalculatePoseidonHash(inputs1);
            //BigInteger pHash4 = poseidon4.CalculatePoseidonHash(inputs1);
            //BigInteger pHash5 = poseidon5.CalculatePoseidonHash(inputs1);
            //BigInteger pHash6 = poseidon6.CalculatePoseidonHash(inputs1);
            //BigInteger pHash7 = poseidon7.CalculatePoseidonHash(inputs1);
            //BigInteger pHash8 = poseidon8.CalculatePoseidonHash(inputs1); 
            //BigInteger pHash9 = poseidon9.CalculatePoseidonHash(inputs1);
            //BigInteger pHash10 = poseidon10.CalculatePoseidonHash(inputs1);
            //BigInteger pHash11 = poseidon11.CalculatePoseidonHash(inputs1);
            //BigInteger pHash12 = poseidon12.CalculatePoseidonHash(inputs1);
            //BigInteger pHash13 = poseidon13.CalculatePoseidonHash(inputs1);

            int MAX_INPUT = 13;
            Poseidon poseidon = new Poseidon(MAX_INPUT + 1, 6, 53, "poseidon", 5, _securityTarget: 128);
            BigInteger[] inputs = { BigInteger.Parse("95262223708097584402615283257936266522564860189809682357548133077263290491192") };
            BigInteger poseidonHash = poseidon.CalculatePoseidonHash(inputs);
            Eddsa eddsa = new Eddsa(poseidonHash, PrivateKey2);
            string signedMessage = eddsa.Sign();



            Console.WriteLine("Enter to exit");
            Console.ReadKey();
        }
    }
}
